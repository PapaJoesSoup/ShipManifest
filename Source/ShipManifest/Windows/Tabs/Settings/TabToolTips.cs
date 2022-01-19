using ShipManifest.InternalObjects;
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
    private static Rect _rect;
    private const float guiRuleWidth = 350;
    private const float guiToggleWidth = 300;
    private const float guiIndent = 20;
    private const float gui2xIndent = 40;

    internal static string ToolTip = "";
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;
    private static bool _canShowToolTips = true;
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
    internal static GUIContent vesselTtContent     = new GUIContent(SmUtils.SmTags["#smloc_settings_tooltips_018"], SmUtils.SmTags["#smloc_settings_tooltips_tt_018"]);
    internal static GUIContent appIconTtContent     = new GUIContent(SmUtils.SmTags["#smloc_settings_tooltips_019"], SmUtils.SmTags["#smloc_settings_tooltips_tt_019"]);


    internal static void Display(Vector2 displayViewerPosition)
    {
      // Reset Tooltip active flag...
      ToolTipActive = false;
      ToolTip = "";
      _canShowToolTips = WindowSettings.ShowToolTips && ShowToolTips;

      Position = WindowSettings.Position;
      int scrollX = 20;

      // Enable Tool Tips
      GUI.enabled = true;
      GUILayout.Label(titleContent, SMStyle.LabelTabHeader); //"ToolTips"
      GUILayout.Label(WindowSettings.TabRule, SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(guiRuleWidth));

      CurrSettings.ShowToolTips = GUILayout.Toggle(CurrSettings.ShowToolTips, allTtContent, GUILayout.Width(guiToggleWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      GUI.enabled = CurrSettings.ShowToolTips;

      // App Icon Popup
      GUILayout.BeginHorizontal();
      GUILayout.Space(guiIndent);
      PopupSmBtnHover.ShowToolTips = GUILayout.Toggle(PopupSmBtnHover.ShowToolTips, appIconTtContent, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // Debugger Window
      GUILayout.BeginHorizontal();
      GUILayout.Space(guiIndent);
      WindowDebugger.ShowToolTips = GUILayout.Toggle(WindowDebugger.ShowToolTips, debugTtContent, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // Manifest Window
      GUILayout.BeginHorizontal();
      GUILayout.Space(guiIndent);
      WindowManifest.ShowToolTips = GUILayout.Toggle(WindowManifest.ShowToolTips, manifestTtContent, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // Transfer Window
      GUILayout.BeginHorizontal();
      GUILayout.Space(guiIndent);
      WindowTransfer.ShowToolTips = GUILayout.Toggle(WindowTransfer.ShowToolTips, transferTtContent, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // Setting Window
      GUILayout.BeginHorizontal();
      GUILayout.Space(guiIndent);
      WindowSettings.ShowToolTips = GUILayout.Toggle(WindowSettings.ShowToolTips, settingsTtContent, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      GUI.enabled = CurrSettings.ShowToolTips && WindowSettings.ShowToolTips;

      // SW - Realism Tab
      GUILayout.BeginHorizontal();
      GUILayout.Space(gui2xIndent);
      TabRealism.ShowToolTips = GUILayout.Toggle(TabRealism.ShowToolTips, realismTtContent, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // SW - Highlighting Tab
      GUILayout.BeginHorizontal();
      GUILayout.Space(gui2xIndent);
      TabHighlight.ShowToolTips = GUILayout.Toggle(TabHighlight.ShowToolTips, highlightTtContent, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // SW - ToolTips Tab
      GUILayout.BeginHorizontal();
      GUILayout.Space(gui2xIndent);
      ShowToolTips = GUILayout.Toggle(ShowToolTips, tooltipsTtContent, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // SW - Sounds Tab
      GUILayout.BeginHorizontal();
      GUILayout.Space(gui2xIndent);
      TabLight.ShowToolTips = GUILayout.Toggle(TabLight.ShowToolTips, soundsTtContent, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // SW - Config Tab
      GUILayout.BeginHorizontal();
      GUILayout.Space(gui2xIndent);
      TabConfig.ShowToolTips = GUILayout.Toggle(TabConfig.ShowToolTips, configTtContent, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      GUI.enabled = CurrSettings.ShowToolTips;

      // Roster Window
      GUILayout.BeginHorizontal();
      GUILayout.Space(guiIndent);
      WindowRoster.ShowToolTips = GUILayout.Toggle(WindowRoster.ShowToolTips, rosterTtContent, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // Control Window
      GUILayout.BeginHorizontal();
      GUILayout.Space(guiIndent);
      WindowControl.ShowToolTips = GUILayout.Toggle(WindowControl.ShowToolTips, controlTtContent, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      GUI.enabled = CurrSettings.ShowToolTips && WindowControl.ShowToolTips;

      // CW - Vessel Tab
      GUILayout.BeginHorizontal();
      GUILayout.Space(gui2xIndent);
      TabLight.ShowToolTips = GUILayout.Toggle(TabVessel.ShowToolTips, vesselTtContent, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // CW - Hatch Tab
      GUILayout.BeginHorizontal();
      GUILayout.Space(gui2xIndent);
      TabHatch.ShowToolTips = GUILayout.Toggle(TabHatch.ShowToolTips, hatchTtContent, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // CW - Solar Tab
      GUILayout.BeginHorizontal();
      GUILayout.Space(gui2xIndent);
      TabSolarPanel.ShowToolTips = GUILayout.Toggle(TabSolarPanel.ShowToolTips, solarTtContent, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // CW - Antenna Tab
      GUILayout.BeginHorizontal();
      GUILayout.Space(gui2xIndent);
      TabAntenna.ShowToolTips = GUILayout.Toggle(TabAntenna.ShowToolTips, antennaTtContent, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // CW - Light Tab
      GUILayout.BeginHorizontal();
      GUILayout.Space(gui2xIndent);
      TabLight.ShowToolTips = GUILayout.Toggle(TabLight.ShowToolTips, lightTtContent, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // CW - Labs Tab
      GUILayout.BeginHorizontal();
      GUILayout.Space(gui2xIndent);
      TabScienceLab.ShowToolTips = GUILayout.Toggle(TabScienceLab.ShowToolTips, labsTtContent, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      GUI.enabled = true;
    }
  }
}
