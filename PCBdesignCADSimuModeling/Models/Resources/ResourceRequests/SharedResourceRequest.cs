using System;
using System.Collections.Generic;

namespace PcbDesignCADSimuModeling.Models.Resources.ResourceRequests
{
    public abstract class SharedResourceRequest<TResource> : ResourceRequest<TResource>
        where TResource : SharedResource
    {
        protected SharedResourceRequest(int procId) : base(procId)
        {
        }
    }


    public class ServerRequest : SharedResourceRequest<Server>
    {
        public ServerRequest(int procId) : base(procId)
        {
        }


        protected override bool TryGetResourceBody(Server potentialResource) =>
            potentialResource.TryGetResource(ProcId);
    }
}