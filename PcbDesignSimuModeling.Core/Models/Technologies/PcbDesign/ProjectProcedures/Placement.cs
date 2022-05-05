using PcbDesignSimuModeling.Core.Models.Resources;
using PcbDesignSimuModeling.Core.Models.Resources.Algorithms.PlacingAlgorithms;
using PcbDesignSimuModeling.Core.Models.Resources.Cpu;
using PcbDesignSimuModeling.Core.Models.Resources.Ram;

namespace PcbDesignSimuModeling.Core.Models.Technologies.PcbDesign.ProjectProcedures;

public class Placement : PcbDesignProcedure
{
    private readonly IPlacingAlgorithm _placingAlg;
    private (CpuCluster Cpu, double Power) _cpu;

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
        _placingAlg.UpdateModelTime(deltaTime, _cpu.Power);
    }

    public override TimeSpan EstimateEndTime()
    {
        _cpu.Power = _cpu.Cpu.PowerForRequest(CommonResReqId);
        return _placingAlg.EstimateEndTime(_cpu.Power);
    }

    public override void InitResources() =>
        _cpu = (ActiveResources.Select(tuple => tuple.Resource).OfType<CpuCluster>().First(), 0.0);


    private List<IResourceRequest> GetResourceRequestList() => new()
    {
        new CpuRequest(CommonResReqId, _placingAlg.MaxThreadUtilization),
        new RamRequest(CommonResReqId, 2.0)
    };

    public override string Name => "Размещение";
}