using System;
using PCBdesignCADSimuModeling.Models.Resources;
using PCBdesignCADSimuModeling.Models.Resources.Algorithms;
using PCBdesignCADSimuModeling.Models.Resources.Algorithms.PlacingAlgorithms;
using PCBdesignCADSimuModeling.Models.Resources.Algorithms.WireRoutingAlgorithms;
using PCBdesignCADSimuModeling.Models.Technologies.PcbDesign.ProjectProcedures;

namespace PCBdesignCADSimuModeling.Models.Technologies.PcbDesign
{
    public class PcbDesignTechnology
    {
        private PcbDesignProcedure _curProcedure;
        private readonly IResourceManager _resourceManager;


        public PcbDesignTechnology(IResourceManager resourceManager, PcbParams pcbParams,
            PcbAlgFactories pcbAlgFactories)
        {
            _resourceManager = resourceManager;
            PcbParams = pcbParams;
            PcbAlgFactories = pcbAlgFactories;
            CurProcedure = new PcbParamsInput(this);
        }


        public PcbParams PcbParams { get; }
        public PcbAlgFactories PcbAlgFactories { get; }

        public PcbDesignProcedure CurProcedure
        {
            get => _curProcedure;
            set
            {
                _resourceManager.FreeResources(_curProcedure.Resources);
                _curProcedure = value;
                if (_curProcedure is not null)
                    IsWaitResources = !_resourceManager.TryGetResources(_curProcedure.Resources);
            }
        }

        public bool IsWaitResources { get; private set; }


        public TimeSpan UpdateModelTime(TimeSpan deltaTime)
        {
            // if (IsWaitResources)
            //     throw new InvalidOperationException("ProcedureIsWaitResources");
            if (IsWaitResources)
                return TimeSpan.MaxValue;

            return CurProcedure.UpdateModelTime(deltaTime);
        }

        public bool MoveToNextProcedure() => _curProcedure.NextProcedure();
    }
}