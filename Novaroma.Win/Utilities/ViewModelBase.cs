using System.Threading.Tasks;
using Novaroma.Interface;
using Novaroma.Interface.Model;

namespace Novaroma.Win.Utilities {

    public abstract class ViewModelBase: ModelBase {
        protected readonly IDialogService DialogService;

        protected ViewModelBase(IDialogService dialogService) {
            DialogService = dialogService;
        }

        public Task<bool> Confirm(string title, string message) {
            return DialogService.Confirm(title, message);
        }
    }
}
