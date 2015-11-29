using System.Linq;
using System.Reflection;

namespace ShipManifest.APIClients
{
  class ClsClient
  {
    private static ConnectedLivingSpace.ICLSAddon _cls;
    private static bool? _clsAvailable;

    public static ConnectedLivingSpace.ICLSAddon GetCls()
    {
      var clsAddonType = AssemblyLoader.loadedAssemblies.SelectMany(a => a.assembly.GetExportedTypes()).SingleOrDefault(t => t.FullName == "ConnectedLivingSpace.CLSAddon");
      if (clsAddonType != null)
      {
        var realClsAddon = clsAddonType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null, null);
        _cls = (ConnectedLivingSpace.ICLSAddon)realClsAddon;
      }
      return _cls;
    }

    public static bool ClsInstalled
    {
      get
      {
        if (_clsAvailable == null)
        {
          _clsAvailable = GetCls() != null;
        }
        return (bool)_clsAvailable;
      }
    }
  }
}
