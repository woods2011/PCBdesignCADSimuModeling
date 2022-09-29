using Newtonsoft.Json;
using PcbDesignSimuModeling.WPF.Models.SimuSystem;
using PcbDesignSimuModeling.WPF.ViewModels.Shared;

namespace PcbDesignSimuModeling.WPF.ViewModels.SimuSystem;

public class SimulationResult
{
    [JsonProperty("Время проведения имитационного эксперимента")]
    public string? ExperimentTime { get; init; }

    [JsonProperty("Конфигурация САПР")] public CadConfigurationEm CadConfigurationEm { get; }

    [JsonProperty("Условия проведения эксперимента")]
    public SimulationInputParamsEm SimulationInputParamsEm { get; }

    [JsonProperty("Доп. Настройки моделирования")]
    public GeneralSimulationSettings GeneralSimulationSettings { get; }

    [JsonProperty("Суммарная Стоимость Конфигурации")]
    public double? ConfigurtaionTotalCost { get; init; }

    [JsonProperty("Оценка Рассмотренной Конфигурации")]
    public double? ConfigurationScore { get; init; }

    [JsonProperty("Среднее Время Проектирования Одной Платы")]
    public double? AverageProductionTime { get; init; }

    [JsonProperty("СКО от Среднего Времени Проектирования Одной Платы")]
    public double? DevProductionTime { get; init; }

    [JsonProperty("Максимальное Время Проектирования Одной Платы")]
    public double? MaxProductionTime { get; init; }

    [JsonConstructor]
    public SimulationResult(CadConfigurationEm cadConfigurationEm, SimulationInputParamsEm simulationInputParamsEm, GeneralSimulationSettings generalSimulationSettings)
    {
        CadConfigurationEm = cadConfigurationEm;
        SimulationInputParamsEm = simulationInputParamsEm;
        GeneralSimulationSettings = generalSimulationSettings;
    }
}


// public class SimulationResult
// {
//     [JsonProperty("Время проведения имитационного эксперимента")]
//     public string? RealTime { get; set; }
//
//     [JsonProperty("Список ресурсов")]
//     public List<IResource>? ResourcePool { get; set; }  
//
//     [JsonProperty("Алгоритм размещения")]
//     public string? SelectedPlacingAlgStr { get; set; }
//         
//     [JsonProperty("Алгоритм трассировки")]
//     public string? SelectedWireRoutingAlgStr { get; set; }
//         
//             
//     [JsonProperty("Заявок в год")]
//     public int TechPerYear { get; set; }
//         
//     [JsonProperty("Число элементов на плате (норм. распр.)")]
//     public DblNormalDistrVm? ElementCountDistr { get; set; }
//         
//     [JsonProperty("Процент занимаемой площади платы (норм. распр.)")]
//     public DblNormalDistrVm? DimensionUsagePctDistr { get; set; }
//         
//     [JsonProperty("Процент плат с разногабаритными элементами")]    
//     public double? VariousSizePctMean { get; set; }
//             
//     [JsonProperty("Общее время моделирования")]
//     public TimeSpan? FinalTime { get; set; }
//         
//         
//     [JsonProperty("Результат моделирования: Среднее время на одну плату")]
//     public TimeSpan? AverageProductionTime { get; set; }    
//
//     [JsonProperty("Результат моделирования: СКО от среднего времени на одну плату")]
//     public TimeSpan? DevProductionTime { get; set; }        
//         
//     [JsonProperty("Результат моделирования: Стоимость конфигурации")]
//     public double? TotalCost { get; set; }
//         
//     [JsonProperty("Результат моделирования: Оценка по критериям производоительность и стоимость")]
//     public double? CostToTime { get; set; }
// }