using System.Threading.Tasks;

namespace Novaroma.Interface.Download.Provider {

    public interface ITorrentSearchResult {
        string Uri { get; }
        string Name { get; }
        int Seed { get; }
        int Leech { get; }
        string Size { get; }
        int? Files { get; }
        string Age { get; }
        string MagnetUri { get; }
        byte[] Download();
        Task<byte[]> DownloadAsync();
    }
}