using System;
using System.Threading.Tasks;
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
