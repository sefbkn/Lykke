using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.Decred_Api.Settings.ServiceSettings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }
    }
}
