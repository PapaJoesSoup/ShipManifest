using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using HighlightingSystem;
using ConnectedLivingSpace;

namespace ShipManifest
{
    internal static class WindowTransfer
    {
        #region Properties

        internal static Rect Position = new Rect(0, 0, 0, 0);
        internal static bool ShowWindow = false;
        internal static bool ToolTipActive = false;
        internal static bool ShowToolTips = true;

        internal static string ToolTip = "";
        internal static string xferToolTip = "";

        #endregion

        #region TransferWindow (GUI Layout)

        // Resource Transfer Window
        // This window allows you some control over the selected resource on a selected source and target part
        // This window assumes that a resource has been selected on the Ship manifest window.
        internal static void Display(int windowId)
        {
            // Reset Tooltip active flag...
            ToolTipActive = false;

            GUIContent label = new GUIContent("", "Close Window");
            if (SMAddon.smController.CrewTransfer.CrewXferActive || TransferResource.ResourceXferActive)
            {
                label = new GUIContent("", "Action in progress.  Cannot close window");
                GUI.enabled = false;
            }
            Rect rect = new Rect(Position.width - 20, 4, 16, 16);
            if (GUI.Button(rect, label))
            {
                WindowTransfer.ShowWindow = false;
                SMHighlighter.ClearPartsHighlight(SMAddon.smController.SelectedPartsSource);
                SMHighlighter.ClearPartsHighlight(SMAddon.smController.SelectedPartsTarget);
                SMAddon.smController.SelectedResources.Clear();
                SMAddon.smController.SelectedPartsSource.Clear();
                SMAddon.smController.SelectedPartsTarget.Clear();
                ToolTip = "";
                return;
            }
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, WindowTransfer.Position, GUI.tooltip, ref ToolTipActive, 0, 0);
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
                TextBetweenViewers(SMAddon.smController.SelectedPartsSource);

                // Build Details ScrollViewer
                SourceDetailsViewer();

                // Okay, we are done with the left column of the dialog...
                GUILayout.EndVertical();

                // Right Column Begins...
                GUILayout.BeginVertical();

                // Build Target Transfer Viewer
                TargetTransferViewer();

                // Text between viewers
                TextBetweenViewers(SMAddon.smController.SelectedPartsTarget);

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

        private static void TextBetweenViewers(List<Part> SelectedParts)
        {
            if (SMAddon.smController.SelectedResources.Contains("Crew") && SMSettings.ShowIVAUpdateBtn)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(SelectedParts != null ? string.Format("{0}", SelectedParts[0].partInfo.title) : "No Part Selected", GUILayout.Width(190), GUILayout.Height(20));
                if (GUILayout.Button("Update Portraits", SMStyle.ButtonStyle, GUILayout.Width(110), GUILayout.Height(20)))
                {
                    SMAddon.smController.RespawnCrew();
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                if (SelectedParts != null)
                {
                    if (SelectedParts.Count > 1)
                    {
                        GUILayout.Label(string.Format("{0}", "Multiple Parts Selected"), GUILayout.Width(300), GUILayout.Height(20));
                    }
                    else if (SelectedParts.Count == 1)
                        GUILayout.Label(string.Format("{0}", SelectedParts[0].partInfo.title), GUILayout.Width(300), GUILayout.Height(20));
                    else
                        GUILayout.Label(string.Format("{0}", "No Part Selected"), GUILayout.Width(300), GUILayout.Height(20));
                }
            }
        }

        #region Source Viewers (GUI Layout)
        // Transfer Window components
        private static Vector2 SourceTransferViewerScrollPosition = Vector2.zero;
        internal static void SourceTransferViewer()
        {
            try
            {
                // This is a scroll panel (we are using it to make button lists...)
                SourceTransferViewerScrollPosition = GUILayout.BeginScrollView(SourceTransferViewerScrollPosition, GUILayout.Height(125), GUILayout.Width(300));
                GUILayout.BeginVertical();

                TransferViewer(SMAddon.smController.SelectedResources, SMAddon.XFERMode.SourceToTarget, SourceTransferViewerScrollPosition);

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

                if (SMAddon.smController.SelectedResources.Contains("Crew"))
                {
                    CrewDetails(SMAddon.smController.SelectedPartsSource, SMAddon.smController.SelectedPartsTarget, SMAddon.XFERMode.SourceToTarget, SourceDetailsViewerScrollPosition);
                }
                else if (SMAddon.smController.SelectedResources.Contains("Science"))
                {
                    ScienceDetailsSource();
                }
                else
                {
                    // resources are left....
                    ResourceDetails(SMAddon.smController.SelectedPartsSource, SMAddon.smController.SelectedPartsTarget, SMAddon.XFERMode.SourceToTarget, SourceDetailsViewerScrollPosition);
                }
                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in WindowTransfer.SourceDetailsViewer.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        #endregion

        #region Target Viewers (GUI Layout)

        private static Vector2 TargetTransferViewerScrollPosition = Vector2.zero;
        private static void TargetTransferViewer()
        {
            try
            {
                // Adjust target style colors for part selectors when using/not using CLS highlighting
                if (SMSettings.EnableCLSHighlighting && SMAddon.smController.SelectedResources.Contains("Crew"))
                    SMStyle.ButtonToggledTargetStyle.normal.textColor = SMSettings.Colors[SMSettings.TargetPartCrewColor];
                else
                    SMStyle.ButtonToggledTargetStyle.normal.textColor = SMSettings.Colors[SMSettings.TargetPartColor];

                // This is a scroll panel (we are using it to make button lists...)
                TargetTransferViewerScrollPosition = GUILayout.BeginScrollView(TargetTransferViewerScrollPosition, GUILayout.Height(125), GUILayout.Width(300));
                GUILayout.BeginVertical();

                TransferViewer(SMAddon.smController.SelectedResources, SMAddon.XFERMode.TargetToSource, TargetTransferViewerScrollPosition);

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
                if (SMAddon.smController.SelectedResources.Contains("Crew"))
                {
                    CrewDetails(SMAddon.smController.SelectedPartsTarget, SMAddon.smController.SelectedPartsSource, SMAddon.XFERMode.TargetToSource, TargetDetailsViewerScrollPosition);
                }
                else if (SMAddon.smController.SelectedResources.Contains("Science"))
                {
                    ScienceDetailsTarget();
                }
                else
                {
                    ResourceDetails(SMAddon.smController.SelectedPartsTarget, SMAddon.smController.SelectedPartsSource, SMAddon.XFERMode.TargetToSource, TargetDetailsViewerScrollPosition);
                }
                // --------------------------------------------------------------------------
                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in WindowTransfer.TargetDetailsViewer.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }
        #endregion

        #region Viewer Details (GUI Layout)

        private static void TransferViewer(List<string> SelectedResources, SMAddon.XFERMode xferMode, Vector2 ViewerScrollPosition)
        {
            float scrollX = WindowTransfer.Position.x + 20;
            if (xferMode == SMAddon.XFERMode.TargetToSource)
                scrollX = WindowTransfer.Position.x + 320;
            float scrollY = WindowTransfer.Position.y + 30 - ViewerScrollPosition.y;
            string step = "begin";
            try
            {
                step = "begin button loop";
                foreach (Part part in SMAddon.smController.SelectedResourcesParts)
                {
                    // Build the part button title...
                    step = "part button title";
                    string strDescription = GetResourceDescription(SelectedResources, part);

                    // set the conditions for a button style change.
                    int btnWidth = 265;
                    if (!SMSettings.RealismMode && !SelectedResources.Contains("Crew") && !SelectedResources.Contains("Science"))
                        btnWidth = 180;

                    // Set style based on viewer and toggled state.
                    step = "Set style";
                    GUIStyle style = GetPartButtonStyle(xferMode, part);

                    GUILayout.BeginHorizontal();

                    // Now let's account for any target buttons already pressed. (sources and targets for resources cannot be the same)
                    GUI.enabled = IsPartSelectable(SelectedResources[0], xferMode, part);

                    step = "Render part Buttons";
                    if (GUILayout.Button(string.Format("{0}", strDescription), style, GUILayout.Width(btnWidth), GUILayout.Height(20)))
                    {
                        PartButtonToggled(xferMode, part);
                    }
                    Rect rect = GUILayoutUtility.GetLastRect();
                    if (Event.current.type == EventType.Repaint && rect.Contains(Event.current.mousePosition))
                    {
                        SMHighlighter.isMouseOver = true;
                        SMHighlighter.MouseOverMode = xferMode;
                        SMHighlighter.MouseOverRect = new Rect(scrollX + rect.x, scrollY + rect.y, rect.width, rect.height);
                        SMHighlighter.MouseOverpart = part;
                    }

                    // Reset Button enabling.
                    GUI.enabled = true;

                    step = "Render dump/fill buttons";
                    if (!SMSettings.RealismMode && SelectedResources[0] != "Crew" && SelectedResources[0] != "Science")
                    {
                        if (SelectedResources.Count > 1)
                            GUI.enabled = (part.Resources[SelectedResources[0]].amount > 0 && part.Resources[SelectedResources[1]].amount > 0) ? true : false;
                        else
                            GUI.enabled = part.Resources[SelectedResources[0]].amount > 0 ? true : false;
                        var style1 = xferMode == SMAddon.XFERMode.SourceToTarget ? SMStyle.ButtonSourceStyle : SMStyle.ButtonTargetStyle;
                        if (GUILayout.Button(string.Format("{0}", "Dump"), style1, GUILayout.Width(45), GUILayout.Height(20)))
                        {
                            SMController.DumpPartResource(part, SelectedResources[0]);
                            if (SelectedResources.Count > 1)
                                SMController.DumpPartResource(part, SelectedResources[1]);
                        }

                        var style2 = xferMode == SMAddon.XFERMode.SourceToTarget ? SMStyle.ButtonSourceStyle : SMStyle.ButtonTargetStyle;
                        if (SelectedResources.Count > 1)
                            GUI.enabled = part.Resources[SelectedResources[0]].amount < part.Resources[SelectedResources[0]].maxAmount && part.Resources[SelectedResources[1]].amount < part.Resources[SelectedResources[1]].maxAmount ? true : false;
                        else
                            GUI.enabled = part.Resources[SelectedResources[0]].amount < part.Resources[SelectedResources[0]].maxAmount ? true : false;
                        if (GUILayout.Button(string.Format("{0}", "Fill"), style2, GUILayout.Width(30), GUILayout.Height(20)))
                        {
                            SMController.FillPartResource(part, SelectedResources[0]);
                            if (SelectedResources.Count > 1)
                                SMController.FillPartResource(part, SelectedResources[1]);
                        }
                        GUI.enabled = true;
                    }
                    GUILayout.EndHorizontal();
                }
            }
            catch (Exception ex)
            {
                if (!SMAddon.frameErrTripped)
                {
                    Utilities.LogMessage("Error in Windowtransfer.TransferViewer (" + xferMode.ToString() + ") at step:  " + step + ".  Error:  " + ex.ToString(), "Error", true);
                    SMAddon.frameErrTripped = true;
                }
            }
        }

        private static void CrewDetails(List<Part> SelectedPartsSource, List<Part> SelectedPartsTarget, SMAddon.XFERMode xferMode, Vector2 scrollPosition)
        {
            // Since only one Crew Part can be selected, all lists will use an index of [0].
            int scrollX = 20;
            if (xferMode == SMAddon.XFERMode.TargetToSource)
                scrollX = 320;

            if (SelectedPartsSource.Count > 0)
            {
                List<ProtoCrewMember> crewMembers = SelectedPartsSource[0].protoModuleCrew;
                for (int x = 0; x < SelectedPartsSource[0].protoModuleCrew.Count(); x++)
                {
                    ProtoCrewMember crewMember = SelectedPartsSource[0].protoModuleCrew[x];
                    GUILayout.BeginHorizontal();
                    if (crewMember.seat != null)
                    {
                        if (SMAddon.smController.CrewTransfer.CrewXferActive || TransferResource.ResourceXferActive)
                            GUI.enabled = false;

                        if (GUILayout.Button(new GUIContent(">>", "Move Kerbal to another seat within Part"), SMStyle.ButtonStyle, GUILayout.Width(15), GUILayout.Height(20)))
                        {
                            ToolTip = "";
                            SMAddon.smController.CrewTransfer.CrewTransferBegin(crewMember, SelectedPartsSource[0], SelectedPartsSource[0]);
                        }
                        if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                        {
                            Rect rect = GUILayoutUtility.GetLastRect();
                            ToolTip = Utilities.SetActiveTooltip(rect, WindowTransfer.Position, GUI.tooltip, ref ToolTipActive, scrollX, 190 - scrollPosition.y);
                        }
                        GUI.enabled = true;
                    }
                    GUILayout.Label(string.Format("  {0}", crewMember.name + " (" + crewMember.experienceTrait.Title + ")"), GUILayout.Width(190), GUILayout.Height(20));
                    if (SMAddon.CanKerbalsBeXferred(SelectedPartsSource, SelectedPartsTarget))
                        GUI.enabled = true;
                    else
                        GUI.enabled = false;
                    if ((SMAddon.smController.CrewTransfer.FromCrewMember == crewMember || SMAddon.smController.CrewTransfer.ToCrewMember == crewMember) && (SMAddon.smController.CrewTransfer.CrewXferActive || TransferResource.ResourceXferActive))
                    {
                        GUI.enabled = true;
                        GUILayout.Label("Moving", GUILayout.Width(50), GUILayout.Height(20));
                    }
                    else
                    {
                        if (GUILayout.Button(new GUIContent("Xfer", xferToolTip), SMStyle.ButtonStyle, GUILayout.Width(50), GUILayout.Height(20)))
                        {
                            SMAddon.smController.CrewTransfer.FromCrewMember = crewMember;
                            SMAddon.smController.CrewTransfer.CrewTransferBegin(crewMember, SelectedPartsSource[0], SelectedPartsTarget[0]);
                        }
                        if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                        {
                            Rect rect = GUILayoutUtility.GetLastRect();
                            ToolTip = Utilities.SetActiveTooltip(rect, WindowTransfer.Position, GUI.tooltip, ref ToolTipActive, scrollX, 190 - scrollPosition.y);
                        }
                    }
                    GUI.enabled = true;
                    GUILayout.EndHorizontal();
                }
            }
        }

        private static void ScienceDetailsSource()
        {
            if (SMAddon.smController.SelectedPartsSource.Count > 0)
            {
                IScienceDataContainer[] modules = SMAddon.smController.SelectedPartsSource[0].FindModulesImplementing<IScienceDataContainer>().ToArray();
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
                    if (SMAddon.smController.SelectedModuleTarget != null && scienceCount > 0)
                    {
                        string toolTip = "";
                        if (SMSettings.RealismMode && !isCollectable)
                        {
                            GUI.enabled = false;
                            toolTip = "Realism Mode is preventing transfer.\r\nExperiment/data is marked not transferable";
                        }
                        else
                        {
                            toolTip = "Realism is off, or Experiment/data is transferable";
                            GUI.enabled = true;
                        }

                        if (GUILayout.Button(new GUIContent("Xfer", toolTip), SMStyle.ButtonStyle, GUILayout.Width(50), GUILayout.Height(20)))
                        {
                            SMAddon.smController.SelectedModuleSource = pm;
                            TransferScience(SMAddon.smController.SelectedModuleSource, SMAddon.smController.SelectedModuleTarget);
                            SMAddon.smController.SelectedModuleSource = null;
                        }
                        if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                        {
                            Rect rect = GUILayoutUtility.GetLastRect();
                            ToolTip = Utilities.SetActiveTooltip(rect, WindowTransfer.Position, GUI.tooltip, ref ToolTipActive, 20, 190 - TargetDetailsViewerScrollPosition.y);
                        }
                    }
                    GUILayout.EndHorizontal();
                    GUI.enabled = true;
                }
            }
        }

        private static void ScienceDetailsTarget()
        {
            if (SMAddon.smController.SelectedPartsTarget.Count > 0)
            {
                int count = 0;
                foreach (PartModule tpm in SMAddon.smController.SelectedPartsTarget[0].Modules)
                {
                    if (tpm is IScienceDataContainer && tpm.moduleName != "ModuleScienceExperiment")
                        count += 1;
                }

                foreach (PartModule pm in SMAddon.smController.SelectedPartsTarget[0].Modules)
                {
                    // Containers.
                    int scienceCount = 0;
                    if (pm is IScienceDataContainer && pm.moduleName != "ModuleScienceExperiment")
                    {
                        scienceCount = ((IScienceDataContainer)pm).GetScienceCount();
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(string.Format("{0} - ({1})", pm.moduleName, scienceCount.ToString()), GUILayout.Width(205), GUILayout.Height(20));
                        // set the conditions for a button style change.
                        bool isReceiveToggled = false;
                        if (pm == SMAddon.smController.SelectedModuleTarget)
                            isReceiveToggled = true;
                        else if (count == 1)
                        {
                            SMAddon.smController.SelectedModuleTarget = pm;
                            isReceiveToggled = true;
                        }
                        //SelectedModuleTarget = pm;
                        var style = isReceiveToggled ? SMStyle.ButtonToggledTargetStyle : SMStyle.ButtonStyle;

                        // Only containers can receive science data
                        if (pm.moduleName != "ModuleScienceExperiment")
                        {
                            if (GUILayout.Button(new GUIContent("Recv", "Set this module as the receiving container"), style, GUILayout.Width(50), GUILayout.Height(20)))
                            {
                                SMAddon.smController.SelectedModuleTarget = pm;
                            }
                            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                            {
                                Rect rect = GUILayoutUtility.GetLastRect();
                                ToolTip = Utilities.SetActiveTooltip(rect, WindowTransfer.Position, GUI.tooltip, ref ToolTipActive, 300, 190 - TargetDetailsViewerScrollPosition.y);
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                }
            }
        }

        private static void ResourceDetails(List<Part> partsSource, List<Part> partsTarget, SMAddon.XFERMode XferMode, Vector2 scrollPosition)
        {
            // This routine assumes that a resource has been selected on the Resource manifest window.
            // Pass in static vars to improve readability.
            List<string> selectedResources = SMAddon.smController.SelectedResources;
            List<TransferResource> XferResources = SMAddon.smController.ResourcesToXfer;

            if (partsSource.Count > 0)
            {
                TransferResource modResource = TransferResource.GetXferResource(XferResources, XferMode, false);
                TransferResource ratioResource = TransferResource.GetXferResource(XferResources, XferMode, true);
                modResource.XferRatio = 1;
                ratioResource.XferRatio = TransferResource.CalcRatio(XferResources, XferMode);

                double thisXferAmount = modResource.XferAmount(XferMode);
                double ratioXferAmt = ratioResource.XferAmount(XferMode);
                
                string strXferAmount = modResource.XferAmount(XferMode).ToString();

                // Set tooltips directional data
                string strTarget = XferMode == SMAddon.XFERMode.SourceToTarget ? "Target" : "Source";

                // Set scrollX offsets for left and right viewers
                int scrollX = XferMode == SMAddon.XFERMode.SourceToTarget ? 30 : 330;
                int scrollY = 180;

                // Resource Flow control Display loop
                ResourceFlowButtons(partsSource, XferMode, scrollPosition, scrollX, scrollY);

                // Xfer Controls Display
                // let's determine how much of a resource we can move to the target.
                double maxXferAmount = TransferResource.CalcMaxXferAmt(partsSource, partsTarget, selectedResources);
                if (maxXferAmount > 0)
                {
                    GUILayout.BeginHorizontal();
                    if (TransferResource.ResourceXferActive)
                    {
                        // We want to show this during transfer if the direction is correct...
                        if (SMAddon.XferMode == XferMode)
                        {
                            GUILayout.Label("Xfer Remaining:", GUILayout.Width(120));
                            GUILayout.Label((modResource.XferAmount(SMAddon.XferMode) - modResource.AmtXferred).ToString("#######0.##"));
                            GUILayout.Label(" / " + (ratioResource.XferAmount(SMAddon.XferMode) - ratioResource.AmtXferred).ToString("#######0.##"));
                        }
                    }
                    else
                    {
                        // Lets parse the string to allow decimal points.
                        strXferAmount = modResource.XferAmount(XferMode).ToString();
                        // add the decimal point if it was typed.
                        strXferAmount = modResource.GetStringDecimal(strXferAmount, XferMode);
                        // add the zero if it was typed.
                        strXferAmount = modResource.GetStringZero(strXferAmount, XferMode);

                        // Now update the static var
                        modResource.SetXferAmountString(strXferAmount, XferMode);

                        GUILayout.Label("Xfer Amt:", GUILayout.Width(60));
                        strXferAmount = GUILayout.TextField(strXferAmount, 20, GUILayout.Width(100), GUILayout.Height(20));
                        // update decimal bool 
                        modResource.SetStringDecimal(strXferAmount, XferMode);
                        //update zero bool 
                        modResource.SetStringZero(strXferAmount, XferMode);
                        // Update static Xfer Amount var
                        thisXferAmount = modResource.UpdateXferAmount(strXferAmount, XferMode);
                        ratioXferAmt = thisXferAmount * ratioResource.XferRatio > ratioResource.FromCapacity(XferMode) ? ratioResource.FromCapacity(XferMode) : thisXferAmount * ratioResource.XferRatio;
                        GUILayout.Label(" / " + ratioXferAmt.ToString("#######0.##"), GUILayout.Width(80));
                    }
                    GUILayout.EndHorizontal();

                    if (IsShipControllable() && CanResourceBeXferred(XferMode, maxXferAmount))
                    {
                        GUILayout.BeginHorizontal();
                        GUIStyle noPad = SMStyle.LabelStyleNoPad;
                        GUILayout.Label("Xfer:", noPad, GUILayout.Width(50), GUILayout.Height(20));
                        thisXferAmount = (double)GUILayout.HorizontalSlider((float)thisXferAmount, 0, (float)maxXferAmount, GUILayout.Width(180));

                        // set button style
                        GUIContent xferContent = null;
                        if (!TransferResource.ResourceXferActive)
                            xferContent = new GUIContent("Xfer", "Transfers the selected resource\r\nto the selected " + strTarget + " Part");
                        else
                            xferContent = new GUIContent("Stop", "Halts the Transfer of the selected resource\r\nto the selected " + strTarget + " Part");

                        if (GUILayout.Button(xferContent, GUILayout.Width(40), GUILayout.Height(18)))
                        {
                            if (!TransferResource.ResourceXferActive)
                            {
                                //Calc amounts and update xfer modules
                                AssignXferAmounts(XferResources, XferMode, thisXferAmount);
                                TransferResources(partsSource, partsTarget);

                            }
                            else if (TransferResource.ResourceXferActive && SMSettings.RealismMode)
                                TransferResource.XferState = TransferResource.ResourceXFERState.Stop;
                        }
                        if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                        {
                            Rect rect = GUILayoutUtility.GetLastRect();
                            ToolTip = Utilities.SetActiveTooltip(rect, WindowTransfer.Position, GUI.tooltip, ref ToolTipActive, scrollX, scrollY - scrollPosition.y);
                        }
                        GUILayout.EndHorizontal();
                    }
                    if (!TransferResource.ResourceXferActive)
                        modResource.UpdateXferAmount(thisXferAmount.ToString(), XferMode);
                }
            }
        }

        private static void AssignXferAmounts(List<TransferResource> XferResources, SMAddon.XFERMode XferMode, double thisXferAmount)
        {
            if (XferResources.Count > 1)
            {
                // Calculate Ratio and transfer amounts.  Ratio is based off the largest amount to move, so will always be less than 1.
                double ratio = TransferResource.CalcRatio(XferResources, XferMode);

                if (XferResources[0].ToCapacity(XferMode) > XferResources[1].ToCapacity(XferMode))
                {
                    XferResources[0].XferRatio = 1;
                    XferResources[1].XferRatio = ratio;
                    if (XferMode == SMAddon.XFERMode.SourceToTarget)
                    {
                        XferResources[0].srcXferAmount = thisXferAmount;
                        XferResources[1].srcXferAmount = thisXferAmount * ratio <= XferResources[1].FromCapacity(XferMode) ? thisXferAmount * ratio : XferResources[1].FromCapacity(XferMode);
                    }
                    else
                    {
                        XferResources[0].tgtXferAmount = thisXferAmount;
                        XferResources[1].tgtXferAmount = thisXferAmount * ratio <= XferResources[1].FromCapacity(XferMode) ? thisXferAmount * ratio : XferResources[1].FromCapacity(XferMode);
                    }
                }
                else
                {
                    XferResources[1].XferRatio = 1;
                    XferResources[0].XferRatio = ratio;
                    if (XferMode == SMAddon.XFERMode.SourceToTarget)
                    {
                        XferResources[1].srcXferAmount = thisXferAmount;
                        XferResources[0].srcXferAmount = thisXferAmount * ratio <= XferResources[0].FromCapacity(XferMode) ? thisXferAmount * ratio : XferResources[0].FromCapacity(XferMode);
                    }
                    else
                    {
                        XferResources[1].tgtXferAmount = thisXferAmount;
                        XferResources[0].tgtXferAmount = thisXferAmount * ratio <= XferResources[0].FromCapacity(XferMode) ? thisXferAmount * ratio : XferResources[0].FromCapacity(XferMode);
                    }
                }
            }
            else
            {
                XferResources[0].XferRatio = 1;
                if (XferMode == SMAddon.XFERMode.SourceToTarget)
                    XferResources[0].srcXferAmount = thisXferAmount;
                else
                    XferResources[0].tgtXferAmount = thisXferAmount;                         
            }
        }

        private static void ResourceFlowButtons(List<Part> partsSource, SMAddon.XFERMode XferMode, Vector2 scrollPosition, int scrollX, int scrollY)
        {
            string step = "";
            try
            {
                foreach (TransferResource modResource in SMAddon.smController.ResourcesToXfer)
                {
                    string resource = modResource.ResourceName;

                    // this var is used for button state change management
                    bool flowState = true;
                    // Loop through parts to establish flow state.   Any part that is off, means all are off for display purposes.
                    if (partsSource.Count > 0)
                    {
                        step = "We have parts.  Get flowstate";
                        foreach (Part part in partsSource)
                        {
                            if (!part.Resources[resource].flowState)
                                flowState = false;
                        }
                        string flowtext = flowState ? "On" : "Off";
                        

                        // Flow control Display
                        step = "resource quantities labels";

                        GUILayout.BeginHorizontal();

                        GUIStyle noWrap = SMStyle.LabelStyleNoWrap;
                        GUILayout.Label(string.Format("{0}: ({1}/{2})", resource, modResource.FromAmtRemaining(XferMode).ToString("#######0.##"), modResource.FromCapacity(XferMode).ToString("######0.##")), noWrap, GUILayout.Width(210), GUILayout.Height(18));
                        GUILayout.Label(string.Format("{0}", flowtext), GUILayout.Width(20), GUILayout.Height(18));
                        if (SMAddon.vessel.IsControllable)
                        {
                            step = "render flow button(s)";
                            if (GUILayout.Button(new GUIContent("Flow", "Enables/Disables flow of selected resource(s) from selected part(s)."), GUILayout.Width(40), GUILayout.Height(18)))
                            {
                                foreach (Part part in partsSource)
                                {
                                    part.Resources[resource].flowState = !flowState;
                                }
                                flowtext = flowState ? "Off" : "On";
                            }
                            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                            {
                                Rect rect = GUILayoutUtility.GetLastRect();
                                ToolTip = Utilities.SetActiveTooltip(rect, WindowTransfer.Position, GUI.tooltip, ref ToolTipActive, scrollX, scrollY - scrollPosition.y);
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                }
            }
            catch (Exception ex)
            {
                if (!SMAddon.frameErrTripped)
                {
                    Utilities.LogMessage(string.Format(" in WindowTransfer.ResourceFlowButtons at step:  " + step + ".  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                    SMAddon.frameErrTripped = true;
                }
            }
        }

        #endregion

        #endregion

        #region Methods

        private static void PartButtonToggled(SMAddon.XFERMode xferMode, Part part)
        {
            string step = "Part Button Toggled";
            try
            {
                if (!SMAddon.smController.CrewTransfer.CrewXferActive && !TransferResource.ResourceXferActive)
                {
                    if (xferMode == SMAddon.XFERMode.SourceToTarget)
                    {
                        // first, lets clear any highlighting...
                        SMHighlighter.ClearPartsHighlight(SMAddon.smController.SelectedPartsSource);

                        // Now lets update the list...
                        if (SMAddon.smController.SelectedPartsSource.Contains(part))
                        {
                            SMAddon.smController.SelectedPartsSource.Remove(part);
                        }
                        else
                        {
                            if (SMAddon.smController.SelectedResources.Contains("Crew") || SMAddon.smController.SelectedResources.Contains("Science"))
                                SMAddon.smController.SelectedPartsSource.Clear();
                            SMAddon.smController.SelectedPartsSource.Add(part);
                        }
                        if (SMAddon.smController.SelectedResources.Contains("Crew") && SMSettings.EnableCLS)
                        {
                            SMAddon.UpdateCLSSpaces();
                        }
                        SMAddon.smController.SelectedModuleSource = null;
                    }
                    else
                    {
                        // first, lets clear any highlighting...
                        SMHighlighter.ClearPartsHighlight(SMAddon.smController.SelectedPartsTarget);
                        if (SMAddon.smController.SelectedPartsTarget.Contains(part))
                        {
                            SMAddon.smController.SelectedPartsTarget.Remove(part);
                        }
                        else
                        {
                            if (SMAddon.smController.SelectedResources.Contains("Crew") || SMAddon.smController.SelectedResources.Contains("Science"))
                                SMAddon.smController.SelectedPartsTarget.Clear();
                            SMAddon.smController.SelectedPartsTarget.Add(part);
                        }
                        SMAddon.smController.SelectedModuleTarget = null;
                    }
                    step = "Set Xfer amounts?";
                    if (!SMAddon.smController.SelectedResources.Contains("Crew") && !SMAddon.smController.SelectedResources.Contains("Science"))
                    {
                        step = "Set Xfer amounts = yes";
                        foreach (TransferResource modResource in SMAddon.smController.ResourcesToXfer)
                        {
                            modResource.srcXferAmount = TransferResource.CalcMaxResourceXferAmt(SMAddon.smController.SelectedPartsSource, SMAddon.smController.SelectedPartsTarget, modResource.ResourceName);
                            modResource.tgtXferAmount = TransferResource.CalcMaxResourceXferAmt(SMAddon.smController.SelectedPartsTarget, SMAddon.smController.SelectedPartsSource, modResource.ResourceName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (!SMAddon.frameErrTripped)
                {
                    Utilities.LogMessage("Error in WindowTransfer.PartButtonToggled (" + xferMode.ToString() + ") at step:  " + step + ".  Error:  " + ex.ToString(), "Error", true);
                    SMAddon.frameErrTripped = true;
                }
            }
        }

        private static void TransferScience(PartModule source, PartModule target)
        {
            ScienceData[] moduleScience = null;
            try
            {
                moduleScience = ((IScienceDataContainer)source) != null ? ((IScienceDataContainer)source).GetData() : null;

                if (moduleScience != null && moduleScience.Length > 0)
                {
                    Utilities.LogMessage(string.Format("moduleScience has data..."), "Info", SMSettings.VerboseLogging);

                    if (((IScienceDataContainer)target) != null)
                    {
                        // Lets store the data from the source.
                        if (((ModuleScienceContainer)target).StoreData( new List<IScienceDataContainer> { (IScienceDataContainer)source }, false))
                        {
                            Utilities.LogMessage(string.Format("((ModuleScienceContainer)source) data stored"), "Info", SMSettings.VerboseLogging);
                            foreach (ScienceData data in moduleScience)
                            {
                                ((IScienceDataContainer)source).DumpData(data);
                            }

                            if (SMSettings.RealismMode)
                            {
                                Utilities.LogMessage(string.Format("((Module ScienceExperiment xferred.  Dump Source data"), "Info", SMSettings.VerboseLogging);
                            }
                            else
                            {
                                Utilities.LogMessage(string.Format("((Module ScienceExperiment xferred.  Dump Source data, reset Experiment"), "Info", SMSettings.VerboseLogging);
                                ((ModuleScienceExperiment)source).ResetExperiment();
                            }
                        }
                        else
                        {
                            Utilities.LogMessage(string.Format("Science Data transfer failed..."), "Info", SMSettings.VerboseLogging);
                        }
                    }
                    else
                    {
                        Utilities.LogMessage(string.Format("((IScienceDataContainer)target) is null"), "Info", SMSettings.VerboseLogging);
                    }
                    Utilities.LogMessage(string.Format("Transfer Complete."), "Info", SMSettings.VerboseLogging);
                }
                else if (moduleScience == null)
                {
                    Utilities.LogMessage(string.Format("moduleScience is null..."), "Info", SMSettings.VerboseLogging);
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(" in TransferScience:  Error:  " + ex.ToString(), "Info", SMSettings.VerboseLogging);
            }
        }

        private static void TransferResources(List<Part> source, List<Part> target)
        {
            try
            {
                // Create Xfer Objects for timed process...
                List<TransferResource> XferResources = SMAddon.smController.ResourcesToXfer;

                if (SMSettings.RealismMode)
                {
                    // This flag enables the Update handler in SMAddon and sets the direction
                    SMAddon.XferMode = source == SMAddon.smController.SelectedPartsSource ? SMAddon.XFERMode.SourceToTarget : SMAddon.XFERMode.TargetToSource;

                    // let's get the capacity of the source for flow calculations.
                    // Flow is based on the largest resource capacity
                    double AmtCapacity = XferResources[0].FromCapacity(SMAddon.XferMode);
                    if (XferResources.Count == 2)
                        if (XferResources[1].FromCapacity(SMAddon.XferMode) > AmtCapacity)
                            AmtCapacity = XferResources[1].FromCapacity(SMAddon.XferMode);

                    // Calculate the actual flow rate, based on source capacity and max flow time setting...
                    TransferResource.act_flow_rate = AmtCapacity / SMSettings.FlowRate > SMSettings.MaxFlowTimeSec ? AmtCapacity / SMSettings.MaxFlowTimeSec : SMSettings.FlowRate;

                    // now lets make some noise and simulate the pumping process...
                    Utilities.LogMessage("Playing pump sound...", "Info", SMSettings.VerboseLogging);

                    // Start the process
                    TransferResource.ResourceXferActive = true;
                }
                else
                {
                    //Not in Realism mode, so just move the resource...
                    foreach (TransferResource modResource in XferResources)
                    {
                        TransferResource.XferResource(source, modResource, modResource.XferAmount(SMAddon.XferMode), SMAddon.XferMode, true);
                        TransferResource.XferResource(target, modResource, modResource.XferAmount(SMAddon.XferMode), SMAddon.XferMode, false);
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in  TransferResource.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        #endregion

        #region Utilities

        private static bool CanResourceBeXferred(SMAddon.XFERMode thisXferMode, double maxXferAmount)
        {
            return ((!TransferResource.ResourceXferActive && maxXferAmount > 0) || (TransferResource.ResourceXferActive && SMAddon.XferMode == thisXferMode));
        }

        internal static bool IsShipControllable()
        {
            return ((SMAddon.vessel.IsControllable && SMSettings.RealismMode) || !SMSettings.RealismMode);
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

        private static bool IsPartSelectable(string SelectedResource, SMAddon.XFERMode xferMode, Part part)
        {
            bool isSelectable = true;
            if (SelectedResource != "Crew" && SelectedResource != "Science")
            {
                if (xferMode == SMAddon.XFERMode.SourceToTarget)
                {
                    if (SMAddon.smController.SelectedPartsTarget.Contains(part))
                        isSelectable = false;
                }
                else
                {
                    if (SMAddon.smController.SelectedPartsSource.Contains(part))
                        isSelectable = false;
                }
            }
            return isSelectable;
        }

        private static GUIStyle GetPartButtonStyle(SMAddon.XFERMode xferMode, Part part)
        {
            GUIStyle style = SMStyle.ButtonSourceStyle;
            if (xferMode == SMAddon.XFERMode.SourceToTarget)
            {
                if (SMAddon.smController.SelectedPartsSource.Contains(part))
                    style = SMStyle.ButtonToggledSourceStyle;
                else
                    style = SMStyle.ButtonSourceStyle;
            }
            else
            {
                if (SMAddon.smController.SelectedPartsTarget.Contains(part))
                    style = SMStyle.ButtonToggledTargetStyle;
                else
                    style = SMStyle.ButtonTargetStyle;
            }
            return style;
        }

        private static string GetResourceDescription(List<string> SelectedResources, Part part)
        {
            string strDescription = "";

            if (SelectedResources.Contains("Crew"))
            {
                strDescription = part.protoModuleCrew.Count.ToString() + " - " + part.partInfo.title;
            }
            else if (SelectedResources.Contains("Science"))
            {
                int cntScience = GetScienceCount(part, false);
                strDescription = cntScience.ToString() + " - " + part.partInfo.title;
            }
            else
            {
                strDescription = part.Resources[SelectedResources[0]].amount.ToString("######0.##") + " - " + part.partInfo.title;
            }
            return strDescription;
        }

        internal static void MouseOverHighlight(Part _part)
        {
            string step = "begin";
            try
            {
                step = "inside box - Part Selection?";
                SMHighlighter.SetPartHighlight(_part, SMSettings.Colors[SMSettings.MouseOverColor]);
                SMHighlighter.EdgeHighight(_part, true);
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in Light.Highlight at step {0}.  Error:  {1}", step, ex.ToString()), "Error", true);
            }
        }
        
        #endregion

    }
}
