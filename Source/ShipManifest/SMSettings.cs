using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ShipManifest.InternalObjects;
using ShipManifest.Windows;
using ShipManifest.Windows.Tabs.Control;
using ShipManifest.Windows.Tabs.Settings;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShipManifest
{
  // ReSharper disable once InconsistentNaming
  internal static class Orig
  {
    internal static bool VerboseLogging;
    internal static bool ShowDebugger;
    internal static string ErrorLogLength = "1000";
    internal static bool SaveLogOnExit = true;
    internal static bool AutoSave;
    internal static bool UseUnityStyle = true;
    internal static int SaveIntervalSec = 60;
    internal static int RealismMode = 3;
    internal static bool RealXfers;
    internal static bool RealCrewXfers;
    internal static bool RealControl;
    internal static bool LockSettings;
    internal static double FlowRate = 100;
    internal static double FlowCost = 0.0015;
    internal static double MaxFlowRate = 1000;
    internal static double MinFlowRate;
    internal static int MaxFlowTimeSec = 100;
    internal static bool EnableXferCost = true;
    internal static bool EnableHighlighting = true;
    internal static bool OnlySourceTarget;
    internal static bool EnableClsHighlighting = true;
    internal static bool EnableEdgeHighlighting = true;
    internal static bool EnableScience = true;
    internal static bool EnableCrew = true;
    internal static bool EnableCrewModify = true;
    internal static bool EnablePfCrews;
    internal static bool EnableStockCrewXfer = true;
    internal static bool OverrideStockCrewXfer = true;
    internal static bool EnableClsAllowTransfer = true;
    internal static bool EnablePfResources = true;
    internal static bool EnableCls = true;
    internal static bool EnableBlizzyToolbar;
    internal static string PumpSoundStart = "ShipManifest/Sounds/59328-1";
    internal static string PumpSoundRun = "ShipManifest/Sounds/59328-2";
    internal static string PumpSoundStop = "ShipManifest/Sounds/59328-3";
    internal static string CrewSoundStart = "ShipManifest/Sounds/14214-1";
    internal static string CrewSoundRun = "ShipManifest/Sounds/14214-2";
    internal static string CrewSoundStop = "ShipManifest/Sounds/14214-3";
    internal static double PumpSoundVol = 1;
    internal static double CrewSoundVol = 1;
    internal static bool ShowToolTips = true;
    internal static bool ManifestToolTips = true;
    internal static bool TransferToolTips = true;
    internal static bool SettingsToolTips = true;
    internal static bool RosterToolTips = true;
    internal static bool ControlToolTips = true;
    internal static bool HatchToolTips = true;
    internal static bool PanelToolTips = true;
    internal static bool AntennaToolTips = true;
    internal static bool LightToolTips = true;
    internal static bool RealismToolTips = true;
    internal static bool HighlightToolTips = true;
    internal static bool SoundsToolTips = true;
    internal static bool ToolTipsToolTips = true;
    internal static bool ConfigToolTips = true;
    internal static bool ModsToolTips = true;
    internal static bool DebuggerToolTips = true;
    internal static bool EnableKerbalRename = true;
    internal static bool EnableChangeProfession = true;
  }

  internal static class Curr
  {
    // UI Managed Settings
    // Realism Tab Feature Options
    internal static int RealismMode = 2;
    internal static bool RealXfers = true;
    internal static bool RealCrewXfers = true;
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
    // Tooltip Options
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
    internal static Rect DefaultPosition = new Rect(40,40,0,0);


    // Options unmanaged by UI.
    internal static string ResourcePartColor = "yellow";
    internal static string SourcePartColor = "red";
    internal static string TargetPartColor = "green";
    internal static string TargetPartCrewColor = "blue";
    internal static string ClsSpaceColor = "green";
    internal static string MouseOverColor = "burntorange";
    internal static double CrewXferDelaySec = 7;
    internal static int IvaUpdateFrameDelay = 10;


  }

  internal static class SMSettings
  {
    #region Properties

    internal static bool Loaded;

    internal static Dictionary<string, Color> Colors;

    internal static List<SMSuit> SmSuits = new List<SMSuit>();

    internal static ConfigNode Settings;

    private static readonly string SettingsPath =
      $"{KSPUtil.ApplicationRootPath}GameData/ShipManifest/Plugins/PluginData";

    private static readonly string SettingsFile = $"{SettingsPath}/SMSettings.dat";

    // This value is assigned from AssemblyInfo.cs
    internal static string CurVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

    // Persisted properties

    // Sound Tab Options
    // All Default sounds licensing is: CC-By-SA

    // Pump motor sound
    // http://www.freesound.org/people/vibe_crc/sounds/59328/

    // Bumping and scraping sounds...
    // http://www.freesound.org/people/adcbicycle/sounds/14214/

    // Minion like kerbal sounds...
    // http://www.freesound.org/people/yummie/sounds/



    // End Persisted Properties

    // Settings Window Option storage for Settings Window Cancel support


    // these values have no non prev counterpart.  Each window contains an EnableToolTip property 
    // that is directly assigned on load and directly retrieved on save

    // Internal properties for plugin management.  Not persisted, not user managed.

    internal static string DebugLogPath = @"Plugins\PluginData\";
    internal static bool ClsInstalled = false;

    #endregion

    #region Methods

    internal static ConfigNode LoadSettingsFile()
    {
      return Settings ?? (Settings = ConfigNode.Load(SettingsFile) ?? new ConfigNode());
    }

    internal static void LoadSuitsFromDatabase()
    {
      ConfigNode[] configNodes = GameDatabase.Instance.GetConfigNodes("SUITCOMBOS");
      foreach (ConfigNode configNode in configNodes)
      {
        var suitNodes = configNode.GetNodes("SUITCOMBO");
        foreach (ConfigNode suitNode in suitNodes)
        {
          // Load suit Node
          ConfigNode smSuitNode = new ConfigNode();
          if (suitNode.TryGetNode("IVA", ref smSuitNode))
          {                   
            SMSuit smSuit = new SMSuit();

            smSuitNode.TryGetValue("suitTexture", ref smSuit.suitTexture);
            smSuitNode.TryGetValue("normalTexture", ref smSuit.normalTexture);
            suitNode.TryGetValue("suitType", ref smSuit.suitType);
            suitNode.TryGetValue("gender", ref smSuit.gender);
            suitNode.TryGetValue("suitTexture", ref smSuit.suitPath);

            bool foundTexture = GameDatabase.Instance.ExistsTexture(smSuit.suitTexture);
            bool foundNormal = GameDatabase.Instance.ExistsTexture(smSuit.normalTexture);

            if (foundTexture && foundNormal)
              SmSuits.Add(smSuit);
            else if (!foundTexture && foundNormal)
              Debug.LogWarning($"[ShipManifest]: Could not find texture '{smSuit.suitTexture}'");
            else
              Debug.LogWarning($"[ShipManifest]: Could not find normal map '{smSuit.normalTexture}'");
          }
          else continue;
        } 
      }
    }

    internal static void LoadSettings()
    {
      LoadSuitsFromDatabase();
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
        Curr.DefaultPosition = GetRectangle(windowsNode, "DefaultPosition", Curr.DefaultPosition);
        WindowManifest.Position = GetRectangle(windowsNode, "ManifestPosition", WindowManifest.Position);
        WindowTransfer.Position = GetRectangle(windowsNode, "TransferPosition", WindowTransfer.Position);
        WindowDebugger.Position = GetRectangle(windowsNode, "DebuggerPosition", WindowDebugger.Position);
        WindowSettings.Position = GetRectangle(windowsNode, "SettingsPosition", WindowSettings.Position);
        WindowControl.Position = GetRectangle(windowsNode, "ControlPosition", WindowControl.Position);
        WindowRoster.Position = GetRectangle(windowsNode, "RosterPosition", WindowRoster.Position);

        // now the settings
        // Realism Settings
        Curr.RealismMode = realismNode.HasValue("RealismMode")
          ? int.Parse(realismNode.GetValue("RealismMode"))
          : Curr.RealismMode;
        Curr.RealXfers = realismNode.HasValue("RealXfers")
          ? bool.Parse(realismNode.GetValue("RealXfers"))
          : Curr.RealXfers;
        Curr.RealCrewXfers = realismNode.HasValue("RealCrewXfers")
          ? bool.Parse(realismNode.GetValue("RealCrewXfers"))
          : Curr.RealXfers;
        Curr.RealControl = realismNode.HasValue("RealControl")
          ? bool.Parse(realismNode.GetValue("RealControl"))
          : Curr.RealControl;
        Curr.EnableCrew = realismNode.HasValue("EnableCrew") 
          ? bool.Parse(realismNode.GetValue("EnableCrew")) 
          : Curr.EnableCrew;
        Curr.EnableKerbalRename = realismNode.HasValue("EnableKerbalRename")
          ? bool.Parse(realismNode.GetValue("EnableKerbalRename"))
          : Curr.EnableKerbalRename;
        Curr.EnableChangeProfession = realismNode.HasValue("EnableChangeProfession")
          ? bool.Parse(realismNode.GetValue("EnableChangeProfession"))
          : Curr.EnableChangeProfession;
        Curr.EnableCrewModify = realismNode.HasValue("EnableCrewModify") 
          ? bool.Parse(realismNode.GetValue("EnableCrewModify")) 
          : Curr.EnableCrewModify;
        Curr.EnablePfCrews = realismNode.HasValue("EnablePfCrews") 
          ? bool.Parse(realismNode.GetValue("EnablePfCrews")) 
          : Curr.EnablePfCrews;
        Curr.EnableStockCrewXfer = realismNode.HasValue("EnableStockCrewTransfer")
          ? bool.Parse(realismNode.GetValue("EnableStockCrewTransfer"))
          : Curr.EnableStockCrewXfer;

        Curr.EnableScience = realismNode.HasValue("EnableScience")
          ? bool.Parse(realismNode.GetValue("EnableScience"))
          : Curr.EnableScience;
        Curr.EnableResources = realismNode.HasValue("EnableResources")
          ? bool.Parse(realismNode.GetValue("EnableResources"))
          : Curr.EnableResources;
        Curr.EnablePfResources = realismNode.HasValue("EnablePFResources")
          ? bool.Parse(realismNode.GetValue("EnablePFResources"))
          : Curr.EnablePfResources;
        Curr.EnableCls = realismNode.HasValue("EnableCLS") 
          ? bool.Parse(realismNode.GetValue("EnableCLS")) 
          : Curr.EnableCls;
        Curr.OverrideStockCrewXfer = realismNode.HasValue("OverrideStockCrewTransfer")
          ? bool.Parse(realismNode.GetValue("OverrideStockCrewTransfer"))
          : Curr.OverrideStockCrewXfer;
        Curr.EnableClsAllowTransfer = realismNode.HasValue("EnableClsAllowTransfer")
          ? bool.Parse(realismNode.GetValue("EnableClsAllowTransfer"))
          : Curr.EnableClsAllowTransfer;
        Curr.FlowRate = realismNode.HasValue("FlowRate") 
          ? double.Parse(realismNode.GetValue("FlowRate")) 
          : Curr.FlowRate;
        Curr.FlowCost = realismNode.HasValue("FlowCost") 
          ? double.Parse(realismNode.GetValue("FlowCost")) 
          : Curr.FlowCost;
        Curr.MinFlowRate = realismNode.HasValue("MinFlowRate")
          ? double.Parse(realismNode.GetValue("MinFlowRate"))
          : Curr.MinFlowRate;
        Curr.MaxFlowRate = realismNode.HasValue("MaxFlowRate")
          ? double.Parse(realismNode.GetValue("MaxFlowRate"))
          : Curr.MaxFlowRate;
        Curr.MaxFlowTimeSec = realismNode.HasValue("MaxFlowTimeSec")
          ? int.Parse(realismNode.GetValue("MaxFlowTimeSec"))
          : Curr.MaxFlowTimeSec;
        Curr.EnableXferCost = realismNode.HasValue("EnableXferCost")
          ? bool.Parse(realismNode.GetValue("EnableXferCost"))
          : Curr.EnableXferCost;
        Curr.LockSettings = realismNode.HasValue("LockSettings")
          ? bool.Parse(realismNode.GetValue("LockSettings"))
          : Curr.LockSettings;

        // Highlighting settings
        Curr.EnableHighlighting = highlightNode.HasValue("EnableHighlighting")
          ? bool.Parse(highlightNode.GetValue("EnableHighlighting"))
          : Curr.EnableHighlighting;
        Curr.OnlySourceTarget = highlightNode.HasValue("OnlySourceTarget")
          ? bool.Parse(highlightNode.GetValue("OnlySourceTarget"))
          : Curr.OnlySourceTarget;
        Curr.EnableClsHighlighting = highlightNode.HasValue("EnableCLSHighlighting")
          ? bool.Parse(highlightNode.GetValue("EnableCLSHighlighting"))
          : Curr.EnableClsHighlighting;
        Curr.EnableEdgeHighlighting = highlightNode.HasValue("EnableEdgeHighlighting")
          ? bool.Parse(highlightNode.GetValue("EnableEdgeHighlighting"))
          : Curr.EnableClsHighlighting;
        Curr.ResourcePartColor = hiddenNode.HasValue("ResourcePartColor")
          ? hiddenNode.GetValue("ResourcePartColor")
          : Curr.ResourcePartColor;

        // ToolTip Settings
        Curr.ShowToolTips = toolTipsNode.HasValue("ShowToolTips")
          ? bool.Parse(toolTipsNode.GetValue("ShowToolTips"))
          : Curr.ShowToolTips;
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
        Curr.PumpSoundStart = soundsNode.HasValue("PumpSoundStart")
          ? soundsNode.GetValue("PumpSoundStart")
          : Curr.PumpSoundStart;
        Curr.PumpSoundRun = soundsNode.HasValue("PumpSoundRun") 
          ? soundsNode.GetValue("PumpSoundRun") 
          : Curr.PumpSoundRun;
        Curr.PumpSoundStop = soundsNode.HasValue("PumpSoundStop") 
          ? soundsNode.GetValue("PumpSoundStop") 
          : Curr.PumpSoundStop;
        Curr.CrewSoundStart = soundsNode.HasValue("CrewSoundStart")
          ? soundsNode.GetValue("CrewSoundStart")
          : Curr.CrewSoundStart;
        Curr.CrewSoundRun = soundsNode.HasValue("CrewSoundRun") 
          ? soundsNode.GetValue("CrewSoundRun") 
          : Curr.CrewSoundRun;
        Curr.CrewSoundStop = soundsNode.HasValue("CrewSoundStop") 
          ? soundsNode.GetValue("CrewSoundStop") 
          : Curr.CrewSoundStop;

        Curr.PumpSoundVol = soundsNode.HasValue("PumpSoundVol")
          ? double.Parse(soundsNode.GetValue("PumpSoundVol"))
          : Curr.PumpSoundVol;
        Curr.CrewSoundVol = soundsNode.HasValue("CrewSoundVol")
          ? double.Parse(soundsNode.GetValue("CrewSoundVol"))
          : Curr.CrewSoundVol;

        // Config Settings
        Curr.EnableBlizzyToolbar = configNode.HasValue("EnableBlizzyToolbar")
          ? bool.Parse(configNode.GetValue("EnableBlizzyToolbar"))
          : Curr.EnableBlizzyToolbar;
        WindowDebugger.ShowWindow = configNode.HasValue("ShowDebugger")
          ? bool.Parse(configNode.GetValue("ShowDebugger"))
          : WindowDebugger.ShowWindow;
        Curr.VerboseLogging = configNode.HasValue("VerboseLogging")
          ? bool.Parse(configNode.GetValue("VerboseLogging"))
          : Curr.VerboseLogging;
        Curr.AutoSave = configNode.HasValue("AutoSave") 
          ? bool.Parse(configNode.GetValue("AutoSave")) 
          : Curr.AutoSave;
        Curr.SaveIntervalSec = configNode.HasValue("SaveIntervalSec")
          ? int.Parse(configNode.GetValue("SaveIntervalSec"))
          : Curr.SaveIntervalSec;
        Curr.AutoDebug = configNode.HasValue("AutoDebug") 
          ? bool.Parse(configNode.GetValue("AutoDebug")) 
          : Curr.AutoDebug;
        DebugLogPath = configNode.HasValue("DebugLogPath") 
          ? configNode.GetValue("DebugLogPath") 
          : DebugLogPath;
        Curr.ErrorLogLength = configNode.HasValue("ErrorLogLength")
          ? configNode.GetValue("ErrorLogLength")
          : Curr.ErrorLogLength;
        Curr.SaveLogOnExit = configNode.HasValue("SaveLogOnExit")
          ? bool.Parse(configNode.GetValue("SaveLogOnExit"))
          : Curr.SaveLogOnExit;
        Curr.UseUnityStyle = configNode.HasValue("UseUnityStyle")
          ? bool.Parse(configNode.GetValue("UseUnityStyle"))
          : Curr.UseUnityStyle;

        // Hidden Settings
        // Hidden Highlighting
        Curr.SourcePartColor = hiddenNode.HasValue("SourcePartColor")
          ? hiddenNode.GetValue("SourcePartColor")
          : Curr.SourcePartColor;
        Curr.TargetPartColor = hiddenNode.HasValue("TargetPartColor")
          ? hiddenNode.GetValue("TargetPartColor")
          : Curr.TargetPartColor;
        Curr.TargetPartCrewColor = hiddenNode.HasValue("TargetPartCrewColor")
          ? hiddenNode.GetValue("TargetPartCrewColor")
          : Curr.TargetPartCrewColor;
        Curr.MouseOverColor = hiddenNode.HasValue("MouseOverColor") 
          ? hiddenNode.GetValue("MouseOverColor") 
          : Curr.MouseOverColor;

        // Hidden config
        Curr.CrewXferDelaySec = hiddenNode.HasValue("CrewXferDelaySec")
          ? double.Parse(hiddenNode.GetValue("CrewXferDelaySec"))
          : Curr.CrewXferDelaySec;
        Curr.IvaUpdateFrameDelay = hiddenNode.HasValue("IvaUpdateFrameDelay")
          ? int.Parse(hiddenNode.GetValue("IvaUpdateFrameDelay"))
          : Curr.IvaUpdateFrameDelay;
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
        if (Curr.EnableStockCrewXfer != Orig.EnableStockCrewXfer)
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
        WriteRectangle(windowsNode, "DefaultPosition", Curr.DefaultPosition);
        WriteRectangle(windowsNode, "ManifestPosition", WindowManifest.Position);
        WriteRectangle(windowsNode, "TransferPosition", WindowTransfer.Position);
        WriteRectangle(windowsNode, "DebuggerPosition", WindowDebugger.Position);
        WriteRectangle(windowsNode, "SettingsPosition", WindowSettings.Position);
        WriteRectangle(windowsNode, "ControlPosition", WindowControl.Position);
        WriteRectangle(windowsNode, "RosterPosition", WindowRoster.Position);

        //Write settings...
        // Realism Settings
        WriteValue(realismNode, "RealismMode", Curr.RealismMode);
        WriteValue(realismNode, "RealXfers", Curr.RealXfers);
        WriteValue(realismNode, "RealCrewXfers", Curr.RealCrewXfers);
        WriteValue(realismNode, "RealControl", Curr.RealControl);
        WriteValue(realismNode, "EnableCrew", Curr.EnableCrew);
        WriteValue(realismNode, "EnableCrewModify", Curr.EnableCrewModify);
        WriteValue(realismNode, "EnableKerbalRename", Curr.EnableKerbalRename);
        WriteValue(realismNode, "EnableChangeProfession", Curr.EnableChangeProfession);
        WriteValue(realismNode, "EnablePfCrews", Curr.EnablePfCrews);
        WriteValue(realismNode, "EnableStockCrewTransfer", Curr.EnableStockCrewXfer);
        WriteValue(realismNode, "EnableScience", Curr.EnableScience);
        WriteValue(realismNode, "EnableResources", Curr.EnableResources);
        WriteValue(realismNode, "EnablePFResources", Curr.EnablePfResources);
        WriteValue(realismNode, "EnableCLS", Curr.EnableCls);
        WriteValue(realismNode, "OverrideStockCrewTransfer", Curr.OverrideStockCrewXfer);
        WriteValue(realismNode, "EnableClsAllowTransfer", Curr.EnableClsAllowTransfer);
        WriteValue(realismNode, "FlowRate", Curr.FlowRate);
        WriteValue(realismNode, "FlowCost", Curr.FlowCost);
        WriteValue(realismNode, "MinFlowRate", Curr.MinFlowRate);
        WriteValue(realismNode, "MaxFlowRate", Curr.MaxFlowRate);
        WriteValue(realismNode, "MaxFlowTimeSec", Curr.MaxFlowTimeSec);
        WriteValue(realismNode, "EnableXferCost", Curr.EnableXferCost);
        WriteValue(realismNode, "LockSettings", Curr.LockSettings);

        // Highlighting Settings
        WriteValue(highlightNode, "EnableHighlighting", Curr.EnableHighlighting);
        WriteValue(highlightNode, "OnlySourceTarget", Curr.OnlySourceTarget);
        WriteValue(highlightNode, "EnableCLSHighlighting", Curr.EnableClsHighlighting);
        WriteValue(highlightNode, "EnableEdgeHighlighting", Curr.EnableEdgeHighlighting);

        // ToolTip Settings
        WriteValue(toolTipsNode, "ShowToolTips", Curr.ShowToolTips);
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
        WriteValue(soundsNode, "PumpSoundStart", Curr.PumpSoundStart);
        WriteValue(soundsNode, "PumpSoundRun", Curr.PumpSoundRun);
        WriteValue(soundsNode, "PumpSoundStop", Curr.PumpSoundStop);
        WriteValue(soundsNode, "CrewSoundStart", Curr.CrewSoundStart);
        WriteValue(soundsNode, "CrewSoundRun", Curr.CrewSoundRun);
        WriteValue(soundsNode, "CrewSoundStop", Curr.CrewSoundStop);
        WriteValue(soundsNode, "PumpSoundVol", Curr.PumpSoundVol);
        WriteValue(soundsNode, "CrewSoundVol", Curr.CrewSoundVol);

        // Config Settings
        WriteValue(configNode, "ShowDebugger", WindowDebugger.ShowWindow);
        WriteValue(configNode, "EnableBlizzyToolbar", Curr.EnableBlizzyToolbar);
        WriteValue(configNode, "VerboseLogging", Curr.VerboseLogging);
        WriteValue(configNode, "AutoSave", Curr.AutoSave);
        WriteValue(configNode, "SaveIntervalSec", Curr.SaveIntervalSec);
        WriteValue(configNode, "AutoDebug", Curr.AutoDebug);
        WriteValue(configNode, "DebugLogPath", DebugLogPath);
        WriteValue(configNode, "ErrorLogLength", Curr.ErrorLogLength);
        WriteValue(configNode, "SaveLogOnExit", Curr.SaveLogOnExit);
        WriteValue(configNode, "UseUnityStyle", Curr.UseUnityStyle);

        // Hidden Settings
        WriteValue(hiddenNode, "ResourcePartColor", Curr.ResourcePartColor);
        WriteValue(hiddenNode, "SourcePartColor", Curr.SourcePartColor);
        WriteValue(hiddenNode, "TargetPartColor", Curr.TargetPartColor);
        WriteValue(hiddenNode, "TargetPartCrewColor", Curr.TargetPartCrewColor);
        WriteValue(hiddenNode, "MouseOverColor", Curr.MouseOverColor);
        WriteValue(hiddenNode, "CrewXferDelaySec", Curr.CrewXferDelaySec);
        WriteValue(hiddenNode, "IvaUpdateFrameDelay", Curr.IvaUpdateFrameDelay);

        if (!Directory.Exists(SettingsPath))
          Directory.CreateDirectory(SettingsPath);
        Settings.Save(SettingsFile);
      }
    }

    internal static void SetRealismMode(int mode)
    {
      if (mode < 3) Curr.RealismMode = mode;
      switch (mode)
      {
        case 0: // Full
          Curr.RealXfers = true;
          Curr.RealCrewXfers = true;
          Curr.RealControl = true;
          Curr.EnableCrew = true;
          Curr.EnableKerbalRename = false;
          Curr.EnableChangeProfession = false;
          Curr.EnableCrewModify = false;
          Curr.EnableStockCrewXfer = true;
          Curr.OverrideStockCrewXfer = true;
          Curr.EnableClsAllowTransfer = true;
          Curr.EnableCls = ClsInstalled;
          Curr.EnableScience = true;
          Curr.EnableResources = true;
          Curr.EnablePfCrews = false;
          Curr.EnablePfResources = false;
          Curr.EnableXferCost = true;
          Curr.FlowCost = 0.0015;
          Curr.FlowRate = 100;
          Curr.MinFlowRate = 0;
          Curr.MaxFlowRate = 1000;
          Curr.Tolerance = 0.000001;
          Curr.MaxFlowTimeSec = 180;
          Curr.LockSettings = false;
          break;
        case 1: // None
          Curr.RealXfers = false;
          Curr.RealCrewXfers = false;
          Curr.RealControl = false;
          Curr.EnableCrew = true;
          Curr.EnableCrewModify = true;
          Curr.EnableKerbalRename = true;
          Curr.EnableChangeProfession = true;
          Curr.EnableStockCrewXfer = true;
          Curr.OverrideStockCrewXfer = false;
          Curr.EnableClsAllowTransfer = true;
          Curr.EnableCls = false;
          Curr.EnableScience = true;
          Curr.EnableResources = true;
          Curr.EnablePfCrews = false;
          Curr.EnablePfResources = true;
          Curr.EnableXferCost = false;
          Curr.FlowCost = 0.0015;
          Curr.FlowRate = 100;
          Curr.MinFlowRate = 0;
          Curr.MaxFlowRate = 1000;
          Curr.Tolerance = 0.000001;
          Curr.MaxFlowTimeSec = 180;
          Curr.LockSettings = false;
          break;
        case 2: // Default
          Curr.RealXfers = true;
          Curr.RealCrewXfers = true;
          Curr.RealControl = true;
          Curr.EnableCrew = true;
          Curr.EnableCrewModify = true;
          Curr.EnableKerbalRename = true;
          Curr.EnableChangeProfession = true;
          Curr.EnableStockCrewXfer = true;
          Curr.OverrideStockCrewXfer = true;
          Curr.EnableClsAllowTransfer = true;
          Curr.EnableCls = ClsInstalled;
          Curr.EnableScience = true;
          Curr.EnableResources = true;
          Curr.EnablePfCrews = false;
          Curr.EnablePfResources = true;
          Curr.EnableXferCost = true;
          Curr.FlowCost = 0.0015;
          Curr.FlowRate = 100;
          Curr.MinFlowRate = 0;
          Curr.MaxFlowRate = 1000;
          Curr.Tolerance = 0.000001;
          Curr.MaxFlowTimeSec = 180;
          Curr.LockSettings = false;
          break;
        case 3: // Custom  Do nothing.
          break;
      }
    }

    internal static int GetRealismMode()
    {
      if ( // Full
          Curr.RealXfers
          && Curr.RealCrewXfers
          && Curr.RealControl
          && Curr.EnableCrew
          && Curr.EnableKerbalRename == false
          && Curr.EnableChangeProfession == false
          && Curr.EnableCrewModify == false
          && Curr.EnableStockCrewXfer
          && Curr.OverrideStockCrewXfer
          && Curr.EnableClsAllowTransfer
          && Curr.EnableCls == ClsInstalled
          && Curr.EnableScience
          && Curr.EnableResources
          && Curr.EnablePfCrews == false
          && Curr.EnablePfResources == false
          && Curr.EnableXferCost
          && Math.Abs(Curr.FlowCost - 0.0015) < 0.0001f
          && Math.Abs(Curr.FlowRate - 100) < 0.0001f
          && Math.Abs(Curr.MinFlowRate) < 0.0001f
          && Math.Abs(Curr.MaxFlowRate - 1000) < 0.0001f
          && Math.Abs(Curr.Tolerance - 0.000001) < 0.0001f
          && Curr.MaxFlowTimeSec == 180
          && Curr.LockSettings == false)
        return 0;
      if ( // None
          Curr.RealXfers == false
          && Curr.RealCrewXfers == false
          && Curr.RealControl == false
          && Curr.EnableCrew
          && Curr.EnableCrewModify
          && Curr.EnableKerbalRename
          && Curr.EnableChangeProfession
          && Curr.EnableStockCrewXfer
          && Curr.OverrideStockCrewXfer == false
          && Curr.EnableClsAllowTransfer
          && Curr.EnableCls == false
          && Curr.EnableScience
          && Curr.EnableResources
          && Curr.EnablePfCrews == false
          && Curr.EnablePfResources
          && Curr.EnableXferCost == false
          && Curr.LockSettings == false)
        return 1;
      if ( // Default
          Curr.RealXfers
          && Curr.RealCrewXfers
          && Curr.RealControl
          && Curr.EnableCrew
          && Curr.EnableCrewModify
          && Curr.EnableKerbalRename
          && Curr.EnableChangeProfession
          && Curr.EnableStockCrewXfer
          && Curr.OverrideStockCrewXfer
          && Curr.EnableClsAllowTransfer
          && Curr.EnableCls == ClsInstalled
          && Curr.EnableScience
          && Curr.EnableResources
          && Curr.EnablePfCrews == false
          && Curr.EnablePfResources
          && Curr.EnableXferCost
          && Math.Abs(Curr.FlowCost - 0.0015) < 0.0001f
          && Math.Abs(Curr.FlowRate - 100) < 0.0001f
          && Math.Abs(Curr.MinFlowRate) < 0.0001f
          && Math.Abs(Curr.MaxFlowRate - 1000) < 0.0001f
          && Math.Abs(Curr.Tolerance - 0.000001) < 0.000001f
          && Curr.MaxFlowTimeSec == 180
          && Curr.LockSettings == false)
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
          part.crewTransferAvailable = Curr.EnableStockCrewXfer;
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
      if (!Curr.EnableCls || !HighLogic.LoadedSceneIsFlight || !Curr.EnableClsAllowTransfer) return;
      SMAddon.OrigClsAllowCrewXferSetting = SMAddon.ClsAddon.AllowUnrestrictedTransfers;
      SMAddon.ClsAddon.AllowUnrestrictedTransfers = true;
    }

    internal static void UpdateClsOverride()
    {
      if (!Curr.EnableCls || !HighLogic.LoadedSceneIsFlight) return;
      SMAddon.ClsAddon.AllowUnrestrictedTransfers = Curr.EnableClsAllowTransfer;
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
        {"burntorange", XKCDColors.BurntOrange},
        {"default", new Color(0.478f, 0.698f, 0.478f, 0.698f)}
      };
    }

    internal static void LoadCustomSuits()
    {

      // we can have multiple suit combos.  parse the list.

      // Populate Suit list
      // Use gamedatabase suitcombos.  this loads all stock and mod based combos.
      // Build suit combo list for display in Roster
    }

    internal static void MemStoreTempSettings()
    {
      Orig.RealismMode = Curr.RealismMode;
      Orig.RealXfers = Curr.RealXfers;
      Orig.RealCrewXfers = Curr.RealCrewXfers;
      Orig.RealControl = Curr.RealControl;
      Orig.ShowDebugger = WindowDebugger.ShowWindow;
      Orig.VerboseLogging = Curr.VerboseLogging;
      Orig.AutoSave = Curr.AutoSave;
      Orig.SaveIntervalSec = Curr.SaveIntervalSec;
      Orig.FlowRate = Curr.FlowRate;
      Orig.FlowCost = Curr.FlowCost;
      Orig.MinFlowRate = Curr.MinFlowRate;
      Orig.MaxFlowRate = Curr.MaxFlowRate;
      Orig.MaxFlowTimeSec = Curr.MaxFlowTimeSec;
      Orig.EnableXferCost = Curr.EnableXferCost;
      Orig.PumpSoundStart = Curr.PumpSoundStart;
      Orig.PumpSoundRun = Curr.PumpSoundRun;
      Orig.PumpSoundStop = Curr.PumpSoundStop;
      Orig.CrewSoundStart = Curr.CrewSoundStart;
      Orig.CrewSoundRun = Curr.CrewSoundRun;
      Orig.CrewSoundStop = Curr.CrewSoundStop;
      Orig.CrewSoundVol = Curr.CrewSoundVol;
      Orig.PumpSoundVol = Curr.PumpSoundVol;

      Orig.EnableScience = Curr.EnableScience;
      Orig.EnableHighlighting = Curr.EnableHighlighting;
      Orig.OnlySourceTarget = Curr.OnlySourceTarget;
      Orig.EnableClsHighlighting = Curr.EnableClsHighlighting;
      Orig.EnableCrew = Curr.EnableCrew;
      Orig.EnableCrewModify = Curr.EnableCrewModify;
      Orig.EnablePfCrews = Curr.EnablePfCrews;
      Orig.EnablePfResources = Curr.EnablePfResources;
      Orig.EnableCls = Curr.EnableCls;
      Orig.EnableStockCrewXfer = Curr.EnableStockCrewXfer;
      Orig.OverrideStockCrewXfer = Curr.OverrideStockCrewXfer;
      Orig.EnableClsAllowTransfer = Curr.EnableClsAllowTransfer;
      Orig.EnableKerbalRename = Curr.EnableKerbalRename;
      Orig.EnableChangeProfession = Curr.EnableChangeProfession;
      Orig.UseUnityStyle = Curr.UseUnityStyle;
      Orig.LockSettings = Curr.LockSettings;
      Orig.EnableBlizzyToolbar = Curr.EnableBlizzyToolbar;
      Orig.SaveLogOnExit = Curr.SaveLogOnExit;
      Orig.ShowToolTips = Curr.ShowToolTips;
      Orig.DebuggerToolTips = WindowDebugger.ShowToolTips;
      Orig.ManifestToolTips = WindowManifest.ShowToolTips;
      Orig.TransferToolTips = WindowTransfer.ShowToolTips;
      Orig.SettingsToolTips = WindowSettings.ShowToolTips;
      Orig.RosterToolTips = WindowRoster.ShowToolTips;
      Orig.ControlToolTips = WindowControl.ShowToolTips;
      Orig.HatchToolTips = TabHatch.ShowToolTips;
      Orig.PanelToolTips = TabSolarPanel.ShowToolTips;
      Orig.AntennaToolTips = TabAntenna.ShowToolTips;
      Orig.LightToolTips = TabLight.ShowToolTips;

      Orig.RealismToolTips = TabRealism.ShowToolTips;
      Orig.ToolTipsToolTips = TabToolTips.ShowToolTips;
      Orig.SoundsToolTips = TabSounds.ShowToolTips;
      Orig.HighlightToolTips = TabHighlight.ShowToolTips;
      Orig.ConfigToolTips = TabConfig.ShowToolTips;

      //debugger Settings
      Orig.ErrorLogLength = Curr.ErrorLogLength;
    }

    internal static void MemRestoreTempSettings()
    {
      Curr.RealismMode = Orig.RealismMode;
      Curr.RealXfers = Orig.RealXfers;
      Curr.RealCrewXfers = Orig.RealCrewXfers;
      Curr.RealControl = Orig.RealControl;
      WindowDebugger.ShowWindow = Orig.ShowDebugger;
      Curr.VerboseLogging = Orig.VerboseLogging;
      Curr.AutoSave = Orig.AutoSave;
      Curr.SaveIntervalSec = Orig.SaveIntervalSec;
      Curr.FlowRate = Orig.FlowRate;
      Curr.FlowCost = Orig.FlowCost;
      Curr.MinFlowRate = Orig.MinFlowRate;
      Curr.MaxFlowRate = Orig.MaxFlowRate;
      Curr.MaxFlowTimeSec = Orig.MaxFlowTimeSec;
      Curr.EnableXferCost = Orig.EnableXferCost;
      Curr.PumpSoundStart = Orig.PumpSoundStart;
      Curr.PumpSoundRun = Orig.PumpSoundRun;
      Curr.PumpSoundStop = Orig.PumpSoundStop;
      Curr.CrewSoundStart = Orig.CrewSoundStart;
      Curr.CrewSoundRun = Orig.CrewSoundRun;
      Curr.CrewSoundStop = Orig.CrewSoundStop;
      Curr.CrewSoundVol = Orig.CrewSoundVol;
      Curr.PumpSoundVol = Orig.PumpSoundVol;
      Curr.EnableScience = Orig.EnableScience;
      Curr.EnableHighlighting = Orig.EnableHighlighting;
      Curr.OnlySourceTarget = Orig.OnlySourceTarget;
      Curr.EnableClsHighlighting = Orig.EnableClsHighlighting;
      Curr.EnableCrew = Orig.EnableCrew;
      Curr.EnableCrewModify = Orig.EnableCrewModify;
      Curr.EnablePfCrews = Orig.EnablePfCrews;
      Curr.EnablePfResources = Orig.EnablePfResources;
      Curr.EnableCls = Orig.EnableCls;
      Curr.EnableStockCrewXfer = Orig.EnableStockCrewXfer;
      Curr.OverrideStockCrewXfer = Orig.OverrideStockCrewXfer;
      Curr.EnableClsAllowTransfer = Orig.EnableClsAllowTransfer;
      Curr.EnableKerbalRename = Orig.EnableKerbalRename;
      Curr.EnableChangeProfession = Orig.EnableChangeProfession;
      Curr.UseUnityStyle = Orig.UseUnityStyle;
      Curr.LockSettings = Orig.LockSettings;
      Curr.EnableBlizzyToolbar = Orig.EnableBlizzyToolbar;
      Curr.SaveLogOnExit = Orig.SaveLogOnExit;
      Curr.ShowToolTips = Orig.ShowToolTips;
      WindowDebugger.ShowToolTips = Orig.DebuggerToolTips;
      WindowManifest.ShowToolTips = Orig.ManifestToolTips;
      WindowTransfer.ShowToolTips = Orig.TransferToolTips;
      WindowSettings.ShowToolTips = Orig.SettingsToolTips;
      WindowRoster.ShowToolTips = Orig.RosterToolTips;
      WindowControl.ShowToolTips = Orig.ControlToolTips;
      TabHatch.ShowToolTips = Orig.HatchToolTips;
      TabSolarPanel.ShowToolTips = Orig.PanelToolTips;
      TabAntenna.ShowToolTips = Orig.AntennaToolTips;
      TabLight.ShowToolTips = Orig.LightToolTips;

      TabRealism.ShowToolTips = Orig.RealismToolTips;
      TabHighlight.ShowToolTips = Orig.HighlightToolTips;
      TabToolTips.ShowToolTips = Orig.ToolTipsToolTips;
      TabSounds.ShowToolTips = Orig.SoundsToolTips;
      TabConfig.ShowToolTips = Orig.ConfigToolTips;
      //TabInstalledMods.ShowToolTips = PrevModsToolTips;

      //debugger Settings
      Orig.ErrorLogLength = Curr.ErrorLogLength;
    }

    #endregion

  }

}
