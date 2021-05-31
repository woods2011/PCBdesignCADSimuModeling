using System;
using System.Collections.Generic;
using PCBdesignCADSimuModeling.Models.Resources;

namespace PCBdesignCADSimuModeling.Models.Technologies.PcbDesign.ProjectProcedures
{
    public class PcbParamsInput : PcbDesignProcedure
    {
        private readonly PcbDesignTechnology _context;

        public PcbParamsInput(PcbDesignTechnology context) : base(context)
        {
            _context = context;
            
            Resources.Add(new Designer());
            Resources.Add(new Server()); //ToDo
        }


        public override bool NextProcedure()
        {
            _context.CurProcedure = new Placement(_context);
            return true;
        }

        public override TimeSpan UpdateModelTime(TimeSpan deltaTime)
        {
            throw new NotImplementedException();
        }
    }
}