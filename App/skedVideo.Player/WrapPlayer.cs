using System.IO;
using Microsoft.Win32;

namespace skedVideo.Player
{
    public class WrapPlayer
    {
        public bool PlayerExists()
        {
            return File.Exists(Settings.Player_Path);
        }

        public void SettingPlayer()
        {
            int isEnableWS = (int)Registry.GetValue(Settings.Player_Setting_RegSetting, Settings.Player_Setiing_WebServerEnable, 0);

            if (isEnableWS != 1)
                Registry.SetValue(Settings.Player_Setting_RegSetting, Settings.Player_Setiing_WebServerEnable, 1, RegistryValueKind.DWord);

                int webPort = (int)Registry.GetValue(Settings.Player_Setting_RegSetting, Settings.Player_Setiing_WebServerEnable, 0);

        }
    }
}
