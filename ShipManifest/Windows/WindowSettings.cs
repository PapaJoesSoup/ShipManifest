using System;
using ShipManifest.InternalObjects;
using ShipManifest.Windows.Tabs;
using ShipManifest.Windows.Tabs.Settings;
using UnityEngine;

namespace ShipManifest.Windows
{
  internal static class WindowSettings
  {
    #region Settings Window (GUI)

    internal static string Title = "Ship Manifest Settings";
    internal static Rect Position = new Rect(0, 0, 0, 0);
    internal static bool ShowWindow;
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;
    internal static string ToolTip = "";
    private static Tab _selectedTab = Tab.Realism;

    private static Vector2 _displayViewerPosition = Vector2.zero;

    internal static void Display(int windowId)
    {
      Title = SMUtils.Localize("#smloc_settings_001");
      // set input locks when mouseover window...
      //_inputLocked = GuiUtils.PreventClickthrough(ShowWindow, Position, _inputLocked);

      // Reset Tooltip active flag...
      ToolTipActive = false;

      Rect rect = new Rect(Position.width - 20, 4, 16, 16);
      // "Close Window.\r\nSettings will not be immediately saved,\r\n but will be remembered while in game.")))
      if (GUI.Button(rect, new GUIContent("", SMUtils.Localize("#smloc_settings_tt_001"))))
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
        GUILayout.Height(300), GUILayout.Width(380));
      GUILayout.BeginVertical();

      DisplaySelectedTab(_displayViewerPosition);

      GUILayout.EndVertical();
      GUILayout.EndScrollView();

      GUILayout.BeginHorizontal();
      // Save
      GUIContent label = new GUIContent(SMUtils.Localize("#smloc_settings_002"), SMUtils.Localize("#smloc_settings_tt_002"));
      if (GUILayout.Button(label, GUILayout.Height(20)))
      {
        ToolTip = "";
        SMSettings.SaveIntervalSec = int.Parse(TabConfig.TxtSaveInterval);
        SMSettings.SaveSettings();

        // Sync SM to CLS override settings with CLS
        if (SMSettings.EnableCls && HighLogic.LoadedSceneIsFlight)
        {
          SMSettings.UpdateClsOverride();
        }

        if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
          SMAddon.OnSmSettingsClicked();
        else
          ShowWindow = false;
      }
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      // Cancel
      label = new GUIContent(SMUtils.Localize("#smloc_settings_003"), SMUtils.Localize("#smloc_settings_tt_003"));
      if (GUILayout.Button(label, GUILayout.Height(20)))
      {
        // We've canclled, so restore original settings.
        ToolTip = "";
        SMSettings.MemRestoreTempSettings();

        if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
          SMAddon.OnSmSettingsClicked();
        else
        {
          SMSettings.MemStoreTempSettings();
          ShowWindow = false;
        }
      }
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      GUILayout.EndHorizontal();
      GUILayout.EndVertical();

      GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
      SMAddon.RepositionWindow(ref Position);
    }

    private static void DisplayTabButtons()
    {
      GUILayout.BeginHorizontal();

      GUIStyle realismStyle = _selectedTab == Tab.Realism ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      GUIContent label = new GUIContent(SMUtils.Localize("#smloc_settings_004"), SMUtils.Localize("#smloc_settings_tt_004"));
      if (GUILayout.Button(label, realismStyle, GUILayout.Height(20)))
      {
        _selectedTab = Tab.Realism;
      }
      GUI.enabled = true;
      Rect rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      GUIStyle highlightStyle = _selectedTab == Tab.Highlight ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      //label = new GUIContent("Highlight", "This tab shows all settings related to highlighting.");
      label = new GUIContent(SMUtils.Localize("#smloc_settings_005"), SMUtils.Localize("#smloc_settings_tt_005"));
      if (GUILayout.Button(label, highlightStyle, GUILayout.Height(20)))
      {
        _selectedTab = Tab.Highlight;
      }
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      GUIStyle tooltipStyle = _selectedTab == Tab.ToolTips ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      //label = new GUIContent("ToolTip", "This tab shows all settings related to tooltip behavior.");
      label = new GUIContent(SMUtils.Localize("#smloc_settings_006"), SMUtils.Localize("#smloc_settings_tt_006"));
      if (GUILayout.Button(label, tooltipStyle, GUILayout.Height(20)))
      {
        _selectedTab = Tab.ToolTips;
      }
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      GUIStyle soundStyle = _selectedTab == Tab.Sounds ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      //label = new GUIContent("Sound", "This tab shows all settings related to sounds.");
      label = new GUIContent(SMUtils.Localize("#smloc_settings_007"), SMUtils.Localize("#smloc_settings_tt_007"));
      if (GUILayout.Button(label, soundStyle, GUILayout.Height(20)))
      {
        _selectedTab = Tab.Sounds;
      }
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      GUIStyle configStyle = _selectedTab == Tab.Config ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      //label = new GUIContent("Config", "This tab shows all settings related to sounds.");
      label = new GUIContent(SMUtils.Localize("#smloc_settings_008"), SMUtils.Localize("#smloc_settings_tt_008"));
      if (GUILayout.Button(label, configStyle, GUILayout.Height(20)))
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

    private enum Tab
    {
      Realism,
      Config,
      Highlight,
      Mods,
      Sounds,
      ToolTips
    }

    #endregion
  }
}