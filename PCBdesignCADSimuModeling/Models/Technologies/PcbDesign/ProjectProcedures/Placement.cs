using System;
using System.Collections.Generic;
using System.Linq;
using PcbDesignCADSimuModeling.Models.Resources;
using PcbDesignCADSimuModeling.Models.Resources.Algorithms;
using PcbDesignCADSimuModeling.Models.Resources.Algorithms.PlacingAlgorithms;
using PcbDesignCADSimuModeling.Models.Resources.ResourceRequests;

namespace PcbDesignCADSimuModeling.Models.Technologies.PcbDesign.ProjectProcedures
{
    public class Placement : PcbDesignProcedure
    {
        private readonly IPlacingAlgorithm _placingAlg;
        private double _cpuPower;

        public Placement(PcbDesignTechnology context) : base(context)
        {
            _placingAlg = context.PcbAlgFactories.PlacingAlgFactory.Create(context.PcbParams);
            RequiredResources.AddRange(GetResourceRequestList());
        }


        public override bool NextProcedure()
        {
            Context.CurProcedure = new WireRouting(Context);
            return true;
        }

        public override void UpdateModelTime(TimeSpan deltaTime)
        {
            _placingAlg.UpdateModelTime(deltaTime, _cpuPower);
        }

        public override TimeSpan EstimateEndTime()
        {
            _cpuPower = ActiveResources.OfType<CpuThreads>().Sum(resource => resource.ResValueForProc(ProcId));
            return _placingAlg.EstimateEndTime(_cpuPower);
        }

        public override void InitResourcesPower()
        {
        }


        private List<IResourceRequest> GetResourceRequestList() => new()
        {
            //RequiredResources.Add(new DesignerRequest(ProcedureId));
            new CpuThreadRequest(ProcId, _placingAlg.MaxThreadUtilization),
        };

        public override string Name => "Размещение";
    }
}