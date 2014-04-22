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

        #region Properties

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

        #endregion

        public ManifestController()
        {
            RenderingManager.AddToPostDrawQueue(3, drawGui);
        }

        private void drawGui()
        {
            try
            {
                if (FlightGlobals.fetch == null)
                { return; }

                if (FlightGlobals.ActiveVessel != Vessel)
                { return; }

                ManifestStyle.SetupGUI();

                // Is the scene one we want to be visible in?
                if (HighLogic.LoadedScene == GameScenes.FLIGHT && !MapView.MapIsEnabled && !PauseMenu.isOpen && !FlightResultsDialog.isDisplaying)
                {
                    if (ShowShipManifest)
                    {
                        // Let's set all highlighting
                        ShipManifestBehaviour.ShipManifestSettings.ManifestPosition = GUILayout.Window(398544, ShipManifestBehaviour.ShipManifestSettings.ManifestPosition, ShipManifestWindow, "Ship's Manifest - " + Vessel.vesselName, GUILayout.MinHeight(20));
                    }

                    // What windows do we want to show?
                    if (ShowShipManifest && ShowTransferWindow)
                    {
                        ShipManifestBehaviour.ShipManifestSettings.TransferPosition = GUILayout.Window(398545, ShipManifestBehaviour.ShipManifestSettings.TransferPosition, TransferWindow, "Transfer - " + Vessel.vesselName + " - " + ShipManifestBehaviour.SelectedResource, GUILayout.MinHeight(20));
                    }
                    if (ShowShipManifest && ShipManifestBehaviour.ShipManifestSettings.ShowSettings)
                    {
                        ShipManifestBehaviour.ShipManifestSettings.SettingsPosition = GUILayout.Window(398546, ShipManifestBehaviour.ShipManifestSettings.SettingsPosition, ShipManifestBehaviour.ShipManifestSettings.SettingsWindow, "Ship Manifest Settings", GUILayout.MinHeight(20));
                    }

                    if (resetRosterSize)
                    {
                        ShipManifestBehaviour.ShipManifestSettings.RosterPosition.height = 100; //reset hight
                        ShipManifestBehaviour.ShipManifestSettings.RosterPosition.width = 360; //reset width
                        resetRosterSize = false;
                    }

                    if (ShowShipManifest && ShipManifestBehaviour.ShipManifestSettings.ShowRoster)
                    {
                        ShipManifestBehaviour.ShipManifestSettings.RosterPosition = GUILayout.Window(398547, ShipManifestBehaviour.ShipManifestSettings.RosterPosition, RosterWindow, "Ship Manifest Roster", GUILayout.MinHeight(20));
                    }
                }
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format(" in drawGui.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        #region Ship Manifest Window Gui Layout Code

        // Ship Manifest Window
        // This window displays options for managing crew, resources, and flight checklists for the focused vessel.
        private Vector2 ScrollViewerShipManifest = Vector2.zero;
        private Vector2 ScrollViewerResourceManifest2 = Vector2.zero;
        private void ShipManifestWindow(int windowId)
        {
            try
            {
                GUILayout.BeginVertical();
                ScrollViewerShipManifest = GUILayout.BeginScrollView(ScrollViewerShipManifest, GUILayout.Height(100), GUILayout.Width(300));
                GUILayout.BeginVertical();

                if (IsPreLaunch)
                {
                    PreLaunchGUI();
                }

                // Now the Resource Buttons
                ResourceButtonList();
                
                GUILayout.EndVertical();
                GUILayout.EndScrollView();

                GUILayout.Label(ShipManifestBehaviour.SelectedResource != null ? string.Format("{0}", ShipManifestBehaviour.SelectedResource) : "No Resource Selected", GUILayout.Width(300), GUILayout.Height(20));

                // Resource Details List Viewer
                ResourceDetailsViewer();

                GUILayout.BeginHorizontal();

                var transferStyle = ShowTransferWindow ? ManifestStyle.ButtonToggledStyle : ManifestStyle.ButtonStyle;
                if (GUILayout.Button("Transfer...", transferStyle, GUILayout.Width(100), GUILayout.Height(20)))
                {
                    try
                    {
                        if (ShipManifestBehaviour.SelectedResource != null)
                        {
                            ShowTransferWindow = !ShowTransferWindow;
                        }
                    }
                    catch (Exception ex)
                    {
                        ManifestUtilities.LogMessage(string.Format(" opening Transfer Window.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                    }
                }

                var settingsStyle = ShipManifestBehaviour.ShipManifestSettings.ShowSettings ? ManifestStyle.ButtonToggledStyle : ManifestStyle.ButtonStyle;
                if (GUILayout.Button("Settings...", settingsStyle, GUILayout.Width(100), GUILayout.Height(20)))
                {
                    try
                    {
                        ShipManifestBehaviour.ShipManifestSettings.ShowSettings = !ShipManifestBehaviour.ShipManifestSettings.ShowSettings;
                    }
                    catch (Exception ex)
                    {
                        ManifestUtilities.LogMessage(string.Format(" opening Settings Window.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                    }
                }

                var rosterStyle = ShipManifestBehaviour.ShipManifestSettings.ShowRoster ? ManifestStyle.ButtonToggledStyle : ManifestStyle.ButtonStyle;
                if (GUILayout.Button("Roster...", rosterStyle, GUILayout.Width(100), GUILayout.Height(20)))
                {
                    try
                    {
                        ShipManifestBehaviour.ShipManifestSettings.ShowRoster = !ShipManifestBehaviour.ShipManifestSettings.ShowRoster;
                    }
                    catch (Exception ex)
                    {
                        ManifestUtilities.LogMessage(string.Format(" opening Roster Window.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                    }
                }

                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format(" in Ship Manifest Window.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
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

                if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.AVAILABLE && IsPreLaunch && ShipManifestBehaviour.SelectedPartSource != null && !PartCrewIsFull(ShipManifestBehaviour.SelectedPartSource))
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
                        AddCrew(ShipManifestBehaviour.SelectedPartSource, kerbal);
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

        #region Manifest Window Gui components

        private void PreLaunchGUI()
        {
            try
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
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format(" in PreLaunchGUI.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        private void ResourceButtonList()
        {
            try
            {
                foreach (string resourceName in ShipManifestBehaviour.PartsByResource.Keys)
                {
                    int width = 265;
                    GUILayout.BeginHorizontal();
                    if ((!ShipManifestBehaviour.ShipManifestSettings.RealismMode || IsPreLaunch) && resourceName != "Crew" && resourceName != "Science")
                        width = 215;

                    var style = ShipManifestBehaviour.SelectedResource == resourceName ? ManifestStyle.ButtonToggledStyle : ManifestStyle.ButtonStyle;
                    if (GUILayout.Button(string.Format("{0}", resourceName), style, GUILayout.Width(width), GUILayout.Height(20)))
                    {
                        try
                        {
                            // Now let's update our lists...
                            ShipManifestBehaviour.SelectedResource = resourceName;
                            ShipManifestBehaviour.SelectedPartSource = ShipManifestBehaviour.SelectedPartTarget = null;
                         }
                        catch (Exception ex)
                        {
                            ManifestUtilities.LogMessage(string.Format("Error selecting Resource.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
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
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format(" in ResourceButtonList.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        private void ResourceDetailsViewer()
        {
            try
            {
                ScrollViewerResourceManifest2 = GUILayout.BeginScrollView(ScrollViewerResourceManifest2, GUILayout.Height(100), GUILayout.Width(300));
                GUILayout.BeginVertical();

                if (ShipManifestBehaviour.SelectedResource != null)
                {
                    foreach (Part part in ShipManifestBehaviour.SelectedResourceParts)
                    {
                        string resourcename = "";
                        if (ShipManifestBehaviour.SelectedResource != "Crew" && ShipManifestBehaviour.SelectedResource != "Science")
                        {
                            resourcename = part.Resources[ShipManifestBehaviour.SelectedResource].info.name;
                            GUILayout.BeginHorizontal();
                            GUILayout.Label(string.Format("{0}, ({1}/{2})", part.partInfo.title, part.Resources[ShipManifestBehaviour.SelectedResource].amount.ToString("######0.####"), part.Resources[ShipManifestBehaviour.SelectedResource].maxAmount.ToString("######0.####")), GUILayout.Width(265));
                            GUILayout.EndHorizontal();
                        }
                        else if (ShipManifestBehaviour.SelectedResource == "Crew")
                        {
                            resourcename = ShipManifestBehaviour.SelectedResource;
                            GUILayout.BeginHorizontal();
                            GUILayout.Label(string.Format("{0}, ({1}/{2})", part.partInfo.title, part.protoModuleCrew.Count.ToString(), part.CrewCapacity.ToString()), GUILayout.Width(265));
                            GUILayout.EndHorizontal();
                        }
                        else if (ShipManifestBehaviour.SelectedResource == "Science")
                        {
                            resourcename = ShipManifestBehaviour.SelectedResource;
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
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format(" in ResourceDetailsViewer.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
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
            ShipManifestBehaviour.FireEventTriggers();
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
            // Add Extraplanetary LaunchPad support.   This is actually the event I was searching for back at the beginning.. yay!
            ShipManifestBehaviour.FireEventTriggers();
        }

        private void FillVesselCrew()
        {
            foreach (var part in ShipManifestBehaviour.PartsByResource["Crew"])
            {
                AddCrew(part.CrewCapacity - part.protoModuleCrew.Count, part);
            }
            ShipManifestBehaviour.FireEventTriggers();
        }

        private void EmptyVesselCrew()
        {
            foreach (var part in ShipManifestBehaviour.PartsByResource["Crew"])
            {
                for (int i = part.protoModuleCrew.Count - 1; i >= 0; i--)
                {
                    RemoveCrew(part.protoModuleCrew[i], part);
                }
                ShipManifestBehaviour.FireEventTriggers();
            }
        }

        private void FillVesselResources()
        {
            List<string> resources = ShipManifestBehaviour.PartsByResource.Keys.ToList<string>();
            foreach (string resourceName in resources)
            {
                if (resourceName != "Crew" && resourceName != "Science")
                {
                    foreach (Part part in ShipManifestBehaviour.PartsByResource[resourceName])
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
            List<string> resources = ShipManifestBehaviour.PartsByResource.Keys.ToList<string>();
            foreach (string resourceName in resources)
            {
                if (resourceName != "Crew" && resourceName != "Science")
                {
                    foreach (Part part in ShipManifestBehaviour.PartsByResource[resourceName])
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
            foreach (Part part in ShipManifestBehaviour.PartsByResource[resourceName])
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

        #endregion
    }
}
