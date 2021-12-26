using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using PcbDesignCADSimuModeling.Models;
using PcbDesignCADSimuModeling.Models.Loggers;
using PcbDesignCADSimuModeling.Models.Resources;
using PcbDesignCADSimuModeling.Models.Resources.Algorithms;
using PcbDesignCADSimuModeling.Models.Resources.Algorithms.PlacingAlgorithms;
using PcbDesignCADSimuModeling.Models.Resources.Algorithms.WireRoutingAlgorithms;
using PcbDesignCADSimuModeling.Models.SimuSystem;
using PcbDesignCADSimuModeling.Models.SimuSystem.SimulationEvents;
using PcbDesignCADSimuModeling.Models.Technologies.PcbDesign;
using PcbDesignCADSimuModeling.ViewModels;

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

        List<IResource> resourcePool = new() { new CpuThreads(16, 2.5), new Server(150) };
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

        var simulator = new PcbDesignCadSimulator(simuEventGenerator, resourcePool, pcbAlgFactories);
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

        Assert.AreEqual(0.12020492411155898, costToTime, Tol);
        Assert.AreEqual(6710351134732, avgProductionTime.Ticks);
        Assert.AreEqual(1462291470957, devProductionTime.Ticks);
        Assert.AreEqual(160671.0, totalConfigCost, Tol);
    }

    [Test]
    public void Test2()
    {
        var rndSource = new Random(1);

        List<IResource> resourcePool = new() { new CpuThreads(16, 2.5), new Server(150) };
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

        var simulator = new PcbDesignCadSimulator(simuEventGenerator, resourcePool, pcbAlgFactories);
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

        Assert.AreEqual(0.14439743817354914, costToTime, Tol);
        Assert.AreEqual(8915406108239, avgProductionTime.Ticks);
        Assert.AreEqual(1254596905072, devProductionTime.Ticks);
        Assert.AreEqual(100671.0, totalConfigCost, Tol);
    }

    [Test]
    public void Test3()
    {
        var rndSource = new Random(1);
        
        List<IResource> resourcePool = new() { new CpuThreads(16, 2.5), new Server(150) };
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
        
        var simulator = new PcbDesignCadSimulator(simuEventGenerator, resourcePool, pcbAlgFactories);
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
        
        Assert.AreEqual(0.02512688870583971, costToTime, Tol);
        Assert.AreEqual(51234429275254, avgProductionTime.Ticks);
        Assert.AreEqual(27233277143173, devProductionTime.Ticks);
        Assert.AreEqual(100671.0, totalConfigCost, Tol);
    }

    [Test]
    public void Test4()
    {
        var rndSource = new Random(1);

        List<IResource> resourcePool = new() { new CpuThreads(2, 2.5), new Server(150) };
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

        var simulator = new PcbDesignCadSimulator(simuEventGenerator, resourcePool, pcbAlgFactories);
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

        Assert.AreEqual(0.0005923184795413121, costToTime, Tol);
        Assert.AreEqual(3205596723380230, avgProductionTime.Ticks);
        Assert.AreEqual(473566999976137, devProductionTime.Ticks);
        Assert.AreEqual(68256.0, totalConfigCost, Tol);
    }
    
    [Test]
    public void Test5Profile()
    {
        var rndSource = new Random(1);
        
        List<IResource> resourcePool = new() { new CpuThreads(1, 2.5), new Server(150) };
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
        
        var simulator = new PcbDesignCadSimulator(simuEventGenerator, resourcePool, pcbAlgFactories, timeTol: TimeSpan.FromDays(15));
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
        
        // Assert.AreEqual(0.02512688870583971, costToTime, Tol);
        // Assert.AreEqual(51234429275254, avgProductionTime.Ticks);
        // Assert.AreEqual(27233277143173, devProductionTime.Ticks);
        // Assert.AreEqual(100671.0, totalConfigCost, Tol);
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