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
    private const float guiToggleWidth = 325;

    //Content vars
    internal static GUIContent titleContent   = new GUIContent(SmUtils.SmTags["#smloc_control_radiator_000"]);
    internal static GUIContent brokenContent  = new GUIContent(SmUtils.SmTags["#smloc_module_004"]);
    internal static GUIContent lockedContent  = new GUIContent(SmUtils.SmTags["#smloc_module_005"]);


    internal static void Display()
    {
      float scrollX = 10;
      float scrollY = WindowControl._displayViewerPosition.y;

      // Reset Tooltip active flag...
      ToolTipActive = false;
      SMHighlighter.IsMouseOver = false;

      GUILayout.BeginVertical();
      GUI.enabled = true;
      //GUILayout.Label("Deployable Radiator Control Center ", SMStyle.LabelTabHeader);
      GUILayout.Label(titleContent, SMStyle.LabelTabHeader);
      GUILayout.Label(WindowControl.TabRule, SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(WindowControl.GuiRuleWidth));
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
            label = $"{iRadiators.Current.PanelStatus} - ({brokenContent}) - {iRadiators.Current.Title}"; // "Broken"
          }
          bool open =
            !(iRadiators.Current.PanelState == ModuleDeployablePart.DeployState.RETRACTED ||
              iRadiators.Current.PanelState == ModuleDeployablePart.DeployState.RETRACTING ||
              iRadiators.Current.PanelState == ModuleDeployablePart.DeployState.BROKEN);

          step = "gui enable";
          GUI.enabled = isEnabled;
          if (!iRadiators.Current.CanBeRetracted)
          {
            label = $"{iRadiators.Current.PanelStatus} - ({lockedContent}) - {iRadiators.Current.Title}"; // "Locked"
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

    internal static void ExtendAllRadiators()
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

    internal static void RetractAllRadiators()
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
