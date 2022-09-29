using PcbDesignSimuModeling.WPF.Models.Resources.EadSybSystems.PlacingSystems;
using PcbDesignSimuModeling.WPF.Models.Resources.EadSybSystems.RoutingSystems;

namespace PcbDesignSimuModeling.WPF.Models.Resources.EadSybSystems;

public interface IEadSubSystemFactories
{
    public IPlacingSysFactory PlacingSysFactory { get; }
    public IRoutingSysFactory RoutingSysFactory { get; }
}