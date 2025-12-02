namespace BlazorAutoPulse.Service.Interface;

public interface IService<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> GetByIdAsync(int id);
    Task<T> CreateAsync(T entity);
    Task UpdateAsync(int id, T entity);
    Task DeleteAsync(int id);
    public Task<ServiceResult<T>> PostWithErrorHandlingAsync(T entity, string action = "Post");
}