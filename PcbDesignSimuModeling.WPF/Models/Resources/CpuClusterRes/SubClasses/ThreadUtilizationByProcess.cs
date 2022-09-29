namespace PcbDesignSimuModeling.WPF.Models.Resources.CpuClusterRes.SubClasses;

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