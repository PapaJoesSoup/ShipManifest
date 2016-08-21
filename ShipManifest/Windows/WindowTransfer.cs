using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ShipManifest.APIClients;
using ShipManifest.InternalObjects;
using ShipManifest.Modules;
using ShipManifest.Process;
using UnityEngine;

namespace ShipManifest.Windows
{
  internal static class WindowTransfer
  {
    #region Properties

    internal static string Title = "";
    internal static Rect Position = new Rect(0, 0, 0, 0);
    internal static bool ShowWindow;
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;

    internal static string ToolTip = "";
    internal static string XferToolTip = "";
    internal static string EvaToolTip = "";

    // Switches for List Viewers
    internal static bool ShowSourceVessels;
    internal static bool ShowTargetVessels;

    // this list is for display use.  Transfers are executed against a separate list.  
    // These objects may be used to derive objects to be added to the transfer process queue.
    internal static List<TransferPump> DisplayPumps = new List<TransferPump>();

    private static Dictionary<PartModule, bool> _scienceModulesSource;
    internal static Dictionary<PartModule, bool> ScienceModulesSource
    {
      get
      {
        if (_scienceModulesSource != null) return _scienceModulesSource;
        _scienceModulesSource = new Dictionary<PartModule, bool>();
        if (SMAddon.SmVessel.SelectedPartsSource.Count <= 0) return _scienceModulesSource;
        IScienceDataContainer[] modules = SMAddon.SmVessel.SelectedPartsSource[0].FindModulesImplementing<IScienceDataContainer>().ToArray();
        if (modules.Length <= 0) return _scienceModulesSource;
        IEnumerator sourceModules = modules.GetEnumerator();
        while (sourceModules.MoveNext())
        {
          if (sourceModules.Current == null) continue;
          PartModule pm = (PartModule) sourceModules.Current;
          _scienceModulesSource.Add(pm, false);
        }
        return _scienceModulesSource;
      }
    }

    #endregion

    #region TransferWindow (GUI Layout)

    // Resource Transfer Window
    // This window allows you some control over the selected resource on a selected source and target part
    // This window assumes that a resource has been selected on the Ship manifest window.
    internal static void Display(int windowId)
    {
      string displayAmounts = Utilities.DisplayVesselResourceTotals(SMAddon.SmVessel.SelectedResources[0]);
      Title = "Transfer - " + SMAddon.SmVessel.Vessel.vesselName + displayAmounts;

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
        ShowWindow = false;
        SMAddon.SmVessel.SelectedResources.Clear();
        SMAddon.SmVessel.SelectedPartsSource.Clear();
        SMAddon.SmVessel.SelectedPartsTarget.Clear();

        SMAddon.SmVessel.SelectedVesselsSource.Clear();
        SMAddon.SmVessel.SelectedVesselsTarget.Clear();
        ToolTip = "";
        return;
      }
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      // Update the pumps for this event.
      //TransferPump.UpdateDisplayPumps(false);

      GUI.enabled = true;
      try
      {
        // This window assumes that a resource has been selected on the Ship manifest window.
        GUILayout.BeginHorizontal();
        //Left Column Begins
        GUILayout.BeginVertical();

        // Build source Transfer Viewer
        SourceTransferViewer();

        // Text above Source Details. (Between viewers)
        TextBetweenViewers(SMAddon.SmVessel.SelectedPartsSource, TransferPump.TypePump.SourceToTarget);

        // Build Details ScrollViewer
        SourceDetailsViewer();

        // Okay, we are done with the left column of the dialog...
        GUILayout.EndVertical();

        // Right Column Begins...
        GUILayout.BeginVertical();

        // Build Target Transfer Viewer
        TargetTransferViewer();

        // Text between viewers
        TextBetweenViewers(SMAddon.SmVessel.SelectedPartsTarget, TransferPump.TypePump.TargetToSource);

        // Build Target details Viewer
        TargetDetailsViewer();

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
        SMAddon.RepositionWindow(ref Position);
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(
          string.Format(" in Ship Manifest Window.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), Utilities.LogType.Error, true);
      }
    }

    private static void TextBetweenViewers(IList<Part> selectedParts, TransferPump.TypePump pumpType)
    {
      string labelText = "";
      GUILayout.BeginHorizontal();
      if (SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Crew.ToString()))
        labelText = selectedParts.Count > 0 ? string.Format("{0}", selectedParts[0].partInfo.title) : "No Part Selected";
      else
      {
        if (selectedParts != null)
        {
          if (selectedParts.Count > 1)
            labelText = string.Format("{0}", "Multiple Parts Selected");
          else if (selectedParts.Count == 1)
            labelText = string.Format("{0}", selectedParts[0].partInfo.title);
          else
            labelText = string.Format("{0}", "No Part Selected");
        }
      }
      GUILayout.Label(labelText, SMStyle.LabelStyleNoWrap, GUILayout.Width(200));
      if (CanShowVessels())
      {
        if (pumpType == TransferPump.TypePump.SourceToTarget)
        {
          bool prevValue = ShowSourceVessels;
          ShowSourceVessels = GUILayout.Toggle(ShowSourceVessels, "Vessels", GUILayout.Width(90));
          if (!prevValue && ShowSourceVessels)
            WindowManifest.ResolveResourcePartSelections(SMAddon.SmVessel.SelectedResources);
        }
        else
        {
          bool prevValue = ShowSourceVessels;
          ShowTargetVessels = GUILayout.Toggle(ShowTargetVessels, "Vessels", GUILayout.Width(90));
          if (!prevValue && ShowSourceVessels)
            WindowManifest.ResolveResourcePartSelections(SMAddon.SmVessel.SelectedResources);
        }
      }
      GUILayout.EndHorizontal();
    }

    #region Source Viewers (GUI Layout)

    // Transfer Window components
    private static Vector2 _sourceTransferViewerScrollPosition = Vector2.zero;

    internal static void SourceTransferViewer()
    {
      try
      {
        // This is a scroll panel (we are using it to make button lists...)
        _sourceTransferViewerScrollPosition = GUILayout.BeginScrollView(_sourceTransferViewerScrollPosition,
          SMStyle.ScrollStyle, GUILayout.Height(100), GUILayout.Width(300));
        GUILayout.BeginVertical();

        if (ShowSourceVessels && CanShowVessels())
          VesselTransferViewer(SMAddon.SmVessel.SelectedResources, TransferPump.TypePump.SourceToTarget,
            _sourceTransferViewerScrollPosition);
        else
          PartsTransferViewer(SMAddon.SmVessel.SelectedResources, TransferPump.TypePump.SourceToTarget,
            _sourceTransferViewerScrollPosition);

        GUILayout.EndVertical();
        GUILayout.EndScrollView();
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(
          string.Format(" in Ship Manifest Window - SourceTransferViewer.  Error:  {0} \r\n\r\n{1}", ex.Message,
            ex.StackTrace), Utilities.LogType.Error, true);
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
          SMStyle.ScrollStyle, GUILayout.Height(120), GUILayout.Width(300));
        GUILayout.BeginVertical();

        if (SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Crew.ToString()))
        {
          CrewDetails(SMAddon.SmVessel.SelectedPartsSource, SMAddon.SmVessel.SelectedPartsTarget);
        }
        else if (SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Science.ToString()))
        {
          ScienceDetailsSource();
        }
        else
        {
          // Other resources are left....
          ResourceDetailsViewer(TransferPump.TypePump.SourceToTarget);
        }
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(
          string.Format(" in WindowTransfer.SourceDetailsViewer.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace),
          Utilities.LogType.Error, true);
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
        if (SMSettings.EnableClsHighlighting &&
            SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Crew.ToString()))
          SMStyle.ButtonToggledTargetStyle.normal.textColor = SMSettings.Colors[SMSettings.TargetPartCrewColor];
        else
          SMStyle.ButtonToggledTargetStyle.normal.textColor = SMSettings.Colors[SMSettings.TargetPartColor];

        // This is a scroll panel (we are using it to make button lists...)
        _targetTransferViewerScrollPosition = GUILayout.BeginScrollView(_targetTransferViewerScrollPosition,
          SMStyle.ScrollStyle, GUILayout.Height(100), GUILayout.Width(300));
        GUILayout.BeginVertical();

        if (ShowTargetVessels && CanShowVessels())
          VesselTransferViewer(SMAddon.SmVessel.SelectedResources, TransferPump.TypePump.TargetToSource,
            _targetTransferViewerScrollPosition);
        else
          PartsTransferViewer(SMAddon.SmVessel.SelectedResources, TransferPump.TypePump.TargetToSource,
            _targetTransferViewerScrollPosition);

        GUILayout.EndVertical();
        GUILayout.EndScrollView();
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(
          string.Format(" in Ship Manifest Window - TargetTransferViewer.  Error:  {0} \r\n\r\n{1}", ex.Message,
            ex.StackTrace), Utilities.LogType.Error, true);
      }
    }

    private static Vector2 _targetDetailsViewerScrollPosition = Vector2.zero;

    private static void TargetDetailsViewer()
    {
      try
      {
        // Target Part resource details
        _targetDetailsViewerScrollPosition = GUILayout.BeginScrollView(_targetDetailsViewerScrollPosition,
          SMStyle.ScrollStyle, GUILayout.Height(120), GUILayout.Width(300));
        GUILayout.BeginVertical();

        // --------------------------------------------------------------------------
        if (SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Crew.ToString()))
        {
          CrewDetails(SMAddon.SmVessel.SelectedPartsTarget, SMAddon.SmVessel.SelectedPartsSource);
        }
        else if (SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Science.ToString()))
        {
          ScienceDetailsTarget();
        }
        else
        {
          ResourceDetailsViewer(TransferPump.TypePump.TargetToSource);
        }
        // --------------------------------------------------------------------------
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(
          string.Format(" in WindowTransfer.TargetDetailsViewer.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace),
          Utilities.LogType.Error, true);
      }
    }

    #endregion

    #region Viewer Details (GUI Layout)

    private static void PartsTransferViewer(List<string> selectedResources, TransferPump.TypePump pumpType,
      Vector2 viewerScrollPosition)
    {
      float scrollX = Position.x + (pumpType == TransferPump.TypePump.SourceToTarget ? 20 : 320);
      float scrollY = Position.y + 30 - viewerScrollPosition.y;
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
            btnWidth = !SMSettings.RealismMode || SMSettings.EnablePfResources ? 173 : 223;
          else if (selectedResources.Contains(SMConditions.ResourceType.Crew.ToString()) && SMConditions.CanShowCrewFillDumpButtons())
            btnWidth = 173;

          // Set style based on viewer and toggled state.
          step = "Set style";
          GUIStyle style = GetPartButtonStyle(pumpType, parts.Current);

          GUILayout.BeginHorizontal();

          // Now let's account for any target buttons already pressed. (sources and targets for resources cannot be the same)
          GUI.enabled = IsPartSelectable(selectedResources[0], pumpType, parts.Current);

          step = "Render part Buttons";
          if (GUILayout.Button(strDescription, style, GUILayout.Width(btnWidth), GUILayout.Height(20)))
          {
            PartButtonToggled(pumpType, parts.Current);
          }
          Rect rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && rect.Contains(Event.current.mousePosition))
          {
            SMHighlighter.IsMouseOver = true;
            SMHighlighter.MouseOverMode = pumpType;
            SMHighlighter.MouseOverRect = new Rect(scrollX + rect.x, scrollY + rect.y, rect.width, rect.height);
            SMHighlighter.MouseOverpart = parts.Current;
          }

          // Reset Button enabling.
          GUI.enabled = true;

          step = "Render dump/fill buttons";
          if (selectedResources.Contains(SMConditions.ResourceType.Crew.ToString()))
          {
            if (SMConditions.CanShowCrewFillDumpButtons())
            CrewFillDumpButtons(parts.Current);
          }
          if (SMConditions.AreSelectedResourcesTypeOther(selectedResources))
          {
            ResourceDumpFillButtons(selectedResources, pumpType, parts.Current);
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
            "Error in Windowtransfer.PartsTransferViewer (" + pumpType + ") at step:  " + step + ".  Error:  " + ex,
            Utilities.LogType.Error, true);
          SMAddon.FrameErrTripped = true;
        }
      }
    }

    private static void ResourceDumpFillButtons(List<string> selectedResources, TransferPump.TypePump pumpType, Part part)
    {
      uint pumpId = TransferPump.GetPumpIdFromHash(string.Join("", selectedResources.ToArray()), part, part,
        pumpType, TransferPump.TriggerButton.Transfer);
      GUIContent dumpContent = !TransferPump.IsPumpInProgress(pumpId)
        ? new GUIContent("Dump", "Dumps the selected resource in this vessel")
        : new GUIContent("Stop", "Halts the dumping of the selected resource in this vessel");
      GUIStyle style1 = pumpType == TransferPump.TypePump.SourceToTarget
        ? SMStyle.ButtonSourceStyle
        : SMStyle.ButtonTargetStyle;
      GUI.enabled = CanDumpPart(part);

      if (GUILayout.Button(dumpContent, style1, GUILayout.Width(45), GUILayout.Height(20)))
      {
        SMPart.ToggleDumpResource(part, selectedResources, pumpId);
      }
      Rect rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      GUIStyle style2 = pumpType == TransferPump.TypePump.SourceToTarget
        ? SMStyle.ButtonSourceStyle
        : SMStyle.ButtonTargetStyle;
      // Fills should only be in Non Realism mode or if Preflight Resources are seleced...
      if (SMSettings.RealismMode && !SMSettings.EnablePfResources) return;
      if (selectedResources.Count > 1)
        GUI.enabled = part.Resources[selectedResources[0]].amount <
                      part.Resources[selectedResources[0]].maxAmount ||
                      part.Resources[selectedResources[1]].amount <
                      part.Resources[selectedResources[1]].maxAmount;
      else
        GUI.enabled = part.Resources[selectedResources[0]].amount <
                      part.Resources[selectedResources[0]].maxAmount;
      if (GUILayout.Button("Fill", style2, GUILayout.Width(30), GUILayout.Height(20)))
      {
        SMPart.FillResource(part, selectedResources[0]);
        if (selectedResources.Count > 1)
          SMPart.FillResource(part, selectedResources[1]);
      }
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);
    }

    private static void VesselTransferViewer(List<string> selectedResources, TransferPump.TypePump pumpType,
      Vector2 viewerScrollPosition)
    {
      float scrollX = Position.x + (pumpType == TransferPump.TypePump.SourceToTarget ? 20 : 320);
      float scrollY = Position.y + 30 - viewerScrollPosition.y;
      string step = "begin";
      try
      {
        step = "begin button loop";
        List<ModDockedVessel>.Enumerator modDockedVessels = SMAddon.SmVessel.DockedVessels.GetEnumerator();
        while (modDockedVessels.MoveNext())
        {
          if (modDockedVessels.Current == null) continue;
          // Build the part button title...
          step = "vessel button title";
          string strDescription = GetResourceDescription(selectedResources, modDockedVessels.Current);

          // set the conditions for a button style change.
          int btnWidth = 265;
          if (!SMSettings.RealismMode)
            btnWidth = 180;

          // Set style based on viewer and toggled state.
          step = "Set style";
          GUIStyle style = GetVesselButtonStyle(pumpType, modDockedVessels.Current);

          GUILayout.BeginHorizontal();

          // Now let's account for any target buttons already pressed. (sources and targets for resources cannot be the same)
          GUI.enabled = CanSelectVessel(pumpType, modDockedVessels.Current);

          step = "Render vessel Buttons";
          if (GUILayout.Button(string.Format("{0}", strDescription), style, GUILayout.Width(btnWidth),
            GUILayout.Height(20)))
          {
            VesselButtonToggled(pumpType, modDockedVessels.Current);
          }
          Rect rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && rect.Contains(Event.current.mousePosition))
          {
            SMHighlighter.IsMouseOver = true;
            SMHighlighter.MouseOverMode = pumpType;
            SMHighlighter.MouseOverRect = new Rect(scrollX + rect.x, scrollY + rect.y, rect.width, rect.height);
            SMHighlighter.MouseOverpart = null;
            SMHighlighter.MouseOverparts = modDockedVessels.Current.VesselParts;
          }

          // Reset Button enabling.
          GUI.enabled = true;

          //step = "Render dump/fill buttons";
          if (!SMSettings.RealismMode)
          {
            if (selectedResources.Count > 1)
              GUI.enabled = TransferPump.CalcRemainingResource(modDockedVessels.Current.VesselParts, selectedResources[0]) > 0 ||
                            TransferPump.CalcRemainingResource(modDockedVessels.Current.VesselParts, selectedResources[1]) > 0;
            else
              GUI.enabled = TransferPump.CalcRemainingResource(modDockedVessels.Current.VesselParts, selectedResources[0]) > 0;
            GUIStyle style1 = pumpType == TransferPump.TypePump.SourceToTarget
              ? SMStyle.ButtonSourceStyle
              : SMStyle.ButtonTargetStyle;
            uint pumpId = TransferPump.GetPumpIdFromHash(string.Join("", selectedResources.ToArray()),
              modDockedVessels.Current.VesselParts.First(), modDockedVessels.Current.VesselParts.Last(), pumpType,
              TransferPump.TriggerButton.Transfer);
            GUIContent dumpContent = !TransferPump.IsPumpInProgress(pumpId)
              ? new GUIContent("Dump", "Dumps the selected resource in this Part")
              : new GUIContent("Stop", "Halts the dumping of the selected resource in this part");
            if (GUILayout.Button(dumpContent, style1, GUILayout.Width(45), GUILayout.Height(20)))
            {
              SMPart.ToggleDumpResource(modDockedVessels.Current.VesselParts, selectedResources, pumpId);
            }

            GUIStyle style2 = pumpType == TransferPump.TypePump.SourceToTarget
              ? SMStyle.ButtonSourceStyle
              : SMStyle.ButtonTargetStyle;
            if (selectedResources.Count > 1)
              GUI.enabled = TransferPump.CalcRemainingCapacity(modDockedVessels.Current.VesselParts, selectedResources[0]) > 0 ||
                            TransferPump.CalcRemainingCapacity(modDockedVessels.Current.VesselParts, selectedResources[0]) > 0;
            else
              GUI.enabled = TransferPump.CalcRemainingCapacity(modDockedVessels.Current.VesselParts, selectedResources[0]) > 0;
            if (GUILayout.Button("Fill", style2, GUILayout.Width(30), GUILayout.Height(20)))
            {
              SMPart.FillResource(modDockedVessels.Current.VesselParts, selectedResources[0]);
              if (selectedResources.Count > 1)
                SMPart.FillResource(modDockedVessels.Current.VesselParts, selectedResources[1]);
            }
            GUI.enabled = true;
          }
          GUILayout.EndHorizontal();
        }
      }
      catch (Exception ex)
      {
        if (!SMAddon.FrameErrTripped)
        {
          Utilities.LogMessage(
            "Error in Windowtransfer.VesselTransferViewer (" + pumpType + ") at step:  " + step + ".  Error:  " + ex,
            Utilities.LogType.Error, true);
          SMAddon.FrameErrTripped = true;
        }
      }
    }

    private static void CrewFillDumpButtons(Part part)
    {
      GUIContent dumpContent = new GUIContent("Dump", "Removes any crew members in this part");
      GUIContent fillContent = new GUIContent("Fill", "Fills this part with crew members");

      GUI.enabled = part.protoModuleCrew.Count > 0;
      if (GUILayout.Button(dumpContent, GUILayout.Width(45), GUILayout.Height(20)))
      {
        SMPart.DumpCrew(part);
      }
      Rect rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      GUI.enabled = part.protoModuleCrew.Count < part.CrewCapacity;
      if (GUILayout.Button(fillContent, GUILayout.Width(45), GUILayout.Height(20)))
      {
        SMPart.FillCrew(part);
      }
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

    }
    private static void CrewDetails(List<Part> selectedPartsFrom, List<Part> selectedPartsTo)
    {
      // Since only one Crew Part can currently be selected, all lists will use an index of [0].
      float xOffset = 30;

      if (selectedPartsFrom.Count <= 0) return;
      // ReSharper disable once ForCanBeConvertedToForeach
      for (int x = 0; x < selectedPartsFrom[0].protoModuleCrew.Count; x++)
      {
        ProtoCrewMember crewMember = selectedPartsFrom[0].protoModuleCrew[x];
        GUILayout.BeginHorizontal();
        if (SMConditions.IsTransferInProgress()) GUI.enabled = false;

        if (GUILayout.Button(new GUIContent("»", "Move Kerbal to another seat within Part"), SMStyle.ButtonStyle,
          GUILayout.Width(25), GUILayout.Height(20)))
        {
          ToolTip = "";
          SMAddon.SmVessel.TransferCrewObj.CrewTransferBegin(crewMember, selectedPartsFrom[0], selectedPartsFrom[0]);
        }
        if (Event.current.type == EventType.Repaint && ShowToolTips)
        {
          Rect rect = GUILayoutUtility.GetLastRect();
          ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);
        }
        GUI.enabled = true;
        GUILayout.Label(string.Format("  {0}", crewMember.name + " (" + crewMember.experienceTrait.Title + ")"),
          GUILayout.Width(190), GUILayout.Height(20));
        GUI.enabled = SMConditions.CanKerbalsBeXferred(selectedPartsFrom, selectedPartsTo);
        if ((SMAddon.SmVessel.TransferCrewObj.FromCrewMember == crewMember ||
             SMAddon.SmVessel.TransferCrewObj.ToCrewMember == crewMember) && SMConditions.IsTransferInProgress())
        {
          GUI.enabled = true;
          GUILayout.Label("Moving", GUILayout.Width(50), GUILayout.Height(20));
        }
        else if (!SMConditions.IsClsInSameSpace())
        {
          GUI.enabled = true;
          if (GUILayout.Button(new GUIContent("EVA", EvaToolTip), SMStyle.ButtonStyle, GUILayout.Width(50),
            GUILayout.Height(20)))
          {
            ToolTip = "";
            FlightEVA.SpawnEVA(crewMember.KerbalRef); 
          }
          if (Event.current.type == EventType.Repaint && ShowToolTips)
          {
            Rect rect = GUILayoutUtility.GetLastRect();
            ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);
          }
        }
        else
        {
          if (GUILayout.Button(new GUIContent("Xfer", XferToolTip), SMStyle.ButtonStyle, GUILayout.Width(50),
            GUILayout.Height(20)))
          {
            SMAddon.SmVessel.TransferCrewObj.FromCrewMember = crewMember;
            SMAddon.SmVessel.TransferCrewObj.CrewTransferBegin(crewMember, selectedPartsFrom[0], selectedPartsTo[0]);
          }
          if (Event.current.type == EventType.Repaint && ShowToolTips)
          {
            Rect rect = GUILayoutUtility.GetLastRect();
            ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);
          }
        }
        GUI.enabled = true;
        GUILayout.EndHorizontal();
      }
      // Cater for DeepFreeze Continued... parts - list frozen kerbals
      if (!InstalledMods.IsDfApiReady) return;
      try
      {
        PartModule deepFreezer = (from PartModule pm in selectedPartsFrom[0].Modules where pm.moduleName == "DeepFreezer" select pm).SingleOrDefault();
        if (deepFreezer == null) return;
        DFWrapper.DeepFreezer sourcepartFrzr = new DFWrapper.DeepFreezer(deepFreezer);
        if (sourcepartFrzr.StoredCrewList.Count <= 0) return;
        //Dictionary<string, DFWrapper.KerbalInfo> frozenKerbals = DFWrapper.DeepFreezeAPI.FrozenKerbals;
        List<DFWrapper.FrznCrewMbr>.Enumerator frznCrew = sourcepartFrzr.StoredCrewList.GetEnumerator();
        while (frznCrew.MoveNext())
        {
          if (frznCrew.Current == null) continue;
          GUILayout.BeginHorizontal();
          GUI.enabled = false;
          if (GUILayout.Button(new GUIContent("»", "Move Kerbal to another seat within Part"), SMStyle.ButtonStyle,
            GUILayout.Width(15), GUILayout.Height(20)))
          {
            ToolTip = "";
          }
          if (Event.current.type == EventType.Repaint && ShowToolTips)
          {
            Rect rect = GUILayoutUtility.GetLastRect();
            ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);
          }
          string trait = "";
          ProtoCrewMember frozenKerbal = FindFrozenKerbal(frznCrew.Current.CrewName);
          if (frozenKerbal != null) trait = frozenKerbal.trait;
          GUI.enabled = true;
          GUILayout.Label(string.Format("  {0}", frznCrew.Current.CrewName + " (" + trait + ")"), SMStyle.LabelStyleCyan,
            GUILayout.Width(190), GUILayout.Height(20));

          if (GUILayout.Button(new GUIContent("Thaw", "This Kerbal is Frozen. Click to Revive kerbal"),
            SMStyle.ButtonStyle, GUILayout.Width(50), GUILayout.Height(20)))
          {
            WindowRoster.ThawKerbal(frznCrew.Current.CrewName);
            ToolTip = "";
          }
          if (Event.current.type == EventType.Repaint && ShowToolTips)
          {
            Rect rect = GUILayoutUtility.GetLastRect();
            ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);
          }
          GUILayout.EndHorizontal();
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(
          string.Format(" in WindowTransfer.CrewDetails.  Error attempting to check DeepFreeze for FrozenKerbals.  Error:  {0} \r\n\r\n{1}",
            ex.Message, ex.StackTrace), Utilities.LogType.Error, true);
        //Debug.Log("Error attempting to check DeepFreeze for FrozenKerbals");
        //Debug.Log(ex.Message);
      }
    }

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
      return retval;
    }

    private static void ScienceDetailsSource()
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
        string toolTip = "Expand/Collapse Science detail.";
        if (!GUI.enabled)
          toolTip += " (Disabled, nothing to xfer)";
        GUIStyle expandStyle = ScienceModulesSource[modules.Current] ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
        if (ScienceModulesSource[modules.Current])
          label = "-";
        if (GUILayout.Button(new GUIContent(label, toolTip), expandStyle, GUILayout.Width(15), GUILayout.Height(20)))
        {
          ScienceModulesSource[modules.Current] = !ScienceModulesSource[modules.Current];
        }
        if (Event.current.type == EventType.Repaint && ShowToolTips)
        {
          Rect rect = GUILayoutUtility.GetLastRect();
          ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);
        }
        GUI.enabled = true;
        GUILayout.Label(string.Format("{0} - ({1})", modules.Current.moduleName, scienceCount), GUILayout.Width(205),
          GUILayout.Height(20));

        // If we have target selected, it is not the same as the source, there is science to xfer.
        if (SMAddon.SmVessel.SelectedModuleTarget != null && scienceCount > 0)
        {
          if (SMSettings.RealismMode && !isCollectable)
          {
            GUI.enabled = false;
            toolTip = "Realism Mode is preventing transfer.\r\nExperiment/data is marked not transferable";
          }
          else
          {
            GUI.enabled = true;
            toolTip = "Realism is off, or Experiment/data is transferable";
          }
          if (GUILayout.Button(new GUIContent("Xfer", toolTip), SMStyle.ButtonStyle, GUILayout.Width(40),
            GUILayout.Height(20)))
          {
            SMAddon.SmVessel.SelectedModuleSource = modules.Current;
            ProcessController.TransferScience(SMAddon.SmVessel.SelectedModuleSource,
              SMAddon.SmVessel.SelectedModuleTarget);
            SMAddon.SmVessel.SelectedModuleSource = null;
          }
          if (Event.current.type == EventType.Repaint && ShowToolTips)
          {
            Rect rect = GUILayoutUtility.GetLastRect();
            ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);
          }
          if (GUI.enabled && SMAddon.SmVessel.Vessel.FindPartModulesImplementing<ModuleScienceLab>().Count > 0)
          {
            if (GUILayout.Button(new GUIContent("Proc", "Transfer only science that was already processed"), SMStyle.ButtonStyle, GUILayout.Width(40),
                                 GUILayout.Height(20)))
            {
              SMAddon.SmVessel.SelectedModuleSource = modules.Current;
              ProcessController.TransferScienceLab(SMAddon.SmVessel.SelectedModuleSource,
                                                   SMAddon.SmVessel.SelectedModuleTarget,
                                             ProcessController.Selection.OnlyProcessed);
              SMAddon.SmVessel.SelectedModuleSource = null;
            }
            if (Event.current.type == EventType.Repaint && ShowToolTips)
            {
              Rect rect = GUILayoutUtility.GetLastRect();
              ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);
            }
            if (GUILayout.Button(new GUIContent("Unproc", "Transfer only science that was not processed yet"), SMStyle.ButtonStyle, GUILayout.Width(50),
                                 GUILayout.Height(20)))
            {
              SMAddon.SmVessel.SelectedModuleSource = modules.Current;
              ProcessController.TransferScienceLab(SMAddon.SmVessel.SelectedModuleSource,
                                                   SMAddon.SmVessel.SelectedModuleTarget,
                                             ProcessController.Selection.OnlyUnprocessed);
              SMAddon.SmVessel.SelectedModuleSource = null;
            }
            if (Event.current.type == EventType.Repaint && ShowToolTips)
            {
              Rect rect = GUILayoutUtility.GetLastRect();
              ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);
            }
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
            //toolTip += "\r\n-SubjectID:   " + item.subjectID;
            toolTip += "\r\n-Results:    " + results;
            toolTip += "\r\n-Data Amt:   " + ((ScienceData)items.Current).dataAmount + " Mits";
            toolTip += "\r\n-Xmit Value: " + ((ScienceData)items.Current).transmitValue;
            toolTip += "\r\n-Lab Value:  " + ((ScienceData)items.Current).labValue;
            toolTip += "\r\n-Lab Boost:  " + ((ScienceData)items.Current).labBoost;

            GUILayout.Label(new GUIContent(se.experimentTitle, toolTip), SMStyle.LabelStyleNoWrap, GUILayout.Width(205),
              GUILayout.Height(20));
            if (Event.current.type == EventType.Repaint && ShowToolTips)
            {
              Rect rect = GUILayoutUtility.GetLastRect();
              ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);
            }
            if (SMSettings.RealismMode && !isCollectable)
            {
              GUI.enabled = false;
              toolTip = "Realism Mode is preventing transfer.\r\nData is marked not transferable";
            }
            else
            {
              toolTip = "Realism is off, or Data is transferable";
              GUI.enabled = true;
            }
            if (SMAddon.SmVessel.SelectedModuleTarget != null && scienceCount > 0)
            {
              if (GUILayout.Button(new GUIContent("Xfer", toolTip), SMStyle.ButtonStyle, GUILayout.Width(40),
                GUILayout.Height(20)))
              {
                if (((ModuleScienceContainer)SMAddon.SmVessel.SelectedModuleTarget).AddData(((ScienceData)items.Current)))
                  ((IScienceDataContainer)modules.Current).DumpData(((ScienceData)items.Current));
              }
              if (Event.current.type == EventType.Repaint && ShowToolTips)
              {
                Rect rect = GUILayoutUtility.GetLastRect();
                ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);
              }
            }
            GUILayout.EndHorizontal();
          }
        }
        GUI.enabled = true;
      }
    }

    private static void ScienceDetailsTarget()
    {
      float xOffset = 30;
      if (SMAddon.SmVessel.SelectedPartsTarget.Count <= 0) return;
      int count =
        SMAddon.SmVessel.SelectedPartsTarget[0].Modules.Cast<PartModule>()
          .Count(tpm => tpm is IScienceDataContainer && tpm.moduleName != "ModuleScienceExperiment");

      IEnumerator modules = SMAddon.SmVessel.SelectedPartsTarget[0].Modules.GetEnumerator();
      while (modules.MoveNext())
      {
        if (modules.Current == null) continue;
        // Containers.
        if (!(modules.Current is IScienceDataContainer) || ((PartModule)modules.Current).moduleName == "ModuleScienceExperiment") continue;
        int scienceCount = ((IScienceDataContainer)modules.Current).GetScienceCount();
        GUILayout.BeginHorizontal();
        GUILayout.Label(string.Format("{0} - ({1})", ((PartModule)modules.Current).moduleName, scienceCount), GUILayout.Width(220),
          GUILayout.Height(20));
        // set the conditions for a button style change.
        bool isReceiveToggled = false;
        if ((PartModule)modules.Current == SMAddon.SmVessel.SelectedModuleTarget)
          isReceiveToggled = true;
        else if (count == 1)
        {
          SMAddon.SmVessel.SelectedModuleTarget = (PartModule)modules.Current;
          isReceiveToggled = true;
        }
        //SelectedModuleTarget = pm;
        GUIStyle style = isReceiveToggled ? SMStyle.ButtonToggledTargetStyle : SMStyle.ButtonStyle;

        // Only containers can receive science data
        if (((PartModule)modules.Current).moduleName != "ModuleScienceExperiment")
        {
          if (GUILayout.Button(new GUIContent("Recv", "Set this module as the receiving container"), style,
            GUILayout.Width(40), GUILayout.Height(20)))
          {
            SMAddon.SmVessel.SelectedModuleTarget = (PartModule)modules.Current;
          }
          if (Event.current.type == EventType.Repaint && ShowToolTips)
          {
            Rect rect = GUILayoutUtility.GetLastRect();
            ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);
          }
        }
        GUILayout.EndHorizontal();
      }
    }

    private static void ResourceDetailsViewer(TransferPump.TypePump pumpType)
    {
      // Let's get the pump objects we may use...
      List<string> selectedResources = SMAddon.SmVessel.SelectedResources;
      List<TransferPump> pumps = TransferPump.GetDisplayPumpsByType(pumpType);
      if (!(from pump in pumps where pump.FromParts.Count > 0 select pump).Any()) return;

      // This routine assumes that a resource has been selected on the Resource manifest window.
      // Set scrollX offsets for left and right viewers
      int xOffset = 30;
      string toolTip = "";

      TransferPump activePump = TransferPump.GetRatioPump(pumps);
      TransferPump ratioPump = TransferPump.GetRatioPump(pumps, true);
      activePump.PumpRatio = 1;
      ratioPump.PumpRatio = TransferPump.CalcRatio(pumps);

      // Set tooltips directional data
      string strTarget = pumpType == TransferPump.TypePump.SourceToTarget ? "Target" : "Source";


      // Resource Flow control Display loop
      ResourceFlowButtons(pumpType, xOffset);

      // Xfer Controls Display
      // let's determine how much of a resource we can move to the target.
      double thisXferAmount = double.Parse(activePump.EditSliderAmount);
      double maxPumpAmount = TransferPump.CalcMaxPumpAmt(pumps[0].FromParts, pumps[0].ToParts, selectedResources);
      if (maxPumpAmount <= 0 && !TransferPump.PumpProcessOn) return;
      GUILayout.BeginHorizontal();
      string label;
      Rect rect;
      if (TransferPump.PumpProcessOn)
      {
        // We want to show this during transfer if the direction is correct...
        if (activePump.PumpType == pumpType)
        {
          GUILayout.Label("Xfer Remaining:", GUILayout.Width(120));
          GUILayout.Label(activePump.PumpBalance.ToString("#######0.##"));
          if (SMAddon.SmVessel.SelectedResources.Count > 1)
            GUILayout.Label(" | " + ratioPump.PumpBalance.ToString("#######0.##"));
        }
      }
      else
      {
        if (selectedResources.Count > 1)
        {
          label = "Xfer Amts:";
          toolTip = "Displays xfer amounts of both resourses selected.";
          toolTip += "\r\nAllows editing of part's larger capacity resourse xfer value.";
          toolTip += "\r\nIt then calculates the smaller xfer amount using a ratio";
          toolTip += "\r\n of the smaller capacity resource to the larger.";
        }
        else
        {
          label = "Xfer Amt:";
          toolTip += "Displays the Amount of selected resource to xfer.";
          toolTip += "\r\nAllows editing of the xfer value.";
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
          label = " | " + ratioXferAmt.ToString("#######0.##");
          toolTip = "Smaller Tank xfer amount.  Calculated at " + ratioPump.PumpRatio +
                    ".\r\n(Note: A value of 0.818181 = 0.9/1.1)";
          GUILayout.Label(new GUIContent(label, toolTip), GUILayout.Width(80));
          rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && ShowToolTips)
            ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);
        }
      }
      GUILayout.EndHorizontal();

      if (SMConditions.IsShipControllable() && SMConditions.CanResourceBeXferred(pumpType, maxPumpAmount))
      {
        GUILayout.BeginHorizontal();
        GUIStyle noPad = SMStyle.LabelStyleNoPad;
        label = "Xfer:";
        toolTip =
          "Xfer amount slider control.\r\nMove slider to select a different value.\r\nYou can use this instead of the text box above.";
        GUILayout.Label(new GUIContent(label, toolTip), noPad, GUILayout.Width(50), GUILayout.Height(20));
        rect = GUILayoutUtility.GetLastRect();
        if (Event.current.type == EventType.Repaint && ShowToolTips)
          ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, xOffset);
        thisXferAmount = GUILayout.HorizontalSlider((float)thisXferAmount, 0, (float)maxPumpAmount,
          GUILayout.Width(190));
        activePump.EditSliderAmount = thisXferAmount.ToString(CultureInfo.InvariantCulture);
        // set Xfer button style
        GUIContent xferContent = !TransferPump.PumpProcessOn
          ? new GUIContent("Xfer", "Transfers the selected resource(s)\r\nto the selected " + strTarget + " Part(s)")
          : new GUIContent("Stop",
            "Halts the Transfer of the selected resource(s)\r\nto the selected " + strTarget + " Part(s)");

        if (GUILayout.Button(xferContent, GUILayout.Width(40), GUILayout.Height(18)))
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
    }

    private static void ResourceFlowButtons(TransferPump.TypePump pumpType, int scrollX)
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
          string flowtext = flowState ? "On" : "Off";

          // Flow control Display
          step = "resource quantities labels";

          GUILayout.BeginHorizontal();

          GUILayout.Label(
            string.Format("{0}: ({1}/{2})", displayPumps.Current.Resource, displayPumps.Current.FromRemaining.ToString("#######0.##"),
              displayPumps.Current.FromCapacity.ToString("######0.##")), SMStyle.LabelStyleNoWrap, GUILayout.Width(220),
            GUILayout.Height(18));
          GUILayout.Label(string.Format("{0}", flowtext), GUILayout.Width(20), GUILayout.Height(18));
          if (SMAddon.SmVessel.Vessel.IsControllable)
          {
            step = "render flow button(s)";
            if (
              GUILayout.Button(
                new GUIContent("Flow", "Enables/Disables flow of selected resource(s) from selected part(s)."),
                GUILayout.Width(40), GUILayout.Height(20)))
            {
              List<Part>.Enumerator parts = displayPumps.Current.FromParts.GetEnumerator();
              while (parts.MoveNext())
              {
                if (parts.Current == null) continue;
                parts.Current.Resources[displayPumps.Current.Resource].flowState = !flowState;
              }
            }
            if (Event.current.type == EventType.Repaint && ShowToolTips)
            {
              Rect rect = GUILayoutUtility.GetLastRect();
              ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, scrollX);
            }
          }
          GUILayout.EndHorizontal();
        }
      }
      catch (Exception ex)
      {
        if (!SMAddon.FrameErrTripped)
        {
          Utilities.LogMessage(
            string.Format(" in WindowTransfer.ResourceFlowButtons at step:  " + step + ".  Error:  {0} \r\n\r\n{1}",
              ex.Message, ex.StackTrace), Utilities.LogType.Error, true);
          SMAddon.FrameErrTripped = true;
        }
      }
    }

    #endregion

    #endregion

    #region Button Action Methods

    private static void PartButtonToggled(TransferPump.TypePump pumpType, Part part)
    {
      string step = "Part Button Toggled";
      try
      {
        if (SMConditions.IsTransferInProgress()) return;
        if (pumpType == TransferPump.TypePump.SourceToTarget)
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
          Utilities.LogMessage(
            "Error in WindowTransfer.PartButtonToggled (" + pumpType + ") at step:  " + step + ".  Error:  " + ex,
            Utilities.LogType.Error, true);
          SMAddon.FrameErrTripped = true;
        }
      }
    }

    private static void VesselButtonToggled(TransferPump.TypePump pumpType, ModDockedVessel modVessel)
    {
      string step = "Vessel Button Toggled";
      try
      {
        if (pumpType == TransferPump.TypePump.SourceToTarget)
        {
          // Now lets update the list...
          if (SMAddon.SmVessel.SelectedVesselsSource.Contains(modVessel))
            SMAddon.SmVessel.SelectedVesselsSource.Remove(modVessel);
          else
            SMAddon.SmVessel.SelectedVesselsSource.Add(modVessel);
          SMAddon.SmVessel.SelectedPartsSource =
            SMAddon.SmVessel.GetSelectedVesselsParts(SMAddon.SmVessel.SelectedVesselsSource,
              SMAddon.SmVessel.SelectedResources);
        }
        else
        {
          if (SMAddon.SmVessel.SelectedVesselsTarget.Contains(modVessel))
            SMAddon.SmVessel.SelectedVesselsTarget.Remove(modVessel);
          else
            SMAddon.SmVessel.SelectedVesselsTarget.Add(modVessel);
          SMAddon.SmVessel.SelectedPartsTarget =
            SMAddon.SmVessel.GetSelectedVesselsParts(SMAddon.SmVessel.SelectedVesselsTarget,
              SMAddon.SmVessel.SelectedResources);
        }
        WindowManifest.ResolveResourcePartSelections(SMAddon.SmVessel.SelectedResources);
      }
      catch (Exception ex)
      {
        if (!SMAddon.FrameErrTripped)
        {
          Utilities.LogMessage(
            "Error in WindowTransfer.VesselButtonToggled (" + pumpType + ") at step:  " + step + ".  Error:  " + ex,
            Utilities.LogType.Error, true);
          SMAddon.FrameErrTripped = true;
        }
      }
    }

    #endregion

    #region Utilities

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

    private static bool CanShowVessels()
    {
      return SMAddon.SmVessel.DockedVessels.Count > 0 && SMConditions.AreSelectedResourcesTypeOther(SMAddon.SmVessel.SelectedResources);
    }

    private static bool CanSelectVessel(TransferPump.TypePump pumpType, ModDockedVessel modDockedVessel)
    {
      bool isSelectable = true;
      if (pumpType == TransferPump.TypePump.SourceToTarget)
      {
        if (SMAddon.SmVessel.SelectedVesselsTarget.Contains(modDockedVessel))
          isSelectable = false;
      }
      else
      {
        if (SMAddon.SmVessel.SelectedVesselsSource.Contains(modDockedVessel))
          isSelectable = false;
      }
      return isSelectable;
    }

    private static GUIStyle GetPartButtonStyle(TransferPump.TypePump pumpType, Part part)
    {
      GUIStyle style;
      if (pumpType == TransferPump.TypePump.SourceToTarget)
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

    private static string GetResourceDescription(IList<string> selectedResources, Part part)
    {
      string strDescription;

      if (selectedResources.Contains(SMConditions.ResourceType.Crew.ToString()))
      {
        strDescription = Utilities.GetPartCrewCount(part) + " - " + part.partInfo.title;
      }
      else if (selectedResources.Contains(SMConditions.ResourceType.Science.ToString()))
      {
        int cntScience = GetScienceCount(part);
        strDescription = cntScience + " - " + part.partInfo.title;
      }
      else
      {
        strDescription = part.Resources[selectedResources[0]].amount.ToString("######0.##") + " - " +
                         part.partInfo.title;
      }
      return strDescription;
    }

    private static string GetResourceDescription(List<string> selectedResources, ModDockedVessel modDockedVvessel)
    {
      string strDescription = GetVesselResourceTotals(modDockedVvessel, selectedResources) + " - " +
                           modDockedVvessel.VesselName;
      return strDescription;
    }

    private static int GetScienceCount(Part part)
    {
      try
      {
        return part.Modules.OfType<IScienceDataContainer>().Sum(pm => pm.GetScienceCount());
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format(" in GetScienceCount.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace),
          Utilities.LogType.Error, true);
        return 0;
      }
    }

    private static GUIStyle GetVesselButtonStyle(TransferPump.TypePump pumpType, ModDockedVessel modDockedVessel)
    {
      GUIStyle style;
      if (pumpType == TransferPump.TypePump.SourceToTarget)
      {
        style = SMAddon.SmVessel.SelectedVesselsSource.Contains(modDockedVessel)
          ? SMStyle.ButtonToggledSourceStyle
          : SMStyle.ButtonSourceStyle;
      }
      else
      {
        style = SMAddon.SmVessel.SelectedVesselsTarget.Contains(modDockedVessel)
          ? SMStyle.ButtonToggledTargetStyle
          : SMStyle.ButtonTargetStyle;
      }
      return style;
    }

    internal static string GetVesselResourceTotals(ModDockedVessel modDockedVessel, List<string> selectedResources)
    {
      double currAmount = 0;
      double totAmount = 0;
      try
      {
        List<ModDockedVessel> modDockedVessels = new List<ModDockedVessel> { modDockedVessel };
        List<Part>.Enumerator parts = SMAddon.SmVessel.GetSelectedVesselsParts(modDockedVessels, selectedResources).GetEnumerator();
        while (parts.MoveNext())
        {
          if (parts.Current == null) continue;
          currAmount += parts.Current.Resources[selectedResources[0]].amount;
          totAmount += parts.Current.Resources[selectedResources[0]].maxAmount;
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format(" in DisplayVesselResourceTotals().  Error:  {0}", ex), Utilities.LogType.Error, true);
      }
      string displayAmount = string.Format("({0}/{1})", currAmount.ToString("#######0"), totAmount.ToString("######0"));

      return displayAmount;
    }

    private static bool IsPartSelectable(string selectedResource, TransferPump.TypePump pumpType, Part part)
    {
      if (selectedResource == SMConditions.ResourceType.Crew.ToString() ||
          selectedResource == SMConditions.ResourceType.Science.ToString()) return true;
      bool isSelectable = true;
      if (pumpType == TransferPump.TypePump.SourceToTarget)
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

    #endregion
  }
}