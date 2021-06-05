using System;
using PCBdesignCADSimuModeling.Models.Technologies.PcbDesign;

namespace PCBdesignCADSimuModeling.Models.Resources.Algorithms.PlacingAlgorithms
{
    public interface IPlacingAlgFactory
    {
        public IPlacingAlgorithm Create(PcbParams pcbInfo);
   
        
        public static readonly IPlacingAlgFactory PlacingSequential =
            new PlacingAlgFactory(pcbParams =>
                new PlacingOneThreadAlgorithm(new PlacingSequentialCxtyEst(pcbParams)));
        
        public static readonly IPlacingAlgFactory PlacingPartitioning =
            new PlacingAlgFactory(pcbParams =>
                new PlacingMultiThreadAlgorithm(new PlacingPartitioningCxtyEst(pcbParams), 8, 0.75));
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