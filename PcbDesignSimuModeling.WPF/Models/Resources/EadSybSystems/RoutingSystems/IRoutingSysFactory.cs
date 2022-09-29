using PcbDesignSimuModeling.WPF.Models.Technologies.PcbDesign;

namespace PcbDesignSimuModeling.WPF.Models.Resources.EadSybSystems.RoutingSystems;

public interface IRoutingSysFactory
{
    public IRoutingSystem Create(PcbDescription pcbInfo);
}