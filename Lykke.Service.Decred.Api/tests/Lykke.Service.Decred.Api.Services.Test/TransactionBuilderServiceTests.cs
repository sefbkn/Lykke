﻿using System.Linq;
using System.Threading.Tasks;
using Decred.BlockExplorer;
using Decred.Common.Client;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using Moq;
using NDecred.Common;
using Paymetheus.Decred;
using Xunit;

namespace Lykke.Service.Decred.Api.Services.Test
{
    public class TransactionBuilderTests
    {
        private readonly Mock<ITransactionRepository> _mockTxRepo;
        private readonly Mock<IDcrdClient> _mockDcrdClient;
        
        public TransactionBuilderTests()
        {
            _mockDcrdClient = new Mock<IDcrdClient>();
            _mockTxRepo = new Mock<ITransactionRepository>();
        }

        [Fact]
        public async Task BuildSingleTransactionAsync_WithSingleUnspentOutput_BuildsExpectedTx()
        {
            var fromAddr = "TscuZZicesEnx3H6sJ5hizmrjEcMv4iWA7k";
            var toAddr = "Tsb44YQ7QPuxqDimNMRF4QjcBTxw8nnsfP9";
            
            // Send 1 decred out of 2 total
            var amountToSend = 100000000;
            var unspentOutput = new UnspentTxOutput()
            {
                BlockHeight = 0,
                BlockIndex = 0,
                Hash = HexUtil.FromByteArray(new byte[32]),
                OutputIndex = 1,
                OutputValue = 2 * 100000000,
                OutputVersion = 0,
                Tree = 0,
                PkScript = new byte[0]
            };
            
            _mockTxRepo.Setup(m => m.GetUnspentTxOutputs(fromAddr)).ReturnsAsync(new[]{unspentOutput});
            _mockDcrdClient.Setup(m => m.EstimateFeeAsync(It.IsAny<int>())).ReturnsAsync(0.001m);
            
            var txFeeService = new TransactionFeeService(_mockDcrdClient.Object);
            var subject = new TransactionBuilder(
                txFeeService,
                _mockTxRepo.Object);
            
            var request = new BuildSingleTransactionRequest
            {
                Amount = amountToSend.ToString(),
                AssetId = "DCR",
                FromAddress = fromAddr,
                ToAddress = toAddr,
                IncludeFee = true
            };

            var result = await subject.BuildSingleTransactionAsync(request, 1);
            var transaction = Transaction.Deserialize(HexUtil.ToByteArray(result.TransactionContext));
            var expectedFee = txFeeService.CalculateFee(0.001m, 1, 2, 1);
            
            Assert.Equal(expectedFee, 2 * 100000000 - transaction.Outputs.Sum(o => o.Amount));
            Assert.Equal(2, transaction.Outputs.Length);
            
            // Sent amount - fee
            Assert.Equal(1, transaction.Outputs.Count(o => o.Amount + expectedFee == 100000000));
            
            // Change
            Assert.Equal(1, transaction.Outputs.Count(o => o.Amount == 100000000));
        }
    }
}
