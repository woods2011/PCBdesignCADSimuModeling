using System;
using PCBdesignCADSimuModeling.Models.Technologies.PcbDesign;

namespace PCBdesignCADSimuModeling.Models.Resources.Algorithms
{
    public interface IComplexityEstimator
    {
        int EstimateComplexity();
    }
    
    public class ComplexityEstimator : IComplexityEstimator
    {
        private readonly PcbParams _pcbInfo;
        private readonly Func<PcbParams, int> _complexityByPcbInfoConvolution;

        public ComplexityEstimator(PcbParams pcbInfo, Func<PcbParams, int> complexityByPcbInfoConvolution)
        {
            _pcbInfo = pcbInfo;
            _complexityByPcbInfoConvolution = complexityByPcbInfoConvolution;
        }

        public int EstimateComplexity() => _complexityByPcbInfoConvolution(_pcbInfo);
    }

    public class PlacingExampleCxtyEst : ComplexityEstimator
    {
        public PlacingExampleCxtyEst(PcbParams pcbInfo) : base(pcbInfo,
            info => info.ElementsCount * 1000
        )
        {
        }
    }
        
    public class WireRoutingExampleCxtyEst : ComplexityEstimator
    {
        public WireRoutingExampleCxtyEst(PcbParams pcbInfo) : base(pcbInfo,
            info => info.ElementsCount * 1010
        )
        {
        }
    }
}