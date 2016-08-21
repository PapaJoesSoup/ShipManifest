using System;
using ShipManifest.InternalObjects;
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
    private static Tab _selectedTab = Tab.Realism;

    private static Vector2 _displayViewerPosition = Vector2.zero;

    internal static void Display(int windowId)
    {
      // Reset Tooltip active flag...
      ToolTipActive = false;

      Rect rect = new Rect(Position.width - 20, 4, 16, 16);
      if (GUI.Button(rect,
        new GUIContent("",
          "Close Window.\r\nSettings will not be immediately saved,\r\n but will be remembered while in game.")))
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
      if (GUILayout.Button("Save", GUILayout.Height(20)))
      {
        SMSettings.SaveIntervalSec = int.Parse(TabConfig.TxtSaveInterval);
        SMSettings.SaveSettings();
        if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
          SMAddon.OnSmSettingsClicked();
        else
          ShowWindow = false;
      }
      if (GUILayout.Button("Cancel", GUILayout.Height(20)))
      {
        // We've canclled, so restore original settings.
        SMSettings.MemRestoreTempSettings();

        if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
          SMAddon.OnSmSettingsClicked();
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

      GUIStyle realismStyle = _selectedTab == Tab.Realism ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button("Realism", realismStyle, GUILayout.Height(20)))
      {
        _selectedTab = Tab.Realism;
      }
      GUI.enabled = true;
      GUIStyle highlightStyle = _selectedTab == Tab.Highlight ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button("Highlight", highlightStyle, GUILayout.Height(20)))
      {
        _selectedTab = Tab.Highlight;
      }
      GUIStyle tooltipStyle = _selectedTab == Tab.ToolTips ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button("ToolTip", tooltipStyle, GUILayout.Height(20)))
      {
        _selectedTab = Tab.ToolTips;
      }
      GUIStyle soundStyle = _selectedTab == Tab.Sounds ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button("Sound", soundStyle, GUILayout.Height(20)))
      {
        _selectedTab = Tab.Sounds;
      }
      GUIStyle configStyle = _selectedTab == Tab.Config ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button("Config", configStyle, GUILayout.Height(20)))
      {
        _selectedTab = Tab.Config;
      }
      GUIStyle modStyle = _selectedTab == Tab.Mods ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button("Mods", modStyle, GUILayout.Height(20)))
      {
        _selectedTab = Tab.Mods;
      }
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
        case Tab.Mods:
          TabInstalledMods.Display(displayViewerPosition);
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