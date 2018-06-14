using System;
using System.Net;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Balances;
using Lykke.Service.Decred.Api.Common;
using Lykke.Service.Decred.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Decred.Api.Controllers
{
    public class BalanceController : Controller
    {
        private readonly ILog _log;
        private readonly BalanceService _service;

        public BalanceController(ILog log, BalanceService service)
        {
            _log = log;
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
            try
            {
                await _service.SubscribeAsync(address);
                return Ok();
            }
            catch (BusinessException e) when (e.Reason == ErrorReason.DuplicateRecord)
            {
                Response.StatusCode = (int) HttpStatusCode.Conflict;
                return Json(new {errorMessage = "Address already being observed"});
            }
        }
        
        /// <summary>
        /// Stop watching the address for balance changes
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        [HttpDelete("api/balances/{address}/observation")]
        public async Task<IActionResult> Unsubscribe(string address)
        {
            try
            {
                await _service.UnsubscribeAsync(address);
                return Ok();
            }
            catch (BusinessException e) when (e.Reason == ErrorReason.RecordNotFound)
            {
                return NoContent();
            }
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
        
        private async Task<JsonResult> GenericErrorResponse(Exception ex, Guid operationId, HttpStatusCode status)
        {
            Response.StatusCode = (int) status;
            await _log.WriteErrorAsync(nameof(TransactionController), nameof(GenericErrorResponse), operationId.ToString(), ex);
            return Json(new { errorMessage = ex.ToString() });
        }
    }
}
