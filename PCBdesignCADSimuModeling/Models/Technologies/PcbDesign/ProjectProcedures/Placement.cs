using System;
using System.Collections.Generic;
using PCBdesignCADSimuModeling.Models.Resources;
using PCBdesignCADSimuModeling.Models.Resources.Algorithms;
using PCBdesignCADSimuModeling.Models.Resources.Algorithms.PlacingAlgorithms;

namespace PCBdesignCADSimuModeling.Models.Technologies.PcbDesign.ProjectProcedures
{
    public class Placement : PcbDesignProcedure
    {
        private readonly PcbDesignTechnology _context;
        private readonly IPlacingAlgorithm _placingAlg;
        
        public Placement(PcbDesignTechnology context) : base(context)
        {
            _context = context;
            _placingAlg = context.PcbAlgFactories.PlacingAlgFactory.Create(context.PcbParams);

            Resources.Add(new Designer());
            Resources.AddRange(CpuThread.CreateList(_placingAlg.MaxThreadUtilization)); //ToDo
        }

        public override bool NextProcedure()
        {
            _context.CurProcedure = new WireRouting(_context);
            return true;
        }

        public override TimeSpan UpdateModelTime(TimeSpan deltaTime)
        {
            throw new NotImplementedException();
            //_placingAlg.UpdateModelTime(deltaTime, );
        }
    }
}