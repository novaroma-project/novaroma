using Novaroma.Interface;

namespace Novaroma.RuntimeServices {

    public abstract class RuntimeServiceBase<TService>: IRuntimeService<TService> where TService : INovaromaService {
        private string _code;
        private TService _instance;

        protected RuntimeServiceBase() {
        }

        protected RuntimeServiceBase(string code, TService instance) {
            SetInstance(code, instance);
        }

        public void SetInstance(string code, TService instance) {
            _code = code;
            _instance = instance;
        }

        #region IRuntimeService<IFileBackupper> Members

        public string Code {
            get { return _code; }
        }

        public TService Instance {
            get { return _instance; }
        }

        public abstract string DefaultCode { get; }

        #endregion

        #region INovaromaService Members

        public string ServiceName {
            get { return Instance.ServiceName; }
        }

        #endregion
    }
}
