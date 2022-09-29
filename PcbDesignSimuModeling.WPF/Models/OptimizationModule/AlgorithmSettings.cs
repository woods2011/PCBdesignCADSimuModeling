using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using static PcbDesignSimuModeling.WPF.Models.Resources.EadSybSystems.PlacingSystems.PlacingSysFactoryProvider;
using static PcbDesignSimuModeling.WPF.Models.Resources.EadSybSystems.RoutingSystems.RoutingSysFactoryProvider;

namespace PcbDesignSimuModeling.WPF.Models.OptimizationModule;

public class AlgorithmSettings : INotifyPropertyChanged
{
    [JsonProperty("Границы поиска")] public IntervalsOfVariables SearchIntervals { get; set; } = new();

    [JsonProperty("Число источников пищи")]
    public int FoodSourceCount { get; set; } = 30;

    [JsonProperty("Число итераций")] public int NumOfIterations { get; set; } = 200;

    [JsonProperty("Начальная температура")]
    public double InitTemperature { get; set; } = 200;

    [JsonProperty("Параметр Alpha")] public double Alpha { get; set; } = 1.0;


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
    [JsonProperty("Мин. Число Потоков")] public int ThreadsCountMin { get; set; } = 12;

    [JsonProperty("Макс. Число Потоков")] public int ThreadsCountMax { get; set; } = 192;

    [JsonProperty("Мин. Частота (ГГц)")] public double ClockRateMin { get; set; } = 1.9;
    [JsonProperty("Макс. Частота (ГГц)")] public double ClockRateMax { get; set; } = 4.5;


    [JsonProperty("Мин. Объем Оперативной Памяти (Гб)")]
    public double RamAmountMin { get; set; } = 12;

    [JsonProperty("Макс. Объем Оперативной Памяти (Гб)")]
    public double RamAmountMax { get; set; } = 100;


    [JsonProperty("Мин. Скорость Интернет Соединения (Мбит/c)")]
    public double ServerSpeedMin { get; set; } = 25;

    [JsonProperty("Макс. Скорость Интернет Соединения (Мбит/c)")]
    public double ServerSpeedMax { get; set; } = 1000;


    [JsonIgnore] public int[] PlacingAlgsIndexes { get; set; } = {0};

    [JsonIgnore] public int[] WireRoutingAlgsIndexes { get; set; } = {0};


    [JsonProperty("Мин. Число Проектировщиков")]
    public int DesignersCountMin { get; set; } = 4;

    [JsonProperty("Макс. Число Проектировщиков")]
    public int DesignersCountMax { get; set; } = 20;


    public IntervalsOfVariables Copy()
    {
        var intervalsOfVariables = (IntervalsOfVariables) MemberwiseClone();
        intervalsOfVariables.PlacingAlgsIndexes = PlacingAlgsIndexes.ToArray();
        intervalsOfVariables.WireRoutingAlgsIndexes = WireRoutingAlgsIndexes.ToArray();
        return intervalsOfVariables;
    }


    [JsonProperty("Рассматриваемые Подсистемы Автоматического Размещения")]
    public string[] PlacingAlgsStrs => PlacingAlgsIndexes.Select(index => PlacingAlgIndexNameMap[index]).ToArray();

    [JsonProperty("Рассматриваемые Подсистемы Автоматической Трассировки")]
    public string[] WireRoutingAlgsStrs => PlacingAlgsIndexes.Select(index => RoutingAlgIndexNameMap[index]).ToArray();

    public event PropertyChangedEventHandler? PropertyChanged;
}