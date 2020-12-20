using System;
using System.Windows.Forms;

namespace skedVideo.tray
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.Run(new AppTrayContext());
        }
    }
}
