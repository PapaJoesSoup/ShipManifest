using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;
using System.Reflection;

namespace ShipManifest
{
    internal  static class Settings
    {
        #region Properties

        internal static bool Loaded = false;
        
        internal static Dictionary<string, Color> Colors;

        internal static ConfigNode settings = null;
        private static readonly string SETTINGS_PATH = KSPUtil.ApplicationRootPath + "GameData/ShipManifest/Plugins/PluginData";
        private static readonly string SETTINGS_FILE = SETTINGS_PATH + "/SMSettings.dat";
        internal static string CurVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        // Persisted properties
        // Window Positions
        internal static Rect ManifestPosition = new Rect(0, 0, 0, 0);
        internal static Rect TransferPosition = new Rect(0, 0, 0, 0);
        internal static Rect RosterPosition = new Rect(0, 0, 0, 0);
        internal static Rect SettingsPosition = new Rect(0, 0, 0, 0);
        internal static Rect ControlPosition = new Rect(0, 0, 0, 0);
        internal static Rect DebuggerPosition = new Rect(0, 0, 0, 0);

        // Realism Feature Options
        internal static bool RealismMode = true;
        internal static bool EnableCrew = true;
        internal static bool EnableScience = true;
        internal static bool EnableResources = true;
        internal static bool EnablePFResources = true;
        internal static bool EnableCLS = true;
        internal static bool OverrideStockCrewXfer = true;
        internal static bool LockSettings = false;

        //Resource Xfer flow rate options
        internal static double FlowRate = 100;
        internal static double FlowCost = 0.0015;
        internal static double MaxFlowRate = 1000;
        internal static double MinFlowRate = 0;
        internal static int MaxFlowTimeSec = 180;
        internal static bool EnableXferCost = true;

        internal static bool ShowDebugger = false;
        internal static bool VerboseLogging = false;
        internal static bool AutoDebug = false;
        internal static bool SaveLogOnExit = false;

        internal static bool AutoSave = false;
        internal static int SaveIntervalSec = 60;

        //Highlighting Options
        internal static bool EnableHighlighting = true;
        internal static bool OnlySourceTarget = false;
        internal static bool EnableCLSHighlighting = true;
        internal static bool EnableEdgeHighlighting = true;
        internal static string ResourcePartColor = "yellow";
        internal static string SourcePartColor = "red";
        internal static string TargetPartColor = "green";
        internal static string TargetPartCrewColor = "blue";
        internal static string CLS_SpaceColor = "green";
        internal static string MouseOverColor = "green";

        // Sound Options
        // Default sound license: CC-By-SA
        // http://www.freesound.org/people/vibe_crc/sounds/59328/

        internal static string PumpSoundStart = "ShipManifest/Sounds/59328-1";
        internal static string PumpSoundRun = "ShipManifest/Sounds/59328-2";
        internal static string PumpSoundStop = "ShipManifest/Sounds/59328-3";
        internal static string CrewSoundStart = "ShipManifest/Sounds/14214-1";
        internal static string CrewSoundRun = "ShipManifest/Sounds/14214-2";
        internal static string CrewSoundStop = "ShipManifest/Sounds/14214-3";
        internal static double PumpSoundVol = 3;
        internal static double CrewSoundVol = 3;

        internal static bool EnableBlizzyToolbar = false;
        internal static bool EnableTextureReplacer = false;
        internal static string ErrorLogLength = "1000";
        internal static double IVATimeDelaySec = 7;
        internal static bool ShowIVAUpdateBtn = false;

        // Tooltip Options
        internal static bool ShowToolTips = true;
        internal static bool ManifestToolTips = true;
        internal static bool TransferToolTips = true;
        internal static bool SettingsToolTips = true;
        internal static bool RosterToolTips = true;
        internal static bool HatchToolTips = true;
        internal static bool PanelToolTips = true;
        internal static bool AntennaToolTips = true;
        internal static bool LightToolTips = true;
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
        internal static int prevSaveIntervalSec = 60;

        internal static bool prevRealismMode = false;
        internal static bool prevLockSettings = false;

        internal static double prevFlowRate = 100;
        internal static double prevFlowCost = 0.0015;
        internal static double prevMaxFlowRate = 1000;
        internal static double prevMinFlowRate = 0;
        internal static int prevMaxFlowTimeSec = 100;
        internal static bool prevEnableXferCost = true;

        internal static bool prevEnableHighlighting = true;
        internal static bool prevOnlySourceTarget = false;
        internal static bool prevEnableCLSHighlighting = true;
        internal static bool prevEnableEdgeHighlighting = true;
        internal static bool prevEnableScience = true;
        internal static bool prevEnableCrew = true;
        internal static bool prevOverrideStockCrewXfer = true;
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
        internal static bool prevAntennaToolTips = true;
        internal static bool prevLightToolTips = true;
        internal static bool prevDebuggerToolTips = true;
        internal static bool prevEnableKerbalRename = false;
        internal static bool prevRenameWithProfession = false;
        internal static bool prevEnableTextureReplacer = false;

        // Internal properties for plugin management.  Not persisted, not user managed.
        // Flags to show windows

        internal static bool _ShowUI = true;
        internal static bool ShowUI {
            get
            {
                return _ShowUI;
            }
            set
            {
                _ShowUI = value;
            }
        }
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

        internal static bool ShowControl { get; set; }

        internal static string DebugLogPath = @"Plugins\PluginData\";
        internal static bool CLSInstalled = false;


        #endregion

        #region Methods


        internal static ConfigNode loadSettingsFile()
        {
            if (settings == null)
                settings = ConfigNode.Load(SETTINGS_FILE) ?? new ConfigNode();
            return settings;
        }

        internal static void ApplySettings()
        {
            LoadColors();

            if (settings == null)
                loadSettingsFile();
            ConfigNode WindowsNode = settings.HasNode("SM_Windows") ? settings.GetNode("SM_Windows") : settings.AddNode("SM_Windows");
            ConfigNode SettingsNode = settings.HasNode("SM_Settings") ? settings.GetNode("SM_Settings") : settings.AddNode("SM_Settings");

            // Lets get our rectangles...
            ManifestPosition = getRectangle(WindowsNode, "ManifestPosition", ManifestPosition);
            TransferPosition = getRectangle(WindowsNode, "TransferPosition", TransferPosition);
            DebuggerPosition = getRectangle(WindowsNode, "DebuggerPosition", DebuggerPosition);
            SettingsPosition = getRectangle(WindowsNode, "SettingsPosition", SettingsPosition);
            ControlPosition = getRectangle(WindowsNode, "ControlPosition", ControlPosition);
            RosterPosition = getRectangle(WindowsNode, "RosterPosition", RosterPosition);


            // now the settings

            // Realism Settings
            RealismMode = SettingsNode.HasValue("RealismMode") ? bool.Parse(SettingsNode.GetValue("RealismMode")) : RealismMode;
            EnableCrew = SettingsNode.HasValue("EnableCrew") ? bool.Parse(SettingsNode.GetValue("EnableCrew")) : EnableCrew;
            EnableScience = SettingsNode.HasValue("EnableScience") ? bool.Parse(SettingsNode.GetValue("EnableScience")) : EnableScience;
            EnableResources = SettingsNode.HasValue("EnableResources") ? bool.Parse(SettingsNode.GetValue("EnableResources")) : EnableResources;
            EnablePFResources = SettingsNode.HasValue("EnablePFResources") ? bool.Parse(SettingsNode.GetValue("EnablePFResources")) : EnablePFResources;
            EnableCLS = SettingsNode.HasValue("EnableCLS") ? bool.Parse(SettingsNode.GetValue("EnableCLS")) : EnableCLS;
            OverrideStockCrewXfer = SettingsNode.HasValue("OverrideStockCrewXfer") ? bool.Parse(SettingsNode.GetValue("OverrideStockCrewXfer")) : OverrideStockCrewXfer;
            FlowRate = SettingsNode.HasValue("FlowRate") ? double.Parse(SettingsNode.GetValue("FlowRate")) : FlowRate;
            FlowCost = SettingsNode.HasValue("FlowCost") ? double.Parse(SettingsNode.GetValue("FlowCost")) : FlowCost;
            MinFlowRate = SettingsNode.HasValue("MinFlowRate") ? double.Parse(SettingsNode.GetValue("MinFlowRate")) : MinFlowRate;
            MaxFlowRate = SettingsNode.HasValue("MaxFlowRate") ? double.Parse(SettingsNode.GetValue("MaxFlowRate")) : MaxFlowRate;
            MaxFlowTimeSec = SettingsNode.HasValue("MaxFlowTimeSec") ? int.Parse(SettingsNode.GetValue("MaxFlowTimeSec")) : MaxFlowTimeSec;
            EnableXferCost = SettingsNode.HasValue("EnableXferCost") ? bool.Parse(SettingsNode.GetValue("EnableXferCost")) : EnableXferCost;
            LockSettings = SettingsNode.HasValue("LockSettings") ? bool.Parse(SettingsNode.GetValue("LockSettings")) : LockSettings;

            // Highlighting settings
            EnableHighlighting = SettingsNode.HasValue("EnableHighlighting") ? bool.Parse(SettingsNode.GetValue("EnableHighlighting")) : EnableHighlighting;
            OnlySourceTarget = SettingsNode.HasValue("OnlySourceTarget") ? bool.Parse(SettingsNode.GetValue("OnlySourceTarget")) : OnlySourceTarget;
            EnableCLSHighlighting = SettingsNode.HasValue("EnableCLSHighlighting") ? bool.Parse(SettingsNode.GetValue("EnableCLSHighlighting")) : EnableCLSHighlighting;
            EnableEdgeHighlighting = SettingsNode.HasValue("EnableEdgeHighlighting") ? bool.Parse(SettingsNode.GetValue("EnableEdgeHighlighting")) : EnableCLSHighlighting;
            ResourcePartColor = SettingsNode.HasValue("ResourcePartColor") ? SettingsNode.GetValue("ResourcePartColor") : ResourcePartColor;
            SourcePartColor = SettingsNode.HasValue("SourcePartColor") ? SettingsNode.GetValue("SourcePartColor") : SourcePartColor;
            TargetPartColor = SettingsNode.HasValue("TargetPartColor") ? SettingsNode.GetValue("TargetPartColor") : TargetPartColor;
            TargetPartCrewColor = SettingsNode.HasValue("TargetPartCrewColor") ? SettingsNode.GetValue("TargetPartCrewColor") : TargetPartCrewColor;
            MouseOverColor = SettingsNode.HasValue("MouseOverColor") ? SettingsNode.GetValue("MouseOverColor") : MouseOverColor;

            // ToolTip Settings
            ShowToolTips = SettingsNode.HasValue("ShowToolTips") ? bool.Parse(SettingsNode.GetValue("ShowToolTips")) : ShowToolTips;
            ManifestToolTips = SettingsNode.HasValue("ManifestToolTips") ? bool.Parse(SettingsNode.GetValue("ManifestToolTips")) : ManifestToolTips;
            TransferToolTips = SettingsNode.HasValue("TransferToolTips") ? bool.Parse(SettingsNode.GetValue("TransferToolTips")) : TransferToolTips;
            SettingsToolTips = SettingsNode.HasValue("SettingsToolTips") ? bool.Parse(SettingsNode.GetValue("SettingsToolTips")) : SettingsToolTips;
            RosterToolTips = SettingsNode.HasValue("RosterToolTips") ? bool.Parse(SettingsNode.GetValue("RosterToolTips")) : RosterToolTips;
            HatchToolTips = SettingsNode.HasValue("HatchToolTips") ? bool.Parse(SettingsNode.GetValue("HatchToolTips")) : HatchToolTips;
            PanelToolTips = SettingsNode.HasValue("PanelToolTips") ? bool.Parse(SettingsNode.GetValue("PanelToolTips")) : PanelToolTips;
            AntennaToolTips = SettingsNode.HasValue("AntennaToolTips") ? bool.Parse(SettingsNode.GetValue("AntennaToolTips")) : AntennaToolTips;
            LightToolTips = SettingsNode.HasValue("LightToolTips") ? bool.Parse(SettingsNode.GetValue("LightToolTips")) : LightToolTips;
            DebuggerToolTips = SettingsNode.HasValue("DebuggerToolTips") ? bool.Parse(SettingsNode.GetValue("DebuggerToolTips")) : DebuggerToolTips;

            // Sounds Settings
            PumpSoundStart = SettingsNode.HasValue("PumpSoundStart") ? SettingsNode.GetValue("PumpSoundStart") : PumpSoundStart;
            PumpSoundRun = SettingsNode.HasValue("PumpSoundRun") ? SettingsNode.GetValue("PumpSoundRun") : PumpSoundRun;
            PumpSoundStop = SettingsNode.HasValue("PumpSoundStop") ? SettingsNode.GetValue("PumpSoundStop") : PumpSoundStop;
            CrewSoundStart = SettingsNode.HasValue("CrewSoundStart") ? SettingsNode.GetValue("CrewSoundStart") : CrewSoundStart;
            CrewSoundRun = SettingsNode.HasValue("CrewSoundRun") ? SettingsNode.GetValue("CrewSoundRun") : CrewSoundRun;
            CrewSoundStop = SettingsNode.HasValue("CrewSoundStop") ? SettingsNode.GetValue("CrewSoundStop") : CrewSoundStop;
            PumpSoundVol = SettingsNode.HasValue("PumpSoundVol") ? double.Parse(SettingsNode.GetValue("PumpSoundVol")) : PumpSoundVol;
            CrewSoundVol = SettingsNode.HasValue("CrewSoundVol") ? double.Parse(SettingsNode.GetValue("CrewSoundVol")) : CrewSoundVol;

            // Config Settings
            EnableBlizzyToolbar = SettingsNode.HasValue("EnableBlizzyToolbar") ? bool.Parse(SettingsNode.GetValue("EnableBlizzyToolbar")) : EnableBlizzyToolbar;
            ShowDebugger = SettingsNode.HasValue("ShowDebugger") ? bool.Parse(SettingsNode.GetValue("ShowDebugger")) : ShowDebugger;
            VerboseLogging = SettingsNode.HasValue("VerboseLogging") ? bool.Parse(SettingsNode.GetValue("VerboseLogging")) : VerboseLogging;
            AutoSave = SettingsNode.HasValue("AutoSave") ? bool.Parse(SettingsNode.GetValue("AutoSave")) : AutoSave;
            SaveIntervalSec = SettingsNode.HasValue("ShowDebugger") ? int.Parse(SettingsNode.GetValue("SaveIntervalSec")) : SaveIntervalSec;
            AutoDebug = SettingsNode.HasValue("AutoDebug") ? bool.Parse(SettingsNode.GetValue("AutoDebug")) : AutoDebug;
            DebugLogPath = SettingsNode.HasValue("DebugLogPath") ? SettingsNode.GetValue("DebugLogPath") : DebugLogPath;
            ErrorLogLength = SettingsNode.HasValue("ErrorLogLength") ? SettingsNode.GetValue("ErrorLogLength") : ErrorLogLength;
            SaveLogOnExit = SettingsNode.HasValue("SaveLogOnExit") ? bool.Parse(SettingsNode.GetValue("SaveLogOnExit")) : SaveLogOnExit;
            EnableKerbalRename = SettingsNode.HasValue("EnableKerbalRename") ? bool.Parse(SettingsNode.GetValue("EnableKerbalRename")) : EnableKerbalRename;
            RenameWithProfession = SettingsNode.HasValue("RenameWithProfession") ? bool.Parse(SettingsNode.GetValue("RenameWithProfession")) : RenameWithProfession;


            // Other Settings
            IVATimeDelaySec = SettingsNode.HasValue("IVATimeDelaySec") ? double.Parse(SettingsNode.GetValue("IVATimeDelaySec")) : IVATimeDelaySec;
            ShowIVAUpdateBtn = SettingsNode.HasValue("ShowIVAUpdateBtn") ? bool.Parse(SettingsNode.GetValue("ShowIVAUpdateBtn")) : ShowIVAUpdateBtn;
            EnableTextureReplacer = SettingsNode.HasValue("EnableTextureReplacer") ? bool.Parse(SettingsNode.GetValue("EnableTextureReplacer")) : EnableTextureReplacer;

            // Okay, set the Settings loaded flag
            Loaded = true;

            // Lets make sure that the windows can be seen on the screen. (supports different resolutions)
            RepositionWindows();

        }

        internal static void SaveSettings()
        {
            if (settings == null)
                settings = loadSettingsFile();

            ConfigNode WindowsNode = settings.HasNode("SM_Windows") ? settings.GetNode("SM_Windows") : settings.AddNode("SM_Windows");
            ConfigNode SettingsNode = settings.HasNode("SM_Settings") ? settings.GetNode("SM_Settings") : settings.AddNode("SM_Settings");

            // Write window positions
            WriteRectangle(WindowsNode, "ManifestPosition", ManifestPosition);
            WriteRectangle(WindowsNode, "TransferPosition", TransferPosition);
            WriteRectangle(WindowsNode, "DebuggerPosition", DebuggerPosition);
            WriteRectangle(WindowsNode, "SettingsPosition", SettingsPosition);
            WriteRectangle(WindowsNode, "ControlPosition", ControlPosition);
            WriteRectangle(WindowsNode, "RosterPosition", RosterPosition);

            //Write settings...
            // Realism Settings
            WriteValue(SettingsNode, "RealismMode", RealismMode);
            WriteValue(SettingsNode, "EnableCrew", EnableCrew);
            WriteValue(SettingsNode, "EnableScience", EnableScience);
            WriteValue(SettingsNode, "EnableResources", EnableResources);
            WriteValue(SettingsNode, "EnablePFResources", EnablePFResources);
            WriteValue(SettingsNode, "EnableCLS", EnableCLS);
            WriteValue(SettingsNode, "OverrideStockCrewXfer", OverrideStockCrewXfer);
            WriteValue(SettingsNode, "FlowRate", FlowRate);
            WriteValue(SettingsNode, "FlowCost", FlowCost);
            WriteValue(SettingsNode, "MinFlowRate", MinFlowRate);
            WriteValue(SettingsNode, "MaxFlowRate", MaxFlowRate);
            WriteValue(SettingsNode, "MaxFlowTimeSec", MaxFlowTimeSec);
            WriteValue(SettingsNode, "EnableXferCost", EnableXferCost);
            WriteValue(SettingsNode, "LockSettings", LockSettings);

            // Highlighting Settings
            WriteValue(SettingsNode, "EnableHighlighting", EnableHighlighting);
            WriteValue(SettingsNode, "OnlySourceTarget", OnlySourceTarget);
            WriteValue(SettingsNode, "EnableCLSHighlighting", EnableCLSHighlighting);
            WriteValue(SettingsNode, "EnableEdgeHighlighting", EnableEdgeHighlighting);
            WriteValue(SettingsNode, "ResourcePartColor", ResourcePartColor);
            WriteValue(SettingsNode, "SourcePartColor", SourcePartColor);
            WriteValue(SettingsNode, "TargetPartColor", TargetPartColor);
            WriteValue(SettingsNode, "TargetPartCrewColor", TargetPartCrewColor);
            WriteValue(SettingsNode, "MouseOverColor", MouseOverColor);


            // ToolTip Settings
            WriteValue(SettingsNode, "ShowToolTips", ShowToolTips);
            WriteValue(SettingsNode, "ManifestToolTips", ManifestToolTips);
            WriteValue(SettingsNode, "TransferToolTips", TransferToolTips);
            WriteValue(SettingsNode, "SettingsToolTips", SettingsToolTips);
            WriteValue(SettingsNode, "RosterToolTips", RosterToolTips);
            WriteValue(SettingsNode, "HatchToolTips", HatchToolTips);
            WriteValue(SettingsNode, "PanelToolTips", PanelToolTips);
            WriteValue(SettingsNode, "AntennaToolTips", AntennaToolTips);
            WriteValue(SettingsNode, "LightToolTips", LightToolTips);
            WriteValue(SettingsNode, "DebuggerToolTips", DebuggerToolTips);

            // Sound Settings
            WriteValue(SettingsNode, "PumpSoundStart", PumpSoundStart);
            WriteValue(SettingsNode, "PumpSoundRun", PumpSoundRun);
            WriteValue(SettingsNode, "PumpSoundStop", PumpSoundStop);
            WriteValue(SettingsNode, "CrewSoundStart", CrewSoundStart);
            WriteValue(SettingsNode, "CrewSoundRun", CrewSoundRun);
            WriteValue(SettingsNode, "CrewSoundStop", CrewSoundStop);
            WriteValue(SettingsNode, "PumpSoundVol", PumpSoundVol);
            WriteValue(SettingsNode, "CrewSoundVol", CrewSoundVol);

            // Config Settings
            WriteValue(SettingsNode, "ShowDebugger", ShowDebugger);
            WriteValue(SettingsNode, "EnableBlizzyToolbar", EnableBlizzyToolbar);
            WriteValue(SettingsNode, "VerboseLogging", VerboseLogging);
            WriteValue(SettingsNode, "AutoSave", AutoSave);
            WriteValue(SettingsNode, "SaveIntervalSec", SaveIntervalSec);
            WriteValue(SettingsNode, "AutoDebug", AutoDebug);
            WriteValue(SettingsNode, "DebugLogPath", DebugLogPath);
            WriteValue(SettingsNode, "ErrorLogLength", ErrorLogLength);
            WriteValue(SettingsNode, "SaveLogOnExit", SaveLogOnExit);
            WriteValue(SettingsNode, "EnableKerbalRename", EnableKerbalRename);
            WriteValue(SettingsNode, "RenameWithProfession", RenameWithProfession);

            // Other Settings
            WriteValue(SettingsNode, "IVATimeDelaySec", IVATimeDelaySec);
            WriteValue(SettingsNode, "ShowIVAUpdateBtn", ShowIVAUpdateBtn);
            WriteValue(SettingsNode, "EnableTextureReplacer", EnableTextureReplacer);

            if (!Directory.Exists(SETTINGS_PATH))
                Directory.CreateDirectory(SETTINGS_PATH);
            settings.Save(SETTINGS_FILE);
        }

        private static Rect getRectangle(ConfigNode WindowsNode, string RectName, Rect defaultvalue)
        {
            Rect thisRect = new Rect();
            ConfigNode RectNode = WindowsNode.HasNode(RectName) ? WindowsNode.GetNode(RectName) : WindowsNode.AddNode(RectName);
            thisRect.x = RectNode.HasValue("x") ? int.Parse(RectNode.GetValue("x")) : defaultvalue.x;
            thisRect.y = RectNode.HasValue("y") ? int.Parse(RectNode.GetValue("y")) : defaultvalue.y;
            thisRect.width = RectNode.HasValue("width") ? int.Parse(RectNode.GetValue("width")) : defaultvalue.width;
            thisRect.height = RectNode.HasValue("height") ? int.Parse(RectNode.GetValue("height")) : defaultvalue.height;

            return thisRect;
        }

        private static void WriteRectangle(ConfigNode WindowsNode, string RectName, Rect rectValue)
        {
            ConfigNode RectNode = WindowsNode.HasNode(RectName) ? WindowsNode.GetNode(RectName) : WindowsNode.AddNode(RectName);
            WriteValue(RectNode, "x", rectValue.x);
            WriteValue(RectNode, "y", rectValue.y);
            WriteValue(RectNode, "width", rectValue.width);
            WriteValue(RectNode, "height", rectValue.height);
        }

        private static void WriteValue(ConfigNode configNode, string ValueName, object value)
        {
            if (configNode.HasValue(ValueName))
                configNode.RemoveValue(ValueName);
            configNode.AddValue(ValueName, value.ToString());
        }

        private static void RepositionWindows()
        {
            if (ManifestPosition.xMax > Screen.currentResolution.width)
                ManifestPosition.x = Screen.currentResolution.width - ManifestPosition.width;
            if (ManifestPosition.yMax > Screen.currentResolution.height)
                ManifestPosition.y = Screen.currentResolution.height - ManifestPosition.height;

            if (TransferPosition.xMax > Screen.currentResolution.width)
                TransferPosition.x = Screen.currentResolution.width - TransferPosition.width;
            if (TransferPosition.yMax > Screen.currentResolution.height)
                TransferPosition.y = Screen.currentResolution.height - TransferPosition.height;

            if (DebuggerPosition.xMax > Screen.currentResolution.width)
                DebuggerPosition.x = Screen.currentResolution.width - DebuggerPosition.width;
            if (DebuggerPosition.yMax > Screen.currentResolution.height)
                DebuggerPosition.y = Screen.currentResolution.height - DebuggerPosition.height;

            if (SettingsPosition.xMax > Screen.currentResolution.width)
                SettingsPosition.x = Screen.currentResolution.width - SettingsPosition.width;
            if (SettingsPosition.yMax > Screen.currentResolution.height)
                SettingsPosition.y = Screen.currentResolution.height - SettingsPosition.height;

            if (ControlPosition.xMax > Screen.currentResolution.width)
                ControlPosition.x = Screen.currentResolution.width - ControlPosition.width;
            if (ControlPosition.yMax > Screen.currentResolution.height)
                ControlPosition.y = Screen.currentResolution.height - ControlPosition.height;

            if (RosterPosition.xMax > Screen.currentResolution.width)
                RosterPosition.x = Screen.currentResolution.width - RosterPosition.width;
            if (RosterPosition.yMax > Screen.currentResolution.height)
                RosterPosition.y = Screen.currentResolution.height - RosterPosition.height;
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
            Colors.Add("default", new Color(0.478f, 0.698f, 0.478f, 0.698f));
        }

        internal static void StoreTempSettings()
        {
            prevRealismMode = RealismMode;
            prevShowDebugger = ShowDebugger;
            prevVerboseLogging = VerboseLogging;
            prevAutoSave = AutoSave;
            prevSaveIntervalSec = SaveIntervalSec;
            prevFlowRate = FlowRate;
            prevFlowCost = FlowCost;
            prevMinFlowRate = MinFlowRate;
            prevMaxFlowRate = MaxFlowRate;
            prevMaxFlowTimeSec = MaxFlowTimeSec;
            prevEnableXferCost = EnableXferCost;
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
            prevOverrideStockCrewXfer = OverrideStockCrewXfer;
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
            prevAntennaToolTips = AntennaToolTips;
            prevLightToolTips = LightToolTips;
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
            FlowCost = prevFlowCost;
            MinFlowRate = prevMinFlowRate;
            MaxFlowRate = prevMaxFlowRate;
            MaxFlowTimeSec = prevMaxFlowTimeSec;
            EnableXferCost = prevEnableXferCost;
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
            OverrideStockCrewXfer = prevOverrideStockCrewXfer;
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
            AntennaToolTips = prevAntennaToolTips;
            LightToolTips = prevLightToolTips;
            DebuggerToolTips = prevDebuggerToolTips;

            //debugger Settings
            prevErrorLogLength = ErrorLogLength;
        }

        #endregion
    }
}
