using System.Windows.Forms;
using skedVideo.tray.Properties;

namespace skedVideo.tray
{
    class AppTrayContext : ApplicationContext
    {
        private NotifyIcon trayIcon;

        public AppTrayContext()
        {
            trayIcon = new NotifyIcon()
            {
                Icon = Resources.favicon,
                Visible = true
            };
        }
    }
}
