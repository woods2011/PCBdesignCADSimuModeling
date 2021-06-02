using System;
using System.Collections.Generic;
using PCBdesignCADSimuModeling.Models.Resources;

namespace PCBdesignCADSimuModeling.Models.Technologies.PcbDesign.ProjectProcedures
{
    public class PcbParamsInput : PcbDesignProcedure
    {
        public PcbParamsInput(PcbDesignTechnology context) : base(context)
        {
            //RequiredResources.Add(new Designer());
            //RequiredResources.Add(new Server()); //ToDo
        }


        public override bool NextProcedure()
        {
            Context.CurProcedure = new Placement(Context);
            return true;
        }

        public override TimeSpan UpdateModelTime(TimeSpan deltaTime)
        {
            throw new NotImplementedException();
        }
    }
}