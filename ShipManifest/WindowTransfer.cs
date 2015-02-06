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

        #endregion

        #region TransferWindow GUI Layout)

        // Resource Transfer Window
        // This window allows you some control over the selected resource on a selected source and target part
        // This window assumes that a resource has been selected on the Ship manifest window.
        internal static void Display(int windowId)
        {
            // Reset Tooltip active flag...
            ToolTipActive = false;
            ShowToolTips = Settings.TransferToolTips;

            Rect rect = new Rect(604, 4, 16, 16);
            if (GUI.Button(rect, new GUIContent("", "Close Window")))
            {
                Settings.ShowTransferWindow = false;
                SMAddon.smController.SelectedResource = null;
                SMAddon.smController.SelectedPartSource = SMAddon.smController.SelectedPartTarget = null;
                ToolTip = "";
            }
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Settings.TransferPosition, GUI.tooltip, ref ToolTipActive, 0, 0);
            try
            {
                // This window assumes that a resource has been selected on the Ship manifest window.
                if (Settings.EnableCLS && SMAddon.smController.SelectedResource == "Crew")
                    SMAddon.UpdateCLSSpaces();

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

        // Transfer Window components
        private static Vector2 SourceTransferViewerScrollPosition = Vector2.zero;
        internal static void SourceTransferViewer()
        {
            try
            {
                // This is a scroll panel (we are using it to make button lists...)
                SourceTransferViewerScrollPosition = GUILayout.BeginScrollView(SourceTransferViewerScrollPosition, GUILayout.Height(120), GUILayout.Width(300));
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
                SourceDetailsViewerScrollPosition = GUILayout.BeginScrollView(SourceDetailsViewerScrollPosition, GUILayout.Height(90), GUILayout.Width(300));
                GUILayout.BeginVertical();

                if (SMAddon.smController.SelectedPartSource != null)
                {
                    if (SMAddon.smController.SelectedResource == "Crew")
                    {
                        SourceDetailsCrew();
                    }
                    else if (SMAddon.smController.SelectedResource == "Science")
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
                Utilities.LogMessage(string.Format(" in Ship Manifest Window - SourceDetailsViewer.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        private static void SourceDetailsCrew()
        {
            List<ProtoCrewMember> crewMembers = SMAddon.smController.SelectedPartSource.protoModuleCrew;
            for (int x = 0; x < SMAddon.smController.SelectedPartSource.protoModuleCrew.Count(); x++)
            {
                ProtoCrewMember crewMember = SMAddon.smController.SelectedPartSource.protoModuleCrew[x];
                GUILayout.BeginHorizontal();
                if (crewMember.seat != null)
                {
                    if (SMAddon.crewXfer || SMAddon.XferOn)
                        GUI.enabled = false;

                    if (GUILayout.Button(new GUIContent(">>", "Move Kerbal to another seat within Part"), ManifestStyle.ButtonStyle, GUILayout.Width(15), GUILayout.Height(20)))
                    {
                        ToolTip = "";
                        TransferCrewMember(crewMember, SMAddon.smController.SelectedPartSource, SMAddon.smController.SelectedPartSource);
                    }
                    if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                    {
                        Rect rect = GUILayoutUtility.GetLastRect();
                        ToolTip = Utilities.SetActiveTooltip(rect, Settings.TransferPosition, GUI.tooltip, ref ToolTipActive, 10, 190 - SourceDetailsViewerScrollPosition.y);
                    }
                    GUI.enabled = true;
                }
                GUILayout.Label(string.Format("  {0}", crewMember.name + " (" + crewMember.experienceTrait.Title + ")"), GUILayout.Width(190), GUILayout.Height(20));
                if (SMAddon.CanKerbalsBeXferred(SMAddon.smController.SelectedPartSource))
                {
                    if (SMAddon.crewXfer || SMAddon.XferOn)
                        GUI.enabled = false;
                }
                else
                    GUI.enabled = false;

                if (GUILayout.Button(new GUIContent("Xfer", xferToolTip), ManifestStyle.ButtonStyle, GUILayout.Width(50), GUILayout.Height(20)))
                {
                    TransferCrewMember(crewMember, SMAddon.smController.SelectedPartSource, SMAddon.smController.SelectedPartTarget);
                }
                if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                {
                    Rect rect = GUILayoutUtility.GetLastRect();
                    ToolTip = Utilities.SetActiveTooltip(rect, Settings.TransferPosition, GUI.tooltip, ref ToolTipActive, 20, 190 - TargetDetailsViewerScrollPosition.y);
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }
        }

        private static void SourceDetailsScience()
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

        private static void SourceDetailsResources()
        {
            foreach (PartResource resource in SMAddon.smController.SelectedPartSource.Resources)
            {
                if (resource.info.name == SMAddon.smController.SelectedResource)
                {
                    // This routine assumes that a resource has been selected on the Resource manifest window.
                    string flowtextS = "Off";
                    bool flowboolS = SMAddon.smController.SelectedPartSource.Resources[SMAddon.smController.SelectedResource].flowState;
                    if (flowboolS)
                    {
                        flowtextS = "On";
                    }
                    else
                    {
                        flowtextS = "Off";
                    }
                    PartResource.FlowMode flowmodeS = SMAddon.smController.SelectedPartSource.Resources[SMAddon.smController.SelectedResource].flowMode;

                    GUILayout.BeginHorizontal();
                    GUILayout.Label(string.Format("({0}/{1})", resource.amount.ToString("#######0.####"), resource.maxAmount.ToString("######0.####")), GUILayout.Width(175), GUILayout.Height(20));
                    GUILayout.Label(string.Format("{0}", flowtextS), GUILayout.Width(30), GUILayout.Height(20));
                    if (GUILayout.Button(new GUIContent("Flow", "Enables/Disables flow of selected resource from this part."), GUILayout.Width(50), GUILayout.Height(20)))
                    {
                        if (flowboolS)
                        {
                            SMAddon.smController.SelectedPartSource.Resources[SMAddon.smController.SelectedResource].flowState = false;
                            flowtextS = "Off";
                        }
                        else
                        {
                            SMAddon.smController.SelectedPartSource.Resources[SMAddon.smController.SelectedResource].flowState = true;
                            flowtextS = "On";
                        }
                    }
                    if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                    {
                        Rect rect = GUILayoutUtility.GetLastRect();
                        ToolTip = Utilities.SetActiveTooltip(rect, Settings.TransferPosition, GUI.tooltip, ref ToolTipActive, 30, 190 - TargetDetailsViewerScrollPosition.y);
                    }
                    GUILayout.EndHorizontal();
                    if ((SMAddon.smController.SelectedPartTarget != null && SMAddon.smController.SelectedPartSource != SMAddon.smController.SelectedPartTarget) &&
                        (SMAddon.smController.SelectedPartSource.Resources[resource.info.name].amount > 0 && SMAddon.smController.SelectedPartTarget.Resources[resource.info.name].amount < SMAddon.smController.SelectedPartTarget.Resources[resource.info.name].maxAmount))
                    {
                        if (!SMAddon.crewXfer && !SMAddon.XferOn)
                        {
                            // let's determine how much of a resource we can move to the target.
                            double maxXferAmount = SMAddon.smController.SelectedPartTarget.Resources[resource.info.name].maxAmount - SMAddon.smController.SelectedPartTarget.Resources[resource.info.name].amount;
                            if (maxXferAmount > SMAddon.smController.SelectedPartSource.Resources[resource.info.name].amount)
                                maxXferAmount = SMAddon.smController.SelectedPartSource.Resources[resource.info.name].amount;
                            if (maxXferAmount < 0)
                                maxXferAmount = 0;

                            // This is used to set the slider to the max amount by default.  
                            // OnUpdate draws every frame, so we need a way to ignore this or the slider will stay at max
                            // We set XferAmount to -1 when we set new source or target parts.
                            if (SMAddon.smController.sXferAmount < 0)
                                SMAddon.smController.sXferAmount = (float)maxXferAmount;

                            // Left Details...
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Enter Xfer Amt:  ", GUILayout.Width(100));

                            // Lets parse the string to allow decimal points.
                            string strXferAmount = SMAddon.smController.sXferAmount.ToString();
                            float newAmount = 0;

                            // add the decimal point if it was typed.
                            if (SMAddon.smController.sXferAmountHasDecimal)
                                strXferAmount += ".";
                            // add the zero if it was typed.
                            if (SMAddon.smController.sXferAmountHasZero)
                                strXferAmount += "0";

                            strXferAmount = GUILayout.TextField(strXferAmount, 20, GUILayout.Width(105));

                            // update decimal bool 
                            if (strXferAmount.EndsWith(".") || strXferAmount.EndsWith(".0"))
                                SMAddon.smController.sXferAmountHasDecimal = true;
                            else
                                SMAddon.smController.sXferAmountHasDecimal = false;

                            //update zero bool 
                            if (strXferAmount.Contains(".") && strXferAmount.EndsWith("0"))
                                SMAddon.smController.sXferAmountHasZero = true;
                            else
                                SMAddon.smController.sXferAmountHasZero = false;

                            if (float.TryParse(strXferAmount, out newAmount))
                                SMAddon.smController.sXferAmount = newAmount;

                            if (GUILayout.Button(new GUIContent("Xfer", "Transfers the selected resource to the selected Target Part"), GUILayout.Width(50), GUILayout.Height(20)))
                            {
                                TransferResource(SMAddon.smController.SelectedPartSource, SMAddon.smController.SelectedPartTarget, (double)SMAddon.smController.sXferAmount);
                            }
                            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                            {
                                Rect rect = GUILayoutUtility.GetLastRect();
                                ToolTip = Utilities.SetActiveTooltip(rect, Settings.TransferPosition, GUI.tooltip, ref ToolTipActive, 30, 190 - TargetDetailsViewerScrollPosition.y);
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Xfer:  ", GUILayout.Width(50), GUILayout.Height(20));
                            SMAddon.smController.sXferAmount = GUILayout.HorizontalSlider(SMAddon.smController.sXferAmount, 0, (float)maxXferAmount, GUILayout.Width(210));
                            GUILayout.EndHorizontal();
                        }
                    }
                }
            }
        }

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
                TargetTransferViewerScrollPosition = GUILayout.BeginScrollView(TargetTransferViewerScrollPosition, GUILayout.Height(120), GUILayout.Width(300));
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
                TargetDetailsViewerScrollPosition = GUILayout.BeginScrollView(TargetDetailsViewerScrollPosition, GUILayout.Height(90), GUILayout.Width(300));
                GUILayout.BeginVertical();

                // --------------------------------------------------------------------------
                if (SMAddon.smController.SelectedPartTarget != null)
                {
                    if (SMAddon.smController.SelectedResource == "Crew")
                    {
                        TargetDetailsCrew();
                    }
                    else if (SMAddon.smController.SelectedResource == "Science")
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
                Utilities.LogMessage(string.Format(" in Ship Manifest Window - TargetDetailsViewer.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        private static void TargetDetailsCrew()
        {
            for (int x = 0; x < SMAddon.smController.SelectedPartTarget.protoModuleCrew.Count(); x++)
            {
                ProtoCrewMember crewMember = SMAddon.smController.SelectedPartTarget.protoModuleCrew[x];
                // This routine assumes that a resource has been selected on the Resource manifest window.
                GUILayout.BeginHorizontal();
                if (crewMember.seat != null)
                {
                    if (SMAddon.crewXfer || SMAddon.XferOn)
                        GUI.enabled = false;

                    if (GUILayout.Button(new GUIContent(">>", "Move Kerbal to another seat within Part"), ManifestStyle.ButtonStyle, GUILayout.Width(15), GUILayout.Height(20)))
                    {
                        ToolTip = "";
                        TransferCrewMember(crewMember, SMAddon.smController.SelectedPartTarget, SMAddon.smController.SelectedPartTarget);
                    }
                    if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                    {
                        Rect rect = GUILayoutUtility.GetLastRect();
                        ToolTip = Utilities.SetActiveTooltip(rect, Settings.TransferPosition, GUI.tooltip, ref ToolTipActive, 320, 190 - TargetDetailsViewerScrollPosition.y);
                    }
                    GUI.enabled = true;
                }
                GUILayout.Label(string.Format("  {0}", crewMember.name + " (" + crewMember.experienceTrait.Title + ")"), GUILayout.Width(190), GUILayout.Height(20));
                if (SMAddon.CanKerbalsBeXferred(SMAddon.smController.SelectedPartTarget))
                {
                    if (SMAddon.crewXfer || SMAddon.XferOn)
                        GUI.enabled = false;
                }
                else
                    GUI.enabled = false;

                if (GUILayout.Button(new GUIContent("Xfer", xferToolTip), ManifestStyle.ButtonStyle, GUILayout.Width(50), GUILayout.Height(20)))
                {
                    TransferCrewMember(crewMember, SMAddon.smController.SelectedPartTarget, SMAddon.smController.SelectedPartSource);
                }
                if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                {
                    Rect rect = GUILayoutUtility.GetLastRect();
                    ToolTip = Utilities.SetActiveTooltip(rect, Settings.TransferPosition, GUI.tooltip, ref ToolTipActive, 320, 190 - TargetDetailsViewerScrollPosition.y);
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }
        }

        private static void TargetDetailsScience()
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

        private static void TargetDetailsResources()
        {
            // Resources
            foreach (PartResource resource in SMAddon.smController.SelectedPartTarget.Resources)
            {
                if (resource.info.name == SMAddon.smController.SelectedResource)
                {
                    // This routine assumes that a resource has been selected on the Resource manifest window.
                    string flowtextT = "Off";
                    bool flowboolT = SMAddon.smController.SelectedPartTarget.Resources[SMAddon.smController.SelectedResource].flowState;
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
                    if (GUILayout.Button(new GUIContent("Flow","Enable/Disable flow of selected resource from this part."), ManifestStyle.ButtonStyle, GUILayout.Width(50), GUILayout.Height(20)))
                    {
                        if (flowboolT)
                        {
                            SMAddon.smController.SelectedPartTarget.Resources[SMAddon.smController.SelectedResource].flowState = false;
                            flowtextT = "Off";
                        }
                        else
                        {
                            SMAddon.smController.SelectedPartTarget.Resources[SMAddon.smController.SelectedResource].flowState = true;
                            flowtextT = "On";
                        }
                    }
                    if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                    {
                        Rect rect = GUILayoutUtility.GetLastRect();
                        ToolTip = Utilities.SetActiveTooltip(rect, Settings.TransferPosition, GUI.tooltip, ref ToolTipActive, 300, 190 - TargetDetailsViewerScrollPosition.y);
                    }
                    GUILayout.EndHorizontal();
                    if ((SMAddon.smController.SelectedPartSource != null && SMAddon.smController.SelectedPartSource != SMAddon.smController.SelectedPartTarget) && (SMAddon.smController.SelectedPartTarget.Resources[resource.info.name].amount > 0 && SMAddon.smController.SelectedPartSource.Resources[resource.info.name].amount < SMAddon.smController.SelectedPartSource.Resources[resource.info.name].maxAmount))
                    {
                        // create xfer slider;
                        if (!SMAddon.crewXfer && !SMAddon.XferOn)
                        {
                            // let's determine how much of a resource we can move to the Source.
                            double maxXferAmount = SMAddon.smController.SelectedPartSource.Resources[resource.info.name].maxAmount - SMAddon.smController.SelectedPartSource.Resources[resource.info.name].amount;
                            if (maxXferAmount > SMAddon.smController.SelectedPartTarget.Resources[resource.info.name].amount)
                                maxXferAmount = SMAddon.smController.SelectedPartTarget.Resources[resource.info.name].amount;
                            if (maxXferAmount < 0)
                                maxXferAmount = 0;

                            // This is used to set the slider to the max amount by default.  
                            // OnUpdate draws every frame, so we need a way to ignore this or the slider will stay at max
                            // We set XferAmount to -1 when we set new source or target parts.
                            if (SMAddon.smController.tXferAmount < 0)
                                SMAddon.smController.tXferAmount = (float)maxXferAmount;

                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Enter Xfer Amt:  ", GUILayout.Width(100));

                            // Lets parse the string to allow decimal points.
                            string strXferAmount = SMAddon.smController.tXferAmount.ToString();
                            float newAmount = 0;

                            // add the decimal point if it was typed.
                            if (SMAddon.smController.tXferAmountHasDecimal)
                                strXferAmount += ".";
                            if (SMAddon.smController.tXferAmountHasZero)
                                strXferAmount += "0";

                            strXferAmount = GUILayout.TextField(strXferAmount, 20, GUILayout.Width(105));

                            // update decimal bool with new string
                            if (strXferAmount.EndsWith(".") || strXferAmount.EndsWith(".0"))
                                SMAddon.smController.tXferAmountHasDecimal = true;
                            else
                                SMAddon.smController.tXferAmountHasDecimal = false;

                            //update zero bool 
                            if (strXferAmount.Contains(".") && strXferAmount.EndsWith("0"))
                                SMAddon.smController.tXferAmountHasZero = true;
                            else
                                SMAddon.smController.tXferAmountHasZero = false;

                            if (float.TryParse(strXferAmount, out newAmount))
                                SMAddon.smController.tXferAmount = newAmount;
                                
                            if (GUILayout.Button(new GUIContent("Xfer","Transfers the selected resource to the selected Source Part"), GUILayout.Width(50), GUILayout.Height(20)))
                                TransferResource(SMAddon.smController.SelectedPartTarget, SMAddon.smController.SelectedPartSource, (double)SMAddon.smController.tXferAmount);
                            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                            {
                                Rect rect = GUILayoutUtility.GetLastRect();
                                ToolTip = Utilities.SetActiveTooltip(rect, Settings.TransferPosition, GUI.tooltip, ref ToolTipActive, 340, 190 - TargetDetailsViewerScrollPosition.y);
                            }

                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Xfer:  ", GUILayout.Width(50), GUILayout.Height(20));
                            SMAddon.smController.tXferAmount = GUILayout.HorizontalSlider(SMAddon.smController.tXferAmount, 0, (float)maxXferAmount, GUILayout.Width(210));
                            GUILayout.EndHorizontal();
                        }
                    }
                }
            }
        }

        #endregion

        #region Methods

        private static void TransferCrewMember(ProtoCrewMember sourceMember, Part sourcePart, Part targetPart)
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
                        SMController.RemoveCrew(sourceMember, sourcePart);
                        SMController.RemoveCrew(targetMember, targetPart);

                        // At this point, the kerbals are in the "ether".
                        // this may be why there is an issue with refreshing the internal view.. 
                        // It may allow (or expect) a board call from an (invisible) eva object.   
                        // If I can manage to properly trigger that call... then all should properly refresh...
                        // I'll look into that...

                        // Update:  Thanks to Extraplanetary LaunchPads for helping me solve this problem!
                        // Send the kerbal(s) eva.  This is the eva trigger I was looking for
                        // We will fie the board event when we are ready, in the update code.
                        SMAddon.smController.evaAction = new GameEvents.FromToAction<Part, Part>(sourcePart, targetPart);
                        if (Settings.EnableTextureReplacer)
                            GameEvents.onCrewOnEva.Fire(SMAddon.smController.evaAction);

                        // Add the crew members back into the part(s) at their new seats.
                        sourcePart.AddCrewmemberAt(targetMember, curIdx);
                        targetPart.AddCrewmemberAt(sourceMember, newIdx);
                    }
                    else
                    {
                        // Just move.
                        SMController.RemoveCrew(sourceMember, sourcePart);
                        SMAddon.smController.evaAction = new GameEvents.FromToAction<Part, Part>(sourcePart, targetPart);

                        if (Settings.EnableTextureReplacer)
                            GameEvents.onCrewOnEva.Fire(SMAddon.smController.evaAction);

                        targetPart.AddCrewmemberAt(sourceMember, newIdx);
                    }

                    // if moving within a part, set the seat2seat flag
                    if (sourcePart == targetPart)
                        SMAddon.isSeat2Seat = true;
                    else
                        SMAddon.isSeat2Seat = false;

                    // set the crew transfer flag and wait forthe timeout before firing the board event.
                    SMAddon.crewXfer = true;
                }
                else
                {
                    // no portraits, so let's just move kerbals...
                    SMController.RemoveCrew(sourceMember, sourcePart);
                    SMController.AddCrew(sourceMember, targetPart);
                    SMAddon.crewXfer = true;
                }
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
                            Utilities.LogMessage(string.Format("Science Data transfer failed..."), "Info", true);
                        }
                    }
                    else
                    {
                        Utilities.LogMessage(string.Format("((IScienceDataContainer)target) is null"), "Info", true);
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
                Utilities.LogMessage(" in TransferScience:  Error:  " + ex.ToString(), "Info", true);
            }
        }

        private static void TransferResource(Part source, Part target, double XferAmount)
        {
            try
            {
                if (source.Resources.Contains(SMAddon.smController.SelectedResource) && target.Resources.Contains(SMAddon.smController.SelectedResource))
                {
                    double maxAmount = target.Resources[SMAddon.smController.SelectedResource].maxAmount;
                    double sourceAmount = source.Resources[SMAddon.smController.SelectedResource].amount;
                    double targetAmount = target.Resources[SMAddon.smController.SelectedResource].amount;
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
                        Utilities.LogMessage("Playing pump sound...", "Info", Settings.VerboseLogging);


                        // This flag enables the Update handler in ShipManifestAddon and sets the direction
                        if (source == SMAddon.smController.SelectedPartSource)
                            SMAddon.XferMode = SMAddon.XFERMode.SourceToTarget;
                        else
                            SMAddon.XferMode = SMAddon.XFERMode.TargetToSource;

                            SMAddon.XferOn = true;
                    }
                    else
                    {
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
