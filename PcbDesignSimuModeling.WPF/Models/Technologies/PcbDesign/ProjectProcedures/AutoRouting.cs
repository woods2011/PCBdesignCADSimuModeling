using System;
using System.Collections.Generic;
using System.Linq;
using PcbDesignSimuModeling.WPF.Models.Resources;
using PcbDesignSimuModeling.WPF.Models.Resources.CpuClusterRes;
using PcbDesignSimuModeling.WPF.Models.Resources.EadSybSystems.RoutingSystems;
using PcbDesignSimuModeling.WPF.Models.Resources.RamRes;
using static PcbDesignSimuModeling.WPF.Models.SimuSystem.GeneralSimulationSettings;

namespace PcbDesignSimuModeling.WPF.Models.Technologies.PcbDesign.ProjectProcedures;

public class AutoRouting : PcbDesignProcedure
{
    private readonly IRoutingSystem _routingAlg;
    private (CpuCluster Cpu, double Power) _cpu;

    public AutoRouting(PcbDesignTechnology context) : base(context)
    {
        _routingAlg = context.EadSubSystemFactories.RoutingSysFactory.Create(context.PcbDescription);
        RequiredResources.AddRange(GetResourceRequestList());
    }


    public override bool NextProcedure()
    {
        Context.CurProcedure = new ManualRouting(Context);
        return true;
    }

    public override void UpdateModelTime(TimeSpan deltaTime)
    {
        _routingAlg.UpdateModelTime(deltaTime, _cpu.Power);
    }

    public override TimeSpan EstimateEndTime()
    {
        _cpu.Power = _cpu.Cpu.PowerForRequest(CommonResReqId);
        return _routingAlg.EstimateEndTime(_cpu.Power);
    }

    public override void InitResources() =>
        _cpu = (ActiveResources.Select(tuple => tuple.Resource).OfType<CpuCluster>().First(), 0.0);

    private List<IResourceRequest> GetResourceRequestList() => new()
    {
        new CpuRequest(CommonResReqId, _routingAlg.MaxThreadUtilization),
        new RamRequest(CommonResReqId,
            Convert.ToDouble(CurSettings.OsAvgRamUsage + CurSettings.MainEadAvgRamUsage)
            + 200.0 / 1024.0
            + Context.PcbDescription.ElementsCount * (112.0 / 154.0) / 1024.0)
    };

    public override string Name => "Автоматическая Трассировка";
}