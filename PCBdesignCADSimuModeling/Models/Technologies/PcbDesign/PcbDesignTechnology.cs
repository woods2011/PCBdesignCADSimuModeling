using System;
using PCBdesignCADSimuModeling.Models.Resources;
using PCBdesignCADSimuModeling.Models.Technologies.PcbDesign.ProjectProcedures;

namespace PCBdesignCADSimuModeling.Models.Technologies.PcbDesign
{
    public class PcbDesignTechnology
    {
        private PcbDesignProcedure _curProcedure;
        private readonly IResourceManager _resourceManager;
        
        
        public PcbDesignTechnology(IResourceManager resourceManager, PcbParams pcb)
        {
            _resourceManager = resourceManager;
            PCB = pcb;
        }

        
        public PcbDesignProcedure CurProcedure
        {
            get => _curProcedure;
            set
            {
                _resourceManager.FreeResources(_curProcedure.Resources);
                _curProcedure = value;
                IsWaitResources = !_resourceManager.TryGetResources(_curProcedure.Resources);
            }
        }
        public bool IsWaitResources { get; private set; }
        public PcbParams PCB { get; }
        
        
        
        public (PcbDesignProcedure, TimeSpan) UpdateModelTime(TimeSpan deltaTime)
        {
            if (IsWaitResources)
                throw new InvalidOperationException("ProcedureIsWaitResources");
            
            return (CurProcedure, CurProcedure.UpdateModelTime(deltaTime));
        }
    }
}