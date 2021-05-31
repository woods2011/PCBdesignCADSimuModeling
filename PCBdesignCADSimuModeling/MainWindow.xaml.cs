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
using MathNet.Numerics.Distributions;
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
        public TimeSpan Times { get; set; } = new TimeSpan(1, 1, 1, 1);
        public String TestStr { get; set; } = "Fdsfdf";

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;


            string placingAlgName = "Example";
            string wireRoutingAlgName = "Example";

            var kek = (itemz: PlacingAlgProviderFactory.Create(placingAlgName),
                WireRoutingAlgProviderFactory.Create(wireRoutingAlgName));
            
                        
            
            var techIntervalDistr =
                new Normal(new TimeSpan(1, 0, 0, 0).TotalSeconds, new TimeSpan(6, 0, 0).TotalSeconds);
            var elementCountDistr = new Normal(2000, 100);
            var dimensionUsagePctDistr = new Normal(0.8, 0.1);

            double variousSizePctMean = 0.7;
            var variousSizePctDistr = new Beta(variousSizePctMean, 1.0 - variousSizePctMean);

            var simuEventGenerator = new SimuEventGenerator.Builder()
                .NewTechInterval(techIntervalDistr)
                .PcbParams(elementCountDistr, dimensionUsagePctDistr, variousSizePctDistr).Build();

            var simulationEvents = simuEventGenerator.GeneratePcbDesignTech(TimeSpan.FromDays(300));



            //PcbDesignCadSimulator<PcbParams> pcbDesignCadSimulator = new PcbDesignCadSimulator<PcbParams>();
        }
    }
}