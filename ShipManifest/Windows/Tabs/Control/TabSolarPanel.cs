using System;
using System.Collections.Generic;
using ShipManifest.InternalObjects;
using ShipManifest.Modules;
using UnityEngine;

namespace ShipManifest.Windows.Tabs.Control
{
  internal static class TabSolarPanel
  {
    internal static string ToolTip = "";
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;

    internal static void Display(Vector2 displayViewerPosition)
    {
      float scrollX = 10;
      float scrollY = displayViewerPosition.y;

      // Reset Tooltip active flag...
      ToolTipActive = false;
      SMHighlighter.IsMouseOver = false;

      GUILayout.BeginVertical();
      GUI.enabled = true;
      //GUILayout.Label("Deployable Solar Panel Control Center ", SMStyle.LabelTabHeader);
      GUILayout.Label(SmUtils.Localize("#smloc_control_panel_000"), SMStyle.LabelTabHeader);
      GUILayout.Label("____________________________________________________________________________________________",
        SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(350));
      string step = "start";
      try
      {
        // Display all hatches
        List<ModSolarPanel>.Enumerator iPanels = SMAddon.SmVessel.SolarPanels.GetEnumerator();
        while (iPanels.MoveNext())
        {
          if (iPanels.Current == null) continue;
          bool isEnabled = true;
          string label = $"{iPanels.Current.PanelStatus} - {iPanels.Current.Title}";
          if (iPanels.Current.PanelState == ModuleDeployablePart.DeployState.BROKEN)
          {
            isEnabled = false;
            label = $"{iPanels.Current.PanelStatus} - ({SmUtils.Localize("#smloc_module_004")}) - {iPanels.Current.Title}"; // "Broken"
          }
          bool open =
            !(iPanels.Current.PanelState == ModuleDeployablePart.DeployState.RETRACTED ||
              iPanels.Current.PanelState == ModuleDeployablePart.DeployState.RETRACTING ||
              iPanels.Current.PanelState == ModuleDeployablePart.DeployState.BROKEN);

          step = "gui enable";
          GUI.enabled = isEnabled;
          if (!iPanels.Current.CanBeRetracted)
          {
            label = $"{iPanels.Current.PanelStatus} - ({SmUtils.Localize("#smloc_module_005")}) - {iPanels.Current.Title}"; // "Locked"
          }
          bool newOpen = GUILayout.Toggle(open, label, GUILayout.Width(325), GUILayout.Height(40));
          step = "button toggle check";
          if (!open && newOpen)
            iPanels.Current.ExtendPanel();
          else if (open && !newOpen)
            iPanels.Current.RetractPanel();

          Rect rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && rect.Contains(Event.current.mousePosition))
            SMHighlighter.SetMouseOverData(rect, scrollY, scrollX, WindowControl.TabBox.height, iPanels.Current.SPart);
        }
        iPanels.Dispose();

        // Display MouseOverHighlighting, if any
        SMHighlighter.MouseOverHighlight();
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage(
          $" in Solar Panel Tab at step {step}.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}",
          SmUtils.LogType.Error, true);
      }
      GUILayout.EndVertical();
    }

    internal static void ExtendAllPanels()
    {
      // TODO: for realism, add a closing/opening sound
      List<ModSolarPanel>.Enumerator iPanels = SMAddon.SmVessel.SolarPanels.GetEnumerator();
      while (iPanels.MoveNext())
      {
        if (iPanels.Current == null) continue;
        if (((ModuleDeployableSolarPanel)iPanels.Current.PanelModule).deployState != ModuleDeployablePart.DeployState.RETRACTED) continue;
        ((ModuleDeployableSolarPanel)iPanels.Current.PanelModule).Extend();
      }
      iPanels.Dispose();
    }

    internal static void RetractAllPanels()
    {
      // TODO: for realism, add a closing/opening sound
      List<ModSolarPanel>.Enumerator iPanels = SMAddon.SmVessel.SolarPanels.GetEnumerator();
      while (iPanels.MoveNext())
      {
        if (iPanels.Current == null) continue;
        if (((ModuleDeployableSolarPanel)iPanels.Current.PanelModule).deployState != ModuleDeployablePart.DeployState.EXTENDED) continue;
        ((ModuleDeployableSolarPanel)iPanels.Current.PanelModule).Retract();
      }
      iPanels.Dispose();
    }
  }
}