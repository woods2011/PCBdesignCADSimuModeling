using System;
using System.Collections.Generic;

namespace PCBdesignCADSimuModeling.Models.Resources.ResourceRequests
{
    public abstract class SharedResourceRequest<TResource> : ResourceRequest<TResource>
        where TResource : SharedResource
    {
        protected SharedResourceRequest(Guid procId) : base(procId)
        {
        }
    }


    public class ServerRequest : SharedResourceRequest<Server>
    {
        public ServerRequest(Guid procId) : base(procId)
        {
        }


        protected override bool TryGetResourceBody(Server potentialResource) =>
            potentialResource.TryGetResource(ProcId);
    }
}