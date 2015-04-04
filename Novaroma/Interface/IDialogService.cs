using System.Threading.Tasks;

namespace Novaroma.Interface {

    public interface IDialogService {
        Task Information(string title, string message);
        Task<bool> Confirm(string title, string message);
        Task Error(string title, string message);
        string SaveFileDialog(string title, string fileName, string extension, string filter = null, string currentDirectory = null);
    }
}
