using System;
using System.Collections.Generic;
using PCBdesignCADSimuModeling.Models.Resources;
using PCBdesignCADSimuModeling.Models.Resources.Algorithms;
using PCBdesignCADSimuModeling.Models.Resources.Algorithms.WireRoutingAlgorithms;

namespace PCBdesignCADSimuModeling.Models.Technologies.PcbDesign.ProjectProcedures
{
    public class WireRouting : PcbDesignProcedure
    {
        private readonly PcbDesignTechnology _context;
        private readonly IWireRoutingAlgorithm _wireRoutingAlg;
        
        public WireRouting(PcbDesignTechnology context) : base(context)
        {
            _context = context;
            _wireRoutingAlg = context.PcbAlgFactories.WireRoutingAlgFactory.Create(context.PcbParams);
            
            Resources.Add(new Designer());
            Resources.AddRange(CpuThread.CreateList(_wireRoutingAlg.MaxThreadUtilization)); //ToDo
        }

        public override bool NextProcedure()
        {
            _context.CurProcedure = new QualityControl(_context);
            return true;
        }

        public override TimeSpan UpdateModelTime(TimeSpan deltaTime)
        {
            throw new NotImplementedException();
            //_wireRoutingAlg.UpdateModelTime(deltaTime, );
        }
    }


}