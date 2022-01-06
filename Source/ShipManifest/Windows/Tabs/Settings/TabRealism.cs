using System.Globalization;
using ShipManifest.InternalObjects;
using UnityEngine;

namespace ShipManifest.Windows.Tabs.Settings
{
  internal static class TabRealism
  {
    internal static string StrFlowCost = "0";

    // GUI tooltip and label support
    private static string _toolTip = "";
    private static Rect _rect;
    private static string _label = "";
    private static GUIContent _guiLabel;
    private const float guiRuleWidth = 350;
    private const float guiToggleWidth = 300;
    private const float guiIndent = 20;

    internal static string ToolTip = "";
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;
    private static bool _canShowToolTips = true;
    internal static Rect Position = WindowSettings.Position;

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
    internal static GUIContent flowSliderContent    = new GUIContent(Curr.MaxFlowRate.ToString(CultureInfo.InvariantCulture), SmUtils.SmTags["#smloc_settings_realism_tt_024"]);
    internal static GUIContent minRateContent       = new GUIContent($" - {SmUtils.SmTags["#smloc_settings_realism_027"]}:", SmUtils.SmTags["#smloc_settings_realism_tt_025"]);
    internal static GUIContent unitSecMinContent    = new GUIContent(SmUtils.SmTags["#smloc_settings_realism_026"], SmUtils.SmTags["#smloc_settings_realism_tt_025"]);
    internal static GUIContent maxRateContent       = new GUIContent($" - {SmUtils.SmTags["#smloc_settings_realism_028"]}:", SmUtils.SmTags["#smloc_settings_realism_tt_026"]);
    internal static GUIContent unitSecMaxContent    = new GUIContent(SmUtils.SmTags["#smloc_settings_realism_026"], SmUtils.SmTags["#smloc_settings_realism_tt_026"]);
    internal static GUIContent maxFlowTimeContent   = new GUIContent($" - {SmUtils.SmTags["#smloc_settings_realism_029"]}:", SmUtils.SmTags["#smloc_settings_realism_tt_027"]);
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
      ToolTipActive = false;
      _canShowToolTips = WindowSettings.ShowToolTips && ShowToolTips;

      Position = WindowSettings.Position;
      int scrollX = 20;

      GUI.enabled = true;
      if (!Curr.LockSettings)
      {
        // "Realism Settings / Options"
        GUILayout.Label(optionsContent, SMStyle.LabelTabHeader);        
      }
      else
      {
        // "Realism Settings / Options  (Locked.  To unlock, edit SMSettings.dat file)"
        GUILayout.Label(lockContent, SMStyle.LabelTabHeader);
      }
      GUILayout.Label(WindowSettings.TabRule, SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(guiRuleWidth));

      bool isEnabled = !Curr.LockSettings;
      //RealismMode Buttons.
      DisplayRealismButtons();

      GUILayout.Label(WindowSettings.TabRule, SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(guiRuleWidth));


      // RealXfers Mode
      GUI.enabled = isEnabled;
      // Realistic Transfers;
      Curr.RealXfers = GUILayout.Toggle(Curr.RealXfers, realXferContent, GUILayout.Width(guiToggleWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // RealControl Mode
      GUI.enabled = isEnabled;
      // Realistic Control
      Curr.RealControl = GUILayout.Toggle(Curr.RealControl, realControlContent, GUILayout.Width(guiToggleWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // EnableCrew Modifications
      GUI.enabled = isEnabled;
      GUILayout.BeginHorizontal();
      // Enable Roster Modifications
      Curr.EnableCrewModify = GUILayout.Toggle(Curr.EnableCrewModify, crewModifyContent, GUILayout.Width(guiToggleWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      GUILayout.EndHorizontal();

      // Enable Kerbal Renaming
      Curr.EnableKerbalRename = GUILayout.Toggle(Curr.EnableKerbalRename, crewRenameContent, GUILayout.Width(guiToggleWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      GUILayout.BeginHorizontal();
      //Enable Profession management
      Curr.EnableChangeProfession = GUILayout.Toggle(Curr.EnableChangeProfession, professionContent, GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      GUI.enabled = true;

      // Enable stock Crew Xfers
      GUI.enabled = Curr.EnableCrew && isEnabled;
      GUILayout.BeginHorizontal();
      // Enable Stock Crew Xfers;
      Curr.EnableStockCrewXfer = GUILayout.Toggle(Curr.EnableStockCrewXfer, stkCrewXferContent, GUILayout.Width(guiToggleWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      GUILayout.EndHorizontal();

      // EnableCrew Xfer Mode
      GUI.enabled = isEnabled;
      GUILayout.BeginHorizontal();
      // Enable SM Crew Xfers
      Curr.EnableCrew = GUILayout.Toggle(Curr.EnableCrew, smCrewXfersContent, GUILayout.Width(guiToggleWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      GUILayout.EndHorizontal();

      if (!Curr.EnableCrew && HighLogic.LoadedSceneIsFlight)
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
      GUI.enabled = Curr.EnableCrew && isEnabled;

      // Realistic Crew Xfer Mode
      GUILayout.BeginHorizontal();
      GUILayout.Space(20);
      //Turns on/off Realistic Resource Transfers.
      Curr.RealCrewXfers = GUILayout.Toggle(Curr.RealCrewXfers, realCrewXferContent, GUILayout.Width(guiToggleWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      GUILayout.EndHorizontal();

      // EnablePFResources Mode
      GUILayout.BeginHorizontal();
      GUILayout.Space(20);
      //Enable Crew Fill and Empty Ops in Pre-Flight
      Curr.EnablePfCrews = GUILayout.Toggle(Curr.EnablePfCrews, pfCrewContent, GUILayout.Width(guiToggleWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      GUILayout.EndHorizontal();

      // Enable stock Crew Xfer Override
      GUI.enabled = Curr.EnableCrew && isEnabled && Curr.EnableStockCrewXfer;
      GUILayout.BeginHorizontal();
      //Override Stock Crew Xfers
      GUILayout.Space(20);
      Curr.OverrideStockCrewXfer = GUILayout.Toggle(Curr.OverrideStockCrewXfer, orStkCrewXferContent, GUILayout.Width(300));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      GUILayout.EndHorizontal();

      // EnableCLS Mode
      GUILayout.BeginHorizontal();
      GUILayout.Space(20);
      if (!Curr.EnableCrew || !SMSettings.ClsInstalled)
        GUI.enabled = false;
      else
        GUI.enabled = isEnabled;
      //Enable CLS  (Connected Living Spaces)";
      Curr.EnableCls = GUILayout.Toggle(Curr.EnableCls, clsContent, GUILayout.Width(guiToggleWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      GUILayout.EndHorizontal();

      if (Curr.EnableCls != Orig.EnableCls && HighLogic.LoadedSceneIsFlight)
      {
        if (!Curr.EnableCls)
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
      if (!Curr.EnableCrew || !SMSettings.ClsInstalled)
        GUI.enabled = false;
      else
        GUI.enabled = isEnabled;
      GUILayout.BeginHorizontal();
      GUILayout.Space(20);
      Curr.EnableClsAllowTransfer = GUILayout.Toggle(Curr.EnableClsAllowTransfer, clsAllowXferContent,
        GUILayout.Width(guiToggleWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      GUILayout.EndHorizontal();

      // EnableScience Mode
      GUILayout.BeginHorizontal();
      GUI.enabled = isEnabled;
      //Enable Science Xfers
      Curr.EnableScience = GUILayout.Toggle(Curr.EnableScience, modeScienceContent, GUILayout.Width(guiToggleWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      GUILayout.EndHorizontal();

      if (!Curr.EnableScience && HighLogic.LoadedSceneIsFlight)
      {
        // Clear Resource selection.
        if (SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Science.ToString()))
          SMAddon.SmVessel.SelectedResources.Clear();
      }

      // EnableResources Mode
      GUILayout.BeginHorizontal();
      GUI.enabled = isEnabled;
      //Enable Resource Xfers
      Curr.EnableResources = GUILayout.Toggle(Curr.EnableResources, modeResourcesContent, GUILayout.Width(guiToggleWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      GUILayout.EndHorizontal();

      if (!Curr.EnableResources && HighLogic.LoadedSceneIsFlight)
      {
        // Clear Resource selection.
        if (SMAddon.SmVessel.SelectedResources.Count > 0 &&
            !SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Crew.ToString()) &&
            !SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Science.ToString()))
          SMAddon.SmVessel.SelectedResources.Clear();
      }

      // Set Gui.enabled for child settings to resources...
      GUI.enabled = Curr.EnableResources && isEnabled;

      // EnablePFResources Mode
      GUILayout.BeginHorizontal();
      GUILayout.Space(20);
      //Enable Resources in Pre-Flight
      Curr.EnablePfResources = GUILayout.Toggle(Curr.EnablePfResources, pfResourcesContent, GUILayout.Width(guiToggleWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      GUILayout.EndHorizontal();

      // EnableXferCost Mode
      GUILayout.BeginHorizontal();
      GUILayout.Space(20);
      // Resource Xfers Consume Power";
      Curr.EnableXferCost = GUILayout.Toggle(Curr.EnableXferCost, xferCostOnContent, GUILayout.Width(guiToggleWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      GUILayout.EndHorizontal();

      // Resource Xfer EC cost
      GUI.enabled = !Curr.LockSettings && Curr.EnableResources && Curr.EnableXferCost;
      GUILayout.BeginHorizontal();
      GUILayout.Space(35);
      // Xfer Power Cost:
      GUILayout.Label(xferCostAmtContent, GUILayout.Width(125), GUILayout.Height(20));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // Lets parse the string to allow decimal points.
      StrFlowCost = Curr.FlowCost.ToString(CultureInfo.InvariantCulture);
      // add the decimal point if it was typed.
      StrFlowCost = SmUtils.GetStringDecimal(StrFlowCost);
      // add the zero if it was typed.
      StrFlowCost = SmUtils.GetStringZero(StrFlowCost);

      StrFlowCost = GUILayout.TextField(StrFlowCost, 20, GUILayout.Height(20), GUILayout.Width(80));
      // EC/Unit
      GUILayout.Label(ecUnitContent, GUILayout.Width(80), GUILayout.Height(20));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      GUILayout.EndHorizontal();

      // update decimal bool 
      SmUtils.SetStringDecimal(StrFlowCost);
      //update zero bool 
      SmUtils.SetStringZero(StrFlowCost);

      if (float.TryParse(StrFlowCost, out float newCost))
        Curr.FlowCost = newCost;

      // create xfer Flow Rate slider;
      // Lets parse the string to allow decimal points.
      string strFlowRate = Curr.FlowRate.ToString(CultureInfo.InvariantCulture);
      string strMinFlowRate = Curr.MinFlowRate.ToString(CultureInfo.InvariantCulture);
      string strMaxFlowRate = Curr.MaxFlowRate.ToString(CultureInfo.InvariantCulture);
      string strMaxFlowTime = Curr.MaxFlowTimeSec.ToString();


      // Resource Flow Rate
      GUI.enabled = !Curr.LockSettings && Curr.EnableResources && Curr.RealXfers;
      GUILayout.BeginHorizontal();
      GUILayout.Space(25);
      // Resource Flow Rate:
      GUILayout.Label(flowRateContent, GUILayout.Width(135), GUILayout.Height(20));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      strFlowRate = GUILayout.TextField(strFlowRate, 20, GUILayout.Height(20), GUILayout.Width(80));
      // Units/Sec
      GUILayout.Label(unitsSecContent, GUILayout.Width(80), GUILayout.Height(20));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      GUILayout.EndHorizontal();

      if (float.TryParse(strFlowRate, out float newRate))
        Curr.FlowRate = (int)newRate;

      // Resource Flow rate Slider
      GUILayout.BeginHorizontal();
      GUILayout.Space(30);
      GUILayout.Label(Curr.MinFlowRate.ToString(CultureInfo.InvariantCulture), GUILayout.Width(10),
        GUILayout.Height(20));
      Curr.FlowRate = GUILayout.HorizontalSlider((float) Curr.FlowRate, (float) Curr.MinFlowRate,
        (float) Curr.MaxFlowRate, GUILayout.Width(240), GUILayout.Height(20));
      GUILayout.Label(flowSliderContent, GUILayout.Width(40), GUILayout.Height(20));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      GUILayout.EndHorizontal();

      // Min Flow Rate for Slider
      GUILayout.BeginHorizontal();
      GUILayout.Space(30);
      // - Min Flow Rate:
      GUILayout.Label(minRateContent, GUILayout.Width(130), GUILayout.Height(20));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      strMinFlowRate = GUILayout.TextField(strMinFlowRate, 20, GUILayout.Height(20), GUILayout.Width(80));
      //Units/Sec";
      GUILayout.Label(unitSecMinContent, GUILayout.Width(80), GUILayout.Height(20));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      GUILayout.EndHorizontal();
      if (float.TryParse(strMinFlowRate, out newRate))
        Curr.MinFlowRate = (int) newRate;

      // Max Flow Rate for Slider
      GUILayout.BeginHorizontal();
      GUILayout.Space(30);
      // - Max Flow Rate:
      GUILayout.Label(maxRateContent, GUILayout.Width(130), GUILayout.Height(20));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      strMaxFlowRate = GUILayout.TextField(strMaxFlowRate, 20, GUILayout.Height(20), GUILayout.Width(80));
      // Units/Sec
      GUILayout.Label(unitSecMaxContent, GUILayout.Width(80), GUILayout.Height(20));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      GUILayout.EndHorizontal();
      if (float.TryParse(strMaxFlowRate, out newRate))
        Curr.MaxFlowRate = (int) newRate;

      // Max Flow Time 
      GUILayout.BeginHorizontal();
      GUILayout.Space(30);
      // - Max Flow Time:
      GUILayout.Label(maxFlowTimeContent, GUILayout.Width(130), GUILayout.Height(20));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      strMaxFlowTime = GUILayout.TextField(strMaxFlowTime, 20, GUILayout.Height(20), GUILayout.Width(80));
      //_label = "Sec";
      //_toolTip = "Sets the maximum duration (in sec) of a resource transfer.";
      //_toolTip += "\r\nWorks in conjunction with the Flow rate.  if time it would take";
      //_toolTip += "\r\n to move a resource exceeds this number, this number will be used";
      //_toolTip += "\r\n to calculate an adjusted flow rate.";
      //_toolTip += "\r\n(protects you from long Xfers)";
      _label = SmUtils.SmTags["#smloc_settings_realism_030"];
      _toolTip = SmUtils.SmTags["#smloc_settings_realism_tt_027"];
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Label(_guiLabel, GUILayout.Width(80), GUILayout.Height(20));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      GUILayout.EndHorizontal();
      if (float.TryParse(strMaxFlowTime, out newRate))
        Curr.MaxFlowTimeSec = (int) newRate;

      // reset gui.enabled to default
      GUI.enabled = true;
      GUILayout.Label(WindowSettings.TabRule, SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(guiRuleWidth));

      // LockSettings Mode
      GUI.enabled = isEnabled;
      // Lock Realism Settings  (If set ON, disable in config file)
      Curr.LockSettings = GUILayout.Toggle(Curr.LockSettings, lockSettingContent, GUILayout.Width(guiToggleWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      GUI.enabled = true;
      GUILayout.Label(WindowSettings.TabRule, SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(guiRuleWidth));
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
        Curr.RealismMode == 0 ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle,
        Curr.RealismMode == 1 ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle,
        Curr.RealismMode == 2 ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle,
        Curr.RealismMode == 3 ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle,
      };

      // Realism Mode Label
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, 10);

      GUI.enabled = true;
      GUILayout.BeginHorizontal();
      GUILayout.Label(modeOptionContent, SMStyle.LabelStyleNoWrap, GUILayout.Width(90));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, 10);

      // Build Option Buttons
      Curr.RealismMode = SMSettings.GetRealismMode();
      for (int x = 0; x <= 3; x++)
      {
        if (x == 3) GUI.enabled = false;
        if (GUILayout.Button(options[x], styles[x], GUILayout.Height(20)))
        {
          if (x != Curr.RealismMode) SMSettings.SetRealismMode(x);
        }
        _rect = GUILayoutUtility.GetLastRect();
        if (Event.current.type == EventType.Repaint && ShowToolTips)
          ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, 10);
        GUI.enabled = true;
      }
      GUILayout.EndHorizontal();
    }
  }
}
