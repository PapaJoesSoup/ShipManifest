using System;
using System.Linq;
using System.Reflection;

namespace ShipManifest.APIClients
{
  internal static class ClsClient
  {
    private static PropertyInfo _cls;

    static ClsClient()
    {
      try
      {
        // Original call.  deep dives into all assemblies...
        //Type cls_type = AssemblyLoader
        //  .loadedAssemblies
        //  .SelectMany(a => a.assembly.GetExportedTypes())
        //  .SingleOrDefault(t => t.FullName == "ConnectedLivingSpace.CLSAddon");

        // this replacement call attempts to filter dynamic assemblies...  Dot.Net 2.0 vs Dot.Net 4.0
        //Type newType = AssemblyLoader
        //  .loadedAssemblies.Where(a => a.assembly.ManifestModule is System.Reflection.Emit.ModuleBuilder == false)
        //  .SelectMany(a => a.assembly.GetExportedTypes())
        //  .SingleOrDefault(t => t.FullName == "ConnectedLivingSpace.CLSAddon");

        // Lighter weight, and should not "dive into" assemblies unnecessarily.
        Type clsType =
          AssemblyLoader.loadedAssemblies.Where(a => a.name.Contains("ConnectedLivingSpace"))
            .SelectMany(a => a.assembly.GetExportedTypes())
            .SingleOrDefault(t => t.FullName == "ConnectedLivingSpace.CLSAddon");

        if (clsType != null) _cls = clsType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage($"Cannot load CLS assembly.  Error:  {ex}", SmUtils.LogType.Error, false);
      }
    }

    public static bool ClsInstalled()
    {
      return _cls != null;
    }

    public static ConnectedLivingSpace.ICLSAddon GetCls()
    {
      return (ConnectedLivingSpace.ICLSAddon) _cls?.GetValue(null, null);
    }
  }
}
