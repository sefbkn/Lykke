using System;
using System.Net;
using Lykke.Service.Decred.SignService.Models;
using Lykke.Service.Decred.SignService.Services;
using Microsoft.AspNetCore.Mvc;
using NDecred.Common;

namespace Lykke.Service.Decred.SignService.Controllers
{
    [Route("api/[controller]")]
    public class SignController : Controller
    {
        private readonly ISigningService _signingService;

        public SignController(ISigningService signingService)
        {
            _signingService = signingService;
        }
        
        [HttpPost]
        [ProducesResponseType(typeof(SignedTransactionResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public IActionResult Sign([FromBody] SignTransactionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse("Validation error", ModelState));

            try
            {
                var txBytes = HexUtil.ToByteArray(request.TransactionContext);
                var result = _signingService.SignRawTransaction(request.Keys, txBytes);
                var response = new SignedTransactionResponse {
                    SignedTransaction = result
                };
                
                return Ok(response);
            }
            
            catch (Exception e)
            {
                return BadRequest(new ErrorResponse("SigningError")
                {
                    Errors =
                    {
                        {"message", new[]{e.Message}}
                    }
                });
            }
        }
    }
}
