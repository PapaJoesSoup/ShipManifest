using System;
using System.Linq;
using System.Reflection;

// TODO: Change this namespace to something specific to your plugin here.
//EG:
// namespace MyPlugin_ShipManifestWrapper
namespace ShipManifest.Distribution
{
  ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
  // BELOW HERE SHOULD NOT BE EDITED - this links to the loaded ShipManifest module without requiring a Hard Dependancy
  ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

  /// <summary>
  /// The Wrapper class to access ShipManifest from another plugin
  /// </summary>
  public class SmWrapper
  {
    protected static Type SmType;
    protected static Type TransferCrewType;
    protected static object ActualSm;

    /// <summary>
    /// This is the ShipManifest object
    ///
    /// SET AFTER INIT
    /// </summary>
    public static Smapi ShipManifestApi;

    /// <summary>
    /// Whether we found the ShipManifest assembly in the loadedassemblies.
    ///
    /// SET AFTER INIT
    /// </summary>
    public static bool AssemblyExists => SmType != null && TransferCrewType != null;

    /// <summary>
    /// Whether we managed to hook the running Instance from the assembly.
    ///
    /// SET AFTER INIT
    /// </summary>
    public static bool InstanceExists => ShipManifestApi != null;

    /// <summary>
    /// Whether we managed to wrap all the methods/functions from the instance.
    ///
    /// SET AFTER INIT
    /// </summary>
    private static bool _smWrapped;

    /// <summary>
    /// Whether the object has been wrapped and the APIReady flag is set in the real ShipManifest
    /// </summary>
    public static bool SmapiReady => _smWrapped;

    /// <summary>
    /// This method will set up the ShipManifest object and wrap all the methods/functions
    /// </summary>
    /// <returns>
    /// Bool indicating success of call
    /// </returns>
    public static bool InitSmWrapper()
    {
      try
      {
        //reset the internal objects
        _smWrapped = false;
        ActualSm = null;
        ShipManifestApi = null;
        LogFormatted("Attempting to Grab ShipManifest Types...");

        //find the base type
        SmType = AssemblyLoader.loadedAssemblies
            .Select(a => a.assembly.GetExportedTypes())
            .SelectMany(t => t)
            .FirstOrDefault(t => t.FullName == "ShipManifest.SMAddon");

        if (SmType == null)
        {
          return false;
        }

        LogFormatted("ShipManifest Version:{0}", SmType.Assembly.GetName().Version.ToString());

        //now the KerbalInfo Type
        TransferCrewType = AssemblyLoader.loadedAssemblies
            .Select(a => a.assembly.GetExportedTypes())
            .SelectMany(t => t)
            .FirstOrDefault(t => t.FullName == "ShipManifest.Process.TransferCrew");

        if (TransferCrewType == null)
        {
          return false;
        }

        //now grab the running instance
        LogFormatted("Got Assembly Types, grabbing Instance");
        try
        {
          ActualSm = SmType.GetField("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
        }
        catch (Exception)
        {
          LogFormatted("No Instance found - most likely you have an old ShipManifest installed");
          return false;
        }
        if (ActualSm == null)
        {
          LogFormatted("Failed grabbing SMAddon Instance");
          return false;
        }

        //If we get this far we can set up the local object and its methods/functions
        LogFormatted("Got Instance, Creating Wrapper Objects");
        ShipManifestApi = new Smapi(ActualSm);
        _smWrapped = true;
        return true;
      }
      catch (Exception ex)
      {
        LogFormatted("Unable to setup InitSMrapper Reflection");
        LogFormatted("Exception: {0}", ex);
        _smWrapped = false;
        return false;
      }
    }

    /// <summary>
    /// The API Class that is an analogue of the real ShipManifest. This lets you access all the API-able properties and Methods of the ShipManifest
    /// </summary>
    public class Smapi
    {
      internal Smapi(object a)
      {
        try
        {
          //store the actual object
          _actualSmapi = a;

          //these sections get and store the reflection info and actual objects where required. Later in the properties we then read the values from the actual objects
          //for events we also add a handler

          LogFormatted("Getting TransferCrew Instance");
          _transferCrewMethod = SmType.GetMethod("get_CrewTransferProcess", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
          _actualCrewTransfer = GetCrewTransferProcess;
          LogFormatted($"Success: {_transferCrewMethod != null}");
          LogFormatted("Getting CrewProcessOn Instance");
          _crewProcessOnMethod = SmType.GetMethod("get_CrewProcessOn", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
          LogFormatted($"Success: {_crewProcessOnMethod != null}");
          LogFormatted("Getting getCrewXferActiveMethod");
          _getCrewXferActiveMethod = TransferCrewType.GetMethod("get_CrewXferActive", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
          LogFormatted($"Success: {_getCrewXferActiveMethod != null}");
          LogFormatted("Getting setCrewXferActiveMethod");
          _setCrewXferActiveMethod = TransferCrewType.GetMethod("set_CrewXferActive", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
          LogFormatted($"Success: {_setCrewXferActiveMethod != null}");
          LogFormatted("Getting getIsStockXferMethod");
          _getIsStockXferMethod = TransferCrewType.GetMethod("get_IsStockXfer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
          LogFormatted($"Success: {_getIsStockXferMethod != null}");
          LogFormatted("Getting getOverrideStockCrewXferMethod");
          _getOverrideStockCrewXferMethod = TransferCrewType.GetMethod("get_OverrideStockCrewXfer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
          LogFormatted($"Success: {_getOverrideStockCrewXferMethod != null}");
          LogFormatted("Getting getCrewXferDelaySecMethod");
          _getCrewXferDelaySecMethod = TransferCrewType.GetMethod("get_CrewXferDelaySec", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
          LogFormatted($"Success: {_getCrewXferDelaySecMethod != null}");
          LogFormatted("Getting getIsSeat2SeatXferMethod");
          _getIsSeat2SeatXferMethod = TransferCrewType.GetMethod("get_IsSeat2SeatXfer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
          LogFormatted($"Success: {_getIsSeat2SeatXferMethod != null}");
          LogFormatted("Getting getSeat2SeatXferDelaySecMethod");
          _getSeat2SeatXferDelaySecMethod = TransferCrewType.GetMethod("get_Seat2SeatXferDelaySec", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
          LogFormatted($"Success: {_getSeat2SeatXferDelaySecMethod != null}");
          LogFormatted("Getting getFromSeatMethod");
          _getFromSeatMethod = TransferCrewType.GetMethod("get_FromSeat", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
          LogFormatted($"Success: {_getFromSeatMethod != null}");
          LogFormatted("Getting getToSeatMethod");
          _getToSeatMethod = TransferCrewType.GetMethod("get_ToSeat", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
          LogFormatted($"Success: {_getToSeatMethod != null}");
          LogFormatted("Getting getXferVesselIdMethod");
          _getXferVesselIdMethod = TransferCrewType.GetMethod("get_XferVesselId", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
          LogFormatted($"Success: {_getXferVesselIdMethod != null}");
          LogFormatted("Getting getIvaDelayActiveMethod");
          _getIvaDelayActiveMethod = TransferCrewType.GetMethod("get_IvaDelayActive", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
          LogFormatted($"Success: {_getIvaDelayActiveMethod != null}");
          LogFormatted("Getting getIvaPortraitDelayMethod");
          _getIvaPortraitDelayMethod = TransferCrewType.GetMethod("get_IvaPortraitDelay", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
          LogFormatted($"Success: {_getIvaPortraitDelayMethod != null}");
          LogFormatted("Getting getFromPartMethod");
          _getFromPartMethod = TransferCrewType.GetMethod("get_FromPart", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
          LogFormatted($"Success: {_getFromPartMethod != null}");
          LogFormatted("Getting getToPartMethod");
          _getToPartMethod = TransferCrewType.GetMethod("get_ToPart", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
          LogFormatted($"Success: {_getToPartMethod != null}");
          LogFormatted("Getting getFromCrewMemberMethod");
          _getFromCrewMemberMethod = TransferCrewType.GetMethod("get_FromCrewMember", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
          LogFormatted($"Success: {_getFromCrewMemberMethod != null}");
          LogFormatted("Getting getToCrewMemberMethod");
          _getToCrewMemberMethod = TransferCrewType.GetMethod("get_ToCrewMember", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
          LogFormatted($"Success: {_getToCrewMemberMethod != null}");
        }

        catch (Exception ex)
        {
          LogFormatted("Unable to Instantiate SMAPI object using Reflection");
          LogFormatted("Exception: {0}", ex);
        }
      }

      private readonly object _actualSmapi;
      private object _actualCrewTransfer;

      /// <summary>
      /// True if a crewXfer on the Active Vessel is currently active
      /// </summary>
      public bool CrewProcessOn;

      private readonly MethodInfo _crewProcessOnMethod;
      private bool GetCrewProcessOn => (bool)_crewProcessOnMethod.Invoke(_actualSmapi, null);

      private readonly MethodInfo _transferCrewMethod;
      /// <summary>
      /// Get the actual CrewTransfer object from the SMAddon Instance
      /// </summary>
      /// <returns>
      /// Object reference to the CrewTransfer Instance
      /// </returns>
      public object GetCrewTransferProcess => _transferCrewMethod?.Invoke(_actualSmapi, null);

      private readonly MethodInfo _getCrewXferActiveMethod;
      private readonly MethodInfo _setCrewXferActiveMethod;
      /// <summary>
      /// True if a crewXfter on the Active Vessel is currently active
      /// If you set it to False whilst a Xfer is active it will cancel the current transfer
      /// </summary> 
      /// <returns>
      /// Bool
      /// </returns>
      public bool CrewXferActive
      {
        get
        {
          if (_actualCrewTransfer == null)
            _actualCrewTransfer = GetCrewTransferProcess;
          return (bool)_getCrewXferActiveMethod.Invoke(_actualCrewTransfer, null);
        }
        set
        {
          if (_actualCrewTransfer == null)
            _actualCrewTransfer = GetCrewTransferProcess;
          _setCrewXferActiveMethod.Invoke(_actualCrewTransfer, new object[] { value });
        }
      }

      private readonly MethodInfo _getIsStockXferMethod;
      /// <summary>
      /// This flag indicates if the transfer was initiated by the Stock Crew Transfer interface. 
      /// It stays enabled during the Crew Transfer Process (just like CrewXferActive)
      /// </summary> 
      /// <returns>
      /// Bool
      /// </returns>
      public bool IsStockXfer
      {
        get
        {
          if (_actualCrewTransfer == null)
            _actualCrewTransfer = GetCrewTransferProcess;
          return (bool)_getIsStockXferMethod.Invoke(_actualCrewTransfer, null);
        }
      }

      private readonly MethodInfo _getOverrideStockCrewXferMethod;
      /// <summary>
      /// This flag indicates if SM is overriding the Stock Crew Transfer process.
      /// </summary> 
      /// <returns>
      /// Bool
      /// </returns>
      public bool OverrideStockCrewXfer
      {
        get
        {
          if (_actualCrewTransfer == null)
            _actualCrewTransfer = GetCrewTransferProcess;
          return (bool)_getOverrideStockCrewXferMethod.Invoke(_actualCrewTransfer, null);
        }
      }

      private readonly MethodInfo _getCrewXferDelaySecMethod;
      /// <summary>
      /// This is the number of seconds delay used for the transfer in progress
      /// </summary> 
      /// <returns>
      /// Double
      /// </returns>
      public double CrewXferDelaySec
      {
        get
        {
          if (_actualCrewTransfer == null)
            _actualCrewTransfer = GetCrewTransferProcess;
          return (double)_getCrewXferDelaySecMethod.Invoke(_actualCrewTransfer, null);
        }
      }

      private readonly MethodInfo _getIsSeat2SeatXferMethod;
      /// <summary>
      /// This flag indicates if the transfer is Seat 2 seat, i.e. within the same part.
      /// </summary> 
      /// <returns>
      /// Bool
      /// </returns>
      public bool IsSeat2SeatXfer
      {
        get
        {
          if (_actualCrewTransfer == null)
            _actualCrewTransfer = GetCrewTransferProcess;
          return (bool)_getIsSeat2SeatXferMethod.Invoke(_actualCrewTransfer, null);
        }
      }

      private readonly MethodInfo _getSeat2SeatXferDelaySecMethod;
      /// <summary>
      /// This is the number of seconds used for Seat2seat transfer delays
      /// </summary> 
      /// <returns>
      /// Double
      /// </returns>
      public double Seat2SeatXferDelaySec
      {
        get
        {
          if (_actualCrewTransfer == null)
            _actualCrewTransfer = GetCrewTransferProcess;
          return (double)_getSeat2SeatXferDelaySecMethod.Invoke(_actualCrewTransfer, null);
        }
      }

      private readonly MethodInfo _getFromSeatMethod;
      /// <summary>
      /// This is the part seat where the kerbal is being moved from. In the case of parts with no internal view, it can be null.
      /// </summary> 
      /// <returns>
      /// InternalSeat
      /// </returns>
      public InternalSeat FromSeat
      {
        get
        {
          if (_actualCrewTransfer == null)
            _actualCrewTransfer = GetCrewTransferProcess;
          return (InternalSeat)_getFromSeatMethod.Invoke(_actualCrewTransfer, null);
        }
      }

      private readonly MethodInfo _getToSeatMethod;
      /// <summary>
      /// This is the part seat where the kerbal is being moved to. In the case of parts with no internal view, it can be null.
      /// </summary> 
      /// <returns>
      /// internalSeat
      /// </returns>
      public InternalSeat ToSeat
      {
        get
        {
          if (_actualCrewTransfer == null)
            _actualCrewTransfer = GetCrewTransferProcess;
          return (InternalSeat)_getToSeatMethod.Invoke(_actualCrewTransfer, null);
        }
      }

      private readonly MethodInfo _getXferVesselIdMethod;
      /// <summary>
      /// This is the vessel id for the transfer in question
      /// </summary> 
      /// <returns>
      /// Guid
      /// </returns>
      public Guid XferVesselId
      {
        get
        {
          if (_actualCrewTransfer == null)
            _actualCrewTransfer = GetCrewTransferProcess;
          return (Guid)_getXferVesselIdMethod.Invoke(_actualCrewTransfer, null);
        }
      }

      private readonly MethodInfo _getIvaDelayActiveMethod;
      /// <summary>
      /// This flag indicates the IVA delay is active. This means that the transfer has occurred (kerbals have actually moved) and we are cleaning up portraits.
      /// </summary> 
      /// <returns>
      /// Bool
      /// </returns>
      public bool IvaDelayActive
      {
        get
        {
          if (_actualCrewTransfer == null)
            _actualCrewTransfer = GetCrewTransferProcess;
          return (bool)_getIvaDelayActiveMethod.Invoke(_actualCrewTransfer, null);
        }
      }

      private readonly MethodInfo _getIvaPortraitDelayMethod;
      /// <summary>
      /// This stores the number of frames that have passed since transfer has completed. 
      /// In order for the portraits to update properly, a wait period is needed to allow for unity/ksp async callbacks to complete after crew are moved. 
      /// This is currently hard coded at 20 Frames (update cycles). I then fire an OnVesselChanged event to refresh the portraits.
      /// </summary> 
      /// <returns>
      /// int
      /// </returns>
      public int IvaPortraitDelay
      {
        get
        {
          if (_actualCrewTransfer == null)
            _actualCrewTransfer = GetCrewTransferProcess;
          return (int)_getIvaPortraitDelayMethod.Invoke(_actualCrewTransfer, null);
        }
      }

      private readonly MethodInfo _getFromPartMethod;
      /// <summary>
      /// This is the part from which the kerbal is being transferred.
      /// </summary> 
      /// <returns>
      /// Part
      /// </returns>
      public Part FromPart
      {
        get
        {
          if (_actualCrewTransfer == null)
            _actualCrewTransfer = GetCrewTransferProcess;
          return (Part)_getFromPartMethod.Invoke(_actualCrewTransfer, null);
        }
      }

      private readonly MethodInfo _getToPartMethod;
      /// <summary>
      /// This is the part to which the kerbal is being transferred. It can be the same as the source (Seat2Seat transfers).
      /// </summary> 
      /// <returns>
      /// Part
      /// </returns>
      public Part ToPart
      {
        get
        {
          if (_actualCrewTransfer == null)
            _actualCrewTransfer = GetCrewTransferProcess;
          return (Part)_getToPartMethod.Invoke(_actualCrewTransfer, null);
        }
      }

      private readonly MethodInfo _getFromCrewMemberMethod;
      /// <summary>
      /// This is the crew member being transferred.
      /// </summary> 
      /// <returns>
      /// ProtoCrewMember
      /// </returns>
      public ProtoCrewMember FromCrewMember
      {
        get
        {
          if (_actualCrewTransfer == null)
            _actualCrewTransfer = GetCrewTransferProcess;
          return (ProtoCrewMember)_getFromCrewMemberMethod.Invoke(_actualCrewTransfer, null);
        }
      }

      private readonly MethodInfo _getToCrewMemberMethod;
      /// <summary>
      /// This is the crew member that would be swapped if the target seat is occupied. can be null.
      /// </summary>
      /// <returns>
      /// protoCrewMember
      /// </returns> 
      public ProtoCrewMember ToCrewMember
      {
        get
        {
          if (_actualCrewTransfer == null)
            _actualCrewTransfer = GetCrewTransferProcess;
          return (ProtoCrewMember)_getToCrewMemberMethod.Invoke(_actualCrewTransfer, null);
        }
      }
    }

    #region Logging Stuff

    /// <summary>
    /// Some Structured logging to the debug file - ONLY RUNS WHEN DLL COMPILED IN DEBUG MODE
    /// </summary>
    /// <param name="message">Text to be printed - can be formatted as per String.format</param>
    /// <param name="strParams">Objects to feed into a String.format</param>
    [System.Diagnostics.Conditional("DEBUG")]
    internal static void LogFormatted_DebugOnly(string message, params object[] strParams)
    {
      LogFormatted(message, strParams);
    }

    /// <summary>
    /// Some Structured logging to the debug file
    /// </summary>
    /// <param name="message">Text to be printed - can be formatted as per String.format</param>
    /// <param name="strParams">Objects to feed into a String.format</param>
    internal static void LogFormatted(string message, params object[] strParams)
    {
      message = string.Format(message, strParams);
      string callingMethod = MethodBase.GetCurrentMethod().DeclaringType?.Name;
      string strMessageLine = $"{DateTime.Now},{Assembly.GetExecutingAssembly().GetName().Name}-{callingMethod},{message}";
      UnityEngine.Debug.Log(strMessageLine);
    }

    #endregion Logging Stuff
  }
}
