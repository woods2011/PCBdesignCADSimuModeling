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

        public List<string> SelectedPlacingAlgStrList { get; set; } = new();

        public IReadOnlyList<string> WireRoutingAlgStrList { get; } = new List<string>
            { WireRoutingAlgProviderFactory.WireRoutingWaveStr, WireRoutingAlgProviderFactory.WireRoutingChannelStr };

        public List<string> SelectedWireRoutingAlgStrList { get; set; } = new();


        public TechIntervalBuilderVm TechIntervalDistr { get; }
        public DblNormalDistributionBuilderVm ElementCountDistr { get; }
        public DblNormalDistributionBuilderVm DimensionUsagePctDistr { get; }
        public double VariousSizePctProb { get; set; } = 0.8;
        public TimeSpan FinalTime { get; set; } = TimeSpan.FromDays(30);


        public OptimizationModuleViewModel(Random? rndSource = null)
        {
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
            new ActionCommand(_ => BeginOptimizationHandler());

        public ICommand SaveLastOptimizationResultCommand => new ActionCommand(_ => SaveLastOptimizationResultHandler(),
            _ => LastResult is not null);

        public ICommand SaveLastOptimizationLogCommand => new ActionCommand(_ => SaveLastOptimizationLogHandler(),
            _ => _lastResultLog is not null && _lastResultLog.Count >= 1);


        public void BeginOptimizationHandler()
        {
            try
            {
                _savedAlgSettings = AlgSettings.Copy();

                var simuEventGenerator = new SimuEventGenerator(
                    timeBetweenTechsDistr: TechIntervalDistr.Build(),
                    pcbElemsCountDistr: ElementCountDistr.Build(),
                    pcbDimUsagePctDistr: DimensionUsagePctDistr.Build(),
                    pcbElemsIsVarSizeProb: VariousSizePctProb,
                    random: _rndSource);

                var simuSystemFuncWrapper = new SimuSystemFuncWrapper(simuEventGenerator, FinalTime);
                Func<double, double, double, double, double, double, double> objectiveFunction =
                    simuSystemFuncWrapper.ObjectiveFunction;

                var abcAlgorithm = new AbcAlgorithm(_savedAlgSettings, _rndSource, objectiveFunction);

                _lastResultLog = abcAlgorithm.FindMinimum().Select(foodSource =>
                {
                    var source = foodSource.Copy();
                    source.Cost *= -1.0;
                    return source;
                }).ToList();
                LastResult = abcAlgorithm.BestFoodSource.Copy(); // ToDo: check
                LastResult.Cost *= -1.0;
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

            var tempLastResult = LastResult.Copy();
            tempLastResult.Cost *= -1.0;
            var tempLastResultLog = _lastResultLog.Select(foodSource =>
            {
                var source = foodSource.Copy();
                source.Cost *= -1.0;
                return source;
            }).ToList();

            PlotModel = new PlotModel { Title = "Значение Функции / Итерация" };
            PlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "Итерация" });
            if (LastResult.Cost >= -1e-20)
                PlotModel.Axes.Add(new LogarithmicAxis { Position = AxisPosition.Left, Title = "Значение Функции" });
            else
                PlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Значение Функции" });

            var lineSeries = new LineSeries { Color = OxyColors.Blue, MarkerType = MarkerType.Cross };
            lineSeries.Points.AddRange(_lastResultLog.Select((foodSource, i) => new DataPoint(i, foodSource.Cost)));

            PlotModel.Series.Add(lineSeries);
            PlotModel.InvalidatePlot(true);
        }

        private void DrawLog()
        {
            if (_lastResultLog is null) return;

            var (curIter, stringBuilder, divider) =
                (0, new StringBuilder(), Math.Max(1, Math.Ceiling(_lastResultLog.Count / 500.0)));

            foreach (var iterBestBacteria in _lastResultLog)
            {
                if (curIter % divider == 0 || curIter == _lastResultLog.Count - 1)
                    stringBuilder.Append(
                        $"Итерация {curIter}: Лучший результат: {iterBestBacteria.Cost} в точке:" +
                        $"({iterBestBacteria.X1} ; {iterBestBacteria.X2} ; {iterBestBacteria.X3} ; {iterBestBacteria.X4} ; {iterBestBacteria.X5} ; {iterBestBacteria.X6})" +
                        $"{Environment.NewLine}"
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
                Formatting.Indented, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });

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
                Formatting.Indented, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });

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