using System;
using PcbDesignCADSimuModeling.Models.Technologies.PcbDesign;

namespace PcbDesignCADSimuModeling.Models.Resources.Algorithms
{
    public class MultiThreadPcbAlgorithm : PcbAlgorithm
    {
        private readonly double _parallelismRatio;


        public MultiThreadPcbAlgorithm(long totalComplexity, int maxThreadUtilization,
            double parallelismRatio) : base(totalComplexity, maxThreadUtilization)
        {
            _parallelismRatio = parallelismRatio;   
        }


        public sealed override void UpdateModelTime(TimeSpan deltaTime, double cpuPower) =>
            base.UpdateModelTime(deltaTime,
                cpuPower * (_parallelismRatio +
                            (1 - _parallelismRatio) * 1.0 / MaxThreadUtilization *
                            OneThreadBoostApprox(MaxThreadUtilization))
            );


        public sealed override TimeSpan EstimateEndTime(double cpuPower) =>
            base.EstimateEndTime(
                cpuPower * (_parallelismRatio +
                            (1 - _parallelismRatio) * 1.0 / MaxThreadUtilization *
                            OneThreadBoostApprox(MaxThreadUtilization))
            );


        // ToDo: kostil
        public static double OneThreadBoostApprox(int maxThreadUtilization) =>
            maxThreadUtilization < 2 ? 1.0 : 1.3 + 0.6 / Math.Exp(4.0 / (maxThreadUtilization - 2.0));
    }
}