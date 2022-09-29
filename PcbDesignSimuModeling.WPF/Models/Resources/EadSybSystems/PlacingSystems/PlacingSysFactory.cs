using System;
using PcbDesignSimuModeling.WPF.Models.Technologies.PcbDesign;

namespace PcbDesignSimuModeling.WPF.Models.Resources.EadSybSystems.PlacingSystems;

public class PlacingSysFactory : IPlacingSysFactory
{
    private readonly Func<PcbDescription, IPlacingSystem> _funcPlacing;

    public PlacingSysFactory(Func<PcbDescription, IPlacingSystem> funcPlacing)
    {
        _funcPlacing = funcPlacing;
    }

    public IPlacingSystem Create(PcbDescription pcbInfo) => _funcPlacing(pcbInfo);
}