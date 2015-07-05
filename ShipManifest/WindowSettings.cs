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

        // GUI tooltip and label support
        internal static Rect rect = new Rect();
        internal static string label = "";
        internal static string toolTip = "";
        internal static GUIContent guiLabel;

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

        internal static float xOffset = 80;
        internal static float yOffset = 60;

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
            if (Event.current.type == EventType.Repaint && ShowToolTips)
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
            SMAddon.RepositionWindows("WindowSettings");
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
            GUI.enabled = true;
            if (!SMSettings.LockSettings)
                GUILayout.Label("Realism Settings / Options", SMStyle.LabelTabHeader);
            else
                GUILayout.Label("Realism Settings / Options  (Locked.  To unlock, edit SMSettings.dat file)", SMStyle.LabelTabHeader);
            GUILayout.Label("____________________________________________________________________________________________", SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(350));

            bool isEnabled = (!SMSettings.LockSettings);
            // Realism Mode
            GUI.enabled = isEnabled;
            label = "Enable Realism Mode";
            toolTip = "Turns on/off Realism Mode.";
            toolTip += "\r\nWhen ON, causes changes in the interface and limits";
            toolTip += "\r\nyour freedom to do things that would not be 'Realistic'.";
            toolTip += "\r\nWhen Off, Allows Fills, Dumps, Repeating Science,";
            toolTip += "\r\ninstantaneous Xfers, Crew Xfers anywwhere, etc.";
            guiLabel = new GUIContent(label, toolTip);
            SMSettings.RealismMode = GUILayout.Toggle(SMSettings.RealismMode, guiLabel, GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);

            // EnableCrew Mode
            GUI.enabled = isEnabled;
            GUILayout.BeginHorizontal();
            label = "Enable Crew Xfers";
            toolTip = "Turns on/off Crew transfers.";
            toolTip += "\r\nWhen ON, The Crew option will appear in your resource list.";
            toolTip += "\r\nWhen Off, Crew transfers are not possible.";
            guiLabel = new GUIContent(label, toolTip);
            SMSettings.EnableCrew = GUILayout.Toggle(SMSettings.EnableCrew, guiLabel, GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);

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
            label = "Override Stock Crew Xfers";
            toolTip = "Turns on/off Overriding the stock Crew Transfer mechanism with the SM style.";
            toolTip += "\r\nWhen ON (requires Realism Mode On), stock crew transfers will behave like SM style transfers.";
            toolTip += "\r\nWhen Off (or Realism is off), Stock Crew transfers behave normally.";
            guiLabel = new GUIContent(label, toolTip);
            GUILayout.Space(20);
            SMSettings.OverrideStockCrewXfer = GUILayout.Toggle(SMSettings.OverrideStockCrewXfer, guiLabel, GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);

            GUILayout.EndHorizontal();

            // EnableCLS Mode
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            if (!SMSettings.EnableCrew || !SMSettings.CLSInstalled)
                GUI.enabled = false;
            else
                GUI.enabled = isEnabled;
            label = "Enable CLS  (Connected Living Spaces)";
            toolTip = "Turns on/off Connected Living space support.";
            toolTip += "\r\nWhen on, Crew can only be xfered to a part in the same 'Living Space'.";
            toolTip += "\r\nWhen Off, Crew transfers are possible to any part that can hold a kerbal.";
            guiLabel = new GUIContent(label, toolTip);
            SMSettings.EnableCLS = GUILayout.Toggle(SMSettings.EnableCLS, guiLabel, GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);
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
            label = "Enable Science Xfers";
            toolTip = "Turns on/off Science Xfers.";
            toolTip += "\r\nWhen on, Science transfers are possible and show up in the Resource list.";
            toolTip += "\r\nWhen Off, Science transfers will not appear in the resource list.";
            guiLabel = new GUIContent(label, toolTip);
            SMSettings.EnableScience = GUILayout.Toggle(SMSettings.EnableScience, guiLabel, GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);
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
            label = "Enable Resource Xfers";
            toolTip = "Turns on/off Resource Xfers.\r\nWhen on, Resource transfers are possible and show up in the Resource list.\r\nWhen Off, Resources (fuel, monoprpellent, etc) will not appear in the resource list.";
            guiLabel = new GUIContent(label, toolTip);
            SMSettings.EnableResources = GUILayout.Toggle(SMSettings.EnableResources, guiLabel, GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);
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
            label = "Enable Resources in Pre-Flight";
            toolTip = "Turns on/off Fill and Empty Resources when in preflight.";
            toolTip += "\r\nWhen on, Fill & Dump resources vessel wide are possible (shows in the Resource list).";
            toolTip += "\r\nWhen Off, Fill and Dump Resources vessel wide will not appear in the resource list.";
            guiLabel = new GUIContent(label, toolTip);
            SMSettings.EnablePFResources = GUILayout.Toggle(SMSettings.EnablePFResources, guiLabel, GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);
            GUILayout.EndHorizontal();

            // EnableXferCost Mode
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            label = "Enable Resource Xfer Costs";
            toolTip = "Turns on/off ElectricCharge cost forResource Xfers.";
            toolTip += "\r\nWhen on, Resource transfers will cost ElectricCharge.";
            toolTip += "\r\nWhen Off, Resources Xfers are Free (original SM behavior).";
            guiLabel = new GUIContent(label, toolTip);
            SMSettings.EnableXferCost = GUILayout.Toggle(SMSettings.EnableXferCost, guiLabel, GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);
            GUILayout.EndHorizontal();

            // Resource Xfer EC cost
            float newCost = 0;
            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            label = "Resource Flow Cost:";
            toolTip = "Sets the Electrical cost of resource Xfers when Realism Mode is on.";
            toolTip += "\r\nThe higher the number the more ElectricCharge used.";
            guiLabel = new GUIContent(label, toolTip);
            GUILayout.Label(guiLabel, GUILayout.Width(130), GUILayout.Height(20));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);

            // Lets parse the string to allow decimal points.
            strFlowCost = SMSettings.FlowCost.ToString();
            // add the decimal point if it was typed.
            strFlowCost = Utilities.GetStringDecimal(strFlowCost);
            // add the zero if it was typed.
            strFlowCost = Utilities.GetStringZero(strFlowCost);
            
            strFlowCost = GUILayout.TextField(strFlowCost, 20, GUILayout.Height(20), GUILayout.Width(80));
            label = "EC/Unit";
            toolTip = "Sets the Electrical cost of resource Xfers when Realism Mode is on.";
            toolTip += "\r\nThe higher the number the more ElectricCharge used.";
            guiLabel = new GUIContent(label, toolTip);
            GUILayout.Label(guiLabel, GUILayout.Width(80), GUILayout.Height(20));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 20, yOffset - ScrollViewerPosition.y);
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
            label = "Resource Flow Rate:";
            toolTip = "Sets the rate that resources Xfer when Realism Mode is on.";
            toolTip += "\r\nThe higher the number the faster resources move.";
            toolTip += "\r\nYou can also use the slider below to change this value.";
            guiLabel = new GUIContent(label, toolTip);
            GUILayout.Label(guiLabel, GUILayout.Width(130), GUILayout.Height(20));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);
            strFlowRate = GUILayout.TextField(strFlowRate, 20, GUILayout.Height(20), GUILayout.Width(80));
            label = "Units/Sec";
            toolTip = "Sets the rate that resources Xfer when Realism Mode is on.";
            toolTip += "\r\nThe higher the number the faster resources move.";
            toolTip += "\r\nYou can also use the slider below to change this value.";
            guiLabel = new GUIContent(label, toolTip);
            GUILayout.Label(guiLabel, GUILayout.Width(80), GUILayout.Height(20));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 20, yOffset - ScrollViewerPosition.y);
            GUILayout.EndHorizontal();
            if (float.TryParse(strFlowRate, out newRate))
                SMSettings.FlowRate = (int)newRate;

            // Resource Flow rate Slider
            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            GUILayout.Label(SMSettings.MinFlowRate.ToString(), GUILayout.Width(10), GUILayout.Height(20));
            SMSettings.FlowRate = (double)GUILayout.HorizontalSlider((float)SMSettings.FlowRate, (float)SMSettings.MinFlowRate, (float)SMSettings.MaxFlowRate, GUILayout.Width(240), GUILayout.Height(20));
            label = SMSettings.MaxFlowRate.ToString();
            toolTip = "Slide control to change the Resource Flow Rate shown above.";
            guiLabel = new GUIContent(label, toolTip);
            GUILayout.Label(guiLabel, GUILayout.Width(40), GUILayout.Height(20));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 20, yOffset - ScrollViewerPosition.y);
            GUILayout.EndHorizontal();

            // Min Flow Rate for Slider
            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            label = " - Min Flow Rate:";
            toolTip = "Sets the lowest range (left side) on the Flow rate Slider control.";
            guiLabel = new GUIContent(label, toolTip);
            GUILayout.Label(guiLabel, GUILayout.Width(130), GUILayout.Height(20));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);
            strMinFlowRate = GUILayout.TextField(strMinFlowRate, 20, GUILayout.Height(20), GUILayout.Width(80));
            label = "Units/Sec";
            toolTip = "Sets the lowest range (left side) on the Flow rate Slider control.";
            guiLabel = new GUIContent(label, toolTip);
            GUILayout.Label(guiLabel, GUILayout.Width(80), GUILayout.Height(20));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 20, yOffset - ScrollViewerPosition.y);
            GUILayout.EndHorizontal();
            if (float.TryParse(strMinFlowRate, out newRate))
                SMSettings.MinFlowRate = (int)newRate;

            // Max Flow Rate for Slider
            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            label = " - Max Flow Rate:";
            toolTip = "Sets the highest range (right side) on the Flow rate Slider control.";
            guiLabel = new GUIContent(label, toolTip);
            GUILayout.Label(guiLabel, GUILayout.Width(130), GUILayout.Height(20));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);
            strMaxFlowRate = GUILayout.TextField(strMaxFlowRate, 20, GUILayout.Height(20), GUILayout.Width(80));
            label = "Units/Sec";
            toolTip = "Sets the highest range (right side) on the Flow rate Slider control.";
            guiLabel = new GUIContent(label, toolTip);
            GUILayout.Label(guiLabel, GUILayout.Width(80), GUILayout.Height(20));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 20, yOffset - ScrollViewerPosition.y);
            GUILayout.EndHorizontal();
            if (float.TryParse(strMaxFlowRate, out newRate))
                SMSettings.MaxFlowRate = (int)newRate;

            // Max Flow Time 
            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            label = " - Max Flow Time:";
            toolTip = "Sets the maximum duration (in sec) of a resource transfer.";
            toolTip += "\r\nWorks in conjunction with the Flow rate.  if time it would take";
            toolTip += "\r\nto move a resource exceeds this number, this number will be used";
            toolTip += "\r\nto calculate an adjusted flow rate.";
            toolTip += "\r\n(protects your from 20 minute Xfers)";
            guiLabel = new GUIContent(label, toolTip);
            GUILayout.Label(guiLabel, GUILayout.Width(130), GUILayout.Height(20));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);
            strMaxFlowTime = GUILayout.TextField(strMaxFlowTime, 20, GUILayout.Height(20), GUILayout.Width(80));
            label = "Sec";
            toolTip = "Sets the maximum duration (in sec) of a resource transfer.";
            toolTip += "\r\nWorks in conjunction with the Flow rate.  if time it would take";
            toolTip += "\r\nto move a resource exceeds this number, this number will be used";
            toolTip += "\r\nto calculate an adjusted flow rate.";
            toolTip += "\r\n(protects your from 20 minute Xfers)";
            guiLabel = new GUIContent(label, toolTip);
            GUILayout.Label(guiLabel, GUILayout.Width(80), GUILayout.Height(20));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 20, yOffset - ScrollViewerPosition.y);
            GUILayout.EndHorizontal();
            if (float.TryParse(strMaxFlowTime, out newRate))
                SMSettings.MaxFlowTimeSec = (int)newRate;

            // reset gui.enabled to default
            GUI.enabled = isEnabled;

            // LockSettings Mode
            label = "Lock Settings  (If set ON, disable in config file)";
            toolTip = "Locks the settings in this section so they cannot be altered in game.";
            toolTip += "\r\nTo turn off Locking you MUST edit the Config.xml file.";
            guiLabel = new GUIContent(label, toolTip);
            SMSettings.LockSettings = GUILayout.Toggle(SMSettings.LockSettings, guiLabel, GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 20, yOffset - ScrollViewerPosition.y);
        }

        private static void DisplayHighlighting()
        {
            GUI.enabled = true;
            GUILayout.Label("Highlighting", SMStyle.LabelTabHeader);
            GUILayout.Label("____________________________________________________________________________________________", SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(350));

            // EnableHighlighting Mode
            GUILayout.BeginHorizontal();
            label = "Enable Highlighting";
            toolTip = "Enables highlighting of all parts that contain a given resource or resources when selected in the Manifest Window.\r\nThis is a global setting.  Does not affect mouseover highlighting.";
            guiLabel = new GUIContent(label, toolTip);
            SMSettings.EnableHighlighting = GUILayout.Toggle(SMSettings.EnableHighlighting, guiLabel, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 20, yOffset - ScrollViewerPosition.y);
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
            toolTip = "Disables general highlighting of parts for a selected Resource or resources.\r\nRestricts highlighting of parts to only the part or parts selected in the Transfer Window.\r\nRequires 'Enable Highlighting' to be On.";
            guiLabel = new GUIContent(label, toolTip);
            SMSettings.OnlySourceTarget = GUILayout.Toggle(SMSettings.OnlySourceTarget, guiLabel, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 20, yOffset - ScrollViewerPosition.y);
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
            toolTip = "Changes highlighting behavoir when CLS is installed and Crew is selected in the Manifest Window.\r\nHighlights the parts associated with livable/passable spaces on vessel.\r\nRequires 'Enable Highlighting' to be On and is mutually exclusive with Source & Target parts only.";
            guiLabel = new GUIContent(label, toolTip);
            SMSettings.EnableCLSHighlighting = GUILayout.Toggle(SMSettings.EnableCLSHighlighting, guiLabel, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 20, yOffset - ScrollViewerPosition.y);
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
            toolTip = "Changes highlighting behavoir when you mouseover a part in the Transfer Window part selection sections.\r\ncauses the edge of the part to glow, making it easier to see.\r\nRequires Edge Highlighting to be enable in the general KSP Game settings.";
            guiLabel = new GUIContent(label, toolTip);
            SMSettings.EnableEdgeHighlighting = GUILayout.Toggle(SMSettings.EnableEdgeHighlighting, guiLabel, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 20, yOffset - ScrollViewerPosition.y);
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
            GUI.enabled = true;
            GUILayout.Label("ToolTips", SMStyle.LabelTabHeader);
            GUILayout.Label("____________________________________________________________________________________________", SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(350));

            label = "Enable Tool Tips";
            toolTip = "Allows tooltips to be turned on or off\r\nThis is a global setting for all windows/tabs.";
            guiLabel = new GUIContent(label, toolTip);
            SMSettings.ShowToolTips = GUILayout.Toggle(SMSettings.ShowToolTips, guiLabel, GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);

            GUI.enabled = SMSettings.ShowToolTips;

            GUILayout.BeginHorizontal();
            label = "Debugger Window Tool Tips";
            toolTip = "Allows tooltips to be turned on or off for the Debugger Window only\r\nRequires global ToolTips setting to be enabled.";
            guiLabel = new GUIContent(label, toolTip);
            GUILayout.Space(20);
            WindowDebugger.ShowToolTips = GUILayout.Toggle(WindowDebugger.ShowToolTips, guiLabel, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);
            GUILayout.BeginHorizontal();
            label = "Manifest Window Tool Tips";
            toolTip = "Allows tooltips to be turned on or off for the Debugger Window only\r\nRequires global ToolTips setting to be enabled.";
            guiLabel = new GUIContent(label, toolTip);
            GUILayout.Space(20);
            WindowManifest.ShowToolTips = GUILayout.Toggle(WindowManifest.ShowToolTips, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);
            GUILayout.BeginHorizontal();
            label = "Transfer Window Tool Tips";
            toolTip = "Allows tooltips to be turned on or off for the Manifest Window only\r\nRequires global ToolTips setting to be enabled.";
            guiLabel = new GUIContent(label, toolTip);
            GUILayout.Space(20);
            WindowTransfer.ShowToolTips = GUILayout.Toggle(WindowTransfer.ShowToolTips, guiLabel, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);
            GUILayout.BeginHorizontal();
            label = "Settings Window Tool Tips";
            toolTip = "Allows tooltips to be turned on or off for the Settings Window only\r\nRequires global ToolTips setting to be enabled.";
            guiLabel = new GUIContent(label, toolTip);
            GUILayout.Space(20);
            WindowSettings.ShowToolTips = GUILayout.Toggle(WindowSettings.ShowToolTips, guiLabel, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);
            GUILayout.BeginHorizontal();
            label = "Roster Window Tool Tips";
            toolTip = "Allows tooltips to be turned on or off for the Roster Window only\r\nRequires global ToolTips setting to be enabled.";
            guiLabel = new GUIContent(label, toolTip);
            GUILayout.Space(20);
            WindowRoster.ShowToolTips = GUILayout.Toggle(WindowRoster.ShowToolTips, guiLabel, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);
            GUILayout.BeginHorizontal();
            label = "Control Window Tool Tips";
            toolTip = "Allows tooltips to be turned on or off for the Control Window only\r\nRequires global ToolTips setting to be enabled.\r\nIf disabled, tooltips in all tabs will also be disabled.";
            guiLabel = new GUIContent(label, toolTip);
            GUILayout.Space(20);
            WindowControl.ShowToolTips = GUILayout.Toggle(WindowControl.ShowToolTips, guiLabel, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);

            GUI.enabled = SMSettings.ShowToolTips && WindowControl.ShowToolTips;

            GUILayout.BeginHorizontal();
            label = "Hatch Tab Tool Tips";
            toolTip = "Allows tooltips to be turned on or off for the Control Window's Hatch Tab only\r\nRequires global ToolTips setting to be enabled.\r\nRequires Control Window tooltips to be enabled.";
            guiLabel = new GUIContent(label, toolTip);
            GUILayout.Space(40);
            TabHatch.ShowToolTips = GUILayout.Toggle(TabHatch.ShowToolTips, guiLabel, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);
            GUILayout.BeginHorizontal();
            label = "Solar Tab Window Tool Tips";
            toolTip = "Allows tooltips to be turned on or off for the Control Window's Solar Panels Tab only\r\nRequires global ToolTips setting to be enabled.\r\nRequires Control Window tooltips to be enabled.";
            guiLabel = new GUIContent(label, toolTip);
            GUILayout.Space(40);
            TabSolarPanel.ShowToolTips = GUILayout.Toggle(TabSolarPanel.ShowToolTips, guiLabel, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);
            GUILayout.BeginHorizontal();
            label = "Antenna Tab Tool Tips";
            toolTip = "Allows tooltips to be turned on or off for the Control Window's Antennas Tab only\r\nRequires global ToolTips setting to be enabled.\r\nRequires Control Window tooltips to be enabled.";
            guiLabel = new GUIContent(label, toolTip);
            GUILayout.Space(40);
            TabAntenna.ShowToolTips = GUILayout.Toggle(TabAntenna.ShowToolTips, guiLabel, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);
            GUILayout.BeginHorizontal();
            label = "Light Tab Tool Tips";
            toolTip = "Allows tooltips to be turned on or off for the Control Window's Lights Tab only\r\nRequires global ToolTips setting to be enabled.\r\nRequires Control Window tooltips to be enabled.";
            guiLabel = new GUIContent(label, toolTip);
            GUILayout.Space(40);
            TabLight.ShowToolTips = GUILayout.Toggle(TabLight.ShowToolTips, guiLabel, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);

            GUI.enabled = true;
        }

        private static void DisplaySounds()
        {
            GUILayout.Label("Sounds", SMStyle.LabelTabHeader);
            GUILayout.Label("____________________________________________________________________________________________", SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(350));

            GUILayout.Label("Transfer Pump:", GUILayout.Height(20));

            // Pump Start Sound
            GUILayout.BeginHorizontal();
            label = "Pump Starting: ";
            toolTip = "Folder location where Pump Starting sound is stored.\r\nChange to point to your own custom sounds if desired.";
            guiLabel = new GUIContent(label, toolTip);
            GUILayout.Label(guiLabel, GUILayout.Width(100));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);
            SMSettings.PumpSoundStart = GUILayout.TextField(SMSettings.PumpSoundStart, GUILayout.Width(220));
            GUILayout.EndHorizontal();

            // Pump Run Sound
            GUILayout.BeginHorizontal();
            label = "Pump Running: ";
            toolTip = "Folder location where Pump Running sound is stored.\r\nChange to point to your own custom sounds if desired.";
            guiLabel = new GUIContent(label, toolTip);
            GUILayout.Label(guiLabel, GUILayout.Width(100));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);
            SMSettings.PumpSoundRun = GUILayout.TextField(SMSettings.PumpSoundRun, GUILayout.Width(220));
            GUILayout.EndHorizontal();

            // Pump Stop Sound
            GUILayout.BeginHorizontal();
            label = "Pump Stopping: ";
            toolTip = "Folder location where Pump Stopping sound is stored.\r\nChange to point to your own custom sounds if desired.";
            guiLabel = new GUIContent(label, toolTip);
            GUILayout.Label(guiLabel, GUILayout.Width(100));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);
            SMSettings.PumpSoundStop = GUILayout.TextField(SMSettings.PumpSoundStop, GUILayout.Width(220));
            GUILayout.EndHorizontal();

            GUILayout.Label(" ", GUILayout.Height(10));
            GUILayout.Label("Crew:", GUILayout.Height(20));
            // Crew Start Sound
            GUILayout.BeginHorizontal();
            label = "Crew Exiting: ";
            toolTip = "Folder location where Crew Exiting their seat sound is stored.\r\nChange to point to your own custom sounds if desired.";
            guiLabel = new GUIContent(label, toolTip);
            GUILayout.Label(guiLabel, GUILayout.Width(100));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);
            SMSettings.CrewSoundStart = GUILayout.TextField(SMSettings.CrewSoundStart, GUILayout.Width(220));
            GUILayout.EndHorizontal();

            // Crew Run Sound
            GUILayout.BeginHorizontal();
            label = "Crew Xfering: ";
            toolTip = "Folder location where Crew transferring sound is stored.\r\nChange to point to your own custom sounds if desired.";
            guiLabel = new GUIContent(label, toolTip);
            GUILayout.Label(guiLabel, GUILayout.Width(100));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);
            SMSettings.CrewSoundRun = GUILayout.TextField(SMSettings.CrewSoundRun, GUILayout.Width(220));
            GUILayout.EndHorizontal();

            // Crew Stop Sound
            GUILayout.BeginHorizontal();
            label = "Crew Entering: ";
            toolTip = "Folder location where Crew Entering sound is stored.\r\nChange to point to your own custom sounds if desired.";
            guiLabel = new GUIContent(label, toolTip);
            GUILayout.Label(guiLabel, GUILayout.Width(100));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);
            SMSettings.CrewSoundStop = GUILayout.TextField(SMSettings.CrewSoundStop, GUILayout.Width(220));
            GUILayout.EndHorizontal();
        }

        private static void DisplayConfig()
        {
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
            toolTip = "Switches the toolbar Icons over to Blizzy's toolbar, if installed.\r\nIf Blizzy's toolbar is not installed, option is not selectable.";
            guiLabel = new GUIContent(label, toolTip);
            SMSettings.EnableBlizzyToolbar = GUILayout.Toggle(SMSettings.EnableBlizzyToolbar, guiLabel, GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);

            GUI.enabled = true;
            // UnityStyle Mode
            label = "Enable Unity Style GUI Interface";
            toolTip = "Switches the toolbar Icons over to Blizzy's toolbar, if installed.\r\nIf Blizzy's toolbar is not installed, option is not selectable.";
            guiLabel = new GUIContent(label, toolTip);
            SMSettings.UseUnityStyle = GUILayout.Toggle(SMSettings.UseUnityStyle, guiLabel, GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);
            if (SMSettings.UseUnityStyle != SMSettings.prevUseUnityStyle)
                SMStyle.WindowStyle = null;

            label = "Enable Debug Window";
            toolTip = "Turns on or off the SM Debug window to allow viewing of log entries or errors generated by the mod.";
            guiLabel = new GUIContent(label, toolTip);
            WindowDebugger.ShowWindow = GUILayout.Toggle(WindowDebugger.ShowWindow, guiLabel, GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);

            label = "Enable Verbose Logging";
            toolTip = "Turns on or off Expanded logging to aid in troubleshooting issues in Ship Manifest";
            guiLabel = new GUIContent(label, toolTip);
            SMSettings.VerboseLogging = GUILayout.Toggle(SMSettings.VerboseLogging, guiLabel, GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);

            label = "Enable SM Debug Window On Error";
            toolTip = "When enabled, caused Ship Manifest to automatically display the SM Debug window when an error is generated.\r\nTroubleshooting aid.";
            guiLabel = new GUIContent(label, toolTip);
            SMSettings.AutoDebug = GUILayout.Toggle(SMSettings.AutoDebug, guiLabel, GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);

            label = "Save Error log on Exit";
            toolTip = "When enabled, caused Ship Manifest to automatically save the SM debug log when the program exits.\r\nTroubleshooting aid.";
            guiLabel = new GUIContent(label, toolTip);
            SMSettings.SaveLogOnExit = GUILayout.Toggle(SMSettings.SaveLogOnExit, guiLabel, GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);

            // create Limit Error Log Length slider;
            GUILayout.BeginHorizontal();
            label = "Error Log Length: ";
            toolTip = "Sets the maximum number of error entries stored in the log.\r\nAdditional entries will cause initial entries to be removed from the log (rolling).\r\nSetting this value to '0' will allow inlimited entries.";
            guiLabel = new GUIContent(label, toolTip);
            GUILayout.Label(guiLabel, GUILayout.Width(140));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);
            SMSettings.ErrorLogLength = GUILayout.TextField(SMSettings.ErrorLogLength, GUILayout.Width(40));
            GUILayout.Label("(lines)", GUILayout.Width(50));
            GUILayout.EndHorizontal();

            label = "Enable Kerbal Renaming";
            toolTip = "Allows renaming a Kerbal.  The Profession may change when the kerbal is renamed.";
            guiLabel = new GUIContent(label, toolTip);
            SMSettings.EnableKerbalRename = GUILayout.Toggle(SMSettings.EnableKerbalRename, guiLabel, GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);

            if (!SMSettings.EnableKerbalRename)
                GUI.enabled = false;
            GUILayout.BeginHorizontal();
            label = "Rename and Keep Profession (Experimental)";
            toolTip = "When On, SM will remember the selected profesison when Kerbal is Renamed.\r\nAdds non printing chars to Kerbal name in your game save.\r\n(Should be no issue, but use at your own risk.)";
            guiLabel = new GUIContent(label, toolTip);
            GUILayout.Space(20);
            SMSettings.RenameWithProfession = GUILayout.Toggle(SMSettings.RenameWithProfession, guiLabel, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);
            GUI.enabled = true;

            label = "Enable AutoSave Settings";
            toolTip = "When On, SM will automatically save any changes made to settings on a regular interval.";
            guiLabel = new GUIContent(label, toolTip);
            SMSettings.AutoSave = GUILayout.Toggle(SMSettings.AutoSave, guiLabel, GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);

            GUILayout.BeginHorizontal();
            label = "Save Interval: ";
            toolTip = "Sets the time (in seconds) between automatic saves when Autosave Settings is enabled.";
            guiLabel = new GUIContent(label, toolTip);
            GUILayout.Label(guiLabel, GUILayout.Width(120));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - ScrollViewerPosition.y);
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
