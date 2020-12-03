using System.Collections.Generic;

namespace Webscan.NotificationProcessor.Models.Repository
{
    public interface IStatusCheckRepository<TEntity>
    {
        IEnumerable<TEntity> GetAll();
        TEntity Get(int id);
        void Add(TEntity entity);
        void Update(TEntity statusCheck);
        void Delete(TEntity entity);
    }
}
