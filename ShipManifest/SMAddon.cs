using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine;
using HighlightingSystem;
using ConnectedLivingSpace;
using DF;


namespace ShipManifest
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    internal partial class SMAddon : MonoBehaviour
    {
        #region Properties

        //Game object that keeps us running
        //internal static GameObject SmInstance;  
        internal static SMController smController;
        internal static string TextureFolder = "ShipManifest/Textures/";
        internal static string saveMessage = string.Empty;

        // DeepFreeze Frozen Crew interface
        internal static Dictionary<string, KerbalInfo> FrozenKerbals = new Dictionary<string, KerbalInfo>();
        
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
        internal static double elapsed = 0.0;

        // Resource xfer vars
        internal static XFERMode XferMode = XFERMode.SourceToTarget;

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

        internal static bool ShowUI = true;

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
                    SMSettings.LoadSettings();
                    Utilities.LogMessage("SMAddon.Awake Active...", "info", SMSettings.VerboseLogging);

                    if (SMSettings.AutoSave)
                        InvokeRepeating("RunSave", SMSettings.SaveIntervalSec, SMSettings.SaveIntervalSec);

                    if (SMSettings.EnableBlizzyToolbar)
                    {
                        // Let't try to use Blizzy's toolbar
                        Utilities.LogMessage("SMAddon.Awake - Blizzy Toolbar Selected.", "Info", SMSettings.VerboseLogging);
                        if (!ActivateBlizzyToolBar())
                        {
                            // We failed to activate the toolbar, so revert to stock
                            GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
                            GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGUIAppLauncherDestroyed);
                            Utilities.LogMessage("SMAddon.Awake - Stock Toolbar Selected.", "Info", SMSettings.VerboseLogging);
                        }
                    }
                    else
                    {
                        // Use stock Toolbar
                        Utilities.LogMessage("SMAddon.Awake - Stock Toolbar Selected.", "Info", SMSettings.VerboseLogging);
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
            Utilities.LogMessage("SMAddon.Start.", "Info", SMSettings.VerboseLogging);
            try
            {
                // Reset frame error latch if set
                if (frameErrTripped)
                    frameErrTripped = false;

                if (WindowRoster.resetRosterSize)
                    if (SMSettings.UseUnityStyle)
                        WindowRoster.Position.height = 330; //reset hight
                    else
                        WindowRoster.Position.height = 350; //reset hight

                if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                {
                    if (GetCLSAddon())
                    {
                        SMSettings.CLSInstalled = true;
                    }
                    else
                    {
                        SMSettings.EnableCLS = false;
                        SMSettings.CLSInstalled = false;
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
                        SMSettings.CLSInstalled = true;
                        RunSave();
                        UpdateCLSSpaces();
                    }
                    else
                    {
                        Utilities.LogMessage("Start - CLS is not installed.", "Info", SMSettings.VerboseLogging);
                        SMSettings.EnableCLS = false;
                        SMSettings.CLSInstalled = false;
                        RunSave();
                    }
                    Utilities.LogMessage("CLS Installed?  " + SMSettings.CLSInstalled.ToString(), "Info", SMSettings.VerboseLogging);
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
                if (SMSettings.Loaded)
                {
                    RunSave();
                    SMSettings.SaveSettings();
                }
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
                //WindowRoster.ShowWindow = false;

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
                if (SMSettings.UseUnityStyle)
                    GUI.skin = null;
                else
                    GUI.skin = HighLogic.Skin;

                SMStyle.SetupGUIStyles();
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
                        SMHighlighter.Update_Highlighter();
               
                        // Realism Mode Resource transfer operation (real time)
                        // XferOn is flagged in the Resource Controller
                        if (TransferResource.ResourceXferActive)
                        {
                            TransferResource.ResourceTransferProcess();
                        }

                        // Realism Mode Crew transfer operation (real time)
                        if (smController.CrewTransfer.CrewXferActive)
                            smController.CrewTransfer.CrewTransferProcess();
                        else if (smController.CrewTransfer.IsStockXfer)
                        {
                            TransferCrew.RevertCrewTransfer(smController.CrewTransfer.FromCrewMember, smController.CrewTransfer.FromPart, smController.CrewTransfer.ToPart);
                            smController.CrewTransfer.CrewTransferBegin(smController.CrewTransfer.FromCrewMember, smController.CrewTransfer.FromPart, smController.CrewTransfer.ToPart);
                        }

                        if (SMSettings.EnableOnCrewTransferEvent && TransferCrew.FireSourceXferEvent)
                        {
                            // Now let's deal with third party mod support...
                            TransferCrew.FireSourceXferEvent = false;
                            GameEvents.onCrewTransferred.Fire(TransferCrew.SourceAction);

                            //If a swap, we need to handle that too...
                            if (TransferCrew.FireTargetXferEvent)
                            {
                                TransferCrew.FireTargetXferEvent = false;
                                GameEvents.onCrewTransferred.Fire(TransferCrew.TargetAction);
                            }
                        }
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
            if (SMSettings.Loaded)
            {
                RunSave();
                SMSettings.SaveSettings();
            }
        }

        // UI visible state handlers
        private void OnShowUI()
        {
            Debug.Log("[ShipManifest]:  ShipManifestAddon.OnShowUI");
            SMAddon.ShowUI = true;
        }
        private void OnHideUI()
        {
            Debug.Log("[ShipManifest]:  ShipManifestAddon.OnHideUI");
            SMAddon.ShowUI = false;
        }

        // Crew Event handlers
        internal void OnCrewTransferred(GameEvents.HostedFromToAction<ProtoCrewMember, Part> action)
        {
            if ((action.host == TransferCrew.SourceAction.host && action.from == TransferCrew.SourceAction.from && action.to == TransferCrew.SourceAction.to)
                || action.host == TransferCrew.TargetAction.host && action.from == TransferCrew.TargetAction.from && action.to == TransferCrew.TargetAction.to)
            {
                // We are performing a mod notification. Ignore the event.
                return;
            }
            else if (!smController.CrewTransfer.CrewXferActive && (!SMSettings.OverrideStockCrewXfer ||
                action.to.Modules.Cast<PartModule>().Any(x => x is KerbalEVA) ||
                action.from.Modules.Cast<PartModule>().Any(x => x is KerbalEVA)))
            {
                // no SM crew Xfers in progress, so Non-override stock Xfers and EVAs require no action
                return;
            }

            if (smController.CrewTransfer.CrewXferActive)
            {
                // Remove the transfer message that stock displayed. 
                string failMessage = string.Format("<color=orange>{0} is unable to xfer to {1}.  An SM Crew Xfer is in progress</color>", action.host.name, action.to.partInfo.title);
                DisplayScreenMsg(failMessage);
                TransferCrew.RevertCrewTransfer(action.host, action.from, action.to);
                return;
            }
            else
            {
                //Check for DeepFreezer full. if full, abort handling Xfer.
                if (DFInterface.IsDFInstalled && action.to.Modules.Contains("DeepFreezer"))
                    if (((IDeepFreezer)action.to.Modules["DeepFreezer"]).DFIPartFull)
                        return;

                // If we are here, then we want to override the Stock Xfer...
                RemoveScreenMsg();

                // store data from event.
                smController.CrewTransfer.FromPart = action.from;
                smController.CrewTransfer.ToPart = action.to;
                smController.CrewTransfer.FromCrewMember = action.host;
                if (smController.CrewTransfer.FromPart != null && smController.CrewTransfer.ToPart != null)
                    smController.CrewTransfer.IsStockXfer = true;
            }
        }

        internal static void DisplayScreenMsg(string strMessage)
        {
            var smessage = new ScreenMessage(string.Empty, 15f, ScreenMessageStyle.LOWER_CENTER);
            var smessages = FindObjectOfType<ScreenMessages>();
            if (smessages != null)
            {
                var smessagesToRemove = smessages.activeMessages.Where(x => x.startTime == smessage.startTime && x.style == ScreenMessageStyle.LOWER_CENTER).ToList();
                foreach (var m in smessagesToRemove)
                    ScreenMessages.RemoveMessage(m);
                var failmessage = new ScreenMessage(string.Empty, 15f, ScreenMessageStyle.UPPER_CENTER);
                ScreenMessages.PostScreenMessage(strMessage, failmessage, true);
            }
        }

        internal static void RemoveScreenMsg()
        {
            var smessage = new ScreenMessage(string.Empty, 15f, ScreenMessageStyle.LOWER_CENTER);
            var smessages = FindObjectOfType<ScreenMessages>();
            if (smessages != null)
            {
                var smessagesToRemove = smessages.activeMessages.Where(x => x.startTime == smessage.startTime && x.style == ScreenMessageStyle.LOWER_CENTER).ToList();
                foreach (var m in smessagesToRemove)
                    ScreenMessages.RemoveMessage(m);
            }
        }

        //Vessel state handlers
        internal void OnVesselWasModified(Vessel modVessel)
        {
            Utilities.LogMessage("SMAddon.OnVesselWasModified.", "Info", SMSettings.VerboseLogging);
            try
            {
                SMHighlighter.ClearResourceHighlighting(smController.SelectedResourcesParts);
                UpdateSMcontroller(modVessel);
            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  ShipManifestAddon.OnVesselWasModified.  " + ex.ToString(), "Error", true);
            }
        }
        internal void OnVesselChange(Vessel newVessel)
        {
            //Utilities.LogMessage("SMAddon.OnVesselChange active...", "Info", true);
            Utilities.LogMessage("SMAddon.OnVesselChange active...", "Info", SMSettings.VerboseLogging);
            try
            {
                SMHighlighter.ClearResourceHighlighting(smController.SelectedResourcesParts);
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
            Utilities.LogMessage("SMAddon.OnVesselLoaded active...", "Info", SMSettings.VerboseLogging);
            try
            {
                if (data.Equals(FlightGlobals.ActiveVessel) && data != smController.Vessel)
                {
                    SMHighlighter.ClearResourceHighlighting(smController.SelectedResourcesParts);
                    UpdateSMcontroller(data);
                }
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
                Utilities.LogMessage("OnPartUnDock:  Active. - Part name:  " + data.partInfo.name, "Info", SMSettings.VerboseLogging);
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
            if (SMSettings.EnableBlizzyToolbar && !SMSettings.prevEnableBlizzyToolbar)
            {
                // Let't try to use Blizzy's toolbar
                Utilities.LogMessage("CheckForToolbarToggle - Blizzy Toolbar Selected.", "Info", SMSettings.VerboseLogging);
                if (!ActivateBlizzyToolBar())
                {
                    // We failed to activate the toolbar, so revert to stock
                    GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
                    GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGUIAppLauncherDestroyed);

                    Utilities.LogMessage("SMAddon.Awake - Stock Toolbar Selected.", "Info", SMSettings.VerboseLogging);
                    SMSettings.EnableBlizzyToolbar = SMSettings.prevEnableBlizzyToolbar;
                }
                else
                {
                    OnGUIAppLauncherDestroyed();
                    GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
                    GameEvents.onGUIApplicationLauncherDestroyed.Remove(OnGUIAppLauncherDestroyed);
                    SMSettings.prevEnableBlizzyToolbar = SMSettings.EnableBlizzyToolbar;
                    if (HighLogic.LoadedSceneIsFlight)
                        SMButton_Blizzy.Visible = true;
                    if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                    {
                        SMRoster_Blizzy.Visible = true;
                        SMSettings_Blizzy.Visible = true;
                    }
                }

            }
            else if (!SMSettings.EnableBlizzyToolbar && SMSettings.prevEnableBlizzyToolbar)
            {
                // Use stock Toolbar
                Utilities.LogMessage("SMAddon.Awake - Stock Toolbar Selected.", "Info", SMSettings.VerboseLogging);
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
                SMSettings.prevEnableBlizzyToolbar = SMSettings.EnableBlizzyToolbar;
            }
        }

        // Stock Toolbar Startup and cleanup
        private void OnGUIAppLauncherReady()
        {
            Utilities.LogMessage("SMAddon.OnGUIAppLauncherReady active...", "Info", SMSettings.VerboseLogging);
            try
            {
                // Setup SM WIndow button
                if (HighLogic.LoadedSceneIsFlight && SMButton_Stock == null && !SMSettings.EnableBlizzyToolbar)
                {
                    string Iconfile = "IconOff_38"; 
                    SMButton_Stock = ApplicationLauncher.Instance.AddModApplication(
                        OnSMButtonToggle,
                        OnSMButtonToggle,
                        DummyHandler,
                        DummyHandler,
                        DummyHandler,
                        DummyHandler,
                        ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW,
                        (Texture)GameDatabase.Instance.GetTexture(TextureFolder + Iconfile, false));

                    if (WindowManifest.ShowWindow)
                        SMButton_Stock.SetTexture((Texture)GameDatabase.Instance.GetTexture(WindowManifest.ShowWindow ? TextureFolder + "IconOn_38" : TextureFolder + "IconOff_38", false));
                }

                // Setup Settings Button
                if (HighLogic.LoadedScene == GameScenes.SPACECENTER && SMSettings_Stock == null && !SMSettings.EnableBlizzyToolbar)
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
                        (Texture)GameDatabase.Instance.GetTexture(TextureFolder + Iconfile, false));

                    if (WindowSettings.ShowWindow)
                        SMSettings_Stock.SetTexture((Texture)GameDatabase.Instance.GetTexture(WindowSettings.ShowWindow ? TextureFolder + "IconS_On_38" : TextureFolder + "IconS_Off_38", false));
                }

                // Setup Roster Button
                if (HighLogic.LoadedScene == GameScenes.SPACECENTER && SMRoster_Stock == null && !SMSettings.EnableBlizzyToolbar)
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
                        (Texture)GameDatabase.Instance.GetTexture(TextureFolder + Iconfile, false));

                    if (WindowRoster.ShowWindow)
                        SMRoster_Stock.SetTexture((Texture)GameDatabase.Instance.GetTexture(WindowRoster.ShowWindow ? TextureFolder + "IconR_On_38" : TextureFolder + "IconR_Off_38", false));
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
                if (WindowManifest.ShowWindow)
                {
                    // SM is showing.  Turn off.
                    if (smController.CrewTransfer.CrewXferActive || TransferResource.ResourceXferActive)
                        return;

                    SMHighlighter.ClearResourceHighlighting(SMAddon.smController.SelectedResourcesParts);
                    SMAddon.smController.SelectedResources.Clear();
                    SMAddon.smController.SelectedPartsSource.Clear();
                    SMAddon.smController.SelectedPartsTarget.Clear();
                    WindowManifest.ShowWindow = !WindowManifest.ShowWindow;
                }
                else
                {
                    // SM is not showing. turn on if we can.
                    if (SMAddon.CanShowShipManifest(true))
                        WindowManifest.ShowWindow = !WindowManifest.ShowWindow;
                    else
                        return;
                }

                if (SMSettings.EnableBlizzyToolbar)
                    SMButton_Blizzy.TexturePath = WindowManifest.ShowWindow ? TextureFolder + "IconOn_24" : TextureFolder + "IconOff_24";
                else
                    SMButton_Stock.SetTexture((Texture)GameDatabase.Instance.GetTexture(WindowManifest.ShowWindow ? TextureFolder + "IconOn_38" : TextureFolder + "IconOff_38", false));

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
                    WindowRoster.ShowWindow = !WindowRoster.ShowWindow;
                    if (SMSettings.EnableBlizzyToolbar)
                        SMRoster_Blizzy.TexturePath = WindowRoster.ShowWindow ? TextureFolder + "IconR_On_24" : TextureFolder + "IconR_Off_24";
                    else
                        SMRoster_Stock.SetTexture((Texture)GameDatabase.Instance.GetTexture(WindowRoster.ShowWindow ? TextureFolder + "IconR_On_38" : TextureFolder + "IconR_Off_38", false));

                    SMAddon.FrozenKerbals = WindowRoster.GetFrozenKerbals();
                }

            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  ShipManifestAddon.OnSMRosterToggle.  " + ex.ToString(), "Error", true);
            }
        }
        internal static void OnSMSettingsToggle()
        {
            //Debug.Log("[ShipManifest]:  ShipManifestAddon.OnSMRosterToggle");
            try
            {
                if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                {
                    WindowSettings.ShowWindow = !WindowSettings.ShowWindow;
                    SMSettings.StoreTempSettings();
                    if (SMSettings.EnableBlizzyToolbar)
                        SMSettings_Blizzy.TexturePath = WindowSettings.ShowWindow ? TextureFolder + "IconS_On_24" : TextureFolder + "IconS_Off_24";
                    else
                        SMSettings_Stock.SetTexture((Texture)GameDatabase.Instance.GetTexture(WindowSettings.ShowWindow ? TextureFolder + "IconS_On_38" : TextureFolder + "IconS_Off_38", false));
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
                if (SMAddon.smController.CrewTransfer.CrewXferActive || TransferResource.ResourceXferActive)
                {
                    WindowTransfer.xferToolTip = "Transfer in progress.  Xfers disabled.";
                    return results;
                }
                if (SelectedPartsSource.Count == 0 || SelectedPartsTarget.Count == 0)
                {
                    WindowTransfer.xferToolTip = "Source or Target Part is not selected.\r\nPlease Select a Source AND a Target part.";
                    return results;
                }
                if (SelectedPartsSource[0] == SelectedPartsTarget[0])
                {
                    WindowTransfer.xferToolTip = "Source and Target Part are the same.\r\nUse Move Kerbal (>>) instead.";
                    return results;
                }

                // Are there kerbals to move?
                if (SelectedPartsSource[0].protoModuleCrew.Count == 0)
                {
                    WindowTransfer.xferToolTip = "No Kerbals to Move.";
                    return results;
                }
                // now if realism mode, are the parts connected to each other in the same living space?
                results = IsCLSInSameSpace(SelectedPartsSource[0], SelectedPartsTarget[0]);
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in CanBeXferred.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
            if (WindowTransfer.xferToolTip == "")
                WindowTransfer.xferToolTip = "Source and target Part are the same.  Use Move Kerbal instead.";
            return results;
        }

        private static bool IsCLSInSameSpace(Part SelectedPartSource, Part SelectedPartTarget)
        {
            bool results = false;
            try
            {
                if (SMSettings.EnableCLS && SMSettings.RealismMode)
                {
                    if (SMAddon.clsAddon.Vessel != null)
                    {
                        if (smController.clsSpaceSource == null || smController.clsSpaceTarget == null)
                            UpdateCLSSpaces();
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
                            WindowTransfer.xferToolTip = "You should NOT be seeing this, as Source or Target Space is missing.\r\nPlease reselect source or target part.";
                    }
                    else
                        WindowTransfer.xferToolTip = "You should NOT be seeing this, as CLS is not behaving correctly.\r\nPlease check your CLS installation.";
                }
                else
                {
                    WindowTransfer.xferToolTip = "Realism and/or CLS disabled.\r\nXfers anywhere are allowed.";
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
                if (SMAddon.ShowUI
                    && HighLogic.LoadedScene == GameScenes.FLIGHT
                    //&& !MapView.MapIsEnabled
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
                        canShow = WindowManifest.ShowWindow;
                return canShow;
            }
            catch (Exception ex)
            {
                if (!frameErrTripped)
                {
                    string values = "SMAddon.ShowUI = " + SMAddon.ShowUI.ToString() + "\r\n";
                    values += "HighLogic.LoadedScene = " + HighLogic.LoadedScene.ToString() + "\r\n";
                    //values += "!MapView.MapIsEnabled = " +MapView.MapIsEnabled.ToString() + "\r\n";
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
                SMHighlighter.ClearResourceHighlighting(smController.SelectedResourcesParts);
                if (vessel != newVessel)
                {
                    if(smController.CrewTransfer.CrewXferActive && !smController.CrewTransfer.IvaDelayActive)
                        smController.CrewTransfer.CrewTransferAbort();
                    if (TransferResource.ResourceXferActive && SMSettings.RealismMode)
                        TransferResource.ResourceTransferAbort();
                }

                if (vessel != null && SMAddon.CanShowShipManifest(false))
                {
                    if (newVessel.isEVA && !vessel.isEVA)
                    {
                        if (WindowManifest.ShowWindow == true)
                            OnSMButtonToggle();

                        // kill selected resource and its associated highlighting.
                        smController.SelectedResources.Clear();
                        Utilities.LogMessage("New Vessel is a Kerbal on EVA.  ", "Info", SMSettings.VerboseLogging);
                    }
                }

                // Now let's update the current vessel view...
                vessel = newVessel;
                smController = SMController.GetInstance(vessel);
                smController.RefreshLists();
            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  ShipManifestAddon.UpdateSMcontroller.  " + ex.ToString(), "Error", true);
            }
        }
        
        internal static void UpdateCLSSpaces()
        {
            if (GetCLSVessel())
            {
                try
                {
                    smController.clsPartSource = null;
                    smController.clsSpaceSource = null;
                    smController.clsPartTarget = null;
                    smController.clsSpaceTarget = null;
                    foreach (ICLSSpace sSpace in SMAddon.clsAddon.Vessel.Spaces)
                    {
                        foreach (ICLSPart sPart in sSpace.Parts)
                        {
                            if (smController.SelectedPartsSource.Contains(sPart.Part) && smController.clsPartSource == null)
                            {
                                smController.clsPartSource = sPart;
                                smController.clsSpaceSource = sSpace;
                                Utilities.LogMessage("UpdateCLSSpaces - clsPartSource found;", "info", SMSettings.VerboseLogging);
                            }
                            if (smController.SelectedPartsTarget.Contains(sPart.Part) && smController.clsPartTarget == null)
                            {
                                smController.clsPartTarget = sPart;
                                smController.clsSpaceTarget = sSpace;
                                Utilities.LogMessage("UpdateCLSSpaces - clsPartTarget found;", "info", SMSettings.VerboseLogging);
                            }
                            if (smController.clsPartSource != null && smController.clsPartTarget != null)
                                break;
                        }
                        if (smController.clsSpaceSource != null && smController.clsSpaceTarget != null)
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Utilities.LogMessage(string.Format(" in UpdateCLSSpaces.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                }
            }
            else
                Utilities.LogMessage("UpdateCLSSpaces - clsVessel is null... done.", "info", SMSettings.VerboseLogging);
        }

        internal static bool GetCLSAddon()
        {
            clsAddon = CLSClient.GetCLS();
            if (clsAddon == null)
            {
                Utilities.LogMessage("GetCLSVessel - clsAddon is null.", "Info", SMSettings.VerboseLogging);
                return false;
            }
            return true;
        }

        internal static bool GetCLSVessel()
        {
            try
            {
                Utilities.LogMessage("GetCLSVessel - Active.", "Info", SMSettings.VerboseLogging);

                if (SMAddon.clsAddon.Vessel != null)
                {
                    return true;
                }
                else
                {
                    Utilities.LogMessage("GetCLSVessel - clsVessel is null.", "Info", SMSettings.VerboseLogging);
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
            if (SMSettings.EnableBlizzyToolbar)
            {
                try
                {
                    if (ToolbarManager.ToolbarAvailable)
                    {
                        if (HighLogic.LoadedScene == GameScenes.FLIGHT)
                        {
                            SMButton_Blizzy = ToolbarManager.Instance.add("ShipManifest", "Manifest");
                            SMButton_Blizzy.TexturePath = WindowManifest.ShowWindow ? TextureFolder + "IconOn_24" : TextureFolder + "IconOff_24";
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
                            SMSettings_Blizzy.TexturePath = WindowSettings.ShowWindow ? TextureFolder + "IconS_On_24" : TextureFolder + "IconS_Off_24";
                            SMSettings_Blizzy.ToolTip = "Ship Manifest Settings Window";
                            SMSettings_Blizzy.Visibility = new GameScenesVisibility(GameScenes.SPACECENTER);
                            SMSettings_Blizzy.Visible = true;
                            SMSettings_Blizzy.OnClick += (e) =>
                            {
                                OnSMSettingsToggle();
                            };

                            SMRoster_Blizzy = ToolbarManager.Instance.add("ShipManifest", "Roster");
                            SMRoster_Blizzy.TexturePath = WindowRoster.ShowWindow ? TextureFolder + "IconR_On_24" : TextureFolder + "IconR_Off_24";
                            SMRoster_Blizzy.ToolTip = "Ship Manifest Roster Window";
                            SMRoster_Blizzy.Visibility = new GameScenesVisibility(GameScenes.SPACECENTER);
                            SMRoster_Blizzy.Visible = true;
                            SMRoster_Blizzy.OnClick += (e) =>
                            {
                                OnSMRosterToggle();
                            };
                        }
                        Utilities.LogMessage("Blizzy Toolbar available!", "Info", SMSettings.VerboseLogging);
                        return true;
                    }
                    else
                    {
                        Utilities.LogMessage("Blizzy Toolbar not available!", "Info", SMSettings.VerboseLogging);
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
                Utilities.LogMessage("Blizzy Toolbar not Enabled...", "Info", SMSettings.VerboseLogging);
                return false;
            }
        }

        internal void Display()
        {
            string step = "";
            try
            {
                step = "0 - Start";
                if (WindowDebugger.ShowWindow)
                    WindowDebugger.Position = GUILayout.Window(398643, WindowDebugger.Position, WindowDebugger.Display, " Ship Manifest -  Debug Console - Ver. " + SMSettings.CurVersion, GUILayout.MinHeight(20));

                if (HighLogic.LoadedScene == GameScenes.FLIGHT || HighLogic.LoadedScene == GameScenes.SPACECENTER)
                {
                    if (WindowSettings.ShowWindow && SMAddon.ShowUI)
                    {
                        step = "4 - Show Settings";
                        WindowSettings.Position = GUILayout.Window(398546, WindowSettings.Position, WindowSettings.Display, "Ship Manifest Settings", GUILayout.MinHeight(20));
                    }

                    if (WindowRoster.ShowWindow && SMAddon.ShowUI)
                    {
                        if (WindowRoster.resetRosterSize)
                            if (SMSettings.UseUnityStyle)
                                WindowRoster.Position.height = 330; //reset hight
                            else
                                WindowRoster.Position.height = 350; //reset hight

                        step = "6 - Show Roster";
                        WindowRoster.Position = GUILayout.Window(398547, WindowRoster.Position, WindowRoster.Display, "Ship Manifest Roster", GUILayout.MinHeight(20));
                    }
                }
                if (HighLogic.LoadedScene == GameScenes.FLIGHT && (FlightGlobals.fetch == null || FlightGlobals.ActiveVessel != vessel))
                {
                    step = "0a - Vessel Change";
                    smController.SelectedPartsSource.Clear();
                    smController.SelectedPartsTarget.Clear();
                    smController.SelectedResources.Clear();
                    return;
                }

                step = "1 - Show Interface(s)";
                // Is the scene one we want to be visible in?
                if (SMAddon.CanShowShipManifest(false))
                {
                    // What windows do we want to show?
                    step = "2 - Can Show Manifest - true";
                    WindowManifest.Position = GUILayout.Window(398544, WindowManifest.Position, WindowManifest.Display, "Ship's Manifest - " + vessel.vesselName, GUILayout.MinHeight(20));
                        
                    if (WindowTransfer.ShowWindow && smController.SelectedResources.Count > 0)
                    {
                        step = "3 - Show Transfer";
                        // Lets build the running totals for each resource for display in title...
                        string DisplayAmounts = Utilities.DisplayVesselResourceTotals(smController.SelectedResources[0]);
                        WindowTransfer.Position = GUILayout.Window(398545, WindowTransfer.Position, WindowTransfer.Display, "Transfer - " + vessel.vesselName + DisplayAmounts, GUILayout.MinHeight(20));
                    }

                    if (WindowManifest.ShowWindow && WindowControl.ShowWindow)
                    {
                        step = "7 - Show Hatches";
                        WindowControl.Position = GUILayout.Window(398548, WindowControl.Position, WindowControl.Display, "Ship Manifest Part Control Center", GUILayout.MinWidth(350), GUILayout.MinHeight(20));
                    }
                }
                else
                {
                    step = "2 - Can Show Manifest = false";
                    if (SMSettings.EnableCLS && smController != null)
                        if (smController.SelectedResources.Contains("Crew"))
                            SMHighlighter.HighlightCLSVessel(false, true);  
                }
            }
            catch (Exception ex)
            {
                if (!frameErrTripped)
                {
                    Utilities.LogMessage(string.Format(" in Display at or near step:  " + step + ".  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                    frameErrTripped = true;
                }
            }
        }

        internal static void LoadSounds(string SoundType, string path1, string path2, string path3, double dblVol)
        {
            try
            {
                elapsed = 0;
                Utilities.LogMessage("Loading " + SoundType + " sounds...", "Info", SMSettings.VerboseLogging);

                GameObject go = new GameObject("Audio");

                source1 = go.AddComponent<AudioSource>();
                source2 = go.AddComponent<AudioSource>();
                source3 = go.AddComponent<AudioSource>();

                if (GameDatabase.Instance.ExistsAudioClip(path1) && GameDatabase.Instance.ExistsAudioClip(path2) && GameDatabase.Instance.ExistsAudioClip(path3))
                {
                    sound1 = GameDatabase.Instance.GetAudioClip(path1);
                    sound2 = GameDatabase.Instance.GetAudioClip(path2);
                    sound3 = GameDatabase.Instance.GetAudioClip(path3);
                    Utilities.LogMessage(SoundType + " sounds loaded...", "Info", SMSettings.VerboseLogging);

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
                    Utilities.LogMessage("Play " + SoundType + " sound (start)...", "Info", SMSettings.VerboseLogging);
                }
                else
                {
                    Utilities.LogMessage(SoundType + " sound failed to load...", "Info", SMSettings.VerboseLogging);
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
                Utilities.LogMessage("RunSave in progress...", "info", SMSettings.VerboseLogging);
                Save();
                Utilities.LogMessage("RunSave complete.", "info", SMSettings.VerboseLogging);
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
                    Utilities.LogMessage("Save in progress...", "info", SMSettings.VerboseLogging);
                    SMSettings.SaveSettings();
                    Utilities.LogMessage("Save comlete.", "info", SMSettings.VerboseLogging);
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
            Utilities.LogMessage("FireEventTriggers:  Active.", "info", SMSettings.VerboseLogging);
            GameEvents.onVesselChange.Fire(vessel);
        }

        #endregion

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