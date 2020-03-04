﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ShipManifest.APIClients
{
  ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
  // BELOW HERE SHOULD NOT BE EDITED - this links to the loaded DeepFreeze module without requiring a Hard Dependancy
  ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

  /// <summary>
  /// The Wrapper class to access DeepFreeze from another plugin
  /// </summary>
  public class DfWrapper
  {
    internal static Type DfType;
    internal static Type KerbalInfoType;
    internal static Type DeepFreezerType;
    internal static Type FrznCrewMbrType;
    internal static object ActualDf;

    /// <summary>
    /// This is the DeepFreeze object
    ///
    /// SET AFTER INIT
    /// </summary>
    public static DfApi DeepFreezeApi;

    /// <summary>
    /// Whether we found the DeepFreeze assembly in the loadedassemblies.
    ///
    /// SET AFTER INIT
    /// </summary>
    public static bool AssemblyExists { get { return (DfType != null); } }

    /// <summary>
    /// Whether we managed to hook the running Instance from the assembly.
    ///
    /// SET AFTER INIT
    /// </summary>
    public static bool InstanceExists { get { return (DeepFreezeApi != null); } }

    /// <summary>
    /// Whether we managed to wrap all the methods/functions from the instance.
    ///
    /// SET AFTER INIT
    /// </summary>
    private static bool _dfWrapped;

    /// <summary>
    /// Whether the object has been wrapped and the APIReady flag is set in the real DeepFreeze
    /// </summary>
    public static bool ApiReady { get { return _dfWrapped && DeepFreezeApi.IsApiReady; } }

    /// <summary>
    /// This method will set up the DeepFreeze object and wrap all the methods/functions
    /// </summary>
    /// <returns>
    /// Bool indicating success of call
    /// </returns>
    public static bool InitDfWrapper()
    {
      try
      {
        //reset the internal objects
        _dfWrapped = false;
        ActualDf = null;
        DeepFreezeApi = null;
        DfType = null;
        KerbalInfoType = null;
        DeepFreezerType = null;
        FrznCrewMbrType = null;
        LogFormatted("Attempting to Grab DeepFreeze Types...");

        //find the base type
        DfType = AssemblyLoader.loadedAssemblies
            .Select(a => a.assembly.GetExportedTypes())
            .SelectMany(t => t)
            .FirstOrDefault(t => t.FullName == "DF.DeepFreeze");

        if (DfType == null)
        {
          return false;
        }

        LogFormatted("DeepFreeze Version:{0}", DfType.Assembly.GetName().Version.ToString());

        //now the KerbalInfo Type
        KerbalInfoType = AssemblyLoader.loadedAssemblies
            .Select(a => a.assembly.GetExportedTypes())
            .SelectMany(t => t)
            .FirstOrDefault(t => t.FullName == "DF.KerbalInfo");

        if (KerbalInfoType == null)
        {
          return false;
        }

        //now the DeepFreezer (partmodule) Type
        DeepFreezerType = AssemblyLoader.loadedAssemblies
            .Select(a => a.assembly.GetExportedTypes())
            .SelectMany(t => t)
            .FirstOrDefault(t => t.FullName == "DF.DeepFreezer");

        if (DeepFreezerType == null)
        {
          return false;
        }

        //now the FrznCrewMbr Type
        FrznCrewMbrType = AssemblyLoader.loadedAssemblies
            .Select(a => a.assembly.GetExportedTypes())
            .SelectMany(t => t)
            .FirstOrDefault(t => t.FullName == "DF.FrznCrewMbr");

        if (FrznCrewMbrType == null)
        {
          return false;
        }

        //now grab the running instance
        LogFormatted("Got Assembly Types, grabbing Instance");
        try
        {
          ActualDf = DfType.GetField("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
        }
        catch (Exception)
        {
          LogFormatted("No Instance found - most likely you have an old DeepFreeze installed");
          return false;
        }
        if (ActualDf == null)
        {
          LogFormatted("Failed grabbing Instance");
          return false;
        }

        //If we get this far we can set up the local object and its methods/functions
        LogFormatted("Got Instance, Creating Wrapper Objects");
        DeepFreezeApi = new DfApi(ActualDf);
        _dfWrapped = true;
        return true;
      }
      catch (Exception ex)
      {
        LogFormatted("Unable to setup InitDFWrapper Reflection");
        LogFormatted("Exception: {0}", ex);
        _dfWrapped = false;
        return false;
      }
    }

    /// <summary>
    /// The API Class that is an analogue of the real DeepFreeze. This lets you access all the API-able properties and Methods of the DeepFreeze
    /// </summary>
    public class DfApi
    {
      internal DfApi(object a)
      {
        try
        {
          //store the actual object
          _actualDfapi = a;

          //these sections get and store the reflection info and actual objects where required. Later in the properties we then read the values from the actual objects
          //for events we also add a handler
          //Object tstfrozenkerbals = DFType.GetField("FrozenKerbals", BindingFlags.Public | BindingFlags.Static).GetValue(null);

          LogFormatted("Getting APIReady Object");
          _apiReadyField = DfType.GetField("APIReady", BindingFlags.Public | BindingFlags.Static);
          LogFormatted($"Success: {_apiReadyField != null}");

          LogFormatted("Getting FrozenKerbals Object");
          _frozenKerbalsMethod = DfType.GetMethod("get_FrozenKerbals", BindingFlags.Public | BindingFlags.Instance);
          _actualFrozenKerbals = _frozenKerbalsMethod.Invoke(_actualDfapi, null);
          LogFormatted($"Success: {_actualFrozenKerbals != null}");
        }
        catch (Exception ex)
        {
          LogFormatted("Unable to Instantiate DFAPI object using Reflection");
          LogFormatted("Exception: {0}", ex);
        }
      }

      private object _actualDfapi;

      private FieldInfo _apiReadyField;
      /// <summary>
      /// Whether the APIReady flag is set in the real KAC
      /// </summary>
      /// <returns>
      /// Bool Indicating if DeepFreeze is ready for API calls
      /// </returns>
      public bool IsApiReady
      {
        get
        {
          if (_apiReadyField == null)
            return false;

          return (bool)_apiReadyField.GetValue(null);
        }
      }

      #region Frozenkerbals

      private object _actualFrozenKerbals;
      private MethodInfo _frozenKerbalsMethod;

      /// <summary>
      /// The dictionary of Frozen Kerbals that are currently active in game
      /// </summary>
      /// <returns>
      /// Dictionary&lt;string, KerbalInfo&gt; of Frozen Kerbals
      /// </returns>
      internal Dictionary<string, KerbalInfo> FrozenKerbals
      {
        get
        {
          Dictionary<string, KerbalInfo> returnvalue = new Dictionary<string, KerbalInfo>();
          if (_frozenKerbalsMethod == null)
          {
            LogFormatted("Error getting FrozenKerbals - Reflection Method is Null");
            return returnvalue;
          }
          object actualDFtest = DfType.GetField("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
          FieldInfo gamesettingsfield = DfType.GetField("DFgameSettings", BindingFlags.Instance | BindingFlags.NonPublic);
          object gamesettings;
          if (gamesettingsfield != null)
            gamesettings = gamesettingsfield.GetValue(_actualDfapi);
          _actualFrozenKerbals = null;
          _actualFrozenKerbals = _frozenKerbalsMethod.Invoke(_actualDfapi, null);
          returnvalue = ExtractFrozenKerbalDict(_actualFrozenKerbals);
          return returnvalue;
        }
      }

      /// <summary>
      /// This converts the actualFrozenKerbals actual object to a new dictionary for consumption
      /// </summary>
      /// <param name="actualFrozenKerbals"></param>
      /// <returns>
      /// Dictionary&lt;string, KerbalInfo&gt; of Frozen Kerbals
      /// </returns>
      private Dictionary<string, KerbalInfo> ExtractFrozenKerbalDict(object actualFrozenKerbals)
      {
        Dictionary<string, KerbalInfo> dictToReturn = new Dictionary<string, KerbalInfo>();
        try
        {
          // SM - Replaced foreach with enumerator for performance (foreach = bad in Unity)
          IEnumerator frnKbls = ((IDictionary) actualFrozenKerbals).GetEnumerator();
          while (frnKbls.MoveNext())
          {
            if (frnKbls.Current == null) continue;
            Type typeitem = frnKbls.Current.GetType();
            PropertyInfo[] itemprops = typeitem.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            string itemkey = (string)itemprops[0].GetValue(frnKbls.Current, null);
            object itemvalue = itemprops[1].GetValue(frnKbls.Current, null);
            KerbalInfo itemkerbalinfo = new KerbalInfo(itemvalue);
            dictToReturn[itemkey] = itemkerbalinfo;
          }
        }
        catch (Exception ex)
        {
          LogFormatted("Unable to extract FrozenKerbals Dictionary: {0}", ex.Message);
        }
        return dictToReturn;
      }

      #endregion Frozenkerbals
    }

    #region DeepFreezerPart

    /// <summary>
    /// The Class that is an analogue of the real DeepFreezer PartModule. This lets you access all the API-able properties and Methods of the DeepFreezer
    /// </summary>
    public class DeepFreezer
    {
      internal DeepFreezer(object a)
      {
        _actualDeepFreezer = a;
        //Fields available from Freezer part

        _freezerSizeMethod = DeepFreezerType.GetMethod("get_DFIFreezerSize", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        FreezerSize = GetFreezerSize;
        _totalFrozenMethod = DeepFreezerType.GetMethod("get_DFITotalFrozen", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        TotalFrozen = GetTotalFrozen;
        _freezerSpaceMethod = DeepFreezerType.GetMethod("get_DFIFreezerSpace", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        FreezerSpace = GetFreezerSpace;
        _partFullMethod = DeepFreezerType.GetMethod("get_DFIPartFull", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        PartFull = GetPartFull;
        _isFreezeActiveMethod = DeepFreezerType.GetMethod("get_DFIIsFreezeActive", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        IsFreezeActive = GetIsFreezeActive;
        _isThawActiveMethod = DeepFreezerType.GetMethod("get_DFIIsThawActive", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        IsThawActive = GetIsThawActive;
        _freezerOutofEcMethod = DeepFreezerType.GetMethod("get_DFIFreezerOutofEC", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        FreezerOutofEc = GetFreezerOutofEc;
        _frzrTmpMethod = DeepFreezerType.GetMethod("get_DFIFrzrTmp", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        FrzrTmp = GetFrzrTmp;
        _storedCrewListMethod = DeepFreezerType.GetMethod("get_DFIStoredCrewList", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        _actualStoredCrewList = _storedCrewListMethod.Invoke(_actualDeepFreezer, null);

        //Methods
        //LogFormatted("Getting beginFreezeKerbalMethod Method");
        _beginFreezeKerbalMethod = DeepFreezerType.GetMethod("beginFreezeKerbal", BindingFlags.Public | BindingFlags.Instance);
        //LogFormatted_DebugOnly($"Success: {beginFreezeKerbalMethod != null}");

        //LogFormatted("Getting beginThawKerbalMethod Method");
        _beginThawKerbalMethod = DeepFreezerType.GetMethod("beginThawKerbal", BindingFlags.Public | BindingFlags.Instance);
        //LogFormatted_DebugOnly($"Success: {beginThawKerbalMethod != null}");
      }

      private object _actualDeepFreezer;

      #region DeepFreezerFieldMethods

      /// <summary>
      /// The number of cryopods in this DeepFreezer
      /// </summary>
      public int FreezerSize;

      private MethodInfo _freezerSizeMethod;

      private int GetFreezerSize
      {
        get { return (int)_freezerSizeMethod.Invoke(_actualDeepFreezer, null); }
      }

      /// <summary>
      /// The number of currently frozen Kerbals in this DeepFreezer
      /// </summary>
      public int TotalFrozen;

      private MethodInfo _totalFrozenMethod;

      private int GetTotalFrozen
      {
        get { return (int)_totalFrozenMethod.Invoke(_actualDeepFreezer, null); }
      }

      /// <summary>
      /// The number of empty cryopods in this DeepFreezer
      /// </summary>
      public int FreezerSpace;

      private MethodInfo _freezerSpaceMethod;

      private int GetFreezerSpace
      {
        get { return (int)_freezerSpaceMethod.Invoke(_actualDeepFreezer, null); }
      }

      /// <summary>
      /// True if all the cryopods are taken in this DeepFreezer (includes, frozen and thawed kerbals).
      /// </summary>
      public bool PartFull;

      private MethodInfo _partFullMethod;

      private bool GetPartFull
      {
        get { return (bool)_partFullMethod.Invoke(_actualDeepFreezer, null); }
      }

      /// <summary>
      /// True if a Freeze kerbal event is currently active in this DeepFreezer
      /// </summary>
      public bool IsFreezeActive;

      private MethodInfo _isFreezeActiveMethod;

      private bool GetIsFreezeActive
      {
        get { return (bool)_isFreezeActiveMethod.Invoke(_actualDeepFreezer, null); }
      }

      /// <summary>
      /// True if a Thaw kerbal event is currently active in this DeepFreezer
      /// </summary>
      public bool IsThawActive;

      private MethodInfo _isThawActiveMethod;

      private bool GetIsThawActive
      {
        get { return (bool)_isThawActiveMethod.Invoke(_actualDeepFreezer, null); }
      }

      /// <summary>
      /// True if this DeepFreezer is currently out of Electric Charge
      /// </summary>
      public bool FreezerOutofEc;

      private MethodInfo _freezerOutofEcMethod;

      private bool GetFreezerOutofEc
      {
        get { return (bool)_freezerOutofEcMethod.Invoke(_actualDeepFreezer, null); }
      }

      /// <summary>
      /// The current freezer temperature status of this DeepFreezer
      /// </summary>
      public FrzrTmpStatus FrzrTmp;

      private MethodInfo _frzrTmpMethod;

      private FrzrTmpStatus GetFrzrTmp
      {
        get { return (FrzrTmpStatus)_frzrTmpMethod.Invoke(_actualDeepFreezer, null); }
      }

      private object _actualStoredCrewList;
      private MethodInfo _storedCrewListMethod;

      /// <summary>
      /// a List&lt;FrznCrewMbr&gt; of all Frozen Crew in this DeepFreezer
      /// </summary>
      public FrznCrewList StoredCrewList
      {
        get { return ExtractStoredCrewList(_actualStoredCrewList); }
      }

      /// <summary>
      /// This converts the StoredCrewList actual object to a new List for consumption
      /// </summary>
      /// <param name="actualStoredCrewList"></param>
      /// <returns></returns>
      private FrznCrewList ExtractStoredCrewList(object actualStoredCrewList)
      {
        FrznCrewList listToReturn = new FrznCrewList();
        try
        {
          //iterate each "value" in the dictionary
          // SM modification for performance (foreach = bad in unity)
          IEnumerator crewList = ((IList)actualStoredCrewList).GetEnumerator();
          while (crewList.MoveNext())
          {
            FrznCrewMbr r1 = new FrznCrewMbr(crewList.Current);
            listToReturn.Add(r1);
          }
        }
        catch (Exception ex)
        {
          LogFormatted("Arrggg: {0}", ex.Message);
          //throw ex;
          //
        }
        return listToReturn;
      }

      #endregion DeepFreezerFieldMethods

      #region DeepFreezerMethods

      private MethodInfo _beginFreezeKerbalMethod;

      /// <summary>
      /// Begin the Freezing of a Kerbal
      /// </summary>
      /// <param name="crewMember">ProtoCrewMember that you want frozen</param>
      /// <returns>Bool indicating success of call</returns>
      public bool BeginFreezeKerbal(ProtoCrewMember crewMember)
      {
        try
        {
          _beginFreezeKerbalMethod.Invoke(_actualDeepFreezer, new object[] { crewMember });
          return true;
        }
        catch (Exception ex)
        {
          LogFormatted("Arrggg: {0}", ex.Message);
          return false;
        }
      }

      private MethodInfo _beginThawKerbalMethod;

      /// <summary>
      /// Begin the Thawing of a Kerbal
      /// </summary>
      /// <param name="frozenkerbal">string containing the name of the kerbal you want thawed</param>
      /// <returns>Bool indicating success of call</returns>
      public bool BeginThawKerbal(string frozenkerbal)
      {
        try
        {
          _beginThawKerbalMethod.Invoke(_actualDeepFreezer, new object[] { frozenkerbal });
          return true;
        }
        catch (Exception ex)
        {
          LogFormatted("Arrggg: {0}", ex.Message);
          return false;
        }
      }

      #endregion DeepFreezerMethods
    }

    public enum FrzrTmpStatus
    {
      Ok = 0,
      Warn = 1,
      Red = 2,
    }

    /// <summary>
    /// The Class that is an analogue of the real FrznCrewMbr as part of the StoredCrewList field in the DeepFreezer PartModule.
    /// </summary>
    public class FrznCrewMbr
    {
      internal FrznCrewMbr(object a)
      {
        _actualFrznCrewMbr = a;
        _crewNameMethod = FrznCrewMbrType.GetMethod("get_CrewName", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        CrewName = GetCrewName;
        _seatIdxMethod = FrznCrewMbrType.GetMethod("get_SeatIdx", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        SeatIdx = GetSeatIdx;
        _vesselIdMethod = FrznCrewMbrType.GetMethod("get_VesselID", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        VesselId = GetVesselId;
        _vesselNameMethod = FrznCrewMbrType.GetMethod("get_VesselName", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        VesselName = GetVesselName;
      }

      private object _actualFrznCrewMbr;


      /// <summary>
      /// Crew Members Name
      /// </summary>
      public string CrewName;

      private MethodInfo _crewNameMethod;

      private string GetCrewName
      {
        get { return (string)_crewNameMethod.Invoke(_actualFrznCrewMbr, null); }
      }

      /// <summary>
      /// Seat Index for Crew member
      /// </summary>
      public int SeatIdx;

      private MethodInfo _seatIdxMethod;

      private int GetSeatIdx
      {
        get { return (int)_seatIdxMethod.Invoke(_actualFrznCrewMbr, null); }
      }

      /// <summary>
      /// Vessel ID
      /// </summary>
      public Guid VesselId;

      private MethodInfo _vesselIdMethod;

      private Guid GetVesselId
      {
        get { return (Guid)_vesselIdMethod.Invoke(_actualFrznCrewMbr, null); }
      }

      /// <summary>
      /// Vessel Name
      /// </summary>
      public string VesselName;

      private MethodInfo _vesselNameMethod;

      private string GetVesselName
      {
        get { return (string)_vesselNameMethod.Invoke(_actualFrznCrewMbr, null); }
      }
    }

    public class FrznCrewList : List<FrznCrewMbr>
    {
    }

    #endregion DeepFreezerPart

    /// <summary>
    /// The Value Class of the FrozenCrewList Dictionary that is an analogue of the real FrozenKerbals Dictionary in the DeepFreezer Class.
    /// </summary>
    public class KerbalInfo
    {
      internal KerbalInfo(object a)
      {
        _actualFrozenKerbalInfo = a;
        _lastUpdateField = KerbalInfoType.GetField("lastUpdate");
        _statusField = KerbalInfoType.GetField("status");
        _typeField = KerbalInfoType.GetField("type");
        _vesselIdField = KerbalInfoType.GetField("vesselID");
        _vesselNameField = KerbalInfoType.GetField("vesselName");
        _partIdField = KerbalInfoType.GetField("partID");
        _seatIdxField = KerbalInfoType.GetField("seatIdx");
        _seatNameField = KerbalInfoType.GetField("seatName");
        _experienceTraitNameField = KerbalInfoType.GetField("experienceTraitName");
      }

      private object _actualFrozenKerbalInfo;

      private FieldInfo _lastUpdateField;

      /// <summary>
      /// last time the FrozenKerbalInfo was updated
      /// </summary>
      public double LastUpdate
      {
        get { return (double)_lastUpdateField.GetValue(_actualFrozenKerbalInfo); }
      }

      private FieldInfo _statusField;

      /// <summary>
      /// RosterStatus of the frozen kerbal
      /// </summary>
      public ProtoCrewMember.RosterStatus Status
      {
        get { return (ProtoCrewMember.RosterStatus)_statusField.GetValue(_actualFrozenKerbalInfo); }
      }

      private FieldInfo _typeField;

      /// <summary>
      /// KerbalType of the frozen kerbal
      /// </summary>
      public ProtoCrewMember.KerbalType Type
      {
        get { return (ProtoCrewMember.KerbalType)_typeField.GetValue(_actualFrozenKerbalInfo); }
      }

      private FieldInfo _vesselIdField;

      /// <summary>
      /// Guid of the vessel the frozen kerbal is aboard
      /// </summary>
      public Guid VesselId
      {
        get { return (Guid)_vesselIdField.GetValue(_actualFrozenKerbalInfo); }
      }

      private FieldInfo _vesselNameField;

      /// <summary>
      /// Name of the vessel the frozen kerbal is aboard
      /// </summary>
      public string VesselName
      {
        get { return (string)_vesselNameField.GetValue(_actualFrozenKerbalInfo); }
      }

      private FieldInfo _partIdField;

      /// <summary>
      /// partID of the vessel part the frozen kerbal is aboard
      /// </summary>
      public uint PartId
      {
        get { return (uint)_partIdField.GetValue(_actualFrozenKerbalInfo); }
      }

      private FieldInfo _seatIdxField;

      /// <summary>
      /// seat index that the frozen kerbal is in
      /// </summary>
      public int SeatIdx
      {
        get { return (int)_seatIdxField.GetValue(_actualFrozenKerbalInfo); }
      }

      private FieldInfo _seatNameField;

      /// <summary>
      /// seat name that the frozen kerbal is in
      /// </summary>
      public string SeatName
      {
        get { return (string)_seatNameField.GetValue(_actualFrozenKerbalInfo); }
      }

      private FieldInfo _experienceTraitNameField;

      /// <summary>
      /// name of the experience trait for the frozen kerbal
      /// </summary>
      public string ExperienceTraitName
      {
        get { return (string)_experienceTraitNameField.GetValue(_actualFrozenKerbalInfo); }
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
      string strMessageLine = $"{DateTime.Now},{Assembly.GetExecutingAssembly().GetName().Name}-{MethodBase.GetCurrentMethod()?.DeclaringType?.Name},{message}";
      UnityEngine.Debug.Log(strMessageLine);
    }

    #endregion Logging Stuff
  }
}