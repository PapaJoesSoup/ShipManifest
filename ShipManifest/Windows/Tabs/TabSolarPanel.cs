using System;
using UnityEngine;

namespace ShipManifest.Windows.Tabs
{
  internal static class TabSolarPanel
  {
    internal static string ToolTip = "";
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;

    internal static void Display(Vector2 displayViewerPosition)
    {
      var scrollX = WindowControl.Position.x + 10;
      var scrollY = WindowControl.Position.y + 50 - displayViewerPosition.y;

      // Reset Tooltip active flag...
      ToolTipActive = false;

      GUILayout.BeginVertical();
      GUI.enabled = true;
      GUILayout.Label("Deployable Solar Panel Control Center ", SMStyle.LabelTabHeader);
      GUILayout.Label("____________________________________________________________________________________________",
        SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(350));
      var step = "start";
      try
      {
        // Display all hatches
        foreach (var iPanel in SMAddon.SmVessel.SolarPanels)
        {
          var isEnabled = true;
          var label = iPanel.PanelStatus + " - " + iPanel.Title;
          if (iPanel.PanelState == ModuleDeployableSolarPanel.panelStates.BROKEN)
          {
            isEnabled = false;
            label = iPanel.PanelStatus + " - (Broken) - " + iPanel.Title;
          }
          var open =
            !(iPanel.PanelState == ModuleDeployableSolarPanel.panelStates.RETRACTED ||
              iPanel.PanelState == ModuleDeployableSolarPanel.panelStates.RETRACTING ||
              iPanel.PanelState == ModuleDeployableSolarPanel.panelStates.BROKEN);

          step = "gui enable";
          GUI.enabled = isEnabled;
          if (!iPanel.CanBeRetracted)
          {
            label = iPanel.PanelStatus + " - (Locked) - " + iPanel.Title;
          }
          var newOpen = GUILayout.Toggle(open, label, GUILayout.Width(325), GUILayout.Height(40));
          step = "button toggle check";
          if (!open && newOpen)
            iPanel.ExtendPanel();
          else if (open && !newOpen)
            iPanel.RetractPanel();

          var rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && rect.Contains(Event.current.mousePosition))
          {
            SMHighlighter.IsMouseOver = true;
            SMHighlighter.MouseOverRect = new Rect(scrollX + rect.x, scrollY + rect.y, rect.width, rect.height);
            SMHighlighter.MouseOverpart = iPanel.SPart;
            SMHighlighter.MouseOverparts = null;
          }
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(
          string.Format(" in Solar Panel Tab at step {0}.  Error:  {1} \r\n\r\n{2}", step, ex.Message, ex.StackTrace),
          "Error", true);
      }
      GUILayout.EndVertical();
    }

    internal static void ExtendAllPanels()
    {
      // TODO: for realism, add a closing/opening sound
      foreach (var iPanel in SMAddon.SmVessel.SolarPanels)
      {
        var iModule = (ModuleDeployableSolarPanel) iPanel.PanelModule;
        if (iModule.panelState == ModuleDeployableSolarPanel.panelStates.RETRACTED)
        {
          iModule.Extend();
        }
      }
    }

    internal static void RetractAllPanels()
    {
      // TODO: for realism, add a closing/opening sound
      foreach (var iPanel in SMAddon.SmVessel.SolarPanels)
      {
        var iModule = (ModuleDeployableSolarPanel) iPanel.PanelModule;
        if (iModule.panelState == ModuleDeployableSolarPanel.panelStates.EXTENDED)
        {
          iModule.Retract();
        }
      }
    }
  }
}