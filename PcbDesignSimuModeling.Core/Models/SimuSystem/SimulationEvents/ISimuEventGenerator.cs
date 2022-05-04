using MathNet.Numerics.Distributions;
using PcbDesignSimuModeling.Core.Models.Resources;
using PcbDesignSimuModeling.Core.Models.Resources.Designer;
using PcbDesignSimuModeling.Core.Models.Technologies.PcbDesign;

namespace PcbDesignSimuModeling.Core.Models.SimuSystem.SimulationEvents;

public interface ISimuEventGenerator
{
    List<SimulationEvent> GenerateSimuEvent(TimeSpan finalTime);
}

public class PcbDesignTechGenerator : ISimuEventGenerator
{
    private readonly Random _rndSource;

    private readonly IPcbGenerator _pcbGenerator;
    private readonly IContinuousDistribution _requestsFlowDistrDays;

    public PcbDesignTechGenerator(IContinuousDistribution requestsFlowDistrDays, IPcbGenerator pcbGenerator,
        Random rndSource)
    {
        _requestsFlowDistrDays = requestsFlowDistrDays;
        _pcbGenerator = pcbGenerator;
        _rndSource = rndSource;
    }


    public List<SimulationEvent> GenerateSimuEvent(TimeSpan finalTime) =>
        new(_requestsFlowDistrDays.Samples()
            .CumulativeSum()
            .Select(TimeSpan.FromDays)
            .TakeWhile(curTime => curTime < finalTime)
            .Select(curTime => new PcbDesignTechnologyStart(curTime, _pcbGenerator.GeneratePcb())));
}

public class ResourceFailureGenerator : ISimuEventGenerator
{
    private readonly Random _rndSource;

    private readonly IReadOnlyList<IResource> _resources;
    private readonly IContinuousDistribution _illnessFlowDistrDays;

    public ResourceFailureGenerator(IContinuousDistribution illnessFlowDistrDays, IReadOnlyList<IResource> resources)
    {
        _resources = resources;
        _illnessFlowDistrDays = illnessFlowDistrDays;
        _rndSource = new Random(1);
    }

    public ResourceFailureGenerator(IReadOnlyList<IResource> resources, Random rndSource)
    {
        _resources = resources;
        _rndSource = rndSource;
        _illnessFlowDistrDays = new Exponential(2.0 / 365.0, _rndSource);
    }


    public List<SimulationEvent> GenerateSimuEvent(TimeSpan finalTime)
    {
        var result = new List<SimulationEvent>();

        foreach (var designer in _resources.OfType<Designer>())
        {
            var designerFailureTimes = _illnessFlowDistrDays.Samples().CumulativeSum()
                .Select(TimeSpan.FromDays)
                .TakeWhile(curTime => curTime < finalTime).ToList();

            foreach (var designerFailureTime in designerFailureTimes)
            {
                result.Add(new ResourceFailure(designerFailureTime, designer));
                result.Add(new ResourceRestored(
                    designerFailureTime + TimeSpan.FromDays(4 + Normal.Sample(_rndSource, 0.0, 1.0)), designer));
            }
        }

        return result;
    }
}