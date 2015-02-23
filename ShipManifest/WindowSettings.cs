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

        internal static string ToolTip = "";
        internal static bool ToolTipActive = false;
        internal static bool ShowToolTips = Settings.SettingsToolTips;

        private static Vector2 ScrollViewerPosition = Vector2.zero;
        internal static void Display(int windowId)
        {
            // Reset Tooltip active flag...
            Rect rect = new Rect();
            ToolTipActive = false;
            ShowToolTips = Settings.SettingsToolTips;

            rect = new Rect(371, 4, 16, 16);
            if (GUI.Button(rect, new GUIContent("", "Close Window")))
            {
                ToolTip = "";
                if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                    SMAddon.OnSMSettingsToggle();
                else
                    Settings.ShowSettings = false;

            }
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Settings.SettingsPosition, GUI.tooltip, ref ToolTipActive, 0, 0);

            // Store settings in case we cancel later...
            Settings.StoreTempSettings();

            GUILayout.BeginVertical();

            DisplayOptions();

            DisplayHighlighting();

            DisplayToolTips();

            DisplaySounds();

            DisplayConfiguration();

            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
        }

        private static void DisplayConfiguration()
        {
            Rect rect = new Rect();
            string label = "";
            string toolTip = "";

            string txtSaveInterval = Settings.SaveIntervalSec.ToString();
            GUILayout.Label("-------------------------------------------------------------------", GUILayout.Height(10));
            GUILayout.Label("Configuraton", GUILayout.Height(10));
            GUILayout.Label("-------------------------------------------------------------------", GUILayout.Height(16));

            if (!ToolbarManager.ToolbarAvailable)
            {
                if (Settings.EnableBlizzyToolbar)
                    Settings.EnableBlizzyToolbar = false;
                GUI.enabled = false;
            }
            else
                GUI.enabled = true;

            label = "Enable Blizzy Toolbar (Replaces Stock Toolbar)";
            Settings.EnableBlizzyToolbar = GUILayout.Toggle(Settings.EnableBlizzyToolbar, label, GUILayout.Width(300));

            GUI.enabled = true;
            // TextureReplacer Mode
            label = "Enable Texture Replacer Events";
            Settings.EnableTextureReplacer = GUILayout.Toggle(Settings.EnableTextureReplacer, label, GUILayout.Width(300));

            label = "Enable Debug Window";
            Settings.ShowDebugger = GUILayout.Toggle(Settings.ShowDebugger, label, GUILayout.Width(300));

            label = "Enable Verbose Logging";
            Settings.VerboseLogging = GUILayout.Toggle(Settings.VerboseLogging, label, GUILayout.Width(300));

            label = "Enable SM Debug Window On Error";
            Settings.AutoDebug = GUILayout.Toggle(Settings.AutoDebug, label, GUILayout.Width(300));

            label = "Save Error log on Exit";
            Settings.SaveLogOnExit = GUILayout.Toggle(Settings.SaveLogOnExit, label, GUILayout.Width(300));

            // create Limit Error Log Length slider;
            GUILayout.BeginHorizontal();
            GUILayout.Label("Error Log Length: ", GUILayout.Width(140));
            Settings.ErrorLogLength = GUILayout.TextField(Settings.ErrorLogLength, GUILayout.Width(40));
            GUILayout.Label("(lines)", GUILayout.Width(50));
            GUILayout.EndHorizontal();

            label = "Enable Kerbal Renaming";
            toolTip = "Allows renaming a Kerbal.  The Profession may change when the kerbal is renamed.";
            Settings.EnableKerbalRename = GUILayout.Toggle(Settings.EnableKerbalRename, new GUIContent(label, toolTip), GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Settings.SettingsPosition, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);

            if (!Settings.EnableKerbalRename)
                GUI.enabled = false;
            GUILayout.BeginHorizontal();
            label = "Rename and Keep Profession (Experimental)";
            toolTip = "When On, SM will remember the selected profesison when Kerbal is Renamed.\r\nAdds non printing chars to Kerbal name in your game save.\r\n(Should be no issue, but use at your own risk.)";
            GUILayout.Space(20);
            Settings.RenameWithProfession = GUILayout.Toggle(Settings.RenameWithProfession, new GUIContent(label, toolTip), GUILayout.Width(300));
            GUILayout.EndHorizontal();
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Settings.SettingsPosition, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);
            GUI.enabled = true;

            label = "Enable AutoSave Settings";
            Settings.AutoSave = GUILayout.Toggle(Settings.AutoSave, label, GUILayout.Width(300));

            GUILayout.BeginHorizontal();
            GUILayout.Label("Save Interval: ", GUILayout.Width(120));
            txtSaveInterval = GUILayout.TextField(txtSaveInterval, GUILayout.Width(40));
            GUILayout.Label("(sec)", GUILayout.Width(40));
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save"))
            {
                Settings.SaveIntervalSec = float.Parse(txtSaveInterval);
                Settings.Save();
                Settings.ShowSettings = false;
            }
            if (GUILayout.Button("Cancel"))
            {
                // We've canclled, so restore original settings.
                Settings.RestoreTempSettings();
                Settings.ShowSettings = false;
            }
            GUILayout.EndHorizontal();
        }

        private static void DisplaySounds()
        {
            GUILayout.Label("-------------------------------------------------------------------", GUILayout.Height(10));
            GUILayout.Label("Sounds", GUILayout.Height(10));
            GUILayout.Label("-------------------------------------------------------------------", GUILayout.Height(16));

            GUILayout.Label("Transfer Pump:", GUILayout.Height(20));

            // Pump Start Sound
            GUILayout.BeginHorizontal();
            GUILayout.Label("Pump Starting: ", GUILayout.Width(100));
            Settings.PumpSoundStart = GUILayout.TextField(Settings.PumpSoundStart, GUILayout.Width(220));
            GUILayout.EndHorizontal();

            // Pump Run Sound
            GUILayout.BeginHorizontal();
            GUILayout.Label("Pump Running: ", GUILayout.Width(100));
            Settings.PumpSoundRun = GUILayout.TextField(Settings.PumpSoundRun, GUILayout.Width(220));
            GUILayout.EndHorizontal();

            // Pump Stop Sound
            GUILayout.BeginHorizontal();
            GUILayout.Label("Pump Stopping: ", GUILayout.Width(100));
            Settings.PumpSoundStop = GUILayout.TextField(Settings.PumpSoundStop, GUILayout.Width(220));
            GUILayout.EndHorizontal();

            GUILayout.Label(" ", GUILayout.Height(10));
            GUILayout.Label("Crew:", GUILayout.Height(20));
            // Crew Start Sound
            GUILayout.BeginHorizontal();
            GUILayout.Label("Crew Exiting: ", GUILayout.Width(100));
            Settings.CrewSoundStart = GUILayout.TextField(Settings.CrewSoundStart, GUILayout.Width(220));
            GUILayout.EndHorizontal();

            // Crew Run Sound
            GUILayout.BeginHorizontal();
            GUILayout.Label("Crew Xfering: ", GUILayout.Width(100));
            Settings.CrewSoundRun = GUILayout.TextField(Settings.CrewSoundRun, GUILayout.Width(220));
            GUILayout.EndHorizontal();

            // Crew Stop Sound
            GUILayout.BeginHorizontal();
            GUILayout.Label("Crew Entering: ", GUILayout.Width(100));
            Settings.CrewSoundStop = GUILayout.TextField(Settings.CrewSoundStop, GUILayout.Width(220));
            GUILayout.EndHorizontal();
        }

        private static void DisplayToolTips()
        {
            // Enable Tool Tips
            string label = "";
            GUI.enabled = true;
            GUILayout.Label("-------------------------------------------------------------------", GUILayout.Height(10));
            GUILayout.Label("ToolTips", GUILayout.Height(10));
            GUILayout.Label("-------------------------------------------------------------------", GUILayout.Height(16));

            label = "Enable Tool Tips";
            Settings.ShowToolTips = GUILayout.Toggle(Settings.ShowToolTips, label, GUILayout.Width(300));

            GUI.enabled = Settings.ShowToolTips;
            label = "Manifest Window Tool Tips";
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            Settings.ManifestToolTips = GUILayout.Toggle(Settings.ManifestToolTips, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            label = "Transfer Window Tool Tips";
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            Settings.TransferToolTips = GUILayout.Toggle(Settings.TransferToolTips, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            label = "Settings Window Tool Tips";
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            Settings.SettingsToolTips = GUILayout.Toggle(Settings.SettingsToolTips, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            label = "Roster Window Tool Tips";
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            Settings.RosterToolTips = GUILayout.Toggle(Settings.RosterToolTips, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            label = "Hatch Window Tool Tips";
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            Settings.HatchToolTips = GUILayout.Toggle(Settings.HatchToolTips, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            label = "Debugger Window Tool Tips";
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            Settings.DebuggerToolTips = GUILayout.Toggle(Settings.DebuggerToolTips, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            GUI.enabled = true;
        }

        private static void DisplayHighlighting()
        {
            string label = "";
            GUI.enabled = true;
            GUILayout.Label("-------------------------------------------------------------------", GUILayout.Height(10));
            GUILayout.Label("Highlighting", GUILayout.Height(10));
            GUILayout.Label("-------------------------------------------------------------------", GUILayout.Height(16));

            // EnableHighlighting Mode
            GUILayout.BeginHorizontal();
            label = "Enable Highlighting";
            Settings.EnableHighlighting = GUILayout.Toggle(Settings.EnableHighlighting, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            if (Settings.EnableHighlighting != Settings.prevEnableHighlighting && HighLogic.LoadedSceneIsFlight)
            {
                if (Settings.EnableCLS)
                {
                    if (SMAddon.smController.SelectedResource == "Crew")
                    {
                        SMAddon.HighlightCLSVessel(Settings.EnableHighlighting, true);
                        // Update spaces and reassign the resource to observe new settings.
                        SMAddon.UpdateCLSSpaces();
                        SMAddon.smController.SelectedResource = "Crew";
                    }
                }
            }

            // OnlySourceTarget Mode
            GUI.enabled = true;
            if (!Settings.EnableHighlighting)
                GUI.enabled = false;
            else
                GUI.enabled = true;
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            label = "Highlight Only Source / Target Parts";
            Settings.OnlySourceTarget = GUILayout.Toggle(Settings.OnlySourceTarget, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            if (Settings.OnlySourceTarget && !Settings.prevOnlySourceTarget)
            {
                Settings.EnableCLSHighlighting = false;
                if (HighLogic.LoadedSceneIsFlight && Settings.EnableCLS && SMAddon.smController.SelectedResource == "Crew")
                {
                    SMAddon.HighlightCLSVessel(false, true);
                    // Update spaces and reassign the resource to observe new settings.
                    SMAddon.UpdateCLSSpaces();
                    SMAddon.smController.SelectedResource = "Crew";
                }
            }
            if (!Settings.EnableHighlighting || !Settings.EnableCLS)
                GUI.enabled = false;
            else
                GUI.enabled = true;
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            label = "Enable CLS Highlighting";
            Settings.EnableCLSHighlighting = GUILayout.Toggle(Settings.EnableCLSHighlighting, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            if (Settings.EnableCLSHighlighting && !Settings.prevEnableCLSHighlighting)
                Settings.OnlySourceTarget = false;
            if (HighLogic.LoadedSceneIsFlight && Settings.EnableCLS && SMAddon.smController.SelectedResource == "Crew" && Settings.ShowTransferWindow)
            {
                if (Settings.EnableCLSHighlighting != Settings.prevEnableCLSHighlighting)
                    SMAddon.HighlightCLSVessel(Settings.EnableCLSHighlighting);
            }
        }

        private static void DisplayOptions()
        {
            Rect rect = new Rect();
            ScrollViewerPosition = GUILayout.BeginScrollView(ScrollViewerPosition, GUILayout.Height(280), GUILayout.Width(375));
            GUILayout.BeginVertical();
            GUI.enabled = true;
            GUILayout.Label("-------------------------------------------------------------------", GUILayout.Height(10));
            if (!Settings.LockSettings)
                GUILayout.Label("Settings / Options", GUILayout.Height(10));
            else
                GUILayout.Label("Settings / Options  (Locked.  Unlock in Config file)", GUILayout.Height(10));
            GUILayout.Label("-------------------------------------------------------------------", GUILayout.Height(16));

            bool isEnabled = (!Settings.LockSettings);
            // Realism Mode
            GUI.enabled = isEnabled;
            GUIContent guiLabel = new GUIContent("Enable Realism Mode","Turns on/off Realism Mode.\r\nWhen ON, causes changes in the interface and limits\r\nyour freedom to things that would not be 'Realistic'.\r\nWhen Off, Allows Fills, Dumps, Repeating Science, instantaneous Xfers, Crew Xfers anywwhere, etc.");
            Settings.RealismMode = GUILayout.Toggle(Settings.RealismMode, guiLabel, GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Settings.SettingsPosition, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);

            // EnableCrew Mode
            GUI.enabled = isEnabled;
            GUILayout.BeginHorizontal();
            guiLabel = new GUIContent("Enable Crew Xfers","Turns on/off Crew transfers.\r\nWhen ON, The Crew option will appear in your resource list.\r\nWhen Off, Crew transfers are not possible.");
            Settings.EnableCrew = GUILayout.Toggle(Settings.EnableCrew, guiLabel, GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Settings.SettingsPosition, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);

            GUILayout.EndHorizontal();

            if (!Settings.EnableCrew && HighLogic.LoadedSceneIsFlight)
            {
                if (SMAddon.smController.SelectedResource == "Crew")
                {
                    // Clear Resource selection.
                    SMAddon.smController.SelectedResource = null;
                    Settings.ShowTransferWindow = false;
                }
            }

            // EnableCLS Mode
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            if (!Settings.EnableCrew || !Settings.CLSInstalled)
                GUI.enabled = false;
            else
                GUI.enabled = isEnabled;
            guiLabel = new GUIContent("Enable CLS  (Connected Living Spaces)", "Turns on/off Connected Living space support.\r\nWhen on, Crew can only be xfered to a part in the same 'Living Space'.\r\nWhen Off, Crew transfers are possible to any part that can hold a kerbal.");
            Settings.EnableCLS = GUILayout.Toggle(Settings.EnableCLS, guiLabel, GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Settings.SettingsPosition, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);
            GUILayout.EndHorizontal();

            if (Settings.EnableCLS != Settings.prevEnableCLS && HighLogic.LoadedSceneIsFlight)
            {
                if (!Settings.EnableCLS)
                    SMAddon.HighlightCLSVessel(false, true);
                else if (SMAddon.smController.SelectedResource == "Crew")
                {
                    // Update spaces and reassign the resource to observe new settings.
                    SMAddon.UpdateCLSSpaces();
                    SMAddon.smController.SelectedResource = "Crew";
                }
            }

            // EnableScience Mode
            GUILayout.BeginHorizontal();
            GUI.enabled = isEnabled;
            guiLabel = new GUIContent("Enable Science Xfers", "Turns on/off Science Xfers.\r\nWhen on, Science transfers are possible and show up in the Resource list.\r\nWhen Off, Science transfers will not appear in the resource list.");
            Settings.EnableScience = GUILayout.Toggle(Settings.EnableScience, guiLabel, GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Settings.SettingsPosition, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);
            GUILayout.EndHorizontal();

            if (!Settings.EnableScience && HighLogic.LoadedSceneIsFlight)
            {
                // Clear Resource selection.
                if (SMAddon.smController.SelectedResource == "Science")
                    SMAddon.smController.SelectedResource = null;
            }

            // EnableResources Mode
            GUILayout.BeginHorizontal();
            GUI.enabled = isEnabled;
            guiLabel = new GUIContent("Enable Resource Xfers", "Turns on/off Resource Xfers.\r\nWhen on, Resource transfers are possible and show up in the Resource list.\r\nWhen Off, Resources (fuel, monoprpellent, etc) will not appear in the resource list.");
            Settings.EnableResources = GUILayout.Toggle(Settings.EnableResources, guiLabel, GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Settings.SettingsPosition, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);
            GUILayout.EndHorizontal();

            if (!Settings.EnableResources && HighLogic.LoadedSceneIsFlight)
            {
                // Clear Resource selection.
                if (SMAddon.smController.SelectedResource == "Resources")
                    SMAddon.smController.SelectedResource = null;
            }

            // EnablePFResources Mode
            GUILayout.BeginHorizontal();
            GUI.enabled = isEnabled;
            guiLabel = new GUIContent("Enable Resources in Pre-Flight", "Turns on/off Fill and Empty Resources when in preflight.\r\nWhen on, Resource Fill and Dump resources vessel wide are possible and show up in the Resource list.\r\nWhen Off, Fill and Dump Resources vessel wide will not appear in the resource list.");
            Settings.EnablePFResources = GUILayout.Toggle(Settings.EnablePFResources, guiLabel, GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Settings.SettingsPosition, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);
            GUILayout.EndHorizontal();

            // create xfer Flow Rate slider;
            // Lets parse the string to allow decimal points.
            string strFlowRate = Settings.FlowRate.ToString();
            string strMinFlowRate = Settings.MinFlowRate.ToString();
            string strMaxFlowRate = Settings.MaxFlowRate.ToString();
            string strMaxFlowTime = Settings.MaxFlowTimeSec.ToString();

            float newRate = 0;

            GUILayout.BeginHorizontal();
            guiLabel = new GUIContent("Resource Flow Rate:","Sets the rate that resources Xfer when Realism Mode is on.\r\nThe higher the number the faster resources move.\r\nWhen on, Resource transfers are possible and show up in the Resource list.");
            GUILayout.Label(guiLabel, GUILayout.Width(130), GUILayout.Height(20));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Settings.SettingsPosition, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);
            strFlowRate = GUILayout.TextField(strFlowRate, 20, GUILayout.Height(20), GUILayout.Width(80));
            GUILayout.EndHorizontal();
            if (float.TryParse(strFlowRate, out newRate))
                Settings.FlowRate = (int)newRate;

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            Settings.FlowRate = GUILayout.HorizontalSlider(Settings.FlowRate, Settings.MinFlowRate, Settings.MaxFlowRate, GUILayout.Width(300));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            guiLabel = new GUIContent(" - Min Flow Rate:","Sets the lowest range (left side) on the Flow rate Slider control.");
            GUILayout.Label(guiLabel, GUILayout.Width(130), GUILayout.Height(20));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Settings.SettingsPosition, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);
            strMinFlowRate = GUILayout.TextField(strMinFlowRate, 20, GUILayout.Height(20), GUILayout.Width(80));
            GUILayout.EndHorizontal();
            if (float.TryParse(strMinFlowRate, out newRate))
                Settings.MinFlowRate = (int)newRate;

            GUILayout.BeginHorizontal();
            guiLabel = new GUIContent(" - Max Flow Rate:","Sets the highest range (right side) on the Flow rate Slider control.");
            GUILayout.Label(guiLabel, GUILayout.Width(130), GUILayout.Height(20));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Settings.SettingsPosition, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);
            strMaxFlowRate = GUILayout.TextField(strMaxFlowRate, 20, GUILayout.Height(20), GUILayout.Width(80));
            GUILayout.EndHorizontal();
            if (float.TryParse(strMaxFlowRate, out newRate))
                Settings.MaxFlowRate = (int)newRate;

            GUILayout.BeginHorizontal();
            guiLabel = new GUIContent(" - Max Flow Time:","Sets the maximum duration (in sec) of a resource transfer.\r\nWorks in conjunction with the Flow rate.  if time it would take\r\nto move a resource exceeds this number, this number will be used to calculate an adjusted flow rate.\r\n(protects your from 20 minute Xfers)");
            GUILayout.Label(guiLabel, GUILayout.Width(130), GUILayout.Height(20));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Settings.SettingsPosition, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);
            strMaxFlowTime = GUILayout.TextField(strMaxFlowTime, 20, GUILayout.Height(20), GUILayout.Width(80));
            GUILayout.EndHorizontal();
            if (float.TryParse(strMaxFlowTime, out newRate))
                Settings.MaxFlowTimeSec = (int)newRate;

            // LockSettings Mode
            GUI.enabled = isEnabled;
            guiLabel = new GUIContent("Lock Settings  (If set ON, disable in config file)","Locks the settings in this section so they cannot be altered in game.\r\nTo turn off Locking you MUST edit the Config.xml file.");
            Settings.LockSettings = GUILayout.Toggle(Settings.LockSettings, guiLabel, GUILayout.Width(300));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Settings.SettingsPosition, GUI.tooltip, ref ToolTipActive, 80, 0 - ScrollViewerPosition.y);
        }

        #endregion
    }
}
