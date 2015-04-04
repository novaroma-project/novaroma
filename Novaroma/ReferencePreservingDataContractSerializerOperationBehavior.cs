using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel.Description;
using System.Xml;

namespace Novaroma {

    public class ReferencePreservingDataContractSerializerOperationBehavior: DataContractSerializerOperationBehavior {

        public ReferencePreservingDataContractSerializerOperationBehavior(OperationDescription operationDescription) : base(operationDescription) {
        }
        
        public override XmlObjectSerializer CreateSerializer(Type type, XmlDictionaryString name, XmlDictionaryString ns, IList<Type> knownTypes) {
            return new DataContractSerializer(type, name, ns, knownTypes, 2147483646, false, true, null);
        }
    }
}
