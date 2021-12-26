using System;
using PcbDesignCADSimuModeling.Models.Technologies.PcbDesign;

namespace PcbDesignCADSimuModeling.Models.Resources.Algorithms.PlacingAlgorithms
{
    public interface IPlacingAlgFactory
    {
        public IPlacingAlgorithm Create(PcbParams pcbInfo);
    }
    
    
    
    public class PlacingAlgFactory : IPlacingAlgFactory
    {
        private readonly Func<PcbParams, IPlacingAlgorithm> _funcPlacing;

        public PlacingAlgFactory(Func<PcbParams, IPlacingAlgorithm> funcPlacing)
        {
            _funcPlacing = funcPlacing;
        }

        public IPlacingAlgorithm Create(PcbParams pcbInfo) => _funcPlacing(pcbInfo);
    }
}