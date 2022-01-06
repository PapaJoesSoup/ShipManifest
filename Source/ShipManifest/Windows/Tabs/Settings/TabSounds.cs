using ShipManifest.InternalObjects;
using ShipManifest.InternalObjects.Settings;
using UnityEngine;

namespace ShipManifest.Windows.Tabs.Settings
{
  internal static class TabSounds
  {
    internal static string StrFlowCost = "0";

    // GUI tooltip and label support
    private static Rect _rect;
    private const float guiRuleWidth = 350;
    private const float guiTextWidth = 220;
    private const float guiLabelWidth = 100;

    internal static string ToolTip = "";
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;
    private static bool _canShowToolTips = true;
    internal static Rect Position = WindowSettings.Position;

    // Content strings
    internal static GUIContent titleContent     = new GUIContent(SmUtils.SmTags["#smloc_settings_sounds_000"]);
    internal static GUIContent xferPumpContent  = new GUIContent($"{SmUtils.SmTags["#smloc_settings_sounds_001"]}:");
    internal static GUIContent startPumpContent = new GUIContent($"{SmUtils.SmTags["#smloc_settings_sounds_002"]}:", SmUtils.SmTags["#smloc_settings_sounds_tt_001"]);
    internal static GUIContent runPumpContent   = new GUIContent($"{SmUtils.SmTags["#smloc_settings_sounds_003"]}:", SmUtils.SmTags["#smloc_settings_sounds_tt_002"]);
    internal static GUIContent stopPumpContent  = new GUIContent($"{SmUtils.SmTags["#smloc_settings_sounds_004"]}:", SmUtils.SmTags["#smloc_settings_sounds_tt_003"]);
    internal static GUIContent volPumpContent   = new GUIContent($"{SmUtils.SmTags["#smloc_settings_sounds_005"]}:", SmUtils.SmTags["#smloc_settings_sounds_tt_004"]);
    internal static GUIContent volMinContent    = new GUIContent(SmUtils.SmTags["#smloc_settings_sounds_006"]);
    internal static GUIContent volMaxContent    = new GUIContent(SmUtils.SmTags["#smloc_settings_sounds_007"], SmUtils.SmTags["#smloc_settings_sounds_004"]);
    internal static GUIContent crewContent      = new GUIContent($"{SmUtils.SmTags["#smloc_settings_sounds_008"]}:");
    internal static GUIContent startCrewContent = new GUIContent($"{SmUtils.SmTags["#smloc_settings_sounds_009"]}:", SmUtils.SmTags["#smloc_settings_sounds_tt_005"]);
    internal static GUIContent runCrewContent   = new GUIContent($"{SmUtils.SmTags["#smloc_settings_sounds_010"]}:", SmUtils.SmTags["#smloc_settings_sounds_tt_006"]);
    internal static GUIContent stopCrewContent  = new GUIContent($"{SmUtils.SmTags["#smloc_settings_sounds_011"]}:", SmUtils.SmTags["#smloc_settings_sounds_tt_007"]);
    internal static GUIContent volCrewContent   = new GUIContent($"{SmUtils.SmTags["#smloc_settings_sounds_012"]}:", SmUtils.SmTags["#smloc_settings_sounds_tt_004"]);


    internal static void Display(Vector2 displayViewerPosition)
    {
      // Reset Tooltip active flag...
      ToolTipActive = false;
      _canShowToolTips = WindowSettings.ShowToolTips && ShowToolTips;

      Position = WindowSettings.Position;
      int scrollX = 20;

      GUILayout.Label(titleContent, SMStyle.LabelTabHeader);
      GUILayout.Label(WindowSettings.TabRule, SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(guiRuleWidth));

      GUILayout.Label(xferPumpContent, GUILayout.Height(20)); //"Transfer Pump:"

      // Pump Start Sound
      GUILayout.BeginHorizontal();
      // Pump Starting:
      GUILayout.Label(startPumpContent, GUILayout.Width(guiLabelWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      CurrSettings.PumpSoundStart = GUILayout.TextField(CurrSettings.PumpSoundStart, GUILayout.Width(guiTextWidth));
      GUILayout.EndHorizontal();

      // Pump Run Sound
      GUILayout.BeginHorizontal();
      GUILayout.Label(runPumpContent, GUILayout.Width(guiLabelWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      CurrSettings.PumpSoundRun = GUILayout.TextField(CurrSettings.PumpSoundRun, GUILayout.Width(guiTextWidth));
      GUILayout.EndHorizontal();

      // Pump Stop Sound
      GUILayout.BeginHorizontal();
      GUILayout.Label(stopPumpContent, GUILayout.Width(guiLabelWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      CurrSettings.PumpSoundStop = GUILayout.TextField(CurrSettings.PumpSoundStop, GUILayout.Width(guiTextWidth));
      GUILayout.EndHorizontal();

      // Pump Sound Volume
      GUILayout.BeginHorizontal();
      GUILayout.Label(volPumpContent, GUILayout.Width(guiLabelWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // Volume Slider Control
      GUILayout.Label(volMinContent, GUILayout.Width(40),GUILayout.Height(20)); // "Min"
      CurrSettings.PumpSoundVol = GUILayout.HorizontalSlider((float)CurrSettings.PumpSoundVol, 0f, 1f, GUILayout.Width(140), GUILayout.Height(20));
      GUILayout.Label(volMaxContent, GUILayout.Width(40), GUILayout.Height(20));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      GUILayout.EndHorizontal();

      GUILayout.Label(" ", GUILayout.Height(10));
      // Crew:
      GUILayout.Label(crewContent, GUILayout.Height(20));
      // Crew Start Sound
      GUILayout.BeginHorizontal();
      GUILayout.Label(startCrewContent, GUILayout.Width(guiLabelWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      CurrSettings.CrewSoundStart = GUILayout.TextField(CurrSettings.CrewSoundStart, GUILayout.Width(guiTextWidth));
      GUILayout.EndHorizontal();

      // Crew Run Sound
      GUILayout.BeginHorizontal();
      GUILayout.Label(runCrewContent, GUILayout.Width(guiLabelWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      CurrSettings.CrewSoundRun = GUILayout.TextField(CurrSettings.CrewSoundRun, GUILayout.Width(guiTextWidth));
      GUILayout.EndHorizontal();

      // Crew Stop Sound
      GUILayout.BeginHorizontal();
      GUILayout.Label(stopCrewContent, GUILayout.Width(guiLabelWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      CurrSettings.CrewSoundStop = GUILayout.TextField(CurrSettings.CrewSoundStop, GUILayout.Width(guiTextWidth));
      GUILayout.EndHorizontal();

      // Crew Sound Volume
      GUILayout.BeginHorizontal();
      GUILayout.Label(volCrewContent, GUILayout.Width(guiLabelWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // Volume Slider Control
      GUILayout.Label(volMinContent, GUILayout.Width(40), GUILayout.Height(20)); // "Min"
      CurrSettings.CrewSoundVol = GUILayout.HorizontalSlider((float)CurrSettings.CrewSoundVol, 0f, 1f, GUILayout.Width(140), GUILayout.Height(20));
      GUILayout.Label(volMaxContent, GUILayout.Width(40), GUILayout.Height(20));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      GUILayout.EndHorizontal();
    }
  }
}
