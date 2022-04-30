using PcbDesignSimuModeling.Core.Models.Loggers;
using PcbDesignSimuModeling.Core.Models.Resources;
using PcbDesignSimuModeling.Core.Models.Resources.Algorithms;
using PcbDesignSimuModeling.Core.Models.SimuSystem.SimulationEvents;
using PcbDesignSimuModeling.Core.Models.Technologies.PcbDesign;

namespace PcbDesignSimuModeling.Core.Models.SimuSystem;

public class PcbDesignSimulator
{
    private readonly ISimpleLogger? _logger;
    private TimeSpan _modelTime = TimeSpan.Zero;
    private TimeSpan _deltaTime = TimeSpan.Zero;
    private readonly IPcbAlgFactories _pcbAlgFactories;
    private readonly IResourceManager _resourceManager;
    private readonly ISimuEventGenerator _simuEventGenerator;
    private readonly List<PcbDesignTechnology> _activePcbDesignTechs = new();
    private readonly List<SimulationEvent> _simulationEvents = new();
    private readonly Dictionary<PcbDesignTechnology, TimeSpan> _techStart = new();
    private readonly Dictionary<PcbDesignTechnology, TimeSpan> _techDuration = new();


    public PcbDesignSimulator(ISimuEventGenerator simuEventGenerator, List<IResource> recoursePool,
        IPcbAlgFactories pcbAlgFactories, ISimpleLogger? logger = null, TimeSpan? timeTol = null)
    {
        _simuEventGenerator = simuEventGenerator;
        _resourceManager = new ResourceManager(recoursePool);
        _pcbAlgFactories = pcbAlgFactories;
        _logger = logger;
        _timeTol = timeTol ?? TimeSpan.MaxValue;
    }


    private TimeSpan ModelTime
    {
        get => _modelTime;
        set
        {
            _deltaTime = value - _modelTime;
            _modelTime = value;
            _logger?.Log($">ТЕКУЩЕЕ МОДЕЛЬНОЕ ВРЕМЯ: {ModelTime:d\\.hh\\:mm\\:ss}");
        }
    }


    public Dictionary<PcbDesignTechnology, TimeSpan> Simulate(TimeSpan finalTime,
        List<SimulationEvent>? initSimulationEvents = null)
    {
        //Generate initial events
        _simulationEvents.AddRange(initSimulationEvents ?? _simuEventGenerator.GeneratePcbDesignTech(finalTime));

        while (_simulationEvents.Count > 0)
        {
            var (closestEvents, closestEventTime) = _simulationEvents.MinsByAndKey(simuEv => simuEv.ActivateTime);
            ModelTime = closestEventTime;
            if (ModelTime > finalTime.AddAndClamp(_timeTol)) return _techDuration;

            //Determine Current state
            foreach (var activeTech in _activePcbDesignTechs.Where(technology => !technology.IsWaitForResources))
                activeTech.UpdateModelTime(_deltaTime);

            //Handle current Events
            foreach (var curEvent in closestEvents)
            {
                HandleEvent(curEvent);
                _simulationEvents.Remove(curEvent);
            }

            //Try Get Resources for pending procedures
            foreach (var activeTech in _activePcbDesignTechs
                         .Where(technology => technology.IsWaitForResources)
                         .OrderBy(technology => technology.TechId)) activeTech.TryGetResources();


            //Estimate time until procedure finish and generate event if its closest
            if (_activePcbDesignTechs.Count <= 0) continue;

            var (closestFinishTechs, closestTechEndTime) = _activePcbDesignTechs
                .Where(technology => !technology.IsWaitForResources)
                .MinsByAndKey(technology => technology.EstimateEndTime());
            closestTechEndTime += _modelTime;

            if (_simulationEvents.Count == 0 ||
                closestTechEndTime <= _simulationEvents.Min(simuEv => simuEv.ActivateTime))
                _simulationEvents.AddRange(closestFinishTechs.Select(technology =>
                    new PcbDesignProcedureFinish(closestTechEndTime, technology)));
        }

        return _techDuration;
    }

    public Dictionary<PcbDesignTechnology, TimeSpan> SimulateForOptimization(TimeSpan finalTime,
        List<SimulationEvent>? initSimulationEvents = null)
    {
        //Generate initial events
        _simulationEvents.AddRange(initSimulationEvents ?? _simuEventGenerator.GeneratePcbDesignTech(finalTime));
        var (closestEvents, closestEventTime) = _simulationEvents.MinsByAndKey(simuEv => simuEv.ActivateTime);

        while (_simulationEvents.Count > 0)
        {
            ModelTime = closestEventTime;
            if (ModelTime > finalTime.AddAndClamp(_timeTol)) return _techDuration;

            //Determine Current state
            foreach (var activeTech in _activePcbDesignTechs.Where(technology => !technology.IsWaitForResources))
                activeTech.UpdateModelTime(_deltaTime);

            //Handle current Events
            foreach (var curEvent in closestEvents)
            {
                HandleEvent(curEvent);
                _simulationEvents.Remove(curEvent);
            }

            //Get Resources for needEd
            foreach (var activeTech in _activePcbDesignTechs
                         .Where(technology => technology.IsWaitForResources)
                         .OrderBy(technology => technology.TechId)) activeTech.TryGetResources();


            //Correct activate time for events
            if (_activePcbDesignTechs.Count <= 0)
            {
                if (_simulationEvents.Count != 0)
                    (closestEvents, closestEventTime) = _simulationEvents.MinsByAndKey(ev => ev.ActivateTime);
                continue;
            }

            var (closestFinishTechs, closestTechEndTime) = _activePcbDesignTechs
                .Where(technology => !technology.IsWaitForResources)
                .MinsByAndKey(technology => technology.EstimateEndTime());
            closestTechEndTime += _modelTime;

            if (_simulationEvents.Count != 0)
            {
                (closestEvents, closestEventTime) = _simulationEvents.MinsByAndKey(ev => ev.ActivateTime);
                if (closestTechEndTime > closestEventTime) continue;

                var procFinishEvents = closestFinishTechs.Select<PcbDesignTechnology, SimulationEvent>(
                    technology => new PcbDesignProcedureFinish(closestTechEndTime, technology)).ToList();
                _simulationEvents.AddRange(procFinishEvents);

                if (closestTechEndTime == closestEventTime)
                {
                    closestEvents.AddRange(procFinishEvents);
                    continue;
                }

                (closestEvents, closestEventTime) = (procFinishEvents, closestTechEndTime);
            }
            else
            {
                var procFinishEvents = closestFinishTechs.Select<PcbDesignTechnology, SimulationEvent>(
                    technology => new PcbDesignProcedureFinish(closestTechEndTime, technology)).ToList();
                _simulationEvents.AddRange(procFinishEvents);
                (closestEvents, closestEventTime) = (procFinishEvents, closestTechEndTime);
            }
        }

        return _techDuration;
    }

    private void HandleEvent(SimulationEvent curEvent)
    {
        switch (curEvent)
        {
            case PcbDesignTechnologyStart pcbDesignTechnologyStart:
            {
                var newTechnology = new PcbDesignTechnology(_resourceManager, _pcbAlgFactories,
                    pcbDesignTechnologyStart.GeneratedPcb, NewId, _logger);
                _activePcbDesignTechs.Add(newTechnology);
                _techStart.Add(newTechnology, ModelTime);
                _logger?.Log($"{String.Concat(Enumerable.Repeat("---", (newTechnology.TechId - 1) % 15))}" +
                             $"Технология: {newTechnology.TechId} - СТАРТ ТЕХНОЛОГИИ");
                break;
            }

            case PcbDesignProcedureFinish pcbDesignProcedureFinish:
            {
                var tech = pcbDesignProcedureFinish.PcbDesignTechnology;
                _logger?.Log($"{String.Concat(Enumerable.Repeat("---", (tech.TechId - 1) % 15))}" +
                             $"Технология: {tech.TechId} - Финиш проектной процедуры: {tech.CurProcedure.Name}");

                if (!tech.MoveToNextProcedure())
                {
                    _activePcbDesignTechs.Remove(tech);
                    _techDuration[tech] = _modelTime - _techStart[tech];
                    _logger?.Log($"{String.Concat(Enumerable.Repeat("---", (tech.TechId - 1) % 15))}" +
                                 $"Технология: {tech.TechId} - ФИНИШ ТЕХНОЛОГИИ");
                }

                break;
            }
            
            case ResourceFailure resourceFailure:
                break;
            
            case ResourceRestored resourceRestored:
                break;

            default: throw new ArgumentOutOfRangeException(nameof(curEvent));
        }
    }


    private readonly TimeSpan _timeTol;
    private int _curId = 1;
    private int NewId => _curId++;
}


// var events = new List<SimulationEvent>();
// foreach (var activeTech in _activePcbDesignTechs
//              .OrderBy(technology => _techStartAndFinish[technology].Start))
// {
//     var endTime = _modelTime + activeTech.EstimateEndTime();
//     if (endTime > nextEventTime) continue;
//     events.Clear();
//     nextEventTime = endTime;
//     events.Add(new PcbDesignProcedureFinish(activeTech, endTime));
//     _techStartAndFinish[activeTech] = (_techStartAndFinish[activeTech].Start, endTime);
// }


// public Dictionary<PcbDesignTechnology, TimeSpan> SimulateOptimized2(TimeSpan finalTime, List<SimulationEvent>? initSimulationEvents = null)
// {
//     if (_modelTime > finalTime) throw new InvalidOperationException("Simulation already completed");
//
//     //Generate initial events
//     _simulationEvents.AddRange(initSimulationEvents ?? _simuEventGenerator.GeneratePcbDesignTech(finalTime));
//     var (closestEvents, closestEventTime) = _simulationEvents.MinsByAndKey(simuEvent => simuEvent.ActivateTime);
//
//     while (_simulationEvents.Count > 0)
//     {
//         ModelTime = closestEventTime;
//
//         //Determine Current state
//         foreach (var activeTech in _activePcbDesignTechs.Where(technology => !technology.IsWaitForResources))
//             activeTech.UpdateModelTime(_deltaTime);
//
//         //Handle current Events
//         foreach (var curEvent in closestEvents)
//         {
//             HandleEvent(curEvent);
//             _simulationEvents.Remove(curEvent);
//         }
//
//         //Get Resources for needEd
//         foreach (var activeTech in _activePcbDesignTechs
//                      .Where(technology => technology.IsWaitForResources)
//                      .OrderBy(technology => technology.TechId)) activeTech.TryGetResources();
//
//
//         //Correct activate time for events
//         if (_simulationEvents.Count >= 1)
//             (closestEvents, closestEventTime) =
//                 _simulationEvents.MinsByAndKey(simuEvent => simuEvent.ActivateTime);
//         else
//         {
//             closestEventTime = TimeSpan.MaxValue;
//             closestEvents.Clear();
//         }
//
//         if (_activePcbDesignTechs.Count <= 0) continue;
//
//         var (closestFinishTechs, closestTechEndTime) = _activePcbDesignTechs
//             .Where(technology => !technology.IsWaitForResources)
//             .MinsByAndKey(technology => technology.EstimateEndTime());
//         closestTechEndTime += _modelTime;
//
//         if (closestTechEndTime > closestEventTime) continue;
//
//         var closestFinishTechsEvents = closestFinishTechs.Select(technology =>
//             new PcbDesignProcedureFinish(technology, closestTechEndTime) as SimulationEvent).ToList();
//         _simulationEvents.AddRange(closestFinishTechsEvents);
//
//         if (closestTechEndTime == closestEventTime) closestEvents.AddRange(closestFinishTechsEvents);
//         else
//         {
//             closestEvents = closestFinishTechsEvents;
//             closestEventTime = closestTechEndTime;
//         }
//     }
//
//     return _techDuration;
// }