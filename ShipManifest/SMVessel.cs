using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

    // Multi-Part Xfer Storage
    private List<ModDockedVessel> _dockedVessels;
    internal List<ModDockedVessel> DockedVessels
    {
      get
      {
        return _dockedVessels ?? (_dockedVessels = new List<ModDockedVessel>());
      }
      set { _dockedVessels = value; }
    }

    internal List<ModDockedVessel> SelectedVesselsSource = new List<ModDockedVessel>();
    internal List<ModDockedVessel> SelectedVesselsTarget = new List<ModDockedVessel>();

    internal List<Part> SelectedResourcesParts = new List<Part>();
    internal List<Part> SelectedPartsSource = new List<Part>();
    internal List<Part> SelectedPartsTarget = new List<Part>();

    internal PartModule SelectedModuleSource;
    internal PartModule SelectedModuleTarget;

    internal ICLSPart ClsPartSource;
    internal ICLSPart ClsPartTarget;
    internal ICLSSpace ClsSpaceSource;
    internal ICLSSpace ClsSpaceTarget;

    internal TransferCrew TransferCrewObj = new TransferCrew();

    // Control Window parts
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

      GetAntennas();
      GetLights();
      GetSolarPanels();
      GetLabs();
      WindowRoster.GetRosterList();
      //Utilities.LogMessage("Exiting:  SMVessel.RefreshLists", Utilities.LogType.Info, SMSettings.VerboseLogging);

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
            if (SMSettings.RealismMode &&
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
    }
    private void UpdateDockedVessels()
    {
      //Utilities.LogMessage("Entered:  SMVessel.UpdateDockedVessels", Utilities.LogType.Info, SMSettings.VerboseLogging);
      if (FlightGlobals.ActiveVessel == null) return;
      _dockedVessels = new List<ModDockedVessel>();
      List<Part>.Enumerator dockingParts = (from p in Vessel.parts where p.Modules.Contains("ModuleDockingNode") select p).ToList().GetEnumerator();
      while (dockingParts.MoveNext())
      {
        if (dockingParts.Current == null) continue;
        Part dPart = dockingParts.Current;
        List<PartModule>.Enumerator dNodes = (from PartModule m in dPart.Modules where m.moduleName == "ModuleDockingNode" select m).ToList().GetEnumerator();
        while (dNodes.MoveNext())
        {
          if (dNodes.Current == null) continue;
          PartModule pModule = dNodes.Current;
          DockedVesselInfo dockedInfo = ((ModuleDockingNode) pModule).vesselInfo;
          if (dockedInfo == null) continue;
          ModDockedVessel modDockedVessel = new ModDockedVessel(dockedInfo);
          List<uint> launchIds = (from m in _dockedVessels where m.LaunchId > 0 select m.LaunchId).ToList();
          if (!launchIds.Contains(modDockedVessel.LaunchId))
            _dockedVessels.Add(modDockedVessel);
        }
      }
      //Utilities.LogMessage("Exiting:  SMVessel.UpdateDockedVessels", Utilities.LogType.Info, SMSettings.VerboseLogging);
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
          break;
      }
    }

    internal void GetHatches()
    {
      _hatches.Clear();
      try
      {
        List<ICLSPart>.Enumerator hParts = SMAddon.ClsAddon.Vessel.Parts.GetEnumerator();
        while (hParts.MoveNext())
        {
          if (hParts.Current == null) continue;
          IEnumerator hModules = hParts.Current.Part.Modules.GetEnumerator();
          while (hModules.MoveNext())
          {
            if (hModules.Current == null) continue;
            PartModule pModule = (PartModule) hModules.Current;
            if (pModule.moduleName != "ModuleDockingHatch") continue;
            ModHatch pHatch = new ModHatch
            {
              HatchModule = (PartModule) hModules.Current,
              ClsPart = hParts.Current
            };
            _hatches.Add(pHatch);
          }
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format("Error in GetHatches().\r\nError:  {0}", ex), Utilities.LogType.Error, true);
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
          IEnumerator pModules = pParts.Current.Modules.GetEnumerator();
          while (pModules.MoveNext())
          {
            if (pModules.Current == null) continue;
            PartModule pModule = (PartModule)pModules.Current;
            if (pModule.moduleName != "ModuleDeployableSolarPanel") continue;
            ModuleDeployableSolarPanel iModule = (ModuleDeployableSolarPanel)pModule;
            if (!iModule.Events["Extend"].active && !iModule.Events["Retract"].active) continue;
            ModSolarPanel pPanel = new ModSolarPanel
            {
              PanelModule = pModule,
              SPart = pParts.Current
            };
            _solarPanels.Add(pPanel);
          }
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format("Error in GetSolarPanels().\r\nError:  {0}", ex), Utilities.LogType.Error, true);
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
          IEnumerator pModules = pParts.Current.Modules.GetEnumerator();
          while (pModules.MoveNext())
          {
            if (pModules.Current == null) continue;
            PartModule pModule = (PartModule) pModules.Current;
            if (pModule.moduleName == "ModuleDataTransmitter" || pModule.moduleName == "ModuleRTAntenna")
            {
              pAntenna.XmitterModule = pModule;
            }
            if (pModule.moduleName == "ModuleAnimateGeneric" &&
                (pModule.Events["Toggle"].guiName == "Extend" || pModule.Events["Toggle"].guiName == "Retract"))
            {
              pAntenna.AnimateModule = pModule;
            }
          }
          _antennas.Add(pAntenna);
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format("Error in GetAntennas().\r\nError:  {0}", ex), Utilities.LogType.Error, true);
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
          if (!pParts.Current.Modules.Contains("ModuleLight")) continue;
          IEnumerator pModules = pParts.Current.Modules.GetEnumerator();
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
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format("Error in GetLights().\r\nError:  {0}", ex), Utilities.LogType.Error, true);
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
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format("Error in GetLabs().\r\nError:  {0}", ex), Utilities.LogType.Error, true);
      }
    }

    internal List<Part> GetSelectedVesselsParts(List<ModDockedVessel> modDockedVessels, List<string> selectedResources)
    {
      List<Part> resourcePartList = new List<Part>();
      if (modDockedVessels == null || modDockedVessels.Count <= 0 || selectedResources == null ||
          selectedResources.Count <= 0) return resourcePartList;
      try
      {
        List<ModDockedVessel>.Enumerator dVessels = modDockedVessels.GetEnumerator();
        while (dVessels.MoveNext())
        {
          if (dVessels.Current == null) continue;
          List<Part>.Enumerator mdvParts = dVessels.Current.VesselParts.GetEnumerator();
          while (mdvParts.MoveNext())
          {
            if (mdvParts.Current == null) continue;
            if (selectedResources.Count > 1 && mdvParts.Current.Resources.Contains(selectedResources[0]) && mdvParts.Current.Resources.Contains(selectedResources[1]))
            {
              resourcePartList.Add(mdvParts.Current);
            }
            else if (mdvParts.Current.Resources.Contains(selectedResources[0]))
            {
              resourcePartList.Add(mdvParts.Current);
            }
          }
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format("Error in GetSelectedVesselParts().\r\nError:  {0}", ex), Utilities.LogType.Error, true);
        resourcePartList = new List<Part>();
      }
      return resourcePartList;
    }

    internal List<Part> GetDockedVesselParts(DockedVesselInfo vesselInfo)
    {
      List<Part> vesselpartList = new List<Part>();
      try
      {
        if (vesselInfo != null)
        {
          Part vesselRoot =
            (from p in Vessel.parts where p.flightID == vesselInfo.rootPartUId select p).SingleOrDefault();
          if (vesselRoot != null)
          {
            vesselpartList = (from p in Vessel.parts where p.launchID == vesselRoot.launchID select p).ToList();
          }
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format("Error in GetSelectedVesselParts().\r\nError:  {0}", ex), Utilities.LogType.Error, true);
        vesselpartList = new List<Part>();
      }
      return vesselpartList;
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
    }

    internal void DumpAllResources()
    {
      List<string> otherResourcesList =
        (from s in SMAddon.SmVessel.PartsByResource.Keys
         where SMConditions.TypeOfResource(s) == SMConditions.ResourceType.Pump
          select s).ToList();
      uint pumpId = TransferPump.GetPumpIdFromHash(string.Join("", otherResourcesList.ToArray()),
        SMAddon.SmVessel.Vessel.parts.First(), SMAddon.SmVessel.Vessel.parts.Last(), TransferPump.TypePump.Dump,
        TransferPump.TriggerButton.Preflight);
      List<TransferPump> pumpList =
        otherResourcesList.Select(
          resource =>
            new TransferPump(resource, TransferPump.TypePump.Dump, TransferPump.TriggerButton.Preflight,
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
      //Fired by Resource Dump on Manifest Window.
      TransferPump pump = new TransferPump(resourceName, TransferPump.TypePump.Dump, TransferPump.TriggerButton.Manifest,
        TransferPump.CalcRemainingResource(SMAddon.SmVessel.PartsByResource[resourceName], resourceName))
      {
        FromParts = SMAddon.SmVessel.PartsByResource[resourceName],
        PumpId = pumpId
      };
      if (!TransferPump.IsPumpInProgress(pumpId))
      {
        SMAddon.SmVessel.TransferPumps.Add(pump);
        ProcessController.DumpResources(SMAddon.SmVessel.TransferPumps);
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
    }

    #endregion
  }
}