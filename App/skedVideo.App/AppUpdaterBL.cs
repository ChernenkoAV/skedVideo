using System.Diagnostics;
using System.Reflection;
using Cav.WinService;
using skedVideo.Service;

namespace skedVideo.App
{
    internal class AppUpdaterBL
    {

        public void foo()
        {
            Manager.StartService(WinServiceInstaller.ServiceName);
            Manager.StopService(WinServiceInstaller.ServiceName);

        }

        public string GetInformationalVersion(Assembly assembly)
        {
            return FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;
        }


        public bool ExistsNewVersion()
        {
            var ev = false;




            return ev;
        }
    }
}
