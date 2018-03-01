using System;
using System.Threading.Tasks;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Balances;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Decred.Api.Controllers
{
    public class BalanceController : Controller
    {
        [HttpPost("api/balances/{address}/observation")]
        public async Task<IActionResult> Subscribe(string address)
        {
            throw new NotImplementedException();
        }
        
        [HttpDelete("api/balances/{address}/observation")]
        public async Task<IActionResult> Unsubscribe(string address)
        {
            throw new NotImplementedException();
        }

        [HttpGet("api/balances/")]
        public async Task<PaginationResponse<WalletBalanceContract>> GetBalances([FromQuery]int take, [FromQuery] string continuation)
        {
            throw new NotImplementedException();
        }
    }
}
