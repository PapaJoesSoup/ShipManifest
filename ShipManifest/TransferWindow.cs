using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ConnectedLivingSpace;

namespace ShipManifest
{
    public partial class ManifestController
    {
        #region Properties

        // Transfer Controller.  This module contains Resource Manifest specific code. 
        // I made it a partial class to allow file level segregation of resource and crew functionality
        // for improved readability and management by coders.

        // variables used for moving resources.  set to a negative to allow slider to function.
        public float sXferAmount = -1f;
        public bool sXferAmountHasDecimal = false;
        public bool sXferAmountHasZero = false;
        public float tXferAmount = -1f;
        public bool tXferAmountHasDecimal = false;
        public bool tXferAmountHasZero = false;
        public float AmtXferred = 0f;

        #endregion

        #region TransferWindow GUI Layout)

        // Resource Transfer Window
        // This window allows you some control over the selected resource on a selected source and target part
        // This window assumes that a resource has been selected on the Ship manifest window.
        private Vector2 SourceScrollViewerTransfer = Vector2.zero;
        private Vector2 SourceScrollViewerTransfer2 = Vector2.zero;
        private Vector2 TargetScrollViewerTransfer = Vector2.zero;
        private Vector2 TargetScrollViewerTransfer2 = Vector2.zero;
        private void TransferWindow(int windowId)
        {
            try
            {
                // This window assumes that a resource has been selected on the Ship manifest window.
                GUILayout.BeginHorizontal();
                //Left Column Begins
                GUILayout.BeginVertical();

                // Build source Transfer Viewer
                SourceTransferViewer();

                // Text above Source Details. (Between viewers)
                if (SelectedResource == "Crew" && Settings.ShowIVAUpdateBtn)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(SelectedPartSource != null ? string.Format("{0}", SelectedPartSource.partInfo.title) : "No Part Selected", GUILayout.Width(190), GUILayout.Height(20));
                    if (GUILayout.Button("Update Portraits", ManifestStyle.ButtonStyle, GUILayout.Width(110), GUILayout.Height(20)))
                    {
                        RespawnCrew();
                    }
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.Label(SelectedPartSource != null ? string.Format("{0}", SelectedPartSource.partInfo.title) : "No Part Selected", GUILayout.Width(300), GUILayout.Height(20));
                }

                // Build Details ScrollViewer
                SourceDetailsViewer();

                // Okay, we are done with the left column of the dialog...
                GUILayout.EndVertical();

                // Right Column Begins...
                GUILayout.BeginVertical();

                // Build Target Transfer Viewer
                TargetTransferViewer();

                // Text between viewers
                GUILayout.Label(SelectedPartTarget != null ? string.Format("{0}", SelectedPartTarget.partInfo.title) : "No Part Selected", GUILayout.Width(300), GUILayout.Height(20));

                // Build Target details Viewer
                TargetDetailsViewer();

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format(" in Ship Manifest Window.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        // Transfer Window components
        private void SourceTransferViewer()
        {
            try
            {
                // This is a scroll panel (we are using it to make button lists...)
                SourceScrollViewerTransfer = GUILayout.BeginScrollView(SourceScrollViewerTransfer, GUILayout.Height(120), GUILayout.Width(300));
                GUILayout.BeginVertical();

                foreach (Part part in PartsByResource[SelectedResource])
                {
                    // Build the part button title...
                    string strDescription = "";
                    switch (SelectedResource)
                    {
                        case "Crew":
                            strDescription = part.protoModuleCrew.Count.ToString() + " - " + part.partInfo.title;
                            break;
                        case "Science":
                            int cntScience = GetScienceCount(part, false);
                            strDescription = cntScience.ToString() + " - " + part.partInfo.title;
                            break;
                        default:
                            strDescription = part.Resources[SelectedResource].amount.ToString("######0.##") + " - " + part.partInfo.title;
                            break;
                    }

                    // set the conditions for a button style change.
                    int btnWidth = 265;
                    if (!Settings.RealismMode && SelectedResource != "Crew" && SelectedResource != "Science")
                        btnWidth = 180;
                    var style = SelectedPartSource == part ? ManifestStyle.ButtonToggledSourceStyle : ManifestStyle.ButtonSourceStyle;
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(string.Format("{0}", strDescription), style, GUILayout.Width(btnWidth), GUILayout.Height(20)))
                    {
                        if (!ShipManifestAddon.crewXfer && !ShipManifestAddon.XferOn)
                        {
                            SelectedModuleSource = null;
                            SelectedPartSource = part;
                            ManifestUtilities.LogMessage("SelectedPartSource...", "Info", Settings.VerboseLogging);
                        }
                    }
                    if (!Settings.RealismMode && SelectedResource != "Crew" && SelectedResource != "Science")
                    {
                        var style1 = part.Resources[SelectedResource].amount == 0 ? ManifestStyle.ButtonToggledSourceStyle : ManifestStyle.ButtonSourceStyle;
                        var style2 = part.Resources[SelectedResource].amount == part.Resources[SelectedResource].maxAmount ? ManifestStyle.ButtonToggledSourceStyle : ManifestStyle.ButtonSourceStyle;

                        if (GUILayout.Button(string.Format("{0}", "Dump"), style1, GUILayout.Width(45), GUILayout.Height(20)))
                        {
                            DumpPartResource(part, SelectedResource);
                        }
                        if (GUILayout.Button(string.Format("{0}", "Fill"), style2, GUILayout.Width(30), GUILayout.Height(20)))
                        {
                            FillPartResource(part, SelectedResource);
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format(" in Ship Manifest Window - SourceTransferViewer.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        private void SourceDetailsViewer()
        {
            try
            {
                // Source Part resource Details
                // this Scroll viewer is for the details of the part selected above.
                SourceScrollViewerTransfer2 = GUILayout.BeginScrollView(SourceScrollViewerTransfer2, GUILayout.Height(90), GUILayout.Width(300));
                GUILayout.BeginVertical();

                if (SelectedPartSource != null)
                {
                    if (SelectedResource == "Crew")
                    {
                        SourceDetailsCrew();
                    }
                    else if (SelectedResource == "Science")
                    {
                        SourceDetailsScience();
                    }
                    else
                    {
                        // resources are left....
                        SourceDetailsResources();
                    }
                }
                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format(" in Ship Manifest Window - SourceDetailsViewer.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        private void SourceDetailsCrew()
        {
            List<ProtoCrewMember> crewMembers = SelectedPartSource.protoModuleCrew;
            for (int x = 0; x < SelectedPartSource.protoModuleCrew.Count(); x++)
            {
                ProtoCrewMember crewMember = SelectedPartSource.protoModuleCrew[x];
                GUILayout.BeginHorizontal();
                if (crewMember.seat != null)
                {
                    if (ShipManifestAddon.crewXfer || ShipManifestAddon.XferOn)
                        GUI.enabled = false;

                    if (GUILayout.Button(">>", ManifestStyle.ButtonStyle, GUILayout.Width(15), GUILayout.Height(20)))
                    {
                        TransferCrewMember(crewMember, SelectedPartSource, SelectedPartSource);
                    }
                    GUI.enabled = true;
                }
                GUILayout.Label(string.Format("  {0}", crewMember.name), GUILayout.Width(190), GUILayout.Height(20));
                if (ShipManifestAddon.CanKerbalsBeXferred(SelectedPartSource))
                {
                    if (ShipManifestAddon.crewXfer || ShipManifestAddon.XferOn)
                        GUI.enabled = false;

                    if (GUILayout.Button("Xfer", ManifestStyle.ButtonStyle, GUILayout.Width(50), GUILayout.Height(20)))
                    {
                        TransferCrewMember(crewMember, SelectedPartSource, SelectedPartTarget);
                    }
                    GUI.enabled = true;
                }
                GUILayout.EndHorizontal();
            }
        }

        private void SourceDetailsScience()
        {
            IScienceDataContainer[] modules = SelectedPartSource.FindModulesImplementing<IScienceDataContainer>().ToArray();
            foreach (PartModule pm in modules)
            {
                // Containers.
                int scienceCount = ((IScienceDataContainer)pm).GetScienceCount();

                GUILayout.BeginHorizontal();
                GUILayout.Label(string.Format("{0} - ({1})", pm.moduleName, scienceCount.ToString()), GUILayout.Width(205), GUILayout.Height(20));

                // If we have target selected, it is not the same as the source, and there is science to xfer.
                if ((SelectedModuleTarget != null && pm != SelectedModuleTarget) && scienceCount > 0)
                {
                    if (GUILayout.Button("Xfer", ManifestStyle.ButtonStyle, GUILayout.Width(50), GUILayout.Height(20)))
                    {
                        SelectedModuleSource = pm;
                        TransferScience(SelectedModuleSource, SelectedModuleTarget);
                        SelectedModuleSource = null;
                    }
                }
                GUILayout.EndHorizontal();
            }
        }

        private void SourceDetailsResources()
        {
            foreach (PartResource resource in SelectedPartSource.Resources)
            {
                if (resource.info.name == SelectedResource)
                {
                    // This routine assumes that a resource has been selected on the Resource manifest window.
                    string flowtextS = "Off";
                    bool flowboolS = SelectedPartSource.Resources[SelectedResource].flowState;
                    if (flowboolS)
                    {
                        flowtextS = "On";
                    }
                    else
                    {
                        flowtextS = "Off";
                    }
                    PartResource.FlowMode flowmodeS = SelectedPartSource.Resources[SelectedResource].flowMode;

                    GUILayout.BeginHorizontal();
                    GUILayout.Label(string.Format("({0}/{1})", resource.amount.ToString("#######0.####"), resource.maxAmount.ToString("######0.####")), GUILayout.Width(175), GUILayout.Height(20));
                    GUILayout.Label(string.Format("{0}", flowtextS), GUILayout.Width(30), GUILayout.Height(20));
                    if (GUILayout.Button("Flow", GUILayout.Width(50), GUILayout.Height(20)))
                    {
                        if (flowboolS)
                        {
                            SelectedPartSource.Resources[SelectedResource].flowState = false;
                            flowtextS = "Off";
                        }
                        else
                        {
                            SelectedPartSource.Resources[SelectedResource].flowState = true;
                            flowtextS = "On";
                        }
                    }
                    GUILayout.EndHorizontal();
                    if ((SelectedPartTarget != null && SelectedPartSource != SelectedPartTarget) &&
                        (SelectedPartSource.Resources[resource.info.name].amount > 0 && SelectedPartTarget.Resources[resource.info.name].amount < SelectedPartTarget.Resources[resource.info.name].maxAmount))
                    {
                        if (!ShipManifestAddon.crewXfer && !ShipManifestAddon.XferOn)
                        {
                            // let's determine how much of a resource we can move to the target.
                            double maxXferAmount = SelectedPartTarget.Resources[resource.info.name].maxAmount - SelectedPartTarget.Resources[resource.info.name].amount;
                            if (maxXferAmount > SelectedPartSource.Resources[resource.info.name].amount)
                                maxXferAmount = SelectedPartSource.Resources[resource.info.name].amount;
                            if (maxXferAmount < 0)
                                maxXferAmount = 0;

                            // This is used to set the slider to the max amount by default.  
                            // OnUpdate draws every frame, so we need a way to ignore this or the slider will stay at max
                            // We set XferAmount to -1 when we set new source or target parts.
                            if (sXferAmount < 0)
                                sXferAmount = (float)maxXferAmount;

                            // Left Details...
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Enter Xfer Amt:  ", GUILayout.Width(100));

                            // Lets parse the string to allow decimal points.
                            string strXferAmount = sXferAmount.ToString();
                            float newAmount = 0;

                            // add the decimal point if it was typed.
                            if (sXferAmountHasDecimal)
                                strXferAmount += ".";
                            // add the zero if it was typed.
                            if (sXferAmountHasZero)
                                strXferAmount += "0";

                            strXferAmount = GUILayout.TextField(strXferAmount, 20, GUILayout.Width(105));

                            // update decimal bool 
                            if (strXferAmount.EndsWith(".") || strXferAmount.EndsWith(".0"))
                                sXferAmountHasDecimal = true;
                            else
                                sXferAmountHasDecimal = false;

                            //update zero bool 
                            if (strXferAmount.Contains(".") && strXferAmount.EndsWith("0"))
                                sXferAmountHasZero = true;
                            else
                                sXferAmountHasZero = false;

                            if (float.TryParse(strXferAmount, out newAmount))
                                sXferAmount = newAmount;

                            if (GUILayout.Button("Xfer", GUILayout.Width(50), GUILayout.Height(20)))
                            {
                                TransferResource(SelectedPartSource, SelectedPartTarget, (double)sXferAmount);
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Xfer:  ", GUILayout.Width(50), GUILayout.Height(20));
                            sXferAmount = GUILayout.HorizontalSlider(sXferAmount, 0, (float)maxXferAmount, GUILayout.Width(210));
                            GUILayout.EndHorizontal();
                        }
                    }
                }
            }
        }

        private void TargetTransferViewer()
        {
            try
            {
                // This is a scroll panel (we are using it to make button lists...)
                TargetScrollViewerTransfer = GUILayout.BeginScrollView(TargetScrollViewerTransfer, GUILayout.Height(120), GUILayout.Width(300));
                GUILayout.BeginVertical();
                foreach (Part part in PartsByResource[SelectedResource])
                {
                    // Build the part button title...
                    string strDescription = "";
                    switch (SelectedResource)
                    {
                        case "Crew":
                            strDescription = part.protoModuleCrew.Count.ToString() + " - " + part.partInfo.title;
                            break;
                        case "Science":
                            int cntScience = GetScienceCount(part, false);
                            strDescription = cntScience.ToString() + " - " + part.partInfo.title;
                            break;
                        default:
                            strDescription = part.Resources[SelectedResource].amount.ToString("######0.##") + " - " + part.partInfo.title;
                            break;
                    }

                    // set the conditions for a button style change.
                    int btnWidth = 265;
                    if (!Settings.RealismMode && SelectedResource != "Crew" && SelectedResource != "Science")
                        btnWidth = 180;
                    var style = SelectedPartTarget == part ? ManifestStyle.ButtonToggledTargetStyle : ManifestStyle.ButtonTargetStyle;
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(string.Format("{0}", strDescription), style, GUILayout.Width(btnWidth), GUILayout.Height(20)))
                    {
                        if (!ShipManifestAddon.crewXfer && !ShipManifestAddon.XferOn)
                        {
                            SelectedPartTarget = part;
                            ManifestUtilities.LogMessage("SelectedPartTarget...", "Info", Settings.VerboseLogging);
                        }
                    }
                    if (!Settings.RealismMode && SelectedResource != "Crew" && SelectedResource != "Science")
                    {
                        var style1 = part.Resources[SelectedResource].amount == 0 ? ManifestStyle.ButtonToggledTargetStyle : ManifestStyle.ButtonTargetStyle;
                        var style2 = part.Resources[SelectedResource].amount == part.Resources[SelectedResource].maxAmount ? ManifestStyle.ButtonToggledTargetStyle : ManifestStyle.ButtonTargetStyle;

                        if (GUILayout.Button(string.Format("{0}", "Dump"), style1, GUILayout.Width(45), GUILayout.Height(20)))
                        {
                            DumpPartResource(part, SelectedResource);
                        }
                        if (GUILayout.Button(string.Format("{0}", "Fill"), style2, GUILayout.Width(30), GUILayout.Height(20)))
                        {
                            FillPartResource(part, SelectedResource);
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format(" in Ship Manifest Window - TargetTransferViewer.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        private void TargetDetailsViewer()
        {
            try
            {
                // Target Part resource details
                TargetScrollViewerTransfer2 = GUILayout.BeginScrollView(TargetScrollViewerTransfer2, GUILayout.Height(90), GUILayout.Width(300));
                GUILayout.BeginVertical();

                // --------------------------------------------------------------------------
                if (SelectedPartTarget != null)
                {
                    if (SelectedResource == "Crew")
                    {
                        TargetDetailsCrew();
                    }
                    else if (SelectedResource == "Science")
                    {
                        TargetDetailsScience();
                    }
                    else
                        TargetDetailsResources();
                }
                // --------------------------------------------------------------------------
                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format(" in Ship Manifest Window - TargetDetailsViewer.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        private void TargetDetailsCrew()
        {
            for (int x = 0; x < SelectedPartTarget.protoModuleCrew.Count(); x++)
            {
                ProtoCrewMember crewMember = SelectedPartTarget.protoModuleCrew[x];
                // This routine assumes that a resource has been selected on the Resource manifest window.
                GUILayout.BeginHorizontal();
                if (crewMember.seat != null)
                {
                    if (ShipManifestAddon.crewXfer || ShipManifestAddon.XferOn)
                        GUI.enabled = false;

                    if (GUILayout.Button(">>", ManifestStyle.ButtonStyle, GUILayout.Width(15), GUILayout.Height(20)))
                    {
                        TransferCrewMember(crewMember, SelectedPartTarget, SelectedPartTarget);
                    }
                    GUI.enabled = true;
                }
                GUILayout.Label(string.Format("  {0}", crewMember.name), GUILayout.Width(190), GUILayout.Height(20));
                if (ShipManifestAddon.CanKerbalsBeXferred(SelectedPartTarget))
                {
                    if (ShipManifestAddon.crewXfer || ShipManifestAddon.XferOn)
                        GUI.enabled = false;

                    // set the conditions for a button style change.
                    if (GUILayout.Button("Xfer", ManifestStyle.ButtonStyle, GUILayout.Width(50), GUILayout.Height(20)))
                    {
                        TransferCrewMember(crewMember, SelectedPartTarget, SelectedPartSource);
                    }
                    GUI.enabled = true;
                }
                GUILayout.EndHorizontal();
            }
        }

        private void TargetDetailsScience()
        {
            int count = 0;
            foreach (PartModule tpm in SelectedPartTarget.Modules)
            {
                if (tpm is IScienceDataContainer)
                    count += 1;
            }

            foreach (PartModule pm in SelectedPartTarget.Modules)
            {
                // Containers.
                int scienceCount = 0;
                if (pm is IScienceDataContainer)
                {
                    scienceCount = ((IScienceDataContainer)pm).GetScienceCount();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(string.Format("{0} - ({1})", pm.moduleName, scienceCount.ToString()), GUILayout.Width(205), GUILayout.Height(20));
                    // set the conditions for a button style change.
                    bool ShowReceive = false;
                    if (pm == SelectedModuleTarget)
                        ShowReceive = true;
                    else if (count == 1)
                        ShowReceive = true;
                    //SelectedModuleTarget = pm;
                    var style = ShowReceive ? ManifestStyle.ButtonToggledTargetStyle : ManifestStyle.ButtonStyle;
                    if (GUILayout.Button("Recv", style, GUILayout.Width(50), GUILayout.Height(20)))
                    {
                        SelectedModuleTarget = pm;
                    }
                    GUILayout.EndHorizontal();
                }
            }
        }

        private void TargetDetailsResources()
        {
            // Resources
            foreach (PartResource resource in SelectedPartTarget.Resources)
            {
                if (resource.info.name == SelectedResource)
                {
                    // This routine assumes that a resource has been selected on the Resource manifest window.
                    string flowtextT = "Off";
                    bool flowboolT = SelectedPartTarget.Resources[SelectedResource].flowState;
                    if (flowboolT)
                    {
                        flowtextT = "On";
                    }
                    else
                    {
                        flowtextT = "Off";
                    }
                    PartResource.FlowMode flowmodeT = resource.flowMode;

                    GUILayout.BeginHorizontal();
                    GUILayout.Label(string.Format("({0}/{1})", resource.amount.ToString("#######0.####"), resource.maxAmount.ToString("######0.####")), GUILayout.Width(175), GUILayout.Height(20));
                    GUILayout.Label(string.Format("{0}", flowtextT), GUILayout.Width(30), GUILayout.Height(20));
                    if (GUILayout.Button("Flow", ManifestStyle.ButtonStyle, GUILayout.Width(50), GUILayout.Height(20)))
                    {
                        if (flowboolT)
                        {
                            SelectedPartTarget.Resources[SelectedResource].flowState = false;
                            flowtextT = "Off";
                        }
                        else
                        {
                            SelectedPartTarget.Resources[SelectedResource].flowState = true;
                            flowtextT = "On";
                        }
                    }
                    GUILayout.EndHorizontal();
                    if ((SelectedPartSource != null && SelectedPartSource != SelectedPartTarget) && (SelectedPartTarget.Resources[resource.info.name].amount > 0 && SelectedPartSource.Resources[resource.info.name].amount < SelectedPartSource.Resources[resource.info.name].maxAmount))
                    {
                        // create xfer slider;
                        if (!ShipManifestAddon.crewXfer && !ShipManifestAddon.XferOn)
                        {
                            // let's determine how much of a resource we can move to the Source.
                            double maxXferAmount = SelectedPartSource.Resources[resource.info.name].maxAmount - SelectedPartSource.Resources[resource.info.name].amount;
                            if (maxXferAmount > SelectedPartTarget.Resources[resource.info.name].amount)
                                maxXferAmount = SelectedPartTarget.Resources[resource.info.name].amount;
                            if (maxXferAmount < 0)
                                maxXferAmount = 0;

                            // This is used to set the slider to the max amount by default.  
                            // OnUpdate draws every frame, so we need a way to ignore this or the slider will stay at max
                            // We set XferAmount to -1 when we set new source or target parts.
                            if (tXferAmount < 0)
                                tXferAmount = (float)maxXferAmount;

                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Enter Xfer Amt:  ", GUILayout.Width(100));

                            // Lets parse the string to allow decimal points.
                            string strXferAmount = tXferAmount.ToString();
                            float newAmount = 0;

                            // add the decimal point if it was typed.
                            if (tXferAmountHasDecimal)
                                strXferAmount += ".";
                            if (tXferAmountHasZero)
                                strXferAmount += "0";

                            strXferAmount = GUILayout.TextField(strXferAmount, 20, GUILayout.Width(105));

                            // update decimal bool with new string
                            if (strXferAmount.EndsWith(".") || strXferAmount.EndsWith(".0"))
                                tXferAmountHasDecimal = true;
                            else
                                tXferAmountHasDecimal = false;

                            //update zero bool 
                            if (strXferAmount.Contains(".") && strXferAmount.EndsWith("0"))
                                tXferAmountHasZero = true;
                            else
                                tXferAmountHasZero = false;

                            if (float.TryParse(strXferAmount, out newAmount))
                                tXferAmount = newAmount;
                                
                            if (GUILayout.Button("Xfer", GUILayout.Width(50), GUILayout.Height(20)))
                                TransferResource(SelectedPartTarget, SelectedPartSource, (double)tXferAmount);

                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Xfer:  ", GUILayout.Width(50), GUILayout.Height(20));
                            tXferAmount = GUILayout.HorizontalSlider(tXferAmount, 0, (float)maxXferAmount, GUILayout.Width(210));
                            GUILayout.EndHorizontal();
                        }
                    }
                }
            }
        }

        #endregion

        #region Methods

        private void TransferCrewMember(ProtoCrewMember sourceMember, Part sourcePart, Part targetPart)
        {
            try
            {
                if (sourcePart.internalModel != null && targetPart.internalModel != null)
                {
                    // Build source and target seat indexes.
                    int curIdx = sourceMember.seatIdx;
                    int newIdx = curIdx;
                    InternalSeat sourceSeat = sourceMember.seat;
                    InternalSeat targetSeat = null;
                    if (sourcePart == targetPart)
                    {
                        // Must be a move...
                        if (newIdx + 1 >= sourcePart.CrewCapacity)
                            newIdx = 0;
                        else
                            newIdx += 1;
                        // get target seat from part's inernal model
                        targetSeat = sourcePart.internalModel.seats[newIdx];
                    }
                    else
                    {
                        // Xfer to another part
                        // get target seat from part's inernal model
                        for (int x = 0; x < targetPart.internalModel.seats.Count; x++)
                        {
                            InternalSeat seat = targetPart.internalModel.seats[x];
                            if (!seat.taken)
                            {
                                targetSeat = seat;
                                newIdx = x;
                                break;
                            }
                        }
                        // All seats full?
                        if (targetSeat == null)
                        {
                            // try to match seat if possible (swap with counterpart)
                            if (newIdx >= targetPart.internalModel.seats.Count)
                                newIdx = 0;
                            targetSeat = targetPart.internalModel.seats[newIdx];
                        }
                    }

                    // seats have been chosen.
                    // Do we need to swap places with another Kerbal?
                    if (targetSeat.taken)
                    {
                        // Swap places.

                        // get Kerbal to swap with through his seat...
                        ProtoCrewMember targetMember = targetSeat.kerbalRef.protoCrewMember;

                        // Remove the crew members from the part(s)...
                        RemoveCrew(sourceMember, sourcePart);
                        RemoveCrew(targetMember, targetPart);

                        // At this point, the kerbals are in the "ether".
                        // this may be why there is an issue with refreshing the internal view.. 
                        // It may allow (or expect) a board call from an (invisible) eva object.   
                        // If I can manage to properly trigger that call... then all should properly refresh...
                        // I'll look into that...

                        // Update:  Thanks to Extraplanetary LaunchPads for helping me solve this problem!
                        // Send the kerbal(s) eva.  This is the eva trigger I was looking for
                        // We will fie the board event when we are ready, in the update code.
                        evaAction = new GameEvents.FromToAction<Part, Part>(sourcePart, targetPart);
                        if (Settings.EnableTextureReplacer)
                            GameEvents.onCrewOnEva.Fire(evaAction);

                        // Add the crew members back into the part(s) at their new seats.
                        sourcePart.AddCrewmemberAt(targetMember, curIdx);
                        targetPart.AddCrewmemberAt(sourceMember, newIdx);
                    }
                    else
                    {
                        // Just move.
                        RemoveCrew(sourceMember, sourcePart);
                        evaAction = new GameEvents.FromToAction<Part, Part>(sourcePart, targetPart);

                        if (Settings.EnableTextureReplacer)
                            GameEvents.onCrewOnEva.Fire(evaAction);

                        targetPart.AddCrewmemberAt(sourceMember, newIdx);
                    }

                    // if moving within a part, set the seat2seat flag
                    if (sourcePart == targetPart)
                        ShipManifestAddon.isSeat2Seat = true;
                    else
                        ShipManifestAddon.isSeat2Seat = false;

                    // set the crew transfer flag and wait forthe timeout before firing the board event.
                    ShipManifestAddon.crewXfer = true;
                }
                else
                {
                    // no portraits, so let's just move kerbals...
                    RemoveCrew(sourceMember, sourcePart);
                    AddCrew(sourceMember, targetPart);
                    ShipManifestAddon.crewXfer = true;
                }
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format("Error moving crewmember.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        private void TransferScience(PartModule source, PartModule target)
        {
            ScienceData[] moduleScience = null;
            try
            {
                if (((IScienceDataContainer)source) != null)
                    moduleScience = ((IScienceDataContainer)source).GetData();
                else
                    moduleScience = null;

                if (moduleScience != null && moduleScience.Length > 0)
                {
                    ManifestUtilities.LogMessage(string.Format("moduleScience has data..."), "Info", Settings.VerboseLogging);

                    if (((IScienceDataContainer)target) != null)
                    {
                        // Lets store the data from the source.
                        if (((ModuleScienceContainer)target).StoreData( new List<IScienceDataContainer> { (IScienceDataContainer)source }, false))
                        {
                            ManifestUtilities.LogMessage(string.Format("((ModuleScienceContainer)source) data stored"), "Info", Settings.VerboseLogging);
                            foreach (ScienceData data in moduleScience)
                            {
                                ((IScienceDataContainer)source).DumpData(data);
                            }

                            if (Settings.RealismMode)
                            {
                                ManifestUtilities.LogMessage(string.Format("((Module ScienceExperiment xferred.  Dump Source data"), "Info", Settings.VerboseLogging);
                            }
                            else
                            {
                                ManifestUtilities.LogMessage(string.Format("((Module ScienceExperiment xferred.  Dump Source data, reset Experiment"), "Info", Settings.VerboseLogging);
                                ((ModuleScienceExperiment)source).ResetExperiment();
                            }
                        }
                        else
                        {
                            ManifestUtilities.LogMessage(string.Format("Science Data transfer failed..."), "Info", true);
                        }
                    }
                    else
                    {
                        ManifestUtilities.LogMessage(string.Format("((IScienceDataContainer)target) is null"), "Info", true);
                    }
                    ManifestUtilities.LogMessage(string.Format("Transfer Complete."), "Info", Settings.VerboseLogging);
                }
                else if (moduleScience == null)
                {
                    ManifestUtilities.LogMessage(string.Format("moduleScience is null..."), "Info", Settings.VerboseLogging);
                }
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(" in TransferScience:  Error:  " + ex.ToString(), "Info", true);
            }
        }

        private void TransferResource(Part source, Part target, double XferAmount)
        {
            try
            {
                if (source.Resources.Contains(SelectedResource) && target.Resources.Contains(SelectedResource))
                {
                    double maxAmount = target.Resources[SelectedResource].maxAmount;
                    double sourceAmount = source.Resources[SelectedResource].amount;
                    double targetAmount = target.Resources[SelectedResource].amount;
                    if (XferAmount <= 0)
                    {
                        XferAmount = maxAmount - targetAmount;
                    }

                    // make sure we have enough...
                    if (XferAmount > sourceAmount)
                    {
                        XferAmount = sourceAmount;
                    }
                    if (Settings.RealismMode)
                    {
                        // now lets make some noise and slow the process down...
                        ManifestUtilities.LogMessage("Playing pump sound...", "Info", Settings.VerboseLogging);


                        // This flag enables the Update handler in ShipManifestAddon and sets the direction
                        if (source == SelectedPartSource)
                            ShipManifestAddon.XferMode = ShipManifestAddon.XFERMode.SourceToTarget;
                        else
                            ShipManifestAddon.XferMode = ShipManifestAddon.XFERMode.TargetToSource;

                            ShipManifestAddon.XferOn = true;
                    }
                    else
                    {
                        // Fill target
                        target.Resources[SelectedResource].amount += XferAmount;

                        // Drain source...
                        source.Resources[SelectedResource].amount -= XferAmount;
                    }
                }
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format(" in  TransferResource.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        private int GetScienceCount(Part part, bool IsCapacity)
        {
            try
            {
                int scienceCount = 0;
                foreach (PartModule pm in part.Modules)
                {
                    if (pm is IScienceDataContainer)
                    {
                        scienceCount += ((IScienceDataContainer)pm).GetScienceCount();
                    }
                }

                return scienceCount;
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format(" in GetScienceCount.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                return 0;
            }
        }

        #endregion
    }
}
