using Novaroma.Properties;

namespace Novaroma {

    public class ServiceUnavailableException : NovaromaException {

        public ServiceUnavailableException(string serviceName)
            : base(string.Format(Resources.ServiceUnavailable, serviceName)) {
        }
    }
}
