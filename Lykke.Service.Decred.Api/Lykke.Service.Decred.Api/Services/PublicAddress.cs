using Newtonsoft.Json;

namespace Lykke.Service.Decred.Api.Services
{
    public class PublicAddress
    {
        [JsonProperty("assetId")]
        public string AssetId => "DCR";

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("balance")]
        public string Balance { get; set; }
    }
}