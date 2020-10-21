﻿using System;
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
                ServiceBase.Run(new SkedVideoServiceHost());
                return;
            }

            try
            {
                Boolean uninstall = args.Any(x => x == "/uninstall");
                Boolean update = args.Any(x => x == "/update");

                String fileName = Assembly.GetExecutingAssembly().Location;

                #region Установка службы

#if !DEBUG

                if (!Manager.ServiceExist(WinServiceInstaller.ServiceName))
                {
                    if (!Manager.IsAdmin())
                    {
                        Manager.RunAsAdmin(fileName);
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

#endif

                #endregion

                #region Удаление службы

                if (uninstall)
                {
                    if (!Manager.ServiceExist(WinServiceInstaller.ServiceName))
                        return;

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

                #region Обновление приложения

                if (update)
                {


                }

                #endregion

                #region Работа в консольном режиме

                Console.WriteLine();
                Console.WriteLine(WinServiceInstaller.DisplayName);
                Console.WriteLine();
                Console.WriteLine("Для удаления службы выполните с параметром /u");
                Console.WriteLine();
                Console.WriteLine($"Запуск '{WinServiceInstaller.DisplayName}'");
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

                Console.WriteLine($"'{WinServiceInstaller.DisplayName}' запущен.");
                Console.WriteLine();
                Console.WriteLine("Для останова нажмите Enter.");
                while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
                Console.WriteLine();
                Console.WriteLine($"Останов '{WinServiceInstaller.DisplayName}'");
                msh.StopService();

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
