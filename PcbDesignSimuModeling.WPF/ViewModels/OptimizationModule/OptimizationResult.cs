using Newtonsoft.Json;
using PcbDesignSimuModeling.WPF.Models.OptimizationModule;
using PcbDesignSimuModeling.WPF.Models.SimuSystem;
using PcbDesignSimuModeling.WPF.ViewModels.Shared;
using PcbDesignSimuModeling.WPF.ViewModels.SimuSystem;

namespace PcbDesignSimuModeling.WPF.ViewModels.OptimizationModule;

public class OptimizationResult
{
    [JsonProperty("Время Проведения Оптимизации")]
    public string? OptimizationTime { get; init; }
    
    [JsonProperty("Параметры Алгоритма Пчелиной Колонии")]
    public AlgorithmSettings? AlgorithmSettings { get; init; }

    [JsonProperty("Условия Проведения Моделирования")]
    public SimulationInputParamsEm SimulationInputParamsEm { get; }

    [JsonProperty("Доп. Настройки Моделирования")]
    public GeneralSimulationSettings GeneralSimulationSettings { get; }

    [JsonProperty("Оптимиальная Конфигурация САПР")]
    public CadConfigurationEm CadConfigurationEm { get; }

    [JsonProperty("Оценка Найденной Конфигурации")]
    public double? ConfigurationScore { get; init; }

    public OptimizationResult(SimulationInputParamsEm simulationInputParamsEm,
        GeneralSimulationSettings generalSimulationSettings, CadConfigurationEm cadConfigurationEm)
    {
        SimulationInputParamsEm = simulationInputParamsEm;
        GeneralSimulationSettings = generalSimulationSettings;
        CadConfigurationEm = cadConfigurationEm;
    }
}