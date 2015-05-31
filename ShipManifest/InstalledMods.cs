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

        internal static bool IsRTInstalled
        {
            get
            {
                return IsModInstalled("RemoteTech");
            }
        }
        internal static bool IsSMInstalled
        {
            get
            {
                return IsModInstalled("ShipManifest");
            }
        }
        internal static bool IsKISInstalled
        {
            get
            {
                return IsModInstalled("KIS");
            }
        }
        internal static bool IsCLSInstalled
        {
            get
            {
                return IsModInstalled("ConnectedLivingSpace");
            }
        }
        internal static bool IsDFInstalled
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
