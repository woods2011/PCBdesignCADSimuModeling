using PcbDesignSimuModeling.Core.Models.Resources;
using PcbDesignSimuModeling.Core.Models.Resources.Designer;

namespace PcbDesignSimuModeling.Core.Models.Technologies.PcbDesign.ProjectProcedures;

public class QualityControl : PcbDesignProcedure
{
    private double _designerPower;

    public QualityControl(PcbDesignTechnology context) : base(context) =>
        RequiredResources.AddRange(GetResourceRequestList());


    public override bool NextProcedure()
    {
        Context.CurProcedure = new DocumentationProduction(Context);
        return true;
    }

    private TimeSpan _remainTime = TimeSpan.FromDays(0.5);

    public override void UpdateModelTime(TimeSpan deltaTime) => _remainTime -= deltaTime * _designerPower;

    public override TimeSpan EstimateEndTime() =>
        _remainTime < TimeTol ? TimeSpan.Zero : _remainTime / _designerPower;

    public override void InitResourcesPower() => _designerPower =
        ActiveResources.Select(tuple => tuple.Resource).OfType<Designer>().Sum(resource => resource.PowerForRequest(CommonResReqId));

    private List<IResourceRequest> GetResourceRequestList() => new()
    {
        new DesignerRequest(CommonResReqId)
    };

    public override string Name => "Оценка качества";
}