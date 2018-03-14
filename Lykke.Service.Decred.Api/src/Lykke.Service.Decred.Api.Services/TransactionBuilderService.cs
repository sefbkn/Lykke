﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Decred.BlockExplorer;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using Lykke.Service.Decred.Api.Common;
using Paymetheus.Decred;
using Paymetheus.Decred.Wallet;

namespace Lykke.Service.Decred.Api.Services
{
    public class TransactionBuilderService
    {
        private readonly ITransactionFeeService _feeService;
        private readonly ITransactionRepository _txRepo;

        public TransactionBuilderService(ITransactionFeeService feeService, ITransactionRepository txRepo)
        {
            _feeService = feeService;
            _txRepo = txRepo;
        }
        
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
                let txHash = new Blake256Hash(HexUtil.BytesFromHexString(output.Hash))
                let outpoint = new Transaction.OutPoint(txHash, output.OutputIndex, output.Tree)
                select new Transaction.Input(
                    outpoint,
                    sequence,
                    output.OutputValue,
                    output.BlockHeight,
                    output.BlockIndex,
                    output.PkScript
                )).ToArray();

            
            var amount = long.Parse(request.Amount);

            long estFee = 0;
            long totalSpent = 0;
            var consumedInputs = new List<Transaction.Input>();
            foreach (var input in allInputs)
            {
                consumedInputs.Add(input);
                totalSpent += input.InputAmount;
                estFee = _feeService.CalculateFee(consumedInputs.Count, numOutputs, feeFactor); 
                
                // Accumulate inputs until we have enough to cover the cost
                // of the amount + fee
                if (totalSpent > amount + (request.IncludeFee ? 0 : estFee))
                    break;
            }
            
            if(totalSpent < amount + (request.IncludeFee ? 0 : estFee))
                throw new BusinessException(ErrorReason.NotEnoughBalance, "Address balance too low");
            
            // The fee either comes from the change or the sent amount
            var send = amount - (request.IncludeFee ? estFee : 0 );
            var change = (totalSpent - amount) - (request.IncludeFee ? 0 : estFee);
            
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
                TransactionContext = HexUtil.BytesToHexString(newTx.Serialize())
            };
        }
    }
    
    public static class HexUtil
    {
        /// <summary>
        ///     Converts a string of hexadecimal characters to a byte[]
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static byte[] BytesFromHexString(string hex)
        {
            return Enumerable.Range(0, hex.Length).Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16)).ToArray();
        }

        /// <summary>
        ///     Converts a byte[] to a string of lowercase hexadecimal characters
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string BytesToHexString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", string.Empty).ToLower();
        }
    }

}