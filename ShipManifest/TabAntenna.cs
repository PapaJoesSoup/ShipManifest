using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ConnectedLivingSpace;

namespace ShipManifest
{
  static class TabAntenna
  {
    internal static string ToolTip = "";
    internal static bool ToolTipActive = false;
    internal static bool ShowToolTips = true;
    internal static bool isRTAntennas = false;

    internal static void Display(Vector2 DisplayViewerPosition)
    {
      float scrollX = WindowControl.Position.x;
      float scrollY = WindowControl.Position.y + 50 - DisplayViewerPosition.y;

      // Reset Tooltip active flag...
      ToolTipActive = false;
      ShowToolTips = SMSettings.ShowToolTips;

      GUILayout.BeginVertical();
      GUI.enabled = true;
      if (InstalledMods.IsRTInstalled)
        GUILayout.Label("Antenna Control Center  (RemoteTech detected)", SMStyle.LabelTabHeader);
      else
        GUILayout.Label("Antenna Control Center ", SMStyle.LabelTabHeader);
      GUILayout.Label("____________________________________________________________________________________________", SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(350));
      string step = "start";
      try
      {
        // Display all antennas
        foreach (ModAntenna iAntenna in SMAddon.smController.Antennas)
        {
          if (!isRTAntennas && iAntenna.isRTModule)
            isRTAntennas = true;
          step = "get Antenna label";
          string label = iAntenna.AntennaStatus + " - " + iAntenna.Title;
          bool open = iAntenna.Extended;
          bool newOpen = GUILayout.Toggle(open, label, GUILayout.Width(325), GUILayout.Height(40));
          step = "button toggle check";
          if (!open && newOpen)
            iAntenna.ExtendAntenna();
          else if (open && !newOpen)
            iAntenna.RetractAntenna();

          Rect rect = GUILayoutUtility.GetLastRect();
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
      foreach (ModAntenna iAntenna in SMAddon.smController.Antennas)
      {
        iAntenna.ExtendAntenna();
      }
    }

    internal static void RetractAllAntennas()
    {
      // TODO: for realism, add a closing/opening sound
      foreach (ModAntenna iAntenna in SMAddon.smController.Antennas)
      {
        iAntenna.RetractAntenna();
      }
    }

  }
}
