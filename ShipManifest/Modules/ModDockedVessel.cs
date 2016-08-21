using System.Collections.Generic;
using System.Linq;

namespace ShipManifest.Modules
{
  internal class ModDockedVessel
  {
    private readonly List<Part> _vesselParts;

    private Part _rootPart;

    internal ModDockedVessel()
    {
    }

    internal ModDockedVessel(DockedVesselInfo vesselInfo)
    {
      VesselInfo = vesselInfo;
      _vesselParts = SMAddon.SmVessel.GetDockedVesselParts(VesselInfo);
    }

    internal DockedVesselInfo VesselInfo { get; set; }

    internal Part Rootpart
    {
      get
      {
        return _rootPart ??
               (_rootPart =
                 (from p in SMAddon.SmVessel.Vessel.parts where p.flightID == VesselInfo.rootPartUId select p)
                   .SingleOrDefault());
      }
      set { _rootPart = value; }
    }

    internal uint LaunchId
    {
      get { return Rootpart.launchID; }
    }

    internal string VesselName
    {
      get
      {
        string title = VesselInfo.name;
        return title;
      }
    }

    internal List<Part> VesselParts
    {
      get { return _vesselParts; }
    }
  }
}