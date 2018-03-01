using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Decred.Api.Controllers
{
    [Route("api/[controller]")]
    public class AddressController : Controller
    {
        [HttpGet("api/addresses/{address}/validity")]
        public IActionResult IsValid(string address)
        {
            throw new NotImplementedException();
        }
    }
}
