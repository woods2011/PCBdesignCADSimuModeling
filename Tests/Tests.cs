using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using PcbDesignSimuModeling.Core.Models;
using PcbDesignSimuModeling.Core.Models.OptimizationModule;
using PcbDesignSimuModeling.Core.Models.Resources;
using PcbDesignSimuModeling.Core.Models.Resources.Algorithms;
using PcbDesignSimuModeling.Core.Models.Resources.Algorithms.PlacingAlgorithms;
using PcbDesignSimuModeling.Core.Models.Resources.Algorithms.WireRoutingAlgorithms;
using PcbDesignSimuModeling.Core.Models.Resources.Cpu;
using PcbDesignSimuModeling.Core.Models.Resources.Designer;
using PcbDesignSimuModeling.Core.Models.Resources.Server;
using PcbDesignSimuModeling.Core.Models.SimuSystem;
using PcbDesignSimuModeling.Core.Models.SimuSystem.SimulationEvents;
using PcbDesignSimuModeling.Core.ViewModels;

namespace Tests;

public class Tests
{
    private const double Tol = 1e-15;

    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        var rndSource = new Random(1);

        List<IResource> resourcePool = new() { new CpuCluster(16, 2.5), new Server(150) };
        resourcePool.AddRange(Enumerable.Range(0, 2).Select(_ => new Designer()));

        var pcbAlgFactories = new PcbAlgFactories(
            placingAlgFactory: PlacingAlgProviderFactory.Create(PlacingAlgProviderFactory.PlacingSequentialStr),
            wireRoutingAlgFactory: WireRoutingAlgProviderFactory.Create(
                WireRoutingAlgProviderFactory.WireRoutingWaveStr));

        var simuEventGenerator = new SimuEventGenerator(
            timeBetweenTechsDistr: new TechIntervalBuilderVm(new TimeSpan(1, 20, 0, 0), new TimeSpan(6, 30, 0),
                rndSource).Build(),
            pcbElemsCountDistr: new DblNormalDistributionBuilderVm(150, 15, rndSource).Build(),
            pcbDimUsagePctDistr: new DblNormalDistributionBuilderVm(0.6, 0.1, rndSource).Build(),
            pcbElemsIsVarSizeProb: 0.8,
            random: rndSource);

        var simulator = new PcbDesignSimulator(simuEventGenerator, resourcePool, pcbAlgFactories);
        var simulationResult = simulator.Simulate(TimeSpan.FromDays(30));


        if (simulationResult.Values.Count < 1) Assert.Fail();

        var productionTimesSec = simulationResult.Values;

        var avgProductionTimeMilSec = productionTimesSec.Average(time => time.TotalMilliseconds);
        var avgProductionTime = TimeSpan.FromMilliseconds(avgProductionTimeMilSec);
        var devProductionTimeMilSec = Math.Sqrt(
            productionTimesSec.Sum(time => Math.Pow(time.TotalMilliseconds - avgProductionTimeMilSec, 2)) /
            productionTimesSec.Count);
        var devProductionTime = TimeSpan.FromMilliseconds(devProductionTimeMilSec);

        var totalConfigCost = resourcePool.Sum(resource => resource.Cost);

        var costToTime = (0.6 * (100000.0 / avgProductionTime.TotalDays)) / (0.4 * totalConfigCost);

        Console.WriteLine(costToTime);
        Console.WriteLine($"{avgProductionTime} | {avgProductionTime.Ticks}");
        Console.WriteLine($"{devProductionTime} | {devProductionTime.Ticks}");
        Console.WriteLine(totalConfigCost);

        Assert.AreEqual(0.07388813617651108, costToTime, Tol);
        Assert.AreEqual(10968138862302, avgProductionTime.Ticks);
        Assert.AreEqual(2263543331178, devProductionTime.Ticks);
        Assert.AreEqual(159918.0, totalConfigCost, Tol);
    }

    [Test]
    public void Test2()
    {
        var rndSource = new Random(1);

        List<IResource> resourcePool = new() { new CpuCluster(16, 2.5), new Server(150) };
        resourcePool.AddRange(Enumerable.Range(0, 1).Select(_ => new Designer()));

        var pcbAlgFactories = new PcbAlgFactories(
            placingAlgFactory: PlacingAlgProviderFactory.Create(PlacingAlgProviderFactory.PlacingSequentialStr),
            wireRoutingAlgFactory: WireRoutingAlgProviderFactory.Create(
                WireRoutingAlgProviderFactory.WireRoutingWaveStr));

        var simuEventGenerator = new SimuEventGenerator(
            timeBetweenTechsDistr: new TechIntervalBuilderVm(new TimeSpan(1, 20, 0, 0), new TimeSpan(6, 30, 0),
                rndSource).Build(),
            pcbElemsCountDistr: new DblNormalDistributionBuilderVm(150, 15, rndSource).Build(),
            pcbDimUsagePctDistr: new DblNormalDistributionBuilderVm(0.6, 0.1, rndSource).Build(),
            pcbElemsIsVarSizeProb: 0.8,
            random: rndSource);

        var simulator = new PcbDesignSimulator(simuEventGenerator, resourcePool, pcbAlgFactories);
        var simulationResult = simulator.Simulate(TimeSpan.FromDays(30));


        if (simulationResult.Values.Count < 1) Assert.Fail();

        var productionTimesSec = simulationResult.Values;

        var avgProductionTimeMilSec = productionTimesSec.Average(time => time.TotalMilliseconds);
        var avgProductionTime = TimeSpan.FromMilliseconds(avgProductionTimeMilSec);
        var devProductionTimeMilSec = Math.Sqrt(
            productionTimesSec.Sum(time => Math.Pow(time.TotalMilliseconds - avgProductionTimeMilSec, 2)) /
            productionTimesSec.Count);
        var devProductionTime = TimeSpan.FromMilliseconds(devProductionTimeMilSec);

        var totalConfigCost = resourcePool.Sum(resource => resource.Cost);

        var costToTime = (0.6 * (100000.0 / avgProductionTime.TotalDays)) / (0.4 * totalConfigCost);

        Console.WriteLine(costToTime);
        Console.WriteLine($"{avgProductionTime} | {avgProductionTime.Ticks}");
        Console.WriteLine($"{devProductionTime} | {devProductionTime.Ticks}");
        Console.WriteLine(totalConfigCost);

        Assert.AreEqual(0.09572926510257336, costToTime, Tol);
        Assert.AreEqual(13549290185773, avgProductionTime.Ticks);
        Assert.AreEqual(2650890720744, devProductionTime.Ticks);
        Assert.AreEqual(99918.0, totalConfigCost, Tol);
    }

    [Test]
    public void Test3()
    {
        var rndSource = new Random(1);

        List<IResource> resourcePool = new() { new CpuCluster(16, 2.5), new Server(150) };
        resourcePool.AddRange(Enumerable.Range(0, 1).Select(_ => new Designer()));

        var pcbAlgFactories = new PcbAlgFactories(
            placingAlgFactory: PlacingAlgProviderFactory.Create(PlacingAlgProviderFactory.PlacingSequentialStr),
            wireRoutingAlgFactory: WireRoutingAlgProviderFactory.Create(
                WireRoutingAlgProviderFactory.WireRoutingWaveStr));

        var simuEventGenerator = new SimuEventGenerator(
            timeBetweenTechsDistr: new TechIntervalBuilderVm(new TimeSpan(0, 10, 0, 0), new TimeSpan(1, 30, 0),
                rndSource).Build(),
            pcbElemsCountDistr: new DblNormalDistributionBuilderVm(150, 15, rndSource).Build(),
            pcbDimUsagePctDistr: new DblNormalDistributionBuilderVm(0.6, 0.1, rndSource).Build(),
            pcbElemsIsVarSizeProb: 0.8,
            random: rndSource);

        var simulator = new PcbDesignSimulator(simuEventGenerator, resourcePool, pcbAlgFactories);
        var simulationResult = simulator.Simulate(TimeSpan.FromDays(30));


        if (simulationResult.Values.Count < 1) Assert.Fail();

        var productionTimesSec = simulationResult.Values;

        var avgProductionTimeMilSec = productionTimesSec.Average(time => time.TotalMilliseconds);
        var avgProductionTime = TimeSpan.FromMilliseconds(avgProductionTimeMilSec);
        var devProductionTimeMilSec = Math.Sqrt(
            productionTimesSec.Sum(time => Math.Pow(time.TotalMilliseconds - avgProductionTimeMilSec, 2)) /
            productionTimesSec.Count);
        var devProductionTime = TimeSpan.FromMilliseconds(devProductionTimeMilSec);

        var totalConfigCost = resourcePool.Sum(resource => resource.Cost);

        var costToTime = (0.6 * (100000.0 / avgProductionTime.TotalDays)) / (0.4 * totalConfigCost);

        Console.WriteLine(costToTime);
        Console.WriteLine($"{avgProductionTime} | {avgProductionTime.Ticks}");
        Console.WriteLine($"{devProductionTime} | {devProductionTime.Ticks}");
        Console.WriteLine(totalConfigCost);

        Assert.AreEqual(0.02089286768013731, costToTime, Tol);
        Assert.AreEqual(62081644894476, avgProductionTime.Ticks);
        Assert.AreEqual(31901818217638, devProductionTime.Ticks);
        Assert.AreEqual(99918.0, totalConfigCost, Tol);
    }

    [Test]
    public void Test4()
    {
        var rndSource = new Random(1);

        List<IResource> resourcePool = new() { new CpuCluster(2, 2.5), new Server(150) };
        resourcePool.AddRange(Enumerable.Range(0, 1).Select(_ => new Designer()));

        var pcbAlgFactories = new PcbAlgFactories(
            placingAlgFactory: PlacingAlgProviderFactory.Create(PlacingAlgProviderFactory.PlacingSequentialStr),
            wireRoutingAlgFactory: WireRoutingAlgProviderFactory.Create(
                WireRoutingAlgProviderFactory.WireRoutingWaveStr));

        var simuEventGenerator = new SimuEventGenerator(
            timeBetweenTechsDistr: new TechIntervalBuilderVm(new TimeSpan(0, 10, 0, 0), new TimeSpan(1, 30, 0),
                rndSource).Build(),
            pcbElemsCountDistr: new DblNormalDistributionBuilderVm(150, 15, rndSource).Build(),
            pcbDimUsagePctDistr: new DblNormalDistributionBuilderVm(0.6, 0.1, rndSource).Build(),
            pcbElemsIsVarSizeProb: 0.8,
            random: rndSource);

        var simulator = new PcbDesignSimulator(simuEventGenerator, resourcePool, pcbAlgFactories);
        var simulationResult = simulator.Simulate(TimeSpan.FromDays(30));


        if (simulationResult.Values.Count < 1) Assert.Fail();

        var productionTimesSec = simulationResult.Values;

        var avgProductionTimeMilSec = productionTimesSec.Average(time => time.TotalMilliseconds);
        var avgProductionTime = TimeSpan.FromMilliseconds(avgProductionTimeMilSec);
        var devProductionTimeMilSec = Math.Sqrt(
            productionTimesSec.Sum(time => Math.Pow(time.TotalMilliseconds - avgProductionTimeMilSec, 2)) /
            productionTimesSec.Count);
        var devProductionTime = TimeSpan.FromMilliseconds(devProductionTimeMilSec);

        var totalConfigCost = resourcePool.Sum(resource => resource.Cost);

        var costToTime = (0.6 * (100000.0 / avgProductionTime.TotalDays)) / (0.4 * totalConfigCost);

        Console.WriteLine(costToTime);
        Console.WriteLine($"{avgProductionTime} | {avgProductionTime.Ticks}");
        Console.WriteLine($"{devProductionTime} | {devProductionTime.Ticks}");
        Console.WriteLine(totalConfigCost);

        Assert.AreEqual(0.000260382923574954, costToTime, Tol);
        Assert.AreEqual( 7373427735196400, avgProductionTime.Ticks);
        Assert.AreEqual(732277861977545, devProductionTime.Ticks);
        Assert.AreEqual(67503.0, totalConfigCost, Tol);
    }

    [Test]
    public void Test5Profile()
    {
        var rndSource = new Random(1);

        List<IResource> resourcePool = new() { new CpuCluster(1, 2.5), new Server(150) };
        resourcePool.AddRange(Enumerable.Range(0, 1).Select(_ => new Designer()));

        var pcbAlgFactories = new PcbAlgFactories(
            placingAlgFactory: PlacingAlgProviderFactory.Create(PlacingAlgProviderFactory.PlacingSequentialStr),
            wireRoutingAlgFactory: WireRoutingAlgProviderFactory.Create(
                WireRoutingAlgProviderFactory.WireRoutingWaveStr));

        var simuEventGenerator = new SimuEventGenerator(
            timeBetweenTechsDistr: new TechIntervalBuilderVm(new TimeSpan(0, 8, 0, 0), new TimeSpan(1, 30, 0),
                rndSource).Build(),
            pcbElemsCountDistr: new DblNormalDistributionBuilderVm(150, 15, rndSource).Build(),
            pcbDimUsagePctDistr: new DblNormalDistributionBuilderVm(0.6, 0.1, rndSource).Build(),
            pcbElemsIsVarSizeProb: 1.0,
            random: rndSource);

        var simulator = new PcbDesignSimulator(simuEventGenerator, resourcePool, pcbAlgFactories,
            timeTol: TimeSpan.FromDays(15));
        var simulationResult = simulator.Simulate(TimeSpan.FromDays(30));


        if (simulationResult.Values.Count < 1) return;

        var productionTimesSec = simulationResult.Values;

        var avgProductionTimeMilSec = productionTimesSec.Average(time => time.TotalMilliseconds);
        var avgProductionTime = TimeSpan.FromMilliseconds(avgProductionTimeMilSec);
        var devProductionTimeMilSec = Math.Sqrt(
            productionTimesSec.Sum(time => Math.Pow(time.TotalMilliseconds - avgProductionTimeMilSec, 2)) /
            productionTimesSec.Count);
        var devProductionTime = TimeSpan.FromMilliseconds(devProductionTimeMilSec);

        var totalConfigCost = resourcePool.Sum(resource => resource.Cost);

        var costToTime = (0.6 * (100000.0 / avgProductionTime.TotalDays)) / (0.4 * totalConfigCost);

        Console.WriteLine(costToTime);
        Console.WriteLine($"{avgProductionTime} | {avgProductionTime.Ticks}");
        Console.WriteLine($"{devProductionTime} | {devProductionTime.Ticks}");
        Console.WriteLine(totalConfigCost);

        Assert.AreEqual(0.02512688870583971, costToTime, Tol);
        Assert.AreEqual(51234429275254, avgProductionTime.Ticks);
        Assert.AreEqual(27233277143173, devProductionTime.Ticks);
        Assert.AreEqual(100671.0, totalConfigCost, Tol);
    }

    [Test]
    public void TestFuncWrapper()
    {
        var random = new Random(1);

        var techIntervalDistr =
            new TechIntervalBuilderVm(new TimeSpan(1, 20, 0, 0), new TimeSpan(6, 30, 0), random);

        var simuEventGenerator = new SimuEventGenerator(
            timeBetweenTechsDistr: techIntervalDistr.Build(),
            pcbElemsCountDistr: new DblNormalDistributionBuilderVm(150, 15, random).Build(),
            pcbDimUsagePctDistr: new DblNormalDistributionBuilderVm(0.6, 0.1, random).Build(),
            pcbElemsIsVarSizeProb: 0.8,
            random: random);

        var finalTime = TimeSpan.FromDays(30);
        var minFinishedTechs = (int)Math.Round(finalTime / techIntervalDistr.Mean * 0.5);

        var simuSystemFuncWrapper = new SimuSystemFuncWrapper(simuEventGenerator, finalTime, minFinishedTechs);
        var objectiveFunction = simuSystemFuncWrapper.ObjectiveFunction;

        var result = -1.0 * objectiveFunction(16, 2.5, 150, 0, 0, 1);
        Console.WriteLine(result);

        Assert.AreEqual(0.10498959186073112, result, Tol);
    }


    [Test]
    public void TestAbcOptimization()
    {
        var random = new Random(1);

        var techIntervalDistr = new TechIntervalBuilderVm(new TimeSpan(1, 20, 0, 0), new TimeSpan(6, 30, 0), random);
        
        var simuEventGenerator = new SimuEventGenerator(
            timeBetweenTechsDistr:
            techIntervalDistr.Build(),
            pcbElemsCountDistr: new DblNormalDistributionBuilderVm(150, 15, random).Build(),
            pcbDimUsagePctDistr: new DblNormalDistributionBuilderVm(0.6, 0.1, random).Build(),
            pcbElemsIsVarSizeProb: 0.8,
            random: random);

        var finalTime = TimeSpan.FromDays(30);
        var minFinishedTechs = (int)Math.Round(finalTime / techIntervalDistr.Mean * 0.5);
        
        var simuSystemFuncWrapper = new SimuSystemFuncWrapper(simuEventGenerator, finalTime, minFinishedTechs);
        var objectiveFunction = simuSystemFuncWrapper.ObjectiveFunction;

        var algorithmParameters = new AlgorithmSettings()
        {
            FoodSourceCount = 100,
            NumOfIterations = 400
        };

        var abcAlgorithm = new AbcAlgorithm(algorithmParameters, random, objectiveFunction);
        var resultList = abcAlgorithm.FindMinimum().ToList();
        var result = abcAlgorithm.BestFoodSource;

        if (resultList.Count == 0) Assert.Fail($"{nameof(resultList)} has no elements");

        result.FuncValue *= -1.0;
        Console.WriteLine(result);

        Assert.AreEqual(0.15519400985236761, result.FuncValue, Tol);
    }

    [Test]
    public void TestTimeSpanMultiplyAndClamp()
    {
        var time = TimeSpan.FromDays(5);
        Console.WriteLine(time.MultiplyAndClamp(Int64.MaxValue));
    }

    [Test]
    public void TestTimeSpanAddAndClamp()
    {
        var time = TimeSpan.FromDays(2);
        Console.WriteLine(time.AddAndClamp(TimeSpan.MaxValue));
    }
}