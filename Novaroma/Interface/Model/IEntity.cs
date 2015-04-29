using System;

namespace Novaroma.Interface.Model {

    public interface IEntity {
        Guid Id { get; }
        bool IsModified { get; set; }
        void CopyFrom(IEntity entity);
    }
}
