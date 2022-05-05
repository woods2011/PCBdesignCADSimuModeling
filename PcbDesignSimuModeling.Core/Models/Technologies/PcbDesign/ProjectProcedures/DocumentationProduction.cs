using PcbDesignSimuModeling.Core.Models.Resources;
using PcbDesignSimuModeling.Core.Models.Resources.Designer;
using PcbDesignSimuModeling.Core.Models.Resources.Ram;
using PcbDesignSimuModeling.Core.Models.Resources.Server;

namespace PcbDesignSimuModeling.Core.Models.Technologies.PcbDesign.ProjectProcedures;

public class DocumentationProduction : PcbDesignProcedure
{
    private TimeSpan _remainTime = TimeSpan.FromDays(0.5);
    private (Server Server, double Power) _server;

    public DocumentationProduction(PcbDesignTechnology context) : base(context) =>
        RequiredResources.AddRange(GetResourceRequestList());


    public override bool NextProcedure() => false;

    public override void UpdateModelTime(TimeSpan deltaTime) =>
        _remainTime -= deltaTime * _server.Power;

    public override TimeSpan EstimateEndTime()
    {
        _server.Power = 0.6 + 0.4 * (1.0 / Math.Exp(30.0 / _server.Server.PowerForRequest(CommonResReqId)));
        return _remainTime < TimeTol ? TimeSpan.Zero : _remainTime / _server.Power;
    }

    public override void InitResources() =>
        _server = (ActiveResources.Select(tuple => tuple.Resource).OfType<Server>().First(), 0.0);
    
    private List<IResourceRequest> GetResourceRequestList() => new()
    {
        new DesignerRequest(CommonResReqId),
        new ServerRequest(CommonResReqId),
        new RamRequest(CommonResReqId, 1.5)
    };

    public override string Name => "Выпуск документации";
}