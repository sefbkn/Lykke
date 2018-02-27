using Lykke.Service.Decred_SignService.Settings.ServiceSettings;
using Lykke.Service.Decred_SignService.Settings.SlackNotifications;

namespace Lykke.Service.Decred_SignService.Settings
{
    public class AppSettings
    {
        public Decred_SignServiceSettings Decred_SignServiceService { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
    }
}
