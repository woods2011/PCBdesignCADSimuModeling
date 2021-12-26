using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace PcbDesignCADSimuModeling.Models.Resources
{
    public abstract class UndividedResource : IResource
    {
        protected int? UtilizingProcId;
        public abstract double ResValueForProc(int procId);

        public abstract void FreeResource(int procId);

        public abstract IResource Clone();
        public abstract double Cost { get; }
    }


    public class Designer : UndividedResource, INotifyPropertyChanged
    {
        public Designer()
        { 
        }

        public bool TryGetResource(int procId)
        {
            if (UtilizingProcId.HasValue) return false;

            UtilizingProcId = procId;
            return true;
        }

        public override double ResValueForProc(int _) => 1.0;

        public override void FreeResource(int procId) => UtilizingProcId = null;


        public override IResource Clone() => new Designer();

        public override double Cost => 60000;

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}