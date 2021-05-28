using System;
using PCBdesignCADSimuModeling.Models.Technologies.PcbDesign;

namespace PCBdesignCADSimuModeling.Models.Resources.Algorithms
{
    public class OneThreadPlacementAlgorithm : PcbAlgorithm
    {
        protected OneThreadPlacementAlgorithm(PcbParams pcb, IComplexityEstimator complexityEstimator) : base(pcb, complexityEstimator, 1)
        {
        }
        
        
        
        public sealed override TimeSpan UpdateModelTime(TimeSpan deltaTime, int cpuPower) => base.UpdateModelTime(deltaTime, cpuPower);
    }
}