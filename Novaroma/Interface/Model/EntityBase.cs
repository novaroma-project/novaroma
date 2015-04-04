using System;
using System.Runtime.Serialization;

namespace Novaroma.Interface.Model {

    public abstract class EntityBase : ModelBase, IEntity {
        private Guid _id;
        [NonSerialized]
        private bool _isModified;

        protected EntityBase() {
            Id = Guid.NewGuid();
            _isModified = false;
        }

        public Guid Id {
            get { return _id; }
            set {
                if (_id == value) return;
                
                _id = value;
                RaisePropertyChanged("Id");
            }
        }

        object IEntity.Id { get { return Id; } }

        [IgnoreDataMember]
        public virtual bool IsModified {
            get { return _isModified; }
            set { _isModified = value; }
        }

        protected override void RaisePropertyChanged(string propertyName = null) {
            IsModified = true;

            base.RaisePropertyChanged(propertyName);
        }
    }
}
