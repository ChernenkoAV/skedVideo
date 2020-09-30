using System;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceProcess;
using Cav;
using Cav.WinService;
using skedVideo.Common;
using skedVideo.Service;

namespace skedVideo.App
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!Environment.UserInteractive)
            {
                ServiceBase.Run(new SkedVideoServiceHost(args.FirstOrDefault()));
                return;
            }

            try
            {
                Boolean install = args.Any(x => x == "/i");
                Boolean stop = args.Any(x => x == "/s");
                Boolean run = args.Any(x => x == "/r");
                Boolean uninstall = args.Any(x => x == "/u");

                if (
                    (
                        Convert.ToInt16(install) +
                        Convert.ToInt16(stop) +
                        Convert.ToInt16(run) +
                        Convert.ToInt16(uninstall)
                        ) > 1
                    )
                {
                    Console.WriteLine("Указаны взаимоисключающие параметры.");
                    Console.WriteLine("Для выхода нажмите Enter.");
                    Console.ReadLine();
                    return;
                }

                if (args.Any(x => x.StartsWith("/sn:")))
                    WinServiceInstaller.ServiceName = args.First(x => x.StartsWith("/sn:")).Replace("/sn:", "");
                else
                    WinServiceInstaller.ServiceName = null;

                String fileName = Assembly.GetExecutingAssembly().Location;

                #region Установка службы

                if (install)
                {
                    if (Manager.ServiceExist(WinServiceInstaller.ServiceName))
                    {
                        Console.WriteLine($"Служба '{WinServiceInstaller.ServiceName}' '{WinServiceInstaller.DisplayName}' уже установлена.");
                        Console.ReadKey();
                        return;
                    }

                    if (!Manager.IsAdmin())
                    {
                        Manager.RunAsAdmin(fileName, args.JoinValuesToString(" "));
                        return;
                    }

                    Console.WriteLine($"Установка службы '{WinServiceInstaller.DisplayName}'");
                    Console.WriteLine();
                    Manager.InstallAsService(fileName);
                    Console.WriteLine($"Запуск службы '{WinServiceInstaller.DisplayName}'");
                    Console.WriteLine();
                    Manager.StartService(WinServiceInstaller.ServiceName);

                    return;
                }

                #endregion

                #region Удаление службы

                if (uninstall)
                {
                    if (!Manager.ServiceExist(WinServiceInstaller.ServiceName))
                    {
                        Console.WriteLine($"Служба '{WinServiceInstaller.ServiceName}' '{WinServiceInstaller.DisplayName}' не установлена.");
                        Console.ReadKey();
                        return;
                    }

                    if (!Manager.IsAdmin())
                    {
                        Manager.RunAsAdmin(fileName, args.JoinValuesToString(" "));
                        return;
                    }


                    Console.WriteLine("Попытка останова службы.");
                    Console.WriteLine();
                    Manager.StopService(WinServiceInstaller.ServiceName);
                    Console.WriteLine("Удаление службы.");
                    Console.WriteLine();
                    Manager.UninstallService(fileName);

                    if (Manager.ServiceExist(WinServiceInstaller.ServiceName))
                    {
                        Console.WriteLine("Внимание! Служба была помечена на удаление. Необходимо перезагрузить компьютер.");
                        Console.WriteLine();
                        Console.ReadKey();
                    }

                    return;
                }

                #endregion

                #region  Останов службы

                if (stop)
                {
                    if (!Manager.ServiceExist(WinServiceInstaller.ServiceName))
                    {
                        Console.WriteLine($"Служба '{WinServiceInstaller.DisplayName}' не установлена.");
                        Console.ReadKey();
                        return;
                    }

                    if (!Manager.IsAdmin())
                    {
                        Manager.RunAsAdmin(fileName, args.JoinValuesToString(" "));
                        return;
                    }


                    Console.WriteLine("Попытка останова службы.");
                    Console.WriteLine();
                    Manager.StopService(WinServiceInstaller.ServiceName);

                    return;
                }

                #endregion

                #region Запуск службы

                if (run)
                {
                    if (!Manager.ServiceExist(WinServiceInstaller.ServiceName))
                    {
                        Console.WriteLine($"Служба '{WinServiceInstaller.DisplayName}' не установлена.");
                        Console.ReadKey();
                        return;
                    }

                    if (!Manager.IsAdmin())
                    {
                        Manager.RunAsAdmin(fileName, args.JoinValuesToString(" "));
                        return;
                    }


                    Console.WriteLine("Попытка запуска службы.");
                    Console.WriteLine();
                    Manager.StartService(WinServiceInstaller.ServiceName);

                    return;
                }

                #endregion

                #region Работа в консольном режиме

                Console.WriteLine();
                Console.WriteLine(WinServiceInstaller.DisplayName);
                Console.WriteLine();
                Console.WriteLine("Для установки в качестве службы выполните с параметром /i");
                Console.WriteLine("Для удаления службы выполните с параметром /u");
                Console.WriteLine("Для останова службы выполните с параметром /s");
                Console.WriteLine("Для запуска службы выполните с параметром /r");
                Console.WriteLine();
                Console.WriteLine("Указание имени службы: параметр /sn:имя_службы");
                Console.WriteLine();
                Console.WriteLine($"Запуск '{WinServiceInstaller.DisplayName}'");
                var msh = new SkedVideoServiceHost();
                try
                {
                    msh.StartNetService();
                }
                catch (AddressAccessDeniedException)
                {
                    Console.WriteLine();
                    Console.WriteLine("Регистрация пространства имен HTTP.");
                    String netsh = @"c:\Windows\System32\netsh.exe";

                    String hsarg = String.Format(
                        @"http add urlacl url=http://+:{0}/{1} user={2}\{3}",
                        Settings.AppServerPort,
                        Settings.AppServerAddress,
                        //Environment.MachineName,
                        Environment.UserDomainName,
                        Environment.UserName);
                    var regpr = Manager.RunAsAdmin(netsh, hsarg);
                    Console.WriteLine("Ожидание завершение регистрации");
                    regpr.WaitForExit();
                    Console.WriteLine("Повторный запуск сервера приложений");
                    msh.StopWebService();
                    msh.StartNetService();
                }

                Console.WriteLine($"'{WinServiceInstaller.DisplayName}' запущен.");
                Console.WriteLine();
                Console.WriteLine("Для останова нажмите Enter.");
                while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
                Console.WriteLine();
                Console.WriteLine($"Останов '{WinServiceInstaller.DisplayName}'");
                msh.StopWebService();

                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine(ex.Expand());
                Console.WriteLine();
                Console.WriteLine($"'{WinServiceInstaller.DisplayName}' остановлен");
                Console.WriteLine("Для выхода нажмите Enter.");
                Console.ReadLine();
            }

        }
    }
}
