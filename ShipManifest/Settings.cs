using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace ShipManifest
{
    internal  static class Settings
    {
        #region Properties

        internal static Dictionary<string, Color> Colors;

        internal static string CurVersion = "0.90.0_4.1.2";

        // Persisted properties
        // Window Positions
        internal static Rect ManifestPosition;
        internal static Rect TransferPosition;
        internal static Rect RosterPosition;
        internal static Rect SettingsPosition;
        internal static Rect HatchPosition;
        internal static Rect PanelPosition;
        internal static Rect DebuggerPosition;

        // Realism Feature Options
        internal static bool RealismMode = true;
        internal static bool EnableCrew = true;
        internal static bool EnableScience = true;
        internal static bool EnableResources = true;
        internal static bool EnablePFResources = true;
        internal static bool EnableCLS = true;

        //Resource Xfer flow rate options
        internal static float FlowRate = 100;
        internal static float MaxFlowRate = 1000;
        internal static float MinFlowRate = 0;
        internal static int MaxFlowTimeSec = 180;
        internal static bool LockSettings = false;

        internal static bool ShowDebugger = false;
        internal static bool VerboseLogging = false;
        internal static bool AutoDebug = false;
        internal static bool SaveLogOnExit = false;

        internal static bool AutoSave = false;
        internal static float SaveIntervalSec = 60f;

        //Highlighting Options
        internal static bool EnableHighlighting = true;
        internal static bool OnlySourceTarget = false;
        internal static bool EnableCLSHighlighting = true;
        internal static bool EnableBlizzyToolbar = false; // off by default
        internal static bool EnableTextureReplacer = false;
        internal static string SourcePartColor = "red";
        internal static string TargetPartColor = "green";
        internal static string TargetPartCrewColor = "blue";
        internal static string CLS_SpaceColor = "green";
        internal static string HatchOpenColor = "cyan";
        internal static string HatchCloseColor = "red";
        internal static string PanelExtendedColor = "cyan";
        internal static string PanelRetractedColor = "red";

        // Sound Options
        internal static string PumpSoundStart = "ShipManifest/Sounds/59328-1";
        internal static string PumpSoundRun = "ShipManifest/Sounds/59328-2";
        internal static string PumpSoundStop = "ShipManifest/Sounds/59328-3";
        internal static string CrewSoundStart = "ShipManifest/Sounds/14214-1";
        internal static string CrewSoundRun = "ShipManifest/Sounds/14214-2";
        internal static string CrewSoundStop = "ShipManifest/Sounds/14214-3";
        internal static double PumpSoundVol = 3;
        internal static double CrewSoundVol = 3;

        internal static string ErrorLogLength = "1000";
        internal static double IVATimeDelaySec = 5;
        internal static bool ShowIVAUpdateBtn = false;

        // Tooltip Options
        internal static bool ShowToolTips = true;
        internal static bool ManifestToolTips = true;
        internal static bool TransferToolTips = true;
        internal static bool SettingsToolTips = true;
        internal static bool RosterToolTips = true;
        internal static bool HatchToolTips = true;
        internal static bool PanelToolTips = true;
        internal static bool DebuggerToolTips = true;

        // Roster Options
        internal static bool EnableKerbalRename = false;
        internal static bool RenameWithProfession = false;

        // End Persisted Properties

        // Settings Window Option storage for Cancel support
        internal static bool prevVerboseLogging = false;
        internal static bool prevShowDebugger = false;
        internal static string prevErrorLogLength = "1000";
        internal static bool prevSaveLogOnExit = true;
        internal static bool prevAutoSave;
        internal static float prevSaveIntervalSec = 60f;

        internal static bool prevRealismMode = false;
        internal static bool prevLockSettings = false;

        internal static float prevFlowRate = 100;
        internal static float prevMaxFlowRate = 1000;
        internal static float prevMinFlowRate = 0;
        internal static int prevMaxFlowTimeSec = 100;

        internal static bool prevEnableHighlighting = true;
        internal static bool prevOnlySourceTarget = false;
        internal static bool prevEnableCLSHighlighting = true;
        internal static bool prevEnableScience = true;
        internal static bool prevEnableCrew = true;
        internal static bool prevEnablePFResources = true;
        internal static bool prevEnableCLS = true;
        internal static bool prevEnableBlizzyToolbar = false;

        internal static string prevPumpSoundStart = "";
        internal static string prevPumpSoundRun = "";
        internal static string prevPumpSoundStop = "";

        internal static string prevCrewSoundStart = "";
        internal static string prevCrewSoundRun = "";
        internal static string prevCrewSoundStop = "";

        internal static bool prevShowToolTips = true;
        internal static bool prevManifestToolTips = true;
        internal static bool prevTransferToolTips = true;
        internal static bool prevSettingsToolTips = true;
        internal static bool prevRosterToolTips = true;
        internal static bool prevHatchToolTips = true;
        internal static bool prevPanelToolTips = true;
        internal static bool prevDebuggerToolTips = true;
        internal static bool prevEnableKerbalRename = false;
        internal static bool prevRenameWithProfession = false;
        internal static bool prevEnableTextureReplacer = false;

        // Internal properties for plugin management.  Not persisted, not user managed.
        // Flags to show windows
        internal static bool ShowTransferWindow { get; set; }
        private static bool _showShipManifest = false;
        internal static bool ShowShipManifest
        {
            get
            {
                return _showShipManifest;
            }
            set
            {
                _showShipManifest = value;
            }
        }

        private static bool _showSettings = false;
        internal static bool ShowSettings
        {
            get
            {
                return _showSettings;
            }
            set
            {
                _showSettings = value;
            }
        }

        private static bool _showRoster = false;
        internal static bool ShowRoster
        {
            get
            {
                return _showRoster;
            }
            set
            {
                _showRoster = value;
            }
        }

        internal static bool ShowHatch { get; set; }
        internal static bool ShowPanel { get; set; }

        internal static string DebugLogPath = "\\Plugins\\PluginData\\";
        internal static Color defaultColor = new Color(0.478f, 0.698f, 0.478f, 0.698f);
        internal static bool CLSInstalled = false;


        // Default sound license: CC-By-SA
        // http://www.freesound.org/people/vibe_crc/sounds/59328/

        #endregion

        #region Methods

        internal static void Load()
        {
            //Utilities.LogMessage("Settings load started...", "Info", VerboseLogging);

            try
            {
                LoadColors();

                // Interestingly, Floats seem to load fine.   Saves seem to be problematic.  attempts to save float are not persisted in the file...
                // Update:   to save floats. write the ToString value.
                // So, FlowRate vars now use double and are converted at load
                KSP.IO.PluginConfiguration configfile = KSP.IO.PluginConfiguration.CreateForType<ShipManifestModule>();
                configfile.load();

                ManifestPosition = configfile.GetValue<Rect>("ManifestPosition", ManifestPosition);
                TransferPosition = configfile.GetValue<Rect>("TransferPosition", TransferPosition);
                DebuggerPosition = configfile.GetValue<Rect>("DebuggerPosition", DebuggerPosition);
                SettingsPosition = configfile.GetValue<Rect>("SettingsPosition", SettingsPosition);
                HatchPosition = configfile.GetValue<Rect>("HatchPosition", HatchPosition);
                PanelPosition = configfile.GetValue<Rect>("PanelPosition", PanelPosition);
                RosterPosition = configfile.GetValue<Rect>("RosterPosition", RosterPosition);
                ShowDebugger = configfile.GetValue<bool>("ShowDebugger", ShowDebugger);
                RealismMode = configfile.GetValue<bool>("RealismMode", RealismMode);
                LockSettings = configfile.GetValue<bool>("LockSettings", LockSettings);
                VerboseLogging = configfile.GetValue<bool>("VerboseLogging", VerboseLogging);
                AutoSave = configfile.GetValue<bool>("AutoSave", AutoSave);
                SaveIntervalSec = (float)configfile.GetValue<double>("SaveIntervalSec", SaveIntervalSec);
                FlowRate = (float)configfile.GetValue<double>("FlowRate", FlowRate);
                MinFlowRate = (float)configfile.GetValue<double>("MinFlowRate", MinFlowRate);
                MaxFlowRate = (float)configfile.GetValue<double>("MaxFlowRate", MaxFlowRate);
                MaxFlowTimeSec = configfile.GetValue<int>("MaxFlowTimeSec", MaxFlowTimeSec);
                PumpSoundStart = configfile.GetValue<string>("PumpSoundStart", PumpSoundStart);
                PumpSoundRun = configfile.GetValue<string>("PumpSoundRun", PumpSoundRun);
                PumpSoundStop = configfile.GetValue<string>("PumpSoundStop", PumpSoundStop);
                CrewSoundStart = configfile.GetValue<string>("CrewSoundStart", CrewSoundStart);
                CrewSoundRun = configfile.GetValue<string>("CrewSoundRun", CrewSoundRun);
                CrewSoundStop = configfile.GetValue<string>("CrewSoundStop", CrewSoundStop);

                PumpSoundVol = configfile.GetValue<double>("PumpSoundVol", PumpSoundVol);
                CrewSoundVol = configfile.GetValue<double>("CrewSoundVol", CrewSoundVol);

                SourcePartColor = configfile.GetValue<string>("SourcePartColor", SourcePartColor);
                TargetPartColor = configfile.GetValue<string>("TargetPartColor", TargetPartColor);
                TargetPartCrewColor = configfile.GetValue<string>("TargetPartCrewColor", TargetPartCrewColor);
                HatchOpenColor = configfile.GetValue<string>("HatchOpenColor", HatchOpenColor);
                HatchCloseColor = configfile.GetValue<string>("HatchCloseColor", HatchCloseColor);
                PanelExtendedColor = configfile.GetValue<string>("PanelExtendedColor", PanelExtendedColor);
                PanelRetractedColor = configfile.GetValue<string>("PanelRetractedColor", PanelRetractedColor);

                EnableHighlighting = configfile.GetValue<bool>("EnableHighlighting", EnableHighlighting);
                OnlySourceTarget = configfile.GetValue<bool>("OnlySourceTarget", OnlySourceTarget);
                EnableCLSHighlighting = configfile.GetValue<bool>("EnableCLSHighlighting", EnableCLSHighlighting);
                EnableCrew = configfile.GetValue<bool>("EnableCrew", EnableCrew);
                EnableScience = configfile.GetValue<bool>("EnableScience", EnableScience);
                EnableResources = configfile.GetValue<bool>("EnableResources", EnableResources);
                EnablePFResources = configfile.GetValue<bool>("EnablePFResources", EnablePFResources);
                EnableCLS = configfile.GetValue<bool>("EnableCLS", EnableCLS);
                EnableBlizzyToolbar = configfile.GetValue<bool>("EnableBlizzyToolbar", EnableBlizzyToolbar);

                IVATimeDelaySec = configfile.GetValue<double>("IVATimeDelaySec", IVATimeDelaySec);
                ShowIVAUpdateBtn = configfile.GetValue<bool>("ShowIVAUpdateBtn", ShowIVAUpdateBtn);
                AutoDebug = configfile.GetValue<bool>("AutoDebug", AutoDebug);
                DebugLogPath = configfile.GetValue<string>("DebugLogPath", DebugLogPath);
                ErrorLogLength = configfile.GetValue<string>("ErrorLogLength", ErrorLogLength);
                SaveLogOnExit = configfile.GetValue<bool>("SaveLogOnExit", SaveLogOnExit);
                EnableKerbalRename = configfile.GetValue<bool>("EnableKerbalRename", EnableKerbalRename);
                RenameWithProfession = configfile.GetValue<bool>("RenameWithProfession", RenameWithProfession);
                EnableTextureReplacer = configfile.GetValue<bool>("EnableTextureReplacer", EnableTextureReplacer);
                ShowToolTips = configfile.GetValue<bool>("ShowToolTips", ShowToolTips);
                ManifestToolTips = configfile.GetValue<bool>("ManifestToolTips", ManifestToolTips);
                TransferToolTips = configfile.GetValue<bool>("TransferToolTips", TransferToolTips);
                SettingsToolTips = configfile.GetValue<bool>("SettingsToolTips", SettingsToolTips);
                RosterToolTips = configfile.GetValue<bool>("RosterToolTips", RosterToolTips);
                HatchToolTips = configfile.GetValue<bool>("HatchToolTips", HatchToolTips);
                PanelToolTips = configfile.GetValue<bool>("PanelToolTips", PanelToolTips);
                DebuggerToolTips = configfile.GetValue<bool>("DebuggerToolTips", DebuggerToolTips);

                // Lets make sure that the windows can be seen on the screen. (different resolutions)
                ResetWindowPositions();

                // Default values for Flow rates
                if (FlowRate == 0)
                    FlowRate = 100;
                if (MaxFlowRate == 0)
                    MaxFlowRate = 1000;

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

                Utilities.LogMessage(string.Format("ManifestPosition Loaded: {0}, {1}, {2}, {3}", ManifestPosition.xMin, ManifestPosition.xMax, ManifestPosition.yMin, ManifestPosition.yMax), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("TransferPosition Loaded: {0}, {1}, {2}, {3}", TransferPosition.xMin, TransferPosition.xMax, TransferPosition.yMin, TransferPosition.yMax), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("ResourceDebuggerPosition Loaded: {0}, {1}, {2}, {3}", DebuggerPosition.xMin, DebuggerPosition.xMax, DebuggerPosition.yMin, DebuggerPosition.yMax), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("RosterPosition Loaded: {0}, {1}, {2}, {3}", RosterPosition.xMin, RosterPosition.xMax, RosterPosition.yMin, RosterPosition.yMax), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("SettingsPosition Loaded: {0}, {1}, {2}, {3}", SettingsPosition.xMin, SettingsPosition.xMax, SettingsPosition.yMin, SettingsPosition.yMax), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("HatchPosition Loaded: {0}, {1}, {2}, {3}", HatchPosition.xMin, HatchPosition.xMax, HatchPosition.yMin, HatchPosition.yMax), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("ShowDebugger Loaded: {0}", ShowDebugger.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("RealismMode Loaded: {0}", RealismMode.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("LockSettings Loaded: {0}", LockSettings.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("VerboseLogging Loaded: {0}", VerboseLogging.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("AutoSave Loaded: {0}", AutoSave.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("SaveIntervalSec Loaded: {0}", SaveIntervalSec.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("FlowRate Loaded: {0}", FlowRate.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("MinFlowRate Loaded: {0}", MinFlowRate.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("MaxFlowRate Loaded: {0}", MaxFlowRate.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("MaxFlowTimeSec Loaded: {0}", MaxFlowTimeSec.ToString()), "Info", VerboseLogging);
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
                Utilities.LogMessage(string.Format("HatchOpenColor Loaded: {0}", HatchOpenColor), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("HatchCloseColor Loaded: {0}", HatchCloseColor), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("PanelExtendedColor Loaded: {0}", PanelExtendedColor), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("PanelRetractedColor Loaded: {0}", PanelRetractedColor), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("EnableCrew Loaded: {0}", EnableCrew), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("EnableHighlighting Loaded: {0}", EnableHighlighting), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("OnlySourceTarget Loaded: {0}", OnlySourceTarget), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("EnableCLSHighlighting Loaded: {0}", EnableCLSHighlighting), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("EnableScience Loaded: {0}", EnableScience), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("EnableResources Loaded: {0}", EnableResources), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("EnablePFResources Loaded: {0}", EnablePFResources), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("EnableCLS Loaded: {0}", EnableCLS), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("IVATimeDelaySec Loaded: {0}", IVATimeDelaySec), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("ShowIVAUpdateBtn Loaded: {0}", ShowIVAUpdateBtn), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("AutoDebug Loaded: {0}", AutoDebug), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("ErrorLogLength Loaded: {0}", ErrorLogLength), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("SaveLogOnExit Loaded: {0}", SaveLogOnExit), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("EnableKerbalRename Loaded: {0}", EnableKerbalRename), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("RenameWithProfession Loaded: {0}", RenameWithProfession), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("EnableTextureReplacer Loaded: {0}", EnableTextureReplacer), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("EnableBlizzyToolbar Loaded: {0}", EnableBlizzyToolbar), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("ShowToolTips Loaded: {0}", ShowToolTips), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("ManifestToolTips Loaded: {0}", ManifestToolTips), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("TransferToolTips Loaded: {0}", TransferToolTips), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("SettingsToolTips Loaded: {0}", SettingsToolTips), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("RosterToolTips Loaded: {0}", RosterToolTips), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("HatchToolTips Loaded: {0}", HatchToolTips), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("DebuggerToolTips Loaded: {0}", DebuggerToolTips), "Info", VerboseLogging);
                Utilities.LogMessage("Load Settings Complete", "Info", VerboseLogging);

            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format("Failed to Load Settings: {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        private static void ResetWindowPositions()
        {
            if (ManifestPosition.xMax > Screen.currentResolution.width || ManifestPosition.yMax > Screen.currentResolution.height)
            {
                ManifestPosition.x = 0;
                ManifestPosition.y = 0;
            }
            if (TransferPosition.xMax > Screen.currentResolution.width || TransferPosition.yMax > Screen.currentResolution.height)
            {
                TransferPosition.x = 0;
                TransferPosition.y = 0;
            }
            if (DebuggerPosition.xMax > Screen.currentResolution.width || DebuggerPosition.yMax > Screen.currentResolution.height)
            {
                DebuggerPosition.x = 0;
                DebuggerPosition.y = 0;
            }
            if (SettingsPosition.xMax > Screen.currentResolution.width || SettingsPosition.yMax > Screen.currentResolution.height)
            {
                SettingsPosition.x = 0;
                SettingsPosition.y = 0;
            }
            if (HatchPosition.xMax > Screen.currentResolution.width || HatchPosition.yMax > Screen.currentResolution.height)
            {
                HatchPosition.x = 0;
                HatchPosition.y = 0;
            }
            if (PanelPosition.xMax > Screen.currentResolution.width || PanelPosition.yMax > Screen.currentResolution.height)
            {
                PanelPosition.x = 0;
                PanelPosition.y = 0;
            }
            if (RosterPosition.xMax > Screen.currentResolution.width || RosterPosition.yMax > Screen.currentResolution.height)
            {
                RosterPosition.x = 0;
                RosterPosition.y = 0;
            }
        }

        internal static void Save()
        {
            try
            {
                // For some reason, saving floats does not seem to work.  (maybe I just don't know enough, but converted flowrates to doubles and it works.
                KSP.IO.PluginConfiguration configfile = KSP.IO.PluginConfiguration.CreateForType<ShipManifestModule>();

                configfile.SetValue("ManifestPosition", ManifestPosition);
                configfile.SetValue("TransferPosition", TransferPosition);
                configfile.SetValue("RosterPosition", SettingsPosition);
                configfile.SetValue("SettingsPosition", SettingsPosition);
                configfile.SetValue("HatchPosition", HatchPosition);
                configfile.SetValue("PanelPosition", PanelPosition);
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
                configfile.SetValue("MaxFlowTimeSec", MaxFlowTimeSec);
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
                configfile.SetValue("HatchOpenColor", HatchOpenColor);
                configfile.SetValue("HatchCloseColor", HatchCloseColor);
                configfile.SetValue("PanelExtendedColor", PanelExtendedColor);
                configfile.SetValue("PanelRetractedColor", PanelRetractedColor);

                configfile.SetValue("EnableHighlighting", EnableHighlighting);
                configfile.SetValue("OnlySourceTarget", OnlySourceTarget);
                configfile.SetValue("EnableCLSHighlighting", EnableCLSHighlighting);
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
                configfile.SetValue("EnableKerbalRename", EnableKerbalRename);
                configfile.SetValue("RenameWithProfession", RenameWithProfession);
                configfile.SetValue("EnableTextureReplacer", EnableTextureReplacer);
                configfile.SetValue("EnableBlizzyToolbar", EnableBlizzyToolbar);
                configfile.SetValue("ShowToolTips", ShowToolTips);
                configfile.SetValue("ManifestToolTips", ManifestToolTips);
                configfile.SetValue("TransferToolTips", TransferToolTips);
                configfile.SetValue("SettingsToolTips", SettingsToolTips);
                configfile.SetValue("RosterToolTips", RosterToolTips);
                configfile.SetValue("HatchToolTips", HatchToolTips);
                configfile.SetValue("PanelToolTips", PanelToolTips);
                configfile.SetValue("DebuggerToolTips", DebuggerToolTips);

                configfile.save();

                Utilities.LogMessage(string.Format("ManifestPosition Saved: {0}, {1}, {2}, {3}", ManifestPosition.xMin, ManifestPosition.xMax, ManifestPosition.yMin, ManifestPosition.yMax), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("TransferPosition Saved: {0}, {1}, {2}, {3}", TransferPosition.xMin, TransferPosition.xMax, TransferPosition.yMin, TransferPosition.yMax), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("SettingsPosition Saved: {0}, {1}, {2}, {3}", SettingsPosition.xMin, SettingsPosition.xMax, SettingsPosition.yMin, SettingsPosition.yMax), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("RosterPosition Saved: {0}, {1}, {2}, {3}", RosterPosition.xMin, RosterPosition.xMax, RosterPosition.yMin, RosterPosition.yMax), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("DebuggerPosition Saved: {0}, {1}, {2}, {3}", DebuggerPosition.xMin, DebuggerPosition.xMax, DebuggerPosition.yMin, DebuggerPosition.yMax), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("HatchPosition Saved: {0}, {1}, {2}, {3}", HatchPosition.xMin, HatchPosition.xMax, HatchPosition.yMin, HatchPosition.yMax), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("PanelPosition Saved: {0}, {1}, {2}, {3}", PanelPosition.xMin, HatchPosition.xMax, HatchPosition.yMin, HatchPosition.yMax), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("ShowDebugger Saved: {0}", ShowDebugger.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("RealismMode Saved: {0}", RealismMode.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("LockSettings Saved: {0}", LockSettings.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("VerboseLogging Saved: {0}", VerboseLogging.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("AutoSave Saved: {0}", AutoSave.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("SaveIntervalSec Saved: {0}", SaveIntervalSec.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("FlowRate Saved: {0}", FlowRate.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("MinFlowRate Saved: {0}", MinFlowRate.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("MaxFlowRate Saved: {0}", MaxFlowRate.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("MaxFlowTimeSec Saved: {0}", MaxFlowTimeSec.ToString()), "Info", VerboseLogging);
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
                Utilities.LogMessage(string.Format("HatchOpenColor Saved: {0}", HatchOpenColor), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("HatchCloseColor Saved: {0}", HatchCloseColor), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("EnableHighlighting Saved: {0}", EnableHighlighting), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("OnlySourceTarget Saved: {0}", OnlySourceTarget), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("EnableCLSHighlighting Saved: {0}", EnableCLSHighlighting), "Info", VerboseLogging);
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
                Utilities.LogMessage(string.Format("EnableKerbalRename Saved: {0}", EnableKerbalRename.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("RenameWithProfession Saved: {0}", RenameWithProfession.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("EnableTextureReplacer Saved: {0}", EnableTextureReplacer.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("EnableBlizzyToolbar Saved: {0}", EnableBlizzyToolbar.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("ShowToolTips Saved: {0}", ShowToolTips.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("ManifestToolTips Saved: {0}", ManifestToolTips.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("TransferToolTips Saved: {0}", TransferToolTips.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("SettingsToolTips Saved: {0}", SettingsToolTips.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("RosterToolTips Saved: {0}", RosterToolTips.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("HatchToolTips Saved: {0}", HatchToolTips.ToString()), "Info", VerboseLogging);
                Utilities.LogMessage(string.Format("DebuggerToolTips Saved: {0}", DebuggerToolTips.ToString()), "Info", VerboseLogging);
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format("Failed to Save Settings: {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        internal static void LoadColors()
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

        internal static void StoreTempSettings()
        {
            prevRealismMode = RealismMode;
            prevShowDebugger = ShowDebugger;
            prevVerboseLogging = VerboseLogging;
            prevAutoSave = AutoSave;
            prevSaveIntervalSec = SaveIntervalSec;
            prevFlowRate = FlowRate;
            prevMinFlowRate = MinFlowRate;
            prevMaxFlowRate = MaxFlowRate;
            prevMaxFlowTimeSec = MaxFlowTimeSec;
            prevPumpSoundStart = PumpSoundStart;
            prevPumpSoundRun = PumpSoundRun;
            prevPumpSoundStop = PumpSoundStop;
            prevCrewSoundStart = CrewSoundStart;
            prevCrewSoundRun = CrewSoundRun;
            prevCrewSoundStop = CrewSoundStop;
            prevEnableScience = EnableScience;
            prevEnableHighlighting = EnableHighlighting;
            prevOnlySourceTarget = OnlySourceTarget;
            prevEnableCLSHighlighting = EnableCLSHighlighting;
            prevEnableCrew = EnableCrew;
            prevEnablePFResources = EnablePFResources;
            prevEnableCLS = EnableCLS;
            prevEnableKerbalRename = EnableKerbalRename;
            prevRenameWithProfession = RenameWithProfession;
            prevEnableTextureReplacer = EnableTextureReplacer;
            prevLockSettings = LockSettings;
            prevEnableBlizzyToolbar = EnableBlizzyToolbar;
            prevSaveLogOnExit = SaveLogOnExit;
            prevShowToolTips = ShowToolTips;
            prevManifestToolTips = ManifestToolTips;
            prevTransferToolTips = TransferToolTips;
            prevSettingsToolTips = SettingsToolTips;
            prevRosterToolTips = RosterToolTips;
            prevHatchToolTips = HatchToolTips;
            prevPanelToolTips = PanelToolTips;
            prevDebuggerToolTips = DebuggerToolTips;

            // sounds

            //debugger Settings
            prevErrorLogLength = ErrorLogLength;
        }

        internal static void RestoreTempSettings()
        {
            RealismMode = prevRealismMode;
            ShowDebugger = prevShowDebugger;
            VerboseLogging = prevVerboseLogging;
            AutoSave = prevAutoSave;
            SaveIntervalSec = prevSaveIntervalSec;
            FlowRate = prevFlowRate;
            MinFlowRate = prevMinFlowRate;
            MaxFlowRate = prevMaxFlowRate;
            MaxFlowTimeSec = prevMaxFlowTimeSec;
            PumpSoundStart = prevPumpSoundStart;
            PumpSoundRun = prevPumpSoundRun;
            PumpSoundStop = prevPumpSoundStop;
            CrewSoundStart = prevCrewSoundStart;
            CrewSoundRun = prevCrewSoundRun;
            CrewSoundStop = prevCrewSoundStop;
            EnableScience = prevEnableScience;
            EnableHighlighting = prevEnableHighlighting;
            OnlySourceTarget = prevOnlySourceTarget;
            EnableCLSHighlighting = prevEnableCLSHighlighting;
            EnableCrew = prevEnableCrew;
            EnablePFResources = prevEnablePFResources;
            EnableCLS = prevEnableCLS;
            EnableKerbalRename = prevEnableKerbalRename;
            RenameWithProfession = prevRenameWithProfession;
            EnableTextureReplacer = prevEnableTextureReplacer;
            LockSettings = prevLockSettings;
            EnableBlizzyToolbar = prevEnableBlizzyToolbar;
            SaveLogOnExit = prevSaveLogOnExit;
            ShowToolTips = prevShowToolTips;
            ManifestToolTips = prevManifestToolTips;
            TransferToolTips = prevTransferToolTips;
            SettingsToolTips = prevSettingsToolTips;
            RosterToolTips = prevRosterToolTips;
            HatchToolTips = prevHatchToolTips;
            PanelToolTips = prevPanelToolTips;
            DebuggerToolTips = prevDebuggerToolTips;

            //debugger Settings
            prevErrorLogLength = ErrorLogLength;
        }

        #endregion
    }
}
