using UnityEngine;

namespace ShipManifest.InternalObjects.Settings
{
  internal static class OrigSettings
  {
    // Realism Tab Feature Options
    internal static int RealismMode = 3;
    internal static bool RealXfers;
    internal static bool RealCrewXfers;
    internal static bool RealControl;
    internal static bool EnableXferCost = true;
    internal static double FlowRate = 100;
    internal static double FlowCost = 0.0015;
    internal static double MaxFlowRate = 1000;
    internal static double MinFlowRate;
    internal static int MaxFlowTimeSec = 100;
    internal static bool EnableScience = true;
    internal static bool EnableCrew = true;
    internal static bool EnableCrewModify = true;
    internal static bool EnableKerbalRename = true;
    internal static bool EnableChangeProfession = true;
    internal static bool EnablePfCrews;
    internal static bool EnableStockCrewXfer = true;
    internal static bool OverrideStockCrewXfer = true;
    internal static bool EnableClsAllowTransfer = true;
    internal static bool EnablePfResources = true;
    internal static bool EnableCls = true;
    internal static bool LockSettings;

    // Sound Tab Options
    // All Default sounds licensing is: CC-By-SA
    internal static string PumpSoundStart = "ShipManifest/Sounds/59328-1";
    internal static string PumpSoundRun = "ShipManifest/Sounds/59328-2";
    internal static string PumpSoundStop = "ShipManifest/Sounds/59328-3";
    internal static double PumpSoundVol = 1;

    internal static string CrewSoundStart = "ShipManifest/Sounds/14214-1";
    internal static string CrewSoundRun = "ShipManifest/Sounds/14214-2";
    internal static string CrewSoundStop = "ShipManifest/Sounds/14214-3";
    internal static double CrewSoundVol = 1;

    // Tooltip Options
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
    internal static bool AppIconToolTips = true;


    //Highlighting Tab Options
    internal static bool EnableHighlighting = true;
    internal static bool OnlySourceTarget;
    internal static bool EnableClsHighlighting = true;
    internal static bool EnableEdgeHighlighting = true;

    //Config Tab options
    internal static bool EnableBlizzyToolbar;
    internal static bool VerboseLogging;
    internal static bool ShowDebugger;
    internal static string ErrorLogLength = "1000";
    internal static bool SaveLogOnExit = true;
    internal static bool AutoSave;
    internal static bool UseUnityStyle = true;
    internal static int SaveIntervalSec = 60;
  }
}
