using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ConnectedLivingSpace;

namespace ShipManifest
{
    internal static class WindowTransfer
    {
        #region Properties

        internal static string ToolTip = "";
        internal static bool ToolTipActive = false;
        internal static bool ShowToolTips = Settings.TransferToolTips;

        internal static string xferToolTip = "";

        internal static string strSXferAmount = "0";
        internal static string strTXferAmount = "0";

        #endregion

        #region TransferWindow (GUI Layout)

        // Resource Transfer Window
        // This window allows you some control over the selected resource on a selected source and target part
        // This window assumes that a resource has been selected on the Ship manifest window.
        internal static void Display(int windowId)
        {
            // Reset Tooltip active flag...
            ToolTipActive = false;
            ShowToolTips = Settings.TransferToolTips;

            GUIContent label = new GUIContent("", "Close Window");
            if (SMAddon.crewXfer || SMAddon.XferOn)
            {
                label = new GUIContent("", "Action in progress.  Cannot close window");
                GUI.enabled = false;
            }
            Rect rect = new Rect(604, 4, 16, 16);
            if (GUI.Button(rect, label))
            {
                Settings.ShowTransferWindow = false;
                SMAddon.smController.SelectedResource = null;
                SMAddon.smController.SelectedPartSource = SMAddon.smController.SelectedPartTarget = null;
                ToolTip = "";
                return;
            }
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Settings.TransferPosition, GUI.tooltip, ref ToolTipActive, 0, 0);
            GUI.enabled = true;
            try
            {
                // This window assumes that a resource has been selected on the Ship manifest window.
                GUILayout.BeginHorizontal();
                //Left Column Begins
                GUILayout.BeginVertical();


                // Build source Transfer Viewer
                SourceTransferViewer();

                // Text above Source Details. (Between viewers)
                if (SMAddon.smController.SelectedResource == "Crew" && Settings.ShowIVAUpdateBtn)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(SMAddon.smController.SelectedPartSource != null ? string.Format("{0}", SMAddon.smController.SelectedPartSource.partInfo.title) : "No Part Selected", GUILayout.Width(190), GUILayout.Height(20));
                    if (GUILayout.Button("Update Portraits", ManifestStyle.ButtonStyle, GUILayout.Width(110), GUILayout.Height(20)))
                    {
                        SMAddon.smController.RespawnCrew();
                    }
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.Label(SMAddon.smController.SelectedPartSource != null ? string.Format("{0}", SMAddon.smController.SelectedPartSource.partInfo.title) : "No Part Selected", GUILayout.Width(300), GUILayout.Height(20));
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
                GUILayout.Label(SMAddon.smController.SelectedPartTarget != null ? string.Format("{0}", SMAddon.smController.SelectedPartTarget.partInfo.title) : "No Part Selected", GUILayout.Width(300), GUILayout.Height(20));

                // Build Target details Viewer
                TargetDetailsViewer();

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in Ship Manifest Window.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        #region Source Viewers GUI Layout)
        // Transfer Window components
        private static Vector2 SourceTransferViewerScrollPosition = Vector2.zero;
        internal static void SourceTransferViewer()
        {
            try
            {
                // This is a scroll panel (we are using it to make button lists...)
                SourceTransferViewerScrollPosition = GUILayout.BeginScrollView(SourceTransferViewerScrollPosition, GUILayout.Height(125), GUILayout.Width(300));
                GUILayout.BeginVertical();

                foreach (Part part in SMAddon.smController.PartsByResource[SMAddon.smController.SelectedResource])
                {
                    // Build the part button title...
                    string strDescription = "";
                    switch (SMAddon.smController.SelectedResource)
                    {
                        case "Crew":
                            strDescription = part.protoModuleCrew.Count.ToString() + " - " + part.partInfo.title;
                            break;
                        case "Science":
                            int cntScience = GetScienceCount(part, false);
                            strDescription = cntScience.ToString() + " - " + part.partInfo.title;
                            break;
                        default:
                            strDescription = part.Resources[SMAddon.smController.SelectedResource].amount.ToString("######0.##") + " - " + part.partInfo.title;
                            break;
                    }

                    // set the conditions for a button style change.
                    int btnWidth = 265;
                    if (!Settings.RealismMode && SMAddon.smController.SelectedResource != "Crew" && SMAddon.smController.SelectedResource != "Science")
                        btnWidth = 180;
                    var style = SMAddon.smController.SelectedPartSource == part ? ManifestStyle.ButtonToggledSourceStyle : ManifestStyle.ButtonSourceStyle;
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(string.Format("{0}", strDescription), style, GUILayout.Width(btnWidth), GUILayout.Height(20)))
                    {
                        if (!SMAddon.crewXfer && !SMAddon.XferOn)
                        {
                            SMAddon.smController.SelectedModuleSource = null;
                            SMAddon.smController.SelectedPartSource = part;
                            if (SMAddon.smController.SelectedPartTarget != null && SMAddon.smController.SelectedResource != "Crew" && SMAddon.smController.SelectedResource != "Science")
                            {
                                SMAddon.smController.sXferAmount = (float)CalcMaxXferAmt(SMAddon.smController.SelectedResource, SMAddon.smController.SelectedPartSource, SMAddon.smController.SelectedPartTarget);
                                SMAddon.smController.tXferAmount = (float)CalcMaxXferAmt(SMAddon.smController.SelectedResource, SMAddon.smController.SelectedPartTarget, SMAddon.smController.SelectedPartSource);
                            }
                            Utilities.LogMessage("SelectedPartSource...", "Info", Settings.VerboseLogging);
                        }
                    }
                    if (!Settings.RealismMode && SMAddon.smController.SelectedResource != "Crew" && SMAddon.smController.SelectedResource != "Science")
                    {
                        var style1 = part.Resources[SMAddon.smController.SelectedResource].amount == 0 ? ManifestStyle.ButtonToggledSourceStyle : ManifestStyle.ButtonSourceStyle;
                        var style2 = part.Resources[SMAddon.smController.SelectedResource].amount == part.Resources[SMAddon.smController.SelectedResource].maxAmount ? ManifestStyle.ButtonToggledSourceStyle : ManifestStyle.ButtonSourceStyle;

                        if (GUILayout.Button(string.Format("{0}", "Dump"), style1, GUILayout.Width(45), GUILayout.Height(20)))
                        {
                            SMController.DumpPartResource(part, SMAddon.smController.SelectedResource);
                        }
                        if (GUILayout.Button(string.Format("{0}", "Fill"), style2, GUILayout.Width(30), GUILayout.Height(20)))
                        {
                            SMController.FillPartResource(part, SMAddon.smController.SelectedResource);
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in Ship Manifest Window - SourceTransferViewer.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        private static Vector2 SourceDetailsViewerScrollPosition = Vector2.zero;
        private static void SourceDetailsViewer()
        {
            try
            {
                // Source Part resource Details
                // this Scroll viewer is for the details of the part selected above.
                SourceDetailsViewerScrollPosition = GUILayout.BeginScrollView(SourceDetailsViewerScrollPosition, GUILayout.Height(100), GUILayout.Width(300));
                GUILayout.BeginVertical();

                if (SMAddon.smController.SelectedPartSource != null)
                {
                    if (SMAddon.smController.SelectedResource == "Crew")
                    {
                        CrewDetails(SMAddon.smController.SelectedPartSource, SMAddon.smController.SelectedPartTarget, SMAddon.XFERMode.SourceToTarget, SourceDetailsViewerScrollPosition);
                    }
                    else if (SMAddon.smController.SelectedResource == "Science")
                    {
                        ScienceDetailsSource();
                    }
                    else
                    {
                        // resources are left....
                        ResourceDetails(SMAddon.smController.SelectedPartSource, SMAddon.smController.SelectedPartTarget, SMAddon.XFERMode.SourceToTarget, SMAddon.smController.sXferAmount, SourceDetailsViewerScrollPosition);
                    }
                }
                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in Ship Manifest Window - SourceDetailsViewer.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        #endregion

        #region Target Viewers GUI Layout)

        private static Vector2 TargetTransferViewerScrollPosition = Vector2.zero;
        private static void TargetTransferViewer()
        {
            try
            {
                // Adjust style colors for part selectors when using/not using CLS highlighting
                if (Settings.EnableCLSHighlighting)
                {
                    ManifestStyle.ButtonToggledTargetStyle.normal.textColor = Settings.Colors[Settings.TargetPartCrewColor];
                    ManifestStyle.ButtonToggledTargetStyle.hover.textColor = Settings.Colors[Settings.TargetPartColor];
                }
                else
                {
                    ManifestStyle.ButtonToggledTargetStyle.normal.textColor = Settings.Colors[Settings.TargetPartColor];
                    ManifestStyle.ButtonToggledTargetStyle.hover.textColor = Settings.Colors[Settings.TargetPartCrewColor];
                }
                // This is a scroll panel (we are using it to make button lists...)
                TargetTransferViewerScrollPosition = GUILayout.BeginScrollView(TargetTransferViewerScrollPosition, GUILayout.Height(125), GUILayout.Width(300));
                GUILayout.BeginVertical();
                foreach (Part part in SMAddon.smController.PartsByResource[SMAddon.smController.SelectedResource])
                {
                    // Build the part button title...
                    string strDescription = "";
                    switch (SMAddon.smController.SelectedResource)
                    {
                        case "Crew":
                            strDescription = part.protoModuleCrew.Count.ToString() + " - " + part.partInfo.title;
                            break;
                        case "Science":
                            int cntScience = GetScienceCount(part, false);
                            strDescription = cntScience.ToString() + " - " + part.partInfo.title;
                            break;
                        default:
                            strDescription = part.Resources[SMAddon.smController.SelectedResource].amount.ToString("######0.##") + " - " + part.partInfo.title;
                            break;
                    }

                    // set the conditions for a button style change.
                    int btnWidth = 265;
                    if (!Settings.RealismMode && SMAddon.smController.SelectedResource != "Crew" && SMAddon.smController.SelectedResource != "Science")
                        btnWidth = 180;
                    var style = SMAddon.smController.SelectedPartTarget == part ? ManifestStyle.ButtonToggledTargetStyle : ManifestStyle.ButtonTargetStyle;
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(string.Format("{0}", strDescription), style, GUILayout.Width(btnWidth), GUILayout.Height(20)))
                    {
                        if (!SMAddon.crewXfer && !SMAddon.XferOn)
                        {
                            SMAddon.smController.SelectedPartTarget = part;
                            if (SMAddon.smController.SelectedPartSource != null && SMAddon.smController.SelectedResource != "Crew" && SMAddon.smController.SelectedResource != "Science")
                            {
                                SMAddon.smController.sXferAmount = (float)CalcMaxXferAmt(SMAddon.smController.SelectedResource, SMAddon.smController.SelectedPartSource, SMAddon.smController.SelectedPartTarget);
                                SMAddon.smController.tXferAmount = (float)CalcMaxXferAmt(SMAddon.smController.SelectedResource, SMAddon.smController.SelectedPartTarget, SMAddon.smController.SelectedPartSource);
                            }
                            Utilities.LogMessage("SelectedPartTarget...", "Info", Settings.VerboseLogging);
                        }
                    }
                    if (!Settings.RealismMode && SMAddon.smController.SelectedResource != "Crew" && SMAddon.smController.SelectedResource != "Science")
                    {
                        var style1 = part.Resources[SMAddon.smController.SelectedResource].amount == 0 ? ManifestStyle.ButtonToggledTargetStyle : ManifestStyle.ButtonTargetStyle;
                        var style2 = part.Resources[SMAddon.smController.SelectedResource].amount == part.Resources[SMAddon.smController.SelectedResource].maxAmount ? ManifestStyle.ButtonToggledTargetStyle : ManifestStyle.ButtonTargetStyle;

                        if (GUILayout.Button(string.Format("{0}", "Dump"), style1, GUILayout.Width(45), GUILayout.Height(20)))
                        {
                            SMController.DumpPartResource(part, SMAddon.smController.SelectedResource);
                        }
                        if (GUILayout.Button(string.Format("{0}", "Fill"), style2, GUILayout.Width(30), GUILayout.Height(20)))
                        {
                            SMController.FillPartResource(part, SMAddon.smController.SelectedResource);
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in Ship Manifest Window - TargetTransferViewer.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        private static Vector2 TargetDetailsViewerScrollPosition = Vector2.zero;
        private static void TargetDetailsViewer()
        {
            try
            {
                // Target Part resource details
                TargetDetailsViewerScrollPosition = GUILayout.BeginScrollView(TargetDetailsViewerScrollPosition, GUILayout.Height(100), GUILayout.Width(300));
                GUILayout.BeginVertical();

                // --------------------------------------------------------------------------
                if (SMAddon.smController.SelectedPartTarget != null)
                {
                    if (SMAddon.smController.SelectedResource == "Crew")
                    {
                        CrewDetails(SMAddon.smController.SelectedPartTarget, SMAddon.smController.SelectedPartSource, SMAddon.XFERMode.TargetToSource, TargetDetailsViewerScrollPosition);
                    }
                    else if (SMAddon.smController.SelectedResource == "Science")
                    {
                        ScienceDetailsTarget();
                    }
                    else
                    {
                        ResourceDetails(SMAddon.smController.SelectedPartTarget, SMAddon.smController.SelectedPartSource, SMAddon.XFERMode.TargetToSource, SMAddon.smController.tXferAmount, TargetDetailsViewerScrollPosition);
                    }
                }
                // --------------------------------------------------------------------------
                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in Ship Manifest Window - TargetDetailsViewer.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }
        #endregion

        #region Viewer Details

        private static void CrewDetails(Part SelectedPartSource, Part SelectedPartTarget, SMAddon.XFERMode xferMode, Vector2 scrollPosition)
        {
            int scrollX = 20;
            if (xferMode == SMAddon.XFERMode.TargetToSource)
                scrollX = 320;

            List<ProtoCrewMember> crewMembers = SelectedPartSource.protoModuleCrew;
            for (int x = 0; x < SelectedPartSource.protoModuleCrew.Count(); x++)
            {
                ProtoCrewMember crewMember = SelectedPartSource.protoModuleCrew[x];
                GUILayout.BeginHorizontal();
                if (crewMember.seat != null)
                {
                    if (SMAddon.crewXfer || SMAddon.XferOn)
                        GUI.enabled = false;

                    if (GUILayout.Button(new GUIContent(">>", "Move Kerbal to another seat within Part"), ManifestStyle.ButtonStyle, GUILayout.Width(15), GUILayout.Height(20)))
                    {
                        ToolTip = "";
                        TransferCrewMemberBegin(crewMember, SelectedPartSource, SelectedPartSource);
                    }
                    if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                    {
                        Rect rect = GUILayoutUtility.GetLastRect();
                        ToolTip = Utilities.SetActiveTooltip(rect, Settings.TransferPosition, GUI.tooltip, ref ToolTipActive, scrollX, 190 - scrollPosition.y);
                    }
                    GUI.enabled = true;
                }
                GUILayout.Label(string.Format("  {0}", crewMember.name + " (" + crewMember.experienceTrait.Title + ")"), GUILayout.Width(190), GUILayout.Height(20));
                if (SMAddon.CanKerbalsBeXferred(SelectedPartSource))
                {
                    if (SMAddon.crewXfer || SMAddon.XferOn)
                        GUI.enabled = false;
                }
                else
                    GUI.enabled = false;

                if (GUILayout.Button(new GUIContent("Xfer", xferToolTip), ManifestStyle.ButtonStyle, GUILayout.Width(50), GUILayout.Height(20)))
                {
                    SMAddon.smController.CrewXferMember = crewMember;
                    TransferCrewMemberBegin(crewMember, SelectedPartSource, SelectedPartTarget);
                }
                if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                {
                    Rect rect = GUILayoutUtility.GetLastRect();
                    ToolTip = Utilities.SetActiveTooltip(rect, Settings.TransferPosition, GUI.tooltip, ref ToolTipActive, scrollX, 190 - scrollPosition.y);
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }
        }

        private static void ScienceDetailsSource()
        {
            IScienceDataContainer[] modules = SMAddon.smController.SelectedPartSource.FindModulesImplementing<IScienceDataContainer>().ToArray();
            foreach (PartModule pm in modules)
            {
                // Containers.
                int scienceCount = ((IScienceDataContainer)pm).GetScienceCount();
                bool isCollectable = true;
                if (pm.moduleName == "ModuleScienceContainer")
                    isCollectable = ((ModuleScienceContainer)pm).dataIsCollectable;
                else if (pm.moduleName == "ModuleScienceExperiment")
                    isCollectable = ((ModuleScienceExperiment)pm).dataIsCollectable;

                GUILayout.BeginHorizontal();
                GUILayout.Label(string.Format("{0} - ({1})", pm.moduleName, scienceCount.ToString()), GUILayout.Width(205), GUILayout.Height(20));

                // If we have target selected, it is not the same as the source, there is science to xfer.
                if ((SMAddon.smController.SelectedModuleTarget != null && pm != SMAddon.smController.SelectedModuleTarget) && scienceCount > 0)
                {
                    string toolTip = "";
                    if (Settings.RealismMode && !isCollectable)
                    {
                        GUI.enabled = false;
                        toolTip = "Realism Mode is preventing transfer.\r\nExperiment/data is not transferable";
                    }
                    else
                        toolTip = "Experiment/data is transferable";

                    if (GUILayout.Button(new GUIContent("Xfer", toolTip), ManifestStyle.ButtonStyle, GUILayout.Width(50), GUILayout.Height(20)))
                    {
                        SMAddon.smController.SelectedModuleSource = pm;
                        TransferScience(SMAddon.smController.SelectedModuleSource, SMAddon.smController.SelectedModuleTarget);
                        SMAddon.smController.SelectedModuleSource = null;
                    }
                    if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                    {
                        Rect rect = GUILayoutUtility.GetLastRect();
                        ToolTip = Utilities.SetActiveTooltip(rect, Settings.TransferPosition, GUI.tooltip, ref ToolTipActive, 20, 190 - TargetDetailsViewerScrollPosition.y);
                    }
                    GUI.enabled = true;
                }
                GUILayout.EndHorizontal();
            }
        }

        private static void ScienceDetailsTarget()
        {
            int count = 0;
            foreach (PartModule tpm in SMAddon.smController.SelectedPartTarget.Modules)
            {
                if (tpm is IScienceDataContainer)
                    count += 1;
            }

            foreach (PartModule pm in SMAddon.smController.SelectedPartTarget.Modules)
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
                    if (pm == SMAddon.smController.SelectedModuleTarget)
                        ShowReceive = true;
                    else if (count == 1)
                        ShowReceive = true;
                    //SelectedModuleTarget = pm;
                    var style = ShowReceive ? ManifestStyle.ButtonToggledTargetStyle : ManifestStyle.ButtonStyle;
                    if (GUILayout.Button(new GUIContent("Recv", "Set this module as the receiving container"), style, GUILayout.Width(50), GUILayout.Height(20)))
                    {
                        SMAddon.smController.SelectedModuleTarget = pm;
                    }
                    if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                    {
                        Rect rect = GUILayoutUtility.GetLastRect();
                        ToolTip = Utilities.SetActiveTooltip(rect, Settings.TransferPosition, GUI.tooltip, ref ToolTipActive, 300, 190 - TargetDetailsViewerScrollPosition.y);
                    }
                    GUILayout.EndHorizontal();
                }
            }
        }

        private static void ResourceDetails(Part partSource, Part partTarget, SMAddon.XFERMode thisXferMode, float thisXferAmount, Vector2 scrollPosition)
        {
            // This routine assumes that a resource has been selected on the Resource manifest window.
            // Pass in static vars to improve readability.
            string selectedResource = SMAddon.smController.SelectedResource;
            PartResource thisResource = partSource.Resources[selectedResource];
            PartResource.FlowMode flowmode = partSource.Resources[selectedResource].flowMode;
            bool flowbool = partSource.Resources[selectedResource].flowState;
            string strXferAmount = thisXferAmount.ToString();

            // Set scrollX offsets for left and right viewers
            string strTarget = "";
            int scrollX = 30;
            if (thisXferMode == SMAddon.XFERMode.TargetToSource)
                scrollX = 330;

            int scrollY = 180;

            // Set tooltips directional data
            if (thisXferMode == SMAddon.XFERMode.SourceToTarget)
                strTarget = "Target";
            else
                strTarget = "Source";

            string flowtext = "Off";
            if (flowbool)
                flowtext = "On";
            else
                flowtext = "Off";

            // Flow control Display
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("({0}/{1})", thisResource.amount.ToString("#######0.####"), thisResource.maxAmount.ToString("######0.####")), GUILayout.Width(175), GUILayout.Height(20));
            GUILayout.Label(string.Format("{0}", flowtext), GUILayout.Width(30), GUILayout.Height(20));
            if (GUILayout.Button(new GUIContent("Flow", "Enables/Disables flow of selected resource from this part."), GUILayout.Width(50), GUILayout.Height(20)))
            {
                if (flowbool)
                {
                    thisResource.flowState = false;
                    flowtext = "Off";
                }
                else
                {
                    thisResource.flowState = true;
                    flowtext = "On";
                }
            }
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
            {
                Rect rect = GUILayoutUtility.GetLastRect();
                ToolTip = Utilities.SetActiveTooltip(rect, Settings.TransferPosition, GUI.tooltip, ref ToolTipActive, scrollX, scrollY - scrollPosition.y);
            }
            GUILayout.EndHorizontal();

            // Xfer Controls Display
            // let's determine how much of a resource we can move to the target.
            double maxXferAmount = CalcMaxXferAmt(selectedResource, partSource, partTarget);
            if (maxXferAmount > 0 && (partSource != partTarget))
            {
                // Lets parse the string to allow decimal points.
                strXferAmount = thisXferAmount.ToString();

                // add the decimal point if it was typed.
                strXferAmount = GetStringDecimal(strXferAmount, thisXferMode);
                // add the zero if it was typed.
                strXferAmount = GetStringZero(strXferAmount, thisXferMode);
                // Now update the static var
                SetXferAmountString(strXferAmount, thisXferMode);

                GUILayout.BeginHorizontal();
                if (SMAddon.crewXfer || SMAddon.XferOn)
                {
                    // We want to show this during transfer if the direction is correct...
                    if (SMAddon.XferMode == thisXferMode)
                    {
                        GUILayout.Label("Xfer Remaining:", GUILayout.Width(120));
                        GUILayout.Label((thisXferAmount - SMAddon.smController.AmtXferred).ToString(), ManifestStyle.LabelStyleBold, GUILayout.Width(85));
                    }
                }
                else
                {
                    GUILayout.Label("Enter Xfer Amt:", GUILayout.Width(100));
                    strXferAmount = GUILayout.TextField(strXferAmount, 20, GUILayout.Width(105));
                }

                // update decimal bool 
                SetStringDecimal(strXferAmount, thisXferMode);

                //update zero bool 
                SetStringZero(strXferAmount, thisXferMode);

                // Update static Xfer Amount var
                thisXferAmount = UpdateXferAmount(strXferAmount, thisXferMode);

                GUIContent xferContent = null;
                if (!SMAddon.crewXfer && !SMAddon.XferOn)
                    xferContent = new GUIContent("Xfer", "Transfers the selected resource\r\nto the selected " + strTarget + " Part");
                else
                    xferContent = new GUIContent("Stop", "Halts the Transfer of the selected resource\r\nto the selected " + strTarget + " Part");
                if ((!SMAddon.XferOn && maxXferAmount > 0) || (SMAddon.XferOn && SMAddon.XferMode == thisXferMode))
                    if (GUILayout.Button(xferContent, GUILayout.Width(50), GUILayout.Height(20)))
                    {
                        if (!SMAddon.crewXfer && !SMAddon.XferOn)
                            TransferResource(partSource, partTarget, (double)thisXferAmount);
                        else if (SMAddon.XferOn && Settings.RealismMode)
                            SMAddon.XferState = SMAddon.XFERState.Stop;
                    }
                if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                {
                    Rect rect = GUILayoutUtility.GetLastRect();
                    ToolTip = Utilities.SetActiveTooltip(rect, Settings.TransferPosition, GUI.tooltip, ref ToolTipActive, scrollX, scrollY - scrollPosition.y);
                }
                GUILayout.EndHorizontal();
                if (!SMAddon.XferOn)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Xfer:  ", GUILayout.Width(50), GUILayout.Height(20));
                    thisXferAmount = GUILayout.HorizontalSlider(thisXferAmount, 0, (float)maxXferAmount, GUILayout.Width(210));
                    GUILayout.EndHorizontal();
                    UpdateXferAmount(thisXferAmount.ToString(), thisXferMode);
                }
            }
        }

        #endregion

        #endregion

        #region Methods

        internal static double CalcMaxXferAmt(string resource, Part Source, Part Target)
        {
            double maxXferAmount = 0;
            if (Source == null || Target == null || (resource == null || resource == ""))
                maxXferAmount = 0;
            else
            {
                maxXferAmount = Target.Resources[resource].maxAmount - Target.Resources[resource].amount;
                if (maxXferAmount > Source.Resources[resource].amount)
                    maxXferAmount = Source.Resources[resource].amount;
                if (maxXferAmount < 0.0001)
                    maxXferAmount = 0;
            }
            return maxXferAmount;
        }

        internal static double CalcActFlowRate(double XferAmount)
        {
            double actFlowRate = 0;
            double flowTime = XferAmount / Settings.FlowRate;
            if (flowTime > Settings.MaxFlowTimeSec)
                actFlowRate = XferAmount / Settings.MaxFlowTimeSec;
            else 
                actFlowRate = Settings.FlowRate;

            return actFlowRate;
        }

        private static string GetStringDecimal(string strXferAmount, SMAddon.XFERMode thisXferMode)
        {
            if (thisXferMode == SMAddon.XFERMode.SourceToTarget)
            {
                if (SMAddon.smController.sXferAmountHasDecimal)
                    strXferAmount += ".";
            }
            else
            {
                if (SMAddon.smController.tXferAmountHasDecimal)
                    strXferAmount += ".";
            }
            return strXferAmount;
        }

        private static string GetStringZero(string strXferAmount, SMAddon.XFERMode thisXferMode)
        {
            if (thisXferMode == SMAddon.XFERMode.SourceToTarget)
            {
                if (SMAddon.smController.sXferAmountHasZero)
                    strXferAmount += "0";
            }
            else
            {
                if (SMAddon.smController.tXferAmountHasZero)
                    strXferAmount += "0";
            }
            return strXferAmount;
        }

        private static void SetStringZero(string strXferAmount, SMAddon.XFERMode xferMode)
        {
            if (xferMode == SMAddon.XFERMode.SourceToTarget)
            {
                if (strXferAmount.Contains(".") && strXferAmount.EndsWith("0"))
                    SMAddon.smController.sXferAmountHasZero = true;
                else
                    SMAddon.smController.sXferAmountHasZero = false;
            }
            else
            {
                if (strXferAmount.Contains(".") && strXferAmount.EndsWith("0"))
                    SMAddon.smController.tXferAmountHasZero = true;
                else
                    SMAddon.smController.tXferAmountHasZero = false;
            }
        }

        private static void SetStringDecimal(string strXferAmount, SMAddon.XFERMode xferMode)
        {
            if (xferMode == SMAddon.XFERMode.SourceToTarget)
            {
                if (strXferAmount.EndsWith(".") || strXferAmount.EndsWith(".0"))
                    SMAddon.smController.sXferAmountHasDecimal = true;
                else
                    SMAddon.smController.sXferAmountHasDecimal = false;
            }
            else
            {
                if (strXferAmount.EndsWith(".") || strXferAmount.EndsWith(".0"))
                    SMAddon.smController.tXferAmountHasDecimal = true;
                else
                    SMAddon.smController.tXferAmountHasDecimal = false;
            }
        }

        internal static void TransferCrewMemberBegin (ProtoCrewMember crewMember, Part sourcePart, Part targetPart)
        {
            SMAddon.smController.CrewXferSource = sourcePart;
            SMAddon.smController.CrewXferTarget = targetPart;
            SMAddon.smController.CrewXferMember = crewMember;
            SMAddon.crewXfer = true;
        }

        internal static void TransferCrewMemberComplete(ProtoCrewMember crewMember, Part sourcePart, Part targetPart)
        {
            try
            {
                if (sourcePart.internalModel != null && targetPart.internalModel != null)
                {
                    // Build source and target seat indexes.
                    int curIdx = crewMember.seatIdx;
                    int newIdx = curIdx;
                    InternalSeat sourceSeat = crewMember.seat;
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
                        SMController.RemoveCrewMember(crewMember, sourcePart);
                        SMController.RemoveCrewMember(targetMember, targetPart);

                        // Update:  Thanks to Extraplanetary LaunchPads for helping me solve this problem!
                        // Send the kerbal(s) eva.  This is the eva trigger I was looking for
                        // We will fie the board event when we are ready, in the update code.
                        //if (Settings.EnableTextureReplacer)
                        //{
                        //    SMAddon.smController.evaAction = new GameEvents.FromToAction<Part, Part>(sourcePart, targetPart);
                        //    GameEvents.onCrewOnEva.Fire(SMAddon.smController.evaAction);
                        //}

                        // Add the crew members back into the part(s) at their new seats.
                        sourcePart.AddCrewmemberAt(targetMember, curIdx);
                        targetPart.AddCrewmemberAt(crewMember, newIdx);
                    }
                    else
                    {
                        // Just move.
                        SMController.RemoveCrewMember(crewMember, sourcePart);

                        //if (Settings.EnableTextureReplacer)
                        //{                             
                        //    SMAddon.smController.evaAction = new GameEvents.FromToAction<Part, Part>(sourcePart, targetPart);
                        //    GameEvents.onCrewOnEva.Fire(SMAddon.smController.evaAction);
                        //}

                        targetPart.AddCrewmemberAt(crewMember, newIdx);
                    }

                    // if moving within a part, set the seat2seat flag
                    if (sourcePart == targetPart)
                        SMAddon.isSeat2Seat = true;
                    else
                        SMAddon.isSeat2Seat = false;
                }
                else
                {
                    // no portraits, so let's just move kerbals...
                    SMController.RemoveCrewMember(crewMember, sourcePart);
                    SMController.AddCrewMember(crewMember, targetPart);
                }
                SMAddon.smController.RespawnCrew();
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format("Error moving crewmember.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        private static void TransferScience(PartModule source, PartModule target)
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
                    Utilities.LogMessage(string.Format("moduleScience has data..."), "Info", Settings.VerboseLogging);

                    if (((IScienceDataContainer)target) != null)
                    {
                        // Lets store the data from the source.
                        if (((ModuleScienceContainer)target).StoreData( new List<IScienceDataContainer> { (IScienceDataContainer)source }, false))
                        {
                            Utilities.LogMessage(string.Format("((ModuleScienceContainer)source) data stored"), "Info", Settings.VerboseLogging);
                            foreach (ScienceData data in moduleScience)
                            {
                                ((IScienceDataContainer)source).DumpData(data);
                            }

                            if (Settings.RealismMode)
                            {
                                Utilities.LogMessage(string.Format("((Module ScienceExperiment xferred.  Dump Source data"), "Info", Settings.VerboseLogging);
                            }
                            else
                            {
                                Utilities.LogMessage(string.Format("((Module ScienceExperiment xferred.  Dump Source data, reset Experiment"), "Info", Settings.VerboseLogging);
                                ((ModuleScienceExperiment)source).ResetExperiment();
                            }
                        }
                        else
                        {
                            Utilities.LogMessage(string.Format("Science Data transfer failed..."), "Info", Settings.VerboseLogging);
                        }
                    }
                    else
                    {
                        Utilities.LogMessage(string.Format("((IScienceDataContainer)target) is null"), "Info", Settings.VerboseLogging);
                    }
                    Utilities.LogMessage(string.Format("Transfer Complete."), "Info", Settings.VerboseLogging);
                }
                else if (moduleScience == null)
                {
                    Utilities.LogMessage(string.Format("moduleScience is null..."), "Info", Settings.VerboseLogging);
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(" in TransferScience:  Error:  " + ex.ToString(), "Info", Settings.VerboseLogging);
            }
        }

        private static void TransferResource(Part source, Part target, double XferAmount)
        {
            try
            {
                if (source.Resources.Contains(SMAddon.smController.SelectedResource) && target.Resources.Contains(SMAddon.smController.SelectedResource))
                {
                    if (XferAmount <= 0)
                        return;
                    double maxAmount = target.Resources[SMAddon.smController.SelectedResource].maxAmount;
                    double sourceAmount = source.Resources[SMAddon.smController.SelectedResource].amount;
                    double targetAmount = target.Resources[SMAddon.smController.SelectedResource].amount;

                    // make sure we have enough to transfer, or adjust the amount...
                    if (XferAmount > sourceAmount)
                        XferAmount = sourceAmount;

                    if (Settings.RealismMode)
                    {
                        // now lets make some noise and simulate the pumping process...
                        Utilities.LogMessage("Playing pump sound...", "Info", Settings.VerboseLogging);

                        // This flag enables the Update handler in SMAddon and sets the direction
                        if (source == SMAddon.smController.SelectedPartSource)
                            SMAddon.XferMode = SMAddon.XFERMode.SourceToTarget;
                        else
                            SMAddon.XferMode = SMAddon.XFERMode.TargetToSource;

                        // Calculate the actual flow rate, based on source capacity and max flow time setting...
                        SMAddon.act_flow_rate = CalcActFlowRate(source.Resources[SMAddon.smController.SelectedResource].maxAmount);

                        // Start the process
                        SMAddon.XferOn = true;
                    }
                    else
                    {
                        //Not in Realism mode, so just move the resource...

                        // Fill target
                        target.Resources[SMAddon.smController.SelectedResource].amount += XferAmount;

                        // Drain source...
                        source.Resources[SMAddon.smController.SelectedResource].amount -= XferAmount;
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in  TransferResource.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        private static void SetXferAmountString(string xferAmount, SMAddon.XFERMode xferMode)
        {
            if (xferMode == SMAddon.XFERMode.TargetToSource)
                strSXferAmount = xferAmount;
            else
                strTXferAmount = xferAmount;
        }

        private static float UpdateXferAmount(string strXferAmount, SMAddon.XFERMode xferMode)
        {
            float newAmount = 0;
            if (float.TryParse(strXferAmount, out newAmount))
                if (xferMode == SMAddon.XFERMode.SourceToTarget)
                    SMAddon.smController.sXferAmount = newAmount;
                else
                    SMAddon.smController.tXferAmount = newAmount;
            return newAmount;
        }

        private static int GetScienceCount(Part part, bool IsCapacity)
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
                Utilities.LogMessage(string.Format(" in GetScienceCount.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                return 0;
            }
        }

        #endregion
    }
}
