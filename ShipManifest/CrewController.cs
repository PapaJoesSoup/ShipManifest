using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ShipManifest
{
    public partial class ManifestController
    {
        // This module contains crew specific code.  made it a partial class for readability
        // and management by coders.
        #region GUI Stuff

        private bool resetRosterSize = true;
        private bool _showTransferWindow { get; set; }
        private bool _showRosterWindow { get; set; }
        public bool ShowCrewManifest { get; set; }

        private List<Part> _crewableParts;
        private List<Part> CrewableParts
        {
            get
            {
                if (_crewableParts == null)
                    _crewableParts = new List<Part>();
                else
                    _crewableParts.Clear();

                foreach (Part part in Vessel.Parts)
                {
                    if (part.CrewCapacity > 0)
                        _crewableParts.Add(part);
                }

                return _crewableParts;
            }
        }

        private List<Part> _crewablePartsSource;
        private List<Part> CrewablePartsSource
        {
            get
            {
                if (_crewablePartsSource == null)
                    _crewablePartsSource = new List<Part>();
                else
                    _crewablePartsSource.Clear();

                foreach (Part part in Vessel.Parts)
                {
                    if (part.CrewCapacity > 0)
                        _crewablePartsSource.Add(part);
                }

                return _crewablePartsSource;
            }
        }

        private List<Part> _crewablePartsTarget;
        private List<Part> CrewablePartsTarget
        {
            get
            {
                if (_crewablePartsTarget == null)
                    _crewablePartsTarget = new List<Part>();
                else
                    _crewablePartsTarget.Clear();

                foreach (Part part in Vessel.Parts)
                {
                    if (part.CrewCapacity > 0 && !part.Equals(SelectedPartSource))
                        _crewablePartsTarget.Add(part);
                }

                return _crewablePartsTarget;
            }
        }

        private Vector2 partScrollViewer = Vector2.zero;
        private Vector2 partScrollViewer2 = Vector2.zero;
        private void CrewManifestWindow(int windowId)
        {
            GUILayout.BeginVertical();

            partScrollViewer = GUILayout.BeginScrollView(partScrollViewer, GUILayout.Height(200), GUILayout.Width(300));
            GUILayout.BeginVertical();

            if (IsPreLaunch)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(string.Format("Fill Vessel"), GUILayout.Width(130)))
                {
                    FillVessel();
                }
                if (GUILayout.Button(string.Format("Empty Vessel"), GUILayout.Width(130)))
                {
                    EmptyVessel();
                }
                GUILayout.EndHorizontal();
            }

            foreach (Part part in CrewableParts)
            {
                var style = part == SelectedPart ? Resources.ButtonToggledStyle : Resources.ButtonStyle;

                if (GUILayout.Button(string.Format("{0} {1}/{2}", part.partInfo.title, part.protoModuleCrew.Count, part.CrewCapacity), style, GUILayout.Width(265)))
                {
                    SelectedPart = part;
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.Label(SelectedPart != null ? string.Format("{0} {1}/{2}", SelectedPart.partInfo.title, SelectedPart.protoModuleCrew.Count, SelectedPart.CrewCapacity) : "No Part Selected", GUILayout.Width(300));

            partScrollViewer2 = GUILayout.BeginScrollView(partScrollViewer2, GUILayout.Height(200), GUILayout.Width(300));
            GUILayout.BeginVertical();

            if (SelectedPart != null)
            {
                if (!PartCrewIsFull(SelectedPart) && IsPreLaunch)
                {
                    if (GUILayout.Button(string.Format("Add a Kerbal"), GUILayout.Width(275)))
                    {
                        AddCrew(1, SelectedPart);
                    }
                }

                for (int i = 0; i < SelectedPart.protoModuleCrew.Count; i++)
                {
                    ProtoCrewMember kerbal = SelectedPart.protoModuleCrew[i];
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(kerbal.name, GUILayout.Width(200));
                    if (IsPreLaunch)
                    {
                        if (GUILayout.Button("Remove", GUILayout.Width(60)))
                        {
                            RemoveCrew(kerbal, SelectedPart);
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();

            var crewButtonStyle = _showRosterWindow ? Resources.ButtonToggledStyle : Resources.ButtonStyle;
            var transferStyle = _showTransferWindow ? Resources.ButtonToggledStyle : Resources.ButtonStyle;

            if (GUILayout.Button("Crew Roster", crewButtonStyle, GUILayout.Width(150)))
            {
                _showRosterWindow = !_showRosterWindow;
            }

            if (GUILayout.Button("Transfer Crew", transferStyle, GUILayout.Width(150)))
            {
                _showTransferWindow = !_showTransferWindow;
                if (!_showTransferWindow)
                {
                    ClearHighlight(_selectedPartSource);
                    ClearHighlight(_selectedPartTarget);
                    _selectedPartSource = _selectedPartTarget = null;
                }

            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
        }

        private Vector2 partSourceScrollViewer = Vector2.zero;
        private Vector2 partSourceScrollViewer2 = Vector2.zero;
        private Vector2 partTargetScrollViewer = Vector2.zero;
        private Vector2 partTargetScrollViewer2 = Vector2.zero;
        private void CrewTransferWindow(int windowId)
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();

            partSourceScrollViewer = GUILayout.BeginScrollView(partSourceScrollViewer, GUILayout.Width(300), GUILayout.Height(200));
            GUILayout.BeginVertical();

            foreach (Part part in CrewablePartsSource)
            {
                var style = part == SelectedPartSource ? Resources.ButtonToggledStyle : Resources.ButtonStyle;

                if (GUILayout.Button(string.Format("{0} {1}/{2}", part.partInfo.title, part.protoModuleCrew.Count, part.CrewCapacity), style, GUILayout.Width(265)))
                {
                    SelectedPartSource = part;
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.Label(SelectedPartSource != null ? string.Format("{0} {1}/{2}", SelectedPartSource.partInfo.title, SelectedPartSource.protoModuleCrew.Count, SelectedPartSource.CrewCapacity) : "No Part Selected", GUILayout.Width(300));

            partSourceScrollViewer2 = GUILayout.BeginScrollView(partSourceScrollViewer2, GUILayout.Height(200), GUILayout.Width(300));
            GUILayout.BeginVertical();

            if (SelectedPartSource != null)
            {
                for (int i = 0; i < SelectedPartSource.protoModuleCrew.Count; i++)
                {
                    ProtoCrewMember kerbal = SelectedPartSource.protoModuleCrew[i];
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(kerbal.name, GUILayout.Width(200));
                    if (SelectedPartTarget != null && SelectedPartTarget.protoModuleCrew.Count < SelectedPartTarget.CrewCapacity)
                    {
                        if (GUILayout.Button("Out", GUILayout.Width(60)))
                        {
                            MoveKerbal(SelectedPartSource, SelectedPartTarget, kerbal);
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.EndVertical();
            GUILayout.BeginVertical();

            partTargetScrollViewer = GUILayout.BeginScrollView(partTargetScrollViewer, GUILayout.Height(200), GUILayout.Width(300));
            GUILayout.BeginVertical();

            foreach (Part part in CrewablePartsTarget)
            {
                var style = part == SelectedPartTarget ? Resources.ButtonToggledRedStyle : Resources.ButtonStyle;

                if (GUILayout.Button(string.Format("{0} {1}/{2}", part.partInfo.title, part.protoModuleCrew.Count, part.CrewCapacity), style, GUILayout.Width(265)))
                {
                    SelectedPartTarget = part;
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.Label(SelectedPartTarget != null ? string.Format("{0} {1}/{2}", SelectedPartTarget.partInfo.title, SelectedPartTarget.protoModuleCrew.Count, SelectedPartTarget.CrewCapacity) : "No Part Selected", GUILayout.Width(300));

            partTargetScrollViewer2 = GUILayout.BeginScrollView(partTargetScrollViewer2, GUILayout.Height(200), GUILayout.Width(300));
            GUILayout.BeginVertical();

            if (SelectedPartTarget != null)
            {
                for (int i = 0; i < SelectedPartTarget.protoModuleCrew.Count; i++)
                {
                    ProtoCrewMember kerbal = SelectedPartTarget.protoModuleCrew[i];
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(kerbal.name, GUILayout.Width(200));
                    if (SelectedPartSource != null && SelectedPartSource.protoModuleCrew.Count < SelectedPartSource.CrewCapacity)
                    {
                        if (GUILayout.Button("Out", GUILayout.Width(60)))
                        {
                            MoveKerbal(SelectedPartTarget, SelectedPartSource, kerbal);
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
        }

        private KerbalModel _selectedKerbal;
        private KerbalModel SelectedKerbal
        {
            get { return _selectedKerbal; }
            set
            {
                _selectedKerbal = value;
                if (_selectedKerbal == null)
                {
                    saveMessage = string.Empty;
                    resetRosterSize = true;
                }
            }
        }
        private Vector2 rosterScrollViewer = Vector2.zero;
        private void RosterWindow(int windowId)
        {
            GUIStyle style = GUI.skin.button;
            var defaultColor = style.normal.textColor;
            GUILayout.BeginVertical();

            rosterScrollViewer = GUILayout.BeginScrollView(rosterScrollViewer, GUILayout.Height(200), GUILayout.Width(300));
            GUILayout.BeginVertical();

            foreach (ProtoCrewMember kerbal in HighLogic.CurrentGame.CrewRoster)
            {
                GUIStyle labelStyle = null;
                if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.DEAD || kerbal.rosterStatus == ProtoCrewMember.RosterStatus.MISSING)
                    labelStyle = Resources.LabelStyleRed;
                else if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.ASSIGNED)
                    labelStyle = Resources.LabelStyleYellow;
                else
                    labelStyle = Resources.LabelStyle;

                GUILayout.BeginHorizontal();
                GUILayout.Label(kerbal.name, labelStyle, GUILayout.Width(140));
                string buttonText = string.Empty;

                if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.AVAILABLE)
                    GUI.enabled = true;
                else
                    GUI.enabled = false;

                if (GUILayout.Button((SelectedKerbal == null || SelectedKerbal.Kerbal != kerbal) ? "Edit" : "Cancel", GUILayout.Width(60)))
                {
                    if (SelectedKerbal == null || SelectedKerbal.Kerbal != kerbal)
                    {
                        SelectedKerbal = new KerbalModel(kerbal, false);
                    }
                    else
                    {
                        SelectedKerbal = null;
                    }
                }

                if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.AVAILABLE && IsPreLaunch && SelectedPart != null && !PartCrewIsFull(SelectedPart))
                {
                    GUI.enabled = true;
                    buttonText = "Add";
                }
                else if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.DEAD || kerbal.rosterStatus == ProtoCrewMember.RosterStatus.MISSING)
                {
                    GUI.enabled = true;
                    buttonText = "Respawn";
                }
                else
                {
                    GUI.enabled = false;
                    buttonText = "--";
                }

                if (GUILayout.Button(buttonText, GUILayout.Width(60)))
                {
                    if (buttonText == "Add")
                        AddCrew(SelectedPart, kerbal);
                    else if (buttonText == "Respawn")
                        RespawnKerbal(kerbal);
                }
                GUILayout.EndHorizontal();
                GUI.enabled = true;
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            if (SelectedKerbal != null)
            {
                GUILayout.Label(SelectedKerbal.IsNew ? "Create a Kerbal" : "Edit a Kerbal");
                SelectedKerbal.Name = GUILayout.TextField(SelectedKerbal.Name);

                if (!string.IsNullOrEmpty(saveMessage))
                {
                    GUILayout.Label(saveMessage, Resources.ErrorLabelRedStyle);
                }

                GUILayout.Label("Courage");
                SelectedKerbal.Courage = GUILayout.HorizontalSlider(SelectedKerbal.Courage, 0, 1);

                GUILayout.Label("Stupidity");
                SelectedKerbal.Stupidity = GUILayout.HorizontalSlider(SelectedKerbal.Stupidity, 0, 1);

                SelectedKerbal.Badass = GUILayout.Toggle(SelectedKerbal.Badass, "Badass");

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Cancel", GUILayout.MaxWidth(50)))
                {
                    SelectedKerbal = null;
                }
                if (GUILayout.Button("Apply", GUILayout.MaxWidth(50)))
                {
                    saveMessage = SelectedKerbal.SubmitChanges();
                    if (string.IsNullOrEmpty(saveMessage))
                        SelectedKerbal = null;
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                if (GUILayout.Button("Create Kerbal", GUILayout.MaxWidth(120)))
                {
                    SelectedKerbal = CreateKerbal();
                }
            }

            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
        }

        #endregion

        #region Methods

        public void AddCrew(int count, Part part)
        {
            if (IsPreLaunch && !PartCrewIsFull(part))
            {
                for (int i = 0; i < part.CrewCapacity && i < count; i++)
                {
                    ProtoCrewMember kerbal = HighLogic.CurrentGame.CrewRoster.GetNextOrNewCrewMember();
                    part.AddCrewmember(kerbal);

                    if (kerbal.seat != null)
                        kerbal.seat.SpawnCrew();
                }
            }
        }

        public void AddCrew(Part part, ProtoCrewMember kerbal)
        {
            part.AddCrewmember(kerbal);
            kerbal.rosterStatus = ProtoCrewMember.RosterStatus.ASSIGNED;
            if (kerbal.seat != null)
                kerbal.seat.SpawnCrew();
        }

        private bool PartCrewIsFull(Part part)
        {
            return !(part.protoModuleCrew.Count < part.CrewCapacity);
        }

        private void RemoveCrew(ProtoCrewMember member, Part part)
        {
            part.RemoveCrewmember(member);
            member.rosterStatus = ProtoCrewMember.RosterStatus.AVAILABLE;
        }

        private void MoveKerbal(Part source, Part target, ProtoCrewMember kerbal)
        {
            RemoveCrew(kerbal, source);
            target.AddCrewmember(kerbal);
            if (kerbal.seat != null)
                kerbal.seat.SpawnCrew();
            kerbal.rosterStatus = ProtoCrewMember.RosterStatus.ASSIGNED;
        }

        private void FillVessel()
        {
            foreach (var part in CrewableParts)
            {
                AddCrew(part.CrewCapacity - part.protoModuleCrew.Count, part);
            }
        }

        private void EmptyVessel()
        {
            foreach (var part in CrewableParts)
            {
                for (int i = part.protoModuleCrew.Count - 1; i >= 0; i--)
                {
                    RemoveCrew(part.protoModuleCrew[i], part);
                }
            }
        }

        private void RespawnKerbal(ProtoCrewMember kerbal)
        {
            kerbal.SetTimeForRespawn(0);
            kerbal.Spawn();
            kerbal.rosterStatus = ProtoCrewMember.RosterStatus.AVAILABLE;
            HighLogic.CurrentGame.CrewRoster.GetNextAvailableCrewMember();
        }

        private KerbalModel CreateKerbal()
        {
            ProtoCrewMember kerbal = CrewGenerator.RandomCrewMemberPrototype();
            return new KerbalModel(kerbal, true);
        }

        private void RespawnCrew()
        {
            this.Vessel.SpawnCrew();
        }
        #endregion

    }
}
