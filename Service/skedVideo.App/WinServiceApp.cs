﻿using System.ComponentModel;
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
        public WinServiceInstaller()
        {
            ServiceProcessInstaller spi = new ServiceProcessInstaller();
            ServiceInstaller si = new ServiceInstaller();

            spi.Account = ServiceAccount.LocalSystem;
            spi.Password = null;
            spi.Username = null;

            si.Description = Settings.ServiceDescription;
            si.DisplayName = Settings.ServiceDisplayName;
            si.ServiceName = Settings.ServiceName;
            si.StartType = ServiceStartMode.Automatic;

            this.Installers.Add(spi);
            this.Installers.Add(si);
            this.AfterInstall += (a, b) => Manager.AddEventView(Settings.ServiceName, Settings.ServiceDisplayName, Settings.ServiceDescription);
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
