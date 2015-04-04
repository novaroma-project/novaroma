using System;
using Novaroma.Interface;
using Novaroma.Interface.Model;

namespace Novaroma.Model {

    public class Log: EntityBase, ILogItem {
        private LogType _logType;
        private string _message;
        private string _detail;
        private DateTime _logDate;

        public LogType LogType {
            get { return _logType; }
            set {
                if (_logType == value) return;

                _logType = value;
                RaisePropertyChanged("LogType");
            }
        }

        public string Message {
            get { return _message; }
            set {
                if (_message == value) return;

                _message = value;
                RaisePropertyChanged("Message");
            }
        }

        public string Detail {
            get { return _detail; }
            set {
                if (_detail == value) return;

                _detail = value;
                RaisePropertyChanged("Detail");
            }
        }

        public DateTime LogDate {
            get { return _logDate; }
            set {
                if (_logDate == value) return;

                _logDate = value;
                RaisePropertyChanged("LogDate");
            }
        }
    }
}
