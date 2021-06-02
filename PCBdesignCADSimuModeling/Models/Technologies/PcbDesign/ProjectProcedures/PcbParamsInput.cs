using System;
using System.Collections.Generic;
using System.Linq;
using PCBdesignCADSimuModeling.Models.Resources;
using PCBdesignCADSimuModeling.Models.Resources.ResourceRequests;

namespace PCBdesignCADSimuModeling.Models.Technologies.PcbDesign.ProjectProcedures
{
    public class PcbParamsInput : PcbDesignProcedure
    {
        public PcbParamsInput(PcbDesignTechnology context) : base(context)
        {
            RequiredResources.Add(new DesignerRequest(ProcedureId));
            RequiredResources.Add(new ServerRequest(ProcedureId));
        }


        public override bool NextProcedure()
        {
            Context.CurProcedure = new Placement(Context);
            return true;
        }

        private TimeSpan _totalTime = TimeSpan.FromHours(6);

        public override TimeSpan UpdateModelTime(TimeSpan deltaTime)
        {
            var designerPower = ActiveResources.FindAll(resource => resource is Designer)
                .Sum(resource => resource.ResValueForProc(ProcedureId));
            var serverPower = ActiveResources.FindAll(resource => resource is Server)
                .Sum(resource => resource.ResValueForProc(ProcedureId));

            _totalTime -= deltaTime;
            return _totalTime;
        }
    }
}