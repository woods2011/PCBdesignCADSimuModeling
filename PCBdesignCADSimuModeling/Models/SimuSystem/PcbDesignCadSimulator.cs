using System;
using PCBdesignCADSimuModeling.Models.Resources;
using PCBdesignCADSimuModeling.Models.Resources.Algorithms;
using PCBdesignCADSimuModeling.Models.Technologies.PcbDesign;

namespace PCBdesignCADSimuModeling.Models.SimuSystem
{
    public class PcbDesignCadSimulator<TPcbInfo> where TPcbInfo : class 
    {
        private TimeSpan _modelTime = TimeSpan.Zero;
        private readonly IPcbAlgorithmFactory<TPcbInfo> _pcbAlgFactory;

        public PcbDesignCadSimulator(IPcbAlgorithmFactoryHoldPcbInfo pcbAlgorithmFactoryHoldPcbInfo, IPcbAlgorithmFactory<TPcbInfo> pcbAlgFactory)
        {
            _pcbAlgFactory = pcbAlgFactory;
        }


        public void Simulate()
        {

            TPcbInfo pcbInfo = null;
            _ = new PcbDesignTechnology(new ResourceManager(), new PcbAlgorithmFactoryHoldPcbInfo<TPcbInfo>(pcbInfo, _pcbAlgFactory));
        }
    }
}