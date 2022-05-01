using System.ComponentModel;
using System.Diagnostics;
using PcbDesignSimuModeling.Core.Models.Resources.Cpu.SubClasses;

namespace PcbDesignSimuModeling.Core.Models.Resources.Cpu;

public class CpuCluster : MixedResource, INotifyPropertyChanged
{
    private readonly CpuCore[] _cpuCores;
    private readonly CpuThread[] _threadPool;
    private readonly Dictionary<int, List<CpuThread>> _processIdAndThreads = new();

    private readonly double _contextSwitchPenalty;
    private readonly double _hyperThreadingPenalty;

    protected override List<int> UtilizingProcIds => _processIdAndThreads.Keys.ToList();

    public CpuCluster(int threadCount, double clockRate, double hyperThreadingPenalty = 0.725,
        double contextSwitchPenalty = 0.0035)
    {
        ThreadCount = threadCount;
        ClockRate = clockRate;
        _hyperThreadingPenalty = hyperThreadingPenalty;
        _contextSwitchPenalty = contextSwitchPenalty;

        _cpuCores = Enumerable.Range(0, ThreadCount / 2).Select(_ => new CpuCore()).ToArray();
        _threadPool = _cpuCores.SelectMany(core => new[] {core.Threads.Th0, core.Threads.Th1}).ToArray();
    }


    public int ThreadCount { get; set; }
    public double ClockRate { get; set; }


    public bool TryGetResource(int requestId, int reqThreadCount, double avgOneThreadUtil = 1.0)
    {
        var threadsList = new List<CpuThread>();

        for (var i = 0; i < reqThreadCount; i++)
        {
            var unloadedCore = _cpuCores.FirstOrDefault(core => core.ActiveTasks == 0);
            var optimalThread = unloadedCore?.Threads.Th0 ?? _threadPool.MinBy(thread => thread.RealTaskCount)!;

            optimalThread.UtilizationByProcessList.Add(new ThreadUtilizationByProcess(requestId, avgOneThreadUtil));
            threadsList.Add(optimalThread);
        }

        _processIdAndThreads.Add(requestId, threadsList);
        BalanceRes();
        return true;
    }

    public override double PowerForRequest(int requestId)
    {
        var processThreads = _processIdAndThreads[requestId];
        var threadSum = 0.0;

        foreach (var thread in processThreads.Distinct())
        {
            var threadSummaryUsageByTheProcess = thread.UtilizationByProcessList
                .Where(utilization => utilization.ProcessId == requestId)
                .Sum(utilization => utilization.CurUtil);

            threadSummaryUsageByTheProcess *=
                1.0 - Math.Max(0.0, _contextSwitchPenalty * (thread.ActiveTasks - 1) - thread.IdlePercent);

            if (thread.IsInHyperThreadingMode)
                threadSummaryUsageByTheProcess *=
                    thread.CpuCore.IdlePercent + (1.0 - thread.CpuCore.IdlePercent) * _hyperThreadingPenalty;

            threadSum += threadSummaryUsageByTheProcess;
        }

        Debug.WriteLine(threadSum);
        return threadSum * ClockRate;
    }

    public override void FreeResource(int requestId)
    {
        var threadsList = _processIdAndThreads[requestId].Distinct();

        foreach (var thread in threadsList)
            thread.UtilizationByProcessList.RemoveAll(utilization => utilization.ProcessId == requestId);
        _processIdAndThreads.Remove(requestId);

        BalanceRes();
    }


    public virtual void BalanceRes()
    {
        var unloadedCore = _cpuCores.FirstOrDefault(core => core.ActiveTasks == 0);
        var maxLoadThread = _threadPool.MaxBy(thread => thread.RealTaskCount)!;
        var minLoadThread = unloadedCore?.Threads.Th0 ?? _threadPool.MinBy(thread => thread.RealTaskCount)!;

        var deltaLoad = (maxLoadThread.RealTaskCount - minLoadThread.RealTaskCount) / 2.0;
        if (deltaLoad < 1e-12) return; // Check for skip exception (when sequence is empty)

        while (deltaLoad - 1e-12 > maxLoadThread.UtilizationByProcessList.First().MaxUtil / 2.0)
        {
            var utilizationToRemove =
                maxLoadThread.UtilizationByProcessList.Take(Math.Max(1, (int) Math.Round(deltaLoad))).ToList();

            foreach (var utilization in utilizationToRemove)
            {
                _processIdAndThreads[utilization.ProcessId].Remove(maxLoadThread);
                _processIdAndThreads[utilization.ProcessId].Add(minLoadThread);
            }

            maxLoadThread.UtilizationByProcessList.RemoveRange(utilizationToRemove);
            minLoadThread.UtilizationByProcessList.AddRange(utilizationToRemove);

            unloadedCore = _cpuCores.FirstOrDefault(core => core.ActiveTasks == 0);
            maxLoadThread = _threadPool.MaxBy(thread => thread.RealTaskCount)!;
            minLoadThread = unloadedCore?.Threads.Th0 ?? _threadPool.MinBy(thread => thread.RealTaskCount)!;
            
            deltaLoad = (maxLoadThread.RealTaskCount - minLoadThread.RealTaskCount) / 2.0;
            if (deltaLoad < 1e-12) return;
        }
    }


    public override IResource Clone() => new CpuCluster(this.ThreadCount, this.ClockRate);

    public override decimal Cost => (decimal) Math.Round(
        (Math.Exp(ClockRate / 5.0) * 1.0 / ThreadCount +
         Math.Exp(ClockRate / 3.0) * (ThreadCount - 1.0) / ThreadCount) *
        Math.Pow(ThreadCount, 0.87)
        * 1500);

    public event PropertyChangedEventHandler? PropertyChanged;
}