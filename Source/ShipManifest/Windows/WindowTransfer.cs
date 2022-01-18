using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ShipManifest.APIClients;
using ShipManifest.InternalObjects;
using ShipManifest.InternalObjects.Settings;
using ShipManifest.Modules;
using ShipManifest.Process;
using ShipManifest.Windows.Popups;
using UnityEngine;
using VehiclePhysics;

namespace ShipManifest.Windows
{
  internal static class WindowTransfer
  {
    #region Properties

    internal static string Title = "";
    internal static Rect Position = CurrSettings.DefaultPosition;
    internal static float HeightScale;
    internal static float ViewerHeight = 100;
    internal static float MinHeight = 50;
    internal static float WindowHeight = 279;
    internal static bool ResizingWindow = false;
    internal static Rect SelectBox = new Rect(0, 0, 300, ViewerHeight);
    internal static Rect DetailsBox = new Rect(0,0, 300, 120);

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
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;

    internal static string ToolTip = "";
    internal static string XferToolTip = ""; // value filled by SMConditions.CanCrewBeXferred
    internal static string EvaToolTip = ""; // value filled by SMConditions.CanCrewBeXferred

    // Switches for List Viewers
    internal static bool ShowSourceVessels;
    internal static bool ShowTargetVessels;

    // vessel mode crew selection vars.
    internal static bool SelectAllFrom;
    internal static bool SelectAllTo;
    internal static bool TouristsOnlyFrom;
    internal static bool TouristsOnlyTo;

    // Display mode
    internal static SMConditions.TransferMode DisplayMode;

    // this list is for display use.  Transfers are executed against a separate list.  
    // These objects may be used to derive objects to be added to the transfer process queue.
    internal static List<TransferPump> DisplayPumps = new List<TransferPump>();

    #region Localization Strings

    // Localization strings
    internal static string titleContent           = SmUtils.SmTags["#smloc_transfer_000"];
    internal static GUIContent closeContent       = new GUIContent("", SmUtils.SmTags["#smloc_window_tt_001"]);
    internal static GUIContent noCloseContent     = new GUIContent("", SmUtils.SmTags["#smloc_window_tt_002"]);
    internal static string noPartContent          = $"{SmUtils.SmTags["#smloc_transfer_002"]}";
    internal static string multiPartContent       = $"{SmUtils.SmTags["#smloc_transfer_001"]}";
    internal static string vesselsContent         = SmUtils.SmTags["#smloc_transfer_003"];
    internal static GUIContent dumpPContent       = new GUIContent(SmUtils.SmTags["#smloc_transfer_004"], SmUtils.SmTags["#smloc_transfer_tt_004"]);
    internal static GUIContent stopPContent       = new GUIContent(SmUtils.SmTags["#smloc_transfer_005"], SmUtils.SmTags["#smloc_transfer_tt_005"]);
    internal static GUIContent fillVContent       = new GUIContent(SmUtils.SmTags["#smloc_transfer_006"], SmUtils.SmTags["#smloc_transfer_tt_006"]);
    internal static GUIContent dumpVContent       = new GUIContent(SmUtils.SmTags["#smloc_transfer_004"], SmUtils.SmTags["#smloc_transfer_tt_001"]);
    internal static GUIContent stopVContent       = new GUIContent(SmUtils.SmTags["#smloc_transfer_005"], SmUtils.SmTags["#smloc_transfer_tt_002"]);
    internal static GUIContent fillPContent       = new GUIContent(SmUtils.SmTags["#smloc_transfer_006"], SmUtils.SmTags["#smloc_transfer_tt_003"]);
    internal static GUIContent dumpCrewContent    = new GUIContent(SmUtils.SmTags["#smloc_transfer_004"], SmUtils.SmTags["#smloc_transfer_tt_007"]);
    internal static GUIContent fillCrewContent    = new GUIContent(SmUtils.SmTags["#smloc_transfer_006"], SmUtils.SmTags["#smloc_transfer_tt_008"]);
    internal static GUIContent moveKerbalContent  = new GUIContent("»", SmUtils.SmTags["#smloc_transfer_tt_009"]);
    internal static string movingContent          = SmUtils.SmTags["#smloc_transfer_007"];
    internal static string xferCrewContent        = SmUtils.SmTags["#smloc_transfer_021"];
    internal static GUIContent thawContent        = new GUIContent(SmUtils.SmTags["#smloc_transfer_010"], SmUtils.SmTags["#smloc_transfer_tt_010"]);
    internal static GUIContent selectAllContent   = new GUIContent(SmUtils.SmTags["#smloc_transfer_020"], SmUtils.SmTags["#smloc_transfer_tt_033"]);
    internal static GUIContent selectAllxContent  = new GUIContent(SmUtils.SmTags["#smloc_transfer_020"], SmUtils.SmTags["#smloc_transfer_tt_034"]);
    internal static string fillCrewTtContent      = SmUtils.SmTags["#smloc_conditions_tt_008"];
    internal static string moveCrewTtContent      = SmUtils.SmTags["#smloc_conditions_tt_009"];
    internal static GUIContent touristOnlyContent = new GUIContent(SmUtils.SmTags["#smloc_transfer_022"], SmUtils.SmTags["#smloc_transfer_tt_035"]);
    internal static string evaContent             = SmUtils.SmTags["#smloc_transfer_008"];
    internal static string xferContent            = SmUtils.SmTags["#smloc_transfer_009"];
    internal static string expScienceContent      = SmUtils.SmTags["#smloc_transfer_tt_011"];
    internal static string disabledXferContent    = SmUtils.SmTags["#smloc_transfer_tt_012"];
    internal static string noXferRealismContent   = SmUtils.SmTags["#smloc_transfer_tt_013"];
    internal static string xferOnlyProcSciContent = SmUtils.SmTags["#smloc_transfer_tt_014"];
    internal static string moveKerbalTtContent    = SmUtils.SmTags["#smloc_transfer_tt_009"];
    internal static GUIContent xferProcContent    = new GUIContent(SmUtils.SmTags["#smloc_transfer_011"], SmUtils.SmTags["#smloc_transfer_tt_014"]);
    internal static GUIContent xferUnprocContent  = new GUIContent(SmUtils.SmTags["#smloc_transfer_012"], SmUtils.SmTags["#smloc_transfer_tt_015"]);
    internal static string resultsTtContent       = SmUtils.SmTags["#smloc_transfer_tt_016"];
    internal static string dataAmtTtContent       = SmUtils.SmTags["#smloc_transfer_tt_017"];
    internal static string mitsTtContent          = SmUtils.SmTags["#smloc_transfer_tt_018"];
    internal static string xmitValTtContent       = SmUtils.SmTags["#smloc_transfer_tt_019"];
    internal static string labValTtContent        = SmUtils.SmTags["#smloc_transfer_tt_020"];
    internal static string xmitBonusTtContent     = SmUtils.SmTags["#smloc_transfer_tt_021"];
    internal static string noXferSciRlsmTtContent = SmUtils.SmTags["#smloc_transfer_tt_022"];
    internal static string xferSciTtContent       = SmUtils.SmTags["#smloc_transfer_tt_023"];
    internal static GUIContent recvSciContent     = new GUIContent(SmUtils.SmTags["#smloc_transfer_013"], SmUtils.SmTags["#smloc_transfer_tt_024"]);
    internal static string xferRemainContent      = SmUtils.SmTags["#smloc_transfer_014"];
    internal static string xferAmtContent         = SmUtils.SmTags["#smloc_transfer_015"];
    internal static string xferAmtTtBothContent   = SmUtils.SmTags["#smloc_transfer_tt_025"];
    internal static string xferAmtTtContent       = SmUtils.SmTags["#smloc_transfer_tt_026"];
    internal static string resultsContent         = SmUtils.SmTags["#smloc_transfer_016"];
    internal static string smallXferAmtTtContent  = SmUtils.SmTags["#smloc_transfer_tt_027"];
    internal static string noteRatioTtContent     = SmUtils.SmTags["#smloc_transfer_tt_028"];
    internal static GUIContent xferSliderContent  = new GUIContent(SmUtils.SmTags["#smloc_transfer_009"], SmUtils.SmTags["#smloc_transfer_tt_029"]);
    internal static GUIContent xferStartContent   = new GUIContent(SmUtils.SmTags["#smloc_transfer_009"], SmUtils.SmTags["#smloc_transfer_tt_030"]);
    internal static GUIContent xferStopContent    = new GUIContent(SmUtils.SmTags["#smloc_transfer_005"], SmUtils.SmTags["#smloc_transfer_tt_031"]);
    internal static string onContent              = SmUtils.SmTags["#smloc_transfer_017"];
    internal static string offContent             = SmUtils.SmTags["#smloc_transfer_018"];
    internal static GUIContent flowContent        = new GUIContent(SmUtils.SmTags["#smloc_transfer_019"], SmUtils.SmTags["#smloc_transfer_tt_032"]);

    #endregion Localization Strings

    private static Dictionary<PartModule, bool> _scienceModulesSource;
    internal static Dictionary<PartModule, bool> ScienceModulesSource
    {
      get
      {
        if (_scienceModulesSource != null) return _scienceModulesSource;
        _scienceModulesSource = new Dictionary<PartModule, bool>();
        if (SMAddon.SmVessel.SelectedPartsSource.Count <= 0) return _scienceModulesSource;
        List<Part>.Enumerator part = SMAddon.SmVessel.SelectedPartsSource.GetEnumerator();
        while (part.MoveNext())
        {
          if (part.Current == null) continue;
          List<IScienceDataContainer>.Enumerator module = part.Current.FindModulesImplementing<IScienceDataContainer>().GetEnumerator();
          while (module.MoveNext())
          {
            if (module.Current == null) continue;
            PartModule pm = (PartModule) module.Current;
            _scienceModulesSource.Add(pm, false);
          }
          module.Dispose();
        }
        part.Dispose();
        return _scienceModulesSource;
      }
    }

    private static Dictionary<PartModule, bool> _scienceModulesTarget;
    internal static Dictionary<PartModule, bool> ScienceModulesTarget
    {
      get
      {
        if (_scienceModulesTarget != null) return _scienceModulesTarget;
        _scienceModulesTarget = new Dictionary<PartModule, bool>();
        if (SMAddon.SmVessel.SelectedPartsSource.Count <= 0) return _scienceModulesTarget;
        List<Part>.Enumerator part = SMAddon.SmVessel.SelectedPartsSource.GetEnumerator();
        while (part.MoveNext())
        {
          if (part.Current == null) continue;
          List<IScienceDataContainer>.Enumerator module = part.Current.FindModulesImplementing<IScienceDataContainer>().GetEnumerator();
          while (module.MoveNext())
          {
            if (module.Current == null) continue;
            PartModule pm = (PartModule)module.Current;
            _scienceModulesTarget.Add(pm, false);
          }
          module.Dispose();
        }
        part.Dispose();
        return _scienceModulesTarget;
      }
    }

    #endregion Properties
    
    #region TransferWindow (GUI Layout)

    // Resource Transfer Window
    // This window allows you some control over the selected resource on a selected source and target part
    // This window assumes that a resource has been selected on the Ship manifest window.
    internal static void Display(int _windowId)
    {
      GUI.enabled = !SMConditions.IsTransferInProgress() || !PopupCloseTransfer.ShowWindow;

      // set input locks when mouseover window...
      _inputLocked = GuiUtils.PreventClickthrough(ShowWindow, Position, _inputLocked);

      string displayAmounts = SmUtils.DisplayVesselResourceTotals(SMAddon.SmVessel.SelectedResources[0]);
      Title = $"{titleContent} - {SMAddon.SmVessel.Vessel.vesselName}{displayAmounts}"; // "Transfer"

      // Reset Tooltip active flag...
      ToolTipActive = false;
      SMHighlighter.IsMouseOver = false;

      //"Close Window");
      Rect rect = new Rect(Position.width - 20, 4, 16, 16);
      if (GUI.Button(rect, SMConditions.IsTransferInProgress() ? noCloseContent : closeContent))
      {
        if (BtnCloseWindow()) return;
      }
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      try
      {
        // This window assumes that a resource has been selected on the Ship manifest window.
        GUILayout.BeginHorizontal();
        //Left Column Begins
        GUILayout.BeginVertical();

        // Build source Transfer Viewer
        SourceTransferViewer();

        // Text above Source Details. (Between viewers)
        TextBetweenViewers(SMAddon.SmVessel.SelectedPartsSource, TransferPump.TypeXfer.SourceToTarget);

        // Build Details ScrollViewer
        SourceDetailsViewer();

        // Okay, we are done with the left column of the dialog...
        GUILayout.EndVertical();

        // Right Column Begins...
        GUILayout.BeginVertical();

        // Build Target Transfer Viewer
        TargetTransferViewer();

        // Text between viewers
        TextBetweenViewers(SMAddon.SmVessel.SelectedPartsTarget, TransferPump.TypeXfer.TargetToSource);

        // Build Target details Viewer
        TargetDetailsViewer();

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        // Display MouseOverHighlighting, if any
        SMHighlighter.MouseOverHighlight();

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

        GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
        Position.height = WindowHeight + HeightScale;
        GuiUtils.RepositionWindow(ref Position);
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage(
          $" in Ship Manifest Window.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}", SmUtils.LogType.Error, true);
      }
    }

    internal static bool BtnCloseWindow()
    {
      if (SMConditions.IsTransferInProgress() && !PopupCloseTransfer.ShowWindow)
      {
        ToolTip = "";
        GUI.enabled = false;
        TransferPump.Paused = true;
        SMSound.SourcePumpRun.Stop();
        PopupCloseTransfer.Position = new Rect(Position.x + 100, Position.y + 50, 0, 0);
        PopupCloseTransfer.ShowWindow = true;
      }
      else
      {
        ShowWindow = false;
        PopupCloseTransfer.ShowWindow = false;
        SMAddon.SmVessel.SelectedPartsSource.Clear();
        SMAddon.SmVessel.SelectedPartsTarget.Clear();
        SMAddon.SmVessel.SelectedVesselsSource.Clear();
        SMAddon.SmVessel.SelectedVesselsTarget.Clear();
        ToolTip = "";
        SMHighlighter.Update_Highlighter();
        return true;
      }

      return false;
    }

    #region Viewer Selections (Top Half)

    #region Source Viewers (GUI Layout)

    // Transfer Window components
    private static Vector2 _sourceTransferViewerScrollPosition = Vector2.zero;
    internal static void SourceTransferViewer()
    {
      try
      {
        // This is a scroll panel (we are using it to make button lists...)
        _sourceTransferViewerScrollPosition = GUILayout.BeginScrollView(_sourceTransferViewerScrollPosition,
          SMStyle.ScrollStyle, GUILayout.Height(SelectBox.height + HeightScale), GUILayout.Width(SelectBox.width));
        GUILayout.BeginVertical();

        if (ShowSourceVessels)
          VesselTransferViewer(SMAddon.SmVessel.SelectedResources, TransferPump.TypeXfer.SourceToTarget,
            _sourceTransferViewerScrollPosition);
        else
          PartsTransferViewer(SMAddon.SmVessel.SelectedResources, TransferPump.TypeXfer.SourceToTarget,
            _sourceTransferViewerScrollPosition);

        GUILayout.EndVertical();
        GUILayout.EndScrollView();
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage(
          $" in Ship Manifest Window - SourceTransferViewer.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}", SmUtils.LogType.Error, true);
      }
    }

    private static Vector2 _sourceDetailsViewerScrollPosition = Vector2.zero;
    private static void SourceDetailsViewer()
    {
      try
      {
        // Source Part resource Details
        // this Scroll viewer is for the details of the part selected above.
        _sourceDetailsViewerScrollPosition = GUILayout.BeginScrollView(_sourceDetailsViewerScrollPosition,
          SMStyle.ScrollStyle, GUILayout.Height(DetailsBox.height), GUILayout.Width(DetailsBox.width));
        GUILayout.BeginVertical();

        if (SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Crew.ToString()))
        {
          CrewDetails(SMAddon.SmVessel.SourceMembersSelected, SMAddon.SmVessel.SelectedPartsSource, SMAddon.SmVessel.SelectedPartsTarget, ShowSourceVessels, true);
        }
        else if (SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Science.ToString()))
        {
          ScienceDetailsSource(ShowSourceVessels);
        }
        else
        {
          // Other resources are left....
          ResourceDetailsViewer(TransferPump.TypeXfer.SourceToTarget);
        }
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage(
          $" in WindowTransfer.SourceDetailsViewer.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}",
          SmUtils.LogType.Error, true);
      }
    }

    #endregion

    #region Target Viewers (GUI Layout)

    private static Vector2 _targetTransferViewerScrollPosition = Vector2.zero;
    private static void TargetTransferViewer()
    {
      try
      {
        // Adjust target style colors for part selectors when using/not using CLS highlighting
        if (CurrSettings.EnableClsHighlighting &&
            SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Crew.ToString()))
          SMStyle.ButtonToggledTargetStyle.normal.textColor = SMSettings.Colors[CurrSettings.TargetPartCrewColor];
        else
          SMStyle.ButtonToggledTargetStyle.normal.textColor = SMSettings.Colors[CurrSettings.TargetPartColor];

        // This is a scroll panel (we are using it to make button lists...)
        _targetTransferViewerScrollPosition = GUILayout.BeginScrollView(_targetTransferViewerScrollPosition,
          SMStyle.ScrollStyle, GUILayout.Height(SelectBox.height + HeightScale), GUILayout.Width(SelectBox.width));
        GUILayout.BeginVertical();

        if (ShowTargetVessels)
          VesselTransferViewer(SMAddon.SmVessel.SelectedResources, TransferPump.TypeXfer.TargetToSource,
            _targetTransferViewerScrollPosition);
        else
          PartsTransferViewer(SMAddon.SmVessel.SelectedResources, TransferPump.TypeXfer.TargetToSource,
            _targetTransferViewerScrollPosition);

        GUILayout.EndVertical();
        GUILayout.EndScrollView();
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage(
          $" in Ship Manifest Window - TargetTransferViewer.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}", SmUtils.LogType.Error, true);
      }
    }

    private static Vector2 _targetDetailsViewerScrollPosition = Vector2.zero;
    private static void TargetDetailsViewer()
    {
      try
      {
        // Target Part resource details
        _targetDetailsViewerScrollPosition = GUILayout.BeginScrollView(_targetDetailsViewerScrollPosition,
          SMStyle.ScrollStyle, GUILayout.Height(DetailsBox.height), GUILayout.Width(DetailsBox.width));
        GUILayout.BeginVertical();

        // --------------------------------------------------------------------------
        if (SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Crew.ToString()))
        {
          CrewDetails(SMAddon.SmVessel.TargetMembersSelected, SMAddon.SmVessel.SelectedPartsTarget, SMAddon.SmVessel.SelectedPartsSource, ShowTargetVessels, false);
        }
        else if (SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Science.ToString()))
        {
          ScienceDetailsTarget();
        }
        else
        {
          ResourceDetailsViewer(TransferPump.TypeXfer.TargetToSource);
        }
        // --------------------------------------------------------------------------
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage(
          $" in WindowTransfer.TargetDetailsViewer.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}",
          SmUtils.LogType.Error, true);
      }
    }

    #endregion

    #endregion Viewer Selections (Top Half)

    private static void TextBetweenViewers(IList<Part> selectedParts, TransferPump.TypeXfer xferType)
    {
      GUI.enabled = true;
      const float textWidth = 220;
      const float toggleWidth = 65; 
      string labelText = "";

      GUILayout.BeginHorizontal();
      if (SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Crew.ToString()))
        labelText = selectedParts.Count > 0 ? $"{selectedParts[0].partInfo.title}" : noPartContent;
      else
      {
        if (selectedParts != null)
        {
          if (selectedParts.Count > 1)
            labelText = multiPartContent; // "Multiple Parts Selected");
          else if (selectedParts.Count == 1)
            labelText = $"{selectedParts[0].partInfo.title}";
          else
            labelText = noPartContent; // "No Part Selected");
        }
      }
      GUILayout.Label(labelText, SMStyle.LabelStyleNoWrap, GUILayout.Width(textWidth));
      if (SMAddon.SmVessel.ModDockedVessels.Count > 0 && !SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Science.ToString()))
      {
        if (xferType == TransferPump.TypeXfer.SourceToTarget)
        {
          bool prevValue = ShowSourceVessels;
          ShowSourceVessels = GUILayout.Toggle(ShowSourceVessels, vesselsContent, GUILayout.Width(toggleWidth)); // "Vessels"
          if (!prevValue && ShowSourceVessels)
            WindowManifest.ResolveResourcePartSelections(SMAddon.SmVessel.SelectedResources);
        }
        else
        {
          if (SMAddon.SmVessel.ModDockedVessels.Count > 0)
          {
            bool prevValue = ShowSourceVessels;
            ShowTargetVessels = GUILayout.Toggle(ShowTargetVessels, vesselsContent, GUILayout.Width(toggleWidth)); // "Vessels"
            if (!prevValue && ShowSourceVessels)
              WindowManifest.ResolveResourcePartSelections(SMAddon.SmVessel.SelectedResources);
          }
        }
      }
      GUILayout.EndHorizontal();
    }

    #region Viewer Details (Bottom Half)

    #region Part/Vessel Buttons Viewer
    private static void PartsTransferViewer(List<string> selectedResources, TransferPump.TypeXfer xferType,
      Vector2 viewerScrollPosition)
    {
      //float scrollX = Position.x + (pumpType == TransferPump.TypePump.SourceToTarget ? 20 : 320);
      //float scrollY = Position.y + 30 - viewerScrollPosition.y;
      float scrollX = (xferType == TransferPump.TypeXfer.SourceToTarget ? 20 : 320);
      float scrollY = viewerScrollPosition.y;
      string step = "begin";
      try
      {
        step = "begin button loop";
        List<Part>.Enumerator parts = SMAddon.SmVessel.SelectedResourcesParts.GetEnumerator();
        while (parts.MoveNext())
        {
          if (parts.Current == null) continue;
          // Build the part button title...
          step = "part button title";
          string strDescription = GetResourceDescription(selectedResources, parts.Current);

          // set the conditions for a button style change.
          int btnWidth = 273; // Start with full width button...
          if (SMConditions.AreSelectedResourcesTypeOther(selectedResources))
            btnWidth = !CurrSettings.RealXfers || (CurrSettings.EnablePfResources && SMConditions.IsInPreflight()) ? 173 : 223;
          else if (selectedResources.Contains(SMConditions.ResourceType.Crew.ToString()) && SMConditions.CanShowCrewFillDumpButtons())
            btnWidth = 173;

          // Set style based on viewer and toggled state.
          step = "Set style";
          GUIStyle style = GetPartButtonStyle(xferType, parts.Current);

          GUILayout.BeginHorizontal();

          // Now let's account for any target buttons already pressed. (sources and targets for resources cannot be the same)
          GUI.enabled = IsPartSelectable(selectedResources[0], xferType, parts.Current);

          step = "Render part Buttons";
          if (GUILayout.Button(strDescription, style, GUILayout.Width(btnWidth), GUILayout.Height(20)))
          {
            PartButtonToggled(xferType, parts.Current);
            SMHighlighter.Update_Highlighter();
          }
          Rect rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && rect.Contains(Event.current.mousePosition))
            SMHighlighter.SetMouseOverData(rect, scrollY, scrollX, SelectBox.height, parts.Current, Event.current.mousePosition);

          // Reset Button enabling.
          GUI.enabled = true;

          step = "Render dump/fill buttons";
          if (selectedResources.Contains(SMConditions.ResourceType.Crew.ToString()))
          {
            if (SMConditions.CanShowCrewFillDumpButtons())
            CrewFillDumpPartButtons(parts.Current);
          }
          if (SMConditions.AreSelectedResourcesTypeOther(selectedResources))
          {
            ResourceDumpFillButtons(selectedResources, xferType, parts.Current);
          }
          GUI.enabled = true;
          GUILayout.EndHorizontal();
        }
        parts.Dispose();
      }
      catch (Exception ex)
      {
        if (!SMAddon.FrameErrTripped)
        {
          SmUtils.LogMessage($"Error in Windowtransfer.PartsTransferViewer ({xferType}) at step:  {step}.  Error:  {ex}",
            SmUtils.LogType.Error, true);
          SMAddon.FrameErrTripped = true;
        }
      }
    }

    private static void VesselTransferViewer(List<string> selectedResources, TransferPump.TypeXfer xferType,
      Vector2 viewerScrollPosition)
    {
      //float scrollX = Position.x + (pumpType == TransferPump.TypePump.SourceToTarget ? 20 : 320);
      //float scrollY = Position.y + 30 - viewerScrollPosition.y;
      float scrollX = xferType == TransferPump.TypeXfer.SourceToTarget ? 20 : 320;
      float scrollY = viewerScrollPosition.y;
      string step = "begin";
      try
      {
        step = "begin button loop";
        List<ModDockedVessel>.Enumerator modDockedVessels = SMAddon.SmVessel.ModDockedVessels.GetEnumerator();
        while (modDockedVessels.MoveNext())
        {
          if (modDockedVessels.Current == null) continue;
          // Build the part button title...
          step = "vessel button title";
          string strDescription = GetResourceDescription(selectedResources, modDockedVessels.Current);

          // set the conditions for a button style change.
          int btnWidth = 273;
          if (!CurrSettings.RealXfers) btnWidth = 180;

          // Set style based on viewer and toggled state.
          step = "Set style";
          GUIStyle style = GetVesselButtonStyle(xferType, modDockedVessels.Current);

          GUILayout.BeginHorizontal();

          // Now let's account for any target buttons already pressed. (sources and targets for resources cannot be the same)
          GUI.enabled = CanSelectVessel(xferType, modDockedVessels.Current);

          step = "Render vessel Buttons";
          if (GUILayout.Button($"{strDescription}", style, GUILayout.Width(btnWidth), GUILayout.Height(20)))
          {
            VesselButtonToggled(xferType, modDockedVessels.Current);
          }
          Rect rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && rect.Contains(Event.current.mousePosition))
            SMHighlighter.SetMouseOverData(rect, scrollY, scrollX, SelectBox.height, modDockedVessels.Current, Event.current.mousePosition);

          // Reset Button enabling.
          GUI.enabled = true;

          //step = "Render dump/fill buttons";
          // Crew
          if (selectedResources.Contains(SMConditions.ResourceType.Crew.ToString()))
          {
            if (SMConditions.CanShowCrewFillDumpButtons())
              CrewFillDumpVesselButtons(modDockedVessels.Current);
          }

          // Science

          // Resources
          else if (!CurrSettings.RealXfers)
          {
            if (selectedResources.Count > 1)
              GUI.enabled = TransferPump.CalcRemainingResource(modDockedVessels.Current.VesselParts, selectedResources[0]) > 0 ||
                            TransferPump.CalcRemainingResource(modDockedVessels.Current.VesselParts, selectedResources[1]) > 0;
            else
              GUI.enabled = TransferPump.CalcRemainingResource(modDockedVessels.Current.VesselParts, selectedResources[0]) > 0;
            GUIStyle style1 = xferType == TransferPump.TypeXfer.SourceToTarget
              ? SMStyle.ButtonSourceStyle
              : SMStyle.ButtonTargetStyle;
            uint pumpId = TransferPump.GetPumpIdFromHash(string.Join("", selectedResources.ToArray()),
              modDockedVessels.Current.VesselParts.First(), modDockedVessels.Current.VesselParts.Last(), xferType,
              TransferPump.TriggerButton.Transfer);

            GUIContent dumpContent = !TransferPump.IsPumpInProgress(pumpId) ? dumpPContent : stopPContent;
            if (GUILayout.Button(dumpContent, style1, GUILayout.Width(45), GUILayout.Height(20)))
            {
              SMPart.ToggleDumpResource(modDockedVessels.Current.VesselParts, selectedResources, pumpId);
            }
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips)
              ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

            GUIStyle style2 = xferType == TransferPump.TypeXfer.SourceToTarget
              ? SMStyle.ButtonSourceStyle
              : SMStyle.ButtonTargetStyle;
            if (selectedResources.Count > 1)
              GUI.enabled = TransferPump.CalcRemainingCapacity(modDockedVessels.Current.VesselParts, selectedResources[0]) > 0 ||
                            TransferPump.CalcRemainingCapacity(modDockedVessels.Current.VesselParts, selectedResources[0]) > 0;
            else
              GUI.enabled = TransferPump.CalcRemainingCapacity(modDockedVessels.Current.VesselParts, selectedResources[0]) > 0;

            if (GUILayout.Button(fillVContent, style2, GUILayout.Width(30), GUILayout.Height(20)))
            {
              SMPart.FillResource(modDockedVessels.Current.VesselParts, selectedResources[0]);
              if (selectedResources.Count > 1)
                SMPart.FillResource(modDockedVessels.Current.VesselParts, selectedResources[1]);
            }
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips)
              ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);
            GUI.enabled = true;
          }
          GUILayout.EndHorizontal();
        }
        modDockedVessels.Dispose();
      }
      catch (Exception ex)
      {
        if (!SMAddon.FrameErrTripped)
        {
          SmUtils.LogMessage($"Error in Windowtransfer.VesselTransferViewer ({xferType}) at step:  {step}.  Error:  {ex}",
            SmUtils.LogType.Error, true);
          SMAddon.FrameErrTripped = true;
        }
      }
    }

    private static void ResourceDumpFillButtons(List<string> selectedResources, TransferPump.TypeXfer xferType, Part part)
    {
      uint pumpId = TransferPump.GetPumpIdFromHash(string.Join("", selectedResources.ToArray()), part, part,
        xferType, TransferPump.TriggerButton.Transfer);

      GUIContent dumpActionContent = !TransferPump.IsPumpInProgress(pumpId) ? dumpVContent : stopVContent;
      GUIStyle style1 = xferType == TransferPump.TypeXfer.SourceToTarget
        ? SMStyle.ButtonSourceStyle
        : SMStyle.ButtonTargetStyle;
      GUI.enabled = CanDumpPart(part);

      if (GUILayout.Button(dumpActionContent, style1, GUILayout.Width(45), GUILayout.Height(20)))
      {
        SMPart.ToggleDumpResource(part, selectedResources, pumpId);
      }
      Rect rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);


      if (CurrSettings.RealXfers || (!CurrSettings.EnablePfResources || !SMConditions.IsInPreflight())) return;
      GUIStyle style2 = xferType == TransferPump.TypeXfer.SourceToTarget
        ? SMStyle.ButtonSourceStyle
        : SMStyle.ButtonTargetStyle;
      if (selectedResources.Count > 1)
        GUI.enabled = part.Resources[selectedResources[0]].amount <
                      part.Resources[selectedResources[0]].maxAmount ||
                      part.Resources[selectedResources[1]].amount <
                      part.Resources[selectedResources[1]].maxAmount;
      else
        GUI.enabled = part.Resources[selectedResources[0]].amount <
                      part.Resources[selectedResources[0]].maxAmount;
      if (GUILayout.Button(fillPContent, style2, GUILayout.Width(30), GUILayout.Height(20)))
      {
        SMPart.FillResource(part, selectedResources[0]);
        if (selectedResources.Count > 1)
          SMPart.FillResource(part, selectedResources[1]);
      }
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);
    }

    private static void CrewFillDumpVesselButtons(ModDockedVessel vessel)
    {
      int crewCapacity = SmUtils.GetPartsCrewCapacity(vessel.VesselParts);
      int crewCount = SmUtils.GetPartsCrewCount(vessel.VesselParts);
      GUI.enabled = SmUtils.GetPartsCrewCapacity(vessel.VesselParts) > 0;
      if (GUILayout.Button(dumpCrewContent, GUILayout.Width(45), GUILayout.Height(20)))
      {
        List<Part>.Enumerator part = vessel.VesselParts.GetEnumerator();
        while (part.MoveNext())
        {
          if (part.Current == null) continue;
          SMPart.DumpCrew(part.Current);
        }
        part.Dispose();
      }
      Rect rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      GUI.enabled = crewCount < crewCapacity;
      if (GUILayout.Button(fillCrewContent, GUILayout.Width(45), GUILayout.Height(20)))
      {
        List<Part>.Enumerator part = vessel.VesselParts.GetEnumerator();
        while (part.MoveNext())
        {
          if (part.Current == null) continue;
          SMPart.FillCrew(part.Current);
        }
        part.Dispose();
      }
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);
    }

    private static void CrewFillDumpPartButtons(Part part)
    {

      GUI.enabled = part.protoModuleCrew.Count > 0;
      if (GUILayout.Button(dumpCrewContent, GUILayout.Width(45), GUILayout.Height(20)))
      {
        SMPart.DumpCrew(part);
      }
      Rect rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      GUI.enabled = part.protoModuleCrew.Count < part.CrewCapacity;
      if (GUILayout.Button(fillCrewContent, GUILayout.Width(45), GUILayout.Height(20)))
      {
        SMPart.FillCrew(part);
      }
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);
    }
    #endregion

    #region Crew Details Viewer

    private static void CrewDetails(List<ProtoCrewMember> selectedCrewMembers, List<Part> selectedPartsFrom, List<Part> selectedPartsTo, bool isVesselMode, bool isSourceView)
    {
      float xOffset = 30;
      // TODO: these could be moved out of onGUI, and updated statically from events that cause changes.
      int sourceCrewCount = SmUtils.GetPartsCrewCount(selectedPartsFrom);
      int targetCapacity = GetAvailableCrewSpace(selectedPartsTo);
      //bool crewContainsTourists = (SmUtils.CrewContainsTourists(selectedPartsFrom));
      // end_todo

      // Vessel mode only code.
      bool touristsOnly = isSourceView ? TouristsOnlyFrom : TouristsOnlyTo;
      if (selectedPartsFrom.Count <= 0) return;
      if (isVesselMode)
      {
        // If vessel contains any crew, display the tourist filter option.
        if (sourceCrewCount > 0) touristsOnly = ShowOptionTouristsOnly(isSourceView, touristsOnly, xOffset);

        // check to see if crewmembers selected match criteria for a select all setting...
        bool selectAll = IsSelectAll(selectedCrewMembers, sourceCrewCount, isSourceView, targetCapacity);

        // Now display the selectAll option and Xfer button.
        ShowSelectAllOption(selectedCrewMembers, selectedPartsFrom, selectedPartsTo, isSourceView, sourceCrewCount, targetCapacity, selectAll, touristsOnly, xOffset);
        GUI.enabled = true;
      }

      List<ProtoCrewMember>.Enumerator crewMember = SMAddon.SmVessel.GetCrewFromParts(selectedPartsFrom).GetEnumerator();
      // ReSharper disable once ForCanBeConvertedToForeach
      while (crewMember.MoveNext() && GUI.enabled)
      {
        if (crewMember.Current == null) continue;
        if (isVesselMode && touristsOnly && crewMember.Current.type != ProtoCrewMember.KerbalType.Tourist) continue;

        CrewMemberDetails(selectedPartsFrom, selectedPartsTo, selectedCrewMembers, crewMember.Current, xOffset, isVesselMode, targetCapacity);
      }
      crewMember.Dispose();
      // Cater for DeepFreeze Continued... parts - list frozen kerbals
      if (!InstalledMods.IsDfApiReady) return;
      try
      {
        PartModule deepFreezer = (from PartModule pm in selectedPartsFrom[0].Modules
          where pm.moduleName == "DeepFreezer"
          select pm).SingleOrDefault();
        if (deepFreezer == null) return;
        DfWrapper.DeepFreezer sourcepartFrzr = new DfWrapper.DeepFreezer(deepFreezer);
        if (sourcepartFrzr.StoredCrewList.Count <= 0) return;
        //Dictionary<string, DFWrapper.KerbalInfo> frozenKerbals = DFWrapper.DeepFreezeAPI.FrozenKerbals;
        List<DfWrapper.FrznCrewMbr>.Enumerator frznCrew = sourcepartFrzr.StoredCrewList.GetEnumerator();
        while (frznCrew.MoveNext())
        {
          if (frznCrew.Current == null) continue;
          FrozenCrewMemberDetails(xOffset, frznCrew.Current);
        }
        frznCrew.Dispose();
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage(
          $" in WindowTransfer.CrewDetails.  Error attempting to check DeepFreeze for FrozenKerbals.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}", SmUtils.LogType.Error, true);
        //Debug.Log("Error attempting to check DeepFreeze for FrozenKerbals");
        //Debug.Log(ex.Message);
      }
    }

    private static void FrozenCrewMemberDetails(float xOffset, DfWrapper.FrznCrewMbr frznCrew)
    {
      GUILayout.BeginHorizontal();
      GUI.enabled = false;
      if (GUILayout.Button(moveKerbalContent, SMStyle.ButtonStyle,
        GUILayout.Width(15), GUILayout.Height(20))) // "Move Kerbal to another seat within Part"
      {
        ToolTip = "";
      }
      Rect rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);
      string trait = "";
      ProtoCrewMember frozenKerbal = FindFrozenKerbal(frznCrew.CrewName);
      if (frozenKerbal != null) trait = frozenKerbal.trait;
      GUI.enabled = true;
      GUILayout.Label($"  {frznCrew.CrewName} ({trait})", SMStyle.LabelStyleCyan, GUILayout.Width(190),
        GUILayout.Height(20));
     
      if (GUILayout.Button(thawContent, SMStyle.ButtonStyle, GUILayout.Width(50), GUILayout.Height(20)))
      {
        WindowRoster.ThawKerbal(frznCrew.CrewName);
        ToolTip = "";
      }
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);
      GUILayout.EndHorizontal();
    }

    private static void ShowSelectAllOption(List<ProtoCrewMember> selectedCrewMembers, List<Part> selectedPartsFrom, List<Part> selectedPartsTo,
      bool isSourceView, int sourceCrewCount, int targetCapacity, bool selectAll, bool touristsOnly, float xOffset)
    {
      Rect rect;
      GUILayout.BeginHorizontal();
      GUI.enabled = selectedPartsFrom.Count > 0 && selectedPartsTo.Count > 0 && sourceCrewCount > 0 && targetCapacity > 0;
      selectAll = GUILayout.Toggle(selectAll, GUI.enabled ? selectAllContent : selectAllxContent, GUILayout.Width(180));
      if (selectAll != (isSourceView ? SelectAllFrom : SelectAllTo))
      {
        if (selectAll)
        {
          List<ProtoCrewMember>.Enumerator member =
            SMAddon.SmVessel.GetCrewFromParts(selectedPartsFrom).GetEnumerator();
          while (member.MoveNext())
          {
            if (member.Current == null) continue;
            if (touristsOnly && member.Current.type != ProtoCrewMember.KerbalType.Tourist) continue;
            if (!selectedCrewMembers.Contains(member.Current) && targetCapacity > selectedCrewMembers.Count)
              selectedCrewMembers.Add(member.Current);
          }
          member.Dispose();
        }
        else
        {
          selectedCrewMembers.Clear();
        }
        if (isSourceView) SelectAllFrom = selectAll;
        else SelectAllTo = selectAll;
      }
      IsSelectAll(selectedCrewMembers, sourceCrewCount, isSourceView, targetCapacity);
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);

      GUI.enabled = selectedCrewMembers.Count > 0 && SMConditions.CanKerbalsBeXferred(selectedPartsFrom, selectedPartsTo);
      XferToolTip = selectedCrewMembers.Count <= 0 ? fillCrewTtContent : moveCrewTtContent;
      CrewSelectedXferButton(selectedPartsFrom, selectedPartsTo, selectedCrewMembers, xOffset);
      GUILayout.EndHorizontal();
    }

    private static bool ShowOptionTouristsOnly(bool isSourceView, bool touristsOnly, float xOffset)
    {
      Rect rect;
      GUI.enabled = true;
      // check to see if crewmembers selected match criteria for a show Tourists only setting...
      touristsOnly = GUILayout.Toggle(touristsOnly, touristOnlyContent, GUILayout.Width(180));
      if (touristsOnly != (isSourceView ? TouristsOnlyFrom : TouristsOnlyTo))
      {
        if (isSourceView) TouristsOnlyFrom = touristsOnly;
        else TouristsOnlyTo = touristsOnly;
      }
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);
      return touristsOnly;
    }

    private static bool IsSelectAll(List<ProtoCrewMember> selectedCrewMembers, int sourceCrewCount, bool isSourceView, int targetCapacity)
    {
      bool selectAll = false;
      if (sourceCrewCount <= 0 || targetCapacity <= 0) return false;
      if (selectedCrewMembers.Count < sourceCrewCount && selectedCrewMembers.Count < targetCapacity)
      {
        if (isSourceView) SelectAllFrom = false;
        else SelectAllTo = false;
      }
      else if (selectedCrewMembers.Count == sourceCrewCount ||
               selectedCrewMembers.Count == targetCapacity)
      {
        selectAll = true;
        if (isSourceView) SelectAllFrom = true;
        else SelectAllTo = true;
      }
      return selectAll;
    }

    private static void CrewMemberDetails(List<Part> selectedPartsFrom, List<Part> selectedPartsTo, List<ProtoCrewMember> crewMembers, ProtoCrewMember crewMember, float xOffset, bool isVesselMode, int targetCapacity)
    {
      const float cmWidth = 180;
      const float cmMoveWidth = 25;

      Rect rect;
      GUILayout.BeginHorizontal();
      if (isVesselMode)
      {
        bool selected = crewMembers.Contains(crewMember);
        selected = GUILayout.Toggle(selected, $"{crewMember.name} ({crewMember.experienceTrait.Title})", GUILayout.Width(cmWidth), GUILayout.Height(20));
        if (selected && !crewMembers.Contains(crewMember))
        {
          if (crewMembers.Count < targetCapacity) crewMembers.Add(crewMember);
        }
        else if (!selected && crewMembers.Contains(crewMember))
        {
          crewMembers.Remove(crewMember);
        }
      }
      else
      {
        GUI.enabled = true;
        GUILayout.Label($"  {crewMember.name} ({crewMember.experienceTrait.Title })", GUILayout.Width(cmWidth), GUILayout.Height(20));
      }
      GUI.enabled = !SMConditions.IsTransferInProgress();
      if (GUILayout.Button(moveKerbalContent, SMStyle.ButtonStyle, GUILayout.Width(cmMoveWidth), GUILayout.Height(20)))
      {
        ToolTip = "";
        SMAddon.SmVessel.TransferCrewObj.CrewTransferBegin(crewMember, selectedPartsFrom[0], selectedPartsFrom[0]);
      }
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);

      // Display the Transfer Button.
      CrewMemberXferButton(selectedPartsFrom, selectedPartsTo, crewMember, xOffset);
      GUILayout.EndHorizontal();
    }

    private static void CrewMemberXferButton(List<Part> selectedPartsFrom, List<Part> selectedPartsTo, ProtoCrewMember crewMember, float xOffset)
    {
      const float btnWidth = 60;
      Rect rect;
      GUI.enabled = SMConditions.CanKerbalsBeXferred(selectedPartsFrom, selectedPartsTo);
      if ((SMAddon.SmVessel.TransferCrewObj.FromCrewMember == crewMember ||
           SMAddon.SmVessel.TransferCrewObj.ToCrewMember == crewMember) && SMConditions.IsTransferInProgress())
      {
        GUI.enabled = true;
        GUILayout.Label(movingContent, GUILayout.Width(btnWidth), GUILayout.Height(20));
      }
      else if (!SMConditions.IsClsInSameSpace(selectedPartsFrom[0],
        selectedPartsTo.Count > 0 ? selectedPartsTo[0] : null))
      {
        GUI.enabled = crewMember.type != ProtoCrewMember.KerbalType.Tourist &&
          selectedPartsFrom[0].airlock != null;
        //GUIContent evaContent = new GUIContent("EVA", EvaToolTip);
        GUIContent content = new GUIContent(evaContent, EvaToolTip);
        if (GUILayout.Button(content, SMStyle.ButtonStyle, GUILayout.Width(btnWidth), GUILayout.Height(20)))
        {
          ToolTip = "";
          // Bug #7 - KerbalRef can be null if the part has no IVA. This can only happen in a modded install.
          if (crewMember.KerbalRef) {
            FlightEVA.SpawnEVA(crewMember.KerbalRef);
          } else {
            FlightEVA.fetch.spawnEVA(crewMember, selectedPartsFrom[0], selectedPartsFrom[0].airlock);
          }
          GUI.enabled = false;
        }
        rect = GUILayoutUtility.GetLastRect();
        if (Event.current.type == EventType.Repaint && ShowToolTips)
          ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);
      }
      else
      {
        if (GUILayout.Button(new GUIContent(xferContent, XferToolTip),
          SMStyle.ButtonStyle, GUILayout.Width(btnWidth), GUILayout.Height(20))) // "Xfer"
        {
          SMAddon.SmVessel.TransferCrewObj.CrewTransferBegin(crewMember, selectedPartsFrom[0], selectedPartsTo[0]);
        }
        rect = GUILayoutUtility.GetLastRect();
        if (Event.current.type == EventType.Repaint && ShowToolTips)
          ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);
      }
    }

    private static void CrewSelectedXferButton(List<Part> selectedPartsFrom, List<Part> selectedPartsTo, List<ProtoCrewMember> crewMembers, float xOffset)
    {
      Rect rect;
      //if ((SMAddon.SmVessel.TransferCrewObj.FromCrewMembers == crewMembers ||
      //     SMAddon.SmVessel.TransferCrewObj.ToCrewMembers == crewMembers) && SMConditions.IsTransferInProgress())
      if (SMConditions.IsTransferInProgress())
      {
        GUI.enabled = true;
        //GUILayout.Label("Moving", GUILayout.Width(50), GUILayout.Height(20));
        GUILayout.Label(movingContent, GUILayout.Width(50), GUILayout.Height(20));
      }
      else
      {
        if (GUILayout.Button(new GUIContent($"{xferCrewContent} ({crewMembers.Count})", XferToolTip),
          SMStyle.ButtonStyle, GUILayout.Width(90),
          GUILayout.Height(20))) // "Xfer Crew"
        {
          SMAddon.SmVessel.TransferCrewObj.CrewTransfersBegin(crewMembers, selectedPartsFrom, selectedPartsTo);
        }
        rect = GUILayoutUtility.GetLastRect();
        if (Event.current.type == EventType.Repaint && ShowToolTips)
          ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);
      }
    }
    #endregion

    #region Science Details Viewer

    // TODO: This method is currntly unused.  Examine for relavence and need
    private static void ScienceVesselDetails(Dictionary<PartModule, bool> sourceModules, Dictionary<PartModule, bool> targetModules, bool isVesselMode)
    {
      if (sourceModules.Count <= 0 || targetModules.Count <= 0) return;
      const float xOffset = 30;
      // Okay, for vessels we want summaries.  then a result list of modules to transfer based on those summaries.
      // - Collectable Science
      // - Processed Science (Labs)
      // - Uncollectable Science (eva only Experiments)
      // - Unprocessed science (Labs)

      Dictionary<PartModule, bool>.KeyCollection.Enumerator modules = sourceModules.Keys.GetEnumerator();
      while (modules.MoveNext())
      {
        if (modules.Current == null) continue;
        // experiments/Containers.
        int scienceCount = ((IScienceDataContainer)modules.Current).GetScienceCount();
        bool isCollectable = true;
        switch (modules.Current.moduleName)
        {
          case "ModuleScienceExperiment":
            isCollectable = ((ModuleScienceExperiment)modules.Current).dataIsCollectable;
            break;
          case "ModuleScienceContainer":
            isCollectable = ((ModuleScienceContainer)modules.Current).dataIsCollectable;
            break;
          case "ModuleScienceLab":
          case "ModuleScienceConverter":
            isCollectable = true;
            break;
        }

        GUILayout.BeginHorizontal();
        GUI.enabled = ((IScienceDataContainer)modules.Current).GetScienceCount() > 0;

        string label = "+";
        // Expand/Collapse Science detail.
        string toolTip = $"{expScienceContent} {(GUI.enabled ? "" : disabledXferContent)}";
        GUIStyle expandStyle = ScienceModulesSource[modules.Current] ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
        if (ScienceModulesSource[modules.Current]) label = "-";
        if (GUILayout.Button(new GUIContent(label, toolTip), expandStyle, GUILayout.Width(15), GUILayout.Height(20)))
        {
          ScienceModulesSource[modules.Current] = !ScienceModulesSource[modules.Current];
        }
        Rect rect = GUILayoutUtility.GetLastRect();
        if (Event.current.type == EventType.Repaint && ShowToolTips)
          ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);

        GUI.enabled = true;
        GUILayout.Label($"{modules.Current.moduleName} - ({scienceCount})", GUILayout.Width(205),
          GUILayout.Height(20));

        // If we have target selected, it is not the same as the source, there is science to xfer.
        if (SMAddon.SmVessel.SelectedModuleTarget != null && scienceCount > 0)
        {
          if (CurrSettings.RealXfers && !isCollectable)
          {
            GUI.enabled = false;
            toolTip = noXferRealismContent;
          }
          else
          {
            GUI.enabled = true;
            toolTip = xferOnlyProcSciContent;
          }
          GUIContent xferContent = new GUIContent(moveKerbalTtContent, toolTip);
          if (GUILayout.Button(xferContent, SMStyle.ButtonStyle, GUILayout.Width(40),
            GUILayout.Height(20)))
          {
            SMAddon.SmVessel.SelectedModuleSource = modules.Current;
            ProcessController.TransferScience(SMAddon.SmVessel.SelectedModuleSource,
              SMAddon.SmVessel.SelectedModuleTarget);
            SMAddon.SmVessel.SelectedModuleSource = null;
          }
          rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && ShowToolTips)
            ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);

          if (GUI.enabled && SMAddon.SmVessel.Vessel.FindPartModulesImplementing<ModuleScienceLab>().Count > 0)
          {
            if (GUILayout.Button(xferProcContent, SMStyle.ButtonStyle, GUILayout.Width(40), GUILayout.Height(20)))
            {
              SMAddon.SmVessel.SelectedModuleSource = modules.Current;
              ProcessController.TransferScienceLab(SMAddon.SmVessel.SelectedModuleSource,
                SMAddon.SmVessel.SelectedModuleTarget,
                ProcessController.Selection.OnlyProcessed);
              SMAddon.SmVessel.SelectedModuleSource = null;
            }
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips)
              ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);
            if (GUILayout.Button(xferUnprocContent, SMStyle.ButtonStyle, GUILayout.Width(50), GUILayout.Height(20)))
            {
              SMAddon.SmVessel.SelectedModuleSource = modules.Current;
              ProcessController.TransferScienceLab(SMAddon.SmVessel.SelectedModuleSource,
                SMAddon.SmVessel.SelectedModuleTarget,
                ProcessController.Selection.OnlyUnprocessed);
              SMAddon.SmVessel.SelectedModuleSource = null;
            }
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips)
              ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);
          }
        }
        GUILayout.EndHorizontal();
        if (ScienceModulesSource[modules.Current])
        {
          IEnumerator items = ((IScienceDataContainer)modules.Current).GetData().GetEnumerator();
          while (items.MoveNext())
          {
            if (items.Current == null) continue;
            GUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.Width(15), GUILayout.Height(20));

            // Get science data from experiment.
            string expId = ((ScienceData)items.Current).subjectID.Split('@')[0];
            string expKey = ((ScienceData)items.Current).subjectID.Split('@')[1];
            ScienceExperiment se = ResearchAndDevelopment.GetExperiment(expId);
            string key = (from k in se.Results.Keys where expKey.Contains(k) select k).FirstOrDefault();
            key = key ?? "default";
            string results = se.Results[key];

            // Build Tooltip
            toolTip = ((ScienceData)items.Current).title;
            toolTip += $"\n-{resultsTtContent}:  {results}";
            toolTip +=
              $"\n-{dataAmtTtContent}:  {((ScienceData)items.Current).dataAmount} {mitsTtContent}";
            toolTip +=
              $"\n-{xmitValTtContent}:  {((ScienceData)items.Current).baseTransmitValue}"; // was transmitValue;
            toolTip += $"\n-{labValTtContent}:  {((ScienceData)items.Current).labValue}";
            toolTip += $"\n-{xmitBonusTtContent}:  {((ScienceData)items.Current).transmitBonus}";  // Was labBoost

            GUILayout.Label(new GUIContent(se.experimentTitle, toolTip), SMStyle.LabelStyleNoWrap, GUILayout.Width(205), GUILayout.Height(20));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips)
              ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);

            if (CurrSettings.RealXfers && !isCollectable)
            {
              GUI.enabled = false;
              toolTip = noXferSciRlsmTtContent;
            }
            else
            {
              toolTip = xferSciTtContent;
              GUI.enabled = true;
            }
            if (SMAddon.SmVessel.SelectedModuleTarget != null && scienceCount > 0)
            {
              GUIContent content = new GUIContent(xferContent, toolTip);
              if (GUILayout.Button(content, SMStyle.ButtonStyle, GUILayout.Width(40), GUILayout.Height(20)))
              {
                if (((ModuleScienceContainer)SMAddon.SmVessel.SelectedModuleTarget).AddData(((ScienceData)items.Current)))
                  ((IScienceDataContainer)modules.Current).DumpData(((ScienceData)items.Current));
              }
              rect = GUILayoutUtility.GetLastRect();
              if (Event.current.type == EventType.Repaint && ShowToolTips)
                ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);
            }
            GUILayout.EndHorizontal();
          }
        }
        GUI.enabled = true;
      }
      modules.Dispose();
    }

    private static void ScienceDetailsSource(bool isVesselMode)
    {
      if (SMAddon.SmVessel.SelectedPartsSource.Count <= 0) return;
      const float xOffset = 30;

      Dictionary<PartModule, bool>.KeyCollection.Enumerator modules = ScienceModulesSource.Keys.GetEnumerator();
      while (modules.MoveNext())
      {
        if (modules.Current == null) continue;
        // experiments/Containers.
        int scienceCount = ((IScienceDataContainer)modules.Current).GetScienceCount();
        bool isCollectable = true;
        switch (modules.Current.moduleName)
        {
          case "ModuleScienceExperiment":
            isCollectable = ((ModuleScienceExperiment)modules.Current).dataIsCollectable;
            break;
          case "ModuleScienceContainer":
            isCollectable = ((ModuleScienceContainer)modules.Current).dataIsCollectable;
            break;
        }

        GUILayout.BeginHorizontal();
        GUI.enabled = ((IScienceDataContainer)modules.Current).GetScienceCount() > 0;

        string label = "+";
        // string toolTip = string.Format("{0} {1}", "Expand/Collapse Science detail.", GUI.enabled? "" : "(Disabled, nothing to xfer)");
        string toolTip =
          $"{expScienceContent} {(GUI.enabled ? "" : disabledXferContent)}";
        GUIStyle expandStyle = ScienceModulesSource[modules.Current] ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
        if (ScienceModulesSource[modules.Current]) label = "-";
        if (GUILayout.Button(new GUIContent(label, toolTip), expandStyle, GUILayout.Width(15), GUILayout.Height(20)))
        {
          ScienceModulesSource[modules.Current] = !ScienceModulesSource[modules.Current];
        }
        Rect rect = GUILayoutUtility.GetLastRect();
        if (Event.current.type == EventType.Repaint && ShowToolTips)
          ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);

        GUI.enabled = true;
        GUILayout.Label($"{modules.Current.moduleName} - ({scienceCount})", GUILayout.Width(205),
          GUILayout.Height(20));

        // If we have target selected, it is not the same as the source, there is science to xfer.
        if (SMAddon.SmVessel.SelectedModuleTarget != null && scienceCount > 0)
        {
          if (CurrSettings.RealXfers && !isCollectable)
          {
            GUI.enabled = false;
            toolTip = noXferRealismContent;
          }
          else
          {
            GUI.enabled = true;
            toolTip = xferOnlyProcSciContent;
          }
          GUIContent xferSciContent = new GUIContent(xferContent, toolTip);
          if (GUILayout.Button(xferSciContent, SMStyle.ButtonStyle, GUILayout.Width(40),
            GUILayout.Height(20)))
          {
            SMAddon.SmVessel.SelectedModuleSource = modules.Current;
            ProcessController.TransferScience(SMAddon.SmVessel.SelectedModuleSource,
              SMAddon.SmVessel.SelectedModuleTarget);
            SMAddon.SmVessel.SelectedModuleSource = null;
          }
          rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && ShowToolTips)
            ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);

          if (GUI.enabled && SMAddon.SmVessel.Vessel.FindPartModulesImplementing<ModuleScienceLab>().Count > 0)
          {
            if (GUILayout.Button(xferProcContent, SMStyle.ButtonStyle, GUILayout.Width(40), GUILayout.Height(20)))
            {
              SMAddon.SmVessel.SelectedModuleSource = modules.Current;
              ProcessController.TransferScienceLab(SMAddon.SmVessel.SelectedModuleSource,
                                                   SMAddon.SmVessel.SelectedModuleTarget,
                                             ProcessController.Selection.OnlyProcessed);
              SMAddon.SmVessel.SelectedModuleSource = null;
            }
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips)
              ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);
            //content = new GUIContent("Unproc", "Transfer only science that was not processed yet";
            if (GUILayout.Button(xferUnprocContent, SMStyle.ButtonStyle, GUILayout.Width(50), GUILayout.Height(20)))
            {
              SMAddon.SmVessel.SelectedModuleSource = modules.Current;
              ProcessController.TransferScienceLab(SMAddon.SmVessel.SelectedModuleSource,
                                                   SMAddon.SmVessel.SelectedModuleTarget,
                                             ProcessController.Selection.OnlyUnprocessed);
              SMAddon.SmVessel.SelectedModuleSource = null;
            }
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips)
              ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);
          }
        }
        GUILayout.EndHorizontal();
        if (ScienceModulesSource[modules.Current])
        {
          IEnumerator items = ((IScienceDataContainer) modules.Current).GetData().GetEnumerator();
          while (items.MoveNext())
          {
            if (items.Current == null) continue;
            GUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.Width(15), GUILayout.Height(20));

            // Get science data from experiment.
            string expId = ((ScienceData)items.Current).subjectID.Split('@')[0];
            string expKey = ((ScienceData)items.Current).subjectID.Split('@')[1];
            ScienceExperiment se = ResearchAndDevelopment.GetExperiment(expId);
            string key = (from k in se.Results.Keys where expKey.Contains(k) select k).FirstOrDefault();
            key = key ?? "default";
            string results = se.Results[key];

            // Build Tooltip
            toolTip = ((ScienceData)items.Current).title;
            toolTip += $"\n-{resultsTtContent}:  {results}";
            toolTip +=
              $"\n-{dataAmtTtContent}:  {((ScienceData)items.Current).dataAmount} {mitsTtContent}";
            toolTip +=
              $"\n-{xmitValTtContent}:  {((ScienceData)items.Current).baseTransmitValue}"; // was transmitValue;
            toolTip += $"\n-{labValTtContent}:  {((ScienceData)items.Current).labValue}";
            toolTip += $"\n-{xmitBonusTtContent}:  {((ScienceData)items.Current).transmitBonus}";  // Was labBoost

            GUILayout.Label(new GUIContent(se.experimentTitle, toolTip), SMStyle.LabelStyleNoWrap, GUILayout.Width(205), GUILayout.Height(20));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips)
              ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);

            if (CurrSettings.RealXfers && !isCollectable)
            {
              GUI.enabled = false;
              //toolTip = "Realistic Transfers is preventing transfer.\r\nData is marked not transferable";
              toolTip = noXferSciRlsmTtContent;
            }
            else
            {
              //toolTip = "Realistic Transfers is off, or Data is transferable";
              toolTip = xferSciTtContent;
              GUI.enabled = true;
            }
            if (SMAddon.SmVessel.SelectedModuleTarget != null && scienceCount > 0)
            {
              //GUIContent content = new GUIContent("Xfer", toolTip);
              GUIContent content = new GUIContent(xferContent, toolTip);
              if (GUILayout.Button(content, SMStyle.ButtonStyle, GUILayout.Width(40), GUILayout.Height(20)))
              {
                if (((ModuleScienceContainer)SMAddon.SmVessel.SelectedModuleTarget).AddData(((ScienceData)items.Current)))
                  ((IScienceDataContainer)modules.Current).DumpData(((ScienceData)items.Current));
              }
              rect = GUILayoutUtility.GetLastRect();
              if (Event.current.type == EventType.Repaint && ShowToolTips)
                ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);
            }
            GUILayout.EndHorizontal();
          }
        }
        GUI.enabled = true;
      }
      modules.Dispose();
    }

    private static void ScienceDetailsTarget()
    {
      const float xOffset = 30;
      if (SMAddon.SmVessel.SelectedPartsTarget.Count <= 0) return;
      int count =
        SMAddon.SmVessel.SelectedPartsTarget[0].Modules.Cast<PartModule>()
          .Count(tpm => tpm is IScienceDataContainer && tpm.moduleName != "ModuleScienceExperiment");

      List<PartModule>.Enumerator modules = SMAddon.SmVessel.SelectedPartsTarget[0].Modules.GetEnumerator();
      while (modules.MoveNext())
      {
        if (modules.Current == null) continue;
        // Containers.
        if (!(modules.Current is IScienceDataContainer) || (modules.Current).moduleName == "ModuleScienceExperiment") continue;
        int scienceCount = ((IScienceDataContainer)modules.Current).GetScienceCount();
        GUILayout.BeginHorizontal();
        GUILayout.Label($"{modules.Current.moduleName} - ({scienceCount})", GUILayout.Width(220),
          GUILayout.Height(20));
        // set the conditions for a button style change.
        bool isReceiveToggled = false;
        if (modules.Current == SMAddon.SmVessel.SelectedModuleTarget)
          isReceiveToggled = true;
        else if (count == 1)
        {
          SMAddon.SmVessel.SelectedModuleTarget = modules.Current;
          isReceiveToggled = true;
        }
        //SelectedModuleTarget = pm;
        GUIStyle style = isReceiveToggled ? SMStyle.ButtonToggledTargetStyle : SMStyle.ButtonStyle;

        // Only containers can receive science data
        if (modules.Current.moduleName != "ModuleScienceExperiment")
        {
          //GUIContent content = new GUIContent("Recv", "Set this module as the receiving container");
          if (GUILayout.Button(recvSciContent, style, GUILayout.Width(40), GUILayout.Height(20)))
          {
            SMAddon.SmVessel.SelectedModuleTarget = modules.Current;
          }
          Rect rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && ShowToolTips)
            ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);
        }
        GUILayout.EndHorizontal();
      }
      modules.Dispose();
    }
    #endregion

    #region Resources Details Viewer

    private static void ResourceDetailsViewer(TransferPump.TypeXfer pumpType)
    {
      Rect rect;
      // Let's get the pump objects we may use...
      List<string> selectedResources = SMAddon.SmVessel.SelectedResources;
      List<TransferPump> pumps = TransferPump.GetDisplayPumpsByType(pumpType);
      if (!(from pump in pumps where pump.FromParts.Count > 0 select pump).Any()) return;

      // This routine assumes that a resource has been selected on the Resource manifest window.
      // Set scrollX offsets for left and right viewers
      const int xOffset = 30;
      string toolTip;

      // Set pump ratios
      TransferPump activePump = TransferPump.GetRatioPump(pumps);
      TransferPump ratioPump = TransferPump.GetRatioPump(pumps, true);
      activePump.PumpRatio = 1;
      ratioPump.PumpRatio = TransferPump.CalcRatio(pumps);

      // Resource Flow control Display loop
      ResourceFlowButtons(pumpType, xOffset);

      // let's determine how much of a resource we can move to the target.
      double thisXferAmount = double.Parse(activePump.EditSliderAmount);
      double maxPumpAmount = TransferPump.CalcMaxPumpAmt(pumps[0].FromParts, pumps[0].ToParts, selectedResources);
      if (maxPumpAmount <= 0 && !TransferPump.PumpProcessOn) return;

      // Xfer Controls Display
      GUILayout.BeginHorizontal();
      string label;
      if (TransferPump.PumpProcessOn)
      {
        // We want to show this during transfer if the direction is correct...
        if (activePump.PumpType == pumpType)
        {
          //GUILayout.Label("Xfer Remaining:", GUILayout.Width(120));
          GUILayout.Label($"{xferRemainContent}:", GUILayout.Width(120));
          
          GUILayout.Label(activePump.PumpBalance.ToString("#######0.##"));
          if (SMAddon.SmVessel.SelectedResources.Count > 1)
            GUILayout.Label($" | {ratioPump.PumpBalance:#######0.##}");
        }
      }
      else
      {
        if (selectedResources.Count > 1)
        {
          label = $"{xferAmtContent}:";
          toolTip = xferAmtTtBothContent;
        }
        else
        {
          label = $"{resultsContent}:";
          toolTip = xferAmtTtContent;
        }
        GUILayout.Label(new GUIContent(label, toolTip), GUILayout.Width(65));
        rect = GUILayoutUtility.GetLastRect();
        if (Event.current.type == EventType.Repaint && ShowToolTips)
          ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);
        activePump.EditSliderAmount = GUILayout.TextField(activePump.EditSliderAmount, 20, GUILayout.Width(95),
          GUILayout.Height(20));
        thisXferAmount = double.Parse(activePump.EditSliderAmount);
        double ratioXferAmt = thisXferAmount * ratioPump.PumpRatio > ratioPump.FromCapacity
          ? ratioPump.FromCapacity
          : thisXferAmount * ratioPump.PumpRatio;
        if (SMAddon.SmVessel.SelectedResources.Count > 1)
        {
          label = $" | {ratioXferAmt:#######0.##}";
          toolTip = $"{smallXferAmtTtContent}:  {ratioPump.PumpRatio}.\n{noteRatioTtContent}";
          GUILayout.Label(new GUIContent(label, toolTip), GUILayout.Width(80));
          rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && ShowToolTips)
            ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);
        }
      }
      GUILayout.EndHorizontal();

      if (!SMConditions.IsShipControllable() || (!SMConditions.CanResourceBeXferred(pumpType, maxPumpAmount) &&
                                                 (activePump.PumpType != pumpType || !activePump.IsPumpOn))) return;
      GUILayout.BeginHorizontal();
      GUIStyle noPad = SMStyle.LabelStyleNoPad;
      GUILayout.Label(xferSliderContent, noPad, GUILayout.Width(50), GUILayout.Height(20));
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);
      thisXferAmount = GUILayout.HorizontalSlider((float)thisXferAmount, 0, (float)maxPumpAmount,
        GUILayout.Width(190));
      activePump.EditSliderAmount = thisXferAmount.ToString(CultureInfo.InvariantCulture);

      // set Xfer button style
      GUIContent xferPumpContent = !TransferPump.PumpProcessOn || activePump.PumpType == pumpType && !activePump.IsPumpOn ? xferStartContent : xferStopContent;
      GUI.enabled = !TransferPump.PumpProcessOn || activePump.PumpType == pumpType && activePump.IsPumpOn;
      if (GUILayout.Button(xferPumpContent, GUILayout.Width(40), GUILayout.Height(18)))
      {
        uint pumpId = TransferPump.GetPumpIdFromHash(string.Join("", selectedResources.ToArray()),
          pumps[0].FromParts.First(), pumps[0].ToParts.Last(), pumpType, TransferPump.TriggerButton.Transfer);
        if (!TransferPump.PumpProcessOn)
        {
          //Calc amounts and update xfer modules
          TransferPump.AssignPumpAmounts(pumps, thisXferAmount, pumpId);
          ProcessController.TransferResources(pumps);
        }
        else TransferPump.AbortAllPumpsInProcess(pumpId);
      }
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);
      GUILayout.EndHorizontal();
    }

    private static void ResourceFlowButtons(TransferPump.TypeXfer pumpType, int scrollX)
    {
      string step = "";
      try
      {
        List<TransferPump>.Enumerator displayPumps = TransferPump.GetDisplayPumpsByType(pumpType).GetEnumerator();
        while (displayPumps.MoveNext())
        {
          if (displayPumps.Current == null) continue;
          // this var is used for button state change management
          bool flowState = displayPumps.Current.FromParts.Any(part => part.Resources[displayPumps.Current.Resource].flowState);
          string flowtext = flowState ? onContent : offContent;

          // Flow control Display
          step = "resource quantities labels";

          GUILayout.BeginHorizontal();
          string label =
            $"{displayPumps.Current.Resource}: ({displayPumps.Current.FromRemaining:#######0.##}/{displayPumps.Current.FromCapacity:######0.##})";
          GUILayout.Label(label , SMStyle.LabelStyleNoWrap, GUILayout.Width(220),GUILayout.Height(18));
          GUILayout.Label(flowtext, GUILayout.Width(20), GUILayout.Height(18));
          if (SMAddon.SmVessel.Vessel.IsControllable)
          {
            step = "render flow button(s)";
            //GUIContent content = new GUIContent("Flow", "Enables/Disables flow of selected resource(s) from selected part(s).");
            if (GUILayout.Button(flowContent, GUILayout.Width(40), GUILayout.Height(20)))
            {
              List<Part>.Enumerator parts = displayPumps.Current.FromParts.GetEnumerator();
              while (parts.MoveNext())
              {
                if (parts.Current == null) continue;
                parts.Current.Resources[displayPumps.Current.Resource].flowState = !flowState;
              }
              parts.Dispose();
            }
            Rect rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips)
              ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, scrollX);
          }
          GUILayout.EndHorizontal();
        }
        displayPumps.Dispose();
      }
      catch (Exception ex)
      {
        if (!SMAddon.FrameErrTripped)
        {
          SmUtils.LogMessage(
            $" in WindowTransfer.ResourceFlowButtons at step:  {step}.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}", SmUtils.LogType.Error, true);
          SMAddon.FrameErrTripped = true;
        }
      }
    }
    #endregion

    #endregion Viewer Details (Bottom Half)

    #endregion

    #region Button Action Methods

    private static void PartButtonToggled(TransferPump.TypeXfer xferType, Part part)
    {
      string step = "Part Button Toggled";
      try
      {
        if (SMConditions.IsTransferInProgress()) return;
        if (xferType == TransferPump.TypeXfer.SourceToTarget)
        {
          // Now lets update the list...
          if (SMAddon.SmVessel.SelectedPartsSource.Contains(part))
          {
            SMAddon.SmVessel.SelectedPartsSource.Remove(part);
          }
          else
          {
            if (!SMConditions.AreSelectedResourcesTypeOther(SMAddon.SmVessel.SelectedResources))
              SMAddon.SmVessel.SelectedPartsSource.Clear();
            SMAddon.SmVessel.SelectedPartsSource.Add(part);
          }
          if (SMConditions.IsClsActive())
          {
            SMAddon.UpdateClsSpaces();
          }
          SMAddon.SmVessel.SelectedModuleSource = null;
          _scienceModulesSource = null;
        }
        else
        {
          if (SMAddon.SmVessel.SelectedPartsTarget.Contains(part))
          {
            SMAddon.SmVessel.SelectedPartsTarget.Remove(part);
          }
          else
          {
            if (!SMConditions.AreSelectedResourcesTypeOther(SMAddon.SmVessel.SelectedResources))
              SMAddon.SmVessel.SelectedPartsTarget.Clear();
            SMAddon.SmVessel.SelectedPartsTarget.Add(part);
          }
          SMAddon.SmVessel.SelectedModuleTarget = null;
        }
        step = "Set Xfer amounts?";
        if (!SMConditions.AreSelectedResourcesTypeOther(SMAddon.SmVessel.SelectedResources)) return;
        TransferPump.UpdateDisplayPumps();
      }
      catch (Exception ex)
      {
        if (!SMAddon.FrameErrTripped)
        {
          SmUtils.LogMessage(
            $"Error in WindowTransfer.PartButtonToggled ({xferType}) at step:  {step}.  Error:  {ex}",
            SmUtils.LogType.Error, true);
          SMAddon.FrameErrTripped = true;
        }
      }
    }

    private static void VesselButtonToggled(TransferPump.TypeXfer xferType, ModDockedVessel modVessel)
    {
      string step = "Vessel Button Toggled";
      try
      {
        if (xferType == TransferPump.TypeXfer.SourceToTarget)
        {
          // Now lets update the list...
          // Because IEquatable is not implemented by ModDockedVessel, we need to inspect the object for equality.
          // Additionally, we cannot use the object directly for a List<T>.Remove. Use the found object instead.
          // Since the objects are reconstructed during Refresh Lists, the objects are no longer reference objects, but value Objects.
          // I may play with IEquatable to see if it suits my needs at a later date, or alter the object to be consumed differently.
          ModDockedVessel modDockedVessel =
            SMAddon.SmVessel.SelectedVesselsSource.Find(v => v.VesselInfo.rootPartUId == modVessel.VesselInfo.rootPartUId);
          if (modDockedVessel != null)
            SMAddon.SmVessel.SelectedVesselsSource.Remove(modDockedVessel);
          else
            SMAddon.SmVessel.SelectedVesselsSource.Add(modVessel);
          SMAddon.SmVessel.SelectedPartsSource =
            SMAddon.SmVessel.GetVesselsPartsByResource(SMAddon.SmVessel.SelectedVesselsSource,
              SMAddon.SmVessel.SelectedResources);
        }
        else
        {
          ModDockedVessel modDockedVessel =
            SMAddon.SmVessel.SelectedVesselsTarget.Find(v => v.VesselInfo.rootPartUId == modVessel.VesselInfo.rootPartUId);
          if (modDockedVessel != null)
            SMAddon.SmVessel.SelectedVesselsTarget.Remove(modDockedVessel);
          else
            SMAddon.SmVessel.SelectedVesselsTarget.Add(modVessel);
          SMAddon.SmVessel.SelectedPartsTarget =
            SMAddon.SmVessel.GetVesselsPartsByResource(SMAddon.SmVessel.SelectedVesselsTarget,
              SMAddon.SmVessel.SelectedResources);
        }
        WindowManifest.ResolveResourcePartSelections(SMAddon.SmVessel.SelectedResources);
      }
      catch (Exception ex)
      {
        if (!SMAddon.FrameErrTripped)
        {
          SmUtils.LogMessage(
            $"Error in WindowTransfer.VesselButtonToggled ({xferType}) at step:  {step}.  Error:  {ex}",
            SmUtils.LogType.Error, true);
          SMAddon.FrameErrTripped = true;
        }
      }
    }

    #endregion

    #region Utilities

    internal static ProtoCrewMember FindFrozenKerbal(string crewName)
    {
      ProtoCrewMember retval = null;
      IEnumerator<ProtoCrewMember> crew = HighLogic.CurrentGame.CrewRoster.Unowned.GetEnumerator();
      while (crew.MoveNext())
      {
        if (crew.Current == null) continue;
        if (crew.Current.name != crewName) continue;
        retval = crew.Current;
        break;
      }
      crew.Dispose();
      return retval;
    }

    private static bool CanDumpPart(Part part)
    {
      bool isDumpable;
      if (SMAddon.SmVessel.SelectedResources.Count > 1)
        isDumpable = part.Resources[SMAddon.SmVessel.SelectedResources[0]].amount > 0 ||
                     part.Resources[SMAddon.SmVessel.SelectedResources[1]].amount > 0;
      else
        isDumpable = part.Resources[SMAddon.SmVessel.SelectedResources[0]].amount > 0;

      return isDumpable;
    }

    private static bool CanSelectVessel(TransferPump.TypeXfer xferType, ModDockedVessel modDockedVessel)
    {
      bool isSelectable = true;
      if (xferType == TransferPump.TypeXfer.SourceToTarget)
      {
        if (SMAddon.SmVessel.SelectedVesselsTarget.Find(v => v.VesselInfo.rootPartUId == modDockedVessel.VesselInfo.rootPartUId) != null)
          isSelectable = false;
      }
      else
      {
        if (SMAddon.SmVessel.SelectedVesselsSource.Find(v => v.VesselInfo.rootPartUId == modDockedVessel.VesselInfo.rootPartUId) != null)
          isSelectable = false;
      }
      return isSelectable;
    }

    private static GUIStyle GetPartButtonStyle(TransferPump.TypeXfer xferType, Part part)
    {
      GUIStyle style;
      if (xferType == TransferPump.TypeXfer.SourceToTarget)
      {
        style = SMAddon.SmVessel.SelectedPartsSource.Contains(part)
          ? SMStyle.ButtonToggledSourceStyle
          : SMStyle.ButtonSourceStyle;
      }
      else
      {
        style = SMAddon.SmVessel.SelectedPartsTarget.Contains(part)
          ? SMStyle.ButtonToggledTargetStyle
          : SMStyle.ButtonTargetStyle;
      }
      return style;
    }

    private static GUIStyle GetVesselButtonStyle(TransferPump.TypeXfer xferType, ModDockedVessel modDockedVessel)
    {
      GUIStyle style;
      if (xferType == TransferPump.TypeXfer.SourceToTarget)
      {
        style = SMAddon.SmVessel.SelectedVesselsSource.Find(v => v.VesselInfo.rootPartUId == modDockedVessel.VesselInfo.rootPartUId) != null
          ? SMStyle.ButtonToggledSourceStyle
          : SMStyle.ButtonSourceStyle;
      }
      else
      {
        style = SMAddon.SmVessel.SelectedVesselsTarget.Find(v => v.VesselInfo.rootPartUId == modDockedVessel.VesselInfo.rootPartUId) != null
          ? SMStyle.ButtonToggledTargetStyle
          : SMStyle.ButtonTargetStyle;
      }
      return style;
    }

    private static string GetResourceDescription(IList<string> selectedResources, Part part)
    {
      string strDescription;

      if (selectedResources.Contains(SMConditions.ResourceType.Crew.ToString()))
      {
        strDescription = $"{SmUtils.GetPartCrewCount(part)} - {part.partInfo.title}";
      }
      else if (selectedResources.Contains(SMConditions.ResourceType.Science.ToString()))
      {
        int cntScience = GetScienceCount(part);
        strDescription = $"{cntScience} - {part.partInfo.title}";
      }
      else
      {
        strDescription = $"{part.Resources[selectedResources[0]].amount:######0.##} - {part.partInfo.title}";
      }
      return strDescription;
    }

    private static string GetResourceDescription(List<string> selectedResources, ModDockedVessel modDockedVvessel)
    {
      return $"{GetVesselResourceTotals(modDockedVvessel, selectedResources)} - {modDockedVvessel.VesselName}";
    }

    private static int GetScienceCount(Part part)
    {
      try
      {
        return part.Modules.OfType<IScienceDataContainer>().Sum(pm => pm.GetScienceCount());
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage($" in GetScienceCount.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}",
          SmUtils.LogType.Error, true);
        return 0;
      }
    }

    private static int GetScienceCount(ModDockedVessel vessel)
    {
      try
      {
        int count = 0;
        List<Part>.Enumerator part = vessel.VesselParts.GetEnumerator();
        while (part.MoveNext())
        {
          if (part.Current == null) continue;
          count += part.Current.Modules.OfType<IScienceDataContainer>().Sum(pm => pm.GetScienceCount());
        }
        part.Dispose();
        return count;
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage($" in GetScienceCount.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}",
          SmUtils.LogType.Error, true);
        return 0;
      }
    }

    internal static string GetVesselResourceTotals(ModDockedVessel modDockedVessel, List<string> selectedResources)
    {
      double currAmount = 0;
      double totAmount = 0;
      try
      {
        List<ModDockedVessel> modDockedVessels = new List<ModDockedVessel> { modDockedVessel };
        if (selectedResources.Contains(SMConditions.ResourceType.Crew.ToString()))
        {
          currAmount = SmUtils.GetPartsCrewCount(modDockedVessel.VesselParts);
          totAmount = SmUtils.GetPartsCrewCapacity(modDockedVessel.VesselParts);
        }
        else if (selectedResources.Contains(SMConditions.ResourceType.Science.ToString()))
        {
          currAmount = GetScienceCount(modDockedVessel);
          totAmount = currAmount;
        }
        else
        {
          List<Part>.Enumerator parts = SMAddon.SmVessel.GetVesselsPartsByResource(modDockedVessels, selectedResources).GetEnumerator();
          while (parts.MoveNext())
          {
            if (parts.Current == null) continue;
            currAmount += parts.Current.Resources[selectedResources[0]].amount;
            totAmount += parts.Current.Resources[selectedResources[0]].maxAmount;
          }
          parts.Dispose();          
        }
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage($" in DisplayVesselResourceTotals().  Error:  {ex}", SmUtils.LogType.Error, true);
      }
      string displayAmount = $"({currAmount:#######0}/{totAmount:######0})";

      return displayAmount;
    }

    private static bool IsPartSelectable(string selectedResource, TransferPump.TypeXfer xferType, Part part)
    {
      if (selectedResource == SMConditions.ResourceType.Crew.ToString() ||
          selectedResource == SMConditions.ResourceType.Science.ToString()) return true;
      bool isSelectable = true;
      if (xferType == TransferPump.TypeXfer.SourceToTarget)
      {
        if (SMAddon.SmVessel.SelectedPartsTarget.Contains(part))
          isSelectable = false;
      }
      else
      {
        if (SMAddon.SmVessel.SelectedPartsSource.Contains(part))
          isSelectable = false;
      }
      return isSelectable;
    }

    private static int GetAvailableCrewSpace(List<Part> parts)
    {
      int results = 0;
      List<Part>.Enumerator part = parts.GetEnumerator();
      while (part.MoveNext())
      {
        if (part.Current == null) continue;
        results += part.Current.CrewCapacity - part.Current.protoModuleCrew.Count;
      }
      part.Dispose();
      return results;
    }

    #endregion
  }
}
