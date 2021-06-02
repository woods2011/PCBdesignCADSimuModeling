using System;
using System.Collections.Generic;
using System.Linq;
using PCBdesignCADSimuModeling.Models.Resources;
using PCBdesignCADSimuModeling.Models.Resources.ResourceRequests;

namespace PCBdesignCADSimuModeling.Models.Technologies.PcbDesign.ProjectProcedures
{
    public class QualityControl : PcbDesignProcedure
    {
        public QualityControl(PcbDesignTechnology context) : base(context)
        {
            RequiredResources.Add(new DesignerRequest(ProcedureId));
        }


        public override bool NextProcedure()
        {
            if (true)
                Context.CurProcedure = new DocumentationProduction(Context);
            else
                Context.CurProcedure = new Placement(Context);
            
            return true;
        }

        public override TimeSpan UpdateModelTime(TimeSpan deltaTime)
        {
            var designerPower = ActiveResources.FindAll(resource => resource is Designer)
                .Sum(resource => resource.ResValueForProc(ProcedureId));
            
            throw new NotImplementedException();
        }
    }
}