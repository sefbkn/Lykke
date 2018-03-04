using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.Decred.Api.Repository
{
    /// <summary>
    /// Wraps all
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IObservableOperationRepository<T>
    {
        Task InsertAsync(T value);       
        Task DeleteAsync(T value);
        Task<(IEnumerable<T> Entities, string ContinuationToken)> GetDataWithContinuationTokenAsync(int take, string continuation);
    }
}
