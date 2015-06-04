using System;
using System.Reflection;
using System.Linq;

namespace ShipManifest
{
    public interface ICrewTransfer
    {
        bool CrewXferActive { get; set; }
        bool IsStockXfer { get; }
        double CrewXferDelaySec { get; }
        bool IsSeat2SeatXfer { get; }
        double Seat2SeatXferDelaySec { get; }

        bool IvaDelayActive { get; }

        Guid XferVesselID { get; }

        Part FromPart { get; }
        Part ToPart { get; }

        ProtoCrewMember FromCrewMember { get; }
        ProtoCrewMember ToCrewMember { get; }

        InternalSeat FromSeat { get; }
        InternalSeat ToSeat { get; }
    }

    public static class SMInterface
    {
        private static bool _smChecked = false;
        private static bool _smInstalled = false;
        public static bool IsSMInstalled
        {
            get
            {
                if (!_smChecked)
                {
                    string assemblyName = "ShipManifest";
                    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    var assembly = (from a in assemblies
                                    where a.FullName.Contains(assemblyName)
                                    select a).SingleOrDefault();
                    if (assembly != null)
                        _smInstalled = true;
                    else
                        _smInstalled = false;
                    _smChecked = true;
                }
                return _smInstalled;
            }
        }
        public static ICrewTransfer GetCrewTransfer()
        {
            ICrewTransfer _ISMobj = null;
            Type SMAddonType = AssemblyLoader.loadedAssemblies.SelectMany(a => a.assembly.GetExportedTypes()).SingleOrDefault(t => t.FullName == "ShipManifest.TransferCrew");
            if (SMAddonType != null)
            {
                object crewTransferObj = SMAddonType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null, null);
                _ISMobj = (ICrewTransfer)crewTransferObj;
            }
            return _ISMobj;
        }
    }

}
