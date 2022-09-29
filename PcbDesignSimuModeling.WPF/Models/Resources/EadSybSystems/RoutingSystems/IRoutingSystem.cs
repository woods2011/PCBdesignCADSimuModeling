using System;
using PcbDesignSimuModeling.WPF.Models.Technologies.PcbDesign;

namespace PcbDesignSimuModeling.WPF.Models.Resources.EadSybSystems.RoutingSystems;

public interface IRoutingSystem : IEadSubSystem
{
}

public class RoutingSystem : EadSubSystem, IRoutingSystem
{
    public RoutingSystem(long totalComplexity, int maxThreadUtilization) : base(totalComplexity, maxThreadUtilization)
    {
    }

    public static RoutingSystem Topor(PcbDescription pcbDescription) => new (
        totalComplexity: Convert.ToInt64(pcbDescription.PinsCount / (70.0 / 60.0) * 4 * 2 * 3),
        maxThreadUtilization: 2);
}