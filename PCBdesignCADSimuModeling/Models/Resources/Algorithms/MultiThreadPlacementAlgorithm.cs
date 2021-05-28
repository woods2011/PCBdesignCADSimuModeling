using System;
using PCBdesignCADSimuModeling.Models.Technologies.PcbDesign;

namespace PCBdesignCADSimuModeling.Models.Resources.Algorithms
{
    public class MultiThreadPlacementAlgorithm : PcbAlgorithm
    {
        private readonly int _parallelismRatio;
        
        
        protected MultiThreadPlacementAlgorithm(PcbParams pcb, IComplexityEstimator complexityEstimator, int parallelismRatio, int maxThreadUtilization) : base(pcb, complexityEstimator, maxThreadUtilization)
        {
            _parallelismRatio = parallelismRatio;
        }

        
        
        public sealed override TimeSpan UpdateModelTime(TimeSpan deltaTime, int cpuPower) => base.UpdateModelTime(deltaTime, _parallelismRatio * cpuPower);
    }
}