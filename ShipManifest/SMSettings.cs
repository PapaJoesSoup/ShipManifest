using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ShipManifest.InternalObjects;
using ShipManifest.Windows;
using ShipManifest.Windows.Tabs.Control;
using ShipManifest.Windows.Tabs.Settings;
using UnityEngine;

namespace ShipManifest
{
  // ReSharper disable once InconsistentNaming
  internal static class SMSettings
  {
    #region Properties

    internal static bool Loaded;

    internal static Dictionary<string, Color> Colors;

    internal static ConfigNode Settings;

    private static readonly string SettingsPath =
      $"{KSPUtil.ApplicationRootPath}GameData/ShipManifest/Plugins/PluginData";

    private static readonly string SettingsFile = $"{SettingsPath}/SMSettings.dat";

    // This value is assigned from AssemblyInfo.cs
    internal static string CurVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

    // Persisted properties
    // UI Managed Settings
    // Realism Tab Feature Options
    internal static int RealismMode = 2;
    internal static bool RealXfers = true;
    internal static bool RealControl = true;
    internal static bool EnableCrew = true;
    internal static bool EnableCrewModify = true;
    internal static bool EnableKerbalRename = true;
    internal static bool EnableChangeProfession = true;
    internal static bool EnableStockCrewXfer = true;
    internal static bool OverrideStockCrewXfer = true;
    internal static bool EnableClsAllowTransfer = true;
    internal static bool EnableCls = true;
    internal static bool EnableScience = true;
    internal static bool EnableResources = true;
    internal static bool EnablePfCrews;
    internal static bool EnablePfResources = true;
    internal static bool EnableXferCost = true;
    internal static double FlowCost = 0.0015;
    internal static double FlowRate = 100;
    internal static double MinFlowRate;
    internal static double MaxFlowRate = 1000;
    internal static double Tolerance = 0.000001;
    internal static int MaxFlowTimeSec = 180;
    internal static bool LockSettings;

    //Highlighting Tab Options
    internal static bool EnableHighlighting = true;
    internal static bool OnlySourceTarget;
    internal static bool EnableClsHighlighting = true;
    internal static bool EnableEdgeHighlighting = true;

    // Tooltip Options
    internal static bool ShowToolTips = true;
    // These options are managed, but assign their values directly to the window property.
    // Shown here for clarity
    // prevManifestToolTips = WindowManifest.ShowToolTips;
    // prevTransferToolTips = WindowTransfer.ShowToolTips;
    // prevSettingsToolTips = WindowSettings.ShowToolTips;
    // prevRosterToolTips = WindowRoster.ShowToolTips;
    // prevControlToolTips = WindowControl.ShowToolTips;
    // prevHatchToolTips = TabHatch.ShowToolTips;
    // prevPanelToolTips = TabSolarPanel.ShowToolTips;
    // prevAntennaToolTips = TabAntenna.ShowToolTips;
    // prevLightToolTips = TabLight.ShowToolTips;
    // prevRealismToolTips = TabRealism.ShowToolTips;
    // prevToolTipsToolTips = TabToolTips.ShowToolTips;
    // prevSoundsToolTips = TabSounds.ShowToolTips;
    // prevHighlightToolTips = TabHighlight.ShowToolTips;
    // prevConfigToolTips = TabConfig.ShowToolTips;
    // prevModToolTips = TabInstalledMods.ShowToolTips;
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
    internal static double PumpSoundVol = 1; // Range = 0...1

    internal static string CrewSoundStart = "ShipManifest/Sounds/14214-1";
    internal static string CrewSoundRun = "ShipManifest/Sounds/14214-2";
    internal static string CrewSoundStop = "ShipManifest/Sounds/14214-3";
    internal static double CrewSoundVol = 1; // Range = 0...1


    //Config Tab options
    internal static bool EnableBlizzyToolbar;
    internal static bool VerboseLogging;
    internal static bool AutoDebug;
    internal static bool SaveLogOnExit;
    internal static string ErrorLogLength = "1000";
    internal static bool AutoSave;
    internal static int SaveIntervalSec = 60;
    internal static bool UseUnityStyle = true;


    // Options unmanaged by UI.
    internal static string ResourcePartColor = "yellow";
    internal static string SourcePartColor = "red";
    internal static string TargetPartColor = "green";
    internal static string TargetPartCrewColor = "blue";
    internal static string ClsSpaceColor = "green";
    internal static string MouseOverColor = "burntorange";
    internal static double CrewXferDelaySec = 7;
    internal static int IvaUpdateFrameDelay = 10;


    // End Persisted Properties

    // Settings Window Option storage for Settings Window Cancel support
    internal static bool PrevVerboseLogging;
    internal static bool PrevShowDebugger;
    internal static string PrevErrorLogLength = "1000";
    internal static bool PrevSaveLogOnExit = true;
    internal static bool PrevAutoSave;
    internal static bool PrevUseUnityStyle = true;
    internal static int PrevSaveIntervalSec = 60;

    internal static int PrevRealismMode = 3;
    internal static bool PrevRealXfers;
    internal static bool PrevRealControl;
    internal static bool PrevLockSettings;

    internal static double PrevFlowRate = 100;
    internal static double PrevFlowCost = 0.0015;
    internal static double PrevMaxFlowRate = 1000;
    internal static double PrevMinFlowRate;
    internal static int PrevMaxFlowTimeSec = 100;
    internal static bool PrevEnableXferCost = true;

    internal static bool PrevEnableHighlighting = true;
    internal static bool PrevOnlySourceTarget;
    internal static bool PrevEnableClsHighlighting = true;
    internal static bool PrevEnableEdgeHighlighting = true;
    internal static bool PrevEnableScience = true;
    internal static bool PrevEnableCrew = true;
    internal static bool PrevEnableCrewModify = true;
    internal static bool PrevEnablePfCrews;
    internal static bool PrevEnableStockCrewXfer = true;
    internal static bool PrevOverrideStockCrewXfer = true;
    internal static bool PrevEnableClsAllowTransfer = true;
    internal static bool PrevEnablePfResources = true;
    internal static bool PrevEnableCls = true;
    internal static bool PrevEnableBlizzyToolbar;

    internal static string PrevPumpSoundStart = "ShipManifest/Sounds/59328-1";
    internal static string PrevPumpSoundRun = "ShipManifest/Sounds/59328-2";
    internal static string PrevPumpSoundStop = "ShipManifest/Sounds/59328-3";
    internal static string PrevCrewSoundStart = "ShipManifest/Sounds/14214-1";
    internal static string PrevCrewSoundRun = "ShipManifest/Sounds/14214-2";
    internal static string PrevCrewSoundStop = "ShipManifest/Sounds/14214-3";
    internal static double PrevPumpSoundVol = 1;
    internal static double PrevCrewSoundVol = 1;


    // these values have no non prev counterpart.  Each window contains an EnableToolTip property 
    // that is directly assigned on load and directly retrieved on save
    internal static bool PrevShowToolTips = true;
    internal static bool PrevManifestToolTips = true;
    internal static bool PrevTransferToolTips = true;
    internal static bool PrevSettingsToolTips = true;
    internal static bool PrevRosterToolTips = true;
    internal static bool PrevControlToolTips = true;
    internal static bool PrevHatchToolTips = true;
    internal static bool PrevPanelToolTips = true;
    internal static bool PrevAntennaToolTips = true;
    internal static bool PrevLightToolTips = true;
    internal static bool PrevRealismToolTips = true;
    internal static bool PrevHighlightToolTips = true;
    internal static bool PrevSoundsToolTips = true;
    internal static bool PrevToolTipsToolTips = true;
    internal static bool PrevConfigToolTips = true;
    internal static bool PrevModsToolTips = true;
    internal static bool PrevDebuggerToolTips = true;

    internal static bool PrevEnableKerbalRename = true;
    internal static bool PrevEnableChangeProfession = true;

    // Internal properties for plugin management.  Not persisted, not user managed.

    internal static string DebugLogPath = @"Plugins\PluginData\";
    internal static bool ClsInstalled = false;

    #endregion

    #region Methods

    internal static ConfigNode LoadSettingsFile()
    {
      return Settings ?? (Settings = ConfigNode.Load(SettingsFile) ?? new ConfigNode());
    }

    internal static void LoadSettings()
    {
      LoadColors();

      if (Settings == null) LoadSettingsFile();
      if (Settings != null)
      {
        ConfigNode windowsNode = Settings.HasNode("SM_Windows")
          ? Settings.GetNode("SM_Windows")
          : Settings.AddNode("SM_Windows");
        ConfigNode realismNode = Settings.HasNode("SM_Realism")
          ? Settings.GetNode("SM_Realism")
          : Settings.AddNode("SM_Realism");
        ConfigNode highlightNode = Settings.HasNode("SM_Highlight")
          ? Settings.GetNode("SM_Highlight")
          : Settings.AddNode("SM_Highlight");
        ConfigNode toolTipsNode = Settings.HasNode("SM_ToolTips")
          ? Settings.GetNode("SM_ToolTips")
          : Settings.AddNode("SM_ToolTips");
        ConfigNode soundsNode = Settings.HasNode("SM_Sounds")
          ? Settings.GetNode("SM_Sounds")
          : Settings.AddNode("SM_Sounds");
        ConfigNode configNode = Settings.HasNode("SM_Config")
          ? Settings.GetNode("SM_Config")
          : Settings.AddNode("SM_Config");
        ConfigNode hiddenNode = Settings.HasNode("SM_Hidden")
          ? Settings.GetNode("SM_Hidden")
          : Settings.AddNode("SM_Hidden");

        // Lets get our rectangles...
        WindowManifest.Position = GetRectangle(windowsNode, "ManifestPosition", WindowManifest.Position);
        WindowTransfer.Position = GetRectangle(windowsNode, "TransferPosition", WindowTransfer.Position);
        WindowDebugger.Position = GetRectangle(windowsNode, "DebuggerPosition", WindowDebugger.Position);
        WindowSettings.Position = GetRectangle(windowsNode, "SettingsPosition", WindowSettings.Position);
        WindowControl.Position = GetRectangle(windowsNode, "ControlPosition", WindowControl.Position);
        WindowRoster.Position = GetRectangle(windowsNode, "RosterPosition", WindowRoster.Position);

        // now the settings
        // Realism Settings
        RealismMode = realismNode.HasValue("RealismMode")
          ? int.Parse(realismNode.GetValue("RealismMode"))
          : RealismMode;
        RealXfers = realismNode.HasValue("RealXfers")
          ? bool.Parse(realismNode.GetValue("RealXfers"))
          : RealXfers;
        RealControl = realismNode.HasValue("RealControl")
          ? bool.Parse(realismNode.GetValue("RealControl"))
          : RealControl;
        EnableCrew = realismNode.HasValue("EnableCrew") 
          ? bool.Parse(realismNode.GetValue("EnableCrew")) 
          : EnableCrew;
        EnableKerbalRename = realismNode.HasValue("EnableKerbalRename")
          ? bool.Parse(realismNode.GetValue("EnableKerbalRename"))
          : EnableKerbalRename;
        EnableChangeProfession = realismNode.HasValue("EnableChangeProfession")
          ? bool.Parse(realismNode.GetValue("EnableChangeProfession"))
          : EnableChangeProfession;
        EnableCrewModify = realismNode.HasValue("EnableCrewModify") 
          ? bool.Parse(realismNode.GetValue("EnableCrewModify")) 
          : EnableCrewModify;
        EnablePfCrews = realismNode.HasValue("EnablePfCrews") 
          ? bool.Parse(realismNode.GetValue("EnablePfCrews")) 
          : EnablePfCrews;
        EnableStockCrewXfer = realismNode.HasValue("EnableStockCrewTransfer")
          ? bool.Parse(realismNode.GetValue("EnableStockCrewTransfer"))
          : EnableStockCrewXfer;

        EnableScience = realismNode.HasValue("EnableScience")
          ? bool.Parse(realismNode.GetValue("EnableScience"))
          : EnableScience;
        EnableResources = realismNode.HasValue("EnableResources")
          ? bool.Parse(realismNode.GetValue("EnableResources"))
          : EnableResources;
        EnablePfResources = realismNode.HasValue("EnablePFResources")
          ? bool.Parse(realismNode.GetValue("EnablePFResources"))
          : EnablePfResources;
        EnableCls = realismNode.HasValue("EnableCLS") 
          ? bool.Parse(realismNode.GetValue("EnableCLS")) 
          : EnableCls;
        OverrideStockCrewXfer = realismNode.HasValue("OverrideStockCrewTransfer")
          ? bool.Parse(realismNode.GetValue("OverrideStockCrewTransfer"))
          : OverrideStockCrewXfer;
        EnableClsAllowTransfer = realismNode.HasValue("EnableClsAllowTransfer")
          ? bool.Parse(realismNode.GetValue("EnableClsAllowTransfer"))
          : EnableClsAllowTransfer;
        FlowRate = realismNode.HasValue("FlowRate") 
          ? double.Parse(realismNode.GetValue("FlowRate")) 
          : FlowRate;
        FlowCost = realismNode.HasValue("FlowCost") 
          ? double.Parse(realismNode.GetValue("FlowCost")) 
          : FlowCost;
        MinFlowRate = realismNode.HasValue("MinFlowRate")
          ? double.Parse(realismNode.GetValue("MinFlowRate"))
          : MinFlowRate;
        MaxFlowRate = realismNode.HasValue("MaxFlowRate")
          ? double.Parse(realismNode.GetValue("MaxFlowRate"))
          : MaxFlowRate;
        MaxFlowTimeSec = realismNode.HasValue("MaxFlowTimeSec")
          ? int.Parse(realismNode.GetValue("MaxFlowTimeSec"))
          : MaxFlowTimeSec;
        EnableXferCost = realismNode.HasValue("EnableXferCost")
          ? bool.Parse(realismNode.GetValue("EnableXferCost"))
          : EnableXferCost;
        LockSettings = realismNode.HasValue("LockSettings")
          ? bool.Parse(realismNode.GetValue("LockSettings"))
          : LockSettings;

        // Highlighting settings
        EnableHighlighting = highlightNode.HasValue("EnableHighlighting")
          ? bool.Parse(highlightNode.GetValue("EnableHighlighting"))
          : EnableHighlighting;
        OnlySourceTarget = highlightNode.HasValue("OnlySourceTarget")
          ? bool.Parse(highlightNode.GetValue("OnlySourceTarget"))
          : OnlySourceTarget;
        EnableClsHighlighting = highlightNode.HasValue("EnableCLSHighlighting")
          ? bool.Parse(highlightNode.GetValue("EnableCLSHighlighting"))
          : EnableClsHighlighting;
        EnableEdgeHighlighting = highlightNode.HasValue("EnableEdgeHighlighting")
          ? bool.Parse(highlightNode.GetValue("EnableEdgeHighlighting"))
          : EnableClsHighlighting;
        ResourcePartColor = hiddenNode.HasValue("ResourcePartColor")
          ? hiddenNode.GetValue("ResourcePartColor")
          : ResourcePartColor;

        // ToolTip Settings
        ShowToolTips = toolTipsNode.HasValue("ShowToolTips")
          ? bool.Parse(toolTipsNode.GetValue("ShowToolTips"))
          : ShowToolTips;
        WindowManifest.ShowToolTips = toolTipsNode.HasValue("ManifestToolTips")
          ? bool.Parse(toolTipsNode.GetValue("ManifestToolTips"))
          : WindowManifest.ShowToolTips;
        WindowTransfer.ShowToolTips = toolTipsNode.HasValue("TransferToolTips")
          ? bool.Parse(toolTipsNode.GetValue("TransferToolTips"))
          : WindowTransfer.ShowToolTips;
        WindowSettings.ShowToolTips = toolTipsNode.HasValue("SettingsToolTips")
          ? bool.Parse(toolTipsNode.GetValue("SettingsToolTips"))
          : WindowSettings.ShowToolTips;
        TabRealism.ShowToolTips = toolTipsNode.HasValue("RealismToolTips")
          ? bool.Parse(toolTipsNode.GetValue("RealismToolTips"))
          : TabRealism.ShowToolTips;
        TabHighlight.ShowToolTips = toolTipsNode.HasValue("HighlightingToolTips")
          ? bool.Parse(toolTipsNode.GetValue("HighlightingToolTips"))
          : TabHighlight.ShowToolTips;
        TabToolTips.ShowToolTips = toolTipsNode.HasValue("ToolTipsToolTips")
          ? bool.Parse(toolTipsNode.GetValue("ToolTipsToolTips"))
          : TabToolTips.ShowToolTips;
        TabSounds.ShowToolTips = toolTipsNode.HasValue("SoundsToolTips")
          ? bool.Parse(toolTipsNode.GetValue("SoundsToolTips"))
          : TabSounds.ShowToolTips;
        TabConfig.ShowToolTips = toolTipsNode.HasValue("ConfigToolTips")
          ? bool.Parse(toolTipsNode.GetValue("ConfigToolTips"))
          : TabConfig.ShowToolTips;
        WindowRoster.ShowToolTips = toolTipsNode.HasValue("RosterToolTips")
          ? bool.Parse(toolTipsNode.GetValue("RosterToolTips"))
          : WindowRoster.ShowToolTips;
        WindowControl.ShowToolTips = toolTipsNode.HasValue("ControlToolTips")
          ? bool.Parse(toolTipsNode.GetValue("ControlToolTips"))
          : WindowControl.ShowToolTips;
        TabHatch.ShowToolTips = toolTipsNode.HasValue("HatchToolTips")
          ? bool.Parse(toolTipsNode.GetValue("HatchToolTips"))
          : TabHatch.ShowToolTips;
        TabSolarPanel.ShowToolTips = toolTipsNode.HasValue("PanelToolTips")
          ? bool.Parse(toolTipsNode.GetValue("PanelToolTips"))
          : TabSolarPanel.ShowToolTips;
        TabAntenna.ShowToolTips = toolTipsNode.HasValue("AntennaToolTips")
          ? bool.Parse(toolTipsNode.GetValue("AntennaToolTips"))
          : TabAntenna.ShowToolTips;
        TabLight.ShowToolTips = toolTipsNode.HasValue("LightToolTips")
          ? bool.Parse(toolTipsNode.GetValue("LightToolTips"))
          : TabLight.ShowToolTips;
        WindowDebugger.ShowToolTips = toolTipsNode.HasValue("DebuggerToolTips")
          ? bool.Parse(toolTipsNode.GetValue("DebuggerToolTips"))
          : WindowDebugger.ShowToolTips;

        // Sounds Settings
        PumpSoundStart = soundsNode.HasValue("PumpSoundStart")
          ? soundsNode.GetValue("PumpSoundStart")
          : PumpSoundStart;
        PumpSoundRun = soundsNode.HasValue("PumpSoundRun") 
          ? soundsNode.GetValue("PumpSoundRun") 
          : PumpSoundRun;
        PumpSoundStop = soundsNode.HasValue("PumpSoundStop") 
          ? soundsNode.GetValue("PumpSoundStop") 
          : PumpSoundStop;
        CrewSoundStart = soundsNode.HasValue("CrewSoundStart")
          ? soundsNode.GetValue("CrewSoundStart")
          : CrewSoundStart;
        CrewSoundRun = soundsNode.HasValue("CrewSoundRun") 
          ? soundsNode.GetValue("CrewSoundRun") 
          : CrewSoundRun;
        CrewSoundStop = soundsNode.HasValue("CrewSoundStop") 
          ? soundsNode.GetValue("CrewSoundStop") 
          : CrewSoundStop;

        PumpSoundVol = soundsNode.HasValue("PumpSoundVol")
          ? double.Parse(soundsNode.GetValue("PumpSoundVol"))
          : PumpSoundVol;
        CrewSoundVol = soundsNode.HasValue("CrewSoundVol")
          ? double.Parse(soundsNode.GetValue("CrewSoundVol"))
          : CrewSoundVol;

        // Config Settings
        EnableBlizzyToolbar = configNode.HasValue("EnableBlizzyToolbar")
          ? bool.Parse(configNode.GetValue("EnableBlizzyToolbar"))
          : EnableBlizzyToolbar;
        WindowDebugger.ShowWindow = configNode.HasValue("ShowDebugger")
          ? bool.Parse(configNode.GetValue("ShowDebugger"))
          : WindowDebugger.ShowWindow;
        VerboseLogging = configNode.HasValue("VerboseLogging")
          ? bool.Parse(configNode.GetValue("VerboseLogging"))
          : VerboseLogging;
        AutoSave = configNode.HasValue("AutoSave") 
          ? bool.Parse(configNode.GetValue("AutoSave")) 
          : AutoSave;
        SaveIntervalSec = configNode.HasValue("SaveIntervalSec")
          ? int.Parse(configNode.GetValue("SaveIntervalSec"))
          : SaveIntervalSec;
        AutoDebug = configNode.HasValue("AutoDebug") 
          ? bool.Parse(configNode.GetValue("AutoDebug")) 
          : AutoDebug;
        DebugLogPath = configNode.HasValue("DebugLogPath") 
          ? configNode.GetValue("DebugLogPath") 
          : DebugLogPath;
        ErrorLogLength = configNode.HasValue("ErrorLogLength")
          ? configNode.GetValue("ErrorLogLength")
          : ErrorLogLength;
        SaveLogOnExit = configNode.HasValue("SaveLogOnExit")
          ? bool.Parse(configNode.GetValue("SaveLogOnExit"))
          : SaveLogOnExit;
        UseUnityStyle = configNode.HasValue("UseUnityStyle")
          ? bool.Parse(configNode.GetValue("UseUnityStyle"))
          : UseUnityStyle;

        // Hidden Settings
        // Hidden Highlighting
        SourcePartColor = hiddenNode.HasValue("SourcePartColor")
          ? hiddenNode.GetValue("SourcePartColor")
          : SourcePartColor;
        TargetPartColor = hiddenNode.HasValue("TargetPartColor")
          ? hiddenNode.GetValue("TargetPartColor")
          : TargetPartColor;
        TargetPartCrewColor = hiddenNode.HasValue("TargetPartCrewColor")
          ? hiddenNode.GetValue("TargetPartCrewColor")
          : TargetPartCrewColor;
        MouseOverColor = hiddenNode.HasValue("MouseOverColor") 
          ? hiddenNode.GetValue("MouseOverColor") 
          : MouseOverColor;

        // Hidden config
        CrewXferDelaySec = hiddenNode.HasValue("CrewXferDelaySec")
          ? double.Parse(hiddenNode.GetValue("CrewXferDelaySec"))
          : CrewXferDelaySec;
        IvaUpdateFrameDelay = hiddenNode.HasValue("IvaUpdateFrameDelay")
          ? int.Parse(hiddenNode.GetValue("IvaUpdateFrameDelay"))
          : IvaUpdateFrameDelay;
        // Okay, set the Settings loaded flag
        Loaded = true;
        MemStoreTempSettings();
      }

      // Enable/Disable crewed parts CrewTransferDialog 
      SetStockCrewTransferState();

      // Force Styles to refresh/load.
      SMStyle.WindowStyle = null;

      // Lets make sure that the windows can be seen on the screen. (supports different resolutions)
      SMAddon.RepositionWindows();
    }

    internal static void SaveSettings()
    {
      if (Loaded && (HighLogic.LoadedScene == GameScenes.FLIGHT || HighLogic.LoadedScene == GameScenes.SPACECENTER))
      {
        if (EnableStockCrewXfer != PrevEnableStockCrewXfer)
        {
          SetStockCrewTransferState();
        }
        if (SMSound.SoundSettingsChanged()) SMSound.LoadSounds();

        MemStoreTempSettings();
        if (Settings == null)
          Settings = LoadSettingsFile();

        ConfigNode windowsNode = Settings.HasNode("SM_Windows")
          ? Settings.GetNode("SM_Windows")
          : Settings.AddNode("SM_Windows");
        ConfigNode realismNode = Settings.HasNode("SM_Realism")
          ? Settings.GetNode("SM_Realism")
          : Settings.AddNode("SM_Realism");
        ConfigNode highlightNode = Settings.HasNode("SM_Highlight")
          ? Settings.GetNode("SM_Highlight")
          : Settings.AddNode("SM_Highlight");
        ConfigNode toolTipsNode = Settings.HasNode("SM_ToolTips")
          ? Settings.GetNode("SM_ToolTips")
          : Settings.AddNode("SM_ToolTips");
        ConfigNode soundsNode = Settings.HasNode("SM_Sounds")
          ? Settings.GetNode("SM_Sounds")
          : Settings.AddNode("SM_Sounds");
        ConfigNode configNode = Settings.HasNode("SM_Config")
          ? Settings.GetNode("SM_Config")
          : Settings.AddNode("SM_Config");
        ConfigNode hiddenNode = Settings.HasNode("SM_Hidden") 
          ? Settings.GetNode("SM_Hidden") 
          : Settings.AddNode("SM_Hidden");

        // Write window positions
        WriteRectangle(windowsNode, "ManifestPosition", WindowManifest.Position);
        WriteRectangle(windowsNode, "TransferPosition", WindowTransfer.Position);
        WriteRectangle(windowsNode, "DebuggerPosition", WindowDebugger.Position);
        WriteRectangle(windowsNode, "SettingsPosition", WindowSettings.Position);
        WriteRectangle(windowsNode, "ControlPosition", WindowControl.Position);
        WriteRectangle(windowsNode, "RosterPosition", WindowRoster.Position);

        //Write settings...
        // Realism Settings
        WriteValue(realismNode, "RealismMode", RealismMode);
        WriteValue(realismNode, "RealXfers", RealXfers);
        WriteValue(realismNode, "RealControl", RealControl);
        WriteValue(realismNode, "EnableCrew", EnableCrew);
        WriteValue(realismNode, "EnableCrewModify", EnableCrewModify);
        WriteValue(realismNode, "EnableKerbalRename", EnableKerbalRename);
        WriteValue(realismNode, "EnableChangeProfession", EnableChangeProfession);
        WriteValue(realismNode, "EnablePfCrews", EnablePfCrews);
        WriteValue(realismNode, "EnableStockCrewTransfer", EnableStockCrewXfer);
        WriteValue(realismNode, "EnableScience", EnableScience);
        WriteValue(realismNode, "EnableResources", EnableResources);
        WriteValue(realismNode, "EnablePFResources", EnablePfResources);
        WriteValue(realismNode, "EnableCLS", EnableCls);
        WriteValue(realismNode, "OverrideStockCrewTransfer", OverrideStockCrewXfer);
        WriteValue(realismNode, "EnableClsAllowTransfer", EnableClsAllowTransfer);
        WriteValue(realismNode, "FlowRate", FlowRate);
        WriteValue(realismNode, "FlowCost", FlowCost);
        WriteValue(realismNode, "MinFlowRate", MinFlowRate);
        WriteValue(realismNode, "MaxFlowRate", MaxFlowRate);
        WriteValue(realismNode, "MaxFlowTimeSec", MaxFlowTimeSec);
        WriteValue(realismNode, "EnableXferCost", EnableXferCost);
        WriteValue(realismNode, "LockSettings", LockSettings);

        // Highlighting Settings
        WriteValue(highlightNode, "EnableHighlighting", EnableHighlighting);
        WriteValue(highlightNode, "OnlySourceTarget", OnlySourceTarget);
        WriteValue(highlightNode, "EnableCLSHighlighting", EnableClsHighlighting);
        WriteValue(highlightNode, "EnableEdgeHighlighting", EnableEdgeHighlighting);

        // ToolTip Settings
        WriteValue(toolTipsNode, "ShowToolTips", ShowToolTips);
        WriteValue(toolTipsNode, "DebuggerToolTips", WindowDebugger.ShowToolTips);
        WriteValue(toolTipsNode, "ManifestToolTips", WindowManifest.ShowToolTips);
        WriteValue(toolTipsNode, "TransferToolTips", WindowTransfer.ShowToolTips);
        WriteValue(toolTipsNode, "SettingsToolTips", WindowSettings.ShowToolTips);
        WriteValue(toolTipsNode, "RealismToolTips", TabRealism.ShowToolTips);
        WriteValue(toolTipsNode, "HighlightingToolTips", TabHighlight.ShowToolTips);
        WriteValue(toolTipsNode, "ToolTipsToolTips", TabToolTips.ShowToolTips);
        WriteValue(toolTipsNode, "SoundsToolTips", TabSounds.ShowToolTips);
        WriteValue(toolTipsNode, "ConfigToolTips", TabConfig.ShowToolTips);
        WriteValue(toolTipsNode, "RosterToolTips", WindowRoster.ShowToolTips);
        WriteValue(toolTipsNode, "ControlToolTips", WindowControl.ShowToolTips);
        WriteValue(toolTipsNode, "HatchToolTips", TabHatch.ShowToolTips);
        WriteValue(toolTipsNode, "PanelToolTips", TabSolarPanel.ShowToolTips);
        WriteValue(toolTipsNode, "AntennaToolTips", TabAntenna.ShowToolTips);
        WriteValue(toolTipsNode, "LightToolTips", TabLight.ShowToolTips);

        // Sound Settings
        WriteValue(soundsNode, "PumpSoundStart", PumpSoundStart);
        WriteValue(soundsNode, "PumpSoundRun", PumpSoundRun);
        WriteValue(soundsNode, "PumpSoundStop", PumpSoundStop);
        WriteValue(soundsNode, "CrewSoundStart", CrewSoundStart);
        WriteValue(soundsNode, "CrewSoundRun", CrewSoundRun);
        WriteValue(soundsNode, "CrewSoundStop", CrewSoundStop);
        WriteValue(soundsNode, "PumpSoundVol", PumpSoundVol);
        WriteValue(soundsNode, "CrewSoundVol", CrewSoundVol);

        // Config Settings
        WriteValue(configNode, "ShowDebugger", WindowDebugger.ShowWindow);
        WriteValue(configNode, "EnableBlizzyToolbar", EnableBlizzyToolbar);
        WriteValue(configNode, "VerboseLogging", VerboseLogging);
        WriteValue(configNode, "AutoSave", AutoSave);
        WriteValue(configNode, "SaveIntervalSec", SaveIntervalSec);
        WriteValue(configNode, "AutoDebug", AutoDebug);
        WriteValue(configNode, "DebugLogPath", DebugLogPath);
        WriteValue(configNode, "ErrorLogLength", ErrorLogLength);
        WriteValue(configNode, "SaveLogOnExit", SaveLogOnExit);
        WriteValue(configNode, "UseUnityStyle", UseUnityStyle);

        // Hidden Settings
        WriteValue(hiddenNode, "ResourcePartColor", ResourcePartColor);
        WriteValue(hiddenNode, "SourcePartColor", SourcePartColor);
        WriteValue(hiddenNode, "TargetPartColor", TargetPartColor);
        WriteValue(hiddenNode, "TargetPartCrewColor", TargetPartCrewColor);
        WriteValue(hiddenNode, "MouseOverColor", MouseOverColor);
        WriteValue(hiddenNode, "CrewXferDelaySec", CrewXferDelaySec);
        WriteValue(hiddenNode, "IvaUpdateFrameDelay", IvaUpdateFrameDelay);

        if (!Directory.Exists(SettingsPath))
          Directory.CreateDirectory(SettingsPath);
        Settings.Save(SettingsFile);
      }
    }

    internal static void SetRealismMode(int mode)
    {
      if (mode < 3) RealismMode = mode;
      switch (mode)
      {
        case 0: // Full
          RealXfers = true;
          RealControl = true;
          EnableCrew = true;
          EnableKerbalRename = false;
          EnableChangeProfession = false;
          EnableCrewModify = false;
          EnableStockCrewXfer = true;
          OverrideStockCrewXfer = true;
          EnableClsAllowTransfer = true;
          EnableCls = ClsInstalled;
          EnableScience = true;
          EnableResources = true;
          EnablePfCrews = false;
          EnablePfResources = false;
          EnableXferCost = true;
          FlowCost = 0.0015;
          FlowRate = 100;
          MinFlowRate = 0;
          MaxFlowRate = 1000;
          Tolerance = 0.000001;
          MaxFlowTimeSec = 180;
          LockSettings = false;
          break;
        case 1: // None
          RealXfers = false;
          RealControl = false;
          EnableCrew = true;
          EnableCrewModify = true;
          EnableKerbalRename = true;
          EnableChangeProfession = true;
          EnableStockCrewXfer = true;
          OverrideStockCrewXfer = false;
          EnableClsAllowTransfer = true;
          EnableCls = false;
          EnableScience = true;
          EnableResources = true;
          EnablePfCrews = false;
          EnablePfResources = true;
          EnableXferCost = false;
          FlowCost = 0.0015;
          FlowRate = 100;
          MinFlowRate = 0;
          MaxFlowRate = 1000;
          Tolerance = 0.000001;
          MaxFlowTimeSec = 180;
          LockSettings = false;
          break;
        case 2: // Default
          RealXfers = true;
          RealControl = true;
          EnableCrew = true;
          EnableCrewModify = true;
          EnableKerbalRename = true;
          EnableChangeProfession = true;
          EnableStockCrewXfer = true;
          OverrideStockCrewXfer = true;
          EnableClsAllowTransfer = true;
          EnableCls = ClsInstalled;
          EnableScience = true;
          EnableResources = true;
          EnablePfCrews = false;
          EnablePfResources = true;
          EnableXferCost = true;
          FlowCost = 0.0015;
          FlowRate = 100;
          MinFlowRate = 0;
          MaxFlowRate = 1000;
          Tolerance = 0.000001;
          MaxFlowTimeSec = 180;
          LockSettings = false;
          break;
        case 3: // Custom  Do nothing.
          break;
      }
    }

    internal static int GetRealismMode()
    {
      if ( // Full
          RealXfers
          && RealControl
          && EnableCrew
          && EnableKerbalRename == false
          && EnableChangeProfession == false
          && EnableCrewModify == false
          && EnableStockCrewXfer
          && OverrideStockCrewXfer
          && EnableClsAllowTransfer
          && EnableCls == ClsInstalled
          && EnableScience
          && EnableResources
          && EnablePfCrews == false
          && EnablePfResources == false
          && EnableXferCost
          && Math.Abs(FlowCost - 0.0015) < 0.0001f
          && Math.Abs(FlowRate - 100) < 0.0001f
          && Math.Abs(MinFlowRate) < 0.0001f
          && Math.Abs(MaxFlowRate - 1000) < 0.0001f
          && Math.Abs(Tolerance - 0.000001) < 0.0001f
          && MaxFlowTimeSec == 180
          && LockSettings == false)
        return 0;
      if ( // None
          RealXfers == false
          && RealControl == false
          && EnableCrew
          && EnableCrewModify
          && EnableKerbalRename
          && EnableChangeProfession
          && EnableStockCrewXfer
          && OverrideStockCrewXfer == false
          && EnableClsAllowTransfer
          && EnableCls == false
          && EnableScience
          && EnableResources
          && EnablePfCrews == false
          && EnablePfResources
          && EnableXferCost == false
          && LockSettings == false)
        return 1;
      if ( // Default
          RealXfers
          && RealControl
          && EnableCrew
          && EnableCrewModify
          && EnableKerbalRename
          && EnableChangeProfession
          && EnableStockCrewXfer
          && OverrideStockCrewXfer
          && EnableClsAllowTransfer
          && EnableCls == ClsInstalled
          && EnableScience
          && EnableResources
          && EnablePfCrews == false
          && EnablePfResources
          && EnableXferCost
          && Math.Abs(FlowCost - 0.0015) < 0.0001f
          && Math.Abs(FlowRate - 100) < 0.0001f
          && Math.Abs(MinFlowRate) < 0.0001f
          && Math.Abs(MaxFlowRate - 1000) < 0.0001f
          && Math.Abs(Tolerance - 0.000001) < 0.000001f
          && MaxFlowTimeSec == 180
          && LockSettings == false)
        return 2;

        return 3;
    }

    internal static void SetStockCrewTransferState()
    {
      // wrap in a try, as save can be executed outside of the flight scene and we don't care if it fails...
      try
      {
        List<Part>.Enumerator parts = SMAddon.SmVessel.PartsByResource[SMConditions.ResourceType.Crew.ToString()].GetEnumerator();
        while (parts.MoveNext())
        {
          if (parts.Current == null) continue;
          Part part = parts.Current;
          part.crewTransferAvailable = EnableStockCrewXfer;
          //TransferDialogSpawner Tds = part.FindModuleImplementing<TransferDialogSpawner>();
          //if (EnableStockCrewXfer)
          //{
          //  if (Tds != null) continue;
          //  part.AddModule("TransferDialogSpawner");
          //  MonoUtilities.RefreshContextWindows(part);
          //}
          //else
          //{
          //  if (Tds != null) part.RemoveModule(Tds);
          //}
        }
        parts.Dispose();
      }
      catch (Exception)
      {
        // Do nothing.   We don't care if it fails when outside of the Flight Scene.
      }
    }

    internal static void SetClsOverride()
    {
      if (!EnableCls || !HighLogic.LoadedSceneIsFlight || !EnableClsAllowTransfer) return;
      SMAddon.OrigClsAllowCrewXferSetting = SMAddon.ClsAddon.AllowUnrestrictedTransfers;
      SMAddon.ClsAddon.AllowUnrestrictedTransfers = true;
    }

    internal static void UpdateClsOverride()
    {
      if (!EnableCls || !HighLogic.LoadedSceneIsFlight) return;
      SMAddon.ClsAddon.AllowUnrestrictedTransfers = EnableClsAllowTransfer;
    }

    private static Rect GetRectangle(ConfigNode windowsNode, string rectName, Rect defaultvalue)
    {
      Rect thisRect = new Rect();
      try
      {
        ConfigNode rectNode = windowsNode.HasNode(rectName) ? windowsNode.GetNode(rectName) : windowsNode.AddNode(rectName);
        thisRect.x = rectNode.HasValue("x") ? int.Parse(rectNode.GetValue("x")) : defaultvalue.x;
        thisRect.y = rectNode.HasValue("y") ? int.Parse(rectNode.GetValue("y")) : defaultvalue.y;
        thisRect.width = rectNode.HasValue("width") ? int.Parse(rectNode.GetValue("width")) : defaultvalue.width;
        thisRect.height = rectNode.HasValue("height") ? int.Parse(rectNode.GetValue("height")) : defaultvalue.height;
      }
      catch
      {
        thisRect = defaultvalue;
      }


      return thisRect;
    }

    private static void WriteRectangle(ConfigNode windowsNode, string rectName, Rect rectValue)
    {
      ConfigNode rectNode = windowsNode.HasNode(rectName) ? windowsNode.GetNode(rectName) : windowsNode.AddNode(rectName);
      WriteValue(rectNode, "x", (int)rectValue.x);
      WriteValue(rectNode, "y", (int)rectValue.y);
      WriteValue(rectNode, "width", (int)rectValue.width);
      WriteValue(rectNode, "height", (int)rectValue.height);
    }

    private static void WriteValue(ConfigNode configNode, string valueName, object value)
    {
      if (configNode.HasValue(valueName))
        configNode.RemoveValue(valueName);
      configNode.AddValue(valueName, value.ToString());
    }

    internal static void LoadColors()
    {
      Colors = new Dictionary<string, Color>
      {
        {"black", XKCDColors.Black},
        {"blue", XKCDColors.Blue},
        {"clear", Color.clear},
        {"cyan", XKCDColors.Cyan},
        {"gray", XKCDColors.Grey},
        {"green", XKCDColors.Green},
        {"magenta", XKCDColors.Magenta},
        {"red", XKCDColors.Red},
        {"white", XKCDColors.White},
        {"yellow", XKCDColors.Yellow},
        {"burntorange", XKCDColors.BurntOrange },
        {"default", new Color(0.478f, 0.698f, 0.478f, 0.698f)}
      };
    }

    internal static void MemStoreTempSettings()
    {
      PrevRealismMode = RealismMode;
      PrevRealXfers = RealXfers;
      PrevRealControl = RealControl;
      PrevShowDebugger = WindowDebugger.ShowWindow;
      PrevVerboseLogging = VerboseLogging;
      PrevAutoSave = AutoSave;
      PrevSaveIntervalSec = SaveIntervalSec;
      PrevFlowRate = FlowRate;
      PrevFlowCost = FlowCost;
      PrevMinFlowRate = MinFlowRate;
      PrevMaxFlowRate = MaxFlowRate;
      PrevMaxFlowTimeSec = MaxFlowTimeSec;
      PrevEnableXferCost = EnableXferCost;
      PrevPumpSoundStart = PumpSoundStart;
      PrevPumpSoundRun = PumpSoundRun;
      PrevPumpSoundStop = PumpSoundStop;
      PrevCrewSoundStart = CrewSoundStart;
      PrevCrewSoundRun = CrewSoundRun;
      PrevCrewSoundStop = CrewSoundStop;
      PrevCrewSoundVol = CrewSoundVol;
      PrevPumpSoundVol = PumpSoundVol;

      PrevEnableScience = EnableScience;
      PrevEnableHighlighting = EnableHighlighting;
      PrevOnlySourceTarget = OnlySourceTarget;
      PrevEnableClsHighlighting = EnableClsHighlighting;
      PrevEnableCrew = EnableCrew;
      PrevEnableCrewModify = EnableCrewModify;
      PrevEnablePfCrews = EnablePfCrews;
      PrevEnablePfResources = EnablePfResources;
      PrevEnableCls = EnableCls;
      PrevEnableStockCrewXfer = EnableStockCrewXfer;
      PrevOverrideStockCrewXfer = OverrideStockCrewXfer;
      PrevEnableClsAllowTransfer = EnableClsAllowTransfer;
      PrevEnableKerbalRename = EnableKerbalRename;
      PrevEnableChangeProfession = EnableChangeProfession;
      PrevUseUnityStyle = UseUnityStyle;
      PrevLockSettings = LockSettings;
      PrevEnableBlizzyToolbar = EnableBlizzyToolbar;
      PrevSaveLogOnExit = SaveLogOnExit;
      PrevShowToolTips = ShowToolTips;
      PrevDebuggerToolTips = WindowDebugger.ShowToolTips;
      PrevManifestToolTips = WindowManifest.ShowToolTips;
      PrevTransferToolTips = WindowTransfer.ShowToolTips;
      PrevSettingsToolTips = WindowSettings.ShowToolTips;
      PrevRosterToolTips = WindowRoster.ShowToolTips;
      PrevControlToolTips = WindowControl.ShowToolTips;
      PrevHatchToolTips = TabHatch.ShowToolTips;
      PrevPanelToolTips = TabSolarPanel.ShowToolTips;
      PrevAntennaToolTips = TabAntenna.ShowToolTips;
      PrevLightToolTips = TabLight.ShowToolTips;

      PrevRealismToolTips = TabRealism.ShowToolTips;
      PrevToolTipsToolTips = TabToolTips.ShowToolTips;
      PrevSoundsToolTips = TabSounds.ShowToolTips;
      PrevHighlightToolTips = TabHighlight.ShowToolTips;
      PrevConfigToolTips = TabConfig.ShowToolTips;

      //debugger Settings
      PrevErrorLogLength = ErrorLogLength;
    }

    internal static void MemRestoreTempSettings()
    {
      RealismMode = PrevRealismMode;
      RealXfers = PrevRealXfers;
      RealControl = PrevRealControl;
      WindowDebugger.ShowWindow = PrevShowDebugger;
      VerboseLogging = PrevVerboseLogging;
      AutoSave = PrevAutoSave;
      SaveIntervalSec = PrevSaveIntervalSec;
      FlowRate = PrevFlowRate;
      FlowCost = PrevFlowCost;
      MinFlowRate = PrevMinFlowRate;
      MaxFlowRate = PrevMaxFlowRate;
      MaxFlowTimeSec = PrevMaxFlowTimeSec;
      EnableXferCost = PrevEnableXferCost;
      PumpSoundStart = PrevPumpSoundStart;
      PumpSoundRun = PrevPumpSoundRun;
      PumpSoundStop = PrevPumpSoundStop;
      CrewSoundStart = PrevCrewSoundStart;
      CrewSoundRun = PrevCrewSoundRun;
      CrewSoundStop = PrevCrewSoundStop;
      CrewSoundVol = PrevCrewSoundVol;
      PumpSoundVol = PrevPumpSoundVol;
      EnableScience = PrevEnableScience;
      EnableHighlighting = PrevEnableHighlighting;
      OnlySourceTarget = PrevOnlySourceTarget;
      EnableClsHighlighting = PrevEnableClsHighlighting;
      EnableCrew = PrevEnableCrew;
      EnableCrewModify = PrevEnableCrewModify;
      EnablePfCrews = PrevEnablePfCrews;
      EnablePfResources = PrevEnablePfResources;
      EnableCls = PrevEnableCls;
      EnableStockCrewXfer = PrevEnableStockCrewXfer;
      OverrideStockCrewXfer = PrevOverrideStockCrewXfer;
      EnableClsAllowTransfer = PrevEnableClsAllowTransfer;
      EnableKerbalRename = PrevEnableKerbalRename;
      EnableChangeProfession = PrevEnableChangeProfession;
      UseUnityStyle = PrevUseUnityStyle;
      LockSettings = PrevLockSettings;
      EnableBlizzyToolbar = PrevEnableBlizzyToolbar;
      SaveLogOnExit = PrevSaveLogOnExit;
      ShowToolTips = PrevShowToolTips;
      WindowDebugger.ShowToolTips = PrevDebuggerToolTips;
      WindowManifest.ShowToolTips = PrevManifestToolTips;
      WindowTransfer.ShowToolTips = PrevTransferToolTips;
      WindowSettings.ShowToolTips = PrevSettingsToolTips;
      WindowRoster.ShowToolTips = PrevRosterToolTips;
      WindowControl.ShowToolTips = PrevControlToolTips;
      TabHatch.ShowToolTips = PrevHatchToolTips;
      TabSolarPanel.ShowToolTips = PrevPanelToolTips;
      TabAntenna.ShowToolTips = PrevAntennaToolTips;
      TabLight.ShowToolTips = PrevLightToolTips;

      TabRealism.ShowToolTips = PrevRealismToolTips;
      TabHighlight.ShowToolTips = PrevHighlightToolTips;
      TabToolTips.ShowToolTips = PrevToolTipsToolTips;
      TabSounds.ShowToolTips = PrevSoundsToolTips;
      TabConfig.ShowToolTips = PrevConfigToolTips;
      //TabInstalledMods.ShowToolTips = PrevModsToolTips;

      //debugger Settings
      PrevErrorLogLength = ErrorLogLength;
    }

    #endregion
  }
}