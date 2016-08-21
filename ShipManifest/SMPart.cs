using System.Collections.Generic;
using System.Linq;
using ShipManifest.APIClients;
using ShipManifest.InternalObjects;
using ShipManifest.Process;

namespace ShipManifest
{
  // ReSharper disable once InconsistentNaming
  internal static class SMPart
  {
    internal static void FillCrew(Part part)
    {
      //Utilities.LogMessage(string.Format("Entering Fill Crew with part {0}", part.partInfo.name), Utilities.LogType.Info, true);
      if (IsCrewFull(part)) return;
      while (part.CrewCapacity > Utilities.GetPartCrewCount(part))
      {
        ProtoCrewMember kerbal = HighLogic.CurrentGame.CrewRoster.GetNextOrNewKerbal();
        part.AddCrewmember(kerbal);
        //Utilities.LogMessage(string.Format("Filling crew in part {0}", part.partInfo.name), Utilities.LogType.Info, true);
        if (kerbal.seat != null)
          kerbal.seat.SpawnCrew();
      }
    }

    internal static void DumpCrew(Part part)
    {
      if (!part.vessel.IsRecoverable) return;
      while (part.protoModuleCrew.Count > 0)
      {
        ProtoCrewMember kerbal = part.protoModuleCrew.FirstOrDefault();
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
        return part.protoModuleCrew.Count == part.CrewCapacity;
      PartModule deepFreezer = SMConditions.GetFreezerModule(part);
      return deepFreezer != null && new DFWrapper.DeepFreezer(deepFreezer).PartFull;
    }

    internal static void ToggleDumpResource(Part part, List<string> resourceNames, uint pumpId)
    {
      // This is fired by the part dump button on the TransferWindow interface.
      ToggleDumpResource(new List<Part> {part}, resourceNames, pumpId);
    }

    internal static void ToggleDumpResource(List<Part> partList, List<string> resourceNames, uint pumpId)
    {
      // This routine is called by the dump part button on the Transfer Window interface.
      // This routine is also called by the dump docked vessel button on the Transfer window interface.
      List<TransferPump> pumpList =
        resourceNames.Select(
          resource =>
            new TransferPump(resource, TransferPump.TypePump.Dump, TransferPump.TriggerButton.Transfer,
              TransferPump.CalcRemainingResource(partList, resource))
            {
              FromParts = partList,
              PumpId = pumpId,
              PumpRatio = 1
            }).ToList();
      if (!TransferPump.PumpsInProgress(pumpId).Any())
        ProcessController.DumpResources(pumpList);
      else TransferPump.AbortAllPumpsInProcess(pumpId);
    }

    internal static void FillResource(Part part, string resourceName)
    {
      if (part.Resources.Contains(resourceName))
        part.Resources[resourceName].amount = part.Resources[resourceName].maxAmount;
    }

    internal static void FillResource(List<Part> partList, string resourceName)
    {
      IEnumerator<Part> list = partList.Where(part => part.Resources.Contains(resourceName)).GetEnumerator();
      while (list.MoveNext())
      {
        list.Current.Resources[resourceName].amount = list.Current.Resources[resourceName].maxAmount;
      }
    }
  }
}