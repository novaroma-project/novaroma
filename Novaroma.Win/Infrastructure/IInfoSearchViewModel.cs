using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Novaroma.Interface.Info;

namespace Novaroma.Win.Infrastructure {

    public interface IInfoSearchViewModel<TSearchResult> : INotifyPropertyChanged where TSearchResult : IInfoSearchResult {
        Task InitSearch();
        Task InitFromImdbId(string imdbId);
        ICommand SearchCommand { get; }
        IEnumerable<IInfoSearchMediaViewModel<TSearchResult>> Results { get; }
        IInfoSearchMediaViewModel<TSearchResult> SelectedResult { get; set; }
        IMultiCheckSelection<IInfoSearchMediaViewModel<TSearchResult>> ResultSelections { get; }
        bool SearchInitialized { get; }
    }
}
