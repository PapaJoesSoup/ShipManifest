using System;
using ShipManifest.InternalObjects;
using ShipManifest.Windows.Tabs.Control;
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
      Title = SMUtils.Localize("#smloc_control_001");

      // set input locks when mouseover window...
      //_inputLocked = GuiUtils.PreventClickthrough(ShowWindow, Position, _inputLocked);

      // Reset Tooltip active flag...
      ToolTipActive = false;

      Rect rect = new Rect(Position.width - 20, 4, 16, 16);
      if (GUI.Button(rect, new GUIContent("", SMUtils.Localize("#smloc_window_tt_001")))) // "Close Window"
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
        GUILayout.Height(200), GUILayout.Width(380));
      DisplaySelectedTab(_displayViewerPosition);
      GUILayout.EndScrollView();

      DisplayTabActions();
      GUILayout.EndVertical();

      GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
      SMAddon.RepositionWindow(ref Position);
    }

    internal static void DisplayWindowTabs()
    {
      Rect rect;
      GUILayout.BeginHorizontal();
      GUIContent label;
      if (SMSettings.EnableCls)
      {
        label = new GUIContent(SMUtils.Localize("#smloc_control_002"), SMUtils.Localize("#smloc_control_tt_001"));
        GUIStyle hatchesStyle = _selectedTab == Tab.Hatch ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
        if (GUILayout.Button(label, hatchesStyle, GUILayout.Height(20))) // "Hatches"
        {
          try
          {
            SMAddon.UpdateClsSpaces();
            SMAddon.SmVessel.GetHatches();
            _selectedTab = Tab.Hatch;
          }
          catch (Exception ex)
          {
            SMUtils.LogMessage(
              $" opening Hatches Tab.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}",
              SMUtils.LogType.Error, true);
          }
        }
        rect = GUILayoutUtility.GetLastRect();
        if (Event.current.type == EventType.Repaint && ShowToolTips)
          ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);
      }
      GUI.enabled = true;
      label = new GUIContent(SMUtils.Localize("#smloc_control_003"), SMUtils.Localize("#smloc_control_tt_002"));
      GUIStyle panelsStyle = _selectedTab == Tab.Panel ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button(label, panelsStyle, GUILayout.Height(20))) // "Solar Panels"
      {
        try
        {
          SMAddon.SmVessel.GetSolarPanels();
          _selectedTab = Tab.Panel;
        }
        catch (Exception ex)
        {
          SMUtils.LogMessage(
            $" opening Solar Panels Tab.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}", SMUtils.LogType.Error,
            true);
        }
      }
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      label = new GUIContent(SMUtils.Localize("#smloc_control_004"), SMUtils.Localize("#smloc_control_tt_003"));
      GUIStyle antennaStyle = _selectedTab == Tab.Antenna ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button(label, antennaStyle, GUILayout.Height(20))) // "Antennas"
      {
        try
        {
          SMAddon.SmVessel.GetAntennas();
          _selectedTab = Tab.Antenna;
        }
        catch (Exception ex)
        {
          SMUtils.LogMessage(
            $" opening Antennas Tab.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}", SMUtils.LogType.Error, true);
        }
      }
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      label = new GUIContent(SMUtils.Localize("#smloc_control_005"), SMUtils.Localize("#smloc_control_tt_004"));
      GUIStyle lightsStyle = _selectedTab == Tab.Light ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button(label, lightsStyle, GUILayout.Height(20))) // "Lights"
      {
        try
        {
          SMAddon.SmVessel.GetLights();
          _selectedTab = Tab.Light;
        }
        catch (Exception ex)
        {
          SMUtils.LogMessage(
            $" opening Lights Tab.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}", SMUtils.LogType.Error, true);
        }
      }
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      label = new GUIContent(SMUtils.Localize("#smloc_control_006"), SMUtils.Localize("#smloc_control_tt_005"));
      GUIStyle labsStyle = _selectedTab == Tab.Lab ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button(label, labsStyle, GUILayout.Height(20))) // "Labs"
      {
        try
        {
          SMAddon.SmVessel.GetLabs();
          _selectedTab = Tab.Lab;
        }
        catch (Exception ex)
        {
          SMUtils.LogMessage(
            $" opening Labs Tab.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}", SMUtils.LogType.Error, true);
        }
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
          GUI.enabled = SMAddon.SmVessel.SolarPanels.Count > 0 && (!SMSettings.RealControl || SMConditions.IsShipControllable());
          if (GUILayout.Button(SMUtils.Localize("#smloc_control_016"), GUILayout.Height(20))) // "Retract All Solar Panels"
            TabSolarPanel.RetractAllPanels();
          if (GUILayout.Button(SMUtils.Localize("#smloc_control_007"), GUILayout.Height(20))) // "Extend All Solar Panels"
            TabSolarPanel.ExtendAllPanels();
          break;
        case Tab.Hatch:
          GUI.enabled = SMAddon.SmVessel.Hatches.Count > 0 && (!SMSettings.RealControl || SMConditions.IsShipControllable());
          if (GUILayout.Button(SMUtils.Localize("#smloc_control_008"), GUILayout.Height(20))) // "Close All Hatches"
            TabHatch.CloseAllHatches();
          if (GUILayout.Button(SMUtils.Localize("#smloc_control_009"), GUILayout.Height(20))) // "Open All Hatches"
            TabHatch.OpenAllHatches();
          break;
        case Tab.Antenna:
          GUI.enabled = SMAddon.SmVessel.Antennas.Count > 0 && (!SMSettings.RealControl || SMConditions.IsShipControllable());
          if (GUILayout.Button(SMUtils.Localize("#smloc_control_010"), GUILayout.Height(20))) // "Retract All Antennas"
            TabAntenna.RetractAllAntennas();
          if (GUILayout.Button(SMUtils.Localize("#smloc_control_011"), GUILayout.Height(20))) // "Extend All Antennas"
            TabAntenna.ExtendAllAntennas();
          break;
        case Tab.Light:
          GUI.enabled = SMAddon.SmVessel.Lights.Count > 0 && (!SMSettings.RealControl || SMConditions.IsShipControllable());
          if (GUILayout.Button(SMUtils.Localize("#smloc_control_012"), GUILayout.Height(20))) // "Turn Off All Lights"
            TabLight.TurnOffAllLights();
          if (GUILayout.Button(SMUtils.Localize("#smloc_control_013"), GUILayout.Height(20))) // "Turn On All Lights"
            TabLight.TurnOnAllLights();
          break;
        case Tab.Lab:
          GUI.enabled = SMAddon.SmVessel.Labs.Count > 0 && (!SMSettings.RealControl || SMConditions.IsShipControllable());
          if (GUILayout.Button(SMUtils.Localize("#smloc_control_014"), GUILayout.Height(20))) // "Turn Off All Labs"
            TabLight.TurnOffAllLights();
          if (GUILayout.Button(SMUtils.Localize("#smloc_control_015"), GUILayout.Height(20))) // "Turn On All Labs"
            TabLight.TurnOnAllLights();
          break;
        case Tab.None:
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
      GUI.enabled = true;
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