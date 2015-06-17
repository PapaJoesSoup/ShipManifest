using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using HighlightingSystem;

namespace ShipManifest
{
    class ModDockedVessel
    {
        private DockedVesselInfo _vesselInfo;
        internal DockedVesselInfo VesselInfo
        {
            get { return _vesselInfo; }
            set { _vesselInfo = value; }
        }

        private Part _rootPart = null;
        internal Part Rootpart
        {
            get 
            {
                if (_rootPart == null)
                {
                    _rootPart = (from p in SMAddon.smController.Vessel.parts where p.flightID == VesselInfo.rootPartUId select p).SingleOrDefault();
                }
                return _rootPart; 
            }
            set { _rootPart = value; }
        }

        internal uint LaunchID
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
                string title = VesselInfo.name;
                return title;
            }
        }

        private List<Part> _vesselParts = null;
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
            this.VesselInfo = vesselInfo;
            _vesselParts = SMAddon.smController.GetDockedVesselParts(_vesselInfo);
        }
    }
}
