using System;
using PCBdesignCADSimuModeling.Models.Technologies.PcbDesign;

namespace PCBdesignCADSimuModeling.Models.Resources.Algorithms
{
    public class OneThreadPcbAlgorithm : PcbAlgorithm
    {
        protected OneThreadPcbAlgorithm(IComplexityEstimator complexityEstimator) : base(complexityEstimator, 1)
        {
        }

        
        public sealed override void UpdateModelTime(TimeSpan deltaTime, double cpuPower) =>
            base.UpdateModelTime(deltaTime, cpuPower);
        
        
        public sealed override TimeSpan EstimateEndTime(double cpuPower) =>
            base.EstimateEndTime(cpuPower);
    }
}