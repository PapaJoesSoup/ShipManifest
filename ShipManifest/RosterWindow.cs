using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using ConnectedLivingSpace;

namespace ShipManifest
{
    public partial class ManifestController
    {

        private bool resetRosterSize = true;
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
            try
            {
                GUIStyle style = GUI.skin.button;
                var defaultColor = style.normal.textColor;
                GUILayout.BeginVertical();

                RosterListViewer();

                if (SelectedKerbal != null)
                {
                    GUILayout.Label(SelectedKerbal.IsNew ? "Create a Kerbal" : "Edit a Kerbal");
                    SelectedKerbal.Name = GUILayout.TextField(SelectedKerbal.Name);

                    if (!string.IsNullOrEmpty(saveMessage))
                    {
                        GUILayout.Label(saveMessage, ManifestStyle.ErrorLabelRedStyle);
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
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format(" in Roster Window.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        private void RosterListViewer()
        {
            try
            {
                rosterScrollViewer = GUILayout.BeginScrollView(rosterScrollViewer, GUILayout.Height(200), GUILayout.Width(400));
                GUILayout.BeginVertical();

                foreach (ProtoCrewMember kerbal in HighLogic.CurrentGame.CrewRoster.Crew)
                {
                    GUIStyle labelStyle = null;
                    if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Dead || kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Missing)
                        labelStyle = ManifestStyle.LabelStyleRed;
                    else if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Assigned)
                        labelStyle = ManifestStyle.LabelStyleYellow;
                    else
                        labelStyle = ManifestStyle.LabelStyle;

                    // What vessel is this Kerbal Assigned to?
                    string vesselName = "";
                    if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Assigned)
                    {
                        foreach (Vessel thisVessel in FlightGlobals.Vessels)
                        {
                            List<ProtoCrewMember> crew = thisVessel.GetVesselCrew();
                            foreach (ProtoCrewMember crewMember in crew)
                            {
                                if (crewMember == kerbal)
                                {
                                    vesselName = "\r\n  -  " + thisVessel.name.Replace("(unloaded)", "");
                                    break;
                                }
                            }
                        }
                    }
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(string.Format("{0}{1}", kerbal.name, vesselName), labelStyle, GUILayout.Width(230), GUILayout.Height(10));  // + "  (" + kerbal.seat.vessel.name + ")"
                    string buttonText = string.Empty;

                    if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available)
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

                    if (((ShipManifestBehaviour.ShipManifestSettings.RealismMode && IsPreLaunch) || !ShipManifestBehaviour.ShipManifestSettings.RealismMode) && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available && SelectedPartSource != null && !PartCrewIsFull(SelectedPartSource))
                    {
                        GUI.enabled = true;
                        buttonText = "Add";
                    }
                    else if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Dead || kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Missing)
                    {
                        GUI.enabled = true;
                        buttonText = "Respawn";
                    }
                    else if (((ShipManifestBehaviour.ShipManifestSettings.RealismMode && IsPreLaunch) || !ShipManifestBehaviour.ShipManifestSettings.RealismMode) && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Assigned && FlightGlobals.ActiveVessel.GetVesselCrew().Contains(kerbal))
                    {
                        GUI.enabled = true;
                        buttonText = "Remove";
                    }
                    else
                    {
                        GUI.enabled = false;
                        buttonText = "--";
                    }

                    if (GUILayout.Button(buttonText, GUILayout.Width(60)))
                    {
                        if (buttonText == "Add")
                            AddCrew(kerbal, SelectedPartSource);
                        else if (buttonText == "Respawn")
                            RespawnKerbal(kerbal);
                        else if (buttonText == "Remove")
                        {
                            // get part...
                            Part part = FindPart(kerbal);
                            if (part != null)
                                RemoveCrew(kerbal, part);
                        }
                    }
                    GUILayout.EndHorizontal();
                    GUI.enabled = true;
                }

                GUILayout.EndVertical();
                GUILayout.EndScrollView();

            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format(" in RosterListViewer.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }
    }
}
