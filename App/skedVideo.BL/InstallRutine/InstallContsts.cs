namespace skedVideo.BL.InstallRutine
{
    /// <summary>
    /// Всякие константы
    /// </summary>
    static class InstallContsts
    {
        /// <summary>
        /// Скрипт установки chocolatey
        /// </summary>
        public const string Chocolatey_InstalCmd = "@\"%SystemRoot%\\System32\\WindowsPowerShell\\v1.0\\powershell.exe\" -NoProfile -InputFormat None -ExecutionPolicy Bypass -Command \"[System.Net.ServicePointManager]::SecurityProtocol = 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))\" && SET \"PATH=%PATH%;%ALLUSERSPROFILE%\\chocolatey\\bin\"";
        /// <summary>
        /// Команда установки пакета k-lite
        /// </summary>
        public const string Klite_InstalCmd = "choco install k-litecodecpackfull";
        /// <summary>
        /// Путь к установленному плееру MPC-HC
        /// </summary>
        public const string Player_Path = @"c:\Program Files (x86)\K-Lite Codec Pack\MPC-HC64\mpc-hc64.exe";
        /// <summary>
        /// Настройки плеера. Путь в реестре. Активный вебинтерфейс.
        /// </summary>
        public const string Player_Setting_RegSetting = @"HKEY_CURRENT_USER\Software\MPC-HC\MPC-HC\Settings\";
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
