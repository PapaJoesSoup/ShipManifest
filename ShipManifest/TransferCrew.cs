using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine;
using HighlightingSystem;
using ConnectedLivingSpace;

namespace ShipManifest
{
    public class TransferCrew : ICrewTransfer
    {

        // OnCrewTransferred Event Handling Flags
        internal static bool IgnoreFromToXferEvent = false;
        internal static bool IgnoreToFromXferEvent = false;

        // crew xfer inerface properties
        private bool _crewXferActive = false;
        public bool CrewXferActive
        {
            get
            {
                return _crewXferActive;
            }
            set
            {
                if (!value && _crewXferActive && !_ivaDelayActive)
                    CrewTransferAbort();
                _crewXferActive = value;
            }
        }
        private bool _isStockXfer = false;
        public bool IsStockXfer
        {
            get
            {
                return _isStockXfer;
            }
            set
            {
                _isStockXfer = value;
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
        private bool _isSeat2SeatXfer = false;
        public bool IsSeat2SeatXfer
        {
            get
            {
                return _isSeat2SeatXfer;
            }
            set
            {
                _isSeat2SeatXfer = value;
            }
        }
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

        internal static CrewXFERState CrewXferState = CrewXFERState.Off;

        public InternalSeat _fromSeat = null;
        public InternalSeat FromSeat
        {
            get
            {
                return _fromSeat;
            }
            set
            {
                _fromSeat = value;
            }
        }
        public InternalSeat _toSeat = null;
        public InternalSeat ToSeat
        {
            get
            {
                return _toSeat;
            }
            set
            {
                _toSeat = value;
            }
        }

        public Guid XferVesselID
        {
            get
            {
                return SMAddon.smController.Vessel.id;
            }
        }

        // These vars needed to support proper portrait updating.  A small delay is needed to allow for internal KSP crew move callbacks to complete before calling vessel.SpawnCrew
        // Ref:   http://forum.kerbalspaceprogram.com/threads/62270-1-0-2-Ship-Manifest-%28Crew-Science-Resources%29-v-4-1-4-4-10-Apr-15?p=982594&viewfull=1#post982594
        private int _ivaPortraitDelay = 0;
        public int IvaPortraitDelay
        {
            get
            {
                return _ivaPortraitDelay;
            }
            set
            {
                _ivaPortraitDelay = value;
            }
        }
        private bool _ivaDelayActive = false;
        public bool IvaDelayActive
        {
            get
            {
                return _ivaDelayActive;
            }
            set
            {
                _ivaDelayActive = value;
            }
        }

        private Part _fromPart;
        public Part FromPart
        {
            get
            {
                return _fromPart;
            }
            set
            {
                _fromPart = value;
            }
        }
        private Part _toPart;
        public Part ToPart
        {
            get
            {
                return _toPart;
            }
            set
            {
                _toPart = value;
            }
        }
        private ProtoCrewMember _fromCrewMember;
        public ProtoCrewMember FromCrewMember
        {
            get
            {
                return _fromCrewMember;
            }
            set
            {
                _fromCrewMember = value;
            }
        }
        private ProtoCrewMember _toCrewMember;
        public ProtoCrewMember ToCrewMember
        {
            get
            {
                return _toCrewMember;
            }
            set
            {
                _toCrewMember = value;
            }
        }
        
        public static TransferCrew Instance
        {
            get
            {
                return SMAddon.smController.CrewTransfer;
            }
        }

        public TransferCrew() 
        {

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
                    foreach (InternalSeat seat in FromPart.internalModel.seats)
                    {
                        if (seat.taken)
                        {
                            // This supports DeepFreeze frozen kerbals...
                            if (seat.kerbalRef!= null && seat.crew != FromCrewMember)
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
                    foreach (InternalSeat seat in ToPart.internalModel.seats)
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
                        foreach (InternalSeat seat in ToPart.internalModel.seats)
                        {
                            // This supports DeepFreeze frozen kerbals...
                            if (seat.kerbalRef != null)
                            {
                                ToSeat = seat;
                                break;
                            }
                        }
                    }
                }

                // seats have been chosen.
                // Do we need to swap places with another Kerbal?
                if (ToSeat.taken)
                {
                    // get Kerbal to swap with through his seat...
                    ToCrewMember = ToSeat.kerbalRef.protoCrewMember;
                }
                // if moving within a part, set the seat2seat flag
                IsSeat2SeatXfer = FromPart == ToPart ? true : false;
            }
            CrewXferActive = true;
        }

        internal void CrewTransferProcess()
        {
            try
            {
                if (SMAddon.smController.CrewTransfer.CrewXferActive)
                {
                    Part PartFrom = SMAddon.smController.CrewTransfer.FromPart;
                    Part PartTo = SMAddon.smController.CrewTransfer.ToPart;
                    ProtoCrewMember pKerbal = SMAddon.smController.CrewTransfer.FromCrewMember;
                    if (!SMSettings.RealismMode)
                    {
                        switch (CrewXferState)
                        {
                            case CrewXFERState.Off:
                                // We're just starting loop, so set some evnironment stuff.
                                SMAddon.timestamp = 0;
                                CrewXferState = CrewXFERState.Start;
                                break;

                            case CrewXFERState.Start:
                                SMAddon.elapsed += Planetarium.GetUniversalTime() - SMAddon.timestamp;
                                CrewXferState = CrewXFERState.Transfer;
                                break;

                            case CrewXFERState.Transfer:

                                SMAddon.elapsed += Planetarium.GetUniversalTime() - SMAddon.timestamp;
                                if (SMAddon.elapsed > 1)
                                    CrewXferState = CrewXFERState.Stop;
                                break;

                            case CrewXFERState.Stop:

                                // Spawn crew in parts and in vessel.
                                if (PartFrom == null)
                                    PartFrom = PartTo;
                                if (PartTo == null)
                                    PartTo = PartFrom;
                                SMAddon.elapsed = SMAddon.timestamp = 0;
                                CrewTransferComplete();
                                CrewXferState = CrewXFERState.Portraits;

                                break;

                            case CrewXFERState.Portraits:

                                // Account for crew move callbacks by adding a frame delay for portrait updates after crew move...
                                if (SMAddon.smController.CrewTransfer.IvaDelayActive && SMAddon.smController.CrewTransfer.IvaPortraitDelay < SMSettings.IvaUpdateFrameDelay)
                                {
                                    SMAddon.smController.CrewTransfer.IvaPortraitDelay += 1;
                                }
                                else if (SMAddon.smController.CrewTransfer.IvaDelayActive && SMAddon.smController.CrewTransfer.IvaPortraitDelay >= SMSettings.IvaUpdateFrameDelay)
                                {
                                    SMAddon.smController.CrewTransfer.IvaDelayActive = false;
                                    IgnoreFromToXferEvent = IgnoreToFromXferEvent = false;
                                    SMAddon.smController.CrewTransfer.IvaPortraitDelay = 0;
                                    SMAddon.smController.CrewTransfer.FromCrewMember = SMAddon.smController.CrewTransfer.ToCrewMember = null;
                                    SMAddon.smController.RespawnCrew();

                                    CrewXferState = CrewXFERState.Off;
                                    CrewXferActive = IsSeat2SeatXfer = false;
                                    if (IsStockXfer)
                                    {
                                        var message = new ScreenMessage(string.Empty, 15f, ScreenMessageStyle.LOWER_CENTER);
                                        ScreenMessages.PostScreenMessage(string.Format("<color=yellow>{0} moved (by SM) to {1}.</color>", FromCrewMember.name, PartTo.partInfo.title), message, true);
                                    }
                                    IsStockXfer = false;
                                }
                                break;
                        }
                        Utilities.LogMessage("CrewTransferProcess crewXfer State:  " + CrewXferActive.ToString() + "...", "Info", SMSettings.VerboseLogging);
                        if (CrewXferActive)
                            SMAddon.timestamp = Planetarium.GetUniversalTime();
                    }
                    else
                    {
                        switch (CrewXferState)
                        {
                            case CrewXFERState.Off:
                                // We're just starting loop, so set some evnironment stuff.
                                SMAddon.timestamp = 0;

                                // Default sound license: CC-By-SA
                                // http://www.freesound.org/people/adcbicycle/sounds/14214/
                                string path1 = SMSettings.CrewSoundStart != null ? SMSettings.CrewSoundStart : "ShipManifest/Sounds/14214-1";
                                string path2 = SMSettings.CrewSoundRun != null ? SMSettings.CrewSoundRun : "ShipManifest/Sounds/14214-2";
                                string path3 = SMSettings.CrewSoundStop != null ? SMSettings.CrewSoundStop : "ShipManifest/Sounds/14214-3";

                                SMAddon.LoadSounds("Crew", path1, path2, path3, SMSettings.CrewSoundVol);
                                CrewXferState = CrewXFERState.Start;
                                break;

                            case CrewXFERState.Start:

                                SMAddon.elapsed += Planetarium.GetUniversalTime() - SMAddon.timestamp;

                                // Play run sound when start sound is nearly done. (repeats)
                                if (SMAddon.elapsed >= SMAddon.source1.clip.length - 0.25)
                                {
                                    Utilities.LogMessage("source2.play():  started.", "info", SMSettings.VerboseLogging);
                                    SMAddon.source2.Play();
                                    SMAddon.elapsed = 0;
                                    CrewXferState = CrewXFERState.Transfer;
                                }
                                break;

                            case CrewXFERState.Transfer:

                                SMAddon.elapsed += Planetarium.GetUniversalTime() - SMAddon.timestamp;

                                // wait for movement to end...
                                if (SMAddon.elapsed >= CrewXferDelaySec || (IsSeat2SeatXfer && SMAddon.elapsed > Seat2SeatXferDelaySec))
                                {
                                    // Reset State vars
                                    CrewXferState = CrewXFERState.Stop;
                                }
                                break;

                            case CrewXFERState.Stop:

                                // Spawn crew in parts and in vessel.
                                if (PartFrom == null)
                                    PartFrom = PartTo;
                                if (PartTo == null)
                                    PartTo = PartFrom;

                                // play crew sit.
                                SMAddon.source2.Stop();
                                SMAddon.source3.Play();
                                SMAddon.timestamp = SMAddon.elapsed = 0;
                                CrewTransferComplete();
                                CrewXferState = CrewXFERState.Portraits;
                                break;

                            case CrewXFERState.Portraits:

                                // Account for crew move callbacks by adding a frame delay for portrait updates after crew move...
                                if (SMAddon.smController.CrewTransfer.IvaDelayActive && SMAddon.smController.CrewTransfer.IvaPortraitDelay < SMSettings.IvaUpdateFrameDelay)
                                {
                                    SMAddon.smController.CrewTransfer.IvaPortraitDelay += 1;
                                }
                                else if (SMAddon.smController.CrewTransfer.IvaDelayActive && SMAddon.smController.CrewTransfer.IvaPortraitDelay >= SMSettings.IvaUpdateFrameDelay)
                                {
                                    SMAddon.smController.CrewTransfer.IvaDelayActive = false;
                                    IgnoreFromToXferEvent = IgnoreToFromXferEvent = false;
                                    SMAddon.smController.CrewTransfer.IvaPortraitDelay = 0;
                                    SMAddon.smController.CrewTransfer.FromCrewMember = SMAddon.smController.CrewTransfer.ToCrewMember = null;
                                    SMAddon.smController.RespawnCrew();

                                    CrewXferState = CrewXFERState.Off;
                                    CrewXferActive = IsSeat2SeatXfer = false;
                                    if (IsStockXfer)
                                    {
                                        var message = new ScreenMessage(string.Empty, 15f, ScreenMessageStyle.LOWER_CENTER);
                                        ScreenMessages.PostScreenMessage(string.Format("<color=yellow>{0} moved (by SM) to {1}.</color>", FromCrewMember.name, PartTo.partInfo.title), message, true);
                                    }
                                    IsStockXfer = false;
                                }
                                break;
                        }
                        Utilities.LogMessage("Transfer State:  " + TransferCrew.CrewXferState.ToString() + "...", "Info", SMSettings.VerboseLogging);
                        if (CrewXferState != CrewXFERState.Off)
                            SMAddon.timestamp = Planetarium.GetUniversalTime();
                        else
                            Utilities.LogMessage("CrewTransferProcess:  Complete.", "info", SMSettings.VerboseLogging);
                    }
                }
            }
            catch (Exception ex)
            {
                if (!SMAddon.frameErrTripped)
                {
                    Utilities.LogMessage("Transfer State:  " + CrewXferState.ToString() + "...", "Error", true);
                    Utilities.LogMessage(string.Format(" in CrewTransferProcess (repeating error).  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                    CrewXferState = CrewXFERState.Stop;
                    SMAddon.frameErrTripped = true;
                }
            }
        }

        public void CrewTransferComplete()
        {
            try
            {
                if (FromPart.internalModel != null && ToPart.internalModel != null)
                {
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
                // Now let's deal with third party mod support...
                IgnoreFromToXferEvent = true;
                GameEvents.HostedFromToAction<ProtoCrewMember, Part> Sourceaction = new GameEvents.HostedFromToAction<ProtoCrewMember, Part>(FromCrewMember, FromPart, ToPart);
                GameEvents.onCrewTransferred.Fire(Sourceaction);

                //If a swap, we need to handle that too...
                if (ToCrewMember != null)
                {
                    IgnoreToFromXferEvent = true;
                    GameEvents.HostedFromToAction<ProtoCrewMember, Part> targetAction = new GameEvents.HostedFromToAction<ProtoCrewMember, Part>(ToCrewMember, ToPart, FromPart);
                    GameEvents.onCrewTransferred.Fire(targetAction);
                }
                Utilities.LogMessage("RealModeCrewXfer:  Updating Portraits...", "info", SMSettings.VerboseLogging);
                FromPart.SpawnCrew();
                ToPart.SpawnCrew();

                // not sure if these help.   We have been experiencing issues with "ghost" kerbals & EVAs/docking/undocking after Crew Moves.   
                // trying this to see if it "cleans up" any internal tracking inside of KSP...
                FromPart.RegisterCrew();
                ToPart.RegisterCrew();

                SMAddon.smController.RespawnCrew();
                SMAddon.smController.CrewTransfer.IvaDelayActive = true;
                Utilities.LogMessage("RealModeCrewXfer:  Updating Portraits...", "info", SMSettings.VerboseLogging);
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format("in TransferCrewMemberComplete.  Error moving crewmember.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        internal void CrewTransferAbort()
        {
            if (SMSettings.RealismMode)
            {
                SMAddon.source2.Stop();
                SMAddon.source3.Play();
            }
            SMAddon.timestamp = SMAddon.elapsed = 0;
            SMAddon.smController.CrewTransfer.IvaDelayActive = false;
            IgnoreFromToXferEvent = IgnoreToFromXferEvent = false;
            SMAddon.smController.CrewTransfer.IvaPortraitDelay = 0;
            SMAddon.smController.CrewTransfer.FromCrewMember = SMAddon.smController.CrewTransfer.ToCrewMember = null;
            CrewXferState = CrewXFERState.Off;
            _crewXferActive = IsSeat2SeatXfer = false;
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

        internal enum CrewXFERState
        {
            Off,
            Start,
            Transfer,
            Stop,
            Portraits
        }

    }
}
