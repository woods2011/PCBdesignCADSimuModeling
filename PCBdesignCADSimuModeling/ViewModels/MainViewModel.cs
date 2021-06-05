using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using MathNet.Numerics.Distributions;
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
        public MainViewModel()
        {
        }


        public ICommand BeginSimulation => new ActionCommand(_ => BeginSimulationHandler());

        //

        public ICommand AddDesigner =>
            new ActionCommand(_ => DesignersList.Add(new Designer(Designer.ExperienceEn.Little)));

        public ICommand RemoveDesigner => new ActionCommand(_ => DesignersList.Remove(DesignersList.LastOrDefault()));

        public ObservableCollection<Designer> DesignersList { get; } = new()
        {
            new Designer(Designer.ExperienceEn.Little),
            new Designer(Designer.ExperienceEn.Little),
            new Designer(Designer.ExperienceEn.Little),
        };

        
        public Server Server { get; } = new(200);
        public CpuThreads Cpu { get; } = new(16, 2.5);

        
        //

        
        public TechIntervalBuilderDisplayModel TechIntervalDistr { get; } =
            new(new TimeSpan(1, 20, 0, 0), new TimeSpan(6, 30, 0));

        public DistributionBuilderDisplayModelDbl ElementCountDistr { get; } =
            new DistributionBuilderDisplayModelDbl(150, 15);

        public DistributionBuilderDisplayModelDbl DimensionUsagePctDistr { get; } =
            new DistributionBuilderDisplayModelDbl(0.6, 0.1);

        public double VariousSizePctMean { get; set; } = 0.7;
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

        
        private void BeginSimulationHandler()
        {
            var fileSimpleLogger = new FileSimpleLogger();
            ISimpleLogger logger = new CompositionSimpleLogger(new List<ISimpleLogger>()
            {
                fileSimpleLogger, new DebugSimpleLogger()
            });


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


            var simulator = new PcbDesignCadSimulator(simuEventGenerator, resourcePool, pcbAlgFactories, logger);
            //simulator.Simulate(FinalTime);
        }
    }
}


// public List<Designer.ExperienceEn> ExperienceEnumList { get; } = Enum.GetValues(typeof(Designer.ExperienceEn)).Cast<Designer.ExperienceEn>().ToList();