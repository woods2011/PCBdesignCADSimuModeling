using System.Diagnostics;
using System.Windows.Input;
using Newtonsoft.Json;
using PcbDesignSimuModeling.Core.Commands;
using PcbDesignSimuModeling.Core.Models.Loggers;
using PcbDesignSimuModeling.Core.Models.Resources;
using PcbDesignSimuModeling.Core.Models.Resources.Algorithms;
using PcbDesignSimuModeling.Core.Models.Resources.Algorithms.PlacingAlgorithms;
using PcbDesignSimuModeling.Core.Models.Resources.Algorithms.WireRoutingAlgorithms;
using PcbDesignSimuModeling.Core.Models.SimuSystem;
using PcbDesignSimuModeling.Core.Models.SimuSystem.SimulationEvents;

namespace PcbDesignSimuModeling.Core.ViewModels;

public class SimuSystemViewModel : BaseViewModel
{
    private readonly Random _rndSource;

    public MessageViewModel MessageViewModel { get; init; } = new();
    public string LastResultLogPath { get; set; } = Directory.GetCurrentDirectory() + @"\LastSimulationLog.json";
    public SimulationResult? LastSimulationResult { get; set; }
    public string AllResultsPath { get; set; } = Directory.GetCurrentDirectory() + @"\AllSimulationResults.json";
    public string? LastSimulationResultLog { get; set; }
    public string LastSimulationResultLogSearch { get; set; } = String.Empty;
    public string LastSimulationResultLogSearchTransformed => LastSimulationResultLogSearch.Replace(" ", ".");


    public IReadOnlyList<string> PlacingAlgStrList { get; } = new List<string>
        { PlacingAlgProviderFactory.PlacingSequentialStr, PlacingAlgProviderFactory.PlacingPartitioningStr };

    public string SelectedPlacingAlgStr { get; set; } = PlacingAlgProviderFactory.PlacingSequentialStr;

    public IReadOnlyList<string> WireRoutingAlgStrList { get; } = new List<string>
        { WireRoutingAlgProviderFactory.WireRoutingWaveStr, WireRoutingAlgProviderFactory.WireRoutingChannelStr };

    public string SelectedWireRoutingAlgStr { get; set; } = WireRoutingAlgProviderFactory.WireRoutingWaveStr;


    public Server Server { get; } = new(150);
    public CpuCluster Cpu { get; } = new(16, 2.5);
    public int DesignersCount { get; set; } = 1;


    public TechIntervalBuilderVm TechIntervalDistr { get; }
    public DblNormalDistributionBuilderVm ElementCountDistr { get; }
    public DblNormalDistributionBuilderVm DimensionUsagePctDistr { get; }
    public double VariousSizePctProb { get; set; } = 0.8;
    public TimeSpan FinalTime { get; set; } = TimeSpan.FromDays(30);


    public SimuSystemViewModel(Random? rndSource = null)
    {
        _rndSource = rndSource ?? new Random(1);
        TechIntervalDistr =
            new TechIntervalBuilderVm(new TimeSpan(1, 20, 0, 0), new TimeSpan(6, 30, 0), _rndSource);
        ElementCountDistr = new DblNormalDistributionBuilderVm(150, 15, _rndSource);
        DimensionUsagePctDistr = new DblNormalDistributionBuilderVm(0.6, 0.1, _rndSource);
    }


    public ICommand BeginSimulation =>
        new ActionCommand(_ => BeginSimulationHandler());

    public ICommand SaveSimuResult => new ActionCommand(_ => SaveSimuResultHandler(),
        _ => LastSimulationResult is not null && !String.IsNullOrEmpty(LastSimulationResultLog));

    public ICommand SaveLastSimuLog => new ActionCommand(_ => SaveLastSimuLogHandler(),
        _ => !String.IsNullOrEmpty(LastSimulationResultLog));

    public void BeginSimulationHandler()
    {
        try
        {
            List<IResource> resourcePool = new() { Cpu, Server };
            resourcePool.AddRange(Enumerable.Range(0, DesignersCount).Select(_ => new Designer()));
            resourcePool = resourcePool.Select(resource => resource.Clone()).ToList();

            var pcbAlgFactories = new PcbAlgFactories(
                placingAlgFactory: PlacingAlgProviderFactory.Create(SelectedPlacingAlgStr),
                wireRoutingAlgFactory: WireRoutingAlgProviderFactory.Create(SelectedWireRoutingAlgStr));

            var simuEventGenerator = new SimuEventGenerator(
                timeBetweenTechsDistr: TechIntervalDistr.Build(),
                pcbElemsCountDistr: ElementCountDistr.Build(),
                pcbDimUsagePctDistr: DimensionUsagePctDistr.Build(),
                pcbElemsIsVarSizeProb: VariousSizePctProb,
                random: _rndSource);

            InMemorySimpleLogger? inMemorySimpleLogger = new();
            var simulator =
                new PcbDesignSimulator(simuEventGenerator, resourcePool, pcbAlgFactories, inMemorySimpleLogger);
            var simulationResult = simulator.Simulate(FinalTime);

            LastSimulationResultLog = inMemorySimpleLogger?.GetData();

            if (simulationResult.Values.Count < 1) return;

            var productionTimesSec = simulationResult.Values;
            var avgProductionTimeMilSec = productionTimesSec.Average(time => time.TotalMilliseconds);
            var avgProductionTime = TimeSpan.FromMilliseconds(avgProductionTimeMilSec);
            var devProductionTimeMilSec = Math.Sqrt(
                productionTimesSec.Sum(time => Math.Pow(time.TotalMilliseconds - avgProductionTimeMilSec, 2)) /
                productionTimesSec.Count);
            var devProductionTime = TimeSpan.FromMilliseconds(devProductionTimeMilSec);

            var totalConfigCost = resourcePool.Sum(resource => resource.Cost);

            LastSimulationResult = new SimulationResult()
            {
                RealTime = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"),
                ResourcePool = resourcePool,
                SelectedPlacingAlgStr = SelectedPlacingAlgStr,
                SelectedWireRoutingAlgStr = SelectedWireRoutingAlgStr,
                TechIntervalDistr = TechIntervalDistr,
                ElementCountDistr = ElementCountDistr,
                DimensionUsagePctDistr = DimensionUsagePctDistr,
                VariousSizePctMean = VariousSizePctProb,
                FinalTime = FinalTime,
                AverageProductionTime = avgProductionTime,
                DevProductionTime = devProductionTime,
                TotalCost = totalConfigCost,
                // CostToTime = 0.4 * totalConfigCost / Math.Sqrt(totalConfigCost * totalConfigCost + avgProductionTime.Hours * avgProductionTime.Hours) + 
                //              0.6 * avgProductionTime.Hours / Math.Sqrt(totalConfigCost * totalConfigCost + avgProductionTime.Hours * avgProductionTime.Hours)
                //CostToTime = ((0.6 + 0.4 / (1.0 + totalConfigCost)) * (0.4 + 0.6 / (1 + Math.Pow(Math.Max(0, avgProductionTime.TotalDays - 9.0), 1))) - 0.6 * 0.4) / (1 - 0.6 * 0.4)
                CostToTime = (0.6 * (100000.0 / avgProductionTime.TotalDays)) / (0.4 * totalConfigCost)
            };
        }
        catch (Exception e) when (!Debugger.IsAttached)
        {
            MessageViewModel.Message = e.Message;
        }
    }


    private void SaveSimuResultHandler()
    {
        if (LastSimulationResult is null) return;

        var serializedResult = JsonConvert.SerializeObject(LastSimulationResult, Formatting.Indented,
            new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });

        new AppendFileSimpleLogger(AllResultsPath).Log(serializedResult);
    }

    private void SaveLastSimuLogHandler()
    {
        if (LastSimulationResult is null) return;
        if (LastSimulationResultLog is null) return;

        var serializedResult = JsonConvert.SerializeObject(LastSimulationResult, Formatting.Indented,
            new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });

        new TruncateFileSimpleLogger(LastResultLogPath).Log($"{serializedResult}{LastSimulationResultLog}");
    }
}


// public List<Designer.ExperienceEn> ExperienceEnumList { get; } = Enum.GetValues(typeof(Designer.ExperienceEn)).Cast<Designer.ExperienceEn>().ToList();