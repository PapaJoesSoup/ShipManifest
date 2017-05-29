namespace ShipManifest.Modules
{
  internal class ModSolarPanel
  {
    internal ModSolarPanel()
    {
    }

    internal ModSolarPanel(PartModule pModule, Part iPart)
    {
      PanelModule = pModule;
      SPart = iPart;
    }

    internal PartModule PanelModule { get; set; }

    internal Part SPart { get; set; }

    internal ModuleDeployablePart.DeployState PanelState
    {
      get { return Module.deployState; }
    }

    internal string PanelStatus
    {
      get { return Module.deployState.ToString(); }
    }

    internal bool Retractable
    {
      get { return Module.retractable; }
    }

    internal bool CanBeRetracted
    {
      get
      {
        if (SMSettings.RealControl && !Retractable &&
            (PanelState == ModuleDeployablePart.DeployState.EXTENDED ||
             PanelState == ModuleDeployablePart.DeployState.EXTENDING))
          return false;
        return true;
      }
    }

    internal string Title
    {
      get
      {
        string title;
        try
        {
          title = $"{SPart.partInfo.title}\r\n {SMUtils.Localize("#smloc_module_001")} {SPart.parent.partInfo.title}";
        }
        catch
        {
          title = SPart.partInfo.title;
        }
        return title;
      }
    }

    private ModuleDeployableSolarPanel Module
    {
      get { return (ModuleDeployableSolarPanel) PanelModule; }
    }

    internal void ExtendPanel()
    {
      Module.Extend();
    }

    internal void RetractPanel()
    {
      Module.Retract();
    }
  }
}