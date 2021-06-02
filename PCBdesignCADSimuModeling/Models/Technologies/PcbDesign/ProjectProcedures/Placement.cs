using System;
using System.Collections.Generic;
using System.Linq;
using PCBdesignCADSimuModeling.Models.Resources;
using PCBdesignCADSimuModeling.Models.Resources.Algorithms;
using PCBdesignCADSimuModeling.Models.Resources.Algorithms.PlacingAlgorithms;
using PCBdesignCADSimuModeling.Models.Resources.ResourceRequests;

namespace PCBdesignCADSimuModeling.Models.Technologies.PcbDesign.ProjectProcedures
{
    public class Placement : PcbDesignProcedure
    {
        private readonly IPlacingAlgorithm _placingAlg;
        
        public Placement(PcbDesignTechnology context) : base(context)
        {
            _placingAlg = context.PcbAlgFactories.PlacingAlgFactory.Create(context.PcbParams);

            RequiredResources.Add(new DesignerRequest(ProcedureId));
            RequiredResources.Add(new CpuThreadRequest(ProcedureId, _placingAlg.MaxThreadUtilization));
        }

        public override bool NextProcedure()
        {
            Context.CurProcedure = new WireRouting(Context);
            return true;
        }

        public override TimeSpan UpdateModelTime(TimeSpan deltaTime)
        {
            var designerPower = ActiveResources.FindAll(resource => resource is Designer)
                .Sum(resource => resource.ResValueForProc(ProcedureId));
            var cpuPower = ActiveResources.FindAll(resource => resource is CpuThreads)
                .Sum(resource => resource.ResValueForProc(ProcedureId));
            
            return _placingAlg.UpdateModelTime(deltaTime, cpuPower);
        }
    }
}