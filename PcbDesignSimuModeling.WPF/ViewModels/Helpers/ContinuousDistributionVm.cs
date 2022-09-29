using System;
using System.ComponentModel;
using MathNet.Numerics.Distributions;
using Newtonsoft.Json;
using PcbDesignSimuModeling.WPF.Models;

namespace PcbDesignSimuModeling.WPF.ViewModels.Helpers;

public abstract class ContinuousDistributionVm<T> : INotifyPropertyChanged
{
    private T _mean;
    private T _std;

    [JsonIgnore] public Random RndSource { get; set; }

    protected ContinuousDistributionVm(T mean, T std, Random rndSource)
    {
        _mean = mean;
        _std = std;
        RndSource = rndSource;
    }

    
    [JsonProperty("Мат Ожидание:")]
    public virtual T Mean
    {
        get => _mean;
        set => _mean = value;
    }
    
    [JsonProperty("СКО:")]
    public virtual T Std
    {
        get => _std;
        set => _std = value;
    }

    public abstract IContinuousDistribution Build();

    public event PropertyChangedEventHandler? PropertyChanged;
}

public abstract class TimeSpanContDistrVm : ContinuousDistributionVm<TimeSpan>
{
    protected TimeSpanContDistrVm(TimeSpan mean, TimeSpan std, Random rndSource) : base(mean, std,
        rndSource)
    {
    }

    public override TimeSpan Mean
    {
        get => base.Mean;
        set
        {
            base.Mean = value;
            if (AutoStdChange) Std = (Mean * 0.15).RoundSec();
        }
    }

    [JsonIgnore] public bool AutoStdChange { get; set; } = true;
}

public class TimeSpanNormalDistrVm : TimeSpanContDistrVm
{
    public TimeSpanNormalDistrVm(TimeSpan mean, TimeSpan std, Random? rndSource = null) : base(mean, std,
        rndSource ?? new Random(1))
    {
    }

    public override IContinuousDistribution Build() =>
        new Normal(Mean.TotalSeconds, Std.TotalSeconds, RndSource);
}

public class DblNormalDistrVm : ContinuousDistributionVm<double>
{
    public DblNormalDistrVm(double mean, double std, Random? rndSource = null) : base(mean, std,
        rndSource ?? new Random(1))
    {
    }

    public override double Mean
    {
        get => base.Mean;
        set
        {
            base.Mean = value;
            if (AutoStdChange) Std = base.Mean * 0.15;
        }
    }

    [JsonIgnore] public bool AutoStdChange { get; set; } = true;

    public override IContinuousDistribution Build() => new Normal(Mean, Std, RndSource);
}

