using System;
using System.Collections.Generic;
using ShipManifest.InternalObjects;

namespace ShipManifest.Process
{
  public class TransferCrew
  {
    public static TransferCrew Instance;

    #region Public Properties
    #endregion

    #region Internal Properties

    // OnCrewTransferred Event Handling Flags
    internal static DateTime Timestamp;

    internal static XferState CrewXferState = XferState.Off;

    // crew xfer inerface properties
    private bool _crewXferActive;
    private double _crewXferDelaysec = SMSettings.CrewXferDelaySec;

    private double _seat2SeatXferDelaySec = 2;

    public bool IsSeat2SeatXfer { get; internal set; }
    public InternalSeat FromSeat { get; internal set; }
    public InternalSeat ToSeat { get; internal set; }
    public Part FromPart { get; internal set; }
    public Part ToPart { get; internal set; }

    // Used for single crew transfers.
    public ProtoCrewMember FromCrewMember { get; internal set; }
    public ProtoCrewMember ToCrewMember { get; internal set; }


    // Used for multi crew transfers.
    public class TransferCrewMember {
      internal ProtoCrewMember kerbal;   /**< The crew member being transferred. */
      internal Part partSource;          /**< The part from which the Kerbal is being transferred. */
      internal InternalSeat fromSeat;    /**< The source seat from which the Kerbal is being transferred. */
      internal Part partDestination;     /**< Unused for now */
      internal InternalSeat toSeat;      /**< Unused for now */

      public TransferCrewMember(ProtoCrewMember f_kerbal, Part f_fromPart = null, Part f_toPart = null)
      {
        kerbal = f_kerbal;
        partSource = f_fromPart;
        fromSeat = kerbal.seat;
        partDestination = f_toPart;
        toSeat = null;
      }
    }
    public List<TransferCrewMember> CrewMembersToTransfer { get; internal set; }
    public List<Part> ToParts { get; internal set; }

    // These 2 vars needed to support proper portrait updating.  A small delay is needed to allow for internal KSP crew move callbacks to complete before calling vessel.SpawnCrew
    // Ref:   http://forum.kerbalspaceprogram.com/threads/62270-1-0-2-Ship-Manifest-%28Crew-Science-Resources%29-v-4-1-4-4-10-Apr-15?p=982594&viewfull=1#post982594
    public bool IvaDelayActive { get; internal set; }
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

    public double Seat2SeatXferDelaySec
    {
      get { return _seat2SeatXferDelaySec; }
      internal set { _seat2SeatXferDelaySec = value; }
    }


    public Guid XferVesselId
    {
      get { return SMAddon.SmVessel.Vessel.id; }
    }

    #endregion

    #region Public Methods

    public TransferCrew()
    {
      IvaPortraitDelay = 0;
      Instance = this;
    }

    #endregion

    #region Internal Methods

    internal void CrewTransfersBegin(List<ProtoCrewMember> crewMembers, List<Part> fromParts, List<Part> toParts)
    {
      // Build our list of transfer crew members
      CrewMembersToTransfer = new List<TransferCrewMember>(crewMembers.Count);
      foreach(var currentKerbal in crewMembers)
      {
        TransferCrewMember tcm = new TransferCrewMember(currentKerbal);
        foreach(var currentPart in fromParts)
        {
          if (currentPart.protoModuleCrew.Contains(currentKerbal))
          {
            tcm.partSource = currentPart;
          }
        }
        CrewMembersToTransfer.Add(tcm);
      }
      // We'll assign destinations later
      // TODO: Might be better to do it now, as well as working out seat assignments?
      ToParts = toParts;

      CrewHatchController.fetch.DisableInterface();
      _crewXferActive = true;
    }

    internal void CrewTransferBegin(ProtoCrewMember crewMember, Part fromPart, Part toPart)
    {
      FromPart = fromPart;
      ToPart = toPart;
      FromCrewMember = crewMember;
      ToCrewMember = null;

      if (FromPart.internalModel != null && ToPart.internalModel != null)
      {
        // seats have been chosen.
        AssignSeats();

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

    internal void AssignSeats()
    {
      // Obtain source and target seats.
      FromSeat = FromCrewMember.seat;
      ToSeat = null;
      if (FromPart == ToPart)
      {
        // Must be a seat exchange in the same part...
        // Grab open seat first, then look for a taken seat.
        bool isSeatAvailable = IsSeatAvailable(FromPart);
        List<InternalSeat>.Enumerator fromSeats = FromPart.internalModel.seats.GetEnumerator();
        while (fromSeats.MoveNext())
        {
          if (fromSeats.Current == null) continue;
          if (isSeatAvailable && fromSeats.Current.taken) continue;
          if (!fromSeats.Current.taken)
          {
            ToSeat = fromSeats.Current;
            break;
          }
          // This supports DeepFreeze frozen kerbals...
          if (fromSeats.Current.kerbalRef == null || fromSeats.Current.crew == FromCrewMember) continue;
          ToSeat = fromSeats.Current;
          break;
        }
        fromSeats.Dispose();
      }
      else // Xfer to another part
      {
        // get target seat from target part's inernal model
        bool isSeatAvailable = IsSeatAvailable(ToPart);
        List<InternalSeat>.Enumerator toSeats = ToPart.internalModel.seats.GetEnumerator();
        while (toSeats.MoveNext())
        {
          if (toSeats.Current == null) continue;
          if (isSeatAvailable && toSeats.Current.taken) continue;
          if (!toSeats.Current.taken)
          {
            ToSeat = toSeats.Current;
            break;
          }
          // This supports DeepFreeze frozen kerbals...
          if (toSeats.Current.kerbalRef == null || toSeats.Current.kerbalRef.protoCrewMember.rosterStatus == ProtoCrewMember.RosterStatus.Dead) continue;
          ToSeat = toSeats.Current;
          break;
        }
        toSeats.Dispose();
      }
    }

    internal static bool IsSeatAvailable(Part part)
    {
      return part.internalModel.seats.Count > part.protoModuleCrew.Count;
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
            // We want to run the start sound no matter what the realism settings are 
            // to give an audio indication to the player that the process is active
            Timestamp = DateTime.Now;
            SMSound.SourceCrewStart.Play();
            CrewXferState = XferState.Start;
            break;

          case XferState.Start:

            SMAddon.Elapsed += (DateTime.Now - Timestamp).TotalSeconds;

            if (SMSettings.RealXfers)
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

            if (SMSettings.RealXfers)
            {
              // wait for movement to end...
              if (SMAddon.Elapsed >= CrewXferDelaySec || (IsSeat2SeatXfer && SMAddon.Elapsed > Seat2SeatXferDelaySec))
              {
                CrewXferState = XferState.Stop;
                CrewTransferStartAction();
              }
            }
            else
            {
              if (SMAddon.Elapsed > 1)
              {
                CrewXferState = XferState.Stop;
                CrewTransferStartAction();
              }
            }
            break;

          case XferState.Stop:
            if (SMConditions.ListsUpdating) break;
            // Spawn crew in parts and in vessel.
            if (SMSettings.RealXfers)
            {
              // play crew sit.
              SMSound.SourceCrewRun.Stop();
              SMSound.SourceCrewStop.Play();
            }
            SMAddon.Elapsed = 0;
            CrewTransferStopAction();
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
                  $"<color=yellow>{FromCrewMember.name} moved (by SM) to {ToPart.partInfo.title}.</color>", 5f);

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
          SmUtils.LogMessage($"Transfer State:  {CrewXferState}...", SmUtils.LogType.Error, true);
          SmUtils.LogMessage(
            $" in CrewTransferProcess (repeating error).  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}", SmUtils.LogType.Error, true);
          SMAddon.FrameErrTripped = true;
          ResetXferProcess();
        }
      }
    }

    private void ResetXferProcess()
    {
      IvaDelayActive = false;
      IvaPortraitDelay = 0;
      FromPart = ToPart = null;
      FromSeat = ToSeat = null;
      FromCrewMember = ToCrewMember = null;

      //FromParts = ToParts = null;
      //FromCrewMembers = ToCrewMembers = null;
      CrewMembersToTransfer = null;
      ToParts = null;
      SMAddon.SmVessel.SourceMembersSelected.Clear();
      SMAddon.SmVessel.TargetMembersSelected.Clear();

      SMAddon.SmVessel.RespawnCrew();
      CrewHatchController.fetch.EnableInterface();
      CameraManager.ICameras_ResetAll();
      CrewXferState = XferState.Off;
      _crewXferActive = IsSeat2SeatXfer = IsStockXfer = false;
    }

    internal void CrewTransferStartAction()
    {
      // This removes the kerbal(s) from the current (source) part(s)
      if (FromCrewMember != null)
      {
        try
        {
          if (FromCrewMember != null) RemoveCrewMember(FromCrewMember, FromPart);
          if (ToCrewMember != null) RemoveCrewMember(ToCrewMember, ToPart);
          //SMAddon.FireEventTriggers();
        }
        catch (Exception ex)
        {
          SmUtils.LogMessage(
            $"in CrewTransferStartAction.  Error moving crewmember.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}",
            SmUtils.LogType.Error, true);
        }
      }
      else
      {
        // Must be multi-crew...
        try
        {
          foreach( var xferCrew in CrewMembersToTransfer )
          {
            //RemoveCrewMember(xferCrew.kerbal, xferCrew.partSource);
          }
          //if (FromCrewMembers != null) RemoveCrewMembers(FromCrewMembers, FromParts);
          //SMAddon.FireEventTriggers();
        }
        catch (Exception ex)
        {
          SmUtils.LogMessage(
            $"in CrewTransferStartAction.  Error moving crewmember.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}",
            SmUtils.LogType.Error, true);
        }
      }
    }

    internal void CrewTransferStopAction()
    {
      // This adds the kerbal(s) to the destination part(s)
      if (FromCrewMember != null)
      {
        try
        {
          // Add Source Crewmember to target part
          if (FromCrewMember != null && ToPart.CrewCapacity > ToPart.protoModuleCrew.Count)
          {
            AddCrewMember(FromCrewMember, ToPart, ToSeat);
            var action = new GameEvents.HostedFromToAction<ProtoCrewMember, Part>(FromCrewMember, FromPart, ToPart);
            GameEvents.onCrewTransferred.Fire(action);
          }

          // Add Target Crewmember to source part
          if (ToCrewMember != null && FromPart.CrewCapacity > FromPart.protoModuleCrew.Count)
          {
            AddCrewMember(ToCrewMember, FromPart, FromSeat);
            // MW - Not sure if we need this guard
            if(SMSettings.EnableStockCrewXfer)
            {
              var action = new GameEvents.HostedFromToAction<ProtoCrewMember, Part>(ToCrewMember, ToPart, FromPart);
              GameEvents.onCrewTransferred.Fire(action);
            }
          }
        }
        catch (Exception ex)
        {
          SmUtils.LogMessage(
            $"in CrewTransferAction.  Error moving crewmember.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}",
            SmUtils.LogType.Error, true);
        }
      }
      else
      {
        // Must be multi-crew...
        try
        {
          // Step through the destination parts, transferring kerbals into them
          // if they have room.  Any kerbals left over have to go back to whence
          // they came.  Perhaps in future they can evict kerbals already in
          // residence..
          int crewIdx = 0;
          foreach (var toPart in ToParts )
          {
            // Add Source Crewmember(s) to target part
            int space = toPart.CrewCapacity - toPart.protoModuleCrew.Count;
            for (int idx = 0; idx < space; idx++)
            {
              if (crewIdx > CrewMembersToTransfer.Count - 1) break;
              RemoveCrewMember(CrewMembersToTransfer[crewIdx].kerbal, CrewMembersToTransfer[crewIdx].partSource);
              AddCrewMember(CrewMembersToTransfer[crewIdx].kerbal, toPart);
              // MW - Not sure if we need this guard
              if(SMSettings.EnableStockCrewXfer)
              {
                var action = new GameEvents.HostedFromToAction<ProtoCrewMember, Part>(CrewMembersToTransfer[crewIdx].kerbal, CrewMembersToTransfer[crewIdx].partSource, toPart);
                GameEvents.onCrewTransferred.Fire(action);
              }
              crewIdx++;
            }
          }
        }
        catch (Exception ex)
        {
          SmUtils.LogMessage(
            $"in CrewTransferAction.  Error moving crewmember.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}",
            SmUtils.LogType.Error, true);
        }
      }
    }

    internal void CrewTransferAbort()
    {
      if (SMSettings.RealXfers)
      {
        SMSound.SourceCrewRun.Stop();
        SMSound.SourceCrewStop.Play();
        if (FromCrewMember != null) AddCrewMember(FromCrewMember, FromPart, FromSeat);
        if (ToCrewMember != null) AddCrewMember(ToCrewMember, ToPart, ToSeat);
      }
      SMAddon.Elapsed = 0;
      SMAddon.SmVessel.TransferCrewObj.IvaDelayActive = false;
      SMAddon.SmVessel.TransferCrewObj.IvaPortraitDelay = 0;
      SMAddon.SmVessel.TransferCrewObj.FromCrewMember = SMAddon.SmVessel.TransferCrewObj.ToCrewMember = null;
      CrewXferState = XferState.Off;
      _crewXferActive = IsSeat2SeatXfer = false;
      CrewHatchController.fetch.EnableInterface();
      CameraManager.ICameras_ResetAll();
      IsStockXfer = false;
    }

    /// <summary>
    /// Adds a crew member to a specific Part, into a specific Seat, if provided.
    /// </summary>
    /// <param name="pKerbal"></param>
    /// <param name="part"></param>
    /// <param name="seat"></param>
    internal static void AddCrewMember(ProtoCrewMember pKerbal, Part part, InternalSeat seat = null)
    {
      if (seat != null)
        part.AddCrewmemberAt(pKerbal, part.internalModel.seats.IndexOf(seat));
      else
        part.AddCrewmember(pKerbal);
    }

    internal static void RemoveCrewMember(ProtoCrewMember pKerbal, Part part)
    {
      part.RemoveCrewmember(pKerbal);
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
