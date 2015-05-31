using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using ConnectedLivingSpace;

namespace ShipManifest
{
    internal static class WindowRoster
    {
        internal static Rect Position = new Rect(0, 0, 400, 300);
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
                GUIStyle style = GUI.skin.button;
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
                    if (GUILayout.Button("Create Kerbal", GUILayout.MaxWidth(120)))
                    {
                        OnCreate = true;
                    }
                    if (SMSettings.RenameWithProfession)
                    {
                        string toolTip = "This action resets all renamed Kerbals to their KSP default professions.\r\nIt removes any non printing chars used to maintain a specific profession.\r\nUse this when you wish to revert a game save to be compatabile with KerbalStats\r\n or some other mod that creates custom professions.";
                        if (GUILayout.Button(new GUIContent("Reset Professions", toolTip), GUILayout.MaxWidth(120)))
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
                ScrollViewerPosition = GUILayout.BeginScrollView(ScrollViewerPosition, GUILayout.Height(200), GUILayout.Width(400));
                GUILayout.BeginVertical();

                // Support for DeepFreeze
                List<ProtoCrewMember> AllCrew = HighLogic.CurrentGame.CrewRoster.Crew.ToList();
                if (InstalledMods.IsDFInstalled)
                    AllCrew.AddRange(HighLogic.CurrentGame.CrewRoster.Unowned);
                Utilities.LogMessage(string.Format("IsDFInstalled:  {0}", InstalledMods.IsDFInstalled.ToString()), "Info", true);

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
                                        rosterDetails = "\r\n  -  " + thisVessel.GetName().Replace("(unloaded)", "");
                                        break;
                                    }
                                }
                            }
                        }
                        else if (InstalledMods.IsDFInstalled && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available && kerbal.type == ProtoCrewMember.KerbalType.Unowned)
                        {
                            Utilities.LogMessage("Kerbal may be frozen.  Kerbal:  " + kerbal.name, "Info", true);
                            // This kerbal could be frozen.  Lets find out...
                            rosterDetails = GetProtoFrozenDetials(kerbal);
                            labelStyle = SMStyle.LabelStyleCyan;
                        }
                        else
                        {
                            // Since the kerbal has no vessel assignment, lets show what their status is instead...
                            rosterDetails = "\r\n  -  " + kerbal.rosterStatus;
                        }
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(string.Format("{0}{1}", kerbal.name + ", (" + kerbal.experienceTrait.Title + "/" + kerbal.experience.ToString() + ")", rosterDetails), labelStyle, GUILayout.Width(230), GUILayout.Height(10));  // + "  (" + kerbal.seat.vessel.name + ")"
                        string buttonText = string.Empty;
                        string buttonToolTip = string.Empty;

                        if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available && kerbal.type != ProtoCrewMember.KerbalType.Unowned)
                            GUI.enabled = true;
                        else
                            GUI.enabled = false;

                        buttonText = (SelectedKerbal == null || SelectedKerbal.Kerbal != kerbal) ? "Edit" : "Cancel";
                        if (GUI.enabled)
                            buttonToolTip = (SelectedKerbal == null || SelectedKerbal.Kerbal != kerbal) ? "Edit this Kerbal's characteristics" : "Cancel any changes to this Kerbal";
                        else
                            buttonToolTip = "Kerbal is not available at this time.\r\nEditing is disabled";

                        if (GUILayout.Button(new GUIContent(buttonText, buttonToolTip), GUILayout.Width(60)))
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

                        if (HighLogic.LoadedScene != GameScenes.SPACECENTER && ((SMSettings.RealismMode && SMAddon.smController.IsPreLaunch) || !SMSettings.RealismMode) && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available && SMAddon.smController.SelectedPartsSource.Count > 0 && !SMController.CrewPartIsFull(SMAddon.smController.SelectedPartsSource[0]))
                        {
                            GUI.enabled = true;
                            buttonText = "Add";
                            buttonToolTip = "Adds a kerbal to the Selected Source Part,\r\nin the first available seat.";
                        }
                        else if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Dead || kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Missing)
                        {
                            GUI.enabled = true;
                            buttonText = "Respawn";
                            buttonToolTip = "Brings a Kerbal back to life.\r\nWill then become available.";
                        }
                        else if (HighLogic.LoadedScene != GameScenes.SPACECENTER && ((SMSettings.RealismMode && SMAddon.smController.IsPreLaunch) || !SMSettings.RealismMode) && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Assigned && FlightGlobals.ActiveVessel.GetVesselCrew().Contains(kerbal))
                        {
                            GUI.enabled = true;
                            buttonText = "Remove";
                            buttonToolTip = "Removes a Kerbal from the active vessel.\r\nWill then become available.";
                        }
                        else if (HighLogic.LoadedScene != GameScenes.SPACECENTER && ((SMSettings.RealismMode && SMAddon.smController.IsPreLaunch) || !SMSettings.RealismMode) && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available && SMAddon.smController.SelectedPartsSource.Count == 0)
                        {
                            GUI.enabled = false;
                            buttonText = "Add";
                            buttonToolTip = "Add Disabled.  No source part is selected.\r\nTo add a Kerbal, Select a Source Part with an available seat.";
                        }
                        else if (HighLogic.LoadedScene != GameScenes.SPACECENTER && (SMSettings.RealismMode && !SMAddon.smController.IsPreLaunch) && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available)
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

                        if (GUILayout.Button(new GUIContent(buttonText,buttonToolTip), GUILayout.Width(60)))
                        {
                            if (buttonText == "Add")
                                CrewTransfer.AddCrewMember(kerbal, SMAddon.smController.SelectedPartsSource[0]);
                            else if (buttonText == "Respawn")
                                SMController.RespawnKerbal(kerbal);
                            else if (buttonText == "Remove")
                            {
                                // get part...
                                Part part = SMAddon.smController.FindKerbalPart(kerbal);
                                if (part != null)
                                    CrewTransfer.RemoveCrewMember(kerbal, part);
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

        private static string GetFrozenDetials(ProtoCrewMember kerbal)
        {
            string rosterDetails = "";
            bool _found = false;
            foreach (Vessel thisVessel in FlightGlobals.Vessels)
            {
                List<Part> cryoParts = (from p in thisVessel.parts where p.name.Contains("cryofreezer") select p).ToList();
                foreach (Part pPart in cryoParts)
                {
                    List<PartModule> cryoModules = (from PartModule m in pPart.Modules where m.moduleName.Contains("DeepFreezer") select m).ToList();
                    foreach (PartModule pMmodule in cryoModules)
                    {
                        foreach (BaseEvent thisEvent in pMmodule.Events)
                        {
                            if (thisEvent.guiName.Contains(kerbal.name))
                            {
                                _found = true;
                                rosterDetails = "\r\n- Frozen: " + thisVessel.GetName().Replace("(unloaded)", "");
                                break;
                            }
                        }
                        if (_found) break;
                    }
                    if (_found) break;
                }
            }
            if (!_found)
            {
                rosterDetails = "\r\n - Frozen";
            }
            return rosterDetails;
        }

        private static string GetProtoFrozenDetials(ProtoCrewMember kerbal)
        {
            string rosterDetails = "";
            bool _found = false;
            foreach (Vessel thisVessel in FlightGlobals.Vessels)
            {
                List<ProtoPartSnapshot> cryoParts = (from p in thisVessel.protoVessel.protoPartSnapshots where p.partName.Contains("cryofreezer") select p).ToList();
                foreach (ProtoPartSnapshot pPart in cryoParts)
                {
                    List<ProtoPartModuleSnapshot> cryoModules = (from ProtoPartModuleSnapshot m in pPart.modules where m.moduleName.Contains("DeepFreezer") select m).ToList();
                    foreach (ProtoPartModuleSnapshot pMmodule in cryoModules)
                    {
                        ConfigNode cryoNode = pMmodule.moduleValues;
                        {
                            if (cryoNode.HasValue("FrozenCrew"))
                            {
                                string FrozenCrew = cryoNode.GetValue("FrozenCrew");
                                if (FrozenCrew.Contains(kerbal.name))
                                {
                                    _found = true;
                                    rosterDetails = "\r\n- Frozen: " + thisVessel.GetName().Replace("(unloaded)", "");
                                    break;
                                }
                            }
                        }
                    }
                    if (_found) break;
                }
            }
            if (!_found)
            {
                rosterDetails = "Frozen";
            }
            return rosterDetails;
        }

        private static void CreateKerbalViewer()
        {
            DisplaySelectProfession();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Create", GUILayout.MaxWidth(80)))
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
            if (GUILayout.Button("Cancel", GUILayout.MaxWidth(80)))
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
            isDead = GUILayout.Toggle(isDead, "Dead/Missing", GUILayout.Width(80));
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
            else if (isVessel && (FlightGlobals.ActiveVessel.GetVesselCrew().Contains(kerbal) || (InstalledMods.IsDFInstalled && GetFrozenDetials(kerbal).Contains("Frozen -"))))
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
