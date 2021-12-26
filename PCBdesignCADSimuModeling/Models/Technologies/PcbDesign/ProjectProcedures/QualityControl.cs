using System;
using System.Collections.Generic;
using System.Linq;
using PcbDesignCADSimuModeling.Models.Resources;
using PcbDesignCADSimuModeling.Models.Resources.ResourceRequests;

namespace PcbDesignCADSimuModeling.Models.Technologies.PcbDesign.ProjectProcedures
{
    public class QualityControl : PcbDesignProcedure
    {
        private double _designerPower;

        public QualityControl(PcbDesignTechnology context) : base(context) =>
            RequiredResources.AddRange(GetResourceRequestList());


        public override bool NextProcedure()
        {
            Context.CurProcedure = new DocumentationProduction(Context);
            return true;
        }

        private TimeSpan _remainTime = TimeSpan.FromDays(0.5);

        public override void UpdateModelTime(TimeSpan deltaTime) => _remainTime -= deltaTime * _designerPower;

        public override TimeSpan EstimateEndTime() =>
            _remainTime < TimeTol ? TimeSpan.Zero : _remainTime / _designerPower;

        public override void InitResourcesPower() => _designerPower =
            ActiveResources.OfType<Designer>().Sum(resource => resource.ResValueForProc(ProcId));

        private List<IResourceRequest> GetResourceRequestList() => new()
        {
            new DesignerRequest(ProcId)
        };

        public override string Name => "Оценка качества";
    }
}