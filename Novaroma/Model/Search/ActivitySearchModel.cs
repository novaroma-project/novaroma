using System;
using Novaroma.Interface.Model;

namespace Novaroma.Model.Search {

    public class ActivitySearchModel: ModelBase {
        private int _pageSize;
        private int _page;
        private bool? _notRead;

        public ActivitySearchModel() {
            _pageSize = 100;
            _page = 1;
        }

        public int PageSize {
            get { return _pageSize; }
            set {
                if (_pageSize == value) return;

                _pageSize = value;
                RaisePropertyChanged("PageSize");
            }
        }

        public int Page {
            get { return _page; }
            set {
                if (_page == value) return;

                _page = value;
                RaisePropertyChanged("Page");
                OnRefreshNeeded();
            }
        }

        public bool? NotRead {
            get { return _notRead; }
            set {
                if (_notRead == value) return;

                _notRead = value;
                RaisePropertyChanged("NotRead");
            }
        }

        public int NotReadActivityCount { get; set; }

        public event EventHandler RefreshNeeded;
        protected virtual void OnRefreshNeeded() {
            var handler = RefreshNeeded;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}
