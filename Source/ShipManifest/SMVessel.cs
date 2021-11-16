using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ConnectedLivingSpace;
using ShipManifest.InternalObjects;
using ShipManifest.Modules;
using ShipManifest.Process;
using ShipManifest.Windows;

namespace ShipManifest
{
  // ReSharper disable once InconsistentNaming
  internal class SMVessel
  {
    #region Static Singleton stuff

    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private static Dictionary<WeakReference<Vessel>, SMVessel> _controllers =
      new Dictionary<WeakReference<Vessel>, SMVessel>();

    internal static SMVessel GetInstance(Vessel vessel)
    {
      foreach (KeyValuePair<WeakReference<Vessel>, SMVessel> kvp in _controllers.ToArray())
      {
        WeakReference<Vessel> wr = kvp.Key;
        Vessel v = wr.Target;
        if (v == null)
        {
          _controllers.Remove(wr);
        }
        else if (v == vessel)
        {
          return _controllers[wr];
        }
      }

      SMVessel commander = new SMVessel();
      _controllers[new WeakReference<Vessel>(vessel)] = commander;
      return commander;
    }

    #endregion

    #region Constructor(s)

    #endregion

    #region Instance Properties

    internal Vessel Vessel
    {
      get { return _controllers.Single(p => p.Value == this).Key.Target; }
    }

    internal bool IsRecoverable
    {
      get { return Vessel.IsRecoverable; }
    }

    #endregion

    // dataSources for Resource manifest and ResourceTransfer windows
    #region Instance DataSource properties

    // Provides a list of resources
    internal List<string> ResourceList = new List<string>();

    // This is the main part Dictionary.  Provides a list of resources and the parts that contain that resource.
    private Dictionary<string, List<Part>> _partsByResource;
    internal Dictionary<string, List<Part>> PartsByResource
    {
      get
      {
        return _partsByResource ?? (_partsByResource = new Dictionary<string, List<Part>>());
      }
    }

    // dataSources for Resource manifest and ResourceTransfer windows
    // Holds the Resource.info.name selected in the Resource Manifest Window.
    internal List<string> SelectedResources = new List<string>();
    internal List<TransferPump> TransferPumps = new List<TransferPump>();
    internal TransferCrew TransferCrewObj = new TransferCrew();


    // Multi-Part Xfer Storage
    private List<ModDockedVessel> _modDockedVessels;
    internal List<ModDockedVessel> ModDockedVessels
    {
      get
      {
        return _modDockedVessels ?? (_modDockedVessels = new List<ModDockedVessel>());
      }
      set { _modDockedVessels = value; }
    }

    internal List<ModDockedVessel> SelectedVesselsSource = new List<ModDockedVessel>();
    internal List<ModDockedVessel> SelectedVesselsTarget = new List<ModDockedVessel>();
    internal List<ProtoCrewMember> SourceMembersSelected = new List<ProtoCrewMember>();
    internal List<ProtoCrewMember> TargetMembersSelected = new List<ProtoCrewMember>();

    internal List<Part> SelectedResourcesParts = new List<Part>();
    internal List<Part> SelectedPartsSource = new List<Part>();
    internal List<Part> SelectedPartsTarget = new List<Part>();

    // Used for part to part Science transfers.
    internal PartModule SelectedModuleSource;
    internal PartModule SelectedModuleTarget;

    internal ICLSPart ClsPartSource;
    internal ICLSPart ClsPartTarget;
    internal ICLSSpace ClsSpaceSource;
    internal ICLSSpace ClsSpaceTarget;

    // Control Window parts
    private List<ModDockedVessel> _dockedVessels = new List<ModDockedVessel>();

    internal List<ModDockedVessel> DockedVessels
    {
      get { return _dockedVessels ?? (_dockedVessels = new List<ModDockedVessel>()); }
      set
      {
        _dockedVessels.Clear();
        _dockedVessels = value;
      }
    }

    private List<ModHatch> _hatches = new List<ModHatch>();
    internal List<ModHatch> Hatches
    {
      get { return _hatches ?? (_hatches = new List<ModHatch>()); }
      set
      {
        _hatches.Clear();
        _hatches = value;
      }
    }

    private List<ModuleDockingNode> _dockedPorts = new List<ModuleDockingNode>();
    internal List<ModuleDockingNode> DockedPorts
    {
      get { return _dockedPorts ?? (_dockedPorts = new List<ModuleDockingNode>()); }
      set
      {
        _dockedPorts.Clear();
        _dockedPorts = value;
      }
    }

    private List<ModRadiator> _radiators = new List<ModRadiator>();
    internal List<ModRadiator> Radiators
    {
      get { return _radiators ?? (_radiators = new List<ModRadiator>()); }
      set
      {
        _radiators.Clear();
        _radiators = value;
      }
    }

    private List<ModSolarPanel> _solarPanels = new List<ModSolarPanel>();
    internal List<ModSolarPanel> SolarPanels
    {
      get { return _solarPanels ?? (_solarPanels = new List<ModSolarPanel>()); }
      set
      {
        _solarPanels.Clear();
        _solarPanels = value;
      }
    }

    private List<ModLight> _lights = new List<ModLight>();
    internal List<ModLight> Lights
    {
      get { return _lights ?? (_lights = new List<ModLight>()); }
      set
      {
        _lights.Clear();
        _lights = value;
      }
    }

    private List<ModuleScienceLab> _labs = new List<ModuleScienceLab>();
    internal List<ModuleScienceLab> Labs
    {
      get { return _labs ?? (_labs = new List<ModuleScienceLab>()); }
      set
      {
        _labs.Clear();
        _labs = value;
      }
    }

    private List<ModAntenna> _antennas = new List<ModAntenna>();
    internal List<ModAntenna> Antennas
    {
      get { return _antennas ?? (_antennas = new List<ModAntenna>()); }
      set
      {
        _antennas.Clear();
        _antennas = value;
      }
    }

    #endregion

    #region Instance DataSource Methods

    internal void RefreshLists()
    {
      //Utilities.LogMessage("Entered:  SMVessel.RefreshLists", Utilities.LogType.Info, SMSettings.VerboseLogging);
      if (Vessel == null) return;
      SMConditions.ListsUpdating = true;
      UpdatePartsByResource();
      GetSelectedResourcesParts();
      UpdateDockedVessels();

      // now lets reconcile the selected parts based on the new list of resources...
      WindowManifest.ResolveResourcePartSelections(SMAddon.SmVessel.SelectedResources);

      // Now lets update the Resource Xfer Objects...
      TransferPump.UpdateDisplayPumps();

      // now SM settings / hatches.
      if (SMSettings.EnableCls && SMConditions.CanShowShipManifest())
      {
        if (SMAddon.GetClsAddon())
        {
          SMAddon.UpdateClsSpaces();
          if (SMAddon.GetClsVessel()) GetHatches();
        }
      }

      // Control Window datasources
      GetDockedVessels();
      GetHatches();
      GetAntennas();
      GetLights();
      GetSolarPanels();
      GetLabs();
      WindowRoster.GetRosterList();
      //Utilities.LogMessage("Exiting:  SMVessel.RefreshLists", Utilities.LogType.Info, SMSettings.VerboseLogging);
      SMConditions.ListsUpdating = false;
    }

    internal void UpdatePartsByResource()
    {
      if (_partsByResource == null)
        _partsByResource = new Dictionary<string, List<Part>>();
      else
        _partsByResource.Clear();

      // Let's update...
      if (FlightGlobals.ActiveVessel == null) return;
      List<Part>.Enumerator parts = Vessel.Parts.GetEnumerator();
      while (parts.MoveNext())
      {
        if (parts.Current == null) continue;
        Part part = parts.Current;
        // First let's Get any Crew, if desired...
        if (SMSettings.EnableCrew && (part.CrewCapacity > 0 || SMConditions.IsUsiInflatable(part)) && part.partInfo.name != "kerbalEVA")
        {
          bool vResourceFound = false;
          // is resource in the list yet?.
          if (_partsByResource.Keys.Contains(SMConditions.ResourceType.Crew.ToString()))
          {
            // found resource.  lets add part to its list.
            vResourceFound = true;
            List<Part> eParts = _partsByResource[SMConditions.ResourceType.Crew.ToString()];
            part.crewTransferAvailable = SMSettings.EnableStockCrewXfer;
            eParts.Add(part);
          }
          if (!vResourceFound)
          {
            // found a new resource.  lets add it to the list of resources.
            List<Part> nParts = new List<Part> { part };
            _partsByResource.Add(SMConditions.ResourceType.Crew.ToString(), nParts);
          }
        }
        // Let's Get any Science...
        if (SMSettings.EnableScience)
        {
          IScienceDataContainer[] sciModules = part.FindModulesImplementing<IScienceDataContainer>().ToArray();
          if (sciModules.Length > 0)
          {
            // is resource in the list yet?.
            // We only need the first match on the part so stop.
            if (_partsByResource.Keys.Contains(SMConditions.ResourceType.Science.ToString()))
            {
              _partsByResource[SMConditions.ResourceType.Science.ToString()].Add(part);
            }
            else
            {
              // found a new resource.  lets add it to the list of resources.
              List<Part> nParts = new List<Part> { part };
              _partsByResource.Add(SMConditions.ResourceType.Science.ToString(), nParts);
            }
          }
        }

        // Now, let's get flight Resources.
        if (!SMSettings.EnableResources) continue;
        {
          IEnumerator resources = part.Resources.GetEnumerator();
          while (resources.MoveNext())
          {
            if (resources.Current == null) continue;
            PartResource resource = (PartResource)resources.Current;
            // Realism Mode.  we want to exclude Resources with TransferMode = NONE...
            if (SMSettings.RealXfers &&
                (!resource.info.isVisible || resource.info.resourceTransferMode == ResourceTransferMode.NONE))
              continue;
            bool vResourceFound = false;
            // is resource in the list yet?.
            if (_partsByResource.Keys.Contains(resource.info.name))
            {
              vResourceFound = true;
              List<Part> eParts = _partsByResource[resource.info.name];
              eParts.Add(part);
            }
            if (vResourceFound) continue;
            // found a new resource.  lets add it to the list of resources.
            List<Part> nParts = new List<Part> { part };
            _partsByResource.Add(resource.info.name, nParts);
          }
        }
      }
      parts.Dispose();
    }

    private void UpdateDockedVessels()
    {
      //Utilities.LogMessage("Entered:  SMVessel.UpdateDockedVessels", Utilities.LogType.Info, SMSettings.VerboseLogging);
      if (FlightGlobals.ActiveVessel == null) return;
      
      // Build the base Vessel List
      BuildDockedVesselList();

      //Now lets scrub the list to remove child vessels within the list.
      ScrubVesselList();

      // Now lets filter for Resources.
      FilterVesselListByResources();
    }

    private void BuildDockedVesselList()
    {
      if (_modDockedVessels == null) _modDockedVessels = new List<ModDockedVessel>();
      else _modDockedVessels.Clear();
      List<Part>.Enumerator dockingParts =
        (from p in Vessel.parts where p.Modules.Contains("ModuleDockingNode") select p).ToList().GetEnumerator();
      while (dockingParts.MoveNext())
      {
        if (dockingParts.Current == null) continue;
        List<ModuleDockingNode>.Enumerator dNodes =
          dockingParts.Current.FindModulesImplementing<ModuleDockingNode>().GetEnumerator();
        while (dNodes.MoveNext())
        {
          if (dNodes.Current == null) continue;
          if (dNodes.Current.vesselInfo == null) continue;
          ModDockedVessel modDockedVessel = new ModDockedVessel(dNodes.Current);
          if (modDockedVessel.LaunchId == 0) continue;
          _modDockedVessels.Add(modDockedVessel);
        }
        dNodes.Dispose();
      }
      dockingParts.Dispose();
    }

    private void FilterVesselListByResources()
    {
      List<ModDockedVessel>.Enumerator remainingvessels = _modDockedVessels.GetEnumerator();
      List<ModDockedVessel> vesselsToRemove = new List<ModDockedVessel>();
      while (remainingvessels.MoveNext())
      {
        if (remainingvessels.Current == null) continue;
        List<Part>.Enumerator parts = remainingvessels.Current.VesselParts.GetEnumerator();
        int include = 0;
        while (parts.MoveNext())
        {
          if (parts.Current == null) continue;
          include = 0;
          for (int x = 0; x < SelectedResources.Count; x++)
          {
            if (SelectedResources[x] == SMConditions.ResourceType.Crew.ToString())
            {
              if (parts.Current.CrewCapacity > 0) include++;
            }
            else if (SelectedResources[x] == "Science")
            {
              if (parts.Current.FindModulesImplementing<ModuleScienceExperiment>().Count > 0
                || parts.Current.FindModulesImplementing<ModuleScienceContainer>().Count > 0
                || parts.Current.FindModulesImplementing<ModuleScienceConverter>().Count > 0
                || parts.Current.FindModulesImplementing<ModuleScienceLab>().Count > 0) include++;
            }
            else if (parts.Current.Resources.Contains(SelectedResources[x])) include++;
          }
          if (include == SelectedResources.Count) break;
        }
        if (include == SelectedResources.Count) continue;
        vesselsToRemove.Add(remainingvessels.Current);
      }
      remainingvessels.Dispose();
      if (vesselsToRemove.Count <= 0) return;
      List<ModDockedVessel>.Enumerator remove = vesselsToRemove.GetEnumerator();
      while (remove.MoveNext())
      {
        if (remove.Current == null) continue;
        _modDockedVessels.Remove(remove.Current);
      }
      remove.Dispose();
    }

    private void ScrubVesselList()
    {
      //Now lets scrub the list to remove child vessels within the list.
      // this can happen when multiple decouples and docking occur...
      List<ModDockedVessel> vesselsToRemove = new List<ModDockedVessel>();
      // create vessel list to start wtih
      List<ModDockedVessel>.Enumerator srcVessels = _modDockedVessels.GetEnumerator();
      while (srcVessels.MoveNext())
      {
        if (srcVessels.Current == null) continue;
        // create vessel list to compare to
        bool isUnique = false;
        List<ModDockedVessel>.Enumerator cmpVessels = _modDockedVessels.GetEnumerator();
        while (cmpVessels.MoveNext())
        {
          if (cmpVessels.Current == null) continue;
          if (cmpVessels.Current == srcVessels.Current) continue;
          if (vesselsToRemove.Contains(cmpVessels.Current)) continue;
          List<Part>.Enumerator vParts = cmpVessels.Current.VesselParts.GetEnumerator();
          while (vParts.MoveNext())
          {
            // if any part of the target is not in the src vessel, the src is not a child of the target vessel.  move on.
            if (srcVessels.Current.VesselParts.Contains(vParts.Current)) continue;
            isUnique = true;
            break;
          }
          vParts.Dispose();
          if (isUnique) continue;
          // since we are here, then all parts are in another vessel.  this is likely a child.  add it to the remove list.
          vesselsToRemove.Add(cmpVessels.Current);
          break;
        }
        cmpVessels.Dispose();
      }
      srcVessels.Dispose();
      if (vesselsToRemove.Count <= 0) return;
      List<ModDockedVessel>.Enumerator remove = vesselsToRemove.GetEnumerator();
      while (remove.MoveNext())
      {
        if (remove.Current == null) continue;
        _modDockedVessels.Remove(remove.Current);
      }
      remove.Dispose();
    }

    internal void GetSelectedResourcesParts()
    {
      switch (SelectedResources.Count)
      {
        case 1:
          SelectedResourcesParts = PartsByResource[SelectedResources[0]];
          break;
        case 2:
          SelectedResourcesParts.Clear();
          List<Part>.Enumerator partlist = Vessel.Parts.GetEnumerator();
          while (partlist.MoveNext())
          {
            if (partlist.Current == null) continue;
            if (partlist.Current.Resources.Contains(SelectedResources[0]) && partlist.Current.Resources.Contains(SelectedResources[1]))
              SelectedResourcesParts.Add(partlist.Current);
          }
          partlist.Dispose();
          break;
      }
    }

    internal void GetDockedVessels()
    {
      if (_dockedVessels == null) _dockedVessels = new List<ModDockedVessel>();
      else _dockedVessels.Clear();
      List<Part>.Enumerator dockingParts =
        (from p in Vessel.parts where p.Modules.Contains("ModuleDockingNode") select p).ToList().GetEnumerator();
      while (dockingParts.MoveNext())
      {
        if (dockingParts.Current == null) continue;
        List<ModuleDockingNode>.Enumerator dNodes =
          dockingParts.Current.FindModulesImplementing<ModuleDockingNode>().GetEnumerator();
        while (dNodes.MoveNext())
        {
          if (dNodes.Current == null) continue;
          if (dNodes.Current.vesselInfo == null) continue;
          ModDockedVessel modDockedVessel = new ModDockedVessel(dNodes.Current);
          if (modDockedVessel.LaunchId == 0) continue;
          modDockedVessel.IsDocked = SMConditions.IsVesselDocked(modDockedVessel);
          _dockedVessels.Add(modDockedVessel);
        }
        dNodes.Dispose();
      }
      dockingParts.Dispose();

      //Now lets scrub the list to remove child vessels within the list.
      // this can happen when multiple decouples and docking occur...
      List<ModDockedVessel> vesselsToRemove = new List<ModDockedVessel>();
      // create vessel list to start wtih
      List<ModDockedVessel>.Enumerator srcVessels = _dockedVessels.GetEnumerator();
      while (srcVessels.MoveNext())
      {
        if (srcVessels.Current == null) continue;
        // create vessel list to compare to
        bool isUnique = false;
        List<ModDockedVessel>.Enumerator cmpVessels = _dockedVessels.GetEnumerator();
        while (cmpVessels.MoveNext())
        {
          if (cmpVessels.Current == null) continue;
          if (cmpVessels.Current == srcVessels.Current) continue;
          if (vesselsToRemove.Contains(cmpVessels.Current)) continue;
          List<Part>.Enumerator vParts = cmpVessels.Current.VesselParts.GetEnumerator();
          while (vParts.MoveNext())
          {
            // if any part of the target is not in the src vessel, the src is not a child of the target vessel.  move on.
            if (srcVessels.Current.VesselParts.Contains(vParts.Current)) continue;
            isUnique = true;
            break;
          }
          vParts.Dispose();
          if (isUnique) continue;
          // since we are here, then all parts are in another vessel.  this is likely a child.  add it to the remove list.
          vesselsToRemove.Add(cmpVessels.Current);
          break;
        }
        cmpVessels.Dispose();
      }
      srcVessels.Dispose();
      if (vesselsToRemove.Count <= 0) return;
      List<ModDockedVessel>.Enumerator remove = vesselsToRemove.GetEnumerator();
      while (remove.MoveNext())
      {
        if (remove.Current == null) continue;
        _dockedVessels.Remove(remove.Current);
      }
      remove.Dispose();
    }

    internal void GetHatches()
    {
      _hatches.Clear();
      try
      {
        if (!SMSettings.EnableCls || !SMConditions.CanShowShipManifest()) return;
        if (!SMAddon.GetClsAddon()) return;
        SMAddon.UpdateClsSpaces();
        if (!SMAddon.GetClsVessel()) return;
        List<ICLSPart>.Enumerator hParts = SMAddon.ClsAddon.Vessel.Parts.GetEnumerator();
        while (hParts.MoveNext())
        {
          if (hParts.Current == null) continue;
          List<PartModule>.Enumerator hModules = hParts.Current.Part.Modules.GetEnumerator();
          while (hModules.MoveNext())
          {
            if (hModules.Current == null) continue;
            PartModule pModule = (PartModule)hModules.Current;
            if (pModule.moduleName != "ModuleDockingHatch") continue;
            ModHatch pHatch = new ModHatch
            {
              HatchModule = (PartModule)hModules.Current,
              ClsPart = hParts.Current
            };
            _hatches.Add(pHatch);
          }
          hModules.Dispose();
        }
        hParts.Dispose();
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage($"Error in GetHatches().\r\nError:  {ex}", SmUtils.LogType.Error, true);
      }
    }

    internal void GetSolarPanels()
    {
      _solarPanels.Clear();
      try
      {
        List<Part>.Enumerator pParts = Vessel.Parts.GetEnumerator();
        while (pParts.MoveNext())
        {
          if (pParts.Current == null) continue;
          List<ModuleDeployableSolarPanel>.Enumerator pModules = pParts.Current.FindModulesImplementing<ModuleDeployableSolarPanel>().GetEnumerator();
          while (pModules.MoveNext())
          {
            ModuleDeployableSolarPanel solarPanel = pModules.Current;
            if (solarPanel == null) continue;
            if (!solarPanel.Events["Extend"].active && !solarPanel.Events["Retract"].active) continue;
            ModSolarPanel pPanel = new ModSolarPanel
            {
              PanelModule = solarPanel,
              SPart = pParts.Current
            };
            _solarPanels.Add(pPanel);
          }
          pModules.Dispose();
        }
        pParts.Dispose();
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage($"Error in GetSolarPanels().\r\nError:  {ex}", SmUtils.LogType.Error, true);
      }
    }

    internal void GetAntennas()
    {
      _antennas.Clear();
      try
      {
        // Added support for RemoteTech antennas
        List<Part>.Enumerator pParts = Vessel.Parts.GetEnumerator();
        while (pParts.MoveNext())
        {
          if (pParts.Current == null) continue;
          if (!pParts.Current.Modules.Contains("ModuleDataTransmitter") && !pParts.Current.Modules.Contains("ModuleRTAntenna")) continue;
          ModAntenna pAntenna = new ModAntenna { SPart = pParts.Current };
          List<PartModule>.Enumerator pModules = pParts.Current.Modules.GetEnumerator();
          while (pModules.MoveNext())
          {
            if (pModules.Current == null) continue;
            PartModule pModule = (PartModule) pModules.Current;
            if (pModule is ModuleDataTransmitter || pModule.moduleName == "ModuleRTAntenna")
            {
              pAntenna.XmitterModule = pModule;
            }
            if (pModule is ModuleDeployableAntenna
              || pModule is ModuleAnimateGeneric && (pModule.Events["Toggle"].guiName == "Extend" || pModule.Events["Toggle"].guiName == "Retract"))
            {
              pAntenna.AnimateModule = pModule;
            }
          }
          pModules.Dispose();
          if (pAntenna.AnimateModule != null || pAntenna.IsRtModule) _antennas.Add(pAntenna);
        }
        pParts.Dispose();
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage($"Error in GetAntennas().\r\nError:  {ex}", SmUtils.LogType.Error, true);
      }
    }

    internal void GetLights()
    {
      _lights.Clear();
      try
      {
        List<Part>.Enumerator pParts = Vessel.Parts.GetEnumerator();
        while (pParts.MoveNext())
        {
          if (pParts.Current == null) continue;
          if (!pParts.Current.FindModulesImplementing<ModuleLight>().Any()) continue;
          List<PartModule>.Enumerator pModules = pParts.Current.Modules.GetEnumerator();
          while (pModules.MoveNext())
          {
            if (pModules.Current == null) continue;
            PartModule pModule = (PartModule) pModules.Current;
            if (pModule.moduleName != "ModuleLight") continue;
            ModLight pLight = new ModLight
            {
              LightModule = pModule,
              SPart = pParts.Current
            };
            _lights.Add(pLight);
          }
          pModules.Dispose();
        }
        pParts.Dispose();
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage($"Error in GetLights().\r\nError:  {ex}", SmUtils.LogType.Error, true);
      }
    }

    internal void GetLabs()
    {
      _labs.Clear();
      try
      {
        List<Part>.Enumerator pParts = Vessel.Parts.GetEnumerator();
        while (pParts.MoveNext())
        {
          if (pParts.Current == null) continue;
          if (!pParts.Current.Modules.Contains("ModuleScienceLab")) continue;
          IEnumerator pModules = pParts.Current.Modules.GetEnumerator();
          while (pModules.MoveNext())
          {
            if (pModules.Current == null) continue;
            PartModule pModule = (PartModule) pModules.Current;
            if (pModule.moduleName != "ModuleScienceLab") continue;
            _labs.Add((ModuleScienceLab)pModule);
          }
        }
        pParts.Dispose();
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage($"Error in GetLabs().\r\nError:  {ex}", SmUtils.LogType.Error, true);
      }
    }

    internal List<Part> GetVesselsPartsByResource(List<ModDockedVessel> modDockedVessels, List<string> selectedResources)
    {
      List<Part> resourcePartList = new List<Part>();
      if (modDockedVessels == null || modDockedVessels.Count <= 0 
        || selectedResources == null || selectedResources.Count <= 0) return resourcePartList;
      try
      {
        List<ModDockedVessel>.Enumerator dVessel = modDockedVessels.GetEnumerator();
        while (dVessel.MoveNext())
        {
          if (dVessel.Current == null) continue;
          List<Part>.Enumerator mdvPart = dVessel.Current.VesselParts.GetEnumerator();
          while (mdvPart.MoveNext())
          {
            if (mdvPart.Current == null) continue;
            if (selectedResources.Contains(SMConditions.ResourceType.Crew.ToString()))
            {
              if (mdvPart.Current.CrewCapacity > 0) resourcePartList.Add(mdvPart.Current);
            }
            if (selectedResources.Contains(SMConditions.ResourceType.Science.ToString()))
            {
              if (mdvPart.Current.FindModulesImplementing<ModuleScienceExperiment>().Any()
                || mdvPart.Current.FindModulesImplementing<ModuleScienceContainer>().Any()
                || mdvPart.Current.FindModulesImplementing<ModuleScienceConverter>().Any()
                || mdvPart.Current.FindModulesImplementing<ModuleScienceLab>().Any()) resourcePartList.Add(mdvPart.Current);
            }
            else
            {
              // resources
              if (selectedResources.Count > 1 && mdvPart.Current.Resources.Contains(selectedResources[0]) &&
                  mdvPart.Current.Resources.Contains(selectedResources[1]))
              {
                resourcePartList.Add(mdvPart.Current);
              }
              else if (mdvPart.Current.Resources.Contains(selectedResources[0]))
              {
                resourcePartList.Add(mdvPart.Current);
              }
            }
          }
          mdvPart.Dispose();
        }
        dVessel.Dispose();
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage($"Error in GetSelectedVesselParts().\r\nError:  {ex}", SmUtils.LogType.Error, true);
        resourcePartList = new List<Part>();
      }
      return resourcePartList;
    }

    internal List<ProtoCrewMember> GetCrewFromParts(List<Part> parts)
    {
      List<ProtoCrewMember> crew = new List<ProtoCrewMember>();
      List<Part>.Enumerator part = parts.GetEnumerator();
      while (part.MoveNext())
      {
        if (part.Current == null) continue;
        if (part.Current.CrewCapacity <= 0) continue;
        crew.AddRange(part.Current.protoModuleCrew);
      }
      part.Dispose();
      return crew;
    }

    internal List<Part> GetDockedVesselParts(ModuleDockingNode dockingNode)
    {
      List<Part> vesselpartList = new List<Part>();
      try
      {
        if (dockingNode != null)
        {
          // The root part matters for the original vessel.  However, vessels that are split from the original vessel have their docking port as the root.
          // So, if the root is the docking port, we want to grab the children from the docking port.  if the root is different than the port, we want to use
          // the Root, as the docking port will not have a parent...
          if (!vesselpartList.Contains(dockingNode.part)) vesselpartList.Add(dockingNode.part);
          Part vesselRoot =
            (from p in Vessel.parts where p.flightID == dockingNode.vesselInfo.rootPartUId select p).SingleOrDefault();
          if (vesselRoot != null)
          {
            GetChildren(vesselRoot, ref vesselpartList);
          }
        }
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage($"Error in GetSelectedVesselParts().\r\nError:  {ex}", SmUtils.LogType.Error, true);
        vesselpartList = new List<Part>();
      }
      return vesselpartList;
    }

    private static void GetChildren(Part part, ref List<Part> partList)
    {
      if (!partList.Contains(part)) partList.Add(part);
      List<Part>.Enumerator children = part.children.GetEnumerator();
      while (children.MoveNext())
      {
        if (children.Current == null) continue;
        ModuleDockingNode dNode = children.Current.FindModulesImplementing<ModuleDockingNode>().FirstOrDefault();
        if (dNode != null && dNode.vesselInfo != null && dNode.otherNode != null && dNode.otherNode.vesselInfo != null) continue;
        if (!partList.Contains(children.Current)) partList.Add(children.Current);
        GetChildren(children.Current, ref partList);
      }
      children.Dispose();
    }

    internal Part FindPartByKerbal(ProtoCrewMember pKerbal)
    {
      Part kPart = FlightGlobals.ActiveVessel.Parts.Find(x => x.protoModuleCrew.Find(y => y == pKerbal) != null);
      return kPart;
    }

    #endregion

    #region Action Methods

    internal void RespawnCrew()
    {
      Vessel.SpawnCrew();
      SMAddon.FireEventTriggers();
    }

    internal void FillCrew()
    {
      //Utilities.LogMessage("Entering FillCrew", Utilities.LogType.Info, true);
      List<Part>.Enumerator parts = PartsByResource[SMConditions.ResourceType.Crew.ToString()].GetEnumerator();
      while (parts.MoveNext())
      {
        if (parts.Current == null) continue;
        //Utilities.LogMessage(string.Format("Processing FillCrew with part {0}", parts.Current.partInfo.name), Utilities.LogType.Info, true);
        SMPart.FillCrew(parts.Current);
      }
      parts.Dispose();
      SMAddon.FireEventTriggers();
    }

    internal void EmptyCrew()
    {
      List<Part>.Enumerator parts = PartsByResource[SMConditions.ResourceType.Crew.ToString()].GetEnumerator();
      while (parts.MoveNext())
      {
        if (parts.Current == null) continue;
        for (int i = parts.Current.protoModuleCrew.Count - 1; i >= 0; i--)
        {
          TransferCrew.RemoveCrewMember(parts.Current.protoModuleCrew[i], parts.Current);
        }
        SMAddon.FireEventTriggers();
      }
      parts.Dispose();
    }

    internal void DumpAllResources()
    {
      List<string> otherResourcesList =
        (from s in SMAddon.SmVessel.PartsByResource.Keys
         where SMConditions.TypeOfResource(s) == SMConditions.ResourceType.Pump
          select s).ToList();
      uint pumpId = TransferPump.GetPumpIdFromHash(string.Join("", otherResourcesList.ToArray()),
        SMAddon.SmVessel.Vessel.parts.First(), SMAddon.SmVessel.Vessel.parts.Last(), TransferPump.TypeXfer.Dump,
        TransferPump.TriggerButton.Preflight);
      List<TransferPump> pumpList =
        otherResourcesList.Select(
          resource =>
            new TransferPump(resource, TransferPump.TypeXfer.Dump, TransferPump.TriggerButton.Preflight,
              TransferPump.CalcRemainingResource(SMAddon.SmVessel.PartsByResource[resource], resource))
            {
              FromParts = SMAddon.SmVessel.PartsByResource[resource],
              PumpId = pumpId
            }).ToList();
      if (!TransferPump.PumpsInProgress(pumpId).Any())
      {
        SMAddon.SmVessel.TransferPumps.AddRange(pumpList);
        ProcessController.DumpResources(SMAddon.SmVessel.TransferPumps);
      }
      else TransferPump.AbortAllPumpsInProcess(pumpId);
    }

    internal static void ToggleDumpResource(string resourceName, uint pumpId)
    {
      if (!TransferPump.IsPumpInProgress(pumpId))
      {
        //Fired by Resource Dump on Manifest Window.
        TransferPump pump = new TransferPump(resourceName, TransferPump.TypeXfer.Dump, TransferPump.TriggerButton.Manifest,
          TransferPump.CalcRemainingResource(SMAddon.SmVessel.PartsByResource[resourceName], resourceName))
        {
          FromParts = SMAddon.SmVessel.PartsByResource[resourceName],
          PumpId = pumpId
        };
        List<TransferPump> pumps = new List<TransferPump>
        {
          pump
        };
        ProcessController.DumpResources(pumps);
      }
      else TransferPump.AbortAllPumpsInProcess(pumpId);
    }

    internal void FillResources()
    {
      List<Part>.Enumerator parts = Vessel.parts.GetEnumerator();
      while (parts.MoveNext())
      {
        if (parts.Current == null) continue;
        if (parts.Current.Resources.Count <= 0) continue;
        IEnumerator pResources = parts.Current.Resources.GetEnumerator();
        while (pResources.MoveNext())
        {
          if (pResources.Current == null) continue;
          if (((PartResource) pResources.Current).info.name == SMConditions.ResourceType.Crew.ToString() ||
              ((PartResource) pResources.Current).info.name == SMConditions.ResourceType.Science.ToString()) continue;
          ((PartResource) pResources.Current).amount = ((PartResource) pResources.Current).maxAmount;
        }
      }
      parts.Dispose();
    }

    internal void FillResource(string resourceName)
    {
      List<Part>.Enumerator parts = PartsByResource[resourceName].GetEnumerator();
      while (parts.MoveNext())
      {
        if (parts.Current == null) continue;
        if (!parts.Current.Resources.Contains(resourceName)) continue;
        IEnumerator resources = parts.Current.Resources.GetEnumerator();
        while (resources.MoveNext())
        {
          if (resources.Current == null) continue;
          if (((PartResource) resources.Current).info.name != resourceName) continue;
          ((PartResource) resources.Current).amount = ((PartResource) resources.Current).maxAmount;
        }
      }
      parts.Dispose();
    }

    #endregion
  }
}
