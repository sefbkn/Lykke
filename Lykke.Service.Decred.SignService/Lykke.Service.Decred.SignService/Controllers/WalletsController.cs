using System;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Lykke.Service.Decred.SignService.Models;
using Lykke.Service.Decred.SignService.Services;
using Lykke.Service.Decred_SignService.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Lykke.Service.Decred.SignService.Controllers
{
    
    [Route("api/[controller]")]
    public class WalletsController : Controller
    {
        private readonly IWalletProvider _walletProvider;

        public WalletsController(IWalletProvider walletProvider)
        {
            _walletProvider = walletProvider;
        }
        
        [HttpPost]
        public WalletCreationResponse CreateWallet()
        {
            return _walletProvider.CreateNewWallet();
        }
    }
}
