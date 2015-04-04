using System.Threading.Tasks;

namespace Novaroma.Interface {

    public interface IDialogManager {
        Task ShowMessage(string title, string message);
        Task ShowWarning(string title, string message);
        Task ShowError(string title, string error, string detail);
        Task<bool> ShowConfirm(string title, string question);
    }
}
