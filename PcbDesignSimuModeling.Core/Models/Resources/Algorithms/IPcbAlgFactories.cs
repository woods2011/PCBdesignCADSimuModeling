using PcbDesignSimuModeling.Core.Models.Resources.Algorithms.PlacingAlgorithms;
using PcbDesignSimuModeling.Core.Models.Resources.Algorithms.WireRoutingAlgorithms;

namespace PcbDesignSimuModeling.Core.Models.Resources.Algorithms;

public interface IPcbAlgFactories
{
    public IPlacingAlgFactory PlacingAlgFactory { get; }
    public IWireRoutingAlgFactory WireRoutingAlgFactory { get; }
}