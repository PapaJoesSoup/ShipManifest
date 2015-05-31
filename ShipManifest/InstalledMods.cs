using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ShipManifest
{
    class InstalledMods
    {
        private static Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        public static bool IsSMInstalled
        {
            get
            {
                return IsModInstalled("ShipManifest");
            }
        }
        public static bool IsCLSInstalled
        {
            get
            {
                return IsModInstalled("ConnectedLivingSpace");
            }
        }
        public static bool IsDFInstalled
        {
            get
            {
                return IsModInstalled("DeepFreeze");
            }
        }

        internal static bool IsModInstalled(string assemblyName)
        {
            Assembly assembly = (from a in assemblies
                                 where a.FullName.Contains(assemblyName)
                                 select a).SingleOrDefault();
            return assembly != null;
        }
    }
}
