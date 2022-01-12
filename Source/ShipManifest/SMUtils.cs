using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KSP.Localization;
using ShipManifest.APIClients;
using ShipManifest.InternalObjects;
using ShipManifest.InternalObjects.Settings;
using ShipManifest.Windows;
using UnityEngine;

namespace ShipManifest
{
  internal static class SmUtils
  {

    public static Texture2D resizeTexture =
      GameDatabase.Instance.GetTexture(SMAddon.TextureFolder + "resizeSquare", false);
    internal static string AppPath = KSPUtil.ApplicationRootPath.Replace("\\", "/");
    internal static string PlugInPath = $"{AppPath}GameData/ShipManifest/Plugins/PluginData/ShipManifest/";
    internal static Vector2 DebugScrollPosition = Vector2.zero;
    internal static Dictionary<string, string> SmTags;

    // decimal string handlers for tex box
    internal static bool StrHasDecimal;
    internal static bool StrHasZero;

    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private static List<string> _logItemList = new List<string>();

    internal static List<string> LogItemList
    {
      get { return _logItemList; }
    }

    internal static string DisplayVesselResourceTotals(string selectedResource)
    {
      string displayAmount = "";
      double currAmount = 0;
      double totAmount = 0;
      if (selectedResource == null) return $" - ({currAmount:#######0})";
      try
      {
        if (SMConditions.IsResourceTypeOther(selectedResource))
        {
          List<Part>.Enumerator parts = SMAddon.SmVessel.PartsByResource[selectedResource].GetEnumerator();
          while (parts.MoveNext())
          {
            if (parts.Current == null) continue;
            currAmount += parts.Current.Resources[selectedResource].amount;
            totAmount += parts.Current.Resources[selectedResource].maxAmount;
          }
          parts.Dispose();
        }
        switch (SMConditions.TypeOfResource(selectedResource))
        {
          case SMConditions.ResourceType.Crew:
            currAmount = SMAddon.SmVessel.Vessel.GetCrewCount();
            totAmount = SMAddon.SmVessel.Vessel.GetCrewCapacity();

            // if DF installed, get total frozen and add to count.
            if (InstalledMods.IsDfInstalled)
            {
              List<Part>.Enumerator cryofreezers = GetFreezerParts().GetEnumerator();
              // ReSharper disable once SuspiciousTypeConversion.Global
              while (cryofreezers.MoveNext())
              {
                if (cryofreezers.Current == null) continue;
                currAmount += new DfWrapper.DeepFreezer(SMConditions.GetFreezerModule(cryofreezers.Current)).TotalFrozen;
              }
              cryofreezers.Dispose();
            }

            // Now check for occupied external seats
            // external seats that are occupied will show up in getcrewcount and getcrewcapacity
            // Since we cannot yet xfer external crew, we need to remove them from the count..
            List<KerbalSeat> seatCount = (from iPart in SMAddon.SmVessel.Vessel.parts
              where iPart.Modules.Contains("KerbalSeat")
              from PartModule iModule in iPart.Modules
              where iModule.ClassName == "KerbalSeat"
              select (KerbalSeat) iModule
              into kSeat
              where kSeat.Occupant != null
              select kSeat).ToList();
            currAmount -= seatCount.Count;
            totAmount -= seatCount.Count;
            break;
          case SMConditions.ResourceType.Science:
            currAmount +=
              SMAddon.SmVessel.PartsByResource[selectedResource].SelectMany(part => part.Modules.Cast<PartModule>())
                .OfType<IScienceDataContainer>()
                .Sum(module => (double) module.GetScienceCount());
            break;
        }
        displayAmount = selectedResource != SMConditions.ResourceType.Science.ToString()
          ? $" - ({currAmount:#######0}/{totAmount:######0})"
          : $" - ({currAmount:#######0})";
      }
      catch (Exception ex)
      {
        LogMessage($" in DisplayResourceTotals().  Error:  {ex}", LogType.Error, true);
      }

      return displayAmount;
    }

    internal static List<Part> GetFreezerParts()
    {
      return (from p in SMAddon.SmVessel.Vessel.parts where p.Modules.Contains("DeepFreezer") select p).ToList();
    }

    internal static int GetPartCrewCount(Part part)
    {
      int crewCount = part.protoModuleCrew.Count;
      if (!InstalledMods.IsDfApiReady) return crewCount;
      if (!part.Modules.Contains("DeepFreezer")) return crewCount;
      PartModule freezerModule = SMConditions.GetFreezerModule(part);
      // ReSharper disable once SuspiciousTypeConversion.Global
      DfWrapper.DeepFreezer freezer = new DfWrapper.DeepFreezer(freezerModule);
      crewCount += freezer.TotalFrozen;
      return crewCount;
    }

    internal static int GetPartsCrewCount(List<Part> parts)
    {
      int count = 0;
      List<Part>.Enumerator part = parts.GetEnumerator();
      while (part.MoveNext())
      {
        if (part.Current == null) continue;
        if (part.Current.CrewCapacity <= 0) continue;
        count += part.Current.protoModuleCrew.Count;
        if (!InstalledMods.IsDfApiReady) continue;
        if (!part.Current.Modules.Contains("DeepFreezer")) continue;
        PartModule freezerModule = SMConditions.GetFreezerModule(part.Current);
        // ReSharper disable once SuspiciousTypeConversion.Global
        DfWrapper.DeepFreezer freezer = new DfWrapper.DeepFreezer(freezerModule);
        count += freezer.TotalFrozen;
      }
      part.Dispose();
      return count;
    }
    internal static int GetPartsCrewCapacity(List<Part> parts)
    {
      int count = 0;
      List<Part>.Enumerator part = parts.GetEnumerator();
      while (part.MoveNext())
      {
        if (part.Current == null) continue;
        if (part.Current.CrewCapacity <= 0) continue;
        count += part.Current.CrewCapacity;
      }
      part.Dispose();
      return count;
    }

    internal static bool CrewContainsTourists(List<Part> selectedParts)
    {
      bool results = false;
      List<Part>.Enumerator part = selectedParts.GetEnumerator();
      while (part.MoveNext())
      {
        if (part.Current == null) continue;
        List<ProtoCrewMember>.Enumerator crew = part.Current.protoModuleCrew.GetEnumerator();
        while (crew.MoveNext())
        {
          if (crew.Current?.type != ProtoCrewMember.KerbalType.Tourist) continue;
          results = true;
          break;
        }
        crew.Dispose();
        if (results) break;
      }
      part.Dispose();
      return results;
    }
    internal static void LogMessage(string msg, LogType type, bool verbose)
    {
      try
      {
        // Added rolling error list. This limits growth.  Configure with ErrorListLength
        if (_logItemList.Count > int.Parse(CurrSettings.ErrorLogLength) && int.Parse(CurrSettings.ErrorLogLength) > 0)
          _logItemList.RemoveRange(0, _logItemList.Count - int.Parse(CurrSettings.ErrorLogLength));
        if (verbose) _logItemList.Add($"{type}: {msg}");
        if (type == LogType.Error && CurrSettings.AutoDebug)
          WindowDebugger.ShowWindow = true;
        Debug.Log($"[ShipManifest] - {type}:  {msg}");
      }
      catch (Exception ex)
      {
        _logItemList.Add($"Error: {ex}");
        WindowDebugger.ShowWindow = true;
      }
    }

    internal static string GetStringDecimal(string strValue)
    {
      if (StrHasDecimal)
        strValue += ".";
      return strValue;
    }

    internal static string GetStringZero(string strValue)
    {
      if (StrHasZero)
        strValue += "0";
      return strValue;
    }

    internal static void SetStringZero(string strValue)
    {
      if (strValue.Contains(".") && strValue.EndsWith("0"))
        StrHasZero = true;
      else
        StrHasZero = false;
    }

    internal static void SetStringDecimal(string strValue)
    {
      if (strValue.EndsWith(".") || strValue.EndsWith(".0"))
        StrHasDecimal = true;
      else
        StrHasDecimal = false;
    }

    internal static void CacheSmLocalization()
    {
      SmTags = new Dictionary<string, string>();
      IEnumerator tags = Localizer.Tags.Keys.GetEnumerator();
      while (tags.MoveNext())
      {
        if (tags.Current == null) continue;
        if (tags.Current.ToString().Contains("#smloc_"))
        {
          SmTags.Add(tags.Current.ToString(), Localizer.GetStringByTag(tags.Current.ToString()).Replace("\\n", "\n"));
        }
      }
    }


    internal enum LogType
    {
      Info,
      Error
    }

        // Enum extensions
    internal static VesselType Next(this VesselType type)
    {
      // Debris = 0,
      // SpaceObject = 1,
      // Unknown = 2,
      // Probe = 3,
      // Relay = 4,
      // Rover = 5,
      // Lander = 6,
      // Ship = 7,
      // Plane = 8,
      // Station = 9,
      // Base = 10,
      // EVA = 11, skip
      // Flag = 12, skip
      // DeployedScienceController = 13,
      // DeployedSciencePart = 14,
      // DroppedPart = 15,
      // DeployedGroundPart = 16

      switch (type)
      {
        case VesselType.Debris:
          return VesselType.SpaceObject;
        case VesselType.SpaceObject:
          return VesselType.Unknown;
        case VesselType.Unknown:
          return VesselType.Probe;
        case VesselType.Probe:
          return VesselType.Relay;
        case VesselType.Relay:
          return VesselType.Rover;
        case VesselType.Rover:
          return VesselType.Lander;
        case VesselType.Lander:
          return VesselType.Ship;
        case VesselType.Ship:
          return VesselType.Plane;
        case VesselType.Plane:
          return VesselType.Station;
        case VesselType.Station:
          return VesselType.Base;
        case VesselType.Base:
          return VesselType.DeployedScienceController;
        case VesselType.DeployedScienceController:
          return VesselType.DeployedSciencePart;
        case VesselType.DeployedSciencePart:
          return VesselType.DroppedPart;
        case VesselType.DroppedPart:
          return VesselType.DeployedGroundPart;
        case VesselType.DeployedGroundPart:
          return VesselType.Debris;
        default:
          return VesselType.Unknown;
      }
    }
    internal static VesselType Back(this VesselType type)
    {
      // DeployedGroundPart = 16
      // DroppedPart = 15,
      // DeployedSciencePart = 14,
      // DeployedScienceController = 13,
      // Flag = 12,Skip
      // EVA = 11, Skip
      // Base = 10,
      // Station = 9,
      // Plane = 8,
      // Ship = 7,
      // Lander = 6,
      // Rover = 5,
      // Relay = 4,
      // Probe = 3,
      // Unknown = 2,
      // SpaceObject = 1,
      // Debris = 0,

      switch (type)
      {
        case VesselType.DeployedGroundPart:
          return VesselType.DroppedPart;
        case VesselType.DroppedPart:
          return VesselType.DeployedSciencePart;
        case VesselType.DeployedSciencePart:
          return VesselType.DeployedScienceController;
        case VesselType.DeployedScienceController:
          return VesselType.Base;
        case VesselType.Base:
          return VesselType.Station;
        case VesselType.Station:
          return VesselType.Plane;
        case VesselType.Plane:
          return VesselType.Ship;
        case VesselType.Ship:
          return VesselType.Lander;
        case VesselType.Lander:
          return VesselType.Rover;
        case VesselType.Rover:
          return VesselType.Relay;
        case VesselType.Relay:
          return VesselType.Probe;
        case VesselType.Probe:
          return VesselType.Unknown;
        case VesselType.Unknown:
          return VesselType.SpaceObject;
        case VesselType.SpaceObject:
          return VesselType.Debris;
        case VesselType.Debris:
          return VesselType.DeployedGroundPart;
        default:
          return VesselType.Unknown;
      }
    }
  }
}
