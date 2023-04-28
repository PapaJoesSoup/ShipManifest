using System;
using System.Collections.Generic;
using ShipManifest.InternalObjects;
using ShipManifest.InternalObjects.Settings;
using ShipManifest.Modules;
using UnityEngine;

namespace ShipManifest.Windows.Tabs.Control
{
  internal static class TabLight
  {

    static TabLight()
    {
      RefreshUIScale();
    }

    // UIScale settings
    internal static float guiToggleWidth;
    internal static float guiToggleHeight;

    // TooTip vars
    internal static string ToolTip = "";
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;

    //Content vars
    internal static GUIContent titleContent = new GUIContent(SmUtils.SmTags["#smloc_control_light_000"]);


    internal static void Display()
    {

      //float scrollX = WindowControl.Position.x + 20;
      //float scrollY = WindowControl.Position.y + 50 - displayViewerPosition.y;
      float scrollX = 20;
      float scrollY = WindowControl._displayViewerPosition.y;

      // Reset Tooltip active flag...
      ToolTipActive = false;
      SMHighlighter.IsMouseOver = false;

      GUILayout.BeginVertical();
      GUI.enabled = true;
      GUILayout.Label(titleContent, SMStyle.LabelTabHeader);
      GUILayout.Label(WindowControl.TabRule, SMStyle.LabelStyleHardRule, GUILayout.Height(WindowControl.GuiRuleHeight), GUILayout.Width(WindowControl.GuiRuleWidth + WindowControl.WidthScale));
      string step = "start";
      try
      {
        // Display all Lights
        List<ModLight>.Enumerator iLights = SMAddon.SmVessel.Lights.GetEnumerator();
        while (iLights.MoveNext())
        {
          if (iLights.Current == null) continue;
          string label = $"{iLights.Current.Status} - {iLights.Current.Title}";
          bool onState = iLights.Current.IsOn;
          bool newOnState = GUILayout.Toggle(onState, label, SMStyle.SMSkin.toggle, GUILayout.Width(guiToggleWidth), GUILayout.Height(guiToggleHeight + WindowControl.WidthScale));
          step = "button toggle check";
          if (!onState && newOnState)
            iLights.Current.TurnOnLight();
          else if (onState && !newOnState)
            iLights.Current.TurnOffLight();
          Rect rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && rect.Contains(Event.current.mousePosition))
            SMHighlighter.SetMouseOverData(rect, scrollY, scrollX, WindowControl.TabBox.height + WindowControl.HeightScale, iLights.Current.SPart, Event.current.mousePosition);
        }
        iLights.Dispose();

        // Display MouseOverHighlighting, if any
        SMHighlighter.MouseOverHighlight();
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage(
          $" in Light Tab at step {step}.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}", SmUtils.LogType.Error,
          true);
      }
      GUILayout.EndVertical();
    }

    internal static void TurnOnAllLights()
    {
      // iterate thru the hatch parts and open hatches
      List<ModLight>.Enumerator iLights = SMAddon.SmVessel.Lights.GetEnumerator();
      while (iLights.MoveNext())
      {
        if (iLights.Current == null) continue;
        iLights.Current.TurnOnLight();
      }
      iLights.Dispose();
    }

    internal static void TurnOffAllLights()
    {
      // iterate thru the hatch parts and open hatches
      List<ModLight>.Enumerator iLights = SMAddon.SmVessel.Lights.GetEnumerator();
      while (iLights.MoveNext())
      {
        if (iLights.Current == null) continue;
        iLights.Current.TurnOffLight();
      }
      iLights.Dispose();
    }

    internal static void RefreshUIScale()
    {
      guiToggleWidth = 325 * CurrSettings.CurrentUIScale;
      guiToggleHeight = 40 * CurrSettings.CurrentUIScale;
    }

  }
}
