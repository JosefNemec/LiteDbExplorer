using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteDbExplorer
{
    public class Config
    {
        public static string UpdateDataUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["UpdateUrl"];
            }
        }

        public static string IssuesUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["IssuesUrl"];
            }
        }

        public static string HomepageUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["HomepageUrl"];
            }
        }

        public static string ReleasesUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["ReleasesUrl"];
            }
        }

        public static void ConfigureLogger()
        {
            var config = new LoggingConfiguration();
#if DEBUG
            var consoleTarget = new ColoredConsoleTarget()
            {
                Layout = @"${logger}:${message}${exception}"
            };

            config.AddTarget("console", consoleTarget);

            var rule1 = new LoggingRule("*", LogLevel.Debug, consoleTarget);
            config.LoggingRules.Add(rule1);
#endif
            var fileTarget = new FileTarget()
            {
                FileName = Path.Combine(Paths.ProgramFolder, "explorer.log"),
                Layout = "${longdate}|${level:uppercase=true}:${message}${exception:format=toString}",
                KeepFileOpen = false,
                ArchiveFileName = Path.Combine(Paths.ProgramFolder, "explorer.{#####}.log"),
                ArchiveAboveSize = 4096000,
                ArchiveNumbering = ArchiveNumberingMode.Sequence,
                MaxArchiveFiles = 2
            };

            config.AddTarget("file", fileTarget);

            var rule2 = new LoggingRule("*", LogLevel.Debug, fileTarget);
            config.LoggingRules.Add(rule2);

            LogManager.Configuration = config;
        }
    }
}
