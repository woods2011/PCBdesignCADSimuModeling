using System;

namespace PcbDesignSimuModeling.WPF.Models.Resources.ServerRes;

public class Server : MixedResource
{
    public double InternetSpeed { get; }

    public Server(double internetSpeed)
    {
        InternetSpeed = internetSpeed;
        CostPerMonth = Convert.ToDecimal(InternetSpeed * 20);
        //CostPerMonth = Convert.ToDecimal(Math.Exp(1.0 + 125.0 / (InternetSpeed + 31.5)) * InternetSpeed * 7.5) - 4000;
    }

    public override decimal CostPerMonth { get; }

    public bool TryGetResource(int requestId)
    {
        UtilizingRequestsIds.Add(requestId);
        return true;
    }

    public override double PowerForRequest(int requestId) =>
        InternetSpeed / Math.Max(1.0, UtilizingRequestsIds.Count);

    public override void FreeResource(int requestId) => UtilizingRequestsIds.Remove(requestId);

    public int ActiveUsers => UtilizingRequestsIds.Count;
}