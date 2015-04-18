using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine;
using ConnectedLivingSpace;

namespace ShipManifest
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    internal partial class SMAddon : MonoBehaviour
    {
        #region Properties

        //Game object that keeps us running
        internal static GameObject GameObjectInstance;  
        internal static SMController smController;
        internal static string ImageFolder = "ShipManifest/Images/";
        internal static string saveMessage = string.Empty;

        // Vessel vars
        internal static ICLSAddon clsAddon = null;

        internal static Vessel vessel = null;

        // Resource transfer vars
        internal static AudioSource source1;
        internal static AudioSource source2;
        internal static AudioSource source3;

        internal static AudioClip sound1;
        internal static AudioClip sound2;
        internal static AudioClip sound3;

        [KSPField(isPersistant = true)]
        internal static double timestamp = 0.0;

        [KSPField(isPersistant = true)]
        internal static double elapsed = 0.0;

        [KSPField(isPersistant = true)]
        internal static double flow_rate = (double)Settings.FlowRate;

        [KSPField(isPersistant = true)]
        internal static int flow_time = Settings.MaxFlowTimeSec;

        internal static double act_flow_rate = 0;

        // Resource xfer vars
        internal static XFERMode XferMode = XFERMode.SourceToTarget;
        internal static bool XferOn = false;
        internal static XFERState XferState = XFERState.Off;

        // crew xfer vars
        internal static bool crewXfer = false;
        internal static bool stockXfer = false;
        internal static double crewXferDelaySec = Settings.IVATimeDelaySec;
        internal static bool isSeat2Seat = false;
        internal static double Seat2SeatXferDelaySec = 2;

        // Toolbar Integration.
        private static IButton SMButton_Blizzy = null;
        private static IButton SMSettings_Blizzy = null;
        private static IButton SMRoster_Blizzy = null;
        private static ApplicationLauncherButton SMButton_Stock = null;
        private static ApplicationLauncherButton SMSettings_Stock = null;
        private static ApplicationLauncherButton SMRoster_Stock = null;
        internal static bool frameErrTripped = false;

        // Tooltip vars
        internal static Vector2 ToolTipPos;
        internal static string toolTip;


        #endregion


        #region Event handlers

        void DummyHandler() { }

        // Addon state event handlers
        internal void Awake()
        {
            try 
            {
                if (HighLogic.LoadedScene == GameScenes.FLIGHT || HighLogic.LoadedScene == GameScenes.SPACECENTER)
                {
                    DontDestroyOnLoad(this);
                    Settings.Load();
                    Utilities.LogMessage("SMAddon.Awake Active...", "info", Settings.VerboseLogging);

                    if (Settings.AutoSave)
                        InvokeRepeating("RunSave", Settings.SaveIntervalSec, Settings.SaveIntervalSec);

                    if (Settings.EnableBlizzyToolbar)
                    {
                        // Let't try to use Blizzy's toolbar
                        Utilities.LogMessage("SMAddon.Awake - Blizzy Toolbar Selected.", "Info", Settings.VerboseLogging);
                        if (!ActivateBlizzyToolBar())
                        {
                            // We failed to activate the toolbar, so revert to stock
                            GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
                            GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGUIAppLauncherDestroyed);
                            Utilities.LogMessage("SMAddon.Awake - Stock Toolbar Selected.", "Info", Settings.VerboseLogging);
                        }
                    }
                    else
                    {
                        // Use stock Toolbar
                        Utilities.LogMessage("SMAddon.Awake - Stock Toolbar Selected.", "Info", Settings.VerboseLogging);
                        GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
                        GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGUIAppLauncherDestroyed);
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  ShipManifestAddon.Awake.  Error:  " + ex.ToString(), "Error", true);
            }
        }
        internal void Start()
        {
            Utilities.LogMessage("SMAddon.Start.", "Info", Settings.VerboseLogging);
            try
            {
                // Reset frame error latch if set
                if (frameErrTripped)
                    frameErrTripped = false;

                if (WindowRoster.resetRosterSize)
                    Settings.RosterPosition.height = 270; //reset hight

                if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                {
                    if (GetCLSAddon())
                    {
                        Settings.CLSInstalled = true;
                    }
                    else
                    {
                        Settings.EnableCLS = false;
                        Settings.CLSInstalled = false;
                    }
                    RunSave();
                }

                if (HighLogic.LoadedScene == GameScenes.FLIGHT)
                {
                    // Instantiate Event handlers
                    GameEvents.onGameSceneLoadRequested.Add(OnGameSceneLoadRequested);
                    GameEvents.onVesselChange.Add(OnVesselChange);
                    GameEvents.onPartDie.Add(OnPartDie);
                    GameEvents.onPartExplode.Add(OnPartExplode);
                    GameEvents.onPartUndock.Add(OnPartUndock);
                    GameEvents.onStageSeparation.Add(OnStageSeparation);
                    GameEvents.onUndock.Add(OnUndock);
                    GameEvents.onVesselCreate.Add(OnVesselCreate);
                    GameEvents.onVesselDestroy.Add(OnVesselDestroy);
                    GameEvents.onVesselWasModified.Add(OnVesselWasModified);
                    GameEvents.onVesselChange.Add(OnVesselChange);
                    GameEvents.onVesselLoaded.Add(OnVesselLoaded);
                    GameEvents.onVesselTerminated.Add(OnVesselTerminated);
                    GameEvents.onFlightReady.Add(OnFlightReady);
                    GameEvents.onCrewTransferred.Add(OnCrewTransferred);
                    GameEvents.onShowUI.Add(OnShowUI);
                    GameEvents.onHideUI.Add(OnHideUI);

                    // get the current Vessel data
                    vessel = FlightGlobals.ActiveVessel;
                    smController = SMController.GetInstance(vessel);

                    // Is CLS installed and enabled?
                    if (GetCLSAddon())
                    {
                        Settings.CLSInstalled = true;
                        RunSave();
                        UpdateCLSSpaces();
                    }
                    else
                    {
                        Utilities.LogMessage("Start - CLS is not installed.", "Info", Settings.VerboseLogging);
                        Settings.EnableCLS = false;
                        Settings.CLSInstalled = false;
                        RunSave();
                    }
                    Utilities.LogMessage("CLS Installed?  " + Settings.CLSInstalled.ToString(), "Info", Settings.VerboseLogging);
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  ShipManifestAddon.Start.  " + ex.ToString(), "Error", true);
            }
        }
        internal void OnDestroy()
        {
            //Debug.Log("[ShipManifest]:  ShipManifestAddon.OnDestroy");
            try
            {
                if (Settings.Loaded)
                    RunSave();
                GameEvents.onGameSceneLoadRequested.Remove(OnGameSceneLoadRequested);
                GameEvents.onVesselWasModified.Remove(OnVesselWasModified);
                GameEvents.onVesselChange.Remove(OnVesselChange);
                GameEvents.onPartDie.Remove(OnPartDie);
                GameEvents.onPartExplode.Remove(OnPartExplode);
                GameEvents.onPartUndock.Remove(OnPartUndock);
                GameEvents.onStageSeparation.Remove(OnStageSeparation);
                GameEvents.onUndock.Remove(OnUndock);
                GameEvents.onVesselCreate.Remove(OnVesselCreate);
                GameEvents.onVesselDestroy.Remove(OnVesselDestroy);
                GameEvents.onVesselWasModified.Remove(OnVesselWasModified);
                GameEvents.onVesselChange.Remove(OnVesselChange);
                GameEvents.onVesselTerminated.Remove(OnVesselTerminated);
                GameEvents.onVesselLoaded.Remove(OnVesselLoaded);
                GameEvents.onFlightReady.Remove(OnFlightReady);
                GameEvents.onCrewTransferred.Remove(OnCrewTransferred);
                GameEvents.onHideUI.Remove(OnHideUI);
                GameEvents.onShowUI.Remove(OnShowUI);

                CancelInvoke("RunSave");

                // Handle Toolbars
                if (SMRoster_Blizzy == null && SMSettings_Blizzy == null && SMButton_Blizzy == null)
                {
                    if (SMButton_Stock != null)
                    {
                        ApplicationLauncher.Instance.RemoveModApplication(SMButton_Stock);
                        SMButton_Stock = null;
                    }
                    if (SMSettings_Stock != null)
                    {
                        ApplicationLauncher.Instance.RemoveModApplication(SMSettings_Stock);
                        SMSettings_Stock = null;
                    }
                    if (SMRoster_Stock != null)
                    {
                        ApplicationLauncher.Instance.RemoveModApplication(SMRoster_Stock);
                        SMRoster_Stock = null;
                    }
                    if (SMButton_Stock == null && SMSettings_Stock == null && SMRoster_Stock == null)
                    {
                        // Remove the stock toolbar button launcher handler
                        GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
                    }
                }
                else
                {
                    if (SMButton_Blizzy != null)
                        SMButton_Blizzy.Destroy();
                    if (SMRoster_Blizzy != null)
                        SMRoster_Blizzy.Destroy();
                    if (SMSettings_Blizzy != null)
                        SMSettings_Blizzy.Destroy();
                }
                //Reset Roster Window data
                WindowRoster.OnCreate = false;
                WindowRoster.SelectedKerbal = null;
                WindowRoster.ToolTip = "";
                //Settings.ShowRoster = false;

            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  ShipManifestAddon.OnDestroy.  " + ex.ToString(), "Error", true);
            }
        }
        internal void OnGUI()
        {
            //Debug.Log("[ShipManifest]:  ShipManifestAddon.OnGUI");
            try
            {
                Display();

                Utilities.ShowToolTips();

            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  ShipManifestAddon.OnGUI.  " + ex.ToString(), "Error", true);
            }
        }
        internal void Update()
        {
            try
            {
                CheckForToolbarTypeToggle();

                if (HighLogic.LoadedScene == GameScenes.FLIGHT)
                {
                    if (FlightGlobals.fetch != null && FlightGlobals.ActiveVessel != null)
                    {
                        //Instantiate the controller for the active vessel.
                        smController = SMController.GetInstance(vessel);
                        UpdateHighlighting();

                        // If a Stock crew Transfer occurred, let's revert the crew and activate the SM transfer mechanism...
                        if (stockXfer && !crewXfer)
                        {
                            smController.CrewXferTarget.RemoveCrewmember(smController.CrewXferMember);
                            smController.CrewXferSource.AddCrewmember(smController.CrewXferMember);
                            smController.RespawnCrew();
                            WindowTransfer.TransferCrewMemberBegin(smController.CrewXferMember, smController.CrewXferSource, smController.CrewXferTarget);
                        }
               
                        // Realism Mode Resource transfer operation (real time)
                        // XferOn is flagged in the Resource Controller
                        if (XferOn)
                            RealModePumpXfer();

                        // Realism Mode Crew transfer operation (real time)
                        // XferOn is flagged in the Resource Controller
                        if (crewXfer)
                            RealModeCrewXfer();
                    }
                }
            }
            catch (Exception ex)
            {
                if (!frameErrTripped)
                {
                    Utilities.LogMessage(string.Format(" in Update (repeating error).  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                    frameErrTripped = true;
                }
            }
        }

        // save settings on scene changes
        private void OnGameSceneLoadRequested(GameScenes RequestedScene)
        {
            Debug.Log("[ShipManifest]:  ShipManifestAddon.OnGameSceneLoadRequested");
            if (Settings.Loaded)
                RunSave();
        }

        // UI visible state handlers
        private void OnShowUI()
        {
            Debug.Log("[ShipManifest]:  ShipManifestAddon.OnShowUI");
            Settings.ShowUI = true;
        }
        private void OnHideUI()
        {
            Debug.Log("[ShipManifest]:  ShipManifestAddon.OnHideUI");
            Settings.ShowUI = false;
        }

        // Crew Event handlers
        internal void OnCrewTransferred(GameEvents.HostedFromToAction<ProtoCrewMember, Part> action)
        {
            if (!Settings.OverrideStockCrewXfer ||
                action.to.Modules.Cast<PartModule>().Any(x => x is KerbalEVA) ||
                action.from.Modules.Cast<PartModule>().Any(x => x is KerbalEVA))
            {
                // Non override and EVAs require no action
                return;
            }

            // store data from event.
            smController.CrewXferSource = action.from;
            smController.CrewXferTarget = action.to;
            smController.CrewXferMember = action.host;
            if (smController.CrewXferSource != null && smController.CrewXferTarget != null)
            {

                var message = new ScreenMessage(string.Empty, 15f, ScreenMessageStyle.LOWER_CENTER);

                // Remove the transfer message that stock displayed. 
                var messages = FindObjectOfType<ScreenMessages>();
                if (messages != null)
                {
                    var messagesToRemove = messages.activeMessages.Where(x => x.startTime == message.startTime && x.style == ScreenMessageStyle.LOWER_CENTER).ToList();
                    foreach (var m in messagesToRemove)
                        ScreenMessages.RemoveMessage(m);
                }
                stockXfer = true;
                //smController.RespawnCrew();
            }
        }

        //Vessel state handlers
        internal void OnVesselWasModified(Vessel modVessel)
        {
            Utilities.LogMessage("SMAddon.OnVesselWasModified.", "Info", Settings.VerboseLogging);
            try
            {
                UpdateSMcontroller(modVessel);
            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  ShipManifestAddon.OnVesselWasModified.  " + ex.ToString(), "Error", true);
            }
        }
        internal void OnVesselChange(Vessel newVessel)
        {
            Utilities.LogMessage("SMAddon.OnVesselChange active...", "Info", Settings.VerboseLogging);
            try
            {
                UpdateSMcontroller(newVessel);
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in SMAddon.OnVesselChange.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }
        private void OnFlightReady()
        {
            //Debug.Log("[ShipManifest]:  ShipManifestAddon.OnFlightReady");
            try
            {
                
            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  ShipManifestAddon.OnFlightReady.  " + ex.ToString(), "Error", true);
            }
        }
        private void OnVesselLoaded(Vessel data)
        {
            Utilities.LogMessage("SMAddon.OnVesselLoaded active...", "Info", Settings.VerboseLogging);
            try
            {
                if (data.Equals(FlightGlobals.ActiveVessel))
                    UpdateSMcontroller(data);
            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  SMAddon.OnVesselLoaded.  " + ex.ToString(), "Error", true);
            }
        }
        private void OnVesselTerminated(ProtoVessel data)
        {
            //Debug.Log("[ShipManifest]:  ShipManifestAddon.OnVesselTerminated");
            try
            {

            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  ShipManifestAddon.OnVesselTerminated.  " + ex.ToString(), "Error", true);
            }
        }
        private void OnPartDie(Part data)
        {
            //Debug.Log("[ShipManifest]:  ShipManifestAddon.OnPartDie");
            try
            {

            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  ShipManifestAddon.OnPartDie.  " + ex.ToString(), "Error", true);
            }
        }
        private void OnPartExplode(GameEvents.ExplosionReaction data)
        {
            //Debug.Log("[ShipManifest]:  ShipManifestAddon.OnPartExplode");
            try
            {

            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  ShipManifestAddon.OnPartExplode.  " + ex.ToString(), "Error", true);
            }
        }
        private void OnPartUndock(Part data)
        {
            //Debug.Log("[ShipManifest]:  ShipManifestAddon.OnPartUndock");
            try
            {
                Utilities.LogMessage("OnPartUnDock:  Active. - Part name:  " + data.partInfo.name, "Info", Settings.VerboseLogging);
                if (smController.SelectedResource == "Crew")
                {
                    if (Settings.EnableCLS)
                        SMAddon.clsAddon.Vessel.Highlight(false);
                    else
                    {
                        smController.SelectedResource = null;
                        UpdateHighlighting();
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  ShipManifestAddon.OnPartUndock.  " + ex.ToString(), "Error", true);
            }
        }
        private void OnStageSeparation(EventReport eventReport)
        {
            //Debug.Log("[ShipManifest]:  ShipManifestAddon.OnStageSeparation");
            try
            {
            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  ShipManifestAddon.OnStageSeparation.  " + ex.ToString(), "Error", true);
            }
        }
        private void OnUndock(EventReport eventReport)
        {
            //Debug.Log("[ShipManifest]:  ShipManifestAddon.OnUndock");
            try
            {

            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  ShipManifestAddon.OnUndock.  " + ex.ToString(), "Error", true);
            }
        }
        private void OnVesselDestroy(Vessel data)
        {
            //Debug.Log("[ShipManifest]:  ShipManifestAddon.OnVesselDestroy");
            try
            {

            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  ShipManifestAddon.OnVesselDestroy.  " + ex.ToString(), "Error", true);
            }
        }
        private void OnVesselCreate(Vessel data)
        {
            //Debug.Log("[ShipManifest]:  ShipManifestAddon.OnVesselCreate");
            try
            {

            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  ShipManifestAddon.OnVesselCreate.  " + ex.ToString(), "Error", true);
            }
        }

        // Stock vs Blizzy Toolbar switch handler
        private void CheckForToolbarTypeToggle()
        {
            if (Settings.EnableBlizzyToolbar && !Settings.prevEnableBlizzyToolbar)
            {
                // Let't try to use Blizzy's toolbar
                Utilities.LogMessage("CheckForToolbarToggle - Blizzy Toolbar Selected.", "Info", Settings.VerboseLogging);
                if (!ActivateBlizzyToolBar())
                {
                    // We failed to activate the toolbar, so revert to stock
                    GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
                    GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGUIAppLauncherDestroyed);

                    Utilities.LogMessage("SMAddon.Awake - Stock Toolbar Selected.", "Info", Settings.VerboseLogging);
                    Settings.EnableBlizzyToolbar = Settings.prevEnableBlizzyToolbar;
                }
                else
                {
                    OnGUIAppLauncherDestroyed();
                    GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
                    GameEvents.onGUIApplicationLauncherDestroyed.Remove(OnGUIAppLauncherDestroyed);
                    Settings.prevEnableBlizzyToolbar = Settings.EnableBlizzyToolbar;
                    if (HighLogic.LoadedSceneIsFlight)
                        SMButton_Blizzy.Visible = true;
                    if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                    {
                        SMRoster_Blizzy.Visible = true;
                        SMSettings_Blizzy.Visible = true;
                    }
                }

            }
            else if (!Settings.EnableBlizzyToolbar && Settings.prevEnableBlizzyToolbar)
            {
                // Use stock Toolbar
                Utilities.LogMessage("SMAddon.Awake - Stock Toolbar Selected.", "Info", Settings.VerboseLogging);
                if (HighLogic.LoadedSceneIsFlight)
                    SMButton_Blizzy.Visible = false;
                if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                {
                    SMRoster_Blizzy.Visible = false;
                    SMSettings_Blizzy.Visible = false;
                }
                GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
                GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGUIAppLauncherDestroyed);
                OnGUIAppLauncherReady();
                Settings.prevEnableBlizzyToolbar = Settings.EnableBlizzyToolbar;
            }
        }

        // Stock Toolbar Startup and cleanup
        private void OnGUIAppLauncherReady()
        {
            Utilities.LogMessage("SMAddon.OnGUIAppLauncherReady active...", "Info", Settings.VerboseLogging);
            try
            {
                // Setup SM WIndow button
                if (HighLogic.LoadedSceneIsFlight && SMButton_Stock == null && !Settings.EnableBlizzyToolbar)
                {
                    string Iconfile = "IconOff_38"; 
                    SMButton_Stock = ApplicationLauncher.Instance.AddModApplication(
                        OnSMButtonToggle,
                        OnSMButtonToggle,
                        DummyHandler,
                        DummyHandler,
                        DummyHandler,
                        DummyHandler,
                        ApplicationLauncher.AppScenes.FLIGHT,
                        (Texture)GameDatabase.Instance.GetTexture(ImageFolder + Iconfile, false));

                    if (Settings.ShowShipManifest)
                        SMButton_Stock.SetTexture((Texture)GameDatabase.Instance.GetTexture(Settings.ShowShipManifest ? ImageFolder + "IconOn_38" : ImageFolder + "IconOff_38", false));
                }

                // Setup Settings Button
                if (HighLogic.LoadedScene == GameScenes.SPACECENTER && SMSettings_Stock == null && !Settings.EnableBlizzyToolbar)
                {
                    string Iconfile = "IconS_Off_38";
                    SMSettings_Stock = ApplicationLauncher.Instance.AddModApplication(
                        OnSMSettingsToggle,
                        OnSMSettingsToggle,
                        DummyHandler,
                        DummyHandler,
                        DummyHandler,
                        DummyHandler,
                        ApplicationLauncher.AppScenes.SPACECENTER,
                        (Texture)GameDatabase.Instance.GetTexture(ImageFolder + Iconfile, false));

                    if (Settings.ShowSettings)
                        SMSettings_Stock.SetTexture((Texture)GameDatabase.Instance.GetTexture(Settings.ShowSettings ? ImageFolder + "IconS_On_38" : ImageFolder + "IconS_Off_38", false));
                }

                // Setup Roster Button
                if (HighLogic.LoadedScene == GameScenes.SPACECENTER && SMRoster_Stock == null && !Settings.EnableBlizzyToolbar)
                {
                    string Iconfile = "IconR_Off_38";
                    SMRoster_Stock = ApplicationLauncher.Instance.AddModApplication(
                        OnSMRosterToggle,
                        OnSMRosterToggle,
                        DummyHandler,
                        DummyHandler,
                        DummyHandler,
                        DummyHandler,
                        ApplicationLauncher.AppScenes.SPACECENTER,
                        (Texture)GameDatabase.Instance.GetTexture(ImageFolder + Iconfile, false));

                    if (Settings.ShowRoster)
                        SMRoster_Stock.SetTexture((Texture)GameDatabase.Instance.GetTexture(Settings.ShowRoster ? ImageFolder + "IconR_On_38" : ImageFolder + "IconR_Off_38", false));
                }

            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  ShipManifestAddon.OnGUIAppLauncherReady.  " + ex.ToString(), "Error", true);
            }
        }
        private void OnGUIAppLauncherDestroyed()
        {
            //Debug.Log("[ShipManifest]:  ShipManifestAddon.OnGUIAppLauncherDestroyed");
            try
            {
                if (SMButton_Stock != null)
                {
                    ApplicationLauncher.Instance.RemoveModApplication(SMButton_Stock);
                    SMButton_Stock = null;
                }
                if (SMRoster_Stock != null)
                {
                    ApplicationLauncher.Instance.RemoveModApplication(SMRoster_Stock);
                    SMRoster_Stock = null;
                }
                if (SMSettings_Stock != null)
                {
                    ApplicationLauncher.Instance.RemoveModApplication(SMSettings_Stock);
                    SMSettings_Stock = null;
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  ShipManifestAddon.OnGUIAppLauncherDestroyed.  " + ex.ToString(), "Error", true);
            }
        }

        //Toolbar button click handlers
        internal static void OnSMButtonToggle()
        {
            //Debug.Log("[ShipManifest]:  ShipManifestAddon.OnSMButtonToggle");
            try
            {
                if (Settings.ShowShipManifest)
                {
                    // SM is showing.  Turn off.
                    if (SMAddon.crewXfer || SMAddon.XferOn)
                        return;

                    SMAddon.smController.SelectedResource = null;
                    SMAddon.smController.SelectedPartsSource = SMAddon.smController.SelectedPartsTarget = new List<Part>();
                    Settings.ShowShipManifest = !Settings.ShowShipManifest;
                }
                else
                {
                    // SM is not showing. turn on if we can.
                    if (SMAddon.CanShowShipManifest(true))
                        Settings.ShowShipManifest = !Settings.ShowShipManifest;
                    else
                        return;
                }

                if (Settings.EnableBlizzyToolbar)
                    SMButton_Blizzy.TexturePath = Settings.ShowShipManifest ? ImageFolder + "IconOn_24" : ImageFolder + "IconOff_24";
                else
                    SMButton_Stock.SetTexture((Texture)GameDatabase.Instance.GetTexture(Settings.ShowShipManifest ? ImageFolder + "IconOn_38" : ImageFolder + "IconOff_38", false));

            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  ShipManifestAddon.OnSMButtonToggle.  " + ex.ToString(), "Error", true);
            }
        }
        internal static void OnSMRosterToggle()
        {
            //Debug.Log("[ShipManifest]:  ShipManifestAddon.OnSMRosterToggle");
            try
            {
                if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                {
                    Settings.ShowRoster = !Settings.ShowRoster;
                    if (Settings.EnableBlizzyToolbar)
                        SMRoster_Blizzy.TexturePath = Settings.ShowRoster ? ImageFolder + "IconR_On_24" : ImageFolder + "IconR_Off_24";
                    else
                        SMRoster_Stock.SetTexture((Texture)GameDatabase.Instance.GetTexture(Settings.ShowRoster ? ImageFolder + "IconR_On_38" : ImageFolder + "IconR_Off_38", false));
                }

            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  ShipManifestAddon.OnSMRosterToggleOn.  " + ex.ToString(), "Error", true);
            }
        }
        internal static void OnSMSettingsToggle()
        {
            //Debug.Log("[ShipManifest]:  ShipManifestAddon.OnSMRosterToggle");
            try
            {
                if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                {
                    Settings.ShowSettings = !Settings.ShowSettings;
                    if (Settings.EnableBlizzyToolbar)
                        SMSettings_Blizzy.TexturePath = Settings.ShowSettings ? ImageFolder + "IconS_On_24" : ImageFolder + "IconS_Off_24";
                    else
                        SMSettings_Stock.SetTexture((Texture)GameDatabase.Instance.GetTexture(Settings.ShowSettings ? ImageFolder + "IconS_On_38" : ImageFolder + "IconS_Off_38", false));
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  ShipManifestAddon.OnSMSettingsToggle.  " + ex.ToString(), "Error", true);
            }
        }
        
        #endregion

        #region Logic Methods

        internal static bool CanKerbalsBeXferred(List<Part> SelectedPartsSource, List<Part> SelectedPartsTarget)
        {
            bool results = false;
            try
            {
                if (SMAddon.crewXfer || SMAddon.XferOn)
                {
                    WindowTransfer.xferToolTip = "Transfer in progress.  Xfers disabled.";
                    return results;
                }
                // Are the parts capable of holding kerbals and are there kerbals to move?
                if ((SelectedPartsTarget.Count > 0 && SelectedPartsSource[0] != SelectedPartsTarget[0]) && SelectedPartsSource[0].protoModuleCrew.Count > 0)
                {
                    // now if realism mode, are the parts connected to each other in the same living space?
                    results = IsCLSInSameSpace(SelectedPartsSource[0], SelectedPartsTarget[0]);
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in CanBeXferred.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
            if (WindowTransfer.xferToolTip == "")
                WindowTransfer.xferToolTip = "Source and target Part are the same.  Use Move Kerbal instead.";
            return results;
        }

        //internal static bool shouldToggle()
        //{
        //    //Debug.Log("[ShipManifest]:  ShipManifestAddon.shouldToggle");
        //    return (HighLogic.LoadedScene == GameScenes.FLIGHT && !MapView.MapIsEnabled && !PauseMenu.isOpen && !FlightResultsDialog.isDisplaying &&
        //                    FlightGlobals.fetch != null && FlightGlobals.ActiveVessel != null &&
        //                    smController.CanDrawButton
        //                    );
        //}

        private static bool IsCLSInSameSpace(Part SelectedPartSource, Part SelectedPartTarget)
        {
            bool results = false;
            try
            {
                if (SelectedPartSource == null || SelectedPartTarget == null)
                {
                    WindowTransfer.xferToolTip = "Source or Target Part is not selected.\r\nPlease Select a Source AND a Target part.";
                    return false;
                }
                if (SelectedPartSource == SelectedPartTarget)
                {
                    WindowTransfer.xferToolTip = "Source and Target Part are the same.\r\nyou may Transfer, or use Move Kerbal to the left instead.";
                    return true;
                }
                    
                if (Settings.EnableCLS && Settings.RealismMode)
                {
                    if (SMAddon.clsAddon.Vessel != null)
                    {
                        if (smController.clsSpaceSource != null && smController.clsSpaceTarget != null)
                        {
                            if (smController.clsSpaceSource == smController.clsSpaceTarget)
                            {
                                WindowTransfer.xferToolTip = "Source & Target Part are in the same space.\r\nInternal Xfers are allowed.";
                                results = true;
                            }
                            else
                                WindowTransfer.xferToolTip = "Source and Target parts are not in the same Living Space.\r\nKerbals will have to go EVA.";
                        }
                        else
                            WindowTransfer.xferToolTip = "Either the Source or Target Part is not selected.\r\nPlease Select a Source AND Target part.";
                    }
                    else
                        WindowTransfer.xferToolTip = "You should NOT be seeing this, as CLS is not behaving correctly.\r\nPlease check your CLS installation.";
                }
                else
                {
                    WindowTransfer.xferToolTip = "Realism or CLS are disabled.\r\nXfers anywhere are Allowed.";
                    results = true;
                }
            }
            catch (Exception ex)
            {
                if (!frameErrTripped)
                {
                    Utilities.LogMessage(string.Format(" in IsInCLS (repeating error).  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                    frameErrTripped = true;
                }
            }
            //Utilities.LogMessage("IsInCLS() - results = " + results.ToString() , "info", Settings.VerboseLogging);
            return results;
        }

        internal static bool CanShowShipManifest(bool IgnoreShowSM = false)
        {
            try
            {
                bool canShow = false;
                if (Settings.ShowUI
                    && HighLogic.LoadedScene == GameScenes.FLIGHT
                    && !MapView.MapIsEnabled
                    && !isPauseMenuOpen()
                    && !isFlightDialogDisplaying()
                    && FlightGlobals.fetch != null
                    && FlightGlobals.ActiveVessel != null
                    && !FlightGlobals.ActiveVessel.isEVA
                    && FlightGlobals.ActiveVessel.vesselType != VesselType.Flag
                    && FlightGlobals.ActiveVessel.vesselType != VesselType.Debris
                    && FlightGlobals.ActiveVessel.vesselType != VesselType.Unknown
                    && CameraManager.Instance.currentCameraMode != CameraManager.CameraMode.IVA
                    )
                    if (IgnoreShowSM)
                        canShow = true;
                    else
                        canShow = Settings.ShowShipManifest;
                return canShow;
            }
            catch (Exception ex)
            {
                if (!frameErrTripped)
                {
                    string values = "Settings.ShowUI = " + Settings.ShowUI.ToString() + "\r\n";
                    values += "HighLogic.LoadedScene = " + HighLogic.LoadedScene.ToString() + "\r\n";
                    values += "!MapView.MapIsEnabled = " +MapView.MapIsEnabled.ToString() + "\r\n";
                    values += "PauseMenu.isOpen = " + isPauseMenuOpen().ToString() + "\r\n";
                    values += "FlightResultsDialog.isDisplaying = " + isFlightDialogDisplaying().ToString() + "\r\n";
                    values += "FlightGlobals.fetch != null = " + (FlightGlobals.fetch != null).ToString() + "\r\n";
                    values += "FlightGlobals.ActiveVessel != null = " + (FlightGlobals.ActiveVessel != null).ToString() + "\r\n";
                    values += "!FlightGlobals.ActiveVessel.isEVA = " + FlightGlobals.ActiveVessel.isEVA.ToString() + "\r\n";
                    values += "FlightGlobals.ActiveVessel.vesselType = " + FlightGlobals.ActiveVessel.vesselType.ToString() + "\r\n";
                    values += "CameraManager.Instance.currentCameraMode != CameraManager.CameraMode.IVA = " + (CameraManager.Instance.currentCameraMode != CameraManager.CameraMode.IVA).ToString();

                    Utilities.LogMessage(string.Format(" in CanShowShipManifest (repeating error).  Error:  {0} \r\n\r\n{1}\r\n\r\nValues:  {2}", ex.Message, ex.StackTrace, values), "Error", true);
                    frameErrTripped = true;
                }
                return false;
            }
        }

        internal static bool isFlightDialogDisplaying()
        {
            try
            {
                return FlightResultsDialog.isDisplaying;
            }
            catch
            {
                return false;
            }
        }
        internal static bool isPauseMenuOpen()
        {
            try
            {
                return PauseMenu.isOpen;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region Action Methods

        internal void UpdateSMcontroller(Vessel newVessel)
        {
            try
            {
                if (vessel != null && SMAddon.CanShowShipManifest(false))
                {
                    if (newVessel.isEVA && !vessel.isEVA)
                    {
                        if (Settings.ShowShipManifest == true)
                            OnSMButtonToggle();

                        // kill selected resource and its associated highlighting.
                        smController.SelectedResource = null;
                        //smController.SelectedResourceParts = null;
                        Utilities.LogMessage("New Vessel is a Kerbal on EVA.  ", "Info", Settings.VerboseLogging);
                    }
                }

                // Now let's update the current vessel view...
                vessel = newVessel;
                smController = SMController.GetInstance(vessel);
                if (Settings.EnableCLS && SMAddon.CanShowShipManifest(false))
                {
                    if (GetCLSAddon())
                        UpdateCLSSpaces();
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  ShipManifestAddon.UpdateSMcontroller.  " + ex.ToString(), "Error", true);
            }
        }
        
        internal static void UpdateCLSSpaces()
        {
            GetCLSVessel();
            if (clsAddon.Vessel != null)
            {
                try
                {
                    smController.clsPartSource = null;
                    smController.clsSpaceSource = null;
                    foreach (ICLSSpace sSpace in SMAddon.clsAddon.Vessel.Spaces)
                    {
                        foreach (ICLSPart sPart in sSpace.Parts)
                        {
                            if (smController.SelectedPartsSource.Contains(sPart.Part))
                            {
                                smController.clsPartSource = sPart;
                                smController.clsSpaceSource = sSpace;
                                Utilities.LogMessage("UpdateCLSSpaces - clsPartSource found;", "info", Settings.VerboseLogging);
                                break;
                            }
                        }
                        if (smController.clsSpaceSource == null)
                            Utilities.LogMessage("UpdateCLSSpaces - clsSpaceSource is null.", "info", Settings.VerboseLogging);
                    }
                    smController.clsPartTarget = null;
                    smController.clsSpaceTarget = null;
                    foreach (ICLSSpace tSpace in SMAddon.clsAddon.Vessel.Spaces)
                    {
                        foreach (ICLSPart tPart in tSpace.Parts)
                        {
                            if (smController.SelectedPartsTarget.Contains(tPart.Part))
                            {
                                smController.clsPartTarget = tPart;
                                smController.clsSpaceTarget = tSpace;
                                Utilities.LogMessage("UpdateCLSSpaces - clsPartTarget found;", "info", Settings.VerboseLogging);
                                break;
                            }
                        }
                    }
                    if (smController.clsSpaceTarget == null)
                        Utilities.LogMessage("UpdateCLSSpaces - clsSpaceTarget is null.", "info", Settings.VerboseLogging);
                    smController.GetHatches();
                }
                catch (Exception ex)
                {
                    Utilities.LogMessage(string.Format(" in UpdateCLSSpaces.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                }
            }
            else
                Utilities.LogMessage("UpdateCLSSpaces - clsVessel is null... done.", "info", Settings.VerboseLogging);
        }

        internal static bool GetCLSAddon()
        {
            clsAddon = CLSClient.GetCLS();
            if (clsAddon == null)
            {
                Utilities.LogMessage("GetCLSVessel - clsAddon is null.", "Info", Settings.VerboseLogging);
                return false;
            }
            return true;
        }

        internal static bool GetCLSVessel()
        {
            try
            {
                Utilities.LogMessage("GetCLSVessel - Active.", "Info", Settings.VerboseLogging);

                if (SMAddon.clsAddon.Vessel != null)
                {
                    return true;
                }
                else
                {
                    Utilities.LogMessage("GetCLSVessel - clsVessel is null.", "Info", Settings.VerboseLogging);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in GetCLSVessel.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                return false;
            }          
        }

        internal static bool ActivateBlizzyToolBar()
        {
            if (Settings.EnableBlizzyToolbar)
            {
                try
                {
                    if (ToolbarManager.ToolbarAvailable)
                    {
                        if (HighLogic.LoadedScene == GameScenes.FLIGHT)
                        {
                            SMButton_Blizzy = ToolbarManager.Instance.add("ShipManifest", "Manifest");
                            SMButton_Blizzy.TexturePath = Settings.ShowShipManifest ? ImageFolder + "IconOn_24" : ImageFolder + "IconOff_24";
                            SMButton_Blizzy.ToolTip = "Ship Manifest";
                            SMButton_Blizzy.Visibility = new GameScenesVisibility(GameScenes.FLIGHT);
                            SMButton_Blizzy.Visible = true;
                            SMButton_Blizzy.OnClick += (e) =>
                            {
                                OnSMButtonToggle();
                            };
                        }

                        if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                        {
                            
                            SMSettings_Blizzy = ToolbarManager.Instance.add("ShipManifest", "Settings");
                            SMSettings_Blizzy.TexturePath = Settings.ShowSettings ? ImageFolder + "IconS_On_24" : ImageFolder + "IconS_Off_24";
                            SMSettings_Blizzy.ToolTip = "Ship Manifest Settings Window";
                            SMSettings_Blizzy.Visibility = new GameScenesVisibility(GameScenes.SPACECENTER);
                            SMSettings_Blizzy.Visible = true;
                            SMSettings_Blizzy.OnClick += (e) =>
                            {
                                OnSMSettingsToggle();
                            };

                            SMRoster_Blizzy = ToolbarManager.Instance.add("ShipManifest", "Roster");
                            SMRoster_Blizzy.TexturePath = Settings.ShowRoster ? ImageFolder + "IconR_On_24" : ImageFolder + "IconR_Off_24";
                            SMRoster_Blizzy.ToolTip = "Ship Manifest Roster Window";
                            SMRoster_Blizzy.Visibility = new GameScenesVisibility(GameScenes.SPACECENTER);
                            SMRoster_Blizzy.Visible = true;
                            SMRoster_Blizzy.OnClick += (e) =>
                            {
                                OnSMRosterToggle();
                            };
                        }
                        Utilities.LogMessage("Blizzy Toolbar available!", "Info", Settings.VerboseLogging);
                        return true;
                    }
                    else
                    {
                        Utilities.LogMessage("Blizzy Toolbar not available!", "Info", Settings.VerboseLogging);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    // Blizzy Toolbar instantiation error.
                    Utilities.LogMessage("Error in EnableBlizzyToolbar... Error:  " + ex, "Error", true);
                    return false;
                }
            }
            else
            {
                // No Blizzy Toolbar
                Utilities.LogMessage("Blizzy Toolbar not Enabled...", "Info", Settings.VerboseLogging);
                return false;
            }
        }

        internal void Display()
        {
            string step = "";
            try
            {
                step = "0 - Start";
                SMStyle.SetupGUI();

                if (Settings.ShowDebugger)
                    Settings.DebuggerPosition = GUILayout.Window(398643, Settings.DebuggerPosition, WindowDebugger.Display, " Ship Manifest -  Debug Console - Ver. " + Settings.CurVersion, GUILayout.MinHeight(20));

                if (HighLogic.LoadedScene == GameScenes.FLIGHT || HighLogic.LoadedScene == GameScenes.SPACECENTER)
                {
                    if (Settings.ShowSettings && Settings.ShowUI)
                    {
                        step = "4 - Show Settings";
                        Settings.SettingsPosition = GUILayout.Window(398546, Settings.SettingsPosition, WindowSettings.Display, "Ship Manifest Settings", GUILayout.MinHeight(20));
                    }

                    if (Settings.ShowRoster && Settings.ShowUI)
                    {
                        if (WindowRoster.resetRosterSize)
                        {
                            step = "5 - Reset Roster Size";
                            Settings.RosterPosition.height = 270; //reset hight
                        }

                        step = "6 - Show Roster";
                        Settings.RosterPosition = GUILayout.Window(398547, Settings.RosterPosition, WindowRoster.Display, "Ship Manifest Roster", GUILayout.MinHeight(20));
                    }
                }
                if (HighLogic.LoadedScene == GameScenes.FLIGHT && (FlightGlobals.fetch == null || FlightGlobals.ActiveVessel != vessel))
                {
                    step = "0a - Vessel Change";
                    smController.SelectedPartsSource = smController.SelectedPartsTarget = new List<Part>();
                    smController.SelectedResource = null;
                    return;
                }

                step = "1 - Show Interface(s)";
                // Is the scene one we want to be visible in?
                if (SMAddon.CanShowShipManifest(false))
                {
                    // What windows do we want to show?
                    step = "2 - Can Show Manifest - true";
                    Settings.ManifestPosition = GUILayout.Window(398544, Settings.ManifestPosition, WindowManifest.Display, "Ship's Manifest - " + vessel.vesselName, GUILayout.MinHeight(20));
                        
                    if (Settings.ShowTransferWindow && smController.SelectedResource != null)
                    {
                        step = "3 - Show Transfer";
                        // Lets build the running totals for each resource for display in title...
                        string DisplayAmounts = Utilities.DisplayVesselResourceTotals(smController.SelectedResource);
                        Settings.TransferPosition = GUILayout.Window(398545, Settings.TransferPosition, WindowTransfer.Display, "Transfer - " + vessel.vesselName + DisplayAmounts, GUILayout.MinHeight(20));
                    }

                    if (Settings.ShowShipManifest && Settings.ShowControl)
                    {
                        step = "7 - Show Hatches";
                        Settings.ControlPosition = GUILayout.Window(398548, Settings.ControlPosition, WindowControl.Display, "Ship Manifest Part Control Center", GUILayout.MinWidth(350), GUILayout.MinHeight(20));
                    }
                }
                else
                {
                    step = "2 - Can Show Manifest = false";
                    if (Settings.EnableCLS && smController != null)
                        if (smController.SelectedResource == "Crew")
                            SMAddon.HighlightCLSVessel(false, true);  
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in Display at or near step:  " + step + ".  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        private void RealModePumpXfer()
        {
            try
            {
                if (XferOn)
                {
                    double deltaT = 0;
                    flow_rate = Settings.FlowRate;

                    switch (XferState)
                    {
                        case XFERState.Off:
                            // reset counters
                            timestamp = 0;

                            // Default sound license: CC-By-SA
                            // http://www.freesound.org/people/vibe_crc/sounds/59328/
                            string path1 = Settings.PumpSoundStart; // "ShipManifest/Sounds/59328-1";
                            string path2 = Settings.PumpSoundRun;   // "ShipManifest/Sounds/59328-2";
                            string path3 = Settings.PumpSoundStop;  // "ShipManifest/Sounds/59328-3";

                            // Load Sounds, and Play Sound 1
                            LoadSounds("Pump", path1, path2, path3, Settings.PumpSoundVol);
                            XferState = XFERState.Start;
                            break;

                        case XFERState.Start:

                            // calculate elapsed.
                            elapsed += Planetarium.GetUniversalTime();

                            // Play run sound when start sound is nearly done. (repeats)
                            if (elapsed >= source1.clip.length - 0.25)
                            {
                                source2.Play();
                                elapsed = 0;
                                XferState = XFERState.Run;
                            }
                            break;

                        case XFERState.Run:

                            deltaT = Planetarium.GetUniversalTime() - timestamp;

                            double deltaAmt = deltaT * act_flow_rate;
                            Utilities.LogMessage("1. DeltaT = " + deltaT.ToString() + "\r\n    FlowRate = " + act_flow_rate.ToString() + "\r\n    DeltaAmt = " + deltaAmt.ToString(), "Info", Settings.VerboseLogging);

                            // This adjusts the delta when we get to the end of the xfer.
                            double XferAmount = smController.sXferAmount;
                            // which way we going?
                            if (XferMode == XFERMode.TargetToSource)
                                XferAmount = smController.tXferAmount;

                            if (smController.AmtXferred + deltaAmt >= XferAmount)
                            {
                                deltaAmt = XferAmount - smController.AmtXferred;
                                XferState = XFERState.Stop;
                                Utilities.LogMessage("2. Adjusted DeltaAmt = " + deltaAmt.ToString(), "Info", Settings.VerboseLogging);
                            }
                            Utilities.LogMessage("3. DeltaAmt = " + deltaAmt.ToString(), "Info", Settings.VerboseLogging);

                            // Consume electricity
                            double deltaCharge = deltaT * act_flow_rate * Settings.FlowCost;
                            if (!ConsumeCharge(deltaCharge))
                            {
                                XferState = XFERState.Stop;
                            }
                            else
                            {
                                // We had enough electricity, so lets move the resource....
                                List<Part> PartsSource = smController.SelectedPartsSource;
                                List<Part> PartsTarget = smController.SelectedPartsTarget;
                                if (XferMode == XFERMode.TargetToSource)
                                {
                                    // Source is target on Interface...
                                    PartsSource = smController.SelectedPartsTarget;
                                    PartsTarget = smController.SelectedPartsSource;
                                }
                                Utilities.LogMessage("4. AmtXferred = " + smController.AmtXferred.ToString(), "Info", Settings.VerboseLogging);

                                double AmtRemaining = WindowTransfer.CalcResourceRemaining(PartsSource, smController.SelectedResource);
                                double AmtRemianCapacity = WindowTransfer.CalcRemainingCapacity(PartsTarget, smController.SelectedResource);
                                if (deltaAmt > AmtRemaining)
                                    deltaAmt = AmtRemaining;
                                if (deltaAmt > AmtRemianCapacity)
                                    deltaAmt = AmtRemianCapacity;

                                Utilities.LogMessage("5. Adjusted deltaAmt = " + deltaAmt.ToString(), "Info", Settings.VerboseLogging);
                                if (deltaAmt > 0)
                                {
                                    XferResource(PartsSource, smController.SelectedResource, deltaAmt, true);
                                    XferResource(PartsTarget, smController.SelectedResource, deltaAmt, false);
                                    smController.AmtXferred += deltaAmt;
                                }
                                else if (smController.AmtXferred == XferAmount)
                                    XferState = XFERState.Stop;
                            }
                            break;

                        case XFERState.Stop:

                            // play pump shutdown.
                            source2.Stop();
                            source3.Play();
                            timestamp = elapsed = 0;
                            XferState = XFERState.Off;
                            smController.AmtXferred = 0;
                            smController.sXferAmount = WindowTransfer.CalcMaxXferAmt(smController.SelectedPartsSource, smController.SelectedPartsTarget, SMAddon.smController.SelectedResource);
                            if (smController.sXferAmount < 0.0001)
                                smController.sXferAmount = 0;
                            WindowTransfer.strTXferAmount = smController.sXferAmount.ToString();
                            smController.tXferAmount = WindowTransfer.CalcMaxXferAmt(smController.SelectedPartsTarget, smController.SelectedPartsSource, SMAddon.smController.SelectedResource);
                            if (smController.tXferAmount < 0.0001)
                                smController.tXferAmount = 0;
                            WindowTransfer.strTXferAmount = smController.tXferAmount.ToString();
                            XferOn = false;
                            break;
                    }
                    Utilities.LogMessage("Transfer State:  " + XferState.ToString() + "...", "Info", Settings.VerboseLogging);
                    if (XferState != XFERState.Off)
                        timestamp = Planetarium.GetUniversalTime();
                }
            }
            catch (Exception ex)
            {
                if (!frameErrTripped)
                {
                    Utilities.LogMessage(string.Format(" in RealModePumpXfer (repeating error).  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                    frameErrTripped = true;
                    throw ex;
                }
            }
        }

        private void RealModeCrewXfer()
        {
            try
            {
                if (crewXfer)
                {
                    Part PartSource = smController.CrewXferSource;
                    Part PartTarget = smController.CrewXferTarget;
                    ProtoCrewMember pKerbal = smController.CrewXferMember;
                    if (!Settings.RealismMode)
                    {
                        if (timestamp != 0)
                            elapsed += Planetarium.GetUniversalTime() - timestamp;

                        if (elapsed > 1)
                        {
                            // Fire Board event for Texture Replacer.
                            //if (Settings.EnableTextureReplacer)
                                //GameEvents.onCrewBoardVessel.Fire(smController.evaAction);

                            // Spawn crew in parts and in vessel.
                            if (PartSource == null)
                                PartSource = PartTarget;
                            if (PartTarget == null)
                                PartTarget = PartSource;

                            elapsed = timestamp = 0;
                            crewXfer = false;
                            WindowTransfer.TransferCrewMemberComplete(smController.CrewXferMember, PartSource, PartTarget);

                            Utilities.LogMessage("crewXfer State:  " + crewXfer.ToString() + "...", "Info", Settings.VerboseLogging);
                        }
                        if (crewXfer)
                            timestamp = Planetarium.GetUniversalTime();
                    }
                    else
                    {
                        switch (XferState)
                        {
                            case XFERState.Off:
                                // We're just starting loop, so set some evnironment stuff.
                                timestamp = 0;

                                // Default sound license: CC-By-SA
                                // http://www.freesound.org/people/adcbicycle/sounds/14214/
                                string path1 = Settings.CrewSoundStart; // "ShipManifest/Sounds/14214-1";
                                string path2 = Settings.CrewSoundRun;   // "ShipManifest/Sounds/14214-2";
                                string path3 = Settings.CrewSoundStop;  // "ShipManifest/Sounds/14214-3";

                                LoadSounds("Crew", path1, path2, path3, Settings.CrewSoundVol);
                                XferState = XFERState.Start;
                                break;

                            case XFERState.Start:

                                elapsed += Planetarium.GetUniversalTime() - timestamp;

                                // Play run sound when start sound is nearly done. (repeats)
                                if (elapsed >= source1.clip.length - 0.25)
                                {
                                    source2.Play();
                                    elapsed = 0;
                                    XferState = XFERState.Run;
                                }
                                break;

                            case XFERState.Run:

                                elapsed += Planetarium.GetUniversalTime() - timestamp;

                                // wait for movement to end...
                                if (elapsed >= crewXferDelaySec || (isSeat2Seat && elapsed > Seat2SeatXferDelaySec))
                                {
                                    // Reset State vars
                                    XferState = XFERState.Stop;
                                }
                                break;

                            case XFERState.Stop:

                                // Spawn crew in parts and in vessel.
                                if (PartSource == null)
                                    PartSource = PartTarget;
                                if (PartTarget == null)
                                    PartTarget = PartSource;

                                // play crew sit.
                                source2.Stop();
                                source3.Play();
                                SMAddon.timestamp = elapsed = 0;
                                XferState = XFERState.Off;
                                WindowTransfer.TransferCrewMemberComplete(smController.CrewXferMember, PartSource, PartTarget);
                                crewXfer = false;
                                isSeat2Seat = false;
                                if (stockXfer)
                                { 
                                    var message = new ScreenMessage(string.Empty, 15f, ScreenMessageStyle.LOWER_CENTER);
                                    ScreenMessages.PostScreenMessage(string.Format("<color=yellow>{0} moved (by SM) to {1}.</color>", smController.CrewXferMember.name, PartTarget.partInfo.title), message, true);
                                }
                                stockXfer = false;

                                // Fire Board event for Texture Replacer.
                                //if (Settings.EnableTextureReplacer)
                                //    GameEvents.onCrewBoardVessel.Fire(smController.evaAction);

                                // Notify Mods requiring it to update (Texture Replacer Kerbal (IVA) textures, ConnectedLivingSpaces.
                                Utilities.LogMessage("RealModeCrewXfer:  Updating Portraits...", "info", Settings.VerboseLogging);
                                break;
                        }
                        Utilities.LogMessage("Transfer State:  " + XferState.ToString() + "...", "Info", Settings.VerboseLogging);
                        if (XferState != XFERState.Off)
                            timestamp = Planetarium.GetUniversalTime();
                        else
                            Utilities.LogMessage("RealModeCrewAXfer:  Complete.", "info", Settings.VerboseLogging);
                    }
                }
            }
            catch (Exception ex)
            {
                if (!frameErrTripped)
                {
                    Utilities.LogMessage("Transfer State:  " + XferState.ToString() + "...", "Error", true);
                    Utilities.LogMessage(string.Format(" in RealModeCrewXfer (repeating error).  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                    XferState = XFERState.Stop;
                    frameErrTripped = true;
                }
            }
        }

        private bool ConsumeCharge(double deltaCharge)
        {
            if (smController.SelectedResource != "ElectricCharge" && Settings.EnableXferCost)
            {
                foreach (Part iPart in smController._partsByResource["ElectricCharge"])
                {
                    if (iPart.Resources["ElectricCharge"].amount >= deltaCharge)
                    {
                        iPart.Resources["ElectricCharge"].amount -= deltaCharge;
                        deltaCharge = 0;
                        break;
                    }
                    else
                    {
                        deltaCharge -= iPart.Resources["ElectricCharge"].amount;
                        iPart.Resources["ElectricCharge"].amount = 0;
                    }
                }
                if (deltaCharge > 0)
                    return false;
                else
                    return true;
            }
            else
                return true;
        }

        internal static void XferResource(List<Part> XferParts, string SelectedResource, double XferAmount, bool isSource)
        {

            // This var keeps track of what we actually moved..
            double XferBalance = XferAmount;

            // Not all parts will have enough resource to meet the SrcPartAmt to move.   We need to account for that.
            while (XferBalance > 0)
            {
                // Lets account for any empty/full containers
                int PartCount = 0;
                foreach (Part part in XferParts)
                    if ((isSource && part.Resources[SelectedResource].amount > 0) || (!isSource && part.Resources[SelectedResource].amount < part.Resources[SelectedResource].maxAmount))
                        PartCount += 1;

                // now split up the xfer amount evenly across the number of tanks that can send/receive resources
                double PartAmt = XferAmount / PartCount;

                // Calculate Xfer amounts for each part and move.
                foreach (Part part in XferParts)
                {
                    double AmtToMove = 0;
                    if (isSource)
                    {
                        AmtToMove = part.Resources[SelectedResource].amount >= PartAmt ? PartAmt : part.Resources[SelectedResource].amount;
                        part.Resources[SelectedResource].amount -= AmtToMove;
                    }
                    else
                    {
                        double CapacityAvail = part.Resources[SelectedResource].maxAmount - part.Resources[SelectedResource].amount;
                        AmtToMove = CapacityAvail >= PartAmt ? PartAmt : CapacityAvail;
                        part.Resources[SelectedResource].amount += AmtToMove;
                    }
                    // Report ramaining balance after Transfer.
                    XferBalance -= AmtToMove;
                }
            }
        }

        private void LoadSounds(string SoundType, string path1, string path2, string path3, double dblVol)
        {
            try
            {
                elapsed = 0;
                Utilities.LogMessage("Loading " + SoundType + " sounds...", "Info", Settings.VerboseLogging);

                GameObject go = new GameObject("Audio");

                source1 = go.AddComponent<AudioSource>();
                source2 = go.AddComponent<AudioSource>();
                source3 = go.AddComponent<AudioSource>();

                if (GameDatabase.Instance.ExistsAudioClip(path1) && GameDatabase.Instance.ExistsAudioClip(path2) && GameDatabase.Instance.ExistsAudioClip(path3))
                {
                    sound1 = GameDatabase.Instance.GetAudioClip(path1);
                    sound2 = GameDatabase.Instance.GetAudioClip(path2);
                    sound3 = GameDatabase.Instance.GetAudioClip(path3);
                    Utilities.LogMessage(SoundType + " sounds loaded...", "Info", Settings.VerboseLogging);

                    // configure sources
                    source1.clip = sound1; // Start sound
                    source1.volume = (float)dblVol;
                    source1.pitch = 1f;

                    source2.clip = sound2; // Run sound
                    source2.loop = true;
                    source2.volume = (float)dblVol;
                    source2.pitch = 1f;

                    source3.clip = sound3; // Stop Sound
                    source3.volume = (float)dblVol;
                    source3.pitch = 1f;

                    // now let's play the Pump start sound.
                    source1.Play();
                    Utilities.LogMessage("Play " + SoundType + " sound (start)...", "Info", Settings.VerboseLogging);
                }
                else
                {
                    Utilities.LogMessage(SoundType + " sound failed to load...", "Info", Settings.VerboseLogging);
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in LoadSounds.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                throw ex;
            }
        }

        internal void RunSave()
        {
            try
            {
                Utilities.LogMessage("RunSave in progress...", "info", Settings.VerboseLogging);
                Save();
                Utilities.LogMessage("RunSave complete.", "info", Settings.VerboseLogging);
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in RunSave.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        private void Save()
        {
            try
            {
                if (HighLogic.LoadedScene == GameScenes.FLIGHT || HighLogic.LoadedScene == GameScenes.SPACECENTER)
                {
                    Utilities.LogMessage("Save in progress...", "info", Settings.VerboseLogging);
                    Settings.Save();
                    Utilities.LogMessage("Save comlete.", "info", Settings.VerboseLogging);
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in Save.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        internal static void FireEventTriggers()
        {
            // Per suggestion by shaw (http://forum.kerbalspaceprogram.com/threads/62270?p=1033866&viewfull=1#post1033866)
            // and instructions for using CLS API by codepoet.
            Utilities.LogMessage("FireEventTriggers:  Active.", "info", Settings.VerboseLogging);
            GameEvents.onVesselChange.Fire(vessel);
        }

        #endregion

        #region Highlighting methods

        internal static void ClearPartsHighlight(List<Part> parts)
        {
            if (parts != null)
            {
                foreach (Part part in parts)
                    ClearPartHighlight(part);
            }
             
        }
        
        /// <summary>
        /// Remove highlighting on a part.
        /// </summary>
        /// <param name="part">Part to remove highlighting from.</param>
        internal static void ClearPartHighlight(Part part)
        {
            try
            {
                if (part != null)
                {
                    part.SetHighlight(false, false);
                    part.SetHighlightDefault();
                    part.highlightType = Part.HighlightType.OnMouseOver;
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in  ClearPartHighlight.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        /// <summary>
        /// Removes Highlighting on parts belonging to the selected resource list.
        /// </summary>
        /// <param name="_ResourceParts"></param>
        internal static void ClearResourceHighlighting(List<Part> _ResourceParts)
        {
            if (_ResourceParts != null)
            {
                foreach (Part part in _ResourceParts)
                {
                    ClearPartHighlight(part);
                }
                if (Settings.EnableCLS && Settings.EnableCLSHighlighting && SMAddon.clsAddon.Vessel != null)
                    SMAddon.clsAddon.Vessel.Highlight(false);
            }
        }

        internal static void SetPartsHighlight(List<Part> parts, Color color)
        {
            try
            {
                foreach (Part part in parts)
                {
                    SetPartHighlight(part, color);
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in  SetPartsHighlight.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        internal static void SetPartHighlight(Part part, Color color)
        {
            try
            {

                if (part != null)
                {
                    part.SetHighlightColor(color);
                    part.SetHighlight(true, false);
                    part.highlightType = Part.HighlightType.AlwaysOn;
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in  SetPartHighlight.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        internal static void UpdateHighlighting()
        {
            string step = "";
            try
            {
                // Do we even want to highlight?
                if (Settings.EnableHighlighting)
                {
                    step = "Showhipmanifest = true";
                    if (SMAddon.CanShowShipManifest(false))
                    {
                        if (Settings.EnableCLS && Settings.EnableCLSHighlighting && SMAddon.clsAddon.Vessel != null)
                        {
                            if (smController.SelectedResource == "Crew")
                            {
                                step = "Highlight CLS vessel";
                                HighlightCLSVessel(true);

                                // Turn off the source and target cls highlighting.  We are going to replace it.
                                if (smController.clsPartSource != null)
                                    smController.clsPartSource.Highlight(false, true);
                                if (smController.clsPartTarget != null)
                                    smController.clsPartTarget.Highlight(false, true);
                            }
                        }

                        step = "Set Selected Part Colors";
                        SMAddon.SetPartsHighlight(smController.SelectedPartsSource, Settings.Colors[Settings.SourcePartColor]);
                        if (smController.SelectedResource == "Crew" && Settings.EnableCLS)
                            SetPartsHighlight(smController.SelectedPartsTarget, Settings.Colors[Settings.TargetPartCrewColor]);
                        else
                            SetPartsHighlight(smController.SelectedPartsTarget, Settings.Colors[Settings.TargetPartColor]);

                        // Default is yellow
                        step = "Set non selected resource part color";
                        Color partColor = Color.yellow;

                        // match color used by CLS if active
                        if (smController.SelectedResource == "Crew" && Settings.EnableCLS)
                            partColor = Color.green;

                        step = "Set Resource Part Colors";
                        if (smController.SelectedResource != null && smController.SelectedResource != "")
                        {
                            foreach (Part thispart in smController._partsByResource[smController.SelectedResource])
                            {
                                if (!Settings.OnlySourceTarget)
                                {
                                    if (!smController.SelectedPartsSource.Contains(thispart) && !smController.SelectedPartsTarget.Contains(thispart))
                                    {
                                        SetPartHighlight(thispart, partColor);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (!SMAddon.frameErrTripped)
                {
                    Utilities.LogMessage(string.Format(" in SMAddon.UpdateHighlighting (repeating error).  Error in step:  " + step + ".  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                    SMAddon.frameErrTripped = true;
                }
            }
        }

        internal static void HighlightCLSVessel(bool enabled, bool force = false)
        {
            try
            {
                //SMAddon.UpdateCLSSpaces();
                if (SMAddon.clsAddon.Vessel == null)
                    SMAddon.UpdateCLSSpaces();
                if (SMAddon.clsAddon.Vessel != null)
                {
                    foreach (ICLSSpace Space in SMAddon.clsAddon.Vessel.Spaces)
                    {
                        foreach (ICLSPart part in Space.Parts)
                        {
                            part.Highlight(enabled, force);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (!SMAddon.frameErrTripped)
                {
                    Utilities.LogMessage(string.Format(" in HighlightCLSVessel (repeating error).  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                    SMAddon.frameErrTripped = true;
                }
            }
        }

        #endregion


        internal enum XFERState
        {
            Off,
            Start,
            Run,
            Stop
        }

        internal enum XFERMode
        {
            SourceToTarget,
            TargetToSource
        }

    }

    internal class ShipManifestModule : PartModule
    {
        [KSPEvent(guiActive = true, guiName = "Destroy Part", active = true)]
        internal void DestoryPart()
        {
            if (this.part != null)
                this.part.temperature = 5000;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (this.part != null && part.name == "ShipManifest")
                Events["DestoryPart"].active = true;
            else
                Events["DestoryPart"].active = false;
        }
    }

}