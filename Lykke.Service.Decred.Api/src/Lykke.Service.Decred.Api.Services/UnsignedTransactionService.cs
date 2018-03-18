using System.Threading.Tasks;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using Lykke.Service.Decred.Api.Common.Entity;
using Lykke.Service.Decred.Api.Repository;
using Newtonsoft.Json;

namespace Lykke.Service.Decred.Api.Services
{
    public class UnsignedTransactionService
    {
        private readonly TransactionBuilder _builder;
        private readonly IObservableOperationRepository<UnsignedTransactionEntity> _unsignedTxRepo;

        public UnsignedTransactionService(
            TransactionBuilder builder,
            IObservableOperationRepository<UnsignedTransactionEntity> unsignedTxRepo)
        {
            _builder = builder;
            _unsignedTxRepo = unsignedTxRepo;
        }

        public async Task<BuildTransactionResponse> BuildSingleTransactionAsync(BuildSingleTransactionRequest request,
            decimal feeFactor)
        {
            var unsignedTx = await _unsignedTxRepo.GetAsync(request.OperationId.ToString());
            if (unsignedTx?.ResponseJson != null)
                return JsonConvert.DeserializeObject<BuildTransactionResponse>(unsignedTx.ResponseJson);

            var response = await _builder.BuildSingleTransactionAsync(request, feeFactor);

            var entity = new UnsignedTransactionEntity(
                JsonConvert.SerializeObject(request),
                JsonConvert.SerializeObject(response)
            );

            await _unsignedTxRepo.InsertAsync(entity);

            return response;
        }
    }
}
