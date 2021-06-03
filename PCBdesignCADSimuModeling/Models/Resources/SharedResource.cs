using System;
using System.Collections.Generic;

namespace PCBdesignCADSimuModeling.Models.Resources
{
    public abstract class SharedResource : IResource
    {
        protected readonly List<Guid> UtilizingProcIds = new();
        public abstract double ResValueForProc(Guid procId);

        public abstract void FreeResource(Guid procId);
    }


    public class Server : SharedResource
    {
        private readonly Func<Server, double> _resValueConvolution;


        public Server(int internetSpeed, Func<Server, double> resValueConvolution = null)
        {
            InternetSpeed = internetSpeed;
            _resValueConvolution = resValueConvolution ?? (server => server.InternetSpeed);
        }


        public int InternetSpeed { get; set; }


        public bool TryGetResource(Guid procId)
        {
            UtilizingProcIds.Add(procId);
            return true;
        }

        public override double ResValueForProc(Guid procId) => _resValueConvolution(this);

        public override void FreeResource(Guid procId)
        {
            UtilizingProcIds.Remove(procId);
        }
    }
}