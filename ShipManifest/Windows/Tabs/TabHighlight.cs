using System.Collections.Generic;
using ShipManifest.InternalObjects;
using UnityEngine;

namespace ShipManifest.Windows.Tabs
{
  internal static class TabHighlight
  {
    internal static string StrFlowCost = "0";

    // GUI tooltip and label support
    private static string _toolTip = "";
    private static Rect _rect;
    private static string _label = "";
    private static GUIContent _guiLabel;

    internal static string ToolTip = "";
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;
    private static bool _canShowToolTips = true;

    internal static Rect Position = WindowSettings.Position;

    internal static void Display(Vector2 displayViewerPosition)
    {
      // Reset Tooltip active flag...
      ToolTipActive = false;
      _canShowToolTips = WindowSettings.ShowToolTips && ShowToolTips;

      Position = WindowSettings.Position;
      int scrollX = 20;

      GUI.enabled = true;
      GUILayout.Label("Highlighting", SMStyle.LabelTabHeader);
      GUILayout.Label("____________________________________________________________________________________________",
        SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(350));

      // EnableHighlighting Mode
      GUILayout.BeginHorizontal();
      _label = "Enable Highlighting";
      _toolTip = "Enables highlighting of all parts that contain the resource(s) selected in the Manifest Window.";
      _toolTip += "\r\nThis is a global setting.  Does not affect mouseover highlighting.";
      _guiLabel = new GUIContent(_label, _toolTip);
      SMSettings.EnableHighlighting = GUILayout.Toggle(SMSettings.EnableHighlighting, _guiLabel, GUILayout.Width(300));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      if (SMSettings.EnableHighlighting != SMSettings.PrevEnableHighlighting && HighLogic.LoadedSceneIsFlight)
      {
        if (SMSettings.EnableCls)
        {
          if (SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Crew.ToString()))
          {
            // Update spaces and reassign the resource to observe new settings.
            SMHighlighter.HighlightClsVessel(SMSettings.EnableHighlighting, true);
            SMAddon.UpdateClsSpaces();
            SMAddon.SmVessel.SelectedResources.Clear();
            SMAddon.SmVessel.SelectedResources.Add(SMConditions.ResourceType.Crew.ToString());
          }
        }
      }

      // OnlySourceTarget Mode
      GUI.enabled = true;
      GUI.enabled = SMSettings.EnableHighlighting;
      GUILayout.BeginHorizontal();
      GUILayout.Space(20);
      _label = "Highlight Only Source / Target Parts";
      _toolTip = "Disables general highlighting of parts for a selected Resource or resources.";
      _toolTip += "\r\nRestricts highlighting of parts to only the part or parts selected in the Transfer Window.";
      _toolTip += "\r\nRequires 'Enable Highlighting' to be On.";
      _guiLabel = new GUIContent(_label, _toolTip);
      SMSettings.OnlySourceTarget = GUILayout.Toggle(SMSettings.OnlySourceTarget, _guiLabel, GUILayout.Width(300));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      if (SMSettings.OnlySourceTarget && (!SMSettings.PrevOnlySourceTarget || SMSettings.EnableClsHighlighting))
      {
        SMSettings.EnableClsHighlighting = false;
        if (HighLogic.LoadedSceneIsFlight && SMSettings.EnableCls &&
            SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Crew.ToString()))
        {
          // Update spaces and reassign the resource to observe new settings.
          SMHighlighter.HighlightClsVessel(false, true);
          SMAddon.UpdateClsSpaces();
          SMAddon.SmVessel.SelectedResources.Clear();
          SMAddon.SmVessel.SelectedResources.Add(SMConditions.ResourceType.Crew.ToString());
        }
      }
      // Enable CLS Highlighting Mode
      if (!SMSettings.EnableHighlighting || !SMSettings.EnableCls)
        GUI.enabled = false;
      else
        GUI.enabled = true;
      GUILayout.BeginHorizontal();
      GUILayout.Space(20);
      _label = "Enable CLS Highlighting";
      _toolTip = "Changes highlighting behavior if CLS is enabled & Crew selected in Manifest Window.";
      _toolTip += "\r\nHighlights the parts associated with livable/passable spaces on vessel.";
      _toolTip += "\r\nRequires 'Enable Highlighting' to be On and is mutually exclusive with ";
      _toolTip += "\r\n'Highlight Only Source / Target Parts'.";
      _guiLabel = new GUIContent(_label, _toolTip);
      SMSettings.EnableClsHighlighting = GUILayout.Toggle(SMSettings.EnableClsHighlighting, _guiLabel,
        GUILayout.Width(300));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      if (SMSettings.EnableClsHighlighting && (!SMSettings.PrevEnableClsHighlighting || SMSettings.OnlySourceTarget))
        SMSettings.OnlySourceTarget = false;
      if (HighLogic.LoadedSceneIsFlight && SMSettings.EnableCls &&
          SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Crew.ToString()) &&
          WindowTransfer.ShowWindow)
      {
        if (SMSettings.EnableClsHighlighting != SMSettings.PrevEnableClsHighlighting)
          SMHighlighter.HighlightClsVessel(SMSettings.EnableClsHighlighting);
      }

      // Enable Edge Highlighting Mode
      GUI.enabled = SMSettings.EnableHighlighting;
      GUILayout.BeginHorizontal();
      _label = "Enable Edge Highlighting (On Mouse Overs)";
      _toolTip = "Changes highlighting behavior when you mouseover a part button in Transfer Window.";
      _toolTip += "\r\nCauses the edge of the part to glow, making it easier to see.";
      _toolTip += "\r\nRequires Edge Highlighting to be enabled in the KSP Game settings.";
      _guiLabel = new GUIContent(_label, _toolTip);
      SMSettings.EnableEdgeHighlighting = GUILayout.Toggle(SMSettings.EnableEdgeHighlighting, _guiLabel,
        GUILayout.Width(300));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      if (SMSettings.EnableEdgeHighlighting != SMSettings.PrevEnableEdgeHighlighting && HighLogic.LoadedSceneIsFlight)
      {
        if (SMSettings.EnableEdgeHighlighting == false)
        {
          if (SMAddon.SmVessel.SelectedResources.Count > 0)
          {
            List<Part>.Enumerator parts = SMAddon.SmVessel.SelectedResourcesParts.GetEnumerator();
            while (parts.MoveNext())
            {
              if (parts.Current == null) continue;
              SMHighlighter.EdgeHighight(parts.Current, false);
            }
          }
        }
      }
      GUI.enabled = true;
    }
  }
}