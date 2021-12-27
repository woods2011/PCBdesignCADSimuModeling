using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Newtonsoft.Json;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using PcbDesignCADSimuModeling.Commands;
using PcbDesignCADSimuModeling.Models.Loggers;
using PcbDesignCADSimuModeling.Models.OptimizationModule;
using PcbDesignCADSimuModeling.Models.Resources;
using PcbDesignCADSimuModeling.Models.Resources.Algorithms;
using PcbDesignCADSimuModeling.Models.Resources.Algorithms.PlacingAlgorithms;
using PcbDesignCADSimuModeling.Models.Resources.Algorithms.WireRoutingAlgorithms;
using PcbDesignCADSimuModeling.Models.SimuSystem;
using PcbDesignCADSimuModeling.Models.SimuSystem.SimulationEvents;

namespace PcbDesignCADSimuModeling.ViewModels
{
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
            { PlacingAlgProviderFactory.PlacingSequentialStr, PlacingAlgProviderFactory.PlacingPartitioningStr };

        public List<string> SelectedPlacingAlgStrList { get; set; }

        public IReadOnlyList<string> WireRoutingAlgStrList { get; } = new List<string>
            { WireRoutingAlgProviderFactory.WireRoutingWaveStr, WireRoutingAlgProviderFactory.WireRoutingChannelStr };

        public List<string> SelectedWireRoutingAlgStrList { get; set; }


        public TechIntervalBuilderVm TechIntervalDistr { get; }
        public DblNormalDistributionBuilderVm ElementCountDistr { get; }
        public DblNormalDistributionBuilderVm DimensionUsagePctDistr { get; }
        public double VariousSizePctProb { get; set; } = 0.8;
        public TimeSpan FinalTime { get; set; } = TimeSpan.FromDays(30);


        public OptimizationModuleViewModel(Random? rndSource = null)
        {
            SelectedPlacingAlgStrList = PlacingAlgStrList.ToList();
            SelectedWireRoutingAlgStrList = WireRoutingAlgStrList.ToList();

            _rndSource = rndSource ?? new Random(1);
            _savedAlgSettings = AlgSettings.Copy();

            TechIntervalDistr =
                new TechIntervalBuilderVm(new TimeSpan(1, 20, 0, 0), new TimeSpan(6, 30, 0), _rndSource);
            ElementCountDistr = new DblNormalDistributionBuilderVm(150, 15, _rndSource);
            DimensionUsagePctDistr = new DblNormalDistributionBuilderVm(0.6, 0.1, _rndSource);

            PlotModel = new PlotModel { Title = "Значение Функции / Поколение" };
            PlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "Поколение" });
            PlotModel.Axes.Add(new LogarithmicAxis { Position = AxisPosition.Left, Title = "Значение Функции" });
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

                var simuEventGenerator = new SimuEventGenerator(
                    timeBetweenTechsDistr: TechIntervalDistr.Build(),
                    pcbElemsCountDistr: ElementCountDistr.Build(),
                    pcbDimUsagePctDistr: DimensionUsagePctDistr.Build(),
                    pcbElemsIsVarSizeProb: VariousSizePctProb,
                    random: _rndSource);

                var minFinishedTechs = (int)Math.Round(FinalTime / TechIntervalDistr.Mean * 0.8);

                var simuSystemFuncWrapper = new SimuSystemFuncWrapper(simuEventGenerator, FinalTime, minFinishedTechs);
                Func<int, double, double, int, int, int, double> objectiveFunction =
                    simuSystemFuncWrapper.ObjectiveFunction;

                var abcAlgorithm = new AbcAlgorithm(_savedAlgSettings, _rndSource, objectiveFunction);

                _lastResultLog = abcAlgorithm.FindMinimum().Select(foodSource =>
                {
                    var source = foodSource.Copy();
                    source.FuncValue *= -1.0;
                    return source;
                }).ToList();
                var lastResult = abcAlgorithm.BestFoodSource.Copy();
                lastResult.FuncValue *= -1.0; // ToDo: fix
                LastResult = lastResult;

                // LastResult = _lastResultLog.LastOrDefault();

                DrawLog();
                DrawPlot();
            }
            catch (Exception e)
            {
                MessageViewModel.Message = e.Message;
            }
        }

        private void DrawPlot()
        {
            if (LastResult is null) return;
            if (_lastResultLog is null) return;


            PlotModel = new PlotModel { Title = "Значение Функции / Итерация" };
            PlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "Итерация" });
            if (LastResult.FuncValue >= -1e-20)
                PlotModel.Axes.Add(new LogarithmicAxis { Position = AxisPosition.Left, Title = "Значение Функции" });
            else
                PlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Значение Функции" });

            var lineSeries = new LineSeries { Color = OxyColors.Blue, MarkerType = MarkerType.Cross };
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
                new { SaveTime = DateTime.Now, AlgorithmInput = _savedAlgSettings, OptimizationResult = LastResult },
                Formatting.Indented);

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
}