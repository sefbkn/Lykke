using Lykke.Service.Decred_Api.Settings.ServiceSettings;
using Lykke.Service.Decred_Api.Settings.SlackNotifications;

namespace Lykke.Service.Decred_Api.Settings
{
    public class AppSettings
    {
        public Decred_ApiSettings Decred_ApiService { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
    }
}
