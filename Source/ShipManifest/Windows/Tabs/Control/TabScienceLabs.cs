using System;
using System.Collections.Generic;
using ShipManifest.InternalObjects;
using ShipManifest.InternalObjects.Settings;
using UnityEngine;

namespace ShipManifest.Windows.Tabs.Control
{
  internal static class TabScienceLab
  {

    static TabScienceLab()
    {
      RefreshUIScale();
    }

    // UIScale settings
    internal static float guiLabelWidth;
    internal static float guiLabelHeight;

    // ToolTip vars
    internal static string ToolTip = "";
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;

    //Content vars
    internal static GUIContent titleContent = new GUIContent(SmUtils.SmTags["#smloc_control_radiator_000"]);
    internal static string opContent    = SmUtils.SmTags["#smloc_control_lab_001"];
    internal static string inopContent  = SmUtils.SmTags["#smloc_control_lab_002"];
    

    internal static void Display()
    {

      //float scrollX = WindowControl.Position.x + 10;
      //float scrollY = WindowControl.Position.y + 50 - displayViewerPosition.y;
      float scrollX = 10;
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
        // Display all Labs
        List<ModuleScienceLab>.Enumerator iLabs = SMAddon.SmVessel.Labs.GetEnumerator();
        while (iLabs.MoveNext())
        {
          if (iLabs.Current == null) continue;

          step = "gui enable";
          GUI.enabled = true;
          string label = $"{iLabs.Current.name} - ({(iLabs.Current.IsOperational() ? opContent : inopContent)})"; // Operational, InOp
          GUILayout.Label(label, SMStyle.SMSkin.label, GUILayout.Width(guiLabelWidth + WindowControl.WidthScale), GUILayout.Height(guiLabelHeight));

          Rect rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && rect.Contains(Event.current.mousePosition))
            SMHighlighter.SetMouseOverData(rect, scrollY, scrollX, WindowControl.TabBox.height + WindowControl.HeightScale, iLabs.Current.part, Event.current.mousePosition);
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

    internal static void RefreshUIScale()
    {
      guiLabelWidth = 260 * CurrSettings.CurrentUIScale;
      guiLabelHeight = 40 * CurrSettings.CurrentUIScale;
    }

  }
}
