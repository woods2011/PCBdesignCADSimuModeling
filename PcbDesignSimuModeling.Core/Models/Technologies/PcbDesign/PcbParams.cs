namespace PcbDesignSimuModeling.Core.Models.Technologies.PcbDesign;

public class PcbParams
{
    public PcbParams(int elementsCount, double areaUsagePercent, bool isVariousSize)
    {
        ElementsCount = elementsCount;
        DimensionUsagePercent = Math.Clamp(areaUsagePercent, 0.0, 1.0);
        IsVariousSize = isVariousSize;
    }

    public int ElementsCount { get; }
    public double DimensionUsagePercent { get; }
    public bool IsVariousSize { get; }
}