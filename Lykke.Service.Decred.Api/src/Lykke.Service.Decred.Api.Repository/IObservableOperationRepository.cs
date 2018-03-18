using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.Decred.Api.Repository
{
    /// <summary>
    /// Wraps all observable repositories.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IObservableOperationRepository<T>
    {
        Task<T> GetAsync(string key);
        Task<IEnumerable<T>> GetAsync(IEnumerable<string> keys);
        
        Task InsertAsync(T entity);
        Task DeleteAsync(T entity);
        Task<(IEnumerable<T> Entities, string ContinuationToken)> GetDataWithContinuationTokenAsync(int take, string continuation);
    }
}
