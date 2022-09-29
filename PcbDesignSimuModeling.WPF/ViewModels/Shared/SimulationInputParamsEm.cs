using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PcbDesignSimuModeling.WPF.Models.Technologies.PcbDesign;
using PcbDesignSimuModeling.WPF.ViewModels.Helpers;

namespace PcbDesignSimuModeling.WPF.ViewModels.Shared;

public abstract class SimulationInputParamsEm
{
    [JsonProperty("Количество Проектируемых Плат в Год")]
    public int TechPerYear { get; set; }

    
    [JsonProperty("Целевое Время Проектирования Платы (рабочие часы)")]
    public double GoalProductionTimeHours { get; set; }
    
    [JsonProperty("Время Установления Системы")]
    public TimeSpan RunUpSection { get; set; }
    
    [JsonProperty("Время Моделирования")]
    public TimeSpan FinalTime { get; set; }

    protected SimulationInputParamsEm(int techPerYear, double goalProductionTimeHours, TimeSpan runUpSection,
        TimeSpan finalTime)
    {
        TechPerYear = techPerYear;
        GoalProductionTimeHours = goalProductionTimeHours;
        RunUpSection = runUpSection;
        FinalTime = finalTime;
    }
}

public class SimInputPcbGenerationBasedEm : SimulationInputParamsEm
{
    public SimInputPcbGenerationBasedEm(int techPerYear, double goalProductionTimeHours,
        TimeSpan runUpSection, TimeSpan finalTime, PcbGenerationParamsEm pcbGenerationParamsEm)
        : base(techPerYear, goalProductionTimeHours, runUpSection, finalTime)
        => PcbGenerationParamsEm = pcbGenerationParamsEm;
    
    [JsonProperty("Настройки Генерации Описаний Печатных Плат:")]
    public PcbGenerationParamsEm PcbGenerationParamsEm { get; set; }
}

public class SimInputImportPcbBasedEm : SimulationInputParamsEm
{
    public SimInputImportPcbBasedEm(int techPerYear, double goalProductionTimeHours,
        TimeSpan runUpSection, TimeSpan finalTime, List<PcbDescription> importedPcbDescriptions)
        : base(techPerYear, goalProductionTimeHours, runUpSection, finalTime)
        => ImportedPcbDescriptions = importedPcbDescriptions;

    [JsonProperty("Базовый Пул Описаний Печатных Плат")]
    public List<PcbDescription> ImportedPcbDescriptions { get; set; }
}

public class PcbGenerationParamsEm
{
    public DblNormalDistrVm ElementCountDistr { get; }
    public DblNormalDistrVm NewElemsPercentDistr { get; }
    public DblNormalDistrVm ElementsDensityDistr { get; }
    public DblNormalDistrVm PinsCountDistr { get; }
    public DblNormalDistrVm PinDensityDistr { get; }
    public DblNormalDistrVm NumOfLayersDistr { get; }
    public double IsDoubleSidePlacementProb { get; }
    public double IsManualSchemeInputProb { get; }


    public PcbGenerationParamsEm(DblNormalDistrVm elementCountDistr, DblNormalDistrVm newElemsPercentDistr,
        DblNormalDistrVm elementsDensityDistr, DblNormalDistrVm pinsCountDistr, DblNormalDistrVm pinDensityDistr,
        DblNormalDistrVm numOfLayersDistr, double isDoubleSidePlacementProb, double isManualSchemeInputProb)
    {
        ElementCountDistr = elementCountDistr;
        NewElemsPercentDistr = newElemsPercentDistr;
        ElementsDensityDistr = elementsDensityDistr;
        PinsCountDistr = pinsCountDistr;
        PinDensityDistr = pinDensityDistr;
        NumOfLayersDistr = numOfLayersDistr;
        IsDoubleSidePlacementProb = isDoubleSidePlacementProb;
        IsManualSchemeInputProb = isManualSchemeInputProb;
    }
}