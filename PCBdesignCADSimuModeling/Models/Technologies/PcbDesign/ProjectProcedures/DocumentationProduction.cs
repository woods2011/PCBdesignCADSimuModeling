using System;
using System.Linq;
using PCBdesignCADSimuModeling.Models.Resources;
using PCBdesignCADSimuModeling.Models.Resources.ResourceRequests;

namespace PCBdesignCADSimuModeling.Models.Technologies.PcbDesign.ProjectProcedures
{
    public class DocumentationProduction : PcbDesignProcedure
    {
        public DocumentationProduction(PcbDesignTechnology context) : base(context)
        {
            RequiredResources.Add(new ServerRequest(ProcedureId));
        }


        public override bool NextProcedure()
        {
            Context.CurProcedure = null;
            return false;
        }

        public override TimeSpan UpdateModelTime(TimeSpan deltaTime)
        {
            var serverPower = ActiveResources.FindAll(resource => resource is Server)
                .Sum(resource => resource.ResValueForProc(ProcedureId));
            
            throw new NotImplementedException();
        }
    }
}