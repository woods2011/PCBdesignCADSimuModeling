using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PcbDesignSimuModeling.Core.Models.Resources.Server;

public class Server : MixedResource, INotifyPropertyChanged
{
    public double InternetSpeed { get; set; }


    public Server(double internetSpeed) => InternetSpeed = internetSpeed;


    public bool TryGetResource(int requestId)
    {
        UtilizingRequestsIds.Add(requestId);
        return true;
    }

    public override double PowerForRequest(int requestId) =>
        InternetSpeed / Math.Max(1.0, UtilizingRequestsIds.Count * 0.75);

    public override void FreeResource(int requestId) => UtilizingRequestsIds.Remove(requestId);

    public override IResource Clone() => new Server(InternetSpeed);

    public int ActiveUsers => UtilizingRequestsIds.Count;

    public override decimal Cost => (decimal) Math.Round(
        Math.Exp(1.0 + 125.0 / (InternetSpeed + 31.5)) * InternetSpeed * 7.5) - 4000;

    public event PropertyChangedEventHandler? PropertyChanged;
}