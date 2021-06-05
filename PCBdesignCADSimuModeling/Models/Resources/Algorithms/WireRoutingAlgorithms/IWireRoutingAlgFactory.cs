using System;
using PCBdesignCADSimuModeling.Models.Technologies.PcbDesign;

namespace PCBdesignCADSimuModeling.Models.Resources.Algorithms.WireRoutingAlgorithms
{
    public interface IWireRoutingAlgFactory
    {
        public IWireRoutingAlgorithm Create(PcbParams pcbInfo);

        
        public static readonly IWireRoutingAlgFactory WireRoutingWave =
            new WireRoutingAlgFactory(pcbParams =>
                new WireRoutingOneThreadAlgorithm(new WireRoutingWaveCxtyEst(pcbParams)));
        
        public static readonly IWireRoutingAlgFactory WireRoutingChannel =
            new WireRoutingAlgFactory(pcbParams =>
                new WireRoutingMultiThreadAlgorithm(new WireRoutingChannelCxtyEst(pcbParams), 16, 0.8));
    }

    public class WireRoutingAlgFactory : IWireRoutingAlgFactory
    {
        private readonly Func<PcbParams, IWireRoutingAlgorithm> _funcWireRouting;

        public WireRoutingAlgFactory(Func<PcbParams, IWireRoutingAlgorithm> funcWireRouting)
        {
            _funcWireRouting = funcWireRouting;
        }

        public IWireRoutingAlgorithm Create(PcbParams pcbInfo)
        {
            return _funcWireRouting(pcbInfo);
        }
    }
}