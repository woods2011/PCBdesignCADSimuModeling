using MathNet.Numerics.Distributions;
using PcbDesignSimuModeling.Core.Models.Technologies.PcbDesign;

namespace PcbDesignSimuModeling.Core.Models.SimuSystem.SimulationEvents;

public interface IPcbGenerator
{
    PcbParams GeneratePcb();
}

public class PcbProbDistrBasedGenerator : IPcbGenerator
{
    private readonly IContinuousDistribution _pcbElemsCountDistr, _pcbAreaUsagePctDistr;
    private readonly Bernoulli _pcbElemsIsVarSize;

    public PcbProbDistrBasedGenerator(IContinuousDistribution pcbElemsCountDistr,
        IContinuousDistribution pcbAreaUsagePctDistr, Bernoulli pcbElemsIsVarSize)
    {
        _pcbElemsCountDistr = pcbElemsCountDistr;
        _pcbAreaUsagePctDistr = pcbAreaUsagePctDistr;
        _pcbElemsIsVarSize = pcbElemsIsVarSize;
    }

    public PcbParams GeneratePcb() => new
    (
        (int) Math.Round(Math.Max(0.0, _pcbElemsCountDistr.Sample())),
        _pcbAreaUsagePctDistr.Sample(),
        _pcbElemsIsVarSize.Sample() == 1
    );
}

public class PcbPoolBasedGenerator : IPcbGenerator
{
    private readonly Random _rndSource;
    private readonly IReadOnlyList<PcbParams> _pcbPool;

    public PcbPoolBasedGenerator(IEnumerable<PcbParams> pcbPool, Random rndSource)
    {
        _pcbPool = pcbPool.ToList();
        _rndSource = rndSource;
    }

    public PcbParams GeneratePcb() => _pcbPool[_rndSource.Next(0, _pcbPool.Count)];
}