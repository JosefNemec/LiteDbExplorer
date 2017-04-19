using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LiteDbExplorer
{
    public class Update
    {
        public class UpdateData
        {
            public string version
            {
                get; set;
            }

            public string url
            {
                get; set;
            }
        }

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private UpdateData latestData;
        private string updaterPath = Path.Combine(Paths.TempPath, "update.exe");
        private string downloadCompletePath = Path.Combine(Paths.TempPath, "download.done");

        public bool IsUpdateAvailable
        {
            get
            {
                return GetLatestVersion().CompareTo(Versions.CurrentVersion) > 0;
            }
        }

        public void DownloadUpdate()
        {
            if (latestData == null)
            {
                GetLatestVersion();
            }

            DownloadUpdate(latestData.url);
        }

        public void DownloadUpdate(string url)
        {
            logger.Info("Downloading new update from " + url);
            Directory.CreateDirectory(Paths.TempPath);

            if (File.Exists(downloadCompletePath) && File.Exists(updaterPath))
            {                
                var info = FileVersionInfo.GetVersionInfo(updaterPath);
                if (info.FileVersion == GetLatestVersion().ToString())
                {
                    logger.Info("Update already ready to install");
                    return;
                }
                else
                {
                    File.Delete(downloadCompletePath);
                }
            }

            (new WebClient()).DownloadFile(url, updaterPath);
            File.Create(downloadCompletePath);
        }

        public void InstallUpdate()
        {
            var portable = Config.IsPortable ? "/Portable 1" : "/Portable 0";
            logger.Info("Installing new update to {0}, in {1} mode", Paths.ProgramFolder, portable);
            Task.Factory.StartNew(() =>
            {
                Process.Start(updaterPath, string.Format(@"/ProgressOnly 1 {0} /D={1}", portable, Paths.ProgramFolder));
            });

            Application.Current.Dispatcher.Invoke(() =>
            {
                Application.Current.MainWindow.Close();
            });
        }

        public Version GetLatestVersion()
        {
            var dataString = (new WebClient()).DownloadString(Config.UpdateDataUrl);
            latestData = JsonConvert.DeserializeObject<Dictionary<string, UpdateData>>(dataString)["stable"];
            return new Version(latestData.version);
        }
    }
}
