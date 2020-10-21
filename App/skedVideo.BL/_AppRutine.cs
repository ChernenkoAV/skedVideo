using System.Diagnostics;
using System.Net;
using System.Threading;

namespace skedVideo.BL
{
    public static class AppRutine
    {
        public static void StopApp()
        {
            //throw new NotImplementedException();
        }

        public static void StartApp(CancellationToken token, EventLog eventLog)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            //throw new NotImplementedException();  

        }
    }
}
