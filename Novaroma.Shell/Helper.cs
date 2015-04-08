using Novaroma.Interface;

namespace Novaroma.Shell {

    public static class Helper {

        internal static void SetCulture(IShellService client) {
            var language = client.GetSelectedLanguage().Result;
            Novaroma.Helper.SetCulture(language);
        }
    }
}
