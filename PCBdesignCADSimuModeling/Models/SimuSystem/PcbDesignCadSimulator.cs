using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Windows.Input;
using System.Windows.Media.Animation;
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
        private readonly IResourceManager _resourceManager;
        private readonly ISimuEventGenerator _simuEventGenerator;
        private readonly Dictionary<PcbDesignTechnology, PcbDesignProcedureFinish> _activePcbDesignTechs = new();
        private readonly List<SimulationEvent> _simulationEvents = new List<SimulationEvent>();


        public PcbDesignCadSimulator(ISimuEventGenerator simuEventGenerator, List<Resource> recoursePool,
            IPcbAlgFactories pcbAlgFactories, TimeSpan? startTime = null)
        {
            _pcbAlgFactories = pcbAlgFactories;
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
            }
        }

        public bool SimulationIsCompleted => _modelTime > _finalTime;


        public void Simulate(TimeSpan finalTime)
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
                foreach (var (activeTech, activeEvent) in _activePcbDesignTechs)
                    activeEvent.ActivateTime = activeTech.UpdateModelTime(_deltaTime);

                //Handle current Events
                var curEvents = _simulationEvents.Where(simuEvent => simuEvent.ActivateTime == ModelTime)
                    .OrderBy(simuEvent => simuEvent.Priority).ToList();
                foreach (var curEvent in curEvents)
                {
                    HandleEvent(curEvent);
                    _simulationEvents.Remove(curEvent);
                }
            }

            _modelTime = _finalTime + TimeSpan.FromMilliseconds(1); //?
        }

        private void HandleEvent(SimulationEvent curEvent)
        {
            switch (curEvent)
            {
                case PcbDesignTechnologyStart pcbDesignTechnologyStart:
                {
                    var newTechnology = new PcbDesignTechnology(_resourceManager, pcbDesignTechnologyStart.GeneratedPcb,
                        _pcbAlgFactories);
                    _activePcbDesignTechs.Add(newTechnology, new PcbDesignProcedureFinish(newTechnology));
                    break;
                }

                case PcbDesignProcedureFinish pcbDesignProcedureFinish:
                {
                    var tech = pcbDesignProcedureFinish.PcbDesignTechnology;

                    if (tech.UpdateModelTime(_deltaTime) > TimeSpan.Zero) throw new Exception("Paradox?????"); //ToDo

                    _activePcbDesignTechs.Remove(tech);

                    if (tech.MoveToNextProcedure())
                        _activePcbDesignTechs.Add(tech, new PcbDesignProcedureFinish(tech));

                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(curEvent));
            }
        }
    }
}