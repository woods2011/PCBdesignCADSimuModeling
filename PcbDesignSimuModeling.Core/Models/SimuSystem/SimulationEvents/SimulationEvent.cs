using PcbDesignSimuModeling.Core.Models.Resources;

namespace PcbDesignSimuModeling.Core.Models.SimuSystem.SimulationEvents;

public abstract class SimulationEvent
{
    protected SimulationEvent(TimeSpan activateTime) => ActivateTime = activateTime;

    public TimeSpan ActivateTime { get; }
}

public class ResourceFailure : SimulationEvent
{
    public ResourceFailure(TimeSpan activateTime, IPotentialFailureResource resource) : base(activateTime) => Resource = resource;

    public IPotentialFailureResource Resource { get; }
}

public class ResourceRestored : SimulationEvent
{
    public ResourceRestored(TimeSpan activateTime, IPotentialFailureResource resource) : base(activateTime) => Resource = resource;

    public IPotentialFailureResource Resource { get; }
}