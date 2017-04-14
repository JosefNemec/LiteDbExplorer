using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteDbExplorer
{
    class Versions
    {
        public static Version CurrentVersion
        {
            get
            {
                return System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
            }
        }
    }
}
