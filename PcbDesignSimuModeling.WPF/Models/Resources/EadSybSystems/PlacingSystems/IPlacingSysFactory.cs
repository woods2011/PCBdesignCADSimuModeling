using PcbDesignSimuModeling.WPF.Models.Technologies.PcbDesign;

namespace PcbDesignSimuModeling.WPF.Models.Resources.EadSybSystems.PlacingSystems;

public interface IPlacingSysFactory
{
    public IPlacingSystem Create(PcbDescription pcbInfo);
}