using System.Threading.Tasks;

namespace Lykke.Service.Decred.Api.Services
{
    public interface IObservableRepository<TKey, TVal>
    {
        Task Delete(TKey key);
        Task Upsert(TKey key, TVal val);
        Task<AddressBalance> Get(TKey key);
        Task<PaginationResult<TVal>> List(int take, string continuation);
    }
}