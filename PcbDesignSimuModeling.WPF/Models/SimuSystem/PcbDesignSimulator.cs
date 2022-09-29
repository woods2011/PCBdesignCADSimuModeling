using System;
using System.Collections.Generic;
using System.Linq;
using PcbDesignSimuModeling.WPF.Models.Resources;
using PcbDesignSimuModeling.WPF.Models.Resources.CpuClusterRes;
using PcbDesignSimuModeling.WPF.Models.Resources.EadSybSystems;
using PcbDesignSimuModeling.WPF.Models.Resources.RamRes;
using PcbDesignSimuModeling.WPF.Models.SimuSystem.SimulationEvents;
using PcbDesignSimuModeling.WPF.Models.Technologies.PcbDesign;
using static PcbDesignSimuModeling.WPF.Models.SimuSystem.GeneralSimulationSettings;

namespace PcbDesignSimuModeling.WPF.Models.SimuSystem;

public class PcbDesignSimulator
{
    private readonly StatCollector? _statCollector;

    private TimeSpan _modelTime = TimeSpan.Zero;
    private TimeSpan _deltaTime = TimeSpan.Zero;
    private readonly TimeSpan _timeTol;
    private readonly TimeSpan _ranUpTime;

    private readonly IEadSubSystemFactories _eadSubSystemFactories;
    private readonly IResourceManager _resourceManager;

    private readonly List<SimulationEvent> _simulationEvents;
    private readonly List<PcbDesignTechnology> _activePcbDesignTechs = new();
    private readonly Dictionary<PcbDesignTechnology, TimeSpan> _techStart = new();
    private readonly Dictionary<PcbDesignTechnology, TimeSpan> _techDuration = new();

    public PcbDesignSimulator(IEnumerable<SimulationEvent> preCalcEvents, List<IResource> recoursePool,
        IEadSubSystemFactories eadSubSystemFactories,
        TimeSpan? ranUpTime = null, StatCollector? statCollector = null, TimeSpan? timeTol = null)
    {
        _simulationEvents = preCalcEvents.ToList();
        _resourceManager = new ResourceManager(recoursePool);
        _eadSubSystemFactories = eadSubSystemFactories;
        _ranUpTime = (ranUpTime ?? TimeSpan.Zero) / 4.2;
        _statCollector = statCollector;
        _timeTol = (timeTol ?? TimeSpan.Zero) / 4.2;
    }


    private TimeSpan ModelTime
    {
        get => _modelTime;
        set
        {
            _deltaTime = value - _modelTime;
            _modelTime = value;
            _statCollector?.Log(_modelTime);
        }
    }


    public Dictionary<PcbDesignTechnology, TimeSpan> Simulate(TimeSpan finalTime)
    {
        finalTime = finalTime.FromWorkWeekFull();

        if (CurSettings.ExternalLoadThreadsCount > 0)
            _resourceManager.TryGetResources(new[]
            {
                new CpuRequest(_resourceManager.NewRequestId, CurSettings.ExternalLoadThreadsCount,
                    (Convert.ToDouble(CurSettings.ExternalLoadAvgOneThreadUtil),
                    Convert.ToDouble(CurSettings.ExternalLoadAvgOneThreadUtilReferenceClockRate)))
            }, out _);

        if (CurSettings.ExternalLoadAvgRamUsage > 0)
        {
            _resourceManager.TryGetResources(new[]
            {
                new RamRequest(_resourceManager.NewRequestId, Convert.ToDouble(CurSettings.ExternalLoadAvgRamUsage))
            }, out var receivedResources);
            if (receivedResources.Count < 1) return _techDuration;
        }

        while (_simulationEvents.Count > 0)
        {
            var closestEvent = _simulationEvents.First();
            ModelTime = closestEvent.ActivateTime;
            if (ModelTime > finalTime.AddAndClamp(_timeTol))
            {
                _statCollector?.MakeResourceSnapshot(finalTime.AddAndClamp(_timeTol), _activePcbDesignTechs.Count);
                return _techDuration;
            }

            //Determine Current state
            foreach (var activeTech in _activePcbDesignTechs.Where(technology => !technology.IsWaitForResources))
                activeTech.UpdateModelTime(_deltaTime);

            //Handle current Events
            HandleEvent(closestEvent);
            _simulationEvents.Remove(closestEvent);

            //Try Get Resources for pending procedures
            foreach (var activeTech in _activePcbDesignTechs) activeTech.TryGetResources();
            _statCollector?.MakeResourceSnapshot(ModelTime, _activePcbDesignTechs.Count);

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
                var newTechnology = new PcbDesignTechnology(NewId, _resourceManager, technologyStart.GeneratedPcb,
                    _eadSubSystemFactories, _statCollector);
                _activePcbDesignTechs.Add(newTechnology);
                _techStart.Add(newTechnology, ModelTime);
                _statCollector?.Log($"{String.Concat(Enumerable.Repeat("---", (newTechnology.TechId - 1) % 15))}" +
                                    $"Процесс Проектирования: {newTechnology.TechId} - ПОСТУПЛЕНИЕ ЗАЯВКИ НА ПРОЕКТИРОВАНИЕ");
                break;
            }

            case PcbDesignProcedureFinish procedureFinish:
            {
                var tech = procedureFinish.PcbDesignTechnology;
                _statCollector?.LogPcbDesignProcedureFinish(tech);

                if (!tech.MoveToNextProcedure())
                {
                    _activePcbDesignTechs.Remove(tech);
                    if (_techStart[tech] >= _ranUpTime)
                        _techDuration[tech] = _modelTime - _techStart[tech];
                    _statCollector?.Log($"{String.Concat(Enumerable.Repeat("---", (tech.TechId - 1) % 15))}" +
                                        $"Процесс Проектирования: {tech.TechId} - ФИНИШ ПРОЦЕССА ПРОЕКТИРОВАНИЯ");
                }

                break;
            }

            case ResourceFailure resourceFailure:
                resourceFailure.Resource.IsActive = false;
                _statCollector?.Log($"%%% Отказ Ресурса: {resourceFailure.Resource}");
                break;

            case ResourceRestored resourceRestored:
                resourceRestored.Resource.IsActive = true;
                _statCollector?.Log($"%%% Восcтановление Ресурса: {resourceRestored.Resource}");
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