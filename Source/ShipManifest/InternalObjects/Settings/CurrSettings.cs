using UnityEngine;

namespace ShipManifest.InternalObjects.Settings
{
  internal static class CurrSettings
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
    // ManifestToolTips = WindowManifest.ShowToolTips;
    // TransferToolTips = WindowTransfer.ShowToolTips;
    // SettingsToolTips = WindowSettings.ShowToolTips;
    // RosterToolTips = WindowRoster.ShowToolTips;
    // ControlToolTips = WindowControl.ShowToolTips;
    // HatchToolTips = TabHatch.ShowToolTips;
    // PanelToolTips = TabSolarPanel.ShowToolTips;
    // AntennaToolTips = TabAntenna.ShowToolTips;
    // LightToolTips = TabLight.ShowToolTips;
    // RealismToolTips = TabRealism.ShowToolTips;
    // ToolTipsToolTips = TabToolTips.ShowToolTips;
    // SoundsToolTips = TabSounds.ShowToolTips;
    // HighlightToolTips = TabHighlight.ShowToolTips;
    // ConfigToolTips = TabConfig.ShowToolTips;
    // ModToolTips = TabInstalledMods.ShowToolTips;
    // DebuggerToolTips = WindowDebugger.ShowToolTips;
    // AppIconToolTips = PopupSmBtnHover.ShowToolTips;

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
}
