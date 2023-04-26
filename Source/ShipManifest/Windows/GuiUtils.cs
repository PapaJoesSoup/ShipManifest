using System.Globalization;
using ShipManifest.InternalObjects;
using ShipManifest.InternalObjects.Settings;
using UnityEngine;

namespace ShipManifest.Windows
{
  internal static class GuiUtils
  {

    static GuiUtils()
    {
      RefreshUIScale();
    }

    // UIScale settings
    internal static float guiWidth = 20 * CurrSettings.CurrentUIScale;
    internal static float guiHeight = 20 * CurrSettings.CurrentUIScale;

    // Color content strings
    internal static GUIContent blueContent    = new GUIContent("", "Blue");
    internal static GUIContent blackContent   = new GUIContent("", "Black");
    internal static GUIContent clearContent   = new GUIContent("", "Clear");
    internal static GUIContent cyanContent    = new GUIContent("", "Cyan");
    internal static GUIContent grayContent    = new GUIContent("", "Gray");
    internal static GUIContent greenContent   = new GUIContent("", "Green");
    internal static GUIContent magentaContent = new GUIContent("", "Magenta");
    internal static GUIContent redContent     = new GUIContent("", "Red");
    internal static GUIContent whiteContent   = new GUIContent("", "White");
    internal static GUIContent yellowContent  = new GUIContent("", "Yellow");

    private const ControlTypes BLOCK_ALL_CONTROLS = ControlTypes.UI | ControlTypes.All;

    internal static string ColorSelector(out string toolTip)
    {
      string thisColor = "";
      toolTip = "";

      if (GUILayout.Button(blueContent, SMStyle.ButtonStyle, GUILayout.Width(guiWidth), GUILayout.Height(guiHeight)))
        thisColor = "blue";
      if (Event.current.type == EventType.Repaint)
      {
        toolTip = GUI.tooltip;
      }
      if (GUILayout.Button(blackContent, SMStyle.ButtonStyle, GUILayout.Width(guiWidth), GUILayout.Height(guiHeight)))
        thisColor = "black";
      if (Event.current.type == EventType.Repaint)
      {
        toolTip = GUI.tooltip;
      }
      if (GUILayout.Button(clearContent, SMStyle.ButtonStyle, GUILayout.Width(guiWidth), GUILayout.Height(guiHeight)))
        thisColor = "clear";
      if (Event.current.type == EventType.Repaint)
      {
        toolTip = GUI.tooltip;
      }
      if (GUILayout.Button(cyanContent, SMStyle.ButtonStyle, GUILayout.Width(guiWidth), GUILayout.Height(guiHeight)))
        thisColor = "cyan";
      if (Event.current.type == EventType.Repaint)
      {
        toolTip = GUI.tooltip;
      }
      if (GUILayout.Button(grayContent, SMStyle.ButtonStyle, GUILayout.Width(guiWidth), GUILayout.Height(guiHeight)))
        thisColor = "gray";
      if (Event.current.type == EventType.Repaint)
      {
        toolTip = GUI.tooltip;
      }
      if (GUILayout.Button(greenContent, SMStyle.ButtonStyle, GUILayout.Width(guiWidth), GUILayout.Height(guiHeight)))
        thisColor = "green";
      if (Event.current.type == EventType.Repaint)
      {
        toolTip = GUI.tooltip;
      }
      if (GUILayout.Button(magentaContent, SMStyle.ButtonStyle, GUILayout.Width(guiWidth), GUILayout.Height(guiHeight)))
        thisColor = "magenta";
      if (Event.current.type == EventType.Repaint)
      {
        toolTip = GUI.tooltip;
      }
      if (GUILayout.Button(redContent, SMStyle.ButtonStyle, GUILayout.Width(guiWidth), GUILayout.Height(guiHeight)))
        thisColor = "red";
      if (Event.current.type == EventType.Repaint)
      {
        toolTip = GUI.tooltip;
      }
      if (GUILayout.Button(whiteContent, SMStyle.ButtonStyle, GUILayout.Width(guiWidth), GUILayout.Height(guiHeight)))
        thisColor = "white";
      if (Event.current.type == EventType.Repaint)
      {
        toolTip = GUI.tooltip;
      }
      if (GUILayout.Button(yellowContent, SMStyle.ButtonStyle, GUILayout.Width(guiWidth), GUILayout.Height(guiHeight)))
        thisColor = "yellow";
      if (Event.current.type == EventType.Repaint)
      {
        toolTip = GUI.tooltip;
      }
      return thisColor;
    }

    //this block is derived from Kerbal Engineer source. Thanks cybutek!
    internal static void PreventEditorClickthrough(bool visible, Rect position, ref bool lockedInputs)
    {
      bool mouseOverWindow = MouseIsOverWindow(visible, position);
      if (!lockedInputs && mouseOverWindow)
      {
        EditorLogic.fetch.Lock(true, true, true, "SM_Window");
        lockedInputs = true;
      }
      if (!lockedInputs || mouseOverWindow) return;
      EditorLogic.fetch.Unlock("SM_Window");
      lockedInputs = false;
    }

    internal static bool PreventClickthrough(bool visible, Rect position, bool lockedInputs)
    {
      // Still in work.  Not behaving correctly in Flight.  Works fine in Editor and Space Center...
      // Based on testing, it appears that Kerbals can still be accessed
      bool mouseOverWindow = MouseIsOverWindow(visible, position);
      if (!lockedInputs && mouseOverWindow)
      {
        InputLockManager.SetControlLock(BLOCK_ALL_CONTROLS, "SM_Window");
        lockedInputs = true;
      }
      if (!lockedInputs || mouseOverWindow) return lockedInputs;
      InputLockManager.RemoveControlLock("SM_Window");
      return false;
    }

    private static bool MouseIsOverWindow(bool visible, Rect position)
    {
      return visible
             && position.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y));
    }

    internal static void UpdateScale(float diff, float viewerHeight, ref float heightScale, float minHeight)
    {
      heightScale += diff;
      if (viewerHeight + heightScale < minHeight)
      {
        heightScale = minHeight - viewerHeight;
      }
    }

    internal static void ResetResize()
    {
      if (WindowManifest.ResizingWindow) WindowManifest.ResizingWindow = false;
      if (WindowTransfer.ResizingWindow) WindowTransfer.ResizingWindow = false;
      if (WindowRoster.ResizingWindow) WindowRoster.ResizingWindow = false;
      if (WindowControl.ResizingWindow) WindowControl.ResizingWindow = false;
      if (WindowDebugger.ResizingWindow) WindowDebugger.ResizingWindow = false;
      if (WindowSettings.ResizingWindow) WindowSettings.ResizingWindow = false;
    }

    internal static void RepositionWindows()
    {
      RepositionWindow(ref WindowManifest.Position);
      RepositionWindow(ref WindowTransfer.Position);
      RepositionWindow(ref WindowDebugger.Position);
      RepositionWindow(ref WindowSettings.Position);
      RepositionWindow(ref WindowControl.Position);
      RepositionWindow(ref WindowRoster.Position);
    }

    internal static void RepositionWindow(ref Rect windowPosition)
    {
      // This method uses Gui point system.
      if (windowPosition.x < 0) windowPosition.x = 0;
      if (windowPosition.y < 0) windowPosition.y = 0;

      if (windowPosition.xMax > Screen.width)
        windowPosition.x = Screen.width - windowPosition.width;
      if (windowPosition.yMax > Screen.height)
        windowPosition.y = Screen.height - windowPosition.height;

      if (windowPosition.width > Screen.width - windowPosition.x)
        windowPosition.width = Screen.width - windowPosition.x;
      if (windowPosition.height > Screen.height - windowPosition.y)
        windowPosition.height = Screen.height - windowPosition.y;
    }

    internal static Rect GuiToScreenRect(Rect rect)
    {
      // Must run during OnGui to work...
      Rect newRect = new Rect
      {
        position = GUIUtility.GUIToScreenPoint(rect.position),
        width = rect.width,
        height = rect.height
      };
      return newRect;
    }

    internal static bool DisplaySettingsToggle( bool setting, GUIContent content, ref ToolTip toolTip, float toggleWidth, float scrollX)
    {
      setting = GUILayout.Toggle(setting, content, GUILayout.Width(toggleWidth));
      Rect rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && toolTip.CanShow)
        toolTip.Desc = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref toolTip.Active, scrollX);
      return setting;
    }

    internal static string DisplaySettingsTextField(string setting, GUIContent labelContent, float labelWidth, float textWidth, GUIContent unitsContent, float unitsWidth, ToolTip toolTip, float scrollX)
    {
      GUILayout.BeginHorizontal();
      //Error Log Length:
      GUILayout.Label(labelContent, GUILayout.Width(labelWidth));
      Rect rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && toolTip.CanShow)
        toolTip.Desc = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref toolTip.Active, scrollX);
      setting = GUILayout.TextField(setting, GUILayout.Width(textWidth));
      GUILayout.Label(unitsContent, GUILayout.Width(unitsWidth));
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && toolTip.CanShow)
        toolTip.Desc = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref toolTip.Active, scrollX);
      GUILayout.EndHorizontal();
      return setting;
    }

    internal static double DisplaySettingsSlider(SliderData slider, ref ToolTip toolTip, float scrollX)
    {
      GUILayout.Label(slider.minValue.ToString(CultureInfo.InvariantCulture), GUILayout.Width(slider.minWidth), GUILayout.Height(guiHeight));
      Rect rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && toolTip.CanShow)
        toolTip.Desc = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref toolTip.Active, scrollX);
      double setting = GUILayout.HorizontalSlider((float)slider.setting, (float)slider.minValue,
        (float)slider.maxValue, GUILayout.Width(slider.sliderWidth), GUILayout.Height(guiHeight));
      GUILayout.Label(slider.maxContent, GUILayout.Width(slider.maxWidth), GUILayout.Height(guiHeight));
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && toolTip.CanShow)
        toolTip.Desc = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref toolTip.Active, scrollX);
      return setting;
    }

    internal static void RefreshUIScale()
    {
      guiWidth = 20 * CurrSettings.CurrentUIScale;
      guiHeight = 20 * CurrSettings.CurrentUIScale;
    }
  }
}
