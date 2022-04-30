namespace PcbDesignSimuModeling.Core.Models.Resources.Cpu.SubClasses;

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