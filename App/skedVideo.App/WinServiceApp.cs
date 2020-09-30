using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using Cav;
using Cav.WinService;
using skedVideo.BL;

namespace skedVideo.Service
{
    [RunInstaller(true)]
    public class WinServiceInstaller : Installer
    {

        public static String DisplayName = "skedVideo Server";
        public const String Description = "Служба запуска видео по расписанию";

        private const String originalServiceName = "skedVideo";
        public static String ServiceName
        {
            get
            {
                String textFile = originalServiceName;

                if (File.Exists(Path.Combine(DomainContext.TempPath, fileNameForServiceName)))
                    textFile = File.ReadAllText(Path.Combine(DomainContext.TempPath, fileNameForServiceName));

                return textFile.GetNullIfIsNullOrWhiteSpace() ?? originalServiceName;
            }

            set
            {
                var val = value;

                if (File.Exists(Path.Combine(DomainContext.TempPath, fileNameForServiceName)))
                    File.Delete(Path.Combine(DomainContext.TempPath, fileNameForServiceName));

                if (val.IsNullOrWhiteSpace())
                    val = originalServiceName;
                else
                    val = val.Replace(" ", "").ReplaceInvalidPathChars().SubString(0, 80);

                if (val != originalServiceName)
                    File.WriteAllText(Path.Combine(DomainContext.TempPath, fileNameForServiceName), value);
            }
        }

        private const String fileNameForServiceName = "ServiceName.txt";

        public WinServiceInstaller()
        {
            ServiceProcessInstaller spi = new ServiceProcessInstaller();
            ServiceInstaller si = new ServiceInstaller();

            spi.Account = ServiceAccount.LocalSystem;
            spi.Password = null;
            spi.Username = null;

            if (originalServiceName != ServiceName)
                DisplayName = DisplayName + " (" + ServiceName + ")";

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
        public SkedVideoServiceHost(String serviceName = null)
        {
            if (serviceName.IsNullOrWhiteSpace())
                serviceName = WinServiceInstaller.ServiceName;

            if (!serviceName.IsNullOrWhiteSpace())
                ServiceName = serviceName;

            this.AutoLog = true;
        }

        private CancellationTokenSource cancellSource;

        public void StartNetService()
        {
            this.StopWebService();

            cancellSource = new CancellationTokenSource();

            AppRutine.StartApp(cancellSource.Token, this.EventLog);
        }

        public void StopWebService()
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
            StartNetService();
        }

        protected override void OnStop()
        {
            StopWebService();
        }
    }
}
