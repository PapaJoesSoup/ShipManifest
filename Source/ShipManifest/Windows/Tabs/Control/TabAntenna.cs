using System;
using System.Collections.Generic;
using ShipManifest.APIClients;
using ShipManifest.InternalObjects;
using ShipManifest.Modules;
using UnityEngine;

namespace ShipManifest.Windows.Tabs.Control
{
  internal static class TabAntenna
  {
    internal static string ToolTip = "";
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;
    internal static bool IsRtAntennas;
    private const float guiRuleWidth = 350;
    private const float guiToggleWidth = 325;

    internal static void Display(Vector2 displayViewerPosition)
    {
      //float scrollX = WindowControl.Position.x;
      //float scrollY = WindowControl.Position.y + 50 - displayViewerPosition.y;
      float scrollX = 0;
      float scrollY = displayViewerPosition.y;

      // Reset Tooltip active flag...
      ToolTipActive = false;
      SMHighlighter.IsMouseOver = false;

      GUILayout.BeginVertical();
      GUI.enabled = true;
      GUILayout.Label(
        //InstalledMods.IsRtInstalled ? "Antenna Control Center  (RemoteTech detected)" : "Antenna Control Center ",
        InstalledMods.IsRtInstalled ? SmUtils.SmTags["#smloc_control_antenna_001"] : SmUtils.SmTags["#smloc_control_antenna_000"],
        SMStyle.LabelTabHeader);
      GUILayout.Label("____________________________________________________________________________________________",
        SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(guiRuleWidth));
      string step = "start";
      try
      {
        // Display all antennas
        List<ModAntenna>.Enumerator iAntennas = SMAddon.SmVessel.Antennas.GetEnumerator();
        while (iAntennas.MoveNext())
        {
          if (iAntennas.Current == null) continue;
          if (!IsRtAntennas && iAntennas.Current.IsRtModule) IsRtAntennas = true;
          step = "get Antenna label";
          string label = $"{iAntennas.Current.AntennaStatus} - {iAntennas.Current.Title}";
          bool open = iAntennas.Current.Extended;
          bool newOpen = GUILayout.Toggle(open, label, GUILayout.Width(guiToggleWidth), GUILayout.Height(40));
          step = "button toggle check";
          if (!open && newOpen)
            iAntennas.Current.ExtendAntenna();
          else if (open && !newOpen)
            iAntennas.Current.RetractAntenna();

          Rect rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && rect.Contains(Event.current.mousePosition))
            SMHighlighter.SetMouseOverData(rect, scrollY, scrollX, WindowControl.TabBox.height, iAntennas.Current.SPart, Event.current.mousePosition);
        }
        iAntennas.Dispose();

        // Display MouseOverHighlighting, if any
        SMHighlighter.MouseOverHighlight();
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage(
          $" in Antenna Tab at step {step}.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}",
          SmUtils.LogType.Error, true);
      }
      GUILayout.EndVertical();
    }

    internal static void ExtendAllAntennas()
    {
      // TODO: for realism, add a closing/opening sound
      List<ModAntenna>.Enumerator iAntennas = SMAddon.SmVessel.Antennas.GetEnumerator();
      while (iAntennas.MoveNext())
      {
        if (iAntennas.Current == null) continue;
        iAntennas.Current.ExtendAntenna();
      }
      iAntennas.Dispose();
    }

    internal static void RetractAllAntennas()
    {
      // TODO: for realism, add a closing/opening sound
      List<ModAntenna>.Enumerator iAntennas = SMAddon.SmVessel.Antennas.GetEnumerator();
      while (iAntennas.MoveNext())
      {
        if (iAntennas.Current == null) continue;
        iAntennas.Current.RetractAntenna();
      }
      iAntennas.Dispose();
    }
  }
}
