using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Jace;
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

namespace Benchmark;

public class BenchTest
{
    private Random _random = new(1);

    private Func<double[], double> _func;
    private double[] _doubles = new double[1000];

    private PcbAlgFactories _pcbAlgFactories;
    private SimuEventGenerator _simuEventGenerator;

    private List<SimulationEvent> _preCalcEvent;
    private TimeSpan _finalTime = TimeSpan.FromDays(30);
    private CpuCluster _cpuCluster = new(16, 2.5);
    private Server _server = new(150);
    private int _designersCount = 2;
    private readonly TimeSpan? _timeTol = TimeSpan.FromDays(15);
    private Func<int, double, double, int, int, int, double> _objectiveFunction;

    public BenchTest()
    {
        _func = FunctionParser.ParseFunction("4*(x1 - 5)^2 + (x2 - 6)^2", 2);

        _pcbAlgFactories = new PcbAlgFactories(
            PlacingAlgProviderFactory.Create(PlacingAlgProviderFactory.PlacingSequentialStr),
            WireRoutingAlgProviderFactory.Create(WireRoutingAlgProviderFactory.WireRoutingWaveStr));

        var techIntervalDistr = new TechIntervalBuilderVm(new TimeSpan(1, 20, 0, 0), new TimeSpan(6, 30, 0),
            _random);
        _simuEventGenerator = new SimuEventGenerator(
            timeBetweenTechsDistr: techIntervalDistr.Build(),
            pcbElemsCountDistr: new DblNormalDistributionBuilderVm(150, 15, _random).Build(),
            pcbDimUsagePctDistr: new DblNormalDistributionBuilderVm(0.6, 0.1, _random).Build(),
            pcbElemsIsVarSizeProb: 0.8,
            random: _random);
        _preCalcEvent = _simuEventGenerator.GeneratePcbDesignTech(_finalTime);
            
        var minFinishedTechs = (int)Math.Round(_finalTime / techIntervalDistr.Mean * 0.5);

        var simuSystemFuncWrapper = new SimuSystemFuncWrapper(_simuEventGenerator, _finalTime, minFinishedTechs, _preCalcEvent);
        _objectiveFunction = simuSystemFuncWrapper.ObjectiveFunction;
    }


    // [IterationSetup]
    // public void IterationSetup()
    // {
    //     _resourcePool = new List<IResource> { new CpuThreads(16, 2.5), new Server(150) };
    //     _resourcePool.AddRange(Enumerable.Range(0, 2).Select(_ => new Designer()));
    // }


    //[Benchmark]
    public void TestFuncEval()
    {
        _func(new double[] { _random.NextDouble(), _random.NextDouble() });
    }

    //[Benchmark]
    public void TestSimuSystem()
    {
        var resourcePool = new List<IResource> { _cpuCluster, _server };
        resourcePool.AddRange(Enumerable.Range(0, _designersCount).Select(_ => new Designer()));
        resourcePool = resourcePool.Select(resource => resource.Clone()).ToList();

        var simulator =
            new PcbDesignSimulator(_simuEventGenerator, resourcePool, _pcbAlgFactories, timeTol: _timeTol);
        simulator.Simulate(_finalTime, _preCalcEvent);
    }

    [Benchmark]
    public void TestSimuSystemOptimized1()
    {
        var resourcePool = new List<IResource> { _cpuCluster, _server };
        resourcePool.AddRange(Enumerable.Range(0, _designersCount).Select(_ => new Designer()));
        resourcePool = resourcePool.Select(resource => resource.Clone()).ToList();

        var simulator =
            new PcbDesignSimulator(_simuEventGenerator, resourcePool, _pcbAlgFactories, timeTol: _timeTol);
        simulator.SimulateForOptimization(_finalTime, _preCalcEvent);
    }

    [Benchmark]
    public void TestFuncWrapper() => _objectiveFunction(_cpuCluster.ThreadCount, _cpuCluster.ClockRate,
        _server.InternetSpeed, 0, 0, _designersCount);

    public static class FunctionParser
    {
        public static Func<double[], double> ParseFunction(string functionStr, int dim)
        {
            var calculationEngine = new CalculationEngine();
            var formula = calculationEngine.Build(functionStr);

            var variables = Enumerable.Range(0, dim).ToDictionary(d => $"x{d + 1}", _ => 0.0);
            var keys = variables.Keys.ToArray();

            return x =>
            {
                for (var i = 0; i < dim; i++) variables[keys[i]] = x[i];
                return formula(variables);
            };
        }
    }
}