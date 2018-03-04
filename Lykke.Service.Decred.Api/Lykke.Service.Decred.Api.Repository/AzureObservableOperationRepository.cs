using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.Decred.Api.Common;
using Lykke.Service.Decred.Api.Services;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.Decred.Api.Repository
{
    public class AzureObservableOperationRepository<T> : IObservableOperationRepository<T> where T : TableEntity, new()
    {
        private const int RecordNotFoundStatus = 404;
        private const int DuplicateRecordStatus = 409;

        private readonly INoSQLTableStorage<T> _azureRepo;
        
        public AzureObservableOperationRepository(INoSQLTableStorage<T> azureRepo)
        {
            _azureRepo = azureRepo;
        }

        public async Task InsertAsync(T value)
        {
            try
            {
                await _azureRepo.InsertAsync(value);
            }
            catch (StorageException e) when (e.RequestInformation.HttpStatusCode == DuplicateRecordStatus)
            {
                throw new BusinessException(ErrorReason.DuplicateRecord, $"{typeof(T)} already being observed", e);
            }
        }

        public async Task DeleteAsync(T value)
        {
            try
            {
                value.ETag = "*";
                await _azureRepo.DeleteAsync(value);
            }
            catch (StorageException ex) when(ex.RequestInformation.HttpStatusCode == RecordNotFoundStatus)
            {
                throw new BusinessException(ErrorReason.RecordNotFound, $"{typeof(T)} is not being observed", ex);
            }
        }

        public async Task<(IEnumerable<T> Entities, string ContinuationToken)> GetDataWithContinuationTokenAsync(int take, string continuation)
        {
            return await _azureRepo.GetDataWithContinuationTokenAsync(take, continuation);
        }
    }
}
