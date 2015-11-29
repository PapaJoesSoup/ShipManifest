using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DF;
using ShipManifest.Modules;
using ShipManifest.Process;

namespace ShipManifest.Windows
{
  internal static class WindowRoster
  {
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

    //Profession vars
    internal static bool IsPilot;
    internal static bool IsEngineer;
    internal static bool IsScientist;
    internal static string KerbalProfession
    {
      get
      {
        if (IsPilot)
          return "Pilot";
        if (IsEngineer)
          return "Engineer";
        if (IsScientist)
          return "Scientist";
        return "";
      }
    }

    // Gender var
    internal static ProtoCrewMember.Gender Gender = ProtoCrewMember.Gender.Male;

    //Filter vars
    internal static bool IsAll = true;
    internal static bool IsAssign;
    internal static bool IsVessel;
    internal static bool IsAvail;
    internal static bool IsDead;
    internal static bool IsFrozen;

    internal static bool OnCreate;
    internal static bool ResetRosterSize
    {
      get
      {
        if (!OnCreate && SelectedKerbal == null)
          return true;
        else
          return false;
      }
    }

    private static List<ProtoCrewMember> _rosterList;
    internal static List<ProtoCrewMember> RosterList
    {
      get
      {
        if (_rosterList == null)
          GetRosterList();
        return _rosterList;
      }
    }

    private static ModKerbal _selectedKerbal;
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

    private static Vector2 _scrollViewerPosition = Vector2.zero;
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
          SMAddon.OnSmRosterToggle();
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
          //if (SMSettings.RenameWithProfession)
          //{
          //    string toolTip = "This action resets all renamed Kerbals to their KSP default professions.\r\nIt removes any non printing chars previously used to maintain a specific profession.\r\nUse this if you wish to clean up a game save after updating to KSP 1.0.5 or above.";
          //    if (GUILayout.Button(new GUIContent("Reset Professions", toolTip), GUILayout.MaxWidth(120), GUILayout.Height(20)))
          //    {
          //        ResetKerbalNames();
          //    }
          //}
          //rect = GUILayoutUtility.GetLastRect();
          //if (Event.current.type == EventType.Repaint && ShowToolTips == true)
          //    ToolTip = SMToolTips.SetActiveTooltip(rect, WindowRoster.Position, GUI.tooltip, ref ToolTipActive, 10, 0);
          GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
        GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
        SMAddon.RepositionWindow(ref Position);
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format(" in Roster Window.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
      }
    }

    internal static Dictionary<string, KerbalInfo> GetFrozenKerbals()
    {
      if (DFInterface.IsDFInstalled)
      {
        var freezer = DFInterface.GetFrozenKerbals();
        return freezer.FrozenKerbals;
      }
      else
        return new Dictionary<string, KerbalInfo>();
    }

    internal static void GetRosterList()
    {
      try
      {
        if (_rosterList != null)
          _rosterList.Clear();
        _rosterList = HighLogic.CurrentGame.CrewRoster.Crew.ToList();
        // Support for DeepFreeze
        if (DFInterface.IsDFInstalled)
          _rosterList.AddRange(HighLogic.CurrentGame.CrewRoster.Unowned);
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format("Error in GetRosterList().\r\nError:  {0}", ex), "Error", true);
      }
    }

    internal static bool IsKerbalReadyToFreeze(ProtoCrewMember kerbal)
    {
      return kerbal.seat.part.Modules.Contains("DeepFreezer") && !SMPart.IsCrewFull(kerbal.seat.part);
    }

    internal static void ThawKerbal(string kerbalName)
    {
      try
      {
        if (DFInterface.IsDFInstalled)
        {
          var iKerbal = SMAddon.FrozenKerbals[kerbalName];

          var cryofreezers = (from p in SMAddon.SmVessel.Vessel.parts where p.Modules.Contains("DeepFreezer") select p).ToList();
          foreach (var cryoFreezer in cryofreezers)
          {
            if (cryoFreezer.flightID == iKerbal.partID)
            {
              // ReSharper disable once SuspiciousTypeConversion.Global
              var deepFreezer = (from PartModule pm in cryoFreezer.Modules where pm.moduleName == "DeepFreezer" select (IDeepFreezer)pm).SingleOrDefault();
              if (deepFreezer != null) deepFreezer.beginThawKerbal(kerbalName);
              break;
            }
          }
        }
        else
        {
          Utilities.LogMessage(string.Format("ThawKerbal.  IsDFInstalled:  {0}", DFInterface.IsDFInstalled), "Info", true);
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format(" in ThawKerbal.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
      }
    }

    internal static void FreezeKerbal(ProtoCrewMember kerbal)
    {
      try
      {
        if (DFInterface.IsDFInstalled)
        {
          var cryofreezers = (from p in SMAddon.SmVessel.Vessel.parts where p.Modules.Contains("DeepFreezer") select p).ToList();
          foreach (var cryoFreezer in cryofreezers)
          {
            if (cryoFreezer.protoModuleCrew.Contains(kerbal))
            {
              // ReSharper disable once SuspiciousTypeConversion.Global
              var deepFreezer = (from PartModule pm in cryoFreezer.Modules where pm.moduleName == "DeepFreezer" select (IDeepFreezer)pm).SingleOrDefault();
              if (deepFreezer != null) deepFreezer.beginFreezeKerbal(kerbal);
              break;
            }
          }
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format(" in FreezeKerbal.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
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

    private static bool CanDisplayKerbal(ProtoCrewMember kerbal)
    {
      if (IsAll)
        return true;
      else if (IsVessel && (FlightGlobals.ActiveVessel.GetVesselCrew().Contains(kerbal) || (DFInterface.IsDFInstalled && GetFrozenKerbalDetials(kerbal).Contains(FlightGlobals.ActiveVessel.vesselName.Replace("(unloaded)", "")))))
        return true;
      else if (IsAssign && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Assigned)
        return true;
      else if (IsAvail && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available)
        return true;
      else if (IsDead && ((kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Dead && kerbal.type != ProtoCrewMember.KerbalType.Unowned) || kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Missing))
        return true;
      else if (IsFrozen && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Dead && kerbal.type == ProtoCrewMember.KerbalType.Unowned)
        return true;
      else
        return false;
    }

    private static void CreateKerbalViewer()
    {
      DisplaySelectProfession();
      GUILayout.BeginHorizontal();
      var guilabel = new GUIContent("Create", "Creates a Kerbal with profession selected above.\r\nAdds him/her to the Roster.");
      if (GUILayout.Button(guilabel, GUILayout.MaxWidth(80), GUILayout.Height(20)))
      {
        var kerbalFound = false;
        while (!kerbalFound)
        {
          SelectedKerbal = ModKerbal.CreateKerbal();
          if (SelectedKerbal.Trait == KerbalProfession)
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
      IsPilot = GUILayout.Toggle(IsPilot, "Pilot", GUILayout.Width(90));
      if (IsPilot)
        IsEngineer = IsScientist = false;
      else
      {
        if (!IsEngineer && !IsScientist)
          IsPilot = true;
      }
      IsEngineer = GUILayout.Toggle(IsEngineer, "Engineer", GUILayout.Width(90));
      if (IsEngineer)
        IsPilot = IsScientist = false;
      else
      {
        if (!IsPilot && !IsScientist)
          IsEngineer = true;
      }
      IsScientist = GUILayout.Toggle(IsScientist, "Scientist", GUILayout.Width(90));
      if (IsScientist)
        IsPilot = IsEngineer = false;
      else
      {
        if (!IsPilot && !IsEngineer)
          IsScientist = true;
      }
      GUILayout.EndHorizontal();
    }

    private static void DisplayRosterFilter()
    {
      GUILayout.BeginHorizontal();
      GUILayout.Label("Filter:", GUILayout.Width(40));
      IsAll = GUILayout.Toggle(IsAll, "All", GUILayout.Width(60));
      if (IsAll)
        IsVessel = IsAssign = IsAvail = IsDead = IsFrozen = false;
      else
      {
        if (!IsVessel && !IsAssign && !IsAvail && !IsDead && !IsFrozen)
          IsAll = true;
      }
      IsAssign = GUILayout.Toggle(IsAssign, "Assigned", GUILayout.Width(95));
      if (IsAssign)
        IsAll = IsVessel = IsAvail = IsDead = false;
      else
      {
        if (!IsAll && !IsVessel && !IsAvail && !IsDead && !IsFrozen)
          IsAssign = true;
      }
      if (HighLogic.LoadedSceneIsFlight)
      {
        IsVessel = GUILayout.Toggle(IsVessel, "Vessel", GUILayout.Width(80));
        if (IsVessel)
          IsAll = IsAssign = IsAvail = IsDead = IsFrozen = false;
        else
        {
          if (!IsAll && !IsAssign && !IsAvail && !IsDead && !IsFrozen)
            IsVessel = true;
        }
      }
      IsAvail = GUILayout.Toggle(IsAvail, "Available", GUILayout.Width(95));
      if (IsAvail)
        IsAll = IsAssign = IsVessel = IsDead = IsFrozen = false;
      else
      {
        if (!IsAll && !IsVessel && !IsAssign && !IsDead && !IsFrozen)
          IsAvail = true;
      }
      IsDead = GUILayout.Toggle(IsDead, "Dead/Missing", GUILayout.Width(130));
      if (IsDead)
        IsAll = IsAssign = IsVessel = IsAvail = IsFrozen = false;
      else
      {
        if (!IsAll && !IsVessel && !IsAssign && !IsAvail && !IsFrozen)
          IsDead = true;
      }
      if (DFInterface.IsDFInstalled)
      {
        IsFrozen = GUILayout.Toggle(IsFrozen, "Frozen", GUILayout.Width(80));
        if (IsFrozen)
          IsAll = IsAssign = IsVessel = IsAvail = IsDead = false;
        else
        {
          if (!IsAll && !IsVessel && !IsAssign && !IsAvail && !IsDead)
            IsFrozen = true;
        }
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

        _scrollViewerPosition = GUILayout.BeginScrollView(_scrollViewerPosition, SMStyle.ScrollStyle, GUILayout.Height(230), GUILayout.Width(680));
        foreach (var kerbal in RosterList)
        {
          if (CanDisplayKerbal(kerbal))
          {
            GUIStyle labelStyle;
            if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Dead || kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Missing)
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
            else if (DFInterface.IsDFInstalled && kerbal.type == ProtoCrewMember.KerbalType.Unowned)
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

            if (GUILayout.Button(new GUIContent(buttonText, buttonToolTip), GUILayout.Width(55), GUILayout.Height(20), GUILayout.Height(20)))
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
              ToolTip = SMToolTips.SetActiveToolTip(rect, Position, GUI.tooltip, ref ToolTipActive, XOffset, YOffset - _scrollViewerPosition.y);

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
              ToolTip = SMToolTips.SetActiveToolTip(rect2, Position, GUI.tooltip, ref ToolTipActive, XOffset, YOffset - _scrollViewerPosition.y);
            GUILayout.EndHorizontal();
            GUI.enabled = true;
          }
        }

        GUILayout.EndVertical();
        GUILayout.EndScrollView();

      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format(" in RosterListViewer.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
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
        GUILayout.Label(SelectedKerbal.Name + " - (" + SelectedKerbal.Trait + ")", SMStyle.LabelStyleBold, GUILayout.MaxWidth(300));

      if (!string.IsNullOrEmpty(SMAddon.SaveMessage))
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
      var toolTip = "Applies the changes made to this Kerbal.\r\nDesired Name and Profession will be Retained after save.";
      if (GUILayout.Button(new GUIContent(label, toolTip), GUILayout.MaxWidth(50)))
      {
        if (SMSettings.EnableKerbalRename && SMSettings.RenameWithProfession)
        {
          if (SelectedKerbal != null) SelectedKerbal.Trait = KerbalProfession;
        }
        if (SelectedKerbal != null)
        {
          SMAddon.SaveMessage = SelectedKerbal.SubmitChanges();
          if (string.IsNullOrEmpty(SMAddon.SaveMessage))
            SelectedKerbal = null;
        }
      }
      var rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, Position, GUI.tooltip, ref ToolTipActive, 10, 0);
      GUILayout.EndHorizontal();
    }

    private static string GetFrozenKerbalDetials(ProtoCrewMember kerbal)
    {
      string rosterDetails;
      if (GetFrozenKerbals().ContainsKey(kerbal.name))
        rosterDetails = "Frozen - " + GetFrozenKerbals()[kerbal.name].vesselName.Replace("(unloaded)", "");
      else
        rosterDetails = "Frozen";

      return rosterDetails;
    }

    public static void ResetKerbalNames()
    {
      foreach (var kerbal in HighLogic.CurrentGame.CrewRoster.Crew)
      {
        if (kerbal.name.Contains(char.ConvertFromUtf32(1)))
        {
          kerbal.name = kerbal.name.Replace(char.ConvertFromUtf32(1), "");
        }
      }
    }

    private static void SetProfessionFlag()
    {
      if (SelectedKerbal.Trait == "Pilot")
      {
        IsPilot = true;
        IsEngineer = false;
        IsScientist = false;
      }
      else if (SelectedKerbal.Trait == "Engineer")
      {
        IsPilot = false;
        IsEngineer = true;
        IsScientist = false;
      }
      else
      {
        IsPilot = false;
        IsEngineer = false;
        IsScientist = true;
      }
    }

    private static void SetupEditButton(ProtoCrewMember kerbal, out string buttonText, out string buttonToolTip)
    {
      GUI.enabled = kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available;

      buttonText = SelectedKerbal == null || SelectedKerbal.Kerbal != kerbal ? "Edit" : "Cancel";
      if (GUI.enabled)
        buttonToolTip = SelectedKerbal == null || SelectedKerbal.Kerbal != kerbal ? "Edit this Kerbal's attributes" : "Cancel any changes to this Kerbal";
      else
        buttonToolTip = "Kerbal is not available at this time.\r\nEditing is disabled";

    }

    private static void SetupActionButton(ProtoCrewMember kerbal, out string buttonText, out string buttonToolTip)
    {
      if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
      {
        if (((SMSettings.RealismMode && SMAddon.SmVessel.IsRecoverable) || !SMSettings.RealismMode) && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available && SMAddon.SmVessel.SelectedPartsSource.Count > 0 && !SMPart.IsCrewFull(SMAddon.SmVessel.SelectedPartsSource[0]))
        {
          GUI.enabled = true;
          buttonText = "Add";
          buttonToolTip = "Adds a kerbal to the Selected Source Part,\r\nin the first available seat.";
        }
        else if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Dead && kerbal.type == ProtoCrewMember.KerbalType.Unowned && SMAddon.FrozenKerbals[kerbal.name].vesselID != FlightGlobals.ActiveVessel.id)
        {
          GUI.enabled = false;
          buttonText = "Thaw";
          buttonToolTip = "Thaw disabled.  Vessel not active. UnFreeze a Kerbal and Revive them.\r\nWill then become assigned to current vessel.";
        }
        else if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Dead && kerbal.type == ProtoCrewMember.KerbalType.Unowned && SMAddon.FrozenKerbals[kerbal.name].vesselID == FlightGlobals.ActiveVessel.id)
        {
          GUI.enabled = true;
          buttonText = "Thaw";
          buttonToolTip = "UnFreeze a Kerbal and Revive them.\r\nWill then become assigned to current vessel.";
        }
        else if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Assigned && FlightGlobals.ActiveVessel.GetVesselCrew().Contains(kerbal) && IsKerbalReadyToFreeze(kerbal))
        {
          GUI.enabled = true;
          buttonText = "Freeze";
          buttonToolTip = "Freezes a Kerbal in the DeepFreezer.\r\nWill then become Unowned and will not consume life support.";
        }
        else if (((SMSettings.RealismMode && SMAddon.SmVessel.IsRecoverable) || !SMSettings.RealismMode) && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Assigned && FlightGlobals.ActiveVessel.GetVesselCrew().Contains(kerbal))
        {
          GUI.enabled = true;
          buttonText = "Remove";
          buttonToolTip = "Removes a Kerbal from the active vessel.\r\nWill then become available.";
        }
        else if (((SMSettings.RealismMode && SMAddon.SmVessel.IsRecoverable) || !SMSettings.RealismMode) && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available && SMAddon.SmVessel.SelectedPartsSource.Count == 0)
        {
          GUI.enabled = false;
          buttonText = "Add";
          buttonToolTip = "Add Disabled.  No source part is selected.\r\nTo add a Kerbal, Select a Source Part with an available seat.";
        }
        else if (SMSettings.RealismMode && !SMAddon.SmVessel.IsRecoverable && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available)
        {
          GUI.enabled = false;
          buttonText = "Add";
          buttonToolTip = "Add Disabled.  Realism Settings are preventing this action.\r\nTo add a Kerbal, Change your realism Settings.";
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
        buttonToolTip = "Kerbal is not dead or missing.\r\nCurrent status does not allow any action while in Space Center.";
      }

      if ((kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Dead && kerbal.type != ProtoCrewMember.KerbalType.Unowned) || kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Missing)
      {
        GUI.enabled = true;
        buttonText = "Respawn";
        buttonToolTip = "Brings a Kerbal back to life.\r\nWill then become available.";
      }
    }

  }
}
