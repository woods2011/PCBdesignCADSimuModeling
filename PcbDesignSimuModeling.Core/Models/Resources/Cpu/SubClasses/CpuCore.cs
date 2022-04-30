namespace PcbDesignSimuModeling.Core.Models.Resources.Cpu.SubClasses;

public class CpuCore
{
    public (CpuThread Th0, CpuThread Th1) Threads { get; }

    public CpuCore() => Threads = (new CpuThread(this), new CpuThread(this));

    public int ActiveTasks => Threads.Th0.ActiveTasks + Threads.Th1.ActiveTasks;
    public double IdlePercent => (Threads.Th0.IdlePercent + Threads.Th1.IdlePercent) / 2.0;
    public bool IsInHyperThreadingMode => Threads is {Th0.ActiveTasks: > 0, Th1.ActiveTasks: > 0};
}