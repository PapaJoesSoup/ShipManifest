using System;
using System.Collections.Generic;
using ShipManifest.InternalObjects;
using UnityEngine;

namespace ShipManifest.Windows.Tabs.Control
{
  internal static class TabScienceLab
  {
    internal static string ToolTip = "";
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;
    private const float guiLabelWidth = 260;

    internal static void Display(Rect tabBox)
    {
      //float scrollX = WindowControl.Position.x + 10;
      //float scrollY = WindowControl.Position.y + 50 - displayViewerPosition.y;
      float scrollX = 10;
      float scrollY = 50 - tabBox.y;

      // Reset Tooltip active flag...
      ToolTipActive = false;
      SMHighlighter.IsMouseOver = false;

      GUILayout.BeginVertical();
      GUI.enabled = true;
      //GUILayout.Label("Science Lab Control Center ", SMStyle.LabelTabHeader);
      GUILayout.Label(SmUtils.SmTags["#smloc_control_lab_000"], SMStyle.LabelTabHeader);
      GUILayout.Label(WindowControl.TabRule, SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(WindowControl.GuiRuleWidth));
      string step = "start";
      try
      {
        // Display all Labs
        List<ModuleScienceLab>.Enumerator iLabs = SMAddon.SmVessel.Labs.GetEnumerator();
        while (iLabs.MoveNext())
        {
          if (iLabs.Current == null) continue;

          step = "gui enable";
          GUI.enabled = true;
          string label = $"{iLabs.Current.name} - ({(iLabs.Current.IsOperational() ? SmUtils.SmTags["#smloc_control_lab_001"] : SmUtils.SmTags["#smloc_control_lab_002"])})"; // Operational, InOp
          GUILayout.Label(label, GUILayout.Width(guiLabelWidth), GUILayout.Height(40));

          Rect rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && rect.Contains(Event.current.mousePosition))
            SMHighlighter.SetMouseOverData(rect, scrollY, scrollX, tabBox.height, iLabs.Current.part, Event.current.mousePosition);
        }
        iLabs.Dispose();

        // Display MouseOverHighlighting, if any
        SMHighlighter.MouseOverHighlight();
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage(
          $" in Science Labs Tab at step {step}.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}",
          SmUtils.LogType.Error, true);
      }
      GUILayout.EndVertical();
    }
  }
}
