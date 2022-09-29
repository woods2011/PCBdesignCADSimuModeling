﻿using System;
using System.Collections.Generic;
using System.Linq;
using PcbDesignSimuModeling.WPF.Models.Resources;
using PcbDesignSimuModeling.WPF.Models.Resources.CpuClusterRes;
using PcbDesignSimuModeling.WPF.Models.Resources.DesignerRes;
using PcbDesignSimuModeling.WPF.Models.Resources.RamRes;
using PcbDesignSimuModeling.WPF.Models.Resources.ServerRes;
using static PcbDesignSimuModeling.WPF.Models.SimuSystem.GeneralSimulationSettings;

namespace PcbDesignSimuModeling.WPF.Models.Technologies.PcbDesign.ProjectProcedures;

public class BoardParamsAndRules : PcbDesignProcedure
{
    private TimeSpan _remainTime;
    private (Server Server, double Power) _server;

    private double _totalResPower = 1.0;

    private CpuCluster _cpuCluster = null!;

    private readonly int _osCpuReqId;
    private readonly int _eadCpuReqId;
    private readonly int _inputCpuReqId;
    private (double Power, double GoalPower) _osCpu;
    private (double Power, double GoalPower) _eadCpu;
    private (double Power, double GoalPower) _inputCpu;

    public BoardParamsAndRules(PcbDesignTechnology context) : base(context)
    {
        _osCpuReqId = CommonResReqId;
        _eadCpuReqId = Context.NewRequestId;
        _inputCpuReqId = Context.NewRequestId;

        var pcbDescription = Context.PcbDescription;
        _remainTime = TimeSpan.FromMinutes(32.32)
                      + Math.Max(0, pcbDescription.NumberOfLayers - 2) * TimeSpan.FromMinutes(20.21)
                      + TimeSpan.FromMinutes(15.11)
                      + Math.Max(0, pcbDescription.NumberOfLayers - 1) * TimeSpan.FromMinutes(20.21);
        // ToDo: add time for rules creation
        _remainTime *= 1.15;

        RequiredResources.AddRange(GetResourceRequestList());
    }


    public override bool NextProcedure()
    {
        Context.CurProcedure = new AutoPlacement(Context);
        return true;
    }

    public override void UpdateModelTime(TimeSpan deltaTime)
    {
        _remainTime -= deltaTime * _totalResPower;
    }

    public override TimeSpan EstimateEndTime()
    {
        _server.Power = _server.Server.PowerForRequest(CommonResReqId);

        _osCpu.Power = _cpuCluster.PowerForRequest(_osCpuReqId);
        _eadCpu.Power = _cpuCluster.PowerForRequest(_eadCpuReqId);
        _inputCpu.Power = _cpuCluster.PowerForRequest(_inputCpuReqId);

        _totalResPower = Math.Min(1.0, _server.Power / 10)
                         * (_osCpu.Power / _osCpu.GoalPower)
                         * (_eadCpu.Power / _eadCpu.GoalPower)
                         * (_inputCpu.Power / _inputCpu.GoalPower);

        return _remainTime < TimeTol
            ? TimeSpan.Zero
            : _remainTime / _totalResPower;
    }

    public override void InitResources()
    {
        _server = (ActiveResources.Select(tuple => tuple.Resource).OfType<Server>().First(), 0.0);
        _cpuCluster = ActiveResources.Select(tuple => tuple.Resource).OfType<CpuCluster>().First();

        _osCpu.GoalPower = Convert.ToDouble(CurSettings.OsAvgThreadsCount *
                                            CurSettings.OsAvgOneThreadUtil *
                                            CurSettings.OsAvgOneThreadUtilReferenceClockRate);

        _eadCpu.GoalPower = Convert.ToDouble(CurSettings.MainEadAvgThreadsCount *
                                             CurSettings.MainEadAvgOneThreadUtil *
                                             CurSettings.MainEadOneThreadUtilReferenceClockRate);

        _inputCpu.GoalPower = 0.45 * 4;
    }

    private List<IResourceRequest> GetResourceRequestList() =>
        new()
        {
            new CpuRequest(_osCpuReqId, CurSettings.OsAvgThreadsCount,
                (Convert.ToDouble(CurSettings.OsAvgOneThreadUtil),
                    Convert.ToDouble(CurSettings.OsAvgOneThreadUtilReferenceClockRate))),

            new CpuRequest(_eadCpuReqId, CurSettings.MainEadAvgThreadsCount,
                (Convert.ToDouble(CurSettings.MainEadAvgOneThreadUtil),
                    Convert.ToDouble(CurSettings.MainEadOneThreadUtilReferenceClockRate))),

            new CpuRequest(_inputCpuReqId, 1, (0.30, 4.0)),

            new DesignerRequest(CommonResReqId),
            new ServerRequest(CommonResReqId),
            new RamRequest(CommonResReqId,
                Convert.ToDouble(CurSettings.OsAvgRamUsage + CurSettings.MainEadAvgRamUsage))
        };

    public override string Name => "Создание конструктива ПП и задание Правил:";
}