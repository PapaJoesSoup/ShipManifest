using System;
using ShipManifest.InternalObjects;
using ShipManifest.InternalObjects.Settings;
using ShipManifest.Windows.Tabs.Settings;
using UnityEngine;

namespace ShipManifest.Windows
{
  internal static class WindowSettings
  {

    static WindowSettings()
    {
      RefreshUIScale();
    }

    #region Settings Window (GUI)

    // UIScale settings
    internal static float WindowHeight;
    internal static float WindowWidth;
    internal static float MinHeight;
    internal static float MinWidth;
    internal static float ViewerHeight;
    internal static float ViewerWidth;
    internal static float GuiRuleWidth;
    internal static float GuiRuleHeight;
    internal static float guiLineHeight;
    internal static float HeightScale;
    internal static float WidthScale;
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
      if (GUI.Button(rect, closeContent, SMStyle.SMSkin.button))
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
      GUILayout.Label("", SMStyle.SMSkin.label, GUILayout.Height(3 * CurrSettings.CurrentUIScale));
      DisplayTabButtons();

      _displayViewerPosition = GUILayout.BeginScrollView(_displayViewerPosition, SMStyle.ScrollStyle,
        GUILayout.Height(ViewerHeight + HeightScale), GUILayout.Width(ViewerWidth + WidthScale));
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
        if (Mouse.delta.y != 0 || Mouse.delta.x != 0)
        {
          float yDiff = Mouse.delta.y;
          float xDiff = Mouse.delta.x;
          GuiUtils.UpdateHeightScale(yDiff, ViewerHeight, ref HeightScale, MinHeight);
          GuiUtils.UpdateWidthScale(xDiff, ViewerWidth, ref WidthScale, MinWidth);
        }
      }
      GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
      Position.height = WindowHeight + HeightScale;
      Position.width = WindowWidth + WidthScale;
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
      if (GUILayout.Button(saveContent, SMStyle.SMSkin.button, GUILayout.Height(guiLineHeight)))
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
      if (GUILayout.Button(cancelContent, SMStyle.SMSkin.button, GUILayout.Height(guiLineHeight)))
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

    internal static void RefreshUIScale()
    {
      WindowHeight = CurrSettings.UseUnityStyle ? 385 : 395 * CurrSettings.CurrentUIScale;
      WindowWidth = 405 * CurrSettings.CurrentUIScale;
      MinHeight = 300 * CurrSettings.CurrentUIScale;
      MinWidth = 385 * CurrSettings.CurrentUIScale;
      ViewerHeight = 300 * CurrSettings.CurrentUIScale;
      ViewerWidth = 385 * CurrSettings.CurrentUIScale;
      GuiRuleWidth = 350 * CurrSettings.CurrentUIScale;
      GuiRuleHeight = 10 * CurrSettings.CurrentUIScale;
      guiLineHeight = 20 * CurrSettings.CurrentUIScale;
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
