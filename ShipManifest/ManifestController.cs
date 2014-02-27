using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;

namespace ShipManifest
{
    public partial class ManifestController
    {
        #region Singleton stuff

        private static Dictionary<WeakReference<Vessel>, ManifestController> controllers = new Dictionary<WeakReference<Vessel>, ManifestController>();

        public static ManifestController GetInstance(Vessel vessel)
        {
            foreach (var kvp in controllers.ToArray())
            {
                var wr = kvp.Key;
                var v = wr.Target;
                if (v == null)
                {
                    controllers.Remove(wr);
                    RenderingManager.RemoveFromPostDrawQueue(3, kvp.Value.drawGui);
                }
                else if (v == vessel)
                {
                    return controllers[wr];
                }
            }

            var commander = new ManifestController();
            controllers[new WeakReference<Vessel>(vessel)] = commander;
            return commander;
        }

        #endregion

        public ManifestController()
        {
            RenderingManager.AddToPostDrawQueue(3, drawGui);
        }

        public Vessel Vessel
        {
            get { return controllers.Single(p => p.Value == this).Key.Target; }
        }

        public bool IsPreLaunch
        {
            get
            {
                return Vessel.landedAt == "LaunchPad" || Vessel.landedAt == "Runway";
            }
        }

        public bool IsFlightScene
        {
            get { return HighLogic.LoadedScene == GameScenes.FLIGHT; }
        }

        public bool CanDrawButton = false;

        private string saveMessage = string.Empty;

        private bool ShowRosterWindow { get; set; }

        private void drawGui()
        {
            if (FlightGlobals.fetch == null)
            { return; }

            if (FlightGlobals.ActiveVessel != Vessel)
            { return; }

            ManifestStyle.SetupGUI();

            if (HighLogic.LoadedScene == GameScenes.FLIGHT && !MapView.MapIsEnabled && !PauseMenu.isOpen && !FlightResultsDialog.isDisplaying)
            {
                if (ShowResourceManifest)
                {
                    // Let's set all highlighting
                    SetPartHighlights();
                    ShipManifestBehaviour.ShipManifestSettings.ManifestPosition = GUILayout.Window(398544, ShipManifestBehaviour.ShipManifestSettings.ManifestPosition, ShipManifestWindow, "Ship's Manifest - " + Vessel.vesselName, GUILayout.MinHeight(20));
                }
                else
                {
                    // Let's clear all highlighting
                    if (SelectedResourceParts != null)
                    {
                        foreach (Part oldPart in SelectedResourceParts)
                        {
                            ClearHighlight(oldPart);
                        }
                        SelectedResourceParts = null;
                    }
                }

                if (ShowResourceManifest && ShowResourceTransferWindow)
                {
                    ShipManifestBehaviour.ShipManifestSettings.TransferPosition = GUILayout.Window(398545, ShipManifestBehaviour.ShipManifestSettings.TransferPosition, TransferWindow, "Transfer - " + Vessel.vesselName + " - " + SelectedResource, GUILayout.MinHeight(20));
                }
                if (ShowResourceManifest && ShipManifestBehaviour.ShipManifestSettings.ShowSettings)
                {
                    ShipManifestBehaviour.ShipManifestSettings.SettingsPosition = GUILayout.Window(398546, ShipManifestBehaviour.ShipManifestSettings.SettingsPosition, ShipManifestBehaviour.ShipManifestSettings.SettingsWindow, "Ship Manifest Settings", GUILayout.MinHeight(20));
                }

                if (resetRosterSize)
                {
                    ShipManifestBehaviour.ShipManifestSettings.RosterPosition.height = 100; //reset hight
                    ShipManifestBehaviour.ShipManifestSettings.RosterPosition.width = 360; //reset width
                    resetRosterSize = false;
                }

                if (ShowResourceManifest && ShipManifestBehaviour.ShipManifestSettings.ShowRoster)
                {
                    ShipManifestBehaviour.ShipManifestSettings.RosterPosition = GUILayout.Window(398547, ShipManifestBehaviour.ShipManifestSettings.RosterPosition, RosterWindow, "Ship Manifest Roster", GUILayout.MinHeight(20));
                }
            }
        }

        #region GUI code

        // Ship Manifest Window
        // This window displays options for managing crew, resources, and flight checklists for the focused vessel.
        private Vector2 ScrollViewerShipManifest = Vector2.zero;
        private Vector2 ScrollViewerResourceManifest2 = Vector2.zero;
        private void ShipManifestWindow(int windowId)
        {
            GUILayout.BeginVertical();
            ScrollViewerShipManifest = GUILayout.BeginScrollView(ScrollViewerShipManifest, GUILayout.Height(100), GUILayout.Width(300));
            GUILayout.BeginVertical();

            if (IsPreLaunch)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(string.Format("Fill Crew"), ManifestStyle.ButtonStyle, GUILayout.Width(130), GUILayout.Height(20)))
                {
                    FillVesselCrew();
                }
                if (GUILayout.Button(string.Format("Empty Crew"), ManifestStyle.ButtonStyle, GUILayout.Width(130), GUILayout.Height(20)))
                {
                    EmptyVesselCrew();
                }
                GUILayout.EndHorizontal();

                if (ShipManifestBehaviour.ShipManifestSettings.EnablePFResources)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(string.Format("Fill Resources"), ManifestStyle.ButtonStyle, GUILayout.Width(130), GUILayout.Height(20)))
                    {
                        FillVesselResources();
                    }
                    if (GUILayout.Button(string.Format("Empty Resources"), ManifestStyle.ButtonStyle, GUILayout.Width(130), GUILayout.Height(20)))
                    {
                        EmptyVesselResources();
                    }
                    GUILayout.EndHorizontal();
                }
            }

            foreach (string resourceName in PartsByResource.Keys)
            {
                int width = 265;
                var style = resourceName == SelectedResource ? ManifestStyle.ButtonToggledStyle : ManifestStyle.ButtonStyle;
                GUILayout.BeginHorizontal();
                if ((!ShipManifestBehaviour.ShipManifestSettings.RealismMode || IsPreLaunch) && resourceName != "Crew" && resourceName != "Science")
                    width = 215;

                if (GUILayout.Button(string.Format("{0}", resourceName), style, GUILayout.Width(width), GUILayout.Height(20)))
                {
                    ClearHighlight(_selectedPartSource);
                    ClearHighlight(_selectedPartTarget);

                    // Let's clear all highlighting
                    if (SelectedResourceParts != null)
                    {
                        foreach (Part oldPart in SelectedResourceParts)
                        {
                            ClearHighlight(oldPart);
                        }
                    }

                    // Now let's update our lists...
                    _selectedPartSource = _selectedPartTarget = null;
                    SelectedResource = resourceName;
                    SelectedResourceParts = PartsByResource[SelectedResource];

                    // Finally, set highlights on parts with selected resource.
                    if (SelectedResourceParts != null)
                    {
                        foreach (Part newPart in SelectedResourceParts)
                        {
                            SetPartHighlight(newPart, Color.yellow);
                        }
                    }
                }
                if ((!ShipManifestBehaviour.ShipManifestSettings.RealismMode || IsPreLaunch) && resourceName != "Crew" && resourceName != "Science")
                {
                    if (GUILayout.Button(string.Format("{0}", "Dump"), ManifestStyle.ButtonStyle, GUILayout.Width(45), GUILayout.Height(20)))
                    {
                        DumpResource(resourceName);
                    }
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.Label(SelectedResource != null ? string.Format("{0}", SelectedResource) : "No Resource Selected", GUILayout.Width(300), GUILayout.Height(20));

            ScrollViewerResourceManifest2 = GUILayout.BeginScrollView(ScrollViewerResourceManifest2, GUILayout.Height(100), GUILayout.Width(300));
            GUILayout.BeginVertical();

            if (SelectedResource != null)
            {
                SelectedResourceParts = PartsByResource[SelectedResource];
                foreach (Part part in SelectedResourceParts)
                {
                    string resourcename = "";
                    if (SelectedResource != "Crew" && SelectedResource != "Science")
                    {
                        resourcename = part.Resources[SelectedResource].info.name;
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(string.Format("{0}, ({1}/{2})", part.partInfo.title, part.Resources[SelectedResource].amount.ToString("######0.####"), part.Resources[SelectedResource].maxAmount.ToString("######0.####")), GUILayout.Width(265));
                        GUILayout.EndHorizontal();
                    }
                    else if (SelectedResource == "Crew")
                    {
                        resourcename = SelectedResource;
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(string.Format("{0}, ({1}/{2})", part.partInfo.title, part.protoModuleCrew.Count.ToString(), part.CrewCapacity.ToString()), GUILayout.Width(265));
                        GUILayout.EndHorizontal();
                    }
                    else if (SelectedResource == "Science")
                    {
                        resourcename = SelectedResource;
                        int ScienceCount = 0;
                        foreach (PartModule pm in part.Modules)
                        {
                            if (pm is ModuleScienceContainer)
                                ScienceCount += ((ModuleScienceContainer)pm).GetScienceCount();
                            else if (pm is ModuleScienceExperiment)
                                ScienceCount += ((ModuleScienceExperiment)pm).GetScienceCount();
                        }
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(string.Format("{0}, ({1})", part.partInfo.title, ScienceCount.ToString()), GUILayout.Width(265));
                        GUILayout.EndHorizontal();
                    }
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();

            var transferStyle = ShowResourceTransferWindow ? ManifestStyle.ButtonToggledStyle : ManifestStyle.ButtonStyle;
            if (GUILayout.Button("Transfer...", transferStyle, GUILayout.Width(100), GUILayout.Height(20)))
            {
                if (SelectedResource != null)
                {
                    ShowResourceTransferWindow = !ShowResourceTransferWindow;
                }
            }

            var settingsStyle = ShipManifestBehaviour.ShipManifestSettings.ShowSettings ? ManifestStyle.ButtonToggledStyle : ManifestStyle.ButtonStyle;
            if (GUILayout.Button("Settings...", settingsStyle, GUILayout.Width(100), GUILayout.Height(20)))
            {
                ShipManifestBehaviour.ShipManifestSettings.ShowSettings = !ShipManifestBehaviour.ShipManifestSettings.ShowSettings;
            }

            var rosterStyle = ShipManifestBehaviour.ShipManifestSettings.ShowRoster ? ManifestStyle.ButtonToggledStyle : ManifestStyle.ButtonStyle;
            if (GUILayout.Button("Roster...", rosterStyle, GUILayout.Width(100), GUILayout.Height(20)))
            {
                ShipManifestBehaviour.ShipManifestSettings.ShowRoster = !ShipManifestBehaviour.ShipManifestSettings.ShowRoster;
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
        }

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
            GUIStyle style = GUI.skin.button;
            var defaultColor = style.normal.textColor;
            GUILayout.BeginVertical();

            rosterScrollViewer = GUILayout.BeginScrollView(rosterScrollViewer, GUILayout.Height(200), GUILayout.Width(360));
            GUILayout.BeginVertical();

            foreach (ProtoCrewMember kerbal in HighLogic.CurrentGame.CrewRoster)
            {
                GUIStyle labelStyle = null;
                if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.DEAD || kerbal.rosterStatus == ProtoCrewMember.RosterStatus.MISSING)
                    labelStyle = ManifestStyle.LabelStyleRed;
                else if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.ASSIGNED)
                    labelStyle = ManifestStyle.LabelStyleYellow;
                else
                    labelStyle = ManifestStyle.LabelStyle;

                GUILayout.BeginHorizontal();
                GUILayout.Label(kerbal.name, labelStyle, GUILayout.Width(200));  // + "  (" + kerbal.seat.vessel.name + ")"
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

                if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.AVAILABLE && IsPreLaunch && SelectedPartSource != null && !PartCrewIsFull(SelectedPartSource))
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
                        AddCrew(SelectedPartSource, kerbal);
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

        #endregion

        public void HideAllWindows()
        {
            ShowResourceTransferWindow = false;
            ShowRosterWindow = false;
            ClearHighlight(_selectedPartSource);
            ClearHighlight(_selectedPartTarget);
            _selectedPartSource = _selectedPartTarget = null; //clear selections
        }

        public void LoadChecklist()
        {
            string filepath = Vessel.id.ToString();
            // Get Checklist for this vessel from file
            StreamReader myChecklist = System.IO.File.OpenText(filepath);


            // Parse into public dictionary

            // Done
        }

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

        public void RespawnCrew()
        {
            this.Vessel.SpawnCrew();
        }

        private void FillVesselCrew()
        {
            foreach (var part in PartsByResource["Crew"])
            {
                AddCrew(part.CrewCapacity - part.protoModuleCrew.Count, part);
            }
        }

        private void EmptyVesselCrew()
        {
            foreach (var part in PartsByResource["Crew"])
            {
                for (int i = part.protoModuleCrew.Count - 1; i >= 0; i--)
                {
                    RemoveCrew(part.protoModuleCrew[i], part);
                }
            }
        }

        private void FillVesselResources()
        {
            List<string> resources = PartsByResource.Keys.ToList<string>();
            foreach (string resourceName in resources)
            {
                if (resourceName != "Crew" && resourceName != "Science")
                {
                    foreach (Part part in PartsByResource[resourceName])
                    {
                        foreach (PartResource resource in part.Resources)
                        {
                            if (resource.info.name == resourceName)
                                resource.amount = resource.maxAmount;
                        }
                    }
                }
            }
        }

        private void EmptyVesselResources()
        {
            List<string> resources = PartsByResource.Keys.ToList<string>();
            foreach (string resourceName in resources)
            {
                if (resourceName != "Crew" && resourceName != "Science")
                {
                    foreach (Part part in PartsByResource[resourceName])
                    {
                        foreach (PartResource resource in part.Resources)
                        {
                            if (resource.info.name == resourceName)
                                resource.amount = 0;
                        }
                    }
                }
            }
        }

        private void DumpResource(string resourceName)
        {
            foreach (Part part in PartsByResource[resourceName])
            {
                foreach (PartResource resource in part.Resources)
                {
                    if (resource.info.name == resourceName)
                    {
                        resource.amount = 0;
                    }
                }
            }
        }
    }
}
