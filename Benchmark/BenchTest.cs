using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Jace;
using MathNet.Numerics.Distributions;
using PcbDesignSimuModeling.Core.Models.OptimizationModule;
using PcbDesignSimuModeling.Core.Models.Resources;
using PcbDesignSimuModeling.Core.Models.Resources.Algorithms;
using PcbDesignSimuModeling.Core.Models.Resources.Algorithms.PlacingAlgorithms;
using PcbDesignSimuModeling.Core.Models.Resources.Algorithms.WireRoutingAlgorithms;
using PcbDesignSimuModeling.Core.Models.Resources.Cpu;
using PcbDesignSimuModeling.Core.Models.Resources.Designer;
using PcbDesignSimuModeling.Core.Models.Resources.Ram;
using PcbDesignSimuModeling.Core.Models.Resources.Server;
using PcbDesignSimuModeling.Core.Models.SimuSystem;
using PcbDesignSimuModeling.Core.Models.SimuSystem.SimulationEvents;
using PcbDesignSimuModeling.Core.ViewModels;

namespace Benchmark;

public class BenchTest
{
    private readonly Random _random = new(1);

    private readonly Func<double[], double> _func;
    private double[] _doubles = new double[1000];

    private readonly PcbAlgFactories _pcbAlgFactories;
    private readonly List<SimulationEvent> _preCalcEvents;
    private readonly TimeSpan _finalTime = TimeSpan.FromDays(30);
    private readonly Ram _ram = new(16);
    private readonly CpuCluster _cpuCluster = new(16, 2.5);
    private readonly Server _server = new(150);
    private readonly int _designersCount = 2;
    private readonly TimeSpan? _timeTol = TimeSpan.FromDays(15);
    private readonly Func<int, double, double, double, int, int, int, double> _objectiveFunction;

    public BenchTest()
    {
        _func = FunctionParser.ParseFunction("4*(x1 - 5)^2 + (x2 - 6)^2", 2);

        _pcbAlgFactories = new PcbAlgFactories(
            PlacingAlgProviderFactory.Create(PlacingAlgProviderFactory.PlacingSequentialStr),
            WireRoutingAlgProviderFactory.Create(WireRoutingAlgProviderFactory.WireRoutingWaveStr));

        var pcbGenerator = new PcbProbDistrBasedGenerator(
            pcbElemsCountDistr: new DblNormalDistributionBuilderVm(150, 15, _random).Build(),
            pcbAreaUsagePctDistr: new DblNormalDistributionBuilderVm(0.6, 0.1, _random).Build(),
            pcbElemsIsVarSize: new Bernoulli(0.8, _random)
        );

        var techPerYear = 200;
        var pcbDesignTechGenerator = new PcbDesignTechGenerator(
            requestsFlowDistrDays: new Exponential(techPerYear / 365.0, _random),
            pcbGenerator: pcbGenerator,
            rndSource: _random
        );
        _preCalcEvents = pcbDesignTechGenerator.GenerateSimuEvent(_finalTime);

        //var resFailureGenerator = new ResourceFailureGenerator(resourcePool, _random);
        //preCalcEvents.AddRange(resFailureGenerator.GenerateSimuEvent(_finalTime));

        var sampleSize = 1;
        var simuSystemFuncWrapper =
            new SimuSystemFuncWrapper(Enumerable.Repeat(_preCalcEvents, sampleSize), _finalTime);
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
        _func(new double[] {_random.NextDouble(), _random.NextDouble()});
    }

    //[Benchmark]
    public void TestSimuSystem()
    {
        // var resourcePool = new List<IResource> { _cpuCluster, _server };
        // resourcePool.AddRange(Enumerable.Range(0, _designersCount).Select(_ => new Designer()));
        // resourcePool = resourcePool.Select(resource => resource.Clone()).ToList();
        //
        // var simulator =
        //     new PcbDesignSimulator(_pcbDesignTechGenerator, resourcePool, _pcbAlgFactories, timeTol: _timeTol);
        // simulator.Simulate(_finalTime, _preCalcEvent);
    }

    //[Benchmark]
    public void TestSimuSystemOptimized1()
    {
        // var resourcePool = new List<IResource> { _cpuCluster, _server };
        // resourcePool.AddRange(Enumerable.Range(0, _designersCount).Select(_ => new Designer()));
        // resourcePool = resourcePool.Select(resource => resource.Clone()).ToList();
        //
        // var simulator =
        //     new PcbDesignSimulator(_pcbDesignTechGenerator, resourcePool, _pcbAlgFactories, timeTol: _timeTol);
        // simulator.SimulateForOptimization(_finalTime, _preCalcEvent);
    }

    [Benchmark]
    public void TestFuncWrapper() => _objectiveFunction(
        _cpuCluster.ThreadCount, _cpuCluster.ClockRate,
        _ram.AvailableAmount, _server.InternetSpeed, 0, 0, _designersCount);

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