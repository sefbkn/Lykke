using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.Decred_BlockchainApi.Settings.ServiceSettings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }
    }
}
