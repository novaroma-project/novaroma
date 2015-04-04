using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;

namespace Novaroma {

    /// <summary>
    /// Thanks to Thomas Levesque
    /// http://www.thomaslevesque.com/2009/04/17/wpf-binding-to-an-asynchronous-collection/
    /// </summary>
    public class NovaromaObservableCollection<T> : ObservableCollection<T> {
        private readonly SynchronizationContext _synchronizationContext = SynchronizationContext.Current;

        public NovaromaObservableCollection() {
        }

        public NovaromaObservableCollection(IEnumerable<T> list): base(list) {
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
            if (SynchronizationContext.Current == _synchronizationContext)
                RaiseCollectionChanged(e);
            else
                SynchronizationContext.Send(RaiseCollectionChanged, e);
        }

        private void RaiseCollectionChanged(object param) {
            base.OnCollectionChanged((NotifyCollectionChangedEventArgs)param);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e) {
            if (SynchronizationContext.Current == _synchronizationContext)
                RaisePropertyChanged(e);
            else
                SynchronizationContext.Send(RaisePropertyChanged, e);
        }

        private void RaisePropertyChanged(object param) {
            base.OnPropertyChanged((PropertyChangedEventArgs)param);
        }

        protected SynchronizationContext SynchronizationContext {
            get {
                return _synchronizationContext ?? SynchronizationContext.Current;
            }
        }
    }
}
