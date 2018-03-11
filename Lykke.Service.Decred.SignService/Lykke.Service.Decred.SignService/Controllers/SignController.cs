using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lykke.Service.Decred.SignService.Models;
using Lykke.Service.Decred.SignService.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Decred.SignService.Controllers
{
    [Route("api/[controller]")]
    public class SignController : Controller
    {
        private readonly ITransactionService _txService;

        public SignController(ITransactionService txService)
        {
            _txService = txService;
        }
        
        [HttpPost]
        [ProducesResponseType(typeof(SignedTransactionResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Sign([FromBody] SignTransactionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse("Validation error", ModelState));

            try
            {
                var result = await _txService.SignAsync(request);
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(new ErrorResponse("SigningError"));
            }
        }
    }
}
