using System;
using System.Collections.Generic;
using System.Linq;
using ConnectedLivingSpace;
using ShipManifest.InternalObjects;
using ShipManifest.Modules;
using UnityEngine;

namespace ShipManifest.Windows.Tabs.Control
{
  internal static class TabVessel
  {
    internal static string ToolTip = "";
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;

    private const float guiRuleWidth = 350;
    private const float guiLabelWidth = 230;
    private const float guiBtnWidth = 60;

    internal static void Display(Vector2 displayViewerPosition)
    {
      //float scrollX = WindowControl.Position.x + 20;
      //float scrollY = WindowControl.Position.y + 50 - displayViewerPosition.y;
      float scrollX = 20;
      float scrollY = displayViewerPosition.y;

      // Reset Tooltip active flag...
      ToolTipActive = false;
      SMHighlighter.IsMouseOver = false;

      GUILayout.BeginVertical();
      GUI.enabled = true;
      //GUILayout.Label("Vessel Control Center", SMStyle.LabelTabHeader);
      GUILayout.Label(SmUtils.Localize("#smloc_control_vessel_000"), SMStyle.LabelTabHeader);
      GUILayout.Label("____________________________________________________________________________________________",
        SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(guiRuleWidth));
      string step = "start";
      try
      {
        // Display all Vessels Docked together
        // ReSharper disable once ForCanBeConvertedToForeach
        for (int v = 0; v < SMAddon.SmVessel.DockedVessels.Count; v++)
        {
          GUI.enabled = SMAddon.SmVessel.DockedVessels[v].IsDocked;

          GUILayout.BeginHorizontal();
          if (GUILayout.Button("UnDock", GUILayout.Width(guiBtnWidth)))
          {
            // close hatches If CLS applies
            if (SMConditions.IsClsEnabled()) CloseVesselHatches(SMAddon.SmVessel.DockedVessels[v]);

            // Decouple/undock selected vessel.
            UndockSelectedVessel(SMAddon.SmVessel.DockedVessels[v]);
          }
          Rect rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && rect.Contains(Event.current.mousePosition))
            SMHighlighter.SetMouseOverData(rect, scrollY, scrollX, WindowControl.TabBox.height, SMAddon.SmVessel.DockedVessels[v], Event.current.mousePosition);
          GUI.enabled = true;
          if (SMAddon.SmVessel.DockedVessels[v].IsEditing)
            SMAddon.SmVessel.DockedVessels[v].renameVessel = GUILayout.TextField(SMAddon.SmVessel.DockedVessels[v].renameVessel, GUILayout.Width(guiLabelWidth - (guiBtnWidth + 5)));
          else GUILayout.Label($"{SMAddon.SmVessel.DockedVessels[v].VesselInfo.name}", GUILayout.Width(guiLabelWidth));
          rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && rect.Contains(Event.current.mousePosition))
            SMHighlighter.SetMouseOverData(rect, scrollY, scrollX, WindowControl.TabBox.height, SMAddon.SmVessel.DockedVessels[v], Event.current.mousePosition);
          // now editing buttons.
          GUIContent content = SMAddon.SmVessel.DockedVessels[v].IsEditing ? new GUIContent("Save", "Saves the changes to the docked vessel name.") : new GUIContent("Edit", "Change the docked vessel name.");
          if (GUILayout.Button(content, GUILayout.Width(50)))
          {
            if (SMAddon.SmVessel.DockedVessels[v].IsEditing)
            {
              SMAddon.SmVessel.DockedVessels[v].VesselInfo.name = SMAddon.SmVessel.DockedVessels[v].renameVessel;
              SMAddon.SmVessel.DockedVessels[v].renameVessel = null;
              SMAddon.SmVessel.DockedVessels[v].IsEditing = false;
            }
            else
            {
              SMAddon.SmVessel.DockedVessels[v].IsEditing = true;
              SMAddon.SmVessel.DockedVessels[v].renameVessel = SMAddon.SmVessel.DockedVessels[v].VesselInfo.name;

            }
          }
          if (SMAddon.SmVessel.DockedVessels[v].IsEditing)
          {
            GUIContent cancelContent = new GUIContent("Cancel","Cancel changes to docked vessel name");
            if (GUILayout.Button(cancelContent, GUILayout.Width(guiBtnWidth)))
            {
              SMAddon.SmVessel.DockedVessels[v].renameVessel = null;
              SMAddon.SmVessel.DockedVessels[v].IsEditing = false;
            }
          }
          GUILayout.EndHorizontal();
        }
        
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
  }
}