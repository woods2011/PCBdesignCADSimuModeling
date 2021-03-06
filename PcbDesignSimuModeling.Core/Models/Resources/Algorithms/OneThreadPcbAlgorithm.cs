namespace PcbDesignSimuModeling.Core.Models.Resources.Algorithms;

public class OneThreadPcbAlgorithm : PcbAlgorithm
{
    protected OneThreadPcbAlgorithm(long totalComplexity) : base(totalComplexity, 1)
    {
    }


    public sealed override void UpdateModelTime(TimeSpan deltaTime, double cpuPower) =>
        base.UpdateModelTime(deltaTime, cpuPower);
        
        
    public sealed override TimeSpan EstimateEndTime(double cpuPower) =>
        base.EstimateEndTime(cpuPower);
}