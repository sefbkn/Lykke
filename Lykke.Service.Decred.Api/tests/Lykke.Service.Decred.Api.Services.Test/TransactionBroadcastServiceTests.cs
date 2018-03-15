using System.Threading.Tasks;
using WebSocketSharp;
using Xunit;

namespace Lykke.Service.Decred.Api.Services.Test
{
    public class TransactionBroadcastServiceTests
    {
        [Fact]
        public async Task Test()
        {
            using (var ws = new WebSocket ("wss://0.0.0.0:19109/ws")) 
            {
                ws.Connect();
                ws.Send("Test1234");
            }
        }
    }
}
