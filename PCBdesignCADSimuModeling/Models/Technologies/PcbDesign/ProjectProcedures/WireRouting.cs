using System;
using System.Collections.Generic;
using System.Linq;
using PcbDesignCADSimuModeling.Models.Resources;
using PcbDesignCADSimuModeling.Models.Resources.Algorithms;
using PcbDesignCADSimuModeling.Models.Resources.Algorithms.WireRoutingAlgorithms;
using PcbDesignCADSimuModeling.Models.Resources.ResourceRequests;

namespace PcbDesignCADSimuModeling.Models.Technologies.PcbDesign.ProjectProcedures
{
    public class WireRouting : PcbDesignProcedure
    {
        private readonly IWireRoutingAlgorithm _wireRoutingAlg;
        private double _cpuPower;

        public WireRouting(PcbDesignTechnology context) : base(context)
        {
            _wireRoutingAlg = context.PcbAlgFactories.WireRoutingAlgFactory.Create(context.PcbParams);
            RequiredResources.AddRange(GetResourceRequestList());
        }


        public override bool NextProcedure()
        {
            Context.CurProcedure = new QualityControl(Context);
            return true;
        }

        public override void UpdateModelTime(TimeSpan deltaTime)
        {
            _wireRoutingAlg.UpdateModelTime(deltaTime, _cpuPower);
        }

        public override TimeSpan EstimateEndTime()
        {
            _cpuPower = ActiveResources.OfType<CpuThreads>().Sum(resource => resource.ResValueForProc(ProcId));
            return _wireRoutingAlg.EstimateEndTime(_cpuPower);
        }

        public override void InitResourcesPower()
        {
        }

        private List<IResourceRequest> GetResourceRequestList() => new()
        {
            //RequiredResources.Add(new DesignerRequest(ProcedureId));
            new CpuThreadRequest(ProcId, _wireRoutingAlg.MaxThreadUtilization),
        };

        public override string Name => "Трассировка";
    }
}