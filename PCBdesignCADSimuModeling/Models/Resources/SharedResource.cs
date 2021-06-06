using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PCBdesignCADSimuModeling.Annotations;

namespace PCBdesignCADSimuModeling.Models.Resources
{
    public abstract class SharedResource : IResource
    {
        protected readonly List<Guid> UtilizingProcIds = new();
        public abstract double ResValueForProc(Guid procId);

        public abstract void FreeResource(Guid procId);
        public abstract IResource Clone();
        public abstract double Cost { get; }
    }


    public class Server : SharedResource, INotifyPropertyChanged
    {
        private readonly Func<Server, double> _resValueConvolution;
        private int _internetSpeed;


        public Server(int internetSpeed, Func<Server, double> resValueConvolution = null)
        {
            InternetSpeed = internetSpeed;
            _resValueConvolution = resValueConvolution ?? (server => server.InternetSpeed);
        }


        public int InternetSpeed
        {
            get => _internetSpeed;
            set
            {
                if (value == _internetSpeed) return;
                _internetSpeed = value;
                OnPropertyChanged(nameof(Cost));
            }
        }


        public bool TryGetResource(Guid procId)
        {
            UtilizingProcIds.Add(procId);
            return true;
        }

        public override double ResValueForProc(Guid procId) => _resValueConvolution(this);

        public override void FreeResource(Guid procId) => UtilizingProcIds.Remove(procId);


        //


        public override IResource Clone() => new Server(this.InternetSpeed);

        public override double Cost => Math.Round(
            Math.Exp(1.0 + 125.0 / (InternetSpeed + 31.5)) * InternetSpeed * 3.5);

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}