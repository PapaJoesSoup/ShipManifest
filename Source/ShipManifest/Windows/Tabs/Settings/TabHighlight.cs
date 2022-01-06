using System.Collections.Generic;
using ShipManifest.InternalObjects;
using UnityEngine;

namespace ShipManifest.Windows.Tabs.Settings
{
  internal static class TabHighlight
  {
    internal static string StrFlowCost = "0";

    // GUI tooltip and label support
    private static Rect _rect;
    private const float guiRuleWidth = 350;
    private const float guiToggleWidth = 300;
    private const float guiIndent = 20;

    internal static string ToolTip = "";
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;
    private static bool _canShowToolTips = true;

    internal static Rect Position = WindowSettings.Position;

    // Content strings
    internal static GUIContent titleContent     = new GUIContent(SmUtils.SmTags["#smloc_settings_config_000"]);
    internal static GUIContent modeAllContent   = new GUIContent(SmUtils.SmTags["#smloc_settings_highlight_001"], SmUtils.SmTags["#smloc_settings_highlight_tt_001"]);
    internal static GUIContent modeSTContent    = new GUIContent(SmUtils.SmTags["#smloc_settings_highlight_002"], SmUtils.SmTags["#smloc_settings_highlight_tt_002"]);
    internal static GUIContent modeClsContent   = new GUIContent(SmUtils.SmTags["#smloc_settings_highlight_003"], SmUtils.SmTags["#smloc_settings_highlight_tt_003"]);
    internal static GUIContent modeEdgeContent  = new GUIContent(SmUtils.SmTags["#smloc_settings_highlight_004"], SmUtils.SmTags["#smloc_settings_highlight_tt_004"]);


    internal static void Display(Vector2 displayViewerPosition)
    {
      // Reset Tooltip active flag...
      ToolTipActive = false;
      _canShowToolTips = WindowSettings.ShowToolTips && ShowToolTips;

      Position = WindowSettings.Position;
      int scrollX = 20;

      GUI.enabled = true;
      // Tab Title
      GUILayout.Label(titleContent, SMStyle.LabelTabHeader);
      GUILayout.Label(WindowSettings.TabRule, SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(guiRuleWidth));

      // EnableHighlighting Mode
      GUILayout.BeginHorizontal();
      // Enable Highlighting
      Curr.EnableHighlighting = GUILayout.Toggle(Curr.EnableHighlighting, modeAllContent, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      if (Curr.EnableHighlighting != Orig.EnableHighlighting && HighLogic.LoadedSceneIsFlight)
      {
        if (Curr.EnableCls)
        {
          if (SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Crew.ToString()))
          {
            // Update spaces and reassign the resource to observe new settings.
            SMHighlighter.HighlightClsVessel(Curr.EnableHighlighting, true);
            SMAddon.UpdateClsSpaces();
            SMAddon.SmVessel.SelectedResources.Clear();
            SMAddon.SmVessel.SelectedResources.Add(SMConditions.ResourceType.Crew.ToString());
          }
        }
      }

      // OnlySourceTarget Mode
      GUI.enabled = true;
      GUI.enabled = Curr.EnableHighlighting;
      GUILayout.BeginHorizontal();
      GUILayout.Space(guiIndent);
      // Highlight Only Source / Target Parts";
      Curr.OnlySourceTarget = GUILayout.Toggle(Curr.OnlySourceTarget, modeSTContent, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      if (Curr.OnlySourceTarget && (!Orig.OnlySourceTarget || Curr.EnableClsHighlighting))
      {
        Curr.EnableClsHighlighting = false;
        if (HighLogic.LoadedSceneIsFlight && Curr.EnableCls &&
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
      if (!Curr.EnableHighlighting || !Curr.EnableCls)
        GUI.enabled = false;
      else
        GUI.enabled = true;
      GUILayout.BeginHorizontal();
      GUILayout.Space(guiIndent);
      Curr.EnableClsHighlighting = GUILayout.Toggle(Curr.EnableClsHighlighting, modeClsContent, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      if (Curr.EnableClsHighlighting && (!Orig.EnableClsHighlighting || Curr.OnlySourceTarget))
        Curr.OnlySourceTarget = false;
      if (HighLogic.LoadedSceneIsFlight && Curr.EnableCls &&
          SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Crew.ToString()) &&
          WindowTransfer.ShowWindow)
      {
        if (Curr.EnableClsHighlighting != Orig.EnableClsHighlighting)
          SMHighlighter.HighlightClsVessel(Curr.EnableClsHighlighting);
      }

      // Enable Edge Highlighting Mode
      GUI.enabled = Curr.EnableHighlighting;
      GUILayout.BeginHorizontal();
      // Enable Edge Highlighting (On Mouse Overs)
      Curr.EnableEdgeHighlighting = GUILayout.Toggle(Curr.EnableEdgeHighlighting, modeEdgeContent, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      if (Curr.EnableEdgeHighlighting != Orig.EnableEdgeHighlighting && HighLogic.LoadedSceneIsFlight)
      {
        if (Curr.EnableEdgeHighlighting == false)
        {
          if (SMAddon.SmVessel.SelectedResources.Count > 0)
          {
            List<Part>.Enumerator parts = SMAddon.SmVessel.SelectedResourcesParts.GetEnumerator();
            while (parts.MoveNext())
            {
              if (parts.Current == null) continue;
              SMHighlighter.EdgeHighight(parts.Current, false);
            }
            parts.Dispose();
          }
        }
      }
      GUI.enabled = true;
    }
  }
}
