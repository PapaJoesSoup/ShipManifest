using System.Globalization;
using ShipManifest.InternalObjects;
using ShipManifest.InternalObjects.Settings;
using UnityEngine;
using static GUIUtil;

namespace ShipManifest.Windows.Tabs.Settings
{
  internal static class TabRealism
  {

    static TabRealism()
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
    internal static float guiLabelWidth;
    internal static float guiSliderWidth;
    internal static float guiSliderMinWidth;
    internal static float guiSliderMaxWidth;


    // GUI tooltip and label support
    internal static ToolTip toolTip;
    private static bool _showToolTips = true;
    internal static bool ShowToolTips
    {
      get => _showToolTips;
      set => _showToolTips = toolTip.Show = value;
    }

    internal static string StrFlowCost = "0";


    // Content strings
    internal static GUIContent titleContent         = new GUIContent(SmUtils.SmTags["#smloc_settings_config_000"]);
    internal static GUIContent optionsContent       = new GUIContent(SmUtils.SmTags["#smloc_settings_realism_001"]);
    internal static GUIContent lockContent          = new GUIContent(SmUtils.SmTags["#smloc_settings_realism_002"], SmUtils.SmTags["#smloc_settings_realism_tt_001"]);
    internal static GUIContent realXferContent      = new GUIContent(SmUtils.SmTags["#smloc_settings_realism_008"], SmUtils.SmTags["#smloc_settings_realism_tt_007"]);
    internal static GUIContent realControlContent   = new GUIContent(SmUtils.SmTags["#smloc_settings_realism_009"], SmUtils.SmTags["#smloc_settings_realism_tt_008"]);
    internal static GUIContent crewModifyContent    = new GUIContent(SmUtils.SmTags["#smloc_settings_realism_010"], SmUtils.SmTags["#smloc_settings_realism_tt_009"]);
    internal static GUIContent crewRenameContent    = new GUIContent(SmUtils.SmTags["#smloc_settings_realism_011"], SmUtils.SmTags["#smloc_settings_realism_tt_010"]);
    internal static GUIContent professionContent    = new GUIContent(SmUtils.SmTags["#smloc_settings_realism_012"], SmUtils.SmTags["#smloc_settings_realism_tt_011"]);
    internal static GUIContent stkCrewXferContent   = new GUIContent(SmUtils.SmTags["#smloc_settings_realism_013"], SmUtils.SmTags["#smloc_settings_realism_tt_012"]);
    internal static GUIContent smCrewXfersContent   = new GUIContent(SmUtils.SmTags["#smloc_settings_realism_014"], SmUtils.SmTags["#smloc_settings_realism_tt_013"]);
    internal static GUIContent pfCrewContent        = new GUIContent(SmUtils.SmTags["#smloc_settings_realism_015"], SmUtils.SmTags["#smloc_settings_realism_tt_014"]);
    internal static GUIContent orStkCrewXferContent = new GUIContent(SmUtils.SmTags["#smloc_settings_realism_016"], SmUtils.SmTags["#smloc_settings_realism_tt_015"]);
    internal static GUIContent clsContent           = new GUIContent(SmUtils.SmTags["#smloc_settings_realism_017"], SmUtils.SmTags["#smloc_settings_realism_tt_016"]);
    internal static GUIContent clsAllowXferContent  = new GUIContent(SmUtils.SmTags["#smloc_settings_realism_018"], SmUtils.SmTags["#smloc_settings_realism_tt_017"]);
    internal static GUIContent modeScienceContent   = new GUIContent(SmUtils.SmTags["#smloc_settings_realism_019"], SmUtils.SmTags["#smloc_settings_realism_tt_018"]);
    internal static GUIContent modeResourcesContent = new GUIContent(SmUtils.SmTags["#smloc_settings_realism_020"], SmUtils.SmTags["#smloc_settings_realism_tt_019"]);
    internal static GUIContent pfResourcesContent   = new GUIContent(SmUtils.SmTags["#smloc_settings_realism_021"], SmUtils.SmTags["#smloc_settings_realism_tt_020"]);
    internal static GUIContent xferCostOnContent    = new GUIContent(SmUtils.SmTags["#smloc_settings_realism_022"], SmUtils.SmTags["#smloc_settings_realism_tt_021"]);
    internal static GUIContent xferCostAmtContent   = new GUIContent($"{SmUtils.SmTags["#smloc_settings_realism_023"]}:", SmUtils.SmTags["#smloc_settings_realism_tt_022"]);
    internal static GUIContent ecUnitContent        = new GUIContent(SmUtils.SmTags["#smloc_settings_realism_024"], SmUtils.SmTags["#smloc_settings_realism_tt_022"]);
    internal static GUIContent flowRateContent      = new GUIContent($"{SmUtils.SmTags["#smloc_settings_realism_025"]}:", SmUtils.SmTags["#smloc_settings_realism_tt_023"]);
    internal static GUIContent unitsSecContent      = new GUIContent(SmUtils.SmTags["#smloc_settings_realism_026"], SmUtils.SmTags["#smloc_settings_realism_tt_023"]);
    internal static GUIContent flowSliderContent    = new GUIContent(CurrSettings.MaxFlowRate.ToString(CultureInfo.InvariantCulture), SmUtils.SmTags["#smloc_settings_realism_tt_024"]);
    internal static GUIContent minRateContent       = new GUIContent($" - {SmUtils.SmTags["#smloc_settings_realism_027"]}:", SmUtils.SmTags["#smloc_settings_realism_tt_025"]);
    internal static GUIContent unitSecMinContent    = new GUIContent(SmUtils.SmTags["#smloc_settings_realism_026"], SmUtils.SmTags["#smloc_settings_realism_tt_025"]);
    internal static GUIContent maxRateContent       = new GUIContent($" - {SmUtils.SmTags["#smloc_settings_realism_028"]}:", SmUtils.SmTags["#smloc_settings_realism_tt_026"]);
    internal static GUIContent unitSecMaxContent    = new GUIContent(SmUtils.SmTags["#smloc_settings_realism_026"], SmUtils.SmTags["#smloc_settings_realism_tt_026"]);
    internal static GUIContent maxFlowTimeContent   = new GUIContent($" - {SmUtils.SmTags["#smloc_settings_realism_029"]}:", SmUtils.SmTags["#smloc_settings_realism_tt_027"]);
    internal static GUIContent unitFlowTimeContent   = new GUIContent(SmUtils.SmTags["#smloc_settings_realism_030"], SmUtils.SmTags["#smloc_settings_realism_tt_027"]);
    internal static GUIContent lockSettingContent   = new GUIContent(SmUtils.SmTags["#smloc_settings_realism_031"], SmUtils.SmTags["#smloc_settings_realism_tt_028"]);
    internal static GUIContent option1Content       = new GUIContent(SmUtils.SmTags["#smloc_settings_realism_004"], SmUtils.SmTags["#smloc_settings_realism_tt_003"]);
    internal static GUIContent option2Content       = new GUIContent(SmUtils.SmTags["#smloc_settings_realism_005"], SmUtils.SmTags["#smloc_settings_realism_tt_004"]);
    internal static GUIContent option3Content       = new GUIContent(SmUtils.SmTags["#smloc_settings_realism_006"], SmUtils.SmTags["#smloc_settings_realism_tt_005"]);
    internal static GUIContent option4Content       = new GUIContent(SmUtils.SmTags["#smloc_settings_realism_007"], SmUtils.SmTags["#smloc_settings_realism_tt_006"]);
    internal static GUIContent modeOptionContent    = new GUIContent($"{SmUtils.SmTags["#smloc_settings_realism_003"]}:", SmUtils.SmTags["#smloc_settings_realism_tt_002"]);
    internal static GUIContent realCrewXferContent  = new GUIContent(SmUtils.SmTags["#smloc_settings_realism_032"], SmUtils.SmTags["#smloc_settings_realism_tt_029"]);

    internal static void Display(Vector2 displayViewerPosition)
    {

      // Reset Tooltip active flag...
      toolTip.Active = false;
      toolTip.CanShow = WindowSettings.ShowToolTips && ShowToolTips;

      int scrollX = 20;

      GUI.enabled = true;
      GUILayout.Label(!CurrSettings.LockSettings ? optionsContent : lockContent, SMStyle.LabelTabHeader);
      GUILayout.Label(WindowSettings.TabRule, SMStyle.LabelStyleHardRule, GUILayout.Height(WindowSettings.GuiRuleHeight), GUILayout.Width(WindowSettings.GuiRuleWidth + WindowSettings.WidthScale));

      bool isEnabled = !CurrSettings.LockSettings;
      //RealismMode Buttons.
      DisplayRealismButtons();

      GUILayout.Label(WindowSettings.TabRule, SMStyle.LabelStyleHardRule, GUILayout.Height(WindowSettings.GuiRuleHeight), GUILayout.Width(WindowSettings.GuiRuleWidth + WindowSettings.WidthScale));


      // RealXfers Mode
      GUI.enabled = isEnabled;
      // Realistic Transfers;
      CurrSettings.RealXfers = GuiUtils.DisplaySettingsToggle(CurrSettings.RealXfers, realXferContent,
        ref toolTip, guiToggleWidth + WindowSettings.WidthScale, scrollX);

      // RealControl Mode
      GUI.enabled = isEnabled;
      // Realistic Control
      CurrSettings.RealControl = GuiUtils.DisplaySettingsToggle(CurrSettings.RealControl, realControlContent,
        ref toolTip, guiToggleWidth + WindowSettings.WidthScale, scrollX);

      // EnableCrew Modifications
      GUI.enabled = isEnabled;
      CurrSettings.EnableCrewModify = GuiUtils.DisplaySettingsToggle(CurrSettings.EnableCrewModify, crewModifyContent,
        ref toolTip, guiToggleWidth + WindowSettings.WidthScale, scrollX);

      // Enable Kerbal Renaming
      CurrSettings.EnableKerbalRename = GuiUtils.DisplaySettingsToggle(CurrSettings.EnableKerbalRename, crewRenameContent,
        ref toolTip, guiToggleWidth + WindowSettings.WidthScale, scrollX);

      //Enable Profession management
      CurrSettings.EnableChangeProfession = GuiUtils.DisplaySettingsToggle(CurrSettings.EnableChangeProfession, professionContent,
        ref toolTip, guiToggleWidth + WindowSettings.WidthScale, scrollX);
      GUI.enabled = true;

      // Enable stock Crew Xfers
      GUI.enabled = CurrSettings.EnableCrew && isEnabled;
      // Enable Stock Crew Xfers;
      CurrSettings.EnableStockCrewXfer = GuiUtils.DisplaySettingsToggle(CurrSettings.EnableStockCrewXfer, stkCrewXferContent,
        ref toolTip, guiToggleWidth + WindowSettings.WidthScale, scrollX);

      // EnableCrew Xfer Mode
      GUI.enabled = isEnabled;
      // Enable SM Crew Xfers
      CurrSettings.EnableCrew = GuiUtils.DisplaySettingsToggle(CurrSettings.EnableCrew, smCrewXfersContent,
        ref toolTip, guiToggleWidth + WindowSettings.WidthScale, scrollX);
      if (!CurrSettings.EnableCrew && HighLogic.LoadedSceneIsFlight)
      {
        if (SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Crew.ToString()))
        {
          // Clear Resource selection.
          SMHighlighter.ClearResourceHighlighting(SMAddon.SmVessel.SelectedResourcesParts);
          SMAddon.SmVessel.SelectedResources.Clear();
          WindowTransfer.ShowWindow = false;
        }
      }

      // Set Gui.enabled for child settings to resources...
      GUI.enabled = CurrSettings.EnableCrew && isEnabled;

      // Realistic Crew Xfer Mode
      GUILayout.BeginHorizontal();
      //Turns on/off Realistic Resource Transfers.
      GUILayout.Space(guiIndent);
      CurrSettings.RealCrewXfers = GuiUtils.DisplaySettingsToggle(CurrSettings.RealCrewXfers, realCrewXferContent,
        ref toolTip, guiToggleWidth + WindowSettings.WidthScale, scrollX);
      GUILayout.EndHorizontal();

      // EnablePFResources Mode
      GUILayout.BeginHorizontal();
      //Enable Crew Fill and Empty Ops in Pre-Flight
      GUILayout.Space(guiIndent);
      CurrSettings.EnablePfCrews = GuiUtils.DisplaySettingsToggle(CurrSettings.EnablePfCrews, pfCrewContent,
        ref toolTip, guiToggleWidth + WindowSettings.WidthScale, scrollX);
      GUILayout.EndHorizontal();

      // Enable stock Crew Xfer Override
      GUI.enabled = CurrSettings.EnableCrew && isEnabled && CurrSettings.EnableStockCrewXfer;
      //Override Stock Crew Xfers
      GUILayout.BeginHorizontal();
      GUILayout.Space(guiIndent);
      CurrSettings.OverrideStockCrewXfer = GuiUtils.DisplaySettingsToggle(CurrSettings.OverrideStockCrewXfer, orStkCrewXferContent,
        ref toolTip, guiToggleWidth + WindowSettings.WidthScale, scrollX);
      GUILayout.EndHorizontal();

      // EnableCLS Mode
      GUILayout.BeginHorizontal();
      GUILayout.Space(guiIndent);
      if (!CurrSettings.EnableCrew || !SMSettings.ClsInstalled)
        GUI.enabled = false;
      else
        GUI.enabled = isEnabled;
      //Enable CLS  (Connected Living Spaces)";
      CurrSettings.EnableCls = GuiUtils.DisplaySettingsToggle(CurrSettings.EnableCls, clsContent,
        ref toolTip, guiToggleWidth + WindowSettings.WidthScale, scrollX);
      GUILayout.EndHorizontal();

      if (CurrSettings.EnableCls != OrigSettings.EnableCls && HighLogic.LoadedSceneIsFlight)
      {
        if (!CurrSettings.EnableCls)
          SMHighlighter.HighlightClsVessel(false, true);
        else if (SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Crew.ToString()))
        {
          // Update spaces and reassign the resource to observe new settings.
          SMAddon.UpdateClsSpaces();
          SMAddon.SmVessel.SelectedResources.Clear();
          SMAddon.SmVessel.SelectedResources.Add(SMConditions.ResourceType.Crew.ToString());
        }
      }

      // Enable CLS stock Crew Xfer Override
      if (!CurrSettings.EnableCrew || !SMSettings.ClsInstalled)
        GUI.enabled = false;
      else
        GUI.enabled = isEnabled;
      GUILayout.BeginHorizontal();
      GUILayout.Space(guiIndent);
      CurrSettings.EnableClsAllowTransfer = GuiUtils.DisplaySettingsToggle(CurrSettings.EnableClsAllowTransfer, clsAllowXferContent,
        ref toolTip, guiToggleWidth + WindowSettings.WidthScale, scrollX);
      GUILayout.EndHorizontal();

      // EnableScience Mode
      GUI.enabled = isEnabled;
      //Enable Science Xfers
      CurrSettings.EnableScience = GuiUtils.DisplaySettingsToggle(CurrSettings.EnableScience, modeScienceContent,
        ref toolTip, guiToggleWidth + WindowSettings.WidthScale, scrollX);
      if (!CurrSettings.EnableScience && HighLogic.LoadedSceneIsFlight)
      {
        // Clear Resource selection.
        if (SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Science.ToString()))
          SMAddon.SmVessel.SelectedResources.Clear();
      }

      // EnableResources Mode
      GUI.enabled = isEnabled;
      //Enable Resource Xfers
      CurrSettings.EnableResources = GuiUtils.DisplaySettingsToggle(CurrSettings.EnableResources, modeResourcesContent,
        ref toolTip, guiToggleWidth + WindowSettings.WidthScale, scrollX);
      if (!CurrSettings.EnableResources && HighLogic.LoadedSceneIsFlight)
      {
        // Clear Resource selection.
        if (SMAddon.SmVessel.SelectedResources.Count > 0 &&
            !SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Crew.ToString()) &&
            !SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Science.ToString()))
          SMAddon.SmVessel.SelectedResources.Clear();
      }

      // Set Gui.enabled for child settings to resources...
      GUI.enabled = CurrSettings.EnableResources && isEnabled;

      // EnablePFResources Mode
      GUILayout.BeginHorizontal();
      GUILayout.Space(guiIndent);
      //Enable Resources in Pre-Flight
      CurrSettings.EnablePfResources = GuiUtils.DisplaySettingsToggle(CurrSettings.EnablePfResources, pfResourcesContent,
        ref toolTip, guiToggleWidth + WindowSettings.WidthScale, scrollX);
      GUILayout.EndHorizontal();

      // EnableXferCost Mode
      GUILayout.BeginHorizontal();
      GUILayout.Space(guiIndent);
      // Resource Xfers Consume Power";
      CurrSettings.EnableXferCost = GuiUtils.DisplaySettingsToggle(CurrSettings.EnableXferCost, xferCostOnContent,
        ref toolTip, guiToggleWidth + WindowSettings.WidthScale, scrollX);
      GUILayout.EndHorizontal();

      // Resource Xfer EC cost
      GUI.enabled = !CurrSettings.LockSettings && CurrSettings.EnableResources && CurrSettings.EnableXferCost;
      GUILayout.BeginHorizontal();
      GUILayout.Space(guiIndent * 1.75f);
      // Xfer Power Cost:
      // Lets parse the string to allow decimal points.
      StrFlowCost = CurrSettings.FlowCost.ToString(CultureInfo.InvariantCulture);
      // add the decimal point if it was typed.
      StrFlowCost = SmUtils.GetStringDecimal(StrFlowCost);
      // add the zero if it was typed.
      StrFlowCost = SmUtils.GetStringZero(StrFlowCost);

      StrFlowCost = GuiUtils.DisplaySettingsTextField(StrFlowCost, xferCostAmtContent,
        guiLabelWidth - (guiIndent/2), 80 + WindowSettings.WidthScale, ecUnitContent, 80, toolTip, scrollX);
      GUILayout.EndHorizontal();

      // update decimal bool 
      SmUtils.SetStringDecimal(StrFlowCost);
      //update zero bool 
      SmUtils.SetStringZero(StrFlowCost);

      if (float.TryParse(StrFlowCost, out float newCost))
        CurrSettings.FlowCost = newCost;

      // create xfer Flow Rate slider;
      // Lets parse the string to allow decimal points.
      string strFlowRate = CurrSettings.FlowRate.ToString(CultureInfo.InvariantCulture);
      string strMinFlowRate = CurrSettings.MinFlowRate.ToString(CultureInfo.InvariantCulture);
      string strMaxFlowRate = CurrSettings.MaxFlowRate.ToString(CultureInfo.InvariantCulture);
      string strMaxFlowTime = CurrSettings.MaxFlowTimeSec.ToString();


      // Resource Flow Rate
      GUI.enabled = !CurrSettings.LockSettings && CurrSettings.EnableResources && CurrSettings.RealXfers;
      GUILayout.BeginHorizontal();
      GUILayout.Space(guiIndent * 1.25f);
      // Resource Flow Rate:
      strFlowRate = GuiUtils.DisplaySettingsTextField(strFlowRate, flowRateContent,
        guiLabelWidth, 80 + WindowSettings.WidthScale, unitsSecContent, 80, toolTip, scrollX);
      GUILayout.EndHorizontal();

      if (float.TryParse(strFlowRate, out float newRate))
        CurrSettings.FlowRate = (int)newRate;

      // Resource Flow rate Slider
      SliderData slider = new SliderData
      {
        minValue = CurrSettings.MinFlowRate,
        maxValue = CurrSettings.MaxFlowRate,
        setting =  CurrSettings.FlowRate,
        minContent = new GUIContent(CurrSettings.MinFlowRate.ToString(CultureInfo.InvariantCulture)),
        minWidth = guiSliderMinWidth,
        maxContent = flowSliderContent,
        maxWidth = guiSliderMaxWidth,
        sliderWidth = guiSliderWidth
      };

      GUILayout.BeginHorizontal();
      GUILayout.Space(guiIndent* 1.5f);
      CurrSettings.FlowRate = GuiUtils.DisplaySettingsSlider(slider, ref toolTip, scrollX);
      GUILayout.EndHorizontal();

      // Min Flow Rate for Slider
      GUILayout.BeginHorizontal();
      GUILayout.Space(guiIndent * 1.5f);
      // - Min Flow Rate:
      strMinFlowRate = GuiUtils.DisplaySettingsTextField(strMinFlowRate, minRateContent,
        guiLabelWidth, 80 + WindowSettings.WidthScale, unitSecMinContent, 80, toolTip, scrollX);
      if (float.TryParse(strMinFlowRate, out newRate))
        CurrSettings.MinFlowRate = (int) newRate;
      GUILayout.EndHorizontal();

      // Max Flow Rate for Slider
      GUILayout.BeginHorizontal();
      GUILayout.Space(guiIndent * 1.5f);
      // - Max Flow Rate:
      strMaxFlowRate = GuiUtils.DisplaySettingsTextField(strMaxFlowRate, maxRateContent,
        guiLabelWidth, 80 + WindowSettings.WidthScale, unitSecMinContent, 80, toolTip, scrollX);
      if (float.TryParse(strMaxFlowRate, out newRate))
        CurrSettings.MaxFlowRate = (int) newRate;
      GUILayout.EndHorizontal();

      // Max Flow Time 
      GUILayout.BeginHorizontal();
      GUILayout.Space(guiIndent * 1.5f);
      // - Max Flow Time:
      strMaxFlowTime = GuiUtils.DisplaySettingsTextField(strMaxFlowTime, maxFlowTimeContent,
        guiLabelWidth, 80 + WindowSettings.WidthScale, unitFlowTimeContent, 80, toolTip, scrollX);
      if (float.TryParse(strMaxFlowTime, out newRate))
        CurrSettings.MaxFlowTimeSec = (int) newRate;
      GUILayout.EndHorizontal();

      // reset gui.enabled to default
      GUI.enabled = true;
      GUILayout.Label(WindowSettings.TabRule, SMStyle.LabelStyleHardRule, GUILayout.Height(WindowSettings.GuiRuleHeight), GUILayout.Width(WindowSettings.GuiRuleWidth + WindowSettings.WidthScale));

      // LockSettings Mode
      GUI.enabled = isEnabled;
      // Lock Realism Settings  (If set ON, disable in config file)
      CurrSettings.LockSettings = GuiUtils.DisplaySettingsToggle(CurrSettings.LockSettings, lockSettingContent,
        ref toolTip, guiToggleWidth + WindowSettings.WidthScale, scrollX);
      GUI.enabled = true;
      GUILayout.Label(WindowSettings.TabRule, SMStyle.LabelStyleHardRule, GUILayout.Height(WindowSettings.GuiRuleHeight), GUILayout.Width(WindowSettings.GuiRuleWidth + WindowSettings.WidthScale));
    }

    private static void DisplayRealismButtons()
    {
      // RealismMode options
      GUIContent[] options =
      {
        option1Content,
        option2Content,
        option3Content,
        option4Content
      };
      GUIStyle[] styles =
      {
        CurrSettings.RealismMode == 0 ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle,
        CurrSettings.RealismMode == 1 ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle,
        CurrSettings.RealismMode == 2 ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle,
        CurrSettings.RealismMode == 3 ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle,
      };

      // Realism Mode Label
      Rect _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && toolTip.CanShow)
        toolTip.Desc = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref toolTip.Active, 10);

      GUI.enabled = true;
      GUILayout.BeginHorizontal();
      GUILayout.Label(modeOptionContent, SMStyle.LabelStyleNoWrap, GUILayout.Width(90 * CurrSettings.CurrentUIScale + WindowSettings.WidthScale));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && toolTip.CanShow)
        toolTip.Desc = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref toolTip.Active, 10);

      // Build Option Buttons
      CurrSettings.RealismMode = SMSettings.GetRealismMode();
      for (int x = 0; x <= 3; x++)
      {
        if (x == 3) GUI.enabled = false;
        if (GUILayout.Button(options[x], styles[x], GUILayout.Height(WindowSettings.guiLineHeight), GUILayout.Width(60 * CurrSettings.CurrentUIScale)))
        {
          if (x != CurrSettings.RealismMode) SMSettings.SetRealismMode(x);
        }
        _rect = GUILayoutUtility.GetLastRect();
        if (Event.current.type == EventType.Repaint && toolTip.CanShow)
          toolTip.Desc = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref toolTip.Active, 10);
        GUI.enabled = true;
      }
      GUILayout.EndHorizontal();
    }

    internal static void RefreshUIScale()
    {
      guiToggleWidth = 300 * CurrSettings.CurrentUIScale;
      guiIndent = 20 * CurrSettings.CurrentUIScale;
      guiLabelWidth = 135 * CurrSettings.CurrentUIScale;
      guiSliderWidth = 240 * CurrSettings.CurrentUIScale;
      guiSliderMinWidth = 10 * CurrSettings.CurrentUIScale;
      guiSliderMaxWidth = 40 * CurrSettings.CurrentUIScale;
    }

  }
}
