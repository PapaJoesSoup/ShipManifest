using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ShipManifest.InternalObjects;
using ShipManifest.Process;
using UnityEngine;

namespace ShipManifest.Windows
{
  internal static class WindowManifest
  {
    #region Manifest Window - Gui Layout Code

    internal static string Title = string.Format("Ship Manifest {0} - ", SMSettings.CurVersion);
    internal static Rect Position = new Rect(0, 0, 0, 0);
    private static bool _showWindow;

    internal static bool ShowWindow
    {
      get { return _showWindow; }
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
      Title = string.Format("SM {0} - {1}", SMSettings.CurVersion, SMAddon.SmVessel.Vessel.vesselName);
      // Reset Tooltip active flag...
      ToolTipActive = false;

      GUIContent label = new GUIContent("", "Close Window");
      if (SMConditions.IsTransferInProgress())
      {
        label = new GUIContent("", "Action in progress.  Cannot close window");
        GUI.enabled = false;
      }
      Rect rect = new Rect(Position.width - 20, 4, 16, 16);
      if (GUI.Button(rect, label))
      {
        SMAddon.OnSmButtonClicked();
        ToolTip = "";
      }
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);
      GUI.enabled = true;
      try
      {
        GUILayout.BeginVertical();
        _smScrollViewerPosition = GUILayout.BeginScrollView(_smScrollViewerPosition, SMStyle.ScrollStyle,
          GUILayout.Height(100), GUILayout.Width(300));
        GUILayout.BeginVertical();

        // Prelaunch (landed) Gui
        if (SMAddon.SmVessel.IsRecoverable)
        {
          PreLaunchGui();
        }

        // Now the Resource Buttons
        ResourceButtonsList();

        GUILayout.EndVertical();
        GUILayout.EndScrollView();

        string resLabel = "No Resource Selected";
        if (SMAddon.SmVessel.SelectedResources.Count == 1)
          resLabel = SMAddon.SmVessel.SelectedResources[0];
        else if (SMAddon.SmVessel.SelectedResources.Count == 2)
          resLabel = "Multiple Resources selected";
        GUILayout.Label(string.Format("{0}", resLabel), GUILayout.Width(300), GUILayout.Height(20));

        // Resource Details List Viewer
        ResourceDetailsViewer();

        // Window toggle Button List
        WindowToggleButtons();

        GUILayout.EndVertical();
        GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
        SMAddon.RepositionWindow(ref Position);
      }
      catch (Exception ex)
      {
        if (!SMAddon.FrameErrTripped)
        {
          Utilities.LogMessage(
            string.Format(" in Ship Manifest Window.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace),
            Utilities.LogType.Error, true);
          SMAddon.FrameErrTripped = true;
        }
      }
    }

    #endregion

    #region Ship Manifest Window Gui components

    private static void PreLaunchGui()
    {
      try
      {
        if (SMSettings.EnablePfCrews)
        {
          GUILayout.BeginHorizontal();
          // Realism Mode is desirable, as there is a cost associated with a kerbal on a flight.   No cheating!
          if (GUILayout.Button("Fill Crew", SMStyle.ButtonStyle, GUILayout.Width(134), GUILayout.Height(20)))
          {
            SMAddon.SmVessel.FillCrew();
          }
          if (GUILayout.Button("Empty Crew", SMStyle.ButtonStyle, GUILayout.Width(134), GUILayout.Height(20)))
          {
            SMAddon.SmVessel.EmptyCrew();
          }
          GUILayout.EndHorizontal();
        }

        if (!SMSettings.EnablePfResources) return;
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Fill Resources", SMStyle.ButtonStyle, GUILayout.Width(134), GUILayout.Height(20)))
        {
          SMAddon.SmVessel.FillResources();
        }
        if (GUILayout.Button("Empty Resources", SMStyle.ButtonStyle, GUILayout.Width(134), GUILayout.Height(20)))
        {
          SMAddon.SmVessel.DumpAllResources();
        }
        GUILayout.EndHorizontal();
      }
      catch (Exception ex)
      {
        if (!SMAddon.FrameErrTripped)
        {
          Utilities.LogMessage(string.Format(" in PreLaunchGui.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace),
            Utilities.LogType.Error, true);
          SMAddon.FrameErrTripped = true;
        }
      }
    }

    private static void ResourceButtonsList()
    {
      try
      {
        // List required here to prevent loop sync errors with live source.
        Dictionary<string, List<Part>>.KeyCollection.Enumerator keys = SMAddon.SmVessel.PartsByResource.Keys.GetEnumerator();
        while (keys.MoveNext())
        {
          if (string.IsNullOrEmpty(keys.Current)) continue;
          GUILayout.BeginHorizontal();

          // Button Widths
          int width = 273;
          if (!SMSettings.RealismMode && SMConditions.IsResourceTypeOther(keys.Current)) width = 185;
          else if (SMConditions.IsResourceTypeOther(keys.Current)) width = 223;

          // Resource Button
          string displayAmounts = string.Format("{0}{1}", keys.Current, Utilities.DisplayVesselResourceTotals(keys.Current));
          GUIStyle style = SMAddon.SmVessel.SelectedResources.Contains(keys.Current)
            ? SMStyle.ButtonToggledStyle
            : SMStyle.ButtonStyle;
          if (GUILayout.Button(displayAmounts, style, GUILayout.Width(width), GUILayout.Height(20)))
          {
            ResourceButtonToggled(keys.Current);
          }

          // Dump Button
          if (SMConditions.IsResourceTypeOther(keys.Current) && SMAddon.SmVessel.PartsByResource[keys.Current].Count > 0)
          {
            uint pumpId = TransferPump.GetPumpIdFromHash(keys.Current,
              SMAddon.SmVessel.PartsByResource[keys.Current].First(),
              SMAddon.SmVessel.PartsByResource[keys.Current].Last(), TransferPump.TypePump.Dump,
              TransferPump.TriggerButton.Manifest);
            GUIContent dumpContent = !TransferPump.IsPumpInProgress(pumpId)
              ? new GUIContent("Dump", "Dumps the selected resource in this vessel")
              : new GUIContent("Stop", "Halts the dumping of the selected resource in this vessel");
            GUI.enabled = SMConditions.CanResourceBeDumped(keys.Current);
            if (GUILayout.Button(dumpContent, SMStyle.ButtonStyle, GUILayout.Width(45), GUILayout.Height(20)))
            {
              SMVessel.ToggleDumpResource(keys.Current, pumpId);
            }
          }

          // Fill Button
          if (!SMSettings.RealismMode && SMConditions.IsResourceTypeOther(keys.Current) &&
              SMAddon.SmVessel.PartsByResource[keys.Current].Count > 0)
          {
            GUI.enabled = SMConditions.CanResourceBeFilled(keys.Current);
            if (GUILayout.Button(string.Format("{0}", "Fill"), SMStyle.ButtonStyle, GUILayout.Width(35),
              GUILayout.Height(20)))
            {
              SMAddon.SmVessel.FillResource(keys.Current);
            }
          }
          GUI.enabled = true;
          GUILayout.EndHorizontal();
        }
      }
      catch (Exception ex)
      {
        if (!SMAddon.FrameErrTripped)
        {
          Utilities.LogMessage(
            string.Format(" in ResourceButtonList.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace),
            Utilities.LogType.Error, true);
          SMAddon.FrameErrTripped = true;
        }
      }
    }

    private static void ResourceButtonToggled(string resourceName)
    {
      try
      {
        if (SMConditions.IsTransferInProgress()) return;

        // First, lets clear any highlighting...
        SMHighlighter.ClearResourceHighlighting(SMAddon.SmVessel.SelectedResourcesParts);

        // Now let's update our lists...
        if (!SMAddon.SmVessel.SelectedResources.Contains(resourceName))
        {
          // now lets determine what to do with selection
          if (SMConditions.ResourceIsSingleton(resourceName))
          {
            SMAddon.SmVessel.SelectedResources.Clear();
            SMAddon.SmVessel.SelectedResources.Add(resourceName);
          }
          else
          {
            if (SMConditions.ResourcesContainSingleton(SMAddon.SmVessel.SelectedResources))
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
        ResolveResourcePartSelections(SMAddon.SmVessel.SelectedResources);

        // Now, based on the resourceselection, do we show the Transfer window?
        WindowTransfer.ShowWindow = SMAddon.SmVessel.SelectedResources.Count > 0;
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(
          string.Format(" in WindowManifest.ResourceButtonToggled.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace),
          Utilities.LogType.Error, true);
      }
    }

    private static void ResourceDetailsViewer()
    {
      try
      {
        _resourceScrollViewerPosition = GUILayout.BeginScrollView(_resourceScrollViewerPosition, SMStyle.ScrollStyle,
          GUILayout.Height(100), GUILayout.Width(300));
        GUILayout.BeginVertical();

        if (SMAddon.SmVessel.SelectedResources.Count > 0)
        {
          List<Part>.Enumerator pParts = SMAddon.SmVessel.SelectedResourcesParts.GetEnumerator();
          while (pParts.MoveNext())
          {
            if (pParts.Current == null) continue;
            Part part = pParts.Current;
            if (SMConditions.AreSelectedResourcesTypeOther(SMAddon.SmVessel.SelectedResources))
            {
              GUIStyle noWrap = SMStyle.LabelStyleNoWrap;
              GUILayout.Label(string.Format("{0}", part.partInfo.title), noWrap, GUILayout.Width(265),
                GUILayout.Height(18));
              GUIStyle noPad = SMStyle.LabelStyleNoPad;
              List<string>.Enumerator sResources = SMAddon.SmVessel.SelectedResources.GetEnumerator();
              while (sResources.MoveNext())
              {
                if (sResources.Current == null) continue;
                GUILayout.Label(
                  string.Format(" - {0}:  ({1}/{2})", sResources.Current, part.Resources[sResources.Current].amount.ToString("######0.####"),
                    part.Resources[sResources.Current].maxAmount.ToString("######0.####")), noPad, GUILayout.Width(265),
                  GUILayout.Height(16));
              }
            }
            else if (SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Crew.ToString()))
            {
              GUILayout.BeginHorizontal();
              GUILayout.Label(
                string.Format("{0}, ({1}/{2})", part.partInfo.title, Utilities.GetPartCrewCount(part), part.CrewCapacity),
                GUILayout.Width(265), GUILayout.Height(20));
              GUILayout.EndHorizontal();
            }
            else if (SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Science.ToString()))
            {
              int scienceCount = 0;
              IEnumerator pModules = part.Modules.GetEnumerator();
              while (pModules.MoveNext())
              {
                if (pModules.Current == null) continue;
                PartModule pm = (PartModule)pModules.Current;
                ModuleScienceContainer container = pm as ModuleScienceContainer;
                if (container != null)
                  scienceCount += container.GetScienceCount();
                else if (pm is ModuleScienceExperiment)
                  scienceCount += ((ModuleScienceExperiment) pm).GetScienceCount();
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
          Utilities.LogMessage(
            string.Format(" in WindowManifest.ResourceDetailsViewer.  Error:  {0} \r\n\r\n{1}", ex.Message,
              ex.StackTrace), Utilities.LogType.Error, true);
          SMAddon.FrameErrTripped = true;
        }
      }
      GUILayout.EndVertical();
      GUILayout.EndScrollView();
    }

    private static void WindowToggleButtons()
    {
      GUILayout.BeginHorizontal();

      GUIStyle settingsStyle = WindowSettings.ShowWindow ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
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
          Utilities.LogMessage(
            string.Format(" opening Settings Window.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), Utilities.LogType.Error,
            true);
        }
      }

      GUIStyle rosterStyle = WindowRoster.ShowWindow ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button("Roster", rosterStyle, GUILayout.Height(20)))
      {
        try
        {
          WindowRoster.ShowWindow = !WindowRoster.ShowWindow;
          if (WindowRoster.ShowWindow)
          {
            WindowRoster.GetRosterList();
          }
          else
          {
            WindowRoster.SelectedKerbal = null;
            WindowRoster.ToolTip = "";
          }
        }
        catch (Exception ex)
        {
          Utilities.LogMessage(
            string.Format(" opening Roster Window.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), Utilities.LogType.Error,
            true);
        }
      }

      GUIStyle controlStyle = WindowControl.ShowWindow ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button("Control", controlStyle, GUILayout.Height(20)))
      {
        try
        {
          WindowControl.ShowWindow = !WindowControl.ShowWindow;
        }
        catch (Exception ex)
        {
          Utilities.LogMessage(
            string.Format(" opening Control Window.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), Utilities.LogType.Error,
            true);
        }
      }
      GUILayout.EndHorizontal();
    }

    #endregion

    internal static void ResolveResourcePartSelections(List<string> resourceNames)
    {
      try
      {
        if (resourceNames.Count > 0)
        {
          List<Part> newSources = new List<Part>();
          List<Part> newTargets = new List<Part>();
          if (WindowTransfer.ShowSourceVessels &&
              SMConditions.AreSelectedResourcesTypeOther(SMAddon.SmVessel.SelectedResources))
          {
            SMAddon.SmVessel.SelectedPartsSource =
              SMAddon.SmVessel.GetSelectedVesselsParts(SMAddon.SmVessel.SelectedVesselsSource, resourceNames);
            if (!WindowTransfer.ShowTargetVessels)
            {
              List<Part>.Enumerator srcParts = SMAddon.SmVessel.SelectedPartsSource.GetEnumerator();
              while (srcParts.MoveNext())
              {
                if (srcParts.Current == null) continue;
                if (!SMAddon.SmVessel.SelectedPartsTarget.Contains(srcParts.Current)) continue;
                SMAddon.SmVessel.SelectedPartsTarget.Remove(srcParts.Current);
              }
            }
          }
          else
          {
            List<Part>.Enumerator parts = SMAddon.SmVessel.SelectedPartsSource.GetEnumerator();
            while (parts.MoveNext())
            {
              if (parts.Current == null) continue;
              if (resourceNames.Count > 1)
              {
                if (parts.Current.Resources.Contains(resourceNames[0]) && parts.Current.Resources.Contains(resourceNames[1]))
                  newSources.Add(parts.Current);
              }
              else
              {
                if (resourceNames[0] == SMConditions.ResourceType.Crew.ToString() && parts.Current.CrewCapacity > 0)
                  newSources.Add(parts.Current);
                else if (resourceNames[0] == SMConditions.ResourceType.Science.ToString() &&
                         parts.Current.FindModulesImplementing<IScienceDataContainer>().Count > 0)
                  newSources.Add(parts.Current);
                else if (parts.Current.Resources.Contains(resourceNames[0]))
                  newSources.Add(parts.Current);
              }
            }
            SMAddon.SmVessel.SelectedPartsSource.Clear();
            SMAddon.SmVessel.SelectedPartsSource = newSources;
          }

          if (WindowTransfer.ShowTargetVessels &&
              SMConditions.AreSelectedResourcesTypeOther(SMAddon.SmVessel.SelectedResources))
          {
            SMAddon.SmVessel.SelectedPartsTarget =
              SMAddon.SmVessel.GetSelectedVesselsParts(SMAddon.SmVessel.SelectedVesselsTarget, resourceNames);
            if (!WindowTransfer.ShowSourceVessels)
            {
              List<Part>.Enumerator tgtParts = SMAddon.SmVessel.SelectedPartsTarget.GetEnumerator();
              while (tgtParts.MoveNext())
              {
                if (tgtParts.Current == null) continue;
                if (!SMAddon.SmVessel.SelectedPartsSource.Contains(tgtParts.Current)) continue;
                SMAddon.SmVessel.SelectedPartsSource.Remove(tgtParts.Current);
              }
            }
          }
          else
          {
            List<Part>.Enumerator tgtParts = SMAddon.SmVessel.SelectedPartsTarget.GetEnumerator();
            while (tgtParts.MoveNext())
            {
              if (tgtParts.Current == null) continue;
              if (resourceNames.Count > 1)
              {
                if (tgtParts.Current.Resources.Contains(resourceNames[0]) && tgtParts.Current.Resources.Contains(resourceNames[1]))
                  newTargets.Add(tgtParts.Current);
              }
              else
              {
                if (resourceNames[0] == SMConditions.ResourceType.Crew.ToString() && tgtParts.Current.CrewCapacity > 0)
                  newTargets.Add(tgtParts.Current);
                else if (resourceNames[0] == SMConditions.ResourceType.Science.ToString() &&
                         tgtParts.Current.FindModulesImplementing<IScienceDataContainer>().Count > 0)
                  newTargets.Add(tgtParts.Current);
                else if (tgtParts.Current.Resources.Contains(resourceNames[0]))
                  newTargets.Add(tgtParts.Current);
              }
            }
            SMAddon.SmVessel.SelectedPartsTarget.Clear();
            SMAddon.SmVessel.SelectedPartsTarget = newTargets;
          }

          if (SMConditions.AreSelectedResourcesTypeOther(resourceNames))
          {
            TransferPump.CreateDisplayPumps();
            return;
          }

          SMAddon.SmVessel.SelectedVesselsSource.Clear();
          SMAddon.SmVessel.SelectedVesselsTarget.Clear();
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
        Utilities.LogMessage(
          string.Format(" in WindowManifest.ReconcileSelectedXferParts.  Error:  {0} \r\n\r\n{1}", ex.Message,
            ex.StackTrace), Utilities.LogType.Error, true);
      }
    }

  }
}