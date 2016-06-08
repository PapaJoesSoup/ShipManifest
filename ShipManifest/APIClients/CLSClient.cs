using System;
using System.Linq;
using System.Reflection;

namespace ShipManifest.APIClients
{
  static class CLSClient
  {
    static PropertyInfo cls;

    static CLSClient()
    {
      Type cls_type = AssemblyLoader
        .loadedAssemblies
        .SelectMany(a => a.assembly.GetExportedTypes())
        .SingleOrDefault(t => t.FullName == "ConnectedLivingSpace.CLSAddon");

      if (cls_type != null) cls = cls_type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
    }

    public static bool CLSInstalled()
    {
      return cls != null;
    }

    public static ConnectedLivingSpace.ICLSAddon GetCLS()
    {
      return (ConnectedLivingSpace.ICLSAddon) cls?.GetValue(null, null);
    }
  }
}
