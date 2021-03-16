using System;
using Cav;
using HtmlAgilityPack;

namespace skedVideo.Bl.Mpc
{
    public enum PLayerCommand
    {
        BrowseFile = -100,
        GetStatus = -200,

        GoTo = -1,
        FullScreen = 830,
        StopFile = 890,
        CloseFile = 804
    }

    public class CommandParam
    {
        public String Param { get; set; }
        public String Val { get; set; }

        public override string ToString()
        {
            if (Param == null)
                return "null=0";

            return $"{Param}={Val ?? "null"}";
        }
    }

    public enum PalyerState
    {
        ClosedFile = -1,
        Stoped = 0,
        Paused = 1,
        Playing = 2
    }

    /// <summary>
    /// Настройки плеера
    /// </summary>
    internal static class PlayerEnvironment
    {
        /// <summary>
        /// Путь к установленному плееру MPC-HC
        /// </summary>
        public const string PathToExe = @"c:\Program Files (x86)\K-Lite Codec Pack\MPC-HC64\mpc-hc64.exe";
        /// <summary>
        /// Настройки плеера. Путь в реестре.
        /// </summary>
        public const string RegSetting = @"HKEY_CURRENT_USER\Software\MPC-HC\MPC-HC\Settings\";
        /// <summary>
        /// Настройки плеера. Активный вебинтерфейс.
        /// </summary>
        public const string WebServerEnable = "EnableWebServer";
        /// <summary>
        /// Настройки вебсервера плеера. Порт.
        /// </summary>
        public const string WebServerPort = "WebServerPort";
        /// <summary>
        /// Настройки вебсервера плеера. Принимать запросы только от локалхоста
        /// </summary>
        public const string WebServerLocalhostOnly = "WebServerLocalhostOnly";
        /// <summary>
        /// Настройки вебсервера плеера. Выходить из полноэкранного режима по окончании воспроизведения
        /// </summary>
        public const string ExitFullscreenAtTheEnd = "ExitFullscreenAtTheEnd";
        /// <summary>
        /// Настройки вебсервера плеера. Запускать в полноэкранном режиме
        /// </summary>
        public const string LaunchFullScreen = "LaunchFullScreen";
        /// <summary>
        /// Настройки вебсервера плеера. Логотип при отсутствии воспроизведения.
        /// </summary>
        public const string LogoFile = "LogoFile";

    }

    public class PalyerStatus
    {
        public string FilePath { get; set; }
        public PalyerState State { get; set; }
        public TimeSpan Position { get; set; }
        public TimeSpan Duration { get; set; }
        public int VolumeLevel { get; set; }
        public bool Muted { get; set; }

        public static PalyerStatus Parse(string html)
        {
            var res = new PalyerStatus();
            if (html.IsNullOrWhiteSpace())
                return res;

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            htmlDoc.LoadHtml(htmlDoc.DocumentNode.OuterHtml);

            res.FilePath = htmlDoc.GetElementbyId("filepath").InnerText;
            res.State = (PalyerState)Enum.Parse(typeof(PalyerState), htmlDoc.GetElementbyId("state").InnerText);
            res.Position = TimeSpan.FromMilliseconds(Double.Parse(htmlDoc.GetElementbyId("position").InnerText));
            res.Duration = TimeSpan.FromMilliseconds(Double.Parse(htmlDoc.GetElementbyId("duration").InnerText));
            res.VolumeLevel = int.Parse(htmlDoc.GetElementbyId("volumelevel").InnerText);
            res.Muted = htmlDoc.GetElementbyId("muted").InnerText == "0";
            return res;
        }
    }
}
