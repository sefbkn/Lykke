using System;
using System.Threading.Tasks;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Balances;
using Lykke.Service.Decred.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Decred.Api.Controllers
{
    public class BalanceController : Controller
    {
        private readonly BalanceService _service;

        public BalanceController(BalanceService service)
        {
            _service = service;
        }
        
        /// <summary>
        /// Start watching the address for balance changes
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        [HttpPost("api/balances/{address}/observation")]
        public async Task<IActionResult> Subscribe(string address)
        {
            await _service.SubscribeAsync(address);
            return Ok();
        }
        
        /// <summary>
        /// Stop watching the address for balance changes
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        [HttpDelete("api/balances/{address}/observation")]
        public async Task<IActionResult> Unsubscribe(string address)
        {
            await _service.UnsubscribeAsync(address);
            return Ok();
        }

        /// <summary>
        /// Retrieve paged balances for all addresses.
        /// </summary>
        /// <param name="take"></param>
        /// <param name="continuation"></param>
        /// <returns></returns>
        [HttpGet("api/balances/")]
        public async Task<PaginationResponse<WalletBalanceContract>> GetBalances([FromQuery]int take, [FromQuery] string continuation)
        {
            return await _service.GetBalancesAsync(take, continuation);
        }
    }
}
