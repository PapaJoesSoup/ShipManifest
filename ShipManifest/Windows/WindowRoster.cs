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

    internal static float WindowWidth = 700;
    internal static float WindowHeight = 330;
    internal static string Title = "Ship Manifest Roster";
    internal static Rect Position = new Rect(0, 0, 0, 0);
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
          SMAddon.SaveMessage = String.Empty;
        }
      }
    }

    #endregion Properties

    #region Gui Layout
    internal static void Display(int windowId)
    {
      // Reset Tooltip active flag...
      ToolTipActive = false;

      var rect = new Rect(Position.width - 20, 4, 16, 16);
      if (GUI.Button(rect, new GUIContent("", "Close Window")))
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
        ToolTip = SMToolTips.SetActiveToolTip(rect, Position, GUI.tooltip, ref ToolTipActive, 10, 0);
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
          var guilabel = new GUIContent("Create Kerbal", "Opens the Kerbal creation editor.");
          if (GUILayout.Button(guilabel, GUILayout.MaxWidth(120), GUILayout.Height(20)))
          {
            OnCreate = true;
          }
          rect = GUILayoutUtility.GetLastRect();
          if (Event.current.type == EventType.Repaint && ShowToolTips)
            ToolTip = SMToolTips.SetActiveToolTip(rect, Position, GUI.tooltip, ref ToolTipActive, 10, 0);
          GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
        GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
        SMAddon.RepositionWindow(ref Position);
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(String.Format(" in Roster Window.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace),
          "Error", true);
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
               GetFrozenKerbalDetials(kerbal).Contains(FlightGlobals.ActiveVessel.vesselName.Replace("(unloaded)", ""))))
            return true;
          break;
      }
      return false;
    }

    private static void CreateKerbalViewer()
    {
      DisplaySelectProfession();
      GUILayout.BeginHorizontal();
      var guilabel = new GUIContent("Create",
        "Creates a Kerbal with profession selected above.\r\nAdds him/her to the Roster.");
      if (GUILayout.Button(guilabel, GUILayout.MaxWidth(80), GUILayout.Height(20)))
      {
        var kerbalFound = false;
        while (!kerbalFound)
        {
          SelectedKerbal = ModKerbal.CreateKerbal();
          if (SelectedKerbal.Trait == KerbalProfession.ToString())
            kerbalFound = true;
        }
        OnCreate = false;
      }
      var rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, Position, GUI.tooltip, ref ToolTipActive, 10, 0);
      guilabel = new GUIContent("Cancel", "Cancels current creation and exit editor.");
      if (GUILayout.Button(guilabel, GUILayout.MaxWidth(80), GUILayout.Height(20)))
      {
        OnCreate = false;
        SelectedKerbal = null;
      }
      rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, Position, GUI.tooltip, ref ToolTipActive, 10, 0);
      GUILayout.EndHorizontal();
    }

    private static void DisplaySelectProfession()
    {
      GUILayout.BeginHorizontal();
      GUILayout.Label("Profession:", GUILayout.Width(85));
      var isPilot = GUILayout.Toggle(KerbalProfession == Professions.Pilot, "Pilot", GUILayout.Width(90));
      if (isPilot) KerbalProfession = Professions.Pilot;

      var isEngineer = GUILayout.Toggle(KerbalProfession == Professions.Engineer, "Engineer", GUILayout.Width(90));
      if (isEngineer) KerbalProfession = Professions.Engineer;

      var isScientist = GUILayout.Toggle(KerbalProfession == Professions.Scientist, "Scientist", GUILayout.Width(90));
      if (isScientist) KerbalProfession = Professions.Scientist;
      GUILayout.EndHorizontal();
    }

    private static void DisplayRosterFilter()
    {
      GUILayout.BeginHorizontal();
      GUILayout.Label("Filter:", GUILayout.Width(40));

      var isAll = GUILayout.Toggle(CurrentFilter == KerbalFilters.All, "All", GUILayout.Width(60));
      if (isAll) CurrentFilter = KerbalFilters.All;

      var isAssign = GUILayout.Toggle(CurrentFilter == KerbalFilters.Assigned, "Assigned", GUILayout.Width(95));
      if (isAssign) CurrentFilter = KerbalFilters.Assigned;

      if (HighLogic.LoadedSceneIsFlight)
      {
        var isVessel = GUILayout.Toggle(CurrentFilter == KerbalFilters.Vessel, "Vessel", GUILayout.Width(80));
        if (isVessel) CurrentFilter = KerbalFilters.Vessel;
      }

      var isAvail = GUILayout.Toggle(CurrentFilter == KerbalFilters.Available, "Available", GUILayout.Width(95));
      if (isAvail) CurrentFilter = KerbalFilters.Available;

      var isDead = GUILayout.Toggle(CurrentFilter == KerbalFilters.Dead, "Dead/Missing", GUILayout.Width(130));
      if (isDead) CurrentFilter = KerbalFilters.Dead;

      if (InstalledMods.IsDfInstalled)
      {
        var isFrozen = GUILayout.Toggle(CurrentFilter == KerbalFilters.Frozen, "Frozen", GUILayout.Width(80));
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
        GUILayout.Label("Name", GUILayout.Width(140));
        GUILayout.Label("Gender", GUILayout.Width(50));
        GUILayout.Label("Profession", GUILayout.Width(70));
        GUILayout.Label("Skill", GUILayout.Width(30));
        GUILayout.Label("Status", GUILayout.Width(220));
        GUILayout.Label("Edit", GUILayout.Width(55));
        GUILayout.Label("Action", GUILayout.Width(65));
        GUILayout.EndHorizontal();

        _scrollViewerPosition = GUILayout.BeginScrollView(_scrollViewerPosition, SMStyle.ScrollStyle,
          GUILayout.Height(230), GUILayout.Width(680));
        foreach (var kerbal in RosterList)
        {
          if (CanDisplayKerbal(kerbal))
          {
            GUIStyle labelStyle;
            if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Dead ||
                kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Missing)
              labelStyle = SMStyle.LabelStyleRed;
            else if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Assigned)
              labelStyle = SMStyle.LabelStyleYellow;
            else
              labelStyle = SMStyle.LabelStyle;

            // What vessel is this Kerbal Assigned to?
            var rosterDetails = "";
            if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Assigned)
            {
              foreach (var thisVessel in FlightGlobals.Vessels)
              {
                var crew = thisVessel.GetVesselCrew();
                if (crew.Any(crewMember => crewMember == kerbal))
                {
                  rosterDetails = "Assigned - " + thisVessel.GetName().Replace("(unloaded)", "");
                }
              }
            }
            else if (InstalledMods.IsDfInstalled && kerbal.type == ProtoCrewMember.KerbalType.Unowned)
            {
              // This kerbal could be frozen.  Lets find out...
              rosterDetails = GetFrozenKerbalDetials(kerbal);
              labelStyle = SMStyle.LabelStyleCyan;
            }
            else
            {
              // Since the kerbal has no vessel assignment, lets show what their status...
              rosterDetails = kerbal.rosterStatus.ToString();
            }
            string buttonText;
            string buttonToolTip;
            GUILayout.BeginHorizontal();
            GUILayout.Label(kerbal.name, labelStyle, GUILayout.Width(140), GUILayout.Height(20));
            GUILayout.Label(kerbal.gender.ToString(), labelStyle, GUILayout.Width(50));
            GUILayout.Label(kerbal.experienceTrait.Title, labelStyle, GUILayout.Width(70));
            GUILayout.Label(kerbal.experienceLevel.ToString(), labelStyle, GUILayout.Width(30));
            GUILayout.Label(rosterDetails, labelStyle, GUILayout.Width(215));

            SetupEditButton(kerbal, out buttonText, out buttonToolTip);

            if (GUILayout.Button(new GUIContent(buttonText, buttonToolTip), GUILayout.Width(55), GUILayout.Height(20),
              GUILayout.Height(20)))
            {
              if (SelectedKerbal == null || SelectedKerbal.Kerbal != kerbal)
              {
                SelectedKerbal = new ModKerbal(kerbal, false);
                SetProfessionFlag();
              }
              else
              {
                SelectedKerbal = null;
              }
            }
            var rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips)
              ToolTip = SMToolTips.SetActiveToolTip(rect, Position, GUI.tooltip, ref ToolTipActive, XOffset,
                YOffset - _scrollViewerPosition.y);

            // Setup buttons with gui state, button text and tooltip.
            SetupActionButton(kerbal, out buttonText, out buttonToolTip);

            if (GUILayout.Button(new GUIContent(buttonText, buttonToolTip), GUILayout.Width(65), GUILayout.Height(20)))
            {
              if (buttonText == "Add")
                TransferCrew.AddCrewMember(kerbal, SMAddon.SmVessel.SelectedPartsSource[0]);
              else if (buttonText == "Respawn")
                RespawnKerbal(kerbal);
              else if (buttonText == "Thaw")
                ThawKerbal(kerbal.name);
              else if (buttonText == "Freeze")
                FreezeKerbal(kerbal);
              else if (buttonText == "Remove")
              {
                // get part...
                var part = SMAddon.SmVessel.FindPartByKerbal(kerbal);
                if (part != null)
                  TransferCrew.RemoveCrewMember(kerbal, part);
              }
            }
            var rect2 = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips)
              ToolTip = SMToolTips.SetActiveToolTip(rect2, Position, GUI.tooltip, ref ToolTipActive, XOffset,
                YOffset - _scrollViewerPosition.y);
            GUILayout.EndHorizontal();
            GUI.enabled = true;
          }
        }

        GUILayout.EndVertical();
        GUILayout.EndScrollView();
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(
          String.Format(" in RosterListViewer.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
      }
    }

    private static void EditKerbalViewer()
    {
      GUILayout.Label(SelectedKerbal.IsNew ? "Create a Kerbal" : "Edit a Kerbal");
      if (SMSettings.EnableKerbalRename)
      {
        GUILayout.BeginHorizontal();
        SelectedKerbal.Name = GUILayout.TextField(SelectedKerbal.Name, GUILayout.MaxWidth(300));
        GUILayout.Label(" - (" + SelectedKerbal.Kerbal.experienceTrait.Title + ")");
        GUILayout.EndHorizontal();
      }
      else
        GUILayout.Label(SelectedKerbal.Name + " - (" + SelectedKerbal.Trait + ")", SMStyle.LabelStyleBold,
          GUILayout.MaxWidth(300));

      if (!String.IsNullOrEmpty(SMAddon.SaveMessage))
      {
        GUILayout.Label(SMAddon.SaveMessage, SMStyle.ErrorLabelRedStyle);
      }
      if (SMSettings.EnableKerbalRename && SMSettings.RenameWithProfession)
      {
        DisplaySelectProfession();
      }
      var isMale = ProtoCrewMember.Gender.Male == SelectedKerbal.Gender;
      GUILayout.BeginHorizontal();
      GUILayout.Label("Gender");
      isMale = GUILayout.Toggle(isMale, ProtoCrewMember.Gender.Male.ToString(), GUILayout.Width(90));
      isMale = GUILayout.Toggle(!isMale, ProtoCrewMember.Gender.Female.ToString());
      SelectedKerbal.Gender = isMale ? ProtoCrewMember.Gender.Female : ProtoCrewMember.Gender.Male;
      GUILayout.EndHorizontal();

      GUILayout.Label("Courage");
      SelectedKerbal.Courage = GUILayout.HorizontalSlider(SelectedKerbal.Courage, 0, 1, GUILayout.MaxWidth(300));

      GUILayout.Label("Stupidity");
      SelectedKerbal.Stupidity = GUILayout.HorizontalSlider(SelectedKerbal.Stupidity, 0, 1, GUILayout.MaxWidth(300));

      SelectedKerbal.Badass = GUILayout.Toggle(SelectedKerbal.Badass, "Badass");

      GUILayout.BeginHorizontal();
      if (GUILayout.Button("Cancel", GUILayout.MaxWidth(50)))
      {
        SelectedKerbal = null;
      }
      var label = "Apply";
      var toolTip =
        "Applies the changes made to this Kerbal.\r\nDesired Name and Profession will be Retained after save.";
      if (GUILayout.Button(new GUIContent(label, toolTip), GUILayout.MaxWidth(50)))
      {
        if (SMSettings.EnableKerbalRename && SMSettings.RenameWithProfession)
        {
          if (SelectedKerbal != null) SelectedKerbal.Trait = KerbalProfession.ToString();
        }
        if (SelectedKerbal != null)
        {
          SMAddon.SaveMessage = SelectedKerbal.SubmitChanges();
          if (String.IsNullOrEmpty(SMAddon.SaveMessage))
            SelectedKerbal = null;
        }
      }
      var rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, Position, GUI.tooltip, ref ToolTipActive, 10, 0);
      GUILayout.EndHorizontal();
    }

    internal static void GetRosterList()
    {
      try
      {
        RosterList.Clear();
        RosterList = HighLogic.CurrentGame.CrewRoster.Crew.ToList();
        // Support for DeepFreeze
        if (InstalledMods.IsDfInstalled)
          RosterList.AddRange(HighLogic.CurrentGame.CrewRoster.Unowned);
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(String.Format("Error in GetRosterList().\r\nError:  {0}", ex), "Error", true);
      }
    }

    private static void SetupEditButton(ProtoCrewMember kerbal, out string buttonText, out string buttonToolTip)
    {
      GUI.enabled = kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available;

      buttonText = SelectedKerbal == null || SelectedKerbal.Kerbal != kerbal ? "Edit" : "Cancel";
      if (GUI.enabled)
        buttonToolTip = SelectedKerbal == null || SelectedKerbal.Kerbal != kerbal
          ? "Edit this Kerbal's attributes"
          : "Cancel any changes to this Kerbal";
      else
        buttonToolTip = "Kerbal is not available at this time.\r\nEditing is disabled";
    }

    private static void SetupActionButton(ProtoCrewMember kerbal, out string buttonText, out string buttonToolTip)
    {
      if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
      {
        if (SMConditions.CanKerbalBeAdded(kerbal))
        {
          GUI.enabled = true;
          buttonText = "Add";
          buttonToolTip = "Adds a kerbal to the Selected Source Part,\r\nin the first available seat.";
        }
        else if (SMConditions.FrozenKerbalNotThawable(kerbal))
        {
          GUI.enabled = false;
          buttonText = "Thaw";
          buttonToolTip =
            "Thaw disabled.  Vessel not active. UnFreeze a Kerbal and Revive them.\r\nWill then become assigned to current vessel.";
        }
        else if (SMConditions.FrozenKerbalIsThawable(kerbal))
        {
          GUI.enabled = true;
          buttonText = "Thaw";
          buttonToolTip = "UnFreeze a Kerbal and Revive them.\r\nWill then become assigned to current vessel.";
        }
        else if (SMConditions.CanKerbalBeFrozen(kerbal))
        {
          GUI.enabled = true;
          buttonText = "Freeze";
          buttonToolTip =
            "Freezes a Kerbal in the DeepFreezer.\r\nWill then become Unowned and will not consume life support.";
        }
        else if (SMConditions.CanKerbalBeRemoved(kerbal))
        {
          GUI.enabled = true;
          buttonText = "Remove";
          buttonToolTip = "Removes a Kerbal from the active vessel.\r\nWill then become available.";
        }
        else if (SMConditions.KerbalCannotBeAddedNoSource(kerbal))
        {
          GUI.enabled = false;
          buttonText = "Add";
          buttonToolTip =
            "Add Disabled.  No source part is selected.\r\nTo add a Kerbal, Select a Source Part with an available seat.";
        }
        else if (SMConditions.KerbalCannotBeAddedRealism(kerbal))
        {
          GUI.enabled = false;
          buttonText = "Add";
          buttonToolTip =
            "Add Disabled.  Realism Settings are preventing this action.\r\nTo add a Kerbal, Change your realism Settings.";
        }
        else
        {
          GUI.enabled = false;
          buttonText = "--";
          buttonToolTip = "Kerbal is not available.\r\nCurrent status does not allow any action.";
        }
      }
      else
      {
        GUI.enabled = false;
        buttonText = "--";
        buttonToolTip =
          "Kerbal is not dead or missing.\r\nCurrent status does not allow any action while in Space Center.";
      }

      if (SMConditions.CanKerbalBeReSpawned(kerbal))
      {
        GUI.enabled = true;
        buttonText = "Respawn";
        buttonToolTip = "Brings a Kerbal back to life.\r\nWill then become available.";
      }
    }

    #endregion Gui Layout

    #region Methods
    private static string GetFrozenKerbalDetials(ProtoCrewMember kerbal)
    {
      string rosterDetails;
      if (DFWrapper.DeepFreezeAPI.FrozenKerbals.ContainsKey(kerbal.name))
        rosterDetails = "Frozen - " + DFWrapper.DeepFreezeAPI.FrozenKerbals[kerbal.name].vesselName.Replace("(unloaded)", "");
      else
        rosterDetails = "Frozen";

      return rosterDetails;
    }

    public static void ResetKerbalNames()
    {
      foreach (var kerbal in HighLogic.CurrentGame.CrewRoster.Crew)
      {
        if (kerbal.name.Contains(Char.ConvertFromUtf32(1)))
        {
          kerbal.name = kerbal.name.Replace(Char.ConvertFromUtf32(1), "");
        }
      }
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
          var iKerbal = DFWrapper.DeepFreezeAPI.FrozenKerbals[kerbalName];

          var cryofreezers =
            (from p in SMAddon.SmVessel.Vessel.parts where p.Modules.Contains("DeepFreezer") select p).ToList();
          foreach (var cryoFreezer in cryofreezers)
          {
            if (cryoFreezer.flightID == iKerbal.partID)
            {
              // ReSharper disable once SuspiciousTypeConversion.Global
              var deepFreezer =
                (from PartModule pm in cryoFreezer.Modules
                  where pm.moduleName == "DeepFreezer"
                  select new DFWrapper.DeepFreezer(pm)).SingleOrDefault();
              if (deepFreezer != null) deepFreezer.beginThawKerbal(kerbalName);
              break;
            }
          }
        }
        else
        {
          Utilities.LogMessage(String.Format("ThawKerbal.  IsDFInstalled:  {0}", InstalledMods.IsDfInstalled), "Info",
            true);
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(String.Format(" in ThawKerbal.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace),
          "Error", true);
      }
    }

    internal static void FreezeKerbal(ProtoCrewMember kerbal)
    {
      try
      {
        if (InstalledMods.IsDfApiReady)
        {
          var cryofreezers =
            (from p in SMAddon.SmVessel.Vessel.parts where p.Modules.Contains("DeepFreezer") select p).ToList();
          foreach (var cryoFreezer in cryofreezers)
          {
            if (cryoFreezer.protoModuleCrew.Contains(kerbal))
            {
              // ReSharper disable once SuspiciousTypeConversion.Global
              var deepFreezer =
                (from PartModule pm in cryoFreezer.Modules
                  where pm.moduleName == "DeepFreezer"
                  select new DFWrapper.DeepFreezer(pm)).SingleOrDefault();
              if (deepFreezer != null) deepFreezer.beginFreezeKerbal(kerbal);
              break;
            }
          }
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(String.Format(" in FreezeKerbal.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace),
          "Error", true);
      }
    }

    internal static void RespawnKerbal(ProtoCrewMember kerbal)
    {
      kerbal.SetTimeForRespawn(0);
      // This call causes issues in KSC scene, and is not needed.
      //kerbal.Spawn();
      kerbal.rosterStatus = ProtoCrewMember.RosterStatus.Available;
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