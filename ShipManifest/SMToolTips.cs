using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ShipManifest
{
  static class SMToolTips
  {

    // Tooltip vars
    internal static Rect ControlRect;
    internal static Vector2 ControlPos;
    internal static string toolTip;
    internal static Rect Position;
    internal static float X_Offset;

    internal static void ShowToolTips()
    {
      if (toolTip != null && toolTip.Trim().Length > 0)
      {
        //LogMessage(String.Format("ShowToolTips: \r\nToolTip: {0}\r\nToolTipPos:  {1}", SMAddon.toolTip, SMToolTIps.ToolTipPos.ToString()), "Info", Settings.VerboseLogging);
        ShowToolTip(ControlPos, toolTip);
      }

      // Obtain the new value from the last repaint.
      string ToolTip = "";
      if (WindowTransfer.ToolTip != null && WindowTransfer.ToolTip.Trim().Length > 0)
        ToolTip = WindowTransfer.ToolTip;
      if (WindowRoster.ToolTip != null && WindowRoster.ToolTip.Trim().Length > 0)
        ToolTip = WindowRoster.ToolTip;
      if (WindowDebugger.ToolTip != null && WindowDebugger.ToolTip.Trim().Length > 0)
        ToolTip = WindowDebugger.ToolTip;
      if (WindowManifest.ToolTip != null && WindowManifest.ToolTip.Trim().Length > 0)
        ToolTip = WindowManifest.ToolTip;
      if (WindowSettings.ToolTip != null && WindowSettings.ToolTip.Trim().Length > 0)
        ToolTip = WindowSettings.ToolTip;

      // Control WIndow Tab switches
      if (WindowControl.ToolTip != null && WindowControl.ToolTip.Trim().Length > 0)
        ToolTip = WindowControl.ToolTip;
      if (TabHatch.ToolTip != null && TabHatch.ToolTip.Trim().Length > 0)
        ToolTip = TabHatch.ToolTip;
      if (TabSolarPanel.ToolTip != null && TabSolarPanel.ToolTip.Trim().Length > 0)
        ToolTip = TabSolarPanel.ToolTip;
      if (TabAntenna.ToolTip != null && TabSolarPanel.ToolTip.Trim().Length > 0)
        ToolTip = TabAntenna.ToolTip;
      if (TabLight.ToolTip != null && TabSolarPanel.ToolTip.Trim().Length > 0)
        ToolTip = TabLight.ToolTip;

      // Update stored tooltip.  We do this here so change can be picked up after the current onGUI.  
      // Tooltip will not display if changes are made during the curreint OnGUI.  
      // (Unity uses onGUI callbacks so we need to allow for the callback)
      toolTip = ToolTip;
    }

    internal static void ShowToolTip(Vector2 toolTipPos, string ToolTip)
    {
      if (SMSettings.ShowToolTips && (ToolTip != null) && (ToolTip.Trim().Length > 0))
      {
        Vector2 size = SMStyle.ToolTipStyle.CalcSize(new GUIContent(ToolTip));
        Position = new Rect(toolTipPos.x, toolTipPos.y, size.x, size.y);
        RepositionToolTip();
        GUI.Window(0, Position, EmptyWindow, ToolTip, SMStyle.ToolTipStyle);
        GUI.BringWindowToFront(0);
        //Utilities.LogMessage(string.Format("ShowToolTip: \r\nRectangle data:  {0} \r\nToolTip:  {1}\r\nToolTipPos:  {2}", rect.ToString(), ToolTip, toolTipPos.ToString()), "Info", true);
      }
    }

    internal static string SetActiveTooltip(Rect controlRect, Rect WindowPosition, string toolTip, ref bool toolTipActive, float xOffset, float yOffset)
    {

      if (!toolTipActive && controlRect.Contains(Event.current.mousePosition))
      {
        toolTipActive = true;
        // Since we are using GUILayout, the curent mouse position returns a position with reference to the parent. 
        // Add the height of parent GUI elements already drawn to y offset to get the correct screen position
        if (controlRect.Contains(Event.current.mousePosition))
        {
          // Let's use the rectangle as a solid anchor and a stable tooltip, forgiving of mouse movement within bounding box...
          ControlPos = new Vector2(controlRect.xMax, controlRect.y);

          ControlPos.x = ControlPos.x + WindowPosition.x + xOffset;
          ControlPos.y = ControlPos.y + WindowPosition.y + yOffset;

          ControlRect = controlRect;
          X_Offset = xOffset;
          ControlRect.x += WindowPosition.x + xOffset;
          ControlRect.y += WindowPosition.y + yOffset;
          //Utilities.LogMessage(string.Format("Setup Tooltip - Mouse inside Rect: \r\nRectangle data:  {0} \r\nWindowPosition:  {1}\r\nToolTip:  {2}\r\nToolTip Pos:  {3}", rect.ToString(), WindowPosition.ToString(), ToolTip, ShipManifestAddon.ToolTipPos.ToString()), "Info", true);
        }
        else
          toolTip = "";
      }
      // We are in a loop so we don't need the return value from SetUpToolTip.  We will assign it instead.
      if (!toolTipActive)
        toolTip = "";
      return toolTip;
    }

    private static void RepositionToolTip()
    {
      if (Position.xMax > Screen.currentResolution.width)
        Position.x = ControlRect.x - Position.width - (X_Offset > 30 ? 30 : X_Offset);
      if (Position.yMax > Screen.currentResolution.height)
        Position.y = Screen.currentResolution.height - Position.height;
    }
    private static void EmptyWindow(int windowId)
    {

    }

  }
}
