using Novaroma.Interface;

namespace Novaroma.Shell {

    public static class Helper {

        internal static void SetCulture(IShellService client) {
            var languageTask = client.GetSelectedLanguage();
            languageTask.Wait();
            var language = languageTask.Result;
            Novaroma.Helper.SetCulture(language);
        }
    }
}
