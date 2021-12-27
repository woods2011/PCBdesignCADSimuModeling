using System.ComponentModel;

namespace PcbDesignCADSimuModeling.Models.OptimizationModule
{
    public class AlgorithmSettings : INotifyPropertyChanged
    {
        public IntervalsOfVariables SearchIntervals { get; set; } = new();

        public int PopulationSize { get; set; } = 30;
        public int NumOfIterations { get; set; } = 200;

        public double InitTemperature { get; set; } = 200;

        public double Alpha { get; set; } = 1.0;


        public AlgorithmSettings Copy()
        {
            var algorithmParameters = (AlgorithmSettings)MemberwiseClone();
            algorithmParameters.SearchIntervals = SearchIntervals.Copy();
            return algorithmParameters;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public class IntervalsOfVariables : INotifyPropertyChanged
    {
        public double ThreadsCountMin { get; set; } = 1;
        public double ThreadsCountMax { get; set; } = 100;

        public double FreqMin { get; set; } = 1.2;
        public double FreqMax { get; set; } = 4.5;

        public double ServerSpeedMin { get; set; } = 25;
        public double ServerSpeedMax { get; set; } = 1000;

        public double X4Low { get; set; } = 0;
        public double X4Up { get; set; } = 1;

        public double X5Low { get; set; } = 0;
        public double X5Up { get; set; } = 1;

        public double DesignersCountMin { get; set; } = 1;
        public double DesignersCountMax { get; set; } = 10;

        public IntervalsOfVariables Copy() => (IntervalsOfVariables)MemberwiseClone();

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}