using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Distributions;
using PcbDesignSimuModeling.WPF.Models.Technologies.PcbDesign;

namespace PcbDesignSimuModeling.WPF.Models.SimuSystem.SimulationEvents;

public interface IPcbGenerator
{
    PcbDescription GeneratePcb();
}

public class PcbProbDistrBasedGenerator : IPcbGenerator
{
    private readonly IContinuousDistribution
        _pcbElemsCountDistr,
        _newElemsPercentDistr,
        _elementsDensityDistr,
        _pinsCountDistr,
        _pinDensityDistr,
        _numOfLayersDistr;

    private readonly Bernoulli _isDoubleSidePlacement;
    private readonly Bernoulli _isManualSchemeInputProb;

    public PcbProbDistrBasedGenerator(IContinuousDistribution pcbElemsCountDistr,
        IContinuousDistribution newElemsPercentDistr, IContinuousDistribution elementsDensityDistr,
        IContinuousDistribution pinsCountDistr, IContinuousDistribution pinDensityDistr,
        IContinuousDistribution numOfLayersDistr, Bernoulli isDoubleSidePlacement, Bernoulli isManualSchemeInputProb)
    {
        _pcbElemsCountDistr = pcbElemsCountDistr;
        _newElemsPercentDistr = newElemsPercentDistr;
        _elementsDensityDistr = elementsDensityDistr;
        _pinsCountDistr = pinsCountDistr;
        _pinDensityDistr = pinDensityDistr;
        _numOfLayersDistr = numOfLayersDistr;
        _isDoubleSidePlacement = isDoubleSidePlacement;
        _isManualSchemeInputProb = isManualSchemeInputProb;
    }

    public PcbDescription GeneratePcb() => new(
        elementsCount: Convert.ToInt32(Math.Max(20.0, _pcbElemsCountDistr.Sample())),
        newElemsPercent: Math.Clamp(_newElemsPercentDistr.Sample(), 0.0, 1.0),
        elementsDensity: Math.Max(0.05, _elementsDensityDistr.Sample()), // ToDo 0.1
        pinsCount: Convert.ToInt32(Math.Max(80.0, _pinsCountDistr.Sample())),
        pinDensity: Math.Max(0.01, _pinDensityDistr.Sample()),
        numberOfLayers: Convert.ToInt32(Math.Max(1.0, _numOfLayersDistr.Sample())),
        isDoubleSidePlacement: _isDoubleSidePlacement.Sample() == 1,
        isManualSchemeInput: _isManualSchemeInputProb.Sample() == 1,
        classAccuracy: 3
    );
}

public class PcbPoolBasedGenerator : IPcbGenerator
{
    private readonly Random _rndSource;
    private readonly IReadOnlyList<PcbDescription> _pcbPool;

    public PcbPoolBasedGenerator(IEnumerable<PcbDescription> pcbPool, Random rndSource)
    {
        _pcbPool = pcbPool.ToList();
        _rndSource = rndSource;
    }

    public PcbDescription GeneratePcb() => _pcbPool[_rndSource.Next(0, _pcbPool.Count)];
}