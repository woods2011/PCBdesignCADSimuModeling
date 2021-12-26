using PcbDesignCADSimuModeling.Models.Resources.Algorithms.PlacingAlgorithms;
using PcbDesignCADSimuModeling.Models.Resources.Algorithms.WireRoutingAlgorithms;

namespace PcbDesignCADSimuModeling.Models.Resources.Algorithms
{
    public interface IPcbAlgFactories
    {
        public IPlacingAlgFactory PlacingAlgFactory { get; }
        public IWireRoutingAlgFactory WireRoutingAlgFactory { get; }
    }

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
}