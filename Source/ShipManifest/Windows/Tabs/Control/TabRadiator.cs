using System;
using System.Collections.Generic;
using ShipManifest.InternalObjects;
using ShipManifest.Modules;
using UnityEngine;

namespace ShipManifest.Windows.Tabs.Control
{
  internal static class TabRadiator
  {
    internal static string ToolTip = "";
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;
    private const float guiRuleWidth = 350;
    private const float guiToggleWidth = 325;

    internal static void Display(Vector2 displayViewerPosition)
    {
      float scrollX = 10;
      float scrollY = displayViewerPosition.y;

      // Reset Tooltip active flag...
      ToolTipActive = false;
      SMHighlighter.IsMouseOver = false;

      GUILayout.BeginVertical();
      GUI.enabled = true;
      //GUILayout.Label("Deployable Radiator Control Center ", SMStyle.LabelTabHeader);
      GUILayout.Label(SmUtils.SmTags["#smloc_control_radiator_000"], SMStyle.LabelTabHeader);
      GUILayout.Label("____________________________________________________________________________________________",
        SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(guiRuleWidth));
      string step = "start";
      try
      {
        // Display all Radiators
        List<ModRadiator>.Enumerator iRadiators = SMAddon.SmVessel.Radiators.GetEnumerator();
        while (iRadiators.MoveNext())
        {
          if (iRadiators.Current == null) continue;
          bool isEnabled = true;
          string label = $"{iRadiators.Current.PanelStatus} - {iRadiators.Current.Title}";
          if (iRadiators.Current.PanelState == ModuleDeployablePart.DeployState.BROKEN)
          {
            isEnabled = false;
            label = $"{iRadiators.Current.PanelStatus} - ({SmUtils.SmTags["#smloc_module_004"]}) - {iRadiators.Current.Title}"; // "Broken"
          }
          bool open =
            !(iRadiators.Current.PanelState == ModuleDeployablePart.DeployState.RETRACTED ||
              iRadiators.Current.PanelState == ModuleDeployablePart.DeployState.RETRACTING ||
              iRadiators.Current.PanelState == ModuleDeployablePart.DeployState.BROKEN);

          step = "gui enable";
          GUI.enabled = isEnabled;
          if (!iRadiators.Current.CanBeRetracted)
          {
            label = $"{iRadiators.Current.PanelStatus} - ({SmUtils.SmTags["#smloc_module_005"]}) - {iRadiators.Current.Title}"; // "Locked"
          }
          bool newOpen = GUILayout.Toggle(open, label, GUILayout.Width(guiToggleWidth), GUILayout.Height(40));
          step = "button toggle check";
          if (!open && newOpen)
            iRadiators.Current.ExtendPanel();
          else if (open && !newOpen)
            iRadiators.Current.RetractPanel();

          Rect rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && rect.Contains(Event.current.mousePosition))
            SMHighlighter.SetMouseOverData(rect, scrollY, scrollX, WindowControl.TabBox.height, iRadiators.Current.SPart, Event.current.mousePosition);
        }
        iRadiators.Dispose();

        // Display MouseOverHighlighting, if any
        SMHighlighter.MouseOverHighlight();
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage(
          $" in Radiators Tab at step {step}.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}",
          SmUtils.LogType.Error, true);
      }
      GUILayout.EndVertical();
    }

    internal static void ExtendAllPanels()
    {
      // TODO: for realism, add a closing/opening sound
      List<ModRadiator>.Enumerator iRadiators = SMAddon.SmVessel.Radiators.GetEnumerator();
      while (iRadiators.MoveNext())
      {
        if (iRadiators.Current == null) continue;
        if (((ModuleDeployableRadiator)iRadiators.Current.PanelModule).deployState != ModuleDeployablePart.DeployState.RETRACTED) continue;
        ((ModuleDeployableRadiator)iRadiators.Current.PanelModule).Extend();
      }
      iRadiators.Dispose();
    }

    internal static void RetractAllPanels()
    {
      // TODO: for realism, add a closing/opening sound
      List<ModRadiator>.Enumerator iRadiators = SMAddon.SmVessel.Radiators.GetEnumerator();
      while (iRadiators.MoveNext())
      {
        if (iRadiators.Current == null) continue;
        if (((ModuleDeployableRadiator)iRadiators.Current.PanelModule).deployState != ModuleDeployablePart.DeployState.EXTENDED) continue;
        ((ModuleDeployableRadiator)iRadiators.Current.PanelModule).Retract();
      }
      iRadiators.Dispose();
    }
  }
}
