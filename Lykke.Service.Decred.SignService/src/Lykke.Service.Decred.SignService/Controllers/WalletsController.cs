using Lykke.Service.Decred.SignService.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Decred.SignService.Controllers
{
    
    [Route("api/[controller]")]
    public class WalletsController : Controller
    {
        private readonly IKeyService _keyService;

        public WalletsController(IKeyService keyService)
        {
            _keyService = keyService;
        }
        
        [HttpPost]
        public WalletCreationResponse CreateWallet()
        {
            return _keyService.Create();
        }
    }
}
