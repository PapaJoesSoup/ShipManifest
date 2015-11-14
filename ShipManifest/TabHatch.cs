using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ConnectedLivingSpace;

namespace ShipManifest
{
  static class TabHatch
  {

    internal static string ToolTip = "";
    internal static bool ToolTipActive = false;
    internal static bool ShowToolTips = true;

    internal static void Display(Vector2 DisplayViewerPosition)
    {
      float scrollX = WindowControl.Position.x + 20;
      float scrollY = WindowControl.Position.y + 50 - DisplayViewerPosition.y;

      // Reset Tooltip active flag...
      ToolTipActive = false;

      GUILayout.BeginVertical();
      GUI.enabled = true;
      GUILayout.Label("Hatch Control Center ", SMStyle.LabelTabHeader);
      GUILayout.Label("____________________________________________________________________________________________", SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(350));
      string step = "start";
      try
      {
        // Display all hatches
        for (int x = 0; x < SMAddon.smController.Hatches.Count; x++)
        {
          ModHatch iHatch = SMAddon.smController.Hatches[x];
          bool isEnabled = true;
          bool open = false;

          // get hatch state
          if (!iHatch.IsDocked)
            isEnabled = false;
          if (iHatch.HatchOpen)
            open = true;

          step = "gui enable";
          GUI.enabled = isEnabled;
          bool newOpen = GUILayout.Toggle(open, iHatch.HatchStatus + " - " + iHatch.Title, GUILayout.Width(325));
          step = "button toggle check";
          if (!open && newOpen)
          {
            iHatch.OpenHatch(true);
          }
          else if (open && !newOpen)
          {
            iHatch.CloseHatch(true);
          }
          Rect rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && rect.Contains(Event.current.mousePosition))
          {
            SMHighlighter.IsMouseOver = true;
            SMHighlighter.MouseOverRect = new Rect(scrollX + rect.x, scrollY + rect.y, rect.width, rect.height);
            SMHighlighter.MouseOverpart = iHatch.CLSPart.Part;
            SMHighlighter.MouseOverparts = null;
          }
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format(" in Hatches Tab at step {0}.  Error:  {1} \r\n\r\n{2}", step, ex.Message, ex.StackTrace), "Error", true);
      }
      GUI.enabled = true;
      GUILayout.EndVertical();

    }

    internal static void OpenAllHatches()
    {
      // TODO: for realism, add a closing/opening sound
      foreach (ModHatch iHatch in SMAddon.smController.Hatches)
      {
        IModuleDockingHatch iModule = (IModuleDockingHatch)iHatch.HatchModule;
        if (iModule.IsDocked)
        {
          iModule.HatchEvents["CloseHatch"].active = true;
          iModule.HatchEvents["OpenHatch"].active = false;
          iModule.HatchOpen = true;
        }
      }
      SMAddon.FireEventTriggers();
    }

    internal static void CloseAllHatches()
    {
      // TODO: for realism, add a closing/opening sound
      foreach (ModHatch iHatch in SMAddon.smController.Hatches)
      {
        IModuleDockingHatch iModule = (IModuleDockingHatch)iHatch.HatchModule;
        if (iModule.IsDocked)
        {
          iModule.HatchEvents["CloseHatch"].active = false;
          iModule.HatchEvents["OpenHatch"].active = true;
          iModule.HatchOpen = false;
        }
      }
      SMAddon.FireEventTriggers();
    }

  }
}
