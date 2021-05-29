using System;
using PCBdesignCADSimuModeling.Models.Technologies.PcbDesign;

namespace PCBdesignCADSimuModeling.Models.Resources.Algorithms
{
    public class OneThreadPcbAlgorithm : PcbAlgorithm
    {
        protected OneThreadPcbAlgorithm(IComplexityEstimator complexityEstimator) : base(complexityEstimator, 1)
        {
        }
        
        
        
        public sealed override TimeSpan UpdateModelTime(TimeSpan deltaTime, double cpuPower) => base.UpdateModelTime(deltaTime, cpuPower);
    }
}