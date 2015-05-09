using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Threading.Tasks;
using Novaroma.DTO;

namespace Novaroma.Interface {

    [ServiceContract]
    public interface IWebUIService {

        [OperationContract]
        Task<IEnumerable<TvShowDTO>> GetUnseenEpisodes();

        [OperationContract]
        void ExecuteDownloads();

        [OperationContract]
        Stream Get(string arguments);
    }
}
