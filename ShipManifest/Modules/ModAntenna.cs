using ShipManifest.APIClients;

namespace ShipManifest.Modules
{
  internal class ModAntenna
  {
    internal ModAntenna()
    {
    }

    internal ModAntenna(PartModule xModule, PartModule pModule, Part iPart)
    {
      XmitterModule = xModule;
      AnimateModule = pModule;
      SPart = iPart;
    }

    internal PartModule XmitterModule { get; set; }

    internal PartModule AnimateModule { get; set; }

    internal Part SPart { get; set; }

    internal bool IsRtModule
    {
      get
      {
        if (XmitterModule.moduleName == "ModuleRTAntenna")
          return true;
        return false;
      }
    }

    internal bool Extended
    {
      get
      {
        if (IsRtModule)
          return XmitterModule.Events["EventClose"].active;
        return Module.Events["Toggle"].guiName == "Retract";
      }
    }

    internal string AntennaStatus
    {
      get
      {
        // RT support:
        if (IsRtModule)
        {
          if (XmitterModule.Events["EventClose"].active)
            return "Activated";
          return "Deactivated";
        }
        if (Module.Events["Toggle"].guiName == "Retract")
          return "Extended";
        return "Retracted";
      }
    }

    internal string Title
    {
      get
      {
        string title;
        try
        {
          title = SPart.partInfo.title + "\r\n on " + SPart.parent.partInfo.title;
        }
        catch
        {
          title = SPart.partInfo.title;
        }
        return title;
      }
    }

    private ModuleAnimateGeneric Module
    {
      get { return (ModuleAnimateGeneric) AnimateModule; }
    }

    internal void ExtendAntenna()
    {
      // RT support:
      if (IsRtModule)
      {
        if (InstalledMods.IsRtInstalled)
        {
          //if (RemoteTech.API.API.HasFlightComputer(SmAddon.vessel.id))
          //{
          //    ConfigNode configNode = new ConfigNode();
          //    configNode.AddValue("GUIDString", SmAddon.vessel.id);
          //    configNode.AddValue("Executor", "ShipManifest");
          //    configNode.AddValue("ReflectionType", "ShipManifest");
          //    RemoteTech.API.API.QueueCommandToFlightComputer(configNode);
          //}
          //else
          XmitterModule.Events["EventOpen"].Invoke();
        }
        else
          XmitterModule.Events["EventOpen"].Invoke();
      }
      else if (Module.Events["Toggle"].guiName == "Extend")
        Module.Toggle();
    }

    internal void RetractAntenna()
    {
      // RT support:
      if (IsRtModule)
        XmitterModule.Events["EventClose"].Invoke();
      else if (Module.Events["Toggle"].guiName == "Retract")
        Module.Toggle();
    }
  }
}