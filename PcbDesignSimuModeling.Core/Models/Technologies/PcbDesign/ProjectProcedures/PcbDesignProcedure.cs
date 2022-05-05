using PcbDesignSimuModeling.Core.Models.Resources;

namespace PcbDesignSimuModeling.Core.Models.Technologies.PcbDesign.ProjectProcedures;

public abstract class PcbDesignProcedure
{
    protected readonly PcbDesignTechnology Context;
    public int CommonResReqId { get; }

    protected PcbDesignProcedure(PcbDesignTechnology context)
    {
        Context = context;
        CommonResReqId = Context.NewRequestId;
    }


    public List<IResourceRequest> RequiredResources { get; } = new();
    public List<(IResource Resource, int RequestId)> ActiveResources { get; } = new();
    public List<IPotentialFailureResource> PotentialFailureResources { get; } = new();

    public abstract bool NextProcedure();
    public abstract void UpdateModelTime(TimeSpan deltaTime);
    public abstract TimeSpan EstimateEndTime();

    public abstract void InitResources();

    public abstract string Name { get; }

    public static readonly TimeSpan TimeTol = PcbDesignTechnology.TimeTol;
}