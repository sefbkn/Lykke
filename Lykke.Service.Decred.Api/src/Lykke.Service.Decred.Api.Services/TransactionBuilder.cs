using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Decred.BlockExplorer;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using Lykke.Service.Decred.Api.Common;
using NDecred.Common;
using Paymetheus.Decred;
using Paymetheus.Decred.Wallet;

namespace Lykke.Service.Decred.Api.Services
{
    public interface ITransactionBuilder
    {
        Task<BuildTransactionResponse> BuildSingleTransactionAsync(
            BuildSingleTransactionRequest request,
            decimal feeFactor);
    }
    
    public class TransactionBuilder : ITransactionBuilder
    {
        private readonly ITransactionFeeService _feeService;
        private readonly ITransactionRepository _txRepo;

        public TransactionBuilder(
            ITransactionFeeService feeService, 
            ITransactionRepository txRepo)
        {
            _feeService = feeService;
            _txRepo = txRepo;
        }

        private long DcrToAtoms(string dcrQuantity)
        {
            var qty = decimal.Parse(dcrQuantity);
            return (long)(qty * (decimal) Math.Pow(10, 8));
        }
        
        /// <summary>
        /// Builds a transaction that sends value from one address to another.
        /// Change is spent to the source address, if necessary.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="feeFactor"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task<BuildTransactionResponse> BuildSingleTransactionAsync(BuildSingleTransactionRequest request, decimal feeFactor)
        {
            const uint sequence = uint.MaxValue;
            const int outputVersion = 0;
            const int lockTime = 0;
            const int expiry = 0;

            // Number of outputs newly build tx will contain,
            // not including the change address
            const int numOutputs = 1;

            // Lykke api doesn't have option to specify a change address.
            var changeAddress = Address.Decode(request.FromAddress);
            var toAddress = Address.Decode(request.ToAddress);

            // Get all unspent transaction outputs to address
            // and map as inputs to new transaction
            var allInputs = 
               (from output in await _txRepo.GetUnspentTxOutputs(request.FromAddress)
                let txHash = new Blake256Hash(HexUtil.ToByteArray(output.Hash).Reverse().ToArray())
                let outpoint = new Transaction.OutPoint(txHash, output.OutputIndex, output.Tree)
                select new Transaction.Input(
                    outpoint,
                    sequence,
                    output.OutputValue,
                    output.BlockHeight,
                    output.BlockIndex,
                    output.PkScript
                )).ToArray();
            
            // The amount that is being requested.
            var amount = DcrToAtoms(request.Amount);

            long estFee = 0;
            long totalSpent = 0;
            var consumedInputs = new List<Transaction.Input>();
            var feePerKb = await _feeService.GetFeePerKb();
            
            // Accumulate inputs until we have enough to cover the cost
            // of the amount + fee
            foreach (var input in allInputs)
            {
                consumedInputs.Add(input);
                totalSpent += input.InputAmount;

                estFee = _feeService.CalculateFee(feePerKb, consumedInputs.Count, numOutputs, feeFactor);
                var estAmount = amount + (request.IncludeFee ? 0 : estFee);
                
                if (totalSpent == estAmount)
                {
                    break;
                }
                
                if (totalSpent > estAmount)
                {
                    // If there will be change, adjust fee calculation to account for this.
                    estFee = _feeService.CalculateFee(feePerKb, consumedInputs.Count, numOutputs+1, feeFactor);
                    break;
                }
            }

            // The fee either comes from the change or the sent amount
            var send = amount - (request.IncludeFee ? estFee : 0 );
            var change = (totalSpent - amount) - (request.IncludeFee ? 0 : estFee);

            // Do not build transactions that are too small.
            if (TransactionRules.IsDustAmount(change, Transaction.PayToPubKeyHashPkScriptSize, new Amount(feePerKb)))
                throw new BusinessException(ErrorReason.AmountTooSmall, "Amount is dust");

            // If all inputs do not have enough value to fund the transaction, throw error.
            if(request.IncludeFee &&  estFee > amount)
                throw new BusinessException(ErrorReason.AmountTooSmall, "Amount not enough to include fee");

            // If all inputs do not have enough value to fund the transaction, throw error.
            if(totalSpent < amount + (request.IncludeFee ? 0 : estFee))
                throw new BusinessException(ErrorReason.NotEnoughBalance, "Address balance too low");

            // Build outputs to address + change address.
            // If any of the outputs is zero value, exclude it.  For example, if there is no change.
            var outputs = new[] {
                new Transaction.Output(send, outputVersion, toAddress.BuildScript().Script),
                new Transaction.Output(change, outputVersion, changeAddress.BuildScript().Script)
            }.Where(o => o.Amount != 0).ToArray();

            var newTx = new Transaction(
                Transaction.SupportedVersion,
                consumedInputs.ToArray(), 
                outputs, 
                lockTime, 
                expiry
            );
            
            return new BuildTransactionResponse
            {
                TransactionContext = HexUtil.FromByteArray(newTx.Serialize())
            };
        }
    }
}
