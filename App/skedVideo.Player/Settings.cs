namespace skedVideo.Player
{
    internal static class Settings
    {
        /// <summary>
        /// Путь к установленному плееру MPC-HC
        /// </summary>
        public const string Player_Path = @"c:\Program Files (x86)\K-Lite Codec Pack\MPC-HC64\mpc-hc64.exe";
        /// <summary>
        /// Настройки плеера. Путь в реестре.
        /// </summary>
        public const string Player_Setting_RegSetting = @"HKEY_CURRENT_USER\Software\MPC-HC\MPC-HC\Settings\";
        /// <summary>
        /// Настройки плеера. Активный вебинтерфейс.
        /// </summary>
        public const string Player_Setiing_WebServerEnable = "EnableWebServer";
        /// <summary>
        /// Настройки вебсервера плеера. Порт.
        /// </summary>
        public const string Player_Setting_WebServerPort = "WebServerPort";
        /// <summary>
        /// Настройки вебсервера плеера. Принимать запросы только от локалхоста
        /// </summary>
        public const string Player_Setting_WebServerLocalhostOnly = "WebServerLocalhostOnly";
    }
}
