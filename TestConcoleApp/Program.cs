using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using MathNet.Numerics.Distributions;
using Microsoft.Extensions.Logging.Abstractions;
using PCBdesignCADSimuModeling.Models.Loggers;
using PCBdesignCADSimuModeling.Models.Resources;
using PCBdesignCADSimuModeling.Models.Resources.Algorithms;
using PCBdesignCADSimuModeling.Models.Resources.Algorithms.PlacingAlgorithms;
using PCBdesignCADSimuModeling.Models.Resources.Algorithms.WireRoutingAlgorithms;
using PCBdesignCADSimuModeling.Models.Resources.ResourceRequests;
using PCBdesignCADSimuModeling.Models.SimuSystem;
using PCBdesignCADSimuModeling.Models.SimuSystem.SimulationEvents;

namespace TestConcoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //

            List<Designer> designers = new()
            {
                new Designer(Designer.ExperienceEn.Average),
                new Designer(Designer.ExperienceEn.Average)
            };
            CpuThreads cpuThreads = new(16, 2.5);
            Server server = new(200);

            List<Resource> resourcePool = new();
            resourcePool.AddRange(designers);
            resourcePool.Add(cpuThreads);
            resourcePool.Add(server);

            

            //


            string placingAlgName = "Example";
            string wireRoutingAlgName = "Example";
            PcbAlgFactories pcbAlgFactories = new PcbAlgFactories(
                PlacingAlgProviderFactory.Create(placingAlgName),
                WireRoutingAlgProviderFactory.Create(wireRoutingAlgName));


            //


            double intervalDistrMean = new TimeSpan(0, 3, 0, 0).TotalSeconds;
            double intervalDistrDev = new TimeSpan(1, 0, 0).TotalSeconds;
            var techIntervalDistr = new Normal(intervalDistrMean, intervalDistrDev);


            int elementCountMean = 2000;
            int elementCountDev = 100;
            var elementCountDistr = new Normal(elementCountMean, elementCountDev);

            double dimensionUsagePctMean = 0.8;
            double dimensionUsagePctDev = 0.1;
            var dimensionUsagePctDistr = new Normal(dimensionUsagePctMean, dimensionUsagePctDev);

            double variousSizePctMean = 0.7;
            var variousSizePctDistr = new Beta(variousSizePctMean, 1.0 - variousSizePctMean);


            var simuEventGenerator = new SimuEventGenerator.Builder()
                .NewTechInterval(techIntervalDistr)
                .PcbParams(elementCountDistr, dimensionUsagePctDistr, variousSizePctDistr)
                .Build();

            //


            ISimpleLogger logger = new ConsoleSimpleLogger();
            
            TimeSpan finalTime = TimeSpan.FromDays(50);

            PcbDesignCadSimulator simulator =
                new PcbDesignCadSimulator(simuEventGenerator, resourcePool, pcbAlgFactories, logger);
            simulator.Simulate(finalTime);
            
        }
    }
    
    
}