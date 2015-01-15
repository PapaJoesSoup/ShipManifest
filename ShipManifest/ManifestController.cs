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
                ClearResourceHighlighting(SelectedResourceParts);
                showRosterWindow = value;
            } 
        }

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
                            if (Settings.EnableCrew && part.CrewCapacity > 0)
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
                            if (Settings.EnableScience)
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
                                if (!Settings.RealismMode || (Settings.RealismMode && resource.info.resourceTransferMode != ResourceTransferMode.NONE))
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
                    ClearResourceHighlighting(SelectedResourceParts);

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
                    if (Settings.EnableCLS)
                    {
                        UpdateCLSSpaces();
                    }

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
                    if (Settings.EnableCLS)
                    {
                        UpdateCLSSpaces();
                    }

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
        public ICLSSpace clsSpaceSource;
        public ICLSSpace clsSpaceTarget;

        private static List<PartModule> _hatchModules = new List<PartModule>();
        public static List<PartModule> HatchModules
        {
            get
            {
                if (_hatchModules == null)
                    _hatchModules = new List<PartModule>();
                return _hatchModules;
            }
            set
            {
                _hatchModules.Clear();
                _hatchModules = value;
            }
        }

        private static void GetHatchModules()
        {
            _hatchModules.Clear();
            try
            {
                foreach (Part iPart in ShipManifestAddon.vessel.Parts)
                {
                    foreach (PartModule iModule in iPart.Modules)
                    {
                        if (iModule.moduleName == "ModuleDockingHatch")
                            _hatchModules.Add(iModule);
                    }
                }
            }
            catch (Exception ex)
            {
                if (!ShipManifestAddon.frameErrTripped)
                {
                    ManifestUtilities.LogMessage(string.Format(" in GetHatchModules.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                    ShipManifestAddon.frameErrTripped = true;
                }
            }
        }

        public void UpdateCLSSpaces()
        {
            ShipManifestAddon.GetCLSVessel();
            if (ShipManifestAddon.clsVessel != null)
            {
                try
                {
                    if (_selectedPartSource != null)
                    {
                        clsPartSource = null;
                        clsSpaceSource = null;
                        foreach (ICLSSpace sSpace in ShipManifestAddon.clsVessel.Spaces)
                        {
                            foreach (ICLSPart sPart in sSpace.Parts)
                            {
                                if (sPart.Part == _selectedPartSource)
                                {
                                    clsPartSource = sPart;
                                    clsSpaceSource = sSpace;
                                    ManifestUtilities.LogMessage("UpdateCLSSpaces - clsPartSource found;", "info", Settings.VerboseLogging);
                                    break;
                                }
                            }
                        }
                        if (clsSpaceSource == null)
                            ManifestUtilities.LogMessage("UpdateCLSSpaces - clsSpaceSource is null.", "info", Settings.VerboseLogging);
                    }
                    if (_selectedPartTarget != null)
                    {
                        clsPartTarget = null;
                        clsSpaceTarget = null;
                        foreach (ICLSSpace tSpace in ShipManifestAddon.clsVessel.Spaces)
                        {
                            foreach (ICLSPart tPart in tSpace.Parts)
                            {
                                if (tPart.Part == _selectedPartTarget)
                                {
                                    clsPartTarget = tPart;
                                    clsSpaceTarget = tSpace;
                                    ManifestUtilities.LogMessage("UpdateCLSSpaces - clsPartTarget found;", "info", Settings.VerboseLogging);
                                    break;
                                }
                            }
                        }
                        if (clsSpaceTarget == null)
                            ManifestUtilities.LogMessage("UpdateCLSSpaces - clsSpaceTarget is null.", "info", Settings.VerboseLogging);
                    }
                }
                catch (Exception ex)
                {
                    ManifestUtilities.LogMessage(string.Format(" in UpdateCLSSpaces.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                }
            }
            else
                ManifestUtilities.LogMessage("UpdateCLSSpaces - clsVessel is null... done.", "info", Settings.VerboseLogging);
        }
        
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
                    // why is this here?
                    //UpdateHighlighting();
                    if (ShipManifestAddon.CanShowShipManifest())
                    {
                        step = "2 - Show Manifest";
                        Settings.ManifestPosition = GUILayout.Window(398544, Settings.ManifestPosition, ShipManifestWindow, "Ship's Manifest - " + Vessel.vesselName, GUILayout.MinHeight(20));
                    }

                    // What windows do we want to show?
                    if (ShipManifestAddon.CanShowShipManifest() && Settings.ShowTransferWindow && SelectedResource != null)
                    {
                        step = "3 - Show Transfer";
                        Settings.TransferPosition = GUILayout.Window(398545, Settings.TransferPosition, TransferWindow, "Transfer - " + Vessel.vesselName + " - " + SelectedResource, GUILayout.MinHeight(20));
                    }

                    if (ShipManifestAddon.CanShowShipManifest() && Settings.ShowSettings)
                    {
                        step = "4 - Show Settings";
                        Settings.SettingsPosition = GUILayout.Window(398546, Settings.SettingsPosition, Settings.SettingsWindow, "Ship Manifest Settings", GUILayout.MinHeight(20));
                    }

                    if (resetRosterSize)
                    {
                        step = "5 - Reset Roster Size";
                        Settings.RosterPosition.height = 100; //reset hight
                        Settings.RosterPosition.width = 400; //reset width
                        resetRosterSize = false;
                    }

                    if (Settings.ShowShipManifest && Settings.ShowRoster)
                    {
                        step = "6 - Show Roster";
                        Settings.RosterPosition = GUILayout.Window(398547, Settings.RosterPosition, RosterWindow, "Ship Manifest Roster", GUILayout.MinHeight(20));
                    }
                }
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format(" in drawGui at or near step:  " + step + ".  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        #region Action Methods

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
                    part.SetHighlight(false, false);
                    part.SetHighlightDefault();
                    part.highlightType = Part.HighlightType.OnMouseOver;
                }
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format(" in  ClearPartHighlight.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        /// <summary>
        /// Removes Highlighting on parts belonging to the selected resource list.
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
                    part.SetHighlight(true, false);
                    part.highlightType = Part.HighlightType.AlwaysOn;
                }
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format(" in  SetPartHighlight.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        public void UpdateHighlighting()
        {
            string step = "";
            try
            {
                // Do we even want to highlight?
                if (Settings.EnableHighlighting)
                {
                    step = "Showhipmanifest = true";
                    if (ShipManifestAddon.CanShowShipManifest())
                    {
                        //if (Settings.EnableCLS && ShipManifestAddon.clsVessel != null)
                        //{
                        //    if (SelectedResource == "Crew")
                        //    {
                        //        step = "Highlight CLS vessel";
                        //        HighlightCLSVessel(true);

                        //        // Turn off the source and target cls highlighting.  We are going to replace it.
                        //        if (clsPartSource != null)
                        //            clsPartSource.Highlight(false);
                        //        if (clsPartTarget != null)
                        //            clsPartTarget.Highlight(false);
                        //    }
                        //    else
                        //    {
                        //        HighlightCLSVessel(false);
                        //    }
                        //}

                        step = "Set Selected Part Colors";
                        if (SelectedPartSource != null)
                            SetPartHighlight(SelectedPartSource, Settings.Colors[Settings.SourcePartColor]);
                        if (SelectedPartTarget != null)
                            if (SelectedResource == "Crew" && Settings.EnableCLS)
                                SetPartHighlight(SelectedPartTarget, Settings.Colors[Settings.TargetPartCrewColor]);
                            else
                                SetPartHighlight(SelectedPartTarget, Settings.Colors[Settings.TargetPartColor]);

                        // Default is yellow
                        step = "Set non selected resource part color";
                        Color partColor = Color.yellow;

                        if (SelectedResource == "Crew" && Settings.EnableCLS)
                            partColor = Color.green;

                        step = "Set Resource Part Colors";
                        foreach (Part thispart in SelectedResourceParts)
                        {
                            if (thispart != SelectedPartSource && thispart != SelectedPartTarget && !Settings.OnlySourceTarget)
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
                        //if (SettingsManager.EnableCLS)
                        //    HighlightCLSVessel(false);
                    }
                }
                else 
                {
                    step = "clsVessel.Highlight(false)";
                    //if (SettingsManager.EnableCLS)
                    //    HighlightCLSVessel(false);
                }
            }
            catch (Exception ex)
            {
                if (!ShipManifestAddon.frameErrTripped)
                {
                    ManifestUtilities.LogMessage(string.Format(" in UpdateHighlight.  Error in step:  " + step + ".  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                    ShipManifestAddon.frameErrTripped = true;
                }
            }
        }

        //public void HighlightCLSVessel(bool enabled)
        //{
        //    try 
        //    { 
        //        if (ShipManifestAddon.clsVessel == null) 
        //            ShipManifestAddon.GetCLSVessel();
        //        if (ShipManifestAddon.clsVessel != null)
        //        {
        //            foreach (ICLSSpace Space in ShipManifestAddon.clsVessel.Spaces)
        //            {
        //                foreach (ICLSPart part in Space.Parts)
        //                {
        //                    part.Highlight(enabled);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        if (!ShipManifestAddon.frameErrTripped)
        //        {
        //            ManifestUtilities.LogMessage(string.Format(" in HighlightCLSVessel.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
        //            ShipManifestAddon.frameErrTripped = true;
        //        }
        //    }
        //}
        #endregion
    }
}
