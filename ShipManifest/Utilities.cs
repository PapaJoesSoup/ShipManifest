using System;
using System.Collections.Generic;
using System.Linq;
using ShipManifest.APIClients;
using ShipManifest.InternalObjects;
using ShipManifest.Windows;
using UnityEngine;

namespace ShipManifest
{
  internal static class Utilities
  {
    internal static string AppPath = KSPUtil.ApplicationRootPath.Replace("\\", "/");
    internal static string PlugInPath = AppPath + "GameData/ShipManifest/Plugins/PluginData/ShipManifest/";
    internal static Vector2 DebugScrollPosition = Vector2.zero;

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
      LogMessage(string.Format("Loading Texture - file://{0}{1}", PlugInPath, fileName), LogType.Info,
        SMSettings.VerboseLogging);
      WWW img1 = new WWW(string.Format("file://{0}{1}", PlugInPath, fileName));
      img1.LoadImageIntoTexture(tex);
    }

    internal static string DisplayVesselResourceTotals(string selectedResource)
    {
      string displayAmount = "";
      double currAmount = 0;
      double totAmount = 0;
      if (selectedResource == null) return string.Format(" - ({0})", currAmount.ToString("#######0"));
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
                currAmount += new DFWrapper.DeepFreezer(SMConditions.GetFreezerModule(cryofreezers.Current)).TotalFrozen;
              }
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
          ? string.Format(" - ({0}/{1})", currAmount.ToString("#######0"), totAmount.ToString("######0"))
          : string.Format(" - ({0})", currAmount.ToString("#######0"));
      }
      catch (Exception ex)
      {
        LogMessage(string.Format(" in DisplayResourceTotals().  Error:  {0}", ex), LogType.Error, true);
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
      DFWrapper.DeepFreezer freezer = new DFWrapper.DeepFreezer(freezerModule);
      crewCount += freezer.TotalFrozen;
      return crewCount + part.protoModuleCrew.Count;
    }

    internal static void LogMessage(string msg, LogType type, bool verbose)
    {
      try
      {
        // Added rolling error list. This limits growth.  Configure with ErrorListLength
        if (_logItemList.Count > int.Parse(SMSettings.ErrorLogLength) && int.Parse(SMSettings.ErrorLogLength) > 0)
          _logItemList.RemoveRange(0, _logItemList.Count - int.Parse(SMSettings.ErrorLogLength));
        if (verbose)
          _logItemList.Add(type + ": " + msg);
        if (type == LogType.Error && SMSettings.AutoDebug)
          WindowDebugger.ShowWindow = true;
        Debug.Log(string.Format("[ShipManifest] - {0}:  {1}", type,msg));
      }
      catch (Exception ex)
      {
        _logItemList.Add("Error: " + ex);
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

    internal enum LogType
    {
      Info,
      Error
    }
  }
}