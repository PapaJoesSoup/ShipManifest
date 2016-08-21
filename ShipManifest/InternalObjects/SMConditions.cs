using System;
using System.Collections.Generic;
using System.Linq;
using ConnectedLivingSpace;
using KSP.UI.Dialogs;
using ShipManifest.APIClients;
using ShipManifest.Process;
using ShipManifest.Windows;

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

    internal static bool IsShipControllable()
    {
      return (SMAddon.SmVessel.Vessel.IsControllable && SMSettings.RealismMode) || !SMSettings.RealismMode;
    }

    internal static bool CanResourceBeXferred(TransferPump.TypePump thisXferMode, double maxXferAmount)
    {
      return (!TransferPump.PumpProcessOn && maxXferAmount > 0) ||
             (TransferPump.PumpProcessOn && SMAddon.ActivePumpType == thisXferMode);
    }

    internal static bool CanKerbalsBeXferred(Part sourcePart, Part targetPart)
    {
      List<Part> sourceParts = new List<Part>();
      sourceParts.Add(sourcePart);
      List<Part> targetParts = new List<Part>();
      targetParts.Add(targetPart);

      return CanKerbalsBeXferred(sourceParts, targetParts);
    }
    internal static bool CanKerbalsBeXferred(List<Part> selectedPartsSource, List<Part> selectedPartsTarget)
    {
      bool results = false;
      try
      {
        if (IsTransferInProgress())
        {
          WindowTransfer.XferToolTip = "Transfer in progress.  Xfers disabled.";
          return false;
        }
        if (selectedPartsSource.Count == 0 || selectedPartsTarget.Count == 0)
        {
          WindowTransfer.XferToolTip =
            "Source or Target Part is not selected.\r\nPlease Select a Source AND a Target part.";
          return false;
        }
        if (selectedPartsSource[0] == selectedPartsTarget[0])
        {
          WindowTransfer.XferToolTip = "Source and Target Part are the same.\r\nUse Move Kerbal (>>) instead.";
          return false;
        }
        // If one of the parts is a DeepFreeze part and no crew are showing in protoModuleCrew, check it isn't full of frozen Kerbals. 
        // This is to prevent SM from Transferring crew into a DeepFreeze part that is full of frozen kerbals.
        // If there is just one spare seat or seat taken by a Thawed Kerbal that is OK because SM will just transfer them into the empty
        // seat or swap them with a thawed Kerbal.
        DFWrapper.DeepFreezer sourcepartFrzr = null; // selectedPartsSource[0].FindModuleImplementing<DFWrapper.DeepFreezer>();
        DFWrapper.DeepFreezer targetpartFrzr = null; // selectedPartsTarget[0].FindModuleImplementing<DFWrapper.DeepFreezer>();

        PartModule sourcedeepFreezer = GetFreezerModule(selectedPartsSource[0]);
        if (sourcedeepFreezer != null) sourcepartFrzr = new DFWrapper.DeepFreezer(sourcedeepFreezer);

        PartModule targetdeepFreezer = GetFreezerModule(selectedPartsTarget[0]);
        if (targetdeepFreezer != null) targetpartFrzr = new DFWrapper.DeepFreezer(targetdeepFreezer);

        if (sourcepartFrzr != null)
        {
          if (sourcepartFrzr.FreezerSpace == 0)
          {
            WindowTransfer.XferToolTip =
              "DeepFreeze Part is full of frozen kerbals.\r\nCannot Xfer until some are thawed.";
            return false;
          }
        }
        if (targetpartFrzr != null)
        {
          if (targetpartFrzr.FreezerSpace == 0)
          {
            WindowTransfer.XferToolTip =
              "DeepFreeze Part is full of frozen kerbals.\r\nCannot Xfer until some are thawed.";
            return false;
          }
        }

        // Are there kerbals to move?
        if (selectedPartsSource[0].protoModuleCrew.Count == 0)
        {
          WindowTransfer.XferToolTip = "No Kerbals to Move.";
          return false;
        }
        // now if realism mode, are the parts connected to each other in the same living space?
        results = IsClsInSameSpace();
        if (!results)
          WindowTransfer.EvaToolTip = "CLS is preventing internal Crew Transfer.  Click to initiate EVA operation.";
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format(" in CanBeXferred.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace),
          Utilities.LogType.Error, true);
      }
      if (WindowTransfer.XferToolTip == "")
        WindowTransfer.XferToolTip = "Source and target Part are the same.  Use Move Kerbal instead.";
      return results;
    }

    internal static PartModule GetFreezerModule(Part selectedPartSource)
    {
      return !selectedPartSource.Modules.Contains("DeepFreezer") ? null : selectedPartSource.Modules["DeepFreezer"];
    }

    internal static bool IsClsInSameSpace(Part source, Part target)
    {
      bool results = false;
      if (SMSettings.EnableCls && SMSettings.RealismMode)
      {
        if (SMAddon.ClsAddon.Vessel != null)
        {
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

          if (sourceSpace != null && targetSpace != null && sourceSpace == targetSpace)
          {
            results = true;
          }
        }
      }
      else
      {
        results = true;
      }
      return results;
    }
    internal static bool IsClsInSameSpace()
    {
      bool results = false;
      try
      {
        if (SMSettings.EnableCls && SMSettings.RealismMode)
        {
          if (SMAddon.ClsAddon.Vessel != null)
          {
            if (SMAddon.SmVessel.ClsSpaceSource == null || SMAddon.SmVessel.ClsSpaceTarget == null)
              SMAddon.UpdateClsSpaces();
            if (SMAddon.SmVessel.ClsSpaceSource != null && SMAddon.SmVessel.ClsSpaceTarget != null)
            {
              if (SMAddon.SmVessel.ClsSpaceSource == SMAddon.SmVessel.ClsSpaceTarget)
              {
                WindowTransfer.XferToolTip =
                  "Source & Target Part are in the same space.\r\nInternal Xfers are allowed.";
                results = true;
              }
              else
                WindowTransfer.XferToolTip =
                  "Source and Target parts are not in the same Living Space.\r\nKerbals will have to go EVA.";
            }
            else
              WindowTransfer.XferToolTip =
                "You should NOT be seeing this, as Source or Target Space is missing.\r\nPlease reselect source or target part.";
          }
          else
            WindowTransfer.XferToolTip =
              "You should NOT be seeing this, as CLS is not behaving correctly.\r\nPlease check your CLS installation.";
        }
        else
        {
          WindowTransfer.XferToolTip = "Realism and/or CLS disabled.\r\nXfers anywhere are allowed.";
          results = true;
        }
      }
      catch (Exception ex)
      {
        if (!SMAddon.FrameErrTripped)
        {
          Utilities.LogMessage(
            string.Format(" in IsInCLS (repeating error).  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), Utilities.LogType.Error,
            true);
          SMAddon.FrameErrTripped = true;
        }
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
          string values = "SmAddon.ShowUI = " + SMAddon.ShowUi + "\r\n";
          values += "HighLogic.LoadedScene = " + HighLogic.LoadedScene + "\r\n";
          //values += "!MapView.MapIsEnabled = " +MapView.MapIsEnabled.ToString() + "\r\n";
          values += "PauseMenu.isOpen = " + IsPauseMenuOpen() + "\r\n";
          values += "FlightResultsDialog.isDisplaying = " + IsFlightDialogDisplaying() + "\r\n";
          values += "FlightGlobals.fetch != null = " + (FlightGlobals.fetch != null) + "\r\n";
          values += "FlightGlobals.ActiveVessel != null = " + (FlightGlobals.ActiveVessel != null) + "\r\n";
          values += "!FlightGlobals.ActiveVessel.isEVA = " +
                    (FlightGlobals.ActiveVessel != null && FlightGlobals.ActiveVessel.isEVA) + "\r\n";
          if (FlightGlobals.ActiveVessel != null)
            values += "FlightGlobals.ActiveVessel.vesselType = " + FlightGlobals.ActiveVessel.vesselType + "\r\n";
          values += "CameraManager.Instance.currentCameraMode != CameraManager.CameraMode.IVA = " +
                    (CameraManager.Instance.currentCameraMode != CameraManager.CameraMode.IVA);

          Utilities.LogMessage(
            string.Format(" in CanShowShipManifest (repeating error).  Error:  {0} \r\n\r\n{1}\r\n\r\nValues:  {2}",
              ex.Message, ex.StackTrace, values), Utilities.LogType.Error, true);
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
        return false;
      }
    }

    internal static bool CanResourceBeFilled(string resourceName)
    {
      return (!SMSettings.RealismMode ||
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
      return !SMSettings.RealismMode ||
             (SMAddon.SmVessel.IsRecoverable && SMSettings.EnablePfCrews);
    }
    internal static bool IsUsiInflatable(Part part)
    {
      if (!part.Modules.Contains("USIAnimation")) return false;
      return part.Modules["USIAnimation"].Fields["CrewCapacity"] != null;
    }

    #endregion

    #region Roster Window methods

    internal static bool CanKerbalBeAdded(ProtoCrewMember kerbal)
    {
      return ((SMSettings.RealismMode && SMAddon.SmVessel.IsRecoverable) || !SMSettings.RealismMode) &&
             kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available &&
             SMAddon.SmVessel.SelectedPartsSource.Count > 0 &&
             !SMPart.IsCrewFull(SMAddon.SmVessel.SelectedPartsSource[0]);
    }

    internal static bool FrozenKerbalNotThawable(ProtoCrewMember kerbal)
    {
      return kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Dead &&
             kerbal.type == ProtoCrewMember.KerbalType.Unowned &&
             DFWrapper.DeepFreezeAPI.FrozenKerbals[kerbal.name].vesselID != FlightGlobals.ActiveVessel.id;
    }

    internal static bool FrozenKerbalIsThawable(ProtoCrewMember kerbal)
    {
      return kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Dead &&
             kerbal.type == ProtoCrewMember.KerbalType.Unowned &&
             DFWrapper.DeepFreezeAPI.FrozenKerbals[kerbal.name].vesselID == FlightGlobals.ActiveVessel.id;
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
      if (deepFreezer != null) return new DFWrapper.DeepFreezer(deepFreezer).FreezerSpace > 0;
      return false;
    }

    internal static bool CanKerbalBeRemoved(ProtoCrewMember kerbal)
    {
      return ((SMSettings.RealismMode && SMAddon.SmVessel.IsRecoverable) || !SMSettings.RealismMode) &&
             kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Assigned &&
             FlightGlobals.ActiveVessel.GetVesselCrew().Contains(kerbal);
    }

    internal static bool KerbalCannotBeAddedNoSource(ProtoCrewMember kerbal)
    {
      return ((SMSettings.RealismMode && SMAddon.SmVessel.IsRecoverable) || !SMSettings.RealismMode) &&
             kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available &&
             SMAddon.SmVessel.SelectedPartsSource.Count == 0;
    }

    internal static bool KerbalCannotBeAddedRealism(ProtoCrewMember kerbal)
    {
      return SMSettings.RealismMode && !SMAddon.SmVessel.IsRecoverable &&
             kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available;
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

    #endregion
  }
}