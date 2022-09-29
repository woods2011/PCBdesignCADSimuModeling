using System;
using PcbDesignSimuModeling.WPF.Models.Technologies.PcbDesign;

namespace PcbDesignSimuModeling.WPF.Models.Resources.EadSybSystems.RoutingSystems;

public class RoutingSysFactory : IRoutingSysFactory
{
    private readonly Func<PcbDescription, IRoutingSystem> _funcWireRouting;

    public RoutingSysFactory(Func<PcbDescription, IRoutingSystem> funcWireRouting) => _funcWireRouting = funcWireRouting;

    public IRoutingSystem Create(PcbDescription pcbInfo) => _funcWireRouting(pcbInfo);
}