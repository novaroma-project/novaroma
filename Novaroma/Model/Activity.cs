using System;
using Novaroma.Interface.Model;

namespace Novaroma.Model {

    public class Activity: EntityBase {
        private string _description;
        private DateTime _activityDate;
        private string _path;
        private bool _isRead;

        public string Description {
            get { return _description; }
            set {
                if (_description == value) return;

                _description = value;
                RaisePropertyChanged("Description");
            }
        }

        public DateTime ActivityDate {
            get { return _activityDate; }
            set {
                if (_activityDate == value) return;

                _activityDate = value;
                RaisePropertyChanged("ActivityDate");
            }
        }

        public string Path {
            get { return _path; }
            set {
                if (_path == value) return;

                _path = value;
                RaisePropertyChanged("Path");
            }
        }

        public bool IsRead {
            get { return _isRead; }
            set {
                if (_isRead == value) return;

                _isRead = value;
                RaisePropertyChanged("IsRead");
            }
        }

        protected override void CopyFrom(IEntity entity) {
            var external = Helper.ConvertTo<Activity>(entity);

            Description = external.Description;
            ActivityDate = external.ActivityDate;
            Path = external.Path;
            IsRead = external.IsRead;
        }
    }
}
