using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ConnectedLivingSpace;

namespace ShipManifest
{
  static class TabLight
  {
    internal static string ToolTip = "";
    internal static bool ToolTipActive = false;
    internal static bool ShowToolTips = true;

    internal static void Display(Vector2 DisplayViewerPosition)
    {
      float scrollX = WindowControl.Position.x + 20;
      float scrollY = WindowControl.Position.y + 50 - DisplayViewerPosition.y;
      // Reset Tooltip active flag...
      ToolTipActive = false;

      GUILayout.BeginVertical();
      GUI.enabled = true;
      GUILayout.Label("External Light Control Center ", SMStyle.LabelTabHeader);
      GUILayout.Label("____________________________________________________________________________________________", SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(350));
      string step = "start";
      try
      {
        // Display all Lights
        foreach (ModLight iLight in SMAddon.smController.Lights)
        {
          string label = iLight.Status + " - " + iLight.Title;
          bool OnState = iLight.isOn;
          bool newOnState = GUILayout.Toggle(OnState, label, GUILayout.Width(325), GUILayout.Height(40));
          step = "button toggle check";
          if (!OnState && newOnState)
            iLight.TurnOnLight();
          else if (OnState && !newOnState)
            iLight.TurnOffLight();
          Rect rect = GUILayoutUtility.GetLastRect();
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
      foreach (ModLight iLight in SMAddon.smController.Lights)
      {
        iLight.TurnOnLight();
      }
    }

    internal static void TurnOffAllLights()
    {
      // iterate thru the hatch parts and open hatches
      foreach (ModLight iLight in SMAddon.smController.Lights)
      {
        iLight.TurnOffLight();
      }
    }

  }
}
