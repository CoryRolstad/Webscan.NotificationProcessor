using System.Collections.Generic;

namespace Webscan.NotificationProcessor.Models.Repository
{
    public interface IUserRepository<TEntity>
    {
        IEnumerable<TEntity> GetAll();
        TEntity Get(int id);
        void Add(TEntity entity);
        void Update(TEntity dbEntity, TEntity entity);
        void Delete(TEntity entity);
    }
}
