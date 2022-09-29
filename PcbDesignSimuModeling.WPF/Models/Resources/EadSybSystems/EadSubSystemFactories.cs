using PcbDesignSimuModeling.WPF.Models.Resources.EadSybSystems.PlacingSystems;
using PcbDesignSimuModeling.WPF.Models.Resources.EadSybSystems.RoutingSystems;

namespace PcbDesignSimuModeling.WPF.Models.Resources.EadSybSystems;

public class EadSubSystemFactories : IEadSubSystemFactories
{
    public IPlacingSysFactory PlacingSysFactory { get; set; }
    public IRoutingSysFactory RoutingSysFactory { get; set; }

    public EadSubSystemFactories(IPlacingSysFactory placingSysFactory, IRoutingSysFactory routingSysFactory)
    {
        PlacingSysFactory = placingSysFactory;
        RoutingSysFactory = routingSysFactory;
    }
}