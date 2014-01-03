using KSP.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ShipManifest
{
    public static class ManifestUtilities
    {
        public static String AppPath = KSPUtil.ApplicationRootPath.Replace("\\", "/");
        public static String PlugInPath = AppPath + "GameData/ShipManifest/Plugins/PluginData/ShipManifest/";
        public static Vector2 DebugScrollPosition = Vector2.zero;
 
        private static List<string> _errors = new List<string>();
        public static List<string> Errors
        {
            get { return _errors; }
        }

        public static void LoadTexture(ref Texture2D tex, String FileName)
        {
            LogMessage(String.Format("Loading Texture - file://{0}{1}", PlugInPath, FileName), "Info");
            WWW img1 = new WWW(String.Format("file://{0}{1}", PlugInPath, FileName));
            img1.LoadImageIntoTexture(tex);
        }

        public static void LogMessage(string error, string type)
        {
            _errors.Add(type + ": " + error);
        }
    }

    public static class ManifestStyle
    {
        public static GUIStyle WindowStyle;
        public static GUIStyle IconStyle;
        public static GUIStyle ButtonToggledStyle;
        public static GUIStyle ButtonToggledRedStyle;
        public static GUIStyle ButtonStyle;
        public static GUIStyle ErrorLabelRedStyle;
        public static GUIStyle LabelStyle;
        public static GUIStyle LabelStyleRed;
        public static GUIStyle LabelStyleYellow;
        public static GUIStyle LabelStyleGreen;

        public static void SetupGUI()
        {
            GUI.skin = HighLogic.Skin;
            if (WindowStyle == null)
            {
                SetStyles();
            }
        }

        public static void SetStyles()
        {
            WindowStyle = new GUIStyle(GUI.skin.window);
            IconStyle = new GUIStyle();

            ButtonToggledStyle = new GUIStyle(GUI.skin.button);
            ButtonToggledStyle.normal.textColor = Color.green;
            ButtonToggledStyle.normal.background = ButtonToggledStyle.onActive.background;
            ButtonToggledStyle.fontSize = 14;
            ButtonToggledStyle.fontStyle = FontStyle.Normal;

            ButtonToggledRedStyle = new GUIStyle(ButtonToggledStyle);
            ButtonToggledRedStyle.normal.textColor = Color.red;
            ButtonToggledRedStyle.fontSize = 14;
            ButtonToggledRedStyle.fontStyle = FontStyle.Normal;

            ButtonStyle = new GUIStyle(GUI.skin.button);
            ButtonStyle.normal.textColor = Color.white;
            ButtonStyle.fontSize = 14;
            ButtonStyle.fontStyle = FontStyle.Normal;

            ErrorLabelRedStyle = new GUIStyle(GUI.skin.label);
            ErrorLabelRedStyle.normal.textColor = Color.red;

            LabelStyle = new GUIStyle(GUI.skin.label);

            LabelStyleRed = new GUIStyle(LabelStyle);
            LabelStyleRed.normal.textColor = Color.red;

            LabelStyleYellow = new GUIStyle(LabelStyle);
            LabelStyleYellow.normal.textColor = Color.yellow;

            LabelStyleGreen = new GUIStyle(LabelStyle);
            LabelStyleGreen.normal.textColor = Color.green;
        }
    }

    public class SettingsManager
    {
        #region Properties

        public Rect ResourceManifestPosition;
        public Rect ResourceTransferPosition;

        public Rect PrevResourceManifestPosition;
        public Rect PrevResourceTransferPosition;
        
        public Rect SettingsPosition;
        public bool ShowSettings { get; set; }

        public bool RealismMode = false;
        public bool PrevRealismMode = false;

        public bool VerboseLogging = false;
        public bool PrevVerboseLogging = false;

        public Rect DebuggerPosition;
        public bool ShowDebugger = false;
        public bool PrevShowDebugger = false;

        public double FlowRate = 1;
        public double PrevFlowRate = 1;

        // Default sound license: CC-By-SA
        // http://www.freesound.org/people/vibe_crc/sounds/59328/

        public string PumpSoundStart = "ShipManifest/Sounds/59328-1";
        public string PumpSoundRun = "ShipManifest/Sounds/59328-2";
        public string PumpSoundStop = "ShipManifest/Sounds/59328-3";
        public string PrevPumpSoundStart = "";
        public string PrevPumpSoundRun = "";
        public string PrevPumpSoundStop = "";


        #endregion

        #region Settings Window (GUI)

        private Vector2 ScrollViewerSettings = Vector2.zero;
        public void SettingsWindow(int windowId)
        {
            // Store settings in case we cancel later...
            PrevRealismMode = RealismMode;
            PrevShowDebugger = ShowDebugger;
            PrevVerboseLogging = VerboseLogging;
            PrevFlowRate = FlowRate;
            PrevPumpSoundStart = PumpSoundStart;
            PrevPumpSoundRun = PumpSoundRun;
            PrevPumpSoundStop = PumpSoundStop;
            string label = "";

            GUILayout.BeginVertical();
            ScrollViewerSettings = GUILayout.BeginScrollView(ScrollViewerSettings, GUILayout.Height(240), GUILayout.Width(350));
            GUILayout.BeginVertical();
            label = VerboseLogging ? "Verbose Logging Enabled" : "Verbose Logging Disabled";
            VerboseLogging = GUILayout.Toggle(VerboseLogging, label, GUILayout.Width(300));

            label = ShowDebugger ? "Disable Debug Console" : "Enable Debug Console";
            ShowDebugger = GUILayout.Toggle(ShowDebugger, label, GUILayout.Width(300));

            label = RealismMode ? "Realism Mode Enabled" : "Realism Mode Disabled";
            RealismMode = GUILayout.Toggle(RealismMode, label, GUILayout.Width(300));

            GUILayout.BeginHorizontal();
            GUILayout.Label("Pump Start Sound: ", GUILayout.Width(120));
            PumpSoundStart = GUILayout.TextField(PumpSoundStart, GUILayout.Width(200));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Pump Run Sound: ", GUILayout.Width(120));
            PumpSoundRun = GUILayout.TextField(PumpSoundRun, GUILayout.Width(200));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Pump Stop Sound: ", GUILayout.Width(120));
            PumpSoundStop = GUILayout.TextField(PumpSoundStop, GUILayout.Width(200));
            GUILayout.EndHorizontal();

            // create xfer slider;
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Flow Rate:  {0}", FlowRate.ToString("#######0.####")), GUILayout.Width(120), GUILayout.Height(20));
            FlowRate = (double) GUILayout.HorizontalSlider((float) FlowRate, 0f, 100f, GUILayout.Width(200));
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save"))
            {
                Save();
                ShowSettings = false;
            }
            if (GUILayout.Button("Cancel"))
            {
                RealismMode = PrevRealismMode;
                ShowDebugger = PrevShowDebugger;
                VerboseLogging = PrevVerboseLogging;
                FlowRate = PrevFlowRate;
                PumpSoundStart = PrevPumpSoundStart;
                PumpSoundRun = PrevPumpSoundRun;
                PumpSoundStop = PrevPumpSoundStop;

                ShowSettings = false;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
        }
        
        #endregion

        #region Methods

        public void Load()
        {
            ManifestUtilities.LogMessage("Settings load started...", "Info");

            try
            {
                KSP.IO.PluginConfiguration configfile = KSP.IO.PluginConfiguration.CreateForType<ResourceManifestModule>();
                configfile.load();

                ResourceManifestPosition = configfile.GetValue<Rect>("ResourceManifestPosition");
                ResourceTransferPosition = configfile.GetValue<Rect>("ResourceTransferPosition");
                DebuggerPosition = configfile.GetValue<Rect>("ResourceDebuggerPosition");
                SettingsPosition = configfile.GetValue<Rect>("SettingsPosition");
                ShowDebugger = configfile.GetValue<bool>("ShowDebugger");
                RealismMode = configfile.GetValue<bool>("RealismMode");
                FlowRate = configfile.GetValue<double>("FlowRate");
                VerboseLogging = configfile.GetValue<bool>("VerboseLogging");
                PumpSoundStart = configfile.GetValue<string>("PumpSoundStart");
                PumpSoundRun = configfile.GetValue<string>("PumpSoundRun");
                PumpSoundStop = configfile.GetValue<string>("PumpSoundStop");

                // Default sound is:

                if (PumpSoundStart == "")
                    PumpSoundStart = "ShipManifest/Sounds/59328-1";
                if (PumpSoundRun == "")
                    PumpSoundRun = "ShipManifest/Sounds/59328-2";
                if (PumpSoundStop == "")
                    PumpSoundStop = "ShipManifest/Sounds/59328-3";

                if (VerboseLogging)
                {
                    ManifestUtilities.LogMessage(string.Format("ResourceManifestPosition Loaded: {0}, {1}, {2}, {3}", ResourceManifestPosition.xMin, ResourceManifestPosition.xMax, ResourceManifestPosition.yMin, ResourceManifestPosition.yMax), "Info");
                    ManifestUtilities.LogMessage(string.Format("ResourceTransferPosition Loaded: {0}, {1}, {2}, {3}", ResourceTransferPosition.xMin, ResourceTransferPosition.xMax, ResourceTransferPosition.yMin, ResourceTransferPosition.yMax), "Info");
                    ManifestUtilities.LogMessage(string.Format("ResourceDebuggerPosition Loaded: {0}, {1}, {2}, {3}", DebuggerPosition.xMin, DebuggerPosition.xMax, DebuggerPosition.yMin, DebuggerPosition.yMax), "Info");
                    ManifestUtilities.LogMessage(string.Format("SettingsPosition Loaded: {0}, {1}, {2}, {3}", SettingsPosition.xMin, SettingsPosition.xMax, SettingsPosition.yMin, SettingsPosition.yMax), "Info");
                    ManifestUtilities.LogMessage(string.Format("ShowDebugger Loaded: {0}", ShowDebugger.ToString()), "Info");
                    ManifestUtilities.LogMessage(string.Format("FlowRate Loaded: {0}", FlowRate.ToString()), "Info");
                    ManifestUtilities.LogMessage(string.Format("RealismMode Loaded: {0}", RealismMode.ToString()), "Info");
                    ManifestUtilities.LogMessage(string.Format("VerboseLogging Loaded: {0}", VerboseLogging.ToString()), "Info");
                    ManifestUtilities.LogMessage(string.Format("PumpSoundStart Loaded: {0}", PumpSoundStart.ToString()), "Info");
                    ManifestUtilities.LogMessage(string.Format("PumpSoundRun Loaded: {0}", PumpSoundRun.ToString()), "Info");
                    ManifestUtilities.LogMessage(string.Format("PumpSoundStop Loaded: {0}", PumpSoundStop.ToString()), "Info");
                }
            }
            catch(Exception e)
            {
                ManifestUtilities.LogMessage(string.Format("Failed to Load Settings: {0} \r\n\r\n{1}", e.Message, e.StackTrace), "Exception");
            }
        }

        public void Save()
        {
            try
            {
                KSP.IO.PluginConfiguration configfile = KSP.IO.PluginConfiguration.CreateForType<ResourceManifestModule>();

                configfile.SetValue("ResourceManifestPosition", ResourceManifestPosition);
                configfile.SetValue("ResourceTransferPosition", ResourceTransferPosition);
                configfile.SetValue("SettingsPosition", SettingsPosition);
                configfile.SetValue("ResourceDebuggerPosition", DebuggerPosition);
                configfile.SetValue("ShowDebugger", ShowDebugger);
                configfile.SetValue("RealismMode", RealismMode);
                configfile.SetValue("FlowRate", FlowRate);
                configfile.SetValue("VerboseLogging", VerboseLogging);
                configfile.SetValue("PumpSoundStart", PumpSoundStart);
                configfile.SetValue("PumpSoundRun", PumpSoundRun);
                configfile.SetValue("PumpSoundStop", PumpSoundStop);
                configfile.save();

                if (VerboseLogging)
                {
                    ManifestUtilities.LogMessage(string.Format("ResourceManifestPosition Saved: {0}, {1}, {2}, {3}", ResourceManifestPosition.xMin, ResourceManifestPosition.xMax, ResourceManifestPosition.yMin, ResourceManifestPosition.yMax), "Info");
                    ManifestUtilities.LogMessage(string.Format("ResourceTransferPosition Saved: {0}, {1}, {2}, {3}", ResourceTransferPosition.xMin, ResourceTransferPosition.xMax, ResourceTransferPosition.yMin, ResourceTransferPosition.yMax), "Info");
                    ManifestUtilities.LogMessage(string.Format("SettingsPosition Saved: {0}, {1}, {2}, {3}", SettingsPosition.xMin, SettingsPosition.xMax, SettingsPosition.yMin, SettingsPosition.yMax), "Info");
                    ManifestUtilities.LogMessage(string.Format("ResourceDebuggerPosition Saved: {0}, {1}, {2}, {3}", DebuggerPosition.xMin, DebuggerPosition.xMax, DebuggerPosition.yMin, DebuggerPosition.yMax), "Info");
                    ManifestUtilities.LogMessage(string.Format("ShowDebugger Saved: {0}", ShowDebugger.ToString()), "Info");
                    ManifestUtilities.LogMessage(string.Format("RealismMode Saved: {0}", RealismMode.ToString()), "Info");
                    ManifestUtilities.LogMessage(string.Format("FlowRate Saved: {0}", FlowRate.ToString()), "Info");
                    ManifestUtilities.LogMessage(string.Format("VerboseLogging Saved: {0}", VerboseLogging.ToString()), "Info");
                    ManifestUtilities.LogMessage(string.Format("PumpSoundStart Saved: {0}", PumpSoundStart.ToString()), "Info");
                    ManifestUtilities.LogMessage(string.Format("PumpSoundRun Saved: {0}", PumpSoundRun.ToString()), "Info");
                    ManifestUtilities.LogMessage(string.Format("PumpSoundStop Saved: {0}", PumpSoundStop.ToString()), "Info");
                }
            }
            catch (Exception e)
            {
                ManifestUtilities.LogMessage(string.Format("Failed to Save Settings: {0} \r\n\r\n{1}", e.Message, e.StackTrace), "Exception");
            }
        }

        #endregion

    }
}
