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
    private readonly TimeSpan _timeTol;

    private readonly IPcbAlgFactories _pcbAlgFactories;
    private readonly IResourceManager _resourceManager;

    private readonly List<SimulationEvent> _simulationEvents;
    private readonly List<PcbDesignTechnology> _activePcbDesignTechs = new();
    private readonly Dictionary<PcbDesignTechnology, TimeSpan> _techStart = new();
    private readonly Dictionary<PcbDesignTechnology, TimeSpan> _techDuration = new();

    public PcbDesignSimulator(IEnumerable<SimulationEvent> preCalcEvents, List<IResource> recoursePool,
        IPcbAlgFactories pcbAlgFactories, ISimpleLogger? logger = null, TimeSpan? timeTol = null)
    {
        _simulationEvents = preCalcEvents.ToList();
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


    public Dictionary<PcbDesignTechnology, TimeSpan> Simulate(TimeSpan finalTime)
    {
        while (_simulationEvents.Count > 0)
        {
            var closestEvent = _simulationEvents.First();
            ModelTime = closestEvent.ActivateTime;
            if (ModelTime > finalTime.AddAndClamp(_timeTol)) return _techDuration;

            //Determine Current state
            foreach (var activeTech in _activePcbDesignTechs.Where(technology => !technology.IsWaitForResources))
                activeTech.UpdateModelTime(_deltaTime);

            //Handle current Events
            HandleEvent(closestEvent);
            _simulationEvents.Remove(closestEvent);

            //Try Get Resources for pending procedures
            foreach (var activeTech in _activePcbDesignTechs) activeTech.TryGetResources();

            //Estimate time until procedure finish and generate event if its closest
            var technologiesWithResources =
                _activePcbDesignTechs.Where(technology => !technology.IsWaitForResources).ToList();

            if (technologiesWithResources.Count <= 0) continue;

            var (closestFinishTech, closestTechEndTime) =
                technologiesWithResources.MinByAndKey(technology => technology.EstimateRemainingTime());
            closestTechEndTime += _modelTime;

            if (_simulationEvents.Count == 0 || closestTechEndTime <= _simulationEvents.First().ActivateTime)
                _simulationEvents.Insert(0, new PcbDesignProcedureFinish(closestTechEndTime, closestFinishTech));
        }

        return _techDuration;
    }

    private void HandleEvent(SimulationEvent curEvent)
    {
        switch (curEvent)
        {
            case PcbDesignTechnologyStart technologyStart:
            {
                var newTechnology = new PcbDesignTechnology(_resourceManager, _pcbAlgFactories,
                    technologyStart.GeneratedPcb, NewId, _logger);
                _activePcbDesignTechs.Add(newTechnology);
                _techStart.Add(newTechnology, ModelTime);
                _logger?.Log($"{String.Concat(Enumerable.Repeat("---", (newTechnology.TechId - 1) % 15))}" +
                             $"Технология: {newTechnology.TechId} - СТАРТ ТЕХНОЛОГИИ");
                break;
            }

            case PcbDesignProcedureFinish procedureFinish:
            {
                var tech = procedureFinish.PcbDesignTechnology;
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
                resourceFailure.Resource.IsActive = false;
                _logger?.Log($"%%% Отказ Ресурса: {resourceFailure.Resource}");
                break;

            case ResourceRestored resourceRestored:
                resourceRestored.Resource.IsActive = true;
                _logger?.Log($"%%% Восcтановление Ресурса: {resourceRestored.Resource}");
                break;

            default: throw new ArgumentOutOfRangeException(nameof(curEvent));
        }
    }

    public Dictionary<PcbDesignTechnology, TimeSpan> SimulateOld(TimeSpan finalTime)
    {
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
            foreach (var activeTech in _activePcbDesignTechs) activeTech.TryGetResources();


            //Estimate time until procedure finish and generate event if its closest
            var technologiesWithResources = _activePcbDesignTechs
                .Where(technology => !technology.IsWaitForResources).ToList();

            if (technologiesWithResources.Count <= 0) continue;

            var (closestFinishTechs, closestTechEndTime) =
                technologiesWithResources.MinsByAndKey(technology => technology.EstimateRemainingTime());
            closestTechEndTime += _modelTime;

            if (_simulationEvents.Count == 0 ||
                closestTechEndTime <= _simulationEvents.Min(simuEv => simuEv.ActivateTime))
                _simulationEvents.AddRange(closestFinishTechs.Select(technology =>
                    new PcbDesignProcedureFinish(closestTechEndTime, technology)));
        }

        return _techDuration;
    }


    private int NewId => _curId++;
    private int _curId = 1;
}


// public Dictionary<PcbDesignTechnology, TimeSpan> SimulateForOptimization(TimeSpan finalTime)
// {
//     var (closestEvents, closestEventTime) = _simulationEvents.MinsByAndKey(simuEv => simuEv.ActivateTime);
//
//     while (_simulationEvents.Count > 0)
//     {
//         ModelTime = closestEventTime;
//         if (ModelTime > finalTime.AddAndClamp(_timeTol)) return _techDuration;
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
//         foreach (var activeTech in _activePcbDesignTechs) activeTech.TryGetResources();
//
//
//         //Correct activate time for events
//         if (_activePcbDesignTechs.Count <= 0)
//         {
//             if (_simulationEvents.Count != 0)
//                 (closestEvents, closestEventTime) = _simulationEvents.MinsByAndKey(ev => ev.ActivateTime);
//             continue;
//         }
//
//         var technologiesWithResources = _activePcbDesignTechs
//             .Where(technology => !technology.IsWaitForResources).ToList();
//
//         if (technologiesWithResources.Count <= 0)
//         {
//             if (_simulationEvents.Count != 0)
//                 (closestEvents, closestEventTime) = _simulationEvents.MinsByAndKey(simuEv => simuEv.ActivateTime);
//             continue;
//         }
//
//         var (closestFinishTechs, closestTechEndTime) =
//             technologiesWithResources.MinsByAndKey(technology => technology.EstimateRemainingTime());
//         closestTechEndTime += _modelTime;
//
//         if (_simulationEvents.Count != 0)
//         {
//             (closestEvents, closestEventTime) = _simulationEvents.MinsByAndKey(ev => ev.ActivateTime);
//             if (closestTechEndTime > closestEventTime) continue;
//
//             var procFinishEvents = closestFinishTechs.Select<PcbDesignTechnology, SimulationEvent>(
//                 technology => new PcbDesignProcedureFinish(closestTechEndTime, technology)).ToList();
//             _simulationEvents.AddRange(procFinishEvents);
//
//             if (closestTechEndTime == closestEventTime)
//             {
//                 closestEvents.AddRange(procFinishEvents);
//                 continue;
//             }
//
//             (closestEvents, closestEventTime) = (procFinishEvents, closestTechEndTime);
//         }
//         else
//         {
//             var procFinishEvents = closestFinishTechs.Select<PcbDesignTechnology, SimulationEvent>(
//                 technology => new PcbDesignProcedureFinish(closestTechEndTime, technology)).ToList();
//             _simulationEvents.AddRange(procFinishEvents);
//             (closestEvents, closestEventTime) = (procFinishEvents, closestTechEndTime);
//         }
//     }
//
//     return _techDuration;
// }