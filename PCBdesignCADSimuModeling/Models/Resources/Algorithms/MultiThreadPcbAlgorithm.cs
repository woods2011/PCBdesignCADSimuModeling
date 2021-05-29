using System;
using PCBdesignCADSimuModeling.Models.Technologies.PcbDesign;

namespace PCBdesignCADSimuModeling.Models.Resources.Algorithms
{
    public class MultiThreadPcbAlgorithm : PcbAlgorithm
    {
        private readonly double _parallelismRatio;


        public MultiThreadPcbAlgorithm(IComplexityEstimator complexityEstimator, int maxThreadUtilization, double parallelismRatio) : base(complexityEstimator, maxThreadUtilization)
        {
            _parallelismRatio = parallelismRatio;
        }

        
        
        public sealed override TimeSpan UpdateModelTime(TimeSpan deltaTime, double cpuPower) => base.UpdateModelTime(deltaTime, _parallelismRatio * cpuPower);
    }
}