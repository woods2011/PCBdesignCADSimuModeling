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
        
        private TimeSpan _remainTime = TimeSpan.FromDays(0.5);

        public override void UpdateModelTime(TimeSpan deltaTime)
        {
            var serverPower = ActiveResources.FindAll(resource => resource is Server)
                .Sum(resource => resource.ResValueForProc(ProcedureId));
            serverPower = 1.8 / Math.Exp(40.0 / serverPower);
            
            _remainTime -= deltaTime * serverPower;
        }

        public override TimeSpan EstimateEndTime() => _remainTime > TimeSpan.Zero ? _remainTime : TimeSpan.Zero;
    }
}