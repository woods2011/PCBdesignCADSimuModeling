using System;
using System.Collections.Generic;

namespace PCBdesignCADSimuModeling.Models.Resources
{
    public abstract class SharedResource : Resource
    {
        protected readonly List<Guid> UtilizingProcIds = new();
    }


    public class Server : SharedResource
    {
        private readonly Func<Server, double> _resValueConvolution;


        public Server(double internetSpeed, Func<Server, double> resValueConvolution = null)
        {
            InternetSpeed = internetSpeed;
            _resValueConvolution = resValueConvolution ?? (server => server.InternetSpeed);
        }


        public double InternetSpeed { get; }


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