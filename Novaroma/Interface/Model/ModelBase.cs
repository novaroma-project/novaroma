using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;

namespace Novaroma.Interface.Model {

    public abstract class ModelBase : INotifyPropertyChanged, IDataErrorInfo {

        protected virtual IEnumerable<ValidationResult> Validate() {
            return Enumerable.Empty<ValidationResult>();
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void RaisePropertyChanged(string propertyName = null) {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region IDataErrorInfo Members

        [IgnoreDataMember]
        public string this[string propertyName] {
            get {
                var results = new List<ValidationResult>();
                var property = GetType().GetProperty(propertyName);
                if (property == null)
                    return string.Empty;
                var value = property.GetValue(this, null);
                var context = new ValidationContext(this, null, null) {
                    MemberName = propertyName
                };
                var error = Validator.TryValidateProperty(value, context, results)
                            ? string.Empty
                            : string.Join(Environment.NewLine, results.Select(r => r.ErrorMessage));
                return error;
            }
        }

        [IgnoreDataMember]
        public string Error {
            get {
                var results = new List<ValidationResult>();
                var context = new ValidationContext(this, null, null);
                Validator.TryValidateObject(this, context, results, true);

                var customError = Validate();
                if (customError != null)
                    results.AddRange(customError);

                return string.Join(Environment.NewLine, results.Select(r => r.ErrorMessage));
            }
        }

        #endregion
    }
}
