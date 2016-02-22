using ShipManifest.Windows;
using ShipManifest.Windows.Tabs;
using UnityEngine;

namespace ShipManifest.InternalObjects
{
  // ReSharper disable once InconsistentNaming
  internal static class SMToolTips
  {
    // Tooltip vars
    internal static Rect ControlRect;
    internal static Vector2 ToolTipPos;
    internal static string ToolTip;
    internal static Rect Position;
    internal static float XOffset;

    internal static void ShowToolTips()
    {
      if (!string.IsNullOrEmpty(ToolTip))
      {
        //LogMessage(String.Format("ShowToolTips: \r\nToolTip: {0}\r\nToolTipPos:  {1}", SmAddon.toolTip, SMToolTIps.ToolTipPos.ToString()), "Info", Settings.VerboseLogging);
        ShowToolTip(ToolTipPos, ToolTip);
      }

      // Obtain the new value from the last repaint.
      ToolTip = GetCurrentToolTip();
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
      }
    }

    internal static string SetActiveToolTip(Rect controlPosition, Rect windowPosition, string toolTip,
      ref bool toolTipActive, float xOffset, float yOffset)
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
        }
        else
          toolTip = "";
      }
      // We are in a loop so we don't need the return value from SetUpToolTip.  We will assign it instead.
      if (!toolTipActive)
        toolTip = "";
      return toolTip;
    }

    private static string GetCurrentToolTip()
    {
      // Only one of these values can be active at a time (onMouseOver), so this will trap it.
      // (Brute force, but functional)
      var toolTip = "";
      if (!string.IsNullOrEmpty(WindowTransfer.ToolTip)) toolTip = WindowTransfer.ToolTip;
      if (!string.IsNullOrEmpty(WindowRoster.ToolTip)) toolTip = WindowRoster.ToolTip;
      if (!string.IsNullOrEmpty(WindowDebugger.ToolTip)) toolTip = WindowDebugger.ToolTip;
      if (!string.IsNullOrEmpty(WindowManifest.ToolTip)) toolTip = WindowManifest.ToolTip;
      if (!string.IsNullOrEmpty(WindowControl.ToolTip)) toolTip = WindowControl.ToolTip;
      if (!string.IsNullOrEmpty(WindowSettings.ToolTip)) toolTip = WindowSettings.ToolTip;

      // Control Window Tab switches
      if (!string.IsNullOrEmpty(TabHatch.ToolTip)) toolTip = TabHatch.ToolTip;
      if (!string.IsNullOrEmpty(TabSolarPanel.ToolTip)) toolTip = TabSolarPanel.ToolTip;
      if (!string.IsNullOrEmpty(TabAntenna.ToolTip)) toolTip = TabAntenna.ToolTip;
      if (!string.IsNullOrEmpty(TabLight.ToolTip)) toolTip = TabLight.ToolTip;

      // Settings Window Tab switches
      if (!string.IsNullOrEmpty(TabRealism.ToolTip)) toolTip = TabRealism.ToolTip;
      if (!string.IsNullOrEmpty(TabHighlight.ToolTip)) toolTip = TabHighlight.ToolTip;
      if (!string.IsNullOrEmpty(TabSounds.ToolTip)) toolTip = TabSounds.ToolTip;
      if (!string.IsNullOrEmpty(TabToolTips.ToolTip)) toolTip = TabToolTips.ToolTip;
      if (!string.IsNullOrEmpty(TabConfig.ToolTip)) toolTip = TabConfig.ToolTip;
      if (!string.IsNullOrEmpty(TabInstalledMods.ToolTip)) toolTip = TabInstalledMods.ToolTip;

      // Update stored tooltip.  We do this here so change can be picked up after the current onGUI.  
      // Tooltip will not display if changes are made during the current OnGUI.  
      // (Unity uses onGUI async callbacks so we need to allow for the callback)
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