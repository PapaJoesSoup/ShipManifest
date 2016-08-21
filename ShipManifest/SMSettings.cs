using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ShipManifest.InternalObjects;
using ShipManifest.Windows;
using ShipManifest.Windows.Tabs;
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

    private static readonly string SettingsPath = string.Format("{0}GameData/ShipManifest/Plugins/PluginData",
      KSPUtil.ApplicationRootPath);

    private static readonly string SettingsFile = string.Format("{0}/SMSettings.dat", SettingsPath);

    // This value is assigned from AssemblyInfo.cs
    internal static string CurVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

    // Persisted properties
    // UI Managed Settings
    // Realism Tab Feature Options
    internal static bool RealismMode = true;
    internal static bool EnableCrew = true;
    internal static bool EnableStockCrewXfer = true;
    internal static bool OverrideStockCrewXfer = true;
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
    internal static bool EnableKerbalRename = true;
    internal static bool EnableChangeProfession = true;
    internal static bool AutoSave;
    internal static int SaveIntervalSec = 60;
    internal static bool UseUnityStyle = true;


    // Options unmanaged by UI.
    internal static string ResourcePartColor = "yellow";
    internal static string SourcePartColor = "red";
    internal static string TargetPartColor = "green";
    internal static string TargetPartCrewColor = "blue";
    internal static string ClsSpaceColor = "green";
    internal static string MouseOverColor = "green";
    internal static double CrewXferDelaySec = 7;
    internal static int IvaUpdateFrameDelay = 20;


    // End Persisted Properties

    // Settings Window Option storage for Settings Window Cancel support
    internal static bool PrevVerboseLogging;
    internal static bool PrevShowDebugger;
    internal static string PrevErrorLogLength = "1000";
    internal static bool PrevSaveLogOnExit = true;
    internal static bool PrevAutoSave;
    internal static bool PrevUseUnityStyle = true;
    internal static int PrevSaveIntervalSec = 60;

    internal static bool PrevRealismMode;
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
    internal static bool PrevEnablePfCrews;
    internal static bool PrevEnableStockCrewXfer = true;
    internal static bool PrevOverrideStockCrewXfer = true;
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
        ConfigNode settingsNode = Settings.HasNode("SM_Settings")
          ? Settings.GetNode("SM_Settings")
          : Settings.AddNode("SM_Settings");
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
        RealismMode = settingsNode.HasValue("RealismMode")
          ? bool.Parse(settingsNode.GetValue("RealismMode"))
          : RealismMode;
        EnableCrew = settingsNode.HasValue("EnableCrew") ? bool.Parse(settingsNode.GetValue("EnableCrew")) : EnableCrew;
        EnablePfCrews = settingsNode.HasValue("EnablePfCrews") ? bool.Parse(settingsNode.GetValue("EnablePfCrews")) : EnablePfCrews;
        EnableStockCrewXfer = settingsNode.HasValue("EnableStockCrewTransfer")
          ? bool.Parse(settingsNode.GetValue("EnableStockCrewTransfer"))
          : EnableStockCrewXfer;

        EnableScience = settingsNode.HasValue("EnableScience")
          ? bool.Parse(settingsNode.GetValue("EnableScience"))
          : EnableScience;
        EnableResources = settingsNode.HasValue("EnableResources")
          ? bool.Parse(settingsNode.GetValue("EnableResources"))
          : EnableResources;
        EnablePfResources = settingsNode.HasValue("EnablePFResources")
          ? bool.Parse(settingsNode.GetValue("EnablePFResources"))
          : EnablePfResources;
        EnableCls = settingsNode.HasValue("EnableCLS") ? bool.Parse(settingsNode.GetValue("EnableCLS")) : EnableCls;
        OverrideStockCrewXfer = settingsNode.HasValue("OverrideStockCrewTransfer")
          ? bool.Parse(settingsNode.GetValue("OverrideStockCrewTransfer"))
          : OverrideStockCrewXfer;
        FlowRate = settingsNode.HasValue("FlowRate") ? double.Parse(settingsNode.GetValue("FlowRate")) : FlowRate;
        FlowCost = settingsNode.HasValue("FlowCost") ? double.Parse(settingsNode.GetValue("FlowCost")) : FlowCost;
        MinFlowRate = settingsNode.HasValue("MinFlowRate")
          ? double.Parse(settingsNode.GetValue("MinFlowRate"))
          : MinFlowRate;
        MaxFlowRate = settingsNode.HasValue("MaxFlowRate")
          ? double.Parse(settingsNode.GetValue("MaxFlowRate"))
          : MaxFlowRate;
        MaxFlowTimeSec = settingsNode.HasValue("MaxFlowTimeSec")
          ? int.Parse(settingsNode.GetValue("MaxFlowTimeSec"))
          : MaxFlowTimeSec;
        EnableXferCost = settingsNode.HasValue("EnableXferCost")
          ? bool.Parse(settingsNode.GetValue("EnableXferCost"))
          : EnableXferCost;
        LockSettings = settingsNode.HasValue("LockSettings")
          ? bool.Parse(settingsNode.GetValue("LockSettings"))
          : LockSettings;

        // Highlighting settings
        EnableHighlighting = settingsNode.HasValue("EnableHighlighting")
          ? bool.Parse(settingsNode.GetValue("EnableHighlighting"))
          : EnableHighlighting;
        OnlySourceTarget = settingsNode.HasValue("OnlySourceTarget")
          ? bool.Parse(settingsNode.GetValue("OnlySourceTarget"))
          : OnlySourceTarget;
        EnableClsHighlighting = settingsNode.HasValue("EnableCLSHighlighting")
          ? bool.Parse(settingsNode.GetValue("EnableCLSHighlighting"))
          : EnableClsHighlighting;
        EnableEdgeHighlighting = settingsNode.HasValue("EnableEdgeHighlighting")
          ? bool.Parse(settingsNode.GetValue("EnableEdgeHighlighting"))
          : EnableClsHighlighting;
        ResourcePartColor = hiddenNode.HasValue("ResourcePartColor")
          ? hiddenNode.GetValue("ResourcePartColor")
          : ResourcePartColor;

        // ToolTip Settings
        ShowToolTips = settingsNode.HasValue("ShowToolTips")
          ? bool.Parse(settingsNode.GetValue("ShowToolTips"))
          : ShowToolTips;
        WindowManifest.ShowToolTips = settingsNode.HasValue("ManifestToolTips")
          ? bool.Parse(settingsNode.GetValue("ManifestToolTips"))
          : WindowManifest.ShowToolTips;
        WindowTransfer.ShowToolTips = settingsNode.HasValue("TransferToolTips")
          ? bool.Parse(settingsNode.GetValue("TransferToolTips"))
          : WindowTransfer.ShowToolTips;
        WindowSettings.ShowToolTips = settingsNode.HasValue("SettingsToolTips")
          ? bool.Parse(settingsNode.GetValue("SettingsToolTips"))
          : WindowSettings.ShowToolTips;
        TabRealism.ShowToolTips = settingsNode.HasValue("RealismToolTips")
          ? bool.Parse(settingsNode.GetValue("RealismToolTips"))
          : TabRealism.ShowToolTips;
        TabHighlight.ShowToolTips = settingsNode.HasValue("HighlightingToolTips")
          ? bool.Parse(settingsNode.GetValue("HighlightingToolTips"))
          : TabHighlight.ShowToolTips;
        TabToolTips.ShowToolTips = settingsNode.HasValue("ToolTipsToolTips")
          ? bool.Parse(settingsNode.GetValue("ToolTipsToolTips"))
          : TabToolTips.ShowToolTips;
        TabSounds.ShowToolTips = settingsNode.HasValue("SoundsToolTips")
          ? bool.Parse(settingsNode.GetValue("SoundsToolTips"))
          : TabSounds.ShowToolTips;
        TabConfig.ShowToolTips = settingsNode.HasValue("ConfigToolTips")
          ? bool.Parse(settingsNode.GetValue("ConfigToolTips"))
          : TabConfig.ShowToolTips;
        TabInstalledMods.ShowToolTips = settingsNode.HasValue("InstalledModsToolTips")
          ? bool.Parse(settingsNode.GetValue("InstalledModsToolTips"))
          : TabInstalledMods.ShowToolTips;
        WindowRoster.ShowToolTips = settingsNode.HasValue("RosterToolTips")
          ? bool.Parse(settingsNode.GetValue("RosterToolTips"))
          : WindowRoster.ShowToolTips;
        WindowControl.ShowToolTips = settingsNode.HasValue("ControlToolTips")
          ? bool.Parse(settingsNode.GetValue("ControlToolTips"))
          : WindowControl.ShowToolTips;
        TabHatch.ShowToolTips = settingsNode.HasValue("HatchToolTips")
          ? bool.Parse(settingsNode.GetValue("HatchToolTips"))
          : TabHatch.ShowToolTips;
        TabSolarPanel.ShowToolTips = settingsNode.HasValue("PanelToolTips")
          ? bool.Parse(settingsNode.GetValue("PanelToolTips"))
          : TabSolarPanel.ShowToolTips;
        TabAntenna.ShowToolTips = settingsNode.HasValue("AntennaToolTips")
          ? bool.Parse(settingsNode.GetValue("AntennaToolTips"))
          : TabAntenna.ShowToolTips;
        TabLight.ShowToolTips = settingsNode.HasValue("LightToolTips")
          ? bool.Parse(settingsNode.GetValue("LightToolTips"))
          : TabLight.ShowToolTips;
        WindowDebugger.ShowToolTips = settingsNode.HasValue("DebuggerToolTips")
          ? bool.Parse(settingsNode.GetValue("DebuggerToolTips"))
          : WindowDebugger.ShowToolTips;

        // Sounds Settings
        PumpSoundStart = settingsNode.HasValue("PumpSoundStart")
          ? settingsNode.GetValue("PumpSoundStart")
          : PumpSoundStart;
        PumpSoundRun = settingsNode.HasValue("PumpSoundRun") ? settingsNode.GetValue("PumpSoundRun") : PumpSoundRun;
        PumpSoundStop = settingsNode.HasValue("PumpSoundStop") ? settingsNode.GetValue("PumpSoundStop") : PumpSoundStop;
        CrewSoundStart = settingsNode.HasValue("CrewSoundStart")
          ? settingsNode.GetValue("CrewSoundStart")
          : CrewSoundStart;
        CrewSoundRun = settingsNode.HasValue("CrewSoundRun") ? settingsNode.GetValue("CrewSoundRun") : CrewSoundRun;
        CrewSoundStop = settingsNode.HasValue("CrewSoundStop") ? settingsNode.GetValue("CrewSoundStop") : CrewSoundStop;

        PumpSoundVol = settingsNode.HasValue("PumpSoundVol")
          ? double.Parse(settingsNode.GetValue("PumpSoundVol"))
          : PumpSoundVol;
        CrewSoundVol = settingsNode.HasValue("CrewSoundVol")
          ? double.Parse(settingsNode.GetValue("CrewSoundVol"))
          : CrewSoundVol;

        // Config Settings
        EnableBlizzyToolbar = settingsNode.HasValue("EnableBlizzyToolbar")
          ? bool.Parse(settingsNode.GetValue("EnableBlizzyToolbar"))
          : EnableBlizzyToolbar;
        WindowDebugger.ShowWindow = settingsNode.HasValue("ShowDebugger")
          ? bool.Parse(settingsNode.GetValue("ShowDebugger"))
          : WindowDebugger.ShowWindow;
        VerboseLogging = settingsNode.HasValue("VerboseLogging")
          ? bool.Parse(settingsNode.GetValue("VerboseLogging"))
          : VerboseLogging;
        AutoSave = settingsNode.HasValue("AutoSave") ? bool.Parse(settingsNode.GetValue("AutoSave")) : AutoSave;
        SaveIntervalSec = settingsNode.HasValue("SaveIntervalSec")
          ? int.Parse(settingsNode.GetValue("SaveIntervalSec"))
          : SaveIntervalSec;
        AutoDebug = settingsNode.HasValue("AutoDebug") ? bool.Parse(settingsNode.GetValue("AutoDebug")) : AutoDebug;
        DebugLogPath = settingsNode.HasValue("DebugLogPath") ? settingsNode.GetValue("DebugLogPath") : DebugLogPath;
        ErrorLogLength = settingsNode.HasValue("ErrorLogLength")
          ? settingsNode.GetValue("ErrorLogLength")
          : ErrorLogLength;
        SaveLogOnExit = settingsNode.HasValue("SaveLogOnExit")
          ? bool.Parse(settingsNode.GetValue("SaveLogOnExit"))
          : SaveLogOnExit;
        EnableKerbalRename = settingsNode.HasValue("EnableKerbalRename")
          ? bool.Parse(settingsNode.GetValue("EnableKerbalRename"))
          : EnableKerbalRename;
        EnableChangeProfession = settingsNode.HasValue("EnableChangeProfession")
          ? bool.Parse(settingsNode.GetValue("EnableChangeProfession"))
          : EnableChangeProfession;
        UseUnityStyle = settingsNode.HasValue("UseUnityStyle")
          ? bool.Parse(settingsNode.GetValue("UseUnityStyle"))
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
        MouseOverColor = hiddenNode.HasValue("MouseOverColor") ? hiddenNode.GetValue("MouseOverColor") : MouseOverColor;

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

        MemStoreTempSettings();
        if (Settings == null)
          Settings = LoadSettingsFile();

        ConfigNode windowsNode = Settings.HasNode("SM_Windows")
          ? Settings.GetNode("SM_Windows")
          : Settings.AddNode("SM_Windows");
        ConfigNode settingsNode = Settings.HasNode("SM_Settings")
          ? Settings.GetNode("SM_Settings")
          : Settings.AddNode("SM_Settings");
        ConfigNode hiddenNode = Settings.HasNode("SM_Hidden") ? Settings.GetNode("SM_Hidden") : Settings.AddNode("SM_Hidden");

        // Write window positions
        WriteRectangle(windowsNode, "ManifestPosition", WindowManifest.Position);
        WriteRectangle(windowsNode, "TransferPosition", WindowTransfer.Position);
        WriteRectangle(windowsNode, "DebuggerPosition", WindowDebugger.Position);
        WriteRectangle(windowsNode, "SettingsPosition", WindowSettings.Position);
        WriteRectangle(windowsNode, "ControlPosition", WindowControl.Position);
        WriteRectangle(windowsNode, "RosterPosition", WindowRoster.Position);

        //Write settings...
        // Realism Settings
        WriteValue(settingsNode, "RealismMode", RealismMode);
        WriteValue(settingsNode, "EnableCrew", EnableCrew);
        WriteValue(settingsNode, "EnablePfCrews", EnablePfCrews);
        WriteValue(settingsNode, "EnableStockCrewTransfer", EnableStockCrewXfer);
        WriteValue(settingsNode, "EnableScience", EnableScience);
        WriteValue(settingsNode, "EnableResources", EnableResources);
        WriteValue(settingsNode, "EnablePFResources", EnablePfResources);
        WriteValue(settingsNode, "EnableCLS", EnableCls);
        WriteValue(settingsNode, "OverrideStockCrewTransfer", OverrideStockCrewXfer);
        WriteValue(settingsNode, "FlowRate", FlowRate);
        WriteValue(settingsNode, "FlowCost", FlowCost);
        WriteValue(settingsNode, "MinFlowRate", MinFlowRate);
        WriteValue(settingsNode, "MaxFlowRate", MaxFlowRate);
        WriteValue(settingsNode, "MaxFlowTimeSec", MaxFlowTimeSec);
        WriteValue(settingsNode, "EnableXferCost", EnableXferCost);
        WriteValue(settingsNode, "LockSettings", LockSettings);

        // Highlighting Settings
        WriteValue(settingsNode, "EnableHighlighting", EnableHighlighting);
        WriteValue(settingsNode, "OnlySourceTarget", OnlySourceTarget);
        WriteValue(settingsNode, "EnableCLSHighlighting", EnableClsHighlighting);
        WriteValue(settingsNode, "EnableEdgeHighlighting", EnableEdgeHighlighting);

        // ToolTip Settings
        WriteValue(settingsNode, "ShowToolTips", ShowToolTips);
        WriteValue(settingsNode, "DebuggerToolTips", WindowDebugger.ShowToolTips);
        WriteValue(settingsNode, "ManifestToolTips", WindowManifest.ShowToolTips);
        WriteValue(settingsNode, "TransferToolTips", WindowTransfer.ShowToolTips);
        WriteValue(settingsNode, "SettingsToolTips", WindowSettings.ShowToolTips);
        WriteValue(settingsNode, "RealismToolTips", TabRealism.ShowToolTips);
        WriteValue(settingsNode, "HighlightingToolTips", TabHighlight.ShowToolTips);
        WriteValue(settingsNode, "ToolTipsToolTips", TabToolTips.ShowToolTips);
        WriteValue(settingsNode, "SoundsToolTips", TabSounds.ShowToolTips);
        WriteValue(settingsNode, "ConfigToolTips", TabConfig.ShowToolTips);
        WriteValue(settingsNode, "InstalledModsToolTips", TabInstalledMods.ShowToolTips);
        WriteValue(settingsNode, "RosterToolTips", WindowRoster.ShowToolTips);
        WriteValue(settingsNode, "ControlToolTips", WindowControl.ShowToolTips);
        WriteValue(settingsNode, "HatchToolTips", TabHatch.ShowToolTips);
        WriteValue(settingsNode, "PanelToolTips", TabSolarPanel.ShowToolTips);
        WriteValue(settingsNode, "AntennaToolTips", TabAntenna.ShowToolTips);
        WriteValue(settingsNode, "LightToolTips", TabLight.ShowToolTips);

        // Sound Settings
        WriteValue(settingsNode, "PumpSoundStart", PumpSoundStart);
        WriteValue(settingsNode, "PumpSoundRun", PumpSoundRun);
        WriteValue(settingsNode, "PumpSoundStop", PumpSoundStop);
        WriteValue(settingsNode, "CrewSoundStart", CrewSoundStart);
        WriteValue(settingsNode, "CrewSoundRun", CrewSoundRun);
        WriteValue(settingsNode, "CrewSoundStop", CrewSoundStop);
        WriteValue(settingsNode, "PumpSoundVol", PumpSoundVol);
        WriteValue(settingsNode, "CrewSoundVol", CrewSoundVol);

        // Config Settings
        WriteValue(settingsNode, "ShowDebugger", WindowDebugger.ShowWindow);
        WriteValue(settingsNode, "EnableBlizzyToolbar", EnableBlizzyToolbar);
        WriteValue(settingsNode, "VerboseLogging", VerboseLogging);
        WriteValue(settingsNode, "AutoSave", AutoSave);
        WriteValue(settingsNode, "SaveIntervalSec", SaveIntervalSec);
        WriteValue(settingsNode, "AutoDebug", AutoDebug);
        WriteValue(settingsNode, "DebugLogPath", DebugLogPath);
        WriteValue(settingsNode, "ErrorLogLength", ErrorLogLength);
        WriteValue(settingsNode, "SaveLogOnExit", SaveLogOnExit);
        WriteValue(settingsNode, "EnableKerbalRename", EnableKerbalRename);
        WriteValue(settingsNode, "EnableChangeProfession", EnableChangeProfession);
        WriteValue(settingsNode, "UseUnityStyle", UseUnityStyle);

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
          TransferDialogSpawner Tds = part.FindModuleImplementing<TransferDialogSpawner>();
          if (EnableStockCrewXfer)
          {
            if (Tds != null) continue;
            part.AddModule("TransferDialogSpawner");
            MonoUtilities.RefreshContextWindows(part);
          }
          else
          {
            if (Tds != null) part.RemoveModule(Tds);
          }
        }
      }
      catch (Exception)
      {
        // Do nothing.   We don't care if it fails when outside of the Flight Scene.
      }
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
        {"black", Color.black},
        {"blue", Color.blue},
        {"clea", Color.clear},
        {"cyan", Color.cyan},
        {"gray", Color.gray},
        {"green", Color.green},
        {"magenta", Color.magenta},
        {"red", Color.red},
        {"white", Color.white},
        {"yellow", Color.yellow},
        {"default", new Color(0.478f, 0.698f, 0.478f, 0.698f)}
      };
    }

    internal static void MemStoreTempSettings()
    {
      PrevRealismMode = RealismMode;
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
      PrevEnablePfCrews = EnablePfCrews;
      PrevEnablePfResources = EnablePfResources;
      PrevEnableCls = EnableCls;
      PrevEnableStockCrewXfer = EnableStockCrewXfer;
      PrevOverrideStockCrewXfer = OverrideStockCrewXfer;
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
      PrevModsToolTips = TabInstalledMods.ShowToolTips;

      //debugger Settings
      PrevErrorLogLength = ErrorLogLength;
    }

    internal static void MemRestoreTempSettings()
    {
      RealismMode = PrevRealismMode;
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
      EnablePfCrews = PrevEnablePfCrews;
      EnablePfResources = PrevEnablePfResources;
      EnableCls = PrevEnableCls;
      EnableStockCrewXfer = PrevEnableStockCrewXfer;
      OverrideStockCrewXfer = PrevOverrideStockCrewXfer;
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
      TabInstalledMods.ShowToolTips = PrevModsToolTips;

      //debugger Settings
      PrevErrorLogLength = ErrorLogLength;
    }

    #endregion
  }
}