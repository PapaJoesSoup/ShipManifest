using System;

namespace ShipManifest.Process
{
  public class TransferCrew : ICrewTransfer
  {

    // OnCrewTransferred Event Handling Flags
    internal static DateTime Timestamp;
    internal static bool FireSourceXferEvent;
    internal static bool FireTargetXferEvent;
    internal static GameEvents.HostedFromToAction<ProtoCrewMember, Part> SourceAction;
    internal static GameEvents.HostedFromToAction<ProtoCrewMember, Part> TargetAction;

    // crew xfer inerface properties
    private bool _crewXferActive;
    public bool CrewXferActive
    {
      get
      {
        return _crewXferActive;
      }
      set
      {
        if (!value && _crewXferActive && !IvaDelayActive)
          CrewTransferAbort();
        _crewXferActive = value;
      }
    }

    public bool IsStockXfer { get; set; }

    public bool OverrideStockCrewXfer
    {
      get
      {
        return SMSettings.OverrideStockCrewXfer;
      }
    }
    private double _crewXferDelaysec = SMSettings.CrewXferDelaySec;
    public double CrewXferDelaySec
    {
      get
      {
        return _crewXferDelaysec;
      }
      set
      {
        _crewXferDelaysec = value;
      }
    }

    public bool IsSeat2SeatXfer { get; set; }

    private double _seat2SeatXferDelaySec = 2;
    public double Seat2SeatXferDelaySec
    {
      get
      {
        return _seat2SeatXferDelaySec;
      }
      set
      {
        _seat2SeatXferDelaySec = value;
      }
    }

    internal static XferState CrewXferState = XferState.Off;

    public InternalSeat FromSeat { get; set; }

    public InternalSeat ToSeat { get; set; }

    public Guid XferVesselID
    {
      get
      {
        return SMAddon.SmVessel.Vessel.id;
      }
    }

    // These vars needed to support proper portrait updating.  A small delay is needed to allow for internal KSP crew move callbacks to complete before calling vessel.SpawnCrew
    // Ref:   http://forum.kerbalspaceprogram.com/threads/62270-1-0-2-Ship-Manifest-%28Crew-Science-Resources%29-v-4-1-4-4-10-Apr-15?p=982594&viewfull=1#post982594
    public int IvaPortraitDelay { get; set; }

    public bool IvaDelayActive { get; set; }

    public Part FromPart { get; set; }

    public Part ToPart { get; set; }

    public ProtoCrewMember FromCrewMember { get; set; }

    public ProtoCrewMember ToCrewMember { get; set; }

    public static TransferCrew Instance
    {
      get
      {
        return SMAddon.SmVessel.TransferCrewObj;
      }
    }

    public TransferCrew()
    {
      IvaPortraitDelay = 0;
    }

    public void CrewTransferBegin(ProtoCrewMember crewMember, Part fromPart, Part toPart)
    {
      FromPart = fromPart;
      ToPart = toPart;
      FromCrewMember = crewMember;
      ToCrewMember = null;

      if (FromPart.internalModel != null && ToPart.internalModel != null)
      {
        // Build source and target seat indexes.
        FromSeat = FromCrewMember.seat;
        ToSeat = null;
        if (FromPart == ToPart)
        {
          // Must be a move...
          //Get the first available valid seat
          foreach (var seat in FromPart.internalModel.seats)
          {
            if (seat.taken)
            {
              // This supports DeepFreeze frozen kerbals...
              if (seat.kerbalRef != null && seat.crew != FromCrewMember)
              {
                ToSeat = seat;
                break;
              }
            }
            else
            {
              ToSeat = seat;
              break;
            }
          }
        }
        else
        {
          // Xfer to another part
          // get target seat from target part's inernal model
          foreach (var seat in ToPart.internalModel.seats)
          {
            if (!seat.taken)
            {
              ToSeat = seat;
              break;
            }
          }
          // All seats full?
          if (ToSeat == null)
          {
            foreach (var seat in ToPart.internalModel.seats)
            {
              // This supports DeepFreeze frozen kerbals...
              if (seat.kerbalRef != null && seat.kerbalRef.protoCrewMember.rosterStatus != ProtoCrewMember.RosterStatus.Dead)
              {
                ToSeat = seat;
                break;
              }
            }
          }
        }

        // seats have been chosen.
        // Do we need to swap places with another Kerbal?
        if (ToSeat != null && ToSeat.taken)
        {
          // get Kerbal to swap with through his seat...
          ToCrewMember = ToSeat.kerbalRef.protoCrewMember;
        }
        // if moving within a part, set the seat2seat flag
        IsSeat2SeatXfer = FromPart == ToPart;
      }
      FlightEVA.fetch.DisableInterface();
      CrewXferActive = true;
    }

    internal void CrewTransferProcess()
    {
      try
      {
        if (CrewXferActive)
        {
          if (CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.IVA)
          {
            ScreenMessages.PostScreenMessage("<color=orange>Cannot go IVA.  An SM Crew Xfer is in progress</color>", 4f);
            CameraManager.Instance.SetCameraMode(CameraManager.CameraMode.Flight);
          }

          switch (CrewXferState)
          {
            case XferState.Off:
              // We're just starting loop, so set some evnironment stuff.
              Timestamp = DateTime.Now;
              CrewXferState = XferState.Start;

              if (SMSettings.RealismMode)
              {
                // Default sound license: CC-By-SA
                // http://www.freesound.org/people/adcbicycle/sounds/14214/
                var path1 = SMSettings.CrewSoundStart ?? "ShipManifest/Sounds/14214-1";
                var path2 = SMSettings.CrewSoundRun ?? "ShipManifest/Sounds/14214-2";
                var path3 = SMSettings.CrewSoundStop ?? "ShipManifest/Sounds/14214-3";

                SMAddon.LoadSounds(SMConditions.ResourceType.Crew.ToString(), path1, path2, path3, SMSettings.CrewSoundVol);
              }
              break;

            case XferState.Start:

              SMAddon.Elapsed += (DateTime.Now - Timestamp).TotalSeconds;

              if (SMSettings.RealismMode)
              {
                // Play run sound when start sound is nearly done. (repeats)
                if (SMAddon.Elapsed >= SMAddon.Source1.clip.length - 0.25)
                {
                  Utilities.LogMessage("Source2.play():  started.", "info", SMSettings.VerboseLogging);
                  SMAddon.Source2.Play();
                  SMAddon.Elapsed = 0;
                  CrewXferState = XferState.Transfer;
                }
              }
              else
              {
                CrewXferState = XferState.Transfer;
              }
              break;

            case XferState.Transfer:

              SMAddon.Elapsed += (DateTime.Now - Timestamp).TotalSeconds;

              if (SMSettings.RealismMode)
              {
                // wait for movement to end...
                if (SMAddon.Elapsed >= CrewXferDelaySec || (IsSeat2SeatXfer && SMAddon.Elapsed > Seat2SeatXferDelaySec))
                  CrewXferState = XferState.Stop;
              }
              else
              {
                if (SMAddon.Elapsed > 1)
                  CrewXferState = XferState.Stop;
              }
              break;

            case XferState.Stop:

              // Spawn crew in parts and in vessel.
              if (SMSettings.RealismMode)
              {
                // play crew sit.
                SMAddon.Source2.Stop();
                SMAddon.Source3.Play();
              }
              SMAddon.Elapsed = 0;
              CrewTransferAction();
              CrewXferState = XferState.Portraits;
              IvaDelayActive = true;
              Utilities.LogMessage("CrewTransferProcess:  Updating Portraits...", "info", SMSettings.VerboseLogging);
              break;

            case XferState.Portraits:

              // Account for crew move callbacks by adding a frame delay for portrait updates after crew move...
              if (IvaDelayActive && IvaPortraitDelay < SMSettings.IvaUpdateFrameDelay)
              {
                IvaPortraitDelay += 1;
              }
              else if ((IvaDelayActive && IvaPortraitDelay >= SMSettings.IvaUpdateFrameDelay) || !IvaDelayActive)
              {
                if (IsStockXfer)
                  ScreenMessages.PostScreenMessage(string.Format("<color=yellow>{0} moved (by SM) to {1}.</color>", FromCrewMember.name, ToPart.partInfo.title), 5f);

                ResetXferProcess();
              }
              break;
          }
          Utilities.LogMessage("Transfer State:  " + CrewXferState + "...", "Info", SMSettings.VerboseLogging);
          if (CrewXferState != XferState.Off)
            Timestamp = DateTime.Now;
          else
            Utilities.LogMessage("CrewTransferProcess:  Complete.", "info", SMSettings.VerboseLogging);
        }
      }
      catch (Exception ex)
      {
        if (!SMAddon.FrameErrTripped)
        {
          Utilities.LogMessage("Transfer State:  " + CrewXferState + "...", "Error", true);
          Utilities.LogMessage(string.Format(" in CrewTransferProcess (repeating error).  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
          SMAddon.FrameErrTripped = true;
          ResetXferProcess();
        }
      }
    }

    private void ResetXferProcess()
    {
      IvaDelayActive = false;
      IvaPortraitDelay = 0;
      FromCrewMember = ToCrewMember = null;
      SMAddon.SmVessel.RespawnCrew();
      FlightEVA.fetch.EnableInterface();
      CameraManager.ICameras_ResetAll();
      CrewXferState = XferState.Off;
      CrewXferActive = IsSeat2SeatXfer = IsStockXfer = false;
    }

    internal static void RevertCrewTransfer(ProtoCrewMember fromCrew, Part fromPart, Part toPart)
    {
      // If a Stock crew Transfer occurred, let's revert the crew and activate the SM transfer mechanism...
      toPart.RemoveCrewmember(fromCrew);
      fromPart.AddCrewmember(fromCrew);
      if (fromCrew.seat != null)
        fromCrew.seat.SpawnCrew();

      SMAddon.SmVessel.RespawnCrew();
    }

    public void CrewTransferAction()
    {
      try
      {
        Utilities.LogMessage("CrewTransferAction:  Begin.", "info", SMSettings.VerboseLogging);
        if (FromPart.internalModel != null && ToPart.internalModel != null)
        {
          Utilities.LogMessage("CrewTransferAction:  InternalModel exists.", "info", SMSettings.VerboseLogging);
          if (ToSeat.taken)
          {
            // Swap places.

            // Remove the crew members from the part(s)...
            RemoveCrewMember(FromCrewMember, FromPart);
            RemoveCrewMember(ToCrewMember, ToPart);

            // Add the crew members back into the part(s) at their new seats.
            FromPart.AddCrewmemberAt(ToCrewMember, FromPart.internalModel.seats.IndexOf(FromSeat));
            ToPart.AddCrewmemberAt(FromCrewMember, ToPart.internalModel.seats.IndexOf(ToSeat));
          }
          else
          {
            // Just move.
            RemoveCrewMember(FromCrewMember, FromPart);
            ToPart.AddCrewmemberAt(FromCrewMember, ToPart.internalModel.seats.IndexOf(ToSeat));
          }
        }
        else
        {
          // no portraits, so let's just move kerbals...
          Utilities.LogMessage("CrewTransferAction:  No InternalModel.", "info", SMSettings.VerboseLogging);
          if (ToCrewMember != null)
          {
            RemoveCrewMember(FromCrewMember, FromPart);
            RemoveCrewMember(ToCrewMember, ToPart);
            AddCrewMember(FromCrewMember, ToPart);
            AddCrewMember(ToCrewMember, FromPart);
          }
          else
          {
            RemoveCrewMember(FromCrewMember, FromPart);
            AddCrewMember(FromCrewMember, ToPart);
          }
        }
        if (SMSettings.EnableOnCrewTransferEvent)
        {
          // Now let's deal with third party mod support...
          SourceAction = new GameEvents.HostedFromToAction<ProtoCrewMember, Part>(FromCrewMember, FromPart, ToPart);
          FireSourceXferEvent = true;

          //If a swap, we need to handle that too...
          if (ToCrewMember != null)
          {
            TargetAction = new GameEvents.HostedFromToAction<ProtoCrewMember, Part>(ToCrewMember, ToPart, FromPart);
            FireTargetXferEvent = true;
          }
        }

        FromPart.SpawnCrew();
        ToPart.SpawnCrew();

        // not sure if these help.   We have been experiencing issues with "ghost" kerbals & EVAs/docking/undocking after Crew Moves.   
        // trying this to see if it "cleans up" any internal tracking inside of KSP...
        FromPart.RegisterCrew();
        ToPart.RegisterCrew();

        SMAddon.SmVessel.RespawnCrew();
        SMAddon.SmVessel.TransferCrewObj.IvaDelayActive = true;
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format("in CrewTransferAction.  Error moving crewmember.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
      }
    }

    internal void CrewTransferAbort()
    {
      if (SMSettings.RealismMode)
      {
        SMAddon.Source2.Stop();
        SMAddon.Source3.Play();
      }
      SMAddon.Elapsed = 0;
      SMAddon.SmVessel.TransferCrewObj.IvaDelayActive = false;
      SMAddon.SmVessel.TransferCrewObj.IvaPortraitDelay = 0;
      SMAddon.SmVessel.TransferCrewObj.FromCrewMember = SMAddon.SmVessel.TransferCrewObj.ToCrewMember = null;
      CrewXferState = XferState.Off;
      _crewXferActive = IsSeat2SeatXfer = false;
      FlightEVA.fetch.EnableInterface();
      CameraManager.ICameras_ResetAll();
      IsStockXfer = false;
    }

    internal static void AddCrewMember(ProtoCrewMember pKerbal, Part part)
    {
      part.AddCrewmember(pKerbal);
      if (part.internalModel != null)
      {
        if (pKerbal.seat != null)
          pKerbal.seat.SpawnCrew();
      }
      pKerbal.rosterStatus = ProtoCrewMember.RosterStatus.Assigned;
      SMAddon.FireEventTriggers();
    }

    internal static void RemoveCrewMember(ProtoCrewMember pKerbal, Part part)
    {
      part.RemoveCrewmember(pKerbal);
      pKerbal.rosterStatus = ProtoCrewMember.RosterStatus.Available;
      SMAddon.FireEventTriggers();
    }

    internal enum XferState
    {
      Off,
      Start,
      Transfer,
      Stop,
      Portraits
    }

  }
}
