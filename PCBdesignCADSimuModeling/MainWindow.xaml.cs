using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PCBdesignCADSimuModeling.Models.Resources.Algorithms;
using PCBdesignCADSimuModeling.Models.Resources.Algorithms.PlacingAlgorithms;
using PCBdesignCADSimuModeling.Models.Resources.Algorithms.WireRoutingAlgorithms;
using PCBdesignCADSimuModeling.Models.SimuSystem;
using PCBdesignCADSimuModeling.Models.Technologies.PcbDesign;

namespace PCBdesignCADSimuModeling
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();


            IPcbAlgorithmFactory<PcbParams> pcbAlgorithmFactory = new PcbAlgorithmFactory<PcbParams>(
                funcPlacing: pcbInfo => new PlacingMultiThreadAlgorithm(new PlacingExampleCxtyEst(pcbInfo), 8, 0.7),
                funcWireRouting: pcbInfo =>new WireRoutingMultiThreadAlgorithm(new WireRoutingExampleCxtyEst(pcbInfo), 8, 0.7));

            //PcbDesignCadSimulator<PcbParams> pcbDesignCadSimulator = new PcbDesignCadSimulator<PcbParams>();
        }

        public class ComplexityEstimator<TPcbInfo> : IComplexityEstimator
        {
            private readonly TPcbInfo _pcbInfo;
            private readonly Func<TPcbInfo, int> _complexityByPcbInfoConvolution;

            public ComplexityEstimator(TPcbInfo pcbInfo, Func<TPcbInfo, int> complexityByPcbInfoConvolution)
            {
                _pcbInfo = pcbInfo;
                _complexityByPcbInfoConvolution = complexityByPcbInfoConvolution;
            }

            public int EstimateComplexity() => _complexityByPcbInfoConvolution(_pcbInfo);
        }

        public class PlacingExampleCxtyEst : ComplexityEstimator<PcbParams>
        {
            public PlacingExampleCxtyEst(PcbParams pcbInfo) : base(pcbInfo,
                info => info.ElementsCount * 1000
            )
            {
            }
        }
        
        public class WireRoutingExampleCxtyEst : ComplexityEstimator<PcbParams>
        {
            public WireRoutingExampleCxtyEst(PcbParams pcbInfo) : base(pcbInfo,
                info => info.ElementsCount * 1000
            )
            {
            }
        }
    }

}