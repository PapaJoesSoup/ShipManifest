using System.Collections.Generic;
using System.Linq;
using DF;

namespace ShipManifest
{
  // ReSharper disable once InconsistentNaming
  internal static class SMPart
  {
    internal static void FillCrew(Part part)
    {
      if (!part.vessel.IsRecoverable || IsCrewFull(part)) return;
      while (part.CrewCapacity > Utilities.GetPartCrewCount(part))
      {
        var kerbal = HighLogic.CurrentGame.CrewRoster.GetNextOrNewKerbal();
        part.AddCrewmember(kerbal);

        if (kerbal.seat != null)
          kerbal.seat.SpawnCrew();
      }
    }

    internal static void DumpCrew(Part part)
    {
      if (!part.vessel.IsRecoverable) return;
      while (part.protoModuleCrew.Count > 0)
      {
        var kerbal = part.protoModuleCrew.FirstOrDefault();
        if (kerbal != null)
        {
          part.RemoveCrewmember(kerbal);
          kerbal.rosterStatus = ProtoCrewMember.RosterStatus.Available;

          if (kerbal.seat != null)
            kerbal.seat.SpawnCrew();
        }
      }
    }

    internal static bool IsCrewFull(Part part)
    {
      if (!DFInterface.IsDFInstalled || !part.Modules.Contains("DeepFreezer"))
        return part.protoModuleCrew.Count < part.CrewCapacity;
      // ReSharper disable once SuspiciousTypeConversion.Global
      var deepFreezer = (from PartModule pm in part.Modules where pm.moduleName == "DeepFreezer" select (IDeepFreezer)pm).SingleOrDefault();
      return deepFreezer != null && deepFreezer.DFIPartFull;
    }

    internal static void DumpResource(Part part, string resourceName)
    {
      if (part.Resources.Contains(resourceName))
        part.Resources[resourceName].amount = 0;
    }

    internal static void DumpResource(List<Part> partList, string resourceName)
    {
      foreach (var part in partList.Where(part => part.Resources.Contains(resourceName)))
      {
        part.Resources[resourceName].amount = 0;
      }
    }

    internal static void FillResource(Part part, string resourceName)
    {
      if (part.Resources.Contains(resourceName))
        part.Resources[resourceName].amount = part.Resources[resourceName].maxAmount;
    }

    internal static void FillResource(List<Part> partList, string resourceName)
    {
      foreach (var part in partList.Where(part => part.Resources.Contains(resourceName)))
      {
        part.Resources[resourceName].amount = part.Resources[resourceName].maxAmount;
      }
    }
  }
}