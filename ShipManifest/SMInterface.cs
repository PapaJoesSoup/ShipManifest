using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ShipManifest
{
  public interface ITransferCrew
  {
    bool CrewXferActive { get; set; }
    bool IsStockXfer { get; }
    bool OverrideStockCrewXfer { get; }
    double CrewXferDelaySec { get; }
    bool IsSeat2SeatXfer { get; }
    double Seat2SeatXferDelaySec { get; }

    bool IvaDelayActive { get; }

    Guid XferVesselId { get; }

    Part FromPart { get; }
    Part ToPart { get; }

    ProtoCrewMember FromCrewMember { get; }
    ProtoCrewMember ToCrewMember { get; }

    InternalSeat FromSeat { get; }
    InternalSeat ToSeat { get; }
  }

  public interface ITransferProcess
  {
    bool PumpProcessOn { get; }
    bool CrewProcessOn { get; }
    ITransferCrew CrewTransferProcess { get; }
    List<ITransferPump> PumpsInProgress { get; }
  }

  public interface ITransferPump
  {
    string Resource { get; }
    List<Part> FromParts { get; }
    List<Part> ToParts { get; }
    bool IsComplete { get; }
    bool IsPumpOn { get; }

    double PumpAmount { get; }
    double AmtPumped { get; }
    double PumpBalance { get; }

    double FlowRate { get; }
    double PumpRatio { get; }

    double FromCapacity { get; }
    double FromRemaining { get; }
    bool FromIsEmpty { get; }
    bool FromIsFull { get; }

    double ToCapacity { get; }
    double ToCapacityRemaining { get; }
    double ToRemaining { get; }
    bool ToIsFull { get; }
    bool ToIsEmpty { get; }

    string Info { get; }

    void Abort();
  }

  // ReSharper disable once InconsistentNaming
  public static class SmInterface
  {
    private static bool _smChecked;
    private static bool _smInstalled;

    public static bool IsSmInstalled
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

    public static ITransferProcess GetSmProcesses()
    {
      var smAddonType =
        AssemblyLoader.loadedAssemblies.SelectMany(a => a.assembly.GetExportedTypes())
          .SingleOrDefault(t => t.FullName == "ShipManifest.SMAddon");
      if (smAddonType == null) return null;
      var transferProcess = smAddonType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)
        .GetValue(null, null);
      return (ITransferProcess) transferProcess;
    }
  }
}