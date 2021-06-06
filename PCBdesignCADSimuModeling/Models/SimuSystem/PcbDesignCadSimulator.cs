using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Windows.Input;
using System.Windows.Media.Animation;
using PCBdesignCADSimuModeling.Models.Loggers;
using PCBdesignCADSimuModeling.Models.Resources;
using PCBdesignCADSimuModeling.Models.Resources.Algorithms;
using PCBdesignCADSimuModeling.Models.SimuSystem.SimulationEvents;
using PCBdesignCADSimuModeling.Models.Technologies.PcbDesign;
using PCBdesignCADSimuModeling.Models.Technologies.PcbDesign.ProjectProcedures;

namespace PCBdesignCADSimuModeling.Models.SimuSystem
{
    public class PcbDesignCadSimulator
    {
        private TimeSpan _modelTime;
        private TimeSpan _deltaTime = TimeSpan.Zero;
        private TimeSpan _finalTime = TimeSpan.Zero;
        private readonly IPcbAlgFactories _pcbAlgFactories;
        private readonly ISimpleLogger _logger;
        private readonly IResourceManager _resourceManager;
        private readonly ISimuEventGenerator _simuEventGenerator;
        private readonly Dictionary<PcbDesignTechnology, PcbDesignProcedureFinish> _activePcbDesignTechs = new();
        private readonly List<SimulationEvent> _simulationEvents = new List<SimulationEvent>();
        private readonly Dictionary<PcbDesignTechnology, (TimeSpan Start, TimeSpan Finish)> _techStartAndFinish = new();
 

        public PcbDesignCadSimulator(ISimuEventGenerator simuEventGenerator, List<IResource> recoursePool,
            IPcbAlgFactories pcbAlgFactories, ISimpleLogger logger, TimeSpan? startTime = null)
        {
            _pcbAlgFactories = pcbAlgFactories;
            _logger = logger;
            _simuEventGenerator = simuEventGenerator;
            _modelTime = startTime ?? TimeSpan.Zero;
            _resourceManager = new ResourceManager(recoursePool);
        }


        private TimeSpan ModelTime
        {
            get => _modelTime;
            set
            {
                _deltaTime = value - _modelTime;
                _modelTime = value;
                _logger.ModelTime = TimeSpan.FromSeconds(Math.Round(value.TotalSeconds)); // ToDo: delete this
            }
        }

        public bool SimulationIsCompleted => _modelTime > _finalTime;


        public Dictionary<PcbDesignTechnology, (TimeSpan Start, TimeSpan Finish)> Simulate(TimeSpan finalTime)
        {
            _finalTime = finalTime;
            if (SimulationIsCompleted) throw new InvalidOperationException("Simulation already completed");

            //Generate initial events
            _simulationEvents.AddRange(
                _simuEventGenerator.GeneratePcbDesignTech(_finalTime, ModelTime));

            while (_simulationEvents.Count > 0)
            {
                ModelTime = _simulationEvents.Min(simuEvent => simuEvent.ActivateTime);

                if (SimulationIsCompleted) break;

                //Determine Current state
                foreach (var activeTech in _activePcbDesignTechs.Keys.OrderBy(technology => _techStartAndFinish[technology].Start))
                    activeTech.UpdateModelTime(_deltaTime);

                //Handle current Events
                var curEvents = _simulationEvents.Where(simuEvent => simuEvent.ActivateTime == ModelTime)
                    .OrderBy(simuEvent => simuEvent.Priority).ToList();
                foreach (var curEvent in curEvents)
                {
                    HandleEvent(curEvent);
                    _simulationEvents.Remove(curEvent);
                }
                
                //Correct activate time for events
                foreach (var (activeTech, activeEvent) in _activePcbDesignTechs.OrderBy(pair => _techStartAndFinish[pair.Key].Start))
                    activeEvent.ActivateTime = _modelTime + activeTech.EstimateEndTime();
            }
            
            _modelTime = _finalTime + TimeSpan.FromMilliseconds(1); //?
            return _techStartAndFinish.Where(pair => pair.Value.Finish != TimeSpan.MaxValue).ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        
        private void HandleEvent(SimulationEvent curEvent)
        {
            switch (curEvent)
            {
                case PcbDesignTechnologyStart pcbDesignTechnologyStart:
                {
                    var newTechnology = new PcbDesignTechnology(_resourceManager, pcbDesignTechnologyStart.GeneratedPcb,
                        _pcbAlgFactories, _logger);
                    var procedureFinishEvent = new PcbDesignProcedureFinish(newTechnology, _modelTime);
                    
                    _activePcbDesignTechs.Add(newTechnology, procedureFinishEvent);
                    _simulationEvents.Add(procedureFinishEvent);
                    
                    
                    _logger.Log($"{_logger.ModelTime} | Технология: {newTechnology.TechId} - Старт технологии");
                    _techStartAndFinish.Add(newTechnology, (_logger.ModelTime, TimeSpan.MaxValue));
                    
                    break;
                }

                case PcbDesignProcedureFinish pcbDesignProcedureFinish:
                {
                    var tech = pcbDesignProcedureFinish.PcbDesignTechnology;

                    if (tech.EstimateEndTime() > TimeSpan.Zero) throw new Exception($"Paradox????? {tech.EstimateEndTime()}"); //ToDo

                    
                    _activePcbDesignTechs.Remove(tech);
                    _logger.Log($"{_logger.ModelTime} | Технология: {tech.TechId} - Финиш проектной процедуры: {tech.CurProcedure.Name}");

                    
                    if (tech.MoveToNextProcedure())
                    {
                        var procedureFinishEvent = new PcbDesignProcedureFinish(tech, _modelTime);
                        
                        _activePcbDesignTechs.Add(tech, procedureFinishEvent);
                        _simulationEvents.Add(procedureFinishEvent);
                    }
                    else
                    {
                        _logger.Log($"{_logger.ModelTime} | Технология: {tech.TechId} - Финиш технологии");
                        _techStartAndFinish[tech] = (_techStartAndFinish[tech].Start, _logger.ModelTime);
                    }

                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(curEvent));
            }
        }
    }
}