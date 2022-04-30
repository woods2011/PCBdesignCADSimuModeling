using PcbDesignSimuModeling.Core.Models.Technologies.PcbDesign;

namespace PcbDesignSimuModeling.Core.Models.Resources.Algorithms.WireRoutingAlgorithms;

public interface IWireRoutingAlgFactory
{
    public IWireRoutingAlgorithm Create(PcbParams pcbInfo);
}

public class WireRoutingAlgFactory : IWireRoutingAlgFactory
{
    private readonly Func<PcbParams, IWireRoutingAlgorithm> _funcWireRouting;

    public WireRoutingAlgFactory(Func<PcbParams, IWireRoutingAlgorithm> funcWireRouting) => _funcWireRouting = funcWireRouting;

    public IWireRoutingAlgorithm Create(PcbParams pcbInfo) => _funcWireRouting(pcbInfo);
}