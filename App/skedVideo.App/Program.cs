using System;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceProcess;
using Cav;
using Cav.WinService;
using skedVideo.Service;
using skedVideo.UpdateApp;

namespace skedVideo.App
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Boolean uninstall = args.Any(x => x == "/uninstall");
                Boolean update = args.Any(x => x == "/update");

                String fileName = Assembly.GetExecutingAssembly().Location;

                #region Установка службы

#if !DEBUG
                if (Environment.UserInteractive && !Manager.ServiceExist(Settings.ServiceName))
                {
                    if (!Manager.IsAdmin())
                    {
                        Manager.RunAsAdmin(fileName);
                        return;
                    }

                    Console.WriteLine($"Установка службы '{Settings.ServiceDisplayName}'");
                    Console.WriteLine();
                    Manager.InstallAsService(fileName);
                    Console.WriteLine($"Запуск службы '{Settings.ServiceDisplayName}'");
                    Console.WriteLine();
                    Manager.StartService(Settings.ServiceName);

                    return;
                }

#endif

                #endregion

                #region Удаление службы

                if (uninstall)
                {
                    if (!Manager.ServiceExist(Settings.ServiceName))
                        return;

                    if (!Manager.IsAdmin())
                    {
                        Manager.RunAsAdmin(fileName, args.JoinValuesToString(" "));
                        return;
                    }


                    Console.WriteLine("Попытка останова службы.");
                    Console.WriteLine();
                    Manager.StopService(Settings.ServiceName);
                    Console.WriteLine("Удаление службы.");
                    Console.WriteLine();
                    Manager.UninstallService(fileName);

                    if (Manager.ServiceExist(Settings.ServiceName))
                    {
                        Console.WriteLine("Внимание! Служба была помечена на удаление. Необходимо перезагрузить компьютер.");
                        Console.WriteLine();
                        Console.ReadKey();
                    }

                    return;
                }

                #endregion

                #region Обновление приложения

                if (update)
                {
                    var targetFile = args[1];
                    var ubl = new UpdaterBL(Settings.ServiceName);
                    ubl.UpdateStep2(targetFile);
                    return;
                }

                #endregion

                #region Работа в консольном режиме
                if (!Environment.UserInteractive)
                {
                    ServiceBase.Run(new SkedVideoServiceHost());
                    return;
                }

                Console.WriteLine();
                Console.WriteLine(Settings.ServiceDisplayName);
                Console.WriteLine();
                Console.WriteLine("Для удаления службы выполните с параметром /uninstall");
                Console.WriteLine();
                Console.WriteLine($"Запуск '{Settings.ServiceDisplayName}'");
                var msh = new SkedVideoServiceHost();
                try
                {
                    msh.StartService();
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
                    msh.StopService();
                    msh.StartService();
                }

                Console.WriteLine($"'{Settings.ServiceDisplayName}' запущен.");
                Console.WriteLine();
                Console.WriteLine("Для останова нажмите Enter.");
                while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
                Console.WriteLine();
                Console.WriteLine($"Останов '{Settings.ServiceDisplayName}'");
                msh.StopService();

                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine(ex.Expand());
                Console.WriteLine();
                Console.WriteLine($"'{Settings.ServiceDisplayName}' остановлен");
                Console.WriteLine("Для выхода нажмите Enter.");
                Console.ReadLine();
            }

        }
    }
}
