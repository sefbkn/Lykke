using System;
using System.Net;
using Lykke.Service.Decred.SignService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.PlatformAbstractions;
using NDecred.Common;

namespace Lykke.Service.Decred.SignService.Controllers
{
    [Route("api/[controller]")]
    public class IsAliveController : Controller
    {
        private readonly Network _network;
#if DEBUG
        private const bool IsDebug = true;
#else
        private const bool IsDebug = false;
#endif
        public IsAliveController(Network network)
        {
            _network = network;
        }
        
        [HttpGet]
        [ProducesResponseType(typeof(IsAliveResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.InternalServerError)]
        public IsAliveResponse Get()
        {
            return new IsAliveResponse(
                PlatformServices.Default.Application.ApplicationName,
                PlatformServices.Default.Application.ApplicationVersion,
                _network.Name,
                IsDebug
            );
        }
    }
}
