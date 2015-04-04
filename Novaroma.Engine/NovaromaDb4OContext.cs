using System.Linq;
using System.Threading.Tasks;
using Db4objects.Db4o;
using Db4objects.Db4o.Linq;
using Novaroma.Interface;
using Novaroma.Interface.Model;
using Novaroma.Model;

namespace Novaroma.Engine {

    public class NovaromaDb4OContext : INovaromaContext {
        private readonly IEmbeddedObjectContainer _container;

        public NovaromaDb4OContext(IEmbeddedObjectContainer container) {
            container.Ext().Configure().OptimizeNativeQueries(true);

            container.Ext().Configure().ObjectClass(typeof(EntityBase)).ObjectField("_id").Indexed(true);

            container.Ext().Configure().ObjectClass(typeof(Media)).ObjectField("_title").Indexed(true);
            container.Ext().Configure().ObjectClass(typeof(Media)).ObjectField("_year").Indexed(true);
            container.Ext().Configure().ObjectClass(typeof(Media)).ObjectField("_rating").Indexed(true);
            container.Ext().Configure().ObjectClass(typeof(Media)).ObjectField("_directory").Indexed(true);
            container.Ext().Configure().ObjectClass(typeof(Media)).ObjectField("_imdbId").Indexed(true);
            container.Ext().Configure().ObjectClass(typeof(Media)).ObjectField("_voteCount").Indexed(true);
            container.Ext().Configure().ObjectClass(typeof(Media)).ObjectField("_runtime").Indexed(true);
            
            container.Ext().Configure().ObjectClass(typeof(Movie)).ObjectField("_filePath").Indexed(true);
            container.Ext().Configure().ObjectClass(typeof(Movie)).ObjectField("_isWatched").Indexed(true);
            container.Ext().Configure().ObjectClass(typeof(Movie)).ObjectField("_subtitleDownloaded").Indexed(true);
            container.Ext().Configure().ObjectClass(typeof(Movie)).ObjectField("_notFound").Indexed(true);
            container.Ext().Configure().ObjectClass(typeof(Movie)).ObjectField("_subtitleNotFound").Indexed(true);
            container.Ext().Configure().ObjectClass(typeof(Movie)).ObjectField("_downloadKey").Indexed(true);
            
            container.Ext().Configure().ObjectClass(typeof(TvShowEpisode)).ObjectField("_filePath").Indexed(true);
            container.Ext().Configure().ObjectClass(typeof(TvShowEpisode)).ObjectField("_isWatched").Indexed(true);
            container.Ext().Configure().ObjectClass(typeof(TvShowEpisode)).ObjectField("_subtitleDownloaded").Indexed(true);
            container.Ext().Configure().ObjectClass(typeof(TvShowEpisode)).ObjectField("_notFound").Indexed(true);
            container.Ext().Configure().ObjectClass(typeof(TvShowEpisode)).ObjectField("_subtitleNotFound").Indexed(true);
            container.Ext().Configure().ObjectClass(typeof(TvShowEpisode)).ObjectField("_downloadKey").Indexed(true);

            container.Ext().Configure().ObjectClass(typeof(TvShow)).CascadeOnDelete(true);
            container.Ext().Configure().ObjectClass(typeof(TvShowSeason)).CascadeOnDelete(true);

            container.Ext().Configure().MarkTransient("System.NonSerialized");
            container.Ext().Configure().UpdateDepth(5);
            container.Ext().Configure().CallConstructors(true);
            
            _container = container;
        }

        #region INovaromaContext Members

        public void Insert(IEntity entity) {
            _container.Store(entity);
        }

        public void Update(IEntity entity) {
            entity.IsModified = false;
            _container.Store(entity);
        }

        public void Attach(IEntity entity) {
            _container.Store(entity);
        }

        public void Delete(IEntity o) {
            _container.Delete(o);
        }

        public IQueryable<Media> Medias {
            get { return _container.AsQueryable<Media>(); }
        }

        public IQueryable<Movie> Movies {
            get { return _container.AsQueryable<Movie>(); }
        }

        public IQueryable<TvShow> TvShows {
            get { return _container.AsQueryable<TvShow>(); }
        }

        public IQueryable<Activity> Activities {
            get { return _container.AsQueryable<Activity>(); }
        }

        public IQueryable<Setting> Settings {
            get { return _container.AsQueryable<Setting>(); }
        }

        public IQueryable<WatchDirectory> WatchDirectories {
            get { return _container.AsQueryable<WatchDirectory>(); }
        }

        public IQueryable<MediaGenre> Genres {
            get { return _container.AsQueryable<MediaGenre>(); }
        }

        public IQueryable<Log> Logs {
            get { return _container.AsQueryable<Log>(); }
        }

        public IQueryable<ScriptService> ScriptServices {
            get { return _container.AsQueryable<ScriptService>(); }
        }

        public Task SaveChanges() {
            return Task.Run(() => _container.Commit());
        }

        public Task Backup(string path) {
            return Task.Run(() => _container.Backup(path));
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {
        }

        #endregion
    }
}
