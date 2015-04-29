using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Novaroma.Interface;
using Novaroma.Interface.Model;

namespace Novaroma.Model.Search {

    public abstract class MediaSearchModel : ModelBase, IConfigurable {
        private readonly MultiCheckSelection<string> _genres;
        private readonly IEnumerable<OrderSelection> _orderList;
        private string _query;
        private int? _releaseYearStart;
        private int? _releaseYearEnd;
        private float? _ratingMin = 0;
        private float? _ratingMax = 10;
        private int? _voteCountMin;
        private int? _voteCountMax;
        private int? _runtimeMin;
        private int? _runtimeMax;
        private bool? _notWatched;
        private bool? _downloaded;
        private bool? _subtitleDownloaded;
        private bool? _notFound;
        private bool? _subtitleNotFound;
        private OrderSelection _selectedOrder;
        private int _pageSize;
        private int _page;

        protected MediaSearchModel(ObservableCollection<string> mediaGenres) {
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
                RaisePropertyChanged("MinimumYear");
            }
        }

        public int? ReleaseYearEnd {
            get { return _releaseYearEnd; }
            set {
                if (_releaseYearEnd == value) return;

                _releaseYearEnd = value;
                RaisePropertyChanged("ReleaseYearEnd");
                RaisePropertyChanged("MaximumYear");
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

        public int? VoteCountMin {
            get { return _voteCountMin; }
            set {
                if (_voteCountMin == value) return;

                _voteCountMin = value;
                RaisePropertyChanged("VoteCountMin");
                RaisePropertyChanged("MinimumVote");
            }
        }

        public int? VoteCountMax {
            get { return _voteCountMax; }
            set {
                if (_voteCountMax == value) return;

                _voteCountMax = value;
                RaisePropertyChanged("VoteCountMax");
                RaisePropertyChanged("MaximumVote");
            }
        }

        public int? RuntimeMin {
            get { return _runtimeMin; }
            set {
                if (_runtimeMin == value) return;

                _runtimeMin = value;
                RaisePropertyChanged("RuntimeMin");
                RaisePropertyChanged("MinimumRuntime");
            }
        }

        public int? RuntimeMax {
            get { return _runtimeMax; }
            set {
                if (_runtimeMax == value) return;

                _runtimeMax = value;
                RaisePropertyChanged("RuntimeMax");
                RaisePropertyChanged("MaximumRuntime");
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

        public int MinimumYear {
            get { return ReleaseYearStart.HasValue ? ReleaseYearStart.Value : 1886; }
        }

        public int MaximumYear {
            get { return ReleaseYearEnd.HasValue ? ReleaseYearEnd.Value : (DateTime.Now.Year + 1); }
        }

        public int MinimumVote {
            get { return VoteCountMin.HasValue ? VoteCountMin.Value : 0; }
        }

        public int? MaximumVote {
            get { return VoteCountMax.HasValue ? VoteCountMax.Value : (int?)null; }
        }

        public int MinimumRuntime {
            get { return RuntimeMin.HasValue ? RuntimeMin.Value : 0; }
        }

        public int? MaximumRuntime {
            get { return RuntimeMax.HasValue ? RuntimeMax.Value : (int?)null; }
        }

        public IEnumerable<OrderSelection> OrderList {
            get { return _orderList; }
        }

        public event EventHandler RefreshNeeded;
        protected virtual void OnRefreshNeeded() {
            var handler = RefreshNeeded;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        protected abstract string SettingName { get; }

        protected virtual Dictionary<string, object> GetSettingsDictionary() {
            return new Dictionary<string, object> {
                { "Downloaded", Downloaded },
                { "Genres", Genres.SelectedItems.Select(g => g) },
                { "NotFound", NotFound },
                { "NotWatched", NotWatched },
                { "VoteCountMax", VoteCountMax },
                { "VoteCountMin", VoteCountMin },
                { "PageSize", PageSize },
                { "Query", Query },
                { "RatingMax", RatingMax },
                { "RatingMin", RatingMin },
                { "ReleaseYearEnd", ReleaseYearEnd },
                { "ReleaseYearStart", ReleaseYearStart },
                { "RuntimeMax", RuntimeMax },
                { "RuntimeMin", RuntimeMin },
                { "SelectedOrderValue", SelectedOrder.Order.Value },
                { "SelectedOrderDescending", SelectedOrder.IsDescending },
                { "SubtitleDownloaded", SubtitleDownloaded },
                { "SubtitleNotFound", SubtitleNotFound }
            };
        }

        protected virtual string SerializeSettings() {
            var o = GetSettingsDictionary();

            return JsonConvert.SerializeObject(o);
        }

        protected virtual void DeserializeSettings(string settings) {
            var json = (JObject)JsonConvert.DeserializeObject(settings);
            
            SetSettingsFromJson(json);
        }

        protected virtual void SetSettingsFromJson(JObject json) {
            Downloaded = (bool?)json["Downloaded"];
            var genres = (IEnumerable)json["Genres"];
            genres.OfType<object>().ToList().ForEach(g => {
                var genre = Genres.Selections.FirstOrDefault(gg => gg.Item == g.ToString());
                if (genre != null)
                    genre.IsSelected = true;
            });
            NotFound = (bool?)json["NotFound"];
            NotWatched = (bool?)json["NotWatched"];
            VoteCountMax = (int?)json["VoteCountMax"];
            VoteCountMin = (int?)json["VoteCountMin"];
            PageSize = (int)json["PageSize"];
            Query = (string)json["Query"];
            RatingMax = (float?)json["RatingMax"];
            RatingMin = (float?)json["RatingMin"];
            ReleaseYearEnd = (int?)json["ReleaseYearEnd"];
            ReleaseYearStart = (int?)json["ReleaseYearStart"];
            RuntimeMax = (int?)json["RuntimeMax"];
            RuntimeMin = (int?)json["RuntimeMin"];
            SelectedOrder = OrderList.First(o => o.Order.Value == (decimal)json["SelectedOrderValue"]);
            var isDescending = (bool?) json["SelectedOrderDescending"];
            if (isDescending.HasValue)
                SelectedOrder.IsDescending = isDescending.Value;
            SubtitleDownloaded = (bool?)json["SubtitleDownloaded"];
            SubtitleNotFound = (bool?)json["SubtitleNotFound"];
        }

        #region IConfigurable Members

        string IConfigurable.SettingName {
            get { return SettingName; }
        }

        INotifyPropertyChanged IConfigurable.Settings {
            get { return this; }
        }

        string IConfigurable.SerializeSettings() {
            return SerializeSettings();
        }

        void IConfigurable.DeserializeSettings(string settings) {
            DeserializeSettings(settings);
        }

        #endregion
    }
}
