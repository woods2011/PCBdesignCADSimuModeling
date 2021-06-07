using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using MathNet.Numerics.Distributions;
using Newtonsoft.Json;
using PCBdesignCADSimuModeling.Commands;
using PCBdesignCADSimuModeling.Models.Loggers;
using PCBdesignCADSimuModeling.Models.Resources;
using PCBdesignCADSimuModeling.Models.Resources.Algorithms;
using PCBdesignCADSimuModeling.Models.Resources.Algorithms.PlacingAlgorithms;
using PCBdesignCADSimuModeling.Models.Resources.Algorithms.WireRoutingAlgorithms;
using PCBdesignCADSimuModeling.Models.SimuSystem;
using PCBdesignCADSimuModeling.Models.SimuSystem.SimulationEvents;
using PCBdesignCADSimuModeling.Models.Technologies.PcbDesign;

namespace PCBdesignCADSimuModeling.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private string _simuResultStr = String.Empty;
        private string _simuResultSearchStr = "Технология: ";
        private SimulationResult _lastSimulationResult = new SimulationResult();
        private FileSimpleLogger _fileSimpleLogger;


        public MainViewModel()
        {
        }


        public ICommand BeginSimulation => new ActionCommand(_ => BeginSimulationHandler(), _ => DesignersList.Count >= 1);

        public ICommand SaveSimuResult => new ActionCommand(_ => SaveSimuResultHandler(),
            _ => LastSimulationResult is not null && !String.IsNullOrEmpty(SimuResultStr));

        public ICommand SaveLastSimuLog => new ActionCommand(_ => SaveLastSimuLogHandler(),
            _ => _fileSimpleLogger is not null && !String.IsNullOrEmpty(SimuResultStr));


        //  


        public ICommand AddDesigner =>
            new ActionCommand(_ => DesignersList.Add(new Designer(Designer.ExperienceEn.Little)));

        public ICommand RemoveDesigner => new ActionCommand(_ => DesignersList.Remove(DesignersList.LastOrDefault()));

        public ObservableCollection<Designer> DesignersList { get; } = new()
        {
            new Designer(Designer.ExperienceEn.Little),
            new Designer(Designer.ExperienceEn.Average),
        };


        public Server Server { get; } = new(150);
        public CpuThreads Cpu { get; } = new(16, 2.5);


        //


        public TechIntervalBuilderDisplayModel TechIntervalDistr { get; } =
            new(new TimeSpan(1, 20, 0, 0), new TimeSpan(6, 30, 0));

        public DistributionBuilderDisplayModelDbl ElementCountDistr { get; } =
            new DistributionBuilderDisplayModelDbl(150, 15);

        public DistributionBuilderDisplayModelDbl DimensionUsagePctDistr { get; } =
            new DistributionBuilderDisplayModelDbl(0.6, 0.1);

        public double VariousSizePctMean { get; set; } = 0.8;
        public TimeSpan FinalTime { get; set; } = TimeSpan.FromDays(30);


        //


        public IReadOnlyList<string> PlacingAlgsStrs { get; } = new List<string>()
        {
            PlacingAlgProviderFactory.PlacingSequentialStr, PlacingAlgProviderFactory.PlacingPartitioningStr
        };

        public string SelectedPlacingAlgStr { get; set; } = PlacingAlgProviderFactory.PlacingSequentialStr;


        public IReadOnlyList<string> WireRoutingAlgsStrs { get; } = new List<string>()
        {
            WireRoutingAlgProviderFactory.WireRoutingWaveStr, WireRoutingAlgProviderFactory.WireRoutingChannelStr
        };

        public string SelectedWireRoutingAlgStr { get; set; } = WireRoutingAlgProviderFactory.WireRoutingWaveStr;


        //

        public MessageViewModel MessageViewModel { get; } = new();

        //

        public string SimuResultStr
        {
            get => _simuResultStr;
            set
            {
                _simuResultStr = value;
                OnPropertyChanged();
            }
        }

        public string SimuResultSearchStr
        {
            get => _simuResultSearchStr;
            set
            {
                _simuResultSearchStr = value;
                OnPropertyChanged(nameof(SimuResultSearchStrFake));
            }
        }

        public string SimuResultSearchStrFake => SimuResultSearchStr.Replace(" ", ".");

        //

        public string LastResultLogFileName { get; set; } = Directory.GetCurrentDirectory() + @"\LastSimulationLog.txt";

        public string AllResultsFileName { get; set; } =
            Directory.GetCurrentDirectory() + @"\AllSimulationResults.json";

        public SimulationResult LastSimulationResult
        {
            get => _lastSimulationResult;
            set
            {
                _lastSimulationResult = value;
                OnPropertyChanged();
            }
        }
        //

        private void BeginSimulationHandler()
        {
            _fileSimpleLogger = new FileSimpleLogger();
            ISimpleLogger logger = new CompositionSimpleLogger(new List<ISimpleLogger>()
            {
                _fileSimpleLogger, new DebugSimpleLogger()
            });
            PcbDesignTechnology.Id = 0;


            //


            List<IResource> resourcePool = new();
            resourcePool.AddRange(DesignersList);
            resourcePool.Add(Cpu);
            resourcePool.Add(Server);


            PcbAlgFactories pcbAlgFactories = new PcbAlgFactories(
                PlacingAlgProviderFactory.Create(SelectedPlacingAlgStr),
                WireRoutingAlgProviderFactory.Create(SelectedWireRoutingAlgStr));


            var simuEventGenerator = new SimuEventGenerator.Builder()
                .NewTechInterval(TechIntervalDistr.Build())
                .PcbParams(ElementCountDistr.Build(),
                    DimensionUsagePctDistr.Build(),
                    variousSizePctDistr: new Beta(VariousSizePctMean, 1.0 - VariousSizePctMean))
                .Build();

            //

            var simulator = new PcbDesignCadSimulator(simuEventGenerator,
                resourcePool.Select(resource => resource.Clone()).ToList(), pcbAlgFactories, logger);
            var simRes = simulator.Simulate(FinalTime);
            SimuResultStr = _fileSimpleLogger.GetData();

            
            
            var productionTimesSec =  simRes.Select(pair =>
                pair.Value.Finish.TotalSeconds - pair.Value.Start.TotalSeconds).ToList();
            var avgTimeSec = productionTimesSec.Average(time => time);
            var devTimeSec = Math.Sqrt(productionTimesSec.Sum(time => Math.Pow(time - avgTimeSec, 2)) / productionTimesSec.Count);


            var totalConfigCost = resourcePool.Sum(resource => resource.Cost) + DesignersList.Count * 10000.0;
            var averageProductionTime = TimeSpan.FromSeconds(avgTimeSec);
            var devProductionTime = TimeSpan.FromSeconds(devTimeSec);
            
            LastSimulationResult = new SimulationResult()
            {
                RealTime = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"),
                ResourcePool = resourcePool,
                SelectedPlacingAlgStr = SelectedPlacingAlgStr,
                SelectedWireRoutingAlgStr = SelectedWireRoutingAlgStr,
                TechIntervalDistr = TechIntervalDistr,
                ElementCountDistr = ElementCountDistr,
                DimensionUsagePctDistr = DimensionUsagePctDistr,
                VariousSizePctMean = VariousSizePctMean,
                FinalTime = FinalTime,
                AverageProductionTime = averageProductionTime,
                DevProductionTime = devProductionTime,
                TotalCost = totalConfigCost,    
                CostToTime = 0.6 * (100000.0 / averageProductionTime.TotalDays) / (0.4 * totalConfigCost)
            };

            //

            
            var a = averageProductionTime;
        }


        private void SaveSimuResultHandler()
        {
            var fileLogger = new FileSimpleLogger();
            var serializeResult = JsonConvert.SerializeObject(LastSimulationResult, Formatting.Indented,
                new JsonSerializerSettings() {TypeNameHandling = TypeNameHandling.Auto});
            fileLogger.Log(serializeResult);
            fileLogger.LogToFile(AllResultsFileName);
        }

        private void SaveLastSimuLogHandler()
        {
            _fileSimpleLogger.Log("Сводка:");
            var serializeResult = JsonConvert.SerializeObject(LastSimulationResult, Formatting.Indented,
                new JsonSerializerSettings() {TypeNameHandling = TypeNameHandling.Auto});
            _fileSimpleLogger.Log(serializeResult);
            
            _fileSimpleLogger.LogToFileTruncate(LastResultLogFileName);

        }
    }
}


// public List<Designer.ExperienceEn> ExperienceEnumList { get; } = Enum.GetValues(typeof(Designer.ExperienceEn)).Cast<Designer.ExperienceEn>().ToList();