using System;
using System.Collections.Generic;
using PCBdesignCADSimuModeling.Models.Resources;

namespace PCBdesignCADSimuModeling.Models.Technologies.PcbDesign.ProjectProcedures
{
    public class QualityControl : PcbDesignProcedure
    {
        public QualityControl(PcbDesignTechnology context) : base(context)
        {
            RequiredResources.Add(new Designer());
        }


        public override bool NextProcedure()
        {
            if (true) //ToDo
                Context.CurProcedure = new DocumentationProduction(Context);
            else
                Context.CurProcedure = new Placement(Context);
            
            return true;
        }

        public override TimeSpan UpdateModelTime(TimeSpan deltaTime)
        {
            throw new NotImplementedException();
        }
    }
}