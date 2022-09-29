using System;
using System.Collections.Generic;
using System.Linq;
using PcbDesignSimuModeling.WPF.Models.Loggers;
using PcbDesignSimuModeling.WPF.Models.Resources;
using PcbDesignSimuModeling.WPF.Models.Resources.CpuClusterRes;
using PcbDesignSimuModeling.WPF.Models.Resources.DesignerRes;
using PcbDesignSimuModeling.WPF.Models.Resources.RamRes;
using PcbDesignSimuModeling.WPF.Models.Resources.ServerRes;
using PcbDesignSimuModeling.WPF.Models.Technologies.PcbDesign;
using PcbDesignSimuModeling.WPF.Models.Technologies.PcbDesign.ProjectProcedures;

namespace PcbDesignSimuModeling.WPF.Models.SimuSystem;

public class StatCollector
{
    private readonly List<IResource> _resources;
    private readonly ISimpleLogger _logger;
    private readonly TimeSpan _ranUpTime;
    private TimeSpan _curTimeNotNormalized = TimeSpan.Zero;

    private readonly Dictionary<PcbDesignProcedure, TimeSpan> _proceduresStarts = new();

    public Dictionary<PcbDesignProcedure, TimeSpan> ProceduresDurations { get; } = new();

    public List<TimeSpan> TimesSnapshots { get; } = new();
    public List<int> TechInWorkCountList { get; } = new();
    public List<List<double>> CpuClusterUsageSnapshots { get; } = new();
    public List<List<double>> CpuClusterTaskCountSnapshots { get; } = new();
    public List<double> RamUsageSnapshots { get; } = new();
    public List<double> BusyDesignersSnapshots { get; } = new();
    public List<double> BusyOrIllDesignersSnapshots { get; } = new();
    public List<double> ServerSnapshots { get; } = new();

    public StatCollector(IEnumerable<IResource> resources, ISimpleLogger logger, TimeSpan? ranUpTime = null)
    {
        _resources = resources.ToList();
        _logger = logger;
        _ranUpTime = (ranUpTime ?? TimeSpan.Zero) / 4.2;
    }

    public void MakeResourceSnapshot(TimeSpan modelTime, int techInWorkCount)
    {
        TechInWorkCountList.Add(techInWorkCount);

        var cpuCluster = _resources.OfType<CpuCluster>().First();
        CpuClusterUsageSnapshots.Add(cpuCluster.UsagePerThreadList);
        CpuClusterTaskCountSnapshots.Add(cpuCluster.NormTaskPerThreadList);

        var ram = _resources.OfType<Ram>().First();
        RamUsageSnapshots.Add(ram.TotalAmount - ram.AvailableAmount);

        var busyDesignersCount = _resources.OfType<Designer>().Count(designer => designer.IsBusy);
        BusyDesignersSnapshots.Add(busyDesignersCount);
        var busyOrIllDesignersCount =
            _resources.OfType<Designer>().Count(designer => designer.IsBusy || !designer.IsActive);
        BusyOrIllDesignersSnapshots.Add(busyOrIllDesignersCount);

        var serverUsage = _resources.OfType<Server>().First().ActiveUsers;
        ServerSnapshots.Add(serverUsage);
    }

    public void Log(object data) => _logger.Log(data);

    public void LogPcbDesignProcedureFinish(PcbDesignTechnology tech)
    {
        _proceduresStarts.Remove(tech.CurProcedure, out var procedureStartTime);
        if (procedureStartTime >= _ranUpTime)
            ProceduresDurations.Add(tech.CurProcedure, _curTimeNotNormalized - procedureStartTime);

        _logger.Log($"{String.Concat(Enumerable.Repeat("---", (tech.TechId - 1) % 15))}" +
                    $"Процесс Проектирования: {tech.TechId} - Финиш проектной процедуры: {tech.CurProcedure.Name}");
    }

    public void LogPcbDesignProcedureStart(PcbDesignTechnology tech) =>
        _proceduresStarts.TryAdd(tech.CurProcedure, _curTimeNotNormalized);

    public void Log(TimeSpan modelTime)
    {
        _curTimeNotNormalized = modelTime;
        TimesSnapshots.Add(modelTime.ToWorkDays().ToWorkWeek());
        _logger.Log(
            $"{Environment.NewLine}>ТЕКУЩЕЕ МОДЕЛЬНОЕ ВРЕМЯ: {modelTime.ToWorkDays().ToWorkWeek():d\\.hh\\:mm\\:ss}");
    }
}

public class ProceduresTimesHandler
{
    public double PcbParamsInputTimeHours { get; }
    public double BoardParamsAndRulesTimeHours { get; }
    public double AutoPlacementTimeHours { get; }
    public double ManualPlacementTimeHours { get; }
    public double AutoRoutingTimeHours { get; }
    public double ManualRoutingTimeHours { get; }
    public double QualityControlTimeHours { get; }
    public double DocumentationProductionTimeHours { get; }

    public ProceduresTimesHandler(Dictionary<PcbDesignProcedure, TimeSpan> proceduresDurations)
    {
        PcbParamsInputTimeHours = proceduresDurations.Where(pair => pair.Key is PcbParamsInput)
            .Average(pair => pair.Value.TotalHours);

        BoardParamsAndRulesTimeHours = proceduresDurations.Where(pair => pair.Key is BoardParamsAndRules)
            .Average(pair => pair.Value.TotalHours);

        AutoPlacementTimeHours = proceduresDurations.Where(pair => pair.Key is AutoPlacement)
            .Average(pair => pair.Value.TotalHours);

        ManualPlacementTimeHours = proceduresDurations.Where(pair => pair.Key is ManualPlacement)
            .Average(pair => pair.Value.TotalHours);

        AutoRoutingTimeHours = proceduresDurations.Where(pair => pair.Key is AutoRouting)
            .Average(pair => pair.Value.TotalHours);

        ManualRoutingTimeHours = proceduresDurations.Where(pair => pair.Key is ManualRouting)
            .Average(pair => pair.Value.TotalHours);

        QualityControlTimeHours = proceduresDurations.Where(pair => pair.Key is QualityControl)
            .Average(pair => pair.Value.TotalHours);

        DocumentationProductionTimeHours = proceduresDurations.Where(pair => pair.Key is DocumentationProduction)
            .Average(pair => pair.Value.TotalHours);
    }
}