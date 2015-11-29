using System;
using System.Collections.Generic;
using System.Linq;
using ShipManifest.Process;
using UnityEngine;

namespace ShipManifest.Windows
{
  internal static class WindowManifest
  {
    #region Manifest Window - Gui Layout Code

    internal static string Title = "Ship's Manifest - ";
    internal static Rect Position = new Rect(0, 0, 0, 0);
    private static bool _showWindow;
    internal static bool ShowWindow
    {
      get
      {
        return _showWindow;
      }
      set
      {
        if (!value && SMAddon.SmVessel != null)
          SMHighlighter.ClearResourceHighlighting(SMAddon.SmVessel.SelectedResourcesParts);
        _showWindow = value;
      }

    }
    internal static string ToolTip = "";
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;

    // Ship Manifest Window
    // This window displays options for managing crew, resources, and flight checklists for the focused vessel.
    private static Vector2 _smScrollViewerPosition = Vector2.zero;
    private static Vector2 _resourceScrollViewerPosition = Vector2.zero;
    internal static void Display(int windowId)
    {
      Title = "Ship's Manifest - " + SMAddon.SmVessel.Vessel.vesselName;
      // Reset Tooltip active flag...
      ToolTipActive = false;

      var label = new GUIContent("", "Close Window");
      if (SMAddon.SmVessel.TransferCrewObj.CrewXferActive || TransferResource.ResourceXferActive)
      {
        label = new GUIContent("", "Action in progress.  Cannot close window");
        GUI.enabled = false;
      }
      var rect = new Rect(Position.width - 20, 4, 16, 16);
      if (GUI.Button(rect, label))
      {
        SMAddon.OnSmButtonToggle();
        ToolTip = "";
      }
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, Position, GUI.tooltip, ref ToolTipActive, 10, 0);
      GUI.enabled = true;
      try
      {
        GUILayout.BeginVertical();
        _smScrollViewerPosition = GUILayout.BeginScrollView(_smScrollViewerPosition, SMStyle.ScrollStyle, GUILayout.Height(100), GUILayout.Width(300));
        GUILayout.BeginVertical();

        if (SMAddon.SmVessel.IsRecoverable)
        {
          PreLaunchGui();
        }

        // Now the Resource Buttons
        ResourceButtonList();

        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        var resLabel = "No Resource Selected";
        if (SMAddon.SmVessel.SelectedResources.Count == 1)
          resLabel = SMAddon.SmVessel.SelectedResources[0];
        else if (SMAddon.SmVessel.SelectedResources.Count == 2)
          resLabel = "Multiple Resources selected";
        GUILayout.Label(string.Format("{0}", resLabel), GUILayout.Width(300), GUILayout.Height(20));

        // Resource Details List Viewer
        ResourceDetailsViewer();

        GUILayout.BeginHorizontal();

        var settingsStyle = WindowSettings.ShowWindow ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
        if (GUILayout.Button("Settings", settingsStyle, GUILayout.Height(20)))
        {
          try
          {
            WindowSettings.ShowWindow = !WindowSettings.ShowWindow;
            if (WindowSettings.ShowWindow)
            {
              // Store settings in case we cancel later...
              SMSettings.MemStoreTempSettings();
            }
          }
          catch (Exception ex)
          {
            Utilities.LogMessage(string.Format(" opening Settings Window.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
          }
        }

        var rosterStyle = WindowRoster.ShowWindow ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
        if (GUILayout.Button("Roster", rosterStyle, GUILayout.Height(20)))
        {
          try
          {
            WindowRoster.ShowWindow = !WindowRoster.ShowWindow;
            if (!WindowRoster.ShowWindow)
            {
              WindowRoster.SelectedKerbal = null;
              WindowRoster.ToolTip = "";
            }
            else
            {
              SMAddon.FrozenKerbals = WindowRoster.GetFrozenKerbals();
            }
          }
          catch (Exception ex)
          {
            Utilities.LogMessage(string.Format(" opening Roster Window.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
          }
        }

        var controlStyle = WindowControl.ShowWindow ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
        if (GUILayout.Button("Control", controlStyle, GUILayout.Height(20)))
        {
          try
          {
            WindowControl.ShowWindow = !WindowControl.ShowWindow;
          }
          catch (Exception ex)
          {
            Utilities.LogMessage(string.Format(" opening Control Window.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
          }
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
        SMAddon.RepositionWindow(ref Position);
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format(" in Ship Manifest Window.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
      }
    }

    #endregion

    #region Ship Manifest Window Gui components

    private static void PreLaunchGui()
    {
      try
      {
        GUILayout.BeginHorizontal();
        if (!SMSettings.RealismMode)
        {
          // Realism Mode is desirable, as there is a cost associated with a kerbal on a flight.   No cheating!
          if (GUILayout.Button("Fill Crew", SMStyle.ButtonStyle, GUILayout.Width(130), GUILayout.Height(20)))
          {
            SMAddon.SmVessel.FillCrew();
          }
          if (GUILayout.Button("Empty Crew", SMStyle.ButtonStyle, GUILayout.Width(130), GUILayout.Height(20)))
          {
            SMAddon.SmVessel.EmptyCrew();
          }          
        }
        GUILayout.EndHorizontal();

        if (SMSettings.EnablePfResources)
        {
          GUILayout.BeginHorizontal();
          if (GUILayout.Button("Fill Resources", SMStyle.ButtonStyle, GUILayout.Width(130), GUILayout.Height(20)))
          {
            SMAddon.SmVessel.FillResources();
          }
          if (GUILayout.Button("Empty Resources", SMStyle.ButtonStyle, GUILayout.Width(130), GUILayout.Height(20)))
          {
            SMAddon.SmVessel.DumpResources();
          }
          GUILayout.EndHorizontal();
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format(" in PreLaunchGui.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
      }
    }

    private static void ResourceButtonList()
    {
      try
      {
        // List required here to prevent foreach sync errors with live source.
        var keys = SMAddon.SmVessel.PartsByResource.Keys.ToList();
        foreach (var resourceName in keys)
        {
          GUILayout.BeginHorizontal();
          var width = 273;
          if ((!SMSettings.RealismMode || SMAddon.SmVessel.IsRecoverable) && resourceName != "Crew" && resourceName != "Science")
            width = 185;

          var displayAmounts = Utilities.DisplayVesselResourceTotals(resourceName);
          var style = SMAddon.SmVessel.SelectedResources.Contains(resourceName) ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
          if (GUILayout.Button(string.Format("{0}", resourceName + displayAmounts), style, GUILayout.Width(width), GUILayout.Height(20)))
          {
            ResourceButtonToggled(resourceName);
          }
          if ((!SMSettings.RealismMode || SMAddon.SmVessel.IsRecoverable) && resourceName != "Crew" && resourceName != "Science")
          {
            if (GUILayout.Button(string.Format("{0}", "Dump"), SMStyle.ButtonStyle, GUILayout.Width(45), GUILayout.Height(20)))
            {
              SMAddon.SmVessel.DumpResource(resourceName);
            }
            if (GUILayout.Button(string.Format("{0}", "Fill"), SMStyle.ButtonStyle, GUILayout.Width(35), GUILayout.Height(20)))
            {
              SMAddon.SmVessel.FillResource(resourceName);
            }
          }
          GUILayout.EndHorizontal();
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format(" in ResourceButtonList.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
      }
    }

    private static void ResourceButtonToggled(string resourceName)
    {
      try
      {
        if (!SMAddon.SmVessel.TransferCrewObj.CrewXferActive && !TransferResource.ResourceXferActive)
        {
          // First, lets clear any highlighting...
          SMHighlighter.ClearResourceHighlighting(SMAddon.SmVessel.SelectedResourcesParts);

          // Now let's update our lists...
          if (!SMAddon.SmVessel.SelectedResources.Contains(resourceName))
          {
            // now lets determine what to do with selection
            if (resourceName == "Crew" || resourceName == "Science" || resourceName == "ElectricCharge")
            {
              SMAddon.SmVessel.SelectedResources.Clear();
              SMAddon.SmVessel.SelectedResources.Add(resourceName);
            }
            else
            {
              if (SMAddon.SmVessel.SelectedResources.Contains("Crew") || SMAddon.SmVessel.SelectedResources.Contains("Science") || SMAddon.SmVessel.SelectedResources.Contains("ElectricCharge"))
              {
                SMAddon.SmVessel.SelectedResources.Clear();
                SMAddon.SmVessel.SelectedResources.Add(resourceName);
              }
              else if (SMAddon.SmVessel.SelectedResources.Count > 1)
              {
                SMAddon.SmVessel.SelectedResources.RemoveRange(0, 1);
                SMAddon.SmVessel.SelectedResources.Add(resourceName);
              }
              else
                SMAddon.SmVessel.SelectedResources.Add(resourceName);
            }
          }
          else if (SMAddon.SmVessel.SelectedResources.Contains(resourceName))
          {
            SMAddon.SmVessel.SelectedResources.Remove(resourceName);
          }

          // Now, refresh the resources parts list
          SMAddon.SmVessel.GetSelectedResourcesParts();

          // now lets reconcile the selected parts based on the new list of resources...
          ReconcileSelectedXferParts(SMAddon.SmVessel.SelectedResources);

          // Now lets update the Xfer Objects...
          SMAddon.SmVessel.ResourcesToXfer.Clear();
          if (!SMAddon.SmVessel.SelectedResources.Contains("Crew") && !SMAddon.SmVessel.SelectedResources.Contains("Science"))
          {
            foreach (var resource in SMAddon.SmVessel.SelectedResources)
            {
              // Lets create a Xfer Object for managing xfer options and data.
              var modResource = new TransferResource(resource)
              {
                SrcXferAmount =
                  TransferResource.CalcMaxResourceXferAmt(SMAddon.SmVessel.SelectedPartsSource,
                    SMAddon.SmVessel.SelectedPartsTarget, resource),
                TgtXferAmount =
                  TransferResource.CalcMaxResourceXferAmt(SMAddon.SmVessel.SelectedPartsTarget,
                    SMAddon.SmVessel.SelectedPartsSource, resource)
              };
              SMAddon.SmVessel.ResourcesToXfer.Add(modResource);
            }
          }

          // Now, based on the resourceselection, do we show the Transfer window?
          WindowTransfer.ShowWindow = SMAddon.SmVessel.SelectedResources.Count > 0;
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format(" in WindowManifest.ResourceButtonToggled.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
      }
    }

    internal static void ReconcileSelectedXferParts(List<string> resourceNames)
    {
      try
      {
        if (resourceNames.Count > 0)
        {
          var newSources = new List<Part>();
          var newTargets = new List<Part>();
          if (WindowTransfer.ShowSourceVessels && !resourceNames.Contains("Crew") && !resourceNames.Contains("Science"))
          {
            SMAddon.SmVessel.SelectedPartsSource = SMAddon.SmVessel.GetSelectedVesselsParts(SMAddon.SmVessel.SelectedVesselsSource, resourceNames);
            if (!WindowTransfer.ShowTargetVessels)
            {
              foreach (var part in SMAddon.SmVessel.SelectedPartsSource)
              {
                if (SMAddon.SmVessel.SelectedPartsTarget.Contains(part))
                  SMAddon.SmVessel.SelectedPartsTarget.Remove(part);
              }
            }
          }
          else
          {
            foreach (var part in SMAddon.SmVessel.SelectedPartsSource)
            {
              if (resourceNames.Count > 1)
              {
                if (part.Resources.Contains(resourceNames[0]) && part.Resources.Contains(resourceNames[1]))
                  newSources.Add(part);
              }
              else
              {
                if (resourceNames[0] == "Crew" && part.CrewCapacity > 0)
                  newSources.Add(part);
                else if (resourceNames[0] == "Science" && part.FindModulesImplementing<IScienceDataContainer>().Count > 0)
                  newSources.Add(part);
                else if (part.Resources.Contains(resourceNames[0]))
                  newSources.Add(part);
              }
            }
            SMAddon.SmVessel.SelectedPartsSource.Clear();
            SMAddon.SmVessel.SelectedPartsSource = newSources;
          }

          if (WindowTransfer.ShowTargetVessels && !resourceNames.Contains("Crew") && !resourceNames.Contains("Science"))
          {
            SMAddon.SmVessel.SelectedPartsTarget = SMAddon.SmVessel.GetSelectedVesselsParts(SMAddon.SmVessel.SelectedVesselsTarget, resourceNames);
            if (!WindowTransfer.ShowSourceVessels)
            {
              foreach (var part in SMAddon.SmVessel.SelectedPartsTarget)
              {
                if (SMAddon.SmVessel.SelectedPartsSource.Contains(part))
                  SMAddon.SmVessel.SelectedPartsSource.Remove(part);
              }
            }
          }
          else
          {
            foreach (var part in SMAddon.SmVessel.SelectedPartsTarget)
            {
              if (resourceNames.Count > 1)
              {
                if (part.Resources.Contains(resourceNames[0]) && part.Resources.Contains(resourceNames[1]))
                  newTargets.Add(part);
              }
              else
              {
                if (resourceNames[0] == "Crew" && part.CrewCapacity > 0)
                  newTargets.Add(part);
                else if (resourceNames[0] == "Science" && part.FindModulesImplementing<IScienceDataContainer>().Count > 0)
                  newTargets.Add(part);
                else if (part.Resources.Contains(resourceNames[0]))
                  newTargets.Add(part);
              }
            }
            SMAddon.SmVessel.SelectedPartsTarget.Clear();
            SMAddon.SmVessel.SelectedPartsTarget = newTargets;
          }
          if (resourceNames.Contains("Crew") || resourceNames.Contains("Science"))
          {
            SMAddon.SmVessel.SelectedVesselsSource.Clear();
            SMAddon.SmVessel.SelectedVesselsTarget.Clear();
          }
        }
        else
        {
          SMAddon.SmVessel.SelectedPartsSource.Clear();
          SMAddon.SmVessel.SelectedPartsTarget.Clear();
          SMAddon.SmVessel.SelectedVesselsSource.Clear();
          SMAddon.SmVessel.SelectedVesselsTarget.Clear();
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format(" in WindowManifest.ReconcileSelectedXferParts.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
      }
    }

    private static void ResourceDetailsViewer()
    {
      try
      {
        _resourceScrollViewerPosition = GUILayout.BeginScrollView(_resourceScrollViewerPosition, SMStyle.ScrollStyle, GUILayout.Height(100), GUILayout.Width(300));
        GUILayout.BeginVertical();

        if (SMAddon.SmVessel.SelectedResources.Count > 0)
        {
          foreach (var part in SMAddon.SmVessel.SelectedResourcesParts)
          {
            if (!SMAddon.SmVessel.SelectedResources.Contains("Crew") && !SMAddon.SmVessel.SelectedResources.Contains("Science"))
            {
              var noWrap = SMStyle.LabelStyleNoWrap;
              GUILayout.Label(string.Format("{0}", part.partInfo.title), noWrap, GUILayout.Width(265), GUILayout.Height(18));
              var noPad = SMStyle.LabelStyleNoPad;
              foreach (var resource in SMAddon.SmVessel.SelectedResources)
                GUILayout.Label(string.Format(" - {0}:  ({1}/{2})", resource, part.Resources[resource].amount.ToString("######0.####"), part.Resources[resource].maxAmount.ToString("######0.####")), noPad, GUILayout.Width(265), GUILayout.Height(16));
            }
            else if (SMAddon.SmVessel.SelectedResources.Contains("Crew"))
            {
              GUILayout.BeginHorizontal();
              GUILayout.Label(string.Format("{0}, ({1}/{2})", part.partInfo.title, Utilities.GetPartCrewCount(part), part.CrewCapacity), GUILayout.Width(265), GUILayout.Height(20));
              GUILayout.EndHorizontal();
            }
            else if (SMAddon.SmVessel.SelectedResources.Contains("Science"))
            {
              var scienceCount = 0;
              foreach (PartModule pm in part.Modules)
              {
                var container = pm as ModuleScienceContainer;
                if (container != null)
                  scienceCount += container.GetScienceCount();
                else if (pm is ModuleScienceExperiment)
                  scienceCount += ((ModuleScienceExperiment)pm).GetScienceCount();
              }
              GUILayout.BeginHorizontal();
              GUILayout.Label(string.Format("{0}, ({1})", part.partInfo.title, scienceCount), GUILayout.Width(265));
              GUILayout.EndHorizontal();
            }
          }
        }
      }
      catch (Exception ex)
      {
        if (!SMAddon.FrameErrTripped)
        {
          Utilities.LogMessage(string.Format(" in WindowManifest.ResourceDetailsViewer.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
          SMAddon.FrameErrTripped = true;
        }
      }
      GUILayout.EndVertical();
      GUILayout.EndScrollView();
    }

    #endregion
  }
}
