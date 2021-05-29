namespace PCBdesignCADSimuModeling.Models.Resources.Algorithms.PlacingAlgorithms
{
    public class PlacingOneThreadAlgorithm : OneThreadPcbAlgorithm, IPlacingAlgorithm
    {
        protected PlacingOneThreadAlgorithm(IComplexityEstimator complexityEstimator) : base(complexityEstimator)
        {
        }
    }
}