using PcbDesignSimuModeling.Core.Models.Resources;
using PcbDesignSimuModeling.Core.Models.Resources.Algorithms.WireRoutingAlgorithms;
using PcbDesignSimuModeling.Core.Models.Resources.Cpu;
using PcbDesignSimuModeling.Core.Models.Resources.Ram;

namespace PcbDesignSimuModeling.Core.Models.Technologies.PcbDesign.ProjectProcedures;

public class WireRouting : PcbDesignProcedure
{
    private readonly IWireRoutingAlgorithm _wireRoutingAlg;
    private (CpuCluster Cpu, double Power) _cpu;

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
        _wireRoutingAlg.UpdateModelTime(deltaTime, _cpu.Power);
    }

    public override TimeSpan EstimateEndTime()
    {
        _cpu.Power = _cpu.Cpu.PowerForRequest(CommonResReqId);
        return _wireRoutingAlg.EstimateEndTime(_cpu.Power);
    }

    public override void InitResources() =>
        _cpu = (ActiveResources.Select(tuple => tuple.Resource).OfType<CpuCluster>().First(), 0.0);

    private List<IResourceRequest> GetResourceRequestList() => new()
    {
        new CpuRequest(CommonResReqId, _wireRoutingAlg.MaxThreadUtilization),
        new RamRequest(CommonResReqId, 2.0)
    };

    public override string Name => "Трассировка";
}