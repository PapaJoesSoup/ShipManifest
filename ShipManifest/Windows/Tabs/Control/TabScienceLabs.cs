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

    internal static void Display(Vector2 displayViewerPosition)
    {
      //float scrollX = WindowControl.Position.x + 10;
      //float scrollY = WindowControl.Position.y + 50 - displayViewerPosition.y;
      float scrollX = 10;
      float scrollY = 50 - displayViewerPosition.y;

      // Reset Tooltip active flag...
      ToolTipActive = false;
      SMHighlighter.IsMouseOver = false;

      GUILayout.BeginVertical();
      GUI.enabled = true;
      //GUILayout.Label("Science Lab Control Center ", SMStyle.LabelTabHeader);
      GUILayout.Label(SMUtils.Localize("#smloc_control_lab_000"), SMStyle.LabelTabHeader);
      GUILayout.Label("____________________________________________________________________________________________",
        SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(350));
      string step = "start";
      try
      {
        // Display all Labs
        List<ModuleScienceLab>.Enumerator iLabs = SMAddon.SmVessel.Labs.GetEnumerator();
        while (iLabs.MoveNext())
        {
          if (iLabs.Current == null) continue;
          bool isEnabled = true;

          step = "gui enable";
          GUI.enabled = isEnabled;
          string label = iLabs.Current.name + " - (" + (iLabs.Current.IsOperational() ? SMUtils.Localize("#smloc_control_lab_001") : SMUtils.Localize("#smloc_control_lab_002")) + ")"; // Operational, InOp
          GUILayout.Label(label, GUILayout.Width(260), GUILayout.Height(40));

          Rect rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && rect.Contains(Event.current.mousePosition))
          {
            SMHighlighter.IsMouseOver = true;
            SMHighlighter.MouseOverRect = new Rect(scrollX + rect.x, scrollY + rect.y, rect.width, rect.height);
            SMHighlighter.MouseOverPart = iLabs.Current.part;
            SMHighlighter.MouseOverParts = null;
          }
        }
        iLabs.Dispose();

        // Display MouseOverHighlighting, if any
        SMHighlighter.MouseOverHighlight();
      }
      catch (Exception ex)
      {
        SMUtils.LogMessage(
          string.Format(" in Solar Panel Tab at step {0}.  Error:  {1} \r\n\r\n{2}", step, ex.Message, ex.StackTrace),
          SMUtils.LogType.Error, true);
      }
      GUILayout.EndVertical();
    }
  }
}