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
    private readonly Random _random = new(1);

    private readonly ISimuEventGenerator _simuEventGenerator;
    private readonly List<SimulationEvent> _preCalcEvent;

    private readonly TimeSpan _finalTime;
    private readonly int _minFinishedTechs;
    private readonly TimeSpan _timeTol;

    public SimuSystemFuncWrapper(ISimuEventGenerator simuEventGenerator, TimeSpan finalTime, int minFinishedTechs,
        List<SimulationEvent>? preCalcEvent = null, TimeSpan? timeTol = null)
    {
        _simuEventGenerator = simuEventGenerator;
        _finalTime = finalTime;
        _minFinishedTechs = minFinishedTechs;
        _timeTol = timeTol ?? finalTime * 0.2;

        _preCalcEvent = preCalcEvent ?? _simuEventGenerator.GeneratePcbDesignTech(_finalTime);
    }

    public double ObjectiveFunction(int threadsCount, double clockRate, double serverSpeed, int placingAlgIndex,
        int wireRoutingAlgIndex, int designersCount)
    {
        var pcbAlgFactories = new PcbAlgFactories(
            placingAlgFactory: PlacingAlgProviderFactory.Create(
                placingAlgIndex switch
                {
                    0 => PlacingAlgProviderFactory.PlacingSequentialStr,
                    1 => PlacingAlgProviderFactory.PlacingPartitioningStr,
                    _ => throw new ArgumentOutOfRangeException($"{nameof(placingAlgIndex)} | {nameof(placingAlgIndex)}")
                }),
            wireRoutingAlgFactory: WireRoutingAlgProviderFactory.Create(
                wireRoutingAlgIndex switch
                {
                    0 => WireRoutingAlgProviderFactory.WireRoutingWaveStr,
                    1 => WireRoutingAlgProviderFactory.WireRoutingChannelStr,
                    _ => throw new ArgumentOutOfRangeException($"{nameof(wireRoutingAlgIndex)} | {nameof(wireRoutingAlgIndex)}")
                })
        );

        var resourcePool = new List<IResource>
            { new CpuCluster(threadsCount, clockRate), new Server(serverSpeed) };
        resourcePool.AddRange(Enumerable.Range(0, designersCount).Select(_ => new Designer()));


        var simulator =
            new PcbDesignSimulator(_simuEventGenerator, resourcePool, pcbAlgFactories, timeTol: _timeTol);
        var simulationResult = simulator.SimulateForOptimization(_finalTime, _preCalcEvent);

        if (simulationResult.Values.Count <= _minFinishedTechs) return Double.MaxValue;


        var productionTimesSec = simulationResult.Values;

        var avgProductionTimeMilSec = productionTimesSec.Average(time => time.TotalMilliseconds);
        var avgProductionTime = TimeSpan.FromMilliseconds(avgProductionTimeMilSec);

        var totalConfigCost = resourcePool.Sum(resource => resource.Cost);

        var costToTime = (0.6 * (100000.0 / avgProductionTime.TotalDays)) / (0.4 * totalConfigCost);

        return -1.0 * costToTime;
    }
}


//return (x1, x2, x3, x4, x5, x6) => 0;