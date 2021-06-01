using System;
using System.Collections.Generic;
using PCBdesignCADSimuModeling.Models.Resources;
using PCBdesignCADSimuModeling.Models.Resources.Algorithms;
using PCBdesignCADSimuModeling.Models.Resources.Algorithms.WireRoutingAlgorithms;

namespace PCBdesignCADSimuModeling.Models.Technologies.PcbDesign.ProjectProcedures
{
    public class WireRouting : PcbDesignProcedure
    {
        private readonly IWireRoutingAlgorithm _wireRoutingAlg;
        
        public WireRouting(PcbDesignTechnology context) : base(context)
        {
            _wireRoutingAlg = context.PcbAlgFactories.WireRoutingAlgFactory.Create(context.PcbParams);
            
            RequiredResources.Add(new Designer());
            RequiredResources.AddRange(CpuThreads.CreateList(_wireRoutingAlg.MaxThreadUtilization)); //ToDo
        }

        public override bool NextProcedure()
        {
            Context.CurProcedure = new QualityControl(Context);
            return true;
        }

        public override TimeSpan UpdateModelTime(TimeSpan deltaTime)
        {
            throw new NotImplementedException();
            //_wireRoutingAlg.UpdateModelTime(deltaTime, );
        }
    }


}