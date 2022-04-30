﻿using PcbDesignSimuModeling.Core.Models.Technologies.PcbDesign;

namespace PcbDesignSimuModeling.Core.Models.Resources.Algorithms.WireRoutingAlgorithms;

public interface IWireRoutingAlgorithm : IPcbAlgorithm
{
}

public class WireRoutingMultiThreadAlgorithm : MultiThreadPcbAlgorithm, IWireRoutingAlgorithm
{
    public WireRoutingMultiThreadAlgorithm(long totalComplexity, int maxThreadUtilization, double parallelismRatio)
        : base(totalComplexity, maxThreadUtilization, parallelismRatio)
    {
    }

    public static WireRoutingMultiThreadAlgorithm WireRoutingChannel(PcbParams pcbParams) => new(
        totalComplexity: (long)Math.Round(
            pcbParams.ElementsCount * pcbParams.ElementsCount / Math.Log2(pcbParams.ElementsCount) /
            Math.Sqrt(Math.Log2(pcbParams.ElementsCount)) * 2.25 * Math.Sqrt(2)
            * (Math.Exp(1.0 / (1.0 - (Math.Pow(pcbParams.DimensionUsagePercent, 1.1) - 0.3))) / 2.15811)
            * (pcbParams.IsVariousSize ? 2.0 : 1.0)
            * 150 * 2.0 * 1.25),
        maxThreadUtilization: 12,
        parallelismRatio: 0.8
    );
}

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