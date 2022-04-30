using PcbDesignSimuModeling.Core.Models.Technologies.PcbDesign;

namespace PcbDesignSimuModeling.Core.Models.Resources.Algorithms.WireRoutingAlgorithms;

public interface IWireRoutingAlgFactory
{
    public IWireRoutingAlgorithm Create(PcbParams pcbInfo);
}