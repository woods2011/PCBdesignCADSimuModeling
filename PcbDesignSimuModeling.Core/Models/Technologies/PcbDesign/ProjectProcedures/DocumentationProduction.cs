using PcbDesignSimuModeling.Core.Models.Resources;
using PcbDesignSimuModeling.Core.Models.Resources.ResourceRequests;

namespace PcbDesignSimuModeling.Core.Models.Technologies.PcbDesign.ProjectProcedures;

public class DocumentationProduction : PcbDesignProcedure
{
    private TimeSpan _remainTime = TimeSpan.FromDays(0.5);
    private double _designerPower;
    private double _serverPower;

    public DocumentationProduction(PcbDesignTechnology context) : base(context) =>
        RequiredResources.AddRange(GetResourceRequestList());


    public override bool NextProcedure() => false;

    public override void UpdateModelTime(TimeSpan deltaTime) =>
        _remainTime -= deltaTime * _designerPower * _serverPower;

    public override TimeSpan EstimateEndTime() =>
        _remainTime < TimeTol ? TimeSpan.Zero : _remainTime / _designerPower / _serverPower;

    public override void InitResourcesPower()
    {
        _designerPower = ActiveResources.OfType<Designer>().Sum(resource => resource.ResValueForProc(ProcId));
        var serverPower = ActiveResources.OfType<Server>().Sum(resource => resource.ResValueForProc(ProcId));
        _serverPower = 0.6 + 0.4 * (1.0 / Math.Exp(30.0 / serverPower));
    }

    private List<IResourceRequest> GetResourceRequestList() => new()
    {
        new DesignerRequest(ProcId),
        new ServerRequest(ProcId)
    };

    public override string Name => "Выпуск документации";
}