using System;
using System.Collections.Generic;
using ShipManifest.InternalObjects;

namespace ShipManifest.Process
{
  public class TransferCrew
  {
    public static TransferCrew Instance;

    #region Internal Properties

    // OnCrewTransferred Event Handling Flags
    internal static DateTime Timestamp;
    internal static bool FireSourceXferEvent;
    internal static bool FireTargetXferEvent;
    internal static GameEvents.HostedFromToAction<ProtoCrewMember, Part> SourceAction;
    internal static GameEvents.HostedFromToAction<ProtoCrewMember, Part> TargetAction;

    internal static XferState CrewXferState = XferState.Off;

    // crew xfer inerface properties
    private bool _crewXferActive;
    private double _crewXferDelaysec = SMSettings.CrewXferDelaySec;

    private double _seat2SeatXferDelaySec = 2;

    #endregion

    #region Public Methods

    public TransferCrew()
    {
      IvaPortraitDelay = 0;
      Instance = this;
    }

    // These vars needed to support proper portrait updating.  A small delay is needed to allow for internal KSP crew move callbacks to complete before calling vessel.SpawnCrew
    // Ref:   http://forum.kerbalspaceprogram.com/threads/62270-1-0-2-Ship-Manifest-%28Crew-Science-Resources%29-v-4-1-4-4-10-Apr-15?p=982594&viewfull=1#post982594
    public int IvaPortraitDelay { get; internal set; }

    public bool CrewXferActive
    {
      get { return _crewXferActive; }
      set
      {
        if (value) return;
        if (_crewXferActive && !IvaDelayActive)
          CrewTransferAbort();
        _crewXferActive = false;
      }
    }

    public bool IsStockXfer { get; internal set; }

    public bool OverrideStockCrewXfer
    {
      get { return SMSettings.OverrideStockCrewXfer; }
    }

    public double CrewXferDelaySec
    {
      get { return _crewXferDelaysec; }
      internal set { _crewXferDelaysec = value; }
    }

    public bool IsSeat2SeatXfer { get; internal set; }

    public double Seat2SeatXferDelaySec
    {
      get { return _seat2SeatXferDelaySec; }
      internal set { _seat2SeatXferDelaySec = value; }
    }

    public InternalSeat FromSeat { get; internal set; }

    public InternalSeat ToSeat { get; internal set; }


    public Guid XferVesselId
    {
      get { return SMAddon.SmVessel.Vessel.id; }
    }

    public bool IvaDelayActive { get; internal set; }

    public Part FromPart { get; internal set; }

    public Part ToPart { get; internal set; }

    public ProtoCrewMember FromCrewMember { get; internal set; }

    public ProtoCrewMember ToCrewMember { get; internal set; }

    #endregion

    #region Internal Methods

    internal void CrewTransferBegin(ProtoCrewMember crewMember, Part fromPart, Part toPart)
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
          List<InternalSeat>.Enumerator fromSeats = FromPart.internalModel.seats.GetEnumerator();
          while  (fromSeats.MoveNext())
          {
            InternalSeat seat = fromSeats.Current;
            if (seat == null) continue;
            if (seat.taken)
            {
              // This supports DeepFreeze frozen kerbals...
              if (seat.kerbalRef == null || seat.crew == FromCrewMember) continue;
              ToSeat = seat;
              break;
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
          List<InternalSeat>.Enumerator toSeats = ToPart.internalModel.seats.GetEnumerator();
          while (toSeats.MoveNext())
          {
            InternalSeat seat = toSeats.Current;
            if (seat == null) continue;
            if (seat.taken) continue;
            ToSeat = seat;
            break;
          }
          // All seats full?
          if (ToSeat == null)
          {
            List<InternalSeat>.Enumerator moreSeats = FromPart.internalModel.seats.GetEnumerator();
            while (moreSeats.MoveNext())
            {
              InternalSeat seat = moreSeats.Current;
              if (seat == null) continue;
              // This supports DeepFreeze frozen kerbals...
              if (seat.kerbalRef == null ||
                  seat.kerbalRef.protoCrewMember.rosterStatus == ProtoCrewMember.RosterStatus.Dead) continue;
              ToSeat = seat;
              break;
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
      //FlightEVA.fetch.DisableInterface();
      CrewHatchController.fetch.DisableInterface();
      _crewXferActive = true;
    }

    internal void CrewTransferProcess()
    {
      try
      {
        if (!CrewXferActive) return;
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
            SMSound.SourceCrewStart.Play();
            CrewXferState = XferState.Start;
            break;

          case XferState.Start:

            SMAddon.Elapsed += (DateTime.Now - Timestamp).TotalSeconds;

            if (SMSettings.RealismMode)
            {
              // Play run sound when start sound is nearly done. (repeats)
              if (SMAddon.Elapsed >= SMSound.ClipPumpStart.length - 0.25)
              {
                SMSound.SourceCrewStart.Stop();
                SMSound.SourceCrewRun.Play();
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
              SMSound.SourceCrewRun.Stop();
              SMSound.SourceCrewStop.Play();
            }
            SMAddon.Elapsed = 0;
            CrewTransferAction();
            CrewXferState = XferState.Portraits;
            IvaDelayActive = true;
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
                ScreenMessages.PostScreenMessage(
                  string.Format("<color=yellow>{0} moved (by SM) to {1}.</color>", FromCrewMember.name,
                    ToPart.partInfo.title), 5f);

              ResetXferProcess();
            }
            break;
        }
        if (CrewXferState != XferState.Off) Timestamp = DateTime.Now;
      }
      catch (Exception ex)
      {
        if (!SMAddon.FrameErrTripped)
        {
          Utilities.LogMessage("Transfer State:  " + CrewXferState + "...", Utilities.LogType.Error, true);
          Utilities.LogMessage(
            string.Format(" in CrewTransferProcess (repeating error).  Error:  {0} \r\n\r\n{1}", ex.Message,
              ex.StackTrace), Utilities.LogType.Error, true);
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
      CrewHatchController.fetch.EnableInterface();
      CameraManager.ICameras_ResetAll();
      CrewXferState = XferState.Off;
      _crewXferActive = IsSeat2SeatXfer = IsStockXfer = false;
    }

    internal void CrewTransferAction()
    {
      try
      {
        if (FromPart.internalModel != null && ToPart.internalModel != null)
        {
          if (ToSeat.taken)
          {

            // Remove the crew members from the part(s)...
            RemoveCrewMember(FromCrewMember, FromPart);

            // Swap places if there is no standing room available
            if (ToPart.CrewCapacity == ToPart.protoModuleCrew.Count)
            {
              RemoveCrewMember(ToCrewMember, ToPart);
              FromPart.AddCrewmemberAt(ToCrewMember, FromPart.internalModel.seats.IndexOf(FromSeat));
              // Add the crew members back into the part(s) at their new seats.
              ToPart.AddCrewmemberAt(FromCrewMember, ToPart.internalModel.seats.IndexOf(ToSeat));             
            }
            else
            {
              // Just move.
              RemoveCrewMember(FromCrewMember, FromPart);
              AddCrewMember(FromCrewMember, ToPart);
            }
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
        if (SMSettings.EnableStockCrewXfer)
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

        FromPart.vessel.SpawnCrew();

        // not sure if these help.   We have been experiencing issues with "ghost" kerbals & EVAs/docking/undocking after Crew Moves.   
        // trying this to see if it "cleans up" any internal tracking inside of KSP...
        FromPart.RegisterCrew();
        ToPart.RegisterCrew();

        SMAddon.SmVessel.RespawnCrew();
        SMAddon.SmVessel.TransferCrewObj.IvaDelayActive = true;
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(
          string.Format("in CrewTransferAction.  Error moving crewmember.  Error:  {0} \r\n\r\n{1}", ex.Message,
            ex.StackTrace), Utilities.LogType.Error, true);
      }
    }

    internal void CrewTransferAbort()
    {
      if (SMSettings.RealismMode)
      {
        SMSound.SourceCrewRun.Stop();
        SMSound.SourceCrewStop.Play();
      }
      SMAddon.Elapsed = 0;
      SMAddon.SmVessel.TransferCrewObj.IvaDelayActive = false;
      SMAddon.SmVessel.TransferCrewObj.IvaPortraitDelay = 0;
      SMAddon.SmVessel.TransferCrewObj.FromCrewMember = SMAddon.SmVessel.TransferCrewObj.ToCrewMember = null;
      CrewXferState = XferState.Off;
      _crewXferActive = IsSeat2SeatXfer = false;
      //FlightEVA.fetch.EnableInterface();
      CrewHatchController.fetch.EnableInterface();
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
    
    #endregion

    public enum XferState
    {
      Off,
      Start,
      Transfer,
      Stop,
      Portraits
    }
  }
}