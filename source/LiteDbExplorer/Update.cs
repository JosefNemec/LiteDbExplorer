using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LiteDbExplorer
{
    class Update
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

        public static bool IsUpdateAvailable
        {
            get
            {
                return GetLatestVersion().CompareTo(Versions.CurrentVersion) > 0;
            }
        }

        public static Version GetLatestVersion()
        {
            var dataString = (new WebClient()).DownloadString(Config.UpdateDataUrl);
            var latestData = JsonConvert.DeserializeObject<Dictionary<string, UpdateData>>(dataString)["stable"];
            return new Version(latestData.version);
        }
    }
}
