using PcbDesignSimuModeling.Core.Models.Resources;
using PcbDesignSimuModeling.Core.Models.Resources.ResourceRequests;

namespace PcbDesignSimuModeling.Core.Models.Technologies.PcbDesign.ProjectProcedures;

public abstract class PcbDesignProcedure
{
    protected readonly PcbDesignTechnology Context;
    public int ProcId { get; } = NewId;

        
    protected PcbDesignProcedure(PcbDesignTechnology context)
    {
        Context = context;
    }

        
    public List<IResourceRequest> RequiredResources { get; } = new();
    public List<IResource> ActiveResources { get; } = new();

    public abstract bool NextProcedure();
    public abstract void UpdateModelTime(TimeSpan deltaTime);
    public abstract TimeSpan EstimateEndTime();

    public abstract void InitResourcesPower();

    public abstract string Name { get; }

    private static int _newId;
    public static int NewId => ++_newId;

    public static readonly TimeSpan TimeTol = PcbDesignTechnology.TimeTol;
}