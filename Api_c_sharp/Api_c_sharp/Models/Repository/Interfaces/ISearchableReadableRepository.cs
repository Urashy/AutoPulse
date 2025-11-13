namespace Api_c_sharp.Models.Repository.Interfaces
{
    public interface ISearchableReadableRepository<T> : ReadableRepository<T> where T : class
    {
        Task<T?> GetByNameAsync(string name);
    }
}
