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
        private bool showRosterWindow = false;
        private bool ShowRosterWindow 
        {
            get
            {
                return showRosterWindow;
            }
            
            set
            {
                showRosterWindow = value;
            } 
        }

        private bool ShowHatchWindow { get; set; }

        #endregion

        #region Datasource properties

        // dataSource for Resource manifest and ResourceTransfer windows
        // Provides a list of resources and the parts that contain that resource.
        public List<string> ResourceList = new List<string>();
        public Dictionary<string, List<Part>> _partsByResource = null;
        public Dictionary<string, List<Part>> PartsByResource
        {
            get
            {
                try
                {
                    if (_partsByResource == null)
                        _partsByResource = new Dictionary<string, List<Part>>();
                    else
                        _partsByResource.Clear();

                    // Let's update...
                    if (FlightGlobals.ActiveVessel != null)
                    {
                        //ManifestUtilities.LogMessage(string.Format(" getting partsbyresource.  "), "Info", SettingsManager.VerboseLogging);
                        ShipManifestAddon.vessel = Vessel;

                        _partsByResource = new Dictionary<string, List<Part>>();
                        foreach (Part part in Vessel.Parts)
                        {
                            // First let's Get any Crew, if desired...
                            if (SettingsManager.EnableCrew && part.CrewCapacity > 0)
                            {
                                bool vResourceFound = false;
                                // is resource in the list yet?.
                                if (_partsByResource.Keys.Contains("Crew"))
                                {
                                    // found resource.  lets add part to its list.
                                    vResourceFound = true;
                                    List<Part> eParts = _partsByResource["Crew"];
                                    eParts.Add(part);
                                }
                                if (!vResourceFound)
                                {
                                    // found a new resource.  lets add it to the list of resources.
                                    List<Part> nParts = new List<Part>();
                                    nParts.Add(part);
                                    _partsByResource.Add("Crew", nParts);
                                }
                            }
                            // Let's Get any Science...
                            if (SettingsManager.EnableScience)
                            {
                                bool mResourceFound = false;
                                IScienceDataContainer[] sciModules = part.FindModulesImplementing<IScienceDataContainer>().ToArray();
                                foreach (IScienceDataContainer pm in sciModules)
                                {
                                    // is resource in the list yet?.
                                    // 
                                    if (!mResourceFound && (pm is IScienceDataContainer))
                                    {
                                        if (_partsByResource.Keys.Contains("Science"))
                                        {
                                            mResourceFound = true;
                                            List<Part> eParts = _partsByResource["Science"];
                                            eParts.Add(part);
                                            break;
                                        }
                                        if (!mResourceFound)
                                        {
                                            // found a new resource.  lets add it to the list of resources.
                                            List<Part> nParts = new List<Part>();
                                            nParts.Add(part);
                                            _partsByResource.Add("Science", nParts);
                                            mResourceFound = true;
                                            break;
                                        }
                                    }
                                }
                            }

                            // Now, let's get flight Resources.
                            foreach (PartResource resource in part.Resources)
                            {
                                // Realism Mode.  we want to exclude Resources with TransferMode = NONE...
                                if (!SettingsManager.RealismMode || (SettingsManager.RealismMode && resource.info.resourceTransferMode != ResourceTransferMode.NONE))
                                {
                                    bool vResourceFound = false;
                                    // is resource in the list yet?.
                                    if (_partsByResource.Keys.Contains(resource.info.name))
                                    {
                                        vResourceFound = true;
                                        List<Part> eParts = _partsByResource[resource.info.name];
                                        eParts.Add(part);
                                    }
                                    if (!vResourceFound)
                                    {
                                        // found a new resource.  lets add it to the list of resources.
                                        List<Part> nParts = new List<Part>();
                                        nParts.Add(part);
                                        _partsByResource.Add(resource.info.name, nParts);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ManifestUtilities.LogMessage(string.Format(" getting partsbyresource.  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                    _partsByResource = null;
                }

                if (_partsByResource != null)
                    ResourceList = new List<string>(_partsByResource.Keys);
                else
                    ResourceList = null;

                return _partsByResource;
            }
        }

        // dataSource for Resource manifest and ResourceTransfer windows
        // Holds the Resource.info.name selected in the Resource Manifest Window.
        private string _prevSelectedResource;
        private string _selectedResource;
        public string SelectedResource
        {
            get
            {
                return _selectedResource;
            }
            set
            {
                try
                {
                    _prevSelectedResource = _selectedResource;
                    _selectedResource = value;

                    if (value == null)
                        SelectedResourceParts = new List<Part>();
                    else
                        SelectedResourceParts = _partsByResource[_selectedResource];
                }
                catch (Exception ex)
                {
                    ManifestUtilities.LogMessage(string.Format(" in Set SelectedResource.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                }
            }
        }

        // Provides a list of parts for a given resource.
        // Used to maintain Add/Remove of OnMouseExit handlers
        private List<Part> _selectedResourceParts;
        public List<Part> SelectedResourceParts
        {
            get
            {
                if (_selectedResourceParts == null)
                    _selectedResourceParts = new List<Part>();
                return _selectedResourceParts;
            }
            set
            {
                ClearResourceHighlighting(_selectedResourceParts);
                _selectedResourceParts = value;

                if (SettingsManager.EnableCLS)
                {
                    if (ShipManifestAddon.clsVessel == null)
                        ShipManifestAddon.GetCLSVessel();
                    if (SelectedResource == "Crew")
                    {
                        if (ShipManifestAddon.clsVessel != null)
                            ShipManifestAddon.clsVessel.Highlight(true);
                        else
                            ManifestUtilities.LogMessage("Set SelectedResourceParts: clsVessel is null.", "Info", SettingsManager.VerboseLogging);
                    }
                    else if (ShipManifestAddon.clsVessel != null)
                        ShipManifestAddon.clsVessel.Highlight(false);
                }

                //ManifestUtilities.LogMessage("Set SelectedResourceParts.", "Info", SettingsManager.VerboseLogging);
            }
        }

        private Part _selectedPartSource;
        public Part SelectedPartSource
        {
            get
            {
                try
                {
                    if (_selectedPartSource != null && !FlightGlobals.ActiveVessel.Parts.Contains(_selectedPartSource))
                        _selectedPartSource = null;

                    return _selectedPartSource;
                }
                catch (Exception ex)
                {
                    ManifestUtilities.LogMessage(string.Format(" in Get SelectedPartSource.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                    return null;
                }
            }
            set
            {
                try
                {
                    _selectedPartSource = value;

                    // reset transfer amount (for resource xfer slider control)
                    ShipManifestAddon.smController.sXferAmount = -1f;
                    ShipManifestAddon.smController.tXferAmount = -1f;
                }
                catch (Exception ex)
                {
                    ManifestUtilities.LogMessage(string.Format(" in Set SelectedPartSource.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                }
                //ManifestUtilities.LogMessage("Set SelectedPartSource.", "Info", SettingsManager.VerboseLogging);
            }
        }

        private Part _selectedPartTarget;
        public Part SelectedPartTarget
        {
            get
            {
                try
                {

                    if (_selectedPartTarget != null && !FlightGlobals.ActiveVessel.Parts.Contains(_selectedPartTarget))
                        _selectedPartTarget = null;
                    return _selectedPartTarget;
                }
                catch (Exception ex)
                {
                    ManifestUtilities.LogMessage(string.Format(" in Get SelectedPartTarget.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                    return null;
                }
            }
            set
            {
                try
                {
                    _selectedPartTarget = value;

                    // reset transfer amount (for resource xfer slider control)
                    ShipManifestAddon.smController.sXferAmount = -1f;
                    ShipManifestAddon.smController.tXferAmount = -1f;
                }
                catch (Exception ex)
                {
                    ManifestUtilities.LogMessage(string.Format(" in Set SelectedPartTarget.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                }
                //ManifestUtilities.LogMessage("Set SelectedPartTarget.", "Info", SettingsManager.VerboseLogging);
            }
        }

        public PartModule SelectedModuleSource;
        public PartModule SelectedModuleTarget;

        public ICLSPart clsPartSource;
        public ICLSPart clsPartTarget;

        public GameEvents.FromToAction<Part, Part> evaAction;

        #endregion

        public ManifestController()
        {
            RenderingManager.AddToPostDrawQueue(3, drawGui);
        }

        private void drawGui()
        {
            string step = "";
            try
            {
                step = "0 - Start";
                if (FlightGlobals.fetch == null || FlightGlobals.ActiveVessel != Vessel)
                {
                    step = "0a - Vessel Change";
                    SelectedPartSource = SelectedPartTarget = null;
                    SelectedResource = null;
                    return;
                }

                ManifestStyle.SetupGUI();

                step = "1 - Show Interface(s)";
                // Is the scene one we want to be visible in?
                if (HighLogic.LoadedScene == GameScenes.FLIGHT && !MapView.MapIsEnabled && !PauseMenu.isOpen && !FlightResultsDialog.isDisplaying)
                {
                    RefreshHighlight();
                    if (SettingsManager.ShowShipManifest)
                    {
                        step = "2 - Show Manifest";
                        ShipManifestAddon.Settings.ManifestPosition = GUILayout.Window(398544, ShipManifestAddon.Settings.ManifestPosition, ShipManifestWindow, "Ship's Manifest - " + Vessel.vesselName, GUILayout.MinHeight(20));
                    }

                    // What windows do we want to show?
                    if (SettingsManager.ShowShipManifest && SettingsManager.ShowTransferWindow && SelectedResource != null)
                    {
                        step = "3 - Show Transfer";
                        ShipManifestAddon.Settings.TransferPosition = GUILayout.Window(398545, ShipManifestAddon.Settings.TransferPosition, TransferWindow, "Transfer - " + Vessel.vesselName + " - " + SelectedResource, GUILayout.MinHeight(20));
                    }

                    //if (ShowShipManifest && ShowHatchWindow)
                    //{
                    //    ShipManifestBehaviour.ShipManifestSettings.HatchesPosition = GUILayout.Window(398549, ShipManifestBehaviour.ShipManifestSettings.HatchesPosition, HatchWindow.Display, "Hatches - " + Vessel.vesselName, GUILayout.MinHeight(20));
                    //}

                    if (SettingsManager.ShowShipManifest && SettingsManager.ShowSettings)
                    {
                        step = "4 - Show Settings";
                        SettingsManager.SettingsPosition = GUILayout.Window(398546, SettingsManager.SettingsPosition, ShipManifestAddon.Settings.SettingsWindow, "Ship Manifest Settings", GUILayout.MinHeight(20));
                    }

                    if (resetRosterSize)
                    {
                        step = "5 - Reset Roster Size";
                        SettingsManager.RosterPosition.height = 100; //reset hight
                        SettingsManager.RosterPosition.width = 400; //reset width
                        resetRosterSize = false;
                    }

                    if (SettingsManager.ShowShipManifest && SettingsManager.ShowRoster)
                    {
                        step = "6 - Show Roster";
                        SettingsManager.RosterPosition = GUILayout.Window(398547, SettingsManager.RosterPosition, RosterWindow, "Ship Manifest Roster", GUILayout.MinHeight(20));
                    }
                }
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format(" in drawGui at or near step:  " + step + ".  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        #region Ship Manifest Window - Gui Layout Code

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

                GUILayout.Label(SelectedResource != null ? string.Format("{0}", SelectedResource) : "No Resource Selected", GUILayout.Width(300), GUILayout.Height(20));

                // Resource Details List Viewer
                ResourceDetailsViewer();

                GUILayout.BeginHorizontal();

                //var hatchStyle = ShowHatchWindow ? ManifestStyle.ButtonToggledStyle : ManifestStyle.ButtonStyle;
                //if (GUILayout.Button("Hatches...", hatchStyle, GUILayout.Width(100), GUILayout.Height(20)))
                //{
                //    try
                //    {
                //        ShowHatchWindow = !ShowHatchWindow;
                //    }
                //    catch (Exception ex)
                //    {
                //        ManifestUtilities.LogMessage(string.Format(" opening Hatches Window.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                //    }
                //}

                var settingsStyle = SettingsManager.ShowSettings ? ManifestStyle.ButtonToggledStyle : ManifestStyle.ButtonStyle;
                if (GUILayout.Button("Settings...", settingsStyle, GUILayout.Width(100), GUILayout.Height(20)))
                {
                    try
                    {
                        SettingsManager.ShowSettings = !SettingsManager.ShowSettings;
                    }
                    catch (Exception ex)
                    {
                        ManifestUtilities.LogMessage(string.Format(" opening Settings Window.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                    }
                }

                var rosterStyle = SettingsManager.ShowRoster ? ManifestStyle.ButtonToggledStyle : ManifestStyle.ButtonStyle;
                if (GUILayout.Button("Roster...", rosterStyle, GUILayout.Width(100), GUILayout.Height(20)))
                {
                    try
                    {
                        SettingsManager.ShowRoster = !SettingsManager.ShowRoster;
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

        #endregion

        #region Ship Manifest Window Gui components

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

                if (SettingsManager.EnablePFResources)
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
                foreach (string resourceName in PartsByResource.Keys)
                {
                    GUILayout.BeginHorizontal();
                    int width = 265;
                    if ((!SettingsManager.RealismMode || IsPreLaunch) && resourceName != "Crew" && resourceName != "Science")
                        width = 175;

                    var style = SelectedResource == resourceName ? ManifestStyle.ButtonToggledStyle : ManifestStyle.ButtonStyle;
                    if (GUILayout.Button(string.Format("{0}", resourceName), style, GUILayout.Width(width), GUILayout.Height(20)))
                    {
                        try
                        {
                            if (!ShipManifestAddon.crewXfer && !ShipManifestAddon.sXferOn && !ShipManifestAddon.tXferOn)
                            {
                                // Now let's update our lists...
                                if (SelectedResource != resourceName)
                                {
                                    SelectedResource = resourceName;
                                    SelectedPartSource = SelectedPartTarget = null;
                                }
                                else if (SelectedResource == resourceName)
                                {
                                    SelectedResource = null;
                                    SelectedPartSource = SelectedPartTarget = null;
                                }
                                if (SelectedResource != null)
                                {
                                    SettingsManager.ShowTransferWindow = true;
                                }
                                else
                                {
                                    SettingsManager.ShowTransferWindow = false;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ManifestUtilities.LogMessage(string.Format("Error selecting Resource.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                        }
                    }
                    if ((!SettingsManager.RealismMode || IsPreLaunch) && resourceName != "Crew" && resourceName != "Science")
                    {
                        if (GUILayout.Button(string.Format("{0}", "Dump"), ManifestStyle.ButtonStyle, GUILayout.Width(45), GUILayout.Height(20)))
                        {
                            DumpResource(resourceName);
                        }
                        if (GUILayout.Button(string.Format("{0}", "Fill"), ManifestStyle.ButtonStyle, GUILayout.Width(35), GUILayout.Height(20)))
                        {
                            FillResource(resourceName);
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

                if (SelectedResource != null)
                {
                    foreach (Part part in PartsByResource[SelectedResource])
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
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format(" in ResourceDetailsViewer.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
                GUILayout.EndVertical();
                GUILayout.EndScrollView();
        }

        #endregion

        #region Methods

        public void AddCrew(int count, Part part)
        {
            if (IsPreLaunch && !PartCrewIsFull(part))
            {
                for (int i = 0; i < part.CrewCapacity && i < count; i++)
                {
                    ProtoCrewMember kerbal = HighLogic.CurrentGame.CrewRoster.GetNextOrNewKerbal();
                    part.AddCrewmember(kerbal);

                    if (kerbal.seat != null)
                        kerbal.seat.SpawnCrew();
                }
            }
        }

        public void AddCrew(ProtoCrewMember kerbal, Part part)
        {
            part.AddCrewmember(kerbal);
            kerbal.rosterStatus = ProtoCrewMember.RosterStatus.Assigned;
            if (part.internalModel != null)
            {
                if (kerbal.seat != null)
                    kerbal.seat.SpawnCrew();
            }
            ShipManifestAddon.FireEventTriggers();
        }

        private bool PartCrewIsFull(Part part)
        {
            return !(part.protoModuleCrew.Count < part.CrewCapacity);
        }

        private Part FindPart(ProtoCrewMember kerbal)
        {
            foreach (Part part in FlightGlobals.ActiveVessel.Parts)
            {
                foreach (ProtoCrewMember curkerbal in part.protoModuleCrew)
                {
                    if (curkerbal == kerbal)
                    {
                        return part;
                    }
                }
            }
            return null;
        }

        private void RemoveCrew(ProtoCrewMember member, Part part)
        {
            part.RemoveCrewmember(member);
            member.rosterStatus = ProtoCrewMember.RosterStatus.Available;
        }

        private void RespawnKerbal(ProtoCrewMember kerbal)
        {
            kerbal.SetTimeForRespawn(0);
            kerbal.Spawn();
            kerbal.rosterStatus = ProtoCrewMember.RosterStatus.Available;
            HighLogic.CurrentGame.CrewRoster.GetNextAvailableKerbal();
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
            ShipManifestAddon.FireEventTriggers();
        }

        private void FillVesselCrew()
        {
            foreach (var part in _partsByResource["Crew"])
            {
                AddCrew(part.CrewCapacity - part.protoModuleCrew.Count, part);
            }
            ShipManifestAddon.FireEventTriggers();
        }

        private void EmptyVesselCrew()
        {
            foreach (var part in _partsByResource["Crew"])
            {
                for (int i = part.protoModuleCrew.Count - 1; i >= 0; i--)
                {
                    RemoveCrew(part.protoModuleCrew[i], part);
                }
                ShipManifestAddon.FireEventTriggers();
            }
        }

        private void FillVesselResources()
        {
            List<string> resources = _partsByResource.Keys.ToList<string>();
            foreach (string resourceName in resources)
            {
                if (resourceName != "Crew" && resourceName != "Science")
                {
                    foreach (Part part in _partsByResource[resourceName])
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
            List<string> resources = _partsByResource.Keys.ToList<string>();
            foreach (string resourceName in resources)
            {
                if (resourceName != "Crew" && resourceName != "Science")
                {
                    foreach (Part part in _partsByResource[resourceName])
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
            foreach (Part part in _partsByResource[resourceName])
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

        private void FillResource(string resourceName)
        {
            foreach (Part part in _partsByResource[resourceName])
            {
                foreach (PartResource resource in part.Resources)
                {
                    if (resource.info.name == resourceName)
                    {
                        resource.amount = resource.maxAmount;
                    }
                }
            }
        }

        private void DumpPartResource(Part part, string resourceName)
        {
            foreach (PartResource resource in part.Resources)
            {
                if (resource.info.name == resourceName)
                {
                    resource.amount = 0;
                }
            }
        }

        private void FillPartResource(Part part, string resourceName)
        {
            foreach (PartResource resource in part.Resources)
            {
                if (resource.info.name == resourceName)
                {
                    resource.amount = resource.maxAmount;
                }
            }
        }

        #endregion

        #region Highlighting methods

        /// <summary>
        /// Remove highlighting on a part.
        /// </summary>
        /// <param name="part">Part to remove highlighting from.</param>
        public static void ClearPartHighlight(Part part)
        {
            try
            {
                if (part != null)
                {
                    part.SetHighlightDefault();
                    part.SetHighlight(false);
                }
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format(" in  ClearPartHighlight.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        /// <summary>
        /// Clear highlighting on parts belonging to the selected resource list.
        /// </summary>
        /// <param name="_ResourceParts"></param>
        public static void ClearResourceHighlighting(List<Part> _ResourceParts)
        {
            if (_ResourceParts != null)
            {
                foreach (Part part in _ResourceParts)
                {
                    ClearPartHighlight(part);
                }
            }
        }

        public static void SetPartHighlight(Part part, Color color)
        {
            try
            {

                if (part != null)
                {
                    part.SetHighlightColor(color);
                    part.SetHighlight(true);
                }
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format(" in  SetPartHighlight.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        public void RefreshHighlight()
        {
            string step = "";
            try
            {
                // Do we even want to highlight?
                if (SettingsManager.ShowShipManifest && SelectedResourceParts != null && SettingsManager.EnableHighlighting)
                {
                    step = "Showhipmanifest = true";
                    if (SettingsManager.ShowTransferWindow)
                    {
                        if (SelectedResource == "Crew" && SettingsManager.EnableCLS && ShipManifestAddon.clsVessel != null)
                        {
                            step = "Highlight CLS vessel";
                            ShipManifestAddon.clsVessel.Highlight(true);
                            if (clsPartSource != null)
                                clsPartSource.Highlight(false);
                            if (clsPartTarget != null)
                                clsPartTarget.Highlight(false);
                        }

                        step = "Set Selected Part Colors";
                        if (SelectedPartSource != null)
                            SetPartHighlight(SelectedPartSource, SettingsManager.Colors[SettingsManager.SourcePartColor]);
                        if (SelectedPartTarget!= null)
                            if (SelectedResource == "Crew" && SettingsManager.EnableCLS)
                                SetPartHighlight(SelectedPartTarget, SettingsManager.Colors[SettingsManager.TargetPartCrewColor]);
                            else
                                SetPartHighlight(SelectedPartTarget, SettingsManager.Colors[SettingsManager.TargetPartColor]);

                        // Default is yellow
                        step = "Set non selected resource part color";
                        Color partColor = Color.yellow;

                        if (SelectedResource == "Crew" && SettingsManager.EnableCLS)
                            partColor = Color.green;

                        step = "Set Resource Part Colors";
                        foreach (Part thispart in SelectedResourceParts)
                        {
                            if (thispart != SelectedPartSource && thispart != SelectedPartTarget)
                            {
                                SetPartHighlight(thispart, partColor);
                            }
                        }
                    }
                    else
                    {
                        step = "ShowShipManifest = false";
                        if (SelectedResourceParts != null)
                            foreach (Part thispart in SelectedResourceParts)
                            {
                                ClearPartHighlight(thispart);
                            }

                        step = "clsVessel.Highlight(false)";
                        if (SettingsManager.EnableCLS && ShipManifestAddon.clsVessel != null)
                            ShipManifestAddon.clsVessel.Highlight(false);
                    }
                }
            }
            catch (Exception ex)
            {
                if (!ShipManifestAddon.frameErrTripped)
                {
                    ManifestUtilities.LogMessage(string.Format(" in RefreshHighlight.  Error in step:  " + step + ".  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                    ShipManifestAddon.frameErrTripped = true;
                }
            }
        }

        #endregion
    }
}
