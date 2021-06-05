using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MathNet.Numerics.Distributions;

namespace PCBdesignCADSimuModeling.ViewModels
{
    public abstract class DistributionBuilderDisplayModel<T> : INotifyPropertyChanged
    {
        private T _mean;
        private T _std;

        protected DistributionBuilderDisplayModel(T mean, T std)
        {
            _mean = mean;
            _std = std;
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


        public abstract IContinuousDistribution Build(Func<double, double, IContinuousDistribution> func = null);


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    
    public class DistributionBuilderDisplayModelTs : DistributionBuilderDisplayModel<TimeSpan>
    {
        public DistributionBuilderDisplayModelTs(TimeSpan mean, TimeSpan std) : base(mean, std)
        {
        }

        
        public override TimeSpan Mean
        {
            get => base.Mean;
            set
            {
                if (value.Equals(base.Mean)) return;
                base.Mean = value;
                OnPropertyChanged();
                Std = TimeSpan.FromMinutes(Math.Round(base.Mean.TotalMinutes * 0.15));
            }
        }

        public override TimeSpan Std
        {
            get => base.Std;
            set
            {
                if (value.Equals(base.Std)) return;
                base.Std = value;
                OnPropertyChanged();
            }
        }
        
        
        public override IContinuousDistribution Build(Func<double, double, IContinuousDistribution> func = null)
            => func?.Invoke(Mean.TotalSeconds, Std.TotalSeconds) ?? new Normal(Mean.TotalSeconds, Std.TotalSeconds);
    }

    
    public class TechIntervalBuilderDisplayModel : DistributionBuilderDisplayModelTs
    {
        public TechIntervalBuilderDisplayModel(TimeSpan mean, TimeSpan std) : base(mean, std)
        {
        }
        

        public override TimeSpan Mean
        {
            get => base.Mean;
            set
            {
                base.Mean = value;
                OnPropertyChanged(nameof(TechPerYear));
            }
        }
        
        public double TechPerYear => Math.Round(TimeSpan.FromDays(366.65) / Mean, 1) ;
    }
    
    
    public class DistributionBuilderDisplayModelDbl : DistributionBuilderDisplayModel<double>
    {
        public DistributionBuilderDisplayModelDbl(double mean, double std) : base(mean, std)
        {
        }

        
        public override double Mean
        {
            get => base.Mean;
            set
            {
                if (value.Equals(base.Mean)) return;
                base.Mean = value;
                OnPropertyChanged();
                Std = base.Mean * 0.15;
            }
        }

        public override double Std
        {
            get => base.Std;
            set
            {
                if (value.Equals(base.Std)) return;
                base.Std = value;
                OnPropertyChanged();
            }
        }
        
        
        public override IContinuousDistribution Build(Func<double, double, IContinuousDistribution> func = null)
            => func?.Invoke(Mean, Std) ?? new Normal(Mean, Std);
    }
}