using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Jace;
using PcbDesignCADSimuModeling.Models.OptimizationModule;
using PcbDesignCADSimuModeling.Models.Resources;
using PcbDesignCADSimuModeling.Models.Resources.Algorithms;
using PcbDesignCADSimuModeling.Models.Resources.Algorithms.PlacingAlgorithms;
using PcbDesignCADSimuModeling.Models.Resources.Algorithms.WireRoutingAlgorithms;
using PcbDesignCADSimuModeling.Models.SimuSystem;
using PcbDesignCADSimuModeling.Models.SimuSystem.SimulationEvents;
using PcbDesignCADSimuModeling.ViewModels;

namespace Benchmark
{
    public class BenchTest
    {
        private Random _random = new(1);

        private Func<double[], double> _func;
        private double[] _doubles = new double[1000];

        private PcbAlgFactories _pcbAlgFactories;
        private SimuEventGenerator _simuEventGenerator;

        private List<SimulationEvent> _preCalcEvent;
        private TimeSpan _finalTime = TimeSpan.FromDays(30);
        private CpuThreads _cpuThreads = new(16, 2.5);
        private Server _server = new(150);
        private int _designersCount = 2;
        private readonly TimeSpan? _timeTol = TimeSpan.FromDays(15);
        private Func<double, double, double, double, double, double, double> _objectiveFunction;

        public BenchTest()
        {
            _func = FunctionParser.ParseFunction("4*(x1 - 5)^2 + (x2 - 6)^2", 2);

            _pcbAlgFactories = new PcbAlgFactories(
                PlacingAlgProviderFactory.Create(PlacingAlgProviderFactory.PlacingSequentialStr),
                WireRoutingAlgProviderFactory.Create(WireRoutingAlgProviderFactory.WireRoutingWaveStr));

            _simuEventGenerator = new SimuEventGenerator(
                timeBetweenTechsDistr: new TechIntervalBuilderVm(new TimeSpan(1, 20, 0, 0), new TimeSpan(6, 30, 0),
                    _random).Build(),
                pcbElemsCountDistr: new DblNormalDistributionBuilderVm(150, 15, _random).Build(),
                pcbDimUsagePctDistr: new DblNormalDistributionBuilderVm(0.6, 0.1, _random).Build(),
                pcbElemsIsVarSizeProb: 0.8,
                random: _random);
            _preCalcEvent = _simuEventGenerator.GeneratePcbDesignTech(_finalTime);

            var simuSystemFuncWrapper = new SimuSystemFuncWrapper(_simuEventGenerator, _finalTime, _preCalcEvent);
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
            var resourcePool = new List<IResource> { _cpuThreads, _server };
            resourcePool.AddRange(Enumerable.Range(0, _designersCount).Select(_ => new Designer()));
            resourcePool = resourcePool.Select(resource => resource.Clone()).ToList();

            var simulator =
                new PcbDesignCadSimulator(_simuEventGenerator, resourcePool, _pcbAlgFactories, timeTol: _timeTol);
            simulator.Simulate(_finalTime, _preCalcEvent);
        }

        [Benchmark]
        public void TestSimuSystemOptimized1()
        {
            var resourcePool = new List<IResource> { _cpuThreads, _server };
            resourcePool.AddRange(Enumerable.Range(0, _designersCount).Select(_ => new Designer()));
            resourcePool = resourcePool.Select(resource => resource.Clone()).ToList();

            var simulator =
                new PcbDesignCadSimulator(_simuEventGenerator, resourcePool, _pcbAlgFactories, timeTol: _timeTol);
            simulator.SimulateOptimized1(_finalTime, _preCalcEvent);
        }

        [Benchmark]
        public void TestFuncWrapper() => _objectiveFunction(_cpuThreads.ThreadCount, _cpuThreads.ClockRate,
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
}