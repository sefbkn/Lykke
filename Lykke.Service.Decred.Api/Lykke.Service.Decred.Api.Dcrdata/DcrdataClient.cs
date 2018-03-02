using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Lykke.Service.Decred.Api.Dcrdata
{
    /// <summary>
    /// Web socket client for dcrdata.
    /// </summary>
    public class DcrdataClient
    {
        private readonly Uri _apiEndpoint;

        public DcrdataClient(Uri apiEndpoint)
        {
            _apiEndpoint = apiEndpoint;
        }
    }
}
