using System.Collections;
using System.ComponentModel;
using MathNet.Numerics.LinearAlgebra.Double.Solvers;

namespace PcbDesignSimuModeling.Core.Models.Resources;

public abstract class MixedResource : IResource
{
    protected virtual List<int> UtilizingProcIds { get; } = new();
    public abstract double ResValueForProc(int requestId);
    public abstract void FreeResource(int requestId);
    public abstract IResource Clone();
    public abstract double Cost { get; }
}

public class CpuCore
{
    public (CpuThread Th0, CpuThread Th1) Threads { get; }

    public CpuCore() => Threads = (new CpuThread(this), new CpuThread(this));

    public int ActiveTasks => Threads.Th0.ActiveTasks + Threads.Th1.ActiveTasks;
    public double IdlePercent => (Threads.Th0.IdlePercent + Threads.Th1.IdlePercent) / 2.0;
    public bool IsInHyperThreadingMode => Threads is {Th0.ActiveTasks: > 0, Th1.ActiveTasks: > 0};
}

public class CpuThread
{
    public CpuCore CpuCore { get; }

    public CpuThread(CpuCore cpuCore) => CpuCore = cpuCore;

    public ThreadUtilizationList UtilizationByProcessList { get; set; } = new();

    public int ActiveTasks => UtilizationByProcessList.ActiveTasks;
    public double RealTaskCount => UtilizationByProcessList.RealTaskCount;
    public double IdlePercent => UtilizationByProcessList.IdlePercent;
    public bool IsInHyperThreadingMode => CpuCore.IsInHyperThreadingMode;
}

public class ThreadUtilizationByProcess
{
    public ThreadUtilizationByProcess(int processId, double maxUtil = 1.0)
    {
        ProcessId = processId;
        MaxUtil = maxUtil;
    }

    public int ProcessId { get; }
    public double CurUtil { get; set; } = 0.0;
    public double MaxUtil { get; }
}

public static class ThreadUtilizationListExtensions
{
    internal static ThreadUtilizationList ToThreadUtilizationList(
        this IEnumerable<ThreadUtilizationByProcess> enumerable) => new(enumerable);
}

public class ThreadUtilizationList : IList<ThreadUtilizationByProcess>
{
    private readonly List<ThreadUtilizationByProcess> _listImplementation;

    public ThreadUtilizationList() => _listImplementation = new List<ThreadUtilizationByProcess>();

    public ThreadUtilizationList(IEnumerable<ThreadUtilizationByProcess> collection) =>
        _listImplementation = new List<ThreadUtilizationByProcess>(collection);

    public ThreadUtilizationList(int capacity) => _listImplementation = new List<ThreadUtilizationByProcess>(capacity);


    public void Add(ThreadUtilizationByProcess item)
    {
        var index = _listImplementation
            .TakeWhile(threadByProcUtilization => item.MaxUtil - 1e-12 > threadByProcUtilization.MaxUtil).Count();
        _listImplementation.Insert(index, item);

        RecalculateUtilization();
    }

    public void AddRange(IEnumerable<ThreadUtilizationByProcess> collection)
    {
        _listImplementation.AddRange(collection);
        RecalculateUtilization();
    }

    public bool Remove(ThreadUtilizationByProcess item)
    {
        var result = _listImplementation.Remove(item);
        if (result) RecalculateUtilization();
        return result;
    }

    public int RemoveRange(IEnumerable<ThreadUtilizationByProcess> items)
    {
        var deletedCount = items.Count(item => _listImplementation.Remove(item));
        if (deletedCount > 0) RecalculateUtilization();
        return deletedCount;
    }

    public int RemoveAll(Predicate<ThreadUtilizationByProcess> match)
    {
        var result = _listImplementation.RemoveAll(match);
        if (result > 0) RecalculateUtilization();
        return result;
    }

    public void RecalculateUtilization()
    {
        var taskCount = ActiveTasks;
        if (taskCount < 1)
        {
            RealTaskCount = 0.0;
            IdlePercent = 1.0;
            return;
        }

        _listImplementation.ForEach(utilization => utilization.CurUtil = 1.0 / taskCount);

        var shift = 0;
        while (shift < taskCount)
        {
            var overheadSum = 0.0;

            while (shift < taskCount)
            {
                var utilization = _listImplementation[shift];
                var utilOverhead = utilization.CurUtil - utilization.MaxUtil;
                if (utilOverhead <= 1e-12) break;

                utilization.CurUtil -= utilOverhead;
                overheadSum += utilOverhead;
                shift++;
            }

            if (overheadSum <= 1e-12) break;
            var overheadAvg = overheadSum / (taskCount - shift);

            for (var i = shift; i < taskCount; i++)
                _listImplementation[i].CurUtil += overheadAvg;
        }

        RealTaskCount = 1.0 + _listImplementation.Sum(utilization => utilization.MaxUtil - 1.0 / taskCount);
        IdlePercent = Math.Max(0.0, 1.0 - RealTaskCount);
    }

    public double IdlePercent { get; private set; }

    public double RealTaskCount { get; private set; }

    public int ActiveTasks => Count;

    #region DelegateToList

    public IEnumerator<ThreadUtilizationByProcess> GetEnumerator() => _listImplementation.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _listImplementation).GetEnumerator();

    public void Clear() => _listImplementation.Clear();

    public bool Contains(ThreadUtilizationByProcess item) => _listImplementation.Contains(item);

    public void CopyTo(ThreadUtilizationByProcess[] array, int arrayIndex) =>
        _listImplementation.CopyTo(array, arrayIndex);

    public int Count => _listImplementation.Count;

    public bool IsReadOnly => ((IList<ThreadUtilizationByProcess>) _listImplementation).IsReadOnly;

    public int IndexOf(ThreadUtilizationByProcess item) => _listImplementation.IndexOf(item);

    public void Insert(int index, ThreadUtilizationByProcess item) => _listImplementation.Insert(index, item);

    public void RemoveAt(int index) => _listImplementation.RemoveAt(index);

    public ThreadUtilizationByProcess this[int index]
    {
        get => _listImplementation[index];
        set => _listImplementation[index] = value;
    }

    #endregion
}

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
        _hyperThreadingPenalty = 1;
        _contextSwitchPenalty = contextSwitchPenalty;

        _cpuCores = Enumerable.Range(0, ThreadCount / 2).Select(_ => new CpuCore()).ToArray();
        _threadPool = _cpuCores.SelectMany(core => new[] {core.Threads.Th0, core.Threads.Th1}).ToArray();
    }


    public int ThreadCount { get; set; }
    public double ClockRate { get; set; }


    public bool TryGetResource(int procId, int reqThreadCount, double avgOneThreadUtil = 1.0)
    {
        var threadsList = new List<CpuThread>();

        for (var i = 0; i < reqThreadCount; i++)
        {
            var unloadedCore = _cpuCores.FirstOrDefault(core => core.ActiveTasks == 0);
            var optimalThread = unloadedCore?.Threads.Th0 ?? _threadPool.MinBy(thread => thread.RealTaskCount)!;

            optimalThread.UtilizationByProcessList.Add(new ThreadUtilizationByProcess(procId, avgOneThreadUtil));
            threadsList.Add(optimalThread);
        }

        _processIdAndThreads.Add(procId, threadsList);
        BalanceRes();
        return true;
    }

    public override double ResValueForProc(int requestId)
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
                    thread.CpuCore.IdlePercent * _hyperThreadingPenalty + (1.0 - thread.CpuCore.IdlePercent);

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


    protected virtual void BalanceRes()
    {
        (CpuThread Thread, double RealTaskCount)
            maxLoadThread = _threadPool.MaxByAndKey(thread => thread.RealTaskCount);
        (CpuThread Thread, double RealTaskCount)
            minLoadThread = _threadPool.MinByAndKey(thread => thread.RealTaskCount);
        var deltaLoad = (maxLoadThread.RealTaskCount - minLoadThread.RealTaskCount) / 2.0;
        if (deltaLoad < 1e-12) return;

        while (deltaLoad - 1e-12 > maxLoadThread.Thread.UtilizationByProcessList.First().MaxUtil / 2.0)
        {
            var utilizationToRemove =
                maxLoadThread.Thread.UtilizationByProcessList.Take(Math.Max(1, (int) Math.Round(deltaLoad))).ToList();

            utilizationToRemove.ForEach(utilization =>
            {
                _processIdAndThreads[utilization.ProcessId].Remove(maxLoadThread.Thread);
                _processIdAndThreads[utilization.ProcessId].Add(minLoadThread.Thread);
            });

            maxLoadThread.Thread.UtilizationByProcessList.RemoveRange(utilizationToRemove);
            minLoadThread.Thread.UtilizationByProcessList.AddRange(utilizationToRemove);

            maxLoadThread = _threadPool.MaxByAndKey(thread => thread.RealTaskCount);
            minLoadThread = _threadPool.MinByAndKey(thread => thread.RealTaskCount);
            deltaLoad = (maxLoadThread.RealTaskCount - minLoadThread.RealTaskCount) / 2.0;
        }
    }


    public override IResource Clone() => new CpuCluster(this.ThreadCount, this.ClockRate);

    public override double Cost => Math.Round(
        (Math.Exp(ClockRate / 5.0) * 1.0 / ThreadCount +
         Math.Exp(ClockRate / 3.0) * (ThreadCount - 1.0) / ThreadCount) *
        Math.Pow(ThreadCount, 0.87)
        * 1500);

    public event PropertyChangedEventHandler? PropertyChanged;
}