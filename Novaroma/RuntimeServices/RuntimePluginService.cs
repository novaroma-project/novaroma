using System.Threading.Tasks;
using Novaroma.Interface;

namespace Novaroma.RuntimeServices {

    public class RuntimePluginService : RuntimeServiceBase<IPluginService>, IPluginService {

        public override string DefaultCode {
            get { return DEFAULT_CODE; }
        }

        #region IPluginService Members

        public string DisplayName {
            get { return Instance.DisplayName; }
        }

        public Task Activate() {
            return Instance.Activate();
        }

        #endregion

        public const string DEFAULT_CODE =
@"
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Novaroma.Interface;

namespace Novaroma.MyServices {

    public class MyPluginService : IPluginService {

        #region IPluginService Members

        public string DisplayName {
            get { return ""MyPluginService""; }
        }

        public Task Activate() {
            throw new System.NotImplementedException();
        }

        #endregion

        #region INovaromaService Members

        public string ServiceName {
            get { return ""MyPluginService""; }
        }

        #endregion
    }

}
";
    }
}
