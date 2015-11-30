using ShipManifest.Windows;
using ShipManifest.Windows.Tabs;
using UnityEngine;

namespace ShipManifest
{
  // ReSharper disable once InconsistentNaming
  static class SMToolTips
  {

    // Tooltip vars
    internal static Rect ControlRect;
    internal static Vector2 ToolTipPos;
    internal static string ToolTip;
    internal static Rect Position;
    internal static float XOffset;

    internal static void ShowToolTips()
    {
      if (ToolTip != null && ToolTip.Trim().Length > 0)
      {
        //LogMessage(String.Format("ShowToolTips: \r\nToolTip: {0}\r\nToolTipPos:  {1}", SmAddon.toolTip, SMToolTIps.ToolTipPos.ToString()), "Info", Settings.VerboseLogging);
        ShowToolTip(ToolTipPos, ToolTip);
      }

      // Obtain the new value from the last repaint.
      var toolTip = "";
      if (WindowTransfer.ToolTip != null && WindowTransfer.ToolTip.Trim().Length > 0)
        toolTip = WindowTransfer.ToolTip;
      if (WindowRoster.ToolTip != null && WindowRoster.ToolTip.Trim().Length > 0)
        toolTip = WindowRoster.ToolTip;
      if (WindowDebugger.ToolTip != null && WindowDebugger.ToolTip.Trim().Length > 0)
        toolTip = WindowDebugger.ToolTip;
      if (WindowManifest.ToolTip != null && WindowManifest.ToolTip.Trim().Length > 0)
        toolTip = WindowManifest.ToolTip;

      // Control Window Tab switches
      if (WindowControl.ToolTip != null && WindowControl.ToolTip.Trim().Length > 0)
        toolTip = WindowControl.ToolTip;
      if (TabHatch.ToolTip != null && TabHatch.ToolTip.Trim().Length > 0)
        toolTip = TabHatch.ToolTip;
      if (TabSolarPanel.ToolTip != null && TabSolarPanel.ToolTip.Trim().Length > 0)
        toolTip = TabSolarPanel.ToolTip;
      if (TabAntenna.ToolTip != null && TabAntenna.ToolTip.Trim().Length > 0)
        toolTip = TabAntenna.ToolTip;
      if (TabLight.ToolTip != null && TabLight.ToolTip.Trim().Length > 0)
        toolTip = TabLight.ToolTip;

      // Settings Window Tab switches
      if (WindowSettings.ToolTip != null && WindowSettings.ToolTip.Trim().Length > 0)
        toolTip = WindowSettings.ToolTip;
      if (TabRealism.ToolTip != null && TabRealism.ToolTip.Trim().Length > 0)
        toolTip = TabRealism.ToolTip;
      if (TabHighlight.ToolTip != null && TabHighlight.ToolTip.Trim().Length > 0)
        toolTip = TabHighlight.ToolTip;
      if (TabSounds.ToolTip != null && TabSounds.ToolTip.Trim().Length > 0)
        toolTip = TabSounds.ToolTip;
      if (TabToolTips.ToolTip != null && TabToolTips.ToolTip.Trim().Length > 0)
        toolTip = TabToolTips.ToolTip;
      if (TabConfig.ToolTip != null && TabConfig.ToolTip.Trim().Length > 0)
        toolTip = TabConfig.ToolTip;
      if (TabInstalledMods.ToolTip != null && TabInstalledMods.ToolTip.Trim().Length > 0)
        toolTip = TabInstalledMods.ToolTip;

      // Update stored tooltip.  We do this here so change can be picked up after the current onGUI.  
      // Tooltip will not display if changes are made during the current OnGUI.  
      // (Unity uses onGUI callbacks so we need to allow for the callback)
      ToolTip = toolTip;
    }

    internal static void ShowToolTip(Vector2 toolTipPos, string toolTip)
    {
      if (SMSettings.ShowToolTips && (toolTip != null) && (toolTip.Trim().Length > 0))
      {
        var size = SMStyle.ToolTipStyle.CalcSize(new GUIContent(toolTip));
        Position = new Rect(toolTipPos.x, toolTipPos.y, size.x, size.y);
        RepositionToolTip();
        GUI.Window(0, Position, EmptyWindow, toolTip, SMStyle.ToolTipStyle);
        GUI.BringWindowToFront(0);
        //Utilities.LogMessage(string.Format("ShowToolTip: \r\nRectangle data:  {0} \r\nToolTip:  {1}\r\nToolTipPos:  {2}", rect.ToString(), toolTip, toolTipPos.ToString()), "Info", true);
      }
    }

    internal static string SetActiveToolTip(Rect controlPosition, Rect windowPosition, string toolTip, ref bool toolTipActive, float xOffset, float yOffset)
    {

      if (!toolTipActive && controlPosition.Contains(Event.current.mousePosition))
      {
        toolTipActive = true;
        // Since we are using GUILayout, the curent mouse position returns a position with reference to the parent. 
        // Add the height of parent GUI elements already drawn to y offset to get the correct screen position
        if (controlPosition.Contains(Event.current.mousePosition))
        {
          // Let's use the rectangle as a solid anchor and a stable tooltip, forgiving of mouse movement within bounding box...
          ToolTipPos = new Vector2(controlPosition.xMax, controlPosition.y);

          ToolTipPos.x = ToolTipPos.x + windowPosition.x + xOffset;
          ToolTipPos.y = ToolTipPos.y + windowPosition.y + yOffset;

          ControlRect = controlPosition;
          XOffset = xOffset;
          ControlRect.x += windowPosition.x + xOffset;
          ControlRect.y += windowPosition.y + yOffset;
          //Utilities.LogMessage(string.Format("Setup Tooltip - Mouse inside Rect: \r\nRectangle data:  {0} \r\nWindowPosition:  {1}\r\nToolTip:  {2}\r\nToolTip Pos:  {3}", rect.ToString(), WindowPosition.ToString(), toolTip, ShipManifestAddon.ToolTipPos.ToString()), "Info", true);
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
        Position.x = ControlRect.x - Position.width - (XOffset > 30 ? 30 : XOffset);
      if (Position.yMax > Screen.currentResolution.height)
        Position.y = Screen.currentResolution.height - Position.height;
    }
    private static void EmptyWindow(int windowId)
    {

    }

  }
}
