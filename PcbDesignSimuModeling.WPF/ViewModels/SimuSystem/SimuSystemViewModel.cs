using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MathNet.Numerics.Distributions;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using MvvmDialogs.FrameworkDialogs.SaveFile;
using Newtonsoft.Json;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using PcbDesignSimuModeling.WPF.Commands;
using PcbDesignSimuModeling.WPF.Models;
using PcbDesignSimuModeling.WPF.Models.Loggers;
using PcbDesignSimuModeling.WPF.Models.OptimizationModule;
using PcbDesignSimuModeling.WPF.Models.Resources;
using PcbDesignSimuModeling.WPF.Models.Resources.DesignerRes;
using PcbDesignSimuModeling.WPF.Models.Resources.EadSybSystems;
using PcbDesignSimuModeling.WPF.Models.Resources.EadSybSystems.PlacingSystems;
using PcbDesignSimuModeling.WPF.Models.Resources.EadSybSystems.RoutingSystems;
using PcbDesignSimuModeling.WPF.Models.SimuSystem;
using PcbDesignSimuModeling.WPF.Models.SimuSystem.SimulationEvents;
using PcbDesignSimuModeling.WPF.ViewModels.Helpers;
using PcbDesignSimuModeling.WPF.ViewModels.OptimizationModule;
using PcbDesignSimuModeling.WPF.ViewModels.Shared;

namespace PcbDesignSimuModeling.WPF.ViewModels.SimuSystem;

public class SimuSystemViewModel : BaseViewModel
{
    private readonly Random _rndSource;
    private readonly DialogService _dialogService = new();
    private StatCollector? _statCollector;

    public MessageViewModel MessageViewModel { get; }


    public CadConfigurationVm CadConfigurationVm { get; }
    public CadConfigExpenseReportVm CadConfigExpenseReportVm { get; private set; }
    public SimulationInputParamsVm SimInputParamsVm { get; }


    public SimulationResult? LastSimulationResult { get; private set; }
    public ProceduresTimesHandler? LastProceduresTimes { get; private set; }
    public bool IsImportWithoutParams { get; set; } = false;
    public bool IsDrawPlotsForAllThreads { get; set; } = false;
    public List<PlotModel> ResourceUsagePlots { get; private set; } = new();

    public string? LastSimulationResultLog { get; set; }
    public string LastSimulationResultLogSearch { get; set; } = String.Empty;
    public string LastSimulationResultLogSearchTransformed => LastSimulationResultLogSearch.Replace(" ", ".");


    public SimuSystemViewModel() : this(null, null)
    {
    }

    public SimuSystemViewModel(Random? rndSource = null, MessageViewModel? messageViewModel = null)
    {
        _rndSource = rndSource ?? new Random(1);
        MessageViewModel = messageViewModel ?? new MessageViewModel();

        SimInputParamsVm = new SimulationInputParamsVm(_rndSource, MessageViewModel);
        CadConfigurationVm = new CadConfigurationVm();
        CadConfigExpenseReportVm = new CadConfigExpenseReportVm(CadConfigurationVm);
    }


    public ICommand BeginSimulationCommand => new ActionCommand(_ => BeginSimulation(),
        _ => SimInputParamsVm is {IsImportFromFile: false} or {ImportedPcbDescriptions.Count: >= 1});

    public ICommand ImportConfigCommand => new ActionCommand(_ => ImportConfig());

    public ICommand SaveSimuResultCommand => new ActionCommand(_ => SaveSimuResult(),
        _ => LastSimulationResult is not null && !String.IsNullOrEmpty(LastSimulationResultLog));

    public ICommand SaveLastSimuLogCommand => new ActionCommand(_ => SaveLastSimuLog(),
        _ => !String.IsNullOrEmpty(LastSimulationResultLog));


    internal void BeginSimulation()
    {
        try
        {
            var resourcePool = new List<IResource>
                {CadConfigurationVm.Ram.Model, CadConfigurationVm.Server.Model, CadConfigurationVm.Cpu.Model};
            resourcePool.AddRange(Enumerable.Range(0, CadConfigurationVm.DesignersCount).Select(_ => new Designer()));

            var pcbAlgFactories = new EadSubSystemFactories(
                placingSysFactory: PlacingSysFactoryProvider.Create(CadConfigurationVm.SelectedPlacingAlgStr),
                routingSysFactory:
                RoutingSysFactoryProvider.Create(CadConfigurationVm.SelectedWireRoutingAlgStr));

            IPcbGenerator pcbGenerator;
            if (SimInputParamsVm.IsImportFromFile)
            {
                if (SimInputParamsVm.ImportedPcbDescriptions is null ||
                    SimInputParamsVm.ImportedPcbDescriptions.Count < 1)
                    throw new Exception("Количество импортируемых описаний печатных плат < 1");
                pcbGenerator = new PcbPoolBasedGenerator(SimInputParamsVm.ImportedPcbDescriptions, _rndSource);
            }
            else
            {
                pcbGenerator = new PcbProbDistrBasedGenerator(
                    pcbElemsCountDistr: SimInputParamsVm.ElementCountDistr.Build(),
                    newElemsPercentDistr: SimInputParamsVm.NewElemsPercentDistr.Build(),
                    elementsDensityDistr: SimInputParamsVm.ElementsDensityDistr.Build(),
                    pinsCountDistr: SimInputParamsVm.PinsCountDistr.Build(),
                    pinDensityDistr: SimInputParamsVm.PinDensityDistr.Build(),
                    numOfLayersDistr: SimInputParamsVm.NumOfLayersDistr.Build(),
                    isDoubleSidePlacement: new Bernoulli(SimInputParamsVm.IsDoubleSidePlacementProb, _rndSource),
                    isManualSchemeInputProb: new Bernoulli(SimInputParamsVm.IsManualSchemeInputProb, _rndSource)
                );
            }

            var pcbDesignTechGenerator = new PcbDesignTechGenerator(
                requestsFlowDistrDays: new Exponential(SimInputParamsVm.TechPerYear * 4.2 / 365.0, _rndSource),
                pcbGenerator: pcbGenerator,
                rndSource: _rndSource
            );
            var preCalcEvents = pcbDesignTechGenerator.GenerateSimuEvent(SimInputParamsVm.FinalTime);

            var resFailureGenerator = new ResourceFailureGenerator(resourcePool, _rndSource);
            preCalcEvents.InsertRangeAfterCondition(
                resFailureGenerator.GenerateSimuEvent(SimInputParamsVm.FinalTime),
                (itemSource, insItem) => insItem.ActivateTime > itemSource.ActivateTime);


            var inMemorySimpleLogger = new InMemorySimpleLogger();
            _statCollector = new StatCollector(resourcePool, inMemorySimpleLogger, SimInputParamsVm.RunUpSection);

            var simulator = new PcbDesignSimulator(
                preCalcEvents, resourcePool, pcbAlgFactories, SimInputParamsVm.RunUpSection, _statCollector);
            var simulationResult = simulator.Simulate(SimInputParamsVm.FinalTime);

            LastSimulationResultLog = inMemorySimpleLogger.GetData();
            DrawPlots();

            if (simulationResult.Values.Count < 1) return;

            var productionTimes = simulationResult.Values.Select(time => time).ToList();
            var avgProductionTimeHours = productionTimes.Average(time => time.TotalHours);
            var devProductionTimeHours = Math.Sqrt(
                productionTimes.Sum(time => Math.Pow(time.TotalHours - avgProductionTimeHours, 2)) /
                productionTimes.Count);
            var maxProductionTimeHours = productionTimes.MaxBy(time => time.TotalHours).TotalHours;

            var totalConfigCost = Convert.ToDouble(new CadConfigCostEstimator().EstimateFullCost(resourcePool));
            var configScore = new CadConfigScoreGetter2(SimInputParamsVm.GoalProductionTimeHours)
                .GetScore(totalConfigCost, avgProductionTimeHours);

            LastProceduresTimes = new ProceduresTimesHandler(_statCollector.ProceduresDurations);
            LastSimulationResult = new SimulationResult
            (
                cadConfigurationEm: CadConfigurationVm.Export(),
                simulationInputParamsEm: SimInputParamsVm.Export(),
                generalSimulationSettings: GeneralSimulationSettings.CurSettings
            )
            {
                ExperimentTime = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"),
                ConfigurtaionTotalCost = totalConfigCost,
                ConfigurationScore = configScore,
                AverageProductionTime = avgProductionTimeHours,
                DevProductionTime = devProductionTimeHours,
                MaxProductionTime = maxProductionTimeHours
            };
        }
        catch (Exception e) // ToDo: Убрать, заменить на отлов отдельных Exc
        {
            Debug.WriteLine(e);
            MessageViewModel.Message = e.Message;
        }
    }

    private void DrawPlots()
    {
        if (_statCollector is null) return;

        var activeTechsPlot = BuildTechsFlowPlot();

        TabFixedPlotModel BuildTechsFlowPlot()
        {
            var activeTechsSeries = new StairStepSeries {Color = OxyColors.Blue, YAxisKey = "1", StrokeThickness = 1.5};
            activeTechsSeries.Points.AddRange(_statCollector.TechInWorkCountList.Select(
                (techCount, i) => new DataPoint(
                    x: TimeSpanAxis.ToDouble(_statCollector.TimesSnapshots[i].RoundSec()),
                    y: techCount)));

            var techInputSeries = new StemSeries
            {
                Color = OxyColors.Red, MarkerType = MarkerType.Circle, MarkerSize = 1.75, YAxisKey = "2",
                StrokeThickness = 1.5
            };
            techInputSeries.Points.AddRange(_statCollector.TechInWorkCountList.SelectWithPreviousCustomFirst(
                0, (prevCount, curCount) => Math.Max(0, curCount - prevCount)).Select(
                (techCount, i) => new DataPoint(
                    x: TimeSpanAxis.ToDouble(_statCollector.TimesSnapshots[i].RoundSec()),
                    y: techCount)));

            var tabFixedPlotModel = new TabFixedPlotModel
                {Title = "Количество заявок в системе / Модельное время", TitleFontSize = 15};
            tabFixedPlotModel.Axes.Add(new TimeSpanAxis {Position = AxisPosition.Bottom, Title = "Модельное время"});
            tabFixedPlotModel.Axes.Add(new LinearAxis
                {Position = AxisPosition.Left, Title = "Количество заявок", Key = "1"});
            tabFixedPlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Right, Maximum = 2, Minimum = 0, MinimumDataMargin = 2, FilterMinValue = 0.5,
                Title = "Поступление заявок", Key = "2"
            });
            tabFixedPlotModel.Series.Add(techInputSeries);
            tabFixedPlotModel.Series.Add(activeTechsSeries);
            return tabFixedPlotModel;
        }

        
        var (threadsUsagePlot, taskCountPlot) = BuildCpuUsagePlots();

        (TabFixedPlotModel,TabFixedPlotModel) BuildCpuUsagePlots()
        {
            var threadsUsageSeries = new StairStepSeries {Color = OxyColors.Blue, StrokeThickness = 1.5};
            threadsUsageSeries.Points.AddRange(_statCollector.CpuClusterUsageSnapshots.Select(
                (usagePerThreadList, i) => new DataPoint(
                    x: TimeSpanAxis.ToDouble(_statCollector.TimesSnapshots[i].RoundSec()),
                    y: usagePerThreadList.Average() * 100)));

            var threadsUsagePlotModel = new TabFixedPlotModel {Title = "Загрузка ЦП / Модельное время", TitleFontSize = 15};
            threadsUsagePlotModel.Axes.Add(new TimeSpanAxis {Position = AxisPosition.Bottom, Title = "Модельное время"});
            threadsUsagePlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left, MaximumDataMargin = 1, Maximum = 100, Minimum = 0, MinimumDataMargin = 2,
                Title = "Загрузка ЦП (%)"
            });
            threadsUsagePlotModel.Series.Add(threadsUsageSeries);


            var taskCountSeries = new StairStepSeries {Color = OxyColors.Blue, StrokeThickness = 1.5};
            taskCountSeries.Points.AddRange(_statCollector.CpuClusterTaskCountSnapshots.Select(
                (taskCountPerThreadList, i) => new DataPoint(
                    x: TimeSpanAxis.ToDouble(_statCollector.TimesSnapshots[i].RoundSec()),
                    y: taskCountPerThreadList.Average())));

            var taskCountPlotModel = new TabFixedPlotModel
                {Title = "Количество задач на 1 поток / Модельное время", TitleFontSize = 15};
            taskCountPlotModel.Axes.Add(new TimeSpanAxis {Position = AxisPosition.Bottom, Title = "Модельное время"});
            taskCountPlotModel.Axes.Add(new LinearAxis
                {Position = AxisPosition.Left, Minimum = 0, MinimumDataMargin = 2, Title = "Задач на поток"});
            taskCountPlotModel.Series.Add(taskCountSeries);
            
            return (threadsUsagePlotModel, taskCountPlotModel);
        }


        var ramUsagePlot = BuildRamUsagePlot();

        TabFixedPlotModel BuildRamUsagePlot()
        {
            var ramUsageSeries = new StairStepSeries {Color = OxyColors.Blue, StrokeThickness = 1.5};
            ramUsageSeries.Points.AddRange(_statCollector.RamUsageSnapshots.Select(
                (ramUsage, i) => new DataPoint(
                    x: TimeSpanAxis.ToDouble(_statCollector.TimesSnapshots[i].RoundSec()),
                    y: ramUsage)));

            var ramUsagePlotModel = new TabFixedPlotModel
                {Title = "Использование оперативное памяти / Модельное время", TitleFontSize = 15};
            ramUsagePlotModel.Axes.Add(new TimeSpanAxis {Position = AxisPosition.Bottom, Title = "Модельное время"});
            ramUsagePlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left, MaximumDataMargin = 1, MinimumDataMargin = 2,
                Minimum = 0, Maximum = CadConfigurationVm.Ram.TotalAmount, Title = "Использование памяти"
            });
            ramUsagePlotModel.Series.Add(ramUsageSeries);
            return ramUsagePlotModel;
        }

        
        var designersCountPlot = BuildDesignersLoadPlot();

        TabFixedPlotModel BuildDesignersLoadPlot()
        {
            var busyOrIllDesignersCountSeries =
                new StairStepSeries {Color = OxyColors.Red, StrokeThickness = 1.5, YAxisKey = "1"};
            busyOrIllDesignersCountSeries.Points.AddRange(_statCollector.BusyOrIllDesignersSnapshots.Select(
                (busyOrIllDesignerCount, i) => new DataPoint(
                    x: TimeSpanAxis.ToDouble(_statCollector.TimesSnapshots[i].RoundSec()),
                    y: busyOrIllDesignerCount)));

            var busyDesignersCountSeries =
                new StairStepSeries {Color = OxyColors.Blue, StrokeThickness = 1.5, YAxisKey = "2"};
            busyDesignersCountSeries.Points.AddRange(_statCollector.BusyDesignersSnapshots.Select(
                (busyDesignerCount, i) => new DataPoint(
                    x: TimeSpanAxis.ToDouble(_statCollector.TimesSnapshots[i].RoundSec()),
                    y: busyDesignerCount)));

            var designersCountPlotModel = new TabFixedPlotModel
                {Title = "Число занятых (или болеющих) проектировщиков / Модельное время", TitleFontSize = 15};
            designersCountPlotModel.Axes.Add(new TimeSpanAxis {Position = AxisPosition.Bottom, Title = "Модельное время"});
            designersCountPlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Right, MaximumDataMargin = 1, Maximum = CadConfigurationVm.DesignersCount,
                Minimum = 0, Title = "Занятые или болеющие проектировщики", Key = "1", MinimumDataMargin = 2
            });
            designersCountPlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left, MaximumDataMargin = 1, Maximum = CadConfigurationVm.DesignersCount,
                Minimum = 0, Title = "Занятые проектировщики", Key = "2", MinimumDataMargin = 2,
            });
            designersCountPlotModel.Series.Add(busyOrIllDesignersCountSeries);
            designersCountPlotModel.Series.Add(busyDesignersCountSeries);
            return designersCountPlotModel;
        }

        
        var serverUsagePlot = BuildServerUsagePlot();

        TabFixedPlotModel BuildServerUsagePlot()
        {
            var serverUsageSeries = new StairStepSeries {Color = OxyColors.Blue, StrokeThickness = 1.5};
            serverUsageSeries.Points.AddRange(_statCollector.ServerSnapshots.Select(
                (serverUsage, i) => new DataPoint(
                    x: TimeSpanAxis.ToDouble(_statCollector.TimesSnapshots[i].RoundSec()),
                    y: serverUsage)));

            var serverUsagePlotModel = new TabFixedPlotModel
                {Title = "Число активных пользователей сервера / Модельное время", TitleFontSize = 15};
            serverUsagePlotModel.Axes.Add(new TimeSpanAxis {Position = AxisPosition.Bottom, Title = "Модельное время"});
            serverUsagePlotModel.Axes.Add(new LinearAxis {Position = AxisPosition.Left, Title = "Число пользователей"});
            serverUsagePlotModel.Series.Add(serverUsageSeries);
            return serverUsagePlotModel;
        }

        
        var plots = new List<PlotModel>
            {activeTechsPlot, threadsUsagePlot, taskCountPlot, ramUsagePlot, designersCountPlot, serverUsagePlot};


        // Per Threads Plots
        var cpuThreadCount = CadConfigurationVm.Cpu.ThreadCount;
        if (IsDrawPlotsForAllThreads && cpuThreadCount <= 40)
        {
            for (var j = 0; j < cpuThreadCount; j++)
            {
                var oneThreadUsageSeries = new StairStepSeries {Color = OxyColors.Blue, StrokeThickness = 1.5};
                oneThreadUsageSeries.Points.AddRange(_statCollector.CpuClusterUsageSnapshots.Select(
                    (usagePerThreadList, i) => new DataPoint(
                        x: TimeSpanAxis.ToDouble(_statCollector.TimesSnapshots[i].RoundSec()),
                        y: usagePerThreadList[j] * 100)));

                var oneThreadUsagePlot = new TabFixedPlotModel
                    {Title = $"Загрузка Потока{j} / Модельное время", TitleFontSize = 15};
                oneThreadUsagePlot.Axes.Add(
                    new TimeSpanAxis {Position = AxisPosition.Bottom, Title = "Модельное время"});
                oneThreadUsagePlot.Axes.Add(new LinearAxis
                {
                    Position = AxisPosition.Left, MaximumDataMargin = 1, Minimum = 0, Maximum = 100,
                    Title = $"Загрузка Потока{j} (%)", MinimumDataMargin = 2
                });
                oneThreadUsagePlot.Series.Add(oneThreadUsageSeries);
                plots.Add(oneThreadUsagePlot);
            }

            for (var j = 0; j < cpuThreadCount; j++)
            {
                var oneThreadTaskCountSeries = new StairStepSeries {Color = OxyColors.Blue, StrokeThickness = 1.5};
                oneThreadTaskCountSeries.Points.AddRange(_statCollector.CpuClusterTaskCountSnapshots.Select(
                    (taskCountPerThreadList, i) => new DataPoint(
                        x: TimeSpanAxis.ToDouble(_statCollector.TimesSnapshots[i].RoundSec()),
                        y: taskCountPerThreadList[j])));

                var oneThreadUsagePlot =
                    new TabFixedPlotModel
                        {Title = $"Количество задач на Потоке{j} / Модельное время", TitleFontSize = 15};
                oneThreadUsagePlot.Axes.Add(
                    new TimeSpanAxis {Position = AxisPosition.Bottom, Title = "Модельное время"});
                oneThreadUsagePlot.Axes.Add(new LinearAxis
                    {Position = AxisPosition.Left, Minimum = 0, MinimumDataMargin = 2, Title = $"Задач на Потоке{j}"});
                oneThreadUsagePlot.Series.Add(oneThreadTaskCountSeries);
                plots.Add(oneThreadUsagePlot);
            }
        }

        ResourceUsagePlots = plots;
    }


    private void SaveSimuResult()
    {
        if (LastSimulationResult is null) return;

        var dialogSettings = new SaveFileDialogSettings()
        {
            Title = "Сохранение Результатов Моделирования",
            FileName = "AllSimulationResults",
            InitialDirectory = $@"{Directory.GetCurrentDirectory()}\Files\CadConfigurations",
            Filter = "JSON file (*.json)|*.json|Text Documents (*.txt)|*.txt|All Files (*.*)|*.*"
        };

        var success = _dialogService.ShowSaveFileDialog(this, dialogSettings);
        if (success != true) return;

        var fileName = dialogSettings.FileName;

        var serializedResult = JsonConvert.SerializeObject(LastSimulationResult, Formatting.Indented,
            new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Auto});

        new AppendFileSimpleLogger(fileName).Log(serializedResult);
    }

    private void SaveLastSimuLog()
    {
        if (LastSimulationResult is null) return;
        if (LastSimulationResultLog is null) return;

        var dialogSettings = new SaveFileDialogSettings()
        {
            Title = "Сохранение Лога Моделирования",
            FileName = "LastSimulationLog",
            InitialDirectory = $@"{Directory.GetCurrentDirectory()}\Files\Logs",
            Filter = "Text Documents (*.txt)|*.txt|JSON file (*.json)|*.json|All Files (*.*)|*.*"
        };

        var success = _dialogService.ShowSaveFileDialog(this, dialogSettings);
        if (success != true) return;

        var fileName = dialogSettings.FileName;

        var serializedResult = JsonConvert.SerializeObject(LastSimulationResult, Formatting.Indented,
            new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Auto});

        new TruncateFileSimpleLogger(fileName).Log(
            $"{serializedResult}{Environment.NewLine}{Environment.NewLine}Лог Моделирования:{Environment.NewLine}{LastSimulationResultLog}");
    }


    public void ImportConfigFromOptimizationModule(OptimizationResult optimizationResultConfig)
    {
        CadConfigurationVm.Import(optimizationResultConfig.CadConfigurationEm);
        CadConfigExpenseReportVm = new CadConfigExpenseReportVm(CadConfigurationVm);
        if (IsImportWithoutParams) return;
        SimInputParamsVm.Import(optimizationResultConfig.SimulationInputParamsEm);
        GeneralSimulationSettings.ApplySettings(optimizationResultConfig.GeneralSimulationSettings);
    }

    private void ImportConfig()
    {
        var dialogSettings = new OpenFileDialogSettings
        {
            Title = "Импорт конфигурации САПР",
            InitialDirectory = $@"{Directory.GetCurrentDirectory()}\Files\CadConfigurations",
            Filter = "JSON file (*.json)|*.json|Text Documents (*.txt)|*.txt|All Files (*.*)|*.*"
        };

        var success = _dialogService.ShowOpenFileDialog(this, dialogSettings);
        if (success != true) return;

        var fileName = dialogSettings.FileName;
        try
        {
            var allText = File.ReadAllText(fileName);

            var simulationResult = JsonConvert.DeserializeObject<SimulationResult?>(allText,
                new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Auto});

            if (simulationResult is {CadConfigurationEm: { }})
            {
                CadConfigurationVm.Import(simulationResult.CadConfigurationEm);
                CadConfigExpenseReportVm = new CadConfigExpenseReportVm(CadConfigurationVm);
                if (IsImportWithoutParams) return;
                SimInputParamsVm.Import(simulationResult.SimulationInputParamsEm);
                GeneralSimulationSettings.ApplySettings(simulationResult.GeneralSimulationSettings);
                return;
            }

            var optimizationResult = JsonConvert.DeserializeObject<OptimizationResult?>(allText,
                new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Auto});

            if (optimizationResult != null)
            {
                CadConfigurationVm.Import(optimizationResult.CadConfigurationEm);
                CadConfigExpenseReportVm = new CadConfigExpenseReportVm(CadConfigurationVm);
                if (IsImportWithoutParams) return;
                SimInputParamsVm.Import(optimizationResult.SimulationInputParamsEm);
                GeneralSimulationSettings.ApplySettings(optimizationResult.GeneralSimulationSettings);
                return;
            }

            _dialogService.ShowMessageBox(this, "Не удалось импортировать конфигурацию", "Предупреждение!",
                icon: MessageBoxImage.Warning);
        }
        catch (FileNotFoundException fileNotFoundException)
        {
            _dialogService.ShowMessageBox(this, fileNotFoundException.Message, "Ошибка!", icon: MessageBoxImage.Error);
        }
        catch (Exception e) // ToDo: Убрать, заменить на отлов отдельных Exc
        {
            Debug.WriteLine(e);
            MessageViewModel.Message = e.Message;
        }
    }


    public override string Error => String.Empty;
    public override bool IsValid => true;
    public override string this[string columnName] => String.Empty;
}

// public List<Designer.ExperienceEn> ExperienceEnumList { get; } = Enum.GetValues(typeof(Designer.ExperienceEn)).Cast<Designer.ExperienceEn>().ToList();

//var costToTime = ((0.6 + 0.4 / (1.0 + totalConfigCost)) * (0.4 + 0.6 / (1 + Math.Pow(Math.Max(0, avgProductionTime.TotalDays - 9.0), 1))) - 0.6 * 0.4) / (1 - 0.6 * 0.4)
//var costToTime = (0.6 * (100000.0 / avgProductionTime.TotalDays)) / (0.4 * Convert.ToDouble(totalConfigCost));
//var costToTime = (0.6 + 0.4 * 400000 / totalConfigCost) * (0.4 + 0.6 / Math.Pow(1 + Math.Max(0, avgProductionTime.TotalDays - 4.0), 2));