using Lykke.Service.Decred.Api.Services;
using Newtonsoft.Json;

namespace Lykke.Service.Decred.Api
{
    public class AppSettings
    {
        public string Network { get; set; }        
        public AssetConfig Asset { get; set; }

        public string DcrdApiUrl { get; set; }
        public string DcrdRpcUser { get; set; }
        public string DcrdRpcPass { get; set; }
    }

    public class AssetConfig
    {
        public string AssetId { get; set; }
        public string Name { get; set; }
        public int Precision { get; set; }
    }
}
