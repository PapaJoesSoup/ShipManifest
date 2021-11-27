using System.Collections.Generic;
using System.Linq;

namespace ShipManifest.Modules
{
  internal class ModDockedVessel
  {
    private List<Part> _vesselParts;

    // used during display in control window for selecting vessels to combine
    internal bool Combine = false;

    private Part _rootPart;
    private Part _dockingPort;
    internal bool IsDocked;
    internal bool IsEditing = false;
    internal bool IsReTyping = false;
    internal string RenameVessel = null;

    internal ModDockedVessel()
    {
    }

    internal ModDockedVessel(ModuleDockingNode dockingNode)
    {
      _dockingPort = dockingNode.part;
      VesselInfo = dockingNode.vesselInfo;
      _vesselParts = SMAddon.SmVessel.GetDockedVesselParts(dockingNode);
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
      get { return Rootpart != null ? Rootpart.launchID : 0; }
    }

    internal string VesselName
    {
      get { return VesselInfo.name; }
      set { VesselInfo.name = value; }
    }

    internal VesselType TypeVessel
    {
      get { return VesselInfo.vesselType; }
      set
      {
        if (Rootpart)
        {
          Rootpart.vesselNaming.vesselType = value;
          Rootpart.vesselType = value;
          Rootpart.vessel.vesselType = value;
        }
        VesselInfo.vesselType = value;
      }
    }

    internal List<Part> VesselParts
    {
      get { return _vesselParts; }
      set { _vesselParts = value; }
    }
  }
}
