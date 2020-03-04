using ShipManifest.Windows;
using ShipManifest.Windows.Tabs.Control;
using ShipManifest.Windows.Tabs.Settings;
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
    internal static string Source = "";
    internal static Rect Position;
    internal static float XOffset;

    internal static void ShowToolTips()
    {
      if (!string.IsNullOrEmpty(ToolTip) && CanShowToolTip())
      {
        //LogMessage(String.Format("ShowToolTips: \r\nToolTip: {0}\r\nToolTipPos:  {1}", SmAddon.toolTip, SMToolTIps.ToolTipPos.ToString()), Utilities.LogType.Info, Settings.VerboseLogging);
        ShowToolTip(ToolTipPos, ToolTip);
      }

      // Obtain the new value and Source from the last repaint.
      ToolTip = GetCurrentToolTip(out Source);
    }

    internal static void ShowToolTip(Vector2 toolTipPos, string toolTip)
    {
      if (!SMSettings.ShowToolTips || (toolTip == null) || (toolTip.Trim().Length <= 0)) return;
      Vector2 size = SMStyle.ToolTipStyle.CalcSize(new GUIContent(toolTip));
      Position = new Rect(toolTipPos.x, toolTipPos.y, size.x, size.y);
      RepositionToolTip();
      GUI.Window(0, Position, EmptyWindow, toolTip, SMStyle.ToolTipStyle);
      GUI.BringWindowToFront(0);
    }

    internal static string SetActiveToolTip(Rect control, string toolTip, ref bool toolTipActive, float xOffset)
    {
      // Note:  all values are screen point based.  (0,0 in lower left).  this removes confusion with the gui point of elements (0,0 in upper left).
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

    private static string GetCurrentToolTip(out string source)
    {
      // Update stored tooltip.  We do this here so change can be picked up after the current onGUI.  
      // Tooltip will not display if changes are made during the current OnGUI.  
      // (Unity uses onGUI async callbacks so we need to allow for the callback)

      // Only one of these values can be active at a time (onMouseOver), so this will trap it.
      // (Brute force, but functional)
      string toolTip = "";
      if (!string.IsNullOrEmpty(WindowTransfer.ToolTip))
      {
        toolTip = WindowTransfer.ToolTip;
        source = "WindowTransfer";
        return toolTip;
      }
      if (!string.IsNullOrEmpty(WindowRoster.ToolTip))
      {
        toolTip = WindowRoster.ToolTip;
        source = "WindowRoster";
        return toolTip;
      }
      if (!string.IsNullOrEmpty(WindowDebugger.ToolTip))
      {
        toolTip = WindowDebugger.ToolTip;
        source = "WindowDebugger";
        return toolTip;
      }
      if (!string.IsNullOrEmpty(WindowManifest.ToolTip))
      {
        toolTip = WindowManifest.ToolTip;
        source = "WindowManifest";
        return toolTip;
      }
      if (!string.IsNullOrEmpty(WindowControl.ToolTip))
      {
        toolTip = WindowControl.ToolTip;
        source = "WindowControl";
        return toolTip;
      }
      if (!string.IsNullOrEmpty(WindowSettings.ToolTip))
      {
        toolTip = WindowSettings.ToolTip;
        source = "WindowSettings";
        return toolTip;
      }

      // Control Window Tab switches
      if (!string.IsNullOrEmpty(TabVessel.ToolTip))
      {
        toolTip = TabVessel.ToolTip;
        source = "TabVessel";
        return toolTip;
      }
      if (!string.IsNullOrEmpty(TabHatch.ToolTip))
      {
        toolTip = TabHatch.ToolTip;
        source = "TabHatch";
        return toolTip;
      }
      if (!string.IsNullOrEmpty(TabSolarPanel.ToolTip))
      {
        toolTip = TabSolarPanel.ToolTip;
        source = "TabSolarPanel";
        return toolTip;
      }
      if (!string.IsNullOrEmpty(TabAntenna.ToolTip))
      {
        toolTip = TabAntenna.ToolTip;
        source = "TabAntenna";
        return toolTip;
      }
      if (!string.IsNullOrEmpty(TabLight.ToolTip))
      {
        toolTip = TabLight.ToolTip;
        source = "TabLight";
        return toolTip;
      }

      // Settings Window Tab switches
      if (!string.IsNullOrEmpty(TabRealism.ToolTip))
      {
        toolTip = TabRealism.ToolTip;
        source = "TabRealism";
        return toolTip;
      }
      if (!string.IsNullOrEmpty(TabHighlight.ToolTip))
      {
        toolTip = TabHighlight.ToolTip;
        source = "TabHighlight";
        return toolTip;
      }
      if (!string.IsNullOrEmpty(TabSounds.ToolTip))
      {
        toolTip = TabSounds.ToolTip;
        source = "TabSounds";
        return toolTip;
      }
      if (!string.IsNullOrEmpty(TabToolTips.ToolTip))
      {
        toolTip = TabToolTips.ToolTip;
        source = "TabToolTips";
        return toolTip;
      }
      if (!string.IsNullOrEmpty(TabConfig.ToolTip))
      {
        toolTip = TabConfig.ToolTip;
        source = "TabConfig";
        return toolTip;
      }
      source = "";
      return toolTip;
    }

    private static bool CanShowToolTip()
    {
      switch (Source)
      {
        case "WindowSettings":
        case "TabRealism":
        case "TabHighlight":
        case "TabSounds":
        case "TabToolTips":
        case "TabConfig":
        case "WindowRoster":
          return HighLogic.LoadedScene == GameScenes.SPACECENTER && SMAddon.ShowUi && !SMConditions.IsPauseMenuOpen() ||
                 HighLogic.LoadedScene == GameScenes.FLIGHT && SMConditions.CanShowShipManifest();
        case "WindowManifest":
        case "WindowTransfer":
        case "WindowControl":
        case "TabVessel":
        case "TabHatch":
        case "TabSolarPanel":
        case "TabAntenna":
        case "TabLight":
          return SMConditions.CanShowShipManifest();
        case "WindowDebugger":
          return true;
        default:
          return false;
      }
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