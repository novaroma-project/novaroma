using System.ComponentModel;

namespace Novaroma.Interface {

    public interface IConfigurable {
        string SettingName { get; }
        INotifyPropertyChanged Settings { get; }
        string SerializeSettings();
        void DeserializeSettings(string settings);
    }
}
