using MathNet.Numerics.Distributions;
using PcbDesignSimuModeling.Core.Models.Technologies.PcbDesign;

namespace PcbDesignSimuModeling.Core.Models.SimuSystem.SimulationEvents;

public interface ISimuEventGenerator
{
    List<SimulationEvent> GeneratePcbDesignTech(TimeSpan finalTime);
}


public class SimuEventGenerator : ISimuEventGenerator
{
    private readonly IContinuousDistribution _timeBetweenTechsDistr;
    private readonly IContinuousDistribution _pcbElemsCountDistr, _pcbDimUsagePctDistr;
    private readonly double _pcbElemsIsVarSizeProb;
    private readonly Random _random;

    public SimuEventGenerator(
        IContinuousDistribution timeBetweenTechsDistr,
        IContinuousDistribution pcbElemsCountDistr, IContinuousDistribution pcbDimUsagePctDistr,
        double pcbElemsIsVarSizeProb, Random random)
    {
        _timeBetweenTechsDistr = timeBetweenTechsDistr;
        _pcbElemsCountDistr = pcbElemsCountDistr;
        _pcbDimUsagePctDistr = pcbDimUsagePctDistr;
        _pcbElemsIsVarSizeProb = pcbElemsIsVarSizeProb;
        _random = random;
    }


    public List<SimulationEvent> GeneratePcbDesignTech(TimeSpan finalTime)
    {
        List<SimulationEvent> simulationEvents = new();
        var curTime = TimeSpan.FromSeconds(Math.Round(Math.Max(0.0, _timeBetweenTechsDistr.Sample())));
            
            
        while (curTime < finalTime)
        {
            simulationEvents.Add(new PcbDesignTechnologyStart(curTime, GeneratePcbParams()));

            curTime += TimeSpan.FromSeconds(
                Math.Round(Math.Max(0.0, _timeBetweenTechsDistr.Sample())));
        }

        return simulationEvents;
    }

    private PcbParams GeneratePcbParams()
    {
        return new PcbParams((int)Math.Round(Math.Max(0.0, _pcbElemsCountDistr.Sample())),
            _pcbDimUsagePctDistr.Sample(), _pcbElemsIsVarSizeProb >= _random.NextDouble());
    }
}