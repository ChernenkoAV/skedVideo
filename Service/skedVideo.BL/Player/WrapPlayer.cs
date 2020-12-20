using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Cav;
using Microsoft.Win32;
using RestSharp;
using skedVideo.Player;

namespace skedVideo.BL
{
    public class WrapPlayerBL
    {
        public static bool PlayerExists()
        {
            return File.Exists(PlayerSettings.Player_Path);
        }

        private int webPort;
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

            var req = new RestRequest("command.html", Method.POST);
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
            int isEnableWS = (int)Registry.GetValue(PlayerSettings.Player_Setting_RegSetting, PlayerSettings.Player_Setiing_WebServerEnable, 0);

            if (isEnableWS != 1)
                Registry.SetValue(PlayerSettings.Player_Setting_RegSetting, PlayerSettings.Player_Setiing_WebServerEnable, 1, RegistryValueKind.DWord);

            webPort = (int)Registry.GetValue(PlayerSettings.Player_Setting_RegSetting, PlayerSettings.Player_Setting_WebServerPort, 0);
            if (webPort == 0)
            {
                webPort = 13579;
                Registry.SetValue(PlayerSettings.Player_Setting_RegSetting, PlayerSettings.Player_Setting_WebServerPort, webPort, RegistryValueKind.DWord);
            }

            var onlyLocal = (int)Registry.GetValue(PlayerSettings.Player_Setting_RegSetting, PlayerSettings.Player_Setting_WebServerLocalhostOnly, 0);
            if (onlyLocal != 1)
                Registry.SetValue(PlayerSettings.Player_Setting_RegSetting, PlayerSettings.Player_Setting_WebServerLocalhostOnly, 1, RegistryValueKind.DWord);

            var exitFullscreen = (int)Registry.GetValue(PlayerSettings.Player_Setting_RegSetting, PlayerSettings.Player_Setting_ExitFullscreenAtTheEnd, 0);
            if (exitFullscreen != 0)
                Registry.SetValue(PlayerSettings.Player_Setting_RegSetting, PlayerSettings.Player_Setting_ExitFullscreenAtTheEnd, 0, RegistryValueKind.DWord);

            var launchFullScreen = (int)Registry.GetValue(PlayerSettings.Player_Setting_RegSetting, PlayerSettings.Player_Setting_LaunchFullScreen, 0);
            if (launchFullScreen != 1)
                Registry.SetValue(PlayerSettings.Player_Setting_RegSetting, PlayerSettings.Player_Setting_LaunchFullScreen, 1, RegistryValueKind.DWord);

        }

        public void OpenFile(string path)
        {
            var req = new RestRequest(Method.GET);
            req.Timeout = 1;
            req.Resource = "browser.html";
            req.AddQueryParameter("path", path);
            var res = execRequest(req);

            if (res.StatusCode == 0)
                Process.Start(PlayerSettings.Player_Path, $"\"{path}\"");

            while (GetStatus().State != StateKind.Play)
            { }
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

        public PlayerStatus GetStatus()
        {
            PlayerStatus res = new PlayerStatus();

            var req = new RestRequest(Method.GET);
            req.Timeout = 5000;
            req.Resource = "variables.html";
            var resp = execRequest(req);

            var timeOut = DateTime.Now.AddSeconds(5);

            while (resp.Content.IsNullOrWhiteSpace())
            {
                if (timeOut < DateTime.Now)
                    throw new TimeoutException("не удалось запустить воспроизведение");

                Thread.Sleep(500);

                resp = execRequest(req);
            }

            var cont = resp
                .Content
                .Replace("href=\"favicon.ico\">", "href=\"favicon.ico\"/>")
                .Replace("href=\"default.css\">", "href=\"default.css\"/>")
                .Replace("charset=\"utf-8\">", "charset=\"utf-8\"/>");

            var xDoc = XDocument.Parse(cont);

            var elts = xDoc.Descendants();
            res.FilePath = elts.First(x => x.Attributes().Any(y => y.Value == "filepath")).Value;
            if (!res.FilePath.IsNullOrWhiteSpace())
                res.State = (StateKind)Enum.Parse(typeof(StateKind), elts.First(x => x.Attributes().Any(y => y.Value == "state")).Value);

            return res;
        }
    }
}
