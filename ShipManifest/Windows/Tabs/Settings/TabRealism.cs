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

    internal static void Display(Vector2 displayViewerPosition)
    {
      // Reset Tooltip active flag...
      ToolTipActive = false;
      _canShowToolTips = WindowSettings.ShowToolTips && ShowToolTips;

      Position = WindowSettings.Position;
      int scrollX = 20;

      GUI.enabled = true;
      if (!SMSettings.LockSettings)
      {
        // "Realism Settings / Options"
        GUILayout.Label(SmUtils.SmTags["#smloc_settings_realism_001"], SMStyle.LabelTabHeader);        
      }
      else
      {
        // "Realism Settings / Options  (Locked.  To unlock, edit SMSettings.dat file)"
        GUILayout.Label(
          new GUIContent(SmUtils.SmTags["#smloc_settings_realism_002"],
            SmUtils.SmTags["#smloc_settings_realism_tt_001"]), SMStyle.LabelTabHeader);
      }
      GUILayout.Label("____________________________________________________________________________________________",
        SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(guiRuleWidth));

      bool isEnabled = !SMSettings.LockSettings;
      //RealismMode Buttons.
      DisplayRealismButtons();

      GUILayout.Label("____________________________________________________________________________________________",
        SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(guiRuleWidth));


      // RealXfers Mode
      GUI.enabled = isEnabled;
      //_label = "Realistic Transfers";
      //_toolTip = "Turns on/off Realistic Resource Transfers.";
      //_toolTip += "\r\nWhen ON, Resource fills, Dumps, Crew and Science transfers will behave realistically";
      //_toolTip += "\r\nWhen Off, Allows Fills, Dumps, Repeating Science,";
      //_toolTip += "\r\ninstantaneous Xfers, Crew Xfers anywwhere, etc.";
      _label = SmUtils.SmTags["#smloc_settings_realism_008"];
      _toolTip = SmUtils.SmTags["#smloc_settings_realism_tt_007"];
      _guiLabel = new GUIContent(_label, _toolTip);
      SMSettings.RealXfers = GUILayout.Toggle(SMSettings.RealXfers, _guiLabel, GUILayout.Width(guiToggleWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // RealControl Mode
      GUI.enabled = isEnabled;
      //_label = "Realistic Control";
      //_toolTip = "Turns on/off Realistic Shipboard Control.";
      //_toolTip += "\r\nWhen ON, you must have crew aboard, or a valid comm link to a control station or satellite";
      //_toolTip += "\r\nWhen Off, you have full control of the vessel at any time (subject to the availability of resources).";
      _label = SmUtils.SmTags["#smloc_settings_realism_009"];
      _toolTip = SmUtils.SmTags["#smloc_settings_realism_tt_008"];
      _guiLabel = new GUIContent(_label, _toolTip);
      SMSettings.RealControl = GUILayout.Toggle(SMSettings.RealControl, _guiLabel, GUILayout.Width(guiToggleWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // EnableCrew Modifications
      GUI.enabled = isEnabled;
      GUILayout.BeginHorizontal();
      //_label = "Enable Roster Modifications";
      //_toolTip = "Enables/Disable Crew Modifications in the Roster Window.";
      //_toolTip += "\r\nWhen ON, You cannot Edit, Create, or Respawn Crew members.";
      //_toolTip += "\r\nWhen Off, You can Edit, Create, or Respawn Crew members.";
      _label = SmUtils.SmTags["#smloc_settings_realism_010"];
      _toolTip = SmUtils.SmTags["#smloc_settings_realism_tt_009"];
      _guiLabel = new GUIContent(_label, _toolTip);
      SMSettings.EnableCrewModify = GUILayout.Toggle(SMSettings.EnableCrewModify, _guiLabel, GUILayout.Width(guiToggleWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      GUILayout.EndHorizontal();

      //_label = "Enable Kerbal Renaming";
      //_toolTip = "Allows renaming a Kerbal.";
      _label = SmUtils.SmTags["#smloc_settings_realism_011"];
      _toolTip = SmUtils.SmTags["#smloc_settings_realism_tt_010"];
      _guiLabel = new GUIContent(_label, _toolTip);
      SMSettings.EnableKerbalRename = GUILayout.Toggle(SMSettings.EnableKerbalRename, _guiLabel, GUILayout.Width(guiToggleWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      GUILayout.BeginHorizontal();
      //_label = "Enable Profession management";
      //_toolTip = "When On, SM allows you to change a Kerbal's profession.";
      _label = SmUtils.SmTags["#smloc_settings_realism_012"];
      _toolTip = SmUtils.SmTags["#smloc_settings_realism_tt_011"];
      _guiLabel = new GUIContent(_label, _toolTip);
      SMSettings.EnableChangeProfession = GUILayout.Toggle(SMSettings.EnableChangeProfession, _guiLabel,
        GUILayout.Width(guiToggleWidth));
      GUILayout.EndHorizontal();
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      GUI.enabled = true;

      // Enable stock Crew Xfers
      GUI.enabled = SMSettings.EnableCrew && isEnabled;
      GUILayout.BeginHorizontal();
      //_label = "Enable Stock Crew Xfers";
      //_toolTip = "Turns On/Off the stock Crew Transfer mechanism.";
      //_toolTip += "\r\nWhen ON stock crew transfers will be Allowed.";
      //_toolTip += "\r\nWhen OFF Stock Crew transfers are disabled.";
      _label = SmUtils.SmTags["#smloc_settings_realism_013"];
      _toolTip = SmUtils.SmTags["#smloc_settings_realism_tt_012"];
      _guiLabel = new GUIContent(_label, _toolTip);
      SMSettings.EnableStockCrewXfer = GUILayout.Toggle(SMSettings.EnableStockCrewXfer, _guiLabel,
        GUILayout.Width(guiToggleWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      GUILayout.EndHorizontal();

      // EnableCrew Xfer Mode
      GUI.enabled = isEnabled;
      GUILayout.BeginHorizontal();
      //_label = "Enable SM Crew Xfers";
      //_toolTip = "Turns on/off Crew transfers using SM.";
      //_toolTip += "\r\nWhen ON, The Crew option will appear in your resource list.";
      //_toolTip += "\r\nWhen Off, Crew transfers are not possible using SM.";
      _label = SmUtils.SmTags["#smloc_settings_realism_014"];
      _toolTip = SmUtils.SmTags["#smloc_settings_realism_tt_013"];
      _guiLabel = new GUIContent(_label, _toolTip);
      SMSettings.EnableCrew = GUILayout.Toggle(SMSettings.EnableCrew, _guiLabel, GUILayout.Width(guiToggleWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      GUILayout.EndHorizontal();

      if (!SMSettings.EnableCrew && HighLogic.LoadedSceneIsFlight)
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
      GUI.enabled = SMSettings.EnableCrew && isEnabled;

      // EnablePFResources Mode
      GUILayout.BeginHorizontal();
      GUILayout.Space(20);
      //_label = "Enable Crew Fill and Empty Ops in Pre-Flight";
      //_toolTip = "Turns on/off Fill and Empty Crew when in preflight.";
      //_toolTip += "\r\nWhen ON, Fill & Empty Crew vessel wide are possible (shows in the Resource list).";
      //_toolTip += "\r\nWhen Off, Fill and Empty Crew vessel wide will not appear in the resource list.";
      _label = SmUtils.SmTags["#smloc_settings_realism_015"];
      _toolTip = SmUtils.SmTags["#smloc_settings_realism_tt_014"];
      _guiLabel = new GUIContent(_label, _toolTip);
      SMSettings.EnablePfCrews = GUILayout.Toggle(SMSettings.EnablePfCrews, _guiLabel, GUILayout.Width(guiToggleWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      GUILayout.EndHorizontal();

      // Enable stock Crew Xfer Override
      GUI.enabled = SMSettings.EnableCrew && isEnabled && SMSettings.EnableStockCrewXfer;
      GUILayout.BeginHorizontal();
      //_label = "Override Stock Crew Xfers";
      //_toolTip = "Turns on/off Overriding the stock Crew Transfer mechanism with the SM style.";
      //_toolTip += "\r\nWhen ON stock crew transfers will behave like SM style transfers.\n(requires both Stock Crew Transfers & SM Crew Transfers ON)";
      //_toolTip += "\r\nWhen Off Stock Crew transfers behave normally if enabled.";
      _label = SmUtils.SmTags["#smloc_settings_realism_016"];
      _toolTip = SmUtils.SmTags["#smloc_settings_realism_tt_015"];
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(20);
      SMSettings.OverrideStockCrewXfer = GUILayout.Toggle(SMSettings.OverrideStockCrewXfer, _guiLabel,
        GUILayout.Width(300));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      GUILayout.EndHorizontal();

      // EnableCLS Mode
      GUILayout.BeginHorizontal();
      GUILayout.Space(20);
      if (!SMSettings.EnableCrew || !SMSettings.ClsInstalled)
        GUI.enabled = false;
      else
        GUI.enabled = isEnabled;
      //_label = "Enable CLS  (Connected Living Spaces)";
      //_toolTip = "Turns on/off Connected Living space support.";
      //_toolTip += "\r\nWhen ON, Crew can only be xfered to a part in the same 'Living Space'.";
      //_toolTip += "\r\nWhen Off, Crew transfers are possible to any part that can hold a kerbal.";
      _label = SmUtils.SmTags["#smloc_settings_realism_017"];
      _toolTip = SmUtils.SmTags["#smloc_settings_realism_tt_016"];
      _guiLabel = new GUIContent(_label, _toolTip);
      SMSettings.EnableCls = GUILayout.Toggle(SMSettings.EnableCls, _guiLabel, GUILayout.Width(guiToggleWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      GUILayout.EndHorizontal();

      if (SMSettings.EnableCls != SMSettings.PrevEnableCls && HighLogic.LoadedSceneIsFlight)
      {
        if (!SMSettings.EnableCls)
          SMHighlighter.HighlightClsVessel(false, true);
        else if (SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Crew.ToString()))
        {
          // Update spaces and reassign the resource to observe new settings.
          SMAddon.UpdateClsSpaces();
          SMAddon.SmVessel.SelectedResources.Clear();
          SMAddon.SmVessel.SelectedResources.Add(SMConditions.ResourceType.Crew.ToString());
        }
      }

      // Enable stock Crew Xfer Override
      if (!SMSettings.EnableCrew || !SMSettings.ClsInstalled)
        GUI.enabled = false;
      else
        GUI.enabled = isEnabled;
      GUILayout.BeginHorizontal();
      //_label = "Enable CLS' Allow Unrestricted Crew Xfers switch";
      //_toolTip = "Turns on/off Enabling the CLS Switch allowing unrestricted crew transfers.";
      //_toolTip += "\r\nWhen ON (requires Realism Mode On), SM Manages Stock and CLS aware Crew Transfers.";
      //_toolTip += "\r\nWhen Off (or Realism is off), the setting in CLS is not touched.";
      _label = SmUtils.SmTags["#smloc_settings_realism_018"];
      _toolTip = SmUtils.SmTags["#smloc_settings_realism_tt_017"];
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Space(20);
      SMSettings.EnableClsAllowTransfer = GUILayout.Toggle(SMSettings.EnableClsAllowTransfer, _guiLabel,
        GUILayout.Width(guiToggleWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      GUILayout.EndHorizontal();

      // EnableScience Mode
      GUILayout.BeginHorizontal();
      GUI.enabled = isEnabled;
      //_label = "Enable Science Xfers";
      //_toolTip = "Turns on/off Science Xfers.";
      //_toolTip += "\r\nWhen ON, Science transfers are possible and show up in the Resource list.";
      //_toolTip += "\r\nWhen Off, Science transfers will not appear in the resource list.";
      _label = SmUtils.SmTags["#smloc_settings_realism_019"];
      _toolTip = SmUtils.SmTags["#smloc_settings_realism_tt_018"];
      _guiLabel = new GUIContent(_label, _toolTip);
      SMSettings.EnableScience = GUILayout.Toggle(SMSettings.EnableScience, _guiLabel, GUILayout.Width(guiToggleWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      GUILayout.EndHorizontal();

      if (!SMSettings.EnableScience && HighLogic.LoadedSceneIsFlight)
      {
        // Clear Resource selection.
        if (SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Science.ToString()))
          SMAddon.SmVessel.SelectedResources.Clear();
      }

      // EnableResources Mode
      GUILayout.BeginHorizontal();
      GUI.enabled = isEnabled;
      //_label = "Enable Resource Xfers";
      //_toolTip = "Turns on/off Resource Xfers.";
      //_toolTip += "\r\nWhen ON, Resource transfers are possible and display in Manifest Window.";
      //_toolTip += "\r\nWhen Off, Resources will not appear in the resource list.";
      _label = SmUtils.SmTags["#smloc_settings_realism_020"];
      _toolTip = SmUtils.SmTags["#smloc_settings_realism_tt_019"];
      _guiLabel = new GUIContent(_label, _toolTip);
      SMSettings.EnableResources = GUILayout.Toggle(SMSettings.EnableResources, _guiLabel, GUILayout.Width(guiToggleWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      GUILayout.EndHorizontal();

      if (!SMSettings.EnableResources && HighLogic.LoadedSceneIsFlight)
      {
        // Clear Resource selection.
        if (SMAddon.SmVessel.SelectedResources.Count > 0 &&
            !SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Crew.ToString()) &&
            !SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Science.ToString()))
          SMAddon.SmVessel.SelectedResources.Clear();
      }

      // Set Gui.enabled for child settings to resources...
      GUI.enabled = SMSettings.EnableResources && isEnabled;

      // EnablePFResources Mode
      GUILayout.BeginHorizontal();
      GUILayout.Space(20);
      //_label = "Enable Resources in Pre-Flight";
      //_toolTip = "Turns on/off Fill and Empty Resources when in preflight.";
      //_toolTip += "\r\nWhen ON, Fill & Dump resources vessel wide are possible (shows in the Resource list).";
      //_toolTip += "\r\nWhen Off, Fill and Dump Resources vessel wide will not appear in the resource list.";
      _label = SmUtils.SmTags["#smloc_settings_realism_021"];
      _toolTip = SmUtils.SmTags["#smloc_settings_realism_tt_020"];
      _guiLabel = new GUIContent(_label, _toolTip);
      SMSettings.EnablePfResources = GUILayout.Toggle(SMSettings.EnablePfResources, _guiLabel, GUILayout.Width(guiToggleWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      GUILayout.EndHorizontal();

      // EnableXferCost Mode
      GUILayout.BeginHorizontal();
      GUILayout.Space(20);
      //_label = "Resource Xfers Consume Power";
      //_toolTip = "Turns on/off ElectricCharge cost forResource Xfers.";
      //_toolTip += "\r\nWhen ON, Resource transfers will consume ElectricCharge.";
      //_toolTip += "\r\nWhen Off, Resources Xfers consume no ElectricCharge.";
      _label = SmUtils.SmTags["#smloc_settings_realism_022"];
      _toolTip = SmUtils.SmTags["#smloc_settings_realism_tt_021"];
      _guiLabel = new GUIContent(_label, _toolTip);
      SMSettings.EnableXferCost = GUILayout.Toggle(SMSettings.EnableXferCost, _guiLabel, GUILayout.Width(guiToggleWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      GUILayout.EndHorizontal();

      // Resource Xfer EC cost
      GUI.enabled = !SMSettings.LockSettings && SMSettings.EnableResources && SMSettings.EnableXferCost;
      GUILayout.BeginHorizontal();
      GUILayout.Space(35);
      //_label = "Xfer Power Cost:";
      //_toolTip = "Sets the Electrical cost of resource Xfers.";
      //_toolTip += "\r\nThe higher the number the more ElectricCharge used.";
      _label = $"{SmUtils.SmTags["#smloc_settings_realism_023"]}:";
      _toolTip = SmUtils.SmTags["#smloc_settings_realism_tt_022"];
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Label(_guiLabel, GUILayout.Width(125), GUILayout.Height(20));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // Lets parse the string to allow decimal points.
      StrFlowCost = SMSettings.FlowCost.ToString(CultureInfo.InvariantCulture);
      // add the decimal point if it was typed.
      StrFlowCost = SmUtils.GetStringDecimal(StrFlowCost);
      // add the zero if it was typed.
      StrFlowCost = SmUtils.GetStringZero(StrFlowCost);

      StrFlowCost = GUILayout.TextField(StrFlowCost, 20, GUILayout.Height(20), GUILayout.Width(80));
      //_label = "EC/Unit";
      //_toolTip = "Sets the Electrical cost of resource Xfers when Realism Mode is on.";
      //_toolTip += "\r\nThe higher the number the more ElectricCharge used.";
      _label = SmUtils.SmTags["#smloc_settings_realism_024"];
      _toolTip = SmUtils.SmTags["#smloc_settings_realism_tt_022"];
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Label(_guiLabel, GUILayout.Width(80), GUILayout.Height(20));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      GUILayout.EndHorizontal();

      // update decimal bool 
      SmUtils.SetStringDecimal(StrFlowCost);
      //update zero bool 
      SmUtils.SetStringZero(StrFlowCost);

      if (float.TryParse(StrFlowCost, out float newCost))
        SMSettings.FlowCost = newCost;

      // create xfer Flow Rate slider;
      // Lets parse the string to allow decimal points.
      string strFlowRate = SMSettings.FlowRate.ToString(CultureInfo.InvariantCulture);
      string strMinFlowRate = SMSettings.MinFlowRate.ToString(CultureInfo.InvariantCulture);
      string strMaxFlowRate = SMSettings.MaxFlowRate.ToString(CultureInfo.InvariantCulture);
      string strMaxFlowTime = SMSettings.MaxFlowTimeSec.ToString();


      // Resource Flow Rate
      GUI.enabled = !SMSettings.LockSettings && SMSettings.EnableResources && SMSettings.RealXfers;
      GUILayout.BeginHorizontal();
      GUILayout.Space(25);
      //_label = "Resource Flow Rate:";
      //_toolTip = "Sets the rate that resources Xfer when Realistic Transfers is on.";
      //_toolTip += "\r\nThe higher the number the faster resources move.";
      //_toolTip += "\r\nYou can also use the slider below to change this value.";
      _label = $"{SmUtils.SmTags["#smloc_settings_realism_025"]}:";
      _toolTip = SmUtils.SmTags["#smloc_settings_realism_tt_023"];
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Label(_guiLabel, GUILayout.Width(135), GUILayout.Height(20));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      strFlowRate = GUILayout.TextField(strFlowRate, 20, GUILayout.Height(20), GUILayout.Width(80));
      //_label = "Units/Sec";
      //_toolTip = "Sets the rate that resources Xfer when Realism Mode is on.";
      //_toolTip += "\r\nThe higher the number the faster resources move.";
      //_toolTip += "\r\nYou can also use the slider below to change this value.";
      _label = SmUtils.SmTags["#smloc_settings_realism_026"];
      _toolTip = SmUtils.SmTags["#smloc_settings_realism_tt_023"];
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Label(_guiLabel, GUILayout.Width(80), GUILayout.Height(20));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      GUILayout.EndHorizontal();

      if (float.TryParse(strFlowRate, out float newRate))
        SMSettings.FlowRate = (int)newRate;

      // Resource Flow rate Slider
      GUILayout.BeginHorizontal();
      GUILayout.Space(30);
      GUILayout.Label(SMSettings.MinFlowRate.ToString(CultureInfo.InvariantCulture), GUILayout.Width(10),
        GUILayout.Height(20));
      SMSettings.FlowRate = GUILayout.HorizontalSlider((float) SMSettings.FlowRate, (float) SMSettings.MinFlowRate,
        (float) SMSettings.MaxFlowRate, GUILayout.Width(240), GUILayout.Height(20));
      _label = SMSettings.MaxFlowRate.ToString(CultureInfo.InvariantCulture);
      //_toolTip = "Slide control to change the Resource Flow Rate shown above.";
      _toolTip = SmUtils.SmTags["#smloc_settings_realism_tt_024"];
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Label(_guiLabel, GUILayout.Width(40), GUILayout.Height(20));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      GUILayout.EndHorizontal();

      // Min Flow Rate for Slider
      GUILayout.BeginHorizontal();
      GUILayout.Space(30);
      //_label = " - Min Flow Rate:";
      //_toolTip = "Sets the lower limit (left side) of the Flow rate Slider range.";
      _label = $" - {SmUtils.SmTags["#smloc_settings_realism_027"]}:";
      _toolTip = SmUtils.SmTags["#smloc_settings_realism_tt_025"];
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Label(_guiLabel, GUILayout.Width(130), GUILayout.Height(20));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      strMinFlowRate = GUILayout.TextField(strMinFlowRate, 20, GUILayout.Height(20), GUILayout.Width(80));
      //_label = "Units/Sec";
      //_toolTip = "Sets the lower limit (left side) of the Flow rate Slider range.";
      _label = SmUtils.SmTags["#smloc_settings_realism_026"];
      _toolTip = SmUtils.SmTags["#smloc_settings_realism_tt_025"];
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Label(_guiLabel, GUILayout.Width(80), GUILayout.Height(20));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      GUILayout.EndHorizontal();
      if (float.TryParse(strMinFlowRate, out newRate))
        SMSettings.MinFlowRate = (int) newRate;

      // Max Flow Rate for Slider
      GUILayout.BeginHorizontal();
      GUILayout.Space(30);
      //_label = " - Max Flow Rate:";
      //_toolTip = "Sets the upper limit (right side) of the Flow rate Slider range.";
      _label = $" - {SmUtils.SmTags["#smloc_settings_realism_028"]}:";
      _toolTip = SmUtils.SmTags["#smloc_settings_realism_tt_026"];
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Label(_guiLabel, GUILayout.Width(130), GUILayout.Height(20));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      strMaxFlowRate = GUILayout.TextField(strMaxFlowRate, 20, GUILayout.Height(20), GUILayout.Width(80));
      //_label = "Units/Sec";
      //_toolTip = "Sets the upper limit (right side) of the Flow rate Slider range.";
      _label = SmUtils.SmTags["#smloc_settings_realism_026"];
      _toolTip = SmUtils.SmTags["#smloc_settings_realism_tt_026"];
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Label(_guiLabel, GUILayout.Width(80), GUILayout.Height(20));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      GUILayout.EndHorizontal();
      if (float.TryParse(strMaxFlowRate, out newRate))
        SMSettings.MaxFlowRate = (int) newRate;

      // Max Flow Time 
      GUILayout.BeginHorizontal();
      GUILayout.Space(30);
      //_label = " - Max Flow Time:";
      //_toolTip = "Sets the maximum duration (in sec) of a resource transfer.";
      //_toolTip += "\r\nWorks in conjunction with the Flow rate.  if time it would take";
      //_toolTip += "\r\n to move a resource exceeds this number, this number will be used";
      //_toolTip += "\r\n to calculate an adjusted flow rate.";
      //_toolTip += "\r\n(protects you from long Xfers)";
      _label = $" - {SmUtils.SmTags["#smloc_settings_realism_029"]}:";
      _toolTip = SmUtils.SmTags["#smloc_settings_realism_tt_027"];
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Label(_guiLabel, GUILayout.Width(130), GUILayout.Height(20));
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
        SMSettings.MaxFlowTimeSec = (int) newRate;

      // reset gui.enabled to default
      GUI.enabled = true;
      GUILayout.Label("____________________________________________________________________________________________",
        SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(guiRuleWidth));

      // LockSettings Mode
      GUI.enabled = isEnabled;
      //_label = "Lock Realism Settings  (If set ON, disable in config file)";
      //_toolTip = "Locks the settings in this section so they cannot be altered in game.";
      //_toolTip += "\r\nTo turn off Locking you MUST edit the SMSettings.dat file.";
      _label = SmUtils.SmTags["#smloc_settings_realism_031"];
      _toolTip = SmUtils.SmTags["#smloc_settings_realism_tt_028"];
      _guiLabel = new GUIContent(_label, _toolTip);
      SMSettings.LockSettings = GUILayout.Toggle(SMSettings.LockSettings, _guiLabel, GUILayout.Width(guiToggleWidth));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      GUI.enabled = true;
      GUILayout.Label("____________________________________________________________________________________________",
        SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(guiRuleWidth));
    }

    private static void DisplayRealismButtons()
    {
      // RealismMode options
      GUIContent[] options =
      {
        new GUIContent(SmUtils.SmTags["#smloc_settings_realism_004"], SmUtils.SmTags["#smloc_settings_realism_tt_003"]),
        new GUIContent(SmUtils.SmTags["#smloc_settings_realism_005"], SmUtils.SmTags["#smloc_settings_realism_tt_004"]),
        new GUIContent(SmUtils.SmTags["#smloc_settings_realism_006"], SmUtils.SmTags["#smloc_settings_realism_tt_005"]),
        new GUIContent(SmUtils.SmTags["#smloc_settings_realism_007"], SmUtils.SmTags["#smloc_settings_realism_tt_006"])
      };
      GUIStyle[] styles =
      {
        SMSettings.RealismMode == 0 ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle,
        SMSettings.RealismMode == 1 ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle,
        SMSettings.RealismMode == 2 ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle,
        SMSettings.RealismMode == 3 ? SMStyle.ButtonToggledStyle : SMStyle.ButtonStyle,
      };

      // "Realism Mode Label"
      _label = $"{SmUtils.SmTags["#smloc_settings_realism_003"]}:";
      _toolTip = SmUtils.SmTags["#smloc_settings_realism_tt_002"];
      _guiLabel = new GUIContent(_label, _toolTip);
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, 10);

      GUI.enabled = true;
      GUILayout.BeginHorizontal();
      GUILayout.Label(_guiLabel, SMStyle.LabelStyleNoWrap, GUILayout.Width(90));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, 10);

      // Build Option Buttons
      SMSettings.RealismMode = SMSettings.GetRealismMode();
      for (int x = 0; x <= 3; x++)
      {
        if (x == 3) GUI.enabled = false;
        if (GUILayout.Button(options[x], styles[x], GUILayout.Height(20)))
        {
          if (x != SMSettings.RealismMode) SMSettings.SetRealismMode(x);
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