using System;
using PcbDesignCADSimuModeling.Models.Technologies.PcbDesign;
using PcbDesignCADSimuModeling.Models.Technologies.PcbDesign.ProjectProcedures;

namespace PcbDesignCADSimuModeling.Models.Resources.Algorithms.PlacingAlgorithms
{
    public interface IPlacingAlgorithm : IPcbAlgorithm
    {
    }


    public class PlacingMultiThreadAlgorithm : MultiThreadPcbAlgorithm, IPlacingAlgorithm
    {
        public PlacingMultiThreadAlgorithm(long totalComplexity, int maxThreadUtilization,
            double parallelismRatio) : base(totalComplexity, maxThreadUtilization, parallelismRatio)
        {
        }

        public static PlacingMultiThreadAlgorithm PlacingPartitioning(PcbParams pcbParams) => new(
            totalComplexity: (long)Math.Round(
                pcbParams.ElementsCount * pcbParams.ElementsCount / Math.Log2(pcbParams.ElementsCount) /
                Math.Sqrt(Math.Log2(pcbParams.ElementsCount)) * 2.25 * Math.Sqrt(2) *
                (Math.Exp(1.0 / (1.0 - (Math.Pow(pcbParams.DimensionUsagePercent, 1.1) - 0.3))) / 2.15811) *
                (pcbParams.IsVariousSize ? 2.5 : 1.0)
                * 60.0),
            maxThreadUtilization: 8,
            parallelismRatio: 0.75
        );
    }


    public class PlacingOneThreadAlgorithm : OneThreadPcbAlgorithm, IPlacingAlgorithm
    {
        public PlacingOneThreadAlgorithm(long totalComplexity) : base(totalComplexity)
        {
        }

        public static PlacingOneThreadAlgorithm PlacingSequential(PcbParams pcbParams) => new(
            totalComplexity: (long)Math.Round(
                pcbParams.ElementsCount * Math.Log2(pcbParams.ElementsCount) * Math.Log2(pcbParams.ElementsCount) /
                4.0 *
                (Math.Exp(1.0 / (1.0 - (Math.Pow(pcbParams.DimensionUsagePercent, 1.1) - 0.3))) / 2.15811) *
                (pcbParams.IsVariousSize ? 2.0 : 1.0)
                * 60.0)
        );
    }
}