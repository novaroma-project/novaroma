using System.Collections.Generic;
using System.Threading.Tasks;

namespace Novaroma.Interface {

    public interface IImdbIdConverter {
        Task<IDictionary<string, object>> GetServiceIds(string imdbId);
    }
}
