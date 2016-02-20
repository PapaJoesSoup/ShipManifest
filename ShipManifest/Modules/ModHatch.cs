using ConnectedLivingSpace;

namespace ShipManifest.Modules
{
  internal class ModHatch
  {
    internal ModHatch()
    {
    }

    internal ModHatch(PartModule pModule, ICLSPart iPart)
    {
      HatchModule = pModule;
      ClsPart = iPart;
    }

    internal PartModule HatchModule { get; set; }

    internal ICLSPart ClsPart { get; set; }

    internal bool HatchOpen
    {
      get { return Module.HatchOpen; }
      set { Module.HatchOpen = value; }
    }

    internal string HatchStatus
    {
      get { return Module.HatchStatus; }
    }

    internal bool IsDocked
    {
      get { return Module.IsDocked; }
    }

    internal string Title
    {
      get
      {
        string title;
        try
        {
          title = null != ClsPart.Part.parent ? ClsPart.Part.parent.partInfo.title : ClsPart.Part.partInfo.title;
        }
        catch
        {
          title = "Unknown";
        }
        return title;
      }
    }

    private IModuleDockingHatch Module
    {
      // ReSharper disable once SuspiciousTypeConversion.Global
      get { return (IModuleDockingHatch) HatchModule; }
    }

    internal void OpenHatch(bool fireEvent = false)
    {
      Module.HatchEvents["CloseHatch"].active = true;
      Module.HatchEvents["OpenHatch"].active = false;
      Module.HatchOpen = true;
      if (fireEvent)
        SMAddon.FireEventTriggers();
    }

    internal void CloseHatch(bool fireEvent = false)
    {
      Module.HatchEvents["CloseHatch"].active = false;
      Module.HatchEvents["OpenHatch"].active = true;
      Module.HatchOpen = false;
      if (fireEvent)
        SMAddon.FireEventTriggers();
    }
  }
}