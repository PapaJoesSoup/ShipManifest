using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ShipManifest.InternalObjects;
using ShipManifest.InternalObjects.Settings;
using ShipManifest.Process;
using UnityEngine;

namespace ShipManifest.Windows
{
  internal static class WindowManifest
  {

    #region Properties

    internal static string Title = $"Ship Manifest - {SMSettings.CurVersion}"; // 
    internal static Rect Position = CurrSettings.DefaultPosition;
    internal static float HeightScale;
    internal static float ViewerHeight = 100;
    internal static float MinHeight = 50;
    internal static float WindowHeight = 282;
    internal static bool ResizingWindow = false;

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

    #region Localization strings

    internal static string titleContent           = SmUtils.SmTags["#smloc_manifest_002"];
    internal static GUIContent closeContent       = new GUIContent("", SmUtils.SmTags["#smloc_window_tt_001"]);
    internal static string noResSelContent        = SmUtils.SmTags["#smloc_manifest_003"];
    internal static string multiResSelContent     = SmUtils.SmTags["#smloc_manifest_004"];
    internal static string fillCrewContent        = SmUtils.SmTags["#smloc_manifest_005"];
    internal static string emptyCrewContent       = SmUtils.SmTags["#smloc_manifest_006"];
    internal static string fillResContent         = SmUtils.SmTags["#smloc_manifest_007"];
    internal static string emptyResContent        = SmUtils.SmTags["#smloc_manifest_008"];
    internal static GUIContent dumpResContent     = new GUIContent(SmUtils.SmTags["#smloc_manifest_009"], SmUtils.SmTags["#smloc_manifest_tt_001"]);
    internal static GUIContent stopDumpResContent = new GUIContent(SmUtils.SmTags["#smloc_manifest_010"], SmUtils.SmTags["#smloc_manifest_tt_002"]);
    internal static string fillContent            = SmUtils.SmTags["#smloc_manifest_011"];
    internal static string settingsContent        = SmUtils.SmTags["#smloc_manifest_012"];
    internal static string rosterContent          = SmUtils.SmTags["#smloc_manifest_013"];
    internal static string transferContent        = SmUtils.SmTags["#smloc_transfer_000"];
    internal static string controlContent         = SmUtils.SmTags["#smloc_manifest_014"];

    #endregion Localization strings

    #endregion Properties

    #region Manifest Window - Gui Layout Code

    // Ship Manifest Window
    // This window displays options for managing crew, resources, and flight checklists for the focused vessel.
    private static Vector2 _smScrollViewerPosition = Vector2.zero;
    private static Vector2 _resourceScrollViewerPosition = Vector2.zero;

    internal static void Display(int windowId)
    {
      Title = $"{titleContent} {SMSettings.CurVersion} - {SMAddon.SmVessel.Vessel.GetDisplayName()}";

      // set input locks when mouseover window...
      _inputLocked = GuiUtils.PreventClickthrough(ShowWindow, Position, _inputLocked);

      // Reset Tooltip active flag...
      ToolTipActive = false;

      // "Close Window");
      Rect rect = new Rect(Position.width - 20, 4, 16, 16);
      if (GUI.Button(rect, closeContent))
      {
        SMAddon.OnSmButtonClicked();
        ToolTip = "";
        SMHighlighter.Update_Highlighter();
      }
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);
      GUI.enabled = true;
      try
      {
        GUILayout.BeginVertical();
        _smScrollViewerPosition = GUILayout.BeginScrollView(_smScrollViewerPosition, SMStyle.ScrollStyle,
          GUILayout.Height(ViewerHeight + HeightScale), GUILayout.Width(300));
        GUILayout.BeginVertical();

        // Prelaunch (landed) Gui
        if (SMConditions.IsInPreflight())
        {
          PreLaunchGui();
        }

        // Now the Resource Buttons
        ResourceButtonsList();

        GUILayout.EndVertical();
        GUILayout.EndScrollView();

        //string resLabel = "No Resource Selected";
        string resLabel = noResSelContent;
        switch (SMAddon.SmVessel.SelectedResources.Count)
        {
          case 1:
            resLabel = SMAddon.SmVessel.SelectedResources[0];
            break;
          case 2:
            resLabel = multiResSelContent;
            break;
        }
        GUILayout.Label($"{resLabel}", GUILayout.Width(300), GUILayout.Height(20));

        // Resource Details List Viewer
        ResourceDetailsViewer();

        // Window toggle Button List
        WindowToggleButtons();

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
          if (Mouse.delta.y != 0)
          {
            float diff = Mouse.delta.y;
            GuiUtils.UpdateScale(diff, ViewerHeight, ref HeightScale, MinHeight);
          }
        }
        //ResetZoomKeys();
        GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
        Position.height = WindowHeight + HeightScale;
        GuiUtils.RepositionWindow(ref Position);
      }
      catch (Exception ex)
      {
        if (!SMAddon.FrameErrTripped)
        {
          SmUtils.LogMessage(
            $" in WindowManifest.Display.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}",
            SmUtils.LogType.Error, true);
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
        if (CurrSettings.EnablePfCrews)
        {
          GUILayout.BeginHorizontal();
          // Realism Mode is desirable, as there is a cost associated with a kerbal on a flight.   No cheating!
          if (GUILayout.Button(fillCrewContent, SMStyle.ButtonStyle, GUILayout.Width(134), GUILayout.Height(20))) // "Fill Crew"
          {
            SMAddon.SmVessel.FillCrew();
          }
          if (GUILayout.Button(emptyCrewContent, SMStyle.ButtonStyle, GUILayout.Width(134), GUILayout.Height(20))) // "Empty Crew"
          {
            SMAddon.SmVessel.EmptyCrew();
          }
          GUILayout.EndHorizontal();
        }

        if (!CurrSettings.EnablePfResources) return;
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(fillResContent, SMStyle.ButtonStyle, GUILayout.Width(134), GUILayout.Height(20))) // "Fill Resources"
        {
          SMAddon.SmVessel.FillResources();
        }
        if (GUILayout.Button(emptyResContent, SMStyle.ButtonStyle, GUILayout.Width(134), GUILayout.Height(20))) // "Empty Resources"
        {
          SMAddon.SmVessel.DumpAllResources();
        }
        GUILayout.EndHorizontal();
      }
      catch (Exception ex)
      {
        if (!SMAddon.FrameErrTripped)
        {
          SmUtils.LogMessage(
            $" in PreLaunchGui.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}",
            SmUtils.LogType.Error, true);
          SMAddon.FrameErrTripped = true;
        }
      }
    }

    private static void ResourceButtonsList()
    {
      try
      {
        // List required here to prevent loop sync errors with live source.
        List<string>.Enumerator keys = SMAddon.SmVessel.PartsByResource.Keys.ToList().GetEnumerator();
        while (keys.MoveNext())
        {
          if (string.IsNullOrEmpty(keys.Current)) continue;
          GUILayout.BeginHorizontal();

          // Button Widths
          int width = 273;
          if (!CurrSettings.RealXfers && SMConditions.IsResourceTypeOther(keys.Current)) width = 185;
          else if (SMConditions.IsResourceTypeOther(keys.Current)) width = 223;

          // Resource Button
          string displayAmounts = $"{keys.Current}{SmUtils.DisplayVesselResourceTotals(keys.Current)}";
          GUIStyle style = SMAddon.SmVessel.SelectedResources.Contains(keys.Current)
            ? SMStyle.ButtonToggledStyle
            : SMStyle.ButtonStyle;
          if (GUILayout.Button(displayAmounts, style, GUILayout.Width(width), GUILayout.Height(20)))
          {
            ResourceButtonToggled(keys.Current);
            SMHighlighter.Update_Highlighter();
          }

          // Dump Button
          if (SMConditions.IsResourceTypeOther(keys.Current) && SMAddon.SmVessel.PartsByResource[keys.Current].Count > 0)
          {
            uint pumpId = TransferPump.GetPumpIdFromHash(keys.Current,
              SMAddon.SmVessel.PartsByResource[keys.Current].First(),
              SMAddon.SmVessel.PartsByResource[keys.Current].Last(), TransferPump.TypeXfer.Dump,
              TransferPump.TriggerButton.Manifest);
            GUIContent dumpContent = !TransferPump.IsPumpInProgress(pumpId) ? dumpResContent : stopDumpResContent;
            GUI.enabled = SMConditions.CanResourceBeDumped(keys.Current);
            if (GUILayout.Button(dumpContent, SMStyle.ButtonStyle, GUILayout.Width(45), GUILayout.Height(20)))
            {
              SMVessel.ToggleDumpResource(keys.Current, pumpId);
            }
          }

          // Fill Button
          if (!CurrSettings.RealXfers && SMConditions.IsResourceTypeOther(keys.Current) &&
              SMAddon.SmVessel.PartsByResource[keys.Current].Count > 0)
          {
            GUI.enabled = SMConditions.CanResourceBeFilled(keys.Current);
            if (GUILayout.Button($"{fillContent}", SMStyle.ButtonStyle, GUILayout.Width(35),
              GUILayout.Height(20))) // "Fill"
            {
              SMAddon.SmVessel.FillResource(keys.Current);
            }
          }
          GUI.enabled = true;
          GUILayout.EndHorizontal();
        }
        keys.Dispose();
      }
      catch (Exception ex)
      {
        if (!SMAddon.FrameErrTripped)
        {
          SmUtils.LogMessage(
            $" in WindowManifest.ResourceButtonList.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}",
            SmUtils.LogType.Error, true);
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
        SMAddon.SmVessel.RefreshLists();
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
        if (WindowTransfer.ShowWindow) WindowTransfer.DisplayMode = SMConditions.GetTransferMode();
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage(
          $" in WindowManifest.ResourceButtonToggled.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}",
          SmUtils.LogType.Error, true); // in, Error
      }
      SMAddon.SmVessel.RefreshLists();
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
              GUILayout.Label($"{part.partInfo.title}", noWrap, GUILayout.Width(265),
                GUILayout.Height(18));
              GUIStyle noPad = SMStyle.LabelStyleNoPad;
              List<string>.Enumerator sResources = SMAddon.SmVessel.SelectedResources.GetEnumerator();
              while (sResources.MoveNext())
              {
                if (sResources.Current == null) continue;
                GUILayout.Label(
                  $" - {sResources.Current}:  ({part.Resources[sResources.Current].amount:######0.####}/{part.Resources[sResources.Current].maxAmount:######0.####})", noPad, GUILayout.Width(265),
                  GUILayout.Height(16));
              }
              sResources.Dispose();
            }
            else if (SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Crew.ToString()))
            {
              GUILayout.BeginHorizontal();
              GUILayout.Label(
                $"{part.partInfo.title}, ({SmUtils.GetPartCrewCount(part)}/{part.CrewCapacity})",
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
                ModuleScienceContainer container = (PartModule)pModules.Current as ModuleScienceContainer;
                ModuleScienceExperiment experiment = (PartModule)pModules.Current as ModuleScienceExperiment;
                if (container != null)
                  scienceCount += container.GetScienceCount();
                else if (experiment != null)
                  scienceCount += experiment.GetScienceCount();
              }
              GUILayout.BeginHorizontal();
              GUILayout.Label($"{part.partInfo.title}, ({scienceCount})", GUILayout.Width(265));
              GUILayout.EndHorizontal();
            }
          }
          pParts.Dispose();
        }
      }
      catch (Exception ex)
      {
        if (!SMAddon.FrameErrTripped)
        {
          SmUtils.LogMessage(
            $" in WindowManifest.ResourceDetailsViewer.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}",
            SmUtils.LogType.Error, true); // in, Error
          SMAddon.FrameErrTripped = true;
        }
      }
      GUILayout.EndVertical();
      GUILayout.EndScrollView();
    }

    private static void WindowToggleButtons()
    {
      GUILayout.BeginHorizontal();

      GUI.enabled = SMConditions.CanShowWindowTransfer(); 
      GUIStyle transferStyle = WindowTransfer.ShowWindow ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button(transferContent, transferStyle, GUILayout.Height(20))) // "Transfer"
      {
        try
        {
          if (WindowTransfer.ShowWindow)
          {
            WindowTransfer.BtnCloseWindow();
          }
          else
          {
            WindowTransfer.ShowWindow = true;
          }
        }
        catch (Exception ex)
        {
          SmUtils.LogMessage(
            $" opening Roster Window.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}", SmUtils.LogType.Error,
            true);
        }
      }
      GUI.enabled = true;
      GUIStyle settingsStyle = WindowSettings.ShowWindow ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button(settingsContent, settingsStyle, GUILayout.Height(20))) // "Settings"
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
          SmUtils.LogMessage(
            $" opening Settings Window.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}", SmUtils.LogType.Error,
            true);
        }
      }

      GUIStyle rosterStyle = WindowRoster.ShowWindow ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button(rosterContent, rosterStyle, GUILayout.Height(20))) // "Roster"
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
          SmUtils.LogMessage(
            $" opening Roster Window.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}", SmUtils.LogType.Error,
            true);
        }
      }

      GUIStyle controlStyle = WindowControl.ShowWindow ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
      if (GUILayout.Button(controlContent, controlStyle, GUILayout.Height(20))) // "Control"
      {
        try
        {
          WindowControl.ShowWindow = !WindowControl.ShowWindow;
        }
        catch (Exception ex)
        {
          SmUtils.LogMessage(
            $" opening Control Window.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}", SmUtils.LogType.Error,
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
          List<Part> newSourceParts = new List<Part>();
          List<Part> newTargetParts = new List<Part>();
          if (WindowTransfer.ShowSourceVessels)
          {
              SMAddon.SmVessel.GetVesselsPartsByResource(SMAddon.SmVessel.SelectedVesselsSource, resourceNames);
            if (!WindowTransfer.ShowTargetVessels)
            {
              List<Part>.Enumerator srcParts = SMAddon.SmVessel.SelectedPartsSource.GetEnumerator();
              while (srcParts.MoveNext())
              {
                if (srcParts.Current == null) continue;
                if (!SMAddon.SmVessel.SelectedPartsTarget.Contains(srcParts.Current)) continue;
                SMAddon.SmVessel.SelectedPartsTarget.Remove(srcParts.Current);
              }
              srcParts.Dispose();
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
                  newSourceParts.Add(parts.Current);
              }
              else
              {
                if (resourceNames.Contains(SMConditions.ResourceType.Crew.ToString()) && parts.Current.CrewCapacity > 0)
                  newSourceParts.Add(parts.Current);
                else if (resourceNames.Contains(SMConditions.ResourceType.Science.ToString()) 
                  && parts.Current.FindModulesImplementing<IScienceDataContainer>().Count > 0)
                  newSourceParts.Add(parts.Current);
                else if (parts.Current.Resources.Contains(resourceNames[0]))
                  newSourceParts.Add(parts.Current);
              }
            }
            parts.Dispose();
            SMAddon.SmVessel.SelectedPartsSource.Clear();
            SMAddon.SmVessel.SelectedPartsSource = newSourceParts;
          }

          if (WindowTransfer.ShowTargetVessels)
          {
            SMAddon.SmVessel.SelectedPartsTarget =
              SMAddon.SmVessel.GetVesselsPartsByResource(SMAddon.SmVessel.SelectedVesselsTarget, resourceNames);
            if (!WindowTransfer.ShowSourceVessels)
            {
              List<Part>.Enumerator tgtParts = SMAddon.SmVessel.SelectedPartsTarget.GetEnumerator();
              while (tgtParts.MoveNext())
              {
                if (tgtParts.Current == null) continue;
                if (!SMAddon.SmVessel.SelectedPartsSource.Contains(tgtParts.Current)) continue;
                SMAddon.SmVessel.SelectedPartsSource.Remove(tgtParts.Current);
              }
              tgtParts.Dispose();
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
                  newTargetParts.Add(tgtParts.Current);
              }
              else
              {
                if (resourceNames.Contains(SMConditions.ResourceType.Crew.ToString()) && tgtParts.Current.CrewCapacity > 0)
                  newTargetParts.Add(tgtParts.Current);
                else if (resourceNames.Contains(SMConditions.ResourceType.Science.ToString()) &&
                         tgtParts.Current.FindModulesImplementing<IScienceDataContainer>().Count > 0)
                  newTargetParts.Add(tgtParts.Current);
                else if (tgtParts.Current.Resources.Contains(resourceNames[0]))
                  newTargetParts.Add(tgtParts.Current);
              }
            }
            tgtParts.Dispose();
            SMAddon.SmVessel.SelectedPartsTarget.Clear();
            SMAddon.SmVessel.SelectedPartsTarget = newTargetParts;
          }

          if (!SMConditions.AreSelectedResourcesTypeOther(resourceNames)) return;
          TransferPump.CreateDisplayPumps();
          return;
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
        SmUtils.LogMessage(
          $" in WindowManifest.ReconcileSelectedXferParts.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}",
            SmUtils.LogType.Error, true); // in, Error
      }
    }

  }
}
