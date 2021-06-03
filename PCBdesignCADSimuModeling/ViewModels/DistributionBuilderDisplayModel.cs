using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MathNet.Numerics.Distributions;

namespace PCBdesignCADSimuModeling.ViewModels
{
    public class DistributionBuilderDisplayModel : INotifyPropertyChanged
    {
        private TimeSpan _std = TimeSpan.Zero;
        private TimeSpan _mean;
        private readonly bool _meanOnly = false;


        public DistributionBuilderDisplayModel(TimeSpan mean, bool meanOnly = true)
        {
            _meanOnly = meanOnly;
            Mean = mean;
            Std = TimeSpan.Zero;
        }
        
        public DistributionBuilderDisplayModel(TimeSpan mean, TimeSpan std)
        {
            Mean = mean;
            Std = std;
        }

        
        public virtual TimeSpan Mean
        {
            get => _mean;
            set
            {
                if (value.Equals(_mean)) return;
                _mean = value;
                OnPropertyChanged();

                if (!_meanOnly)
                    Std = TimeSpan.FromMinutes(Math.Round(_mean.TotalMinutes * 0.15));
            }
        }

        public TimeSpan Std
        {
            get => _std;
            set
            {
                if (value.Equals(_std)) return;
                _std = value;
                OnPropertyChanged();
            }
        }
        
        public IContinuousDistribution Build(Func<double, double, IContinuousDistribution> func = null)
            => func?.Invoke(Mean.TotalSeconds, Std.TotalSeconds) ?? new Normal(Mean.TotalSeconds, Std.TotalSeconds);


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class TechIntervalBuilderDisplayModel : DistributionBuilderDisplayModel
    {
        public TechIntervalBuilderDisplayModel(TimeSpan mean, bool meanOnly = true) : base(mean, meanOnly)
        {
        }

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
}