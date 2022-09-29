using System;
using System.Collections.Generic;
using System.Linq;
using PcbDesignSimuModeling.WPF.Models.Resources.CpuClusterRes.SubClasses;
using static PcbDesignSimuModeling.WPF.Models.SimuSystem.GeneralSimulationSettings;

namespace PcbDesignSimuModeling.WPF.Models.Resources.CpuClusterRes;

public class CpuCluster : MixedResource
{
    private readonly double _contextSwitchPenalty;
    private readonly double _hyperThreadingPenalty;

    private readonly CpuCore[] _cpuCores;
    private readonly CpuThread[] _threadPool;
    private readonly Dictionary<int, List<CpuThread>> _processIdAndThreads = new();
    private readonly Dictionary<int, List<CpuThread>> _processIdAndThreadsDistinct = new();

    protected override List<int> UtilizingRequestsIds => _processIdAndThreads.Keys.ToList();

    public CpuCluster(int threadCount, double clockRate,
        double hyperThreadingPenalty = 0.725, double contextSwitchPenalty = 0.0035)
    {
        ThreadCount = threadCount;
        ClockRate = clockRate;
        _hyperThreadingPenalty = hyperThreadingPenalty;
        _contextSwitchPenalty = contextSwitchPenalty;

        _cpuCores = Enumerable.Range(0, ThreadCount / 2).Select(_ => new CpuCore()).ToArray();
        _threadPool = _cpuCores.SelectMany(core => new[] {core.Threads.Th0, core.Threads.Th1}).ToArray();

        CoveringConfig = CpuModelInfo.ChooseConfigurationOrDefault(ThreadCount, ClockRate,
            (CurSettings.OneSocketMBoardPrice, CurSettings.DualSocketMBoardPrice));
        Cost = CoveringConfig is null
            ? Decimal.MaxValue
            : CoveringConfig.Value.cpuModel.Price * (CoveringConfig.Value.IsDualSocket ? 2 : 1);
        CostPerMonth = Cost.WithMonthAmort(CurSettings.CpuAmortization);
    }


    public int ThreadCount { get; }
    public double ClockRate { get; }


    public (CpuModelInfo cpuModel, bool IsDualSocket)? CoveringConfig { get; }
    public decimal Cost { get; }
    public override decimal CostPerMonth { get; }


    public bool TryGetResource(int requestId, int reqThreadCount, double avgOneThreadUtil = 1.0)
    {
        avgOneThreadUtil = Math.Clamp(avgOneThreadUtil, 0.0, 1.0);
        var threadsList = new List<CpuThread>();

        for (var i = 0; i < reqThreadCount; i++)
        {
            var unloadedCore = _cpuCores.FirstOrDefault(core => core.TaskCount == 0);
            var optimalThread = unloadedCore?.Threads.Th0 ?? _threadPool.MinBy(thread => thread.NormTaskCount)!;

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
                1.0 - Math.Max(0.0, _contextSwitchPenalty * (thread.TaskCount - 1) - thread.IdlePercent);

            if (thread.IsInHyperThreadingMode)
                threadSummaryUsageByTheProcess *=
                    thread.CpuCore.IdlePercent + (1.0 - thread.CpuCore.IdlePercent) * _hyperThreadingPenalty;

            threadSum += threadSummaryUsageByTheProcess;
        }

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
        var unloadedCore = _cpuCores.FirstOrDefault(core => core.TaskCount == 0);
        var maxLoadThread = _threadPool.MaxBy(thread => thread.NormTaskCount)!;
        var minLoadThread = unloadedCore?.Threads.Th0 ?? _threadPool.MinBy(thread => thread.NormTaskCount)!;

        var deltaLoad = (maxLoadThread.NormTaskCount - minLoadThread.NormTaskCount) / 2.0;
        if (deltaLoad < 1e-12) return; // Check for skip exception (when sequence is empty)

        while (deltaLoad - 1e-12 > maxLoadThread.UtilizationByProcessList.First().MaxUtil / 2.0)
        {
            var utilizationToRemove =
                maxLoadThread.UtilizationByProcessList.Take(Math.Max(1, Convert.ToInt32(deltaLoad))).ToList();

            foreach (var utilization in utilizationToRemove)
            {
                _processIdAndThreads[utilization.ProcessId].Remove(maxLoadThread);
                _processIdAndThreads[utilization.ProcessId].Add(minLoadThread);
            }

            maxLoadThread.UtilizationByProcessList.RemoveRange(utilizationToRemove);
            minLoadThread.UtilizationByProcessList.AddRange(utilizationToRemove);

            unloadedCore = _cpuCores.FirstOrDefault(core => core.TaskCount == 0);
            maxLoadThread = _threadPool.MaxBy(thread => thread.NormTaskCount)!;
            minLoadThread = unloadedCore?.Threads.Th0 ?? _threadPool.MinBy(thread => thread.NormTaskCount)!;

            deltaLoad = (maxLoadThread.NormTaskCount - minLoadThread.NormTaskCount) / 2.0;
            if (deltaLoad < 1e-12) return;
        }
    }

    public List<double> UsagePerThreadList => _threadPool.Select(thread => 1.0 - thread.IdlePercent).ToList();
    public List<double> NormTaskPerThreadList => _threadPool.Select(thread => thread.NormTaskCount).ToList();
}

// var (cpuModel, isDualSocket) = CoveringConfig.Value;
// return cpuModel.Price * (isDualSocket ? 2 : 1) +
//        (isDualSocket ? CurSettings.DualSocketMBoardPrice : CurSettings.OneSocketMBoardPrice);

// public decimal Cost => Convert.ToDecimal(
//     (Math.Exp(ClockRate / 5.0) * 1.0 / ThreadCount +
//      Math.Exp(ClockRate / 3.0) * (ThreadCount - 1.0) / ThreadCount) *
//     Math.Pow(ThreadCount, 0.87)
//     * 1500);