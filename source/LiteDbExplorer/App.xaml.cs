using NLog;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace LiteDbExplorer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string instanceMuxet = "LiteDBExplorerInstaceMutex";
        private Mutex appMutex;

        public bool OriginalInstance
        {
            get;
            private set;
        } = false;

        public static Settings Settings
        {
            get; set;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
#if !DEBUG
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#endif
            Settings = Settings.LoadSettings();
            Config.ConfigureLogger();

            // For now we want to allow multiple instances if app is started without args
            if (Mutex.TryOpenExisting(instanceMuxet, out var mutex))
            {
                var client = new PipeClient(ConfigurationManager.AppSettings["PipeEndpoint"]);

                if (e.Args.Count() > 0)
                {
                    client.InvokeCommand(CmdlineCommands.Open, e.Args[0]);                    
                    Shutdown();
                    return;
                }
            }
            else
            {
                appMutex = new Mutex(true, instanceMuxet);
                OriginalInstance = true;
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Settings.SaveSettings();

            if (appMutex != null)
            {
                appMutex.ReleaseMutex();
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var log = LogManager.Configuration.FindTargetByName("file") as FileTarget;
            logger.Error((Exception)e.ExceptionObject, "Unhandled exception: ");
            MessageBox.Show(string.Format("Unhandled exception occured.\nAdditional information written into: {0}\n\nApplication will shutdown.", log.FileName),
                "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            Process.GetCurrentProcess().Kill();
        }
    }
}
