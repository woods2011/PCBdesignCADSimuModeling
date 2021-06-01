using System;
using System.Collections.Generic;
using PCBdesignCADSimuModeling.Models.Resources;
using PCBdesignCADSimuModeling.Models.Resources.Algorithms;
using PCBdesignCADSimuModeling.Models.Resources.Algorithms.PlacingAlgorithms;

namespace PCBdesignCADSimuModeling.Models.Technologies.PcbDesign.ProjectProcedures
{
    public class Placement : PcbDesignProcedure
    {
        private readonly IPlacingAlgorithm _placingAlg;
        
        public Placement(PcbDesignTechnology context) : base(context)
        {
            _placingAlg = context.PcbAlgFactories.PlacingAlgFactory.Create(context.PcbParams);

            RequiredResources.Add(new Designer());
            RequiredResources.AddRange(CpuThreads.CreateList(_placingAlg.MaxThreadUtilization)); //ToDo
        }

        public override bool NextProcedure()
        {
            Context.CurProcedure = new WireRouting(Context);
            return true;
        }

        public override TimeSpan UpdateModelTime(TimeSpan deltaTime)
        {
            throw new NotImplementedException();
            //_placingAlg.UpdateModelTime(deltaTime, );
        }
    }
}