using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PcbDesignSimuModeling.Core.Models.Resources.Server;

public class Server : SharedResource, INotifyPropertyChanged
{
    [JsonIgnore]
    public Func<Server, double> ResValueConvolution { get; init; } = server => server.InternetSpeed;

    public double InternetSpeed { get; set; }
        

    public Server(double internetSpeed) => InternetSpeed = internetSpeed;


    public bool TryGetResource(int _) => true;

    public override double ResValueForProc(int requestId) => ResValueConvolution(this);

    public override void FreeResource(int requestId) {}


    public override IResource Clone() => new Server(InternetSpeed) {ResValueConvolution = ResValueConvolution};

    public override double Cost => Math.Round(
        Math.Exp(1.0 + 125.0 / (InternetSpeed + 31.5)) * InternetSpeed * 7.5) - 4000;

    public event PropertyChangedEventHandler? PropertyChanged;
}