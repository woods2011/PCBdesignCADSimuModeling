namespace PCBdesignCADSimuModeling.Models.Technologies.PcbDesign
{
    public class PcbParams
    {
        public int ElementsCount { get; set; }
        
        public (int, int) Dimensions { get; set; }

        public bool IsVariousSize { get; set; }
    }
}