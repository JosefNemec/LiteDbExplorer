using NLog;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
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

        private void Application_Startup(object sender, StartupEventArgs e)
        {
#if !DEBUG
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#endif
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
