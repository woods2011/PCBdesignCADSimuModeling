using PcbDesignSimuModeling.Core.Models.Technologies.PcbDesign;

namespace PcbDesignSimuModeling.Core.Models.Resources.Algorithms.WireRoutingAlgorithms;

public class WireRoutingOneThreadAlgorithm : OneThreadPcbAlgorithm, IWireRoutingAlgorithm
{
    public WireRoutingOneThreadAlgorithm(long totalComplexity) : base(totalComplexity)
    {
    }

    public static WireRoutingOneThreadAlgorithm WireRoutingWave(PcbParams pcbParams) => new(
        totalComplexity: (long)Math.Round(
            pcbParams.ElementsCount * Math.Log2(pcbParams.ElementsCount) * Math.Log2(pcbParams.ElementsCount) /
            4.0 *
            (Math.Exp(1.0 / (1.0 - (Math.Pow(pcbParams.DimensionUsagePercent, 1.1) - 0.3))) / 2.15811) *
            (pcbParams.IsVariousSize ? 1.5 : 1.0)
            * 170 * 2.0)
    );
}