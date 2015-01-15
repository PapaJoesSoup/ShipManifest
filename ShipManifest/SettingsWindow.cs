using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace ShipManifest
{
    public static partial class Settings
    {
        #region Settings Window (GUI)

        private static Vector2 ScrollViewerSettings = Vector2.zero;
        public static void SettingsWindow(int windowId)
        {
            if (GUI.Button(new Rect(371, 4, 16, 16), ""))
            {
                Settings.ShowSettings = false;
                ShipManifestAddon.ToggleToolbar();
            }
            // Store settings in case we cancel later...
            StoreTempSettings();

            string label = "";
            string txtSaveInterval = SaveIntervalSec.ToString();

            GUILayout.BeginVertical();
            ScrollViewerSettings = GUILayout.BeginScrollView(ScrollViewerSettings, GUILayout.Height(280), GUILayout.Width(375));
            GUILayout.BeginVertical();
            GUI.enabled = true;
            GUILayout.Label("-------------------------------------------------------------------", GUILayout.Height(10));
            if (!LockSettings)
                GUILayout.Label("Settings / Options", GUILayout.Height(10));
            else
                GUILayout.Label("Settings / Options  (Locked.  Unlock in Config file)", GUILayout.Height(10));
            GUILayout.Label("-------------------------------------------------------------------", GUILayout.Height(16));

            bool isEnabled = (!LockSettings);
            // Realism Mode
            GUI.enabled = isEnabled;
            label = "Enable Realism Mode";
            RealismMode = GUILayout.Toggle(RealismMode, label, GUILayout.Width(300));

            // EnableHighlighting Mode
            GUI.enabled = isEnabled;
            GUILayout.BeginHorizontal();
            label = "Enable Highlighting";
            EnableHighlighting = GUILayout.Toggle(EnableHighlighting, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();

            // OnlySourceTarget Mode
            GUI.enabled = isEnabled;
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            if (!EnableHighlighting)
                GUI.enabled = false;
            else
                GUI.enabled = isEnabled;
            label = "Highlight Only Source / Target Parts";
            OnlySourceTarget = GUILayout.Toggle(OnlySourceTarget, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();

            // EnableCrew Mode
            GUI.enabled = isEnabled;
            GUILayout.BeginHorizontal();
            label = "Enable Crew Xfers";
            EnableCrew = GUILayout.Toggle(EnableCrew, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();

            if (!EnableCrew)
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
            if (!EnableCrew || !Settings.CLSInstalled)
                GUI.enabled = false;
            else
                GUI.enabled = isEnabled;
            label = "Enable CLS  (Connected Living Spaces)";
            EnableCLS = GUILayout.Toggle(EnableCLS, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();

            if (!EnableCLS && prevEnableCLS)
            {
                if (ShipManifestAddon.smController.SelectedResource == "Crew")
                {
                    //Reassign the resource to observe new settings.
                    ShipManifestAddon.smController.SelectedResource = "Crew";
                }
            }
            else if (EnableCLS && !prevEnableCLS)
            {
                if (ShipManifestAddon.smController.SelectedResource == "Crew")
                {
                    //Refresh the clsVessel if needed.
                    ShipManifestAddon.smController.UpdateCLSSpaces();

                    //Reassign the resource to observe new settings.
                    ShipManifestAddon.smController.SelectedResource = "Crew";
                }
            }

            // EnableScience Mode
            GUILayout.BeginHorizontal();
            GUI.enabled = isEnabled;
            label = "Enable Science Xfers";
            EnableScience = GUILayout.Toggle(EnableScience, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();

            if (!EnableScience)
            {
                if (ShipManifestAddon.smController.SelectedResource == "Science")
                {
                    // Clear Resource selection.
                    ShipManifestAddon.smController.SelectedResource = null;
                }
            }

            // EnableResources Mode
            GUILayout.BeginHorizontal();
            GUI.enabled = isEnabled;
            label = "Enable Resource Xfers";
            EnableResources = GUILayout.Toggle(EnableResources, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();

            if (!EnableResources)
            {
                if (ShipManifestAddon.smController.SelectedResource == "Resources")
                {
                    // Clear Resource selection.
                    ShipManifestAddon.smController.SelectedResource = null;
                }
            }

            // EnablePFResources Mode
            GUILayout.BeginHorizontal();
            GUI.enabled = isEnabled;
            label = "Enable Resources in Pre-Flight";
            EnablePFResources = GUILayout.Toggle(EnablePFResources, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();

            // LockSettings Mode
            GUI.enabled = isEnabled;
            label = "Lock Settings  (If set ON, disable in config file)";
            LockSettings = GUILayout.Toggle(LockSettings, label, GUILayout.Width(300));

            GUI.enabled = true;
            GUILayout.Label("-------------------------------------------------------------------", GUILayout.Height(10));
            GUILayout.Label("Sounds", GUILayout.Height(10));
            GUILayout.Label("-------------------------------------------------------------------", GUILayout.Height(16));

            GUILayout.Label("Transfer Pump:", GUILayout.Height(20));
            // Pump Start Sound
            GUILayout.BeginHorizontal();
            GUILayout.Label("Pump Starting: ", GUILayout.Width(100));
            PumpSoundStart = GUILayout.TextField(PumpSoundStart, GUILayout.Width(220));
            GUILayout.EndHorizontal();

            // Pump Run Sound
            GUILayout.BeginHorizontal();
            GUILayout.Label("Pump Running: ", GUILayout.Width(100));
            PumpSoundRun = GUILayout.TextField(PumpSoundRun, GUILayout.Width(220));
            GUILayout.EndHorizontal();

            // Pump Stop Sound
            GUILayout.BeginHorizontal();
            GUILayout.Label("Pump Stopping: ", GUILayout.Width(100));
            PumpSoundStop = GUILayout.TextField(PumpSoundStop, GUILayout.Width(220));
            GUILayout.EndHorizontal();

            // create xfer Flow Rate slider;
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Flow Rate:  {0}", FlowRate.ToString("#######0.####")), GUILayout.Width(100), GUILayout.Height(20));
            FlowRate = GUILayout.HorizontalSlider(FlowRate, MinFlowRate, MaxFlowRate, GUILayout.Width(220));
            GUILayout.EndHorizontal();

            GUILayout.Label(" ", GUILayout.Height(10));
            GUILayout.Label("Crew:", GUILayout.Height(20));
            // Crew Start Sound
            GUILayout.BeginHorizontal();
            GUILayout.Label("Crew Exiting: ", GUILayout.Width(100));
            CrewSoundStart = GUILayout.TextField(CrewSoundStart, GUILayout.Width(220));
            GUILayout.EndHorizontal();

            // Crew Run Sound
            GUILayout.BeginHorizontal();
            GUILayout.Label("Crew Xfering: ", GUILayout.Width(100));
            CrewSoundRun = GUILayout.TextField(CrewSoundRun, GUILayout.Width(220));
            GUILayout.EndHorizontal();

            // Crew Stop Sound
            GUILayout.BeginHorizontal();
            GUILayout.Label("Crew Entering: ", GUILayout.Width(100));
            CrewSoundStop = GUILayout.TextField(CrewSoundStop, GUILayout.Width(220));
            GUILayout.EndHorizontal();

            GUILayout.Label("-------------------------------------------------------------------", GUILayout.Height(10));
            GUILayout.Label("Configuraton", GUILayout.Height(10));
            GUILayout.Label("-------------------------------------------------------------------", GUILayout.Height(16));

            // AutoDebug Mode
            label = "Enable SM Debug Window On Error";
            AutoDebug = GUILayout.Toggle(AutoDebug, label, GUILayout.Width(300));

            label = "Enable Debug Window";
            ShowDebugger = GUILayout.Toggle(ShowDebugger, label, GUILayout.Width(300));

            label = "Enable Verbose Logging";
            VerboseLogging = GUILayout.Toggle(VerboseLogging, label, GUILayout.Width(300));

            label = "Save Error log on Exit";
            SaveLogOnExit = GUILayout.Toggle(SaveLogOnExit, label, GUILayout.Width(300));

            // create Limit Error Log Length slider;
            GUILayout.BeginHorizontal();
            GUILayout.Label("Error Log Length: ", GUILayout.Width(140));
            ErrorLogLength = GUILayout.TextField(ErrorLogLength, GUILayout.Width(40));
            GUILayout.Label("(lines)", GUILayout.Width(50));
            GUILayout.EndHorizontal();

            // TextureReplacer Mode
            label = "Enable Texture Replacer Events";
            EnableTextureReplacer = GUILayout.Toggle(EnableTextureReplacer, label, GUILayout.Width(300));

            if (!ToolbarManager.ToolbarAvailable)
            {
                if (EnableBlizzyToolbar)
                    EnableBlizzyToolbar = false;
                GUI.enabled = false;
            }
            else
                GUI.enabled = isEnabled;

            label = "Enable Blizzy Toolbar (requires game restart)";
            EnableBlizzyToolbar = GUILayout.Toggle(EnableBlizzyToolbar, label, GUILayout.Width(300));

            GUI.enabled = isEnabled;
            label = "Enable AutoSave Settings";
            AutoSave = GUILayout.Toggle(AutoSave, label, GUILayout.Width(300));

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
                SaveIntervalSec = float.Parse(txtSaveInterval);
                Save();
                ShowSettings = false;
            }
            if (GUILayout.Button("Cancel"))
            {
                // We've canclled, so restore original settings.
                RestoreTempSettings();
                ShowSettings = false;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
        }

        #endregion
    }
}
