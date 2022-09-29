using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using MathNet.Numerics.Distributions;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.SaveFile;
using Newtonsoft.Json;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using PcbDesignSimuModeling.WPF.Commands;
using PcbDesignSimuModeling.WPF.Models;
using PcbDesignSimuModeling.WPF.Models.Loggers;
using PcbDesignSimuModeling.WPF.Models.OptimizationModule;
using PcbDesignSimuModeling.WPF.Models.SimuSystem;
using PcbDesignSimuModeling.WPF.Models.SimuSystem.SimulationEvents;
using PcbDesignSimuModeling.WPF.ViewModels.Helpers;
using PcbDesignSimuModeling.WPF.ViewModels.Shared;
using PcbDesignSimuModeling.WPF.ViewModels.SimuSystem;
using static PcbDesignSimuModeling.WPF.Models.Resources.EadSybSystems.PlacingSystems.PlacingSysFactoryProvider;
using static PcbDesignSimuModeling.WPF.Models.Resources.EadSybSystems.RoutingSystems.RoutingSysFactoryProvider;

namespace PcbDesignSimuModeling.WPF.ViewModels.OptimizationModule;

public class OptimizationModuleViewModel : BaseViewModel
{
    private readonly Random _rndSource;
    private readonly DialogService _dialogService = new();
    private AlgorithmSettings _savedAlgSettings;
    private List<FoodSource>? _lastOptimizationResultLog;

    public MessageViewModel MessageViewModel { get; }


    public AlgorithmSettings AlgSettings { get; set; } = new();

    public IReadOnlyList<string> PlacingAlgStrList { get; } = new[] {PlacingToporStr};
    public List<string> SelectedPlacingAlgStrList { get; set; }

    public IReadOnlyList<string> WireRoutingAlgStrList { get; } = new[] {RoutingToporStr};
    public List<string> SelectedWireRoutingAlgStrList { get; set; }


    public SimulationInputParamsVm SimInputParamsVm { get; }
    public int SampleSize { get; set; } = 2;


    public OptimizationResult? LastOptimizationResult { get; private set; }

    public string? LastOptimizationResultStr { get; private set; }
    public string LastOptimizationResultLogStr { get; set; } = String.Empty;
    public PlotModel OptimizationProcessPlot { get; set; }


    public OptimizationModuleViewModel() : this(null, null)
    {
    }

    public OptimizationModuleViewModel(Random? rndSource = null, MessageViewModel? messageViewModel = null)
    {
        _rndSource = rndSource ?? new Random(1);
        MessageViewModel = messageViewModel ?? new MessageViewModel();

        SelectedPlacingAlgStrList = PlacingAlgStrList.ToList();
        SelectedWireRoutingAlgStrList = WireRoutingAlgStrList.ToList();

        _savedAlgSettings = AlgSettings.Copy();

        SimInputParamsVm = new SimulationInputParamsVm(_rndSource, MessageViewModel);

        OptimizationProcessPlot = new PlotModel
            {Title = "Оценка конфигурации САПР (значение цеелевой функции) / Итерация", TitleFontSize = 15};
        OptimizationProcessPlot.Axes.Add(new LinearAxis {Position = AxisPosition.Bottom, Title = "Итерация"});
        OptimizationProcessPlot.Axes.Add(new LogarithmicAxis
            {Position = AxisPosition.Left, Title = "Оценка конфигурации САПР"});
    }


    public ICommand BeginOptimizationCommand => new ActionCommand(_ => BeginOptimization(),
        _ => SelectedPlacingAlgStrList.Count >= 1 && SelectedWireRoutingAlgStrList.Count >= 1);

    public ICommand ExportConfigCommand => new ActionCommand(_ => OnShareConfiguration(),
        _ => LastOptimizationResult is not null);

    public ICommand SaveLastOptimizationResultCommand => new ActionCommand(_ => SaveLastOptimizationResult(),
        _ => LastOptimizationResult is not null);

    public ICommand SaveLastOptimizationLogCommand => new ActionCommand(_ => SaveLastOptimizationLog(),
        _ => _lastOptimizationResultLog is not null && _lastOptimizationResultLog.Count >= 1);


    public void BeginOptimization()
    {
        try
        {
            AlgSettings.SearchIntervals.PlacingAlgsIndexes = SelectedPlacingAlgStrList
                .Select(str => PlacingAlgNameIndexMap[str]).ToArray();
            AlgSettings.SearchIntervals.WireRoutingAlgsIndexes = SelectedWireRoutingAlgStrList
                .Select(str => RoutingAlgNameIndexMap[str]).ToArray();
            _savedAlgSettings = AlgSettings.Copy();

            var pcbGenerator = new PcbProbDistrBasedGenerator(
                pcbElemsCountDistr: SimInputParamsVm.ElementCountDistr.Build(),
                newElemsPercentDistr: SimInputParamsVm.NewElemsPercentDistr.Build(),
                elementsDensityDistr: SimInputParamsVm.ElementsDensityDistr.Build(),
                pinsCountDistr: SimInputParamsVm.PinsCountDistr.Build(),
                pinDensityDistr: SimInputParamsVm.PinDensityDistr.Build(),
                numOfLayersDistr: SimInputParamsVm.NumOfLayersDistr.Build(),
                isDoubleSidePlacement: new Bernoulli(SimInputParamsVm.IsDoubleSidePlacementProb, _rndSource),
                isManualSchemeInputProb: new Bernoulli(SimInputParamsVm.IsManualSchemeInputProb, _rndSource)
            );

            var simuEventGenerator = new PcbDesignTechGenerator(
                requestsFlowDistrDays: new Exponential(SimInputParamsVm.TechPerYear * 4.2 / 365.0, _rndSource),
                pcbGenerator: pcbGenerator,
                rndSource: _rndSource
            );

            var preCalcEventsList = Enumerable.Range(0, SampleSize).Select(
                _ => simuEventGenerator.GenerateSimuEvent(SimInputParamsVm.FinalTime));
            var simuSystemFuncWrapper = new SimuSystemFuncWrapper(preCalcEventsList, SimInputParamsVm.FinalTime,
                _rndSource, SimInputParamsVm.GoalProductionTimeHours, SimInputParamsVm.RunUpSection, TimeSpan.Zero);
            var objectiveFunction = simuSystemFuncWrapper.ObjectiveFunction;


            var abcAlgorithm = new AbcAlgorithm(_savedAlgSettings, _rndSource, objectiveFunction);
            _lastOptimizationResultLog = abcAlgorithm.FindMinimum().ToList();

            var bestFoodSource = abcAlgorithm.BestFoodSource.Copy();
            LastOptimizationResultStr = bestFoodSource.ToString();
            LastOptimizationResult = new OptimizationResult
            (
                simulationInputParamsEm: SimInputParamsVm.Export(),
                generalSimulationSettings: GeneralSimulationSettings.CurSettings,
                cadConfigurationEm: new CadConfigurationEm(bestFoodSource)
            )
            {
                OptimizationTime = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"),
                AlgorithmSettings = _savedAlgSettings,
                ConfigurationScore = bestFoodSource.FuncValue
            };

            DrawLog();
            DrawPlot();
        }
        catch (Exception e) // ToDo: Убрать, заменить на отлов отдельных Exc
        {
            Debug.WriteLine(e);
            MessageViewModel.Message = e.Message;
        }
    }

    private void DrawPlot()
    {
        if (LastOptimizationResult is null) return;
        if (_lastOptimizationResultLog is null) return;

        OptimizationProcessPlot = new TabFixedPlotModel()
            {Title = "Оценка конфигурации САПР (значение цеелевой функции) / Итерация"};
        OptimizationProcessPlot.Axes.Add(new LinearAxis {Position = AxisPosition.Bottom, Title = "Итерация"});
        if (LastOptimizationResult.ConfigurationScore >= -1e-20)
            OptimizationProcessPlot.Axes.Add(new LogarithmicAxis
                {Position = AxisPosition.Left, Title = "Оценка конфигурации САПР"});
        else
            OptimizationProcessPlot.Axes.Add(new LinearAxis
                {Position = AxisPosition.Left, Title = "Оценка конфигурации САПР"});

        var lineSeries = new LineSeries {Color = OxyColors.Blue, StrokeThickness = 1.5};
        lineSeries.Points.AddRange(_lastOptimizationResultLog.Select((foodSource, i) =>
            new DataPoint(i, foodSource.FuncValue)));

        OptimizationProcessPlot.Series.Add(lineSeries);
        OptimizationProcessPlot.InvalidatePlot(true);
    }

    private void DrawLog()
    {
        if (_lastOptimizationResultLog is null) return;

        var (curIter, stringBuilder, divider) =
            (0, new StringBuilder(), Math.Max(1, Math.Ceiling(_lastOptimizationResultLog.Count / 250.0)));

        foreach (var curIterFoodSource in _lastOptimizationResultLog)
        {
            if (curIter % divider == 0 || curIter == _lastOptimizationResultLog.Count - 1)
                stringBuilder.Append(
                    $">>>Итерация {curIter}:{Environment.NewLine}{curIterFoodSource}{Environment.NewLine}");
            curIter++;
        }

        LastOptimizationResultLogStr = stringBuilder.ToString();
    }

    private void SaveLastOptimizationResult()
    {
        if (LastOptimizationResult is null) return;

        var dialogSettings = new SaveFileDialogSettings()
        {
            Title = "Сохранение Результатов Оптимизации",
            FileName = "AllOptimizationResults",
            InitialDirectory = $@"{Directory.GetCurrentDirectory()}\Files\CadConfigurations",
            Filter = "JSON file (*.json)|*.json|Text Documents (*.txt)|*.txt|All Files (*.*)|*.*"
        };

        var success = _dialogService.ShowSaveFileDialog(this, dialogSettings);
        if (success != true) return;

        var fileName = dialogSettings.FileName;

        var serializedResult = JsonConvert.SerializeObject(LastOptimizationResult, Formatting.Indented,
            new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Auto});

        new AppendFileSimpleLogger(fileName).Log(serializedResult);
    }

    private void SaveLastOptimizationLog()
    {
        if (LastOptimizationResult is null) return;
        if (_lastOptimizationResultLog is null) return;

        var dialogSettings = new SaveFileDialogSettings()
        {
            Title = "Сохранение Результатов Оптимизации",
            FileName = "LastOptimizationLog",
            InitialDirectory = $@"{Directory.GetCurrentDirectory()}\Files\Logs",
            Filter = "JSON file (*.json)|*.json|Text Documents (*.txt)|*.txt|All Files (*.*)|*.*"
        };

        var success = _dialogService.ShowSaveFileDialog(this, dialogSettings);
        if (success != true) return;

        var fileName = dialogSettings.FileName;

        var serializedResult = JsonConvert.SerializeObject(LastOptimizationResult, Formatting.Indented,
            new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Auto});

        var lastOptimizationResultLog = JsonConvert.SerializeObject(
            value: new
            {
                OptimizationLog = _lastOptimizationResultLog.Select(
                    (curIterationBest, curIteration) =>
                        new {Iteration = curIteration, BestResult = new CadConfigurationEm(curIterationBest)})
            },
            formatting: Formatting.Indented
        );

        new TruncateFileSimpleLogger(fileName).Log($"{serializedResult}{Environment.NewLine}{lastOptimizationResultLog}");
    }

    protected virtual void OnShareConfiguration()
    {
        if (LastOptimizationResult is not null)
            ShareConfiguration?.Invoke(LastOptimizationResult);
    }

    public event Action<OptimizationResult>? ShareConfiguration;

    public override string Error => String.Empty;
    public override bool IsValid => true;
    public override string this[string columnName] => String.Empty;
}