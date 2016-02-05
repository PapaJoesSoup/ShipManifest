using System.Collections.Generic;
using System.Linq;
using ShipManifest.APIClients;
using ShipManifest.Process;

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
      if (!InstalledMods.IsDfInstalled || !part.Modules.Contains("DeepFreezer"))
        return part.protoModuleCrew.Count < part.CrewCapacity;
      var deepFreezer = (from PartModule pm in part.Modules where pm.moduleName == "DeepFreezer" select new DFWrapper.DeepFreezer(pm)).SingleOrDefault();
      return deepFreezer != null && deepFreezer.PartFull;
    }

    internal static void ToggleDumpResource(Part part, List<string> resourceNames, uint pumpId)
    {
      // This is fired by the part dump button on the TransferWindow interface.
      ToggleDumpResource(new List<Part>(){ part }, resourceNames, pumpId);
    }

    internal static void ToggleDumpResource(List<Part> partList, List<string> resourceNames, uint pumpId)
    {
      // This routine is called by the dump part buttin on the Transfer Window interface.
      // This routine is also called by the dump docked vessel button on the Transfer window interface.
      List<TransferPump> pumpList = resourceNames.Select(resource => new TransferPump(resource, TransferPump.TypePump.Dump, TransferPump.TriggerButton.Transfer, TransferPump.CalcRemainingResource(partList, resource))
      {
        FromParts = partList, PumpId = pumpId
      }).ToList();
      if (!TransferPump.PumpsInProgress(pumpId).Any())
        ProcessController.DumpResources(pumpList);
      else TransferPump.AbortPumpProcess(pumpId);
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