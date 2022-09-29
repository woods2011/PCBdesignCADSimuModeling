using System;
using System.Collections.Generic;
using System.Linq;
using PcbDesignSimuModeling.WPF.Models.Resources;
using PcbDesignSimuModeling.WPF.Models.Resources.CpuClusterRes;
using PcbDesignSimuModeling.WPF.Models.Resources.EadSybSystems.PlacingSystems;
using PcbDesignSimuModeling.WPF.Models.Resources.RamRes;
using static PcbDesignSimuModeling.WPF.Models.SimuSystem.GeneralSimulationSettings;

namespace PcbDesignSimuModeling.WPF.Models.Technologies.PcbDesign.ProjectProcedures;

public class AutoPlacement : PcbDesignProcedure
{
    private readonly IPlacingSystem _placingAlg;
    private (CpuCluster Cpu, double Power) _cpu;

    public AutoPlacement(PcbDesignTechnology context) : base(context)
    {
        _placingAlg = context.EadSubSystemFactories.PlacingSysFactory.Create(context.PcbDescription);
        RequiredResources.AddRange(GetResourceRequestList());
    }


    public override bool NextProcedure()
    {
        Context.CurProcedure = new ManualPlacement(Context);
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
        new RamRequest(CommonResReqId,
            Convert.ToDouble(CurSettings.OsAvgRamUsage + CurSettings.MainEadAvgRamUsage)
            + 100.0 / 1024.0 
            + Context.PcbDescription.ElementsCount * (112.0 / 154.0) / 1024.0) 
    };

    public override string Name => "Автоматическое Размещение";
}