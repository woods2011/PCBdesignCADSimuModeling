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

        private TimeSpan _remainTime = TimeSpan.FromDays(1);

        public override void UpdateModelTime(TimeSpan deltaTime)
        {
            var maxDesignerPower =  (double) Enum.GetValues<Designer.ExperienceEn>().Max();
            var designerPower = ActiveResources.FindAll(resource => resource is Designer)
                .Sum(resource => 0.5 + resource.ResValueForProc(ProcedureId) / maxDesignerPower);
            
            var serverPower = ActiveResources.FindAll(resource => resource is Server)
                .Sum(resource => resource.ResValueForProc(ProcedureId));
            serverPower = 1.8 / Math.Exp(80.0 / serverPower);

            _remainTime -= deltaTime * designerPower * (0.4 + serverPower * 0.6);
        }

        public override TimeSpan EstimateEndTime()
        {
            var maxDesignerPower =  (double) Enum.GetValues<Designer.ExperienceEn>().Max();
            var designerPower = ActiveResources.FindAll(resource => resource is Designer)
                .Sum(resource => 0.5 + resource.ResValueForProc(ProcedureId) / maxDesignerPower);
            
            var serverPower = ActiveResources.FindAll(resource => resource is Server)
                .Sum(resource => resource.ResValueForProc(ProcedureId));
            serverPower = 1.8 / Math.Exp(80.0 / serverPower);
            
            var estimateEndTime = _remainTime > TimeSpan.FromSeconds(0.5) ? _remainTime / designerPower /  (0.4 + serverPower * 0.6) : TimeSpan.Zero;
            return estimateEndTime;
        }


        public override string Name { get; } = "Ввод описания";
    }
}