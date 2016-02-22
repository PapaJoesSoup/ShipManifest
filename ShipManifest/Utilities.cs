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
    private static List<string> _errors = new List<string>();

    internal static List<string> Errors
    {
      get { return _errors; }
    }

    internal static void LoadTexture(ref Texture2D tex, string fileName)
    {
      LogMessage(string.Format("Loading Texture - file://{0}{1}", PlugInPath, fileName), "Info",
        SMSettings.VerboseLogging);
      var img1 = new WWW(string.Format("file://{0}{1}", PlugInPath, fileName));
      img1.LoadImageIntoTexture(tex);
    }

    internal static string DisplayVesselResourceTotals(string selectedResource)
    {
      var displayAmount = "";
      double currAmount = 0;
      double totAmount = 0;
      try
      {
        if (SMConditions.IsResourceTypeOther(selectedResource))
        {
          foreach (var part in SMAddon.SmVessel.PartsByResource[selectedResource])
          {
            currAmount += part.Resources[selectedResource].amount;
            totAmount += part.Resources[selectedResource].maxAmount;
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
              var cryofreezers =
                (from p in SMAddon.SmVessel.Vessel.parts where p.Modules.Contains("DeepFreezer") select p).ToList();
              // ReSharper disable once SuspiciousTypeConversion.Global
              currAmount =
                cryofreezers.Select(
                  cryoFreezer =>
                    (from PartModule pm in cryoFreezer.Modules where pm.moduleName == "DeepFreezer" select pm)
                      .SingleOrDefault())
                  .Aggregate(currAmount,
                    (current, deepFreezer) => current + new DFWrapper.DeepFreezer(deepFreezer).TotalFrozen);
            }

            // Now check for occupied external seats
            // external seats that are occupied will show up in getcrewcount and getcrewcapacity
            // Since we cannot yet xfer external crew, we need to remove them from the count..
            var seatCount = (from iPart in SMAddon.SmVessel.Vessel.parts
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
        LogMessage(string.Format(" in DisplayResourceTotals().  Error:  {0}", ex), "Error", true);
      }

      return displayAmount;
    }

    internal static int GetPartCrewCount(Part part)
    {
      var crewCount = 0;
      if (!InstalledMods.IsDfInstalled) return crewCount + part.protoModuleCrew.Count;
      if (part.Modules.Contains("DeepFreezer"))
      {
        var freezerModule =
          (from PartModule pm in part.Modules where pm.moduleName == "DeepFreezer" select pm).SingleOrDefault();
        // ReSharper disable once SuspiciousTypeConversion.Global
        var freezer = new DFWrapper.DeepFreezer(freezerModule);
        crewCount += freezer.TotalFrozen;
      }
      return crewCount + part.protoModuleCrew.Count;
    }

    internal static void LogMessage(string error, string type, bool verbose)
    {
      try
      {
        // Add rolling error list. This limits growth.  Configure with ErrorListLength
        if (_errors.Count > int.Parse(SMSettings.ErrorLogLength) && int.Parse(SMSettings.ErrorLogLength) > 0)
          _errors.RemoveRange(0, _errors.Count - int.Parse(SMSettings.ErrorLogLength));
        if (verbose)
          _errors.Add(type + ": " + error);
        if (type == "Error" && SMSettings.AutoDebug)
          WindowDebugger.ShowWindow = true;
      }
      catch (Exception ex)
      {
        _errors.Add("Error: " + ex);
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
  }
}