using System;
using PCBdesignCADSimuModeling.Models.Resources;

namespace PCBdesignCADSimuModeling.Models.Technologies.PcbDesign.ProjectProcedures
{
    public class DocumentationProduction : PcbDesignProcedure
    {
        public DocumentationProduction(PcbDesignTechnology context) : base(context)
        {
            //RequiredResources.Add(new Server()); //ToDo
        }


        public override bool NextProcedure()
        {
            Context.CurProcedure = null;
            return false;
        }

        public override TimeSpan UpdateModelTime(TimeSpan deltaTime)
        {
            throw new NotImplementedException();
        }
    }
}