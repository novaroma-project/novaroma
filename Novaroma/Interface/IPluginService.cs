using System.Threading.Tasks;

namespace Novaroma.Interface {

    public interface IPluginService : INovaromaService {
        string DisplayName { get; }
        Task Activate();
    }
}
