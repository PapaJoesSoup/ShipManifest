using ShipManifest.InternalObjects.Settings;
using ShipManifest.Windows.Popups;
using ShipManifest.Windows.Tabs.Control;
using UnityEngine;

namespace ShipManifest.Windows.Tabs.Settings
{
  internal static class TabToolTips
  {
    internal static string StrFlowCost = "0";

    // GUI tooltip and label support
    private static float guiRuleWidth = 350 * GameSettings.UI_SCALE;
    private static float guiRuleHeight = 10 * GameSettings.UI_SCALE;
    private static float guiToggleWidth = 300 * GameSettings.UI_SCALE;
    private static float guiIndent = 20 * GameSettings.UI_SCALE;
    private static float gui2xIndent = 40 * GameSettings.UI_SCALE;

    internal static ToolTip toolTip;

    private static bool _showToolTips = true;
    internal static bool ShowToolTips
    {
      get => _showToolTips;
      set => _showToolTips = toolTip.Show = value;
    }

    internal static Rect Position = WindowSettings.Position;

    // Content strings
    internal static GUIContent titleContent       = new GUIContent(SmUtils.SmTags["#smloc_settings_tooltips_000"]);
    internal static GUIContent allTtContent       = new GUIContent(SmUtils.SmTags["#smloc_settings_tooltips_001"], SmUtils.SmTags["#smloc_settings_tooltips_tt_001"]);
    internal static GUIContent debugTtContent     = new GUIContent(SmUtils.SmTags["#smloc_settings_tooltips_002"], SmUtils.SmTags["#smloc_settings_tooltips_tt_002"]);
    internal static GUIContent manifestTtContent  = new GUIContent(SmUtils.SmTags["#smloc_settings_tooltips_003"], SmUtils.SmTags["#smloc_settings_tooltips_tt_003"]);
    internal static GUIContent transferTtContent  = new GUIContent(SmUtils.SmTags["#smloc_settings_tooltips_004"], SmUtils.SmTags["#smloc_settings_tooltips_tt_004"]);
    internal static GUIContent settingsTtContent  = new GUIContent(SmUtils.SmTags["#smloc_settings_tooltips_005"], SmUtils.SmTags["#smloc_settings_tooltips_tt_005"]);
    internal static GUIContent realismTtContent   = new GUIContent(SmUtils.SmTags["#smloc_settings_tooltips_006"], SmUtils.SmTags["#smloc_settings_tooltips_tt_006"]);
    internal static GUIContent highlightTtContent = new GUIContent(SmUtils.SmTags["#smloc_settings_tooltips_007"], SmUtils.SmTags["#smloc_settings_tooltips_tt_007"]);
    internal static GUIContent tooltipsTtContent  = new GUIContent(SmUtils.SmTags["#smloc_settings_tooltips_008"], SmUtils.SmTags["#smloc_settings_tooltips_tt_008"]);
    internal static GUIContent soundsTtContent    = new GUIContent(SmUtils.SmTags["#smloc_settings_tooltips_009"], SmUtils.SmTags["#smloc_settings_tooltips_tt_009"]);
    internal static GUIContent configTtContent    = new GUIContent(SmUtils.SmTags["#smloc_settings_tooltips_010"], SmUtils.SmTags["#smloc_settings_tooltips_tt_010"]);
    internal static GUIContent rosterTtContent    = new GUIContent(SmUtils.SmTags["#smloc_settings_tooltips_011"], SmUtils.SmTags["#smloc_settings_tooltips_tt_011"]);
    internal static GUIContent controlTtContent   = new GUIContent(SmUtils.SmTags["#smloc_settings_tooltips_012"], SmUtils.SmTags["#smloc_settings_tooltips_tt_012"]);
    internal static GUIContent hatchTtContent     = new GUIContent(SmUtils.SmTags["#smloc_settings_tooltips_013"], SmUtils.SmTags["#smloc_settings_tooltips_tt_013"]);
    internal static GUIContent solarTtContent     = new GUIContent(SmUtils.SmTags["#smloc_settings_tooltips_014"], SmUtils.SmTags["#smloc_settings_tooltips_tt_014"]);
    internal static GUIContent antennaTtContent   = new GUIContent(SmUtils.SmTags["#smloc_settings_tooltips_015"], SmUtils.SmTags["#smloc_settings_tooltips_tt_015"]);
    internal static GUIContent lightTtContent     = new GUIContent(SmUtils.SmTags["#smloc_settings_tooltips_016"], SmUtils.SmTags["#smloc_settings_tooltips_tt_016"]);
    internal static GUIContent labsTtContent      = new GUIContent(SmUtils.SmTags["#smloc_settings_tooltips_017"], SmUtils.SmTags["#smloc_settings_tooltips_tt_017"]);
    internal static GUIContent vesselTtContent    = new GUIContent(SmUtils.SmTags["#smloc_settings_tooltips_018"], SmUtils.SmTags["#smloc_settings_tooltips_tt_018"]);
    internal static GUIContent appIconTtContent   = new GUIContent(SmUtils.SmTags["#smloc_settings_tooltips_019"], SmUtils.SmTags["#smloc_settings_tooltips_tt_019"]);

    static TabToolTips() { toolTip = new ToolTip { Show = ShowToolTips }; }

    internal static void Display(Vector2 displayViewerPosition)
    {
      // Reset Tooltip active flag...
      toolTip.Active = false;
      toolTip.Desc = "";
      toolTip.CanShow = WindowSettings.ShowToolTips && ShowToolTips;

      Position = WindowSettings.Position;
      int scrollX = 20;

      GUI.enabled = true;
      // Tab Header
      GUILayout.Label(titleContent, SMStyle.LabelTabHeader); //"ToolTips"
      GUILayout.Label(WindowSettings.TabRule, SMStyle.LabelStyleHardRule, GUILayout.Height(guiRuleHeight), GUILayout.Width(guiRuleWidth));

      // Enable Tool Tips
      CurrSettings.ShowToolTips = GuiUtils.DisplaySettingsToggle(CurrSettings.ShowToolTips, allTtContent,
        ref toolTip, guiToggleWidth, scrollX);

      GUI.enabled = CurrSettings.ShowToolTips;

      // App Icon Popup
      GUILayout.BeginHorizontal();
      GUILayout.Space(guiIndent);
      PopupSmBtnHover.ShowToolTips = GuiUtils.DisplaySettingsToggle(PopupSmBtnHover.ShowToolTips, appIconTtContent,
        ref toolTip, guiToggleWidth, scrollX);
      GUILayout.EndHorizontal();

      // Debugger Window
      GUILayout.BeginHorizontal();
      GUILayout.Space(guiIndent);
      WindowDebugger.ShowToolTips = GuiUtils.DisplaySettingsToggle(WindowDebugger.ShowToolTips, debugTtContent,
        ref toolTip, guiToggleWidth, scrollX);
      GUILayout.EndHorizontal();

      // Manifest Window
      GUILayout.BeginHorizontal();
      GUILayout.Space(guiIndent);
      WindowManifest.ShowToolTips = GuiUtils.DisplaySettingsToggle(WindowManifest.ShowToolTips, manifestTtContent,
        ref toolTip, guiToggleWidth, scrollX);
      GUILayout.EndHorizontal();

      // Transfer Window
      GUILayout.BeginHorizontal();
      GUILayout.Space(guiIndent);
      WindowTransfer.ShowToolTips = GuiUtils.DisplaySettingsToggle(WindowTransfer.ShowToolTips, transferTtContent,
        ref toolTip, guiToggleWidth, scrollX);
      GUILayout.EndHorizontal();

      // Setting Window
      GUILayout.BeginHorizontal();
      GUILayout.Space(guiIndent);
      WindowSettings.ShowToolTips = GuiUtils.DisplaySettingsToggle(WindowSettings.ShowToolTips, settingsTtContent,
        ref toolTip, guiToggleWidth, scrollX);
      GUILayout.EndHorizontal();
      GUI.enabled = CurrSettings.ShowToolTips && WindowSettings.ShowToolTips;

      // SW - Realism Tab
      GUILayout.BeginHorizontal();
      GUILayout.Space(gui2xIndent);
      TabRealism.ShowToolTips = GuiUtils.DisplaySettingsToggle(TabRealism.ShowToolTips, realismTtContent,
        ref toolTip, guiToggleWidth, scrollX);
      GUILayout.EndHorizontal();

      // SW - Highlighting Tab
      GUILayout.BeginHorizontal();
      GUILayout.Space(gui2xIndent);
      TabHighlight.ShowToolTips = GuiUtils.DisplaySettingsToggle(TabHighlight.ShowToolTips, highlightTtContent,
        ref toolTip, guiToggleWidth, scrollX);
      GUILayout.EndHorizontal();

      // SW - ToolTips Tab
      GUILayout.BeginHorizontal();
      GUILayout.Space(gui2xIndent);
      ShowToolTips = GuiUtils.DisplaySettingsToggle(ShowToolTips, tooltipsTtContent,
        ref toolTip, guiToggleWidth, scrollX);
      GUILayout.EndHorizontal();

      // SW - Sounds Tab
      GUILayout.BeginHorizontal();
      GUILayout.Space(gui2xIndent);
      TabSounds.ShowToolTips = GuiUtils.DisplaySettingsToggle(TabSounds.ShowToolTips, soundsTtContent,
        ref toolTip, guiToggleWidth, scrollX);
      GUILayout.EndHorizontal();

      // SW - Config Tab
      GUILayout.BeginHorizontal();
      GUILayout.Space(gui2xIndent);
      TabConfig.ShowToolTips = GuiUtils.DisplaySettingsToggle(TabConfig.ShowToolTips, configTtContent,
        ref toolTip, guiToggleWidth, scrollX);
      GUILayout.EndHorizontal();

      GUI.enabled = CurrSettings.ShowToolTips;

      // Roster Window
      GUILayout.BeginHorizontal();
      GUILayout.Space(guiIndent);
      WindowRoster.ShowToolTips = GuiUtils.DisplaySettingsToggle(WindowRoster.ShowToolTips, rosterTtContent,
        ref toolTip, guiToggleWidth, scrollX);
      GUILayout.EndHorizontal();

      // Control Window
      GUILayout.BeginHorizontal();
      GUILayout.Space(guiIndent);
      WindowControl.ShowToolTips = GuiUtils.DisplaySettingsToggle(WindowControl.ShowToolTips, controlTtContent,
        ref toolTip, guiToggleWidth, scrollX);
      GUILayout.EndHorizontal();

      GUI.enabled = CurrSettings.ShowToolTips && WindowControl.ShowToolTips;

      // CW - Vessel Tab
      GUILayout.BeginHorizontal();
      GUILayout.Space(gui2xIndent);
      TabVessel.ShowToolTips = GuiUtils.DisplaySettingsToggle(TabVessel.ShowToolTips, vesselTtContent,
        ref toolTip, guiToggleWidth, scrollX);
      GUILayout.EndHorizontal();

      // CW - Hatch Tab
      GUILayout.BeginHorizontal();
      GUILayout.Space(gui2xIndent);
      TabHatch.ShowToolTips = GuiUtils.DisplaySettingsToggle(TabHatch.ShowToolTips, hatchTtContent,
        ref toolTip, guiToggleWidth, scrollX);
      GUILayout.EndHorizontal();

      // CW - Solar Tab
      GUILayout.BeginHorizontal();
      GUILayout.Space(gui2xIndent);
      TabSolarPanel.ShowToolTips = GuiUtils.DisplaySettingsToggle(TabSolarPanel.ShowToolTips, solarTtContent,
        ref toolTip, guiToggleWidth, scrollX);
      GUILayout.EndHorizontal();

      // CW - Antenna Tab
      GUILayout.BeginHorizontal();
      GUILayout.Space(gui2xIndent);
      TabAntenna.ShowToolTips = GuiUtils.DisplaySettingsToggle(TabAntenna.ShowToolTips, antennaTtContent,
        ref toolTip, guiToggleWidth, scrollX);
      GUILayout.EndHorizontal();

      // CW - Light Tab
      GUILayout.BeginHorizontal();
      GUILayout.Space(gui2xIndent);
      TabLight.ShowToolTips = GuiUtils.DisplaySettingsToggle(TabLight.ShowToolTips, lightTtContent,
        ref toolTip, guiToggleWidth, scrollX);
      GUILayout.EndHorizontal();

      // CW - Labs Tab
      GUILayout.BeginHorizontal();
      GUILayout.Space(gui2xIndent);
      TabScienceLab.ShowToolTips = GuiUtils.DisplaySettingsToggle(TabScienceLab.ShowToolTips, labsTtContent,
        ref toolTip, guiToggleWidth, scrollX);
      GUILayout.EndHorizontal();

      GUI.enabled = true;
    }
  }
}
