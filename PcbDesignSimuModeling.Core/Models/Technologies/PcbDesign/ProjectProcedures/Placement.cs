﻿using PcbDesignSimuModeling.Core.Models.Resources;
using PcbDesignSimuModeling.Core.Models.Resources.Algorithms.PlacingAlgorithms;
using PcbDesignSimuModeling.Core.Models.Resources.ResourceRequests;

namespace PcbDesignSimuModeling.Core.Models.Technologies.PcbDesign.ProjectProcedures;

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
        _cpuPower = ActiveResources.OfType<CpuCluster>().Sum(resource => resource.ResValueForProc(ProcId));
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