namespace ShipManifest.Modules
{
  internal class ModLight
  {
    internal ModLight()
    {
    }

    internal ModLight(PartModule pModule, Part iPart)
    {
      LightModule = pModule;
      SPart = iPart;
    }

    internal PartModule LightModule { get; set; }

    internal Part SPart { get; set; }

    internal string Title
    {
      get
      {
        string title;
        try
        {
          title = SPart.partInfo.title + "\r\n on " + Module.part.parent.partInfo.title;
        }
        catch
        {
          title = SPart.partInfo.title;
        }
        return title;
      }
    }

    internal bool IsOn
    {
      get { return Module.isOn; }
    }

    internal string Status
    {
      get
      {
        if (Module.isOn)
          return "ON";
        return "OFF";
      }
    }

    private ModuleLight Module
    {
      get { return (ModuleLight) LightModule; }
    }

    internal void TurnOnLight()
    {
      Module.LightsOn();
    }

    internal void TurnOffLight()
    {
      Module.LightsOff();
    }
  }
}