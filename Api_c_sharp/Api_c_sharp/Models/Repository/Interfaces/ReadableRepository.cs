namespace Api_c_sharp.Models.Repository.Interfaces
{
    public interface ReadableRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
    }
}
