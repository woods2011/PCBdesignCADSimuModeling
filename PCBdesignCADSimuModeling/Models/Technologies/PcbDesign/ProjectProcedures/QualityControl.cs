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

        private TimeSpan _remainTime = TimeSpan.FromDays(0.5);
        
        public override void UpdateModelTime(TimeSpan deltaTime)
        {
            var maxDesignerPower =  (double) Enum.GetValues<Designer.ExperienceEn>().Max();
            var designerPower = ActiveResources.FindAll(resource => resource is Designer)
                .Sum(resource => 0.5 + resource.ResValueForProc(ProcedureId) / maxDesignerPower);
            
            _remainTime -= deltaTime * designerPower;
        }

        public override TimeSpan EstimateEndTime()
        {
            var maxDesignerPower =  (double) Enum.GetValues<Designer.ExperienceEn>().Max();
            var designerPower = ActiveResources.FindAll(resource => resource is Designer)
                .Sum(resource => 0.5 + resource.ResValueForProc(ProcedureId) / maxDesignerPower);
            
            var estimateEndTime = _remainTime > TimeSpan.FromSeconds(0.5) ? _remainTime / designerPower : TimeSpan.Zero;
            return estimateEndTime;
        }


        public override string Name { get; } = "Оценка качества";
    }
}