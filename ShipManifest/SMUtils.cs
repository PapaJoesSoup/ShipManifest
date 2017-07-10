using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KSP.Localization;
using ShipManifest.APIClients;
using ShipManifest.InternalObjects;
using ShipManifest.Windows;
using UnityEngine;

namespace ShipManifest
{
  internal static class SmUtils
  {
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

    internal static void LoadTexture(ref Texture2D tex, string fileName)
    {
      LogMessage($"Loading Texture - file://{PlugInPath}{fileName}", LogType.Info,
        SMSettings.VerboseLogging);
      WWW img1 = new WWW($"file://{PlugInPath}{fileName}");
      img1.LoadImageIntoTexture(tex);
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
      int crewCount = 0;
      if (!InstalledMods.IsDfApiReady) return crewCount + part.protoModuleCrew.Count;
      if (!part.Modules.Contains("DeepFreezer")) return crewCount + part.protoModuleCrew.Count;
      PartModule freezerModule = SMConditions.GetFreezerModule(part);
      // ReSharper disable once SuspiciousTypeConversion.Global
      DfWrapper.DeepFreezer freezer = new DfWrapper.DeepFreezer(freezerModule);
      crewCount += freezer.TotalFrozen;
      return crewCount + part.protoModuleCrew.Count;
    }

    internal static int GetCrewCount(List<Part> parts)
    {
      int count = 0;
      List<Part>.Enumerator part = parts.GetEnumerator();
      while (part.MoveNext())
      {
        if (part.Current == null) continue;
        if (part.Current.CrewCapacity <= 0) continue;
        count += part.Current.protoModuleCrew.Count;
      }
      part.Dispose();
      return count;
    }
    internal static int GetCrewCapacity(List<Part> parts)
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
    internal static void LogMessage(string msg, LogType type, bool verbose)
    {
      try
      {
        // Added rolling error list. This limits growth.  Configure with ErrorListLength
        if (_logItemList.Count > int.Parse(SMSettings.ErrorLogLength) && int.Parse(SMSettings.ErrorLogLength) > 0)
          _logItemList.RemoveRange(0, _logItemList.Count - int.Parse(SMSettings.ErrorLogLength));
        if (verbose) _logItemList.Add($"{type}: {msg}");
        if (type == LogType.Error && SMSettings.AutoDebug)
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

    internal static string Localize(string tag)
    {
      return SmTags[tag];
    }

    internal enum LogType
    {
      Info,
      Error
    }
  }
}