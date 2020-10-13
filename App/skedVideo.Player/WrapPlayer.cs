using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Win32;
using RestSharp;

namespace skedVideo.Player
{
    public class WrapPlayer
    {
        public bool PlayerExists()
        {
            return File.Exists(Settings.Player_Path);
        }

        private int webPort;
        private const string control = "command.html";
        private const string status = "status.html";

        private void execCommand(
            string code,
            string varName = null,
            string varValue = null)
        {
            if (varName == null)
                varName = "null";

            if (varValue == null)
                varValue = "0";

            var varCmd = $"{varName}={varValue}";

            var req = new RestRequest(control, Method.POST);
            req.AddParameter(String.Empty, $"wm_command={code}&{varCmd}", ParameterType.RequestBody);
            execRequest(req);
        }

        private IRestResponse execRequest(IRestRequest request)
        {
            var client = new RestClient(new Uri($"http://localhost:{webPort}"));
            return client.Execute(request);
        }

        public void SettingPlayer()
        {
            int isEnableWS = (int)Registry.GetValue(Settings.Player_Setting_RegSetting, Settings.Player_Setiing_WebServerEnable, 0);

            if (isEnableWS != 1)
                Registry.SetValue(Settings.Player_Setting_RegSetting, Settings.Player_Setiing_WebServerEnable, 1, RegistryValueKind.DWord);

            webPort = (int)Registry.GetValue(Settings.Player_Setting_RegSetting, Settings.Player_Setting_WebServerPort, 0);
            if (webPort == 0)
            {
                webPort = 13579;
                Registry.SetValue(Settings.Player_Setting_RegSetting, Settings.Player_Setting_WebServerPort, webPort, RegistryValueKind.DWord);
            }

            var onlyLocal = (int)Registry.GetValue(Settings.Player_Setting_RegSetting, Settings.Player_Setting_WebServerLocalhostOnly, 0);
            if (onlyLocal != 1)
                Registry.SetValue(Settings.Player_Setting_RegSetting, Settings.Player_Setting_WebServerLocalhostOnly, 1, RegistryValueKind.DWord);

            var exitFullscreen = (int)Registry.GetValue(Settings.Player_Setting_RegSetting, Settings.Player_Setting_ExitFullscreenAtTheEnd, 0);
            if (exitFullscreen != 0)
                Registry.SetValue(Settings.Player_Setting_RegSetting, Settings.Player_Setting_ExitFullscreenAtTheEnd, 0, RegistryValueKind.DWord);

            var launchFullScreen = (int)Registry.GetValue(Settings.Player_Setting_RegSetting, Settings.Player_Setting_LaunchFullScreen, 0);
            if (launchFullScreen != 1)
                Registry.SetValue(Settings.Player_Setting_RegSetting, Settings.Player_Setting_LaunchFullScreen, 1, RegistryValueKind.DWord);

        }

        public void OpenFile(string path)
        {
            var req = new RestRequest(Method.GET);
            req.Timeout = 1;
            req.Resource = "browser.html";
            req.AddQueryParameter("path", path);
            var res = execRequest(req);

            if (res.StatusCode == 0)
                Process.Start(Settings.Player_Path, $"\"{path}\"");

            var timeOut = DateTime.Now.AddSeconds(5);
            req = new RestRequest(Method.GET);
            req.Timeout = 5000;
            req.Resource = "status.html";
            res = execRequest(req);

            while (res.StatusCode != System.Net.HttpStatusCode.OK || res.Content.Contains(", \"N/A\", 0, \"00:00:00\", 0, \"00:00:00\", "))
            {
                if (timeOut < DateTime.Now)
                    throw new TimeoutException("не удалось запустить воспроизведение");

                Thread.Sleep(500);

                res = execRequest(req);
            }
        }

        public void GotoPosition(TimeSpan pos)
        {
            execCommand("-1", "position", pos.ToString());
        }

        public void FullScreenMode()
        {
            execCommand("830");
        }

        public void StopPlay()
        {
            execCommand("890");
        }

        public void CloseFile()
        {
            execCommand("804");
        }

    }
}
