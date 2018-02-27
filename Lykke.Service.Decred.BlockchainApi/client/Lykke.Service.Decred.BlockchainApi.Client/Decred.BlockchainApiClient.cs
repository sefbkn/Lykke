using System;
using Common.Log;

namespace Lykke.Service.Decred_BlockchainApi.Client
{
    public class Decred_BlockchainApiClient : IDecred_BlockchainApiClient, IDisposable
    {
        private readonly ILog _log;

        public Decred_BlockchainApiClient(string serviceUrl, ILog log)
        {
            _log = log;
        }

        public void Dispose()
        {
            //if (_service == null)
            //    return;
            //_service.Dispose();
            //_service = null;
        }
    }
}
