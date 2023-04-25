using ShipManifest.APIClients;
using ShipManifest.InternalObjects.Settings;
using UnityEngine;

namespace ShipManifest.Windows.Tabs.Settings
{
  internal static class TabConfig
  {

    static TabConfig()
    {
      RefreshUIScale();
      toolTip = new ToolTip
      {
        Show = ShowToolTips
      };
    }

    // UIScale settings
    internal static float guiMaintoggleWidth;
    internal static float guiLabelWidth;

    // ToolTip vars
    internal static ToolTip toolTip;
    private static bool _showToolTips = true;
    internal static bool ShowToolTips {
      get => _showToolTips;
      set => _showToolTips = toolTip.Show = value;
    }

    internal static string TxtSaveInterval = CurrSettings.SaveIntervalSec.ToString();

    // Content strings
    internal static GUIContent titleContent         = new GUIContent(SmUtils.SmTags["#smloc_settings_config_000"]);
    internal static GUIContent blizzyContent        = new GUIContent(SmUtils.SmTags["#smloc_settings_config_001"], SmUtils.SmTags["#smloc_settings_config_tt_001"]);
    internal static GUIContent unityStyleContent    = new GUIContent(SmUtils.SmTags["#smloc_settings_config_002"], SmUtils.SmTags["#smloc_settings_config_tt_002"]);
    internal static GUIContent debugContent         = new GUIContent(SmUtils.SmTags["#smloc_settings_config_003"], SmUtils.SmTags["#smloc_settings_config_tt_003"]);
    internal static GUIContent loggingContent       = new GUIContent(SmUtils.SmTags["#smloc_settings_config_004"], SmUtils.SmTags["#smloc_settings_config_tt_004"]);
    internal static GUIContent smErrorContent       = new GUIContent(SmUtils.SmTags["#smloc_settings_config_005"], SmUtils.SmTags["#smloc_settings_config_tt_005"]);
    internal static GUIContent logExitContent       = new GUIContent(SmUtils.SmTags["#smloc_settings_config_006"], SmUtils.SmTags["#smloc_settings_config_tt_006"]);
    internal static GUIContent logLengthContent     = new GUIContent($"{SmUtils.SmTags["#smloc_settings_config_007"]}:", SmUtils.SmTags["#smloc_settings_config_tt_007"]);
    internal static GUIContent autoSaveContent      = new GUIContent(SmUtils.SmTags["#smloc_settings_config_008"], SmUtils.SmTags["#smloc_settings_config_tt_008"]);
    internal static GUIContent saveIntervalContent  = new GUIContent($"{SmUtils.SmTags["#smloc_settings_config_009"]}:", SmUtils.SmTags["#smloc_settings_config_tt_009"]);
    internal static GUIContent secondsContent       = new GUIContent(SmUtils.SmTags["#smloc_settings_config_010"]);
    internal static GUIContent linesContent         = new GUIContent(SmUtils.SmTags["#smloc_settings_config_011"]);
    internal static GUIContent settingsIconContent  = new GUIContent(SmUtils.SmTags["#smloc_settings_config_012"], SmUtils.SmTags["#smloc_settings_config_tt_010"]);

    internal static void Display(Vector2 displayViewerPosition)
    {

      // Reset Tooltip active flag...
      toolTip.Active = false;
      toolTip.CanShow = WindowSettings.ShowToolTips && toolTip.Show;

      int scrollX = 20;

      //Configuration Title
      GUILayout.Label(titleContent, SMStyle.LabelTabHeader);
      GUILayout.Label(WindowSettings.TabRule, SMStyle.LabelStyleHardRule, GUILayout.Height(WindowSettings.GuiRuleHeight), GUILayout.Width(WindowSettings.GuiRuleWidth));

      if (!ToolbarManager.ToolbarAvailable)
      {
        if (CurrSettings.EnableBlizzyToolbar)
          CurrSettings.EnableBlizzyToolbar = false;
        GUI.enabled = false;
      }
      else
        GUI.enabled = true;

      // Enable Blizzy Toolbar (Replaces Stock Toolbar)";
      CurrSettings.EnableBlizzyToolbar = GuiUtils.DisplaySettingsToggle(CurrSettings.EnableBlizzyToolbar, blizzyContent,
        ref toolTip, guiMaintoggleWidth, scrollX);

      GUI.enabled = true;
      // Enable Settings Icon on Toolbar;
      CurrSettings.EnableSettingsIcon = GuiUtils.DisplaySettingsToggle(CurrSettings.EnableSettingsIcon, settingsIconContent,
        ref toolTip, guiMaintoggleWidth, scrollX);

      // UnityStyle Mode
      CurrSettings.UseUnityStyle = GuiUtils.DisplaySettingsToggle(CurrSettings.UseUnityStyle, unityStyleContent,
        ref toolTip, guiMaintoggleWidth, scrollX);
      if (CurrSettings.UseUnityStyle != OrigSettings.UseUnityStyle)
        SMStyle.WindowStyle = null;

      // Enable Debug Window;
      WindowDebugger.ShowWindow = GuiUtils.DisplaySettingsToggle(WindowDebugger.ShowWindow, debugContent,
        ref toolTip, guiMaintoggleWidth, scrollX);

      // Enable Verbose Logging
      CurrSettings.VerboseLogging = GuiUtils.DisplaySettingsToggle(CurrSettings.VerboseLogging, loggingContent,
        ref toolTip, guiMaintoggleWidth, scrollX);

      // Enable SM Debug Window On Error
      CurrSettings.AutoDebug = GuiUtils.DisplaySettingsToggle(CurrSettings.AutoDebug, smErrorContent,
        ref toolTip, guiMaintoggleWidth, scrollX);

      // Save Error log on Exit
      CurrSettings.SaveLogOnExit = GuiUtils.DisplaySettingsToggle(CurrSettings.SaveLogOnExit, logExitContent,
        ref toolTip, guiMaintoggleWidth, scrollX);

      //Error Log Length:
      CurrSettings.ErrorLogLength = GuiUtils.DisplaySettingsTextField(CurrSettings.ErrorLogLength, logLengthContent,
        guiLabelWidth, 50, linesContent, 40, toolTip, scrollX);

      // Enable AutoSave Settings
      CurrSettings.AutoSave = GuiUtils.DisplaySettingsToggle(CurrSettings.AutoSave, autoSaveContent, 
        ref toolTip, guiMaintoggleWidth, scrollX);

      // Save Interval Settings
      TxtSaveInterval = GuiUtils.DisplaySettingsTextField(TxtSaveInterval, saveIntervalContent,
        guiLabelWidth, 50, secondsContent, 40, toolTip, scrollX);
    }

    internal static void RefreshUIScale()
    {
      guiMaintoggleWidth = 300 * CurrSettings.CurrentUIScale;
      guiLabelWidth = 110 * CurrSettings.CurrentUIScale;
    }

  }
}
