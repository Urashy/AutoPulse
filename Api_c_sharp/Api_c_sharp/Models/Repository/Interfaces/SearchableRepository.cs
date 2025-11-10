namespace Api_c_sharp.Models.Repository.Interfaces
{
    public interface SearchableRepository<T> : IRepository<T> where T : class
    {
        Task<T?> GetByNameAsync(string name);
    }
}
