using System.Diagnostics;
using System.Net;
using System.Threading;

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



            //Task.Run(() =>
            //{
            //    Thread.Sleep(10000);
            //    var ubl = new UpdaterBL(Settings.ServiceName);

            //    if (!ubl.ExistsNewVersion())
            //        return;

            //    ubl.UpdateStep1();
            //});
        }
    }
}
