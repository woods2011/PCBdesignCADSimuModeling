using System;

namespace PCBdesignCADSimuModeling.Models.Technologies.PcbDesign.ProjectProcedures
{
    class DocumentationProduction : PcbDesignProcedure
    {
        public DocumentationProduction(PcbDesignTechnology context) : base(context)
        {
        }

        protected override void UpdateModelTimeBody(TimeSpan deltaTime)
        {
            throw new NotImplementedException();
        }

        protected override TimeSpan EstimateEndTime()
        {
            throw new NotImplementedException();
        }
    }
}