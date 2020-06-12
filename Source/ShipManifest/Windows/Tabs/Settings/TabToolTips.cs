using ShipManifest.InternalObjects;
using ShipManifest.Windows.Tabs.Control;
using UnityEngine;

namespace ShipManifest.Windows.Tabs.Settings
{
  internal static class TabToolTips
  {
    internal static string StrFlowCost = "0";

    // GUI tooltip and label support
    private static string _toolTip = "";
    private static Rect _rect;
    private static string _label = "";
    private static GUIContent _guiLabel;
    private const float guiRuleWidth = 350;
    private const float guiToggleWidth = 300;
    private const float guiIndent = 20;
    private const float gui2xIndent = 40;

    internal static string ToolTip = "";
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;
    private static bool _canShowToolTips = true;
    internal static Rect Position = WindowSettings.Position;

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
      GUILayout.Label(SmUtils.SmTags["#smloc_settings_tooltips_000"], SMStyle.LabelTabHeader); //"ToolTips"
      GUILayout.Label("____________________________________________________________________________________________",
        SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(guiRuleWidth));

      //_label = "Enable All Tool Tips";
      //_toolTip = "Turns all tooltips On or Off.";
      //_toolTip += "\r\nThis is a global setting for all windows/tabs";
      _label = SmUtils.SmTags["#smloc_settings_tooltips_001"];
      _toolTip = SmUtils.SmTags["#smloc_settings_tooltips_tt_001"];
      _guiLabel = new GUIContent(_label, _toolTip);
      SMSettings.ShowToolTips = GUILayout.Toggle(SMSettings.ShowToolTips, _guiLabel, GUILayout.Width(guiToggleWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      GUI.enabled = SMSettings.ShowToolTips;

      // Debugger Window
      GUILayout.BeginHorizontal();
      //_label = "Debugger Window Tool Tips";
      //_toolTip = "Turns tooltips On or Off for the Debugger Window only.";
      //_toolTip += "\r\nRequires All ToolTips setting to be enabled.";
      _label = SmUtils.SmTags["#smloc_settings_tooltips_002"];
      _toolTip = SmUtils.SmTags["#smloc_settings_tooltips_tt_002"];
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(guiIndent);
      WindowDebugger.ShowToolTips = GUILayout.Toggle(WindowDebugger.ShowToolTips, _guiLabel, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // Manifest Window
      GUILayout.BeginHorizontal();
      //_label = "Manifest Window Tool Tips";
      //_toolTip = "Turns tooltips On or Off for the Manifest Window only.";
      //_toolTip += "\r\nRequires global ToolTips setting to be enabled.";
      _label = SmUtils.SmTags["#smloc_settings_tooltips_003"];
      _toolTip = SmUtils.SmTags["#smloc_settings_tooltips_tt_003"];
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(guiIndent);
      WindowManifest.ShowToolTips = GUILayout.Toggle(WindowManifest.ShowToolTips, _guiLabel, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // Transfer Window
      GUILayout.BeginHorizontal();
      //_label = "Transfer Window Tool Tips";
      //_toolTip = "Turns tooltips On or Off for the Manifest Window only.";
      //_toolTip += "\r\nRequires global ToolTips setting to be enabled.";
      _label = SmUtils.SmTags["#smloc_settings_tooltips_004"];
      _toolTip = SmUtils.SmTags["#smloc_settings_tooltips_tt_004"];
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(guiIndent);
      WindowTransfer.ShowToolTips = GUILayout.Toggle(WindowTransfer.ShowToolTips, _guiLabel, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // Setting Window
      GUILayout.BeginHorizontal();
      //_label = "Settings Window Tool Tips";
      //_toolTip = "Turns tooltips On or Off for the Settings Window only.";
      //_toolTip += "\r\nRequires global ToolTips setting to be enabled.";
      _label = SmUtils.SmTags["#smloc_settings_tooltips_005"];
      _toolTip = SmUtils.SmTags["#smloc_settings_tooltips_tt_005"];
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(guiIndent);
      WindowSettings.ShowToolTips = GUILayout.Toggle(WindowSettings.ShowToolTips, _guiLabel, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      GUI.enabled = SMSettings.ShowToolTips && WindowSettings.ShowToolTips;

      // SW - Realism Tab
      GUILayout.BeginHorizontal();
      //_label = "Realism Tab Tool Tips";
      //_toolTip = "Turns tooltips On or Off for the Settings Window's Realism Tab only.";
      //_toolTip += "\r\nRequires global ToolTips setting to be enabled.";
      //_toolTip += "\r\nAlso requires Settings Window tooltips to be enabled.";
      _label = SmUtils.SmTags["#smloc_settings_tooltips_006"];
      _toolTip = SmUtils.SmTags["#smloc_settings_tooltips_tt_006"];
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(gui2xIndent);
      TabRealism.ShowToolTips = GUILayout.Toggle(TabRealism.ShowToolTips, _guiLabel, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // SW - Highlighting Tab
      GUILayout.BeginHorizontal();
      //_label = "Highlghting Tab Tool Tips";
      //_toolTip = "Turns tooltips On or Off for the Settings Window's Highlighting Tab only.";
      //_toolTip += "\r\nRequires global ToolTips setting to be enabled.";
      //_toolTip += "\r\nAlso requires Settings Window tooltips to be enabled.";
      _label = SmUtils.SmTags["#smloc_settings_tooltips_007"];
      _toolTip = SmUtils.SmTags["#smloc_settings_tooltips_tt_007"];
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(gui2xIndent);
      TabHighlight.ShowToolTips = GUILayout.Toggle(TabHighlight.ShowToolTips, _guiLabel, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // SW - ToolTips Tab
      GUILayout.BeginHorizontal();
      //_label = "ToolTips Tab Tool Tips";
      //_toolTip = "Turns tooltips On or Off for the Settings Window's ToolTips Tab only.";
      //_toolTip += "\r\nRequires global ToolTips setting to be enabled.";
      //_toolTip += "\r\nAlso requires Settings Window tooltips to be enabled.";
      _label = SmUtils.SmTags["#smloc_settings_tooltips_008"];
      _toolTip = SmUtils.SmTags["#smloc_settings_tooltips_tt_008"];
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(gui2xIndent);
      ShowToolTips = GUILayout.Toggle(ShowToolTips, _guiLabel, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // SW - Sounds Tab
      GUILayout.BeginHorizontal();
      //_label = "Sounds Tab Tool Tips";
      //_toolTip = "Turns tooltips On or Off for the Settings Window's Sounds Tab only.";
      //_toolTip += "\r\nRequires global ToolTips setting to be enabled.";
      //_toolTip += "\r\nAlso requires Settings Window tooltips to be enabled.";
      _label = SmUtils.SmTags["#smloc_settings_tooltips_009"];
      _toolTip = SmUtils.SmTags["#smloc_settings_tooltips_tt_009"];
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(gui2xIndent);
      TabLight.ShowToolTips = GUILayout.Toggle(TabLight.ShowToolTips, _guiLabel, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // SW - Config Tab
      GUILayout.BeginHorizontal();
      //_label = "Config Tab Tool Tips";
      //_toolTip = "Turns tooltips On or Off for the Settings Window's Config Tab only.";
      //_toolTip += "Requires global ToolTips setting to be enabled.";
      //_toolTip += "\r\nAlso requires Settings Window tooltips to be enabled.";
      _label = SmUtils.SmTags["#smloc_settings_tooltips_010"];
      _toolTip = SmUtils.SmTags["#smloc_settings_tooltips_tt_010"];
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(gui2xIndent);
      TabConfig.ShowToolTips = GUILayout.Toggle(TabConfig.ShowToolTips, _guiLabel, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      GUI.enabled = SMSettings.ShowToolTips;

      // Roster Window
      GUILayout.BeginHorizontal();
      //_label = "Roster Window Tool Tips";
      //_toolTip = "Turns tooltips On or Off for the Roster Window only.";
      //_toolTip += "Requires global ToolTips setting to be enabled.";
      _label = SmUtils.SmTags["#smloc_settings_tooltips_011"];
      _toolTip = SmUtils.SmTags["#smloc_settings_tooltips_tt_011"];
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(guiIndent);
      WindowRoster.ShowToolTips = GUILayout.Toggle(WindowRoster.ShowToolTips, _guiLabel, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // Control Window
      GUILayout.BeginHorizontal();
      //_label = "Control Window Tool Tips";
      //_toolTip = "Turns tooltips On or Off for the Control Window only.";
      //_toolTip += "\r\nRequires global ToolTips setting to be enabled.";
      _label = SmUtils.SmTags["#smloc_settings_tooltips_012"];
      _toolTip = SmUtils.SmTags["#smloc_settings_tooltips_tt_012"];
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(guiIndent);
      WindowControl.ShowToolTips = GUILayout.Toggle(WindowControl.ShowToolTips, _guiLabel, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      GUI.enabled = SMSettings.ShowToolTips && WindowControl.ShowToolTips;

      // CW - Light Tab
      GUILayout.BeginHorizontal();
      //_label = "Vessle Tab Tool Tips";
      //_toolTip = "Turns tooltips On or Off for the Control Window's Vessels Tab only.";
      //_toolTip += "\r\nRequires global ToolTips setting to be enabled.";
      //_toolTip += "\r\nAlso requires Control Window tooltips to be enabled.";
      _label = SmUtils.SmTags["#smloc_settings_tooltips_018"];
      _toolTip = SmUtils.SmTags["#smloc_settings_tooltips_tt_018"];
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(gui2xIndent);
      TabLight.ShowToolTips = GUILayout.Toggle(TabLight.ShowToolTips, _guiLabel, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // CW - Hatch Tab
      GUILayout.BeginHorizontal();
      //_label = "Hatch Tab Tool Tips";
      //_toolTip = "Turns tooltips On or Off for the Control Window's Hatch Tab only.";
      //_toolTip += "\r\nRequires global ToolTips setting to be enabled.";
      //_toolTip += "\r\nAlso requires Control Window tooltips to be enabled.";
      _label = SmUtils.SmTags["#smloc_settings_tooltips_013"];
      _toolTip = SmUtils.SmTags["#smloc_settings_tooltips_tt_013"];
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(gui2xIndent);
      TabHatch.ShowToolTips = GUILayout.Toggle(TabHatch.ShowToolTips, _guiLabel, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // CW - Solar Tab
      GUILayout.BeginHorizontal();
      //_label = "Solar Tab Window Tool Tips";
      //_toolTip = "Turns tooltips On or Off for the Control Window's Solar Panels Tab only.";
      //_toolTip += "\r\nRequires global ToolTips setting to be enabled.";
      //_toolTip += "\r\nAlso requires Control Window tooltips to be enabled.";
      _label = SmUtils.SmTags["#smloc_settings_tooltips_014"];
      _toolTip = SmUtils.SmTags["#smloc_settings_tooltips_tt_014"];
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(gui2xIndent);
      TabSolarPanel.ShowToolTips = GUILayout.Toggle(TabSolarPanel.ShowToolTips, _guiLabel, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // CW - Antenna Tab
      GUILayout.BeginHorizontal();
      //_label = "Antenna Tab Tool Tips";
      //_toolTip = "Turns tooltips On or Off for the Control Window's Antennas Tab only.";
      //_toolTip += "\r\nRequires global ToolTips setting to be enabled.";
      //_toolTip += "\r\nAlso requires Control Window tooltips to be enabled.";
      _label = SmUtils.SmTags["#smloc_settings_tooltips_015"];
      _toolTip = SmUtils.SmTags["#smloc_settings_tooltips_tt_015"];
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(gui2xIndent);
      TabAntenna.ShowToolTips = GUILayout.Toggle(TabAntenna.ShowToolTips, _guiLabel, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // CW - Light Tab
      GUILayout.BeginHorizontal();
      //_label = "Light Tab Tool Tips";
      //_toolTip = "Turns tooltips On or Off for the Control Window's Lights Tab only.";
      //_toolTip += "\r\nRequires global ToolTips setting to be enabled.";
      //_toolTip += "\r\nAlso requires Control Window tooltips to be enabled.";
      _label = SmUtils.SmTags["#smloc_settings_tooltips_016"];
      _toolTip = SmUtils.SmTags["#smloc_settings_tooltips_tt_016"];
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(gui2xIndent);
      TabLight.ShowToolTips = GUILayout.Toggle(TabLight.ShowToolTips, _guiLabel, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // CW - Labs Tab
      GUILayout.BeginHorizontal();
      //_label = "Lab Tab Tool Tips";
      //_toolTip = "Turns tooltips On or Off for the Control Window's Lab Tab only.";
      //_toolTip += "\r\nRequires global ToolTips setting to be enabled.";
      //_toolTip += "\r\nAlso requires Control Window tooltips to be enabled.";
      _label = SmUtils.SmTags["#smloc_settings_tooltips_017"];
      _toolTip = SmUtils.SmTags["#smloc_settings_tooltips_tt_017"];
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(gui2xIndent);
      TabScienceLab.ShowToolTips = GUILayout.Toggle(TabScienceLab.ShowToolTips, _guiLabel, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      GUI.enabled = true;
    }
  }
}
