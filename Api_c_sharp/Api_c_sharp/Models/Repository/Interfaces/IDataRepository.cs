namespace Api_c_sharp.Models.Repository.Interfaces
{
    public interface IDataRepository<T, TKey> : ReadableRepository<T>, WritableRepository<T>, SearchableRepository<T, TKey> where T : class
    {

    }
}
