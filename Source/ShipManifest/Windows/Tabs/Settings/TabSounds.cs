using ShipManifest.InternalObjects;
using ShipManifest.InternalObjects.Settings;
using UnityEngine;

namespace ShipManifest.Windows.Tabs.Settings
{
  internal static class TabSounds
  {

    static TabSounds()
    {
      RefreshUIScale();
      toolTip = new ToolTip
      {
        Show = ShowToolTips
      };
    }

    // UIScale settings
    internal static float guiTextWidth;
    internal static float guiLabelWidth;
    internal static float guiLineHeight;

    // ToolTip vars
    internal static ToolTip toolTip;
    private static bool _showToolTips = true;
    internal static bool ShowToolTips
    {
      get => _showToolTips;
      set => _showToolTips = toolTip.Show = value;
    }

    internal static string StrFlowCost = "0";
    private static Rect _rect;

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
    internal static GUIContent filePathContent  = new GUIContent($"{SmUtils.SmTags["#smloc_settings_sounds_013"]}");

    internal static void Display(Vector2 displayViewerPosition)
    {

      // Reset Tooltip active flag...
      toolTip.Active = false;
      toolTip.CanShow = WindowSettings.ShowToolTips && ShowToolTips;

      int scrollX = 20;

      GUILayout.Label(titleContent, SMStyle.LabelTabHeader);
      GUILayout.Label(WindowSettings.TabRule, SMStyle.LabelStyleHardRule, GUILayout.Height(WindowSettings.GuiRuleHeight), GUILayout.Width(WindowSettings.GuiRuleWidth));

      GUILayout.Label(xferPumpContent, GUILayout.Height(guiLineHeight)); //"Transfer Pump:"

      // Pump Start Sound
      // Pump Starting:
      CurrSettings.PumpSoundStart = GuiUtils.DisplaySettingsTextField(CurrSettings.PumpSoundStart, startPumpContent,
        guiLabelWidth, guiTextWidth, filePathContent, 40, toolTip, scrollX);

      // Pump Run Sound
      CurrSettings.PumpSoundRun = GuiUtils.DisplaySettingsTextField(CurrSettings.PumpSoundRun, runPumpContent,
        guiLabelWidth, guiTextWidth, filePathContent, 40, toolTip, scrollX);

      // Pump Stop Sound
      CurrSettings.PumpSoundStop = GuiUtils.DisplaySettingsTextField(CurrSettings.PumpSoundStop, stopPumpContent,
        guiLabelWidth, guiTextWidth, filePathContent, 40, toolTip, scrollX);

      // Pump Sound Volume
      GUILayout.BeginHorizontal();
      GUILayout.Label(volPumpContent, GUILayout.Width(guiLabelWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && toolTip.CanShow)
        toolTip.Desc = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref toolTip.Active, scrollX);

      // Volume Slider Control
      SliderData slider = new SliderData
      {
        minContent = volMinContent,
        maxContent = volMaxContent,
        setting = CurrSettings.PumpSoundVol,
        minValue = 0f,
        maxValue = 1f,
        minWidth = 40,
        maxWidth = 40,
        sliderWidth = 140
      };

      CurrSettings.PumpSoundVol = GuiUtils.DisplaySettingsSlider(slider, ref toolTip, scrollX);
      GUILayout.EndHorizontal();

      GUILayout.Label(" ", GUILayout.Height(WindowSettings.GuiRuleHeight));

      // Crew:
      GUILayout.Label(crewContent, GUILayout.Height(guiLineHeight));
      // Crew Start Sound
      CurrSettings.CrewSoundStart = GuiUtils.DisplaySettingsTextField(CurrSettings.CrewSoundStart, startCrewContent,
        guiLabelWidth, guiTextWidth, filePathContent, 40, toolTip, scrollX);

      // Crew Run Sound
      CurrSettings.CrewSoundRun = GuiUtils.DisplaySettingsTextField(CurrSettings.CrewSoundRun, runCrewContent,
        guiLabelWidth, guiTextWidth, filePathContent, 40, toolTip, scrollX);

      // Crew Stop Sound
      CurrSettings.CrewSoundStop = GuiUtils.DisplaySettingsTextField(CurrSettings.CrewSoundStop, stopCrewContent,
        guiLabelWidth, guiTextWidth, filePathContent, 40, toolTip, scrollX);

      // Crew Sound Volume
      GUILayout.BeginHorizontal();
      GUILayout.Label(volCrewContent, GUILayout.Width(guiLabelWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && toolTip.CanShow)
        toolTip.Desc = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref toolTip.Active, scrollX);

      // Volume Slider Control
      slider = new SliderData
      {
        minContent = volMinContent,
        maxContent = volMaxContent,
        setting = CurrSettings.CrewSoundVol,
        minValue = 0f,
        maxValue = 1f,
        minWidth = 40,
        maxWidth = 40,
        sliderWidth = 140
      };
      CurrSettings.CrewSoundVol = GuiUtils.DisplaySettingsSlider(slider, ref toolTip, scrollX);
      GUILayout.EndHorizontal();
    }

    internal static void RefreshUIScale()
    {
      guiTextWidth = 200 * (CurrSettings.CurrentUIScale / 2);
      guiLabelWidth = 100 * CurrSettings.CurrentUIScale;
      guiLineHeight = 20 * CurrSettings.CurrentUIScale;
    }

  }
}
