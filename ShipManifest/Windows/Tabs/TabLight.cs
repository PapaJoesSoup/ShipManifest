using System;
using UnityEngine;

namespace ShipManifest.Windows.Tabs
{
  static class TabLight
  {
    internal static string ToolTip = "";
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;

    internal static void Display(Vector2 displayViewerPosition)
    {
      var scrollX = WindowControl.Position.x + 20;
      var scrollY = WindowControl.Position.y + 50 - displayViewerPosition.y;
      // Reset Tooltip active flag...
      ToolTipActive = false;

      GUILayout.BeginVertical();
      GUI.enabled = true;
      GUILayout.Label("External Light Control Center ", SMStyle.LabelTabHeader);
      GUILayout.Label("____________________________________________________________________________________________", SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(350));
      var step = "start";
      try
      {
        // Display all Lights
        foreach (var iLight in SMAddon.SmVessel.Lights)
        {
          var label = iLight.Status + " - " + iLight.Title;
          var onState = iLight.IsOn;
          var newOnState = GUILayout.Toggle(onState, label, GUILayout.Width(325), GUILayout.Height(40));
          step = "button toggle check";
          if (!onState && newOnState)
            iLight.TurnOnLight();
          else if (onState && !newOnState)
            iLight.TurnOffLight();
          var rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && rect.Contains(Event.current.mousePosition))
          {
            SMHighlighter.IsMouseOver = true;
            SMHighlighter.MouseOverRect = new Rect(scrollX + rect.x, scrollY + rect.y, rect.width, rect.height);
            SMHighlighter.MouseOverpart = iLight.SPart;
            SMHighlighter.MouseOverparts = null;
          }
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format(" in Light Tab at step {0}.  Error:  {1} \r\n\r\n{2}", step, ex.Message, ex.StackTrace), "Error", true);
      }
      GUILayout.EndVertical();
    }

    internal static void TurnOnAllLights()
    {
      // iterate thru the hatch parts and open hatches
      foreach (var iLight in SMAddon.SmVessel.Lights)
      {
        iLight.TurnOnLight();
      }
    }

    internal static void TurnOffAllLights()
    {
      // iterate thru the hatch parts and open hatches
      foreach (var iLight in SMAddon.SmVessel.Lights)
      {
        iLight.TurnOffLight();
      }
    }

  }
}
