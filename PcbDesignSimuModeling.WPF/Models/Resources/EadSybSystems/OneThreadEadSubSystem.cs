using System;

namespace PcbDesignSimuModeling.WPF.Models.Resources.EadSybSystems;

public class OneThreadEadSubSystem : EadSubSystem
{
    protected OneThreadEadSubSystem(long totalComplexity) : base(totalComplexity, 1)
    {
    }


    public sealed override void UpdateModelTime(TimeSpan deltaTime, double cpuPower) =>
        base.UpdateModelTime(deltaTime, cpuPower);
        
        
    public sealed override TimeSpan EstimateEndTime(double cpuPower) =>
        base.EstimateEndTime(cpuPower);
}