using System;
using System.Collections.Generic;
using ConnectedLivingSpace;
using KSP.UI.Dialogs;
using ShipManifest.APIClients;
using ShipManifest.Modules;
using ShipManifest.Process;
using ShipManifest.Windows;
using UniLinq;
using UnityEngine;

namespace ShipManifest.InternalObjects
{
  /// <summary>
  ///   This class contains nothing but conditional logic methods and enums
  ///   This will likely get refactored out as a better understanding of the various conditions is realized with this
  ///   refactor.
  ///   All are bools, and many are being refactored into enums.  Therefore, enums and their supporting methods reside here
  ///   as well.
  /// </summary>
  // ReSharper disable once InconsistentNaming
  internal static class SMConditions
  {
    #region Condition Methods

    internal static bool ListsUpdating;

    internal static bool IsShipControllable()
    {
      return (SMAddon.SmVessel.Vessel.IsControllable && SMSettings.RealControl) || !SMSettings.RealControl;
    }

    internal static bool IsInPreflight()
    {
      return SMAddon.SmVessel.IsRecoverable && SMAddon.SmVessel.Vessel.Landed;
    }

    internal static bool CanResourceBeXferred(TransferPump.TypePump thisXferMode, double maxXferAmount)
    {
      return (!TransferPump.PumpProcessOn && maxXferAmount > 0) ||
             (TransferPump.PumpProcessOn && SMAddon.ActivePumpType == thisXferMode);
    }

    internal static bool CanKerbalsBeXferred(Part sourcePart, Part targetPart)
    {
      List<Part> sourceParts = new List<Part> {sourcePart};
      List<Part> targetParts = new List<Part> {targetPart};

      return CanKerbalsBeXferred(sourceParts, targetParts);
    }

    internal static bool CanKerbalsBeXferred(List<Part> selectedPartsSource, List<Part> selectedPartsTarget)
    {
      bool results = true;
      WindowTransfer.XferToolTip = "";
      try
      {
        if (IsTransferInProgress())
        {
          //WindowTransfer.XferToolTip = "Transfer in progress.  Xfers disabled.";
          WindowTransfer.XferToolTip = SmUtils.Localize("#smloc_conditions_tt_001");
          return false;
        }
        if (selectedPartsSource.Count == 0 || selectedPartsTarget.Count == 0)
        {
          WindowTransfer.XferToolTip = SmUtils.Localize("#smloc_conditions_tt_002");
          //  "Source or Target Part is not selected.\r\nPlease Select a Source AND a Target part.";
          return false;
        }
        if (selectedPartsSource.Count == 1 && selectedPartsTarget.Count == 1)
        {
          if (selectedPartsSource[0] == selectedPartsTarget[0])
          {
            WindowTransfer.XferToolTip = SmUtils.Localize("#smloc_conditions_tt_003");
            // "Source and Target Parts are the same.\r\nUse Move Kerbal (>>) instead.";
            return false;
          }
        }
        else if (selectedPartsSource.Count > 1 || selectedPartsTarget.Count > 1)
        {
          if (selectedPartsSource == selectedPartsTarget)
          {
            WindowTransfer.XferToolTip = SmUtils.Localize("#smloc_conditions_tt_003");
            // "Source and Target Parts are the same.\r\nUse Move individual Kerbals (>>) instead.";
            return false;
          }
        }
        // If one of the parts is a DeepFreeze part and no crew are showing in protoModuleCrew, check it isn't full of frozen Kerbals. 
        // This is to prevent SM from Transferring crew into a DeepFreeze part that is full of frozen kerbals.
        // If there is just one spare seat or seat taken by a Thawed Kerbal that is OK because SM will just transfer them into the empty
        // seat or swap them with a thawed Kerbal.
        DfWrapper.DeepFreezer sourcepartFrzr = null; // selectedPartsSource[0].FindModuleImplementing<DFWrapper.DeepFreezer>();
        DfWrapper.DeepFreezer targetpartFrzr = null; // selectedPartsTarget[0].FindModuleImplementing<DFWrapper.DeepFreezer>();

        List<Part>.Enumerator srcPart = selectedPartsSource.GetEnumerator();
        while (srcPart.MoveNext())
        {
          if (srcPart.Current == null) continue;
          PartModule sourcedeepFreezer = GetFreezerModule(srcPart.Current);
          if (sourcedeepFreezer != null) sourcepartFrzr = new DfWrapper.DeepFreezer(sourcedeepFreezer);
          if (sourcepartFrzr?.FreezerSpace != 0) continue;
          WindowTransfer.XferToolTip = SmUtils.Localize("#smloc_conditions_tt_004");
          // "DeepFreeze Part is full of frozen kerbals.\r\nCannot Xfer until some are thawed.";
          return false;
        }

        List<Part>.Enumerator tgtPart = selectedPartsSource.GetEnumerator();
        while (tgtPart.MoveNext())
        {
          if (tgtPart.Current == null) continue;
          PartModule targetdeepFreezer = GetFreezerModule(tgtPart.Current);
          if (targetdeepFreezer != null) targetpartFrzr = new DfWrapper.DeepFreezer(targetdeepFreezer);
          if (targetpartFrzr?.FreezerSpace != 0) continue;
          WindowTransfer.XferToolTip = SmUtils.Localize("#smloc_conditions_tt_004");
          // "DeepFreeze Part is full of frozen kerbals.\r\nCannot Xfer until some are thawed.";
          return false;
        }

        // Are there kerbals to move?
        if (SmUtils.GetCrewCount(selectedPartsSource) == 0)
        {
          //WindowTransfer.XferToolTip = "No Kerbals to Move.";
          WindowTransfer.XferToolTip = SmUtils.Localize("#smloc_conditions_tt_005");
          return false;
        }
        // now if realistic xfers is enabled, are the parts connected to each other in the same living space?
        results = IsClsInSameSpace(selectedPartsSource, selectedPartsTarget);
        if (!results)
          WindowTransfer.EvaToolTip = SmUtils.Localize("#smloc_conditions_tt_006");
        // "CLS is preventing internal Crew Transfer.  Click to initiate EVA operation.";
        else
          WindowTransfer.XferToolTip = SmUtils.Localize("#smloc_conditions_tt_007");  
          // "Kerbal can be Transfered.";
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage($" in CanBeXferred.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}",
          SmUtils.LogType.Error, true);
      }
      return results;
    }

    internal static PartModule GetFreezerModule(Part selectedPartSource)
    {
      return !selectedPartSource.Modules.Contains("DeepFreezer") ? null : selectedPartSource.Modules["DeepFreezer"];
    }

    internal static bool IsClsInSameSpace(List<Part> source, List<Part> target)
    {
      if (!SMSettings.EnableCls ||  !SMSettings.RealXfers) return true;

      bool result = false;
      List<Part>.Enumerator srcPart = source.GetEnumerator();
      while (srcPart.MoveNext())
      {
        if (srcPart.Current == null) continue;
        List<Part>.Enumerator destPart = target.GetEnumerator();
        while (destPart.MoveNext())
        {
          if (destPart.Current == null) continue;
          result = IsClsInSameSpace(srcPart.Current, destPart.Current);
        }
      }
      return result;
    }

    internal static bool IsClsInSameSpace(Part source, Part target)
    {
      bool results = false;
      if (source == null || target == null) return results;
      if (SMSettings.EnableCls && SMSettings.RealXfers)
      {
        if (SMAddon.ClsAddon.Vessel == null) return results;
        ICLSSpace sourceSpace = null;
        ICLSSpace targetSpace = null;
        List<ICLSPart>.Enumerator parts = SMAddon.ClsAddon.Vessel.Parts.GetEnumerator();
        while (parts.MoveNext())
        {
          ICLSPart ipart = parts.Current;
          if (ipart == null) continue;
          if (ipart.Part == source) sourceSpace = ipart.Space;
          if (ipart.Part == target) targetSpace = ipart.Space;
          if (sourceSpace != null && targetSpace != null) break;
        }
        parts.Dispose();
        if (sourceSpace != null && targetSpace != null && sourceSpace == targetSpace)
        {
          results = true;
        }
      }
      else
      {
        results = true;
      }
      return results;
    }

    internal static bool CanShowShipManifest(bool ignoreShowSm = false)
    {
      try
      {
        bool canShow = false;
        if (SMAddon.ShowUi
            && HighLogic.LoadedScene == GameScenes.FLIGHT
            && !IsPauseMenuOpen()
            && !IsFlightDialogDisplaying()
            && FlightGlobals.fetch != null
            && FlightGlobals.ActiveVessel != null
            && !FlightGlobals.ActiveVessel.isEVA
            && FlightGlobals.ActiveVessel.vesselType != VesselType.Flag
            //&& FlightGlobals.ActiveVessel.vesselType != VesselType.Debris
            //&& FlightGlobals.ActiveVessel.vesselType != VesselType.Unknown
            //&& CameraManager.Instance.currentCameraMode != CameraManager.CameraMode.IVA
          )
          canShow = ignoreShowSm || WindowManifest.ShowWindow;
        return canShow;
      }
      catch (Exception ex)
      {
        if (!SMAddon.FrameErrTripped)
        {
          string values = $"SmAddon.ShowUI = {SMAddon.ShowUi}\r\n";
          values += $"HighLogic.LoadedScene = {HighLogic.LoadedScene}\r\n";
          values += $"PauseMenu.isOpen = {IsPauseMenuOpen()}\r\n";
          values += $"FlightResultsDialog.isDisplaying = {IsFlightDialogDisplaying()}\r\n";
          values += $"FlightGlobals.fetch != null = {(FlightGlobals.fetch != null)}\r\n";
          values += $"FlightGlobals.ActiveVessel != null = {(FlightGlobals.ActiveVessel != null)}\r\n";
          values += $"!FlightGlobals.ActiveVessel.isEVA = {(FlightGlobals.ActiveVessel != null && FlightGlobals.ActiveVessel.isEVA)}\r\n";
          if (FlightGlobals.ActiveVessel != null)
            values += $"FlightGlobals.ActiveVessel.vesselType = {FlightGlobals.ActiveVessel.vesselType}\r\n";
          values += $"CameraManager.Instance.currentCameraMode != CameraManager.CameraMode.IVA = {(CameraManager.Instance.currentCameraMode != CameraManager.CameraMode.IVA)}";

          SmUtils.LogMessage(
            $" in CanShowShipManifest (repeating error).  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}\r\n\r\nValues:  {values}", SmUtils.LogType.Error, true);
          SMAddon.FrameErrTripped = true;
        }
        return false;
      }
    }

    internal static bool IsFlightDialogDisplaying()
    {
      try
      {
        return FlightResultsDialog.isDisplaying;
      }
      catch
      {
        return false;
      }
    }

    internal static bool IsPauseMenuOpen()
    {
      try
      {
        return PauseMenu.isOpen;
      }
      catch
      {
        try
        {
          return KSCPauseMenu.Instance.isActiveAndEnabled;
        }
        catch
        {
          return false;
        }
      }
    }

    internal static bool CanResourceBeFilled(string resourceName)
    {
      return (!SMSettings.RealXfers ||
              SMAddon.SmVessel.IsRecoverable && AreSelectedResourcesTypeOther(new List<string> {resourceName}))
             &&
             TransferPump.CalcRemainingCapacity(SMAddon.SmVessel.PartsByResource[resourceName], resourceName) >
             SMSettings.Tolerance;
    }

    internal static bool CanResourceBeDumped(string resourceName)
    {
      return TransferPump.CalcRemainingResource(SMAddon.SmVessel.PartsByResource[resourceName], resourceName) >
             SMSettings.Tolerance &&
             IsResourceTypeOther(resourceName);
    }

    internal static bool IsTransferInProgress()
    {
      return SMAddon.SmVessel.TransferCrewObj.CrewXferActive || TransferPump.PumpProcessOn;
    }

    internal static bool ResourceIsSingleton(string resourceName)
    {
      return resourceName == ResourceType.Crew.ToString() || resourceName == ResourceType.Science.ToString() ||
             resourceName == "ElectricCharge";
    }

    internal static bool ResourcesContainSingleton(List<string> resourceNames)
    {
      return resourceNames.Contains(ResourceType.Crew.ToString()) ||
             resourceNames.Contains(ResourceType.Science.ToString()) ||
             SMAddon.SmVessel.SelectedResources.Contains("ElectricCharge");
    }

    internal static bool IsResourceTypeOther(string resourceName)
    {
      return !resourceName.Contains(ResourceType.Crew.ToString()) &&
             !resourceName.Contains(ResourceType.Science.ToString());
    }

    internal static bool IsSelectedResourceTypeOther(string resourceName)
    {
      return AreSelectedResourcesTypeOther(new List<string> {resourceName});
    }

    internal static bool AreSelectedResourcesTypeOther(List<string> resourceNames)
    {
      return !resourceNames.Contains(ResourceType.Crew.ToString()) &&
             !resourceNames.Contains(ResourceType.Science.ToString());
    }

    internal static bool IsClsEnabled()
    {
      return SMSettings.EnableCls && SMAddon.ClsAddon.Vessel != null;
    }

    internal static bool IsClsActive()
    {
      return SMSettings.EnableCls && SMAddon.ClsAddon.Vessel != null &&
             SMAddon.SmVessel.SelectedResources.Contains(ResourceType.Crew.ToString());
    }

    internal static bool IsClsHighlightingEnabled()
    {
      return SMSettings.EnableCls && SMSettings.EnableClsHighlighting && SMAddon.ClsAddon.Vessel != null &&
             SMAddon.SmVessel.SelectedResources.Contains(ResourceType.Crew.ToString());
    }

    internal static bool CanKerbalBeReSpawned(ProtoCrewMember kerbal)
    {
      return (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Dead &&
              kerbal.type != ProtoCrewMember.KerbalType.Unowned) ||
             kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Missing;
    }

    internal static bool CanShowCrewFillDumpButtons()
    {
      return !SMSettings.RealXfers ||
             (SMAddon.SmVessel.IsRecoverable && SMSettings.EnablePfCrews);
    }

    internal static bool IsUsiInflatable(Part part)
    {
      if (!part.Modules.Contains("USIAnimation")) return false;
      return part.Modules["USIAnimation"].Fields["CrewCapacity"] != null;
    }

    internal static bool IsVesselDocked(ModDockedVessel vessel)
    {
      bool result = false;
      List<Part>.Enumerator parts = vessel.VesselParts.GetEnumerator();
      while (parts.MoveNext())
      {
        if (parts.Current == null ) continue;
        List<ModuleDockingNode>.Enumerator nodes = parts.Current.FindModulesImplementing<ModuleDockingNode>().GetEnumerator();
        while (nodes.MoveNext())
        {
          if (nodes.Current == null) continue;
          if (nodes.Current.otherNode == null) continue;
          result = true;
          break;
        }
        nodes.Dispose();
        if (result) break;
      }
      parts.Dispose();
      return result;
    }
    #endregion

    #region Roster Window methods

    internal static bool CanKerbalBeAdded(ProtoCrewMember kerbal)
    {
      return SMSettings.EnableCrewModify 
        && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available 
        && SMAddon.SmVessel.SelectedPartsSource.Count > 0 
        && !ShipManifest.SMPart.IsCrewFull(SMAddon.SmVessel.SelectedPartsSource[0]);
    }

    internal static bool FrozenKerbalNotThawable(ProtoCrewMember kerbal)
    {
      return kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Dead &&
             kerbal.type == ProtoCrewMember.KerbalType.Unowned &&
             DfWrapper.DeepFreezeApi.FrozenKerbals[kerbal.name].VesselId != FlightGlobals.ActiveVessel.id;
    }

    internal static bool FrozenKerbalIsThawable(ProtoCrewMember kerbal)
    {
      return kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Dead &&
             kerbal.type == ProtoCrewMember.KerbalType.Unowned &&
             DfWrapper.DeepFreezeApi.FrozenKerbals[kerbal.name].VesselId == FlightGlobals.ActiveVessel.id;
    }

    internal static bool CanKerbalBeFrozen(ProtoCrewMember kerbal)
    {
      return kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Assigned &&
             FlightGlobals.ActiveVessel.GetVesselCrew().Contains(kerbal) && IsKerbalReadyToFreeze(kerbal);
    }

    internal static bool IsKerbalReadyToFreeze(ProtoCrewMember kerbal)
    {
      //return kerbal.seat.part.Modules.Contains("DeepFreezer") && !SMPart.IsCrewFull(kerbal.seat.part);
      PartModule deepFreezer = GetFreezerModule(kerbal.seat.part);
      if (deepFreezer != null) return new DfWrapper.DeepFreezer(deepFreezer).FreezerSpace > 0;
      return false;
    }

    internal static bool CanKerbalBeRemoved(ProtoCrewMember kerbal)
    {
      return SMSettings.EnableCrewModify 
        && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Assigned 
        && FlightGlobals.ActiveVessel.GetVesselCrew().Contains(kerbal);
    }

    internal static bool KerbalCannotBeRemovedRealism(ProtoCrewMember kerbal)
    {
      return !SMSettings.EnableCrewModify
        && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Assigned
        && FlightGlobals.ActiveVessel.GetVesselCrew().Contains(kerbal);
    }

    internal static bool KerbalCannotBeAddedNoSource(ProtoCrewMember kerbal)
    {
      return SMSettings.EnableCrewModify 
        && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available 
        && SMAddon.SmVessel.SelectedPartsSource.Count == 0;
    }

    internal static bool KerbalCannotBeAddedRealism(ProtoCrewMember kerbal)
    {
      return !SMSettings.EnableCrewModify 
        && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available;
    }

    internal static TransferMode GetTransferMode()
    {
      if (SMAddon.SmVessel.SelectedResources.Contains(TransferMode.Crew.ToString())) return TransferMode.Crew;
      return SMAddon.SmVessel.SelectedResources.Contains(TransferMode.Crew.ToString()) ? TransferMode.Science : TransferMode.Resources;
    }

    #endregion

    #region Enums

    internal static ResourceType TypeOfResource(string resourceName)
    {
      if (resourceName == ResourceType.Crew.ToString()) return ResourceType.Crew;
      if (resourceName == ResourceType.Science.ToString()) return ResourceType.Science;
      return ResourceType.Pump;
    }

    internal enum ResourceType
    {
      Crew,
      Science,
      Pump
    }

    internal enum Window
    {
      WindowControl,
      WindowDebugger,
      WindowManifest,
      WindowSettings,
      WindowTransfer
    }

    public enum TransferMode
    {
      Crew,
      Science,
      Resources
    }

    #endregion
  }
}