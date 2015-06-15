using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using ConnectedLivingSpace;
using DF;

namespace ShipManifest
{
    internal static class WindowRoster
    {
        internal static float windowWidth = 700;
        internal static float windowHeight = 330;
        internal static Rect Position = new Rect(0, 0, 0, 0);
        internal static bool ShowWindow = false;
        internal static string ToolTip = "";
        internal static bool ToolTipActive = false;
        internal static bool ShowToolTips = true;

        //Profession vars
        internal static bool isPilot = false;
        internal static bool isEngineer = false;
        internal static bool isScientist = false;
        internal static string KerbalProfession
        {
            get
            {
                if (isPilot)
                    return "Pilot";
                else if (isEngineer)
                    return "Engineer";
                else if (isScientist)
                    return "Scientist";
                else
                    return "";
            }
        }
        
        // Gender var
        internal static ProtoCrewMember.Gender gender = ProtoCrewMember.Gender.Male;

        //Filter vars
        internal static bool isAll = true;
        internal static bool isVessel = false;
        internal static bool isAvail = false;
        internal static bool isDead = false;

        internal static bool OnCreate = false;
        internal static bool resetRosterSize
        {
            get
            {
                if (!OnCreate && SelectedKerbal == null)
                    return true;
                else
                    return false;
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
                    SMAddon.saveMessage = string.Empty;
                }
            }
        }

        private static Vector2 ScrollViewerPosition = Vector2.zero;
        internal static void Display(int windowId)
        {

            // Reset Tooltip active flag...
            ToolTipActive = false;

            Rect rect = new Rect(Position.width - 20, 4, 16, 16);
            if (GUI.Button(rect, new GUIContent("", "Close Window")))
            {
                OnCreate = false;
                SelectedKerbal = null;
                ToolTip = "";
                if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                    SMAddon.OnSMRosterToggle();
                else
                    ShowWindow = false;

            }
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Position, GUI.tooltip, ref ToolTipActive, 0, 0);
            try
            {
                GUIStyle style = new GUIStyle(GUI.skin.button);
                var defaultColor = style.normal.textColor;
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
                    if (GUILayout.Button("Create Kerbal", GUILayout.MaxWidth(120), GUILayout.Height(20)))
                    {
                        OnCreate = true;
                    }
                    if (SMSettings.RenameWithProfession)
                    {
                        string toolTip = "This action resets all renamed Kerbals to their KSP default professions.\r\nIt removes any non printing chars used to maintain a specific profession.\r\nUse this when you wish to revert a game save to be compatabile with KerbalStats\r\n or some other mod that creates custom professions.";
                        if (GUILayout.Button(new GUIContent("Reset Professions", toolTip), GUILayout.MaxWidth(120), GUILayout.Height(20)))
                        {
                            ResetKerbalProfessions();
                        }
                    }
                    rect = GUILayoutUtility.GetLastRect();
                    if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                        ToolTip = Utilities.SetActiveTooltip(rect, WindowRoster.Position, GUI.tooltip, ref ToolTipActive, 30, 20 - ScrollViewerPosition.y);
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();
                GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in Roster Window.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        private static void DisplayRosterListViewer()
        {
            try
            {
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Name", GUILayout.Width(140));
                GUILayout.Label("Gender", GUILayout.Width(50));
                GUILayout.Label("Profession", GUILayout.Width(70));
                GUILayout.Label("Skill", GUILayout.Width(30));
                GUILayout.Label("Status", GUILayout.Width(220));
                GUILayout.EndHorizontal();

                ScrollViewerPosition = GUILayout.BeginScrollView(ScrollViewerPosition, SMStyle.ScrollStyle, GUILayout.Height(230), GUILayout.Width(680));
                List<ProtoCrewMember> AllCrew = HighLogic.CurrentGame.CrewRoster.Crew.ToList();
                // Support for DeepFreeze
                if (DFInterface.IsDFInstalled)
                    AllCrew.AddRange(HighLogic.CurrentGame.CrewRoster.Unowned);

                foreach (ProtoCrewMember kerbal in AllCrew)
                {
                    if (CanDisplayKerbal(kerbal))
                    {
                        GUIStyle labelStyle = null;
                        if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Dead || kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Missing)
                            labelStyle = SMStyle.LabelStyleRed;
                        else if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Assigned)
                            labelStyle = SMStyle.LabelStyleYellow;
                        else
                            labelStyle = SMStyle.LabelStyle;

                        // What vessel is this Kerbal Assigned to?
                        string rosterDetails = "";
                        if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Assigned)
                        {
                            foreach (Vessel thisVessel in FlightGlobals.Vessels)
                            {
                                List<ProtoCrewMember> crew = thisVessel.GetVesselCrew();
                                foreach (ProtoCrewMember crewMember in crew)
                                {
                                    if (crewMember == kerbal)
                                    {
                                        rosterDetails = "Assigned - " + thisVessel.GetName().Replace("(unloaded)", "");
                                        break;
                                    }
                                }
                            }
                        }
                        else if (DFInterface.IsDFInstalled && kerbal.type == ProtoCrewMember.KerbalType.Unowned)
                        {
                            // This kerbal could be frozen.  Lets find out...
                            rosterDetails = GetFrozenDetials(kerbal);
                            labelStyle = SMStyle.LabelStyleCyan;
                        }
                        else
                        {
                            // Since the kerbal has no vessel assignment, lets show what their status...
                            rosterDetails = kerbal.rosterStatus.ToString();
                        }
                        string buttonText = string.Empty;
                        string buttonToolTip = string.Empty;
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(kerbal.name, labelStyle, GUILayout.Width(140), GUILayout.Height(20));
                        GUILayout.Label(kerbal.gender.ToString(), labelStyle, GUILayout.Width(50));
                        GUILayout.Label(kerbal.experienceTrait.Title, labelStyle, GUILayout.Width(70));
                        GUILayout.Label(kerbal.experienceLevel.ToString().ToString(), labelStyle, GUILayout.Width(30));
                        GUILayout.Label(rosterDetails, labelStyle, GUILayout.Width(220));

                        if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available && kerbal.type != ProtoCrewMember.KerbalType.Unowned)
                            GUI.enabled = true;
                        else
                            GUI.enabled = false;

                        buttonText = (SelectedKerbal == null || SelectedKerbal.Kerbal != kerbal) ? "Edit" : "Cancel";
                        if (GUI.enabled)
                            buttonToolTip = (SelectedKerbal == null || SelectedKerbal.Kerbal != kerbal) ? "Edit this Kerbal's characteristics" : "Cancel any changes to this Kerbal";
                        else
                            buttonToolTip = "Kerbal is not available at this time.\r\nEditing is disabled";

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
                        Rect rect = GUILayoutUtility.GetLastRect();
                        if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                            ToolTip = Utilities.SetActiveTooltip(rect, WindowRoster.Position, GUI.tooltip, ref ToolTipActive, 30, 20-ScrollViewerPosition.y);

                        if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
                        {
                            if (((SMSettings.RealismMode && SMAddon.smController.IsPreLaunch) || !SMSettings.RealismMode) && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available && SMAddon.smController.SelectedPartsSource.Count > 0 && !SMController.CrewPartIsFull(SMAddon.smController.SelectedPartsSource[0]))
                            {
                                GUI.enabled = true;
                                buttonText = "Add";
                                buttonToolTip = "Adds a kerbal to the Selected Source Part,\r\nin the first available seat.";
                            }
                            else if ((kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Dead && kerbal.type == ProtoCrewMember.KerbalType.Unowned) && SMAddon.FrozenKerbals[kerbal.name].vesselID != FlightGlobals.ActiveVessel.id)
                            {
                                GUI.enabled = false;
                                buttonText = "Thaw";
                                buttonToolTip = "Thaw disabled.  Vessel not active. UnFreeze a Kerbal and Revive them.\r\nWill then become assigned to current vessel.";
                            }
                            else if ((kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Dead && kerbal.type == ProtoCrewMember.KerbalType.Unowned) && SMAddon.FrozenKerbals[kerbal.name].vesselID == FlightGlobals.ActiveVessel.id)
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
                            else if (((SMSettings.RealismMode && SMAddon.smController.IsPreLaunch) || !SMSettings.RealismMode) && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Assigned && FlightGlobals.ActiveVessel.GetVesselCrew().Contains(kerbal))
                            {
                                GUI.enabled = true;
                                buttonText = "Remove";
                                buttonToolTip = "Removes a Kerbal from the active vessel.\r\nWill then become available.";
                            }
                            else if (((SMSettings.RealismMode && SMAddon.smController.IsPreLaunch) || !SMSettings.RealismMode) && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available && SMAddon.smController.SelectedPartsSource.Count == 0)
                            {
                                GUI.enabled = false;
                                buttonText = "Add";
                                buttonToolTip = "Add Disabled.  No source part is selected.\r\nTo add a Kerbal, Select a Source Part with an available seat.";
                            }
                            else if ((SMSettings.RealismMode && !SMAddon.smController.IsPreLaunch) && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available)
                            {
                                GUI.enabled = false;
                                buttonText = "Add";
                                buttonToolTip = "Add Disabled.  Realism Settings are preventing this action.\r\nTo add a Kerbal, Change your realism Settings.";
                            }
                            else
                            {
                                GUI.enabled = false;
                                buttonText = "--";
                                buttonToolTip = "Kerbal is not available (" + kerbal.rosterStatus + ").\r\nCurrent status does not allow any action.";
                            }
                        }

                        if ((kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Dead && kerbal.type != ProtoCrewMember.KerbalType.Unowned) || kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Missing)
                        {
                            GUI.enabled = true;
                            buttonText = "Respawn";
                            buttonToolTip = "Brings a Kerbal back to life.\r\nWill then become available.";
                        }

                        if (GUILayout.Button(new GUIContent(buttonText, buttonToolTip), GUILayout.Width(60), GUILayout.Height(20)))
                        {
                            if (buttonText == "Add")
                                TransferCrew.AddCrewMember(kerbal, SMAddon.smController.SelectedPartsSource[0]);
                            else if (buttonText == "Respawn")
                                SMController.RespawnKerbal(kerbal);
                            else if (buttonText == "Thaw")
                                ThawKerbal(kerbal);
                            else if (buttonText == "Freeze")
                                FreezeKerbal(kerbal);
                            else if (buttonText == "Remove")
                            {
                                // get part...
                                Part part = SMAddon.smController.FindKerbalPart(kerbal);
                                if (part != null)
                                    TransferCrew.RemoveCrewMember(kerbal, part);
                            }
                        }
                        Rect rect2 = GUILayoutUtility.GetLastRect();
                        if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                            ToolTip = Utilities.SetActiveTooltip(rect2, WindowRoster.Position, GUI.tooltip, ref ToolTipActive, 30, 20-ScrollViewerPosition.y);
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

        internal static Dictionary<string, KerbalInfo> GetFrozenKerbals()
        {
            if (DFInterface.IsDFInstalled)
            {
                IDFInterface IDF = DFInterface.GetFrozenKerbals();
                return IDF.FrozenKerbals;
            }
            else
                return new Dictionary<string, KerbalInfo>();
        }

        private static string GetFrozenDetials(ProtoCrewMember kerbal)
        {
            string rosterDetails = "";
            if (SMAddon.FrozenKerbals.ContainsKey(kerbal.name))
                rosterDetails = "Frozen - " + (SMAddon.FrozenKerbals[kerbal.name]).vesselName.Replace("(unloaded)", "");
            else
                rosterDetails = "Frozen";

            return rosterDetails;
        }

        internal static bool IsKerbalReadyToFreeze(ProtoCrewMember kerbal)
        {
            if (DFInterface.IsDFInstalled)
            {
                List<Part> cryofreezers = (from p in SMAddon.smController.Vessel.parts where p.partInfo.name == "cryofreezer" select p).ToList();
                foreach (Part CryoFreezer in cryofreezers)
                {
                    if (CryoFreezer.protoModuleCrew.Contains(kerbal))
                    {
                        return true;
                    }
                }
                return false;
            }
            else
                return false;
        }

        internal static void ThawKerbal(ProtoCrewMember kerbal)
        {
            try
            {
                if (DFInterface.IsDFInstalled)
                {
                    DF.DeepFreeze objFreeze = DF.DeepFreeze.Instance;
                    KerbalInfo iKerbal = SMAddon.FrozenKerbals[kerbal.name];

                    List<Part> cryofreezers = (from p in SMAddon.smController.Vessel.parts where p.partInfo.name == "cryofreezer" select p).ToList();
                    foreach (Part CryoFreezer in cryofreezers)
                    {
                        if (CryoFreezer.flightID == iKerbal.partID)
                        {
                            PartModule deepFreezer = (from PartModule pm in CryoFreezer.Modules where pm.moduleName == "DeepFreezer" select pm).SingleOrDefault();
                            ((DF.DeepFreezer)deepFreezer).beginThawKerbal(kerbal.name);
                            break;
                        }
                    }
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
                    DF.DeepFreeze objFreeze = DF.DeepFreeze.Instance;

                    List<Part> cryofreezers = (from p in SMAddon.smController.Vessel.parts where p.partInfo.name == "cryofreezer" select p).ToList();
                    foreach (Part CryoFreezer in cryofreezers)
                    {
                        if (CryoFreezer.protoModuleCrew.Contains(kerbal))
                        {
                            PartModule deepFreezer = (from PartModule pm in CryoFreezer.Modules where pm.moduleName == "DeepFreezer" select pm).SingleOrDefault();
                            ((DF.DeepFreezer)deepFreezer).beginFreezeKerbal(kerbal);
                            break;
                        }
                    }

                    // this only works in SpaceCenter, and makes them available (removed from vessel)
                    // objFreeze.ThawFrozenCrew(kerbal.name, FlightGlobals.ActiveVessel.id, false);
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in ThawKerbal.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        private static void CreateKerbalViewer()
        {
            DisplaySelectProfession();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Create", GUILayout.MaxWidth(80), GUILayout.Height(20)))
            {
                bool kerbalFound = false;
                while (!kerbalFound)
                {
                    SelectedKerbal = ModKerbal.CreateKerbal();
                    if (SelectedKerbal.Title == KerbalProfession)
                        kerbalFound = true;
                }
                OnCreate = false;
            }
            if (GUILayout.Button("Cancel", GUILayout.MaxWidth(80), GUILayout.Height(20)))
            {
                OnCreate = false;
                SelectedKerbal = null;
            }
            GUILayout.EndHorizontal();
        }

        private static void EditKerbalViewer()
        {
            Rect rect = new Rect();
            string label = "";
            string toolTip = "";
            GUILayout.Label(SelectedKerbal.IsNew ? "Create a Kerbal" : "Edit a Kerbal");
            if (SMSettings.EnableKerbalRename)
            {
                GUILayout.BeginHorizontal();
                SelectedKerbal.Name = GUILayout.TextField(SelectedKerbal.Name, GUILayout.MaxWidth(300));
                GUILayout.Label(" - (" + SelectedKerbal.Kerbal.experienceTrait.Title + ")");
                GUILayout.EndHorizontal();
            }
            else
                GUILayout.Label(SelectedKerbal.Name + " - (" + SelectedKerbal.Title + ")", SMStyle.LabelStyleBold, GUILayout.MaxWidth(300));

            if (!string.IsNullOrEmpty(SMAddon.saveMessage))
            {
                GUILayout.Label(SMAddon.saveMessage, SMStyle.ErrorLabelRedStyle);
            }
            if (SMSettings.EnableKerbalRename && SMSettings.RenameWithProfession)
            {
                DisplaySelectProfession();
            }
            bool isMale = ProtoCrewMember.Gender.Male == SelectedKerbal.Gender ? true : false;
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
            label = "Apply";
            toolTip = "Applies the changes made to this Kerbal.\r\nDesired Name and Profession will be Retained after save.";
            if (GUILayout.Button(new GUIContent(label, toolTip), GUILayout.MaxWidth(50)))
            {
                if (SMSettings.EnableKerbalRename && SMSettings.RenameWithProfession)
                {
                    SelectedKerbal.Title = KerbalProfession;
                }
                SMAddon.saveMessage = SelectedKerbal.SubmitChanges();
                if (string.IsNullOrEmpty(SMAddon.saveMessage))
                    SelectedKerbal = null;
            }
            rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, WindowRoster.Position, GUI.tooltip, ref ToolTipActive, 30, 20);
            GUILayout.EndHorizontal();
        }

        private static void SetProfessionFlag()
        {
            if (SelectedKerbal.Title == "Pilot")
            {
                isPilot = true;
                isEngineer = false;
                isScientist = false;
            }
            else if (SelectedKerbal.Title == "Engineer")
            {
                isPilot = false;
                isEngineer = true;
                isScientist = false;
            }
            else
            {
                isPilot = false;
                isEngineer = false;
                isScientist = true;
            }
        }

        private static void DisplaySelectProfession()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Profession:", GUILayout.Width(85));
            isPilot = GUILayout.Toggle(isPilot, "Pilot", GUILayout.Width(90));
            if (isPilot)
                isEngineer = isScientist = false;
            else
            {
                if (!isEngineer && !isScientist)
                    isPilot = true;
            }
            isEngineer = GUILayout.Toggle(isEngineer, "Engineer", GUILayout.Width(90));
            if (isEngineer)
                isPilot = isScientist = false;
            else
            {
                if (!isPilot && !isScientist)
                    isEngineer = true;
            }
            isScientist = GUILayout.Toggle(isScientist, "Scientist", GUILayout.Width(90));
            if (isScientist)
                isPilot = isEngineer = false;
            else
            {
                if (!isPilot && !isEngineer)
                    isScientist = true;
            }
            GUILayout.EndHorizontal();
        }

        private static void DisplayRosterFilter()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Filter:", GUILayout.Width(40));
            isAll = GUILayout.Toggle(isAll, "All", GUILayout.Width(60));
            if (isAll)
                isVessel = isAvail = isDead = false;
            else
            {
                if (!isVessel && !isAvail && !isDead)
                    isAll = true;
            }
            if (HighLogic.LoadedSceneIsFlight)
            {
                isVessel = GUILayout.Toggle(isVessel, "Vessel", GUILayout.Width(80));
                if (isVessel)
                    isAll = isAvail = isDead = false;
                else
                {
                    if (!isAll && !isAvail && !isDead)
                        isVessel = true;
                }
            }
            isAvail = GUILayout.Toggle(isAvail, "Available", GUILayout.Width(95));
            if (isAvail)
                isAll = isVessel = isDead = false;
            else
            {
                if (!isAll && !isVessel && !isDead)
                    isAvail = true;
            }
            isDead = GUILayout.Toggle(isDead, "Dead/Missing");
            if (isDead)
                isAll = isVessel = isAvail = false;
            else
            {
                if (!isAll && !isVessel && !isAvail)
                    isDead = true;
            }
            GUILayout.EndHorizontal();
        }

        private static bool CanDisplayKerbal(ProtoCrewMember kerbal)
        {
            if (isAll)
                return true;
            else if (isVessel && (FlightGlobals.ActiveVessel.GetVesselCrew().Contains(kerbal) || (DFInterface.IsDFInstalled && GetFrozenDetials(kerbal).Contains(FlightGlobals.ActiveVessel.vesselName.Replace("(unloaded)", "")))))
                return true;
            else if (isAvail && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available && kerbal.type != ProtoCrewMember.KerbalType.Unowned)
                return true;
            else
                return false;
        }

        private static void ResetKerbalProfessions()
        {
            foreach (ProtoCrewMember kerbal in HighLogic.CurrentGame.CrewRoster.Crew)
            {
                if (kerbal.name.Contains(char.ConvertFromUtf32(1)))
                {
                    kerbal.name = kerbal.name.Replace(char.ConvertFromUtf32(1), "");
                    KerbalRoster.SetExperienceTrait(kerbal);
                }
            }
        }
    }
}
