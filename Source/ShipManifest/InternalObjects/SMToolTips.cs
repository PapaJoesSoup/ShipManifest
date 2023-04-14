using ShipManifest.InternalObjects.Settings;
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
      if (!CurrSettings.ShowToolTips || (toolTip == null) || (toolTip.Trim().Length <= 0)) return;
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
        source = nameof(WindowTransfer);
        return toolTip;
      }
      if (!string.IsNullOrEmpty(WindowRoster.ToolTip))
      {
        toolTip = WindowRoster.ToolTip;
        source = nameof(WindowRoster);
        return toolTip;
      }
      if (!string.IsNullOrEmpty(WindowDebugger.ToolTip))
      {
        toolTip = WindowDebugger.ToolTip;
        source = nameof(WindowDebugger);
        return toolTip;
      }
      if (!string.IsNullOrEmpty(WindowManifest.ToolTip))
      {
        toolTip = WindowManifest.ToolTip;
        source = nameof(WindowManifest);
        return toolTip;
      }
      if (!string.IsNullOrEmpty(WindowControl.ToolTip))
      {
        toolTip = WindowControl.ToolTip;
        source = nameof(WindowControl);
        return toolTip;
      }
      if (!string.IsNullOrEmpty(WindowSettings.ToolTip))
      {
        toolTip = WindowSettings.ToolTip;
        source = nameof(WindowSettings);
        return toolTip;
      }

      // Control Window Tab switches
      if (!string.IsNullOrEmpty(TabVessel.ToolTip))
      {
        toolTip = TabVessel.ToolTip;
        source = nameof(TabVessel);
        return toolTip;
      }
      if (!string.IsNullOrEmpty(TabHatch.ToolTip))
      {
        toolTip = TabHatch.ToolTip;
        source = nameof(TabHatch);
        return toolTip;
      }
      if (!string.IsNullOrEmpty(TabSolarPanel.ToolTip))
      {
        toolTip = TabSolarPanel.ToolTip;
        source = nameof(TabSolarPanel);
        return toolTip;
      }
      if (!string.IsNullOrEmpty(TabAntenna.ToolTip))
      {
        toolTip = TabAntenna.ToolTip;
        source = nameof(TabAntenna);
        return toolTip;
      }
      if (!string.IsNullOrEmpty(TabLight.ToolTip))
      {
        toolTip = TabLight.ToolTip;
        source = nameof(TabLight);
        return toolTip;
      }

      // Settings Window Tab switches
      if (!string.IsNullOrEmpty(TabRealism.toolTip.Desc))
      {
        toolTip = TabRealism.toolTip.Desc;
        source = nameof(TabRealism);
        return toolTip;
      }
      if (!string.IsNullOrEmpty(TabHighlight.toolTip.Desc))
      {
        toolTip = TabHighlight.toolTip.Desc;
        source = nameof(TabHighlight);
        return toolTip;
      }
      if (!string.IsNullOrEmpty(TabSounds.toolTip.Desc))
      {
        toolTip = TabSounds.toolTip.Desc;
        source = nameof(TabSounds);
        return toolTip;
      }
      if (!string.IsNullOrEmpty(TabToolTips.toolTip.Desc))
      {
        toolTip = TabToolTips.toolTip.Desc;
        source = nameof(TabToolTips);
        return toolTip;
      }
      if (!string.IsNullOrEmpty(TabConfig.toolTip.Desc))
      {
        toolTip = TabConfig.toolTip.Desc;
        source = nameof(TabConfig);
        return toolTip;
      }
      source = "";
      return toolTip;
    }

    private static bool CanShowToolTip()
    {
      switch (Source)
      {
        case nameof(WindowSettings):
        case nameof(TabRealism):
        case nameof(TabHighlight):
        case nameof(TabSounds):
        case nameof(TabToolTips):
        case nameof(TabConfig):
        case nameof(WindowRoster):
          return HighLogic.LoadedScene == GameScenes.SPACECENTER && SMAddon.ShowUi && !SMConditions.IsPauseMenuOpen() ||
                 HighLogic.LoadedScene == GameScenes.FLIGHT && SMConditions.CanShowShipManifest();
        case nameof(WindowManifest):
        case nameof(WindowTransfer):
        case nameof(WindowControl):
        case nameof(TabVessel):
        case nameof(TabHatch):
        case nameof(TabSolarPanel):
        case nameof(TabAntenna):
        case nameof(TabLight):
          return SMConditions.CanShowShipManifest();
        case nameof(WindowDebugger):
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
