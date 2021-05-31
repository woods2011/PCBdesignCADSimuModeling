namespace PCBdesignCADSimuModeling.Models.Resources.Algorithms.WireRoutingAlgorithms
{
    public interface IWireRoutingAlgorithm : IPcbAlgorithm
    {
    }
    
    public class WireRoutingMultiThreadAlgorithm : MultiThreadPcbAlgorithm, IWireRoutingAlgorithm
    {
        public WireRoutingMultiThreadAlgorithm(IComplexityEstimator complexityEstimator, int maxThreadUtilization, double parallelismRatio) : base(complexityEstimator, maxThreadUtilization, parallelismRatio)
        {
        }
    }
    
    public class WireRoutingOneThreadAlgorithm : OneThreadPcbAlgorithm, IWireRoutingAlgorithm
    {
        protected WireRoutingOneThreadAlgorithm(IComplexityEstimator complexityEstimator) : base(complexityEstimator)
        {
        }
    }
}