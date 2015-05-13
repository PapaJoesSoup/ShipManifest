using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using ConnectedLivingSpace;

namespace ShipManifest
{
    internal class SMController
    {
        #region Singleton stuff

        private static Dictionary<WeakReference<Vessel>, SMController> controllers = new Dictionary<WeakReference<Vessel>, SMController>();

        internal static SMController GetInstance(Vessel vessel)
        {
            foreach (var kvp in controllers.ToArray())
            {
                var wr = kvp.Key;
                var v = wr.Target;
                if (v == null)
                {
                    controllers.Remove(wr);
                }
                else if (v == vessel)
                {
                    return controllers[wr];
                }
            }

            var commander = new SMController();
            controllers[new WeakReference<Vessel>(vessel)] = commander;
            return commander;
        }

        #endregion

        #region Properties

        // variables used for tracking xfer sliders for resources.
        internal double AmtXferred = 0;
        internal List<ModXferResource> ResourcesToXfer = new List<ModXferResource>();

        internal Vessel Vessel
        {
            get { return controllers.Single(p => p.Value == this).Key.Target; }
        }

        internal bool IsPreLaunch
        {
            get { return Vessel.landedAt == "LaunchPad" || Vessel.landedAt == "Runway"; }
        }


        #endregion

        #region Datasource properties

        // dataSource for Resource manifest and ResourceTransfer windows
        // Provides a list of resources and the parts that contain that resource.
        internal List<string> ResourceList = new List<string>();
        internal Dictionary<string, List<Part>> _partsByResource = null;
        internal Dictionary<string, List<Part>> PartsByResource
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
                        //Utilities.LogMessage(string.Format(" getting partsbyresource.  "), "Info", SettingsManager.VerboseLogging);
                        SMAddon.vessel = Vessel;

                        _partsByResource = new Dictionary<string, List<Part>>();
                        foreach (Part part in Vessel.Parts)
                        {
                            // First let's Get any Crew, if desired...
                            if (Settings.EnableCrew && (part.CrewCapacity > 0 && part.partInfo.name != "kerbalEVA"))
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
                            if (Settings.EnableResources)
                            {
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
                }
                catch (Exception ex)
                {
                    Utilities.LogMessage(string.Format(" getting partsbyresource.  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                    _partsByResource = null;
                }

                if (_partsByResource != null)
                    ResourceList = new List<string>(_partsByResource.Keys);
                else
                    ResourceList.Clear();

                return _partsByResource;
            }
        }

        // dataSource for Resource manifest and ResourceTransfer windows
        // Holds the Resource.info.name selected in the Resource Manifest Window.
        internal List<string> SelectedResources = new List<string>();

        // Multi-Part Xfer Storage
        internal List<Part> SelectedResourcesParts = new List<Part>();
        internal List<Part> SelectedPartsSource = new List<Part>();
        internal List<Part> SelectedPartsTarget = new List<Part>();

        internal Part CrewXferSource;
        internal Part CrewXferTarget;
        internal ProtoCrewMember CrewXferMember;

        internal PartModule SelectedModuleSource;
        internal PartModule SelectedModuleTarget;

        internal ICLSPart clsPartSource;
        internal ICLSPart clsPartTarget;
        internal ICLSSpace clsSpaceSource;
        internal ICLSSpace clsSpaceTarget;

        // Control Window parts
        private List<ModHatch> _hatches = new List<ModHatch>();
        internal List<ModHatch> Hatches
        {
            get
            {
                if (_hatches == null)
                    _hatches = new List<ModHatch>();
                return _hatches;
            }
            set
            {
                _hatches.Clear();
                _hatches = value;
            }
        }

        private List<ModSolarPanel> _solarPanels = new List<ModSolarPanel>();
        internal List<ModSolarPanel> SolarPanels
        {
            get
            {
                if (_solarPanels == null)
                    _solarPanels = new List<ModSolarPanel>();
                return _solarPanels;
            }
            set
            {
                _solarPanels.Clear();
                _solarPanels = value;
            }
        }

        private List<ModLight> _lights = new List<ModLight>();
        internal List<ModLight> Lights
        {
            get
            {
                if (_lights == null)
                    _lights = new List<ModLight>();
                return _lights;
            }
            set
            {
                _lights.Clear();
                _lights = value;
            }
        }

        private List<ModAntenna> _antennas = new List<ModAntenna>();
        internal List<ModAntenna> Antennas
        {
            get
            {
                if (_antennas == null)
                    _antennas = new List<ModAntenna>();
                return _antennas;
            }
            set
            {
                _antennas.Clear();
                _antennas = value;
            }
        }

        //internal GameEvents.FromToAction<Part, Part> evaAction;

        #endregion

        internal SMController()
        {
        }

        #region Action Methods

        internal void GetSelectedResourcesParts()
        {
            if (SelectedResources.Count == 1)
            {
                SelectedResourcesParts = _partsByResource[SelectedResources[0]];
            }
            else if (SelectedResources.Count == 2)
            {
                SelectedResourcesParts.Clear();
                foreach (Part part in Vessel.Parts)
                {
                    if (part.Resources.Contains(SelectedResources[0]) && part.Resources.Contains(SelectedResources[1]))
                        SelectedResourcesParts.Add(part);
                }
            }
        }

        private void FillPartCrew(int count, Part part)
        {
            if (IsPreLaunch && !CrewPartIsFull(part))
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

        internal static void AddCrewMember(ProtoCrewMember kerbal, Part part)
        {
            part.AddCrewmember(kerbal);
            kerbal.rosterStatus = ProtoCrewMember.RosterStatus.Assigned;
            if (part.internalModel != null)
            {
                if (kerbal.seat != null)
                    kerbal.seat.SpawnCrew();
            }
            SMAddon.FireEventTriggers();
        }

        internal static void RemoveCrewMember(ProtoCrewMember member, Part part)
        {
            part.RemoveCrewmember(member);
            member.rosterStatus = ProtoCrewMember.RosterStatus.Available;
            SMAddon.FireEventTriggers();
        }

        internal static bool CrewPartIsFull(Part part)
        {
            return !(part.protoModuleCrew.Count < part.CrewCapacity);
        }

        internal Part FindKerbalPart(ProtoCrewMember pKerbal)
        {
            Part kPart = FlightGlobals.ActiveVessel.Parts.Find(x => x.protoModuleCrew.Find(y => y == pKerbal) != null);
            return kPart;
        }

        internal static void RespawnKerbal(ProtoCrewMember kerbal)
        {
            kerbal.SetTimeForRespawn(0);
            // This call causes issues in KSC scene, and is not needed.
            //kerbal.Spawn();
            kerbal.rosterStatus = ProtoCrewMember.RosterStatus.Available;
            HighLogic.CurrentGame.CrewRoster.GetNextAvailableKerbal();
        }

        internal void RespawnCrew()
        {
            this.Vessel.SpawnCrew();
            SMAddon.FireEventTriggers();
        }

        internal void FillVesselCrew()
        {
            foreach (var part in _partsByResource["Crew"])
            {
                FillPartCrew(part.CrewCapacity - part.protoModuleCrew.Count, part);
            }
            SMAddon.FireEventTriggers();
        }

        internal void EmptyVesselCrew()
        {
            foreach (var part in _partsByResource["Crew"])
            {
                for (int i = part.protoModuleCrew.Count - 1; i >= 0; i--)
                {
                    RemoveCrewMember(part.protoModuleCrew[i], part);
                }
                SMAddon.FireEventTriggers();
            }
        }

        internal void FillVesselResources()
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

        internal void EmptyVesselResources()
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

        internal void DumpResource(string resourceName)
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

        internal void FillResource(string resourceName)
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

        internal static void DumpPartResource(Part part, string resourceName)
        {
            foreach (PartResource resource in part.Resources)
            {
                if (resource.info.name == resourceName)
                {
                    resource.amount = 0;
                }
            }
        }

        internal static void FillPartResource(Part part, string resourceName)
        {
            foreach (PartResource resource in part.Resources)
            {
                if (resource.info.name == resourceName)
                {
                    resource.amount = resource.maxAmount;
                }
            }
        }

        internal void GetHatches()
        {
            _hatches.Clear();
            try
            {
                foreach (ICLSPart iPart in SMAddon.clsAddon.Vessel.Parts)
                {
                    foreach (PartModule pModule in iPart.Part.Modules)
                    {
                        if (pModule.moduleName == "ModuleDockingHatch")
                        {
                            ModHatch pHatch = new ModHatch();
                            pHatch.HatchModule = pModule;
                            pHatch.CLSPart = iPart;
                            _hatches.Add(pHatch);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format("Error in GetHatches().\r\nError:  {0}", ex.ToString()), "Error", true);
            }
        }

        internal void GetSolarPanels()
        {
            _solarPanels.Clear();
            try
            {
                foreach (Part pPart in SMAddon.vessel.Parts)
                {
                    foreach (PartModule pModule in pPart.Modules)
                    {
                        if (pModule.moduleName == "ModuleDeployableSolarPanel")
                        {
                            ModuleDeployableSolarPanel iModule = (ModuleDeployableSolarPanel)pModule;
                            if (iModule.Events["Extend"].active || iModule.Events["Retract"].active)
                            {
                                ModSolarPanel pPanel = new ModSolarPanel();
                                pPanel.PanelModule = pModule;
                                pPanel.SPart = pPart;
                                _solarPanels.Add(pPanel);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format("Error in GetSolarPanels().\r\nError:  {0}", ex.ToString()), "Error", true);
            }
        }

        internal void GetAntennas()
        {
            _antennas.Clear();
            try
            {
                // Added support for RemoteTech antennas
                foreach (Part pPart in SMAddon.vessel.Parts)
                {
                    if (pPart.Modules.Contains("ModuleDataTransmitter") || pPart.Modules.Contains("ModuleRTAntenna"))
                    {
                        ModAntenna pAntenna = new ModAntenna();
                        pAntenna.SPart = pPart;
                        foreach (PartModule pModule in pPart.Modules)
                        {
                            if (pModule.moduleName == "ModuleDataTransmitter" || pModule.moduleName == "ModuleRTAntenna")
                            {
                                pAntenna.XmitterModule = pModule;
                            }
                            if (pModule.moduleName == "ModuleAnimateGeneric" && (pModule.Events["Toggle"].guiName == "Extend" || pModule.Events["Toggle"].guiName == "Retract"))
                            {
                                pAntenna.AnimateModule = pModule;
                            }
                        }
                        _antennas.Add(pAntenna);
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format("Error in GetAntennas().\r\nError:  {0}", ex.ToString()), "Error", true);
            }
        }

        internal void GetLights()
        {
            _lights.Clear();
            try
            {
                foreach (Part pPart in SMAddon.vessel.Parts)
                {
                    foreach (PartModule pModule in pPart.Modules)
                    {
                        if (pModule.moduleName == "ModuleLight")
                        {
                            ModLight pLight = new ModLight();
                            pLight.LightModule = pModule;
                            pLight.SPart = pPart;
                            _lights.Add(pLight);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format("Error in GetLights().\r\nError:  {0}", ex.ToString()), "Error", true);
            }
        }

        #endregion

    }
}
