using System.Diagnostics;
using System.Net;
using System.Threading;
using skedVideo.UpdateApp;

namespace skedVideo.BL
{
    public static class AppRutine
    {
        public static void StopApp()
        {

        }

        public static void StartApp(CancellationToken token, EventLog eventLog)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            UpdaterBL.VisitSystemComponents(token);
        }
    }
}
