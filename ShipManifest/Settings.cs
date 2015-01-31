using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace ShipManifest
{
    public  static class Settings
    {
        #region Properties

        public static Dictionary<string, Color> Colors;

        public static string CurVersion = "0.90.0_3.4.0";

        public static Rect ManifestPosition;
        public static Rect TransferPosition;
        public static Rect DebuggerPosition;
        public static Rect RosterPosition;

        // Flags to show windows
        public static bool ShowTransferWindow { get; set; }
        private static bool _showShipManifest = false;
        public static bool ShowShipManifest
        {
            get
            {
                return _showShipManifest;
            }
            set
            {
                //if (CLSInstalled && EnableCLS && ShipManifestAddon.smController != null)
                //{ 
                //    if (_showShipManifest && !value)
                //        ShipManifestAddon.smController.HighlightCLSVessel(false);
                //    else if (!_showShipManifest && value)
                //    {
                //        if (ShipManifestAddon.smController.SelectedResource == "Crew")
                //            ShipManifestAddon.smController.HighlightCLSVessel(false);
                //    }
                //}
                _showShipManifest = value;

            }
        }

        public static bool VerboseLogging = false;
        public static bool prevVerboseLogging = false;

        public static bool ShowDebugger = false;
        public static bool prevShowDebugger = false;
        public static bool AutoDebug = false;
        public static string ErrorLogLength = "1000";
        public static string prevErrorLogLength = "1000";
        public static bool prevSaveLogOnExit = true;
        public static bool SaveLogOnExit = true;

        public static Rect SettingsPosition;
        public static bool ShowSettings { get; set; }

        public static Rect HatchWindowPosition;
        public static bool ShowHatchWindow { get; set; }

        public static string DebugLogPath = "\\Plugins\\PluginData\\";

        public static bool AutoSave;
        public static float SaveIntervalSec = 60f;
        public static bool prevAutoSave;
        public static float prevSaveIntervalSec = 60f;

        public static bool ShowRoster { get; set; }

        public static bool RealismMode = false;
        public static bool prevRealismMode = false;
        public static bool LockSettings = false;
        public static bool prevLockSettings = false;


        public static float FlowRate = 100;
        public static float prevFlowRate = 100;
        public static float MaxFlowRate = 100;
        public static float MinFlowRate = 0;

        // Feature Options
        public static bool EnableHighlighting = true;
        public static bool prevEnableHighlighting = true;
        public static bool OnlySourceTarget = false;
        public static bool prevOnlySourceTarget = false;
        public static bool EnableScience = true;
        public static bool EnableResources = true;
        public static bool EnableCrew = true;
        public static bool EnablePFResources = true;
        public static bool EnableCLS = false; // off by default
        public static bool prevEnableScience = true;
        public static bool prevEnableCrew = true;
        public static bool prevEnablePFResources = true;
        public static bool prevEnableCLS = true;
        public static bool EnableBlizzyToolbar = false; // off by default
        public static bool prevEnableBlizzyToolbar = false;

        // Internal setting.  Not persisted.  Value is set when checking for presence of CLS.
        public static bool CLSInstalled = false;

        public static bool EnableTextureReplacer = false;
        public static bool prevEnableTextureReplacer = false;

        public static double IVATimeDelaySec = 5;
        public static bool ShowIVAUpdateBtn = false;

        // Default sound license: CC-By-SA
        // http://www.freesound.org/people/vibe_crc/sounds/59328/

        public static string PumpSoundStart = "ShipManifest/Sounds/59328-1";
        public static string PumpSoundRun = "ShipManifest/Sounds/59328-2";
        public static string PumpSoundStop = "ShipManifest/Sounds/59328-3";
        public static string prevPumpSoundStart = "";
        public static string prevPumpSoundRun = "";
        public static string prevPumpSoundStop = "";

        public static double PumpSoundVol = 3;
        public static double CrewSoundVol = 3;

        public static string CrewSoundStart = "ShipManifest/Sounds/14214-1";
        public static string CrewSoundRun = "ShipManifest/Sounds/14214-2";
        public static string CrewSoundStop = "ShipManifest/Sounds/14214-3";
        public static string prevCrewSoundStart = "";
        public static string prevCrewSoundRun = "";
        public static string prevCrewSoundStop = "";

        public static string SourcePartColor = "red";
        public static string TargetPartColor = "green";
        public static string TargetPartCrewColor = "blue";
        public static string CLS_SpaceColor = "green";
        public static string HatchColor = "cyan";
        public static string HatchOpenColor = "cyan";
        public static string HatchCloseColor = "red";
        public static Color defaultColor = new Color(0.478f, 0.698f, 0.478f, 0.698f);
        public static bool ShowToolTips = true;
        public static bool prevShowTooltips = true;

        #endregion

        #region Methods

        public static void Load()
        {
            //Utilities.LogMessage("Settings load started...", "Info", VerboseLogging);

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
                HatchWindowPosition = configfile.GetValue<Rect>("HatchWindowPosition");
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
                HatchColor = configfile.GetValue<string>("HatchColor");
                HatchOpenColor = configfile.GetValue<string>("HatchOpenColor");
                HatchCloseColor = configfile.GetValue<string>("HatchCloseColor");

                EnableHighlighting = configfile.GetValue<bool>("EnableHighlighting");
                OnlySourceTarget = configfile.GetValue<bool>("OnlySourceTarget");
                EnableCrew = configfile.GetValue<bool>("EnableCrew");
                EnableScience = configfile.GetValue<bool>("EnableScience");
                EnableResources = configfile.GetValue<bool>("EnableResources");
                EnablePFResources = configfile.GetValue<bool>("EnablePFResources");
                EnableCLS = configfile.GetValue<bool>("EnableCLS");
                EnableBlizzyToolbar = configfile.GetValue<bool>("EnableBlizzyToolbar");

                IVATimeDelaySec = configfile.GetValue<double>("IVATimeDelaySec");
                ShowIVAUpdateBtn = configfile.GetValue<bool>("ShowIVAUpdateBtn");
                AutoDebug = configfile.GetValue<bool>("AutoDebug");
                DebugLogPath = configfile.GetValue<string>("DebugLogPath");
                ErrorLogLength = configfile.GetValue<string>("ErrorLogLength");
                SaveLogOnExit = configfile.GetValue<bool>("SaveLogOnExit");
                EnableTextureReplacer = configfile.GetValue<bool>("EnableTextureReplacer");
                ShowToolTips = configfile.GetValue<bool>("ShowToolTips");

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
                if (!Colors.Keys.Contains(HatchColor))
                    SourcePartColor = "cyan";

                Utilities.LogMessage(string.Format("ManifestPosition Loaded: {0}, {1}, {2}, {3}", ManifestPosition.xMin, ManifestPosition.xMax, ManifestPosition.yMin, ManifestPosition.yMax), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("TransferPosition Loaded: {0}, {1}, {2}, {3}", TransferPosition.xMin, TransferPosition.xMax, TransferPosition.yMin, TransferPosition.yMax), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("ResourceDebuggerPosition Loaded: {0}, {1}, {2}, {3}", DebuggerPosition.xMin, DebuggerPosition.xMax, DebuggerPosition.yMin, DebuggerPosition.yMax), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("RosterPosition Loaded: {0}, {1}, {2}, {3}", RosterPosition.xMin, RosterPosition.xMax, RosterPosition.yMin, RosterPosition.yMax), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("SettingsPosition Loaded: {0}, {1}, {2}, {3}", SettingsPosition.xMin, SettingsPosition.xMax, SettingsPosition.yMin, SettingsPosition.yMax), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("ShowDebugger Loaded: {0}", ShowDebugger.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("RealismMode Loaded: {0}", RealismMode.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("LockSettings Loaded: {0}", LockSettings.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("VerboseLogging Loaded: {0}", VerboseLogging.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("AutoSave Loaded: {0}", AutoSave.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("SaveIntervalSec Loaded: {0}", SaveIntervalSec.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("FlowRate Loaded: {0}", FlowRate.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("MinFlowRate Loaded: {0}", MinFlowRate.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("MaxFlowRate Loaded: {0}", MaxFlowRate.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("PumpSoundStart Loaded: {0}", PumpSoundStart.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("PumpSoundRun Loaded: {0}", PumpSoundRun.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("PumpSoundStop Loaded: {0}", PumpSoundStop.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("PumpSoundVol Loaded: {0}", PumpSoundVol.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("CrewSoundStart Loaded: {0}", CrewSoundStart.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("CrewSoundRun Loaded: {0}", CrewSoundRun.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("CrewSoundStop Loaded: {0}", CrewSoundStop.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("CrewSoundVol Loaded: {0}", CrewSoundVol.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("SourcePartColor Loaded: {0}", SourcePartColor), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("TargetPartColor Loaded: {0}", TargetPartColor), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("TargetPartCrewColor Loaded: {0}", TargetPartCrewColor), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("HatchColor Loaded: {0}", HatchColor), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("HatchOpenColor Loaded: {0}", HatchOpenColor), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("HatchCloseColor Loaded: {0}", HatchCloseColor), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("EnableCrew Loaded: {0}", EnableCrew), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("EnableHighlighting Loaded: {0}", EnableHighlighting), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("OnlySourceTarget Loaded: {0}", OnlySourceTarget), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("EnableScience Loaded: {0}", EnableScience), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("EnableResources Loaded: {0}", EnableResources), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("EnablePFResources Loaded: {0}", EnablePFResources), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("EnableCLS Loaded: {0}", EnableCLS), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("IVATimeDelaySec Loaded: {0}", IVATimeDelaySec), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("ShowIVAUpdateBtn Loaded: {0}", ShowIVAUpdateBtn), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("AutoDebug Loaded: {0}", AutoDebug), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("ErrorLogLength Loaded: {0}", ErrorLogLength), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("SaveLogOnExit Loaded: {0}", SaveLogOnExit), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("EnableTextureReplacer Loaded: {0}", EnableTextureReplacer), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("EnableBlizzyToolbar Loaded: {0}", EnableBlizzyToolbar), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("ShowToolTips Loaded: {0}", ShowToolTips), "Info", VerboseLogging);
                Utilities.LogMessage("Load Settings Complete", "Info", VerboseLogging);

                ValidateLoad();
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format("Failed to Load Settings: {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        private static void ValidateLoad()
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

        public static void Save()
        {
            try
            {
                // For some reason, saving floats does not seem to work.  (maybe I just don't know enough, but converted flowrates to doubles and it works.
                KSP.IO.PluginConfiguration configfile = KSP.IO.PluginConfiguration.CreateForType<ShipManifestModule>();

                configfile.SetValue("ManifestPosition", ManifestPosition);
                configfile.SetValue("TransferPosition", TransferPosition);
                configfile.SetValue("RosterPosition", SettingsPosition);
                configfile.SetValue("SettingsPosition", SettingsPosition);
                configfile.SetValue("HatchWindowPosition", HatchWindowPosition);
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
                configfile.SetValue("HatchColor", HatchColor);
                configfile.SetValue("HatchOpenColor", HatchOpenColor);
                configfile.SetValue("HatchCloseColor", HatchCloseColor);

                configfile.SetValue("EnableHighlighting", EnableHighlighting);
                configfile.SetValue("OnlySourceTarget", OnlySourceTarget);
                configfile.SetValue("EnableCrew", EnableCrew);
                configfile.SetValue("EnableScience", EnableScience);
                configfile.SetValue("EnableResources", EnableResources);
                configfile.SetValue("EnablePFResources", EnablePFResources);
                configfile.SetValue("EnableCLS", EnableCLS);

                configfile.SetValue("DebugLogPath", DebugLogPath);

                configfile.SetValue("IVATimeDelaySec", IVATimeDelaySec);
                configfile.SetValue("ShowIVAUpdateBtn", ShowIVAUpdateBtn);
                configfile.SetValue("AutoDebug", AutoDebug);
                configfile.SetValue("ErrorLogLength", ErrorLogLength);
                configfile.SetValue("SaveLogOnExit", SaveLogOnExit);
                configfile.SetValue("EnableTextureReplacer", EnableTextureReplacer);
                configfile.SetValue("EnableBlizzyToolbar", EnableBlizzyToolbar);
                configfile.SetValue("ShowToolTips", ShowToolTips);

                configfile.save();

                Utilities.LogMessage(string.Format("ManifestPosition Saved: {0}, {1}, {2}, {3}", ManifestPosition.xMin, ManifestPosition.xMax, ManifestPosition.yMin, ManifestPosition.yMax), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("TransferPosition Saved: {0}, {1}, {2}, {3}", TransferPosition.xMin, TransferPosition.xMax, TransferPosition.yMin, TransferPosition.yMax), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("SettingsPosition Saved: {0}, {1}, {2}, {3}", SettingsPosition.xMin, SettingsPosition.xMax, SettingsPosition.yMin, SettingsPosition.yMax), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("DebuggerPosition Saved: {0}, {1}, {2}, {3}", DebuggerPosition.xMin, DebuggerPosition.xMax, DebuggerPosition.yMin, DebuggerPosition.yMax), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("ShowDebugger Saved: {0}", ShowDebugger.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("RealismMode Saved: {0}", RealismMode.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("LockSettings Saved: {0}", LockSettings.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("VerboseLogging Saved: {0}", VerboseLogging.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("AutoSave Saved: {0}", AutoSave.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("SaveIntervalSec Saved: {0}", SaveIntervalSec.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("FlowRate Saved: {0}", FlowRate.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("MinFlowRate Saved: {0}", MinFlowRate.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("MaxFlowRate Saved: {0}", MaxFlowRate.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("PumpSoundStart Saved: {0}", PumpSoundStart.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("PumpSoundRun Saved: {0}", PumpSoundRun.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("PumpSoundStop Saved: {0}", PumpSoundStop.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("PumpSoundVol Saved: {0}", PumpSoundVol.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("CrewSoundStart Saved: {0}", CrewSoundStart.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("CrewSoundRun Saved: {0}", CrewSoundRun.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("CrewSoundStop Saved: {0}", CrewSoundStop.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("CrewSoundVol Saved: {0}", CrewSoundVol.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("SourcePartColor Saved: {0}", SourcePartColor), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("TargetPartColor Saved: {0}", TargetPartColor), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("TargetPartCrewColor Saved: {0}", TargetPartCrewColor), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("HatchColor Saved: {0}", HatchColor), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("HatchOpenColor Saved: {0}", HatchOpenColor), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("HatchCloseColor Saved: {0}", HatchCloseColor), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("EnableHighlighting Saved: {0}", EnableHighlighting), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("OnlySourceTarget Saved: {0}", OnlySourceTarget), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("EnableCrew Saved: {0}", EnableCrew), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("EnableScience Saved: {0}", EnableScience.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("EnableResources Saved: {0}", EnableResources), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("EnablePFResources Saved: {0}", EnablePFResources), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("EnableCLS Saved: {0}", EnableCLS), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("DebugLogPath Saved: {0}", DebugLogPath.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("IVATimeDelaySec Saved: {0}", IVATimeDelaySec.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("ShowIVAUpdateBtn Saved: {0}", ShowIVAUpdateBtn.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("AutoDebug Saved: {0}", AutoDebug.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("ErrorListLength Saved: {0}", ErrorLogLength.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("SaveLogOnExit Saved: {0}", SaveLogOnExit.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("EnableTextureReplacer Saved: {0}", EnableTextureReplacer.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("EnableBlizzyToolbar Saved: {0}", EnableBlizzyToolbar.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("ShowToolTips Saved: {0}", ShowToolTips.ToString()), "Info", VerboseLogging);
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format("Failed to Save Settings: {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
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

        public static void StoreTempSettings()
        {
            prevRealismMode = RealismMode;
            prevShowDebugger = ShowDebugger;
            prevVerboseLogging = VerboseLogging;
            prevAutoSave = AutoSave;
            prevSaveIntervalSec = SaveIntervalSec;
            prevFlowRate = FlowRate;
            prevPumpSoundStart = PumpSoundStart;
            prevPumpSoundRun = PumpSoundRun;
            prevPumpSoundStop = PumpSoundStop;
            prevCrewSoundStart = CrewSoundStart;
            prevCrewSoundRun = CrewSoundRun;
            prevCrewSoundStop = CrewSoundStop;
            prevEnableScience = EnableScience;
            prevEnableHighlighting = EnableHighlighting;
            prevOnlySourceTarget = OnlySourceTarget;
            prevEnableCrew = EnableCrew;
            prevEnablePFResources = EnablePFResources;
            prevEnableCLS = EnableCLS;
            prevEnableTextureReplacer = EnableTextureReplacer;
            prevLockSettings = LockSettings;
            prevEnableBlizzyToolbar = EnableBlizzyToolbar;
            prevSaveLogOnExit = SaveLogOnExit;
            prevShowTooltips = ShowToolTips;

            // sounds

            //debugger Settings
            prevErrorLogLength = ErrorLogLength;
        }

        public static void RestoreTempSettings()
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
            EnableHighlighting = prevEnableHighlighting;
            OnlySourceTarget = prevOnlySourceTarget;
            EnableCrew = prevEnableCrew;
            EnablePFResources = prevEnablePFResources;
            EnableCLS = prevEnableCLS;
            EnableTextureReplacer = prevEnableTextureReplacer;
            LockSettings = prevLockSettings;
            EnableBlizzyToolbar = prevEnableBlizzyToolbar;
            SaveLogOnExit = prevSaveLogOnExit;
            ShowToolTips = prevShowTooltips;

            //debugger Settings
            prevErrorLogLength = ErrorLogLength;
        }

        #endregion
    }
}
