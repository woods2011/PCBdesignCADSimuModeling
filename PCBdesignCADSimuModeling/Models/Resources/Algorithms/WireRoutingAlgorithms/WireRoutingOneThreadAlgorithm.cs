namespace PCBdesignCADSimuModeling.Models.Resources.Algorithms.WireRoutingAlgorithms
{
    public class WireRoutingOneThreadAlgorithm : OneThreadPcbAlgorithm, IWireRoutingAlgorithm
    {
        protected WireRoutingOneThreadAlgorithm(IComplexityEstimator complexityEstimator) : base(complexityEstimator)
        {
        }
    }
}