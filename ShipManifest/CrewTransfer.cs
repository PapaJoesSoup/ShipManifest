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
    public class CrewTransfer : ICrewTransfer
    {
        // crew xfer inerface properties

        internal static bool IgnoreSourceXferEvent = false;
        internal static bool IgnoreTargetXferEvent = false;

        private bool _crewXferActive = false;
        public bool CrewXferActive
        {
            get
            {
                return _crewXferActive;
            }
            set
            {
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
        private double _crewXferDelaysec = SMSettings.IVATimeDelaySec;
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

        public InternalSeat _sourceSeat = null;
        public InternalSeat SourceSeat
        {
            get
            {
                return _sourceSeat;
            }
            set
            {
                _sourceSeat = value;
            }
        }
        public InternalSeat _targetSeat = null;
        public InternalSeat TargetSeat
        {
            get
            {
                return _targetSeat;
            }
            set
            {
                _targetSeat = value;
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

        private Part _sourcePart;
        public Part SourcePart
        {
            get
            {
                return _sourcePart;
            }
            set
            {
                _sourcePart = value;
            }
        }
        private Part _targetPart;
        public Part TargetPart
        {
            get
            {
                return _targetPart;
            }
            set
            {
                _targetPart = value;
            }
        }
        private ProtoCrewMember _sourceCrewMember;
        public ProtoCrewMember SourceCrewMember
        {
            get
            {
                return _sourceCrewMember;
            }
            set
            {
                _sourceCrewMember = value;
            }
        }
        private ProtoCrewMember _targetCrewMember;
        public ProtoCrewMember TargetCrewMember
        {
            get
            {
                return _targetCrewMember;
            }
            set
            {
                _targetCrewMember = value;
            }
        }
        
        public static CrewTransfer Instance
        {
            get
            {
                return SMAddon.smController.CrewTransfer;
            }
        }

        public CrewTransfer() 
        {

        }

        public void CrewTransferBegin(ProtoCrewMember crewMember, Part sourcePart, Part targetPart)
        {
            SourcePart = sourcePart;
            TargetPart = targetPart;
            SourceCrewMember = crewMember;

            if (SourcePart.internalModel != null && TargetPart.internalModel != null)
            {
                // Build source and target seat indexes.
                SourceSeat = SourceCrewMember.seat;
                TargetSeat = null;
                if (SourcePart == TargetPart)
                {
                    // Must be a move...
                    //Get the first available valid seat
                    foreach (InternalSeat seat in SourcePart.internalModel.seats)
                    {
                        if (seat.taken)
                        {
                            // This supports DeepFreeze frozen kerbals...
                            if (seat.kerbalRef!= null && seat.crew != SourceCrewMember)
                            {
                                TargetSeat = seat;
                                break;
                            }
                        }
                        else
                        {
                            TargetSeat = seat;
                            break;
                        }
                    }
                }
                else
                {
                    // Xfer to another part
                    // get target seat from target part's inernal model
                    foreach (InternalSeat seat in TargetPart.internalModel.seats)
                    {
                        if (!seat.taken)
                        {
                            TargetSeat = seat;
                            break;
                        }
                    }
                    // All seats full?
                    if (TargetSeat == null)
                    {
                        foreach (InternalSeat seat in TargetPart.internalModel.seats)
                        {
                            // This supports DeepFreeze frozen kerbals...
                            if (seat.kerbalRef != null)
                            {
                                TargetSeat = seat;
                                break;
                            }
                        }
                    }
                }

                // seats have been chosen.
                // Do we need to swap places with another Kerbal?
                if (TargetSeat.taken)
                {
                    // get Kerbal to swap with through his seat...
                    TargetCrewMember = TargetSeat.kerbalRef.protoCrewMember;
                }
                // if moving within a part, set the seat2seat flag
                IsSeat2SeatXfer = SourcePart == TargetPart ? true : false;
            }
            CrewXferActive = true;
        }

        internal void CrewTransferProcess()
        {
            try
            {
                if (SMAddon.smController.CrewTransfer.CrewXferActive)
                {
                    Part PartSource = SMAddon.smController.CrewTransfer.SourcePart;
                    Part PartTarget = SMAddon.smController.CrewTransfer.TargetPart;
                    ProtoCrewMember pKerbal = SMAddon.smController.CrewTransfer.SourceCrewMember;
                    if (!SMSettings.RealismMode)
                    {
                        if (SMAddon.timestamp != 0)
                            SMAddon.elapsed += Planetarium.GetUniversalTime() - SMAddon.timestamp;

                        if (SMAddon.elapsed > 1)
                        {

                            // Spawn crew in parts and in vessel.
                            if (PartSource == null)
                                PartSource = PartTarget;
                            if (PartTarget == null)
                                PartTarget = PartSource;

                            SMAddon.elapsed = SMAddon.timestamp = 0;
                            CrewXferActive = false;
                            if (IsStockXfer)
                            {
                                var message = new ScreenMessage(string.Empty, 15f, ScreenMessageStyle.LOWER_CENTER);
                                ScreenMessages.PostScreenMessage(string.Format("<color=yellow>{0} moved (by SM) to {1}.</color>", SourceCrewMember.name, PartTarget.partInfo.title), message, true);
                            }
                            IsStockXfer = false;
                            CrewTransferComplete();

                            Utilities.LogMessage("CrewTransferProcess crewXfer State:  " + CrewXferActive.ToString() + "...", "Info", SMSettings.VerboseLogging);
                        }
                        if (CrewXferActive)
                            SMAddon.timestamp = Planetarium.GetUniversalTime();
                    }
                    else
                    {
                        switch (SMAddon.XferState)
                        {
                            case SMAddon.XFERState.Off:
                                // We're just starting loop, so set some evnironment stuff.
                                SMAddon.timestamp = 0;

                                // Default sound license: CC-By-SA
                                // http://www.freesound.org/people/adcbicycle/sounds/14214/
                                string path1 = SMSettings.CrewSoundStart != null ? SMSettings.CrewSoundStart : "ShipManifest/Sounds/14214-1";
                                string path2 = SMSettings.CrewSoundRun != null ? SMSettings.CrewSoundRun : "ShipManifest/Sounds/14214-2";
                                string path3 = SMSettings.CrewSoundStop != null ? SMSettings.CrewSoundStop  : "ShipManifest/Sounds/14214-3";

                                SMAddon.LoadSounds("Crew", path1, path2, path3, SMSettings.CrewSoundVol);
                                SMAddon.XferState = SMAddon.XFERState.Start;
                                break;

                            case SMAddon.XFERState.Start:

                                SMAddon.elapsed += Planetarium.GetUniversalTime() - SMAddon.timestamp;

                                // Play run sound when start sound is nearly done. (repeats)
                                if (SMAddon.elapsed >= SMAddon.source1.clip.length - 0.25)
                                {
                                    Utilities.LogMessage("source2.play():  started.", "info", true);
                                    SMAddon.source2.Play();
                                    SMAddon.elapsed = 0;
                                    SMAddon.XferState = SMAddon.XFERState.Run;
                                }
                                break;

                            case SMAddon.XFERState.Run:

                                SMAddon.elapsed += Planetarium.GetUniversalTime() - SMAddon.timestamp;

                                // wait for movement to end...
                                if (SMAddon.elapsed >= CrewXferDelaySec || (IsSeat2SeatXfer && SMAddon.elapsed > Seat2SeatXferDelaySec))
                                {
                                    // Reset State vars
                                    SMAddon.XferState = SMAddon.XFERState.Stop;
                                }
                                break;

                            case SMAddon.XFERState.Stop:

                                // Spawn crew in parts and in vessel.
                                if (PartSource == null)
                                    PartSource = PartTarget;
                                if (PartTarget == null)
                                    PartTarget = PartSource;

                                // play crew sit.
                                SMAddon.source2.Stop();
                                SMAddon.source3.Play();
                                SMAddon.timestamp = SMAddon.elapsed = 0;
                                SMAddon.XferState = SMAddon.XFERState.Off;
                                CrewXferActive = false;
                                IsSeat2SeatXfer = false;
                                if (IsStockXfer)
                                {
                                    var message = new ScreenMessage(string.Empty, 15f, ScreenMessageStyle.LOWER_CENTER);
                                    ScreenMessages.PostScreenMessage(string.Format("<color=yellow>{0} moved (by SM) to {1}.</color>", SourceCrewMember.name, PartTarget.partInfo.title), message, true);
                                }
                                IsStockXfer = false;
                                CrewTransferComplete();

                                break;
                        }
                        Utilities.LogMessage("Transfer State:  " + SMAddon.XferState.ToString() + "...", "Info", SMSettings.VerboseLogging);
                        if (SMAddon.XferState != SMAddon.XFERState.Off)
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
                    Utilities.LogMessage("Transfer State:  " + SMAddon.XferState.ToString() + "...", "Error", true);
                    Utilities.LogMessage(string.Format(" in CrewTransferProcess (repeating error).  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                    SMAddon.XferState = SMAddon.XFERState.Stop;
                    SMAddon.frameErrTripped = true;
                }
            }
        }

        public void CrewTransferComplete()
        {
            try
            {
                if (SourcePart.internalModel != null && TargetPart.internalModel != null)
                {
                    if (TargetSeat.taken)
                    {
                        // Swap places.

                        // Remove the crew members from the part(s)...
                        RemoveCrewMember(SourceCrewMember, SourcePart);
                        RemoveCrewMember(TargetCrewMember, TargetPart);

                        // Add the crew members back into the part(s) at their new seats.
                        SourcePart.AddCrewmemberAt(TargetCrewMember, SourcePart.internalModel.seats.IndexOf(SourceSeat));
                        TargetPart.AddCrewmemberAt(SourceCrewMember, TargetPart.internalModel.seats.IndexOf(TargetSeat));
                    }
                    else
                    {
                        // Just move.
                        RemoveCrewMember(SourceCrewMember, SourcePart);
                        TargetPart.AddCrewmemberAt(SourceCrewMember, TargetPart.internalModel.seats.IndexOf(TargetSeat));
                    }

                    // Now let's deal with third party mod support...
                    if (InstalledMods.IsKISInstalled)
                    {
                        if (TargetSeat != SourceSeat)
                        {
                            GameEvents.HostedFromToAction<ProtoCrewMember, Part> Sourceaction = new GameEvents.HostedFromToAction<ProtoCrewMember, Part>(SourceCrewMember, SourcePart, TargetPart);
                            GameEvents.onCrewTransferred.Fire(Sourceaction);

                            //If a swap, we need to handle that too...
                            if (TargetCrewMember != null)
                            {
                                GameEvents.HostedFromToAction<ProtoCrewMember, Part> targetAction = new GameEvents.HostedFromToAction<ProtoCrewMember, Part>(TargetCrewMember, TargetPart, SourcePart);
                                GameEvents.onCrewTransferred.Fire(targetAction);
                            }
                        }
                    }
                }
                else
                {
                    // no portraits, so let's just move kerbals...
                    RemoveCrewMember(SourceCrewMember, SourcePart);
                    AddCrewMember(SourceCrewMember, TargetPart);
                }
                Utilities.LogMessage("RealModeCrewXfer:  Updating Portraits...", "info", SMSettings.VerboseLogging);
                SourcePart.SpawnCrew();
                TargetPart.SpawnCrew();

                // not sure if these help.   We have been experiencing issues with "ghost" kerbals & EVAs/docking/undocking after Crew Moves.   
                // trying this to see if it "cleans up" any internal tracking inside of KSP...
                SourcePart.RegisterCrew();
                TargetPart.RegisterCrew();

                SMAddon.smController.RespawnCrew();
                SMAddon.smController.CrewTransfer.IvaDelayActive = true;
                Utilities.LogMessage("RealModeCrewXfer:  Updating Portraits...", "info", SMSettings.VerboseLogging);
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format("in TransferCrewMemberComplete.  Error moving crewmember.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
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


    }
}
