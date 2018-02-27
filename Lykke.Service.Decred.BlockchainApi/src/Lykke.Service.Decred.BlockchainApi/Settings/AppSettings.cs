using Lykke.Service.Decred_BlockchainApi.Settings.ServiceSettings;
using Lykke.Service.Decred_BlockchainApi.Settings.SlackNotifications;

namespace Lykke.Service.Decred_BlockchainApi.Settings
{
    public class AppSettings
    {
        public Decred_BlockchainApiSettings Decred_BlockchainApiService { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
    }
}
