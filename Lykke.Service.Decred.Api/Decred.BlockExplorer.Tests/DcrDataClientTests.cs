using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;

namespace Decred.BlockExplorer.Tests
{
    public class DcrDataClientTests
    {
        /// <summary>
        /// Given a known response to an API call,
        /// ensure that the response object was deserialized correctly.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAddressTxRaw_GivenKnownResponse_DeserializesIntoExpectedValue()
        {
            var address = "Dcur2mcGjmENx4DhNqDctW5wJCVyT3Qeqkx";
            var baseUri = new Uri("http://0.0.0.0:7777");

            var messageHandler = new Mock<HttpMessageHandler>();
            var mockResponse = File.ReadAllText($"data/api.address.{address}.count.1.json");
            
            messageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", 
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(
                    new HttpResponseMessage
                    {
                        Content = new StringContent(mockResponse),
                        StatusCode = HttpStatusCode.OK
                    }
                ));

            
            AddressTxRaw[] results;
            using (var httpClient = new HttpClient(messageHandler.Object))
            {
                var client = new DcrdataClient(httpClient, baseUri);
                results = await client.GetAddressTxRawAsync(address);
            }

            Assert.Equal(1, results.Length);
            Assert.Equal(1, results.Single().Vin.Length);
            Assert.Equal(3, results.Single().Vout.Length);
            
            var result = results.Single();
            Assert.Equal("00000000000000086a3651f0355039b771087844cdf067ea36ed292451d080ca", result.BlockHash);
            Assert.Equal(1519959559, result.Blocktime);
            Assert.Equal(1519959559, result.Time);
            Assert.Equal((uint) 0, result.Locktime);
            Assert.Equal(2, result.Confirmations);
            Assert.Equal(1, result.Version);
            Assert.Equal(200, result.Size);
            Assert.Equal("520d23f40d9dddbc4e4c763f0c89071ca95b3af41a85358409a5f499a325ac87", result.TxId);

            var vin = result.Vin.First();
            Assert.Equal("00002f646372642f", vin.Coinbase);
            Assert.Equal(15.41510377m, vin.AmountIn);
            Assert.Equal(4294967295, vin.Sequence);
        }

        [Fact]
        public async Task TestActualCall()
        {
            var address = "Dcur2mcGjmENx4DhNqDctW5wJCVyT3Qeqkx";
            var baseUri = new Uri("https://dcrdata.org");
            using (var httpClient = new HttpClient())
            {
                var client = new DcrdataClient(httpClient, baseUri);
                var result = await client.GetAddressBalance(address);
                Assert.Equal(0, result);
            }
        }
    }
}
