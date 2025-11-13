namespace Api_c_sharp.Models.Repository.Interfaces
{
    public interface SearchableRepository<T, TKey> 
    {
        Task<T?> GetByNameAsync(TKey key);
    }
}
