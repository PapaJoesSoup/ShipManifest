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
    internal  static class SMSettings
    {
        #region Properties

        internal static bool Loaded = false;
        
        internal static Dictionary<string, Color> Colors;

        internal static ConfigNode settings = null;
        private static readonly string SETTINGS_PATH = KSPUtil.ApplicationRootPath + "GameData/ShipManifest/Plugins/PluginData";
        private static readonly string SETTINGS_FILE = SETTINGS_PATH + "/SMSettings.dat";

        // This value is assigned from AssemblyInfo.cs
        internal static string CurVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        // Persisted properties
        // UI Managed Settings
        // Realism Tab Feature Options
        internal static bool RealismMode = true;
        internal static bool EnableCrew = true;
        internal static bool OverrideStockCrewXfer = true;
        internal static bool EnableCLS = true;
        internal static bool EnableScience = true;
        internal static bool EnableResources = true;
        internal static bool EnablePFResources = true;
        internal static bool EnableXferCost = true;
        internal static double FlowCost = 0.0015;
        internal static double FlowRate = 100;
        internal static double MinFlowRate = 0;
        internal static double MaxFlowRate = 1000;
        internal static int MaxFlowTimeSec = 180;
        internal static bool LockSettings = false;

        //Highlighting Tab Options
        internal static bool EnableHighlighting = true;
        internal static bool OnlySourceTarget = false;
        internal static bool EnableCLSHighlighting = true;
        internal static bool EnableEdgeHighlighting = true;

        // Tooltip Options
        internal static bool ShowToolTips = true;
        // These options are managed, but assign their values directly to the window property.
        // Shown here for clarity
        // prevManifestToolTips = WindowManifest.ShowToolTips;
        // prevTransferToolTips = WindowTransfer.ShowToolTips;
        // prevSettingsToolTips = WindowSettings.ShowToolTips;
        // prevRosterToolTips = WindowRoster.ShowToolTips;
        // prevHatchToolTips = TabHatch.ShowToolTips;
        // prevPanelToolTips = TabSolarPanel.ShowToolTips;
        // prevAntennaToolTips = TabAntenna.ShowToolTips;
        // prevLightToolTips = TabLight.ShowToolTips;
        // prevDebuggerToolTips = WindowDebugger.ShowToolTips;

        // Sound Tab Options
        // All Default sounds licensing is: CC-By-SA

        // Pump motor sound
        // http://www.freesound.org/people/vibe_crc/sounds/59328/

        // Bumping and scraping sounds...
        // http://www.freesound.org/people/adcbicycle/sounds/14214/

        // Minion like kerbal sounds...
        // http://www.freesound.org/people/yummie/sounds/

        internal static string PumpSoundStart = "ShipManifest/Sounds/59328-1";
        internal static string PumpSoundRun = "ShipManifest/Sounds/59328-2";
        internal static string PumpSoundStop = "ShipManifest/Sounds/59328-3";
        internal static string CrewSoundStart = "ShipManifest/Sounds/14214-1";
        internal static string CrewSoundRun = "ShipManifest/Sounds/14214-2";
        internal static string CrewSoundStop = "ShipManifest/Sounds/14214-3";

        //Config Tab options
        internal static bool EnableBlizzyToolbar = false;
        internal static bool VerboseLogging = false;
        internal static bool AutoDebug = false;
        internal static bool SaveLogOnExit = false;
        internal static string ErrorLogLength = "1000";
        internal static bool EnableKerbalRename = false;
        internal static bool RenameWithProfession = false;
        internal static bool AutoSave = false;
        internal static int SaveIntervalSec = 60;



        // Roster Options

        // Unmanaged options.
        internal static string ResourcePartColor = "yellow";
        internal static string SourcePartColor = "red";
        internal static string TargetPartColor = "green";
        internal static string TargetPartCrewColor = "blue";
        internal static string CLS_SpaceColor = "green";
        internal static string MouseOverColor = "green";
        internal static double IVATimeDelaySec = 7;
        internal static bool ShowIVAUpdateBtn = false;
        internal static double PumpSoundVol = 3;
        internal static double CrewSoundVol = 3;


        
        // End Persisted Properties

        // Settings Window Option storage for Settings Window Cancel support
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
        internal static bool prevOverrideStockCrewTransfer = true;
        internal static bool prevEnablePFResources = true;
        internal static bool prevEnableCLS = true;
        internal static bool prevEnableBlizzyToolbar = false;

        internal static string prevPumpSoundStart = "ShipManifest/Sounds/59328-1";
        internal static string prevPumpSoundRun = "ShipManifest/Sounds/59328-2";
        internal static string prevPumpSoundStop = "ShipManifest/Sounds/59328-3";
        internal static string prevCrewSoundStart = "ShipManifest/Sounds/14214-1";
        internal static string prevCrewSoundRun = "ShipManifest/Sounds/14214-2";
        internal static string prevCrewSoundStop = "ShipManifest/Sounds/14214-3";

        // these values have no non prev counterpart.  Each window contains an EnableToolTip property 
        // that is directly assigned on load and directly retrieved on save
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

        // Internal properties for plugin management.  Not persisted, not user managed.

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
            ConfigNode HiddenNode = settings.HasNode("SM_Hidden") ? settings.GetNode("SM_Hidden") : settings.AddNode("SM_Hidden");

            // Lets get our rectangles...
            WindowManifest.Position = getRectangle(WindowsNode, "ManifestPosition", WindowManifest.Position);
            WindowTransfer.Position = getRectangle(WindowsNode, "TransferPosition", WindowTransfer.Position);
            WindowDebugger.Position = getRectangle(WindowsNode, "DebuggerPosition", WindowDebugger.Position);
            WindowSettings.Position = getRectangle(WindowsNode, "SettingsPosition", WindowSettings.Position);
            WindowControl.Position = getRectangle(WindowsNode, "ControlPosition", WindowControl.Position);
            WindowRoster.Position = getRectangle(WindowsNode, "RosterPosition", WindowRoster.Position);

            // now the settings

            // Realism Settings
            RealismMode = SettingsNode.HasValue("RealismMode") ? bool.Parse(SettingsNode.GetValue("RealismMode")) : RealismMode;
            EnableCrew = SettingsNode.HasValue("EnableCrew") ? bool.Parse(SettingsNode.GetValue("EnableCrew")) : EnableCrew;
            EnableScience = SettingsNode.HasValue("EnableScience") ? bool.Parse(SettingsNode.GetValue("EnableScience")) : EnableScience;
            EnableResources = SettingsNode.HasValue("EnableResources") ? bool.Parse(SettingsNode.GetValue("EnableResources")) : EnableResources;
            EnablePFResources = SettingsNode.HasValue("EnablePFResources") ? bool.Parse(SettingsNode.GetValue("EnablePFResources")) : EnablePFResources;
            EnableCLS = SettingsNode.HasValue("EnableCLS") ? bool.Parse(SettingsNode.GetValue("EnableCLS")) : EnableCLS;
            OverrideStockCrewXfer = SettingsNode.HasValue("OverrideStockCrewTransfer") ? bool.Parse(SettingsNode.GetValue("OverrideStockCrewTransfer")) : OverrideStockCrewXfer;
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
            ResourcePartColor = HiddenNode.HasValue("ResourcePartColor") ? HiddenNode.GetValue("ResourcePartColor") : ResourcePartColor;

            // ToolTip Settings
            ShowToolTips = SettingsNode.HasValue("ShowToolTips") ? bool.Parse(SettingsNode.GetValue("ShowToolTips")) : ShowToolTips;
            WindowManifest.ShowToolTips = SettingsNode.HasValue("ManifestToolTips") ? bool.Parse(SettingsNode.GetValue("ManifestToolTips")) : WindowManifest.ShowToolTips;
            WindowTransfer.ShowToolTips = SettingsNode.HasValue("TransferToolTips") ? bool.Parse(SettingsNode.GetValue("TransferToolTips")) : WindowTransfer.ShowToolTips;
            WindowSettings.ShowToolTips = SettingsNode.HasValue("SettingsToolTips") ? bool.Parse(SettingsNode.GetValue("SettingsToolTips")) : WindowSettings.ShowToolTips;
            WindowRoster.ShowToolTips = SettingsNode.HasValue("RosterToolTips") ? bool.Parse(SettingsNode.GetValue("RosterToolTips")) : WindowRoster.ShowToolTips;
            TabHatch.ShowToolTips = SettingsNode.HasValue("HatchToolTips") ? bool.Parse(SettingsNode.GetValue("HatchToolTips")) : TabHatch.ShowToolTips;
            TabSolarPanel.ShowToolTips = SettingsNode.HasValue("PanelToolTips") ? bool.Parse(SettingsNode.GetValue("PanelToolTips")) : TabSolarPanel.ShowToolTips;
            TabAntenna.ShowToolTips = SettingsNode.HasValue("AntennaToolTips") ? bool.Parse(SettingsNode.GetValue("AntennaToolTips")) : TabAntenna.ShowToolTips;
            TabLight.ShowToolTips = SettingsNode.HasValue("LightToolTips") ? bool.Parse(SettingsNode.GetValue("LightToolTips")) : TabLight.ShowToolTips;
            WindowDebugger.ShowToolTips = SettingsNode.HasValue("DebuggerToolTips") ? bool.Parse(SettingsNode.GetValue("DebuggerToolTips")) : WindowDebugger.ShowToolTips;

            // Sounds Settings
            PumpSoundStart = SettingsNode.HasValue("PumpSoundStart") ? SettingsNode.GetValue("PumpSoundStart") : PumpSoundStart;
            PumpSoundRun = SettingsNode.HasValue("PumpSoundRun") ? SettingsNode.GetValue("PumpSoundRun") : PumpSoundRun;
            PumpSoundStop = SettingsNode.HasValue("PumpSoundStop") ? SettingsNode.GetValue("PumpSoundStop") : PumpSoundStop;
            CrewSoundStart = SettingsNode.HasValue("CrewSoundStart") ? SettingsNode.GetValue("CrewSoundStart") : CrewSoundStart;
            CrewSoundRun = SettingsNode.HasValue("CrewSoundRun") ? SettingsNode.GetValue("CrewSoundRun") : CrewSoundRun;
            CrewSoundStop = SettingsNode.HasValue("CrewSoundStop") ? SettingsNode.GetValue("CrewSoundStop") : CrewSoundStop;

            // Config Settings
            EnableBlizzyToolbar = SettingsNode.HasValue("EnableBlizzyToolbar") ? bool.Parse(SettingsNode.GetValue("EnableBlizzyToolbar")) : EnableBlizzyToolbar;
            WindowDebugger.ShowWindow = SettingsNode.HasValue("ShowDebugger") ? bool.Parse(SettingsNode.GetValue("ShowDebugger")) : WindowDebugger.ShowWindow;
            VerboseLogging = SettingsNode.HasValue("VerboseLogging") ? bool.Parse(SettingsNode.GetValue("VerboseLogging")) : VerboseLogging;
            AutoSave = SettingsNode.HasValue("AutoSave") ? bool.Parse(SettingsNode.GetValue("AutoSave")) : AutoSave;
            SaveIntervalSec = SettingsNode.HasValue("ShowDebugger") ? int.Parse(SettingsNode.GetValue("SaveIntervalSec")) : SaveIntervalSec;
            AutoDebug = SettingsNode.HasValue("AutoDebug") ? bool.Parse(SettingsNode.GetValue("AutoDebug")) : AutoDebug;
            DebugLogPath = SettingsNode.HasValue("DebugLogPath") ? SettingsNode.GetValue("DebugLogPath") : DebugLogPath;
            ErrorLogLength = SettingsNode.HasValue("ErrorLogLength") ? SettingsNode.GetValue("ErrorLogLength") : ErrorLogLength;
            SaveLogOnExit = SettingsNode.HasValue("SaveLogOnExit") ? bool.Parse(SettingsNode.GetValue("SaveLogOnExit")) : SaveLogOnExit;
            EnableKerbalRename = SettingsNode.HasValue("EnableKerbalRename") ? bool.Parse(SettingsNode.GetValue("EnableKerbalRename")) : EnableKerbalRename;
            RenameWithProfession = SettingsNode.HasValue("RenameWithProfession") ? bool.Parse(SettingsNode.GetValue("RenameWithProfession")) : RenameWithProfession;


            // Hidden Settings
            // Hidden Highlighting
            SourcePartColor = HiddenNode.HasValue("SourcePartColor") ? HiddenNode.GetValue("SourcePartColor") : SourcePartColor;
            TargetPartColor = HiddenNode.HasValue("TargetPartColor") ? HiddenNode.GetValue("TargetPartColor") : TargetPartColor;
            TargetPartCrewColor = HiddenNode.HasValue("TargetPartCrewColor") ? HiddenNode.GetValue("TargetPartCrewColor") : TargetPartCrewColor;
            MouseOverColor = HiddenNode.HasValue("MouseOverColor") ? HiddenNode.GetValue("MouseOverColor") : MouseOverColor;
            //Hidden sound
            PumpSoundVol = HiddenNode.HasValue("PumpSoundVol") ? double.Parse(HiddenNode.GetValue("PumpSoundVol")) : PumpSoundVol;
            CrewSoundVol = HiddenNode.HasValue("CrewSoundVol") ? double.Parse(HiddenNode.GetValue("CrewSoundVol")) : CrewSoundVol;
            // Hidden config
            IVATimeDelaySec = HiddenNode.HasValue("IVATimeDelaySec") ? double.Parse(HiddenNode.GetValue("IVATimeDelaySec")) : IVATimeDelaySec;
            ShowIVAUpdateBtn = HiddenNode.HasValue("ShowIVAUpdateBtn") ? bool.Parse(HiddenNode.GetValue("ShowIVAUpdateBtn")) : ShowIVAUpdateBtn;

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
            ConfigNode HiddenNode = settings.HasNode("SM_Hidden") ? settings.GetNode("SM_Hidden") : settings.AddNode("SM_Hidden");

            // Write window positions
            WriteRectangle(WindowsNode, "ManifestPosition", WindowManifest.Position);
            WriteRectangle(WindowsNode, "TransferPosition", WindowTransfer.Position);
            WriteRectangle(WindowsNode, "DebuggerPosition", WindowDebugger.Position);
            WriteRectangle(WindowsNode, "SettingsPosition", WindowSettings.Position);
            WriteRectangle(WindowsNode, "ControlPosition", WindowControl.Position);
            WriteRectangle(WindowsNode, "RosterPosition", WindowRoster.Position);

            //Write settings...
            // Realism Settings
            WriteValue(SettingsNode, "RealismMode", RealismMode);
            WriteValue(SettingsNode, "EnableCrew", EnableCrew);
            WriteValue(SettingsNode, "EnableScience", EnableScience);
            WriteValue(SettingsNode, "EnableResources", EnableResources);
            WriteValue(SettingsNode, "EnablePFResources", EnablePFResources);
            WriteValue(SettingsNode, "EnableCLS", EnableCLS);
            WriteValue(SettingsNode, "OverrideStockCrewTransfer", OverrideStockCrewXfer);
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

            // ToolTip Settings
            WriteValue(SettingsNode, "ShowToolTips", ShowToolTips);
            WriteValue(SettingsNode, "ManifestToolTips", WindowManifest.ShowToolTips);
            WriteValue(SettingsNode, "TransferToolTips", WindowTransfer.ShowToolTips);
            WriteValue(SettingsNode, "SettingsToolTips", WindowSettings.ShowToolTips);
            WriteValue(SettingsNode, "RosterToolTips", WindowRoster.ShowToolTips);
            WriteValue(SettingsNode, "HatchToolTips", TabHatch.ShowToolTips);
            WriteValue(SettingsNode, "PanelToolTips", TabSolarPanel.ShowToolTips);
            WriteValue(SettingsNode, "AntennaToolTips", TabAntenna.ShowToolTips);
            WriteValue(SettingsNode, "LightToolTips", TabLight.ShowToolTips);
            WriteValue(SettingsNode, "DebuggerToolTips", WindowDebugger.ShowToolTips);

            // Sound Settings
            WriteValue(SettingsNode, "PumpSoundStart", PumpSoundStart);
            WriteValue(SettingsNode, "PumpSoundRun", PumpSoundRun);
            WriteValue(SettingsNode, "PumpSoundStop", PumpSoundStop);
            WriteValue(SettingsNode, "CrewSoundStart", CrewSoundStart);
            WriteValue(SettingsNode, "CrewSoundRun", CrewSoundRun);
            WriteValue(SettingsNode, "CrewSoundStop", CrewSoundStop);

            // Config Settings
            WriteValue(SettingsNode, "ShowDebugger", WindowDebugger.ShowWindow);
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

            // Hidden Settings
            WriteValue(HiddenNode, "ResourcePartColor", ResourcePartColor);
            WriteValue(HiddenNode, "SourcePartColor", SourcePartColor);
            WriteValue(HiddenNode, "TargetPartColor", TargetPartColor);
            WriteValue(HiddenNode, "TargetPartCrewColor", TargetPartCrewColor);
            WriteValue(HiddenNode, "MouseOverColor", MouseOverColor);
            WriteValue(HiddenNode, "PumpSoundVol", PumpSoundVol);
            WriteValue(HiddenNode, "CrewSoundVol", CrewSoundVol);
            WriteValue(HiddenNode, "IVATimeDelaySec", IVATimeDelaySec);
            WriteValue(HiddenNode, "ShowIVAUpdateBtn", ShowIVAUpdateBtn);

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
            if (WindowManifest.Position.xMax > Screen.currentResolution.width)
                WindowManifest.Position.x = Screen.currentResolution.width - WindowManifest.Position.width;
            if (WindowManifest.Position.yMax > Screen.currentResolution.height)
                WindowManifest.Position.y = Screen.currentResolution.height - WindowManifest.Position.height;

            if (WindowTransfer.Position.xMax > Screen.currentResolution.width)
                WindowTransfer.Position.x = Screen.currentResolution.width - WindowTransfer.Position.width;
            if (WindowTransfer.Position.yMax > Screen.currentResolution.height)
                WindowTransfer.Position.y = Screen.currentResolution.height - WindowTransfer.Position.height;

            if (WindowDebugger.Position.xMax > Screen.currentResolution.width)
                WindowDebugger.Position.x = Screen.currentResolution.width - WindowDebugger.Position.width;
            if (WindowDebugger.Position.yMax > Screen.currentResolution.height)
                WindowDebugger.Position.y = Screen.currentResolution.height - WindowDebugger.Position.height;

            if (WindowSettings.Position.xMax > Screen.currentResolution.width)
                WindowSettings.Position.x = Screen.currentResolution.width - WindowSettings.Position.width;
            if (WindowSettings.Position.yMax > Screen.currentResolution.height)
                WindowSettings.Position.y = Screen.currentResolution.height - WindowSettings.Position.height;

            if (WindowControl.Position.xMax > Screen.currentResolution.width)
                WindowControl.Position.x = Screen.currentResolution.width - WindowControl.Position.width;
            if (WindowControl.Position.yMax > Screen.currentResolution.height)
                WindowControl.Position.y = Screen.currentResolution.height - WindowControl.Position.height;

            if (WindowRoster.Position.xMax > Screen.currentResolution.width)
                WindowRoster.Position.x = Screen.currentResolution.width - WindowRoster.Position.width;
            if (WindowRoster.Position.yMax > Screen.currentResolution.height)
                WindowRoster.Position.y = Screen.currentResolution.height - WindowRoster.Position.height;
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
            prevShowDebugger = WindowDebugger.ShowWindow;
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
            prevOverrideStockCrewTransfer = OverrideStockCrewXfer;
            prevEnableKerbalRename = EnableKerbalRename;
            prevRenameWithProfession = RenameWithProfession;
            prevLockSettings = LockSettings;
            prevEnableBlizzyToolbar = EnableBlizzyToolbar;
            prevSaveLogOnExit = SaveLogOnExit;
            prevShowToolTips = ShowToolTips;
            prevManifestToolTips = WindowManifest.ShowToolTips;
            prevTransferToolTips = WindowTransfer.ShowToolTips;
            prevSettingsToolTips = WindowSettings.ShowToolTips;
            prevRosterToolTips = WindowRoster.ShowToolTips;
            prevHatchToolTips = TabHatch.ShowToolTips;
            prevPanelToolTips = TabSolarPanel.ShowToolTips;
            prevAntennaToolTips = TabAntenna.ShowToolTips;
            prevLightToolTips = TabLight.ShowToolTips;
            prevDebuggerToolTips = WindowDebugger.ShowToolTips;

            // sounds

            //debugger Settings
            prevErrorLogLength = ErrorLogLength;
        }

        internal static void RestoreTempSettings()
        {
            RealismMode = prevRealismMode;
            WindowDebugger.ShowWindow = prevShowDebugger;
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
            OverrideStockCrewXfer = prevOverrideStockCrewTransfer;
            EnableKerbalRename = prevEnableKerbalRename;
            RenameWithProfession = prevRenameWithProfession;
            LockSettings = prevLockSettings;
            EnableBlizzyToolbar = prevEnableBlizzyToolbar;
            SaveLogOnExit = prevSaveLogOnExit;
            ShowToolTips = prevShowToolTips;
            WindowManifest.ShowToolTips = prevManifestToolTips;
            WindowTransfer.ShowToolTips = prevTransferToolTips;
            WindowSettings.ShowToolTips = prevSettingsToolTips;
            WindowRoster.ShowToolTips = prevRosterToolTips;
            TabHatch.ShowToolTips = prevHatchToolTips;
            TabSolarPanel.ShowToolTips = prevPanelToolTips;
            TabAntenna.ShowToolTips = prevAntennaToolTips;
            TabLight.ShowToolTips = prevLightToolTips;
            WindowDebugger.ShowToolTips = prevDebuggerToolTips;

            //debugger Settings
            prevErrorLogLength = ErrorLogLength;
        }

        #endregion
    }
}
