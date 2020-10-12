using System;
using System.Diagnostics;
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
            //throw new NotImplementedException();
            var wp = new skedVideo.Player.WrapPlayer();
            var x = wp.PlayerExists();
            wp.SettingPlayer();

        }
    }
}
