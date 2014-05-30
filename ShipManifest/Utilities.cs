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
            LogMessage(String.Format("Loading Texture - file://{0}{1}", PlugInPath, FileName), "Info", true);
            WWW img1 = new WWW(String.Format("file://{0}{1}", PlugInPath, FileName));
            img1.LoadImageIntoTexture(tex);
        }

        public static void LogMessage(string error, string type, bool verbose)
        {
            if (verbose)
                _errors.Add(type + ": " + error);
            if (type == "Error" && SettingsManager.AutoDebug)
                SettingsManager.ShowDebugger = true;
        }
    }

    public static class ManifestStyle
    {
        public static GUIStyle WindowStyle;
        public static GUIStyle IconStyle;
        public static GUIStyle ButtonSourceStyle;
        public static GUIStyle ButtonTargetStyle;
        public static GUIStyle ButtonToggledSourceStyle;
        public static GUIStyle ButtonToggledTargetStyle;
        public static GUIStyle ButtonStyle;
        public static GUIStyle ButtonToggledStyle;
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
                SettingsManager.LoadColors();
                SetStyles();
            }
        }

        public static void SetStyles()
        {
            WindowStyle = new GUIStyle(GUI.skin.window);
            IconStyle = new GUIStyle();

            ButtonStyle = new GUIStyle(GUI.skin.button);
            ButtonStyle.normal.textColor = Color.white;
            ButtonStyle.hover.textColor = Color.blue;
            ButtonStyle.fontSize = 14;
            ButtonStyle.fontStyle = FontStyle.Normal;

            ButtonToggledStyle = new GUIStyle(GUI.skin.button);
            ButtonToggledStyle.normal.textColor = Color.green;
            ButtonToggledStyle.fontSize = 14;
            ButtonToggledStyle.hover.textColor = Color.blue;
            ButtonToggledStyle.normal.background = ButtonToggledStyle.onActive.background;
            ButtonToggledStyle.fontStyle = FontStyle.Normal;

            ButtonSourceStyle = new GUIStyle(GUI.skin.button);
            ButtonSourceStyle.normal.textColor = Color.white;
            ButtonSourceStyle.fontSize = 14;
            ButtonSourceStyle.hover.textColor = Color.blue;
            ButtonSourceStyle.fontStyle = FontStyle.Normal;
            ButtonSourceStyle.alignment = TextAnchor.UpperLeft;

            ButtonToggledSourceStyle = new GUIStyle(GUI.skin.button);
            ButtonToggledSourceStyle.normal.textColor = SettingsManager.Colors[SettingsManager.SourcePartColor];
            ButtonToggledSourceStyle.fontSize = 14;
            ButtonToggledSourceStyle.hover.textColor = Color.blue;
            ButtonToggledSourceStyle.normal.background = ButtonToggledSourceStyle.onActive.background;
            ButtonToggledSourceStyle.fontStyle = FontStyle.Normal;
            ButtonToggledSourceStyle.alignment = TextAnchor.UpperLeft;

            ButtonTargetStyle = new GUIStyle(GUI.skin.button);
            ButtonTargetStyle.normal.textColor = Color.white;
            ButtonTargetStyle.fontSize = 14;
            ButtonTargetStyle.hover.textColor = Color.blue;
            ButtonTargetStyle.fontStyle = FontStyle.Normal;
            ButtonTargetStyle.alignment = TextAnchor.UpperLeft;

            ButtonToggledTargetStyle = new GUIStyle(GUI.skin.button);
            ButtonToggledTargetStyle.normal.textColor = SettingsManager.Colors[SettingsManager.TargetPartColor];
            ButtonToggledTargetStyle.fontSize = 14;
            ButtonToggledTargetStyle.hover.textColor = Color.blue;
            ButtonToggledTargetStyle.normal.background = ButtonToggledSourceStyle.onActive.background;
            ButtonToggledTargetStyle.fontStyle = FontStyle.Normal;
            ButtonToggledTargetStyle.alignment = TextAnchor.UpperLeft;

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

        public static Dictionary<string, Color> Colors;

        public string CurVersion = "0.23.5.3.3";

        public Rect ManifestPosition;
        public Rect TransferPosition;

        public Rect SettingsPosition;
        public bool ShowSettings { get; set; }

        public string DebugLogPath = "\\Plugins\\PluginData\\";

        public bool AutoSave;
        public float SaveIntervalSec = 60f;
        public bool prevAutoSave;
        public float prevSaveIntervalSec = 60f;

        public Rect RosterPosition;
        public bool ShowRoster { get; set; }

        public bool RealismMode = false;
        public bool prevRealismMode = false;
        public bool LockSettings = false;
        public bool prevLockSettings = false;

        public static bool VerboseLogging = false;
        public static bool prevVerboseLogging = false;

        public Rect DebuggerPosition;
        public static bool ShowDebugger = false;
        public static bool prevShowDebugger = false;
        public static bool AutoDebug = false;

        public float FlowRate = 100;
        public float prevFlowRate = 100;
        public float MaxFlowRate = 100;
        public float MinFlowRate = 0;

        // Feature Options
        public bool EnableScience = true;
        public bool EnableCrew = true;
        public bool EnablePFResources = true;
        public bool EnableCLS = true;
        public bool prevEnableScience = true;
        public bool prevEnableCrew = true;
        public bool prevEnablePFResources = true;
        public bool prevEnableCLS = true;

        // Internal setting.  Not persisted.  Value is set when checking for presence of CLS.
        public static bool CLSInstalled = true;

        public static bool EnableTextureReplacer = false;
        public static bool prevEnableTextureReplacer = false;

        public static double IVATimeDelaySec = 5;
        public static bool ShowIVAUpdateBtn = false;

        // Default sound license: CC-By-SA
        // http://www.freesound.org/people/vibe_crc/sounds/59328/

        public string PumpSoundStart = "ShipManifest/Sounds/59328-1";
        public string PumpSoundRun = "ShipManifest/Sounds/59328-2";
        public string PumpSoundStop = "ShipManifest/Sounds/59328-3";
        public string prevPumpSoundStart = "";
        public string prevPumpSoundRun = "";
        public string prevPumpSoundStop = "";

        public double PumpSoundVol = 3;
        public double CrewSoundVol = 3;

        public string CrewSoundStart = "ShipManifest/Sounds/xxxxx-1";
        public string CrewSoundRun = "ShipManifest/Sounds/xxxxx-2";
        public string CrewSoundStop = "ShipManifest/Sounds/xxxxx-3";
        public string prevCrewSoundStart = "";
        public string prevCrewSoundRun = "";
        public string prevCrewSoundStop = "";

        public static string SourcePartColor = "red";
        public static string TargetPartColor = "green";
        public static string TargetPartCrewColor = "blue";
        
        #endregion

        #region Settings Window (GUI)

        private Vector2 ScrollViewerSettings = Vector2.zero;
        public void SettingsWindow(int windowId)
        {
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

            // EnableCrew Mode
            GUI.enabled = isEnabled;
            GUILayout.BeginHorizontal();
            label = "Enable Crew Xfers";
            EnableCrew = GUILayout.Toggle(EnableCrew, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();

            if (!EnableCrew)
            {
                if (ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedResource == "Crew")
                {
                    // Clear Resource selection.
                    ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedResource = null;
                    ManifestController.GetInstance(FlightGlobals.ActiveVessel).ShowTransferWindow = false;
                }
            }
            // EnableCLS Mode
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            if (!EnableCrew || !SettingsManager.CLSInstalled)
                GUI.enabled = false;
            else
                GUI.enabled = isEnabled;
            label = "Enable CLS  (Connected Living Spaces)";
            EnableCLS = GUILayout.Toggle(EnableCLS, label, GUILayout.Width(300));
            GUILayout.EndHorizontal();

            if (!EnableCLS && prevEnableCLS)
            {
                if (ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedResource == "Crew")
                {
                    //Reassign the resource to observe new settings.
                    ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedResource = "Crew";
                }
            }
            else if (EnableCLS && !prevEnableCLS)
            {
                if (ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedResource == "Crew")
                {
                    //Reassign the resource to observe new settings.
                    ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedResource = "Crew";
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
                if (ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedResource == "Science")
                {
                    // Clear Resource selection.
                    ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedResource = null;
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
            label = "Lock Settings  (If set ON, edit config File to Turn OFF)";
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

            // TextureReplacer Mode
            label = "Enable Texture Replacer Events";
            EnableTextureReplacer = GUILayout.Toggle(EnableTextureReplacer, label, GUILayout.Width(300));

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

        #region Methods

        public void Load()
        {
            ManifestUtilities.LogMessage("Settings load started...", "Info", VerboseLogging);

            try
            {
                LoadColors();

                // Interestingly, Floats seem to load fine.   Saves seem to be problematic.  attempts to save float are not persisted in the file...  
                // So, FlowRate vars now use double and are converted at load
                KSP.IO.PluginConfiguration configfile = KSP.IO.PluginConfiguration.CreateForType<ShipManifestModule>();
                configfile.load();

                ManifestPosition = configfile.GetValue<Rect>("ManifestPosition");
                TransferPosition = configfile.GetValue<Rect>("TransferPosition");
                DebuggerPosition = configfile.GetValue<Rect>("DebuggerPosition");
                SettingsPosition = configfile.GetValue<Rect>("SettingsPosition");
                RosterPosition = configfile.GetValue<Rect>("RosterPosition");
                ShowDebugger = configfile.GetValue<bool>("ShowDebugger");
                RealismMode = configfile.GetValue<bool>("RealismMode");
                LockSettings = configfile.GetValue<bool>("LockSettings");
                VerboseLogging = configfile.GetValue<bool>("VerboseLogging");
                AutoSave = configfile.GetValue<bool>("AutoSave");
                SaveIntervalSec = (float)configfile.GetValue<double>("SaveIntervalSec");
                FlowRate = (float)configfile.GetValue<double>("FlowRate");
                MinFlowRate = (float)configfile.GetValue<double>("MinFlowRate");
                MaxFlowRate = (float)configfile.GetValue<double>("MaxFlowRate");
                PumpSoundStart = configfile.GetValue<string>("PumpSoundStart");
                PumpSoundRun = configfile.GetValue<string>("PumpSoundRun");
                PumpSoundStop = configfile.GetValue<string>("PumpSoundStop");
                CrewSoundStart = configfile.GetValue<string>("CrewSoundStart");
                CrewSoundRun = configfile.GetValue<string>("CrewSoundRun");
                CrewSoundStop = configfile.GetValue<string>("CrewSoundStop");

                PumpSoundVol = configfile.GetValue<double>("PumpSoundVol");
                CrewSoundVol = configfile.GetValue<double>("CrewSoundVol");

                SourcePartColor = configfile.GetValue<string>("SourcePartColor");
                TargetPartColor = configfile.GetValue<string>("TargetPartColor");
                TargetPartCrewColor = configfile.GetValue<string>("TargetPartCrewColor");

                EnableScience = configfile.GetValue<bool>("EnableScience");
                EnableCrew = configfile.GetValue<bool>("EnableCrew");
                EnablePFResources = configfile.GetValue<bool>("EnablePFResources");
                EnableCLS = configfile.GetValue<bool>("EnableCLS");

                DebugLogPath = configfile.GetValue<string>("DebugLogPath");

                IVATimeDelaySec = configfile.GetValue<double>("IVATimeDelaySec");
                ShowIVAUpdateBtn = configfile.GetValue<bool>("ShowIVAUpdateBtn");
                AutoDebug = configfile.GetValue<bool>("AutoDebug");
                EnableTextureReplacer = configfile.GetValue<bool>("EnableTextureReplacer");

                // Default values for Flow rates
                if (FlowRate == 0)
                    FlowRate = 100;
                if (MaxFlowRate == 0)
                    MaxFlowRate = 100;

                // Default sound license: CC-By-SA
                // http://www.freesound.org/people/vibe_crc/sounds/59328/
                if (PumpSoundStart == "")
                    PumpSoundStart = "ShipManifest/Sounds/59328-1";
                if (PumpSoundRun == "")
                    PumpSoundRun = "ShipManifest/Sounds/59328-2";
                if (PumpSoundStop == "")
                    PumpSoundStop = "ShipManifest/Sounds/59328-3";

                // Default sound license: CC-By-SA
                // http://www.freesound.org/people/vibe_crc/sounds/14214/
                if (CrewSoundStart == "")
                    CrewSoundStart = "ShipManifest/Sounds/14214-1";
                if (CrewSoundRun == "")
                    CrewSoundRun = "ShipManifest/Sounds/14214-2";
                if (CrewSoundStop == "")
                    CrewSoundStop = "ShipManifest/Sounds/14214-3";

                if (!Colors.Keys.Contains(SourcePartColor))
                    SourcePartColor = "red";
                if (!Colors.Keys.Contains(TargetPartColor))
                    SourcePartColor = "green";

                ManifestUtilities.LogMessage(string.Format("ManifestPosition Loaded: {0}, {1}, {2}, {3}", ManifestPosition.xMin, ManifestPosition.xMax, ManifestPosition.yMin, ManifestPosition.yMax), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("TransferPosition Loaded: {0}, {1}, {2}, {3}", TransferPosition.xMin, TransferPosition.xMax, TransferPosition.yMin, TransferPosition.yMax), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("ResourceDebuggerPosition Loaded: {0}, {1}, {2}, {3}", DebuggerPosition.xMin, DebuggerPosition.xMax, DebuggerPosition.yMin, DebuggerPosition.yMax), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("RosterPosition Loaded: {0}, {1}, {2}, {3}", RosterPosition.xMin, RosterPosition.xMax, RosterPosition.yMin, RosterPosition.yMax), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("SettingsPosition Loaded: {0}, {1}, {2}, {3}", SettingsPosition.xMin, SettingsPosition.xMax, SettingsPosition.yMin, SettingsPosition.yMax), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("ShowDebugger Loaded: {0}", ShowDebugger.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("RealismMode Loaded: {0}", RealismMode.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("LockSettings Loaded: {0}", LockSettings.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("VerboseLogging Loaded: {0}", VerboseLogging.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("AutoSave Loaded: {0}", AutoSave.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("SaveIntervalSec Loaded: {0}", SaveIntervalSec.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("FlowRate Loaded: {0}", FlowRate.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("MinFlowRate Loaded: {0}", MinFlowRate.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("MaxFlowRate Loaded: {0}", MaxFlowRate.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("PumpSoundStart Loaded: {0}", PumpSoundStart.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("PumpSoundRun Loaded: {0}", PumpSoundRun.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("PumpSoundStop Loaded: {0}", PumpSoundStop.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("PumpSoundVol Loaded: {0}", PumpSoundVol.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("CrewSoundStart Loaded: {0}", CrewSoundStart.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("CrewSoundRun Loaded: {0}", CrewSoundRun.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("CrewSoundStop Loaded: {0}", CrewSoundStop.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("CrewSoundVol Loaded: {0}", CrewSoundVol.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("SourcePartColor Loaded: {0}", SourcePartColor), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("TargetPartColor Loaded: {0}", TargetPartColor), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("TargetPartCrewColor Loaded: {0}", TargetPartCrewColor), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("EnableScience Loaded: {0}", EnableScience), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("EnablePFResources Loaded: {0}", EnablePFResources), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("EnableCLS Loaded: {0}", EnableCLS), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("IVATimeDelaySec Loaded: {0}", IVATimeDelaySec), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("ShowIVAUpdateBtn Loaded: {0}", ShowIVAUpdateBtn), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("AutoDebug Loaded: {0}", AutoDebug), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("EnableTextureReplacer Loaded: {0}", EnableTextureReplacer), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("Load Settings Complete"), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format(" "), "Info", VerboseLogging);

                ValidateLoad();
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format("Failed to Load Settings: {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                ManifestUtilities.LogMessage(string.Format(" "), "Info", VerboseLogging);
            }
        }

        private void ValidateLoad()
        {

            //ShowDebugger = configfile.GetValue<bool>("ShowDebugger");
            //RealismMode = configfile.GetValue<bool>("RealismMode");
            //LockSettings = configfile.GetValue<bool>("LockSettings");
            //VerboseLogging = configfile.GetValue<bool>("VerboseLogging");
            //AutoSave = configfile.GetValue<bool>("AutoSave");
            //SaveIntervalSec = (float)configfile.GetValue<double>("SaveIntervalSec");
            //FlowRate = (float)configfile.GetValue<double>("FlowRate");
            //MinFlowRate = (float)configfile.GetValue<double>("MinFlowRate");
            //MaxFlowRate = (float)configfile.GetValue<double>("MaxFlowRate");
            //PumpSoundStart = configfile.GetValue<string>("PumpSoundStart");
            //PumpSoundRun = configfile.GetValue<string>("PumpSoundRun");
            //PumpSoundStop = configfile.GetValue<string>("PumpSoundStop");
            //CrewSoundStart = configfile.GetValue<string>("CrewSoundStart");
            //CrewSoundRun = configfile.GetValue<string>("CrewSoundRun");
            //CrewSoundStop = configfile.GetValue<string>("CrewSoundStop");

            //SourcePartColor = configfile.GetValue<string>("SourcePartColor");
            //TargetPartColor = configfile.GetValue<string>("TargetPartColor");
            //TargetPartCrewColor = configfile.GetValue<string>("TargetPartCrewColor");

            //EnableScience = configfile.GetValue<bool>("EnableScience");
            //EnableCrew = configfile.GetValue<bool>("EnableCrew");
            //EnablePFResources = configfile.GetValue<bool>("EnablePFResources");
            //EnableCLS = configfile.GetValue<bool>("EnableCLS");

            //DebugLogPath = configfile.GetValue<string>("DebugLogPath");

            //IVATimeDelaySec = configfile.GetValue<double>("IVATimeDelaySec");
            //ShowIVAUpdateBtn = configfile.GetValue<bool>("ShowIVAUpdateBtn");
            //AutoDebug = configfile.GetValue<bool>("AutoDebug");

        }

        public void Save()
        {
            try
            {
                // For some reason, saving floats does not seem to work.  (maybe I just don't know enough, but converted flowrates to doubles and it works.
                KSP.IO.PluginConfiguration configfile = KSP.IO.PluginConfiguration.CreateForType<ShipManifestModule>();

                configfile.SetValue("ManifestPosition", ManifestPosition);
                configfile.SetValue("TransferPosition", TransferPosition);
                configfile.SetValue("RosterPosition", SettingsPosition);
                configfile.SetValue("SettingsPosition", SettingsPosition);
                configfile.SetValue("DebuggerPosition", DebuggerPosition);
                configfile.SetValue("ShowDebugger", ShowDebugger);
                configfile.SetValue("RealismMode", RealismMode);
                configfile.SetValue("LockSettings", LockSettings);
                configfile.SetValue("VerboseLogging", VerboseLogging);
                configfile.SetValue("AutoSave", AutoSave);
                configfile.SetValue("SaveIntervalSec", (double)SaveIntervalSec);
                configfile.SetValue("FlowRate", (double)FlowRate);
                configfile.SetValue("MinFlowRate", (double)MinFlowRate);
                configfile.SetValue("MaxFlowRate", (double)MaxFlowRate);
                configfile.SetValue("PumpSoundStart", PumpSoundStart);
                configfile.SetValue("PumpSoundRun", PumpSoundRun);
                configfile.SetValue("PumpSoundStop", PumpSoundStop);
                configfile.SetValue("PumpSoundVol", PumpSoundVol);
                configfile.SetValue("CrewSoundStart", CrewSoundStart);
                configfile.SetValue("CrewSoundRun", CrewSoundRun);
                configfile.SetValue("CrewSoundStop", CrewSoundStop);
                configfile.SetValue("CrewSoundVol", CrewSoundVol);
                configfile.SetValue("SourcePartColor", SourcePartColor);
                configfile.SetValue("TargetPartColor", TargetPartColor);
                configfile.SetValue("TargetPartCrewColor", TargetPartCrewColor);

                configfile.SetValue("EnableScience", EnableScience);
                configfile.SetValue("EnableCrew", EnableCrew);
                configfile.SetValue("EnablePFResources", EnablePFResources);
                configfile.SetValue("EnableCLS", EnableCLS);

                configfile.SetValue("DebugLogPath", DebugLogPath);

                configfile.SetValue("IVATimeDelaySec", IVATimeDelaySec);
                configfile.SetValue("ShowIVAUpdateBtn", ShowIVAUpdateBtn);
                configfile.SetValue("AutoDebug", AutoDebug);
                configfile.SetValue("EnableTextureReplacer", EnableTextureReplacer);

                configfile.save();

                ManifestUtilities.LogMessage(string.Format("ManifestPosition Saved: {0}, {1}, {2}, {3}", ManifestPosition.xMin, ManifestPosition.xMax, ManifestPosition.yMin, ManifestPosition.yMax), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("TransferPosition Saved: {0}, {1}, {2}, {3}", TransferPosition.xMin, TransferPosition.xMax, TransferPosition.yMin, TransferPosition.yMax), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("SettingsPosition Saved: {0}, {1}, {2}, {3}", SettingsPosition.xMin, SettingsPosition.xMax, SettingsPosition.yMin, SettingsPosition.yMax), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("DebuggerPosition Saved: {0}, {1}, {2}, {3}", DebuggerPosition.xMin, DebuggerPosition.xMax, DebuggerPosition.yMin, DebuggerPosition.yMax), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("ShowDebugger Saved: {0}", ShowDebugger.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("RealismMode Saved: {0}", RealismMode.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("LockSettings Saved: {0}", LockSettings.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("VerboseLogging Saved: {0}", VerboseLogging.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("AutoSave Saved: {0}", AutoSave.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("SaveIntervalSec Saved: {0}", SaveIntervalSec.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("FlowRate Saved: {0}", FlowRate.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("MinFlowRate Saved: {0}", MinFlowRate.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("MaxFlowRate Saved: {0}", MaxFlowRate.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("PumpSoundStart Saved: {0}", PumpSoundStart.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("PumpSoundRun Saved: {0}", PumpSoundRun.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("PumpSoundStop Saved: {0}", PumpSoundStop.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("PumpSoundVol Saved: {0}", PumpSoundVol.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("CrewSoundStart Saved: {0}", CrewSoundStart.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("CrewSoundRun Saved: {0}", CrewSoundRun.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("CrewSoundStop Saved: {0}", CrewSoundStop.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("CrewSoundVol Saved: {0}", CrewSoundVol.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("SourcePartColor Saved: {0}", SourcePartColor), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("TargetPartColor Saved: {0}", TargetPartColor), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("TargetPartCrewColor Saved: {0}", TargetPartCrewColor), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("EnableScience Saved: {0}", EnableScience.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("EnableCrew Saved: {0}", EnableCrew), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("EnablePFResources Saved: {0}", EnablePFResources), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("EnableCLS Saved: {0}", EnableCLS), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("DebugLogPath Saved: {0}", DebugLogPath.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("IVATimeDelaySec Saved: {0}", IVATimeDelaySec.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("ShowIVAUpdateBtn Saved: {0}", ShowIVAUpdateBtn.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("AutoDebug Saved: {0}", AutoDebug.ToString()), "Info", VerboseLogging);
                ManifestUtilities.LogMessage(string.Format("EnableTextureReplacer Saved: {0}", EnableTextureReplacer.ToString()), "Info", VerboseLogging);
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format("Failed to Save Settings: {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        public static void LoadColors()
        {
            Colors = new Dictionary<string, Color>();

            Colors.Add("black", Color.black);
            Colors.Add("blue", Color.blue);
            Colors.Add("clea", Color.clear);
            Colors.Add("cyan", Color.cyan);
            Colors.Add("gray", Color.gray);
            Colors.Add("green", Color.green);
            Colors.Add("magenta", Color.magenta);
            Colors.Add("red", Color.red);
            Colors.Add("white", Color.white);
            Colors.Add("yellow", Color.yellow);
        }

        private void StoreTempSettings()
        {
            prevShowDebugger = ShowDebugger;
            prevVerboseLogging = VerboseLogging;
            prevAutoSave = AutoSave;
            prevSaveIntervalSec = SaveIntervalSec;

            // Realism Mode settings.
            prevRealismMode = RealismMode;
            prevEnableScience = EnableScience;
            prevEnableCrew = EnableCrew;
            prevEnablePFResources = EnablePFResources;
            prevEnableCLS = EnableCLS;
            prevEnableTextureReplacer = EnableTextureReplacer;
            prevLockSettings = LockSettings;

            // sounds
            prevFlowRate = FlowRate;
            prevPumpSoundStart = PumpSoundStart;
            prevPumpSoundRun = PumpSoundRun;
            prevPumpSoundStop = PumpSoundStop;
            prevCrewSoundStart = CrewSoundStart;
            prevCrewSoundRun = CrewSoundRun;
            prevCrewSoundStop = CrewSoundStop;
        }

        private void RestoreTempSettings()
        {
            RealismMode = prevRealismMode;
            ShowDebugger = prevShowDebugger;
            VerboseLogging = prevVerboseLogging;
            AutoSave = prevAutoSave;
            SaveIntervalSec = prevSaveIntervalSec;
            FlowRate = prevFlowRate;
            PumpSoundStart = prevPumpSoundStart;
            PumpSoundRun = prevPumpSoundRun;
            PumpSoundStop = prevPumpSoundStop;
            CrewSoundStart = prevCrewSoundStart;
            CrewSoundRun = prevCrewSoundRun;
            CrewSoundStop = prevCrewSoundStop;
            EnableScience = prevEnableScience;
            EnableCrew = prevEnableCrew;
            EnablePFResources = prevEnablePFResources;
            EnableCLS = prevEnableCLS;
            EnableTextureReplacer = prevEnableTextureReplacer;
            LockSettings = prevLockSettings;
        }

        #endregion

    }
}
