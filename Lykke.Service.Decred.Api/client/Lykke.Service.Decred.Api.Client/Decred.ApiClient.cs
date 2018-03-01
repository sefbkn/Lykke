using System;
using Common.Log;

namespace Lykke.Service.Decred_Api.Client
{
    public class Decred_ApiClient : IDecred_ApiClient, IDisposable
    {
        private readonly ILog _log;

        public Decred_ApiClient(string serviceUrl, ILog log)
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
