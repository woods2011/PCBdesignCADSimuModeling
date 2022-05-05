using PcbDesignSimuModeling.Core.Models.Resources;
using PcbDesignSimuModeling.Core.Models.Resources.Cpu;
using PcbDesignSimuModeling.Core.Models.Resources.Designer;
using PcbDesignSimuModeling.Core.Models.Resources.Ram;
using PcbDesignSimuModeling.Core.Models.Resources.Server;

namespace PcbDesignSimuModeling.Core.Models.SimuSystem;

public class StatCollector
{
    public void ResourceUsage(IEnumerable<IResource> resources, TimeSpan modelTime)
    {
        var resList = resources.ToList();

        var clusterUsagePerThreadListList =
            resList.OfType<CpuCluster>().Select(cluster => cluster.UsagePerThreadList).ToList();
        var clusterTotalUsageList =
            clusterUsagePerThreadListList.Select(usagePerThreadList => usagePerThreadList.Sum()).ToList();

        var ramUsage = resList.OfType<Ram>().First().AvailableAmount;

        var busyDesigners = resList.OfType<Designer>().Count(designer => designer.IsBusy || !designer.IsActive);

        var serverUsage = resList.OfType<Server>().First().ActiveUsers;
    }
}