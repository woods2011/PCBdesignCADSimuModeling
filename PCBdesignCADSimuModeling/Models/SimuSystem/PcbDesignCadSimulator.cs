using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Windows.Input;
using PCBdesignCADSimuModeling.Models.Resources;
using PCBdesignCADSimuModeling.Models.Resources.Algorithms;
using PCBdesignCADSimuModeling.Models.Technologies.PcbDesign;
using PCBdesignCADSimuModeling.Models.Technologies.PcbDesign.ProjectProcedures;

namespace PCBdesignCADSimuModeling.Models.SimuSystem
{
    public class PcbDesignCadSimulator
    {
        private TimeSpan _modelTime = TimeSpan.Zero;
        private TimeSpan _finalTime;
        private readonly PcbAlgFactories _pcbAlgFactories;
        private readonly IResourceManager _resourceManager = new ResourceManager();
        private readonly List<PcbDesignTechnology> _activePcbDesignTechs = new List<PcbDesignTechnology>();
        private readonly List<SimulationEvent> _simulationEvents = new List<SimulationEvent>();


        public PcbDesignCadSimulator(PcbAlgFactories pcbAlgFactories, TimeSpan finalTime)
        {
            _pcbAlgFactories = pcbAlgFactories;
            _finalTime = finalTime;
        }


        public void Simulate()
        {
            while (_simulationEvents.Count > 0 && _modelTime < _finalTime)
            {
                var curEvent = _simulationEvents.OrderBy(simuEvent => simuEvent.ActivateTime)
                    .ThenBy(simuEvent => simuEvent.Priority).First();
                
                HandleEvent(curEvent);
                
                GenerateEvents();
            }

            PcbParams pcbParams = null;

            throw new NotImplementedException();

            _ = new PcbDesignTechnology(_resourceManager, pcbParams, _pcbAlgFactories);
        }

        private void GenerateEvents()
        {
            throw new NotImplementedException();
        }

        private void HandleEvent(SimulationEvent curEvent)
        {
            var curEventActivateTime = curEvent.ActivateTime;
            var deltaTime = _modelTime - curEventActivateTime;
            _modelTime = curEventActivateTime;
            
            switch (curEvent)
            {
                case null:
                    throw new ArgumentNullException(nameof(curEvent));
                
                case PcbDesignTechnologyStart pcbDesignTechnologyStart:
                    _activePcbDesignTechs.Add(new PcbDesignTechnology(_resourceManager, pcbDesignTechnologyStart.GeneratedPcb, _pcbAlgFactories));
                    break;
                
                case PcbDesignTechnologyFinish pcbDesignTechnologyFinish:
                    var technology = pcbDesignTechnologyFinish.PcbDesignTechnology;
                    
                    if (technology.UpdateModelTime(deltaTime).Item2 > TimeSpan.Zero)
                        throw new Exception("Paradox?????"); //ToDo
                    
                    
                    
                    _activePcbDesignTechs.Remove(technology);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(curEvent));
            }
        }
    }

    public class SimulationEvent
    {
        public SimulationEvent(int priority, TimeSpan activateTime)
        {
            Priority = priority;
            ActivateTime = activateTime;
        }
        protected SimulationEvent(int priority)
        {
            Priority = priority;
        }
        
        
        public TimeSpan ActivateTime { get; set; }
        public int Priority { get; }
    }

    public sealed class PcbDesignTechnologyFinish : SimulationEvent
    {
        public PcbDesignTechnologyFinish(PcbDesignTechnology pcbDesignTechnology) : base(1)
        {
            PcbDesignTechnology = pcbDesignTechnology;
            ActivateTime = pcbDesignTechnology.UpdateModelTime(TimeSpan.Zero).Item2;
        }

        public PcbDesignTechnology PcbDesignTechnology { get; }
    }

    public sealed class PcbDesignTechnologyStart : SimulationEvent
    {
        public PcbDesignTechnologyStart(TimeSpan activateTime, PcbParams generatedPcb) : base(2, activateTime)
        {
            GeneratedPcb = generatedPcb;
        }
        
        public PcbParams GeneratedPcb { get; }
    }
}