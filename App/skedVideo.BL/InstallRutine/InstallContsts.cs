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
        public const string Klite_InstalCmd = @"choco install k-litecodecpackfull /y";
    }
}
