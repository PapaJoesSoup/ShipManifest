using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ConnectedLivingSpace;
using KSP.UI.Screens;
using KSP.UI.Screens.Flight;
using KSP.UI.Screens.Flight.Dialogs;
using ShipManifest.APIClients;
using ShipManifest.InternalObjects;
using ShipManifest.Process;
using ShipManifest.Windows;
using UnityEngine;

namespace ShipManifest
{
  [KSPAddon(KSPAddon.Startup.EveryScene, false)]
  // ReSharper disable once InconsistentNaming
  public class SMAddon : MonoBehaviour
  {
    // Object Scope:  Current Unity/KSP Scene.  Object will be destroyed and recreated when scene changes!

    #region Static Properties

    // Game object that keeps us running
    // internal static GameObject SmInstance;

    internal static bool SceneChangeInitDfWrapper;
    
    // current vessel's controller instance
    internal static SMVessel SmVessel;
    internal static ICLSAddon ClsAddon;

    internal static string TextureFolder = "ShipManifest/Textures/";
    internal static string SaveMessage = string.Empty;

    [KSPField(isPersistant = true)] internal static double Elapsed;

    // Resource xfer vars
    // This is still very entrenched.   Need to look at implications for conversion to instanced.
    internal static TransferPump.TypePump ActivePumpType = TransferPump.TypePump.SourceToTarget;

    // Toolbar Integration.
    private static IButton _smButtonBlizzy;
    private static IButton _smSettingsBlizzy;
    private static IButton _smRosterBlizzy;
    private static ApplicationLauncherButton _smButtonStock;
    private static ApplicationLauncherButton _smSettingsStock;
    private static ApplicationLauncherButton _smRosterStock;

    // Repeating error latch
    internal static bool FrameErrTripped;

    // SM UI toggle
    internal static bool ShowUi = true;

    #endregion

    // Makes instance available via reflection
    public static SMAddon Instance;

    public SMAddon()
    {
      Instance = this;
    }

    #region Public Instance Properties

    public bool PumpProcessOn
    {
      get { return TransferPump.PumpProcessOn; }
    }

    public bool CrewProcessOn
    {
      get
      {
        return SmVessel != null && SmVessel.TransferCrewObj.CrewXferActive;
      }
    }

    public TransferCrew CrewTransferProcess
    {
      get { return SmVessel.TransferCrewObj; }
    }

    public List<TransferPump> PumpsInProgress
    {
      get { return (from pump in SmVessel.TransferPumps select pump).ToList(); }
    }

    #endregion

    #region Event handlers

    private static void DummyHandler()
    {
    }

    // Addon state event handlers
    internal void Awake()
    {
      try
      {
        if (HighLogic.LoadedScene != GameScenes.FLIGHT && HighLogic.LoadedScene != GameScenes.SPACECENTER) return;
        DontDestroyOnLoad(this);
        SMSettings.LoadSettings();

        if (SMSettings.AutoSave)
          InvokeRepeating("RunSave", SMSettings.SaveIntervalSec, SMSettings.SaveIntervalSec);

        if (SMSettings.EnableBlizzyToolbar)
        {
          // Let't try to use Blizzy's toolbar
          if (ActivateBlizzyToolBar()) return;
          // We failed to activate the toolbar, so revert to stock
          GameEvents.onGUIApplicationLauncherReady.Add(OnGuiAppLauncherReady);
          GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGuiAppLauncherDestroyed);
        }
        else
        {
          // Use stock Toolbar
          GameEvents.onGUIApplicationLauncherReady.Add(OnGuiAppLauncherReady);
          GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGuiAppLauncherDestroyed);
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage("Error in:  SMAddon.Awake.  Error:  " + ex, Utilities.LogType.Error, true);
      }
    }

    internal void Start()
    {
      try
      {
        // Reset frame error latch if set
        if (FrameErrTripped) 
          FrameErrTripped = false;

        if (WindowRoster.ResetRosterSize)
          WindowRoster.Position.height = SMSettings.UseUnityStyle ? 330 : 350;

        if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
        {
          if (GetClsAddon())
          {
            SMSettings.ClsInstalled = true;
          }
          else
          {
            SMSettings.EnableCls = false;
            SMSettings.ClsInstalled = false;
          }
          // reset any hacked kerbal names in game save from old version of SM/KSP
          if (SMSettings.EnableChangeProfession)
            WindowRoster.ResetKerbalNames();

          SMSettings.SaveSettings();
          //RunSave();
        }

        if (HighLogic.LoadedScene != GameScenes.FLIGHT) return;
        // Instantiate Event handlers
        GameEvents.onCrewTransferPartListCreated.Add(OnCrewTransferPartListCreated);
        GameEvents.onCrewTransferSelected.Add(OnCrewTransferSelected);
        GameEvents.onGameSceneLoadRequested.Add(OnGameSceneLoadRequested);
        GameEvents.onVesselChange.Add(OnVesselChange);
        GameEvents.onVesselWasModified.Add(OnVesselWasModified);
        GameEvents.onVesselChange.Add(OnVesselChange);
        GameEvents.onVesselLoaded.Add(OnVesselLoaded);
        GameEvents.onShowUI.Add(OnShowUi);
        GameEvents.onHideUI.Add(OnHideUi);

        // get the current Vessel data
        SmVessel = SMVessel.GetInstance(FlightGlobals.ActiveVessel);

        // Is CLS installed and enabled?
        if (GetClsAddon())
        {
          SMSettings.ClsInstalled = true;
          SMSettings.SaveSettings();
          UpdateClsSpaces();
        }
        else
        {
          SMSettings.EnableCls = false;
          SMSettings.ClsInstalled = false;
          SMSettings.SaveSettings();
        }

        // Support for DeepFreeze
        //Trigger Update to check and initialize the DeepFreeze Wrapper API
        SceneChangeInitDfWrapper = true;

        // Load sounds for transfers.
        SMSound.LoadSounds();
      }
      catch (Exception ex)
      {
        Utilities.LogMessage("Error in:  SMAddon.Start.  " + ex, Utilities.LogType.Error, true);
      }
    }

    internal void OnDestroy()
    {
      //Debug.Log("[ShipManifest]:  SmAddon.OnDestroy");
      try
      {
        if (HighLogic.LoadedSceneIsFlight)
          WindowControl.ShowWindow =
            WindowManifest.ShowWindow =
              WindowTransfer.ShowWindow = WindowRoster.ShowWindow = WindowSettings.ShowWindow = false;
        if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
          WindowRoster.ShowWindow = WindowSettings.ShowWindow = false;

        if (SMSettings.Loaded)
          SMSettings.SaveSettings();

        GameEvents.onCrewTransferPartListCreated.Remove(OnCrewTransferPartListCreated);
        GameEvents.onCrewTransferSelected.Remove(OnCrewTransferSelected);
        GameEvents.onGameSceneLoadRequested.Remove(OnGameSceneLoadRequested);
        GameEvents.onVesselWasModified.Remove(OnVesselWasModified);
        GameEvents.onVesselChange.Remove(OnVesselChange);
        GameEvents.onVesselWasModified.Remove(OnVesselWasModified);
        GameEvents.onVesselChange.Remove(OnVesselChange);
        GameEvents.onVesselLoaded.Remove(OnVesselLoaded);
        GameEvents.onHideUI.Remove(OnHideUi);
        GameEvents.onShowUI.Remove(OnShowUi);

        CancelInvoke("RunSave");

        // Handle Toolbars
        if (_smRosterBlizzy == null && _smSettingsBlizzy == null && _smButtonBlizzy == null)
        {
          if (_smButtonStock != null)
          {
            ApplicationLauncher.Instance.RemoveModApplication(_smButtonStock);
            _smButtonStock = null;
          }
          if (_smSettingsStock != null)
          {
            ApplicationLauncher.Instance.RemoveModApplication(_smSettingsStock);
            _smSettingsStock = null;
          }
          if (_smRosterStock != null)
          {
            ApplicationLauncher.Instance.RemoveModApplication(_smRosterStock);
            _smRosterStock = null;
          }
          if (_smButtonStock == null && _smSettingsStock == null && _smRosterStock == null)
          {
            // Remove the stock toolbar button launcher handler
            GameEvents.onGUIApplicationLauncherReady.Remove(OnGuiAppLauncherReady);
          }
        }
        else
        {
          if (_smButtonBlizzy != null)
            _smButtonBlizzy.Destroy();
          if (_smRosterBlizzy != null)
            _smRosterBlizzy.Destroy();
          if (_smSettingsBlizzy != null)
            _smSettingsBlizzy.Destroy();
        }
        //Reset Roster Window data
        WindowRoster.OnCreate = false;
        WindowRoster.SelectedKerbal = null;
        WindowRoster.ToolTip = "";
        //WindowRoster.ShowWindow = false;
      }
      catch (Exception ex)
      {
        Utilities.LogMessage("Error in:  SMAddon.OnDestroy.  " + ex, Utilities.LogType.Error, true);
      }
    }

    // ReSharper disable once InconsistentNaming
    internal void OnGUI()
    {
      try
      {
        GUI.skin = SMSettings.UseUnityStyle ? null : HighLogic.Skin;

        SMStyle.SetupGuiStyles();
        Display();
        SMToolTips.ShowToolTips();
      }
      catch (Exception ex)
      {
        Utilities.LogMessage("Error in:  SMAddon.OnGUI.  " + ex, Utilities.LogType.Error, true);
      }
    }

    internal void Update()
    {
      try
      {
        CheckForToolbarTypeToggle();

        if (SceneChangeInitDfWrapper && Time.timeSinceLevelLoad > 3f && !InstalledMods.IsDfApiReady)
        {
          DFWrapper.InitDFWrapper();
          SceneChangeInitDfWrapper = false;
        }

        if (HighLogic.LoadedScene != GameScenes.FLIGHT) return;
        if (FlightGlobals.fetch == null || FlightGlobals.ActiveVessel == null) return;

        if (SmVessel.SelectedResources.Count > 0) SMHighlighter.Update_Highlighter();

        // Realism Mode Resource transfer operation (real time)
        // PumpActive is flagged in the Resource Controller
        if (TransferPump.PumpProcessOn)
        {
          if (TransferPump.PumpHalfCycleLatch)
          {
            if ((from pump in SmVessel.TransferPumps where pump.IsPumpOn select pump).Any())
              TransferPump.ProcessActivePumps();
            else
            {
              TransferPump.PumpProcessOn = false;
              SmVessel.TransferPumps.Clear();
            }
          }
          TransferPump.PumpHalfCycleLatch = !TransferPump.PumpHalfCycleLatch;
        }

        // Realism Mode Crew transfer operation (real time)
        if (SmVessel.TransferCrewObj.CrewXferActive)
          SmVessel.TransferCrewObj.CrewTransferProcess();
        else if (SmVessel.TransferCrewObj.IsStockXfer)
        {
          SmVessel.TransferCrewObj.CrewTransferBegin(SmVessel.TransferCrewObj.FromCrewMember,
            SmVessel.TransferCrewObj.FromPart, SmVessel.TransferCrewObj.ToPart);
        }

        if (!SMSettings.EnableStockCrewXfer || !TransferCrew.FireSourceXferEvent) return;
        // Now let's deal with third party mod support...
        TransferCrew.FireSourceXferEvent = false;
        GameEvents.onCrewTransferred.Fire(TransferCrew.SourceAction);

        //If a swap, we need to handle that too...
        if (!TransferCrew.FireTargetXferEvent) return;
        TransferCrew.FireTargetXferEvent = false;
        GameEvents.onCrewTransferred.Fire(TransferCrew.TargetAction);
      }
      catch (Exception ex)
      {
        if (!FrameErrTripped)
        {
          Utilities.LogMessage(
            string.Format(" in SMAddon.Update (repeating error).  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace),
            Utilities.LogType.Error, true);
          FrameErrTripped = true;
        }
      }
    }

    // save settings on scene changes
    private void OnGameSceneLoadRequested(GameScenes requestedScene)
    {
      //Debug.Log("[ShipManifest]:  SMAddon.OnGameSceneLoadRequested");
      SMSettings.SaveSettings();
      if (InstalledMods.IsDfInstalled)
        SceneChangeInitDfWrapper = true;
    }

    // SM UI toggle handlers
    private void OnShowUi()
    {
      //Debug.Log("[ShipManifest]:  SMAddon.OnShowUI");
      ShowUi = true;
    }

    private void OnHideUi()
    {
      //Debug.Log("[ShipManifest]:  SMAddon.OnHideUI");
      ShowUi = false;
    }

    // Crew Event handlers
    internal void OnCrewTransferPartListCreated(GameEvents.FromToAction<List<Part>, List<Part>> eventData)
    {
      // We can skip this event if a stock CrewTransfer is enabled, Override is off & no SM  Crew Transfers are active
      if (SMSettings.EnableStockCrewXfer && !SMSettings.OverrideStockCrewXfer && TransferCrew.CrewXferState == TransferCrew.XferState.Off) return;

      // If override is off, then ignore.
      if (!SMSettings.OverrideStockCrewXfer) return;

      // How can I tell if the parts are in the same space?... I need a starting point!  What part initiated the event?
      Part sourcePart = null;

      // Get the Dialog and find the source part.
      CrewHatchDialog dialog = Resources.FindObjectsOfTypeAll<CrewHatchDialog>().FirstOrDefault();
      if (dialog?.Part == null) return;
      sourcePart = dialog.Part;

      //Let's manhandle the lists
      List<Part> fullList = new List<Part>();
      List<Part>.Enumerator fromList = eventData.from.GetEnumerator();
      while (fromList.MoveNext())
      {
        if (fromList.Current == null) continue;
        //Check for DeepFreezer full. if full, abort handling Xfer.
        if (InstalledMods.IsDfInstalled && InstalledMods.IsDfApiReady && fromList.Current.Modules.Contains("DeepFreezer"))
        {
          if (new DFWrapper.DeepFreezer(fromList.Current.Modules["DeepFreezer"]).FreezerSpace == 0)
          {
            fullList.Add(fromList.Current);
          }
        }
        // If CLS is enabled and parts are not in same space.
        if (!SMConditions.IsClsInSameSpace(sourcePart, fromList.Current))
        {
          fullList.Add(fromList.Current);
        };
      }
      if (fullList.Count <= 0) return;
      CrewTransfer.fullMessage = "<color=orange>SM - This module is either full or internally unreachable.</color>";
      List<Part>.Enumerator removeList = fullList.GetEnumerator();
      while (removeList.MoveNext())
      {
        eventData.from.Remove(removeList.Current);
      }
      eventData.to.AddRange(fullList);
    }

    /// <summary>
    /// OnCrewTransferSelected (GameEvent Handler)
    /// This method captures a Stock Crew Transfer event just prior to the actual crew move.
    /// </summary>
    /// <param name="crewTransferData"></param>
    internal void OnCrewTransferSelected(CrewTransfer.CrewTransferData crewTransferData)
    {
      // We can skip this event if a stock CrewTransfer is enabled, Override is off & no SM  Crew Transfers are active
      if (SMSettings.EnableStockCrewXfer && !SMSettings.OverrideStockCrewXfer && TransferCrew.CrewXferState == TransferCrew.XferState.Off) return;

      // Disable the stock Transfer action if SM dictates.
      if (!SMSettings.EnableStockCrewXfer || TransferCrew.CrewXferState != TransferCrew.XferState.Off)
      {
        if (!SMSettings.EnableStockCrewXfer) DisplayScreenMsg("SM Has Disabled Stock Crew Transfers. (Check your SM settings)");
        if (TransferCrew.CrewXferState != TransferCrew.XferState.Off) DisplayScreenMsg("Stock Crew Transfer Disabled.  SM Crew Transfer in Progress.");
        crewTransferData.canTransfer = false;
        return;
      }

      // If override is off, then ignore.
      if (!SMSettings.OverrideStockCrewXfer) return;

      //Check for DeepFreezer full. if full, abort handling Xfer.
      if (InstalledMods.IsDfInstalled && InstalledMods.IsDfApiReady && crewTransferData.destPart.Modules.Contains("DeepFreezer"))
      {
        if (new DFWrapper.DeepFreezer(crewTransferData.destPart.Modules["DeepFreezer"]).FreezerSpace == 0)
        {
          DisplayScreenMsg("Destination part is a Freezer and it is full. Aborting Transfer.");
          crewTransferData.canTransfer = false;
          return;
        }
      }

      // CLS is enabled and parts are not in same space.  Abort Transfer.
      if (!SMConditions.IsClsInSameSpace(crewTransferData.sourcePart, crewTransferData.destPart))
      {
         DisplayScreenMsg("CLS is Enabled in SM. Parts are not in Same Space. Aborting Transfer.");
        crewTransferData.canTransfer = false;
        return;
      };

      if (!SMSettings.RealismMode) return;
      // OK, ealism and override are on lets manage the Crew transfer
      // store data from event.
      DisplayScreenMsg("SM is overriding Stock Transfers.  SM based Crew Transfer initiating...");
      SmVessel.TransferCrewObj.FromPart = crewTransferData.sourcePart;
      SmVessel.TransferCrewObj.ToPart = crewTransferData.destPart;
      SmVessel.TransferCrewObj.FromCrewMember = crewTransferData.crewMember;
      SmVessel.TransferCrewObj.IsStockXfer = true;
      crewTransferData.canTransfer = false;
    }

    internal void OnVesselWasModified(Vessel modVessel)
    {
      try
      {
        SMHighlighter.ClearResourceHighlighting(SmVessel.SelectedResourcesParts);
        UpdateSMcontroller(modVessel);
      }
      catch (Exception ex)
      {
        Utilities.LogMessage("Error in:  SMAddon.OnVesselWasModified.  " + ex, Utilities.LogType.Error, true);
      }
    }

    internal void OnVesselChange(Vessel newVessel)
    {
      try
      {
        SMHighlighter.ClearResourceHighlighting(SmVessel.SelectedResourcesParts);
        UpdateSMcontroller(newVessel);
        SMSettings.SetStockCrewTransferState();
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(
          string.Format(" in SMAddon.OnVesselChange.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), Utilities.LogType.Error,
          true);
      }
    }

    private void OnVesselLoaded(Vessel data)
    {
      try
      {
        if (data.Equals(FlightGlobals.ActiveVessel) && data != SmVessel.Vessel)
        {
          SMHighlighter.ClearResourceHighlighting(SmVessel.SelectedResourcesParts);
          UpdateSMcontroller(data);
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage("Error in:  SMAddon.OnVesselLoaded.  " + ex, Utilities.LogType.Error, true);
      }
    }

    // Stock vs Blizzy Toolbar switch handler
    private void CheckForToolbarTypeToggle()
    {
      if (SMSettings.EnableBlizzyToolbar && !SMSettings.PrevEnableBlizzyToolbar)
      {
        // Let't try to use Blizzy's toolbar
        if (!ActivateBlizzyToolBar())
        {
          // We failed to activate the toolbar, so revert to stock
          GameEvents.onGUIApplicationLauncherReady.Add(OnGuiAppLauncherReady);
          GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGuiAppLauncherDestroyed);

          SMSettings.EnableBlizzyToolbar = SMSettings.PrevEnableBlizzyToolbar;
        }
        else
        {
          OnGuiAppLauncherDestroyed();
          GameEvents.onGUIApplicationLauncherReady.Remove(OnGuiAppLauncherReady);
          GameEvents.onGUIApplicationLauncherDestroyed.Remove(OnGuiAppLauncherDestroyed);
          SMSettings.PrevEnableBlizzyToolbar = SMSettings.EnableBlizzyToolbar;
          if (HighLogic.LoadedSceneIsFlight)
            _smButtonBlizzy.Visible = true;
          if (HighLogic.LoadedScene != GameScenes.SPACECENTER) return;
          _smRosterBlizzy.Visible = true;
          _smSettingsBlizzy.Visible = true;
        }
      }
      else if (!SMSettings.EnableBlizzyToolbar && SMSettings.PrevEnableBlizzyToolbar)
      {
        // Use stock Toolbar
        if (HighLogic.LoadedSceneIsFlight)
          _smButtonBlizzy.Visible = false;
        if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
        {
          _smRosterBlizzy.Visible = false;
          _smSettingsBlizzy.Visible = false;
        }
        GameEvents.onGUIApplicationLauncherReady.Add(OnGuiAppLauncherReady);
        GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGuiAppLauncherDestroyed);
        OnGuiAppLauncherReady();
        SMSettings.PrevEnableBlizzyToolbar = SMSettings.EnableBlizzyToolbar;
      }
    }

    // Stock Toolbar Startup and cleanup
    private void OnGuiAppLauncherReady()
    {
      try
      {
        // Setup SM WIndow button
        if (HighLogic.LoadedSceneIsFlight && _smButtonStock == null && !SMSettings.EnableBlizzyToolbar)
        {
          string iconfile = "IconOff_38";
          _smButtonStock = ApplicationLauncher.Instance.AddModApplication(
            OnSmButtonClicked,
            OnSmButtonClicked,
            DummyHandler,
            DummyHandler,
            DummyHandler,
            DummyHandler,
            ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW,
            GameDatabase.Instance.GetTexture(TextureFolder + iconfile, false));

          if (WindowManifest.ShowWindow)
            _smButtonStock.SetTexture(
              GameDatabase.Instance.GetTexture(
                WindowManifest.ShowWindow ? TextureFolder + "IconOn_38" : TextureFolder + "IconOff_38", false));
        }

        // Setup Settings Button
        if (HighLogic.LoadedScene == GameScenes.SPACECENTER && _smSettingsStock == null &&
            !SMSettings.EnableBlizzyToolbar)
        {
          string iconfile = "IconS_Off_38";
          _smSettingsStock = ApplicationLauncher.Instance.AddModApplication(
            OnSmSettingsClicked,
            OnSmSettingsClicked,
            DummyHandler,
            DummyHandler,
            DummyHandler,
            DummyHandler,
            ApplicationLauncher.AppScenes.SPACECENTER,
            GameDatabase.Instance.GetTexture(TextureFolder + iconfile, false));

          if (WindowSettings.ShowWindow)
            _smSettingsStock.SetTexture(
              GameDatabase.Instance.GetTexture(
                WindowSettings.ShowWindow ? TextureFolder + "IconS_On_38" : TextureFolder + "IconS_Off_38", false));
        }

        // Setup Roster Button
        if (HighLogic.LoadedScene != GameScenes.SPACECENTER || _smRosterStock != null || SMSettings.EnableBlizzyToolbar)
          return;
        {
          string iconfile = "IconR_Off_38";
          _smRosterStock = ApplicationLauncher.Instance.AddModApplication(
            OnSmRosterClicked,
            OnSmRosterClicked,
            DummyHandler,
            DummyHandler,
            DummyHandler,
            DummyHandler,
            ApplicationLauncher.AppScenes.SPACECENTER,
            GameDatabase.Instance.GetTexture(TextureFolder + iconfile, false));

          if (WindowRoster.ShowWindow)
            _smRosterStock.SetTexture(
              GameDatabase.Instance.GetTexture(
                WindowRoster.ShowWindow ? TextureFolder + "IconR_On_38" : TextureFolder + "IconR_Off_38", false));
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage("Error in:  SMAddon.OnGUIAppLauncherReady.  " + ex, Utilities.LogType.Error, true);
      }
    }

    private void OnGuiAppLauncherDestroyed()
    {
      //Debug.Log("[ShipManifest]:  ShipManifestAddon.OnGUIAppLauncherDestroyed");
      try
      {
        if (_smButtonStock != null)
        {
          ApplicationLauncher.Instance.RemoveModApplication(_smButtonStock);
          _smButtonStock = null;
        }
        if (_smRosterStock != null)
        {
          ApplicationLauncher.Instance.RemoveModApplication(_smRosterStock);
          _smRosterStock = null;
        }
        if (_smSettingsStock == null) return;
        ApplicationLauncher.Instance.RemoveModApplication(_smSettingsStock);
        _smSettingsStock = null;
      }
      catch (Exception ex)
      {
        Utilities.LogMessage("Error in:  SMAddon.OnGUIAppLauncherDestroyed.  " + ex, Utilities.LogType.Error, true);
      }
    }

    //Toolbar button click handlers
    internal static void OnSmButtonClicked()
    {
      //Debug.Log("[ShipManifest]:  ShipManifestAddon.OnSMButtonToggle");
      try
      {
        if (WindowManifest.ShowWindow)
        {
          // SM is showing.  Turn off.
          if (SmVessel.TransferCrewObj.CrewXferActive || TransferPump.PumpProcessOn)
            return;

          SMHighlighter.ClearResourceHighlighting(SmVessel.SelectedResourcesParts);
          SmVessel.SelectedResources.Clear();
          SmVessel.SelectedPartsSource.Clear();
          SmVessel.SelectedPartsTarget.Clear();
          WindowManifest.ShowWindow = !WindowManifest.ShowWindow;
        }
        else
        {
          // SM is not showing. turn on if we can.
          if (SMConditions.CanShowShipManifest(true))
            WindowManifest.ShowWindow = !WindowManifest.ShowWindow;
          else
            return;
        }

        if (SMSettings.EnableBlizzyToolbar)
          _smButtonBlizzy.TexturePath = WindowManifest.ShowWindow
            ? TextureFolder + "IconOn_24"
            : TextureFolder + "IconOff_24";
        else
          _smButtonStock.SetTexture(
            GameDatabase.Instance.GetTexture(
              WindowManifest.ShowWindow ? TextureFolder + "IconOn_38" : TextureFolder + "IconOff_38", false));
      }
      catch (Exception ex)
      {
        Utilities.LogMessage("Error in:  SMAddon.OnSMButtonToggle.  " + ex, Utilities.LogType.Error, true);
      }
    }

    internal static void OnSmRosterClicked()
    {
      //Debug.Log("[ShipManifest]:  SMAddon.OnSMRosterToggle");
      try
      {
        if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
        {
          WindowRoster.ShowWindow = !WindowRoster.ShowWindow;
          if (SMSettings.EnableBlizzyToolbar)
            _smRosterBlizzy.TexturePath = WindowRoster.ShowWindow
              ? TextureFolder + "IconR_On_24"
              : TextureFolder + "IconR_Off_24";
          else
            _smRosterStock.SetTexture(
              GameDatabase.Instance.GetTexture(
                WindowRoster.ShowWindow ? TextureFolder + "IconR_On_38" : TextureFolder + "IconR_Off_38", false));
          if (WindowRoster.ShowWindow) WindowRoster.GetRosterList();
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage("Error in:  SMAddon.OnSMRosterToggle.  " + ex, Utilities.LogType.Error, true);
      }
    }

    internal static void OnSmSettingsClicked()
    {
      //Debug.Log("[ShipManifest]:  SMAddon.OnSMRosterToggle. Val:  " + WindowSettings.ShowWindow);
      try
      {
        if (HighLogic.LoadedScene != GameScenes.SPACECENTER) return;
        WindowSettings.ShowWindow = !WindowSettings.ShowWindow;
        SMSettings.MemStoreTempSettings();
        if (SMSettings.EnableBlizzyToolbar)
          _smSettingsBlizzy.TexturePath = WindowSettings.ShowWindow
            ? TextureFolder + "IconS_On_24"
            : TextureFolder + "IconS_Off_24";
        else
          _smSettingsStock.SetTexture(
            GameDatabase.Instance.GetTexture(
              WindowSettings.ShowWindow ? TextureFolder + "IconS_On_38" : TextureFolder + "IconS_Off_38", false));
      }
      catch (Exception ex)
      {
        Utilities.LogMessage("Error in:  SMAddon.OnSMSettingsToggle.  " + ex, Utilities.LogType.Error, true);
      }
    }

    #endregion

    #region GUI Methods

    internal void Display()
    {
      string step = "";
      try
      {
        step = "0 - Start";
        if (WindowDebugger.ShowWindow)
          WindowDebugger.Position = GUILayout.Window(398643, WindowDebugger.Position, WindowDebugger.Display,
            WindowDebugger.Title, GUILayout.MinHeight(20));

        if ((HighLogic.LoadedScene == GameScenes.FLIGHT || HighLogic.LoadedScene == GameScenes.SPACECENTER) && ShowUi)
        {
          if (WindowSettings.ShowWindow)
          {
            step = "4 - Show Settings";
            WindowSettings.Position = GUILayout.Window(398546, WindowSettings.Position, WindowSettings.Display,
              WindowSettings.Title, GUILayout.MinHeight(20));
          }

          if (WindowRoster.ShowWindow)
          {
            step = "6 - Show Roster";
            if (WindowRoster.ResetRosterSize)
              WindowRoster.Position.height = SMSettings.UseUnityStyle ? 330 : 350;
            WindowRoster.Position = GUILayout.Window(398547, WindowRoster.Position, WindowRoster.Display,
              WindowRoster.Title, GUILayout.MinHeight(20));
          }
        }
        if (HighLogic.LoadedScene == GameScenes.FLIGHT &&
            (FlightGlobals.fetch == null ||
             (FlightGlobals.ActiveVessel != null && FlightGlobals.ActiveVessel != SmVessel.Vessel)))
        {
          step = "0a - Vessel Change";
          SmVessel.SelectedPartsSource.Clear();
          SmVessel.SelectedPartsTarget.Clear();
          SmVessel.SelectedResources.Clear();
          return;
        }

        step = "1 - Show Interface(s)";
        // Is the scene one we want to be visible in?
        if (SMConditions.CanShowShipManifest())
        {
          // What windows do we want to show?
          step = "2 - Can Show Manifest - true";
          WindowManifest.Position = GUILayout.Window(398544, WindowManifest.Position, WindowManifest.Display,
            WindowManifest.Title, GUILayout.MinHeight(20));

          if (WindowTransfer.ShowWindow && SmVessel.SelectedResources.Count > 0)
          {
            step = "3 - Show Transfer";
            // Lets build the running totals for each resource for display in title...
            WindowTransfer.Position = GUILayout.Window(398545, WindowTransfer.Position, WindowTransfer.Display,
              WindowTransfer.Title, GUILayout.MinHeight(20));
          }

          if (!WindowManifest.ShowWindow || !WindowControl.ShowWindow) return;
          step = "7 - Show Control";
          WindowControl.Position = GUILayout.Window(398548, WindowControl.Position, WindowControl.Display,
            WindowControl.Title, GUILayout.MinWidth(350), GUILayout.MinHeight(20));
        }
        else
        {
          step = "2 - Can Show Manifest = false";
          if (!SMSettings.EnableCls || SmVessel == null) return;
          if (SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Crew.ToString()))
            SMHighlighter.HighlightClsVessel(false, true);
        }
      }
      catch (Exception ex)
      {
        if (!FrameErrTripped)
        {
          Utilities.LogMessage(
            string.Format(" in Display at or near step:  " + step + ".  Error:  {0} \r\n\r\n{1}", ex.Message,
              ex.StackTrace), Utilities.LogType.Error, true);
          FrameErrTripped = true;
        }
      }
    }

    internal static void RepositionWindows()
    {
      RepositionWindow(ref WindowManifest.Position);
      RepositionWindow(ref WindowTransfer.Position);
      RepositionWindow(ref WindowDebugger.Position);
      RepositionWindow(ref WindowSettings.Position);
      RepositionWindow(ref WindowControl.Position);
      RepositionWindow(ref WindowRoster.Position);
    }

    internal static void RepositionWindow(ref Rect windowPosition)
    {
      // This method uses Gui point system.
      if (windowPosition.x < 0) windowPosition.x = 0;
      if (windowPosition.y < 0) windowPosition.y = 0;

      if (windowPosition.xMax > Screen.width)
        windowPosition.x = Screen.width - windowPosition.width;
      if (windowPosition.yMax > Screen.height)
        windowPosition.y = Screen.height - windowPosition.height;
    }

    internal static Rect GuiToScreenRect(Rect rect)
    {
      // Must run during OnGui to work...
      Rect newRect = new Rect
      {
        position = GUIUtility.GUIToScreenPoint(rect.position),
        width = rect.width,
        height = rect.height
      };
      return newRect;
    }
    #endregion

    #region Action Methods

    internal void UpdateSMcontroller(Vessel newVessel)
    {
      try
      {
        SMHighlighter.ClearResourceHighlighting(SmVessel.SelectedResourcesParts);
        if (SmVessel.Vessel != newVessel)
        {
          if (SmVessel.TransferCrewObj.CrewXferActive && !SmVessel.TransferCrewObj.IvaDelayActive)
            SmVessel.TransferCrewObj.CrewTransferAbort();
          if (TransferPump.PumpProcessOn) TransferPump.PumpProcessOn = false;
        }

        if (SmVessel.Vessel != null && SMConditions.CanShowShipManifest())
        {
          if (newVessel.isEVA && !SmVessel.Vessel.isEVA)
          {
            if (WindowManifest.ShowWindow) OnSmButtonClicked();

            // kill selected resource and its associated highlighting.
            SmVessel.SelectedResources.Clear();
          }

        }

        // Now let's update the current vessel view...
        SmVessel = SMVessel.GetInstance(newVessel);
        SmVessel.RefreshLists();
      }
      catch (Exception ex)
      {
        Utilities.LogMessage("Error in:  SMAddon.UpdateSMcontroller.  " + ex, Utilities.LogType.Error, true);
      }
    }

    internal static void UpdateClsSpaces()
    {
      if (GetClsVessel())
      {
        try
        {
          SmVessel.ClsPartSource = null;
          SmVessel.ClsSpaceSource = null;
          SmVessel.ClsPartTarget = null;
          SmVessel.ClsSpaceTarget = null;
          List<ICLSSpace>.Enumerator spaces = ClsAddon.Vessel.Spaces.GetEnumerator();
          while (spaces.MoveNext())
          {
            List<ICLSPart>.Enumerator parts = spaces.Current.Parts.GetEnumerator();
            while (parts.MoveNext())
            {
              if (SmVessel.SelectedPartsSource.Contains(parts.Current.Part) && SmVessel.ClsPartSource == null)
              {
                SmVessel.ClsPartSource = parts.Current;
                SmVessel.ClsSpaceSource = spaces.Current;
              }
              if (SmVessel.SelectedPartsTarget.Contains(parts.Current.Part) && SmVessel.ClsPartTarget == null)
              {
                SmVessel.ClsPartTarget = parts.Current;
                SmVessel.ClsSpaceTarget = spaces.Current;
              }
              if (SmVessel.ClsPartSource != null && SmVessel.ClsPartTarget != null)
                break;
            }
            if (SmVessel.ClsSpaceSource != null && SmVessel.ClsSpaceTarget != null)
              break;
          }
        }
        catch (Exception ex)
        {
          Utilities.LogMessage(
            string.Format(" in UpdateCLSSpaces.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), Utilities.LogType.Error, true);
        }
      }
    }

    internal static bool GetClsAddon()
    {
      ClsAddon = CLSClient.GetCLS();
      return ClsAddon != null;
    }

    internal static bool GetClsVessel()
    {
      try
      {
        return ClsAddon.Vessel != null;
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(
          string.Format(" in SMAddon.GetCLSVessel.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), Utilities.LogType.Error, true);
        return false;
      }
    }

    internal static bool ActivateBlizzyToolBar()
    {
      if (!SMSettings.EnableBlizzyToolbar) return false;
      if (!ToolbarManager.ToolbarAvailable) return false;
      try
      {
        if (HighLogic.LoadedScene == GameScenes.FLIGHT)
        {
          _smButtonBlizzy = ToolbarManager.Instance.add("ShipManifest", "Manifest");
          _smButtonBlizzy.TexturePath = WindowManifest.ShowWindow
            ? TextureFolder + "IconOn_24"
            : TextureFolder + "IconOff_24";
          _smButtonBlizzy.ToolTip = "Ship Manifest";
          _smButtonBlizzy.Visibility = new GameScenesVisibility(GameScenes.FLIGHT);
          _smButtonBlizzy.Visible = true;
          _smButtonBlizzy.OnClick += e => { OnSmButtonClicked(); };
        }
        else if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
        {
          _smSettingsBlizzy = ToolbarManager.Instance.add("ShipManifest", "Settings");
          _smSettingsBlizzy.TexturePath = WindowSettings.ShowWindow
            ? TextureFolder + "IconS_On_24"
            : TextureFolder + "IconS_Off_24";
          _smSettingsBlizzy.ToolTip = "Ship Manifest Settings Window";
          _smSettingsBlizzy.Visibility = new GameScenesVisibility(GameScenes.SPACECENTER);
          _smSettingsBlizzy.Visible = true;
          _smSettingsBlizzy.OnClick += e => { OnSmSettingsClicked(); };

          _smRosterBlizzy = ToolbarManager.Instance.add("ShipManifest", "Roster");
          _smRosterBlizzy.TexturePath = WindowRoster.ShowWindow
            ? TextureFolder + "IconR_On_24"
            : TextureFolder + "IconR_Off_24";
          _smRosterBlizzy.ToolTip = "Ship Manifest Roster Window";
          _smRosterBlizzy.Visibility = new GameScenesVisibility(GameScenes.SPACECENTER);
          _smRosterBlizzy.Visible = true;
          _smRosterBlizzy.OnClick += e => { OnSmRosterClicked(); };
        }
        return true;
      }
      catch (Exception)
      {
        // Blizzy Toolbar instantiation error.
        return false;
      }
    }

    internal static void FireEventTriggers()
    {
      // Per suggestion by shaw (http://forum.kerbalspaceprogram.com/threads/62270?p=1033866&viewfull=1#post1033866)
      // and instructions for using CLS API by codepoet.
      GameEvents.onVesselChange.Fire(SmVessel.Vessel);
    }

    internal static void DisplayScreenMsg(string strMessage)
    {
      ScreenMessage smessage = new ScreenMessage(strMessage, 15f, ScreenMessageStyle.UPPER_CENTER);
      ScreenMessages.PostScreenMessage(smessage);
    }

    internal static void RemoveScreenMsg()
    {
      ScreenMessage smessage = new ScreenMessage(string.Empty, 15f, ScreenMessageStyle.UPPER_CENTER);
      ScreenMessages smessages = FindObjectOfType<ScreenMessages>();
      if (smessages != null)
      {
        IEnumerator<ScreenMessage> smessagesToRemove =
          smessages.ActiveMessages.Where(
            x =>
              Math.Abs(x.startTime - smessage.startTime) < SMSettings.Tolerance &&
              x.style == ScreenMessageStyle.UPPER_CENTER).GetEnumerator();
        while (smessagesToRemove.MoveNext())
        {
          if (smessagesToRemove.Current == null) continue;
          ScreenMessages.RemoveMessage(smessagesToRemove.Current);
        }
      }
    }

    // This method is used for autosave...
    internal void RunSave()
    {
      try
      {
        SMSettings.SaveSettings();
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format(" in SMAddon.RunSave.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace),
          Utilities.LogType.Error, true);
      }
    }

    #endregion
  }

  internal class ShipManifestModule : PartModule
  {
    [KSPEvent(guiActive = true, guiName = "Destroy Part", active = true)]
    internal void DestoryPart()
    {
      if (part != null)
        part.temperature = 5000;
    }

    public override void OnUpdate()
    {
      base.OnUpdate();

      if (part != null && part.name == "ShipManifest")
        Events["DestoryPart"].active = true;
      else
        Events["DestoryPart"].active = false;
    }
  }
}