using System.Collections.Concurrent;
using PcbDesignSimuModeling.Core.Models.Resources;
using PcbDesignSimuModeling.Core.Models.Resources.Algorithms;
using PcbDesignSimuModeling.Core.Models.Resources.Algorithms.PlacingAlgorithms;
using PcbDesignSimuModeling.Core.Models.Resources.Algorithms.WireRoutingAlgorithms;
using PcbDesignSimuModeling.Core.Models.Resources.Cpu;
using PcbDesignSimuModeling.Core.Models.Resources.Designer;
using PcbDesignSimuModeling.Core.Models.Resources.Server;
using PcbDesignSimuModeling.Core.Models.SimuSystem;
using PcbDesignSimuModeling.Core.Models.SimuSystem.SimulationEvents;

namespace PcbDesignSimuModeling.Core.Models.OptimizationModule;

public class SimuSystemFuncWrapper
{
    private readonly Random _rndSource;

    private readonly IList<IEnumerable<SimulationEvent>> _preCalcEventsList;

    private readonly TimeSpan _finalTime;
    private readonly int _minFinishedTechs;
    private readonly TimeSpan _timeTol;

    public SimuSystemFuncWrapper(IEnumerable<IEnumerable<SimulationEvent>> preCalcEventsList, TimeSpan finalTime,
        int minFinishedTechs, Random? rndSource = null, TimeSpan? timeTol = null)
    {
        _finalTime = finalTime;
        _minFinishedTechs = minFinishedTechs;
        _rndSource = rndSource ?? new Random(1);
        _timeTol = timeTol ?? finalTime * 0.2;

        _preCalcEventsList = preCalcEventsList.ToList();
    }

    public double ObjectiveFunction(int threadsCount, double clockRate, double serverSpeed, int placingAlgIndex,
        int wireRoutingAlgIndex, int designersCount)
    {
        // var results = new ConcurrentBag<double>();
        // var preCalcEvents = _preCalcEventsList[0].ToList();
        //
        // var pcbAlgFactories = new PcbAlgFactories(
        //     placingAlgFactory: PlacingAlgProviderFactory.Create(placingAlgIndex),
        //     wireRoutingAlgFactory: WireRoutingAlgProviderFactory.Create(wireRoutingAlgIndex)
        // );
        //
        // var resourcePool = new List<IResource>
        //     {new CpuCluster(threadsCount, clockRate), new Server(serverSpeed)};
        // resourcePool.AddRange(Enumerable.Range(0, designersCount).Select(_ => new Designer()));
        //
        // var resFailuresEvents = new ResourceFailureGenerator(resourcePool, _rndSource).GenerateSimuEvent(_finalTime);
        // preCalcEvents.InsertRangeAfterCondition(resFailuresEvents,
        //     (itemSource, insItem) => insItem.ActivateTime > itemSource.ActivateTime);
        //
        // var simulator = new PcbDesignSimulator(preCalcEvents, resourcePool, pcbAlgFactories, timeTol: _timeTol);
        // var simulationResult = simulator.Simulate(_finalTime);
        //
        // if (simulationResult.Values.Count <= _minFinishedTechs)
        // {
        //     results.Add(Double.MaxValue);
        //     return results.Average();
        // }
        //
        //
        // var productionTimesSec = simulationResult.Values;
        //
        // var avgProductionTimeMilSec = productionTimesSec.Average(time => time.TotalMilliseconds);
        // var avgProductionTime = TimeSpan.FromMilliseconds(avgProductionTimeMilSec);
        //
        // var totalConfigCost = resourcePool.Sum(resource => resource.Cost);
        //
        // var costToTime = (0.6 * (100000.0 / avgProductionTime.TotalDays)) / (0.4 * (double) totalConfigCost);
        //
        // results.Add(-1.0 * costToTime);
        //
        //
        // return results.Average();
        // var results = new ConcurrentBag<double>();
        // Parallel.ForEach(_preCalcEventsList, preCalcEventsSource =>
        //     {
        //         var preCalcEvents = preCalcEventsSource.ToList();
        //
        //         var pcbAlgFactories = new PcbAlgFactories(
        //             placingAlgFactory: PlacingAlgProviderFactory.Create(placingAlgIndex),
        //             wireRoutingAlgFactory: WireRoutingAlgProviderFactory.Create(wireRoutingAlgIndex)
        //         );
        //
        //         var resourcePool = new List<IResource>
        //             {new CpuCluster(threadsCount, clockRate), new Server(serverSpeed)};
        //         resourcePool.AddRange(Enumerable.Range(0, designersCount).Select(_ => new Designer()));
        //
        //         var resFailuresEvents =
        //             new ResourceFailureGenerator(resourcePool, _rndSource).GenerateSimuEvent(_finalTime);
        //         preCalcEvents.InsertRangeAfterCondition(resFailuresEvents,
        //             (itemSource, insItem) => insItem.ActivateTime > itemSource.ActivateTime);
        //
        //         var simulator = new PcbDesignSimulator(preCalcEvents, resourcePool, pcbAlgFactories, timeTol: _timeTol);
        //         var simulationResult = simulator.Simulate(_finalTime);
        //
        //         if (simulationResult.Values.Count <= _minFinishedTechs)
        //         {
        //             results.Add(Double.MaxValue);
        //             return;
        //         }
        //
        //
        //         var productionTimesSec = simulationResult.Values;
        //
        //         var avgProductionTimeMilSec = productionTimesSec.Average(time => time.TotalMilliseconds);
        //         var avgProductionTime = TimeSpan.FromMilliseconds(avgProductionTimeMilSec);
        //
        //         var totalConfigCost = resourcePool.Sum(resource => resource.Cost);
        //
        //         var costToTime = (0.6 * (100000.0 / avgProductionTime.TotalDays)) / (0.4 * (double) totalConfigCost);
        //
        //         results.Add(-1.0 * costToTime);
        //     }
        // );
        // return results.Average();
        var sampleSize = _preCalcEventsList.Count;
        var input = _preCalcEventsList.Select(preCalcEventsSource =>
        {
            var preCalcEvents = preCalcEventsSource.ToList();

            var resourcePool = new List<IResource>
                {new CpuCluster(threadsCount, clockRate), new Server(serverSpeed)};
            resourcePool.AddRange(Enumerable.Range(0, designersCount).Select(_ => new Designer()));

            var resFailuresEvents =
                new ResourceFailureGenerator(resourcePool, _rndSource).GenerateSimuEvent(_finalTime);
            preCalcEvents.InsertRangeAfterCondition(resFailuresEvents,
                (itemSource, insItem) => insItem.ActivateTime > itemSource.ActivateTime);

            return (preCalcEvents, resourcePool);
        }).ToList();

        return input.AsParallel().Average(pair =>
            {
                var pcbAlgFactories = new PcbAlgFactories(
                    placingAlgFactory: PlacingAlgProviderFactory.Create(placingAlgIndex),
                    wireRoutingAlgFactory: WireRoutingAlgProviderFactory.Create(wireRoutingAlgIndex)
                );

                var simulator = new PcbDesignSimulator(pair.preCalcEvents, pair.resourcePool, pcbAlgFactories,
                    timeTol: _timeTol);
                var simulationResult = simulator.Simulate(_finalTime);

                if (simulationResult.Values.Count <= _minFinishedTechs)
                    return Double.MaxValue / (sampleSize * sampleSize);


                var productionTimesSec = simulationResult.Values;

                var avgProductionTimeMilSec = productionTimesSec.Average(time => time.TotalMilliseconds);
                var avgProductionTime = TimeSpan.FromMilliseconds(avgProductionTimeMilSec);

                var totalConfigCost = pair.resourcePool.Sum(resource => resource.Cost);

                var costToTime = (0.6 * (100000.0 / avgProductionTime.TotalDays)) / (0.4 * (double) totalConfigCost);

                return -1.0 * costToTime;
            }
        );
    }
}

//return (x1, x2, x3, x4, x5, x6) => 0;