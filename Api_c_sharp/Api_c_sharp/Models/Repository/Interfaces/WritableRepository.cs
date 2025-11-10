namespace Api_c_sharp.Models.Repository.Interfaces
{
    public interface WritableRepository<T> where T : class
    {
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
    }
}
