using System;
using System.Collections.Generic;
using ShipManifest.InternalObjects;
using ShipManifest.Modules;
using UniLinq;
using UnityEngine;

namespace ShipManifest.Windows.Tabs.Control
{
  internal static class TabVessel
  {
    internal static string ToolTip = "";
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;
    private static bool _canShowToolTips = true;

    // Temporary change in label width to allow release.  Will work vessel combining in a later release.
    //private const float guiLabelWidth = 200;
    private const float guiLabelWidth = 230;
    private const float guiBtnWidth = 60;

    internal static int CombineVesselCount = 0;

    internal static void Display()
    {
      //float scrollX = WindowControl.Position.x + 20;
      //float scrollY = WindowControl.Position.y + 50 - displayViewerPosition.y;
      float scrollX = 20;
      float scrollY = WindowControl._displayViewerPosition.y;

      // Reset Tooltip active flag...
      ToolTipActive = false;
      SMHighlighter.IsMouseOver = false;
      // Reset Tooltip active flag...
      ToolTipActive = false;
      _canShowToolTips = WindowSettings.ShowToolTips && ShowToolTips;

      GUILayout.BeginVertical();
      GUI.enabled = true;
      //GUILayout.Label("Vessel Control Center", SMStyle.LabelTabHeader);
      GUILayout.Label(SmUtils.SmTags["#smloc_control_vessel_000"], SMStyle.LabelTabHeader);
      GUILayout.Label(WindowControl.TabRule, SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(WindowControl.GuiRuleWidth));
      string step = "start";
      try
      {
        int combineVesselcount = 0;
        // Display all Vessels Docked together
        // ReSharper disable once ForCanBeConvertedToForeach
        for (int v = 0; v < SMAddon.SmVessel.DockedVessels.Count; v++)
        {
          ModDockedVessel mdv = SMAddon.SmVessel.DockedVessels[v];
          GUI.enabled = mdv.IsDocked;
          
          GUILayout.BeginHorizontal();
          GUIContent content = new GUIContent("", SmUtils.SmTags["#smloc_control_vessel_tt_001"]); //"Include in list of vessels to combine into a single docked vessel"

          bool isChecked = mdv.Combine;
          // temporary commenting of comnbine code to allow release.  Will work for next version.
          //isChecked = GUILayout.Toggle(isChecked, content, GUILayout.Width(20));
          if (isChecked) combineVesselcount += 1;
          if (isChecked != mdv.Combine)
          {
            mdv.Combine = isChecked;
          }
          // temporary commenting to allow release.  Will work on comnbine code for next version.
          //Rect rect = GUILayoutUtility.GetLastRect();
          //if (Event.current.type == EventType.Repaint && _canShowToolTips)
          //  ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, scrollX);
          //if (Event.current.type == EventType.Repaint && rect.Contains(Event.current.mousePosition))
          //{
          //  SMHighlighter.SetMouseOverData(rect, scrollY, scrollX, tabBox.height, mdv,
          //    Event.current.mousePosition);
          //}

          content = new GUIContent(SmUtils.SmTags["#smloc_control_vessel_001"], SmUtils.SmTags["#smloc_control_vessel_tt_005"]);
          if (GUILayout.Button(content, GUILayout.Width(guiBtnWidth))) //"UnDock"
          {
            // close hatches If CLS applies
            if (SMConditions.IsClsEnabled()) CloseVesselHatches(mdv);

            // Decouple/undock selected vessel.
            UndockSelectedVessel(mdv);
          }
          Rect rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && _canShowToolTips)
            ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, scrollX);
          if (Event.current.type == EventType.Repaint && rect.Contains(Event.current.mousePosition))
          {
            SMHighlighter.SetMouseOverData(rect, scrollY, scrollX, WindowControl.TabBox.height, mdv,
              Event.current.mousePosition);
          }

          GUI.enabled = true;
          if (mdv.IsEditing)
            mdv.RenameVessel = GUILayout.TextField(mdv.RenameVessel, GUILayout.Width(guiLabelWidth - (guiBtnWidth + 5)));
          else
            GUILayout.Label($"{mdv.VesselInfo.name}", GUILayout.Width(guiLabelWidth));
          rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && rect.Contains(Event.current.mousePosition))
          {
            SMHighlighter.SetMouseOverData(rect, scrollY, scrollX, WindowControl.TabBox.height, mdv,
              Event.current.mousePosition);
          }

          // now editing buttons.
          content = mdv.IsEditing ? new GUIContent(SmUtils.SmTags["#smloc_control_vessel_002"], SmUtils.SmTags["#smloc_control_vessel_tt_002"]) : new GUIContent(SmUtils.SmTags["#smloc_control_vessel_003"], SmUtils.SmTags["#smloc_control_vessel_tt_003"]); // "Save" // "Saves the changes to the docked vessel name." // Edit // "Change the docked vessel name." 
          if (GUILayout.Button(content, GUILayout.Width(50)))
          {
            if (SMAddon.SmVessel.DockedVessels[v].IsEditing)
            {
              mdv.VesselInfo.name = mdv.RenameVessel;
              mdv.RenameVessel = null;
              mdv.IsEditing = false;
            }
            else
            {
              mdv.IsEditing = true;
              mdv.RenameVessel = SMAddon.SmVessel.DockedVessels[v].VesselInfo.name;
            }
          }
          rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && _canShowToolTips)
            ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, scrollX);
          if (Event.current.type == EventType.Repaint && rect.Contains(Event.current.mousePosition))
          {
            SMHighlighter.SetMouseOverData(rect, scrollY, scrollX, WindowControl.TabBox.height, mdv,
              Event.current.mousePosition);
          }

          if (mdv.IsEditing)
          {
            GUIContent cancelContent = new GUIContent(SmUtils.SmTags["#smloc_control_vessel_004"], SmUtils.SmTags["#smloc_control_vessel_tt_004"]); // "Cancel","Cancel changes to docked vessel name"
            if (GUILayout.Button(cancelContent, GUILayout.Width(guiBtnWidth)))
            {
              mdv.RenameVessel = null;
              mdv.IsEditing = false;
            }
          }
          rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && _canShowToolTips)
            ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, scrollX);
          if (Event.current.type == EventType.Repaint && rect.Contains(Event.current.mousePosition))
          {
            SMHighlighter.SetMouseOverData(rect, scrollY, scrollX, WindowControl.TabBox.height, mdv,
              Event.current.mousePosition);
          }
          GUILayout.EndHorizontal();
        }
        // update static count for control window action buttons.
        CombineVesselCount = combineVesselcount;
        // Display MouseOverHighlighting, if any
        SMHighlighter.MouseOverHighlight();
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage(
          $" in Vessels Tab at step {step}.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}",
          SmUtils.LogType.Error, true);
      }
      GUI.enabled = true;
      GUILayout.EndVertical();
    }

    internal static void CloseVesselHatches(ModDockedVessel mVessel)
    {
      // ReSharper disable once ForCanBeConvertedToForeach
      for (int x = 0; x < SMAddon.SmVessel.Hatches.Count; x++)
      {
        ModHatch iHatch = SMAddon.SmVessel.Hatches[x];
        if (!mVessel.VesselParts.Contains(iHatch.ClsPart.Part)) continue;
        if (iHatch.HatchOpen) iHatch.CloseHatch(true);
      }
    }

    internal static void UndockSelectedVessel(ModDockedVessel mVessel)
    {
      List<Part>.Enumerator parts = mVessel.VesselParts.GetEnumerator();
      while (parts.MoveNext())
      {
        if (parts.Current == null) continue;
        List<ModuleDockingNode>.Enumerator dockingNodes = parts.Current.FindModulesImplementing<ModuleDockingNode>().GetEnumerator();
        while (dockingNodes.MoveNext())
        {
          if (dockingNodes.Current == null) continue;
          if (dockingNodes.Current.otherNode == null) continue;
          dockingNodes.Current.Undock();
        }
        dockingNodes.Dispose();
      }
      parts.Dispose();
    }

    internal static void CombineSelectedVessels()
    {
      List<ModDockedVessel>.Enumerator mdv = SMAddon.SmVessel.DockedVessels.GetEnumerator();
      while (mdv.MoveNext())
      {
        if (mdv.Current == null) continue;
        if (!mdv.Current.Combine) continue;
        List<ModuleDockingNode>.Enumerator port;
     
        if (mdv.Current.Rootpart.FindModulesImplementing<ModuleDockingNode>().Any())
        {
          port = mdv.Current.Rootpart.FindModulesImplementing<ModuleDockingNode>().GetEnumerator();
          while (port.MoveNext())
          {
            if (port.Current == null) continue;
            if (port.Current.vesselInfo == null || port.Current.otherNode) continue;
            port.Current.vesselInfo = null;
            port.Current.otherNode.vesselInfo = null;
            port.Current.otherNode = null;
          }
          port.Dispose();
        }
        else
        {
          // Since the root part is not a docking port, we need to locate the docking port that is currently docked.
          List<Part>.Enumerator part = mdv.Current.VesselParts.GetEnumerator();
          while (part.MoveNext())
          {
            if (part.Current == null) continue;
            if (!part.Current.FindModulesImplementing<ModuleDockingNode>().Any()) continue;
            port = part.Current.FindModulesImplementing<ModuleDockingNode>().GetEnumerator();
            while (port.MoveNext())
            {
              if (port.Current == null) continue;
              if (port.Current.vesselInfo == null || port.Current.otherNode == null) continue;
              // TODO:  Sort out the coupling criteria.  with current setup, still have an undock on the tweakable.  and a null ref when clicking. 
              //  That makes sense since the other node is now null.  So part/module state is still not correct after combining.
              Part thisPart = port.Current.part;
              Part otherPart = port.Current.otherNode.part;

              thisPart.Couple(otherPart);
              otherPart.Couple(thisPart);

              port.Current.state = "PreAttached";
              port.Current.dockedPartUId = 0;
              port.Current.dockingNodeModuleIndex = 0;
              port.Current.Events["Undock"].active = false;
              port.Current.Events["Undock"].guiActive = false;
              port.Current.Events["UndockSameVessel"].active = false;
              port.Current.Events["Decouple"].active = true;
              port.Current.Events["Decouple"].guiActive = true;

              port.Current.otherNode.state = "PreAttached";
              port.Current.otherNode.dockedPartUId = 0;
              port.Current.otherNode.dockingNodeModuleIndex = 0;

              port.Current.otherNode.Events["Undock"].active = false;
              port.Current.Events["Undock"].guiActive = false;
              port.Current.otherNode.Events["UndockSameVessel"].active = false;
              port.Current.otherNode.Events["Decouple"].active = true;
              port.Current.Events["Decouple"].guiActive = true;

              port.Current.vesselInfo = (DockedVesselInfo)null;
              port.Current.otherNode.vesselInfo = (DockedVesselInfo) null;
              port.Current.otherNode.otherNode = (ModuleDockingNode) null;
              port.Current.otherNode = (ModuleDockingNode) null;

            }
            port.Dispose();
          }
          part.Dispose();
        } 
      }
      mdv.Dispose();
      SMAddon.FireEventTriggers();
    }

    internal static void ClearVesselCount()
    {
      List<ModDockedVessel>.Enumerator mdv = SMAddon.SmVessel.DockedVessels.GetEnumerator();
      while (mdv.MoveNext())
      {
        if (mdv.Current == null) continue;
        mdv.Current.Combine = false;
      }
      mdv.Dispose();
    }
  }
}
