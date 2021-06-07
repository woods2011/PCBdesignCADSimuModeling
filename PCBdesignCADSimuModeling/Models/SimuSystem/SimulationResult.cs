using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PCBdesignCADSimuModeling.Models.Resources;
using PCBdesignCADSimuModeling.ViewModels;

namespace PCBdesignCADSimuModeling.Models.SimuSystem
{
    public class SimulationResult
    {
        [JsonProperty("Время проведения имитационного эксперимента")]
        public string RealTime { get; set; }
        
        
        [JsonProperty("Список ресурсов")]
        public List<IResource> ResourcePool { get; set; }

        [JsonProperty("Алгоритм размещения")]
        public string SelectedPlacingAlgStr { get; set; }
        
        [JsonProperty("Алгоритм трассировки")]
        public string SelectedWireRoutingAlgStr { get; set; }
        
        
        [JsonProperty("Интервал между изделиями (норм. распр.)")]
        public TechIntervalBuilderDisplayModel TechIntervalDistr { get; set; }
        
        [JsonProperty("Число элементов на плате (норм. распр.)")]
        public DistributionBuilderDisplayModelDbl ElementCountDistr { get; set; }
        
        [JsonProperty("Процент занимаемой площади платы (норм. распр.)")]
        public DistributionBuilderDisplayModelDbl DimensionUsagePctDistr { get; set; }
        
        [JsonProperty("Процент плат с разногабаритными элементами")]
        public double VariousSizePctMean { get; set; }
        
        [JsonProperty("Общее время моделирования")]
        public TimeSpan FinalTime { get; set; }
        
        
        [JsonProperty("Результат моделирования: Среднее время на одну плату")]
        public TimeSpan AverageProductionTime { get; set; }

        [JsonProperty("Результат моделирования: СКО от среднего времени на одну плату")]
        public TimeSpan DevProductionTime { get; set; }
        
        [JsonProperty("Результат моделирования: Стоимость конфигурации")]
        public double TotalCost { get; set; }
        
        [JsonProperty("Результат моделирования: Оценка по критериям производоительность и стоимость")]
        public double CostToTime { get; set; }
    }
}