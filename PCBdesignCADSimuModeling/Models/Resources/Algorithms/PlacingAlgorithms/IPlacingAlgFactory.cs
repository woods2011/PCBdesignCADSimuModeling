using System;
using PCBdesignCADSimuModeling.Models.Technologies.PcbDesign;

namespace PCBdesignCADSimuModeling.Models.Resources.Algorithms.PlacingAlgorithms
{
    public interface IPlacingAlgFactory
    {
        public IPlacingAlgorithm Create(PcbParams pcbInfo);
        
        public static readonly IPlacingAlgFactory ExampleTrace =
            new PlacingAlgFactory(pcbParams =>
                new PlacingMultiThreadAlgorithm(new WireRoutingExampleCxtyEst(pcbParams), 8, 0.7));
    }
    
    
    
    public class PlacingAlgFactory : IPlacingAlgFactory
    {
        private readonly Func<PcbParams, IPlacingAlgorithm> _funcPlacing;

        public PlacingAlgFactory(Func<PcbParams, IPlacingAlgorithm> funcPlacing)
        {
            _funcPlacing = funcPlacing;
        }

        public IPlacingAlgorithm Create(PcbParams pcbInfo)
        {
            return _funcPlacing(pcbInfo);
        }
    }
}