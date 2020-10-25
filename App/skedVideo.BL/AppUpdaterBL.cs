﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Cav;
using Cav.WinService;
using RestSharp;
using skedVideo.BL;
using skedVideo.BL.InstallRutine;

namespace skedVideo.UpdateApp
{
    public class UpdaterBL
    {
        private class relInfoT
        {
            public class assetsT
            {
                public string browser_download_url { get; set; }
            }

            public string tag_name { get; set; }
            public List<assetsT> assets { get; set; }
        }

        public UpdaterBL(string serviceName)
        {
            this.serviceName = serviceName;
            this.tempDirFromrepo = Path.Combine(DomainContext.TempPath, "fromrepo");
            this.tempNewBin = Path.Combine(DomainContext.TempPath, "newbin");
        }

        private string serviceName;
        private string tempDirFromrepo;
        private string tempNewBin;

        private relInfoT infoFromRepoGet()
        {
            var req = new RestRequest("https://api.github.com/repos/ChernenkoAV/skedVideo/releases/latest");
            var resp = new RestClient().Execute(req);
            return resp.Content.JsonDeserealize<relInfoT>();
        }

        public void foo()
        {
            Manager.StartService(serviceName);
            Manager.StopService(serviceName);
        }


        public bool ExistsNewVersion()
        {
            var ev = false;

            var currebtVer = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).ProductVersion;

            var repInfo = infoFromRepoGet();

            ev = new[] { currebtVer, repInfo.tag_name }.OrderByDescending(x => x).FirstOrDefault() != currebtVer;

            return ev;
        }

        public void UpdateStep1()
        {
            Utils.DeleteDirectory(tempDirFromrepo);
            Directory.CreateDirectory(tempDirFromrepo);

            var targetFile = Path.Combine(tempDirFromrepo, "nw.zip");

            var repinfo = infoFromRepoGet();

            using (var wc = new WebClient())
                wc.DownloadFile(repinfo.assets.First().browser_download_url, targetFile);

            Utils.DeleteDirectory(tempNewBin);
            Directory.CreateDirectory(tempNewBin);

            ZipFile.ExtractToDirectory(targetFile, tempNewBin);
            Utils.DeleteDirectory(tempDirFromrepo);

            var fileName = Assembly.GetEntryAssembly().Location;
            Process.Start(Path.Combine(tempNewBin, Path.GetFileName(fileName)), $"/update \"{fileName}\"");
        }

        public void UpdateStep2(string targetFileName)
        {
            Manager.StopService(serviceName);

            var sourcePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var targetPath = Path.GetDirectoryName(targetFileName);

            var oldFiles = new DirectoryInfo(targetPath).GetFileSystemInfos("*.*", SearchOption.TopDirectoryOnly);

            foreach (var of in oldFiles)
            {
                if ((of.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    Utils.DeleteDirectory(of.FullName);
                    continue;
                }

                if (File.Exists(of.FullName))
                    File.Delete(of.FullName);

            }

            new DirectoryInfo(sourcePath).GetFileSystemInfos("*.*", SearchOption.AllDirectories)
                .ToList()
                .ForEach(fn =>
                {
                    var targFn = fn.FullName.Replace(sourcePath, targetPath);

                    if ((fn.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        Directory.CreateDirectory(targFn);
                        return;
                    }

                    File.Copy(fn.FullName, targFn);
                });

            Manager.StartService(serviceName);
        }

        public static void VisitSystemComponents(CancellationToken token)
        {

            if (WrapPlayerBL.PlayerExists())
                return;

            var processExitCode = Process.Start("choco").ExitCode;
            if (processExitCode != 1)
                processExitCode = Process.Start(InstallContsts.Chocolatey_InstalCmd).ExitCode;

            if (processExitCode != 0)
                throw new Exception("уставновка chocolatley неуспешна. Установите вручную.");

            if (!WrapPlayerBL.PlayerExists())
                processExitCode = Process.Start(InstallContsts.Klite_InstalCmd).ExitCode;

            if (processExitCode != 0)
                throw new Exception("уставновка K-Lite Codec Pack неуспешна. Установите K-Lite Codec Pack вручную редакции не ниже Standard.");

            //Запуск потока проверки обновления
            new Task(() =>
                {
                    var day = DateTime.Now.Day;

                    while (!token.IsCancellationRequested)
                    {
                        Task.Delay(TimeSpan.FromMinutes(1), token).ContinueWith((a) => { }).GetAwaiter().GetResult();

                        if (day == DateTime.Now.Day)
                            continue;

                        day = DateTime.Now.Day;

                        var ubl = new UpdaterBL(Settings.ServiceName);

                        if (!ubl.ExistsNewVersion())
                            continue;

                        ubl.UpdateStep1();
                    }
                },
                creationOptions: TaskCreationOptions.LongRunning)
            .Start();
        }

    }
}
