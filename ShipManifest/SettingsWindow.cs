using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace ShipManifest
{
    public static class SettingsWindow
    {
        #region Settings Window (GUI)

        public static string ToolTip = "";

        private static Vector2 ScrollViewerSettings = Vector2.zero;
        public static void Display(int windowId)
        {
            Rect rect = new Rect(371, 4, 16, 16);
            if (GUI.Button(rect, new GUIContent("", "Close Window")))
            {
                Settings.ShowSettings = false;
                ShipManifestAddon.ToggleToolbar();
                ToolTip = "";
            }
            if (Event.current.type == EventType.Repaint)
                ToolTip = Utilities.SetUpToolTip(rect, Settings.SettingsPosition, GUI.tooltip);

            // Store settings in case we cancel later...
            Settings.StoreTempSettings();

            string label = "";
            string txtSaveInterval = Settings.SaveIntervalSec.ToString();

            GUILayout.BeginVertical();
            ScrollViewerSettings = GUILayout.BeginScrollView(ScrollViewerSettings, GUILayout.Height(280), GUILayout.Width(375));
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
            label = "Enable Realism Mode";
            Settings.RealismMode = GUILayout.Toggle(Settings.RealismMode, label, GUILayout.Width(300));

            // EnableCrew Mode
            GUI.enabled = isEnabled;
            GUILayout.BeginHorizontal();
            label = "Enable Crew Xfers";
            Settings.EnableCrew = GUILayout.Toggle(Settings.EnableCrew, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();

            if (!Settings.EnableCrew)
            {
                if (ShipManifestAddon.smController.SelectedResource == "Crew")
                {
                    // Clear Resource selection.
                    ShipManifestAddon.smController.SelectedResource = null;
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
            label = "Enable CLS  (Connected Living Spaces)";
            Settings.EnableCLS = GUILayout.Toggle(Settings.EnableCLS, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();

            if (Settings.EnableCLS != Settings.prevEnableCLS && ShipManifestAddon.smController.SelectedResource == "Crew")
            {
                // Update spaces and reassign the resource to observe new settings.
                ShipManifestAddon.smController.UpdateCLSSpaces();
                ShipManifestAddon.smController.SelectedResource = "Crew";
            }

            // EnableScience Mode
            GUILayout.BeginHorizontal();
            GUI.enabled = isEnabled;
            label = "Enable Science Xfers";
            Settings.EnableScience = GUILayout.Toggle(Settings.EnableScience, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();

            if (!Settings.EnableScience)
            {
                // Clear Resource selection.
                if (ShipManifestAddon.smController.SelectedResource == "Science")
                    ShipManifestAddon.smController.SelectedResource = null;
            }

            // EnableResources Mode
            GUILayout.BeginHorizontal();
            GUI.enabled = isEnabled;
            label = "Enable Resource Xfers";
            Settings.EnableResources = GUILayout.Toggle(Settings.EnableResources, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();

            if (!Settings.EnableResources)
            {
                // Clear Resource selection.
                if (ShipManifestAddon.smController.SelectedResource == "Resources")
                    ShipManifestAddon.smController.SelectedResource = null;
            }

            // EnablePFResources Mode
            GUILayout.BeginHorizontal();
            GUI.enabled = isEnabled;
            label = "Enable Resources in Pre-Flight";
            Settings.EnablePFResources = GUILayout.Toggle(Settings.EnablePFResources, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();

            // LockSettings Mode
            GUI.enabled = isEnabled;
            label = "Lock Settings  (If set ON, disable in config file)";
            Settings.LockSettings = GUILayout.Toggle(Settings.LockSettings, label, GUILayout.Width(300));

            GUILayout.Label("-------------------------------------------------------------------", GUILayout.Height(10));
            GUILayout.Label("Highlighting & ToolTips", GUILayout.Height(10));
            GUILayout.Label("-------------------------------------------------------------------", GUILayout.Height(16));

            // EnableHighlighting Mode
            GUI.enabled = true;
            GUILayout.BeginHorizontal();
            label = "Enable Highlighting";
            Settings.EnableHighlighting = GUILayout.Toggle(Settings.EnableHighlighting, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();

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

            if (!Settings.EnableHighlighting || !Settings.EnableCLS)
                GUI.enabled = false;
            else
                GUI.enabled = true;
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            label = "Enable CLS Highlighting";
            Settings.EnableCLSHighlighting = GUILayout.Toggle(Settings.EnableCLSHighlighting, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            if (Settings.EnableCLS && ShipManifestAddon.smController.SelectedResource == "Crew" && Settings.ShowTransferWindow)
            {
                if (Settings.EnableCLSHighlighting != Settings.prevEnableCLSHighlighting)
                    ManifestController.HighlightCLSVessel(Settings.EnableCLSHighlighting);
            }

            // Enable Tool Tips
            GUI.enabled = true;
            label = "Enable Tool Tips";
            Settings.ShowToolTips = GUILayout.Toggle(Settings.ShowToolTips, label, GUILayout.Width(300));

            GUI.enabled = true;
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

            // create xfer Flow Rate slider;
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Flow Rate:  {0}", Settings.FlowRate.ToString("#######0.####")), GUILayout.Width(100), GUILayout.Height(20));
            Settings.FlowRate = GUILayout.HorizontalSlider(Settings.FlowRate, Settings.MinFlowRate, Settings.MaxFlowRate, GUILayout.Width(220));
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

            GUILayout.Label("-------------------------------------------------------------------", GUILayout.Height(10));
            GUILayout.Label("Configuraton", GUILayout.Height(10));
            GUILayout.Label("-------------------------------------------------------------------", GUILayout.Height(16));

            label = "Enable SM Debug Window On Error";
            Settings.AutoDebug = GUILayout.Toggle(Settings.AutoDebug, label, GUILayout.Width(300));

            label = "Enable Debug Window";
            Settings.ShowDebugger = GUILayout.Toggle(Settings.ShowDebugger, label, GUILayout.Width(300));

            label = "Enable Verbose Logging";
            Settings.VerboseLogging = GUILayout.Toggle(Settings.VerboseLogging, label, GUILayout.Width(300));

            label = "Save Error log on Exit";
            Settings.SaveLogOnExit = GUILayout.Toggle(Settings.SaveLogOnExit, label, GUILayout.Width(300));

            // create Limit Error Log Length slider;
            GUILayout.BeginHorizontal();
            GUILayout.Label("Error Log Length: ", GUILayout.Width(140));
            Settings.ErrorLogLength = GUILayout.TextField(Settings.ErrorLogLength, GUILayout.Width(40));
            GUILayout.Label("(lines)", GUILayout.Width(50));
            GUILayout.EndHorizontal();

            // TextureReplacer Mode
            label = "Enable Texture Replacer Events";
            Settings.EnableTextureReplacer = GUILayout.Toggle(Settings.EnableTextureReplacer, label, GUILayout.Width(300));

            if (!ToolbarManager.ToolbarAvailable)
            {
                if (Settings.EnableBlizzyToolbar)
                    Settings.EnableBlizzyToolbar = false;
                GUI.enabled = false;
            }
            else
                GUI.enabled = isEnabled;

            label = "Enable Blizzy Toolbar (requires game restart)";
            Settings.EnableBlizzyToolbar = GUILayout.Toggle(Settings.EnableBlizzyToolbar, label, GUILayout.Width(300));

            GUI.enabled = isEnabled;
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
            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
        }

        #endregion
    }
}
