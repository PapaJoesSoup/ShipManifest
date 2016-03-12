using System;
using System.Reflection;
using System.Linq;

namespace ShipManifest
{
  public interface ICrewTransfer
  {
    bool CrewXferActive { get; set; }
    bool IsStockXfer { get; }
    bool OverrideStockCrewXfer { get; }
    double CrewXferDelaySec { get; }
    bool IsSeat2SeatXfer { get; }
    double Seat2SeatXferDelaySec { get; }

    bool IvaDelayActive { get; }

    // ReSharper disable once InconsistentNaming
    Guid XferVesselID { get; }

    Part FromPart { get; }
    Part ToPart { get; }

    ProtoCrewMember FromCrewMember { get; }
    ProtoCrewMember ToCrewMember { get; }

    InternalSeat FromSeat { get; }
    InternalSeat ToSeat { get; }
  }

  // ReSharper disable once InconsistentNaming
  public static class SMInterface
  {
    private static bool _smChecked;
    private static bool _smInstalled;

    public static bool IsSMInstalled
    {
      get
      {
        if (_smChecked) return _smInstalled;
        const string assemblyName = "ShipManifest";
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var assembly = (from a in assemblies
                        where a.FullName.Contains(assemblyName)
                        select a).SingleOrDefault();
        _smInstalled = assembly != null;
        _smChecked = true;
        return _smInstalled;
      }
    }

    public static ICrewTransfer GetCrewTransfer()
    {
      var smAddonType = AssemblyLoader.loadedAssemblies.SelectMany(a => a.assembly.GetExportedTypes()).SingleOrDefault(t => t.FullName == "ShipManifest..Process.TransferCrew");
      if (smAddonType == null) return null;
      var crewTransferObj = smAddonType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null, null);
      return (ICrewTransfer)crewTransferObj;
    }
  }
}
