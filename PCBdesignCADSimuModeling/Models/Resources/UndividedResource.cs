using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using PCBdesignCADSimuModeling.Annotations;

namespace PCBdesignCADSimuModeling.Models.Resources
{
    public abstract class UndividedResource : IResource
    {
        protected Guid UtilizingProcId = Guid.Empty;
        public abstract double ResValueForProc(Guid procId);

        public abstract void FreeResource(Guid procId);

        public abstract IResource Clone();
        public abstract double Cost { get; }
    }


    public class Designer : UndividedResource, INotifyPropertyChanged
    {
        private readonly Func<Designer, double> _resValueConvolution;
        private ExperienceEn _experience;


        public Designer(ExperienceEn experience, Func<Designer, double> resValueConvolution = null)
        {
            Experience = experience;
            _resValueConvolution = resValueConvolution ?? (designer => (double) designer.Experience);
        }


        public ExperienceEn Experience
        {
            get => _experience;
            set
            {
                if (value == _experience) return;
                _experience = value;
                OnPropertyChanged(nameof(Cost));
            }
        }


        public bool TryGetResource(Guid procId)
        {
            if (UtilizingProcId != Guid.Empty) return false;

            UtilizingProcId = procId;
            return true;
        }

        public override double ResValueForProc(Guid procId) => _resValueConvolution(this);

        public override void FreeResource(Guid procId) => UtilizingProcId = Guid.Empty;


        //


        public override IResource Clone() => new Designer(this.Experience);

        public override double Cost => Math.Round(
            (0.5 + ((double) Experience - 0.5) / (double) Enum.GetValues<Designer.ExperienceEn>().Max()) * 53000.0);


        public enum ExperienceEn
        {
            Little = 1,
            Average = 2,
            Extensive = 3
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}