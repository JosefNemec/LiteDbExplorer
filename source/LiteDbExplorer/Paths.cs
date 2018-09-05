using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteDbExplorer
{
    public class Paths : INotifyPropertyChanged
    {
        public static string AppDataPath
        {
            get
            {
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LiteDbExplorer");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
        }

        public static string ProgramFolder
        {
            get
            {
                return Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            }
        }

        public static string UninstallerPath
        {
            get
            {
                return Path.Combine(ProgramFolder, "uninstall.exe");
            }
        }

        public static string RecentFilesPath
        {
            get
            {
                return Path.Combine(AppDataPath, "recentfiles.txt");
            }
        }

        public static string SettingsFilePath
        {
            get
            {
                return Path.Combine(AppDataPath, "settings.json");
            }
        }

        public static string TempPath
        {
            get
            {
                var path = Path.Combine(Path.GetTempPath(), "LiteDbExplorer");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }

        private ObservableCollection<string> recentFiles;
        public ObservableCollection<string> RecentFiles
        {
            get
            {
                if (recentFiles == null)
                {
                    if (File.Exists(RecentFilesPath))
                    {
                        recentFiles = new ObservableCollection<string>(File.ReadLines(RecentFilesPath));
                    }
                    else
                    {
                        recentFiles = new ObservableCollection<string>();
                    }

                    recentFiles.CollectionChanged += RecentFiles_CollectionChanged;
                }

                return recentFiles;
            }

            set
            {
                recentFiles = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RecentFiles"));
            }
        }

        private void RecentFiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            File.WriteAllText(RecentFilesPath, string.Join(Environment.NewLine, RecentFiles));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
