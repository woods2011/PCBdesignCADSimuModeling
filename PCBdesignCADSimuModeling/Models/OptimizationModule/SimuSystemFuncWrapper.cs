using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using MathNet.Numerics.Random;
using PcbDesignCADSimuModeling.Models.Resources;
using PcbDesignCADSimuModeling.Models.Resources.Algorithms;
using PcbDesignCADSimuModeling.Models.Resources.Algorithms.PlacingAlgorithms;
using PcbDesignCADSimuModeling.Models.Resources.Algorithms.WireRoutingAlgorithms;
using PcbDesignCADSimuModeling.Models.SimuSystem;
using PcbDesignCADSimuModeling.Models.SimuSystem.SimulationEvents;
using PcbDesignCADSimuModeling.ViewModels;

namespace PcbDesignCADSimuModeling.Models.OptimizationModule
{
    public class SimuSystemFuncWrapper
    {
        private readonly Random _random = new(1);

        private readonly SimuEventGenerator _simuEventGenerator;

        private readonly List<SimulationEvent> _preCalcEvent;
        private readonly TimeSpan _finalTime = TimeSpan.FromDays(30);
        private readonly TimeSpan? _timeTol = TimeSpan.FromDays(15);

        public SimuSystemFuncWrapper()
        {
            _simuEventGenerator = new SimuEventGenerator(
                timeBetweenTechsDistr: new TechIntervalBuilderVm(new TimeSpan(0, 12, 0, 0), new TimeSpan(6, 30, 0),
                    _random).Build(),
                pcbElemsCountDistr: new DblNormalDistributionBuilderVm(150, 15, _random).Build(),
                pcbDimUsagePctDistr: new DblNormalDistributionBuilderVm(0.6, 0.1, _random).Build(),
                pcbElemsIsVarSizeProb: 0.8,
                random: _random);

            _preCalcEvent = _simuEventGenerator.GeneratePcbDesignTech(_finalTime);
        }

        public double ObjectiveFunction(double x1, double x2, double x3, double x4, double x5, double x6)
        {
            var threadsCount = (int)Math.Round(x1);
            var clockRate = x2;
            var internetSpeed = (int)Math.Round(x3);
            var placingAlgIndex = (int)Math.Round(x4);
            var wireRoutingAlgIndex = (int)Math.Round(x5);
            var designersCount = (int)Math.Round(x6);

            var pcbAlgFactories = new PcbAlgFactories(
                placingAlgFactory: PlacingAlgProviderFactory.Create(
                    placingAlgIndex switch
                    {
                        0 => PlacingAlgProviderFactory.PlacingSequentialStr,
                        1 => PlacingAlgProviderFactory.PlacingPartitioningStr,
                        _ => throw new ArgumentOutOfRangeException($"{nameof(placingAlgIndex)} | {nameof(x5)}")
                    }),
                wireRoutingAlgFactory: WireRoutingAlgProviderFactory.Create(
                    wireRoutingAlgIndex switch
                    {
                        0 => WireRoutingAlgProviderFactory.WireRoutingWaveStr,
                        1 => WireRoutingAlgProviderFactory.WireRoutingChannelStr,
                        _ => throw new ArgumentOutOfRangeException($"{nameof(wireRoutingAlgIndex)} | {nameof(x6)}")
                    })
            );

            var resourcePool = new List<IResource>
                { new CpuThreads(threadsCount, clockRate), new Server(internetSpeed) };
            resourcePool.AddRange(Enumerable.Range(0, designersCount).Select(_ => new Designer()));


            var simulator =
                new PcbDesignCadSimulator(_simuEventGenerator, resourcePool, pcbAlgFactories, timeTol: _timeTol);
            var simulationResult = simulator.SimulateOptimized1(_finalTime, _preCalcEvent);

            if (simulationResult.Values.Count < 1) return Double.MaxValue;


            var productionTimesSec = simulationResult.Values;

            var avgProductionTimeMilSec = productionTimesSec.Average(time => time.TotalMilliseconds);
            var avgProductionTime = TimeSpan.FromMilliseconds(avgProductionTimeMilSec);

            var totalConfigCost = resourcePool.Sum(resource => resource.Cost);

            var costToTime = (0.6 * (100000.0 / avgProductionTime.TotalDays)) / (0.4 * totalConfigCost);

            return -1.0 * costToTime;
        }
    }
}


//return (x1, x2, x3, x4, x5, x6) => 0;