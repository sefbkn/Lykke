using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RichardSzalay.MockHttp;
using Xunit;

namespace DcrdClient.Tests
{
    public class DcrdHttpClientTests
    {
        private readonly string _baseUrl = "http://test.url";
        private readonly MockHttpMessageHandler _messageHandler;
        private readonly DcrdHttpClient _subject;

        public DcrdHttpClientTests()
        {
            _messageHandler = new MockHttpMessageHandler();
            _subject = new DcrdHttpClient("http://test.url", _messageHandler);
        }

        [Fact]
        public void PingAsync_SendsExpectedRequest()
        {
            var requestBody = BuildRequestBody("ping");
            _messageHandler.Expect(_baseUrl).WithContent(requestBody);
            _subject.PingAsync();
            _messageHandler.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public void EstimateFeeAsync_SendsExpectedRequest()
        {
            var requestBody = BuildRequestBody("estimatefee", 1);
            _messageHandler.Expect(_baseUrl).WithContent(requestBody);
            _subject.EstimateFeeAsync(1);
            _messageHandler.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public void SearchRawTransactions_SendsExpectedRequest()
        {
            var requestBody = BuildRequestBody("searchrawtransactions", "address", 1, 1, 2, 3, false);
            _messageHandler.Expect(_baseUrl).WithContent(requestBody);
            _subject.SearchRawTransactions(
                address: "address",
                skip: 1,
                count: 2,
                vinExtra: 3,
                reverse: false);
            
            _messageHandler.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task Perform_WithErrorResponse_ParsesError()
        {
            var error = File.ReadAllText("./data/dcrd_error_response.json");

            _messageHandler
                .Expect(_baseUrl)
                .Respond(new HttpResponseMessage {
                    Content = new StringContent(error),
                    StatusCode = (HttpStatusCode) 200
                });

            var result = await _subject.PerformAsync<SearchRawTransactionsResult[]>("test");
            _messageHandler.VerifyNoOutstandingExpectation();

            Assert.NotNull(result);
            Assert.Null(result.Result);
            Assert.Equal(-32603, result.Error.Code);
            Assert.Equal("No Txns available", result.Error.Message);
            Assert.Equal("0", result.Id);
            Assert.Equal("1.0", result.Jsonrpc);
        }

        private static string BuildRequestBody(string method, params object[] parameters)
        {
            return JsonConvert.SerializeObject(new
            {
                jsonrpc = "1.0",
                id = "0",
                method = method,
                @params = parameters
            });
        }
    }
}
