using System.Windows;
using PcbDesignCADSimuModeling.ViewModels;
using PcbDesignCADSimuModeling.Views;

namespace PcbDesignCADSimuModeling
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Window window = new MainView();
            window.DataContext = new MainViewModel();
            window.Show();

            base.OnStartup(e);
        }
    }
}