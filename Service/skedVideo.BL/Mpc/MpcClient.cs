using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Win32;
using RestSharp;

namespace skedVideo.Bl.Mpc
{
    public class MpcClient
    {
        #region SysemSetting

        public static class SystemSetting
        {
            public static Boolean IsEnabledWSGet()
            {
                int val = (int)Registry.GetValue(PlayerEnvironment.RegSetting, PlayerEnvironment.WebServerEnable, 0);
                return val == 1;
            }
            public static void IsEnabledWSSet(Boolean state)
            {
                Registry.SetValue(PlayerEnvironment.RegSetting, PlayerEnvironment.WebServerEnable, state ? 1 : 0, RegistryValueKind.DWord);
            }

            public static void WebPortSet(int port)
            {
                if (port == 0)
                    port = 13579;
                Registry.SetValue(PlayerEnvironment.RegSetting, PlayerEnvironment.WebServerPort, port, RegistryValueKind.DWord);
            }

            public static int WebPortGet()
            {
                return (int)Registry.GetValue(PlayerEnvironment.RegSetting, PlayerEnvironment.WebServerPort, 0);
            }

            public static Boolean IsOnlyLocalWebGet()
            {
                var val = (int)Registry.GetValue(PlayerEnvironment.RegSetting, PlayerEnvironment.WebServerLocalhostOnly, 0);
                return val == 1;
            }

            public static void IsOnlyLocalWebSet(Boolean state)
            {
                Registry.SetValue(PlayerEnvironment.RegSetting, PlayerEnvironment.WebServerLocalhostOnly, state ? 1 : 0, RegistryValueKind.DWord);
            }

            public static Boolean IsExitFullscreenAtTheEndGet()
            {
                var val = (int)Registry.GetValue(PlayerEnvironment.RegSetting, PlayerEnvironment.ExitFullscreenAtTheEnd, 0);
                return val == 1;
            }

            public static void IsExitFullscreenAtTheEndSet(Boolean state)
            {
                Registry.SetValue(PlayerEnvironment.RegSetting, PlayerEnvironment.ExitFullscreenAtTheEnd, state ? 1 : 0, RegistryValueKind.DWord);
            }

            public static Boolean IsLaunchFullScreenGet()
            {
                var val = (int)Registry.GetValue(PlayerEnvironment.RegSetting, PlayerEnvironment.LaunchFullScreen, 0);
                return val == 1;
            }

            public static void IsLaunchFullScreenSet(Boolean state)
            {
                Registry.SetValue(PlayerEnvironment.RegSetting, PlayerEnvironment.LaunchFullScreen, state ? 1 : 0, RegistryValueKind.DWord);
            }
        }

        #endregion

        public MpcClient()
        {
            webPort = SystemSetting.WebPortGet();
        }

        private int webPort;
        private IRestResponse execCommand(
            PLayerCommand command,
            CommandParam param = null)
        {
            RestRequest req = new RestRequest();
            req.Timeout = 1;

            switch (command)
            {
                case PLayerCommand.BrowseFile:
                    if (param == null)
                        throw new ArgumentNullException(nameof(param));
                    req.Method = Method.GET;
                    req.Resource = "browser.html";
                    req.AddQueryParameter("path", param.Val);
                    break;
                case PLayerCommand.GetStatus:
                    req = new RestRequest(Method.GET);
                    req.Resource = "variables.html";
                    break;
                default:
                    if (param == null)
                        param = new CommandParam();
                    req.Method = Method.POST;
                    req.Resource = "command.html";
                    req.AddParameter(String.Empty, $"wm_command={command}&{param.ToString()}", ParameterType.RequestBody);
                    break;
            }

            var client = new RestClient(new Uri($"http://localhost:{webPort}"));
            return client.Execute(req);
        }

        public static bool PlayerExists()
        {
            return File.Exists(PlayerEnvironment.PathToExe);
        }

        public void OpenFile(string path)
        {
            var res = execCommand(PLayerCommand.BrowseFile, new CommandParam() { Param = "path", Val = path });

            if (res.StatusCode == 0)
                Process.Start(PlayerEnvironment.PathToExe, $"\"{path}\"");

            while (GetStatus().State != PalyerState.Playing)
                Task.Delay(500).GetAwaiter().GetResult();
        }

        public PalyerStatus GetStatus()
        {
            var resp = execCommand(PLayerCommand.GetStatus);
            return PalyerStatus.Parse(resp.Content);
        }

        public void GotoPosition(TimeSpan pos)
        {
            execCommand(PLayerCommand.GoTo, new CommandParam() { Param = "position", Val = pos.ToString() });
        }

        public void FullScreenMode()
        {
            execCommand(PLayerCommand.FullScreen);
        }

        public void StopPlay()
        {
            execCommand(PLayerCommand.StopFile);
        }

        public void CloseFile()
        {
            execCommand(PLayerCommand.CloseFile);
        }

    }
}
