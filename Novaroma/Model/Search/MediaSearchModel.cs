using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Novaroma.Interface.Model;

namespace Novaroma.Model.Search {

    public class MediaSearchModel : ModelBase {
        private readonly MultiCheckSelection<string> _genres;
        private readonly IEnumerable<OrderSelection> _orderList;
        private string _query;
        private int? _releaseYearStart;
        private int? _releaseYearEnd;
        private float? _ratingMin;
        private float? _ratingMax;
        private int? _numberOfVotesMin;
        private int? _numberOfVotesMax;
        private int? _runtimeMin;
        private int? _runtimeMax;
        private bool? _notWatched;
        private bool? _downloaded;
        private bool? _subtitleDownloaded;
        private bool? _notFound;
        private bool? _subtitleNotFound;
        private string _imdbId;
        private OrderSelection _selectedOrder;
        private int _pageSize;
        private int _page;

        public MediaSearchModel(ObservableCollection<string> mediaGenres) {
            _genres = new MultiCheckSelection<string>(mediaGenres);
            var orderEnumInfo = Constants.OrderFieldsEnumInfo;
            var orderList = new List<OrderSelection>();
            var titleOrder = new OrderSelection(orderEnumInfo.First(i => i.Item == OrderFields.Title));
            orderList.Add(titleOrder);
            orderList.Add(new OrderSelection(orderEnumInfo.First(i => i.Item == OrderFields.Rating), true));
            orderList.Add(new OrderSelection(orderEnumInfo.First(i => i.Item == OrderFields.Year)));

            _orderList = orderList;
            _selectedOrder = titleOrder;
            _selectedOrder.IsSelected = true;

            _pageSize = 50;
            _page = 1;
        }

        public void RaiseResourceProperties() {
            foreach (var orderSelection in _orderList) {
                orderSelection.RaiseResourceProperties();
            }
        }

        public string Query {
            get { return _query; }
            set {
                if (_query == value) return;

                _query = value;
                RaisePropertyChanged("Query");
            }
        }

        public int? ReleaseYearStart {
            get { return _releaseYearStart; }
            set {
                if (_releaseYearStart == value) return;

                _releaseYearStart = value;
                RaisePropertyChanged("ReleaseYearStart");
            }
        }

        public int? ReleaseYearEnd {
            get { return _releaseYearEnd; }
            set {
                if (_releaseYearEnd == value) return;

                _releaseYearEnd = value;
                RaisePropertyChanged("ReleaseYearEnd");
            }
        }

        public float? RatingMin {
            get { return _ratingMin; }
            set {
                if (_ratingMin == value) return;

                _ratingMin = value;
                RaisePropertyChanged("RatingMin");
            }
        }

        public float? RatingMax {
            get { return _ratingMax; }
            set {
                if (_ratingMax == value) return;

                _ratingMax = value;
                RaisePropertyChanged("RatingMax");
            }
        }

        public int? NumberOfVotesMin {
            get { return _numberOfVotesMin; }
            set {
                if (_numberOfVotesMin == value) return;

                _numberOfVotesMin = value;
                RaisePropertyChanged("NumberOfVotesMin");
            }
        }

        public int? NumberOfVotesMax {
            get { return _numberOfVotesMax; }
            set {
                if (_numberOfVotesMax == value) return;

                _numberOfVotesMax = value;
                RaisePropertyChanged("NumberOfVotesMax");
            }
        }

        public int? RuntimeMin {
            get { return _runtimeMin; }
            set {
                if (_runtimeMin == value) return;

                _runtimeMin = value;
                RaisePropertyChanged("RuntimeMin");
            }
        }

        public int? RuntimeMax {
            get { return _runtimeMax; }
            set {
                if (_runtimeMax == value) return;

                _runtimeMax = value;
                RaisePropertyChanged("RuntimeMax");
            }
        }

        public bool? NotWatched {
            get { return _notWatched; }
            set {
                if (_notWatched == value) return;

                _notWatched = value;
                RaisePropertyChanged("NotWatched");
            }
        }

        public bool? Downloaded {
            get { return _downloaded; }
            set {
                if (_downloaded == value) return;

                _downloaded = value;
                RaisePropertyChanged("Downloaded");
            }
        }

        public bool? SubtitleDownloaded {
            get { return _subtitleDownloaded; }
            set {
                if (_subtitleDownloaded == value) return;

                _subtitleDownloaded = value;
                RaisePropertyChanged("SubtitleDownloaded");
            }
        }

        public bool? NotFound {
            get { return _notFound; }
            set {
                if (_notFound == value) return;

                _notFound = value;
                RaisePropertyChanged("NotFound");
            }
        }

        public bool? SubtitleNotFound {
            get { return _subtitleNotFound; }
            set {
                if (_subtitleNotFound == value) return;

                _subtitleNotFound = value;
                RaisePropertyChanged("SubtitleNotFound");
            }
        }

        public string ImdbId {
            get { return _imdbId; }
            set {
                if (_imdbId == value) return;

                _imdbId = value;
                RaisePropertyChanged("ImdbId");
            }
        }

        public OrderSelection SelectedOrder {
            get { return _selectedOrder; }
            set {
                if (_selectedOrder == value)
                    _selectedOrder.IsDescending = !_selectedOrder.IsDescending;
                else {
                    _selectedOrder.IsSelected = false;
                    _selectedOrder = value;
                    _selectedOrder.IsSelected = true;
                    _selectedOrder.Reset();
                }

                Page = 1;
                RaisePropertyChanged("SelectedOrder");
                OnRefreshNeeded();
            }
        }

        public int PageSize {
            get { return _pageSize; }
            set {
                if (_pageSize == value) return;

                _pageSize = value;
                _page = 1;
                RaisePropertyChanged("PageSize");
                RaisePropertyChanged("Page");
                OnRefreshNeeded();
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

        public MultiCheckSelection<string> Genres {
            get { return _genres; }
        }

        public IEnumerable<OrderSelection> OrderList {
            get { return _orderList; }
        }

        public event EventHandler RefreshNeeded;
        protected virtual void OnRefreshNeeded() {
            var handler = RefreshNeeded;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}
