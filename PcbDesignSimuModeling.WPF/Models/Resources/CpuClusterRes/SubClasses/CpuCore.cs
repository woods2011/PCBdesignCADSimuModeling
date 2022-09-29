namespace PcbDesignSimuModeling.WPF.Models.Resources.CpuClusterRes.SubClasses;

public class CpuCore
{
    public (CpuThread Th0, CpuThread Th1) Threads { get; }

    public CpuCore() => Threads = (new CpuThread(this), new CpuThread(this));

    public int TaskCount => Threads.Th0.TaskCount + Threads.Th1.TaskCount;
    public double IdlePercent => (Threads.Th0.IdlePercent + Threads.Th1.IdlePercent) / 2.0;
    public bool IsInHyperThreadingMode => Threads is {Th0.TaskCount: > 0, Th1.TaskCount: > 0};
}