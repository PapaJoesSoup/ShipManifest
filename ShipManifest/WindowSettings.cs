using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace ShipManifest
{
    internal static class WindowSettings
    {
        #region Settings Window (GUI)

        internal static Rect Position = new Rect(0, 0, 0, 0);
        internal static bool ShowWindow = false;
        internal static bool ToolTipActive = false;
        internal static bool ShowToolTips = true;
        internal static string ToolTip = "";
        internal static string txtSaveInterval = SMSettings.SaveIntervalSec.ToString();

        internal static bool _showRealismTab = true;
        internal static bool _showHighlightTab = false;
        internal static bool _showConfigTab = false;
        internal static bool _showSoundsTab = false;
        internal static bool _showToolTipTab = false;
        internal static bool _showModsTab = false;

        internal static bool ShowRealismTab
        {
            get
            { 
                return _showRealismTab; 
            }
            set
            {
                if (value)
                    ResetTabs();
                _showRealismTab = value;
            }
        }
        internal static bool ShowHighlightTab
        {
            get
            { 
                return _showHighlightTab; 
            }
            set
            {
                if (value)
                    ResetTabs();
                _showHighlightTab = value;
            }
        }
        internal static bool ShowConfigTab
        {
            get
            { 
                return _showConfigTab; 
            }
            set
            {
                if (value)
                    ResetTabs();
                _showConfigTab = value;
            }
        }
        internal static bool ShowSoundsTab
        {
            get
            { 
                return _showSoundsTab; 
            }
            set
            {
                if (value)
                    ResetTabs();
                _showSoundsTab = value;
            }
        }
        internal static bool ShowToolTipTab
        {
            get
            { 
                return _showToolTipTab; 
            }
            set
            {
                if (value)
                    ResetTabs();
                _showToolTipTab = value;
            }
        }
        internal static bool ShowModsTab
        {
            get
            {
                return _showModsTab;
            }
            set
            {
                if (value)
                    ResetTabs();
                _showModsTab = value;
            }
        }
        internal static bool ShowAllAssemblies = false;

        internal static string strFlowCost = "0";

        private static Vector2 ScrollViewerPosition = Vector2.zero;
        internal static void Display(int windowId)
        {
            // Reset Tooltip active flag...
            Rect rect = new Rect();
            ToolTipActive = false;

            rect = new Rect(Position.width - 20, 4, 16, 16);
            if (GUI.Button(rect, new GUIContent("", "Close Window.\r\nSettings will not be saved, but they will also not be reverted.")))
            {
                ToolTip = "";
                if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                    SMAddon.OnSMSettingsToggle();
                else
                {
                    SMSettings.StoreTempSettings();
                    ShowWindow = false;
                }
            }
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 0, 0);

            GUILayout.BeginVertical();

            DisplayTabButtons();

            ScrollViewerPosition = GUILayout.BeginScrollView(ScrollViewerPosition, SMStyle.ScrollStyle, GUILayout.Height(300), GUILayout.Width(380));
            GUILayout.BeginVertical();

            DisplaySelectedTab();

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save", GUILayout.Height(20)))
            {
                SMSettings.SaveIntervalSec = int.Parse(txtSaveInterval);
                SMSettings.SaveSettings();
                if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                    SMAddon.OnSMSettingsToggle();
                else
                    ShowWindow = false;
            }
            if (GUILayout.Button("Cancel", GUILayout.Height(20)))
            {
                // We've canclled, so restore original settings.
                SMSettings.RestoreTempSettings();

                if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                    SMAddon.OnSMSettingsToggle();
                else
                {
                    SMSettings.StoreTempSettings();
                    ShowWindow = false;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
        }

        private static void DisplayTabButtons()
        {
            GUILayout.BeginHorizontal();

            GUIStyle realismStyle = ShowRealismTab ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
            if (GUILayout.Button("Realism", realismStyle, GUILayout.Height(20)))
            {
                ShowRealismTab = true;
            }
            GUI.enabled = true;
            var highlightStyle = ShowHighlightTab ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
            if (GUILayout.Button("Highlight", highlightStyle, GUILayout.Height(20)))
            {
                ShowHighlightTab = true;
            }
            var tooltipStyle = ShowToolTipTab ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
            if (GUILayout.Button("ToolTip", tooltipStyle, GUILayout.Height(20)))
            {
                ShowToolTipTab = true;
            }
            var soundStyle = ShowSoundsTab ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
            if (GUILayout.Button("Sound", soundStyle, GUILayout.Height(20)))
            {
                ShowSoundsTab = true;
            }
            var configStyle = ShowConfigTab ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
            if (GUILayout.Button("Config", configStyle, GUILayout.Height(20)))
            {
                ShowConfigTab = true;
            }
            var modStyle = ShowModsTab ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
            if (GUILayout.Button("Mods", modStyle, GUILayout.Height(20)))
            {
                ShowModsTab = true;
            }
            GUILayout.EndHorizontal();
        }

        internal static void DisplaySelectedTab()
        {
            if (ShowRealismTab)
                DisplayRealism();
            else if (ShowHighlightTab)
                DisplayHighlighting();
            else if (ShowSoundsTab)
                DisplaySounds();
            else if (ShowToolTipTab)
                DisplayToolTips();
            else if (ShowConfigTab)
                DisplayConfig();
            else if (ShowModsTab)
                DisplayMods();
        }

        private static void DisplayRealism()
        {
            Rect rect = new Rect();
            GUI.enabled = true;
            if (!SMSettings.LockSettings)
                GUILayout.Label("Realism Settings / Options", SMStyle.LabelTabHeader);
            else
                GUILayout.Label("Realism Settings / Options  (Locked.  Unlock in Config file)", SMStyle.LabelTabHeader);
            GUILayout.Label("____________________________________________________________________________________________", SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(350));

            bool isEnabled = (!SMSettings.LockSettings);
            // Realism Mode
            GUI.enabled = isEnabled;
            GUIContent guiLabel = new GUIContent("Enable Realism Mode","Turns on/off Realism Mode.\r\nWhen ON, causes changes in the interface and limits\r\nyour freedom to do things that would not be 'Realistic'.\r\nWhen Off, Allows Fills, Dumps, Repeating Science, instantaneous Xfers, Crew Xfers anywwhere, etc.");
            SMSettings.RealismMode = GUILayout.Toggle(SMSettings.RealismMode, guiLabel, GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);

            // EnableCrew Mode
            GUI.enabled = isEnabled;
            GUILayout.BeginHorizontal();
            guiLabel = new GUIContent("Enable Crew Xfers","Turns on/off Crew transfers.\r\nWhen ON, The Crew option will appear in your resource list.\r\nWhen Off, Crew transfers are not possible.");
            SMSettings.EnableCrew = GUILayout.Toggle(SMSettings.EnableCrew, guiLabel, GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);

            GUILayout.EndHorizontal();

            if (!SMSettings.EnableCrew && HighLogic.LoadedSceneIsFlight)
            {
                if (SMAddon.smController.SelectedResources.Contains("Crew"))
                {
                    // Clear Resource selection.
                    SMHighlighter.ClearResourceHighlighting(SMAddon.smController.SelectedResourcesParts);
                    SMAddon.smController.SelectedResources.Clear();
                    WindowTransfer.ShowWindow = false;
                }
            }

            // Enable stock Crew Xfer Override
            GUI.enabled = isEnabled;
            GUILayout.BeginHorizontal();
            guiLabel = new GUIContent("Override Stock Crew Xfers", "Turns on/off Overriding the stock Crew Transfer mechanism with the SM style.\r\nWhen ON (along with Realism Mode),\r\nstock crew transfers (Tweakable) will behave like SM style transfers.\r\nWhen Off (or Realism is off), Stock Crew transfers behave normally.");
            GUILayout.Space(20);
            SMSettings.OverrideStockCrewXfer = GUILayout.Toggle(SMSettings.OverrideStockCrewXfer, guiLabel, GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);

            GUILayout.EndHorizontal();

            // EnableCLS Mode
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            if (!SMSettings.EnableCrew || !SMSettings.CLSInstalled)
                GUI.enabled = false;
            else
                GUI.enabled = isEnabled;
            guiLabel = new GUIContent("Enable CLS  (Connected Living Spaces)", "Turns on/off Connected Living space support.\r\nWhen on, Crew can only be xfered to a part in the same 'Living Space'.\r\nWhen Off, Crew transfers are possible to any part that can hold a kerbal.");
            SMSettings.EnableCLS = GUILayout.Toggle(SMSettings.EnableCLS, guiLabel, GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);
            GUILayout.EndHorizontal();

            if (SMSettings.EnableCLS != SMSettings.prevEnableCLS && HighLogic.LoadedSceneIsFlight)
            {
                if (!SMSettings.EnableCLS)
                    SMHighlighter.HighlightCLSVessel(false, true);
                else if (SMAddon.smController.SelectedResources.Contains("Crew"))
                {
                    // Update spaces and reassign the resource to observe new settings.
                    SMAddon.UpdateCLSSpaces();
                    SMAddon.smController.SelectedResources.Clear();
                    SMAddon.smController.SelectedResources.Add("Crew");
                }
            }

            // EnableScience Mode
            GUILayout.BeginHorizontal();
            GUI.enabled = isEnabled;
            guiLabel = new GUIContent("Enable Science Xfers", "Turns on/off Science Xfers.\r\nWhen on, Science transfers are possible and show up in the Resource list.\r\nWhen Off, Science transfers will not appear in the resource list.");
            SMSettings.EnableScience = GUILayout.Toggle(SMSettings.EnableScience, guiLabel, GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);
            GUILayout.EndHorizontal();

            if (!SMSettings.EnableScience && HighLogic.LoadedSceneIsFlight)
            {
                // Clear Resource selection.
                if (SMAddon.smController.SelectedResources.Contains("Science"))
                    SMAddon.smController.SelectedResources.Clear();
            }

            // EnableResources Mode
            GUILayout.BeginHorizontal();
            GUI.enabled = isEnabled;
            guiLabel = new GUIContent("Enable Resource Xfers", "Turns on/off Resource Xfers.\r\nWhen on, Resource transfers are possible and show up in the Resource list.\r\nWhen Off, Resources (fuel, monoprpellent, etc) will not appear in the resource list.");
            SMSettings.EnableResources = GUILayout.Toggle(SMSettings.EnableResources, guiLabel, GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);
            GUILayout.EndHorizontal();

            if (!SMSettings.EnableResources && HighLogic.LoadedSceneIsFlight)
            {
                // Clear Resource selection.
                if (SMAddon.smController.SelectedResources.Count > 0 && !SMAddon.smController.SelectedResources.Contains("Crew") && !SMAddon.smController.SelectedResources.Contains("Science"))
                    SMAddon.smController.SelectedResources.Clear();
            }

            // Set Gui.enabled for child settings to resources...
            if (SMSettings.EnableResources)
                GUI.enabled = isEnabled;
            else
                GUI.enabled = false;

            // EnablePFResources Mode
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            guiLabel = new GUIContent("Enable Resources in Pre-Flight", "Turns on/off Fill and Empty Resources when in preflight.\r\nWhen on, Resource Fill and Dump resources vessel wide are possible and show up in the Resource list.\r\nWhen Off, Fill and Dump Resources vessel wide will not appear in the resource list.");
            SMSettings.EnablePFResources = GUILayout.Toggle(SMSettings.EnablePFResources, guiLabel, GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);
            GUILayout.EndHorizontal();

            // EnableXferCost Mode
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            guiLabel = new GUIContent("Enable Resource Xfer Costs", "Turns on/off ElectricCharge cost forResource Xfers.\r\nWhen on, Resource transfers will cost ElectricCharge.\r\nWhen Off, Resources Xfers are Free (original SM behavior).");
            SMSettings.EnableXferCost = GUILayout.Toggle(SMSettings.EnableXferCost, guiLabel, GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);
            GUILayout.EndHorizontal();

            // Resource Xfer EC cost
            float newCost = 0;
            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            guiLabel = new GUIContent("Resource Flow Cost:", "Sets the Electrical cost of resource Xfers when Realism Mode is on.\r\nThe higher the number the more ElectricCharge used.");
            GUILayout.Label(guiLabel, GUILayout.Width(130), GUILayout.Height(20));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);

            // Lets parse the string to allow decimal points.
            strFlowCost = SMSettings.FlowCost.ToString();
            // add the decimal point if it was typed.
            strFlowCost = Utilities.GetStringDecimal(strFlowCost);
            // add the zero if it was typed.
            strFlowCost = Utilities.GetStringZero(strFlowCost);
            
            strFlowCost = GUILayout.TextField(strFlowCost, 20, GUILayout.Height(20), GUILayout.Width(80));
            guiLabel = new GUIContent("EC/Unit", "Sets the Electrical cost of resource Xfers when Realism Mode is on.\r\nThe higher the number the more ElectricCharge used.");
            GUILayout.Label(guiLabel, GUILayout.Width(80), GUILayout.Height(20));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 10, 0 - ScrollViewerPosition.y);
            GUILayout.EndHorizontal();

            // update decimal bool 
            Utilities.SetStringDecimal(strFlowCost);
            //update zero bool 
            Utilities.SetStringZero(strFlowCost);

            if (float.TryParse(strFlowCost, out newCost))
                SMSettings.FlowCost = newCost;

            // create xfer Flow Rate slider;
            // Lets parse the string to allow decimal points.
            string strFlowRate = SMSettings.FlowRate.ToString();
            string strMinFlowRate = SMSettings.MinFlowRate.ToString();
            string strMaxFlowRate = SMSettings.MaxFlowRate.ToString();
            string strMaxFlowTime = SMSettings.MaxFlowTimeSec.ToString();

            float newRate = 0;

            // Resource Flow Rate
            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            guiLabel = new GUIContent("Resource Flow Rate:", "Sets the rate that resources Xfer when Realism Mode is on.\r\nThe higher the number the faster resources move.\r\nYou can also use the slider below to change this value");
            GUILayout.Label(guiLabel, GUILayout.Width(130), GUILayout.Height(20));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);
            strFlowRate = GUILayout.TextField(strFlowRate, 20, GUILayout.Height(20), GUILayout.Width(80));
            guiLabel = new GUIContent("Units/Sec", "Sets the rate that resources Xfer when Realism Mode is on.\r\nThe higher the number the faster resources move.\r\nYou can also use the slider below to change this value");
            GUILayout.Label(guiLabel, GUILayout.Width(80), GUILayout.Height(20));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 10, 0 - ScrollViewerPosition.y);
            GUILayout.EndHorizontal();
            if (float.TryParse(strFlowRate, out newRate))
                SMSettings.FlowRate = (int)newRate;

            // Resource Flow rate Slider
            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            GUILayout.Label(SMSettings.MinFlowRate.ToString(), GUILayout.Width(10), GUILayout.Height(20));
            SMSettings.FlowRate = (double)GUILayout.HorizontalSlider((float)SMSettings.FlowRate, (float)SMSettings.MinFlowRate, (float)SMSettings.MaxFlowRate, GUILayout.Width(240), GUILayout.Height(20));
            guiLabel = new GUIContent(SMSettings.MaxFlowRate.ToString(), "Slide control to change the Resource Flow Rate shown above.");
            GUILayout.Label(guiLabel, GUILayout.Width(40), GUILayout.Height(20));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 10, 0 - ScrollViewerPosition.y);
            GUILayout.EndHorizontal();

            // Min Flow Rate for Slider
            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            guiLabel = new GUIContent(" - Min Flow Rate:", "Sets the lowest range (left side) on the Flow rate Slider control.");
            GUILayout.Label(guiLabel, GUILayout.Width(130), GUILayout.Height(20));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);
            strMinFlowRate = GUILayout.TextField(strMinFlowRate, 20, GUILayout.Height(20), GUILayout.Width(80));
            guiLabel = new GUIContent("Units/Sec", "Sets the lowest range (left side) on the Flow rate Slider control.");
            GUILayout.Label(guiLabel, GUILayout.Width(80), GUILayout.Height(20));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 10, 0 - ScrollViewerPosition.y);
            GUILayout.EndHorizontal();
            if (float.TryParse(strMinFlowRate, out newRate))
                SMSettings.MinFlowRate = (int)newRate;

            // Max Flow Rate for Slider
            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            guiLabel = new GUIContent(" - Max Flow Rate:", "Sets the highest range (right side) on the Flow rate Slider control.");
            GUILayout.Label(guiLabel, GUILayout.Width(130), GUILayout.Height(20));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);
            strMaxFlowRate = GUILayout.TextField(strMaxFlowRate, 20, GUILayout.Height(20), GUILayout.Width(80));
            guiLabel = new GUIContent("Units/Sec", "Sets the highest range (right side) on the Flow rate Slider control.");
            GUILayout.Label(guiLabel, GUILayout.Width(80), GUILayout.Height(20));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 10, 0 - ScrollViewerPosition.y);
            GUILayout.EndHorizontal();
            if (float.TryParse(strMaxFlowRate, out newRate))
                SMSettings.MaxFlowRate = (int)newRate;

            // Max Flow Time 
            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            guiLabel = new GUIContent(" - Max Flow Time:", "Sets the maximum duration (in sec) of a resource transfer.\r\nWorks in conjunction with the Flow rate.  if time it would take\r\nto move a resource exceeds this number, this number will be used to calculate an adjusted flow rate.\r\n(protects your from 20 minute Xfers)");
            GUILayout.Label(guiLabel, GUILayout.Width(130), GUILayout.Height(20));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);
            strMaxFlowTime = GUILayout.TextField(strMaxFlowTime, 20, GUILayout.Height(20), GUILayout.Width(80));
            guiLabel = new GUIContent("Sec", "Sets the maximum duration (in sec) of a resource transfer.\r\nWorks in conjunction with the Flow rate.  if time it would take\r\nto move a resource exceeds this number, this number will be used to calculate an adjusted flow rate.\r\n(protects your from 20 minute Xfers)");
            GUILayout.Label(guiLabel, GUILayout.Width(80), GUILayout.Height(20));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 10, 0 - ScrollViewerPosition.y);
            GUILayout.EndHorizontal();
            if (float.TryParse(strMaxFlowTime, out newRate))
                SMSettings.MaxFlowTimeSec = (int)newRate;

            // reset gui.enabled to default
            GUI.enabled = isEnabled;

            // LockSettings Mode
            guiLabel = new GUIContent("Lock Settings  (If set ON, disable in config file)","Locks the settings in this section so they cannot be altered in game.\r\nTo turn off Locking you MUST edit the Config.xml file.");
            SMSettings.LockSettings = GUILayout.Toggle(SMSettings.LockSettings, guiLabel, GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 10, 0 - ScrollViewerPosition.y);
        }

        private static void DisplayHighlighting()
        {
            string label = "";
            GUI.enabled = true;
            GUILayout.Label("Highlighting", SMStyle.LabelTabHeader);
            GUILayout.Label("____________________________________________________________________________________________", SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(350));

            // EnableHighlighting Mode
            GUILayout.BeginHorizontal();
            label = "Enable Highlighting";
            SMSettings.EnableHighlighting = GUILayout.Toggle(SMSettings.EnableHighlighting, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            if (SMSettings.EnableHighlighting != SMSettings.prevEnableHighlighting && HighLogic.LoadedSceneIsFlight)
            {
                if (SMSettings.EnableCLS)
                {
                    if (SMAddon.smController.SelectedResources.Contains("Crew"))
                    {
                        // Update spaces and reassign the resource to observe new settings.
                        SMHighlighter.HighlightCLSVessel(SMSettings.EnableHighlighting, true);
                        SMAddon.UpdateCLSSpaces();
                        SMAddon.smController.SelectedResources.Clear();
                        SMAddon.smController.SelectedResources.Add("Crew");
                    }
                }
            }

            // OnlySourceTarget Mode
            GUI.enabled = true;
            if (!SMSettings.EnableHighlighting)
                GUI.enabled = false;
            else
                GUI.enabled = true;
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            label = "Highlight Only Source / Target Parts";
            SMSettings.OnlySourceTarget = GUILayout.Toggle(SMSettings.OnlySourceTarget, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            if (SMSettings.OnlySourceTarget && (!SMSettings.prevOnlySourceTarget || SMSettings.EnableCLSHighlighting))
            {
                SMSettings.EnableCLSHighlighting = false;
                if (HighLogic.LoadedSceneIsFlight && SMSettings.EnableCLS && SMAddon.smController.SelectedResources.Contains("Crew"))
                {
                    // Update spaces and reassign the resource to observe new settings.
                    SMHighlighter.HighlightCLSVessel(false, true);
                    SMAddon.UpdateCLSSpaces();
                    SMAddon.smController.SelectedResources.Clear();
                    SMAddon.smController.SelectedResources.Add("Crew");
                }
            }
            // Enable CLS Highlighting Mode
            if (!SMSettings.EnableHighlighting || !SMSettings.EnableCLS)
                GUI.enabled = false;
            else
                GUI.enabled = true;
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            label = "Enable CLS Highlighting";
            SMSettings.EnableCLSHighlighting = GUILayout.Toggle(SMSettings.EnableCLSHighlighting, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            if (SMSettings.EnableCLSHighlighting && (!SMSettings.prevEnableCLSHighlighting || SMSettings.OnlySourceTarget))
                SMSettings.OnlySourceTarget = false;
            if (HighLogic.LoadedSceneIsFlight && SMSettings.EnableCLS && SMAddon.smController.SelectedResources.Contains("Crew") && WindowTransfer.ShowWindow)
            {
                if (SMSettings.EnableCLSHighlighting != SMSettings.prevEnableCLSHighlighting)
                    SMHighlighter.HighlightCLSVessel(SMSettings.EnableCLSHighlighting);
            }

            // Enable Edge Highlighting Mode
            if (SMSettings.EnableHighlighting)
                GUI.enabled = true;
            else
                GUI.enabled = false;
            GUILayout.BeginHorizontal();
            label = "Enable Edge Highlighting (On Mouse Overs)";
            SMSettings.EnableEdgeHighlighting = GUILayout.Toggle(SMSettings.EnableEdgeHighlighting, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            if (SMSettings.EnableEdgeHighlighting != SMSettings.prevEnableEdgeHighlighting && HighLogic.LoadedSceneIsFlight)
            {
                if (SMSettings.EnableEdgeHighlighting == false)
                {
                    if (SMAddon.smController.SelectedResources.Count > 0)
                    {
                        foreach (Part part in SMAddon.smController.SelectedResourcesParts)
                            SMHighlighter.EdgeHighight(part, false);
                    }
                }
            }
            GUI.enabled = true;
        }

        private static void DisplayToolTips()
        {
            // Enable Tool Tips
            string label = "";
            GUI.enabled = true;
            GUILayout.Label("ToolTips", SMStyle.LabelTabHeader);
            GUILayout.Label("____________________________________________________________________________________________", SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(350));

            label = "Enable Tool Tips";
            SMSettings.ShowToolTips = GUILayout.Toggle(SMSettings.ShowToolTips, label, GUILayout.Width(300));

            GUI.enabled = SMSettings.ShowToolTips;
            label = "Manifest Window Tool Tips";
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            WindowManifest.ShowToolTips = GUILayout.Toggle(WindowManifest.ShowToolTips, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            label = "Transfer Window Tool Tips";
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            WindowTransfer.ShowToolTips = GUILayout.Toggle(WindowTransfer.ShowToolTips, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            label = "Settings Window Tool Tips";
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            WindowSettings.ShowToolTips = GUILayout.Toggle(WindowSettings.ShowToolTips, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            label = "Roster Window Tool Tips";
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            WindowRoster.ShowToolTips = GUILayout.Toggle(WindowRoster.ShowToolTips, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            label = "Hatch Window Tool Tips";
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            TabHatch.ShowToolTips = GUILayout.Toggle(TabHatch.ShowToolTips, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            label = "Solar Panel Window Tool Tips";
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            TabSolarPanel.ShowToolTips = GUILayout.Toggle(TabSolarPanel.ShowToolTips, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            label = "Antenna Window Tool Tips";
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            TabAntenna.ShowToolTips = GUILayout.Toggle(TabAntenna.ShowToolTips, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            label = "Light Window Tool Tips";
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            TabLight.ShowToolTips = GUILayout.Toggle(TabLight.ShowToolTips, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            label = "Debugger Window Tool Tips";
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            WindowDebugger.ShowToolTips = GUILayout.Toggle(WindowDebugger.ShowToolTips, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            GUI.enabled = true;
        }

        private static void DisplaySounds()
        {
            GUILayout.Label("Sounds", SMStyle.LabelTabHeader);
            GUILayout.Label("____________________________________________________________________________________________", SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(350));

            GUILayout.Label("Transfer Pump:", GUILayout.Height(20));

            // Pump Start Sound
            GUILayout.BeginHorizontal();
            GUILayout.Label("Pump Starting: ", GUILayout.Width(100));
            SMSettings.PumpSoundStart = GUILayout.TextField(SMSettings.PumpSoundStart, GUILayout.Width(220));
            GUILayout.EndHorizontal();

            // Pump Run Sound
            GUILayout.BeginHorizontal();
            GUILayout.Label("Pump Running: ", GUILayout.Width(100));
            SMSettings.PumpSoundRun = GUILayout.TextField(SMSettings.PumpSoundRun, GUILayout.Width(220));
            GUILayout.EndHorizontal();

            // Pump Stop Sound
            GUILayout.BeginHorizontal();
            GUILayout.Label("Pump Stopping: ", GUILayout.Width(100));
            SMSettings.PumpSoundStop = GUILayout.TextField(SMSettings.PumpSoundStop, GUILayout.Width(220));
            GUILayout.EndHorizontal();

            GUILayout.Label(" ", GUILayout.Height(10));
            GUILayout.Label("Crew:", GUILayout.Height(20));
            // Crew Start Sound
            GUILayout.BeginHorizontal();
            GUILayout.Label("Crew Exiting: ", GUILayout.Width(100));
            SMSettings.CrewSoundStart = GUILayout.TextField(SMSettings.CrewSoundStart, GUILayout.Width(220));
            GUILayout.EndHorizontal();

            // Crew Run Sound
            GUILayout.BeginHorizontal();
            GUILayout.Label("Crew Xfering: ", GUILayout.Width(100));
            SMSettings.CrewSoundRun = GUILayout.TextField(SMSettings.CrewSoundRun, GUILayout.Width(220));
            GUILayout.EndHorizontal();

            // Crew Stop Sound
            GUILayout.BeginHorizontal();
            GUILayout.Label("Crew Entering: ", GUILayout.Width(100));
            SMSettings.CrewSoundStop = GUILayout.TextField(SMSettings.CrewSoundStop, GUILayout.Width(220));
            GUILayout.EndHorizontal();
        }

        private static void DisplayConfig()
        {
            Rect rect = new Rect();
            string label = "";
            string toolTip = "";
            GUILayout.Label("Configuraton", SMStyle.LabelTabHeader);
            GUILayout.Label("____________________________________________________________________________________________", SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(350));

            if (!ToolbarManager.ToolbarAvailable)
            {
                if (SMSettings.EnableBlizzyToolbar)
                    SMSettings.EnableBlizzyToolbar = false;
                GUI.enabled = false;
            }
            else
                GUI.enabled = true;

            label = "Enable Blizzy Toolbar (Replaces Stock Toolbar)";
            SMSettings.EnableBlizzyToolbar = GUILayout.Toggle(SMSettings.EnableBlizzyToolbar, label, GUILayout.Width(300));

            GUI.enabled = true;
            // UnityStyle Mode
            label = "Enable Unity Style GUI Interface";
            SMSettings.UseUnityStyle = GUILayout.Toggle(SMSettings.UseUnityStyle, label, GUILayout.Width(300));
            if (SMSettings.UseUnityStyle != SMSettings.prevUseUnityStyle)
                SMStyle.WindowStyle = null;

            label = "Enable Debug Window";
            WindowDebugger.ShowWindow = GUILayout.Toggle(WindowDebugger.ShowWindow, label, GUILayout.Width(300));

            label = "Enable Verbose Logging";
            SMSettings.VerboseLogging = GUILayout.Toggle(SMSettings.VerboseLogging, label, GUILayout.Width(300));

            label = "Enable SM Debug Window On Error";
            SMSettings.AutoDebug = GUILayout.Toggle(SMSettings.AutoDebug, label, GUILayout.Width(300));

            label = "Save Error log on Exit";
            SMSettings.SaveLogOnExit = GUILayout.Toggle(SMSettings.SaveLogOnExit, label, GUILayout.Width(300));

            // create Limit Error Log Length slider;
            GUILayout.BeginHorizontal();
            GUILayout.Label("Error Log Length: ", GUILayout.Width(140));
            SMSettings.ErrorLogLength = GUILayout.TextField(SMSettings.ErrorLogLength, GUILayout.Width(40));
            GUILayout.Label("(lines)", GUILayout.Width(50));
            GUILayout.EndHorizontal();

            label = "Enable Kerbal Renaming";
            toolTip = "Allows renaming a Kerbal.  The Profession may change when the kerbal is renamed.";
            SMSettings.EnableKerbalRename = GUILayout.Toggle(SMSettings.EnableKerbalRename, new GUIContent(label, toolTip), GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);

            if (!SMSettings.EnableKerbalRename)
                GUI.enabled = false;
            GUILayout.BeginHorizontal();
            label = "Rename and Keep Profession (Experimental)";
            toolTip = "When On, SM will remember the selected profesison when Kerbal is Renamed.\r\nAdds non printing chars to Kerbal name in your game save.\r\n(Should be no issue, but use at your own risk.)";
            GUILayout.Space(20);
            SMSettings.RenameWithProfession = GUILayout.Toggle(SMSettings.RenameWithProfession, new GUIContent(label, toolTip), GUILayout.Width(300));
            GUILayout.EndHorizontal();
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);
            GUI.enabled = true;

            label = "Enable AutoSave Settings";
            SMSettings.AutoSave = GUILayout.Toggle(SMSettings.AutoSave, label, GUILayout.Width(300));

            GUILayout.BeginHorizontal();
            GUILayout.Label("Save Interval: ", GUILayout.Width(120));
            txtSaveInterval = GUILayout.TextField(txtSaveInterval, GUILayout.Width(40));
            GUILayout.Label("(sec)", GUILayout.Width(40));
            GUILayout.EndHorizontal();
        }

        private static void DisplayMods()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Installed Mods  ", SMStyle.LabelTabHeader, GUILayout.Width(180));
            ShowAllAssemblies = GUILayout.Toggle(ShowAllAssemblies, "Show All Assemblies", SMStyle.ToggleStyleHeader);
            GUILayout.EndHorizontal();
            GUILayout.Label("____________________________________________________________________________________________", SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(350));
            if (ShowAllAssemblies)
                InstalledMods.DisplayAssemblyList();
            else
                InstalledMods.DisplayModList();
        }

        private static void ResetTabs()
        {
            _showRealismTab = _showHighlightTab = _showToolTipTab = _showSoundsTab = _showConfigTab = _showModsTab = false;
        }

        #endregion
    }
}
