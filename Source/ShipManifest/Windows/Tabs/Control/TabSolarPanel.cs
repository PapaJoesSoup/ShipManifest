using System;
using System.Collections.Generic;
using ShipManifest.InternalObjects;
using ShipManifest.InternalObjects.Settings;
using ShipManifest.Modules;
using UnityEngine;

namespace ShipManifest.Windows.Tabs.Control
{
  internal static class TabSolarPanel
  {

    static TabSolarPanel()
    {
      RefreshUIScale();
    }

    // UIScale settings
    internal static float guiToggleWidth = 325 * CurrSettings.CurrentUIScale;
    internal static float guiToggleHeight = 40 * CurrSettings.CurrentUIScale;

    // TooTip vars
    internal static string ToolTip = "";
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;


    //Content vars
    internal static GUIContent titleContent   = new GUIContent(SmUtils.SmTags["#smloc_control_panel_000"]);
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
      GUILayout.Label(titleContent, SMStyle.LabelTabHeader);
      GUILayout.Label(WindowControl.TabRule, SMStyle.LabelStyleHardRule, GUILayout.Height(WindowControl.GuiRuleHeight), GUILayout.Width(WindowControl.GuiRuleWidth + WindowControl.WidthScale));
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
            label = $"{iPanels.Current.PanelStatus} - ({brokenContent}) - {iPanels.Current.Title}"; // "Broken"
          }
          bool open =
            !(iPanels.Current.PanelState == ModuleDeployablePart.DeployState.RETRACTED ||
              iPanels.Current.PanelState == ModuleDeployablePart.DeployState.RETRACTING ||
              iPanels.Current.PanelState == ModuleDeployablePart.DeployState.BROKEN);

          step = "gui enable";
          GUI.enabled = isEnabled;
          if (!iPanels.Current.CanBeRetracted)
          {
            label = $"{iPanels.Current.PanelStatus} - ({lockedContent}) - {iPanels.Current.Title}"; // "Locked"
          }
          bool newOpen = GUILayout.Toggle(open, label, SMStyle.SMSkin.toggle, GUILayout.Width(guiToggleWidth + WindowControl.WidthScale), GUILayout.Height(guiToggleHeight));
          Rect rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && rect.Contains(Event.current.mousePosition))
            SMHighlighter.SetMouseOverData(rect, scrollY, scrollX, WindowControl.TabBox.height + WindowControl.HeightScale, iPanels.Current.SPart, Event.current.mousePosition);
          step = "button toggle check";
          if (!open && newOpen)
            iPanels.Current.ExtendPanel();
          else if (open && !newOpen)
            iPanels.Current.RetractPanel();

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

    internal static void RefreshUIScale()
    {
      guiToggleWidth = 325 * CurrSettings.CurrentUIScale;
      guiToggleHeight = 40 * CurrSettings.CurrentUIScale;
    }

  }
}
