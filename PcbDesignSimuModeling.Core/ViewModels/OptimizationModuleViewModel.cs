using System.Diagnostics;
using System.Text;
using System.Windows.Input;
using MathNet.Numerics.Distributions;
using Newtonsoft.Json;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using PcbDesignSimuModeling.Core.Commands;
using PcbDesignSimuModeling.Core.Models.Loggers;
using PcbDesignSimuModeling.Core.Models.OptimizationModule;
using PcbDesignSimuModeling.Core.Models.Resources.Algorithms.PlacingAlgorithms;
using PcbDesignSimuModeling.Core.Models.Resources.Algorithms.WireRoutingAlgorithms;
using PcbDesignSimuModeling.Core.Models.SimuSystem.SimulationEvents;

namespace PcbDesignSimuModeling.Core.ViewModels;

public class OptimizationModuleViewModel : BaseViewModel
{
    private readonly Random _rndSource;
    private AlgorithmSettings _savedAlgSettings;

    public MessageViewModel MessageViewModel { get; init; } = new();
    public string LastResultLogPath { get; set; } = Directory.GetCurrentDirectory() + @"\LastOptimizationLog.json";

    private List<FoodSource>? _lastResultLog;
    public string AlgTextLog { get; set; } = String.Empty;
    public PlotModel PlotModel { get; set; }
    public string AllResultsPath { get; set; } = Directory.GetCurrentDirectory() + @"\AllOptimizationResults.json";
    public FoodSource? LastResult { get; private set; }


    public AlgorithmSettings AlgSettings { get; set; } = new();

    public IReadOnlyList<string> PlacingAlgStrList { get; } = new List<string>
        {PlacingAlgProviderFactory.PlacingSequentialStr, PlacingAlgProviderFactory.PlacingPartitioningStr};

    public List<string> SelectedPlacingAlgStrList { get; set; }

    public IReadOnlyList<string> WireRoutingAlgStrList { get; } = new List<string>
        {WireRoutingAlgProviderFactory.WireRoutingWaveStr, WireRoutingAlgProviderFactory.WireRoutingChannelStr};

    public List<string> SelectedWireRoutingAlgStrList { get; set; }


    public double TechPerYear { get; set; } = 200;
    public DblNormalDistributionBuilderVm ElementCountDistr { get; }
    public DblNormalDistributionBuilderVm AreaUsagePctDistr { get; }
    public double VariousSizePctProb { get; set; } = 0.8;
    public TimeSpan FinalTime { get; set; } = TimeSpan.FromDays(30);
    public int SampleSize { get; set; } = 2;

    
    public OptimizationModuleViewModel(Random? rndSource = null)
    {
        SelectedPlacingAlgStrList = PlacingAlgStrList.ToList();
        SelectedWireRoutingAlgStrList = WireRoutingAlgStrList.ToList();

        _rndSource = rndSource ?? new Random(1);
        _savedAlgSettings = AlgSettings.Copy();

        ElementCountDistr = new DblNormalDistributionBuilderVm(150, 15, _rndSource);
        AreaUsagePctDistr = new DblNormalDistributionBuilderVm(0.6, 0.1, _rndSource);

        PlotModel = new PlotModel {Title = "Оценка конфигурации САПР (значение цеелевой функции) / Итерация"};
        PlotModel.Axes.Add(new LinearAxis {Position = AxisPosition.Bottom, Title = "Итерация"});
        PlotModel.Axes.Add(new LogarithmicAxis {Position = AxisPosition.Left, Title = "Оценка конфигурации САПР"});
    }


    public ICommand BeginOptimizationCommand =>
        new ActionCommand(_ => BeginOptimizationHandler(),
            _ => SelectedPlacingAlgStrList.Count >= 1 && SelectedWireRoutingAlgStrList.Count >= 1);

    public ICommand SaveLastOptimizationResultCommand => new ActionCommand(_ => SaveLastOptimizationResultHandler(),
        _ => LastResult is not null);

    public ICommand SaveLastOptimizationLogCommand => new ActionCommand(_ => SaveLastOptimizationLogHandler(),
        _ => _lastResultLog is not null && _lastResultLog.Count >= 1);


    public void BeginOptimizationHandler()
    {
        try
        {
            AlgSettings.SearchIntervals.PlacingAlgsIndexes = SelectedPlacingAlgStrList
                .Select(str => PlacingAlgProviderFactory.AlgNameIndexMap[str]).ToArray();
            AlgSettings.SearchIntervals.WireRoutingAlgsIndexes = SelectedWireRoutingAlgStrList
                .Select(str => WireRoutingAlgProviderFactory.AlgNameIndexMap[str]).ToArray();
            _savedAlgSettings = AlgSettings.Copy();

            var pcbGenerator = new PcbProbDistrBasedGenerator(
                pcbElemsCountDistr: ElementCountDistr.Build(),
                pcbAreaUsagePctDistr: AreaUsagePctDistr.Build(),
                pcbElemsIsVarSize: new Bernoulli(VariousSizePctProb, _rndSource)
            );

            var simuEventGenerator = new PcbDesignTechGenerator(
                requestsFlowDistrDays: new Exponential(TechPerYear / 365.0, _rndSource),
                pcbGenerator: pcbGenerator,
                rndSource: _rndSource
            );

            var preCalcEventsList = Enumerable.Range(0, SampleSize).Select(_ => simuEventGenerator.GenerateSimuEvent(FinalTime));
            var simuSystemFuncWrapper = new SimuSystemFuncWrapper(preCalcEventsList, FinalTime);
            var objectiveFunction = simuSystemFuncWrapper.ObjectiveFunction;

            var abcAlgorithm = new AbcAlgorithm(_savedAlgSettings, _rndSource, objectiveFunction);
            _lastResultLog = abcAlgorithm.FindMinimum().ToList();
            LastResult = abcAlgorithm.BestFoodSource.Copy();

            DrawLog();
            DrawPlot();
        }
        catch (Exception e) when (!Debugger.IsAttached)
        {
            MessageViewModel.Message = e.Message;
        }
    }

    private void DrawPlot()
    {
        if (LastResult is null) return;
        if (_lastResultLog is null) return;


        PlotModel = new PlotModel {Title = "Оценка конфигурации САПР (значение цеелевой функции) / Итерация"};
        PlotModel.Axes.Add(new LinearAxis {Position = AxisPosition.Bottom, Title = "Итерация"});
        if (LastResult.FuncValue >= -1e-20)
            PlotModel.Axes.Add(new LogarithmicAxis
                {Position = AxisPosition.Left, Title = "Оценка конфигурации САПР"});
        else
            PlotModel.Axes.Add(new LinearAxis {Position = AxisPosition.Left, Title = "Оценка конфигурации САПР"});

        var lineSeries = new LineSeries {Color = OxyColors.Blue, MarkerType = MarkerType.Cross};
        lineSeries.Points.AddRange(_lastResultLog.Select((foodSource, i) =>
            new DataPoint(i, foodSource.FuncValue)));

        PlotModel.Series.Add(lineSeries);
        PlotModel.InvalidatePlot(true);
    }

    private void DrawLog()
    {
        if (_lastResultLog is null) return;

        var (curIter, stringBuilder, divider) =
            (0, new StringBuilder(), Math.Max(1, Math.Ceiling(_lastResultLog.Count / 300.0)));

        foreach (var curIterFoodSource in _lastResultLog)
        {
            if (curIter % divider == 0 || curIter == _lastResultLog.Count - 1)
                stringBuilder.Append(
                    $">>>Итерация {curIter}:{Environment.NewLine}{curIterFoodSource}{Environment.NewLine}"
                    // $"Итерация {curIter}: Лучший результат: {iterBestBacteria.FuncValue} в точке:" +
                    // $"({iterBestBacteria.ThreadsCount} ; {iterBestBacteria.ClockRate} ; {iterBestBacteria.ServerSpeed} ; {iterBestBacteria.PlacingAlgIndex} ; {iterBestBacteria.WireRoutingAlgIndex} ; {iterBestBacteria.DesignersCount})" +
                    // $"{Environment.NewLine}"
                );

            curIter++;
        }

        AlgTextLog = stringBuilder.ToString();
    }


    private void SaveLastOptimizationResultHandler()
    {
        if (LastResult is null) return;

        var serializedResult = JsonConvert.SerializeObject(
            new {SaveTime = DateTime.Now, AlgorithmInput = _savedAlgSettings, OptimizationResult = LastResult},
            Formatting.Indented
        );

        try
        {
            new AppendFileSimpleLogger(AllResultsPath).Log(serializedResult);
        }
        catch (Exception e) when (!Debugger.IsAttached) // ToDo : remove "when"
        {
        }
    }

    private void SaveLastOptimizationLogHandler()
    {
        if (LastResult is null) return;
        if (_lastResultLog is null) return;

        var serializedResult = JsonConvert.SerializeObject(
            new
            {
                SaveTime = DateTime.Now,
                AlgorithmInput = _savedAlgSettings,
                OptimizationLog = _lastResultLog.Select(
                    (curIterationBest, curIteration) =>
                        new
                        {
                            Iteration = curIteration,
                            BestResult = curIterationBest,
                        }),
                OptimizationResult = LastResult
            },
            Formatting.Indented);

        try
        {
            new TruncateFileSimpleLogger(LastResultLogPath).Log(serializedResult);
        }
        catch (Exception e) when (!Debugger.IsAttached) // ToDo : remove "when"
        {
        }
    }
}