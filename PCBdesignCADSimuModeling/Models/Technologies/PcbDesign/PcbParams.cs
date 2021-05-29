namespace PCBdesignCADSimuModeling.Models.Technologies.PcbDesign
{
    public class PcbParams
    {
        public int ElementsCount { get; init; }

        public (int, int) Dimensions { get; init; }

        public bool IsVariousSize { get; init; }
    }
}