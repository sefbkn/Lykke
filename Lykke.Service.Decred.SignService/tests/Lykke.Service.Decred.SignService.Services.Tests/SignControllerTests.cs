using Lykke.Service.Decred.SignService.Controllers;
using Lykke.Service.Decred.SignService.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NDecred.Common;
using Xunit;

namespace Lykke.Service.Decred.SignService.Services.Tests
{
    public class SignControllerTests
    {
        [Fact]
        public void SignRawTransaction_GivenUnsignedTx_ReturnsCorrectlySignedTx()
        {
            var request = new SignTransactionRequest()
            {
                Keys = new[] {"private_key1", "private_key2"},
                TransactionContext = "00000000"
            };
            
            var txBytes = HexUtil.ToByteArray(request.TransactionContext);
            
            var mockSigningService = new Mock<ISigningService>();
            mockSigningService.Setup(m => m.SignRawTransaction(request.Keys, txBytes))
                .Returns("signed_tx");
            
            var signController = new SignController(mockSigningService.Object);

            var result = signController.Sign(request) as ObjectResult;
            var response = result.Value as SignedTransactionResponse;
            Assert.Equal("signed_tx", response.SignedTransaction);
        }
    }
}