using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ConnectedLivingSpace;

namespace ShipManifest
{
    public static class TransferWindow
    {
        #region Properties

        public static string ToolTip = "";
        public static bool ToolTipActive = false;

        #endregion

        #region TransferWindow GUI Layout)

        // Resource Transfer Window
        // This window allows you some control over the selected resource on a selected source and target part
        // This window assumes that a resource has been selected on the Ship manifest window.
        private static Vector2 SourceScrollViewerTransfer = Vector2.zero;
        private static Vector2 SourceScrollViewerTransfer2 = Vector2.zero;
        private static Vector2 TargetScrollViewerTransfer = Vector2.zero;
        private static Vector2 TargetScrollViewerTransfer2 = Vector2.zero;
        public static void Display(int windowId)
        {
            try
            {
                // This window assumes that a resource has been selected on the Ship manifest window.
                if (Settings.EnableCLS && ShipManifestAddon.smController.SelectedResource == "Crew")
                    ShipManifestAddon.smController.UpdateCLSSpaces();

                GUILayout.BeginHorizontal();
                //Left Column Begins
                GUILayout.BeginVertical();

                // Reset Tooltip active flag...
                ToolTipActive = false;

                // Build source Transfer Viewer
                SourceTransferViewer();

                // Text above Source Details. (Between viewers)
                if (ShipManifestAddon.smController.SelectedResource == "Crew" && Settings.ShowIVAUpdateBtn)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(ShipManifestAddon.smController.SelectedPartSource != null ? string.Format("{0}", ShipManifestAddon.smController.SelectedPartSource.partInfo.title) : "No Part Selected", GUILayout.Width(190), GUILayout.Height(20));
                    if (GUILayout.Button("Update Portraits", ManifestStyle.ButtonStyle, GUILayout.Width(110), GUILayout.Height(20)))
                    {
                        ShipManifestAddon.smController.RespawnCrew();
                    }
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.Label(ShipManifestAddon.smController.SelectedPartSource != null ? string.Format("{0}", ShipManifestAddon.smController.SelectedPartSource.partInfo.title) : "No Part Selected", GUILayout.Width(300), GUILayout.Height(20));
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
                GUILayout.Label(ShipManifestAddon.smController.SelectedPartTarget != null ? string.Format("{0}", ShipManifestAddon.smController.SelectedPartTarget.partInfo.title) : "No Part Selected", GUILayout.Width(300), GUILayout.Height(20));

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
        public static void SourceTransferViewer()
        {
            try
            {
                // This is a scroll panel (we are using it to make button lists...)
                SourceScrollViewerTransfer = GUILayout.BeginScrollView(SourceScrollViewerTransfer, GUILayout.Height(120), GUILayout.Width(300));
                GUILayout.BeginVertical();

                foreach (Part part in ShipManifestAddon.smController.PartsByResource[ShipManifestAddon.smController.SelectedResource])
                {
                    // Build the part button title...
                    string strDescription = "";
                    switch (ShipManifestAddon.smController.SelectedResource)
                    {
                        case "Crew":
                            strDescription = part.protoModuleCrew.Count.ToString() + " - " + part.partInfo.title;
                            break;
                        case "Science":
                            int cntScience = GetScienceCount(part, false);
                            strDescription = cntScience.ToString() + " - " + part.partInfo.title;
                            break;
                        default:
                            strDescription = part.Resources[ShipManifestAddon.smController.SelectedResource].amount.ToString("######0.##") + " - " + part.partInfo.title;
                            break;
                    }

                    // set the conditions for a button style change.
                    int btnWidth = 265;
                    if (!Settings.RealismMode && ShipManifestAddon.smController.SelectedResource != "Crew" && ShipManifestAddon.smController.SelectedResource != "Science")
                        btnWidth = 180;
                    var style = ShipManifestAddon.smController.SelectedPartSource == part ? ManifestStyle.ButtonToggledSourceStyle : ManifestStyle.ButtonSourceStyle;
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(string.Format("{0}", strDescription), style, GUILayout.Width(btnWidth), GUILayout.Height(20)))
                    {
                        if (!ShipManifestAddon.crewXfer && !ShipManifestAddon.XferOn)
                        {
                            ShipManifestAddon.smController.SelectedModuleSource = null;
                            ShipManifestAddon.smController.SelectedPartSource = part;
                            Utilities.LogMessage("SelectedPartSource...", "Info", Settings.VerboseLogging);
                        }
                    }
                    if (!Settings.RealismMode && ShipManifestAddon.smController.SelectedResource != "Crew" && ShipManifestAddon.smController.SelectedResource != "Science")
                    {
                        var style1 = part.Resources[ShipManifestAddon.smController.SelectedResource].amount == 0 ? ManifestStyle.ButtonToggledSourceStyle : ManifestStyle.ButtonSourceStyle;
                        var style2 = part.Resources[ShipManifestAddon.smController.SelectedResource].amount == part.Resources[ShipManifestAddon.smController.SelectedResource].maxAmount ? ManifestStyle.ButtonToggledSourceStyle : ManifestStyle.ButtonSourceStyle;

                        if (GUILayout.Button(string.Format("{0}", "Dump"), style1, GUILayout.Width(45), GUILayout.Height(20)))
                        {
                            ManifestController.DumpPartResource(part, ShipManifestAddon.smController.SelectedResource);
                        }
                        if (GUILayout.Button(string.Format("{0}", "Fill"), style2, GUILayout.Width(30), GUILayout.Height(20)))
                        {
                            ManifestController.FillPartResource(part, ShipManifestAddon.smController.SelectedResource);
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

        private static void SourceDetailsViewer()
        {
            try
            {
                // Source Part resource Details
                // this Scroll viewer is for the details of the part selected above.
                SourceScrollViewerTransfer2 = GUILayout.BeginScrollView(SourceScrollViewerTransfer2, GUILayout.Height(90), GUILayout.Width(300));
                GUILayout.BeginVertical();

                if (ShipManifestAddon.smController.SelectedPartSource != null)
                {
                    if (ShipManifestAddon.smController.SelectedResource == "Crew")
                    {
                        SourceDetailsCrew();
                    }
                    else if (ShipManifestAddon.smController.SelectedResource == "Science")
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
            List<ProtoCrewMember> crewMembers = ShipManifestAddon.smController.SelectedPartSource.protoModuleCrew;
            for (int x = 0; x < ShipManifestAddon.smController.SelectedPartSource.protoModuleCrew.Count(); x++)
            {
                ProtoCrewMember crewMember = ShipManifestAddon.smController.SelectedPartSource.protoModuleCrew[x];
                GUILayout.BeginHorizontal();
                if (crewMember.seat != null)
                {
                    if (ShipManifestAddon.crewXfer || ShipManifestAddon.XferOn)
                        GUI.enabled = false;

                    if (GUILayout.Button(new GUIContent(">>", "Move Kerbal to another seat within Part"), ManifestStyle.ButtonStyle, GUILayout.Width(15), GUILayout.Height(20)))
                    {
                        ToolTip = "";
                        TransferCrewMember(crewMember, ShipManifestAddon.smController.SelectedPartSource, ShipManifestAddon.smController.SelectedPartSource);
                    }
                    if (Event.current.type == EventType.Repaint)
                    {
                        Rect rect = GUILayoutUtility.GetLastRect();
                        if (!ToolTipActive && rect.Contains(Event.current.mousePosition))
                        {
                            ToolTipActive = true;
                            // Since we are using GUILayout, the curent mouse position returns a position with reference to the source Details viewer. 
                            // Add the height of GUI elements already drawn to y offset to get the correct screen position
                            Utilities.SetUpToolTip(rect, Settings.TransferPosition, GUI.tooltip, 10, 190);
                        }
                        // We are in a loop so we don't need the return value from SetUpToolTip.  We will assign it instead.
                        if (ToolTipActive)
                            ToolTip = GUI.tooltip;
                        else
                            ToolTip = "";
                    }
                    GUI.enabled = true;
                }
                GUILayout.Label(string.Format("  {0}", crewMember.name), GUILayout.Width(190), GUILayout.Height(20));
                if (ShipManifestAddon.CanKerbalsBeXferred(ShipManifestAddon.smController.SelectedPartSource))
                {
                    if (ShipManifestAddon.crewXfer || ShipManifestAddon.XferOn)
                        GUI.enabled = false;

                    if (GUILayout.Button("Xfer", ManifestStyle.ButtonStyle, GUILayout.Width(50), GUILayout.Height(20)))
                    {
                        TransferCrewMember(crewMember, ShipManifestAddon.smController.SelectedPartSource, ShipManifestAddon.smController.SelectedPartTarget);
                    }
                    GUI.enabled = true;
                }
                GUILayout.EndHorizontal();
            }
        }

        private static void SourceDetailsScience()
        {
            IScienceDataContainer[] modules = ShipManifestAddon.smController.SelectedPartSource.FindModulesImplementing<IScienceDataContainer>().ToArray();
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
                if ((ShipManifestAddon.smController.SelectedModuleTarget != null && pm != ShipManifestAddon.smController.SelectedModuleTarget) && scienceCount > 0)
                {                    
                    if (Settings.RealismMode && !isCollectable)
                        GUI.enabled = false;
                    if (GUILayout.Button("Xfer", ManifestStyle.ButtonStyle, GUILayout.Width(50), GUILayout.Height(20)))
                    {
                        ShipManifestAddon.smController.SelectedModuleSource = pm;
                        TransferScience(ShipManifestAddon.smController.SelectedModuleSource, ShipManifestAddon.smController.SelectedModuleTarget);
                        ShipManifestAddon.smController.SelectedModuleSource = null;
                    }
                    GUI.enabled = true;
                }
                GUILayout.EndHorizontal();
            }
        }

        private static void SourceDetailsResources()
        {
            foreach (PartResource resource in ShipManifestAddon.smController.SelectedPartSource.Resources)
            {
                if (resource.info.name == ShipManifestAddon.smController.SelectedResource)
                {
                    // This routine assumes that a resource has been selected on the Resource manifest window.
                    string flowtextS = "Off";
                    bool flowboolS = ShipManifestAddon.smController.SelectedPartSource.Resources[ShipManifestAddon.smController.SelectedResource].flowState;
                    if (flowboolS)
                    {
                        flowtextS = "On";
                    }
                    else
                    {
                        flowtextS = "Off";
                    }
                    PartResource.FlowMode flowmodeS = ShipManifestAddon.smController.SelectedPartSource.Resources[ShipManifestAddon.smController.SelectedResource].flowMode;

                    GUILayout.BeginHorizontal();
                    GUILayout.Label(string.Format("({0}/{1})", resource.amount.ToString("#######0.####"), resource.maxAmount.ToString("######0.####")), GUILayout.Width(175), GUILayout.Height(20));
                    GUILayout.Label(string.Format("{0}", flowtextS), GUILayout.Width(30), GUILayout.Height(20));
                    if (GUILayout.Button("Flow", GUILayout.Width(50), GUILayout.Height(20)))
                    {
                        if (flowboolS)
                        {
                            ShipManifestAddon.smController.SelectedPartSource.Resources[ShipManifestAddon.smController.SelectedResource].flowState = false;
                            flowtextS = "Off";
                        }
                        else
                        {
                            ShipManifestAddon.smController.SelectedPartSource.Resources[ShipManifestAddon.smController.SelectedResource].flowState = true;
                            flowtextS = "On";
                        }
                    }
                    GUILayout.EndHorizontal();
                    if ((ShipManifestAddon.smController.SelectedPartTarget != null && ShipManifestAddon.smController.SelectedPartSource != ShipManifestAddon.smController.SelectedPartTarget) &&
                        (ShipManifestAddon.smController.SelectedPartSource.Resources[resource.info.name].amount > 0 && ShipManifestAddon.smController.SelectedPartTarget.Resources[resource.info.name].amount < ShipManifestAddon.smController.SelectedPartTarget.Resources[resource.info.name].maxAmount))
                    {
                        if (!ShipManifestAddon.crewXfer && !ShipManifestAddon.XferOn)
                        {
                            // let's determine how much of a resource we can move to the target.
                            double maxXferAmount = ShipManifestAddon.smController.SelectedPartTarget.Resources[resource.info.name].maxAmount - ShipManifestAddon.smController.SelectedPartTarget.Resources[resource.info.name].amount;
                            if (maxXferAmount > ShipManifestAddon.smController.SelectedPartSource.Resources[resource.info.name].amount)
                                maxXferAmount = ShipManifestAddon.smController.SelectedPartSource.Resources[resource.info.name].amount;
                            if (maxXferAmount < 0)
                                maxXferAmount = 0;

                            // This is used to set the slider to the max amount by default.  
                            // OnUpdate draws every frame, so we need a way to ignore this or the slider will stay at max
                            // We set XferAmount to -1 when we set new source or target parts.
                            if (ShipManifestAddon.smController.sXferAmount < 0)
                                ShipManifestAddon.smController.sXferAmount = (float)maxXferAmount;

                            // Left Details...
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Enter Xfer Amt:  ", GUILayout.Width(100));

                            // Lets parse the string to allow decimal points.
                            string strXferAmount = ShipManifestAddon.smController.sXferAmount.ToString();
                            float newAmount = 0;

                            // add the decimal point if it was typed.
                            if (ShipManifestAddon.smController.sXferAmountHasDecimal)
                                strXferAmount += ".";
                            // add the zero if it was typed.
                            if (ShipManifestAddon.smController.sXferAmountHasZero)
                                strXferAmount += "0";

                            strXferAmount = GUILayout.TextField(strXferAmount, 20, GUILayout.Width(105));

                            // update decimal bool 
                            if (strXferAmount.EndsWith(".") || strXferAmount.EndsWith(".0"))
                                ShipManifestAddon.smController.sXferAmountHasDecimal = true;
                            else
                                ShipManifestAddon.smController.sXferAmountHasDecimal = false;

                            //update zero bool 
                            if (strXferAmount.Contains(".") && strXferAmount.EndsWith("0"))
                                ShipManifestAddon.smController.sXferAmountHasZero = true;
                            else
                                ShipManifestAddon.smController.sXferAmountHasZero = false;

                            if (float.TryParse(strXferAmount, out newAmount))
                                ShipManifestAddon.smController.sXferAmount = newAmount;

                            if (GUILayout.Button("Xfer", GUILayout.Width(50), GUILayout.Height(20)))
                            {
                                TransferResource(ShipManifestAddon.smController.SelectedPartSource, ShipManifestAddon.smController.SelectedPartTarget, (double)ShipManifestAddon.smController.sXferAmount);
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Xfer:  ", GUILayout.Width(50), GUILayout.Height(20));
                            ShipManifestAddon.smController.sXferAmount = GUILayout.HorizontalSlider(ShipManifestAddon.smController.sXferAmount, 0, (float)maxXferAmount, GUILayout.Width(210));
                            GUILayout.EndHorizontal();
                        }
                    }
                }
            }
        }

        private static void TargetTransferViewer()
        {
            try
            {
                // This is a scroll panel (we are using it to make button lists...)
                TargetScrollViewerTransfer = GUILayout.BeginScrollView(TargetScrollViewerTransfer, GUILayout.Height(120), GUILayout.Width(300));
                GUILayout.BeginVertical();
                foreach (Part part in ShipManifestAddon.smController.PartsByResource[ShipManifestAddon.smController.SelectedResource])
                {
                    // Build the part button title...
                    string strDescription = "";
                    switch (ShipManifestAddon.smController.SelectedResource)
                    {
                        case "Crew":
                            strDescription = part.protoModuleCrew.Count.ToString() + " - " + part.partInfo.title;
                            break;
                        case "Science":
                            int cntScience = GetScienceCount(part, false);
                            strDescription = cntScience.ToString() + " - " + part.partInfo.title;
                            break;
                        default:
                            strDescription = part.Resources[ShipManifestAddon.smController.SelectedResource].amount.ToString("######0.##") + " - " + part.partInfo.title;
                            break;
                    }

                    // set the conditions for a button style change.
                    int btnWidth = 265;
                    if (!Settings.RealismMode && ShipManifestAddon.smController.SelectedResource != "Crew" && ShipManifestAddon.smController.SelectedResource != "Science")
                        btnWidth = 180;
                    var style = ShipManifestAddon.smController.SelectedPartTarget == part ? ManifestStyle.ButtonToggledTargetStyle : ManifestStyle.ButtonTargetStyle;
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(string.Format("{0}", strDescription), style, GUILayout.Width(btnWidth), GUILayout.Height(20)))
                    {
                        if (!ShipManifestAddon.crewXfer && !ShipManifestAddon.XferOn)
                        {
                            ShipManifestAddon.smController.SelectedPartTarget = part;
                            Utilities.LogMessage("SelectedPartTarget...", "Info", Settings.VerboseLogging);
                        }
                    }
                    if (!Settings.RealismMode && ShipManifestAddon.smController.SelectedResource != "Crew" && ShipManifestAddon.smController.SelectedResource != "Science")
                    {
                        var style1 = part.Resources[ShipManifestAddon.smController.SelectedResource].amount == 0 ? ManifestStyle.ButtonToggledTargetStyle : ManifestStyle.ButtonTargetStyle;
                        var style2 = part.Resources[ShipManifestAddon.smController.SelectedResource].amount == part.Resources[ShipManifestAddon.smController.SelectedResource].maxAmount ? ManifestStyle.ButtonToggledTargetStyle : ManifestStyle.ButtonTargetStyle;

                        if (GUILayout.Button(string.Format("{0}", "Dump"), style1, GUILayout.Width(45), GUILayout.Height(20)))
                        {
                            ManifestController.DumpPartResource(part, ShipManifestAddon.smController.SelectedResource);
                        }
                        if (GUILayout.Button(string.Format("{0}", "Fill"), style2, GUILayout.Width(30), GUILayout.Height(20)))
                        {
                            ManifestController.FillPartResource(part, ShipManifestAddon.smController.SelectedResource);
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

        private static void TargetDetailsViewer()
        {
            try
            {
                // Target Part resource details
                TargetScrollViewerTransfer2 = GUILayout.BeginScrollView(TargetScrollViewerTransfer2, GUILayout.Height(90), GUILayout.Width(300));
                GUILayout.BeginVertical();

                // --------------------------------------------------------------------------
                if (ShipManifestAddon.smController.SelectedPartTarget != null)
                {
                    if (ShipManifestAddon.smController.SelectedResource == "Crew")
                    {
                        TargetDetailsCrew();
                    }
                    else if (ShipManifestAddon.smController.SelectedResource == "Science")
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
            for (int x = 0; x < ShipManifestAddon.smController.SelectedPartTarget.protoModuleCrew.Count(); x++)
            {
                ProtoCrewMember crewMember = ShipManifestAddon.smController.SelectedPartTarget.protoModuleCrew[x];
                // This routine assumes that a resource has been selected on the Resource manifest window.
                GUILayout.BeginHorizontal();
                if (crewMember.seat != null)
                {
                    if (ShipManifestAddon.crewXfer || ShipManifestAddon.XferOn)
                        GUI.enabled = false;

                    if (GUILayout.Button(new GUIContent(">>", "Move Kerbal to another seat within Part"), ManifestStyle.ButtonStyle, GUILayout.Width(15), GUILayout.Height(20)))
                    {
                        ToolTip = "";
                        TransferCrewMember(crewMember, ShipManifestAddon.smController.SelectedPartTarget, ShipManifestAddon.smController.SelectedPartTarget);
                    }
                    if (Event.current.type == EventType.Repaint)
                    {
                        // Since we are using GUILayout, the curent mouse position returns a position with reference to the Target Details viewer. 
                        // Add the height and width of GUI elements already drawn to the x & y offsets to get the correct screen position
                        Rect rect = GUILayoutUtility.GetLastRect();
                        // we are in a loop, so we need to remember if the tooltip was set earlier
                        if (!ToolTipActive && rect.Contains(Event.current.mousePosition))
                        { 
                            ToolTipActive = true;
                            Utilities.SetUpToolTip(rect, Settings.TransferPosition, GUI.tooltip, 320, 190);
                        }
                        // We are in a loop so we don't need the return value from SetUpToolTip.  We will assign it instead.
                        if (ToolTipActive)
                            ToolTip = GUI.tooltip;
                        else
                            ToolTip = "";
                    }
                    GUI.enabled = true;
                }
                GUILayout.Label(string.Format("  {0}", crewMember.name), GUILayout.Width(190), GUILayout.Height(20));
                if (ShipManifestAddon.CanKerbalsBeXferred(ShipManifestAddon.smController.SelectedPartTarget))
                {
                    if (ShipManifestAddon.crewXfer || ShipManifestAddon.XferOn)
                        GUI.enabled = false;

                    // set the conditions for a button style change.
                    if (GUILayout.Button("Xfer", ManifestStyle.ButtonStyle, GUILayout.Width(50), GUILayout.Height(20)))
                    {
                        TransferCrewMember(crewMember, ShipManifestAddon.smController.SelectedPartTarget, ShipManifestAddon.smController.SelectedPartSource);
                    }
                    GUI.enabled = true;
                }
                GUILayout.EndHorizontal();
            }
        }

        private static void TargetDetailsScience()
        {
            int count = 0;
            foreach (PartModule tpm in ShipManifestAddon.smController.SelectedPartTarget.Modules)
            {
                if (tpm is IScienceDataContainer)
                    count += 1;
            }

            foreach (PartModule pm in ShipManifestAddon.smController.SelectedPartTarget.Modules)
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
                    if (pm == ShipManifestAddon.smController.SelectedModuleTarget)
                        ShowReceive = true;
                    else if (count == 1)
                        ShowReceive = true;
                    //SelectedModuleTarget = pm;
                    var style = ShowReceive ? ManifestStyle.ButtonToggledTargetStyle : ManifestStyle.ButtonStyle;
                    if (GUILayout.Button("Recv", style, GUILayout.Width(50), GUILayout.Height(20)))
                    {
                        ShipManifestAddon.smController.SelectedModuleTarget = pm;
                    }
                    GUILayout.EndHorizontal();
                }
            }
        }

        private static void TargetDetailsResources()
        {
            // Resources
            foreach (PartResource resource in ShipManifestAddon.smController.SelectedPartTarget.Resources)
            {
                if (resource.info.name == ShipManifestAddon.smController.SelectedResource)
                {
                    // This routine assumes that a resource has been selected on the Resource manifest window.
                    string flowtextT = "Off";
                    bool flowboolT = ShipManifestAddon.smController.SelectedPartTarget.Resources[ShipManifestAddon.smController.SelectedResource].flowState;
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
                            ShipManifestAddon.smController.SelectedPartTarget.Resources[ShipManifestAddon.smController.SelectedResource].flowState = false;
                            flowtextT = "Off";
                        }
                        else
                        {
                            ShipManifestAddon.smController.SelectedPartTarget.Resources[ShipManifestAddon.smController.SelectedResource].flowState = true;
                            flowtextT = "On";
                        }
                    }
                    GUILayout.EndHorizontal();
                    if ((ShipManifestAddon.smController.SelectedPartSource != null && ShipManifestAddon.smController.SelectedPartSource != ShipManifestAddon.smController.SelectedPartTarget) && (ShipManifestAddon.smController.SelectedPartTarget.Resources[resource.info.name].amount > 0 && ShipManifestAddon.smController.SelectedPartSource.Resources[resource.info.name].amount < ShipManifestAddon.smController.SelectedPartSource.Resources[resource.info.name].maxAmount))
                    {
                        // create xfer slider;
                        if (!ShipManifestAddon.crewXfer && !ShipManifestAddon.XferOn)
                        {
                            // let's determine how much of a resource we can move to the Source.
                            double maxXferAmount = ShipManifestAddon.smController.SelectedPartSource.Resources[resource.info.name].maxAmount - ShipManifestAddon.smController.SelectedPartSource.Resources[resource.info.name].amount;
                            if (maxXferAmount > ShipManifestAddon.smController.SelectedPartTarget.Resources[resource.info.name].amount)
                                maxXferAmount = ShipManifestAddon.smController.SelectedPartTarget.Resources[resource.info.name].amount;
                            if (maxXferAmount < 0)
                                maxXferAmount = 0;

                            // This is used to set the slider to the max amount by default.  
                            // OnUpdate draws every frame, so we need a way to ignore this or the slider will stay at max
                            // We set XferAmount to -1 when we set new source or target parts.
                            if (ShipManifestAddon.smController.tXferAmount < 0)
                                ShipManifestAddon.smController.tXferAmount = (float)maxXferAmount;

                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Enter Xfer Amt:  ", GUILayout.Width(100));

                            // Lets parse the string to allow decimal points.
                            string strXferAmount = ShipManifestAddon.smController.tXferAmount.ToString();
                            float newAmount = 0;

                            // add the decimal point if it was typed.
                            if (ShipManifestAddon.smController.tXferAmountHasDecimal)
                                strXferAmount += ".";
                            if (ShipManifestAddon.smController.tXferAmountHasZero)
                                strXferAmount += "0";

                            strXferAmount = GUILayout.TextField(strXferAmount, 20, GUILayout.Width(105));

                            // update decimal bool with new string
                            if (strXferAmount.EndsWith(".") || strXferAmount.EndsWith(".0"))
                                ShipManifestAddon.smController.tXferAmountHasDecimal = true;
                            else
                                ShipManifestAddon.smController.tXferAmountHasDecimal = false;

                            //update zero bool 
                            if (strXferAmount.Contains(".") && strXferAmount.EndsWith("0"))
                                ShipManifestAddon.smController.tXferAmountHasZero = true;
                            else
                                ShipManifestAddon.smController.tXferAmountHasZero = false;

                            if (float.TryParse(strXferAmount, out newAmount))
                                ShipManifestAddon.smController.tXferAmount = newAmount;
                                
                            if (GUILayout.Button("Xfer", GUILayout.Width(50), GUILayout.Height(20)))
                                TransferResource(ShipManifestAddon.smController.SelectedPartTarget, ShipManifestAddon.smController.SelectedPartSource, (double)ShipManifestAddon.smController.tXferAmount);

                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Xfer:  ", GUILayout.Width(50), GUILayout.Height(20));
                            ShipManifestAddon.smController.tXferAmount = GUILayout.HorizontalSlider(ShipManifestAddon.smController.tXferAmount, 0, (float)maxXferAmount, GUILayout.Width(210));
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
                        ManifestController.RemoveCrew(sourceMember, sourcePart);
                        ManifestController.RemoveCrew(targetMember, targetPart);

                        // At this point, the kerbals are in the "ether".
                        // this may be why there is an issue with refreshing the internal view.. 
                        // It may allow (or expect) a board call from an (invisible) eva object.   
                        // If I can manage to properly trigger that call... then all should properly refresh...
                        // I'll look into that...

                        // Update:  Thanks to Extraplanetary LaunchPads for helping me solve this problem!
                        // Send the kerbal(s) eva.  This is the eva trigger I was looking for
                        // We will fie the board event when we are ready, in the update code.
                        ShipManifestAddon.smController.evaAction = new GameEvents.FromToAction<Part, Part>(sourcePart, targetPart);
                        if (Settings.EnableTextureReplacer)
                            GameEvents.onCrewOnEva.Fire(ShipManifestAddon.smController.evaAction);

                        // Add the crew members back into the part(s) at their new seats.
                        sourcePart.AddCrewmemberAt(targetMember, curIdx);
                        targetPart.AddCrewmemberAt(sourceMember, newIdx);
                    }
                    else
                    {
                        // Just move.
                        ManifestController.RemoveCrew(sourceMember, sourcePart);
                        ShipManifestAddon.smController.evaAction = new GameEvents.FromToAction<Part, Part>(sourcePart, targetPart);

                        if (Settings.EnableTextureReplacer)
                            GameEvents.onCrewOnEva.Fire(ShipManifestAddon.smController.evaAction);

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
                    ManifestController.RemoveCrew(sourceMember, sourcePart);
                    ManifestController.AddCrew(sourceMember, targetPart);
                    ShipManifestAddon.crewXfer = true;
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
                if (source.Resources.Contains(ShipManifestAddon.smController.SelectedResource) && target.Resources.Contains(ShipManifestAddon.smController.SelectedResource))
                {
                    double maxAmount = target.Resources[ShipManifestAddon.smController.SelectedResource].maxAmount;
                    double sourceAmount = source.Resources[ShipManifestAddon.smController.SelectedResource].amount;
                    double targetAmount = target.Resources[ShipManifestAddon.smController.SelectedResource].amount;
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
                        if (source == ShipManifestAddon.smController.SelectedPartSource)
                            ShipManifestAddon.XferMode = ShipManifestAddon.XFERMode.SourceToTarget;
                        else
                            ShipManifestAddon.XferMode = ShipManifestAddon.XFERMode.TargetToSource;

                            ShipManifestAddon.XferOn = true;
                    }
                    else
                    {
                        // Fill target
                        target.Resources[ShipManifestAddon.smController.SelectedResource].amount += XferAmount;

                        // Drain source...
                        source.Resources[ShipManifestAddon.smController.SelectedResource].amount -= XferAmount;
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
