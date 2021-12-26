using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using MathNet.Numerics.Distributions;
using Microsoft.Extensions.Logging.Abstractions;
using PcbDesignCADSimuModeling.Models.Loggers;
using PcbDesignCADSimuModeling.Models.Resources;
using PcbDesignCADSimuModeling.Models.Resources.Algorithms;
using PcbDesignCADSimuModeling.Models.Resources.Algorithms.PlacingAlgorithms;
using PcbDesignCADSimuModeling.Models.Resources.Algorithms.WireRoutingAlgorithms;
using PcbDesignCADSimuModeling.Models.Resources.ResourceRequests;
using PcbDesignCADSimuModeling.Models.SimuSystem;
using PcbDesignCADSimuModeling.Models.SimuSystem.SimulationEvents;
using PcbDesignCADSimuModeling.Models.Technologies.PcbDesign;
using PcbDesignCADSimuModeling.Models.Technologies.PcbDesign.ProjectProcedures;

namespace TestConcoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var servers = new List<Server>()
            {
                new Server(25),
                new Server(35),
                new Server(45),
                new Server(55),
                new Server(65),
                new Server(75),
                new Server(85),
                new Server(100),
                new Server(120),
                new Server(150),
                new Server(180),
                new Server(210),
                new Server(240),
                new Server(270),
                new Server(300)
            };

            servers.ForEach(server =>
                Console.WriteLine(
                    $"{server.InternetSpeed} : {(0.5 + (1.8 / Math.Exp(80.0 / server.ResValueForProc(PcbDesignProcedure.NewId))) * 0.5) / server.Cost * 1000}"));
        }

        // private static void TestCpuPowerPerCent()
        // {
        //     var cpuThreadsList = new List<CpuThreads>()
        //     {
        //         new CpuThreads(1, 3.5),
        //         new CpuThreads(1, 4.5),
        //         new CpuThreads(8, 2.5),
        //         new CpuThreads(8, 3),
        //         new CpuThreads(8, 4),
        //         new CpuThreads(16, 2.5),
        //         new CpuThreads(16, 3),
        //         new CpuThreads(16, 4)
        //     };
        //
        //     var list = new List<double>();
        //
        //     foreach (var cpuThreads in cpuThreadsList)
        //     {
        //         var newGuid = PcbDesignProcedure.NewId;
        //         cpuThreads.TryGetResource(newGuid, cpuThreads.ThreadCount);
        //         list.Add(cpuThreads.ResValueForProc(newGuid) / cpuThreads.Cost);
        //     }
        //
        //     cpuThreadsList.ForEach(threads => Console.WriteLine(threads.Cost));
        //     list.ForEach(Console.WriteLine);
        // }
        //
        // private static void WireRoutingTest()
        // {
        //     var pcbParams = new PcbParams(150, 0.6, true);
        //
        //     var guid = PcbDesignProcedure.NewId;
        //     var cpu1 = new CpuThreads(1, 2.5);
        //     var cpu2 = new CpuThreads(8, 2.5);
        //
        //
        //     var algFactory1 = WireRoutingAlgProviderFactory.Create(WireRoutingAlgProviderFactory.WireRoutingWaveStr);
        //     var alg1 = algFactory1.Create(pcbParams);
        //     cpu1.TryGetResource(guid, alg1.MaxThreadUtilization);
        //     var cpu1Power = cpu1.ResValueForProc(guid);
        //
        //     var algFactory2 = WireRoutingAlgProviderFactory.Create(WireRoutingAlgProviderFactory.WireRoutingChannelStr);
        //     var alg2 = algFactory2.Create(pcbParams);
        //     cpu2.TryGetResource(guid, alg2.MaxThreadUtilization);
        //     var cpu2Power = cpu2.ResValueForProc(guid);
        //
        //
        //     var time1 = alg1.EstimateEndTime(cpu1Power);
        //     var time2 = alg2.EstimateEndTime(cpu2Power);
        //
        //
        //     Console.WriteLine(time1);
        //     Console.WriteLine(time2);
        //     Console.WriteLine(time1.TotalMinutes);
        //     Console.WriteLine(time2.TotalMinutes);
        //
        //     Console.WriteLine(cpu1.ResValueForProc(guid));
        // }
        //
        // private static void TestPlacing()
        // {
        //     var pcbParams = new PcbParams(150, 0.6, true);
        //
        //     var guid = PcbDesignProcedure.NewId;
        //     var cpu1 = new CpuThreads(1, 2.5);
        //     var cpu2 = new CpuThreads(8, 2.5);
        //
        //
        //     var placingAlgName1 = "Последовательное размещение";
        //     var algFactory1 = PlacingAlgProviderFactory.Create(placingAlgName1);
        //     var alg1 = algFactory1.Create(pcbParams);
        //     cpu1.TryGetResource(guid, alg1.MaxThreadUtilization);
        //     var cpu1Power = cpu1.ResValueForProc(guid);
        //
        //     var placingAlgName2 = "Метод разбиения (параллельный)";
        //     var algFactory2 = PlacingAlgProviderFactory.Create(placingAlgName2);
        //     var alg2 = algFactory2.Create(pcbParams);
        //     cpu2.TryGetResource(guid, alg2.MaxThreadUtilization);
        //     var cpu2Power = cpu2.ResValueForProc(guid);
        //
        //
        //     var time1 = alg1.EstimateEndTime(cpu1Power);
        //     var time2 = alg2.EstimateEndTime(cpu2Power);
        //
        //
        //     Console.WriteLine(time1);
        //     Console.WriteLine(time1.TotalMinutes);
        //     Console.WriteLine(time2);
        //     Console.WriteLine(time2.TotalMinutes);
        //
        //     Console.WriteLine(cpu1.ResValueForProc(guid));
        // }
        //
        // private static double TestServerPower()
        // {
        //     var server = new Server(10);
        //     var serverPower = 1.8 / Math.Exp(40.0 / server.InternetSpeed);
        //     return serverPower;
        // }
        //
        // private static void TestDesignerPower()
        // {
        //     var designer = new Designer(Designer.ExperienceEnum.Little);
        //
        //     var maxDesigner = Enum.GetValues<Designer.ExperienceEnum>().Max();
        //
        //     var a = 0.5 + (double)designer.Experience / (double)maxDesigner;
        //     Console.WriteLine(a);
        // }
        //
        // private static void TestDistr()
        // {
        //     IContinuousDistribution a = new Normal(0, 0);
        //
        //     var z = 0;
        //     for (var i = 0; i < 100000; i++)
        //     {
        //         if (a.Sample() > new ContinuousUniform().Sample())
        //             z++;
        //     }
        //
        //     Console.WriteLine(z / 100000.0);
        // }
        //
        // private static void TestSimulation1()
        // {
        //     CpuThreads cpuThreads = new(16, 2.5);
        //     Server server = new(200);
        //     List<Designer> designers = new()
        //         { new Designer(Designer.ExperienceEnum.Average), new Designer(Designer.ExperienceEnum.Average) };
        //
        //     List<IResource> resourcePool = new() { cpuThreads, server };
        //     resourcePool.AddRange(designers);
        //     //
        //
        //     var pcbAlgFactories = new PcbAlgFactories(
        //         placingAlgFactory: PlacingAlgProviderFactory.Create(PlacingAlgProviderFactory.PlacingSequentialStr),
        //         wireRoutingAlgFactory:
        //         WireRoutingAlgProviderFactory.Create(WireRoutingAlgProviderFactory.WireRoutingWaveStr));
        //     //
        //
        //     var techIntervalDistrMean = new TimeSpan(0, 3, 0, 0).TotalSeconds;
        //     var techIntervalDistrDev = new TimeSpan(1, 0, 0).TotalSeconds;
        //     var techIntervalDistr = new Normal(techIntervalDistrMean, techIntervalDistrDev);
        //
        //
        //     var elementCountMean = 2000;
        //     var elementCountDev = 100;
        //     var elementCountDistr = new Normal(elementCountMean, elementCountDev);
        //
        //     var dimensionUsagePctMean = 0.8;
        //     var dimensionUsagePctDev = 0.1;
        //     var dimensionUsagePctDistr = new Normal(dimensionUsagePctMean, dimensionUsagePctDev);
        //
        //     var variousSizePctMean = 0.7;
        //     var variousSizePctDistr = new Beta(variousSizePctMean, 1.0 - variousSizePctMean, new Random(1));
        //
        //
        //     SimuEventGenerator simuEventGenerator = null;
        //
        //     //
        //
        //
        //     ISimpleLogger logger = new ConsoleSimpleLogger();
        //
        //     var finalTime = TimeSpan.FromDays(50);
        //
        //     var simulator =
        //         new PcbDesignCadSimulator(simuEventGenerator, resourcePool, pcbAlgFactories, logger);
        //     simulator.Simulate(finalTime);
        // }
    }
}