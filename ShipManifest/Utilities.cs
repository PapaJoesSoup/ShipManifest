using KSP.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;

namespace ShipManifest
{
  internal static class Utilities
  {
    internal static String AppPath = KSPUtil.ApplicationRootPath.Replace("\\", "/");
    internal static String PlugInPath = AppPath + "GameData/ShipManifest/Plugins/PluginData/ShipManifest/";
    internal static Vector2 DebugScrollPosition = Vector2.zero;

    // decimal string handlers for tex box
    internal static bool strHasDecimal;
    internal static bool strHasZero;

    private static List<string> _errors = new List<string>();
    internal static List<string> Errors
    {
      get { return _errors; }
    }

    internal static void LoadTexture(ref Texture2D tex, String FileName)
    {
      LogMessage(String.Format("Loading Texture - file://{0}{1}", PlugInPath, FileName), "Info", SMSettings.VerboseLogging);
      WWW img1 = new WWW(String.Format("file://{0}{1}", PlugInPath, FileName));
      img1.LoadImageIntoTexture(tex);
    }

    internal static string DisplayVesselResourceTotals(string selectedResource)
    {
      string displayAmount = "";
      double currAmount = 0;
      double totAmount = 0;
      try
      {
        if (selectedResource != "Crew" && selectedResource != "Science")
        {
          foreach (Part part in SMAddon.smController._partsByResource[selectedResource])
          {
            currAmount += part.Resources[selectedResource].amount;
            totAmount += part.Resources[selectedResource].maxAmount;
          }
        }
        else if (selectedResource == "Crew")
        {
          currAmount = (double)SMAddon.smController.Vessel.GetCrewCount();
          totAmount = (double)SMAddon.smController.Vessel.GetCrewCapacity();

          // if DF installed, get total frozen and add to count.
          if (DF.DFInterface.IsDFInstalled)
          {
            List<Part> cryofreezers = (from p in SMAddon.smController.Vessel.parts where p.Modules.Contains("DeepFreezer") select p).ToList();
            foreach (Part CryoFreezer in cryofreezers)
            {
              PartModule deepFreezer = (from PartModule pm in CryoFreezer.Modules where pm.moduleName == "DeepFreezer" select pm).SingleOrDefault();
              currAmount += ((DF.IDeepFreezer)deepFreezer).DFITotalFrozen ;
            }
          }

          // Now check for occupied external seats
          // external seats that are occupied will show up in getcrewcount and getcrewcapacity
          // Since we cannot yet xfer external crew, we need to remove them from the count..
          foreach (Part iPart in SMAddon.smController.Vessel.parts)
          {
            if (iPart.Modules.Contains("KerbalSeat"))
            {
              foreach (PartModule iModule in iPart.Modules)
              {
                if (iModule.ClassName == "KerbalSeat")
                {
                  KerbalSeat kSeat = (KerbalSeat)iModule;
                  if (kSeat.Occupant != null)
                  {
                    currAmount -= 1;
                    totAmount -= 1;
                  }
                }
              }
            }
          }
        }
        else if (selectedResource == "Science")
        {
          foreach (Part part in SMAddon.smController._partsByResource[selectedResource])
          {
            foreach (PartModule module in part.Modules)
            {
              if (module is IScienceDataContainer)
              {
                currAmount += (double)((IScienceDataContainer)module).GetScienceCount();
              }
            }
          }
        }
        if (selectedResource != "Science")
          displayAmount = string.Format(" - ({0}/{1})", currAmount.ToString("#######0"), totAmount.ToString("######0"));
        else
          displayAmount = string.Format(" - ({0})", currAmount.ToString("#######0"));
      }
      catch (Exception ex)
      {
        LogMessage(String.Format(" in DisplayResourceTotals().  Error:  {0}", ex.ToString()), "Error", true);
      }

      return displayAmount;
    }

    internal static int GetPartCrewCount(Part part)
    {
      int crewCount = 0;
      if (DF.DFInterface.IsDFInstalled)
      {
        if (part.Modules.Contains("DeepFreezer"))
        {
          PartModule deepFreezer = (from PartModule pm in part.Modules where pm.moduleName == "DeepFreezer" select pm).SingleOrDefault();
          crewCount += ((DF.IDeepFreezer)deepFreezer).DFITotalFrozen;
        }
      }
      return (crewCount + part.protoModuleCrew.Count);
    }

    internal static void LogMessage(string error, string type, bool verbose)
    {
      try
      {
        // Add rolling error list. This limits growth.  Configure with ErrorListLength
        if (_errors.Count() > int.Parse(SMSettings.ErrorLogLength) && int.Parse(SMSettings.ErrorLogLength) > 0)
          _errors.RemoveRange(0, _errors.Count() - int.Parse(SMSettings.ErrorLogLength));
        if (verbose)
          _errors.Add(type + ": " + error);
        if (type == "Error" && SMSettings.AutoDebug)
          WindowDebugger.ShowWindow = true;
      }
      catch (Exception ex)
      {
        _errors.Add("Error: " + ex.ToString());
        WindowDebugger.ShowWindow = true;
      }
    }

    internal static string GetStringDecimal(string strValue)
    {
      if (strHasDecimal)
        strValue += ".";
      return strValue;
    }

    internal static string GetStringZero(string strValue)
    {
      if (strHasZero)
        strValue += "0";
      return strValue;
    }

    internal static void SetStringZero(string strValue)
    {
      if (strValue.Contains(".") && strValue.EndsWith("0"))
        strHasZero = true;
      else
        strHasZero = false;
    }

    internal static void SetStringDecimal(string strValue)
    {
      if (strValue.EndsWith(".") || strValue.EndsWith(".0"))
        strHasDecimal = true;
      else
        strHasDecimal = false;
    }
  }
}
