using ShipManifest.InternalObjects;
using UnityEngine;

namespace ShipManifest.Windows.Tabs
{
  internal static class TabToolTips
  {
    internal static string StrFlowCost = "0";

    // GUI tooltip and label support
    private static string _toolTip = "";
    private static Rect _rect;
    private static string _label = "";
    private static GUIContent _guiLabel;

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
      GUILayout.Label("ToolTips", SMStyle.LabelTabHeader);
      GUILayout.Label("____________________________________________________________________________________________",
        SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(350));

      _label = "Enable Tool Tips";
      _toolTip = "Turns tooltips On or Off.";
      _toolTip += "\r\nThis is a global setting for all windows/tabs";
      _guiLabel = new GUIContent(_label, _toolTip);
      SMSettings.ShowToolTips = GUILayout.Toggle(SMSettings.ShowToolTips, _guiLabel, GUILayout.Width(300));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      GUI.enabled = SMSettings.ShowToolTips;

      // Debugger Window
      GUILayout.BeginHorizontal();
      _label = "Debugger Window Tool Tips";
      _toolTip = "Turns tooltips On or Off for the Debugger Window only.";
      _toolTip += "\r\nRequires global ToolTips setting to be enabled.";
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(20);
      WindowDebugger.ShowToolTips = GUILayout.Toggle(WindowDebugger.ShowToolTips, _guiLabel, GUILayout.Width(300));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // Manifest Window
      GUILayout.BeginHorizontal();
      _label = "Manifest Window Tool Tips";
      _toolTip = "Turns tooltips On or Off for the Manifest Window only.";
      _toolTip += "\r\nRequires global ToolTips setting to be enabled.";
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(20);
      WindowManifest.ShowToolTips = GUILayout.Toggle(WindowManifest.ShowToolTips, _guiLabel, GUILayout.Width(300));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // Transfer Window
      GUILayout.BeginHorizontal();
      _label = "Transfer Window Tool Tips";
      _toolTip = "Turns tooltips On or Off for the Manifest Window only.";
      _toolTip += "\r\nRequires global ToolTips setting to be enabled.";
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(20);
      WindowTransfer.ShowToolTips = GUILayout.Toggle(WindowTransfer.ShowToolTips, _guiLabel, GUILayout.Width(300));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // Setting Window
      GUILayout.BeginHorizontal();
      _label = "Settings Window Tool Tips";
      _toolTip = "Turns tooltips On or Off for the Settings Window only.";
      _toolTip += "\r\nRequires global ToolTips setting to be enabled.";
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(20);
      WindowSettings.ShowToolTips = GUILayout.Toggle(WindowSettings.ShowToolTips, _guiLabel, GUILayout.Width(300));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      GUI.enabled = SMSettings.ShowToolTips && WindowSettings.ShowToolTips;

      // SW - Realism Tab
      GUILayout.BeginHorizontal();
      _label = "Realism Tab Tool Tips";
      _toolTip = "Turns tooltips On or Off for the Settings Window's Realism Tab only.";
      _toolTip += "\r\nRequires global ToolTips setting to be enabled.";
      _toolTip += "\r\nAlso requires Settings Window tooltips to be enabled.";
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(40);
      TabRealism.ShowToolTips = GUILayout.Toggle(TabRealism.ShowToolTips, _guiLabel, GUILayout.Width(300));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // SW - Highlighting Tab
      GUILayout.BeginHorizontal();
      _label = "Highlghting Tab Tool Tips";
      _toolTip = "Turns tooltips On or Off for the Settings Window's Highlighting Tab only.";
      _toolTip += "\r\nRequires global ToolTips setting to be enabled.";
      _toolTip += "\r\nAlso requires Settings Window tooltips to be enabled.";
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(40);
      TabHighlight.ShowToolTips = GUILayout.Toggle(TabHighlight.ShowToolTips, _guiLabel, GUILayout.Width(300));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // SW - ToolTips Tab
      GUILayout.BeginHorizontal();
      _label = "ToolTips Tab Tool Tips";
      _toolTip = "Turns tooltips On or Off for the Settings Window's ToolTips Tab only.";
      _toolTip += "\r\nRequires global ToolTips setting to be enabled.";
      _toolTip += "\r\nAlso requires Settings Window tooltips to be enabled.";
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(40);
      ShowToolTips = GUILayout.Toggle(ShowToolTips, _guiLabel, GUILayout.Width(300));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // SW - Sounds Tab
      GUILayout.BeginHorizontal();
      _label = "Sounds Tab Tool Tips";
      _toolTip = "Turns tooltips On or Off for the Settings Window's Sounds Tab only.";
      _toolTip += "\r\nRequires global ToolTips setting to be enabled.";
      _toolTip += "\r\nAlso requires Settings Window tooltips to be enabled.";
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(40);
      TabLight.ShowToolTips = GUILayout.Toggle(TabLight.ShowToolTips, _guiLabel, GUILayout.Width(300));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // SW - Config Tab
      GUILayout.BeginHorizontal();
      _label = "Config Tab Tool Tips";
      _toolTip = "Turns tooltips On or Off for the Settings Window's Config Tab only.";
      _toolTip += "Requires global ToolTips setting to be enabled.";
      _toolTip += "\r\nAlso requires Settings Window tooltips to be enabled.";
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(40);
      TabConfig.ShowToolTips = GUILayout.Toggle(TabConfig.ShowToolTips, _guiLabel, GUILayout.Width(300));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // SW - Installed Mods Tab
      GUILayout.BeginHorizontal();
      _label = "Installed Mods Tool Tips";
      _toolTip = "Turns tooltips On or Off for the Settings Window's Installed Mods Tab only.";
      _toolTip += "\r\nRequires global ToolTips setting to be enabled.";
      _toolTip += "\r\nAlso requires Settings Window tooltips to be enabled.";
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(40);
      TabInstalledMods.ShowToolTips = GUILayout.Toggle(TabInstalledMods.ShowToolTips, _guiLabel, GUILayout.Width(300));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      GUI.enabled = SMSettings.ShowToolTips;

      // Roster Window
      GUILayout.BeginHorizontal();
      _label = "Roster Window Tool Tips";
      _toolTip = "Turns tooltips On or Off for the Roster Window only.";
      _toolTip += "Requires global ToolTips setting to be enabled.";
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(20);
      WindowRoster.ShowToolTips = GUILayout.Toggle(WindowRoster.ShowToolTips, _guiLabel, GUILayout.Width(300));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // Control Window
      GUILayout.BeginHorizontal();
      _label = "Control Window Tool Tips";
      _toolTip = "Turns tooltips On or Off for the Control Window only.";
      _toolTip += "\r\nRequires global ToolTips setting to be enabled.";
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(20);
      WindowControl.ShowToolTips = GUILayout.Toggle(WindowControl.ShowToolTips, _guiLabel, GUILayout.Width(300));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      GUI.enabled = SMSettings.ShowToolTips && WindowControl.ShowToolTips;

      // CW - Hatch Tab
      GUILayout.BeginHorizontal();
      _label = "Hatch Tab Tool Tips";
      _toolTip = "Turns tooltips On or Off for the Control Window's Hatch Tab only.";
      _toolTip += "\r\nRequires global ToolTips setting to be enabled.";
      _toolTip += "\r\nAlso requires Control Window tooltips to be enabled.";
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(40);
      TabHatch.ShowToolTips = GUILayout.Toggle(TabHatch.ShowToolTips, _guiLabel, GUILayout.Width(300));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // CW - Solar Tab
      GUILayout.BeginHorizontal();
      _label = "Solar Tab Window Tool Tips";
      _toolTip = "Turns tooltips On or Off for the Control Window's Solar Panels Tab only.";
      _toolTip += "\r\nRequires global ToolTips setting to be enabled.";
      _toolTip += "\r\nAlso requires Control Window tooltips to be enabled.";
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(40);
      TabSolarPanel.ShowToolTips = GUILayout.Toggle(TabSolarPanel.ShowToolTips, _guiLabel, GUILayout.Width(300));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // CW - Antenna Tab
      GUILayout.BeginHorizontal();
      _label = "Antenna Tab Tool Tips";
      _toolTip = "Turns tooltips On or Off for the Control Window's Antennas Tab only.";
      _toolTip += "\r\nRequires global ToolTips setting to be enabled.";
      _toolTip += "\r\nAlso requires Control Window tooltips to be enabled.";
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(40);
      TabAntenna.ShowToolTips = GUILayout.Toggle(TabAntenna.ShowToolTips, _guiLabel, GUILayout.Width(300));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // CW - Light Tab
      GUILayout.BeginHorizontal();
      _label = "Light Tab Tool Tips";
      _toolTip = "Turns tooltips On or Off for the Control Window's Lights Tab only.";
      _toolTip += "\r\nRequires global ToolTips setting to be enabled.";
      _toolTip += "\r\nAlso requires Control Window tooltips to be enabled.";
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(40);
      TabLight.ShowToolTips = GUILayout.Toggle(TabLight.ShowToolTips, _guiLabel, GUILayout.Width(300));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      GUI.enabled = true;
    }
  }
}