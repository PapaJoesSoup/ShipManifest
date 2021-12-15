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

    internal static float WindowWidth = 830;
    internal static float WindowHeight = 330;
    internal static Rect Position = SMSettings.DefaultPosition;
    internal static Rect ViewBox = new Rect(0, 0, 810, 230);
    private static bool _inputLocked;
    private static bool _showWindow;
    internal static bool ShowWindow
    {
      get => _showWindow;
      set
      {
        if (!value)
        {
          InputLockManager.RemoveControlLock("SM_Window");
          _inputLocked = false;
        }
        _showWindow = value;
      }
    }
    internal static string ToolTip = "";
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;
    internal static float XOffset = 30;
    internal static float YOffset = 90;

    // RosterList must exist outside of the vessel.
    internal static List<ProtoCrewMember> RosterList = new List<ProtoCrewMember>();

    internal static Profession KerbalProfession;

    // Gender var
    internal static ProtoCrewMember.Gender Gender = ProtoCrewMember.Gender.Male;
    internal static KerbalFilter CurrentFilter = KerbalFilter.All;


    internal static EditMode editMode;
    private static ModKerbal _selectedKerbal;

    private static Vector2 _scrollViewerPosition = Vector2.zero;

    #region Localization Strings
    // Content strings
    internal static string Title                = $"{SmUtils.SmTags["#smloc_roster_001"]}:  {SMSettings.CurVersion}";
    internal static GUIContent closeContent     = new GUIContent("", SmUtils.SmTags["#smloc_window_tt_001"]);
    internal static string addKerbalContent     = SmUtils.SmTags["#smloc_roster_002"];
    internal static string addKerbalOffContent  = SmUtils.SmTags["#smloc_roster_tt_001"];
    internal static string addKerbalEditContent = SmUtils.SmTags["#smloc_roster_tt_022"];
    internal static GUIContent createContent    = new GUIContent(SmUtils.SmTags["#smloc_roster_003"], SmUtils.SmTags["#smloc_roster_tt_002"]);
    internal static GUIContent cancelContent    = new GUIContent(SmUtils.SmTags["#smloc_roster_004"], SmUtils.SmTags["#smloc_roster_tt_003"]);
    internal static string cnxEditContent       = SmUtils.SmTags["#smloc_roster_004"];
    internal static GUIContent profContent      = new GUIContent(SmUtils.SmTags["#smloc_roster_005"]);
    internal static string pilotContent         = SmUtils.SmTags["#smloc_roster_006"];
    internal static string engineerContent      = SmUtils.SmTags["#smloc_roster_007"];
    internal static string scientistContent     = SmUtils.SmTags["#smloc_roster_008"];
    internal static GUIContent filterContent    = new GUIContent($"{SmUtils.SmTags["#smloc_roster_009"]}:");
    internal static string allContent           = SmUtils.SmTags["#smloc_roster_010"];
    internal static string assignContent        = SmUtils.SmTags["#smloc_roster_011"];
    internal static string vesselContent        = SmUtils.SmTags["#smloc_roster_012"];
    internal static string availContent         = SmUtils.SmTags["#smloc_roster_013"];
    internal static string deadContent          = SmUtils.SmTags["#smloc_roster_014"];
    internal static string frozenContent        = SmUtils.SmTags["#smloc_roster_015"];
    internal static string nameContent          = SmUtils.SmTags["#smloc_roster_016"];
    internal static string genderContent        = SmUtils.SmTags["#smloc_roster_017"];
    internal static string skillContent         = SmUtils.SmTags["#smloc_roster_018"];
    internal static string statusContent        = SmUtils.SmTags["#smloc_roster_019"];
    internal static string editContent          = SmUtils.SmTags["#smloc_roster_020"];
    internal static string actionContent        = SmUtils.SmTags["#smloc_roster_021"];
    internal static string removeContent        = SmUtils.SmTags["#smloc_roster_022"];
    internal static string addContent           = SmUtils.SmTags["#smloc_roster_023"];
    internal static string respawnContent       = SmUtils.SmTags["#smloc_roster_024"]; 
    internal static string thawContent          = SmUtils.SmTags["#smloc_roster_025"];
    internal static string freezeContent        = SmUtils.SmTags["#smloc_roster_026"];
    internal static string editKerbalContent    = SmUtils.SmTags["#smloc_roster_027"];
    internal static GUIContent applyContent     = new GUIContent(SmUtils.SmTags["#smloc_roster_028"], SmUtils.SmTags["#smloc_roster_tt_006"]);
    internal static string courageContent       = SmUtils.SmTags["#smloc_roster_029"];
    internal static string stupidContent        = SmUtils.SmTags["#smloc_roster_030"];
    internal static string badassContent        = SmUtils.SmTags["#smloc_roster_031"];
    internal static string touristContent       = SmUtils.SmTags["#smloc_roster_032"];
    internal static string applicantContent     = SmUtils.SmTags["#smloc_roster_033"];
    internal static string hireContent          = SmUtils.SmTags["#smloc_roster_034"];
    internal static string veteranContent       = SmUtils.SmTags["#smloc_roster_035"];
    internal static GUIContent suitContent      = new GUIContent(SmUtils.SmTags["#smloc_roster_036"]);
    internal static GUIContent dfltSuitContent  = new GUIContent(SmUtils.SmTags["#smloc_roster_037"]);
    internal static GUIContent vntgeSuitContent = new GUIContent(SmUtils.SmTags["#smloc_roster_038"]);
    internal static GUIContent futrSuitContent  = new GUIContent(SmUtils.SmTags["#smloc_roster_039"]);
    internal static string brokenContent        = SmUtils.SmTags["#smloc_roster_040"];
    internal static string fixContent           = SmUtils.SmTags["#smloc_roster_041"];
    internal static GUIContent slimSuitContent  = new GUIContent(SmUtils.SmTags["#smloc_roster_042"]);

    internal static string editKerbalTtContent  = SmUtils.SmTags["#smloc_roster_tt_007"];
    internal static string cnxEditKrblTtContent = SmUtils.SmTags["#smloc_roster_tt_008"];
    internal static string notAvailTtContent    = SmUtils.SmTags["#smloc_roster_tt_009"];
    internal static string addKerbalTtContent   = SmUtils.SmTags["#smloc_roster_tt_010"];
    internal static string thawNoTtContent      = SmUtils.SmTags["#smloc_roster_tt_011"];
    internal static string thawYesTtContent     = SmUtils.SmTags["#smloc_roster_tt_012"];
    internal static string freezeTtContent      = SmUtils.SmTags["#smloc_roster_tt_013"];
    internal static string removeYesTtContent   = SmUtils.SmTags["#smloc_roster_tt_014"];
    internal static string addNoSrcTtContent    = SmUtils.SmTags["#smloc_roster_tt_015"];
    internal static string addNoModTtContent    = SmUtils.SmTags["#smloc_roster_tt_016"];
    internal static string noAvlKrblTtContent   = SmUtils.SmTags["#smloc_roster_tt_017"];
    internal static string krblNotDeadTtContent = SmUtils.SmTags["#smloc_roster_tt_018"];
    internal static string respawnNoTtContent   = SmUtils.SmTags["#smloc_roster_tt_019"];
    internal static string respwanYesTtContent  = SmUtils.SmTags["#smloc_roster_020"];
    internal static string realismOnTtContent   = SmUtils.SmTags["#smloc_roster_tt_021"];
    internal static string removeNoModTtContent = SmUtils.SmTags["#smloc_roster_tt_023"];
    internal static string hireTtContent        = SmUtils.SmTags["#smloc_roster_tt_024"];
    internal static string fixTtContent         = SmUtils.SmTags["#smloc_roster_tt_025"];
    internal static string suitKerbalTtContent  = "Change this Kerbal's Suit";
    internal static string cnxSuitKrblTtContent = "Cancel changes to this Kerbal'Suit";
    internal static string displayErrorContent  = SmUtils.SmTags["#smloc_error_001"];
    #endregion Localization Strings


    internal static bool ResetRosterSize
    {
      get
      {
        return editMode != EditMode.Create && SelectedKerbal == null;
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

    internal static void Display(int _windowId)
    {
      
      // set input locks when mouseover window...
      _inputLocked = GuiUtils.PreventClickthrough(ShowWindow, Position, _inputLocked);

      // Reset Tooltip active flag...
      ToolTipActive = false;

      Rect rect = new Rect(Position.width - 20, 4, 16, 16);
      if (GUI.Button(rect, closeContent)) // "Close Window"
      {
        editMode = EditMode.None;
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

        if (editMode == EditMode.Create)
          CreateKerbalViewer();
        else if (editMode == EditMode.Edit && SelectedKerbal != null)
        {
          EditKerbalViewer();
        }
        else if (editMode == EditMode.Suit)
        {
          EditSuitViewer();
        }
        else
        {
          GUILayout.BeginHorizontal();
          GUI.enabled = SMSettings.EnableCrewModify;
          GUIContent guilabel = new GUIContent(addKerbalContent, GUI.enabled ? addKerbalOffContent : addKerbalEditContent); // "Opens the Kerbal creation editor."
          if (GUILayout.Button(guilabel, GUILayout.MaxWidth(120), GUILayout.Height(20)))
          {
            editMode = EditMode.Create;
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
        case KerbalFilter.All:
          return true;
        case KerbalFilter.Assigned:
          if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Assigned)
            return true;
          break;
        case KerbalFilter.Available:
          if ( (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available) &&
               (kerbal.type != ProtoCrewMember.KerbalType.Applicant) )
            return true;
          break;
        case KerbalFilter.Dead:
          if ((kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Dead &&
               kerbal.type != ProtoCrewMember.KerbalType.Unowned) ||
              kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Missing)
            return true;
          break;
        case KerbalFilter.Frozen:
          if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Dead &&
              kerbal.type == ProtoCrewMember.KerbalType.Unowned)
            return true;
          break;
        case KerbalFilter.Missing:
          if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Missing)
            return true;
          break;
        case KerbalFilter.Vessel:
          if (FlightGlobals.ActiveVessel.GetVesselCrew().Contains(kerbal) ||
              (InstalledMods.IsDfInstalled &&
               GetFrozenKerbalDetails(kerbal).Contains(FlightGlobals.ActiveVessel.vesselName.Replace("(unloaded)", ""))))
            return true;
          break;
        case KerbalFilter.Applicant:
          if (kerbal.type == ProtoCrewMember.KerbalType.Applicant)
            return true;
          break;

        case KerbalFilter.Broken:
          if (kerbal.type == ProtoCrewMember.KerbalType.Applicant && kerbal.rosterStatus != ProtoCrewMember.RosterStatus.Available)
            return true;
          break;
      }
      return false;
    }

    private static void CreateKerbalViewer()
    {
      DisplaySelectProfession();
      GUILayout.BeginHorizontal();
      // "Create"
      if (GUILayout.Button(createContent, GUILayout.MaxWidth(80), GUILayout.Height(20)))
      {
        bool kerbalFound = false;
        ProtoCrewMember.KerbalType kerbalType = KerbalProfession == Profession.Tourist
          ? ProtoCrewMember.KerbalType.Tourist
          : ProtoCrewMember.KerbalType.Crew;
        while (!kerbalFound)
        {
          SelectedKerbal = ModKerbal.CreateKerbal(kerbalType);
          if (SelectedKerbal.Trait == KerbalProfession.ToString())
            kerbalFound = true;
        }
        editMode = EditMode.Edit;
      }
      Rect rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);
      // Cancel
      if (GUILayout.Button(cancelContent, GUILayout.MaxWidth(80), GUILayout.Height(20)))
      {
        SelectedKerbal = null;
        editMode = EditMode.None;
      }
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);
      GUILayout.EndHorizontal();
    }

    private static void DisplaySelectProfession()
    {
      GUILayout.BeginHorizontal();
      GUILayout.Label(profContent, GUILayout.Width(85)); // "Profession:"
      bool isPilot = GUILayout.Toggle(KerbalProfession == Profession.Pilot, pilotContent, GUILayout.Width(90)); // "Pilot"
      if (isPilot) KerbalProfession = Profession.Pilot;

      bool isEngineer = GUILayout.Toggle(KerbalProfession == Profession.Engineer, engineerContent, GUILayout.Width(90)); // "Engineer"
      if (isEngineer) KerbalProfession = Profession.Engineer;

      bool isScientist = GUILayout.Toggle(KerbalProfession == Profession.Scientist, scientistContent, GUILayout.Width(90)); // "Scientist"
      if (isScientist) KerbalProfession = Profession.Scientist;

      bool isTourist = GUILayout.Toggle(KerbalProfession == Profession.Tourist, touristContent, GUILayout.Width(90)); // "Tourist"
      if (isTourist) KerbalProfession = Profession.Tourist;

      GUILayout.EndHorizontal();
    }

    private static void DisplaySelectSuit(ref ProtoCrewMember.KerbalSuit suit)
    {
      GUILayout.BeginHorizontal();
      GUILayout.Label(suitContent, GUILayout.Width(85)); // "Suit:"

      // Always available
      bool isSet = GUILayout.Toggle(suit == ProtoCrewMember.KerbalSuit.Default, dfltSuitContent, GUILayout.Width(90)); // "Default"
      if (isSet) suit = ProtoCrewMember.KerbalSuit.Default;

      if (Expansions.ExpansionsLoader.IsExpansionKerbalSuitInstalled(ProtoCrewMember.KerbalSuit.Vintage)) {
        isSet = GUILayout.Toggle(suit == ProtoCrewMember.KerbalSuit.Vintage, vntgeSuitContent, GUILayout.Width(90)); // "Vintage"
        if (isSet) suit = ProtoCrewMember.KerbalSuit.Vintage;
      }
      if (Expansions.ExpansionsLoader.IsExpansionKerbalSuitInstalled(ProtoCrewMember.KerbalSuit.Future)) {
        isSet = GUILayout.Toggle(suit == ProtoCrewMember.KerbalSuit.Future, futrSuitContent, GUILayout.Width(90)); // "Future"
        if (isSet) suit = ProtoCrewMember.KerbalSuit.Future;
      }
      if (Expansions.ExpansionsLoader.IsExpansionKerbalSuitInstalled(ProtoCrewMember.KerbalSuit.Slim)) {
        isSet = GUILayout.Toggle(suit == ProtoCrewMember.KerbalSuit.Slim, slimSuitContent, GUILayout.Width(90)); // "Slim"
        if (isSet) suit = ProtoCrewMember.KerbalSuit.Slim;
      }

      GUILayout.EndHorizontal();
    }

    private static void DisplaySuitColor()
    {

    }

    private static void DisplayRosterFilter()
    {
      GUILayout.BeginHorizontal();
      GUILayout.Label(filterContent, GUILayout.Width(40)); // Filter

      bool isAll = GUILayout.Toggle(CurrentFilter == KerbalFilter.All, allContent, GUILayout.Width(60)); // "All"
      if (isAll) CurrentFilter = KerbalFilter.All;

      bool isAssign = GUILayout.Toggle(CurrentFilter == KerbalFilter.Assigned, assignContent, GUILayout.Width(95)); // "Assigned"
      if (isAssign) CurrentFilter = KerbalFilter.Assigned;

      if (HighLogic.LoadedSceneIsFlight)
      {
        bool isVessel = GUILayout.Toggle(CurrentFilter == KerbalFilter.Vessel, vesselContent, GUILayout.Width(80)); // "Vessel"
        if (isVessel) CurrentFilter = KerbalFilter.Vessel;
      }

      bool isAvail = GUILayout.Toggle(CurrentFilter == KerbalFilter.Available, availContent, GUILayout.Width(95)); // "Available"
      if (isAvail) CurrentFilter = KerbalFilter.Available;

      bool isDead = GUILayout.Toggle(CurrentFilter == KerbalFilter.Dead, deadContent, GUILayout.Width(130)); // "Dead/Missing"
      if (isDead) CurrentFilter = KerbalFilter.Dead;

      if (InstalledMods.IsDfInstalled)
      {
        bool isFrozen = GUILayout.Toggle(CurrentFilter == KerbalFilter.Frozen, frozenContent, GUILayout.Width(80)); // "Frozen"
        if (isFrozen) CurrentFilter = KerbalFilter.Frozen;
      }

      bool isApplicant = GUILayout.Toggle(CurrentFilter == KerbalFilter.Applicant, applicantContent, GUILayout.Width(130)); // "Applicant"
      if (isApplicant) CurrentFilter = KerbalFilter.Applicant;

      bool isBroken = GUILayout.Toggle(CurrentFilter == KerbalFilter.Broken, brokenContent, GUILayout.Width(130)); // "Broken"
      if (isBroken) CurrentFilter = KerbalFilter.Broken;

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
        GUILayout.Label(nameContent, GUILayout.Width(140)); // "Name"
        GUILayout.Label(genderContent, GUILayout.Width(50)); // "Gender"
        GUILayout.Label(profContent, GUILayout.Width(70)); // "Profession"
        GUILayout.Label(skillContent, GUILayout.Width(30)); // "Skill"
        GUILayout.Label(suitContent, GUILayout.Width(70)); // "Suit"
        GUILayout.Label(suitContent, GUILayout.Width(55)); // "Suit"
        GUILayout.Label(statusContent, GUILayout.Width(220)); // "Status"
        GUILayout.Label(editContent, GUILayout.Width(55)); // "Edit"
        GUILayout.Label(actionContent, GUILayout.Width(65)); // "Action"
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
          switch (kerbals.Current.rosterStatus)
          {
            case ProtoCrewMember.RosterStatus.Dead:
            case ProtoCrewMember.RosterStatus.Missing:
            case ProtoCrewMember.RosterStatus.Assigned when kerbals.Current.type == ProtoCrewMember.KerbalType.Applicant:
              labelStyle = SMStyle.LabelStyleRed;
              break;
            case ProtoCrewMember.RosterStatus.Assigned:
              labelStyle = SMStyle.LabelStyleYellow;
              break;
            default:
            {
              labelStyle = kerbals.Current.type == ProtoCrewMember.KerbalType.Applicant ? SMStyle.LabelStyle : SMStyle.LabelStyleGreen;
              break;
            }
          }

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
                  $"{assignContent} - {theseVessels.Current.GetName().Replace("(unloaded)", "")}"; // "Assigned"
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
            rosterDetails = kerbals.Current.type == ProtoCrewMember.KerbalType.Applicant ? kerbals.Current.type.ToString() : kerbals.Current.rosterStatus.ToString();
          }
          GUILayout.BeginHorizontal();
          GUILayout.Label(kerbals.Current.name, labelStyle, GUILayout.Width(140), GUILayout.Height(20));
          GUILayout.Label(kerbals.Current.gender.ToString(), labelStyle, GUILayout.Width(50));
          GUILayout.Label(kerbals.Current.experienceTrait.Title, labelStyle, GUILayout.Width(70));
          GUILayout.Label(kerbals.Current.experienceLevel.ToString(), labelStyle, GUILayout.Width(30));
          GUILayout.Label(kerbals.Current.suit.ToString(), labelStyle, GUILayout.Width(70));

          SetupSuitButton(kerbals.Current, out string buttonText, out string buttonToolTip);
          if (GUILayout.Button(new GUIContent(buttonText, buttonToolTip), GUILayout.Width(55), GUILayout.Height(20),
            GUILayout.Height(20)))
          {
            if (SelectedKerbal == null || SelectedKerbal.Kerbal != kerbals.Current)
            {
              SelectedKerbal = new ModKerbal(kerbals.Current, false);
              SetKerbalSuit(SelectedKerbal);
            }
            else
            {
              SelectedKerbal = null;
              editMode = EditMode.None;
            }
          }
          Rect rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && ShowToolTips)
            ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, XOffset);

          GUILayout.Label(rosterDetails, labelStyle, GUILayout.Width(215));

          SetupEditButton(kerbals.Current, out buttonText, out buttonToolTip);
          if (GUILayout.Button(new GUIContent(buttonText, buttonToolTip), GUILayout.Width(55), GUILayout.Height(20),
            GUILayout.Height(20)))
          {
            if (SelectedKerbal == null || SelectedKerbal.Kerbal != kerbals.Current)
            {
              SelectedKerbal = new ModKerbal(kerbals.Current, false);
              SetProfessionFlag();
              editMode = EditMode.Edit;
            }
            else
            {
              SelectedKerbal = null;
              editMode = EditMode.None;
            }
          }
          rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && ShowToolTips)
            ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, XOffset);

          // Setup buttons with gui state, button text and tooltip.
          SetupActionButton(kerbals.Current, out buttonText, out buttonToolTip);

          if (GUILayout.Button(new GUIContent(buttonText, buttonToolTip), GUILayout.Width(65), GUILayout.Height(20)))
          {
            isAction = true;
            actionKerbal = kerbals.Current;
            actionText = buttonText;
            if (actionText == removeContent) // "Remove"
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
        if (actionText == removeContent) // "Remove"
          TransferCrew.RemoveCrewMember(actionKerbal, actionPart);
        else if(actionText == addContent) // "Add"
          TransferCrew.AddCrewMember(actionKerbal, SMAddon.SmVessel.SelectedPartsSource[0]);
        else if (actionText == thawContent) // "Respawn"
          RespawnKerbal(actionKerbal);
        else if (actionText == thawContent) // "Thaw"
          ThawKerbal(actionKerbal.name);
        else if (actionText == freezeContent)// "Freeze"
          FreezeKerbal(actionKerbal);
        else if (actionText == hireContent) // "Hire"
          HireKerbal(actionKerbal);
        else if (actionText == fixContent) // "Fix"
          RepairKerbal(actionKerbal);
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

    private static void EditSuitViewer()
    {
      GUILayout.Label(SelectedKerbal.IsNew ? addKerbalContent : editKerbalContent);
      SetKerbalSuit(SelectedKerbal);
    }

    private static void EditKerbalViewer()
    {
      GUILayout.Label(SelectedKerbal.IsNew ? addKerbalContent : editKerbalContent);
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
      if (Expansions.ExpansionsLoader.IsExpansionAnyKerbalSuitInstalled())
      {
        DisplaySelectSuit(ref SelectedKerbal.Suit);
      }

      // TODO: Realism setting to enable Kerbal Gender Change for existing Kerbals?
      bool isMale = ProtoCrewMember.Gender.Male == SelectedKerbal.Gender;
      GUILayout.BeginHorizontal();
      GUILayout.Label(genderContent, GUILayout.Width(85)); // "Gender"
      isMale = GUILayout.Toggle(isMale, ProtoCrewMember.Gender.Male.ToString(), GUILayout.Width(90));
      isMale = GUILayout.Toggle(!isMale, ProtoCrewMember.Gender.Female.ToString());
      SelectedKerbal.Gender = isMale ? ProtoCrewMember.Gender.Female : ProtoCrewMember.Gender.Male;
      GUILayout.EndHorizontal();

      GUILayout.Label(courageContent); // "Courage"
      SelectedKerbal.Courage = GUILayout.HorizontalSlider(SelectedKerbal.Courage, 0, 1, GUILayout.MaxWidth(300));

      GUILayout.Label(stupidContent); // "Stupidity"
      SelectedKerbal.Stupidity = GUILayout.HorizontalSlider(SelectedKerbal.Stupidity, 0, 1, GUILayout.MaxWidth(300));

      GUILayout.BeginHorizontal();
      SelectedKerbal.Badass = GUILayout.Toggle(SelectedKerbal.Badass, badassContent, GUILayout.Width(90)); // "Badass"
      SelectedKerbal.Veteran = GUILayout.Toggle(SelectedKerbal.Veteran, veteranContent, GUILayout.Width(90)); // "Veteran"
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      if (GUILayout.Button(cancelContent, GUILayout.MaxWidth(50))) // "Cancel"
      {
        SelectedKerbal = null;
      }
      if (GUILayout.Button(applyContent, GUILayout.MaxWidth(50)))
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
        KerbalRoster roster = HighLogic.CurrentGame.CrewRoster;
        bool haveDeepFreeze = InstalledMods.IsDfInstalled && DfWrapper.ApiReady;
        for( int c = 0; c < roster.Count; c++) {
          // Filter out unowned kerbals if we don't have DeepFreeze
          // TODO: Perhaps we should allow editing of these Kerbals anyway?
          ProtoCrewMember kerbal = roster[c];
          if( kerbal.type == ProtoCrewMember.KerbalType.Unowned && !haveDeepFreeze ) {
            continue;
          }
          RosterList.Add(kerbal);
        }
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage($"Error in GetRosterList().\r\nError:  {ex}", SmUtils.LogType.Error, true);
      }
    }

    private static void SetupSuitButton(ProtoCrewMember kerbal, out string buttonText, out string buttonToolTip)
    {
      GUI.enabled = kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Assigned && SMSettings.EnableCrewModify;

      buttonText = SelectedKerbal == null || SelectedKerbal.Kerbal != kerbal ? suitContent.text : cnxEditContent;
      if (GUI.enabled)
        buttonToolTip = SelectedKerbal == null || SelectedKerbal.Kerbal != kerbal
          ? suitKerbalTtContent // "Change this Kerbal's Suit"
          : cnxSuitKrblTtContent; // "Cancel changes to this Kerbal's Suit"
      else
        buttonToolTip = kerbal.rosterStatus != ProtoCrewMember.RosterStatus.Available 
          ? notAvailTtContent // "Kerbal is not available at this time.\r\nEditing is disabled";
          : realismOnTtContent; // "Realistic Control is On.\r\nEditing is disabled";

    }

    private static void SetupEditButton(ProtoCrewMember kerbal, out string buttonText, out string buttonToolTip)
    {
      GUI.enabled = kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available && SMSettings.EnableCrewModify;

      buttonText = SelectedKerbal == null || SelectedKerbal.Kerbal != kerbal ? editContent : cnxEditContent;
      if (GUI.enabled)
        buttonToolTip = SelectedKerbal == null || SelectedKerbal.Kerbal != kerbal
          ? editKerbalTtContent // "Edit this Kerbal's attributes"
          : cnxEditKrblTtContent; // "Cancel any changes to this Kerbal"
      else
        buttonToolTip = kerbal.rosterStatus != ProtoCrewMember.RosterStatus.Available 
          ? notAvailTtContent // "Kerbal is not available at this time.\r\nEditing is disabled";
          : realismOnTtContent; // "Realistic Control is On.\r\nEditing is disabled";
    }

    private static void SetupActionButton(ProtoCrewMember kerbal, out string buttonText, out string buttonToolTip)
    {
      if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
      {
        if (SMConditions.CanKerbalBeAdded(kerbal))
        {
          GUI.enabled = true;
          buttonText = addContent;  // "Add";
          buttonToolTip = addKerbalTtContent;  
        }
        else if (SMConditions.FrozenKerbalNotThawable(kerbal))
        {
          GUI.enabled = false;
          buttonText = thawContent;  // "Thaw";
          buttonToolTip = thawNoTtContent;  
        }
        else if (SMConditions.FrozenKerbalIsThawable(kerbal))
        {
          GUI.enabled = true;
          buttonText = thawContent;  // "Thaw";
          buttonToolTip = thawYesTtContent;
        }
        else if (SMConditions.CanKerbalBeFrozen(kerbal))
        {
          GUI.enabled = true;
          buttonText = freezeContent;  // "Freeze";
          buttonToolTip = freezeTtContent;
        }
        else if (SMConditions.CanKerbalBeRemoved(kerbal))
        {
          GUI.enabled = true;
          buttonText = removeContent;  // "Remove";
          buttonToolTip = removeYesTtContent;  // "Removes a Kerbal from the active vessel.\r\nWill then become available.";
        }
        else if (SMConditions.KerbalCannotBeRemovedRealism(kerbal))
        {
          GUI.enabled = false;
          buttonText = removeContent;  // "Remove";
          buttonToolTip = removeNoModTtContent;  // "Remove Disabled. Roster Modifications is preventing this action.\r\nTo Remove this Kerbal, Change your Roster Modifications Setting.";
        }
        else if (SMConditions.KerbalCannotBeAddedNoSource(kerbal))
        {
          GUI.enabled = false;
          buttonText = addContent;  // "Add";
          buttonToolTip = addNoSrcTtContent;
          // buttonToolTip = "Add Disabled.  No source part is selected.\r\nTo add a Kerbal, Select a Source Part with an available seat.";
        }
        else if (SMConditions.KerbalCannotBeAddedRealism(kerbal))
        {
          GUI.enabled = false;
          buttonText = addContent;  // "Add";
          buttonToolTip = addNoModTtContent;
          // buttonToolTip = "Add Disabled.  Roster Modifications is preventing this action.\r\nTo add a Kerbal, Change your Roster Modifications Setting.";
        }
        else
        {
          GUI.enabled = false;
          buttonText = "--";
          buttonToolTip = noAvlKrblTtContent;
          // buttonToolTip = "Kerbal is not available.\r\nCurrent status does not allow any action.";
        }
      }
      else // HighLogic.LoadedScene == GameScenes.SPACECENTER
      {
        GUI.enabled = false;
        buttonText = "--";
        buttonToolTip = krblNotDeadTtContent;
        // buttonToolTip = "Kerbal is not dead or missing.\r\nCurrent status does not allow any action while in Space Center.";
      }

      // Applies to both scenes.
      if (SMConditions.CanKerbalBeReSpawned(kerbal))
      {
        GUI.enabled = SMSettings.EnableCrewModify;
        buttonText = respawnContent;  // "Respawn";
        buttonToolTip = SMSettings.EnableCrewModify 
          ? respwanYesTtContent // "Brings a Kerbal back to life.\r\nWill then become available.";
          : respawnNoTtContent; // "Realistic Control is preventing this action.";
      }
      else if(kerbal.type == ProtoCrewMember.KerbalType.Applicant)
      {
        GUI.enabled = true;
        buttonText = hireContent;  // "Hire";
        buttonToolTip = hireTtContent; // "Hire the Applicant and make them a member of your Crew.\nPlease note that this will cost you!"
      }
      else if(SMConditions.KerbalIsBroken(kerbal))
      {
        GUI.enabled = true;
        buttonText = fixContent;  // "Fix";
        buttonToolTip = fixTtContent; // "Repairs a Kerbal whose internal state has become corrupted."
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
            ? $"{frozenContent} - {DfWrapper.DeepFreezeApi.FrozenKerbals[kerbal.name].VesselName.Replace("(unloaded)", "")}" 
            : frozenContent;
        }
        return rosterDetails;
      }
      catch (Exception ex)
      {
        if (!SMAddon.FrameErrTripped)
        {
          SmUtils.LogMessage($" in GetRosterList().\r\nError:  {ex}", SmUtils.LogType.Error, true);
        }
        return $"{displayErrorContent}:"; // "Display Error"
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
          KerbalProfession = Profession.Pilot;
          break;
        case "Engineer":
          KerbalProfession = Profession.Engineer;
          break;
        case "Scientist":
          KerbalProfession = Profession.Scientist;
          break;
        default:
          KerbalProfession = Profession.Tourist;
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

    internal static void HireKerbal(ProtoCrewMember kerbal)
    {
      try
      {
        if( kerbal.type != ProtoCrewMember.KerbalType.Applicant ) {
          throw new Exception("Tried to hire a kerbal which isn't an Applicant: " + kerbal.ToString());
        }
        HighLogic.CurrentGame.CrewRoster.HireApplicant(kerbal);
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage($" in HireKerbal.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}",
          SmUtils.LogType.Error, true);
      }
    }

    internal static void RepairKerbal(ProtoCrewMember kerbal)
    {
      try
      {
        if( !SMConditions.KerbalIsBroken(kerbal) ) {
          throw new Exception("Tried to repair an unbroken kerbal: " + kerbal.ToString());
        }
        // For now, the only broken Kerbals are "Assigned" but for some reason are still Applicants. So we convert them to Crew.
        kerbal.type = ProtoCrewMember.KerbalType.Crew;
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage($" in RepairKerbal.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}",
          SmUtils.LogType.Error, true);
      }
    }

    private static void SetKerbalSuit(ModKerbal selectedKerbal)
    {
      editMode = EditMode.Suit;
      DisplaySelectSuit(ref selectedKerbal.Suit);
      GUILayout.BeginHorizontal();
      if (GUILayout.Button(cancelContent, GUILayout.MaxWidth(50))) // "Cancel"
      {
        SelectedKerbal = null;
        editMode = EditMode.None;
      }

      if (GUILayout.Button(applyContent, GUILayout.MaxWidth(50)))
      {
        if (SelectedKerbal != null)
        {
          SMAddon.SaveMessage = SelectedKerbal.SubmitChanges();
          GetRosterList();
          if (string.IsNullOrEmpty(SMAddon.SaveMessage))
          {
            SelectedKerbal = null;
          }
          editMode = EditMode.None;
        }
      }
      Rect rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);
      GUILayout.EndHorizontal();
    }

    #endregion Methods

    #region ENUMs
    internal enum EditMode
    {
      None,
      Edit,
      Create,
      Suit
    }

    //Profession vars
    internal enum Profession
    {
      Pilot,
      Engineer,
      Scientist,
      Tourist,
      Other
    }

    //Filter vars
    internal enum KerbalFilter
    {
      All,
      Assigned,
      Available,
      Dead,
      Frozen,
      Missing,
      Vessel,
      Applicant,
      Broken
    }
    #endregion ENUMs
  }
}
