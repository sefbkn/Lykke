using System;
using Common.Log;

namespace Lykke.Service.Decred_SignService.Client
{
    public class Decred_SignServiceClient : IDecred_SignServiceClient, IDisposable
    {
        private readonly ILog _log;

        public Decred_SignServiceClient(string serviceUrl, ILog log)
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
