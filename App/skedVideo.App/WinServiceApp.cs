using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;
using System.Threading;
using Cav.WinService;
using skedVideo.BL;

namespace skedVideo.Service
{
    [RunInstaller(true)]
    public class WinServiceInstaller : Installer
    {

        public static String DisplayName = "skedVideo.AppServer";
        public const String Description = "Запуск видео по расписанию";
        public const String ServiceName = "skedVideo";

        public WinServiceInstaller()
        {
            ServiceProcessInstaller spi = new ServiceProcessInstaller();
            ServiceInstaller si = new ServiceInstaller();

            spi.Account = ServiceAccount.LocalSystem;
            spi.Password = null;
            spi.Username = null;

            si.Description = Description;
            si.DisplayName = DisplayName;
            si.ServiceName = ServiceName;
            si.StartType = ServiceStartMode.Automatic;

            this.Installers.Add(spi);
            this.Installers.Add(si);
            this.AfterInstall += (a, b) => Manager.AddEventView(ServiceName, DisplayName, Description);
        }

        protected override void OnBeforeInstall(IDictionary savedState)
        {
            var assemblyPath = Context.Parameters["assemblypath"];
            assemblyPath = $"\"{assemblyPath}\" {ServiceName}";
            Context.Parameters["assemblypath"] = assemblyPath;
            base.OnBeforeInstall(savedState);
        }
    }

    public class SkedVideoServiceHost : ServiceBase
    {
        public SkedVideoServiceHost()
        {
            this.AutoLog = true;
        }

        private CancellationTokenSource cancellSource;

        public void StartService()
        {
            this.StopService();

            cancellSource = new CancellationTokenSource();

            AppRutine.StartApp(cancellSource.Token, this.EventLog);
        }

        public void StopService()
        {
            AppRutine.StopApp();

            if (cancellSource != null)
            {
                cancellSource.Cancel();
                cancellSource.Dispose();
                cancellSource = null;
            }
        }


        protected override void OnStart(string[] args)
        {
            StartService();
        }

        protected override void OnStop()
        {
            StopService();
        }
    }
}
