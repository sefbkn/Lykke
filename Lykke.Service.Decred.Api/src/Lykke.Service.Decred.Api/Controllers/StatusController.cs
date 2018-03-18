using System;
using System.Threading.Tasks;
using Common.Log;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Decred.Api.Controllers
{
    public class StatusController : Controller
    {
        private readonly ILog _log;

        public StatusController(ILog log)
        {
            _log = log;
        }
        
        [HttpGet("/api/isalive")]
        public async Task<IActionResult> GetStatus()
        {
            throw new NotImplementedException();
        }
    }
}
