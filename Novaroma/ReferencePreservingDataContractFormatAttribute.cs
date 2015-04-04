using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Novaroma {

    public class ReferencePreservingDataContractFormatAttribute: Attribute, IOperationBehavior {

        public void AddBindingParameters(OperationDescription description,
            BindingParameterCollection parameters) {
        }

        public void ApplyClientBehavior(OperationDescription description, ClientOperation proxy) {
            IOperationBehavior innerBehavior = new ReferencePreservingDataContractSerializerOperationBehavior(description);
            innerBehavior.ApplyClientBehavior(description, proxy);
        }

        public void ApplyDispatchBehavior(OperationDescription description, DispatchOperation dispatch) {
            IOperationBehavior innerBehavior = new ReferencePreservingDataContractSerializerOperationBehavior(description);
            innerBehavior.ApplyDispatchBehavior(description, dispatch);
        }

        public void Validate(OperationDescription description) {
        }
    }
}
