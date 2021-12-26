using System.ComponentModel;

namespace PcbDesignCADSimuModeling.Models.OptimizationModule
{
    public class AlgorithmParameters : INotifyPropertyChanged
    {
        public IntervalsOfVariables SearchIntervals { get; set; } = new();

        public int PopulationSize { get; set; } = 30;
        public int NumOfIterations { get; set; } = 200;

        public double InitTemperature { get; set; } = 200;

        public double MinimalTemperature { get; set; } = 1;

        public double Alpha { get; set; } = 1.0;


        public AlgorithmParameters Copy()
        {
            var algorithmParameters = (AlgorithmParameters)MemberwiseClone();
            algorithmParameters.SearchIntervals = SearchIntervals.Copy();
            return algorithmParameters;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public class IntervalsOfVariables : INotifyPropertyChanged
    {
        public double X1Low { get; set; } = 1;
        public double X1Up { get; set; } = 100;

        public double X2Low { get; set; } = 1.2;
        public double X2Up { get; set; } = 4.5;

        public double X3Low { get; set; } = 25;
        public double X3Up { get; set; } = 200;

        public double X4Low { get; set; } = 0;
        public double X4Up { get; set; } = 1;

        public double X5Low { get; set; } = 0;
        public double X5Up { get; set; } = 1;

        public double X6Low { get; set; } = 1;
        public double X6Up { get; set; } = 10;

        public IntervalsOfVariables Copy() => (IntervalsOfVariables)MemberwiseClone();

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}