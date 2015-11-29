using System.Collections.Generic;
using System.Linq;

namespace ShipManifest.Modules
{
  class ModDockedVessel
  {
    private DockedVesselInfo _vesselInfo;
    internal DockedVesselInfo VesselInfo
    {
      get { return _vesselInfo; }
      set { _vesselInfo = value; }
    }

    private Part _rootPart;
    internal Part Rootpart
    {
      get {
        return _rootPart ??
               (_rootPart =
                 (from p in SMAddon.SmVessel.Vessel.parts where p.flightID == VesselInfo.rootPartUId select p)
                   .SingleOrDefault());
      }
      set { _rootPart = value; }
    }

    internal uint LaunchId
    {
      get
      {
        return Rootpart.launchID;
      }
    }

    internal string VesselName
    {
      get
      {
        var title = VesselInfo.name;
        return title;
      }
    }

    private readonly List<Part> _vesselParts;
    internal List<Part> VesselParts
    {
      get
      {
        return _vesselParts;
      }
    }

    internal ModDockedVessel() { }
    internal ModDockedVessel(DockedVesselInfo vesselInfo)
    {
      VesselInfo = vesselInfo;
      _vesselParts = SMAddon.SmVessel.GetDockedVesselParts(_vesselInfo);
    }
  }
}
