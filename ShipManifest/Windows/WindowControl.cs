using System;
using ShipManifest.InternalObjects;
using ShipManifest.Windows.Tabs;
using UnityEngine;

namespace ShipManifest.Windows
{
  internal static class WindowControl
  {
    internal static string Title = "Ship Manifest Part Control Center";
    internal static Rect Position = new Rect(0, 0, 0, 0);
    internal static bool ShowWindow;
    internal static string ToolTip = "";
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;
    private static Tab _selectedTab = Tab.None;

    private static Vector2 _displayViewerPosition = Vector2.zero;

    internal static void Display(int windowId)
    {
      // Reset Tooltip active flag...
      ToolTipActive = false;

      Rect rect = new Rect(Position.width - 20, 4, 16, 16);
      if (GUI.Button(rect, new GUIContent("", "Close Window")))
      {
        ShowWindow = false;
        ToolTip = "";
      }
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      // This is a scroll panel (we are using it to make button lists...)
      GUILayout.BeginVertical();
      DisplayWindowTabs();
      // This is a scroll panel (we are using it to make button lists...)
      _displayViewerPosition = GUILayout.BeginScrollView(_displayViewerPosition, SMStyle.ScrollStyle,
        GUILayout.Height(200), GUILayout.Width(370));
      DisplaySelectedTab(_displayViewerPosition);
      GUILayout.EndScrollView();

      DisplayTabActions();
      GUILayout.EndVertical();
      GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
      SMAddon.RepositionWindow(ref Position);
    }

    internal static void DisplayWindowTabs()
    {
      GUILayout.BeginHorizontal();

      if (!SMSettings.EnableCls)
        GUI.enabled = false;
      GUIStyle hatchesStyle = _selectedTab == Tab.Hatch ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button("Hatches", hatchesStyle, GUILayout.Height(20)))
      {
        try
        {
          SMAddon.UpdateClsSpaces();
          SMAddon.SmVessel.GetHatches();
          _selectedTab = Tab.Hatch;
        }
        catch (Exception ex)
        {
          Utilities.LogMessage(
            string.Format(" opening Hatches Tab.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), Utilities.LogType.Error, true);
        }
      }
      GUI.enabled = true;
      GUIStyle panelsStyle = _selectedTab == Tab.Panel ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button("Solar Panels", panelsStyle, GUILayout.Height(20)))
      {
        try
        {
          SMAddon.SmVessel.GetSolarPanels();
          _selectedTab = Tab.Panel;
        }
        catch (Exception ex)
        {
          Utilities.LogMessage(
            string.Format(" opening Solar Panels Tab.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), Utilities.LogType.Error,
            true);
        }
      }
      GUIStyle antennaStyle = _selectedTab == Tab.Antenna ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button("Antennas", antennaStyle, GUILayout.Height(20)))
      {
        try
        {
          SMAddon.SmVessel.GetAntennas();
          _selectedTab = Tab.Antenna;
        }
        catch (Exception ex)
        {
          Utilities.LogMessage(
            string.Format(" opening Antennas Tab.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), Utilities.LogType.Error, true);
        }
      }
      GUIStyle lightsStyle = _selectedTab == Tab.Light ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button("Lights", lightsStyle, GUILayout.Height(20)))
      {
        try
        {
          SMAddon.SmVessel.GetLights();
          _selectedTab = Tab.Light;
        }
        catch (Exception ex)
        {
          Utilities.LogMessage(
            string.Format(" opening Lights Tab.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), Utilities.LogType.Error, true);
        }
      }
      GUILayout.EndHorizontal();
    }

    internal static void DisplaySelectedTab(Vector2 displayViewerPosition)
    {
      switch (_selectedTab)
      {
        case Tab.Hatch:
          TabHatch.Display(displayViewerPosition);
          break;
        case Tab.Panel:
          TabSolarPanel.Display(displayViewerPosition);
          break;
        case Tab.Antenna:
          TabAntenna.Display(displayViewerPosition);
          break;
        case Tab.Light:
          TabLight.Display(displayViewerPosition);
          break;
        case Tab.Lab:
          TabScienceLab.Display(displayViewerPosition);
          break;
        case Tab.None:
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    internal static void DisplayTabActions()
    {
      GUILayout.BeginHorizontal();
      switch (_selectedTab)
      {
        case Tab.Panel:
          if (GUILayout.Button("Retract All Solar Panels", GUILayout.Height(20)))
            TabSolarPanel.RetractAllPanels();
          if (GUILayout.Button("Extend All Solar Panels", GUILayout.Height(20)))
            TabSolarPanel.ExtendAllPanels();
          break;
        case Tab.Hatch:
          if (GUILayout.Button("Close All Hatches", GUILayout.Height(20)))
            TabHatch.CloseAllHatches();
          if (GUILayout.Button("Open All Hatches", GUILayout.Height(20)))
            TabHatch.OpenAllHatches();
          break;
        case Tab.Antenna:
          if (GUILayout.Button("Retract All Antennas", GUILayout.Height(20)))
            TabAntenna.RetractAllAntennas();
          if (GUILayout.Button("Extend All Antennas", GUILayout.Height(20)))
            TabAntenna.ExtendAllAntennas();
          break;
        case Tab.Light:
          if (GUILayout.Button("Turn Off All Lights", GUILayout.Height(20)))
            TabLight.TurnOffAllLights();
          if (GUILayout.Button("Turn On All Lights", GUILayout.Height(20)))
            TabLight.TurnOnAllLights();
          break;
        case Tab.Lab:
          if (GUILayout.Button("Turn Off All Labs", GUILayout.Height(20)))
            TabLight.TurnOffAllLights();
          if (GUILayout.Button("Turn On All Labs", GUILayout.Height(20)))
            TabLight.TurnOnAllLights();
          break;
        case Tab.None:
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
      GUILayout.EndHorizontal();
    }

    private enum Tab
    {
      None,
      Hatch,
      Panel,
      Antenna,
      Light,
      Lab
    }
  }
}