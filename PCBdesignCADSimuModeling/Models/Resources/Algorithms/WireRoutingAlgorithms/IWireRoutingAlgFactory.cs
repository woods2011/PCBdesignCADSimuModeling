using System;
using PCBdesignCADSimuModeling.Models.Technologies.PcbDesign;

namespace PCBdesignCADSimuModeling.Models.Resources.Algorithms.WireRoutingAlgorithms
{
    public interface IWireRoutingAlgFactory
    {
        public IWireRoutingAlgorithm Create(PcbParams pcbInfo);

        public static readonly IWireRoutingAlgFactory ExampleWireRouting =
            new WireRoutingAlgFactory(pcbParams =>
                new WireRoutingMultiThreadAlgorithm(new WireRoutingExampleCxtyEst(pcbParams), 8, 0.7));
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