using System;
using System.Collections.Generic;
using System.Linq;
using PcbDesignSimuModeling.WPF.Models.Resources;
using PcbDesignSimuModeling.WPF.Models.Resources.CpuClusterRes;
using PcbDesignSimuModeling.WPF.Models.Resources.DesignerRes;
using PcbDesignSimuModeling.WPF.Models.Resources.RamRes;
using PcbDesignSimuModeling.WPF.Models.Resources.ServerRes;
using static PcbDesignSimuModeling.WPF.Models.SimuSystem.GeneralSimulationSettings;

namespace PcbDesignSimuModeling.WPF.Models.Technologies.PcbDesign.ProjectProcedures;

public class ManualPlacement : PcbDesignProcedure
{
    private TimeSpan _remainTime;

    private (Resources.ServerRes.Server Server, double Power) _server;
    private (double Power, double GoalPower) _placementCpu;

    private CpuCluster _cpuCluster = null!;

    private double _totalResPower = 1.0;

    public ManualPlacement(PcbDesignTechnology context) : base(context)
    {
        RequiredResources.AddRange(GetResourceRequestList());

        var pcbDescription = Context.PcbDescription;
        _remainTime = TimeSpan.FromHours(17.5)
                      * (pcbDescription.ElementsCount / 80.0)
                      * (pcbDescription.ElementsDensity / 1)
                      * 0.5
                      * 0.5;
        _remainTime *= 1.2;
    }


    public override bool NextProcedure()
    {
        Context.CurProcedure = new AutoRouting(Context);
        return true;
    }

    public override void UpdateModelTime(TimeSpan deltaTime)
    {
        _remainTime -= deltaTime * _totalResPower;
    }

    public override TimeSpan EstimateEndTime()
    {
        _server.Power = _server.Server.PowerForRequest(CommonResReqId);

        _placementCpu.Power = _cpuCluster.PowerForRequest(CommonResReqId);

        _placementCpu.GoalPower = Convert.ToDouble(CurSettings.OsAvgThreadsCount *
                                                   CurSettings.OsAvgOneThreadUtil *
                                                   CurSettings.OsAvgOneThreadUtilReferenceClockRate);

        _totalResPower = Math.Min(1.0, _server.Power / 17) * (_placementCpu.Power / _placementCpu.GoalPower);

        return _remainTime < TimeTol ? TimeSpan.Zero : _remainTime / _totalResPower;
    }

    public override void InitResources()
    {
        _server = (ActiveResources.Select(tuple => tuple.Resource).OfType<Server>().First(), 0.0);

        _cpuCluster = ActiveResources.Select(tuple => tuple.Resource).OfType<CpuCluster>().First();
        _placementCpu.GoalPower = 0.5 * 4;
    }


    private List<IResourceRequest> GetResourceRequestList() => new()
    {
        new CpuRequest(CommonResReqId, 1, (0.5, 4.0)),

        new DesignerRequest(CommonResReqId),
        new ServerRequest(CommonResReqId),
        new RamRequest(CommonResReqId,
            Convert.ToDouble(CurSettings.OsAvgRamUsage + CurSettings.MainEadAvgRamUsage)
            + 120.0 / 1024.0
            + Context.PcbDescription.ElementsCount * (112.0 / 154.0) / 1024.0)
    };

    public override string Name => "Интерактивное/Ручное Размещение";
}