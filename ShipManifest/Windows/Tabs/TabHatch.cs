using System;
using System.Linq;
using ConnectedLivingSpace;
using ShipManifest.InternalObjects;
using UnityEngine;

namespace ShipManifest.Windows.Tabs
{
  internal static class TabHatch
  {
    internal static string ToolTip = "";
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;

    internal static void Display(Vector2 displayViewerPosition)
    {
      var scrollX = WindowControl.Position.x + 20;
      var scrollY = WindowControl.Position.y + 50 - displayViewerPosition.y;

      // Reset Tooltip active flag...
      ToolTipActive = false;

      GUILayout.BeginVertical();
      GUI.enabled = true;
      GUILayout.Label("Hatch Control Center ", SMStyle.LabelTabHeader);
      GUILayout.Label("____________________________________________________________________________________________",
        SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(350));
      var step = "start";
      try
      {
        // Display all hatches
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var x = 0; x < SMAddon.SmVessel.Hatches.Count; x++)
        {
          var iHatch = SMAddon.SmVessel.Hatches[x];
          var isEnabled = true;
          var open = false;

          // get hatch state
          if (!iHatch.IsDocked)
            isEnabled = false;
          if (iHatch.HatchOpen)
            open = true;

          step = "gui enable";
          GUI.enabled = isEnabled;
          var newOpen = GUILayout.Toggle(open, iHatch.HatchStatus + " - " + iHatch.Title, GUILayout.Width(325));
          step = "button toggle check";
          if (!open && newOpen)
          {
            iHatch.OpenHatch(true);
          }
          else if (open && !newOpen)
          {
            iHatch.CloseHatch(true);
          }
          var rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && rect.Contains(Event.current.mousePosition))
          {
            SMHighlighter.IsMouseOver = true;
            SMHighlighter.MouseOverRect = new Rect(scrollX + rect.x, scrollY + rect.y, rect.width, rect.height);
            SMHighlighter.MouseOverpart = iHatch.ClsPart.Part;
            SMHighlighter.MouseOverparts = null;
          }
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(
          string.Format(" in Hatches Tab at step {0}.  Error:  {1} \r\n\r\n{2}", step, ex.Message, ex.StackTrace),
          Utilities.LogType.Error, true);
      }
      GUI.enabled = true;
      GUILayout.EndVertical();
    }

    internal static void OpenAllHatches()
    {
      // TODO: for realism, add a closing/opening sound
      // ReSharper disable once SuspiciousTypeConversion.Global
      var iModules = SMAddon.SmVessel.Hatches.Select(iHatch => (IModuleDockingHatch) iHatch.HatchModule)
        .Where(iModule => iModule.IsDocked).GetEnumerator();
      while (iModules.MoveNext())
      {
        if (iModules.Current == null) continue;;
        iModules.Current.HatchEvents["CloseHatch"].active = true;
        iModules.Current.HatchEvents["OpenHatch"].active = false;
        iModules.Current.HatchOpen = true;
      }
      SMAddon.FireEventTriggers();
    }

    internal static void CloseAllHatches()
    {
      // TODO: for realism, add a closing/opening sound
      // ReSharper disable once SuspiciousTypeConversion.Global
      var iModules = SMAddon.SmVessel.Hatches.Select(iHatch => (IModuleDockingHatch)iHatch.HatchModule)
        .Where(iModule => iModule.IsDocked).GetEnumerator();
      while (iModules.MoveNext())
      {
        if (iModules.Current == null) continue; ;
        iModules.Current.HatchEvents["CloseHatch"].active = false;
        iModules.Current.HatchEvents["OpenHatch"].active = true;
        iModules.Current.HatchOpen = false;
      }
      SMAddon.FireEventTriggers();
    }
  }
}