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

namespace PCBdesignCADSimuModeling.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public MainViewModel()
        {
        }


        public MessageViewModel MessageViewModel { get; } = new();
        public ICommand BeginSimulation => new ActionCommand(_ => BeginSimulationHandler());


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
        
        
        private void BeginSimulationHandler()
        {
            foreach (var designer in DesignersList)
                Debug.WriteLine(designer.Experience);

            Debug.WriteLine(Server.InternetSpeed);
            Debug.WriteLine($"{Cpu.ThreadCount} | {Cpu.ClockRate}");

            // List<Designer> designers = new()
            // {
            //     new Designer(Designer.ExperienceEn.Average),
            //     new Designer(Designer.ExperienceEn.Average)
            // };
            // CpuThreads cpuThreads = new(16, 2.5);
            // Server server = new(200);
            //
            // List<IResource> resourcePool = new();
            // resourcePool.AddRange(designers);
            // resourcePool.Add(cpuThreads);
            // resourcePool.Add(server);
            //
            //
            // //
            //
            //
            // string placingAlgName = "Example";
            // string wireRoutingAlgName = "Example";
            // PcbAlgFactories pcbAlgFactories = new PcbAlgFactories(
            //     PlacingAlgProviderFactory.Create(placingAlgName),
            //     WireRoutingAlgProviderFactory.Create(wireRoutingAlgName));
            //
            //
            // //
            //
            //
            // double intervalDistrMean = new TimeSpan(0, 3, 0, 0).TotalSeconds;
            // double intervalDistrDev = new TimeSpan(1, 0, 0).TotalSeconds;
            // var techIntervalDistr = new Normal(intervalDistrMean, intervalDistrDev);
            //
            //
            // int elementCountMean = 2000;
            // int elementCountDev = 100;
            // var elementCountDistr = new Normal(elementCountMean, elementCountDev);
            //
            // double dimensionUsagePctMean = 0.8;
            // double dimensionUsagePctDev = 0.1;
            // var dimensionUsagePctDistr = new Normal(dimensionUsagePctMean, dimensionUsagePctDev);
            //
            // double variousSizePctMean = 0.7;
            // var variousSizePctDistr = new Beta(variousSizePctMean, 1.0 - variousSizePctMean);
            //
            //
            // var simuEventGenerator = new SimuEventGenerator.Builder()
            //     .NewTechInterval(techIntervalDistr)
            //     .PcbParams(elementCountDistr, dimensionUsagePctDistr, variousSizePctDistr)
            //     .Build();
            //
            // //
            //
            //
            // ISimpleLogger logger = new ConsoleSimpleLogger();
            //
            // TimeSpan finalTime = TimeSpan.FromDays(50);
            //
            // PcbDesignCadSimulator simulator =
            //     new PcbDesignCadSimulator(simuEventGenerator, resourcePool, pcbAlgFactories, logger);
            // simulator.Simulate(finalTime);
        }
    }
}


// public List<Designer.ExperienceEn> ExperienceEnumList { get; } = Enum.GetValues(typeof(Designer.ExperienceEn)).Cast<Designer.ExperienceEn>().ToList();