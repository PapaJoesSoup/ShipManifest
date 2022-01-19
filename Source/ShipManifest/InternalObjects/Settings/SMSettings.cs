using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ShipManifest.Modules;
using ShipManifest.Windows;
using ShipManifest.Windows.Popups;
using ShipManifest.Windows.Tabs.Control;
using ShipManifest.Windows.Tabs.Settings;
using UnityEngine;

namespace ShipManifest.InternalObjects.Settings

{
  // ReSharper disable once InconsistentNaming

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
        CurrSettings.DefaultPosition = GetRectangle(windowsNode, "DefaultPosition", CurrSettings.DefaultPosition);
        WindowManifest.Position = GetRectangle(windowsNode, "ManifestPosition", WindowManifest.Position);
        WindowTransfer.Position = GetRectangle(windowsNode, "TransferPosition", WindowTransfer.Position);
        WindowDebugger.Position = GetRectangle(windowsNode, "DebuggerPosition", WindowDebugger.Position);
        WindowSettings.Position = GetRectangle(windowsNode, "SettingsPosition", WindowSettings.Position);
        WindowControl.Position = GetRectangle(windowsNode, "ControlPosition", WindowControl.Position);
        WindowRoster.Position = GetRectangle(windowsNode, "RosterPosition", WindowRoster.Position);

        // Lets ge our window scaling values
        WindowManifest.HeightScale = windowsNode.HasValue("ManifestHeightScale")
          ? float.Parse(windowsNode.GetValue("ManifestHeightScale"))
          : WindowManifest.HeightScale;
        WindowTransfer.HeightScale = windowsNode.HasValue("TransferHeightScale")
          ? float.Parse(windowsNode.GetValue("TransferHeightScale"))
          : WindowTransfer.HeightScale;
        WindowRoster.HeightScale = windowsNode.HasValue("RosterHeightScale")
          ? float.Parse(windowsNode.GetValue("RosterHeightScale"))
          : WindowRoster.HeightScale;
        WindowControl.HeightScale = windowsNode.HasValue("ControlHeightScale")
          ? float.Parse(windowsNode.GetValue("ControlHeightScale"))
          : WindowControl.HeightScale;
        WindowDebugger.HeightScale = windowsNode.HasValue("DebugHeightScale")
          ? float.Parse(windowsNode.GetValue("DebugHeightScale"))
          : WindowDebugger.HeightScale;
        WindowSettings.HeightScale = windowsNode.HasValue("SettingsHeightScale")
          ? float.Parse(windowsNode.GetValue("SettingsHeightScale"))
          : WindowSettings.HeightScale;

        // Realism Settings
        CurrSettings.RealismMode = realismNode.HasValue("RealismMode")
          ? int.Parse(realismNode.GetValue("RealismMode"))
          : CurrSettings.RealismMode;
        CurrSettings.RealXfers = realismNode.HasValue("RealXfers")
          ? bool.Parse(realismNode.GetValue("RealXfers"))
          : CurrSettings.RealXfers;
        CurrSettings.RealCrewXfers = realismNode.HasValue("RealCrewXfers")
          ? bool.Parse(realismNode.GetValue("RealCrewXfers"))
          : CurrSettings.RealXfers;
        CurrSettings.RealControl = realismNode.HasValue("RealControl")
          ? bool.Parse(realismNode.GetValue("RealControl"))
          : CurrSettings.RealControl;
        CurrSettings.EnableCrew = realismNode.HasValue("EnableCrew")
          ? bool.Parse(realismNode.GetValue("EnableCrew"))
          : CurrSettings.EnableCrew;
        CurrSettings.EnableKerbalRename = realismNode.HasValue("EnableKerbalRename")
          ? bool.Parse(realismNode.GetValue("EnableKerbalRename"))
          : CurrSettings.EnableKerbalRename;
        CurrSettings.EnableChangeProfession = realismNode.HasValue("EnableChangeProfession")
          ? bool.Parse(realismNode.GetValue("EnableChangeProfession"))
          : CurrSettings.EnableChangeProfession;
        CurrSettings.EnableCrewModify = realismNode.HasValue("EnableCrewModify")
          ? bool.Parse(realismNode.GetValue("EnableCrewModify"))
          : CurrSettings.EnableCrewModify;
        CurrSettings.EnablePfCrews = realismNode.HasValue("EnablePfCrews")
          ? bool.Parse(realismNode.GetValue("EnablePfCrews"))
          : CurrSettings.EnablePfCrews;
        CurrSettings.EnableStockCrewXfer = realismNode.HasValue("EnableStockCrewTransfer")
          ? bool.Parse(realismNode.GetValue("EnableStockCrewTransfer"))
          : CurrSettings.EnableStockCrewXfer;

        CurrSettings.EnableScience = realismNode.HasValue("EnableScience")
          ? bool.Parse(realismNode.GetValue("EnableScience"))
          : CurrSettings.EnableScience;
        CurrSettings.EnableResources = realismNode.HasValue("EnableResources")
          ? bool.Parse(realismNode.GetValue("EnableResources"))
          : CurrSettings.EnableResources;
        CurrSettings.EnablePfResources = realismNode.HasValue("EnablePFResources")
          ? bool.Parse(realismNode.GetValue("EnablePFResources"))
          : CurrSettings.EnablePfResources;
        CurrSettings.EnableCls = realismNode.HasValue("EnableCLS")
          ? bool.Parse(realismNode.GetValue("EnableCLS"))
          : CurrSettings.EnableCls;
        CurrSettings.OverrideStockCrewXfer = realismNode.HasValue("OverrideStockCrewTransfer")
          ? bool.Parse(realismNode.GetValue("OverrideStockCrewTransfer"))
          : CurrSettings.OverrideStockCrewXfer;
        CurrSettings.EnableClsAllowTransfer = realismNode.HasValue("EnableClsAllowTransfer")
          ? bool.Parse(realismNode.GetValue("EnableClsAllowTransfer"))
          : CurrSettings.EnableClsAllowTransfer;
        CurrSettings.FlowRate = realismNode.HasValue("FlowRate")
          ? double.Parse(realismNode.GetValue("FlowRate"))
          : CurrSettings.FlowRate;
        CurrSettings.FlowCost = realismNode.HasValue("FlowCost")
          ? double.Parse(realismNode.GetValue("FlowCost"))
          : CurrSettings.FlowCost;
        CurrSettings.MinFlowRate = realismNode.HasValue("MinFlowRate")
          ? double.Parse(realismNode.GetValue("MinFlowRate"))
          : CurrSettings.MinFlowRate;
        CurrSettings.MaxFlowRate = realismNode.HasValue("MaxFlowRate")
          ? double.Parse(realismNode.GetValue("MaxFlowRate"))
          : CurrSettings.MaxFlowRate;
        CurrSettings.MaxFlowTimeSec = realismNode.HasValue("MaxFlowTimeSec")
          ? int.Parse(realismNode.GetValue("MaxFlowTimeSec"))
          : CurrSettings.MaxFlowTimeSec;
        CurrSettings.EnableXferCost = realismNode.HasValue("EnableXferCost")
          ? bool.Parse(realismNode.GetValue("EnableXferCost"))
          : CurrSettings.EnableXferCost;
        CurrSettings.LockSettings = realismNode.HasValue("LockSettings")
          ? bool.Parse(realismNode.GetValue("LockSettings"))
          : CurrSettings.LockSettings;

        // Highlighting settings
        CurrSettings.EnableHighlighting = highlightNode.HasValue("EnableHighlighting")
          ? bool.Parse(highlightNode.GetValue("EnableHighlighting"))
          : CurrSettings.EnableHighlighting;
        CurrSettings.OnlySourceTarget = highlightNode.HasValue("OnlySourceTarget")
          ? bool.Parse(highlightNode.GetValue("OnlySourceTarget"))
          : CurrSettings.OnlySourceTarget;
        CurrSettings.EnableClsHighlighting = highlightNode.HasValue("EnableCLSHighlighting")
          ? bool.Parse(highlightNode.GetValue("EnableCLSHighlighting"))
          : CurrSettings.EnableClsHighlighting;
        CurrSettings.EnableEdgeHighlighting = highlightNode.HasValue("EnableEdgeHighlighting")
          ? bool.Parse(highlightNode.GetValue("EnableEdgeHighlighting"))
          : CurrSettings.EnableClsHighlighting;
        CurrSettings.ResourcePartColor = hiddenNode.HasValue("ResourcePartColor")
          ? hiddenNode.GetValue("ResourcePartColor")
          : CurrSettings.ResourcePartColor;

        // ToolTip Settings
        CurrSettings.ShowToolTips = toolTipsNode.HasValue("ShowToolTips")
          ? bool.Parse(toolTipsNode.GetValue("ShowToolTips"))
          : CurrSettings.ShowToolTips;
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
        PopupSmBtnHover.ShowToolTips = toolTipsNode.HasValue("AppIconToolTips")
          ? bool.Parse(toolTipsNode.GetValue("AppIconToolTips"))
          : PopupSmBtnHover.ShowToolTips;

        // Sounds Settings
        CurrSettings.PumpSoundStart = soundsNode.HasValue("PumpSoundStart")
          ? soundsNode.GetValue("PumpSoundStart")
          : CurrSettings.PumpSoundStart;
        CurrSettings.PumpSoundRun = soundsNode.HasValue("PumpSoundRun")
          ? soundsNode.GetValue("PumpSoundRun")
          : CurrSettings.PumpSoundRun;
        CurrSettings.PumpSoundStop = soundsNode.HasValue("PumpSoundStop")
          ? soundsNode.GetValue("PumpSoundStop")
          : CurrSettings.PumpSoundStop;
        CurrSettings.CrewSoundStart = soundsNode.HasValue("CrewSoundStart")
          ? soundsNode.GetValue("CrewSoundStart")
          : CurrSettings.CrewSoundStart;
        CurrSettings.CrewSoundRun = soundsNode.HasValue("CrewSoundRun")
          ? soundsNode.GetValue("CrewSoundRun")
          : CurrSettings.CrewSoundRun;
        CurrSettings.CrewSoundStop = soundsNode.HasValue("CrewSoundStop")
          ? soundsNode.GetValue("CrewSoundStop")
          : CurrSettings.CrewSoundStop;

        CurrSettings.PumpSoundVol = soundsNode.HasValue("PumpSoundVol")
          ? double.Parse(soundsNode.GetValue("PumpSoundVol"))
          : CurrSettings.PumpSoundVol;
        CurrSettings.CrewSoundVol = soundsNode.HasValue("CrewSoundVol")
          ? double.Parse(soundsNode.GetValue("CrewSoundVol"))
          : CurrSettings.CrewSoundVol;

        // Config Settings
        CurrSettings.EnableBlizzyToolbar = configNode.HasValue("EnableBlizzyToolbar")
          ? bool.Parse(configNode.GetValue("EnableBlizzyToolbar"))
          : CurrSettings.EnableBlizzyToolbar;
        WindowDebugger.ShowWindow = configNode.HasValue("ShowDebugger")
          ? bool.Parse(configNode.GetValue("ShowDebugger"))
          : WindowDebugger.ShowWindow;
        CurrSettings.VerboseLogging = configNode.HasValue("VerboseLogging")
          ? bool.Parse(configNode.GetValue("VerboseLogging"))
          : CurrSettings.VerboseLogging;
        CurrSettings.AutoSave = configNode.HasValue("AutoSave")
          ? bool.Parse(configNode.GetValue("AutoSave"))
          : CurrSettings.AutoSave;
        CurrSettings.SaveIntervalSec = configNode.HasValue("SaveIntervalSec")
          ? int.Parse(configNode.GetValue("SaveIntervalSec"))
          : CurrSettings.SaveIntervalSec;
        CurrSettings.AutoDebug = configNode.HasValue("AutoDebug")
          ? bool.Parse(configNode.GetValue("AutoDebug"))
          : CurrSettings.AutoDebug;
        DebugLogPath = configNode.HasValue("DebugLogPath")
          ? configNode.GetValue("DebugLogPath")
          : DebugLogPath;
        CurrSettings.ErrorLogLength = configNode.HasValue("ErrorLogLength")
          ? configNode.GetValue("ErrorLogLength")
          : CurrSettings.ErrorLogLength;
        CurrSettings.SaveLogOnExit = configNode.HasValue("SaveLogOnExit")
          ? bool.Parse(configNode.GetValue("SaveLogOnExit"))
          : CurrSettings.SaveLogOnExit;
        CurrSettings.UseUnityStyle = configNode.HasValue("UseUnityStyle")
          ? bool.Parse(configNode.GetValue("UseUnityStyle"))
          : CurrSettings.UseUnityStyle;

        // Hidden Settings
        // Hidden Highlighting
        CurrSettings.SourcePartColor = hiddenNode.HasValue("SourcePartColor")
          ? hiddenNode.GetValue("SourcePartColor")
          : CurrSettings.SourcePartColor;
        CurrSettings.TargetPartColor = hiddenNode.HasValue("TargetPartColor")
          ? hiddenNode.GetValue("TargetPartColor")
          : CurrSettings.TargetPartColor;
        CurrSettings.TargetPartCrewColor = hiddenNode.HasValue("TargetPartCrewColor")
          ? hiddenNode.GetValue("TargetPartCrewColor")
          : CurrSettings.TargetPartCrewColor;
        CurrSettings.MouseOverColor = hiddenNode.HasValue("MouseOverColor")
          ? hiddenNode.GetValue("MouseOverColor")
          : CurrSettings.MouseOverColor;

        // Hidden config
        CurrSettings.CrewXferDelaySec = hiddenNode.HasValue("CrewXferDelaySec")
          ? double.Parse(hiddenNode.GetValue("CrewXferDelaySec"))
          : CurrSettings.CrewXferDelaySec;
        CurrSettings.IvaUpdateFrameDelay = hiddenNode.HasValue("IvaUpdateFrameDelay")
          ? int.Parse(hiddenNode.GetValue("IvaUpdateFrameDelay"))
          : CurrSettings.IvaUpdateFrameDelay;
        // Okay, set the Settings loaded flag
        Loaded = true;
        MemStoreTempSettings();
      }

      // Enable/Disable crewed parts CrewTransferDialog 
      SetStockCrewTransferState();

      // Force Styles to refresh/load.
      SMStyle.WindowStyle = null;

      // Lets make sure that the windows can be seen on the screen. (supports different resolutions)
      GuiUtils.RepositionWindows();
    }

    internal static void SaveSettings()
    {
      if (!Loaded || (HighLogic.LoadedScene != GameScenes.FLIGHT &&
                      HighLogic.LoadedScene != GameScenes.SPACECENTER)) return;
      if (CurrSettings.EnableStockCrewXfer != OrigSettings.EnableStockCrewXfer)
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
      WriteRectangle(windowsNode, "DefaultPosition", CurrSettings.DefaultPosition);
      WriteRectangle(windowsNode, "ManifestPosition", WindowManifest.Position);
      WriteRectangle(windowsNode, "TransferPosition", WindowTransfer.Position);
      WriteRectangle(windowsNode, "DebuggerPosition", WindowDebugger.Position);
      WriteRectangle(windowsNode, "SettingsPosition", WindowSettings.Position);
      WriteRectangle(windowsNode, "ControlPosition", WindowControl.Position);
      WriteRectangle(windowsNode, "RosterPosition", WindowRoster.Position);

      WriteValue(windowsNode, "ManifestHeightScale", WindowManifest.HeightScale);
      WriteValue(windowsNode, "TransferHeightScale", WindowTransfer.HeightScale);
      WriteValue(windowsNode, "RosterHeightScale", WindowRoster.HeightScale);
      WriteValue(windowsNode, "ControlHeightScale", WindowControl.HeightScale);
      WriteValue(windowsNode, "DebugHeightScale", WindowDebugger.HeightScale);
      WriteValue(windowsNode, "SettingsHeightScale", WindowSettings.HeightScale);


      //Write settings...
      // Realism Settings
      WriteValue(realismNode, "RealismMode", CurrSettings.RealismMode);
      WriteValue(realismNode, "RealXfers", CurrSettings.RealXfers);
      WriteValue(realismNode, "RealCrewXfers", CurrSettings.RealCrewXfers);
      WriteValue(realismNode, "RealControl", CurrSettings.RealControl);
      WriteValue(realismNode, "EnableCrew", CurrSettings.EnableCrew);
      WriteValue(realismNode, "EnableCrewModify", CurrSettings.EnableCrewModify);
      WriteValue(realismNode, "EnableKerbalRename", CurrSettings.EnableKerbalRename);
      WriteValue(realismNode, "EnableChangeProfession", CurrSettings.EnableChangeProfession);
      WriteValue(realismNode, "EnablePfCrews", CurrSettings.EnablePfCrews);
      WriteValue(realismNode, "EnableStockCrewTransfer", CurrSettings.EnableStockCrewXfer);
      WriteValue(realismNode, "EnableScience", CurrSettings.EnableScience);
      WriteValue(realismNode, "EnableResources", CurrSettings.EnableResources);
      WriteValue(realismNode, "EnablePFResources", CurrSettings.EnablePfResources);
      WriteValue(realismNode, "EnableCLS", CurrSettings.EnableCls);
      WriteValue(realismNode, "OverrideStockCrewTransfer", CurrSettings.OverrideStockCrewXfer);
      WriteValue(realismNode, "EnableClsAllowTransfer", CurrSettings.EnableClsAllowTransfer);
      WriteValue(realismNode, "FlowRate", CurrSettings.FlowRate);
      WriteValue(realismNode, "FlowCost", CurrSettings.FlowCost);
      WriteValue(realismNode, "MinFlowRate", CurrSettings.MinFlowRate);
      WriteValue(realismNode, "MaxFlowRate", CurrSettings.MaxFlowRate);
      WriteValue(realismNode, "MaxFlowTimeSec", CurrSettings.MaxFlowTimeSec);
      WriteValue(realismNode, "EnableXferCost", CurrSettings.EnableXferCost);
      WriteValue(realismNode, "LockSettings", CurrSettings.LockSettings);

      // Highlighting Settings
      WriteValue(highlightNode, "EnableHighlighting", CurrSettings.EnableHighlighting);
      WriteValue(highlightNode, "OnlySourceTarget", CurrSettings.OnlySourceTarget);
      WriteValue(highlightNode, "EnableCLSHighlighting", CurrSettings.EnableClsHighlighting);
      WriteValue(highlightNode, "EnableEdgeHighlighting", CurrSettings.EnableEdgeHighlighting);

      // ToolTip Settings
      WriteValue(toolTipsNode, "ShowToolTips", CurrSettings.ShowToolTips);
      WriteValue(toolTipsNode, "AppIconToolTips", PopupSmBtnHover.ShowToolTips);
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
      WriteValue(soundsNode, "PumpSoundStart", CurrSettings.PumpSoundStart);
      WriteValue(soundsNode, "PumpSoundRun", CurrSettings.PumpSoundRun);
      WriteValue(soundsNode, "PumpSoundStop", CurrSettings.PumpSoundStop);
      WriteValue(soundsNode, "CrewSoundStart", CurrSettings.CrewSoundStart);
      WriteValue(soundsNode, "CrewSoundRun", CurrSettings.CrewSoundRun);
      WriteValue(soundsNode, "CrewSoundStop", CurrSettings.CrewSoundStop);
      WriteValue(soundsNode, "PumpSoundVol", CurrSettings.PumpSoundVol);
      WriteValue(soundsNode, "CrewSoundVol", CurrSettings.CrewSoundVol);

      // Config Settings
      WriteValue(configNode, "ShowDebugger", WindowDebugger.ShowWindow);
      WriteValue(configNode, "EnableBlizzyToolbar", CurrSettings.EnableBlizzyToolbar);
      WriteValue(configNode, "VerboseLogging", CurrSettings.VerboseLogging);
      WriteValue(configNode, "AutoSave", CurrSettings.AutoSave);
      WriteValue(configNode, "SaveIntervalSec", CurrSettings.SaveIntervalSec);
      WriteValue(configNode, "AutoDebug", CurrSettings.AutoDebug);
      WriteValue(configNode, "DebugLogPath", DebugLogPath);
      WriteValue(configNode, "ErrorLogLength", CurrSettings.ErrorLogLength);
      WriteValue(configNode, "SaveLogOnExit", CurrSettings.SaveLogOnExit);
      WriteValue(configNode, "UseUnityStyle", CurrSettings.UseUnityStyle);

      // Hidden Settings
      WriteValue(hiddenNode, "ResourcePartColor", CurrSettings.ResourcePartColor);
      WriteValue(hiddenNode, "SourcePartColor", CurrSettings.SourcePartColor);
      WriteValue(hiddenNode, "TargetPartColor", CurrSettings.TargetPartColor);
      WriteValue(hiddenNode, "TargetPartCrewColor", CurrSettings.TargetPartCrewColor);
      WriteValue(hiddenNode, "MouseOverColor", CurrSettings.MouseOverColor);
      WriteValue(hiddenNode, "CrewXferDelaySec", CurrSettings.CrewXferDelaySec);
      WriteValue(hiddenNode, "IvaUpdateFrameDelay", CurrSettings.IvaUpdateFrameDelay);

      if (!Directory.Exists(SettingsPath))
        Directory.CreateDirectory(SettingsPath);
      Settings.Save(SettingsFile);
    }

    internal static void SetRealismMode(int mode)
    {
      if (mode < 3) CurrSettings.RealismMode = mode;
      switch (mode)
      {
        case 0: // Full
          CurrSettings.RealXfers = true;
          CurrSettings.RealCrewXfers = true;
          CurrSettings.RealControl = true;
          CurrSettings.EnableCrew = true;
          CurrSettings.EnableKerbalRename = false;
          CurrSettings.EnableChangeProfession = false;
          CurrSettings.EnableCrewModify = false;
          CurrSettings.EnableStockCrewXfer = true;
          CurrSettings.OverrideStockCrewXfer = true;
          CurrSettings.EnableClsAllowTransfer = true;
          CurrSettings.EnableCls = ClsInstalled;
          CurrSettings.EnableScience = true;
          CurrSettings.EnableResources = true;
          CurrSettings.EnablePfCrews = false;
          CurrSettings.EnablePfResources = false;
          CurrSettings.EnableXferCost = true;
          CurrSettings.FlowCost = 0.0015;
          CurrSettings.FlowRate = 100;
          CurrSettings.MinFlowRate = 0;
          CurrSettings.MaxFlowRate = 1000;
          CurrSettings.Tolerance = 0.000001;
          CurrSettings.MaxFlowTimeSec = 180;
          CurrSettings.LockSettings = false;
          break;
        case 1: // None
          CurrSettings.RealXfers = false;
          CurrSettings.RealCrewXfers = false;
          CurrSettings.RealControl = false;
          CurrSettings.EnableCrew = true;
          CurrSettings.EnableCrewModify = true;
          CurrSettings.EnableKerbalRename = true;
          CurrSettings.EnableChangeProfession = true;
          CurrSettings.EnableStockCrewXfer = true;
          CurrSettings.OverrideStockCrewXfer = false;
          CurrSettings.EnableClsAllowTransfer = true;
          CurrSettings.EnableCls = false;
          CurrSettings.EnableScience = true;
          CurrSettings.EnableResources = true;
          CurrSettings.EnablePfCrews = false;
          CurrSettings.EnablePfResources = true;
          CurrSettings.EnableXferCost = false;
          CurrSettings.FlowCost = 0.0015;
          CurrSettings.FlowRate = 100;
          CurrSettings.MinFlowRate = 0;
          CurrSettings.MaxFlowRate = 1000;
          CurrSettings.Tolerance = 0.000001;
          CurrSettings.MaxFlowTimeSec = 180;
          CurrSettings.LockSettings = false;
          break;
        case 2: // Default
          CurrSettings.RealXfers = true;
          CurrSettings.RealCrewXfers = true;
          CurrSettings.RealControl = true;
          CurrSettings.EnableCrew = true;
          CurrSettings.EnableCrewModify = true;
          CurrSettings.EnableKerbalRename = true;
          CurrSettings.EnableChangeProfession = true;
          CurrSettings.EnableStockCrewXfer = true;
          CurrSettings.OverrideStockCrewXfer = true;
          CurrSettings.EnableClsAllowTransfer = true;
          CurrSettings.EnableCls = ClsInstalled;
          CurrSettings.EnableScience = true;
          CurrSettings.EnableResources = true;
          CurrSettings.EnablePfCrews = false;
          CurrSettings.EnablePfResources = true;
          CurrSettings.EnableXferCost = true;
          CurrSettings.FlowCost = 0.0015;
          CurrSettings.FlowRate = 100;
          CurrSettings.MinFlowRate = 0;
          CurrSettings.MaxFlowRate = 1000;
          CurrSettings.Tolerance = 0.000001;
          CurrSettings.MaxFlowTimeSec = 180;
          CurrSettings.LockSettings = false;
          break;
        case 3: // Custom  Do nothing.
          break;
      }
    }

    internal static int GetRealismMode()
    {
      if ( // Full
          CurrSettings.RealXfers
          && CurrSettings.RealCrewXfers
          && CurrSettings.RealControl
          && CurrSettings.EnableCrew
          && CurrSettings.EnableKerbalRename == false
          && CurrSettings.EnableChangeProfession == false
          && CurrSettings.EnableCrewModify == false
          && CurrSettings.EnableStockCrewXfer
          && CurrSettings.OverrideStockCrewXfer
          && CurrSettings.EnableClsAllowTransfer
          && CurrSettings.EnableCls == ClsInstalled
          && CurrSettings.EnableScience
          && CurrSettings.EnableResources
          && CurrSettings.EnablePfCrews == false
          && CurrSettings.EnablePfResources == false
          && CurrSettings.EnableXferCost
          && Math.Abs(CurrSettings.FlowCost - 0.0015) < 0.0001f
          && Math.Abs(CurrSettings.FlowRate - 100) < 0.0001f
          && Math.Abs(CurrSettings.MinFlowRate) < 0.0001f
          && Math.Abs(CurrSettings.MaxFlowRate - 1000) < 0.0001f
          && Math.Abs(CurrSettings.Tolerance - 0.000001) < 0.0001f
          && CurrSettings.MaxFlowTimeSec == 180
          && CurrSettings.LockSettings == false)
        return 0;
      if ( // None
          CurrSettings.RealXfers == false
          && CurrSettings.RealCrewXfers == false
          && CurrSettings.RealControl == false
          && CurrSettings.EnableCrew
          && CurrSettings.EnableCrewModify
          && CurrSettings.EnableKerbalRename
          && CurrSettings.EnableChangeProfession
          && CurrSettings.EnableStockCrewXfer
          && CurrSettings.OverrideStockCrewXfer == false
          && CurrSettings.EnableClsAllowTransfer
          && CurrSettings.EnableCls == false
          && CurrSettings.EnableScience
          && CurrSettings.EnableResources
          && CurrSettings.EnablePfCrews == false
          && CurrSettings.EnablePfResources
          && CurrSettings.EnableXferCost == false
          && CurrSettings.LockSettings == false)
        return 1;
      if ( // Default
          CurrSettings.RealXfers
          && CurrSettings.RealCrewXfers
          && CurrSettings.RealControl
          && CurrSettings.EnableCrew
          && CurrSettings.EnableCrewModify
          && CurrSettings.EnableKerbalRename
          && CurrSettings.EnableChangeProfession
          && CurrSettings.EnableStockCrewXfer
          && CurrSettings.OverrideStockCrewXfer
          && CurrSettings.EnableClsAllowTransfer
          && CurrSettings.EnableCls == ClsInstalled
          && CurrSettings.EnableScience
          && CurrSettings.EnableResources
          && CurrSettings.EnablePfCrews == false
          && CurrSettings.EnablePfResources
          && CurrSettings.EnableXferCost
          && Math.Abs(CurrSettings.FlowCost - 0.0015) < 0.0001f
          && Math.Abs(CurrSettings.FlowRate - 100) < 0.0001f
          && Math.Abs(CurrSettings.MinFlowRate) < 0.0001f
          && Math.Abs(CurrSettings.MaxFlowRate - 1000) < 0.0001f
          && Math.Abs(CurrSettings.Tolerance - 0.000001) < 0.000001f
          && CurrSettings.MaxFlowTimeSec == 180
          && CurrSettings.LockSettings == false)
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
          part.crewTransferAvailable = CurrSettings.EnableStockCrewXfer;
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
      if (!CurrSettings.EnableCls || !HighLogic.LoadedSceneIsFlight || !CurrSettings.EnableClsAllowTransfer) return;
      SMAddon.OrigClsAllowCrewXferSetting = SMAddon.ClsAddon.AllowUnrestrictedTransfers;
      SMAddon.ClsAddon.AllowUnrestrictedTransfers = true;
    }

    internal static void UpdateClsOverride()
    {
      if (!CurrSettings.EnableCls || !HighLogic.LoadedSceneIsFlight) return;
      SMAddon.ClsAddon.AllowUnrestrictedTransfers = CurrSettings.EnableClsAllowTransfer;
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

    internal static void MemStoreTempSettings()
    {
      OrigSettings.RealismMode = CurrSettings.RealismMode;
      OrigSettings.RealXfers = CurrSettings.RealXfers;
      OrigSettings.RealCrewXfers = CurrSettings.RealCrewXfers;
      OrigSettings.RealControl = CurrSettings.RealControl;
      OrigSettings.ShowDebugger = WindowDebugger.ShowWindow;
      OrigSettings.VerboseLogging = CurrSettings.VerboseLogging;
      OrigSettings.AutoSave = CurrSettings.AutoSave;
      OrigSettings.SaveIntervalSec = CurrSettings.SaveIntervalSec;
      OrigSettings.FlowRate = CurrSettings.FlowRate;
      OrigSettings.FlowCost = CurrSettings.FlowCost;
      OrigSettings.MinFlowRate = CurrSettings.MinFlowRate;
      OrigSettings.MaxFlowRate = CurrSettings.MaxFlowRate;
      OrigSettings.MaxFlowTimeSec = CurrSettings.MaxFlowTimeSec;
      OrigSettings.EnableXferCost = CurrSettings.EnableXferCost;
      OrigSettings.PumpSoundStart = CurrSettings.PumpSoundStart;
      OrigSettings.PumpSoundRun = CurrSettings.PumpSoundRun;
      OrigSettings.PumpSoundStop = CurrSettings.PumpSoundStop;
      OrigSettings.CrewSoundStart = CurrSettings.CrewSoundStart;
      OrigSettings.CrewSoundRun = CurrSettings.CrewSoundRun;
      OrigSettings.CrewSoundStop = CurrSettings.CrewSoundStop;
      OrigSettings.CrewSoundVol = CurrSettings.CrewSoundVol;
      OrigSettings.PumpSoundVol = CurrSettings.PumpSoundVol;

      OrigSettings.EnableScience = CurrSettings.EnableScience;
      OrigSettings.EnableHighlighting = CurrSettings.EnableHighlighting;
      OrigSettings.OnlySourceTarget = CurrSettings.OnlySourceTarget;
      OrigSettings.EnableClsHighlighting = CurrSettings.EnableClsHighlighting;
      OrigSettings.EnableCrew = CurrSettings.EnableCrew;
      OrigSettings.EnableCrewModify = CurrSettings.EnableCrewModify;
      OrigSettings.EnablePfCrews = CurrSettings.EnablePfCrews;
      OrigSettings.EnablePfResources = CurrSettings.EnablePfResources;
      OrigSettings.EnableCls = CurrSettings.EnableCls;
      OrigSettings.EnableStockCrewXfer = CurrSettings.EnableStockCrewXfer;
      OrigSettings.OverrideStockCrewXfer = CurrSettings.OverrideStockCrewXfer;
      OrigSettings.EnableClsAllowTransfer = CurrSettings.EnableClsAllowTransfer;
      OrigSettings.EnableKerbalRename = CurrSettings.EnableKerbalRename;
      OrigSettings.EnableChangeProfession = CurrSettings.EnableChangeProfession;
      OrigSettings.UseUnityStyle = CurrSettings.UseUnityStyle;
      OrigSettings.LockSettings = CurrSettings.LockSettings;
      OrigSettings.EnableBlizzyToolbar = CurrSettings.EnableBlizzyToolbar;
      OrigSettings.SaveLogOnExit = CurrSettings.SaveLogOnExit;
      OrigSettings.ShowToolTips = CurrSettings.ShowToolTips;
      OrigSettings.DebuggerToolTips = WindowDebugger.ShowToolTips;
      OrigSettings.ManifestToolTips = WindowManifest.ShowToolTips;
      OrigSettings.TransferToolTips = WindowTransfer.ShowToolTips;
      OrigSettings.SettingsToolTips = WindowSettings.ShowToolTips;
      OrigSettings.RosterToolTips = WindowRoster.ShowToolTips;
      OrigSettings.ControlToolTips = WindowControl.ShowToolTips;
      OrigSettings.HatchToolTips = TabHatch.ShowToolTips;
      OrigSettings.PanelToolTips = TabSolarPanel.ShowToolTips;
      OrigSettings.AntennaToolTips = TabAntenna.ShowToolTips;
      OrigSettings.LightToolTips = TabLight.ShowToolTips;
      OrigSettings.AppIconToolTips = PopupSmBtnHover.ShowToolTips;

      OrigSettings.RealismToolTips = TabRealism.ShowToolTips;
      OrigSettings.ToolTipsToolTips = TabToolTips.ShowToolTips;
      OrigSettings.SoundsToolTips = TabSounds.ShowToolTips;
      OrigSettings.HighlightToolTips = TabHighlight.ShowToolTips;
      OrigSettings.ConfigToolTips = TabConfig.ShowToolTips;

      //debugger Settings
      OrigSettings.ErrorLogLength = CurrSettings.ErrorLogLength;
    }

    internal static void MemRestoreTempSettings()
    {
      CurrSettings.RealismMode = OrigSettings.RealismMode;
      CurrSettings.RealXfers = OrigSettings.RealXfers;
      CurrSettings.RealCrewXfers = OrigSettings.RealCrewXfers;
      CurrSettings.RealControl = OrigSettings.RealControl;
      WindowDebugger.ShowWindow = OrigSettings.ShowDebugger;
      CurrSettings.VerboseLogging = OrigSettings.VerboseLogging;
      CurrSettings.AutoSave = OrigSettings.AutoSave;
      CurrSettings.SaveIntervalSec = OrigSettings.SaveIntervalSec;
      CurrSettings.FlowRate = OrigSettings.FlowRate;
      CurrSettings.FlowCost = OrigSettings.FlowCost;
      CurrSettings.MinFlowRate = OrigSettings.MinFlowRate;
      CurrSettings.MaxFlowRate = OrigSettings.MaxFlowRate;
      CurrSettings.MaxFlowTimeSec = OrigSettings.MaxFlowTimeSec;
      CurrSettings.EnableXferCost = OrigSettings.EnableXferCost;
      CurrSettings.PumpSoundStart = OrigSettings.PumpSoundStart;
      CurrSettings.PumpSoundRun = OrigSettings.PumpSoundRun;
      CurrSettings.PumpSoundStop = OrigSettings.PumpSoundStop;
      CurrSettings.CrewSoundStart = OrigSettings.CrewSoundStart;
      CurrSettings.CrewSoundRun = OrigSettings.CrewSoundRun;
      CurrSettings.CrewSoundStop = OrigSettings.CrewSoundStop;
      CurrSettings.CrewSoundVol = OrigSettings.CrewSoundVol;
      CurrSettings.PumpSoundVol = OrigSettings.PumpSoundVol;
      CurrSettings.EnableScience = OrigSettings.EnableScience;
      CurrSettings.EnableHighlighting = OrigSettings.EnableHighlighting;
      CurrSettings.OnlySourceTarget = OrigSettings.OnlySourceTarget;
      CurrSettings.EnableClsHighlighting = OrigSettings.EnableClsHighlighting;
      CurrSettings.EnableCrew = OrigSettings.EnableCrew;
      CurrSettings.EnableCrewModify = OrigSettings.EnableCrewModify;
      CurrSettings.EnablePfCrews = OrigSettings.EnablePfCrews;
      CurrSettings.EnablePfResources = OrigSettings.EnablePfResources;
      CurrSettings.EnableCls = OrigSettings.EnableCls;
      CurrSettings.EnableStockCrewXfer = OrigSettings.EnableStockCrewXfer;
      CurrSettings.OverrideStockCrewXfer = OrigSettings.OverrideStockCrewXfer;
      CurrSettings.EnableClsAllowTransfer = OrigSettings.EnableClsAllowTransfer;
      CurrSettings.EnableKerbalRename = OrigSettings.EnableKerbalRename;
      CurrSettings.EnableChangeProfession = OrigSettings.EnableChangeProfession;
      CurrSettings.UseUnityStyle = OrigSettings.UseUnityStyle;
      CurrSettings.LockSettings = OrigSettings.LockSettings;
      CurrSettings.EnableBlizzyToolbar = OrigSettings.EnableBlizzyToolbar;
      CurrSettings.SaveLogOnExit = OrigSettings.SaveLogOnExit;
      CurrSettings.ShowToolTips = OrigSettings.ShowToolTips;
      WindowDebugger.ShowToolTips = OrigSettings.DebuggerToolTips;
      WindowManifest.ShowToolTips = OrigSettings.ManifestToolTips;
      WindowTransfer.ShowToolTips = OrigSettings.TransferToolTips;
      WindowSettings.ShowToolTips = OrigSettings.SettingsToolTips;
      WindowRoster.ShowToolTips = OrigSettings.RosterToolTips;
      WindowControl.ShowToolTips = OrigSettings.ControlToolTips;
      TabHatch.ShowToolTips = OrigSettings.HatchToolTips;
      TabSolarPanel.ShowToolTips = OrigSettings.PanelToolTips;
      TabAntenna.ShowToolTips = OrigSettings.AntennaToolTips;
      TabLight.ShowToolTips = OrigSettings.LightToolTips;
      PopupSmBtnHover.ShowToolTips = OrigSettings.AppIconToolTips;

      TabRealism.ShowToolTips = OrigSettings.RealismToolTips;
      TabHighlight.ShowToolTips = OrigSettings.HighlightToolTips;
      TabToolTips.ShowToolTips = OrigSettings.ToolTipsToolTips;
      TabSounds.ShowToolTips = OrigSettings.SoundsToolTips;
      TabConfig.ShowToolTips = OrigSettings.ConfigToolTips;
      //TabInstalledMods.ShowToolTips = Orig.ModsToolTips;

      //debugger Settings
      OrigSettings.ErrorLogLength = CurrSettings.ErrorLogLength;
    }

    #endregion

  }

}
