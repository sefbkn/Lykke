using Lykke.Service.Decred.Api.Services;
using Lykke.SettingsReader.Attributes;
using Newtonsoft.Json;

namespace Lykke.Service.Decred.Api
{
    public class AppSettings
    {
        public string NetworkType { get; set; }
        public ServiceSettings ServiceSettings { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
        public ConnectionStrings ConnectionStrings { get; set; }
    }

    public class AssetConfig
    {
        public string AssetId { get; set; }
        public string Name { get; set; }
        public int Precision { get; set; }
    }
    
    public class DcrdSettings
    {
        
        public string RpcEndpoint { get; set; }
        public string RpcUser { get; set; }
        public string RpcPass { get; set; }
    }

    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }
    }
    
    public class ServiceSettings
    {
        public string NetworkName { get; set; }        
        public AssetConfig Asset { get; set; }
        public DcrdSettings Dcrd { get; set; }
        public DbSettings Db { get; set; }
    }
    
    public class SlackNotificationsSettings
    {
        public AzureQueuePublicationSettings AzureQueue { get; set; }
    }

    public class AzureQueuePublicationSettings
    {
        public string ConnectionString { get; set; }
        public string QueueName { get; set; }
    }

    public class ConnectionStrings
    {
        public string Azure { get; set; }
        public string Dcrdata { get; set; }
        public string Logging { get; set; }
    }
}
