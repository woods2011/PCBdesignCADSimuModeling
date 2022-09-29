namespace PcbDesignSimuModeling.WPF.Models.Resources.CpuClusterRes.SubClasses;

public class CpuThread
{
    public CpuCore CpuCore { get; }

    public CpuThread(CpuCore cpuCore) => CpuCore = cpuCore;

    public ThreadUtilizationList UtilizationByProcessList { get; set; } = new();

    public int TaskCount => UtilizationByProcessList.ActiveTasks;
    public double NormTaskCount => UtilizationByProcessList.RealTaskCount;
    public double IdlePercent => UtilizationByProcessList.IdlePercent;
    public bool IsInHyperThreadingMode => CpuCore.IsInHyperThreadingMode;
}