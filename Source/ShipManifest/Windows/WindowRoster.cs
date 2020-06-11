using System;
using System.Collections.Generic;
using System.Linq;
using ShipManifest.APIClients;
using ShipManifest.InternalObjects;
using ShipManifest.Modules;
using ShipManifest.Process;
using UnityEngine;

namespace ShipManifest.Windows
{
  internal static class WindowRoster
  {
    #region Properties

    internal static string Title = "Ship Manifest Roster";
    internal static float WindowWidth = 700;
    internal static float WindowHeight = 330;
    internal static Rect Position = new Rect(0, 0, 0, 0);
    internal static Rect ViewBox = new Rect(0, 0, 680, 230);
    internal static bool ShowWindow;
    internal static string ToolTip = "";
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;
    internal static float XOffset = 30;
    internal static float YOffset = 90;

    // RosterList must exist outside of the vessel.
    internal static List<ProtoCrewMember> RosterList = new List<ProtoCrewMember>();

    internal static Professions KerbalProfession;

    // Gender var
    internal static ProtoCrewMember.Gender Gender = ProtoCrewMember.Gender.Male;
    internal static KerbalFilters CurrentFilter = KerbalFilters.All;

    internal static bool OnCreate;

    private static ModKerbal _selectedKerbal;

    private static Vector2 _scrollViewerPosition = Vector2.zero;

    internal static bool ResetRosterSize
    {
      get
      {
        return !OnCreate && SelectedKerbal == null;
      }
    }

    internal static ModKerbal SelectedKerbal
    {
      get { return _selectedKerbal; }
      set
      {
        _selectedKerbal = value;
        if (_selectedKerbal == null)
        {
          SMAddon.SaveMessage = string.Empty;
        }
      }
    }

    #endregion Properties

    #region Gui Layout
    internal static void Display(int windowId)
    {
      Title = SmUtils.SmTags["#smloc_roster_001"];
      
      // set input locks when mouseover window...
      //_inputLocked = GuiUtils.PreventClickthrough(ShowWindow, Position, _inputLocked);

      // Reset Tooltip active flag...
      ToolTipActive = false;

      Rect rect = new Rect(Position.width - 20, 4, 16, 16);
      if (GUI.Button(rect, new GUIContent("", SmUtils.SmTags["#smloc_window_tt_001"]))) // "Close Window"
      {
        OnCreate = false;
        SelectedKerbal = null;
        ToolTip = "";
        if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
          SMAddon.OnSmRosterClicked();
        else
          ShowWindow = false;
      }

      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);
      try
      {
        GUILayout.BeginVertical();
        DisplayRosterFilter();

        DisplayRosterListViewer();

        if (OnCreate)
          CreateKerbalViewer();
        else if (SelectedKerbal != null)
        {
          EditKerbalViewer();
        }
        else
        {
          GUILayout.BeginHorizontal();
          GUI.enabled = SMSettings.EnableCrewModify;
          GUIContent guilabel = new GUIContent(SmUtils.SmTags["#smloc_roster_002"], GUI.enabled // "Create Kerbal"
            ? SmUtils.SmTags["#smloc_roster_tt_001"] // Realistic Control is On.  Create a Kerbal is disabled.
            : SmUtils.SmTags["#smloc_roster_tt_022"]); // "Opens the Kerbal creation editor."
          if (GUILayout.Button(guilabel, GUILayout.MaxWidth(120), GUILayout.Height(20)))
          {
            OnCreate = true;
          }
          rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && ShowToolTips)
            ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);
          GUILayout.EndHorizontal();
          GUI.enabled = true;
        }

        GUILayout.EndVertical();
        GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
        SMAddon.RepositionWindow(ref Position);
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage($" in Roster Window.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}",
          SmUtils.LogType.Error, true);
      }
    }

    private static bool CanDisplayKerbal(ProtoCrewMember kerbal)
    {
      switch (CurrentFilter)
      {
        case KerbalFilters.All:
          return true;
        case KerbalFilters.Assigned:
          if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Assigned)
            return true;
          break;
        case KerbalFilters.Available:
          if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available)
            return true;
          break;
        case KerbalFilters.Dead:
          if ((kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Dead &&
               kerbal.type != ProtoCrewMember.KerbalType.Unowned) ||
              kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Missing)
            return true;
          break;
        case KerbalFilters.Frozen:
          if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Dead &&
              kerbal.type == ProtoCrewMember.KerbalType.Unowned)
            return true;
          break;
        case KerbalFilters.Missing:
          if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Missing)
            return true;
          break;
        case KerbalFilters.Vessel:
          if (FlightGlobals.ActiveVessel.GetVesselCrew().Contains(kerbal) ||
              (InstalledMods.IsDfInstalled &&
               GetFrozenKerbalDetails(kerbal).Contains(FlightGlobals.ActiveVessel.vesselName.Replace("(unloaded)", ""))))
            return true;
          break;
      }
      return false;
    }

    private static void CreateKerbalViewer()
    {
      DisplaySelectProfession();
      GUILayout.BeginHorizontal();
      // "Create", "Creates a Kerbal with profession selected above.\r\nAdds him/her to the Roster."
      GUIContent guilabel = new GUIContent(SmUtils.SmTags["#smloc_roster_003"], SmUtils.SmTags["#smloc_roster_tt_002"]);
      if (GUILayout.Button(guilabel, GUILayout.MaxWidth(80), GUILayout.Height(20)))
      {
        bool kerbalFound = false;
        ProtoCrewMember.KerbalType kerbalType = KerbalProfession == Professions.Tourist
          ? ProtoCrewMember.KerbalType.Tourist
          : ProtoCrewMember.KerbalType.Crew;
        while (!kerbalFound)
        {
          SelectedKerbal = ModKerbal.CreateKerbal(kerbalType);
          if (SelectedKerbal.Trait == KerbalProfession.ToString())
            kerbalFound = true;
        }
        OnCreate = false;
      }
      Rect rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);
      //guilabel = new GUIContent("Cancel", "Cancels current creation and exit editor.");
      guilabel = new GUIContent(SmUtils.SmTags["#smloc_roster_004"], SmUtils.SmTags["#smloc_roster_tt_003"]);
      if (GUILayout.Button(guilabel, GUILayout.MaxWidth(80), GUILayout.Height(20)))
      {
        OnCreate = false;
        SelectedKerbal = null;
      }
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);
      GUILayout.EndHorizontal();
    }

    private static void DisplaySelectProfession()
    {
      GUILayout.BeginHorizontal();
      GUILayout.Label(SmUtils.SmTags["#smloc_roster_005"], GUILayout.Width(85)); // "Profession:"
      bool isPilot = GUILayout.Toggle(KerbalProfession == Professions.Pilot, SmUtils.SmTags["#smloc_roster_006"], GUILayout.Width(90)); // "Pilot"
      if (isPilot) KerbalProfession = Professions.Pilot;

      bool isEngineer = GUILayout.Toggle(KerbalProfession == Professions.Engineer, SmUtils.SmTags["#smloc_roster_007"], GUILayout.Width(90)); // "Engineer"
      if (isEngineer) KerbalProfession = Professions.Engineer;

      bool isScientist = GUILayout.Toggle(KerbalProfession == Professions.Scientist, SmUtils.SmTags["#smloc_roster_008"], GUILayout.Width(90)); // "Scientist"
      if (isScientist) KerbalProfession = Professions.Scientist;

      bool isTourist = GUILayout.Toggle(KerbalProfession == Professions.Tourist, SmUtils.SmTags["#smloc_roster_032"], GUILayout.Width(90)); // "Toruist"
      if (isTourist) KerbalProfession = Professions.Tourist;

      GUILayout.EndHorizontal();
    }

    private static void DisplayRosterFilter()
    {
      GUILayout.BeginHorizontal();
      GUILayout.Label($"{SmUtils.SmTags["#smloc_roster_009"]}:", GUILayout.Width(40)); // Filter

      bool isAll = GUILayout.Toggle(CurrentFilter == KerbalFilters.All, SmUtils.SmTags["#smloc_roster_010"], GUILayout.Width(60)); // "All"
      if (isAll) CurrentFilter = KerbalFilters.All;

      bool isAssign = GUILayout.Toggle(CurrentFilter == KerbalFilters.Assigned, SmUtils.SmTags["#smloc_roster_011"], GUILayout.Width(95)); // "Assigned"
      if (isAssign) CurrentFilter = KerbalFilters.Assigned;

      if (HighLogic.LoadedSceneIsFlight)
      {
        bool isVessel = GUILayout.Toggle(CurrentFilter == KerbalFilters.Vessel, SmUtils.SmTags["#smloc_roster_012"], GUILayout.Width(80)); // "Vessel"
        if (isVessel) CurrentFilter = KerbalFilters.Vessel;
      }

      bool isAvail = GUILayout.Toggle(CurrentFilter == KerbalFilters.Available, SmUtils.SmTags["#smloc_roster_013"], GUILayout.Width(95)); // "Available"
      if (isAvail) CurrentFilter = KerbalFilters.Available;

      bool isDead = GUILayout.Toggle(CurrentFilter == KerbalFilters.Dead, SmUtils.SmTags["#smloc_roster_014"], GUILayout.Width(130)); // "Dead/Missing"
      if (isDead) CurrentFilter = KerbalFilters.Dead;

      if (InstalledMods.IsDfInstalled)
      {
        bool isFrozen = GUILayout.Toggle(CurrentFilter == KerbalFilters.Frozen, SmUtils.SmTags["#smloc_roster_015"], GUILayout.Width(80)); // "Frozen"
        if (isFrozen) CurrentFilter = KerbalFilters.Frozen;
      }
      GUILayout.EndHorizontal();
    }

    private static void DisplayRosterListViewer()
    {
      try
      {
        GUILayout.BeginVertical();
        // Roster List Header...
        GUILayout.BeginHorizontal();
        //GUILayout.Label("", GUILayout.Width(5));
        GUILayout.Label(SmUtils.SmTags["#smloc_roster_016"], GUILayout.Width(140)); // "Name"
        GUILayout.Label(SmUtils.SmTags["#smloc_roster_017"], GUILayout.Width(50)); // "Gender"
        GUILayout.Label(SmUtils.SmTags["#smloc_roster_005"], GUILayout.Width(70)); // "Profession"
        GUILayout.Label(SmUtils.SmTags["#smloc_roster_018"], GUILayout.Width(30)); // "Skill"
        GUILayout.Label(SmUtils.SmTags["#smloc_roster_019"], GUILayout.Width(220)); // "Status"
        GUILayout.Label(SmUtils.SmTags["#smloc_roster_020"], GUILayout.Width(55)); // "Edit"
        GUILayout.Label(SmUtils.SmTags["#smloc_roster_021"], GUILayout.Width(65)); // "Action"
        GUILayout.EndHorizontal();

        _scrollViewerPosition = GUILayout.BeginScrollView(_scrollViewerPosition, SMStyle.ScrollStyle,
          GUILayout.Height(ViewBox.height), GUILayout.Width(ViewBox.width));

        // vars for acton to occurs after button press
        bool isAction = false;
        Part actionPart = null;
        string actionText = "";
        ProtoCrewMember actionKerbal = null;

        List<ProtoCrewMember>.Enumerator kerbals = RosterList.GetEnumerator();
        while (kerbals.MoveNext())
        {
          if (kerbals.Current == null) continue;
          if (!CanDisplayKerbal(kerbals.Current)) continue;
          GUIStyle labelStyle;
          if (kerbals.Current.rosterStatus == ProtoCrewMember.RosterStatus.Dead ||
              kerbals.Current.rosterStatus == ProtoCrewMember.RosterStatus.Missing)
              labelStyle = SMStyle.LabelStyleRed;
          else if (kerbals.Current.rosterStatus == ProtoCrewMember.RosterStatus.Assigned)
              labelStyle = SMStyle.LabelStyleYellow;
          else
            labelStyle = SMStyle.LabelStyle;

          // What vessel is this Kerbal Assigned to?
          string rosterDetails = "";
          if (kerbals.Current.rosterStatus == ProtoCrewMember.RosterStatus.Assigned)
          {
            List<Vessel>.Enumerator theseVessels = FlightGlobals.Vessels.GetEnumerator();
            while (theseVessels.MoveNext())
            {
              if (theseVessels.Current == null) continue;
              List<ProtoCrewMember> crew = theseVessels.Current.GetVesselCrew();
              if (crew.Any(crewMember => crewMember == kerbals.Current))
              {
                rosterDetails =
                  $"{SmUtils.SmTags["#smloc_roster_011"]} - {theseVessels.Current.GetName().Replace("(unloaded)", "")}"; // "Assigned"
              }
            }
            theseVessels.Dispose();
          }
          else if (InstalledMods.IsDfInstalled && DfWrapper.ApiReady && kerbals.Current.type == ProtoCrewMember.KerbalType.Unowned)
          {
            // This kerbal could be frozen.  Lets find out...
            rosterDetails = GetFrozenKerbalDetails(kerbals.Current);
            labelStyle = SMStyle.LabelStyleCyan;
          }
          else
          {
            // Since the kerbal has no vessel assignment, lets show what their status...
              rosterDetails = kerbals.Current.rosterStatus.ToString();
            }
          string buttonText;
          string buttonToolTip;
          GUILayout.BeginHorizontal();
          GUILayout.Label(kerbals.Current.name, labelStyle, GUILayout.Width(140), GUILayout.Height(20));
          GUILayout.Label(kerbals.Current.gender.ToString(), labelStyle, GUILayout.Width(50));
          GUILayout.Label(kerbals.Current.experienceTrait.Title, labelStyle, GUILayout.Width(70));
          GUILayout.Label(kerbals.Current.experienceLevel.ToString(), labelStyle, GUILayout.Width(30));
          GUILayout.Label(rosterDetails, labelStyle, GUILayout.Width(215));

          SetupEditButton(kerbals.Current, out buttonText, out buttonToolTip);
          if (GUILayout.Button(new GUIContent(buttonText, buttonToolTip), GUILayout.Width(55), GUILayout.Height(20),
            GUILayout.Height(20)))
          {
            if (SelectedKerbal == null || SelectedKerbal.Kerbal != kerbals.Current)
            {
              SelectedKerbal = new ModKerbal(kerbals.Current, false);
              SetProfessionFlag();
            }
            else
            {
              SelectedKerbal = null;
            }
          }
          Rect rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && ShowToolTips)
            ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, XOffset);

          // Setup buttons with gui state, button text and tooltip.
          SetupActionButton(kerbals.Current, out buttonText, out buttonToolTip);

          if (GUILayout.Button(new GUIContent(buttonText, buttonToolTip), GUILayout.Width(65), GUILayout.Height(20)))
          {
            isAction = true;
            actionKerbal = kerbals.Current;
            actionText = buttonText;
            if (actionText == SmUtils.SmTags["#smloc_roster_022"]) // "Remove"
              actionPart = SMAddon.SmVessel.FindPartByKerbal(kerbals.Current);
          }
          Rect rect2 = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && ShowToolTips)
            ToolTip = SMToolTips.SetActiveToolTip(rect2, GUI.tooltip, ref ToolTipActive, XOffset);
          GUILayout.EndHorizontal();
          GUI.enabled = true;
        }
        kerbals.Dispose();
        GUILayout.EndVertical();
        GUILayout.EndScrollView();

        // perform action from button press.
        if (!isAction) return;
        if (actionText == SmUtils.SmTags["#smloc_roster_022"]) // "Remove"
          TransferCrew.RemoveCrewMember(actionKerbal, actionPart);
        else if(actionText == SmUtils.SmTags["#smloc_roster_023"]) // "Add"
          TransferCrew.AddCrewMember(actionKerbal, SMAddon.SmVessel.SelectedPartsSource[0]);
        else if (actionText == SmUtils.SmTags["#smloc_roster_024"]) // "Respawn"
          RespawnKerbal(actionKerbal);
        else if (actionText == SmUtils.SmTags["#smloc_roster_025"]) // "Thaw"
          ThawKerbal(actionKerbal.name);
        else if (actionText == SmUtils.SmTags["#smloc_roster_026"])// "Freeze"
          FreezeKerbal(actionKerbal);
        //Refresh all lists...
        if (SMAddon.SmVessel?.Vessel != null) {
          GameEvents.onVesselWasModified.Fire(SMAddon.SmVessel?.Vessel);
        }
      }
      catch (Exception ex)
      {
        if (!SMAddon.FrameErrTripped)
        {
          SmUtils.LogMessage($" in RosterListViewer.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}", SmUtils.LogType.Error, true);
        }
      }
    }

    private static void EditKerbalViewer()
    {
      //GUILayout.Label(SelectedKerbal.IsNew ? "Create Kerbal" : "Edit Kerbal");
      GUILayout.Label(SelectedKerbal.IsNew ? SmUtils.SmTags["#smloc_roster_002"] : SmUtils.SmTags["#smloc_roster_027"]);
      if (SMSettings.EnableKerbalRename)
      {
        GUILayout.BeginHorizontal();
        SelectedKerbal.Name = GUILayout.TextField(SelectedKerbal.Name, GUILayout.MaxWidth(300));
        GUILayout.Label($" - ({SelectedKerbal.Kerbal.experienceTrait.Title})");
        GUILayout.EndHorizontal();
      }
      else
        GUILayout.Label($"{SelectedKerbal.Name} - ({SelectedKerbal.Trait})", SMStyle.LabelStyleBold,
          GUILayout.MaxWidth(300));

      if (!string.IsNullOrEmpty(SMAddon.SaveMessage))
      {
        GUILayout.Label(SMAddon.SaveMessage, SMStyle.ErrorLabelRedStyle);
      }
      if (SMSettings.EnableKerbalRename && SMSettings.EnableChangeProfession)
      {
        DisplaySelectProfession();
      }
      bool isMale = ProtoCrewMember.Gender.Male == SelectedKerbal.Gender;
      GUILayout.BeginHorizontal();
      GUILayout.Label(SmUtils.SmTags["#smloc_roster_017"], GUILayout.Width(85)); // "Gender"
      isMale = GUILayout.Toggle(isMale, ProtoCrewMember.Gender.Male.ToString(), GUILayout.Width(90));
      isMale = GUILayout.Toggle(!isMale, ProtoCrewMember.Gender.Female.ToString());
      SelectedKerbal.Gender = isMale ? ProtoCrewMember.Gender.Female : ProtoCrewMember.Gender.Male;
      GUILayout.EndHorizontal();

      GUILayout.Label(SmUtils.SmTags["#smloc_roster_029"]); // "Courage"
      SelectedKerbal.Courage = GUILayout.HorizontalSlider(SelectedKerbal.Courage, 0, 1, GUILayout.MaxWidth(300));

      GUILayout.Label(SmUtils.SmTags["#smloc_roster_030"]); // "Stupidity"
      SelectedKerbal.Stupidity = GUILayout.HorizontalSlider(SelectedKerbal.Stupidity, 0, 1, GUILayout.MaxWidth(300));

      SelectedKerbal.Badass = GUILayout.Toggle(SelectedKerbal.Badass, SmUtils.SmTags["#smloc_roster_031"], GUILayout.Height(30)); // "Badass"

      GUILayout.BeginHorizontal();
      if (GUILayout.Button(SmUtils.SmTags["#smloc_roster_004"], GUILayout.MaxWidth(50))) // "Cancel"
      {
        SelectedKerbal = null;
      }
      string label = SmUtils.SmTags["#smloc_roster_028"]; // "Apply"
      //string toolTip = "Applies the changes made to this Kerbal.\r\nDesired Name and Profession will be Retained after save.";
      string toolTip = SmUtils.SmTags["#smloc_roster_tt_006"];
      if (GUILayout.Button(new GUIContent(label, toolTip), GUILayout.MaxWidth(50)))
      {
        if (SMSettings.EnableKerbalRename && SMSettings.EnableChangeProfession)
        {
          if (SelectedKerbal != null) SelectedKerbal.Trait = KerbalProfession.ToString();
        }
        if (SelectedKerbal != null)
        {
          SMAddon.SaveMessage = SelectedKerbal.SubmitChanges();
          GetRosterList();
          if (string.IsNullOrEmpty(SMAddon.SaveMessage))
            SelectedKerbal = null;
        }
      }
      Rect rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);
      GUILayout.EndHorizontal();
    }

    internal static void GetRosterList()
    {
      try
      {
        RosterList.Clear();
        RosterList = HighLogic.CurrentGame.CrewRoster.Crew.ToList();
        RosterList.AddRange(HighLogic.CurrentGame.CrewRoster.Tourist);
        // Support for DeepFreeze
        if (InstalledMods.IsDfInstalled && DfWrapper.ApiReady)
          RosterList.AddRange(HighLogic.CurrentGame.CrewRoster.Unowned);
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage($"Error in GetRosterList().\r\nError:  {ex}", SmUtils.LogType.Error, true);
      }
    }

    private static void SetupEditButton(ProtoCrewMember kerbal, out string buttonText, out string buttonToolTip)
    {
      GUI.enabled = kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available && SMSettings.EnableCrewModify;

      //buttonText = SelectedKerbal == null || SelectedKerbal.Kerbal != kerbal ? "Edit" : "Cancel";
      buttonText = SelectedKerbal == null || SelectedKerbal.Kerbal != kerbal ? SmUtils.SmTags["#smloc_roster_020"] : SmUtils.SmTags["#smloc_roster_004"];
      if (GUI.enabled)
        buttonToolTip = SelectedKerbal == null || SelectedKerbal.Kerbal != kerbal
          ? SmUtils.SmTags["#smloc_roster_tt_007"] // "Edit this Kerbal's attributes"
          : SmUtils.SmTags["#smloc_roster_tt_008"]; // "Cancel any changes to this Kerbal"
      else
        buttonToolTip = kerbal.rosterStatus != ProtoCrewMember.RosterStatus.Available 
          ? SmUtils.SmTags["#smloc_roster_tt_009"] // "Kerbal is not available at this time.\r\nEditing is disabled";
          : SmUtils.SmTags["#smloc_roster_tt_021"]; // "Realistic Control is On.\r\nEditing is disabled";
    }

    private static void SetupActionButton(ProtoCrewMember kerbal, out string buttonText, out string buttonToolTip)
    {
      if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
      {
        if (SMConditions.CanKerbalBeAdded(kerbal))
        {
          GUI.enabled = true;
          buttonText = SmUtils.SmTags["#smloc_roster_023"];  // "Add";
          buttonToolTip = SmUtils.SmTags["#smloc_roster_tt_010"];  
          //buttonToolTip = "Adds a kerbal to the Selected Source Part,\r\nin the first available seat.";
        }
        else if (SMConditions.FrozenKerbalNotThawable(kerbal))
        {
          GUI.enabled = false;
          buttonText = SmUtils.SmTags["#smloc_roster_025"];  // "Thaw";
          buttonToolTip = SmUtils.SmTags["#smloc_roster_tt_011"];  
          // buttonToolTip = "Thaw disabled.  Vessel not active. UnFreeze a Kerbal and Revive them.\r\nWill then become assigned to current vessel.";
        }
        else if (SMConditions.FrozenKerbalIsThawable(kerbal))
        {
          GUI.enabled = true;
          buttonText = SmUtils.SmTags["#smloc_roster_025"];  // "Thaw";
          buttonToolTip = SmUtils.SmTags["#smloc_roster_tt_012"];
          // buttonToolTip = "UnFreeze a Kerbal and Revive them.\r\nWill then become assigned to current vessel.";
        }
        else if (SMConditions.CanKerbalBeFrozen(kerbal))
        {
          GUI.enabled = true;
          buttonText = SmUtils.SmTags["#smloc_roster_026"];  // "Freeze";
          buttonToolTip = SmUtils.SmTags["#smloc_roster_tt_013"];
          // buttonToolTip = "Freezes a Kerbal in the DeepFreezer.\r\nWill then become Unowned and will not consume life support.";
        }
        else if (SMConditions.CanKerbalBeRemoved(kerbal))
        {
          GUI.enabled = true;
          buttonText = SmUtils.SmTags["#smloc_roster_022"];  // "Remove";
          buttonToolTip = SmUtils.SmTags["#smloc_roster_tt_014"];  // "Removes a Kerbal from the active vessel.\r\nWill then become available.";
        }
        else if (SMConditions.KerbalCannotBeRemovedRealism(kerbal))
        {
          GUI.enabled = false;
          buttonText = SmUtils.SmTags["#smloc_roster_022"];  // "Remove";
          buttonToolTip = SmUtils.SmTags["#smloc_roster_tt_023"];  // "Remove Disabled. Roster Modifications is preventing this action.\r\nTo Remove this Kerbal, Change your Roster Modifications Setting.";
        }
        else if (SMConditions.KerbalCannotBeAddedNoSource(kerbal))
        {
          GUI.enabled = false;
          buttonText = SmUtils.SmTags["#smloc_roster_023"];  // "Add";
          buttonToolTip = SmUtils.SmTags["#smloc_roster_tt_015"];
          // buttonToolTip = "Add Disabled.  No source part is selected.\r\nTo add a Kerbal, Select a Source Part with an available seat.";
        }
        else if (SMConditions.KerbalCannotBeAddedRealism(kerbal))
        {
          GUI.enabled = false;
          buttonText = SmUtils.SmTags["#smloc_roster_023"];  // "Add";
          buttonToolTip = SmUtils.SmTags["#smloc_roster_tt_016"];
          // buttonToolTip = "Add Disabled.  Roster Modifications is preventing this action.\r\nTo add a Kerbal, Change your Roster Modifications Setting.";
        }
        else
        {
          GUI.enabled = false;
          buttonText = "--";
          buttonToolTip = SmUtils.SmTags["#smloc_roster_tt_017"];
          // buttonToolTip = "Kerbal is not available.\r\nCurrent status does not allow any action.";
        }
      }
      else // HighLogic.LoadedScene == GameScenes.SPACECENTER
      {
        GUI.enabled = false;
        buttonText = "--";
        buttonToolTip = SmUtils.SmTags["#smloc_roster_tt_018"];
        // buttonToolTip = "Kerbal is not dead or missing.\r\nCurrent status does not allow any action while in Space Center.";
      }

      // Applies to both scenes.
      if (SMConditions.CanKerbalBeReSpawned(kerbal))
      {
        GUI.enabled = SMSettings.EnableCrewModify;
        buttonText = SmUtils.SmTags["#smloc_roster_024"];  // "Respawn";
        buttonToolTip = SMSettings.EnableCrewModify 
          ? SmUtils.SmTags["#smloc_roster_tt_020"] // "Brings a Kerbal back to life.\r\nWill then become available.";
          : SmUtils.SmTags["#smloc_roster_tt_019"]; // "Realistic Control is preventing this action.";
      }
    }

    #endregion Gui Layout

    #region Action Methods
    private static string GetFrozenKerbalDetails(ProtoCrewMember kerbal)
    {
      try
      {
        string rosterDetails = "";
        if (!DfWrapper.ApiReady) DfWrapper.InitDfWrapper();
        if (DfWrapper.ApiReady)
        {
          rosterDetails = DfWrapper.DeepFreezeApi.FrozenKerbals.ContainsKey(kerbal.name) 
            ? $"{SmUtils.SmTags["#smloc_roster_015"]} - {DfWrapper.DeepFreezeApi.FrozenKerbals[kerbal.name].VesselName.Replace("(unloaded)", "")}" 
            : SmUtils.SmTags["#smloc_roster_015"];
        }
        return rosterDetails;
      }
      catch (Exception ex)
      {
        if (!SMAddon.FrameErrTripped)
        {
          SmUtils.LogMessage($" in GetRosterList().\r\nError:  {ex}", SmUtils.LogType.Error, true);
        }
        return $"{SmUtils.SmTags["#smloc_error_001"]}:"; // "Display Error"
      }
    }

    public static void ResetKerbalNames()
    {
      IEnumerator<ProtoCrewMember> kerbals = HighLogic.CurrentGame.CrewRoster.Crew.GetEnumerator();
      while (kerbals.MoveNext())
      {
        if (kerbals.Current == null) continue;
        if (!kerbals.Current.name.Contains(char.ConvertFromUtf32(1))) continue;
        kerbals.Current.ChangeName(kerbals.Current.name.Replace(char.ConvertFromUtf32(1), ""));
      }
      kerbals.Dispose();
    }

    private static void SetProfessionFlag()
    {
      switch (SelectedKerbal.Trait)
      {
        case "Pilot":
          KerbalProfession = Professions.Pilot;
          break;
        case "Engineer":
          KerbalProfession = Professions.Engineer;
          break;
        case "Scientist":
          KerbalProfession = Professions.Scientist;
          break;
        default:
          KerbalProfession = Professions.Tourist;
          break;
      }
    }

    internal static void ThawKerbal(string kerbalName)
    {
      try
      {
        if (InstalledMods.IsDfApiReady)
        {
          DfWrapper.KerbalInfo iKerbal = DfWrapper.DeepFreezeApi.FrozenKerbals[kerbalName];

          List<Part>.Enumerator cryofreezers = SmUtils.GetFreezerParts().GetEnumerator();
          while (cryofreezers.MoveNext())
          {
            if (cryofreezers.Current == null) continue;
            if (cryofreezers.Current.flightID == iKerbal.PartId)
            {
              // ReSharper disable once SuspiciousTypeConversion.Global
              PartModule deepFreezer = SMConditions.GetFreezerModule(cryofreezers.Current);
              if (deepFreezer != null) new DfWrapper.DeepFreezer(deepFreezer).BeginThawKerbal(kerbalName);
              break;
            }
          }
          cryofreezers.Dispose();
        }
        else
        {
          SmUtils.LogMessage($"ThawKerbal.  IsDFInstalled:  {InstalledMods.IsDfInstalled}", SmUtils.LogType.Info,
            true);
        }
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage($" in ThawKerbal.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}",
          SmUtils.LogType.Error, true);
      }
    }

    internal static void FreezeKerbal(ProtoCrewMember kerbal)
    {
      try
      {
        if (!InstalledMods.IsDfApiReady) return;
        List<Part>.Enumerator cryofreezers = SmUtils.GetFreezerParts().GetEnumerator();
        while (cryofreezers.MoveNext())
        {
          if (cryofreezers.Current == null) continue;
          if (!cryofreezers.Current.protoModuleCrew.Contains(kerbal)) continue;
          // ReSharper disable once SuspiciousTypeConversion.Global
          PartModule deepFreezer = SMConditions.GetFreezerModule(cryofreezers.Current);
          if (deepFreezer != null) new DfWrapper.DeepFreezer(deepFreezer).BeginFreezeKerbal(kerbal);
          break;
        }
        cryofreezers.Dispose();
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage($" in FreezeKerbal.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}",
          SmUtils.LogType.Error, true);
      }
    }

    internal static void RespawnKerbal(ProtoCrewMember kerbal)
    {
      kerbal.SetTimeForRespawn(0);
      kerbal.rosterStatus = ProtoCrewMember.RosterStatus.Available;
      kerbal.KerbalRef.rosterStatus = ProtoCrewMember.RosterStatus.Available;
      HighLogic.CurrentGame.CrewRoster.GetNextAvailableKerbal();
    }

    #endregion Methods

    //Profession vars
    internal enum Professions
    {
      Pilot,
      Engineer,
      Scientist,
      Tourist,
      Other
    }

    //Filter vars
    internal enum KerbalFilters
    {
      All,
      Assigned,
      Available,
      Dead,
      Frozen,
      Missing,
      Vessel
    }
  }
}
