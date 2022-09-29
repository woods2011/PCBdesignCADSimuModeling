using System;
using PcbDesignSimuModeling.WPF.Models.Technologies.PcbDesign;

namespace PcbDesignSimuModeling.WPF.Models.Resources.EadSybSystems.PlacingSystems;

public interface IPlacingSystem : IEadSubSystem
{
}

public class PlacingSystem : EadSubSystem, IPlacingSystem
{
    public PlacingSystem(long totalComplexity, int maxThreadUtilization) : base(totalComplexity, maxThreadUtilization)
    {
    }

    public static PlacingSystem Topor(PcbDescription pcbDescription) => new (
        totalComplexity: Convert.ToInt64(pcbDescription.ElementsCount / (31.0 / 60.0) * 4 * 3),
        maxThreadUtilization: 1);
}