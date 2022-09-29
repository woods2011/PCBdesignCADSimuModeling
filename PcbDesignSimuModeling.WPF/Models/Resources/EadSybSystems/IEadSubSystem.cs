using System;

namespace PcbDesignSimuModeling.WPF.Models.Resources.EadSybSystems;

public interface IEadSubSystem
{
    public int MaxThreadUtilization { get; }
    public bool IsComplete { get; }
    public void UpdateModelTime(TimeSpan deltaTime, double cpuPower);
    public TimeSpan EstimateEndTime(double cpuPower);
}