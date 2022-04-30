using PcbDesignSimuModeling.Core.Models.Resources.Algorithms.PlacingAlgorithms;
using PcbDesignSimuModeling.Core.Models.Resources.Algorithms.WireRoutingAlgorithms;

namespace PcbDesignSimuModeling.Core.Models.Resources.Algorithms;

public class PcbAlgFactories : IPcbAlgFactories
{
    public IPlacingAlgFactory PlacingAlgFactory { get; set; }
    public IWireRoutingAlgFactory WireRoutingAlgFactory { get; set; }

    public PcbAlgFactories(IPlacingAlgFactory placingAlgFactory, IWireRoutingAlgFactory wireRoutingAlgFactory)
    {
        PlacingAlgFactory = placingAlgFactory;
        WireRoutingAlgFactory = wireRoutingAlgFactory;
    }
}