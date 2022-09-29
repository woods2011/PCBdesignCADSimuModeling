using System;
using System.Collections.Generic;
using System.Linq;
using PcbDesignSimuModeling.WPF.Models.Resources;
using PcbDesignSimuModeling.WPF.Models.Resources.CpuClusterRes;
using PcbDesignSimuModeling.WPF.Models.Resources.DesignerRes;
using PcbDesignSimuModeling.WPF.Models.Resources.EadSybSystems;
using PcbDesignSimuModeling.WPF.Models.Resources.EadSybSystems.PlacingSystems;
using PcbDesignSimuModeling.WPF.Models.Resources.EadSybSystems.RoutingSystems;
using PcbDesignSimuModeling.WPF.Models.Resources.RamRes;
using PcbDesignSimuModeling.WPF.Models.Resources.ServerRes;
using PcbDesignSimuModeling.WPF.Models.SimuSystem;
using PcbDesignSimuModeling.WPF.Models.SimuSystem.SimulationEvents;

namespace PcbDesignSimuModeling.WPF.Models.OptimizationModule;

public class SimuSystemFuncWrapper
{
    private readonly Random _rndSource;

    private readonly IList<IEnumerable<SimulationEvent>> _preCalcEventsList;

    private readonly TimeSpan _finalTime;
    private readonly TimeSpan _timeTol;
    private readonly double _goalProductionTimeHours;
    private readonly TimeSpan? _ranUpTime;

    public SimuSystemFuncWrapper(IEnumerable<IEnumerable<SimulationEvent>> preCalcEventsList, TimeSpan finalTime,
        Random? rndSource = null, double? goalProductionTimeHours = null, TimeSpan? ranUpTime = null, TimeSpan? timeTol = null)
    {
        _finalTime = finalTime;
        _goalProductionTimeHours = goalProductionTimeHours ?? TimeSpan.FromDays(5).TotalHours / 3.0;
        _rndSource = rndSource ?? new Random(1);
        _ranUpTime = ranUpTime;
        _timeTol = timeTol ?? finalTime * 0.2;
        _preCalcEventsList = preCalcEventsList.ToList();
    }

    public double ObjectiveFunction(int threadsCount, double clockRate, double ramAmount, double serverSpeed,
        int placingAlgIndex, int wireRoutingAlgIndex, int designersCount)
    {
        var sampleSize = _preCalcEventsList.Count;
        var input = _preCalcEventsList.Select(preCalcEventsSource =>
        {
            var preCalcEvents = preCalcEventsSource.ToList();
            var minFinishedTechs = Convert.ToInt32(preCalcEvents.Count * 0.5 * (1 - _ranUpTime / _finalTime));

            var resourcePool = new List<IResource>
                {new Ram(ramAmount), new Server(serverSpeed), new CpuCluster(threadsCount, clockRate)};
            resourcePool.AddRange(Enumerable.Range(0, designersCount).Select(_ => new Designer()));

            var resFailuresEvents =
                new ResourceFailureGenerator(resourcePool, _rndSource).GenerateSimuEvent(_finalTime);
            preCalcEvents.InsertRangeAfterCondition(resFailuresEvents,
                (itemSource, insItem) => insItem.ActivateTime > itemSource.ActivateTime);

            return (preCalcEvents, resourcePool, minFinishedTechs);
        }).ToList();

        var configsScores = input.AsParallel().Select(tuple =>
            {
                var pcbAlgFactories = new EadSubSystemFactories(
                    placingSysFactory: PlacingSysFactoryProvider.Create(placingAlgIndex),
                    routingSysFactory: RoutingSysFactoryProvider.Create(wireRoutingAlgIndex)
                );

                var simulator = new PcbDesignSimulator(tuple.preCalcEvents, tuple.resourcePool, pcbAlgFactories,
                    _ranUpTime, timeTol: _timeTol);
                var simulationResult = simulator.Simulate(_finalTime);

                if (simulationResult.Values.Count <= tuple.minFinishedTechs) return -1;

                var productionTimes = simulationResult.Values.Select(time => time).ToList();
                var avgProductionTimeHours = productionTimes.Average(time => time.TotalHours);

                var totalConfigCost =
                    Convert.ToDouble(new CadConfigCostEstimator().EstimateFullCost(tuple.resourcePool));
                var costToTime = new CadConfigScoreGetter2(_goalProductionTimeHours)
                    .GetScore(totalConfigCost, avgProductionTimeHours);

                return costToTime;
            }
        ).TakeWhile(score => score > 0).ToList();

        return configsScores.Count == sampleSize ? configsScores.Average() : Double.MinValue;
    }
}

//return (x1, x2, x3, x4, x5, x6) => 0;