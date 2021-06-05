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
using PCBdesignCADSimuModeling.Models.Technologies.PcbDesign;
using TestConcoleApp.Annotations;

namespace TestConcoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // string wireRoutingAlgName = "Example";
            // PcbAlgFactories pcbAlgFactories = new PcbAlgFactories(
            //     PlacingAlgProviderFactory.Create(placingAlgName),
            //     WireRoutingAlgProviderFactory.Create(wireRoutingAlgName));

            PcbParams pcbParams = new PcbParams(150, 0.8, true);

            var guid = Guid.NewGuid();
            var cpu1 = new CpuThreads(1, 2.5);
            var cpu2 = new CpuThreads(8, 2.5);


            var placingAlgName1 = "Последовательное размещение";
            var algFactory1 = PlacingAlgProviderFactory.Create(placingAlgName1);
            var alg1= algFactory1.Create(pcbParams);
            cpu1.TryGetResource(guid, alg1.MaxThreadUtilization);
            var cpu1Power = cpu1.ResValueForProc(guid);
            
            var placingAlgName2 = "Метод разбиения (параллельный)";
            var algFactory2 = PlacingAlgProviderFactory.Create(placingAlgName2);
            var alg2= algFactory2.Create(pcbParams);
            cpu2.TryGetResource(guid, alg2.MaxThreadUtilization);
            var cpu2Power = cpu2.ResValueForProc(guid);
            

            var time1 = TimeSpan.MaxValue;
            var time2 = TimeSpan.MaxValue;
            
            //alg1.UpdateModelTime(TimeSpan.FromMinutes(1), cpu1Power); 
            time1 = alg1.EstimateEndTime(cpu1Power);
            
            
            //alg2.UpdateModelTime(TimeSpan.FromMinutes(1), cpu2Power);
            time2 = alg2.EstimateEndTime(cpu2Power);


            Console.WriteLine(time1);
            Console.WriteLine(time2);
            Console.WriteLine(time2.TotalMinutes);
            

            Console.WriteLine(cpu1.ResValueForProc(guid));

            // Console.WriteLine($"OneThreadCompl: {convolutionOne(pcbParams)}");
            // Console.WriteLine($"MultiThreadCompl: {convolutionMulti(pcbParams)}");
            // Console.WriteLine(convolutionMulti(pcbParams) / (double)convolutionOne(pcbParams));
            //ComplexityEstimator estimator = new ComplexityEstimator(pcbParams, convolution);
        }

        private static double TestServerPower()
        {
            var server = new Server(10);
            var serverPower = 1.8 / Math.Exp(40.0 / server.InternetSpeed);
            return serverPower;
        }

        private static void TestDesignerPower()
        {
            var designer = new Designer(Designer.ExperienceEn.Little);

            var maxDesigner = Enum.GetValues<Designer.ExperienceEn>().Max();

            var a = 0.5 + (double) designer.Experience / (double) maxDesigner;
            Console.WriteLine(a);
        }

        private static void TestDistr()
        {
            IContinuousDistribution a = new Normal(0, 0);

            var z = 0;
            for (var i = 0; i < 100000; i++)
            {
                if (a.Sample() > new ContinuousUniform().Sample())
                    z++;
            }

            Console.WriteLine(z / 100000.0);
        }

        private static void TestSimulation1()
        {
            //

            List<Designer> designers = new()
            {
                new Designer(Designer.ExperienceEn.Average),
                new Designer(Designer.ExperienceEn.Average)
            };
            CpuThreads cpuThreads = new(16, 2.5);
            Server server = new(200);

            List<IResource> resourcePool = new();
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


            double techIntervalDistrMean = new TimeSpan(0, 3, 0, 0).TotalSeconds;
            double techIntervalDistrDev = new TimeSpan(1, 0, 0).TotalSeconds;
            var techIntervalDistr = new Normal(techIntervalDistrMean, techIntervalDistrDev);


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