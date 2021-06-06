using System;
using PCBdesignCADSimuModeling.Models.Technologies.PcbDesign;

namespace PCBdesignCADSimuModeling.Models.Resources.Algorithms
{
    public interface IComplexityEstimator
    {
        long EstimateComplexity();
    }

    public class ComplexityEstimator : IComplexityEstimator
    {
        private readonly PcbParams _pcbInfo;
        private readonly Func<PcbParams, long> _complexityByPcbInfoConvolution;

        public ComplexityEstimator(PcbParams pcbInfo, Func<PcbParams, long> complexityByPcbInfoConvolution)
        {
            _pcbInfo = pcbInfo;
            _complexityByPcbInfoConvolution = complexityByPcbInfoConvolution;
        }

        public long EstimateComplexity() => _complexityByPcbInfoConvolution(_pcbInfo);
    }

    public class PlacingSequentialCxtyEst : ComplexityEstimator
    {
        public PlacingSequentialCxtyEst(PcbParams pcbInfo) : base(pcbInfo,
            pcb => (long) Math.Round(
                pcb.ElementsCount * Math.Log2(pcb.ElementsCount) * Math.Log2(pcb.ElementsCount) / 4.0 *
                (Math.Exp(1.0 / (1.0 - (Math.Pow(pcb.DimensionUsagePercent, 1.1) - 0.3))) / 2.15811) *
                (pcb.IsVariousSize ? 2.0 : 1.0)
                * 60.0
            ))
        {
        }
    }

    public class PlacingPartitioningCxtyEst : ComplexityEstimator
    {
        public PlacingPartitioningCxtyEst(PcbParams pcbInfo) : base(pcbInfo,
            pcb => (long) Math.Round(
                pcb.ElementsCount * pcb.ElementsCount / Math.Log2(pcb.ElementsCount) / Math.Sqrt(Math.Log2(pcb.ElementsCount)) * 2.25 * Math.Sqrt(2) *
                (Math.Exp(1.0 / (1.0 - (Math.Pow(pcb.DimensionUsagePercent, 1.1) - 0.3))) / 2.15811) *
                (pcb.IsVariousSize ? 2.5 : 1.0) 
                * 60.0
            ))
        {
        }
    }


    public class WireRoutingWaveCxtyEst : ComplexityEstimator
    {
        public WireRoutingWaveCxtyEst(PcbParams pcbInfo) : base(pcbInfo,
            pcb => (long) Math.Round(
                pcb.ElementsCount * Math.Log2(pcb.ElementsCount) * Math.Log2(pcb.ElementsCount) / 4.0 *
                (Math.Exp(1.0 / (1.0 - (Math.Pow(pcb.DimensionUsagePercent, 1.1) - 0.3))) / 2.15811) *
                (pcb.IsVariousSize ? 2.0 : 1.0)
                * 60 * 2.0
            ))
        {
        }
    }
    
    
    public class  WireRoutingChannelCxtyEst : ComplexityEstimator
    {
        public WireRoutingChannelCxtyEst(PcbParams pcbInfo) : base(pcbInfo,
            pcb => (long) Math.Round(
                pcb.ElementsCount * pcb.ElementsCount / Math.Log2(pcb.ElementsCount) / Math.Sqrt(Math.Log2(pcb.ElementsCount)) * 2.25 * Math.Sqrt(2) *
                (Math.Exp(1.0 / (1.0 - (Math.Pow(pcb.DimensionUsagePercent, 1.1) - 0.3))) / 2.15811) *
                (pcb.IsVariousSize ? 2.2 : 1) 
                * 60 * 2.0
            ))
        {
        }
    }
}