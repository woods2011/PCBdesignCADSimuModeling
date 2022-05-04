using PcbDesignSimuModeling.Core.Models.Resources;
using PcbDesignSimuModeling.Core.Models.Resources.Algorithms.WireRoutingAlgorithms;
using PcbDesignSimuModeling.Core.Models.Resources.Cpu;

namespace PcbDesignSimuModeling.Core.Models.Technologies.PcbDesign.ProjectProcedures;

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
        _cpuPower = ActiveResources.Select(tuple => tuple.Resource).OfType<CpuCluster>().Sum(resource => resource.PowerForRequest(CommonResReqId));
        return _wireRoutingAlg.EstimateEndTime(_cpuPower);
    }

    public override void InitResourcesPower()
    {
    }

    private List<IResourceRequest> GetResourceRequestList() => new()
    {
        new CpuRequest(CommonResReqId, _wireRoutingAlg.MaxThreadUtilization),
    };

    public override string Name => "Трассировка";
}