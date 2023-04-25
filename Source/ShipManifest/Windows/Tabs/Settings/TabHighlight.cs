using System.Collections.Generic;
using ShipManifest.InternalObjects;
using ShipManifest.InternalObjects.Settings;
using UnityEngine;

namespace ShipManifest.Windows.Tabs.Settings
{
  internal static class TabHighlight
  {

    static TabHighlight()
    {
      RefreshUIScale();
      toolTip = new ToolTip
      {
        Show = ShowToolTips
      };
    }

    // UIScale settings
    internal static float guiToggleWidth;
    internal static float guiIndent;

    // ToolTip vars
    internal static ToolTip toolTip;
    private static bool _showToolTips = true;
    internal static bool ShowToolTips
    {
      get => _showToolTips;
      set => _showToolTips = toolTip.Show = value;
    }

    internal static string StrFlowCost = "0";


    // Content strings
    internal static GUIContent titleContent     = new GUIContent(SmUtils.SmTags["#smloc_settings_highlight_000"]);
    internal static GUIContent modeAllContent   = new GUIContent(SmUtils.SmTags["#smloc_settings_highlight_001"], SmUtils.SmTags["#smloc_settings_highlight_tt_001"]);
    internal static GUIContent modeSTContent    = new GUIContent(SmUtils.SmTags["#smloc_settings_highlight_002"], SmUtils.SmTags["#smloc_settings_highlight_tt_002"]);
    internal static GUIContent modeClsContent   = new GUIContent(SmUtils.SmTags["#smloc_settings_highlight_003"], SmUtils.SmTags["#smloc_settings_highlight_tt_003"]);
    internal static GUIContent modeEdgeContent  = new GUIContent(SmUtils.SmTags["#smloc_settings_highlight_004"], SmUtils.SmTags["#smloc_settings_highlight_tt_004"]);


    internal static void Display(Vector2 displayViewerPosition)
    {

      // Reset Tooltip active flag...
      toolTip.Active = false;
      toolTip.CanShow = WindowSettings.ShowToolTips && ShowToolTips;

      int scrollX = 20;

      GUI.enabled = true;
      // Tab Title
      GUILayout.Label(titleContent, SMStyle.LabelTabHeader);
      GUILayout.Label(WindowSettings.TabRule, SMStyle.LabelStyleHardRule, GUILayout.Height(WindowSettings.GuiRuleHeight), GUILayout.Width(WindowSettings.GuiRuleWidth));

      // EnableHighlighting Mode
      // Enable Highlighting
      CurrSettings.EnableHighlighting = GuiUtils.DisplaySettingsToggle(CurrSettings.EnableHighlighting, modeAllContent,
        ref toolTip, guiToggleWidth, scrollX);
      if (CurrSettings.EnableHighlighting != OrigSettings.EnableHighlighting && HighLogic.LoadedSceneIsFlight)
      {
        if (CurrSettings.EnableCls)
        {
          if (SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Crew.ToString()))
          {
            // Update spaces and reassign the resource to observe new settings.
            SMHighlighter.HighlightClsVessel(CurrSettings.EnableHighlighting, true);
            SMAddon.UpdateClsSpaces();
            SMAddon.SmVessel.SelectedResources.Clear();
            SMAddon.SmVessel.SelectedResources.Add(SMConditions.ResourceType.Crew.ToString());
          }
        }
      }

      // OnlySourceTarget Mode
      GUI.enabled = true;
      GUI.enabled = CurrSettings.EnableHighlighting;
      GUILayout.BeginHorizontal();
      GUILayout.Space(guiIndent);
      // Highlight Only Source / Target Parts";
      CurrSettings.OnlySourceTarget = GuiUtils.DisplaySettingsToggle(CurrSettings.OnlySourceTarget, modeSTContent,
        ref toolTip, guiToggleWidth, scrollX);
      GUILayout.EndHorizontal();
      if (CurrSettings.OnlySourceTarget && (!OrigSettings.OnlySourceTarget || CurrSettings.EnableClsHighlighting))
      {
        CurrSettings.EnableClsHighlighting = false;
        if (HighLogic.LoadedSceneIsFlight && CurrSettings.EnableCls &&
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
      if (!CurrSettings.EnableHighlighting || !CurrSettings.EnableCls)
        GUI.enabled = false;
      else
        GUI.enabled = true;
      GUILayout.BeginHorizontal();
      GUILayout.Space(guiIndent);
      CurrSettings.EnableClsHighlighting = GuiUtils.DisplaySettingsToggle(CurrSettings.EnableClsHighlighting, modeClsContent,
        ref toolTip, guiToggleWidth, scrollX);
      GUILayout.EndHorizontal();
      if (CurrSettings.EnableClsHighlighting && (!OrigSettings.EnableClsHighlighting || CurrSettings.OnlySourceTarget))
        CurrSettings.OnlySourceTarget = false;
      if (HighLogic.LoadedSceneIsFlight && CurrSettings.EnableCls &&
          SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Crew.ToString()) &&
          WindowTransfer.ShowWindow)
      {
        if (CurrSettings.EnableClsHighlighting != OrigSettings.EnableClsHighlighting)
          SMHighlighter.HighlightClsVessel(CurrSettings.EnableClsHighlighting);
      }

      // Enable Edge Highlighting Mode
      GUI.enabled = CurrSettings.EnableHighlighting;
      CurrSettings.EnableEdgeHighlighting = GuiUtils.DisplaySettingsToggle(CurrSettings.EnableEdgeHighlighting, modeEdgeContent,
        ref toolTip, guiToggleWidth, scrollX);
      if (CurrSettings.EnableEdgeHighlighting != OrigSettings.EnableEdgeHighlighting && HighLogic.LoadedSceneIsFlight)
      {
        if (CurrSettings.EnableEdgeHighlighting == false)
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

    internal static void RefreshUIScale()
    {
      guiToggleWidth = 300 * CurrSettings.CurrentUIScale;
      guiIndent = 20 * CurrSettings.CurrentUIScale;
    }

  }
}
