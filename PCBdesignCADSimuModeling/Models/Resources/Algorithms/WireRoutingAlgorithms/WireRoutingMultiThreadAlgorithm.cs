namespace PCBdesignCADSimuModeling.Models.Resources.Algorithms.WireRoutingAlgorithms
{
    public class WireRoutingMultiThreadAlgorithm : MultiThreadPcbAlgorithm, IWireRoutingAlgorithm
    {
        public WireRoutingMultiThreadAlgorithm(IComplexityEstimator complexityEstimator, int maxThreadUtilization, double parallelismRatio) : base(complexityEstimator, maxThreadUtilization, parallelismRatio)
        {
        }
    }
}