﻿using System;

namespace PCBdesignCADSimuModeling.Models.Technologies.PcbDesign.ProjectProcedures
{
    public class DocumentationProduction : PcbDesignProcedure
    {
        public DocumentationProduction(PcbDesignTechnology context) : base(context)
        {
        }
        

        public override TimeSpan UpdateModelTime(TimeSpan deltaTime)
        {
            throw new NotImplementedException();
        }
    }
}