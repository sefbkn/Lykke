using System;
using System.Threading.Tasks;
using Lykke.Common.Api.Contract.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Decred.Api.Controllers
{
    public class StatusController : Controller
    {
        [HttpGet("/api/isalive")]
        public async Task<IActionResult> GetStatus()
        {
            throw new NotImplementedException();
        }
    }
}
