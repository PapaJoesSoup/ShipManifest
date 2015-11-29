using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using DF;
using ShipManifest.Modules;
using ShipManifest.Process;

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

    // Switches for List Viewers
    internal static bool ShowSourceVessels;
    internal static bool ShowTargetVessels;

    private static Dictionary<PartModule, bool> _scienceModulesSource;
    internal static Dictionary<PartModule, bool> ScienceModulesSource
    {
      get
      {
        if (_scienceModulesSource == null)
        {
          if (SMAddon.SmVessel.SelectedPartsSource.Count > 0)
          {
            _scienceModulesSource = new Dictionary<PartModule, bool>();
            var modules = SMAddon.SmVessel.SelectedPartsSource[0].FindModulesImplementing<IScienceDataContainer>().ToArray();
            if (modules.Length > 0)
            {
              foreach (var scienceDataContainer in modules)
              {
                var pm = (PartModule) scienceDataContainer;
                _scienceModulesSource.Add(pm, false);
              }
            }
            return _scienceModulesSource;
          }
          else
          {
            return new Dictionary<PartModule, bool>();
          }
        }
        else
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
      var displayAmounts = Utilities.DisplayVesselResourceTotals(SMAddon.SmVessel.SelectedResources[0]);
      Title = "Transfer - " + SMAddon.SmVessel.Vessel.vesselName + displayAmounts;

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
        ToolTip = SMToolTips.SetActiveToolTip(rect, Position, GUI.tooltip, ref ToolTipActive, 10, 0);
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
        TextBetweenViewers(SMAddon.SmVessel.SelectedPartsSource, SMAddon.XferDirection.SourceToTarget);

        // Build Details ScrollViewer
        SourceDetailsViewer();

        // Okay, we are done with the left column of the dialog...
        GUILayout.EndVertical();

        // Right Column Begins...
        GUILayout.BeginVertical();

        // Build Target Transfer Viewer
        TargetTransferViewer();

        // Text between viewers
        TextBetweenViewers(SMAddon.SmVessel.SelectedPartsTarget, SMAddon.XferDirection.TargetToSource);

        // Build Target details Viewer
        TargetDetailsViewer();

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
        SMAddon.RepositionWindow(ref Position);
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format(" in Ship Manifest Window.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
      }
    }

    private static void TextBetweenViewers(List<Part> selectedParts, SMAddon.XferDirection xferMode)
    {
      var labelText = "";
      GUILayout.BeginHorizontal();
      if (SMAddon.SmVessel.SelectedResources.Contains("Crew"))
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
        if (xferMode == SMAddon.XferDirection.SourceToTarget)
        {
          var prevValue = ShowSourceVessels;
          ShowSourceVessels = GUILayout.Toggle(ShowSourceVessels, "Vessels", GUILayout.Width(90));
          if (!prevValue && ShowSourceVessels)
            WindowManifest.ReconcileSelectedXferParts(SMAddon.SmVessel.SelectedResources);
        }
        else
        {
          var prevValue = ShowSourceVessels;
          ShowTargetVessels = GUILayout.Toggle(ShowTargetVessels, "Vessels", GUILayout.Width(90));
          if (!prevValue && ShowSourceVessels)
            WindowManifest.ReconcileSelectedXferParts(SMAddon.SmVessel.SelectedResources);
        }
      }
      GUILayout.EndHorizontal();
    }

    private static bool CanShowVessels()
    {
      return SMAddon.SmVessel.DockedVessels.Count > 0 && !SMAddon.SmVessel.SelectedResources.Contains("Crew") && !SMAddon.SmVessel.SelectedResources.Contains("Science");
    }

    #region Source Viewers (GUI Layout)
    // Transfer Window components
    private static Vector2 _sourceTransferViewerScrollPosition = Vector2.zero;
    internal static void SourceTransferViewer()
    {
      try
      {
        // This is a scroll panel (we are using it to make button lists...)
        _sourceTransferViewerScrollPosition = GUILayout.BeginScrollView(_sourceTransferViewerScrollPosition, SMStyle.ScrollStyle, GUILayout.Height(100), GUILayout.Width(300));
        GUILayout.BeginVertical();

        if (ShowSourceVessels && CanShowVessels())
          VesselTransferViewer(SMAddon.SmVessel.SelectedResources, SMAddon.XferDirection.SourceToTarget, _sourceTransferViewerScrollPosition);
        else
          PartsTransferViewer(SMAddon.SmVessel.SelectedResources, SMAddon.XferDirection.SourceToTarget, _sourceTransferViewerScrollPosition);

        GUILayout.EndVertical();
        GUILayout.EndScrollView();
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format(" in Ship Manifest Window - SourceTransferViewer.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
      }
    }

    private static Vector2 _sourceDetailsViewerScrollPosition = Vector2.zero;
    private static void SourceDetailsViewer()
    {
      try
      {
        // Source Part resource Details
        // this Scroll viewer is for the details of the part selected above.
        _sourceDetailsViewerScrollPosition = GUILayout.BeginScrollView(_sourceDetailsViewerScrollPosition, SMStyle.ScrollStyle, GUILayout.Height(120), GUILayout.Width(300));
        GUILayout.BeginVertical();

        if (SMAddon.SmVessel.SelectedResources.Contains("Crew"))
        {
          CrewDetails(SMAddon.SmVessel.SelectedPartsSource, SMAddon.SmVessel.SelectedPartsTarget, SMAddon.XferDirection.SourceToTarget, _sourceDetailsViewerScrollPosition);
        }
        else if (SMAddon.SmVessel.SelectedResources.Contains("Science"))
        {
          ScienceDetailsSource();
        }
        else
        {
          // resources are left....
          ResourceDetails(SMAddon.SmVessel.SelectedPartsSource, SMAddon.SmVessel.SelectedPartsTarget, SMAddon.XferDirection.SourceToTarget, _sourceDetailsViewerScrollPosition);
        }
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format(" in WindowTransfer.SourceDetailsViewer.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
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
        if (SMSettings.EnableClsHighlighting && SMAddon.SmVessel.SelectedResources.Contains("Crew"))
          SMStyle.ButtonToggledTargetStyle.normal.textColor = SMSettings.Colors[SMSettings.TargetPartCrewColor];
        else
          SMStyle.ButtonToggledTargetStyle.normal.textColor = SMSettings.Colors[SMSettings.TargetPartColor];

        // This is a scroll panel (we are using it to make button lists...)
        _targetTransferViewerScrollPosition = GUILayout.BeginScrollView(_targetTransferViewerScrollPosition, SMStyle.ScrollStyle, GUILayout.Height(100), GUILayout.Width(300));
        GUILayout.BeginVertical();

        if (ShowTargetVessels && CanShowVessels())
          VesselTransferViewer(SMAddon.SmVessel.SelectedResources, SMAddon.XferDirection.TargetToSource, _targetTransferViewerScrollPosition);
        else
          PartsTransferViewer(SMAddon.SmVessel.SelectedResources, SMAddon.XferDirection.TargetToSource, _targetTransferViewerScrollPosition);

        GUILayout.EndVertical();
        GUILayout.EndScrollView();
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format(" in Ship Manifest Window - TargetTransferViewer.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
      }
    }

    private static Vector2 _targetDetailsViewerScrollPosition = Vector2.zero;
    private static void TargetDetailsViewer()
    {
      try
      {
        // Target Part resource details
        _targetDetailsViewerScrollPosition = GUILayout.BeginScrollView(_targetDetailsViewerScrollPosition, SMStyle.ScrollStyle, GUILayout.Height(120), GUILayout.Width(300));
        GUILayout.BeginVertical();

        // --------------------------------------------------------------------------
        if (SMAddon.SmVessel.SelectedResources.Contains("Crew"))
        {
          CrewDetails(SMAddon.SmVessel.SelectedPartsTarget, SMAddon.SmVessel.SelectedPartsSource, SMAddon.XferDirection.TargetToSource, _targetDetailsViewerScrollPosition);
        }
        else if (SMAddon.SmVessel.SelectedResources.Contains("Science"))
        {
          ScienceDetailsTarget();
        }
        else
        {
          ResourceDetails(SMAddon.SmVessel.SelectedPartsTarget, SMAddon.SmVessel.SelectedPartsSource, SMAddon.XferDirection.TargetToSource, _targetDetailsViewerScrollPosition);
        }
        // --------------------------------------------------------------------------
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format(" in WindowTransfer.TargetDetailsViewer.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
      }
    }
    #endregion

    #region Viewer Details (GUI Layout)

    private static void PartsTransferViewer(List<string> selectedResources, SMAddon.XferDirection xferMode, Vector2 viewerScrollPosition)
    {
      var scrollX = Position.x + (xferMode == SMAddon.XferDirection.SourceToTarget ? 20 : 320);
      var scrollY = Position.y + 30 - viewerScrollPosition.y;
      var step = "begin";
      try
      {
        step = "begin button loop";
        foreach (var part in SMAddon.SmVessel.SelectedResourcesParts)
        {
          // Build the part button title...
          step = "part button title";
          var strDescription = GetResourceDescription(selectedResources, part);

          // set the conditions for a button style change.
          var btnWidth = 270;
          if (!SMSettings.RealismMode && !selectedResources.Contains("Science"))
            btnWidth = 190;

          // Set style based on viewer and toggled state.
          step = "Set style";
          var style = GetPartButtonStyle(xferMode, part);

          GUILayout.BeginHorizontal();

          // Now let's account for any target buttons already pressed. (sources and targets for resources cannot be the same)
          GUI.enabled = IsPartSelectable(selectedResources[0], xferMode, part);

          step = "Render part Buttons";
          if (GUILayout.Button(strDescription, style, GUILayout.Width(btnWidth), GUILayout.Height(20)))
          {
            PartButtonToggled(xferMode, part);
          }
          var rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && rect.Contains(Event.current.mousePosition))
          {
            SMHighlighter.IsMouseOver = true;
            SMHighlighter.MouseOverMode = xferMode;
            SMHighlighter.MouseOverRect = new Rect(scrollX + rect.x, scrollY + rect.y, rect.width, rect.height);
            SMHighlighter.MouseOverpart = part;
          }

          // Reset Button enabling.
          GUI.enabled = true;

          step = "Render dump/fill buttons";
          if (!SMSettings.RealismMode && selectedResources[0] != "Science")
          {

            if (selectedResources[0] == "Crew")
              GUI.enabled = part.protoModuleCrew.Count > 0;
            else if (selectedResources.Count > 1)
              GUI.enabled = part.Resources[selectedResources[0]].amount > 0 || part.Resources[selectedResources[1]].amount > 0;
            else
              GUI.enabled = part.Resources[selectedResources[0]].amount > 0;
            var style1 = xferMode == SMAddon.XferDirection.SourceToTarget ? SMStyle.ButtonSourceStyle : SMStyle.ButtonTargetStyle;
            if (GUILayout.Button("Dump", style1, GUILayout.Width(45), GUILayout.Height(20)))
            {
              if (selectedResources[0] == "Crew")
                SMPart.DumpCrew(part);
              else
              {
                SMPart.DumpResource(part, selectedResources[0]);
                if (selectedResources.Count > 1)
                  SMPart.DumpResource(part, selectedResources[1]);
              }
            }

            var style2 = xferMode == SMAddon.XferDirection.SourceToTarget ? SMStyle.ButtonSourceStyle : SMStyle.ButtonTargetStyle;
            if (selectedResources[0] == "Crew")
              GUI.enabled = part.protoModuleCrew.Count < part.CrewCapacity;
            else if (selectedResources.Count > 1)
              GUI.enabled = part.Resources[selectedResources[0]].amount < part.Resources[selectedResources[0]].maxAmount || part.Resources[selectedResources[1]].amount < part.Resources[selectedResources[1]].maxAmount;
            else
              GUI.enabled = part.Resources[selectedResources[0]].amount < part.Resources[selectedResources[0]].maxAmount;
            if (GUILayout.Button("Fill", style2, GUILayout.Width(30), GUILayout.Height(20)))
            {
              if (selectedResources[0] == "Crew")
                SMPart.FillCrew(part);
              SMPart.FillResource(part, selectedResources[0]);
              if (selectedResources.Count > 1)
                SMPart.FillResource(part, selectedResources[1]);
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
          Utilities.LogMessage("Error in Windowtransfer.PartsTransferViewer (" + xferMode + ") at step:  " + step + ".  Error:  " + ex, "Error", true);
          SMAddon.FrameErrTripped = true;
        }
      }
    }

    private static void VesselTransferViewer(List<string> selectedResources, SMAddon.XferDirection xferMode, Vector2 viewerScrollPosition)
    {
      var scrollX = Position.x + (xferMode == SMAddon.XferDirection.SourceToTarget ? 20 : 320);
      var scrollY = Position.y + 30 - viewerScrollPosition.y;
      var step = "begin";
      try
      {
        step = "begin button loop";
        foreach (var modDockedVessel in SMAddon.SmVessel.DockedVessels)
        {
          // Build the part button title...
          step = "vessel button title";
          var strDescription = GetResourceDescription(selectedResources, modDockedVessel);

          // set the conditions for a button style change.
          var btnWidth = 265;
          if (!SMSettings.RealismMode)
            btnWidth = 180;

          // Set style based on viewer and toggled state.
          step = "Set style";
          var style = GetVesselButtonStyle(xferMode, modDockedVessel);

          GUILayout.BeginHorizontal();

          // Now let's account for any target buttons already pressed. (sources and targets for resources cannot be the same)
          GUI.enabled = IsVesselSelectable(xferMode, modDockedVessel);

          step = "Render part Buttons";
          if (GUILayout.Button(string.Format("{0}", strDescription), style, GUILayout.Width(btnWidth), GUILayout.Height(20)))
          {
            VesselButtonToggled(xferMode, modDockedVessel);
          }
          var rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && rect.Contains(Event.current.mousePosition))
          {
            SMHighlighter.IsMouseOver = true;
            SMHighlighter.MouseOverMode = xferMode;
            SMHighlighter.MouseOverRect = new Rect(scrollX + rect.x, scrollY + rect.y, rect.width, rect.height);
            SMHighlighter.MouseOverpart = null;
            SMHighlighter.MouseOverparts = modDockedVessel.VesselParts;
          }

          // Reset Button enabling.
          GUI.enabled = true;

          //step = "Render dump/fill buttons";
          if (!SMSettings.RealismMode)
          {
            if (selectedResources.Count > 1)
              GUI.enabled = TransferResource.CalcResourceRemaining(modDockedVessel.VesselParts, selectedResources[0]) > 0 || TransferResource.CalcResourceRemaining(modDockedVessel.VesselParts, selectedResources[1]) > 0;
            else
              GUI.enabled = TransferResource.CalcResourceRemaining(modDockedVessel.VesselParts, selectedResources[0]) > 0;
            var style1 = xferMode == SMAddon.XferDirection.SourceToTarget ? SMStyle.ButtonSourceStyle : SMStyle.ButtonTargetStyle;
            if (GUILayout.Button("Dump", style1, GUILayout.Width(45), GUILayout.Height(20)))
            {
              SMPart.DumpResource(modDockedVessel.VesselParts, selectedResources[0]);
              if (selectedResources.Count > 1)
                SMPart.DumpResource(modDockedVessel.VesselParts, selectedResources[1]);
            }

            var style2 = xferMode == SMAddon.XferDirection.SourceToTarget ? SMStyle.ButtonSourceStyle : SMStyle.ButtonTargetStyle;
            if (selectedResources.Count > 1)
              GUI.enabled = TransferResource.CalcRemainingCapacity(modDockedVessel.VesselParts, selectedResources[0]) > 0 || TransferResource.CalcRemainingCapacity(modDockedVessel.VesselParts, selectedResources[0]) > 0;
            else
              GUI.enabled = TransferResource.CalcRemainingCapacity(modDockedVessel.VesselParts, selectedResources[0]) > 0;
            if (GUILayout.Button("Fill", style2, GUILayout.Width(30), GUILayout.Height(20)))
            {
              SMPart.FillResource(modDockedVessel.VesselParts, selectedResources[0]);
              if (selectedResources.Count > 1)
                SMPart.FillResource(modDockedVessel.VesselParts, selectedResources[1]);
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
          Utilities.LogMessage("Error in Windowtransfer.VesselTransferViewer (" + xferMode + ") at step:  " + step + ".  Error:  " + ex, "Error", true);
          SMAddon.FrameErrTripped = true;
        }
      }
    }

    private static void CrewDetails(List<Part> selectedPartsFrom, List<Part> selectedPartsTo, SMAddon.XferDirection xferMode, Vector2 scrollPosition)
    {
      // Since only one Crew Part can be selected, all lists will use an index of [0].
      float xOffset = xferMode == SMAddon.XferDirection.SourceToTarget ? 30 : 330;
      float yOffset = 160;

      if (selectedPartsFrom.Count <= 0) return;
      // ReSharper disable once ForCanBeConvertedToForeach
      for (var x = 0; x < selectedPartsFrom[0].protoModuleCrew.Count; x++)
      {
        var crewMember = selectedPartsFrom[0].protoModuleCrew[x];
        GUILayout.BeginHorizontal();
        if (crewMember.seat != null)
        {
          if (SMAddon.SmVessel.TransferCrewObj.CrewXferActive || TransferResource.ResourceXferActive)
            GUI.enabled = false;

          if (GUILayout.Button(new GUIContent(">>", "Move Kerbal to another seat within Part"), SMStyle.ButtonStyle, GUILayout.Width(15), GUILayout.Height(20)))
          {
            ToolTip = "";
            SMAddon.SmVessel.TransferCrewObj.CrewTransferBegin(crewMember, selectedPartsFrom[0], selectedPartsFrom[0]);
          }
          if (Event.current.type == EventType.Repaint && ShowToolTips)
          {
            var rect = GUILayoutUtility.GetLastRect();
            ToolTip = SMToolTips.SetActiveToolTip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - scrollPosition.y);
          }
          GUI.enabled = true;
        }
        GUILayout.Label(string.Format("  {0}", crewMember.name + " (" + crewMember.experienceTrait.Title + ")"), GUILayout.Width(190), GUILayout.Height(20));
        GUI.enabled = SMAddon.CanKerbalsBeXferred(selectedPartsFrom, selectedPartsTo);
        if ((SMAddon.SmVessel.TransferCrewObj.FromCrewMember == crewMember || SMAddon.SmVessel.TransferCrewObj.ToCrewMember == crewMember) && (SMAddon.SmVessel.TransferCrewObj.CrewXferActive || TransferResource.ResourceXferActive))
        {
          GUI.enabled = true;
          GUILayout.Label("Moving", GUILayout.Width(50), GUILayout.Height(20));
        }
        else
        {
          if (GUILayout.Button(new GUIContent("Xfer", XferToolTip), SMStyle.ButtonStyle, GUILayout.Width(50), GUILayout.Height(20)))
          {
            SMAddon.SmVessel.TransferCrewObj.FromCrewMember = crewMember;
            SMAddon.SmVessel.TransferCrewObj.CrewTransferBegin(crewMember, selectedPartsFrom[0], selectedPartsTo[0]);
          }
          if (Event.current.type == EventType.Repaint && ShowToolTips)
          {
            var rect = GUILayoutUtility.GetLastRect();
            ToolTip = SMToolTips.SetActiveToolTip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - scrollPosition.y);
          }
        }
        GUI.enabled = true;
        GUILayout.EndHorizontal();
      }
      // Cater for DeepFreeze Continued... parts - list frozen kerbals
      if (DFInterface.IsDFInstalled)
      {
        try
        {
          var sourcepartFrzr = selectedPartsFrom[0].FindModuleImplementing<IDeepFreezer>();
          if (sourcepartFrzr != null)
          {
            if (sourcepartFrzr.DFIStoredCrewList.Count > 0)
            {
              var dfOjbect = DFInterface.GetFrozenKerbals();
              foreach (var frzncrew in sourcepartFrzr.DFIStoredCrewList)
              {
                GUILayout.BeginHorizontal();
                GUI.enabled = false;
                if (GUILayout.Button(new GUIContent(">>", "Move Kerbal to another seat within Part"), SMStyle.ButtonStyle, GUILayout.Width(15), GUILayout.Height(20)))
                {
                  ToolTip = "";
                }
                if (Event.current.type == EventType.Repaint && ShowToolTips)
                {
                  var rect = GUILayoutUtility.GetLastRect();
                  ToolTip = SMToolTips.SetActiveToolTip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - scrollPosition.y);
                }

                var trait = dfOjbect.FrozenKerbals[frzncrew.CrewName].experienceTraitName;
                GUI.enabled = true;
                GUILayout.Label(string.Format("  {0}", frzncrew.CrewName + " (" + trait + ")"), SMStyle.LabelStyleCyan, GUILayout.Width(190), GUILayout.Height(20));

                if (GUILayout.Button(new GUIContent("Thaw", "This Kerbal is Frozen. Click to Revive kerbal"), SMStyle.ButtonStyle, GUILayout.Width(50), GUILayout.Height(20)))
                {
                  WindowRoster.ThawKerbal(frzncrew.CrewName);
                  ToolTip = "";
                }
                if (Event.current.type == EventType.Repaint && ShowToolTips)
                {
                  var rect = GUILayoutUtility.GetLastRect();
                  ToolTip = SMToolTips.SetActiveToolTip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - scrollPosition.y);
                }
                GUILayout.EndHorizontal();
              }
            }
          }
        }
        catch (Exception ex)
        {
          Debug.Log("Error attempting to check DeepFreeze for FrozenKerbals");
          Debug.Log(ex.Message);

        }
      }
    }

    private static void ScienceDetailsSource()
    {
      if (SMAddon.SmVessel.SelectedPartsSource.Count > 0)
      {
        float xOffset = 30;
        float yOffset = 160;
        var modules = ScienceModulesSource.Keys.ToArray();
        foreach (var pm in modules)
        {
          // experiments/Containers.
          var scienceCount = ((IScienceDataContainer)pm).GetScienceCount();
          var isCollectable = true;
          if (pm.moduleName == "ModuleScienceExperiment")
            isCollectable = ((ModuleScienceExperiment)pm).dataIsCollectable;
          else if (pm.moduleName == "ModuleScienceContainer")
            isCollectable = ((ModuleScienceContainer)pm).dataIsCollectable;

          GUILayout.BeginHorizontal();
          GUI.enabled = ((IScienceDataContainer)pm).GetScienceCount() > 0;

          var label = "+";
          var toolTip = "Expand/Collapse Science detail.";
          if (!GUI.enabled)
            toolTip += " (Disabled, nothing to xfer)";
          var expandStyle = ScienceModulesSource[pm] ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle;
          if (ScienceModulesSource[pm])
            label = "-";
          if (GUILayout.Button(new GUIContent(label, toolTip), expandStyle, GUILayout.Width(15), GUILayout.Height(20)))
          {
            ScienceModulesSource[pm] = !ScienceModulesSource[pm];
          }
          if (Event.current.type == EventType.Repaint && ShowToolTips)
          {
            var rect = GUILayoutUtility.GetLastRect();
            ToolTip = SMToolTips.SetActiveToolTip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - _targetDetailsViewerScrollPosition.y);
          }
          GUI.enabled = true;
          GUILayout.Label(string.Format("{0} - ({1})", pm.moduleName, scienceCount), GUILayout.Width(205), GUILayout.Height(20));

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
            if (GUILayout.Button(new GUIContent("Xfer", toolTip), SMStyle.ButtonStyle, GUILayout.Width(40), GUILayout.Height(20)))
            {
              SMAddon.SmVessel.SelectedModuleSource = pm;
              Transfers.TransferScience(SMAddon.SmVessel.SelectedModuleSource, SMAddon.SmVessel.SelectedModuleTarget);
              SMAddon.SmVessel.SelectedModuleSource = null;
            }
            if (Event.current.type == EventType.Repaint && ShowToolTips)
            {
              var rect = GUILayoutUtility.GetLastRect();
              ToolTip = SMToolTips.SetActiveToolTip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - _targetDetailsViewerScrollPosition.y);
            }
          }
          GUILayout.EndHorizontal();
          if (ScienceModulesSource[pm])
          {
            var data = ((IScienceDataContainer)pm).GetData();

            foreach (var item in data)
            {
              GUILayout.BeginHorizontal();
              GUILayout.Label("", GUILayout.Width(15), GUILayout.Height(20));

              // Get science data from experiment.
              var expId = item.subjectID.Split('@')[0];
              var expKey = item.subjectID.Split('@')[1];
              var se = ResearchAndDevelopment.GetExperiment(expId);
              var key = (from k in se.Results.Keys where expKey.Contains(k) select k).FirstOrDefault();
              key = key ?? "default";
              var results = se.Results[key];

              // Build Tooltip
              toolTip = item.title;
              //toolTip += "\r\n-SubjectID:   " + item.subjectID;
              toolTip += "\r\n-Results:    " + results;
              toolTip += "\r\n-Data Amt:   " + item.dataAmount + " Mits";
              toolTip += "\r\n-Xmit Value: " + item.transmitValue;
              toolTip += "\r\n-Lab Value:  " + item.labValue;
              toolTip += "\r\n-Lab Boost:  " + item.labBoost;

              GUILayout.Label(new GUIContent(se.experimentTitle, toolTip), SMStyle.LabelStyleNoWrap, GUILayout.Width(205), GUILayout.Height(20));
              if (Event.current.type == EventType.Repaint && ShowToolTips)
              {
                var rect = GUILayoutUtility.GetLastRect();
                ToolTip = SMToolTips.SetActiveToolTip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - _targetDetailsViewerScrollPosition.y);
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
                if (GUILayout.Button(new GUIContent("Xfer", toolTip), SMStyle.ButtonStyle, GUILayout.Width(40), GUILayout.Height(20)))
                {
                  if (((ModuleScienceContainer)SMAddon.SmVessel.SelectedModuleTarget).AddData(item))
                    ((IScienceDataContainer)pm).DumpData(item);
                }
                if (Event.current.type == EventType.Repaint && ShowToolTips)
                {
                  var rect = GUILayoutUtility.GetLastRect();
                  ToolTip = SMToolTips.SetActiveToolTip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - _targetDetailsViewerScrollPosition.y);
                }
              }
              GUILayout.EndHorizontal();
            }
          }
          GUI.enabled = true;
        }
      }
    }

    private static void ScienceDetailsTarget()
    {
      float xOffset = 330;
      float yOffset = 160;
      if (SMAddon.SmVessel.SelectedPartsTarget.Count > 0)
      {
        var count = SMAddon.SmVessel.SelectedPartsTarget[0].Modules.Cast<PartModule>().Count(tpm => tpm is IScienceDataContainer && tpm.moduleName != "ModuleScienceExperiment");

        foreach (PartModule pm in SMAddon.SmVessel.SelectedPartsTarget[0].Modules)
        {
          // Containers.
          if (!(pm is IScienceDataContainer) || pm.moduleName == "ModuleScienceExperiment") continue;
          var scienceCount = ((IScienceDataContainer)pm).GetScienceCount();
          GUILayout.BeginHorizontal();
          GUILayout.Label(string.Format("{0} - ({1})", pm.moduleName, scienceCount), GUILayout.Width(220), GUILayout.Height(20));
          // set the conditions for a button style change.
          var isReceiveToggled = false;
          if (pm == SMAddon.SmVessel.SelectedModuleTarget)
            isReceiveToggled = true;
          else if (count == 1)
          {
            SMAddon.SmVessel.SelectedModuleTarget = pm;
            isReceiveToggled = true;
          }
          //SelectedModuleTarget = pm;
          var style = isReceiveToggled ? SMStyle.ButtonToggledTargetStyle : SMStyle.ButtonStyle;

          // Only containers can receive science data
          if (pm.moduleName != "ModuleScienceExperiment")
          {
            if (GUILayout.Button(new GUIContent("Recv", "Set this module as the receiving container"), style, GUILayout.Width(40), GUILayout.Height(20)))
            {
              SMAddon.SmVessel.SelectedModuleTarget = pm;
            }
            if (Event.current.type == EventType.Repaint && ShowToolTips)
            {
              var rect = GUILayoutUtility.GetLastRect();
              ToolTip = SMToolTips.SetActiveToolTip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - _targetDetailsViewerScrollPosition.y);
            }
          }
          GUILayout.EndHorizontal();
        }
      }
    }

    private static void ResourceDetails(List<Part> partsSource, List<Part> partsTarget, SMAddon.XferDirection xferMode, Vector2 scrollPosition)
    {
      // This routine assumes that a resource has been selected on the Resource manifest window.
      // Set scrollX offsets for left and right viewers
      var xOffset = xferMode == SMAddon.XferDirection.SourceToTarget ? 30 : 330;
      var yOffset = 160;
      var toolTip = "";

      // Pass in static vars to improve readability.
      var selectedResources = SMAddon.SmVessel.SelectedResources;
      var xferResources = SMAddon.SmVessel.ResourcesToXfer;

      if (partsSource.Count > 0)
      {
        var modResource = TransferResource.GetXferResource(xferResources, xferMode);
        var ratioResource = TransferResource.GetXferResource(xferResources, xferMode, true);
        modResource.XferRatio = 1;
        ratioResource.XferRatio = TransferResource.CalcRatio(xferResources, xferMode);

        var thisXferAmount = modResource.XferAmount(xferMode);

        // Set tooltips directional data
        var strTarget = xferMode == SMAddon.XferDirection.SourceToTarget ? "Target" : "Source";


        // Resource Flow control Display loop
        ResourceFlowButtons(partsSource, xferMode, scrollPosition, xOffset, yOffset);

        // Xfer Controls Display
        // let's determine how much of a resource we can move to the target.
        var maxXferAmount = TransferResource.CalcMaxXferAmt(partsSource, partsTarget, selectedResources);
        if (maxXferAmount > 0 || TransferResource.ResourceXferActive)
        {
          GUILayout.BeginHorizontal();
          string label;
          Rect rect;
          if (TransferResource.ResourceXferActive)
          {
            // We want to show this during transfer if the direction is correct...
            if (SMAddon.XferMode == xferMode)
            {
              GUILayout.Label("Xfer Remaining:", GUILayout.Width(120));
              GUILayout.Label((modResource.XferAmount(SMAddon.XferMode) - modResource.AmtXferred).ToString("#######0.##"));
              if (SMAddon.SmVessel.SelectedResources.Count > 1)
                GUILayout.Label(" | " + (ratioResource.XferAmount(SMAddon.XferMode) - ratioResource.AmtXferred).ToString("#######0.##"));
            }
          }
          else
          {
            // Lets parse the string to allow decimal points.
            var strXferAmount = modResource.XferAmount(xferMode).ToString(CultureInfo.InvariantCulture);
            // add the decimal point if it was typed.
            strXferAmount = modResource.GetStringDecimal(strXferAmount, xferMode);
            // add the zero if it was typed.
            strXferAmount = modResource.GetStringZero(strXferAmount, xferMode);

            // Now update the static var
            modResource.SetXferAmountString(strXferAmount, xferMode);
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
              ToolTip = SMToolTips.SetActiveToolTip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - scrollPosition.y);
            strXferAmount = GUILayout.TextField(strXferAmount, 20, GUILayout.Width(95), GUILayout.Height(20));
            // update decimal bool 
            modResource.SetStringDecimal(strXferAmount, xferMode);
            //update zero bool 
            modResource.SetStringZero(strXferAmount, xferMode);
            // Update static Xfer Amount var
            thisXferAmount = modResource.UpdateXferAmount(strXferAmount, xferMode);
            var ratioXferAmt = thisXferAmount * ratioResource.XferRatio > ratioResource.FromCapacity(xferMode) ? ratioResource.FromCapacity(xferMode) : thisXferAmount * ratioResource.XferRatio;
            if (SMAddon.SmVessel.SelectedResources.Count > 1)
            {
              label = " | " + ratioXferAmt.ToString("#######0.##");
              toolTip = "Smaller Tank xfer amount.  Calculated at " + ratioResource.XferRatio + ".\r\n(Note: A value of 0.818181 = 0.9/1.1)";
              GUILayout.Label(new GUIContent(label, toolTip), GUILayout.Width(80));
              rect = GUILayoutUtility.GetLastRect();
              if (Event.current.type == EventType.Repaint && ShowToolTips)
                ToolTip = SMToolTips.SetActiveToolTip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - scrollPosition.y);
            }
          }
          GUILayout.EndHorizontal();

          if (IsShipControllable() && CanResourceBeXferred(xferMode, maxXferAmount))
          {
            GUILayout.BeginHorizontal();
            var noPad = SMStyle.LabelStyleNoPad;
            label = "Xfer:";
            toolTip = "Xfer amount slider control.\r\nMove slider to select a different value.\r\nYou can use this instead of the text box above.";
            GUILayout.Label(new GUIContent(label, toolTip), noPad, GUILayout.Width(50), GUILayout.Height(20));
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips)
              ToolTip = SMToolTips.SetActiveToolTip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - scrollPosition.y);
            thisXferAmount = GUILayout.HorizontalSlider((float)thisXferAmount, 0, (float)maxXferAmount, GUILayout.Width(190));

            // set button style
            var xferContent = !TransferResource.ResourceXferActive ? new GUIContent("Xfer", "Transfers the selected resource\r\nto the selected " + strTarget + " Part") : new GUIContent("Stop", "Halts the Transfer of the selected resource\r\nto the selected " + strTarget + " Part");

            if (GUILayout.Button(xferContent, GUILayout.Width(40), GUILayout.Height(18)))
            {
              if (!TransferResource.ResourceXferActive)
              {
                //Calc amounts and update xfer modules
                AssignXferAmounts(xferResources, xferMode, thisXferAmount);
                Transfers.TransferResources(partsSource, partsTarget);
              }
              else if (TransferResource.ResourceXferActive && SMSettings.RealismMode)
                TransferResource.XferState = TransferResource.ResourceXferState.Stop;
            }
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips)
              ToolTip = SMToolTips.SetActiveToolTip(rect, Position, GUI.tooltip, ref ToolTipActive, xOffset, yOffset - scrollPosition.y);
            GUILayout.EndHorizontal();
          }
          if (!TransferResource.ResourceXferActive)
            modResource.UpdateXferAmount(thisXferAmount.ToString(CultureInfo.InvariantCulture), xferMode);
        }
      }
    }

    private static void AssignXferAmounts(List<TransferResource> xferResources, SMAddon.XferDirection xferMode, double thisXferAmount)
    {
      if (xferResources.Count > 1)
      {
        // Calculate Ratio and transfer amounts.  Ratio is based off the largest amount to move, so will always be less than 1.
        var ratio = TransferResource.CalcRatio(xferResources, xferMode);

        if (xferResources[0].ToCapacity(xferMode) > xferResources[1].ToCapacity(xferMode))
        {
          xferResources[0].XferRatio = 1;
          xferResources[1].XferRatio = ratio;
          if (xferMode == SMAddon.XferDirection.SourceToTarget)
          {
            xferResources[0].SrcXferAmount = thisXferAmount;
            xferResources[1].SrcXferAmount = thisXferAmount * ratio <= xferResources[1].FromCapacity(xferMode) ? thisXferAmount * ratio : xferResources[1].FromCapacity(xferMode);
          }
          else
          {
            xferResources[0].TgtXferAmount = thisXferAmount;
            xferResources[1].TgtXferAmount = thisXferAmount * ratio <= xferResources[1].FromCapacity(xferMode) ? thisXferAmount * ratio : xferResources[1].FromCapacity(xferMode);
          }
        }
        else
        {
          xferResources[1].XferRatio = 1;
          xferResources[0].XferRatio = ratio;
          if (xferMode == SMAddon.XferDirection.SourceToTarget)
          {
            xferResources[1].SrcXferAmount = thisXferAmount;
            xferResources[0].SrcXferAmount = thisXferAmount * ratio <= xferResources[0].FromCapacity(xferMode) ? thisXferAmount * ratio : xferResources[0].FromCapacity(xferMode);
          }
          else
          {
            xferResources[1].TgtXferAmount = thisXferAmount;
            xferResources[0].TgtXferAmount = thisXferAmount * ratio <= xferResources[0].FromCapacity(xferMode) ? thisXferAmount * ratio : xferResources[0].FromCapacity(xferMode);
          }
        }
      }
      else
      {
        xferResources[0].XferRatio = 1;
        if (xferMode == SMAddon.XferDirection.SourceToTarget)
          xferResources[0].SrcXferAmount = thisXferAmount;
        else
          xferResources[0].TgtXferAmount = thisXferAmount;
      }
    }

    private static void ResourceFlowButtons(List<Part> partsSource, SMAddon.XferDirection xferMode, Vector2 scrollPosition, int scrollX, int scrollY)
    {
      var step = "";
      try
      {
        foreach (var modResource in SMAddon.SmVessel.ResourcesToXfer)
        {
          var resource = modResource.ResourceName;

          // this var is used for button state change management
          var flowState = true;
          // Loop through parts to establish flow state.   Any part that is off, means all are off for display purposes.
          if (partsSource.Count > 0)
          {
            step = "We have parts.  Get flowstate";
            foreach (var part in partsSource)
            {
              if (!part.Resources[resource].flowState)
                flowState = false;
            }
            var flowtext = flowState ? "On" : "Off";


            // Flow control Display
            step = "resource quantities labels";

            GUILayout.BeginHorizontal();

            GUILayout.Label(string.Format("{0}: ({1}/{2})", resource, modResource.FromAmtRemaining(xferMode).ToString("#######0.##"), modResource.FromCapacity(xferMode).ToString("######0.##")), SMStyle.LabelStyleNoWrap, GUILayout.Width(220), GUILayout.Height(18));
            GUILayout.Label(string.Format("{0}", flowtext), GUILayout.Width(20), GUILayout.Height(18));
            if (SMAddon.SmVessel.Vessel.IsControllable)
            {
              step = "render flow button(s)";
              if (GUILayout.Button(new GUIContent("Flow", "Enables/Disables flow of selected resource(s) from selected part(s)."), GUILayout.Width(40), GUILayout.Height(20)))
              {
                foreach (var part in partsSource)
                {
                  part.Resources[resource].flowState = !flowState;
                }
              }
              if (Event.current.type == EventType.Repaint && ShowToolTips)
              {
                var rect = GUILayoutUtility.GetLastRect();
                ToolTip = SMToolTips.SetActiveToolTip(rect, Position, GUI.tooltip, ref ToolTipActive, scrollX, scrollY - scrollPosition.y);
              }
            }
            GUILayout.EndHorizontal();
          }
        }
      }
      catch (Exception ex)
      {
        if (!SMAddon.FrameErrTripped)
        {
          Utilities.LogMessage(string.Format(" in WindowTransfer.ResourceFlowButtons at step:  " + step + ".  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
          SMAddon.FrameErrTripped = true;
        }
      }
    }

    #endregion

    #endregion

    #region Methods

    private static void PartButtonToggled(SMAddon.XferDirection xferMode, Part part)
    {
      var step = "Part Button Toggled";
      try
      {
        if (!SMAddon.SmVessel.TransferCrewObj.CrewXferActive && !TransferResource.ResourceXferActive)
        {
          if (xferMode == SMAddon.XferDirection.SourceToTarget)
          {
            // Now lets update the list...
            if (SMAddon.SmVessel.SelectedPartsSource.Contains(part))
            {
              SMAddon.SmVessel.SelectedPartsSource.Remove(part);
            }
            else
            {
              if (SMAddon.SmVessel.SelectedResources.Contains("Crew") || SMAddon.SmVessel.SelectedResources.Contains("Science"))
                SMAddon.SmVessel.SelectedPartsSource.Clear();
              SMAddon.SmVessel.SelectedPartsSource.Add(part);
            }
            if (SMAddon.SmVessel.SelectedResources.Contains("Crew") && SMSettings.EnableCls)
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
              if (SMAddon.SmVessel.SelectedResources.Contains("Crew") || SMAddon.SmVessel.SelectedResources.Contains("Science"))
                SMAddon.SmVessel.SelectedPartsTarget.Clear();
              SMAddon.SmVessel.SelectedPartsTarget.Add(part);
            }
            SMAddon.SmVessel.SelectedModuleTarget = null;
          }
          step = "Set Xfer amounts?";
          if (!SMAddon.SmVessel.SelectedResources.Contains("Crew") && !SMAddon.SmVessel.SelectedResources.Contains("Science"))
          {
            step = "Set Xfer amounts = yes";
            foreach (var modResource in SMAddon.SmVessel.ResourcesToXfer)
            {
              modResource.SrcXferAmount = TransferResource.CalcMaxResourceXferAmt(SMAddon.SmVessel.SelectedPartsSource, SMAddon.SmVessel.SelectedPartsTarget, modResource.ResourceName);
              modResource.TgtXferAmount = TransferResource.CalcMaxResourceXferAmt(SMAddon.SmVessel.SelectedPartsTarget, SMAddon.SmVessel.SelectedPartsSource, modResource.ResourceName);
            }
          }
        }
      }
      catch (Exception ex)
      {
        if (!SMAddon.FrameErrTripped)
        {
          Utilities.LogMessage("Error in WindowTransfer.PartButtonToggled (" + xferMode + ") at step:  " + step + ".  Error:  " + ex, "Error", true);
          SMAddon.FrameErrTripped = true;
        }
      }
    }

    private static void VesselButtonToggled(SMAddon.XferDirection xferMode, ModDockedVessel modVessel)
    {
      var step = "Vessel Button Toggled";
      try
      {
        if (xferMode == SMAddon.XferDirection.SourceToTarget)
        {
          // Now lets update the list...
          if (SMAddon.SmVessel.SelectedVesselsSource.Contains(modVessel))
            SMAddon.SmVessel.SelectedVesselsSource.Remove(modVessel);
          else
            SMAddon.SmVessel.SelectedVesselsSource.Add(modVessel);
          SMAddon.SmVessel.SelectedPartsSource = SMAddon.SmVessel.GetSelectedVesselsParts(SMAddon.SmVessel.SelectedVesselsSource, SMAddon.SmVessel.SelectedResources);
        }
        else
        {
          if (SMAddon.SmVessel.SelectedVesselsTarget.Contains(modVessel))
            SMAddon.SmVessel.SelectedVesselsTarget.Remove(modVessel);
          else
            SMAddon.SmVessel.SelectedVesselsTarget.Add(modVessel);
          SMAddon.SmVessel.SelectedPartsTarget = SMAddon.SmVessel.GetSelectedVesselsParts(SMAddon.SmVessel.SelectedVesselsTarget, SMAddon.SmVessel.SelectedResources);
        }
        WindowManifest.ReconcileSelectedXferParts(SMAddon.SmVessel.SelectedResources);
        step = "Set Xfer amounts?";
        foreach (var modResource in SMAddon.SmVessel.ResourcesToXfer)
        {
          modResource.SrcXferAmount = TransferResource.CalcMaxResourceXferAmt(SMAddon.SmVessel.SelectedPartsSource, SMAddon.SmVessel.SelectedPartsTarget, modResource.ResourceName);
          modResource.TgtXferAmount = TransferResource.CalcMaxResourceXferAmt(SMAddon.SmVessel.SelectedPartsTarget, SMAddon.SmVessel.SelectedPartsSource, modResource.ResourceName);
        }
      }
      catch (Exception ex)
      {
        if (!SMAddon.FrameErrTripped)
        {
          Utilities.LogMessage("Error in WindowTransfer.VesselButtonToggled (" + xferMode + ") at step:  " + step + ".  Error:  " + ex, "Error", true);
          SMAddon.FrameErrTripped = true;
        }
      }
    }

    #endregion

    #region Utilities

    private static bool CanResourceBeXferred(SMAddon.XferDirection thisXferMode, double maxXferAmount)
    {
      return (!TransferResource.ResourceXferActive && maxXferAmount > 0) || (TransferResource.ResourceXferActive && SMAddon.XferMode == thisXferMode);
    }

    internal static bool IsShipControllable()
    {
      return (SMAddon.SmVessel.Vessel.IsControllable && SMSettings.RealismMode) || !SMSettings.RealismMode;
    }

    private static int GetScienceCount(Part part)
    {
      try
      {
        return part.Modules.OfType<IScienceDataContainer>().Sum(pm => pm.GetScienceCount());
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format(" in GetScienceCount.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
        return 0;
      }
    }

    private static bool IsPartSelectable(string selectedResource, SMAddon.XferDirection xferMode, Part part)
    {
      var isSelectable = true;
      if (selectedResource != "Crew" && selectedResource != "Science")
      {
        if (xferMode == SMAddon.XferDirection.SourceToTarget)
        {
          if (SMAddon.SmVessel.SelectedPartsTarget.Contains(part))
            isSelectable = false;
        }
        else
        {
          if (SMAddon.SmVessel.SelectedPartsSource.Contains(part))
            isSelectable = false;
        }
      }
      return isSelectable;
    }

    private static bool IsVesselSelectable(SMAddon.XferDirection xferMode, ModDockedVessel modDockedVessel)
    {
      var isSelectable = true;
      if (xferMode == SMAddon.XferDirection.SourceToTarget)
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

    private static GUIStyle GetPartButtonStyle(SMAddon.XferDirection xferMode, Part part)
    {
      GUIStyle style;
      if (xferMode == SMAddon.XferDirection.SourceToTarget)
      {
        style = SMAddon.SmVessel.SelectedPartsSource.Contains(part) ? SMStyle.ButtonToggledSourceStyle : SMStyle.ButtonSourceStyle;
      }
      else
      {
        style = SMAddon.SmVessel.SelectedPartsTarget.Contains(part) ? SMStyle.ButtonToggledTargetStyle : SMStyle.ButtonTargetStyle;
      }
      return style;
    }

    private static GUIStyle GetVesselButtonStyle(SMAddon.XferDirection xferMode, ModDockedVessel modDockedVessel)
    {
      GUIStyle style;
      if (xferMode == SMAddon.XferDirection.SourceToTarget)
      {
        style = SMAddon.SmVessel.SelectedVesselsSource.Contains(modDockedVessel) ? SMStyle.ButtonToggledSourceStyle : SMStyle.ButtonSourceStyle;
      }
      else
      {
        style = SMAddon.SmVessel.SelectedVesselsTarget.Contains(modDockedVessel) ? SMStyle.ButtonToggledTargetStyle : SMStyle.ButtonTargetStyle;
      }
      return style;
    }

    private static string GetResourceDescription(List<string> selectedResources, Part part)
    {
      string strDescription;

      if (selectedResources.Contains("Crew"))
      {
        strDescription = Utilities.GetPartCrewCount(part) + " - " + part.partInfo.title;
      }
      else if (selectedResources.Contains("Science"))
      {
        var cntScience = GetScienceCount(part);
        strDescription = cntScience + " - " + part.partInfo.title;
      }
      else
      {
        strDescription = part.Resources[selectedResources[0]].amount.ToString("######0.##") + " - " + part.partInfo.title;
      }
      return strDescription;
    }

    private static string GetResourceDescription(List<string> selectedResources, ModDockedVessel modDockedVvessel)
    {
      var strDescription = DisplayVesselResourceTotals(modDockedVvessel, selectedResources) + " - " + modDockedVvessel.VesselName;
      return strDescription;
    }

    internal static string DisplayVesselResourceTotals(ModDockedVessel modDockedVessel, List<string> selectedResources)
    {
      double currAmount = 0;
      double totAmount = 0;
      try
      {
        var modDockedVessels = new List<ModDockedVessel> {modDockedVessel};
        foreach (var part in SMAddon.SmVessel.GetSelectedVesselsParts(modDockedVessels, selectedResources))
        {
          currAmount += part.Resources[selectedResources[0]].amount;
          totAmount += part.Resources[selectedResources[0]].maxAmount;
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(String.Format(" in DisplayVesselResourceTotals().  Error:  {0}", ex), "Error", true);
      }
      var displayAmount = string.Format("({0}/{1})", currAmount.ToString("#######0"), totAmount.ToString("######0"));

      return displayAmount;
    }

    #endregion

  }
}
