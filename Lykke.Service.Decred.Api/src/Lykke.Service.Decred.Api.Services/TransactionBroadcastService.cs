using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Decred.BlockExplorer;
using Decred.Common.Client;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using Lykke.Service.Decred.Api.Common;
using Lykke.Service.Decred.Api.Common.Entity;
using Lykke.Service.Decred.Api.Repository;
using NDecred.Common;
using Newtonsoft.Json;

namespace Lykke.Service.Decred.Api.Services
{
    public interface ITransactionBroadcastService
    {
        Task Broadcast(Guid operationId, string hexTransaction);
        Task UnsubscribeBroadcastedTx(Guid operationId);
        Task<BroadcastedSingleTransactionResponse> GetBroadcastedTxSingle(Guid operationId);
    }
    
    public class TransactionBroadcastService : ITransactionBroadcastService
    {
        private readonly IDcrdClient _dcrdClient;
        private readonly IBlockRepository _blockRepository;
        private readonly ITransactionRepository _txRepo;
        
        private readonly IObservableOperationRepository<BroadcastedTransaction> _broadcastTxRepo;
        private readonly IObservableOperationRepository<BroadcastedTransactionByHash> _broadcastTxHashRepo;

        public TransactionBroadcastService(
            IDcrdClient dcrdClient, 
            IBlockRepository blockRepository,
            ITransactionRepository txRepo,
            IObservableOperationRepository<BroadcastedTransaction> broadcastTxRepo,
            IObservableOperationRepository<BroadcastedTransactionByHash> broadcastTxHashRepo)
        {
            _dcrdClient = dcrdClient;
            _blockRepository = blockRepository;
            _txRepo = txRepo;
            _broadcastTxRepo = broadcastTxRepo;
            _broadcastTxHashRepo = broadcastTxHashRepo;
        }
        
        /// <summary>
        /// Broadcasts a signed transaction to the Decred network
        /// </summary>
        /// <param name="operationId"></param>
        /// <param name="hexTransaction"></param>
        /// <returns></returns>
        /// <exception cref="TransactionBroadcastException"></exception>
        public async Task Broadcast(Guid operationId, string hexTransaction)
        {
            // If the operation exists in the cache, throw exception
            var cachedResult = await _broadcastTxRepo.GetAsync(operationId.ToString());
            if (cachedResult != null)
                throw new BusinessException(ErrorReason.DuplicateRecord);

            // Submit the transaction to the network via dcrd
            var result = await _dcrdClient.SendRawTransactionAsync(hexTransaction);                
            if (result.Error != null)
                throw new TransactionBroadcastException($"[{result.Error.Code}] {result.Error.Message}");
            
            // Calculate the hash to perform lookups with later. 
            var transaction = new MsgTx();
            transaction.Decode(HexUtil.ToByteArray(hexTransaction));
            var txHash = HexUtil.FromByteArray(transaction.GetHash().Reverse().ToArray());
            
            await SaveBroadcastedTransaction(new BroadcastedTransaction
            {
                OperationId = operationId,
                Hash = txHash,
                EncodedTransaction = hexTransaction
            });
            
        }
        
        /// <summary>
        /// Determines the state of a broadcasted transaction
        /// </summary>
        /// <param name="operationId"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task<BroadcastedSingleTransactionResponse> GetBroadcastedTxSingle(Guid operationId)
        {
            // Retrieve the broadcasted transaction and deserialize it.
            var broadcastedTransaction = await GetBroadcastedTransaction(operationId);
            var transaction = new MsgTx();
            transaction.Decode(HexUtil.ToByteArray(broadcastedTransaction.EncodedTransaction));
            
            // Calculate the fee and total amount spent from the transaction.
            var fee = transaction.TxIn.Sum(t => t.ValueIn) - transaction.TxOut.Sum(t => t.Value);
            var amount = transaction.TxOut.Sum(t => t.Value);
            
            // Check to see if the transaction has been included in a block.
            var knownTx = await _txRepo.GetTxInfoByHash(broadcastedTransaction.Hash);
            var topBlock = await _blockRepository.GetHighestBlock();
            var txState = knownTx == null
                ? BroadcastedTransactionState.InProgress
                : BroadcastedTransactionState.Completed;

            // If the tx has been included in a block,
            // use the block height + timestamp from the block
            var blockHeight = knownTx?.BlockHeight ?? topBlock.Height;
            var timestamp = knownTx == null ? DateTime.UtcNow : DateTimeUtil.FromUnixTime(knownTx.BlockTime);
            
            return new BroadcastedSingleTransactionResponse
            {
                Block = blockHeight,
                State = txState,
                Hash = broadcastedTransaction.Hash,
                Amount = amount.ToString(),
                Fee = fee.ToString(),
                Error = "",
                ErrorCode = null,
                OperationId = operationId,
                Timestamp = timestamp
            };
        }

        public async Task UnsubscribeBroadcastedTx(Guid operationId)
        {
            var operation = await _broadcastTxRepo.GetAsync(operationId.ToString());
            if (operation == null)
                throw new BusinessException(ErrorReason.RecordNotFound);
            
            await _broadcastTxRepo.DeleteAsync(operation);
        }
        
        private async Task SaveBroadcastedTransaction(BroadcastedTransaction broadcastedTx)
        {            
            // Store tx Hash to OperationId lookup
            await _broadcastTxHashRepo.InsertAsync(
                new BroadcastedTransactionByHash
                {
                    Hash = broadcastedTx.Hash,
                    OperationId = broadcastedTx.OperationId
                }
            );

            // Store operation
            await _broadcastTxRepo.InsertAsync(broadcastedTx);
        }

        private async Task<BroadcastedTransaction> GetBroadcastedTransaction(Guid operationId)
        {
            // Retrieve previously saved BroadcastedTransaction record.
            var broadcastedTx = await _broadcastTxRepo.GetAsync(operationId.ToString());
            return broadcastedTx ?? throw new BusinessException(ErrorReason.RecordNotFound);
        }
    }
}
