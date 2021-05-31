using System;
using System.Collections.Generic;
using PCBdesignCADSimuModeling.Models.Resources;

namespace PCBdesignCADSimuModeling.Models.Technologies.PcbDesign.ProjectProcedures
{
    public class QualityControl : PcbDesignProcedure
    {
        private readonly PcbDesignTechnology _context;

        public QualityControl(PcbDesignTechnology context) : base(context)
        {
            _context = context;
            
            Resources.Add(new Designer());
        }


        public override bool NextProcedure()
        {
            if (true) //ToDo
                _context.CurProcedure = new DocumentationProduction(_context);
            else
                _context.CurProcedure = new Placement(_context);
            
            return true;
        }

        public override TimeSpan UpdateModelTime(TimeSpan deltaTime)
        {
            throw new NotImplementedException();
        }
    }
}