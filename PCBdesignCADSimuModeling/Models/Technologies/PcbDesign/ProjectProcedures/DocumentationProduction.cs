using System;
using PCBdesignCADSimuModeling.Models.Resources;

namespace PCBdesignCADSimuModeling.Models.Technologies.PcbDesign.ProjectProcedures
{
    public class DocumentationProduction : PcbDesignProcedure
    {
        private readonly PcbDesignTechnology _context;

        public DocumentationProduction(PcbDesignTechnology context) : base(context)
        {
            _context = context;
            
            Resources.Add(new Server()); //ToDo
        }


        public override bool NextProcedure()
        {
            _context.CurProcedure = null;
            return false;
        }

        public override TimeSpan UpdateModelTime(TimeSpan deltaTime)
        {
            throw new NotImplementedException();
        }
    }
}