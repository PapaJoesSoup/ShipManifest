using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ConnectedLivingSpace;

namespace ShipManifest
{
  static class WindowControl
  {
    internal static Rect Position = new Rect(0, 0, 0, 0);
    internal static bool ShowWindow = false;
    internal static string ToolTip = "";
    internal static bool ToolTipActive = false;
    internal static bool ShowToolTips = true;

    private static bool _showHatch;
    private static bool _showPanel;
    private static bool _showAntenna;
    private static bool _showLight;

    internal static bool ShowHatch
    {
      get
      {
        return _showHatch;
      }
      set
      {
        if (value)
          ResetTabs();
        _showHatch = value;
      }
    }
    internal static bool ShowPanel
    {
      get
      {
        return _showPanel;
      }
      set
      {
        if (value)
          ResetTabs();
        _showPanel = value;
      }
    }
    internal static bool ShowAntenna
    {
      get
      {
        return _showAntenna;
      }
      set
      {
        if (value)
          ResetTabs();
        _showAntenna = value;
      }
    }
    internal static bool ShowLight
    {
      get
      {
        return _showLight;
      }
      set
      {
        if (value)
          ResetTabs();
        _showLight = value;
      }
    }

    private static Vector2 DisplayViewerPosition = Vector2.zero;
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
        ToolTip = SMToolTips.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 10, 0);

      // This is a scroll panel (we are using it to make button lists...)
      GUILayout.BeginVertical();
      DisplayWindowTabs();
      // This is a scroll panel (we are using it to make button lists...)
      DisplayViewerPosition = GUILayout.BeginScrollView(DisplayViewerPosition, SMStyle.ScrollStyle, GUILayout.Height(200), GUILayout.Width(370));
      DisplaySelectedTab(DisplayViewerPosition);
      GUILayout.EndScrollView();

      DisplaySelectedActions();
      GUILayout.EndVertical();
      GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
      SMAddon.RepositionWindow(ref WindowControl.Position);
    }

    internal static void DisplayWindowTabs()
    {
      GUILayout.BeginHorizontal();

      if (!SMSettings.EnableCLS)
        GUI.enabled = false;
      var hatchesStyle = ShowHatch ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button("Hatches", hatchesStyle, GUILayout.Height(20)))
      {
        try
        {
          SMAddon.UpdateCLSSpaces();
          SMAddon.smController.GetHatches();
          ShowHatch = !ShowHatch;
        }
        catch (Exception ex)
        {
          Utilities.LogMessage(string.Format(" opening Hatches Tab.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
        }
      }
      GUI.enabled = true;
      var panelsStyle = ShowPanel ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button("Solar Panels", panelsStyle, GUILayout.Height(20)))
      {
        try
        {
          SMAddon.smController.GetSolarPanels();
          ShowPanel = !ShowPanel;
        }
        catch (Exception ex)
        {
          Utilities.LogMessage(string.Format(" opening Solar Panels Tab.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
        }
      }
      var antennaStyle = ShowAntenna ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button("Antennas", antennaStyle, GUILayout.Height(20)))
      {
        try
        {
          SMAddon.smController.GetAntennas();
          ShowAntenna = !ShowAntenna;
        }
        catch (Exception ex)
        {
          Utilities.LogMessage(string.Format(" opening Antennas Tab.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
        }
      }
      var lightsStyle = ShowLight ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button("Lights", lightsStyle, GUILayout.Height(20)))
      {
        try
        {
          SMAddon.smController.GetLights();
          ShowLight = !ShowLight;
        }
        catch (Exception ex)
        {
          Utilities.LogMessage(string.Format(" opening Lights Tab.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
        }
      }
      GUILayout.EndHorizontal();
    }

    internal static void DisplaySelectedTab(Vector2 DisplayViewerPosition)
    {
      if (ShowHatch)
        TabHatch.Display(DisplayViewerPosition);
      else if (ShowPanel)
        TabSolarPanel.Display(DisplayViewerPosition);
      else if (ShowAntenna)
        TabAntenna.Display(DisplayViewerPosition);
      else if (ShowLight)
        TabLight.Display(DisplayViewerPosition);
    }

    internal static void DisplaySelectedActions()
    {
      if (ShowPanel)
      {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Retract All Solar Panels", GUILayout.Height(20)))
          TabSolarPanel.RetractAllPanels();

        if (GUILayout.Button("Extend All Solar Panels", GUILayout.Height(20)))
          TabSolarPanel.ExtendAllPanels();

        GUILayout.EndHorizontal();
      }
      else if (ShowHatch)
      {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Close All Hatches", GUILayout.Height(20)))
          TabHatch.CloseAllHatches();

        if (GUILayout.Button("Open All Hatches", GUILayout.Height(20)))
          TabHatch.OpenAllHatches();

        GUILayout.EndHorizontal();
      }
      else if (ShowAntenna)
      {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Retract All Antennas", GUILayout.Height(20)))
          TabAntenna.RetractAllAntennas();

        if (GUILayout.Button("Extend All Antennas", GUILayout.Height(20)))
          TabAntenna.ExtendAllAntennas();

        GUILayout.EndHorizontal();
      }
      else if (ShowLight)
      {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Turn Off All Lights", GUILayout.Height(20)))
          TabLight.TurnOffAllLights();

        if (GUILayout.Button("Turn On All Lights", GUILayout.Height(20)))
          TabLight.TurnOnAllLights();

        GUILayout.EndHorizontal();
      }
    }

    private static void ResetTabs()
    {
      _showHatch = _showPanel = _showAntenna = _showLight = false;
    }
  }
}
