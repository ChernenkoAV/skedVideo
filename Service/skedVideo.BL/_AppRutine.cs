using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Cav;
using skedVideo.UpdateApp;

namespace skedVideo.BL
{
    public static class AppRutine
    {
        public static void StopApp()
        {
            evLog = null;
        }

        private static EventLog evLog;

        public static void WriteToLog(String message, EventLogEntryType type)
        {
            if (Environment.UserInteractive)
            {
                Console.WriteLine("".PadLeft(20, '-'));
                Console.WriteLine(type.ToString());
                Console.WriteLine(message);
            }
            else
                evLog.WriteEntry(message: message, type: type);
        }

        public static void StartApp(CancellationToken token, EventLog eventLog)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            evLog = eventLog;

            UpdaterBL.VisitSystemComponents(token);
        }
    }

    public static class ExExtention
    {
        public static void AddError(this Exception ex)
        {
            AppRutine.WriteToLog(message: ex.Expand(), type: EventLogEntryType.Error);
        }

        public static void AddWarning(this string message)
        {
            AppRutine.WriteToLog(message: message, type: EventLogEntryType.Warning);
        }

        public static void AddInformation(this string message)
        {
            AppRutine.WriteToLog(message: message, type: EventLogEntryType.Information);
        }
    }
}
