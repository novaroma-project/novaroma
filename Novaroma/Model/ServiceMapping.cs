using System;
using Novaroma.Interface.Model;

namespace Novaroma.Model {

    public class ServiceMapping: EntityBase {
        private Guid _mediaId;
        private string _serviceName;
        private string _serviceId;

        public Guid MediaId {
            get { return _mediaId; }
            set {
                if (_mediaId == value) return;

                _mediaId = value;
                RaisePropertyChanged("MediaId");
            }
        }

        public string ServiceName {
            get { return _serviceName; }
            set {
                if (_serviceName == value) return;

                _serviceName = value;
                RaisePropertyChanged("ServiceName");
            }
        }

        public string ServiceId {
            get { return _serviceId; }
            set {
                if (_serviceId == value) return;

                _serviceId = value;
                RaisePropertyChanged("ServiceId");
            }
        }

        protected override void CopyFrom(IEntity entity) {
            var external = Helper.ConvertTo<ServiceMapping>(entity);

            CopyFrom(external);
        }

        internal void CopyFrom(ServiceMapping mapping) {
            ServiceName = mapping.ServiceName;
            ServiceId = mapping.ServiceId;
        }
    }
}
