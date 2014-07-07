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
                        ShipManifestBehaviour.vessel = Vessel;

                        _partsByResource = new Dictionary<string, List<Part>>();
                        foreach (Part part in Vessel.Parts)
                        {
                            // First let's Get any Crew, if desired...
                            if (ShipManifestBehaviour.ShipManifestSettings.EnableCrew && part.CrewCapacity > 0)
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
                            if (ShipManifestBehaviour.ShipManifestSettings.EnableScience)
                            {
                                bool mResourceFound = false;
                                foreach (PartModule pm in part.Modules)
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
                                        }
                                        if (!mResourceFound)
                                        {
                                            // found a new resource.  lets add it to the list of resources.
                                            List<Part> nParts = new List<Part>();
                                            nParts.Add(part);
                                            _partsByResource.Add("Science", nParts);
                                            mResourceFound = true;
                                        }
                                    }
                                }
                            }

                            // Now, let's get flight Resources.
                            foreach (PartResource resource in part.Resources)
                            {
                                // Realism Mode.  we want to exclude Resources with TransferMode = NONE...
                                if (!ShipManifestBehaviour.ShipManifestSettings.RealismMode || (ShipManifestBehaviour.ShipManifestSettings.RealismMode && resource.info.resourceTransferMode != ResourceTransferMode.NONE))
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
                // This removes the event handler from the currently selected parts, 
                // since we are going to be selecting different parts.
                // For Crew, we do something different so we skip.
                ChangeResourceHighlighting();

                _selectedResourceParts = value;

                SetResourceHighlighting();
                OnMouseHighlights();
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

                    ClearSelectedHighlighting(false);

                    _selectedPartSource = value;

                    SetSelectedHighlighting(false);

                    // reset transfer amount (for resource xfer slider control)
                    ManifestController.GetInstance(FlightGlobals.ActiveVessel).sXferAmount = -1f;
                    ManifestController.GetInstance(FlightGlobals.ActiveVessel).tXferAmount = -1f;
                    OnMouseHighlights();
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
                    ClearSelectedHighlighting(true);

                    _selectedPartTarget = value;

                    SetSelectedHighlighting(true);

                    // reset transfer amount (for resource xfer slider control)
                    ManifestController.GetInstance(FlightGlobals.ActiveVessel).sXferAmount = -1f;
                    ManifestController.GetInstance(FlightGlobals.ActiveVessel).tXferAmount = -1f;
                    OnMouseHighlights();
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
            try
            {
                if (FlightGlobals.fetch == null || FlightGlobals.ActiveVessel != Vessel)
                { 
                    SelectedPartSource = SelectedPartTarget = null;
                    SelectedResource = null;
                    return;
                }

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
                    if (ShowShipManifest && ShowTransferWindow && SelectedResource != null)
                    {
                        ShipManifestBehaviour.ShipManifestSettings.TransferPosition = GUILayout.Window(398545, ShipManifestBehaviour.ShipManifestSettings.TransferPosition, TransferWindow, "Transfer - " + Vessel.vesselName + " - " + SelectedResource, GUILayout.MinHeight(20));
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

        #region Ship Manifest & Roster Window - Gui Layout Code

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

                var transferStyle = ShowTransferWindow ? ManifestStyle.ButtonToggledStyle : ManifestStyle.ButtonStyle;
                if (GUILayout.Button("Transfer...", transferStyle, GUILayout.Width(100), GUILayout.Height(20)))
                {
                    try
                    {
                        if (SelectedResource != null)
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
                foreach (string resourceName in PartsByResource.Keys)
                {
                    GUILayout.BeginHorizontal();
                    int width = 265;
                    if ((!ShipManifestBehaviour.ShipManifestSettings.RealismMode || IsPreLaunch) && resourceName != "Crew" && resourceName != "Science")
                        width = 215;

                    var style = SelectedResource == resourceName ? ManifestStyle.ButtonToggledStyle : ManifestStyle.ButtonStyle;
                    if (GUILayout.Button(string.Format("{0}", resourceName), style, GUILayout.Width(width), GUILayout.Height(20)))
                    {
                        try
                        {
                            // Now let's update our lists...
                            if (SelectedResource != resourceName)
                            {
                                SelectedResource = resourceName;
                                SelectedPartSource = SelectedPartTarget = null;
                            }
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
                                if (pm is IScienceDataContainer)
                                    ScienceCount += ((IScienceDataContainer)pm).GetScienceCount();
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

        private void RosterListViewer()
        {
            try
            {
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

                    if (((ShipManifestBehaviour.ShipManifestSettings.RealismMode && IsPreLaunch) || !ShipManifestBehaviour.ShipManifestSettings.RealismMode) && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.AVAILABLE && SelectedPartSource != null && !PartCrewIsFull(SelectedPartSource))
                    {
                        GUI.enabled = true;
                        buttonText = "Add";
                    }
                    else if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.DEAD || kerbal.rosterStatus == ProtoCrewMember.RosterStatus.MISSING)
                    {
                        GUI.enabled = true;
                        buttonText = "Respawn";
                    }
                    else if (((ShipManifestBehaviour.ShipManifestSettings.RealismMode && IsPreLaunch) || !ShipManifestBehaviour.ShipManifestSettings.RealismMode) && kerbal.rosterStatus == ProtoCrewMember.RosterStatus.ASSIGNED && FlightGlobals.ActiveVessel.GetVesselCrew().Contains(kerbal))
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

        public void AddCrew(ProtoCrewMember kerbal, Part part)
        {
            part.AddCrewmember(kerbal);
            kerbal.rosterStatus = ProtoCrewMember.RosterStatus.ASSIGNED;
            if (part.internalModel != null)
            {
                if (kerbal.seat != null)
                    kerbal.seat.SpawnCrew();
            }
            ShipManifestBehaviour.FireEventTriggers();
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
            foreach (var part in PartsByResource["Crew"])
            {
                AddCrew(part.CrewCapacity - part.protoModuleCrew.Count, part);
            }
            ShipManifestBehaviour.FireEventTriggers();
        }

        private void EmptyVesselCrew()
        {
            foreach (var part in PartsByResource["Crew"])
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

        #endregion

        #region Highlighting methods

        public void ClearResourceHighlighting()
        {
            try
            {
                if (_selectedResource == "Crew" && ShipManifestBehaviour.ShipManifestSettings.EnableCLS)
                {
                    if (SelectedPartSource != null)
                    {
                        ClearHighlight(SelectedPartSource);
                        Part.OnActionDelegate OnMouseExit = ShipManifestBehaviour.MouseExit;
                        SelectedPartSource.RemoveOnMouseExit(OnMouseExit);
                    }
                    if (SelectedPartTarget != null)
                    {
                        ClearHighlight(SelectedPartTarget);
                        Part.OnActionDelegate OnMouseExit = ShipManifestBehaviour.MouseExit;
                        SelectedPartTarget.RemoveOnMouseExit(OnMouseExit);
                    }
                    if (ShipManifestBehaviour.clsVessel != null)
                        ShipManifestBehaviour.clsVessel.Highlight(false);
                }
                else if (_selectedResourceParts != null)
                {
                    foreach (Part part in _selectedResourceParts)
                    {
                        ClearHighlight(part);
                        Part.OnActionDelegate OnMouseExit = ShipManifestBehaviour.MouseExit;
                        part.RemoveOnMouseExit(OnMouseExit);
                    }
                }
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format(" in RemoveResourceHighlighting.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        public void ChangeResourceHighlighting()
        {
            try
            {
                if (_prevSelectedResource == "Crew" && ShipManifestBehaviour.ShipManifestSettings.EnableCLS)
                {
                    if (SelectedPartSource != null)
                        SelectedPartSource = null;

                    if (SelectedPartTarget != null)
                        SelectedPartTarget = null;

                    if (ShipManifestBehaviour.clsVessel != null)
                        ShipManifestBehaviour.clsVessel.Highlight(false);
                 }
                else if (_selectedResourceParts != null)
                {
                    foreach (Part part in _selectedResourceParts)
                    {
                        ClearHighlight(part);
                        Part.OnActionDelegate OnMouseExit = ShipManifestBehaviour.MouseExit;
                        part.RemoveOnMouseExit(OnMouseExit);
                    }
                }
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format(" in ChangeResourceHighlighting.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        public void SetResourceHighlighting()
        {
            try
            {
                // This adds an event handler to each newly selected part,
                // to manage mouse exit events and preserve highlighting.
                if (SelectedResource != null)
                {
                    if (SelectedResource == "Crew" && ShipManifestBehaviour.ShipManifestSettings.EnableCLS)
                    {
                        // Handle CLS instantiation
                        if (ShipManifestBehaviour.clsVessel == null)
                        {
                            ShipManifestBehaviour.clsVessel = ShipManifestBehaviour.clsAddon.Vessel;
                        }

                        // Highlight if we can..
                        if (ShipManifestBehaviour.clsVessel != null)
                        {
                            ShipManifestBehaviour.clsVessel.Highlight(true);
                            ManifestUtilities.LogMessage("Spaces highlighted", "Info", SettingsManager.VerboseLogging);
                        }

                        if (SelectedPartSource != null && clsPartSource != null)
                        {
                            clsPartSource.Highlight(false);
                            Part.OnActionDelegate OnMouseExit = ShipManifestBehaviour.MouseExit;
                            SelectedPartSource.AddOnMouseExit(OnMouseExit);
                            ManifestUtilities.LogMessage("Source Part Highlighted", "Info", SettingsManager.VerboseLogging);
                        }

                        if (SelectedPartTarget != null && clsPartTarget != null)
                        {
                            clsPartTarget.Highlight(false);
                            Part.OnActionDelegate OnMouseExit = ShipManifestBehaviour.MouseExit;
                            SelectedPartTarget.AddOnMouseExit(OnMouseExit);
                            ManifestUtilities.LogMessage("Target part highlighted", "Info", SettingsManager.VerboseLogging);
                        }
                    }
                    else if (_selectedResourceParts != null)
                    {
                        foreach (Part part in _selectedResourceParts)
                        {
                            Part.OnActionDelegate OnMouseExit = ShipManifestBehaviour.MouseExit;
                            part.AddOnMouseExit(OnMouseExit);
                        }
                    }
                    OnMouseHighlights();
                }
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format(" in SetResourceHighlighting.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        /// <summary>
        /// This routine is used to remove source or taget highlighting on a part.  
        /// it is cls Aware, and preserves the base space highlighting when selected part coler is removed.
        /// </summary>
        /// <param name="IsTargetPart"></param>
        public void ClearSelectedHighlighting(bool IsTargetPart)
        {
            try
            {
                Part selectedpart = null;
                if (IsTargetPart)
                {
                    selectedpart = _selectedPartTarget;
                }
                else
                    selectedpart = _selectedPartSource;

                if (SelectedResource == "Crew" && ShipManifestBehaviour.ShipManifestSettings.EnableCLS)
                {
                    if (selectedpart != null)
                    {
                        //turn off Selected part highlghting
                        Part.OnActionDelegate OnMouseExit = ShipManifestBehaviour.MouseExit;
                        selectedpart.RemoveOnMouseExit(OnMouseExit);

                        foreach (ICLSPart clsPart in ShipManifestBehaviour.clsVessel.Parts)
                        {
                            if (clsPart.Part == selectedpart)
                            {
                                // turn on default CLS highlighting
                                clsPart.Highlight(true);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    // reset part to normal resource color
                    SetPartHighlight(selectedpart, Color.yellow);
                }
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format(" in  ClearPartHighlighting(" + IsTargetPart.ToString() + ").  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        /// <summary>
        /// This routine is used ot remove source or taget highlighting on a part.  
        /// it is cls Aware, and preserves the base space highlighting when selected part coler is removed.
        /// </summary>
        /// <param name="IsTargetPart"></param>
        public void SetSelectedHighlighting(bool IsTargetPart)
        {
            try
            {
                Part selectedpart = null;
                if (IsTargetPart)
                    selectedpart = _selectedPartTarget;
                else
                    selectedpart = _selectedPartSource;

                if (SelectedResource == "Crew" && ShipManifestBehaviour.ShipManifestSettings.EnableCLS)
                {
                    if (selectedpart != null)
                    {
                        foreach (ICLSPart clsPart in ShipManifestBehaviour.clsVessel.Parts)
                        {
                            if (clsPart.Part == selectedpart)
                            {
                                // let's turn off CLS highlighting and Turn on Target Part Highlighing.
                                if (IsTargetPart)
                                {
                                    clsPartTarget = clsPart;
                                    clsPartTarget.Highlight(false);
                                }
                                else
                                {
                                    clsPartSource = clsPart;
                                    clsPartSource.Highlight(false);
                                }

                                //turn on Selected part highlghting
                                if (IsTargetPart)
                                    SetPartHighlight(selectedpart, SettingsManager.Colors[SettingsManager.TargetPartCrewColor]);
                                else
                                    SetPartHighlight(selectedpart, SettingsManager.Colors[SettingsManager.SourcePartColor]);

                                Part.OnActionDelegate OnMouseExit = ShipManifestBehaviour.MouseExit;
                                selectedpart.AddOnMouseExit(OnMouseExit);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    // Set part highlighting on the selected part...
                    if (IsTargetPart)
                        SetPartHighlight(selectedpart, SettingsManager.Colors[SettingsManager.TargetPartColor]);
                    else
                        SetPartHighlight(selectedpart, SettingsManager.Colors[SettingsManager.SourcePartColor]);
                }
                OnMouseHighlights();
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format(" in  SetPartHighlighting(" + IsTargetPart.ToString() + ").  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        public static void ClearHighlight(Part part)
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
                ManifestUtilities.LogMessage(string.Format(" in  ClearHighlight.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
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

        #endregion

        public void OnMouseHighlights()
        {
            try
            {
                if (ManifestController.GetInstance(FlightGlobals.ActiveVessel).ShowShipManifest && SelectedResourceParts != null)
                {
                    if (SelectedResource == "Crew" && ShipManifestBehaviour.ShipManifestSettings.EnableCLS)
                    {
                         if (ManifestController.GetInstance(FlightGlobals.ActiveVessel).ShowTransferWindow)
                        {
                            SetPartHighlight(SelectedPartSource, SettingsManager.Colors[SettingsManager.SourcePartColor]);
                            SetPartHighlight(SelectedPartTarget, SettingsManager.Colors[SettingsManager.TargetPartCrewColor]);
                        }
                    }
                    else
                    {
                        foreach (Part thispart in SelectedResourceParts)
                        {
                            if (thispart != SelectedPartSource && thispart != SelectedPartTarget)
                            {
                                SetPartHighlight(thispart, Color.yellow);
                            }
                        }
                        if (ManifestController.GetInstance(FlightGlobals.ActiveVessel).ShowTransferWindow)
                        {
                            SetPartHighlight(SelectedPartSource, SettingsManager.Colors[SettingsManager.SourcePartColor]);
                            SetPartHighlight(SelectedPartTarget, SettingsManager.Colors[SettingsManager.TargetPartColor]);
                        }
                        else
                        {
                            SetPartHighlight(SelectedPartSource, Color.yellow);
                            SetPartHighlight(SelectedPartTarget, Color.yellow);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format(" in  OnMouseHighlights.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

    }
}
