using PcbDesignSimuModeling.Core.Models.Resources;
using PcbDesignSimuModeling.Core.Models.Resources.Designer;
using PcbDesignSimuModeling.Core.Models.Resources.Ram;

namespace PcbDesignSimuModeling.Core.Models.Technologies.PcbDesign.ProjectProcedures;

public class QualityControl : PcbDesignProcedure
{
    private TimeSpan _remainTime = TimeSpan.FromDays(0.5);
    
    public QualityControl(PcbDesignTechnology context) : base(context) =>
        RequiredResources.AddRange(GetResourceRequestList());


    public override bool NextProcedure()
    {
        Context.CurProcedure = new DocumentationProduction(Context);
        return true;
    }

    public override void UpdateModelTime(TimeSpan deltaTime) => _remainTime -= deltaTime;

    public override TimeSpan EstimateEndTime() =>
        _remainTime < TimeTol ? TimeSpan.Zero : _remainTime;

    public override void InitResources()
    {
    }

    private List<IResourceRequest> GetResourceRequestList() => new()
    {
        new DesignerRequest(CommonResReqId),
        new RamRequest(CommonResReqId, 1.5)
    };

    public override string Name => "Оценка качества";
}