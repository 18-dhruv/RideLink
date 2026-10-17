public interface IRepo<TKey, TEntity> where TEntity : class, IEntity<TKey>
{
    Task Add(TEntity entity);
    Task<TEntity?> GetById(TKey key);
    Task<TEntity> RemoveById(TKey key);
}
