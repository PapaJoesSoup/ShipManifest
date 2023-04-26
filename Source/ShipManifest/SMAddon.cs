using System;
using System.Collections.Generic;
using System.Linq;
using ConnectedLivingSpace;
using KSP.UI.Screens;
using ShipManifest.APIClients;
using ShipManifest.InternalObjects;
using ShipManifest.InternalObjects.Settings;
using ShipManifest.Process;
using ShipManifest.Windows;
using ShipManifest.Windows.Popups;
using ShipManifest.Windows.Tabs.Settings;
using ShipManifest.Windows.Tabs.Control;
using UnityEngine;

namespace ShipManifest
{
  [KSPAddon(KSPAddon.Startup.FlightAndKSC, false)]
  // ReSharper disable once InconsistentNaming
  public class SMAddon : MonoBehaviour
  {
    // Object Scope:  Current Unity/KSP Scene.  Object will be destroyed and recreated when scene changes!

    //internal SuitCombos suitCombos;

    #region Static Properties

    // Game object that keeps us running
    // internal static GameObject SmInstance;

    internal static bool SceneChangeInitDfWrapper;
    
    // current vessel's controller instance
    internal static SMVessel SmVessel;
    internal static ICLSAddon ClsAddon;
    internal static bool OrigClsAllowCrewXferSetting;

    internal static string TextureFolder = "ShipManifest/Textures/";
    internal static string SaveMessage = string.Empty;
    internal static PartItemTransfer StockTransferItem;

    [KSPField(isPersistant = true)] internal static double Elapsed;

    // Resource xfer vars
    // This is still very entrenched.   Need to look at implications for conversion to instanced.
    internal static TransferPump.TypeXfer ActiveXferType = TransferPump.TypeXfer.SourceToTarget;

    // Toolbar Integration.
    private static IButton _smButtonBlizzy;
    private static IButton _smSettingsBlizzy;
    private static IButton _smRosterBlizzy;
    private static ApplicationLauncherButton _smButtonStock;
    private static ApplicationLauncherButton _smSettingsStock;
    private static ApplicationLauncherButton _smRosterStock;

    internal static Vector3 IconPosition = new Vector3();

    // Repeating error latch
    internal static bool FrameErrTripped;

    // SM UI toggle
    internal static bool ShowUi = true;

    //CLS Event integration
    private EventData<Vessel> _onClsVesselChangeEvent;

    #endregion

    // Makes singleton instance available via reflection
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
    #endregion

    #region Event handlers

    private static void DummyHandler()
    {
      // This is used for Stock Toolbars.  Some delegates are not needed.
    }

    // Addon state event handlers
    internal void Awake()
    {
      try
      {
        if (HighLogic.LoadedScene != GameScenes.FLIGHT && HighLogic.LoadedScene != GameScenes.SPACECENTER) return;

        // Cache Localization Strings
        // (must occur first to allow static string var initialization on static windows)
        SmUtils.CacheSmLocalization();

        //WindowDebugger.Initialize();

        SMSettings.LoadSettings();

        if (CurrSettings.AutoSave)
          InvokeRepeating("RunSave", CurrSettings.SaveIntervalSec, CurrSettings.SaveIntervalSec);

        CreateAppIcons();

      }
      catch (Exception ex)
      {
        SmUtils.LogMessage($"Error in:  SMAddon.Awake.  Error:  {ex}", SmUtils.LogType.Error, true);
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
          WindowRoster.Position.height = (CurrSettings.UseUnityStyle ? 330 : 350) * CurrSettings.CurrentUIScale;


        if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
        {
          if (GetClsAddon())
          {
            SMSettings.ClsInstalled = true;
          }
          else
          {
            CurrSettings.EnableCls = false;
            SMSettings.ClsInstalled = false;
          }
          // reset any hacked kerbal names in game save from old version of SM/KSP
          if (CurrSettings.EnableChangeProfession)
            WindowRoster.ResetKerbalNames();

          SMSettings.SaveSettings();
          //RunSave();

          //Locate Helmet/suit picker
          //suitCombos = FindObjectOfType<SuitCombos>();
        }

        // Instantiate Event handlers
        GameEvents.onGameSceneSwitchRequested.Add(OnGameSceneSwitchRequested);

        // If we are not in flight, the rest does not get done!
        if (HighLogic.LoadedScene != GameScenes.FLIGHT) return;

        GameEvents.onCrewTransferPartListCreated.Add(OnCrewTransferPartListCreated);
        GameEvents.onItemTransferStarted.Add(OnItemTransferStarted);
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
        SmVessel.RefreshLists();        

        // Is CLS installed and enabled?
        if (GetClsAddon())
        {
          SMSettings.ClsInstalled = true;
          SMSettings.SaveSettings();
          UpdateClsSpaces();
          _onClsVesselChangeEvent = GameEvents.FindEvent<EventData<Vessel>>("onCLSVesselChange");
          if (_onClsVesselChangeEvent != null) _onClsVesselChangeEvent.Add(OnClsVesselChange);
        }
        else
        {
          CurrSettings.EnableCls = false;
          SMSettings.ClsInstalled = false;
          SMSettings.SaveSettings();
        }

        SMSettings.SetClsOverride();

        // Support for DeepFreeze
        //Trigger Update to check and initialize the DeepFreeze Wrapper API
        SceneChangeInitDfWrapper = true;

        // Load sounds for transfers.
        SMSound.LoadSounds();
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage($"Error in:  SMAddon.Start.  {ex}", SmUtils.LogType.Error, true);
      }
    }

    private static void ResetWindows()
    {
      // clear window sizes
      WindowManifest.Position.width = 0;
      WindowManifest.Position.height = 0;
      WindowTransfer.Position.width = 0;
      WindowTransfer.Position.height = 0;
      WindowRoster.Position.width = 0;
      WindowRoster.Position.height = 0;
      WindowSettings.Position.width = 0;
      WindowSettings.Position.height = 0;
      WindowDebugger.Position.width = 0;
      WindowDebugger.Position.height = 0;

      // refresh ui scales on all windows
      WindowManifest.RefreshUIScale();
      WindowTransfer.RefreshUIScale();
      WindowControl.RefreshUIScale();
      TabAntenna.RefreshUIScale();
      TabHatch.RefreshUIScale();
      TabLight.RefreshUIScale();
      TabRadiator.RefreshUIScale();
      TabScienceLab.RefreshUIScale();
      TabSolarPanel.RefreshUIScale();
      TabVessel.RefreshUIScale();

      PopupSmBtnHover.RefreshUIScale();
      PopupCloseTransfer.RefreshUIScale();
      
      WindowSettings.RefreshUIScale();
      TabConfig.RefreshUIScale();
      TabHighlight.RefreshUIScale();
      TabRealism.RefreshUIScale();
      TabSounds.RefreshUIScale();
      TabToolTips.RefreshUIScale();

      WindowDebugger.RefreshUIScale();
      WindowRoster.RefreshUIScale();

      WindowManifest.RefreshUIScale();
      WindowManifest.RefreshUIScale();
      WindowManifest.RefreshUIScale();
      WindowManifest.RefreshUIScale();
      WindowManifest.RefreshUIScale();
      WindowManifest.RefreshUIScale();
      WindowManifest.RefreshUIScale();

      GuiUtils.RefreshUIScale();
    }


    internal void OnDestroy()
    {
      //Debug.Log("[ShipManifest]:  SmAddon.OnDestroy");
      try
      {
        if (SMSettings.Loaded)
          SMSettings.SaveSettings();

        GameEvents.onGameSceneSwitchRequested.Remove(OnGameSceneSwitchRequested);

        GameEvents.onCrewTransferPartListCreated.Remove(OnCrewTransferPartListCreated);
        GameEvents.onCrewTransferSelected.Remove(OnCrewTransferSelected);
        GameEvents.onItemTransferStarted.Remove(OnItemTransferStarted);
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
        DestroyAppIcons();

        //Reset Roster Window data
        WindowRoster.editMode = WindowRoster.EditMode.None;
        WindowRoster.SelectedKerbal = null;
        WindowRoster.ToolTip = "";
        //WindowRoster.ShowWindow = false;
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage($"Error in:  SMAddon.OnDestroy.  {ex}", SmUtils.LogType.Error, true);
      }
    }

    internal void CreateAppIcons()
    {
      if (CurrSettings.EnableBlizzyToolbar)
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

    internal void DestroyAppIcons()
    {
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
          // Remove the stock toolbar button launcher handlers
          GameEvents.onGUIApplicationLauncherReady.Remove(OnGuiAppLauncherReady);
          GameEvents.onGUIApplicationLauncherDestroyed.Remove(OnGuiAppLauncherDestroyed);
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
    }

    // ReSharper disable once InconsistentNaming
    internal void OnGUI()
    {
      CheckForUIScaleChange();

      if (Event.current.type == EventType.MouseUp)
      {
        // Turn off window resizing
        GuiUtils.ResetResize();
      }
      try
      {
        GUI.skin = CurrSettings.UseUnityStyle ? null : HighLogic.Skin;

        Display();
        SMToolTips.ShowToolTips();
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage($"Error in:  SMAddon.OnGUI.  {ex}", SmUtils.LogType.Error, true);
      }
    }


    internal void Update()
    {
      try
      {
        CheckForToolbarTypeToggle();

        if (SceneChangeInitDfWrapper && Time.timeSinceLevelLoad > 3f && !InstalledMods.IsDfApiReady)
        {
          DfWrapper.InitDfWrapper();
          SceneChangeInitDfWrapper = false;
        }

        if (HighLogic.LoadedScene != GameScenes.FLIGHT) return;
        if (FlightGlobals.fetch == null || FlightGlobals.ActiveVessel == null) return;

        //if (SmVessel.SelectedResources.Count > 0) SMHighlighter.Update_Highlighter();

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
        {
          SmVessel.TransferCrewObj.CrewTransferProcess();
        }
        else if (SmVessel.TransferCrewObj.IsStockXfer)
        {
          SmVessel.TransferCrewObj.CrewTransferBegin(SmVessel.TransferCrewObj.FromCrewMember,
            SmVessel.TransferCrewObj.FromPart, SmVessel.TransferCrewObj.ToPart);
        }
      }
      catch (Exception ex)
      {
        if (!FrameErrTripped)
        {
          SmUtils.LogMessage(
            $" in SMAddon.Update (repeating error).  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}",
            SmUtils.LogType.Error, true);
          FrameErrTripped = true;
        }
      }
    }

    private void CheckForUIScaleChange()
    {
      if (Math.Abs(CurrSettings.CurrentUIScale - GameSettings.UI_SCALE) < .01f) return;
      CurrSettings.CurrentUIScale = GameSettings.UI_SCALE;
      SMStyle.SetupGuiStyles();
      ResetWindows();
    }

    // save settings on scene changes
    private void OnGameSceneLoadRequested(GameScenes requestedScene)
    {
      //Debug.Log("[ShipManifest]:  SMAddon.OnGameSceneLoadRequested");
      SMSettings.SaveSettings();
      if (InstalledMods.IsDfInstalled)
        SceneChangeInitDfWrapper = true;
    }

    private void OnGameSceneSwitchRequested(GameEvents.FromToAction<GameScenes, GameScenes> sceneData)
    {
      WindowControl.ShowWindow =
        WindowManifest.ShowWindow =
          WindowTransfer.ShowWindow = WindowRoster.ShowWindow = WindowSettings.ShowWindow = false;

      // Since the changes to Startup options, ON destroy is not being called when a Scene change occurs.  Startup is being called when the proper scene is loaded.
      // Let's do some cleanup of the app Icons here as well. to be sure we have only the icons we want...
      DestroyAppIcons();
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
    internal void OnCrewTransferPartListCreated(GameEvents.HostedFromToAction<Part,List<Part>> eventData)
    {
      // We can skip this event if a stock CrewTransfer is enabled, Override is off & no SM Crew Transfers are active
      if (CurrSettings.EnableStockCrewXfer && !CurrSettings.OverrideStockCrewXfer && TransferCrew.CrewXferState == TransferCrew.XferState.Off) return;

      // If override is off, then ignore.
      if (!CurrSettings.OverrideStockCrewXfer) return;

      // How can I tell if the parts are in the same space?... I need a starting point!  What part initiated the event?
      Part sourcePart = eventData.host;

      //Let's manhandle the lists
      List<Part> fullList = new List<Part>();
      List<Part>.Enumerator fromList = eventData.from.GetEnumerator();
      while (fromList.MoveNext())
      {
        if (fromList.Current == null) continue;
        //Check for DeepFreezer full. if full, abort handling Xfer.
        if (InstalledMods.IsDfInstalled && InstalledMods.IsDfApiReady && fromList.Current.Modules.Contains("DeepFreezer"))
        {
          if (new DfWrapper.DeepFreezer(fromList.Current.Modules["DeepFreezer"]).FreezerSpace == 0)
          {
            fullList.Add(fromList.Current);
          }
        }
        // If CLS is enabled and parts are not in same space.
        if (!SMConditions.IsClsInSameSpace(sourcePart, fromList.Current))
        {
          fullList.Add(fromList.Current);
        }
      }
      fromList.Dispose();
      if (fullList.Count <= 0) return;
      List<Part>.Enumerator removeList = fullList.GetEnumerator();
      while (removeList.MoveNext())
      {
        eventData.from.Remove(removeList.Current);
      }
      removeList.Dispose();
      eventData.to.AddRange(fullList);
    }

    internal void OnItemTransferStarted(PartItemTransfer xferPartItem)
    {
      if (CurrSettings.EnableCls && CurrSettings.RealXfers && xferPartItem.type == SMConditions.ResourceType.Crew.ToString())
        xferPartItem.semiValidMessage = "<color=orange>SM - This module is either full or internally unreachable.</color>";
      StockTransferItem = xferPartItem;
    }

    //TODO:  Check to see if this is still needed.
    //internal void OnAttemptTransfer(ProtoCrewMember pkerbal, Part part, CrewHatchController controller)
    //{
    //  if (!SMSettings.EnableCls || !SMSettings.RealismMode) return;
    //  if (SMConditions.IsClsInSameSpace(stockTransferItem.srcPart, part)) return;
    //  stockTransferItem.semiValidMessage = "Parts are not in the same internal Space.  You must EVA.";
    //}

    /// <summary>
    /// OnCrewTransferSelected (GameEvent Handler)
    /// This method captures a Stock Crew Transfer event just prior to the actual crew move.
    /// </summary>
    /// <param name="crewTransferData"></param>
    internal void OnCrewTransferSelected(CrewTransfer.CrewTransferData crewTransferData)
    {
      // We can skip this event if a stock CrewTransfer is enabled, Override is off & no SM Crew Transfers are active
      if (CurrSettings.EnableStockCrewXfer && !CurrSettings.OverrideStockCrewXfer && TransferCrew.CrewXferState == TransferCrew.XferState.Off) return;

      // Disable the stock Transfer action if SM dictates.
      if (!CurrSettings.EnableStockCrewXfer || TransferCrew.CrewXferState != TransferCrew.XferState.Off)
      {
        if (!CurrSettings.EnableStockCrewXfer) DisplayScreenMsg(SmUtils.SmTags["#smloc_addon_001"]);
        if (TransferCrew.CrewXferState != TransferCrew.XferState.Off) DisplayScreenMsg(SmUtils.SmTags["#smloc_addon_002"]);
        crewTransferData.canTransfer = false;
        return;
      }

      // If override is off, then ignore.
      if (!CurrSettings.OverrideStockCrewXfer) return;

      //Check for DeepFreezer full. if full, abort handling Xfer.
      if (InstalledMods.IsDfInstalled && InstalledMods.IsDfApiReady && crewTransferData.destPart.Modules.Contains("DeepFreezer"))
      {
        if (new DfWrapper.DeepFreezer(crewTransferData.destPart.Modules["DeepFreezer"]).FreezerSpace == 0)
        {
          DisplayScreenMsg(SmUtils.SmTags["#smloc_addon_003"]);
          crewTransferData.canTransfer = false;
          return;
        }
      }

      // CLS is enabled and parts are not in same space.  Abort Transfer.
      if (!SMConditions.IsClsInSameSpace(crewTransferData.sourcePart, crewTransferData.destPart))
      {
         DisplayScreenMsg(SmUtils.SmTags["#smloc_addon_004"]);
        crewTransferData.canTransfer = false;
        return;
      }

      // If override is off, then ignore.
      if (!CurrSettings.OverrideStockCrewXfer) return;

      // store data from event.
      DisplayScreenMsg(SmUtils.SmTags["#smloc_addon_005S"]);
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
        if (modVessel != FlightGlobals.ActiveVessel) return;
        SMHighlighter.ClearResourceHighlighting(SmVessel.SelectedResourcesParts);
        UpdateSMcontroller(modVessel);
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage($"Error in:  SMAddon.OnVesselWasModified.  {ex}", SmUtils.LogType.Error, true);
      }
    }

    internal void OnVesselChange(Vessel newVessel)
    {
      if (newVessel != FlightGlobals.ActiveVessel) return;
      try
      {
        SMHighlighter.ClearResourceHighlighting(SmVessel.SelectedResourcesParts);
        UpdateSMcontroller(newVessel);
        SMSettings.SetStockCrewTransferState();
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage(
          $" in SMAddon.OnVesselChange.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}", SmUtils.LogType.Error,
          true);
      }
    }

    private void OnVesselLoaded(Vessel data)
    {
      try
      {
        if (!data.Equals(FlightGlobals.ActiveVessel) || data == SmVessel.Vessel) return;
        SMHighlighter.ClearResourceHighlighting(SmVessel.SelectedResourcesParts);
        UpdateSMcontroller(data);
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage($"Error in:  SMAddon.OnVesselLoaded.  {ex}", SmUtils.LogType.Error, true);
      }
    }

    private void OnClsVesselChange(Vessel data)
    {
      OnVesselChange(data);
    }

    // Stock vs Blizzy Toolbar switch handler
    private void CheckForToolbarTypeToggle()
    {
      if (CurrSettings.EnableBlizzyToolbar && !OrigSettings.EnableBlizzyToolbar)
      {
        // Let't try to use Blizzy's toolbar
        if (!ActivateBlizzyToolBar())
        {
          // We failed to activate the toolbar, so revert to stock
          GameEvents.onGUIApplicationLauncherReady.Add(OnGuiAppLauncherReady);
          GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGuiAppLauncherDestroyed);

          CurrSettings.EnableBlizzyToolbar = OrigSettings.EnableBlizzyToolbar;
        }
        else
        {
          OnGuiAppLauncherDestroyed();
          GameEvents.onGUIApplicationLauncherReady.Remove(OnGuiAppLauncherReady);
          GameEvents.onGUIApplicationLauncherDestroyed.Remove(OnGuiAppLauncherDestroyed);
          OrigSettings.EnableBlizzyToolbar = CurrSettings.EnableBlizzyToolbar;
          if (HighLogic.LoadedSceneIsFlight)
            _smButtonBlizzy.Visible = true;
          if (HighLogic.LoadedScene != GameScenes.SPACECENTER) return;
          _smRosterBlizzy.Visible = true;
          _smSettingsBlizzy.Visible = true;
        }
      }
      else if (!CurrSettings.EnableBlizzyToolbar && OrigSettings.EnableBlizzyToolbar)
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
        OrigSettings.EnableBlizzyToolbar = CurrSettings.EnableBlizzyToolbar;
      }
    }

    // Stock Toolbar Startup and cleanup
    private void OnGuiAppLauncherReady()
    {
      try
      {
        // Setup SM Window button
        if (HighLogic.LoadedSceneIsFlight && _smButtonStock == null && !CurrSettings.EnableBlizzyToolbar)
        {
          string iconfile = "IconOff_128";
          _smButtonStock = ApplicationLauncher.Instance.AddModApplication(
            OnSmButtonClicked,
            OnSmButtonClicked,
            OnSmIconHover,
            OnSmIconHover,
            DummyHandler,
            DummyHandler,
            ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW,
            GameDatabase.Instance.GetTexture(TextureFolder + iconfile, false));
          _smButtonStock.onRightClick = OnSmRightClick;

          if (WindowManifest.ShowWindow)
            _smButtonStock.SetTexture(
              GameDatabase.Instance.GetTexture(
                WindowManifest.ShowWindow ? $"{TextureFolder}IconOn_128" : $"{TextureFolder}IconOff_128", false));
        }

        // Setup Settings Button
        if (HighLogic.LoadedScene == GameScenes.SPACECENTER && CurrSettings.EnableSettingsIcon && _smSettingsStock == null &&
            !CurrSettings.EnableBlizzyToolbar)
        {
          string iconfile = "IconS_Off_128";
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
                WindowSettings.ShowWindow ? $"{TextureFolder}IconS_On_128" : $"{TextureFolder}IconS_Off_128", false));
        }

        // Setup Roster Button
        if (HighLogic.LoadedScene != GameScenes.SPACECENTER || _smRosterStock != null || CurrSettings.EnableBlizzyToolbar)
          return;
        {
          string iconfile = "IconR_Off_128";
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
                WindowRoster.ShowWindow ? $"{TextureFolder}IconR_On_128" : $"{ TextureFolder}IconR_Off_128", false));
        }
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage($"Error in:  SMAddon.OnGUIAppLauncherReady.  {ex}", SmUtils.LogType.Error, true);
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
        SmUtils.LogMessage($"Error in:  SMAddon.OnGUIAppLauncherDestroyed.  {ex}", SmUtils.LogType.Error, true);
      }
    }

    //Toolbar button click handlers
    internal static void OnSmButtonClicked()
    {
      //Debug.Log("[ShipManifest]:  ShipManifestAddon.OnSMButtonToggle Enter");
      try
      {
        //Debug.Log("[ShipManifest]:  ShowWIndow:  " + WindowManifest.ShowWindow + ", ShowUi:  " + ShowUi);
        if (WindowManifest.ShowWindow)
        {
          // SM is showing.  Turn off.
          if (SmVessel.TransferCrewObj.CrewXferActive || TransferPump.PumpProcessOn)
            return;

          SMHighlighter.ClearResourceHighlighting(SmVessel.SelectedResourcesParts);
          //SmVessel.SelectedResources.Clear();
          SmVessel.SelectedPartsSource.Clear();
          SmVessel.SelectedPartsTarget.Clear();
          SmVessel.SelectedVesselsSource.Clear();
          SmVessel.SelectedVesselsTarget.Clear();
          WindowManifest.ShowWindow = !WindowManifest.ShowWindow;
        }
        else
        {
          // SM is not showing. turn on if we can.
          //Debug.Log("[ShipManifest]:  CanShowShipManifest:  " + SMConditions.CanShowShipManifest(true));
          if (SMConditions.CanShowShipManifest(true))
            WindowManifest.ShowWindow = !WindowManifest.ShowWindow;
          else
            return;
        }
        //Debug.Log("[ShipManifest]:  ShowWIndow:  " + WindowManifest.ShowWindow + ", ShowUi:  " + ShowUi);

        if (CurrSettings.EnableBlizzyToolbar)
          _smButtonBlizzy.TexturePath = WindowManifest.ShowWindow
            ? $"{TextureFolder}IconOn_24"
            : $"{TextureFolder}IconOff_24";
        else
          _smButtonStock.SetTexture(
            GameDatabase.Instance.GetTexture(
              WindowManifest.ShowWindow ? $"{TextureFolder}IconOn_128" : $"{TextureFolder}IconOff_128", false));
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage($"Error in:  SMAddon.OnSMButtonToggle.  {ex}", SmUtils.LogType.Error, true);
      }
      //Debug.Log("[ShipManifest]:  ShipManifestAddon.OnSMButtonToggle Exit");
      //Debug.Log("[ShipManifest]:  ShowWIndow:  " + WindowManifest.ShowWindow + ", ShowUi:  " + ShowUi);
    }

    internal static void OnSmRightClick()
    {
      try
      {
        if (WindowTransfer.ShowWindow)
        {
          // SM Transfer window is showing.  Turn off.
          WindowTransfer.BtnCloseWindow();
        }
        else
        {
          // SMTransfer is not showing. turn on if we can.
          if (SMConditions.CanShowShipManifest(true) && SmVessel.SelectedResources.Count > 0)
            WindowTransfer.ShowWindow = !WindowTransfer.ShowWindow;
          else
            return;
        }
        //Debug.Log("[ShipManifest]:  ShowWIndow:  " + WindowManifest.ShowWindow + ", ShowUi:  " + ShowUi);

        if (CurrSettings.EnableBlizzyToolbar)
          _smButtonBlizzy.TexturePath = WindowManifest.ShowWindow
            ? $"{TextureFolder}IconOn_24"
            : $"{TextureFolder}IconOff_24";
        else
          _smButtonStock.SetTexture(
            GameDatabase.Instance.GetTexture(
              WindowManifest.ShowWindow ? $"{TextureFolder}IconOn_128" : $"{TextureFolder}IconOff_128", false));
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage($"Error in:  SMAddon.OnSMButtonToggle.  {ex}", SmUtils.LogType.Error, true);
      }
      //Debug.Log("[ShipManifest]:  ShipManifestAddon.OnSMButtonToggle Exit");
      //Debug.Log("[ShipManifest]:  ShowWIndow:  " + WindowManifest.ShowWindow + ", ShowUi:  " + ShowUi);

    }

    internal static void OnSmIconHover()
    {
      if (!PopupSmBtnHover.ShowToolTips)
      {
        PopupSmBtnHover.ShowWindow = false;
        return;
      }
      PopupSmBtnHover.ShowWindow = !PopupSmBtnHover.ShowWindow;
      if (PopupSmBtnHover.ShowWindow)
      {
        PopupSmBtnHover.Position.y = Event.current.mousePosition.y - 100;
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
          if (CurrSettings.EnableBlizzyToolbar)
            _smRosterBlizzy.TexturePath = WindowRoster.ShowWindow
              ? $"{TextureFolder}IconR_On_24"
              : $"{TextureFolder}IconR_Off_24";
          else
            _smRosterStock.SetTexture(
              GameDatabase.Instance.GetTexture(
                WindowRoster.ShowWindow ? $"{TextureFolder}IconR_On_128" : $"{ TextureFolder}IconR_Off_128", false));
          if (WindowRoster.ShowWindow) WindowRoster.GetRosterList();
        }
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage($"Error in:  SMAddon.OnSMRosterToggle.{ex}", SmUtils.LogType.Error, true);
      }
    }

    internal static void OnSmSettingsClicked()
    {
      //Debug.Log($"[ShipManifest]:  SMAddon.OnSMRosterToggle. Val:  {WindowSettings.ShowWindow}");
      try
      {
        if (HighLogic.LoadedScene != GameScenes.SPACECENTER) return;
        WindowSettings.ShowWindow = !WindowSettings.ShowWindow;
        SMSettings.MemStoreTempSettings();
        if (CurrSettings.EnableBlizzyToolbar)
          _smSettingsBlizzy.TexturePath = WindowSettings.ShowWindow
            ? $"{TextureFolder}IconS_On_24"
            : $"{TextureFolder}IconS_Off_24";
        else
          _smSettingsStock.SetTexture(
            GameDatabase.Instance.GetTexture(
              WindowSettings.ShowWindow ? $"{TextureFolder}IconS_On_128" : $"{ TextureFolder}IconS_Off_128", false));
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage($"Error in:  SMAddon.OnSMSettingsToggle.  {ex}", SmUtils.LogType.Error, true);
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
            WindowDebugger.Title,GUILayout.MinHeight(20 * CurrSettings.CurrentUIScale));

        if(PopupSmBtnHover.ShowWindow)
          PopupSmBtnHover.Position = GUILayout.Window(398650, PopupSmBtnHover.Position, PopupSmBtnHover.Display,
            PopupSmBtnHover.Title, GUILayout.MinHeight(20 * CurrSettings.CurrentUIScale));

        if (HighLogic.LoadedScene == GameScenes.FLIGHT && SMConditions.CanShowShipManifest() || 
            HighLogic.LoadedScene == GameScenes.SPACECENTER && ShowUi && !SMConditions.IsPauseMenuOpen())
        {
          if (WindowSettings.ShowWindow)
          {
            step = "4 - Show Settings";
            WindowSettings.Position = GUILayout.Window(398546, WindowSettings.Position, WindowSettings.Display,
              WindowSettings.Title, GUILayout.MinHeight(20 * CurrSettings.CurrentUIScale));
          }

          if (WindowRoster.ShowWindow)
          {
            step = "6 - Show Roster";
            if (WindowRoster.ResetRosterSize)
              WindowRoster.Position.height = CurrSettings.UseUnityStyle ? 330 * CurrSettings.CurrentUIScale : 350 * CurrSettings.CurrentUIScale;
            WindowRoster.Position = GUILayout.Window(398547, WindowRoster.Position, WindowRoster.Display,
              WindowRoster.Title, GUILayout.MinHeight(20 * CurrSettings.CurrentUIScale));
          }
        }

        step = "1 - Show Interface(s)";
        // Is the scene one we want to be visible in?
        if (SMConditions.CanShowWindowTransfer() && WindowTransfer.ShowWindow)
        {
          step = "3 - Show Transfer";
          // Lets build the running totals for each resource for display in title...
          WindowTransfer.Position = GUILayout.Window(398545, WindowTransfer.Position, WindowTransfer.Display,
            WindowTransfer.Title, GUILayout.MinHeight(20 * CurrSettings.CurrentUIScale));

          if (TransferPump.Paused && PopupCloseTransfer.ShowWindow)
          {
            PopupCloseTransfer.Position = GUILayout.Window(398549, PopupCloseTransfer.Position, PopupCloseTransfer.Display,
              PopupCloseTransfer.Title, SMStyle.PopupStyle, GUILayout.MinHeight(20 * CurrSettings.CurrentUIScale));
          }
        }
        if (SMConditions.CanShowShipManifest())
        {
          // What windows do we want to show?
          step = "2 - Can Show Manifest - true";
          WindowManifest.Position = GUILayout.Window(398544, WindowManifest.Position, WindowManifest.Display,
            WindowManifest.Title, GUILayout.MinHeight(20 * CurrSettings.CurrentUIScale));

          if (WindowTransfer.ShowWindow && SmVessel.SelectedResources.Count > 0)
          {
            step = "3 - Show Transfer";
            // Lets build the running totals for each resource for display in title...
            WindowTransfer.Position = GUILayout.Window(398545, WindowTransfer.Position, WindowTransfer.Display,
              WindowTransfer.Title, GUILayout.MinHeight(20 * CurrSettings.CurrentUIScale));
          }

          if (!WindowManifest.ShowWindow || !WindowControl.ShowWindow) return;
           step = "7 - Show Control";
          WindowControl.Position = GUILayout.Window(398548, WindowControl.Position, WindowControl.Display,
            WindowControl.Title, GUILayout.MinWidth(350 * CurrSettings.CurrentUIScale), GUILayout.MinHeight(20 * CurrSettings.CurrentUIScale));
        }
        else
        {
          step = "2 - Can Show Manifest = false";
          if (!CurrSettings.EnableCls || SmVessel == null) return;
          if (SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Crew.ToString()))
            SMHighlighter.HighlightClsVessel(false, true);
        }

      }
      catch (Exception ex)
      {
        if (!FrameErrTripped)
        {
          SmUtils.LogMessage($" in Display at or near step:  {step}.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}", SmUtils.LogType.Error, true);
          FrameErrTripped = true;
        }
      }
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
        SMHighlighter.Update_Highlighter();
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage($"Error in:  SMAddon.UpdateSMcontroller.  {ex}", SmUtils.LogType.Error, true);
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
            if (spaces.Current == null) continue;
            List<ICLSPart>.Enumerator parts = spaces.Current.Parts.GetEnumerator();
            while (parts.MoveNext())
            {
              if (parts.Current == null) continue;
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
            parts.Dispose();
            if (SmVessel.ClsSpaceSource != null && SmVessel.ClsSpaceTarget != null)
              break;
          }
          spaces.Dispose();
        }
        catch (Exception ex)
        {
          SmUtils.LogMessage(
            $" in UpdateCLSSpaces.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}", SmUtils.LogType.Error, true);
        }
      }
    }

    internal static bool GetClsAddon()
    {
      ClsAddon = ClsClient.GetCls();
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
        SmUtils.LogMessage(
          $" in SMAddon.GetCLSVessel.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}", SmUtils.LogType.Error, true);
        return false;
      }
    }

    internal static bool ActivateBlizzyToolBar()
    {
      if (!CurrSettings.EnableBlizzyToolbar) return false;
      if (!ToolbarManager.ToolbarAvailable) return false;
      try
      {
        if (HighLogic.LoadedScene == GameScenes.FLIGHT)
        {
          _smButtonBlizzy = ToolbarManager.Instance.add("ShipManifest", "Manifest");
          _smButtonBlizzy.TexturePath = WindowManifest.ShowWindow
            ? $"{TextureFolder}IconOn_24"
            : $"{TextureFolder}IconOff_24";
          _smButtonBlizzy.ToolTip = "Ship Manifest";
          _smButtonBlizzy.Visibility = new GameScenesVisibility(GameScenes.FLIGHT);
          _smButtonBlizzy.Visible = true;
          _smButtonBlizzy.OnClick += e => { OnSmButtonClicked(); };
        }
        else if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
        {
          _smSettingsBlizzy = ToolbarManager.Instance.add("ShipManifest", "Settings");
          _smSettingsBlizzy.TexturePath = WindowSettings.ShowWindow
            ? $"{TextureFolder}IconS_On_24"
            : $"{TextureFolder}IconS_Off_24";
          _smSettingsBlizzy.ToolTip = "Ship Manifest Settings Window";
          _smSettingsBlizzy.Visibility = new GameScenesVisibility(GameScenes.SPACECENTER);
          _smSettingsBlizzy.Visible = true;
          _smSettingsBlizzy.OnClick += e => { OnSmSettingsClicked(); };

          _smRosterBlizzy = ToolbarManager.Instance.add("ShipManifest", "Roster");
          _smRosterBlizzy.TexturePath = WindowRoster.ShowWindow
            ? $"{TextureFolder}IconR_On_24"
            : $"{TextureFolder}IconR_Off_24";
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
      if (smessages == null) return;
      IEnumerator<ScreenMessage> smessagesToRemove =
        smessages.ActiveMessages.Where(
          x =>
            Math.Abs(x.startTime - smessage.startTime) < CurrSettings.Tolerance &&
            x.style == ScreenMessageStyle.UPPER_CENTER).GetEnumerator();
      while (smessagesToRemove.MoveNext())
      {
        if (smessagesToRemove.Current == null) continue;
        ScreenMessages.RemoveMessage(smessagesToRemove.Current);
      }
      smessagesToRemove.Dispose();
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
        SmUtils.LogMessage($" in SMAddon.RunSave.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}",
          SmUtils.LogType.Error, true);
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
