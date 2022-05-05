using System.ComponentModel;
using System.Text.Json.Serialization;
using PcbDesignSimuModeling.Core.Models.Resources.Algorithms.PlacingAlgorithms;
using PcbDesignSimuModeling.Core.Models.Resources.Algorithms.WireRoutingAlgorithms;

namespace PcbDesignSimuModeling.Core.Models.OptimizationModule;

public class AlgorithmSettings : INotifyPropertyChanged
{
    public IntervalsOfVariables SearchIntervals { get; set; } = new();

    public int FoodSourceCount { get; set; } = 30;
    public int NumOfIterations { get; set; } = 200;
    public double InitTemperature { get; set; } = 200;
    public double Alpha { get; set; } = 1.0;


    public AlgorithmSettings Copy()
    {
        var algorithmParameters = (AlgorithmSettings) MemberwiseClone();
        algorithmParameters.SearchIntervals = SearchIntervals.Copy();
        return algorithmParameters;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}

public class IntervalsOfVariables : INotifyPropertyChanged
{
    public int ThreadsCountMin { get; set; } = 2;
    public int ThreadsCountMax { get; set; } = 100;

    public double ClockRateMin { get; set; } = 1.2;
    public double ClockRateMax { get; set; } = 4.5;
    
    public double RamAmountMin { get; set; } = 4;
    public double RamAmountMax { get; set; } = 100;
    
    public double ServerSpeedMin { get; set; } = 25;
    public double ServerSpeedMax { get; set; } = 1000;


    [JsonIgnore] public int[] PlacingAlgsIndexes { get; set; } = {0, 1};

    [JsonIgnore] public int[] WireRoutingAlgsIndexes { get; set; } = {0, 1};

    public int DesignersCountMin { get; set; } = 1;
    public int DesignersCountMax { get; set; } = 10;

    public IntervalsOfVariables Copy()
    {
        var intervalsOfVariables = (IntervalsOfVariables) MemberwiseClone();
        intervalsOfVariables.PlacingAlgsIndexes = PlacingAlgsIndexes.ToArray();
        intervalsOfVariables.WireRoutingAlgsIndexes = WireRoutingAlgsIndexes.ToArray();
        return intervalsOfVariables;
    }


    public string[] PlacingAlgsStrs =>
        PlacingAlgsIndexes.Select(index => PlacingAlgProviderFactory.AlgIndexNameMap[index]).ToArray();

    public string[] WireRoutingAlgsStrs =>
        PlacingAlgsIndexes.Select(index => WireRoutingAlgProviderFactory.AlgIndexNameMap[index]).ToArray();

    public event PropertyChangedEventHandler? PropertyChanged;
}