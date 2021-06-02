using System;
using System.Collections.Generic;
using System.Linq;
using PCBdesignCADSimuModeling.Models.Resources;
using PCBdesignCADSimuModeling.Models.Resources.Algorithms;
using PCBdesignCADSimuModeling.Models.Resources.Algorithms.WireRoutingAlgorithms;
using PCBdesignCADSimuModeling.Models.Resources.ResourceRequests;

namespace PCBdesignCADSimuModeling.Models.Technologies.PcbDesign.ProjectProcedures
{
    public class WireRouting : PcbDesignProcedure
    {
        private readonly IWireRoutingAlgorithm _wireRoutingAlg;
        
        public WireRouting(PcbDesignTechnology context) : base(context)
        {
            _wireRoutingAlg = context.PcbAlgFactories.WireRoutingAlgFactory.Create(context.PcbParams);

            RequiredResources.Add(new DesignerRequest(ProcedureId));
            RequiredResources.Add(new CpuThreadRequest(ProcedureId, _wireRoutingAlg.MaxThreadUtilization));
        }

        public override bool NextProcedure()
        {
            Context.CurProcedure = new QualityControl(Context);
            return true;
        }

        public override TimeSpan UpdateModelTime(TimeSpan deltaTime)
        {
            var designerPower = ActiveResources.FindAll(resource => resource is Designer)
                .Sum(resource => resource.ResValueForProc(ProcedureId));
            var cpuPower = ActiveResources.FindAll(resource => resource is CpuThreads)
                .Sum(resource => resource.ResValueForProc(ProcedureId));
            
            return _wireRoutingAlg.UpdateModelTime(deltaTime, cpuPower);
        }
    }


}