namespace PCBdesignCADSimuModeling.Models.Resources.Algorithms.PlacingAlgorithms
{
    public class PlacingMultiThreadAlgorithm : MultiThreadPcbAlgorithm, IPlacingAlgorithm
    {
        public PlacingMultiThreadAlgorithm(IComplexityEstimator complexityEstimator, int maxThreadUtilization, double parallelismRatio) : base(complexityEstimator, maxThreadUtilization, parallelismRatio)
        {
        }
    }
}