using System;
using ShipManifest.InternalObjects;
using ShipManifest.Windows.Tabs.Control;
using UnityEngine;

namespace ShipManifest.Windows
{
  internal static class WindowControl
  {
    internal static Rect Position = SMSettings.DefaultPosition;
    internal static Vector2 _displayViewerPosition = Vector2.zero;

    internal static bool ShowWindow;
    internal static string ToolTip = "";
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;
    private static Tab _selectedTab = Tab.None;

    // Tab only vars, used in each tab
    internal static Rect TabBox = new Rect(0, 0, 450, 200);
    internal static string TabRule = new string('_', 120);
    internal const float GuiRuleWidth = 420;

    // Content strings
    internal static string Title = SmUtils.SmTags["#smloc_control_001"];
    internal static GUIContent closeContent = new GUIContent("", SmUtils.SmTags["#smloc_window_tt_001"]);
    internal static GUIContent hatchContent = new GUIContent(SmUtils.SmTags["#smloc_control_002"], SmUtils.SmTags["#smloc_control_tt_001"]);
    internal static GUIContent solarContent = new GUIContent(SmUtils.SmTags["#smloc_control_003"], SmUtils.SmTags["#smloc_control_tt_002"]);
    internal static GUIContent antennaContent = new GUIContent(SmUtils.SmTags["#smloc_control_004"], SmUtils.SmTags["#smloc_control_tt_003"]);
    internal static GUIContent lightContent = new GUIContent(SmUtils.SmTags["#smloc_control_005"], SmUtils.SmTags["#smloc_control_tt_004"]);
    internal static GUIContent labsContent = new GUIContent(SmUtils.SmTags["#smloc_control_006"], SmUtils.SmTags["#smloc_control_tt_005"]);
    internal static GUIContent vesselContent = new GUIContent(SmUtils.SmTags["#smloc_control_017"], SmUtils.SmTags["#smloc_control_tt_006"]);
    internal static GUIContent radiatorContent = new GUIContent(SmUtils.SmTags["#smloc_control_020"], SmUtils.SmTags["#smloc_control_tt_007"]);
    internal static GUIContent retSolarContent = new GUIContent(SmUtils.SmTags["#smloc_control_016"]);
    internal static GUIContent extSolarContent = new GUIContent(SmUtils.SmTags["#smloc_control_007"]);
    internal static GUIContent retRadiateContent = new GUIContent(SmUtils.SmTags["#smloc_control_022"]);
    internal static GUIContent extRadiateContent = new GUIContent(SmUtils.SmTags["#smloc_control_021"]);
    internal static GUIContent closenHatchContent = new GUIContent(SmUtils.SmTags["#smloc_control_008"]);
    internal static GUIContent openHatchContent = new GUIContent(SmUtils.SmTags["#smloc_control_009"]);
    internal static GUIContent retAntennaContent = new GUIContent(SmUtils.SmTags["#smloc_control_010"]);
    internal static GUIContent extAntennaContent = new GUIContent(SmUtils.SmTags["#smloc_control_011"]);
    internal static GUIContent offLightContent = new GUIContent(SmUtils.SmTags["#smloc_control_012"]);
    internal static GUIContent onLightContent = new GUIContent(SmUtils.SmTags["#smloc_control_013"]);
    internal static GUIContent onLabsContent = new GUIContent(SmUtils.SmTags["#smloc_control_014"]);
    internal static GUIContent offLabsContent = new GUIContent(SmUtils.SmTags["#smloc_control_015"]);
    internal static GUIContent selVesselContent = new GUIContent(SmUtils.SmTags["#smloc_control_018"]);
    internal static GUIContent clrVesselContent = new GUIContent(SmUtils.SmTags["#smloc_control_019"]);


    internal static void Display(int _windowId)
    {

      // set input locks when mouseover window...
      //_inputLocked = GuiUtils.PreventClickthrough(ShowWindow, Position, _inputLocked);

      // Reset Tooltip active flag...
      ToolTipActive = false;

      // "Close Window"
      Rect rect = new Rect(Position.width - 20, 4, 16, 16);
      if (GUI.Button(rect, closeContent))
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
      DisplaySelectedTab();

      DisplayTabActions();
      GUILayout.EndVertical();

      GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
      SMAddon.RepositionWindow(ref Position);
    }

    internal static void DisplayWindowTabs()
    {
      // Vessels
      Rect rect;
      GUILayout.BeginHorizontal();
      GUI.enabled = true;
      GUIStyle vesselsStyle = _selectedTab == Tab.Vessel ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button(vesselContent, vesselsStyle, GUILayout.Height(20))) 
      {
        try
        {
          SMAddon.SmVessel.GetDockedVessels();
          _selectedTab = Tab.Vessel;
        }
        catch (Exception ex)
        {
          SmUtils.LogMessage(
            $" opening Solar Panels Tab.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}", SmUtils.LogType.Error,
            true);
        }
      }
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      // Hatches Tab
      if (SMSettings.EnableCls)
      {
        GUIStyle hatchesStyle = _selectedTab == Tab.Hatch ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
        if (GUILayout.Button(hatchContent, hatchesStyle, GUILayout.Height(20))) // "Hatches"
        {
          try
          {
            SMAddon.UpdateClsSpaces();
            SMAddon.SmVessel.GetHatches();
            _selectedTab = Tab.Hatch;
          }
          catch (Exception ex)
          {
            SmUtils.LogMessage(
              $" opening Hatches Tab.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}",
              SmUtils.LogType.Error, true);
          }
        }
        rect = GUILayoutUtility.GetLastRect();
        if (Event.current.type == EventType.Repaint && ShowToolTips)
          ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);
      }
      GUI.enabled = true;

      // Solar Tab
      GUIStyle panelsStyle = _selectedTab == Tab.Panel ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button(solarContent, panelsStyle, GUILayout.Height(20))) // "Solar Panels"
      {
        try
        {
          SMAddon.SmVessel.GetSolarPanels();
          _selectedTab = Tab.Panel;
        }
        catch (Exception ex)
        {
          SmUtils.LogMessage(
            $" opening Solar Panels Tab.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}", SmUtils.LogType.Error,
            true);
        }
      }
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      // Radiator Tab
      GUIStyle radiatorsStyle = _selectedTab == Tab.Radiator ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button(radiatorContent, radiatorsStyle, GUILayout.Height(20))) // "Radiators"
      {
        try
        {
          SMAddon.SmVessel.GetRadiators();
          _selectedTab = Tab.Radiator;
        }
        catch (Exception ex)
        {
          SmUtils.LogMessage(
            $" opening Radiators Tab.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}", SmUtils.LogType.Error,
            true);
        }
      }
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      // Antenna Tab
      GUIStyle antennaStyle = _selectedTab == Tab.Antenna ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button(antennaContent, antennaStyle, GUILayout.Height(20))) // "Antennas"
      {
        try
        {
          SMAddon.SmVessel.GetAntennas();
          _selectedTab = Tab.Antenna;
        }
        catch (Exception ex)
        {
          SmUtils.LogMessage(
            $" opening Antennas Tab.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}", SmUtils.LogType.Error, true);
        }
      }
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      // Lights Tab
      GUIStyle lightsStyle = _selectedTab == Tab.Light ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button(lightContent, lightsStyle, GUILayout.Height(20))) // "Lights"
      {
        try
        {
          SMAddon.SmVessel.GetLights();
          _selectedTab = Tab.Light;
        }
        catch (Exception ex)
        {
          SmUtils.LogMessage(
            $" opening Lights Tab.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}", SmUtils.LogType.Error, true);
        }
      }
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      // Labs Tab
      GUIStyle labsStyle = _selectedTab == Tab.Lab ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button(labsContent, labsStyle, GUILayout.Height(20))) // "Labs"
      {
        try
        {
          SMAddon.SmVessel.GetLabs();
          _selectedTab = Tab.Lab;
        }
        catch (Exception ex)
        {
          SmUtils.LogMessage(
            $" opening Labs Tab.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}", SmUtils.LogType.Error, true);
        }
      }
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      GUILayout.EndHorizontal();
    }

    internal static void DisplaySelectedTab()
    {
      _displayViewerPosition = GUILayout.BeginScrollView(_displayViewerPosition, SMStyle.ScrollStyle,
       GUILayout.Height(TabBox.height), GUILayout.Width(TabBox.width));
      switch (_selectedTab)
      {
        case Tab.Vessel:
          TabVessel.Display();
          break;
        case Tab.Hatch:
          TabHatch.Display();
          break;
        case Tab.Panel:
          TabSolarPanel.Display();
          break;
        case Tab.Radiator:
          TabRadiator.Display();
          break;
        case Tab.Antenna:
          TabAntenna.Display();
          break;
        case Tab.Light:
          TabLight.Display();
          break;
        case Tab.Lab:
          TabScienceLab.Display();
          break;
        case Tab.None:
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
      GUILayout.EndScrollView();
    }

    internal static void DisplayTabActions()
    {
      GUILayout.BeginHorizontal();
      switch (_selectedTab)
      {
        case Tab.Panel:
          GUI.enabled = SMAddon.SmVessel.SolarPanels.Count > 0 && (!SMSettings.RealControl || SMConditions.IsShipControllable());
          if (GUILayout.Button(retSolarContent, GUILayout.Height(20))) // "Retract All Solar Panels"
            TabSolarPanel.RetractAllPanels();
          if (GUILayout.Button(extSolarContent, GUILayout.Height(20))) // "Extend All Solar Panels"
            TabSolarPanel.ExtendAllPanels();
          break;
        case Tab.Radiator:
          GUI.enabled = SMAddon.SmVessel.Radiators.Count > 0 && (!SMSettings.RealControl || SMConditions.IsShipControllable());
          if (GUILayout.Button(retRadiateContent, GUILayout.Height(20))) // "Retract All Radiators"
            TabRadiator.RetractAllRadiators();
          if (GUILayout.Button(extRadiateContent, GUILayout.Height(20))) // "Extend All Radiators"
            TabRadiator.ExtendAllRadiators();
          break;
        case Tab.Hatch:
          GUI.enabled = SMAddon.SmVessel.Hatches.Count > 0 && (!SMSettings.RealControl || SMConditions.IsShipControllable());
          if (GUILayout.Button(closenHatchContent, GUILayout.Height(20))) // "Close All Hatches"
            TabHatch.CloseAllHatches();
          if (GUILayout.Button(openHatchContent, GUILayout.Height(20))) // "Open All Hatches"
            TabHatch.OpenAllHatches();
          break;
        case Tab.Antenna:
          GUI.enabled = SMAddon.SmVessel.Antennas.Count > 0 && (!SMSettings.RealControl || SMConditions.IsShipControllable());
          if (GUILayout.Button(retAntennaContent, GUILayout.Height(20))) // "Retract All Antennas"
            TabAntenna.RetractAllAntennas();
          if (GUILayout.Button(extAntennaContent, GUILayout.Height(20))) // "Extend All Antennas"
            TabAntenna.ExtendAllAntennas();
          break;
        case Tab.Light:
          GUI.enabled = SMAddon.SmVessel.Lights.Count > 0 && (!SMSettings.RealControl || SMConditions.IsShipControllable());
          if (GUILayout.Button(offLightContent, GUILayout.Height(20))) // "Turn Off All Lights"
            TabLight.TurnOffAllLights();
          if (GUILayout.Button(onLightContent, GUILayout.Height(20))) // "Turn On All Lights"
            TabLight.TurnOnAllLights();
          break;
        case Tab.Lab:
          GUI.enabled = SMAddon.SmVessel.Labs.Count > 0 && (!SMSettings.RealControl || SMConditions.IsShipControllable());
          if (GUILayout.Button(offLabsContent, GUILayout.Height(20))) // "Turn Off All Labs"
            TabLight.TurnOffAllLights();
          if (GUILayout.Button(onLabsContent, GUILayout.Height(20))) // "Turn On All Labs"
            TabLight.TurnOnAllLights();
          break;
        case Tab.Vessel:
          GUILayout.Label("", GUILayout.Height(20));
          // Temporary commenting of code to allow release.  Will work Vessel combining in later release.
          //GUI.enabled = TabVessel.CombineVesselCount > 0;
          //if (GUILayout.Button(selVesselContent, GUILayout.Height(20))) // "Combine Selected Vessels"
          //  TabVessel.CombineSelectedVessels();
          //if (GUILayout.Button(clrVesselContent, GUILayout.Height(20))) // "Clear Vessel Selections"
          //  TabVessel.ClearVesselCount();
          break;
        case Tab.None:
          GUILayout.Label("", GUILayout.Height(20));
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
      Antenna,
      Hatch,
      Lab,
      Light,
      Panel,
      Vessel,
      Radiator
    }
  }
}
