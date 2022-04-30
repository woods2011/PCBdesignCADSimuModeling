using PcbDesignSimuModeling.Core.Models.Technologies.PcbDesign;

namespace PcbDesignSimuModeling.Core.Models.Resources.Algorithms.PlacingAlgorithms;

public interface IPlacingAlgFactory
{
    public IPlacingAlgorithm Create(PcbParams pcbInfo);
}