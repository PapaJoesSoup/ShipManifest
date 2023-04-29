using System;
using ShipManifest.InternalObjects;
using ShipManifest.InternalObjects.Settings;
using ShipManifest.Windows.Tabs.Control;
using UnityEngine;

namespace ShipManifest.Windows
{
  internal static class WindowControl
  {

    static WindowControl()
    {
      RefreshUIScale();
    }

    internal static float HeightScale;
    internal static float WidthScale;

    internal static bool ResizingWindow = false;
    internal static Rect Position = CurrSettings.DefaultPosition;
    internal static Vector2 _displayViewerPosition = Vector2.zero;
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
    internal static string ToolTip = "";
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;
    private static Tab _selectedTab = Tab.None;


    // Tab only vars, used in each tab
    internal static float WindowHeight;
    internal static float WindowWidth;
    internal static float MinHeight;
    internal static float MinWidth;
    internal static float ViewerWidth;
    internal static float ViewerHeight;
    internal static float GuiRuleWidth;
    internal static float GuiRuleHeight;
    internal static float guiLineHeight;

    internal static Rect TabBox = new Rect(0, 0, ViewerWidth, ViewerHeight);
    internal static string TabRule = new string('_', 120);

    // Content strings
    internal static string Title                  = SmUtils.SmTags["#smloc_control_001"];
    internal static GUIContent closeContent       = new GUIContent("", SmUtils.SmTags["#smloc_window_tt_001"]);
    internal static GUIContent hatchContent       = new GUIContent(SmUtils.SmTags["#smloc_control_002"], SmUtils.SmTags["#smloc_control_tt_001"]);
    internal static GUIContent solarContent       = new GUIContent(SmUtils.SmTags["#smloc_control_003"], SmUtils.SmTags["#smloc_control_tt_002"]);
    internal static GUIContent antennaContent     = new GUIContent(SmUtils.SmTags["#smloc_control_004"], SmUtils.SmTags["#smloc_control_tt_003"]);
    internal static GUIContent lightContent       = new GUIContent(SmUtils.SmTags["#smloc_control_005"], SmUtils.SmTags["#smloc_control_tt_004"]);
    internal static GUIContent labsContent        = new GUIContent(SmUtils.SmTags["#smloc_control_006"], SmUtils.SmTags["#smloc_control_tt_005"]);
    internal static GUIContent vesselContent      = new GUIContent(SmUtils.SmTags["#smloc_control_017"], SmUtils.SmTags["#smloc_control_tt_006"]);
    internal static GUIContent radiatorContent    = new GUIContent(SmUtils.SmTags["#smloc_control_020"], SmUtils.SmTags["#smloc_control_tt_007"]);
    internal static GUIContent retSolarContent    = new GUIContent(SmUtils.SmTags["#smloc_control_016"]);
    internal static GUIContent extSolarContent    = new GUIContent(SmUtils.SmTags["#smloc_control_007"]);
    internal static GUIContent retRadiateContent  = new GUIContent(SmUtils.SmTags["#smloc_control_022"]);
    internal static GUIContent extRadiateContent  = new GUIContent(SmUtils.SmTags["#smloc_control_021"]);
    internal static GUIContent closenHatchContent = new GUIContent(SmUtils.SmTags["#smloc_control_008"]);
    internal static GUIContent openHatchContent   = new GUIContent(SmUtils.SmTags["#smloc_control_009"]);
    internal static GUIContent retAntennaContent  = new GUIContent(SmUtils.SmTags["#smloc_control_010"]);
    internal static GUIContent extAntennaContent  = new GUIContent(SmUtils.SmTags["#smloc_control_011"]);
    internal static GUIContent offLightContent    = new GUIContent(SmUtils.SmTags["#smloc_control_012"]);
    internal static GUIContent onLightContent     = new GUIContent(SmUtils.SmTags["#smloc_control_013"]);
    internal static GUIContent onLabsContent      = new GUIContent(SmUtils.SmTags["#smloc_control_014"]);
    internal static GUIContent offLabsContent     = new GUIContent(SmUtils.SmTags["#smloc_control_015"]);
    internal static GUIContent selVesselContent   = new GUIContent(SmUtils.SmTags["#smloc_control_018"]);
    internal static GUIContent clrVesselContent   = new GUIContent(SmUtils.SmTags["#smloc_control_019"]);


    internal static void Display(int windowId)
    {

      // set input locks when mouseover window...
      _inputLocked = GuiUtils.PreventClickthrough(ShowWindow, Position, _inputLocked);

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
      GUILayout.Label("", SMStyle.SMSkin.label, GUILayout.Height(3 * CurrSettings.CurrentUIScale));
      DisplayWindowTabs();

      // This is a scroll panel (we are using it to make button lists...)
      DisplaySelectedTab();

      DisplayTabActions();
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

    internal static void DisplayWindowTabs()
    {

      // Vessels
      Rect rect;
      GUILayout.BeginHorizontal();
      GUI.enabled = true;
      GUIStyle vesselsStyle = _selectedTab == Tab.Vessel ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button(vesselContent, vesselsStyle, GUILayout.Height(guiLineHeight))) 
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
      if (CurrSettings.EnableCls)
      {
        GUIStyle hatchesStyle = _selectedTab == Tab.Hatch ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
        if (GUILayout.Button(hatchContent, hatchesStyle, GUILayout.Height(guiLineHeight))) // "Hatches"
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
      if (GUILayout.Button(solarContent, panelsStyle, GUILayout.Height(guiLineHeight))) // "Solar Panels"
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
      if (GUILayout.Button(radiatorContent, radiatorsStyle, GUILayout.Height(guiLineHeight))) // "Radiators"
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
      if (GUILayout.Button(antennaContent, antennaStyle, GUILayout.Height(guiLineHeight))) // "Antennas"
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
      if (GUILayout.Button(lightContent, lightsStyle, GUILayout.Height(guiLineHeight))) // "Lights"
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
      if (GUILayout.Button(labsContent, labsStyle, GUILayout.Height(guiLineHeight))) // "Labs"
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
       GUILayout.Height((ViewerHeight + HeightScale)), GUILayout.Width(ViewerWidth + WidthScale), GUILayout.MinHeight(MinHeight + HeightScale));
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
          GUI.enabled = SMAddon.SmVessel.SolarPanels.Count > 0 && (!CurrSettings.RealControl || SMConditions.IsShipControllable());
          if (GUILayout.Button(retSolarContent, SMStyle.SMSkin.button, GUILayout.Height(guiLineHeight))) // "Retract All Solar Panels"
            TabSolarPanel.RetractAllPanels();
          if (GUILayout.Button(extSolarContent, SMStyle.SMSkin.button, GUILayout.Height(guiLineHeight))) // "Extend All Solar Panels"
            TabSolarPanel.ExtendAllPanels();
          break;
        case Tab.Radiator:
          GUI.enabled = SMAddon.SmVessel.Radiators.Count > 0 && (!CurrSettings.RealControl || SMConditions.IsShipControllable());
          if (GUILayout.Button(retRadiateContent, SMStyle.SMSkin.button, GUILayout.Height(guiLineHeight))) // "Retract All Radiators"
            TabRadiator.RetractAllRadiators();
          if (GUILayout.Button(extRadiateContent, SMStyle.SMSkin.button, GUILayout.Height(guiLineHeight))) // "Extend All Radiators"
            TabRadiator.ExtendAllRadiators();
          break;
        case Tab.Hatch:
          GUI.enabled = SMAddon.SmVessel.Hatches.Count > 0 && (!CurrSettings.RealControl || SMConditions.IsShipControllable());
          if (GUILayout.Button(closenHatchContent, GUILayout.Height(guiLineHeight))) // "Close All Hatches"
            TabHatch.CloseAllHatches();
          if (GUILayout.Button(openHatchContent, SMStyle.SMSkin.button, GUILayout.Height(guiLineHeight))) // "Open All Hatches"
            TabHatch.OpenAllHatches();
          break;
        case Tab.Antenna:
          GUI.enabled = SMAddon.SmVessel.Antennas.Count > 0 && (!CurrSettings.RealControl || SMConditions.IsShipControllable());
          if (GUILayout.Button(retAntennaContent, SMStyle.SMSkin.button, GUILayout.Height(guiLineHeight))) // "Retract All Antennas"
            TabAntenna.RetractAllAntennas();
          if (GUILayout.Button(extAntennaContent, SMStyle.SMSkin.button, GUILayout.Height(guiLineHeight))) // "Extend All Antennas"
            TabAntenna.ExtendAllAntennas();
          break;
        case Tab.Light:
          GUI.enabled = SMAddon.SmVessel.Lights.Count > 0 && (!CurrSettings.RealControl || SMConditions.IsShipControllable());
          if (GUILayout.Button(offLightContent, SMStyle.SMSkin.button, GUILayout.Height(guiLineHeight))) // "Turn Off All Lights"
            TabLight.TurnOffAllLights();
          if (GUILayout.Button(onLightContent, SMStyle.SMSkin.button, GUILayout.Height(guiLineHeight))) // "Turn On All Lights"
            TabLight.TurnOnAllLights();
          break;
        case Tab.Lab:
          GUI.enabled = SMAddon.SmVessel.Labs.Count > 0 && (!CurrSettings.RealControl || SMConditions.IsShipControllable());
          if (GUILayout.Button(offLabsContent, SMStyle.SMSkin.button, GUILayout.Height(guiLineHeight))) // "Turn Off All Labs"
            TabLight.TurnOffAllLights();
          if (GUILayout.Button(onLabsContent, SMStyle.SMSkin.button, GUILayout.Height(guiLineHeight))) // "Turn On All Labs"
            TabLight.TurnOnAllLights();
          break;
        case Tab.Vessel:
          GUILayout.Label("", SMStyle.SMSkin.label, GUILayout.Height(guiLineHeight));
          // Temporary commenting of code to allow release.  Will work Vessel combining in later release.
          //GUI.enabled = TabVessel.CombineVesselCount > 0;
          //if (GUILayout.Button(selVesselContent, SMStyle.SMSkin.button, GUILayout.Height(guiLineHeight))) // "Combine Selected Vessels"
          //  TabVessel.CombineSelectedVessels();
          //if (GUILayout.Button(clrVesselContent, SMStyle.SMSkin.button, GUILayout.Height(guiLineHeight))) // "Clear Vessel Selections"
          //  TabVessel.ClearVesselCount();
          break;
        case Tab.None:
          GUILayout.Label("", SMStyle.SMSkin.label, GUILayout.Height(guiLineHeight));
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
      GUI.enabled = true;
      GUILayout.EndHorizontal();
    }

    internal static void RefreshUIScale()
    {
      WindowHeight = CurrSettings.UseUnityStyle? 285 : 295 * CurrSettings.CurrentUIScale;
      WindowWidth = 450 * CurrSettings.CurrentUIScale;
      MinHeight = 200 * CurrSettings.CurrentUIScale;
      MinWidth = 450 * CurrSettings.CurrentUIScale;
      ViewerWidth = 430 * CurrSettings.CurrentUIScale;
      ViewerHeight = 200 * CurrSettings.CurrentUIScale;
      GuiRuleWidth = 420 * CurrSettings.CurrentUIScale;
      GuiRuleHeight = 10 * CurrSettings.CurrentUIScale;
      guiLineHeight = 20 * CurrSettings.CurrentUIScale;
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
