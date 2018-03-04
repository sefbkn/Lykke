using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.Decred.Api.Services;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.Decred.Api.Repository
{
    public class AzureObservableOperationRepository<T> : IObservableOperationRepository<T> where T : TableEntity, new()
    {
        private readonly INoSQLTableStorage<T> _azureRepo;
        
        public AzureObservableOperationRepository(INoSQLTableStorage<T> azureRepo)
        {
            _azureRepo = azureRepo;
        }

        public async Task InsertAsync(T value)
        {
            await _azureRepo.InsertAsync(value);
        }

        public async Task DeleteAsync(T value)
        {
            await _azureRepo.DeleteAsync(value);
        }

        public async Task<(IEnumerable<T> Entities, string ContinuationToken)> GetDataWithContinuationTokenAsync(int take, string continuation)
        {
            return await _azureRepo.GetDataWithContinuationTokenAsync(take, continuation);
        }
    }
}
