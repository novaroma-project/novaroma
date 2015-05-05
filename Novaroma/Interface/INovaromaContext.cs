using System;
using System.Linq;
using System.Threading.Tasks;
using Novaroma.Interface.Model;
using Novaroma.Model;

namespace Novaroma.Interface {

    public interface INovaromaContext: IDisposable {
        void Insert(IEntity entity);
        void Update(IEntity entity);
        void Delete(IEntity entity);
        IQueryable<TEntity> GetEntities<TEntity>() where TEntity: IEntity;
        IQueryable<Media> Medias { get; }
        IQueryable<Movie> Movies { get; }
        IQueryable<TvShow> TvShows { get; }
        IQueryable<Activity> Activities { get; }
        IQueryable<Setting> Settings { get; }
        IQueryable<WatchDirectory> WatchDirectories { get; }
        IQueryable<MediaGenre> Genres { get; }
        IQueryable<Log> Logs { get; }
        IQueryable<ScriptService> ScriptServices { get; }
        Task SaveChanges();
        Task Backup(string path);
    }
}
