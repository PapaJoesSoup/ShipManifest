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

        // Switches for List Viewers
        internal static bool ShowSourceVessels = false;
        internal static bool ShowTargetVessels = false;

        internal static Dictionary<PartModule, bool> _ScienceModulesSource;
        internal static Dictionary<PartModule, bool> ScienceModulesSource
        {
            get
            {
                if (_ScienceModulesSource == null)
                {
                    if (SMAddon.smController.SelectedPartsSource.Count > 0)
                    {
                        _ScienceModulesSource = new Dictionary<PartModule, bool>();
                        IScienceDataContainer[] modules = SMAddon.smController.SelectedPartsSource[0].FindModulesImplementing<IScienceDataContainer>().ToArray();
                        if (modules.Length > 0)
                        {
                            foreach (PartModule pm in modules)
                            {
                                _ScienceModulesSource.Add(pm, false);
                            }
                        }
                        return _ScienceModulesSource;
                    } 
                    else
                    {
                        return new Dictionary<PartModule, bool>();
                    }
                }
                else
                    return _ScienceModulesSource;
            }
        }

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
                SMAddon.smController.SelectedResources.Clear();
                SMAddon.smController.SelectedPartsSource.Clear();
                SMAddon.smController.SelectedPartsTarget.Clear();

                SMAddon.smController.SelectedVesselsSource.Clear();
                SMAddon.smController.SelectedVesselsTarget.Clear();
                ToolTip = "";
                return;
            }
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = SMToolTips.SetActiveTooltip(rect, WindowTransfer.Position, GUI.tooltip, ref ToolTipActive, 10, 0);
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
                TextBetweenViewers(SMAddon.smController.SelectedPartsSource, SMAddon.XFERMode.SourceToTarget);

                // Build Details ScrollViewer
                SourceDetailsViewer();

                // Okay, we are done with the left column of the dialog...
                GUILayout.EndVertical();

                // Right Column Begins...
                GUILayout.BeginVertical();

                // Build Target Transfer Viewer
                TargetTransferViewer();

                // Text between viewers
                TextBetweenViewers(SMAddon.smController.SelectedPartsTarget, SMAddon.XFERMode.TargetToSource);

                // Build Target details Viewer
                TargetDetailsViewer();

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
                SMAddon.RepositionWindows("WindowTransfer");
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in Ship Manifest Window.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        private static void TextBetweenViewers(List<Part> SelectedParts, SMAddon.XFERMode xferMode)
        {
            string labelText = "";
            GUILayout.BeginHorizontal();
            if (SMAddon.smController.SelectedResources.Contains("Crew"))
                labelText = SelectedParts.Count > 0 ? string.Format("{0}", SelectedParts[0].partInfo.title) : "No Part Selected";
            else
            {
                if (SelectedParts != null)
                {
                    if (SelectedParts.Count > 1)
                        labelText = string.Format("{0}", "Multiple Parts Selected");
                    else if (SelectedParts.Count == 1)
                        labelText = string.Format("{0}", SelectedParts[0].partInfo.title);
                    else
                        labelText = string.Format("{0}", "No Part Selected");
                }
            }
            GUILayout.Label(labelText, SMStyle.LabelStyleNoWrap, GUILayout.Width(200));
            if (CanShowVessels())
            {
                if (xferMode == SMAddon.XFERMode.SourceToTarget)
                {
                    bool prevValue = WindowTransfer.ShowSourceVessels;
                    WindowTransfer.ShowSourceVessels = GUILayout.Toggle(WindowTransfer.ShowSourceVessels, "Vessels", GUILayout.Width(90));
                    if (!prevValue && WindowTransfer.ShowSourceVessels)
                        WindowManifest.ReconcileSelectedXferParts(SMAddon.smController.SelectedResources);
                }
                else
                {
                    bool prevValue = WindowTransfer.ShowSourceVessels;
                    WindowTransfer.ShowTargetVessels = GUILayout.Toggle(WindowTransfer.ShowTargetVessels, "Vessels", GUILayout.Width(90));
                    if (!prevValue && WindowTransfer.ShowSourceVessels)
                        WindowManifest.ReconcileSelectedXferParts(SMAddon.smController.SelectedResources);
                }
            }
            GUILayout.EndHorizontal();
        }

        private static bool CanShowVessels()
        {
            return SMAddon.smController.DockedVessels.Count > 0 && !SMAddon.smController.SelectedResources.Contains("Crew") && !SMAddon.smController.SelectedResources.Contains("Science");
        }

        #region Source Viewers (GUI Layout)
        // Transfer Window components
        private static Vector2 SourceTransferViewerScrollPosition = Vector2.zero;
        internal static void SourceTransferViewer()
        {
            try
            {
                // This is a scroll panel (we are using it to make button lists...)
                SourceTransferViewerScrollPosition = GUILayout.BeginScrollView(SourceTransferViewerScrollPosition, SMStyle.ScrollStyle, GUILayout.Height(100), GUILayout.Width(300));
                GUILayout.BeginVertical();

                if (ShowSourceVessels && CanShowVessels())
                    VesselTransferViewer(SMAddon.smController.SelectedResources, SMAddon.XFERMode.SourceToTarget, SourceTransferViewerScrollPosition);
                else
                    PartsTransferViewer(SMAddon.smController.SelectedResources, SMAddon.XFERMode.SourceToTarget, SourceTransferViewerScrollPosition);

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
                SourceDetailsViewerScrollPosition = GUILayout.BeginScrollView(SourceDetailsViewerScrollPosition, SMStyle.ScrollStyle, GUILayout.Height(120), GUILayout.Width(300));
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
                TargetTransferViewerScrollPosition = GUILayout.BeginScrollView(TargetTransferViewerScrollPosition, SMStyle.ScrollStyle, GUILayout.Height(100), GUILayout.Width(300));
                GUILayout.BeginVertical();

                if (ShowTargetVessels && CanShowVessels())
                    VesselTransferViewer(SMAddon.smController.SelectedResources, SMAddon.XFERMode.TargetToSource, TargetTransferViewerScrollPosition);
                else
                    PartsTransferViewer(SMAddon.smController.SelectedResources, SMAddon.XFERMode.TargetToSource, TargetTransferViewerScrollPosition);

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
                TargetDetailsViewerScrollPosition = GUILayout.BeginScrollView(TargetDetailsViewerScrollPosition, SMStyle.ScrollStyle, GUILayout.Height(120), GUILayout.Width(300));
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

        private static void PartsTransferViewer(List<string> SelectedResources, SMAddon.XFERMode xferMode, Vector2 ViewerScrollPosition)
        {
            float scrollX = WindowTransfer.Position.x + (xferMode == SMAddon.XFERMode.SourceToTarget ? 20 : 320);
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
                    int btnWidth = 270;
                    if (!SMSettings.RealismMode && !SelectedResources.Contains("Crew") && !SelectedResources.Contains("Science"))
                        btnWidth = 190;

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
                        SMHighlighter.IsMouseOver = true;
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
                            GUI.enabled = (part.Resources[SelectedResources[0]].amount > 0 || part.Resources[SelectedResources[1]].amount > 0) ? true : false;
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
                            GUI.enabled = part.Resources[SelectedResources[0]].amount < part.Resources[SelectedResources[0]].maxAmount || part.Resources[SelectedResources[1]].amount < part.Resources[SelectedResources[1]].maxAmount ? true : false;
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
                    Utilities.LogMessage("Error in Windowtransfer.PartsTransferViewer (" + xferMode.ToString() + ") at step:  " + step + ".  Error:  " + ex.ToString(), "Error", true);
                    SMAddon.frameErrTripped = true;
                }
            }
        }

        private static void VesselTransferViewer(List<string> SelectedResources, SMAddon.XFERMode xferMode, Vector2 ViewerScrollPosition)
        {
            float scrollX = WindowTransfer.Position.x + (xferMode == SMAddon.XFERMode.SourceToTarget ? 20 : 320);
            float scrollY = WindowTransfer.Position.y + 30 - ViewerScrollPosition.y;
            string step = "begin";
            try
            {
                step = "begin button loop";
                foreach (ModDockedVessel modDockedVessel in SMAddon.smController.DockedVessels)
                {
                    // Build the part button title...
                    step = "vessel button title";
                    string strDescription = GetResourceDescription(SelectedResources, modDockedVessel);

                    // set the conditions for a button style change.
                    int btnWidth = 265;
                    if (!SMSettings.RealismMode)
                        btnWidth = 180;

                    // Set style based on viewer and toggled state.
                    step = "Set style";
                    GUIStyle style = GetVesselButtonStyle(xferMode, modDockedVessel);

                    GUILayout.BeginHorizontal();

                    // Now let's account for any target buttons already pressed. (sources and targets for resources cannot be the same)
                    GUI.enabled = IsVesselSelectable(SelectedResources[0], xferMode, modDockedVessel);

                    step = "Render part Buttons";
                    if (GUILayout.Button(string.Format("{0}", strDescription), style, GUILayout.Width(btnWidth), GUILayout.Height(20)))
                    {
                        VesselButtonToggled(xferMode, modDockedVessel);
                    }
                    Rect rect = GUILayoutUtility.GetLastRect();
                    if (Event.current.type == EventType.Repaint && rect.Contains(Event.current.mousePosition))
                    {
                        SMHighlighter.IsMouseOver = true;
                        SMHighlighter.MouseOverMode = xferMode;
                        SMHighlighter.MouseOverRect = new Rect(scrollX + rect.x, scrollY + rect.y, rect.width, rect.height);
                        SMHighlighter.MouseOverpart = null;
                        SMHighlighter.MouseOverparts = modDockedVessel.VesselParts;
                    }

                    // Reset Button enabling.
                    GUI.enabled = true;

                    //step = "Render dump/fill buttons";
                    if (!SMSettings.RealismMode)
                    {
                        if (SelectedResources.Count > 1)
                            GUI.enabled = (TransferResource.CalcResourceRemaining(modDockedVessel.VesselParts, SelectedResources[0]) > 0 || TransferResource.CalcResourceRemaining(modDockedVessel.VesselParts, SelectedResources[1]) > 0) ? true : false;
                        else
                            GUI.enabled = TransferResource.CalcResourceRemaining(modDockedVessel.VesselParts, SelectedResources[0]) > 0 ? true : false;
                        var style1 = xferMode == SMAddon.XFERMode.SourceToTarget ? SMStyle.ButtonSourceStyle : SMStyle.ButtonTargetStyle;
                        if (GUILayout.Button(string.Format("{0}", "Dump"), style1, GUILayout.Width(45), GUILayout.Height(20)))
                        {
                            SMController.DumpPartsResource(modDockedVessel.VesselParts, SelectedResources[0]);
                            if (SelectedResources.Count > 1)
                                SMController.DumpPartsResource(modDockedVessel.VesselParts, SelectedResources[1]);
                        }

                        var style2 = xferMode == SMAddon.XFERMode.SourceToTarget ? SMStyle.ButtonSourceStyle : SMStyle.ButtonTargetStyle;
                        if (SelectedResources.Count > 1)
                            GUI.enabled = TransferResource.CalcRemainingCapacity(modDockedVessel.VesselParts, SelectedResources[0]) > 0 || TransferResource.CalcRemainingCapacity(modDockedVessel.VesselParts, SelectedResources[0]) > 0 ? true : false;
                        else
                            GUI.enabled = TransferResource.CalcRemainingCapacity(modDockedVessel.VesselParts, SelectedResources[0]) > 0 ? true : false;
                        if (GUILayout.Button(string.Format("{0}", "Fill"), style2, GUILayout.Width(30), GUILayout.Height(20)))
                        {
                            SMController.FillPartsResource(modDockedVessel.VesselParts, SelectedResources[0]);
                            if (SelectedResources.Count > 1)
                                SMController.FillPartsResource(modDockedVessel.VesselParts, SelectedResources[1]);
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
                    Utilities.LogMessage("Error in Windowtransfer.VesselTransferViewer (" + xferMode.ToString() + ") at step:  " + step + ".  Error:  " + ex.ToString(), "Error", true);
                    SMAddon.frameErrTripped = true;
                }
            }
        }

        private static void CrewDetails(List<Part> SelectedPartsFrom, List<Part> SelectedPartsTo, SMAddon.XFERMode xferMode, Vector2 scrollPosition)
        {
            // Since only one Crew Part can be selected, all lists will use an index of [0].
            float xOffset = xferMode == SMAddon.XFERMode.SourceToTarget ? 30 : 330;
            float yOffset = 160;

            if (SelectedPartsFrom.Count > 0)
            {
                List<ProtoCrewMember> crewMembers = SelectedPartsFrom[0].protoModuleCrew;
                for (int x = 0; x < SelectedPartsFrom[0].protoModuleCrew.Count(); x++)
                {
                    ProtoCrewMember crewMember = SelectedPartsFrom[0].protoModuleCrew[x];
                    GUILayout.BeginHorizontal();
                    if (crewMember.seat != null)
                    {
                        if (SMAddon.smController.CrewTransfer.CrewXferActive || TransferResource.ResourceXferActive)
                            GUI.enabled = false;

                        if (GUILayout.Button(new GUIContent(">>", "Move Kerbal to another seat within Part"), SMStyle.ButtonStyle, GUILayout.Width(15), GUILayout.Height(20)))
                        {
                            ToolTip = "";
                            SMAddon.smController.CrewTransfer.CrewTransferBegin(crewMember, SelectedPartsFrom[0], SelectedPartsFrom[0]);
                        }
                        if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                        {
                            Rect rect = GUILayoutUtility.GetLastRect();
                            ToolTip = SMToolTips.SetActiveTooltip(rect, WindowTransfer.Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - scrollPosition.y);
                        }
                        GUI.enabled = true;
                    }
                    GUILayout.Label(string.Format("  {0}", crewMember.name + " (" + crewMember.experienceTrait.Title + ")"), GUILayout.Width(190), GUILayout.Height(20));
                    if (SMAddon.CanKerbalsBeXferred(SelectedPartsFrom, SelectedPartsTo))
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
                            SMAddon.smController.CrewTransfer.CrewTransferBegin(crewMember, SelectedPartsFrom[0], SelectedPartsTo[0]);
                        }
                        if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                        {
                            Rect rect = GUILayoutUtility.GetLastRect();
                            ToolTip = SMToolTips.SetActiveTooltip(rect, WindowTransfer.Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - scrollPosition.y);
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
                float xOffset = 30;
                float yOffset = 160;
                PartModule[] modules = ScienceModulesSource.Keys.ToArray();
                foreach (PartModule pm in modules)
                {
                    // Containers.
                    int scienceCount = ((IScienceDataContainer)pm).GetScienceCount();
                    bool isCollectable = true;
                    if (pm.moduleName == "ModuleScienceExperiment")
                        isCollectable = ((ModuleScienceExperiment)pm).dataIsCollectable;
                    else if (pm.moduleName == "ModuleScienceContainer")
                        isCollectable = ((ModuleScienceContainer)pm).dataIsCollectable;

                    GUILayout.BeginHorizontal();
                    if (((IScienceDataContainer)pm).GetScienceCount() > 0)
                        GUI.enabled = true;
                    else
                        GUI.enabled = false;

                    string label = "+";
                    string toolTip = "Expand/Collapse Science detail.";
                    if (!GUI.enabled)
                        toolTip += " (Disabled, nothing to xfer)";
                    // TODO:  add logic for mananging expand/collapse container list.
                    GUIStyle expandStyle = ScienceModulesSource[pm] ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
                    if (ScienceModulesSource[pm])
                        label = "-";
                    if (GUILayout.Button(new GUIContent(label, toolTip), expandStyle, GUILayout.Width(15), GUILayout.Height(20)))
                    {
                        ScienceModulesSource[pm] = !ScienceModulesSource[pm];
                    }
                    if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                    {
                        Rect rect = GUILayoutUtility.GetLastRect();
                        ToolTip = SMToolTips.SetActiveTooltip(rect, WindowTransfer.Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - TargetDetailsViewerScrollPosition.y);
                    }
                    GUI.enabled = true;
                    GUILayout.Label(string.Format("{0} - ({1})", pm.moduleName, scienceCount.ToString()), GUILayout.Width(205), GUILayout.Height(20));

                    // If we have target selected, it is not the same as the source, there is science to xfer.
                    if (SMAddon.smController.SelectedModuleTarget != null && scienceCount > 0)
                    {
                        if (SMSettings.RealismMode && !isCollectable)
                        {
                            GUI.enabled = false;
                            toolTip = "Realism Mode is preventing transfer.\r\nExperiment/data is marked not transferable";
                        }
                        else
                        {
                            GUI.enabled = true;
                            toolTip = "Realism is off, or Experiment/data is transferable";
                        }
                        if (GUILayout.Button(new GUIContent("Xfer", toolTip), SMStyle.ButtonStyle, GUILayout.Width(40), GUILayout.Height(20)))
                        {
                            SMAddon.smController.SelectedModuleSource = pm;
                            TransferScience(SMAddon.smController.SelectedModuleSource, SMAddon.smController.SelectedModuleTarget);
                            SMAddon.smController.SelectedModuleSource = null;
                        }
                        if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                        {
                            Rect rect = GUILayoutUtility.GetLastRect();
                            ToolTip = SMToolTips.SetActiveTooltip(rect, WindowTransfer.Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - TargetDetailsViewerScrollPosition.y);
                        }
                    }
                    GUILayout.EndHorizontal();
                    if (ScienceModulesSource[pm])
                    {
                        ScienceData[] data = ((IScienceDataContainer)pm).GetData();

                        foreach (ScienceData item in data)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("", GUILayout.Width(15), GUILayout.Height(20));

                            //toolTip += "\r\n-SubjectID:   " + item.subjectID;
                            ScienceExperiment se = ResearchAndDevelopment.GetExperiment(item.subjectID.Split('@')[0]);
                            string key = (from k in se.Results.Keys where item.subjectID.Split('@')[1].Contains(k) select k).SingleOrDefault();
                            toolTip = item.title;
                            toolTip += "\r\n-Results:    " + se.Results[key];
                            toolTip += "\r\n-Data Amt:   " + item.dataAmount.ToString() + " Mits";
                            toolTip += "\r\n-Xmit Value: " + item.transmitValue.ToString();
                            toolTip += "\r\n-Lab Value:  " + item.labValue.ToString();
                            toolTip += "\r\n-Lab Boost:  " + item.labBoost.ToString();

                            GUILayout.Label(new GUIContent(se.experimentTitle, toolTip), SMStyle.LabelStyleNoWrap, GUILayout.Width(205), GUILayout.Height(20));
                            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                            {
                                Rect rect = GUILayoutUtility.GetLastRect();
                                ToolTip = SMToolTips.SetActiveTooltip(rect, WindowTransfer.Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - TargetDetailsViewerScrollPosition.y);
                            }
                            if (SMSettings.RealismMode && !isCollectable)
                            {
                                GUI.enabled = false;
                                toolTip = "Realism Mode is preventing transfer.\r\nData is marked not transferable";
                            }
                            else
                            {
                                toolTip = "Realism is off, or Data is transferable";
                                GUI.enabled = true;
                            }
                            if (SMAddon.smController.SelectedModuleTarget != null && scienceCount > 0)
                            {
                                if (GUILayout.Button(new GUIContent("Xfer", toolTip), SMStyle.ButtonStyle, GUILayout.Width(40), GUILayout.Height(20)))
                                {
                                    if (((ModuleScienceContainer)SMAddon.smController.SelectedModuleTarget).AddData(item))
                                        ((IScienceDataContainer)pm).DumpData(item);
                                }
                                if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                                {
                                    Rect rect = GUILayoutUtility.GetLastRect();
                                    ToolTip = SMToolTips.SetActiveTooltip(rect, WindowTransfer.Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - TargetDetailsViewerScrollPosition.y);
                                }
                            }
                            GUILayout.EndHorizontal();
                        }
                    }
                    GUI.enabled = true;
                }
            }
        }

        private static void ScienceDetailsTarget()
        {
            float xOffset = 330;
            float yOffset = 160;
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
                        GUILayout.Label(string.Format("{0} - ({1})", pm.moduleName, scienceCount.ToString()), GUILayout.Width(220), GUILayout.Height(20));
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
                            if (GUILayout.Button(new GUIContent("Recv", "Set this module as the receiving container"), style, GUILayout.Width(40), GUILayout.Height(20)))
                            {
                                SMAddon.smController.SelectedModuleTarget = pm;
                            }
                            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                            {
                                Rect rect = GUILayoutUtility.GetLastRect();
                                ToolTip = SMToolTips.SetActiveTooltip(rect, WindowTransfer.Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - TargetDetailsViewerScrollPosition.y);
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                }
            }
        }

        private static void ResourceDetails(List<Part> partsSource, List<Part> partsTarget, SMAddon.XFERMode xferMode, Vector2 scrollPosition)
        {
            // This routine assumes that a resource has been selected on the Resource manifest window.
            // Set scrollX offsets for left and right viewers
            int xOffset = xferMode == SMAddon.XFERMode.SourceToTarget ? 30 : 330;
            int yOffset = 160;
            string label = "";
            string toolTip = "";
            Rect rect = new Rect();

            // Pass in static vars to improve readability.
            List<string> selectedResources = SMAddon.smController.SelectedResources;
            List<TransferResource> XferResources = SMAddon.smController.ResourcesToXfer;

            if (partsSource.Count > 0)
            {
                TransferResource modResource = TransferResource.GetXferResource(XferResources, xferMode, false);
                TransferResource ratioResource = TransferResource.GetXferResource(XferResources, xferMode, true);
                modResource.XferRatio = 1;
                ratioResource.XferRatio = TransferResource.CalcRatio(XferResources, xferMode);

                double thisXferAmount = modResource.XferAmount(xferMode);
                double ratioXferAmt = ratioResource.XferAmount(xferMode);
                
                string strXferAmount = modResource.XferAmount(xferMode).ToString();

                // Set tooltips directional data
                string strTarget = xferMode == SMAddon.XFERMode.SourceToTarget ? "Target" : "Source";


                // Resource Flow control Display loop
                ResourceFlowButtons(partsSource, xferMode, scrollPosition, xOffset, yOffset);

                // Xfer Controls Display
                // let's determine how much of a resource we can move to the target.
                double maxXferAmount = TransferResource.CalcMaxXferAmt(partsSource, partsTarget, selectedResources);
                if (maxXferAmount > 0 || TransferResource.ResourceXferActive)
                {
                    GUILayout.BeginHorizontal();
                    if (TransferResource.ResourceXferActive)
                    {
                        // We want to show this during transfer if the direction is correct...
                        if (SMAddon.XferMode == xferMode)
                        {
                            GUILayout.Label("Xfer Remaining:", GUILayout.Width(120));
                            GUILayout.Label((modResource.XferAmount(SMAddon.XferMode) - modResource.AmtXferred).ToString("#######0.##"));
                            if (SMAddon.smController.SelectedResources.Count > 1)
                                GUILayout.Label(" | " + (ratioResource.XferAmount(SMAddon.XferMode) - ratioResource.AmtXferred).ToString("#######0.##"));
                        }
                    }
                    else
                    {
                        // Lets parse the string to allow decimal points.
                        strXferAmount = modResource.XferAmount(xferMode).ToString();
                        // add the decimal point if it was typed.
                        strXferAmount = modResource.GetStringDecimal(strXferAmount, xferMode);
                        // add the zero if it was typed.
                        strXferAmount = modResource.GetStringZero(strXferAmount, xferMode);

                        // Now update the static var
                        modResource.SetXferAmountString(strXferAmount, xferMode);
                        if(selectedResources.Count > 1)
                        {
                            label = "Xfer Amts:";
                            toolTip = "Displays xfer amounts of both resourses selected.";
                            toolTip += "\r\nAllows editing of part's larger capacity resourse xfer value.";
                            toolTip += "\r\nIt then calculates the smaller xfer amount using a ratio";
                            toolTip += "\r\n of the smaller capacity resource to the larger.";
                        }
                        else
                        {
                            label = "Xfer Amt:";
                            toolTip += "Displays the Amount of selected resource to xfer.";
                            toolTip += "\r\nAllows editing of the xfer value.";
                        }
                        GUILayout.Label(new GUIContent(label, toolTip), GUILayout.Width(65));
                        rect = GUILayoutUtility.GetLastRect();
                        if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                            ToolTip = SMToolTips.SetActiveTooltip(rect, WindowTransfer.Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - scrollPosition.y);
                        strXferAmount = GUILayout.TextField(strXferAmount, 20, GUILayout.Width(95), GUILayout.Height(20));
                        // update decimal bool 
                        modResource.SetStringDecimal(strXferAmount, xferMode);
                        //update zero bool 
                        modResource.SetStringZero(strXferAmount, xferMode);
                        // Update static Xfer Amount var
                        thisXferAmount = modResource.UpdateXferAmount(strXferAmount, xferMode);
                        ratioXferAmt = thisXferAmount * ratioResource.XferRatio > ratioResource.FromCapacity(xferMode) ? ratioResource.FromCapacity(xferMode) : thisXferAmount * ratioResource.XferRatio;
                        if (SMAddon.smController.SelectedResources.Count > 1)
                        {
                            label = " | " + ratioXferAmt.ToString("#######0.##");
                            toolTip = "Smaller Tank xfer amount.  Calculated at " + ratioResource.XferRatio.ToString() + ".\r\n(Note: A value of 0.818181 = 0.9/1.1)";
                            GUILayout.Label(new GUIContent(label, toolTip), GUILayout.Width(80));
                            rect = GUILayoutUtility.GetLastRect();
                            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                                ToolTip = SMToolTips.SetActiveTooltip(rect, WindowTransfer.Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - scrollPosition.y);
                        }
                    }
                    GUILayout.EndHorizontal();

                    if (IsShipControllable() && CanResourceBeXferred(xferMode, maxXferAmount))
                    {
                        GUILayout.BeginHorizontal();
                        GUIStyle noPad = SMStyle.LabelStyleNoPad;
                        label = "Xfer:";
                        toolTip = "Xfer amount slider control.\r\nMove slider to select a different value.\r\nYou can use this instead of the text box above.";
                        GUILayout.Label(new GUIContent(label, toolTip), noPad, GUILayout.Width(50), GUILayout.Height(20));
                        rect = GUILayoutUtility.GetLastRect();
                        if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                            ToolTip = SMToolTips.SetActiveTooltip(rect, WindowTransfer.Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - scrollPosition.y);
                        thisXferAmount = (double)GUILayout.HorizontalSlider((float)thisXferAmount, 0, (float)maxXferAmount, GUILayout.Width(190));

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
                                AssignXferAmounts(XferResources, xferMode, thisXferAmount);
                                TransferResources(partsSource, partsTarget);
                            }
                            else if (TransferResource.ResourceXferActive && SMSettings.RealismMode)
                                TransferResource.XferState = TransferResource.ResourceXFERState.Stop;
                        }
                        rect = GUILayoutUtility.GetLastRect();
                        if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                            ToolTip = SMToolTips.SetActiveTooltip(rect, WindowTransfer.Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - scrollPosition.y);
                        GUILayout.EndHorizontal();
                    }
                    if (!TransferResource.ResourceXferActive)
                        modResource.UpdateXferAmount(thisXferAmount.ToString(), xferMode);
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
                        GUILayout.Label(string.Format("{0}: ({1}/{2})", resource, modResource.FromAmtRemaining(XferMode).ToString("#######0.##"), modResource.FromCapacity(XferMode).ToString("######0.##")), SMStyle.LabelStyleNoWrap, GUILayout.Width(220), GUILayout.Height(18));
                        GUILayout.Label(string.Format("{0}", flowtext), GUILayout.Width(20), GUILayout.Height(18));
                        if (SMAddon.vessel.IsControllable)
                        {
                            step = "render flow button(s)";
                            if (GUILayout.Button(new GUIContent("Flow", "Enables/Disables flow of selected resource(s) from selected part(s)."), GUILayout.Width(40), GUILayout.Height(20)))
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
                                ToolTip = SMToolTips.SetActiveTooltip(rect, WindowTransfer.Position, GUI.tooltip, ref ToolTipActive, scrollX, scrollY - scrollPosition.y);
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
                        WindowTransfer._ScienceModulesSource = null;
                    }
                    else
                    {
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

        private static void VesselButtonToggled(SMAddon.XFERMode xferMode, ModDockedVessel modVessel)
        {
            string step = "Vessel Button Toggled";
            try
            {
                if (xferMode == SMAddon.XFERMode.SourceToTarget)
                {
                    // Now lets update the list...
                    if (SMAddon.smController.SelectedVesselsSource.Contains(modVessel))
                        SMAddon.smController.SelectedVesselsSource.Remove(modVessel);
                    else
                        SMAddon.smController.SelectedVesselsSource.Add(modVessel);
                    SMAddon.smController.SelectedPartsSource = SMAddon.smController.GetSelectedVesselsParts(SMAddon.smController.SelectedVesselsSource, SMAddon.smController.SelectedResources);
                }
                else
                {
                    if (SMAddon.smController.SelectedVesselsTarget.Contains(modVessel))
                        SMAddon.smController.SelectedVesselsTarget.Remove(modVessel);
                    else
                        SMAddon.smController.SelectedVesselsTarget.Add(modVessel);
                    SMAddon.smController.SelectedPartsTarget = SMAddon.smController.GetSelectedVesselsParts(SMAddon.smController.SelectedVesselsTarget, SMAddon.smController.SelectedResources);
                }
                WindowManifest.ReconcileSelectedXferParts(SMAddon.smController.SelectedResources);
                step = "Set Xfer amounts?";
                foreach (TransferResource modResource in SMAddon.smController.ResourcesToXfer)
                {
                    modResource.srcXferAmount = TransferResource.CalcMaxResourceXferAmt(SMAddon.smController.SelectedPartsSource, SMAddon.smController.SelectedPartsTarget, modResource.ResourceName);
                    modResource.tgtXferAmount = TransferResource.CalcMaxResourceXferAmt(SMAddon.smController.SelectedPartsTarget, SMAddon.smController.SelectedPartsSource, modResource.ResourceName);
                }
            }
            catch (Exception ex)
            {
                if (!SMAddon.frameErrTripped)
                {
                    Utilities.LogMessage("Error in WindowTransfer.VesselButtonToggled (" + xferMode.ToString() + ") at step:  " + step + ".  Error:  " + ex.ToString(), "Error", true);
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
                                ((IScienceDataContainer)source).DumpData(data);

                            if (SMSettings.RealismMode)
                                Utilities.LogMessage(string.Format("((Module ScienceExperiment xferred.  Dump Source data"), "Info", SMSettings.VerboseLogging);
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
                        scienceCount += ((IScienceDataContainer)pm).GetScienceCount();
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

        private static bool IsVesselSelectable(string SelectedResource, SMAddon.XFERMode xferMode, ModDockedVessel modDockedVessel)
        {
            bool isSelectable = true;
            if (xferMode == SMAddon.XFERMode.SourceToTarget)
            {
                if (SMAddon.smController.SelectedVesselsTarget.Contains(modDockedVessel))
                    isSelectable = false;
            }
            else
            {
                if (SMAddon.smController.SelectedVesselsSource.Contains(modDockedVessel))
                    isSelectable = false;
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

        private static GUIStyle GetVesselButtonStyle(SMAddon.XFERMode xferMode, ModDockedVessel modDockedVessel)
        {
            GUIStyle style = SMStyle.ButtonSourceStyle;
            if (xferMode == SMAddon.XFERMode.SourceToTarget)
            {
                if (SMAddon.smController.SelectedVesselsSource.Contains(modDockedVessel))
                    style = SMStyle.ButtonToggledSourceStyle;
                else
                    style = SMStyle.ButtonSourceStyle;
            }
            else
            {
                if (SMAddon.smController.SelectedVesselsTarget.Contains(modDockedVessel))
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

        private static string GetResourceDescription(List<string> SelectedResources, ModDockedVessel modDockedVvessel)
        {
            string strDescription = "";

            strDescription = DisplayVesselResourceTotals(modDockedVvessel, SelectedResources) + " - " + modDockedVvessel.VesselName;
            return strDescription;
        }

        internal static string DisplayVesselResourceTotals(ModDockedVessel modDockedVessel, List<string> selectedResources)
        {
            string displayAmount = "";
            double currAmount = 0;
            double totAmount = 0;
            try
            {
                List<ModDockedVessel> modDockedVessels = new List<ModDockedVessel>();
                modDockedVessels.Add(modDockedVessel);
                foreach (Part part in SMAddon.smController.GetSelectedVesselsParts(modDockedVessels, selectedResources))
                {
                    currAmount += part.Resources[selectedResources[0]].amount;
                    totAmount += part.Resources[selectedResources[0]].maxAmount;
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(String.Format(" in DisplayVesselResourceTotals().  Error:  {0}", ex.ToString()), "Error", true);
            }
            displayAmount = string.Format("({0}/{1})", currAmount.ToString("#######0"), totAmount.ToString("######0"));

            return displayAmount;
        }

        #endregion

    }
}
