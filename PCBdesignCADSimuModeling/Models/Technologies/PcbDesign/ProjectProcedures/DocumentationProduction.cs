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
        
        private TimeSpan _totalTime = TimeSpan.FromHours(6);

        public override TimeSpan UpdateModelTime(TimeSpan deltaTime)
        {
            var serverPower = ActiveResources.FindAll(resource => resource is Server)
                .Sum(resource => resource.ResValueForProc(ProcedureId));
            
            _totalTime -= deltaTime;
            return _totalTime;
        }
    }
}