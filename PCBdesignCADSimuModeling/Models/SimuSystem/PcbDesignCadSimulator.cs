using System;
using PCBdesignCADSimuModeling.Models.Resources;
using PCBdesignCADSimuModeling.Models.Resources.Algorithms;
using PCBdesignCADSimuModeling.Models.Technologies.PcbDesign;
using PCBdesignCADSimuModeling.Models.Technologies.PcbDesign.ProjectProcedures;

namespace PCBdesignCADSimuModeling.Models.SimuSystem
{
    public class PcbDesignCadSimulator
    {
        private TimeSpan _modelTime = TimeSpan.Zero;
        private readonly PcbAlgFactories _pcbAlgFactories;
        private readonly IResourceManager _resourceManager = new ResourceManager();

        public PcbDesignCadSimulator(PcbAlgFactories pcbAlgFactories)
        {
            _pcbAlgFactories = pcbAlgFactories;
        }


        public void Simulate()
        {
            PcbParams pcbParams = null;
            
            throw new NotImplementedException();
            
            _ = new PcbDesignTechnology(_resourceManager, pcbParams, _pcbAlgFactories);
        }
    }
}