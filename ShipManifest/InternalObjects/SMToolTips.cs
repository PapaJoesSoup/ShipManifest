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
        //LogMessage(String.Format("ShowToolTips: \r\nToolTip: {0}\r\nToolTipPos:  {1}", SmAddon.toolTip, SMToolTIps.ToolTipPos.ToString()), Utilities.LogType.Info, Settings.VerboseLogging);
        ShowToolTip(ToolTipPos, ToolTip);
      }

      // Obtain the new value from the last repaint.
      ToolTip = GetCurrentToolTip();
    }

    internal static void ShowToolTip(Vector2 toolTipPos, string toolTip)
    {
      if (SMSettings.ShowToolTips && (toolTip != null) && (toolTip.Trim().Length > 0))
      {
        Vector2 size = SMStyle.ToolTipStyle.CalcSize(new GUIContent(toolTip));
        Position = new Rect(toolTipPos.x, toolTipPos.y, size.x, size.y);
        RepositionToolTip();
        GUI.Window(0, Position, EmptyWindow, toolTip, SMStyle.ToolTipStyle);
        GUI.BringWindowToFront(0);
      }
    }

    internal static string SetActiveToolTip(Rect control, string toolTip, ref bool toolTipActive, float xOffset)
    {
      // Note:  all values are screen point based.  (0,0 in lower left).  this removes confusion with the gui point of elements (o,o in upper left).
      if (!toolTipActive && control.Contains(Event.current.mousePosition))
      {
        toolTipActive = true;
        // Note at this time controlPosition is in Gui Point system and is local position.  convert to screenpoint.
        Rect newControl = new Rect
        {
          position = GUIUtility.GUIToScreenPoint(control.position),
          width = control.width,
          height = control.height
        };

        // Event.current.mousePosition returns sceen mouseposition.  GuI elements return a value in gui position.. 
        // Add the height of parent GUI elements already drawn to y offset to get the correct screen position
        if (control.Contains(Event.current.mousePosition))
        {
          // Let's use the rectangle as a solid anchor and a stable tooltip, forgiving of mouse movement within bounding box...
          ToolTipPos = new Vector2(newControl.xMax + xOffset, newControl.y - 10);

          ControlRect = newControl;
          XOffset = xOffset;
          ControlRect.x += xOffset;
          ControlRect.y -= 10;
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
      string toolTip = "";
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
      if (Position.xMax > Screen.width)
        Position.x = ControlRect.x - Position.width - (XOffset > 30 ? 30 : XOffset);
      if (Position.yMax > Screen.height)
        Position.y = Screen.height - Position.height;
    }

    private static void EmptyWindow(int windowId)
    {
    }
  }
}