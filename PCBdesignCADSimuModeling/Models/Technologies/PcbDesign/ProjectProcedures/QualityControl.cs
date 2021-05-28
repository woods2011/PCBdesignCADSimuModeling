using System;
using System.Collections.Generic;
using PCBdesignCADSimuModeling.Models.Resources;

namespace PCBdesignCADSimuModeling.Models.Technologies.PcbDesign.ProjectProcedures
{
    class QualityControl : PcbDesignProcedure
    {
        public QualityControl(PcbDesignTechnology context) : base(context)
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