using PCBdesignCADSimuModeling.Models.Resources.Algorithms.PlacingAlgorithms;
using PCBdesignCADSimuModeling.Models.Resources.Algorithms.WireRoutingAlgorithms;

namespace PCBdesignCADSimuModeling.Models.Resources.Algorithms
{
    public class PcbAlgFactories
    {
        public IPlacingAlgFactory PlacingAlgFactory { get; init; }
        public IWireRoutingAlgFactory WireRoutingAlgFactory { get; init; }
    }
}