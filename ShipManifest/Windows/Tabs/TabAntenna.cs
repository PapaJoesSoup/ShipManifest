using System;
using ShipManifest.APIClients;
using UnityEngine;

namespace ShipManifest.Windows.Tabs
{
  static class TabAntenna
  {
    internal static string ToolTip = "";
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;
    internal static bool IsRtAntennas;

    internal static void Display(Vector2 displayViewerPosition)
    {
      var scrollX = WindowControl.Position.x;
      var scrollY = WindowControl.Position.y + 50 - displayViewerPosition.y;

      // Reset Tooltip active flag...
      ToolTipActive = false;

      GUILayout.BeginVertical();
      GUI.enabled = true;
      GUILayout.Label(
        InstalledMods.IsRtInstalled ? "Antenna Control Center  (RemoteTech detected)" : "Antenna Control Center ",
        SMStyle.LabelTabHeader);
      GUILayout.Label("____________________________________________________________________________________________", SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(350));
      var step = "start";
      try
      {
        // Display all antennas
        foreach (var iAntenna in SMAddon.SmVessel.Antennas)
        {
          if (!IsRtAntennas && iAntenna.IsRtModule)
            IsRtAntennas = true;
          step = "get Antenna label";
          var label = iAntenna.AntennaStatus + " - " + iAntenna.Title;
          var open = iAntenna.Extended;
          var newOpen = GUILayout.Toggle(open, label, GUILayout.Width(325), GUILayout.Height(40));
          step = "button toggle check";
          if (!open && newOpen)
            iAntenna.ExtendAntenna();
          else if (open && !newOpen)
            iAntenna.RetractAntenna();

          var rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && rect.Contains(Event.current.mousePosition))
          {
            SMHighlighter.IsMouseOver = true;
            SMHighlighter.MouseOverRect = new Rect(scrollX + rect.x, scrollY + rect.y, rect.width, rect.height);
            SMHighlighter.MouseOverpart = iAntenna.SPart;
            SMHighlighter.MouseOverparts = null;
          }
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format(" in Antenna Tab at step {0}.  Error:  {1} \r\n\r\n{2}", step, ex.Message, ex.StackTrace), "Error", true);
      }
      GUILayout.EndVertical();
    }

    internal static void ExtendAllAntennas()
    {
      // TODO: for realism, add a closing/opening sound
      foreach (var iAntenna in SMAddon.SmVessel.Antennas)
      {
        iAntenna.ExtendAntenna();
      }
    }

    internal static void RetractAllAntennas()
    {
      // TODO: for realism, add a closing/opening sound
      foreach (var iAntenna in SMAddon.SmVessel.Antennas)
      {
        iAntenna.RetractAntenna();
      }
    }

  }
}
