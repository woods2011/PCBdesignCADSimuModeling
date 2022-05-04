﻿using PcbDesignSimuModeling.Core.Models.Resources;
using PcbDesignSimuModeling.Core.Models.Resources.Designer;
using PcbDesignSimuModeling.Core.Models.Resources.Server;

namespace PcbDesignSimuModeling.Core.Models.Technologies.PcbDesign.ProjectProcedures;

public class PcbParamsInput : PcbDesignProcedure
{
    private double _designerPower;
    private double _serverPower;

    public PcbParamsInput(PcbDesignTechnology context) : base(context) =>
        RequiredResources.AddRange(GetResourceRequestList());


    public override bool NextProcedure()
    {
        Context.CurProcedure = new Placement(Context);
        return true;
    }

    private TimeSpan _remainTime = TimeSpan.FromDays(1);

    public override void UpdateModelTime(TimeSpan deltaTime)
    {
        _remainTime -= deltaTime * _designerPower * _serverPower;
    }

    public override TimeSpan EstimateEndTime() =>
        _remainTime < TimeTol ? TimeSpan.Zero : _remainTime / _designerPower / _serverPower;

    public override void InitResourcesPower()
    {
        _designerPower = ActiveResources.Select(tuple => tuple.Resource).OfType<Designer>().Sum(resource => resource.PowerForRequest(CommonResReqId));
        var serverPower = ActiveResources.Select(tuple => tuple.Resource).OfType<Server>().Sum(resource => resource.PowerForRequest(CommonResReqId));
        _serverPower = 0.5 + 0.5 * (1.0 / Math.Exp(30.0 / serverPower));
    }

    private List<IResourceRequest> GetResourceRequestList() => new()
    {
        new DesignerRequest(CommonResReqId),
        new ServerRequest(CommonResReqId)
    };

    public override string Name => "Ввод описания";
}