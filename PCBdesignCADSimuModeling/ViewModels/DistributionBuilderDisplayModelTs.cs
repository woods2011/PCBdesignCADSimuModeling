using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MathNet.Numerics.Distributions;

namespace PcbDesignCADSimuModeling.ViewModels
{
    public abstract class DistributionBuilderVm<T> : INotifyPropertyChanged
    {
        protected readonly Random RndSource;
        private T _mean;
        private T _std;

        protected DistributionBuilderVm(T mean, T std, Random rndSource)
        {
            _mean = mean;
            _std = std;
            RndSource = rndSource;
        }

        public virtual T Mean
        {
            get => _mean;
            set => _mean = value;
        }

        public virtual T Std
        {
            get => _std;
            set => _std = value;
        }

        public abstract IContinuousDistribution Build();


        public event PropertyChangedEventHandler? PropertyChanged;
    }


    public abstract class TimeSpanDistributionBuilderVm : DistributionBuilderVm<TimeSpan>
    {
        protected TimeSpanDistributionBuilderVm(TimeSpan mean, TimeSpan std, Random rndSource) : base(mean, std,
            rndSource)
        {
        }

        public override TimeSpan Mean
        {
            get => base.Mean;
            set
            {
                base.Mean = value;
                if (AutoStdChange) Std = TimeSpan.FromMinutes(Math.Round(base.Mean.TotalMinutes * 0.15));
            }
        }

        public bool AutoStdChange { get; set; } = true;
    }

    public class TimeSpanNormalDistributionBuilderVm : TimeSpanDistributionBuilderVm
    {
        public TimeSpanNormalDistributionBuilderVm(TimeSpan mean, TimeSpan std, Random rndSource) : base(mean, std,
            rndSource)
        {
        }

        public override IContinuousDistribution Build() =>
            new Normal(Mean.TotalSeconds, Std.TotalSeconds, RndSource);
    }


    public class TechIntervalBuilderVm : TimeSpanNormalDistributionBuilderVm
    {
        public TechIntervalBuilderVm(TimeSpan mean, TimeSpan std, Random rndSource) : base(mean, std, rndSource)
        {
            TechPerYear = Math.Round(TimeSpan.FromDays(366.65) / mean, 1);
        }

        public override TimeSpan Mean
        {
            get => base.Mean;
            set
            {
                base.Mean = value;
                TechPerYear = Math.Round(TimeSpan.FromDays(366.65) / Mean, 1);
            }
        }

        public double TechPerYear { get; set; }
    }


    public class DblNormalDistributionBuilderVm : DistributionBuilderVm<double>
    {
        public DblNormalDistributionBuilderVm(double mean, double std, Random rndSource) : base(mean, std, rndSource)
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

        public bool AutoStdChange { get; set; } = true;

        public override IContinuousDistribution Build() => new Normal(Mean, Std, RndSource);
    }
}