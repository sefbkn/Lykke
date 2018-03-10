using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Lykke.Service.Decred.Api.Services;
using Newtonsoft.Json;

namespace Lykke.Service.Decred.Api
{
    public class AppSettings
    {
        public ApiConfig ApiConfig { get; set; }
    }
    
    public class ApiConfig
    {
        public AssetConfig Asset { get; set; }
        public NetworkSettings NetworkSettings { get; set; }
        public NetworkServices NetworkServices { get; set; }
    }

    public class AssetConfig
    {
        public string AssetId { get; set; }
        public string Name { get; set; }
        public int Precision { get; set; }
    }

    public class NetworkServices
    {
        public class Config
        {
            public string Host { get; set; }
            public int Port { get; set; }
        }

        [JsonProperty("dcrd")]
        public Config Dcrd { get; set; }
        
        [JsonProperty("dcrwallet")]
        public Config DcrWallet { get; set; }
        
        [JsonProperty("dcrdata")]
        public Config DcrData { get; set; }
    }
}
