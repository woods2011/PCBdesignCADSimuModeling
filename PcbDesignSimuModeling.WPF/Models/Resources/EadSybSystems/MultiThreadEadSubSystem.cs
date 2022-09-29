using System;

namespace PcbDesignSimuModeling.WPF.Models.Resources.EadSybSystems;

public class MultiThreadEadSubSystem : EadSubSystem
{
    public MultiThreadEadSubSystem(long totalComplexity, int maxThreadUtilization) : base(totalComplexity, maxThreadUtilization)
    {
    }

    public sealed override void UpdateModelTime(TimeSpan deltaTime, double cpuPower) =>
        base.UpdateModelTime(deltaTime, cpuPower);

    public sealed override TimeSpan EstimateEndTime(double cpuPower) =>
        base.EstimateEndTime(cpuPower);
}