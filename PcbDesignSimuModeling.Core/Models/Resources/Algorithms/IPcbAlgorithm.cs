namespace PcbDesignSimuModeling.Core.Models.Resources.Algorithms;

public interface IPcbAlgorithm
{
    public int MaxThreadUtilization { get; }
    public bool IsComplete { get; }
    public void UpdateModelTime(TimeSpan deltaTime, double cpuPower);
    public TimeSpan EstimateEndTime(double cpuPower);
}