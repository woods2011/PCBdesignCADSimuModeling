using System;

namespace PCBdesignCADSimuModeling.Models.Technologies.PcbDesign
{
    public class PcbParams
    {
        public PcbParams(int elementsCount, double dimensionUsagePercent, bool isVariousSize)
        {
            ElementsCount = elementsCount;
            DimensionUsagePercent = Math.Clamp(dimensionUsagePercent, 0.0, 1.0);
            IsVariousSize = isVariousSize;
        }

        public int ElementsCount { get; }
        public double DimensionUsagePercent { get; }
        public bool IsVariousSize { get; }
    }
}