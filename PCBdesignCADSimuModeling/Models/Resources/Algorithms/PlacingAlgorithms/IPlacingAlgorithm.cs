namespace PCBdesignCADSimuModeling.Models.Resources.Algorithms.PlacingAlgorithms
{
    public interface IPlacingAlgorithm : IPcbAlgorithm
    {
    }

    
    
    public class PlacingMultiThreadAlgorithm : MultiThreadPcbAlgorithm, IPlacingAlgorithm
    {
        public PlacingMultiThreadAlgorithm(IComplexityEstimator complexityEstimator, int maxThreadUtilization,
            double parallelismRatio) : base(complexityEstimator, maxThreadUtilization, parallelismRatio)
        {
        }
    }

    
    
    public class PlacingOneThreadAlgorithm : OneThreadPcbAlgorithm, IPlacingAlgorithm
    {
        protected PlacingOneThreadAlgorithm(IComplexityEstimator complexityEstimator) : base(complexityEstimator)
        {
        }
    }
}