using System;
using ShipManifest.InternalObjects;
using ShipManifest.InternalObjects.Settings;
using ShipManifest.Windows.Tabs.Settings;
using UnityEngine;

namespace ShipManifest.Windows
{
  internal static class WindowSettings
  {
    #region Settings Window (GUI)

    internal static float WindowHeight = 380 * GameSettings.UI_SCALE;
    internal static float HeightScale;
    internal static float ViewerHeight = 300 * GameSettings.UI_SCALE;
    internal static float ViewerWidth = 380 * GameSettings.UI_SCALE;
    internal static float MinHeight = 300 * GameSettings.UI_SCALE;
    internal static float guiLineHeight = 20 * GameSettings.UI_SCALE;
    internal static bool ResizingWindow = false;
    internal static Rect Position = CurrSettings.DefaultPosition;
    private static bool _inputLocked;
    private static bool _showWindow;
    internal static bool ShowWindow
    {
      get => _showWindow;
      set
      {
        if (!value)
        {
          InputLockManager.RemoveControlLock("SM_Window");
          _inputLocked = false;
        }
        _showWindow = value;
      }
    }
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;
    internal static string ToolTip = "";
    internal static string TabRule = new string('_', 92);

    private static Tab _selectedTab = Tab.Realism;
    private static Vector2 _displayViewerPosition = Vector2.zero;

    // Content strings
    internal static string Title                    = SmUtils.SmTags["#smloc_settings_001"];
    internal static GUIContent closeContent         = new GUIContent("", SmUtils.SmTags["#smloc_settings_tt_001"]);
    internal static GUIContent saveContent          = new GUIContent(SmUtils.SmTags["#smloc_settings_002"], SmUtils.SmTags["#smloc_settings_tt_002"]);
    internal static GUIContent cancelContent        = new GUIContent(SmUtils.SmTags["#smloc_settings_003"], SmUtils.SmTags["#smloc_settings_tt_003"]);
    internal static GUIContent tabRealismContent    = new GUIContent(SmUtils.SmTags["#smloc_settings_004"], SmUtils.SmTags["#smloc_settings_tt_004"]);
    internal static GUIContent tabHighlightContent  = new GUIContent(SmUtils.SmTags["#smloc_settings_005"], SmUtils.SmTags["#smloc_settings_tt_005"]);
    internal static GUIContent tabTooltipContent    = new GUIContent(SmUtils.SmTags["#smloc_settings_006"], SmUtils.SmTags["#smloc_settings_tt_006"]);
    internal static GUIContent tabSoundsContent     = new GUIContent(SmUtils.SmTags["#smloc_settings_007"], SmUtils.SmTags["#smloc_settings_tt_007"]);
    internal static GUIContent tabConfigContent     = new GUIContent(SmUtils.SmTags["#smloc_settings_008"], SmUtils.SmTags["#smloc_settings_tt_008"]);


    internal static void Display(int windowId)
    {
      // set input locks when mouseover window...
      _inputLocked = GuiUtils.PreventClickthrough(ShowWindow, Position, _inputLocked);

      // Reset Tooltip active flag...
      ToolTipActive = false;

      // Close Window
      Rect rect = new Rect(Position.width - 20, 4, 16, 16);
      if (GUI.Button(rect, closeContent))
      {
        ToolTip = "";
        if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
          SMAddon.OnSmSettingsClicked();
        else
        {
          SMSettings.MemStoreTempSettings();
          ShowWindow = false;
        }
      }
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      GUILayout.BeginVertical();

      DisplayTabButtons();

      _displayViewerPosition = GUILayout.BeginScrollView(_displayViewerPosition, SMStyle.ScrollStyle,
        GUILayout.Height(ViewerHeight + HeightScale), GUILayout.Width(ViewerWidth));
      GUILayout.BeginVertical();

      DisplaySelectedTab(_displayViewerPosition);

      GUILayout.EndVertical();
      GUILayout.EndScrollView();

      DisplayActionButtons();

      GUILayout.EndVertical();

      //resizing
      Rect resizeRect =
        new Rect(Position.width - 18, Position.height - 18, 16, 16);
      GUI.DrawTexture(resizeRect, SmUtils.resizeTexture, ScaleMode.StretchToFill, true);
      if (Event.current.type == EventType.MouseDown && resizeRect.Contains(Event.current.mousePosition))
      {
        ResizingWindow = true;
      }

      if (Event.current.type == EventType.Repaint && ResizingWindow)
      {
        if (Mouse.delta.y != 0)
        {
          float diff = Mouse.delta.y;
          GuiUtils.UpdateScale(diff, ViewerHeight, ref HeightScale, MinHeight);
        }
      }
      GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
      Position.height = WindowHeight + HeightScale;
      GuiUtils.RepositionWindow(ref Position);
    }

    private static void DisplayTabButtons()
    {
      GUILayout.BeginHorizontal();

      GUIStyle realismStyle = _selectedTab == Tab.Realism ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button(tabRealismContent, realismStyle, GUILayout.Height(guiLineHeight)))
      {
        _selectedTab = Tab.Realism;
      }
      GUI.enabled = true;
      Rect rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      GUIStyle highlightStyle = _selectedTab == Tab.Highlight ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button(tabHighlightContent, highlightStyle, GUILayout.Height(guiLineHeight)))
      {
        _selectedTab = Tab.Highlight;
      }
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      GUIStyle tooltipStyle = _selectedTab == Tab.ToolTips ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button(tabTooltipContent, tooltipStyle, GUILayout.Height(guiLineHeight)))
      {
        _selectedTab = Tab.ToolTips;
      }
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      GUIStyle soundStyle = _selectedTab == Tab.Sounds ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button(tabSoundsContent, soundStyle, GUILayout.Height(guiLineHeight)))
      {
        _selectedTab = Tab.Sounds;
      }
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      GUIStyle configStyle = _selectedTab == Tab.Config ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button(tabConfigContent, configStyle, GUILayout.Height(guiLineHeight)))
      {
        _selectedTab = Tab.Config;
      }
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      GUILayout.EndHorizontal();
    }

    internal static void DisplaySelectedTab(Vector2 displayViewerPosition)
    {
      switch (_selectedTab)
      {
        case Tab.Realism:
          TabRealism.Display(displayViewerPosition);
          break;
        case Tab.Config:
          TabConfig.Display(displayViewerPosition);
          break;
        case Tab.Highlight:
          TabHighlight.Display(displayViewerPosition);
          break;
        case Tab.Sounds:
          TabSounds.Display(displayViewerPosition);
          break;
        case Tab.ToolTips:
          TabToolTips.Display(displayViewerPosition);
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    private static void DisplayActionButtons()
    {
      GUILayout.BeginHorizontal();

      // Save
      if (GUILayout.Button(saveContent, GUILayout.Height(guiLineHeight)))
      {
        ToolTip = "";
        CurrSettings.SaveIntervalSec = int.Parse(TabConfig.TxtSaveInterval);
        SMSettings.SaveSettings();

        // Sync SM to CLS override settings with CLS
        if (CurrSettings.EnableCls && HighLogic.LoadedSceneIsFlight)
        {
          SMSettings.UpdateClsOverride();
        }

        if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
          SMAddon.OnSmSettingsClicked();
        else
          ShowWindow = false;
      }
      Rect rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      // Cancel
      if (GUILayout.Button(cancelContent, GUILayout.Height(guiLineHeight)))
      {
        ToolTip = "";
        // We've cancelled, so restore original settings.
        SMSettings.MemRestoreTempSettings();

        if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
          SMAddon.OnSmSettingsClicked();
        else
          ShowWindow = false;
      }
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      GUILayout.EndHorizontal();
    }

    private enum Tab
    {
      Realism,
      Config,
      Highlight,
      Sounds,
      ToolTips
    }

    #endregion
  }
}
