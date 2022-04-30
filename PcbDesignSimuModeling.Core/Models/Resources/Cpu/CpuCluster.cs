﻿using System.ComponentModel;
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