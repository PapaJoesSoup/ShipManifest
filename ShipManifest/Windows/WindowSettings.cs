using System.Globalization;
using ShipManifest.APIClients;
using ShipManifest.Windows.Tabs;
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

    private static bool _showRealismTab = true;
    private static bool _showHighlightTab;
    private static bool _showConfigTab;
    private static bool _showSoundsTab;
    private static bool _showToolTipTab;
    private static bool _showModsTab;

    internal static bool ShowRealismTab
    {
      get
      {
        return _showRealismTab;
      }
      set
      {
        if (value)
          ResetTabs();
        _showRealismTab = value;
      }
    }
    internal static bool ShowHighlightTab
    {
      get
      {
        return _showHighlightTab;
      }
      set
      {
        if (value)
          ResetTabs();
        _showHighlightTab = value;
      }
    }
    internal static bool ShowConfigTab
    {
      get
      {
        return _showConfigTab;
      }
      set
      {
        if (value)
          ResetTabs();
        _showConfigTab = value;
      }
    }
    internal static bool ShowSoundsTab
    {
      get
      {
        return _showSoundsTab;
      }
      set
      {
        if (value)
          ResetTabs();
        _showSoundsTab = value;
      }
    }
    internal static bool ShowToolTipTab
    {
      get
      {
        return _showToolTipTab;
      }
      set
      {
        if (value)
          ResetTabs();
        _showToolTipTab = value;
      }
    }
    internal static bool ShowModsTab
    {
      get
      {
        return _showModsTab;
      }
      set
      {
        if (value)
          ResetTabs();
        _showModsTab = value;
      }
    }

    private static Vector2 _displayViewerPosition = Vector2.zero;
    internal static void Display(int windowId)
    {
      // Reset Tooltip active flag...
      ToolTipActive = false;

      var rect = new Rect(Position.width - 20, 4, 16, 16);
      if (GUI.Button(rect, new GUIContent("", "Close Window.\r\nSettings will not be immediately saved,\r\n but will be remembered while in game.")))
      {
        ToolTip = "";
        if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
          SMAddon.OnSmSettingsToggle();
        else
        {
          SMSettings.MemStoreTempSettings();
          ShowWindow = false;
        }
      }
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, Position, GUI.tooltip, ref ToolTipActive, 10, 0);

      GUILayout.BeginVertical();

      DisplayTabButtons();

      _displayViewerPosition = GUILayout.BeginScrollView(_displayViewerPosition, SMStyle.ScrollStyle, GUILayout.Height(300), GUILayout.Width(380));
      GUILayout.BeginVertical();

      DisplaySelectedTab(_displayViewerPosition);

      GUILayout.EndVertical();
      GUILayout.EndScrollView();

      GUILayout.BeginHorizontal();
      if (GUILayout.Button("Save", GUILayout.Height(20)))
      {
        SMSettings.SaveIntervalSec = int.Parse(TabConfig.TxtSaveInterval);
        SMSettings.SaveSettings();
        if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
          SMAddon.OnSmSettingsToggle();
        else
          ShowWindow = false;
      }
      if (GUILayout.Button("Cancel", GUILayout.Height(20)))
      {
        // We've canclled, so restore original settings.
        SMSettings.MemRestoreTempSettings();

        if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
          SMAddon.OnSmSettingsToggle();
        else
        {
          SMSettings.MemStoreTempSettings();
          ShowWindow = false;
        }
      }
      GUILayout.EndHorizontal();
      GUILayout.EndVertical();

      GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
      SMAddon.RepositionWindow(ref Position);
    }

    private static void DisplayTabButtons()
    {
      GUILayout.BeginHorizontal();

      var realismStyle = ShowRealismTab ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button("Realism", realismStyle, GUILayout.Height(20)))
      {
        ShowRealismTab = true;
      }
      GUI.enabled = true;
      var highlightStyle = ShowHighlightTab ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button("Highlight", highlightStyle, GUILayout.Height(20)))
      {
        ShowHighlightTab = true;
      }
      var tooltipStyle = ShowToolTipTab ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button("ToolTip", tooltipStyle, GUILayout.Height(20)))
      {
        ShowToolTipTab = true;
      }
      var soundStyle = ShowSoundsTab ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button("Sound", soundStyle, GUILayout.Height(20)))
      {
        ShowSoundsTab = true;
      }
      var configStyle = ShowConfigTab ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button("Config", configStyle, GUILayout.Height(20)))
      {
        ShowConfigTab = true;
      }
      var modStyle = ShowModsTab ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button("Mods", modStyle, GUILayout.Height(20)))
      {
        ShowModsTab = true;
      }
      GUILayout.EndHorizontal();
    }

    internal static void DisplaySelectedTab(Vector2 displayViewerPosition)
    {
      if (ShowRealismTab)
        TabRealism.Display(displayViewerPosition);
      else if (ShowHighlightTab)
        TabHighlight.Display(displayViewerPosition);
      else if (ShowSoundsTab)
        TabSounds.Display(displayViewerPosition);
      else if (ShowToolTipTab)
        TabToolTips.Display(displayViewerPosition);
      else if (ShowConfigTab)
        TabConfig.Display(displayViewerPosition);
      else if (ShowModsTab)
        TabInstalledMods.Display(displayViewerPosition);
    }

    private static void ResetTabs()
    {
      _showRealismTab = _showHighlightTab = _showToolTipTab = _showSoundsTab = _showConfigTab = _showModsTab = false;
    }

    #endregion
  }
}
