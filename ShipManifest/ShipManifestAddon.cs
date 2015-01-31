using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine;
using ConnectedLivingSpace;

namespace ShipManifest
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public partial class ShipManifestAddon : MonoBehaviour
    {
        #region Properties

        //Game object that keeps us running
        public static GameObject GameObjectInstance;  
        public static ManifestController smController;

        // Vessel vars
        public static ICLSAddon clsAddon = null;
        public static ICLSVessel clsVessel = null;

        public static Vessel vessel = null;

        // Resource transfer vars
        public static AudioSource source1;
        public static AudioSource source2;
        public static AudioSource source3;

        public static AudioClip sound1;
        public static AudioClip sound2;
        public static AudioClip sound3;

        [KSPField(isPersistant = true)]
        public static double timestamp = 0.0;

        [KSPField(isPersistant = true)]
        public static double elapsed = 0.0;

        [KSPField(isPersistant = true)]
        public static double flow_rate = (double)Settings.FlowRate;

        // Resource xfer vars
        public static XFERMode XferMode = XFERMode.SourceToTarget;
        public static bool XferOn = false;
        public static XFERState XferState = XFERState.Off;

        // crew xfer vars
        public static bool crewXfer = false;
        public static double crewXferDelaySec = Settings.IVATimeDelaySec;
        public static bool isSeat2Seat = false;
        public static double Seat2SeatXferDelaySec = 2;

        // Toolbar Integration.
        private static IButton ShipManifestButton_Blizzy = null;
        private static ApplicationLauncherButton ShipManifestButton_Stock = null;
        public static bool frameErrTripped = false;

        // Tooltip vars
        public static Vector2 ToolTipPos;
        public static string toolTip;


        #endregion

        #region Event handlers

        // Addon state event handlers
        public void Start()
        {
            Utilities.LogMessage("ShipManifestAddon.Start.", "Info", Settings.VerboseLogging);
            try
            {
                if (HighLogic.LoadedSceneIsFlight)
                {
                    // Instantiate Event handlers
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

                    // get the current Vessel data
                    vessel = FlightGlobals.ActiveVessel;
                    smController = ManifestController.GetInstance(vessel);

                    // Is CLS installed and enabled?
                    GetCLSVessel();
                    if (clsAddon != null)
                    {
                        Settings.CLSInstalled = true;
                        RunSave();
                    }
                    else
                    {
                        Settings.EnableCLS = false;
                        Settings.CLSInstalled = false;
                        RunSave();
                    }
                    Utilities.LogMessage("CLS Installed?  " + Settings.CLSInstalled.ToString(), "Info", true);
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  ShipManifestAddon.Start.  " + ex.ToString(), "Error", true);
            }
        }
        public void Awake()
        {
            try 
            {
                if (HighLogic.LoadedScene != GameScenes.FLIGHT) { return; } // don't do anything if we're not in a flight scene

                DontDestroyOnLoad(this);
                Settings.Load();
                Utilities.LogMessage("ShipManifestAddon.Awake Active...", "info", Settings.VerboseLogging);

                if (Settings.AutoSave)
                    InvokeRepeating("RunSave", Settings.SaveIntervalSec, Settings.SaveIntervalSec);

                if (Settings.EnableBlizzyToolbar)
                {
                    // Let't try to use Blizzy's toolbar
                    Utilities.LogMessage("ShipManifestAddon.Awake - Blizzy Toolbar Selected.", "Info", Settings.VerboseLogging);
                    if (!EnableBlizzyToolBar())
                    {
                        // We failed to activate the toolbar, so revert to stock
                        GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
                        GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGUIAppLauncherDestroyed);
                        Utilities.LogMessage("ShipManifestAddon.Awake - Stock Toolbar Selected.", "Info", Settings.VerboseLogging);
                   }
                }
                else 
                {
                    // Use stock Toolbar
                    Utilities.LogMessage("ShipManifestAddon.Awake - Stock Toolbar Selected.", "Info", Settings.VerboseLogging);
                    GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  ShipManifestAddon.Awake.  Error:  " + ex.ToString(), "Error", true);
            }
        }
        public void OnGUI()
        {
            //Debug.Log("[ShipManifest]:  ShipManifestAddon.OnGUI");
            try
            {
                DisplayWindows();

                Utilities.ShowToolTips();

            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  ShipManifestAddon.OnGUI.  " + ex.ToString(), "Error", true);
            }
        }
        public void Update()
        {
            try
            {
                if (HighLogic.LoadedScene == GameScenes.FLIGHT)
                {
                    if (FlightGlobals.fetch != null && FlightGlobals.ActiveVessel != null)
                    {
                        //Instantiate the controller for the active vessel.

                        smController = ManifestController.GetInstance(vessel);
                        smController.CanDrawButton = true;
                        smController.UpdateHighlighting();

                        // Realism Mode Resource transfer operation (real time)
                        // XferOn is flagged in the Resource Controller
                        if (XferOn)
                        {
                            RealModePumpXfer();
                        }

                        if (crewXfer)
                        {
                            RealModeCrewXfer();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (!frameErrTripped)
                {
                    Utilities.LogMessage(string.Format(" in Update.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                    frameErrTripped = true;
                }
            }
        }


        void DummyVoid() { }

        //Vessel state handlers
        public void OnVesselWasModified(Vessel modVessel)
        {
            Utilities.LogMessage("ShipManifestAddon.OnVesselWasModified.", "Info", Settings.VerboseLogging);
            try
            {

            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  ShipManifestAddon.OnVesselWasModified.  " + ex.ToString(), "Error", true);
            }
        }
        public void OnVesselChange(Vessel newVessel)
        {
            Utilities.LogMessage("ShipManifestAddon.OnVesselChange active...", "Info", Settings.VerboseLogging);
            try
            {
                if (vessel != null && ShipManifestAddon.CanShowShipManifest())
                {
                    if (newVessel.isEVA && !vessel.isEVA)
                    {
                        Settings.ShowShipManifest = false;
                        ToggleToolbar();

                        // kill selected resource and its associated highlighting.
                        smController.SelectedResource = null;
                        smController.SelectedResourceParts = null;
                        Utilities.LogMessage("New Vessel is a Kerbal on EVA.  ", "Info", true);
                    }
                }

                // Now let's update the current vessel view...
                vessel = newVessel;
                smController = ManifestController.GetInstance(vessel);
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in OnVesselChange.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }
        public void OnDestroy()
        {
            //Debug.Log("[ShipManifest]:  ShipManifestAddon.OnDestroy");
            try
            {
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

                if (Utilities.Errors.Count > 0 && Settings.SaveLogOnExit)
                    Savelog();
                CancelInvoke("RunSave");

                // Handle Toolbars
                GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
                if (ShipManifestButton_Blizzy != null)
                {
                    ShipManifestButton_Blizzy.Destroy();
                }
                if (ShipManifestButton_Stock != null)
                {
                    // Remove the stock toolbar button
                    GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
                    if (ShipManifestButton_Stock != null)
                    {
                        ApplicationLauncher.Instance.RemoveModApplication(ShipManifestButton_Stock);
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  ShipManifestAddon.OnDestroy.  " + ex.ToString(), "Error", true);
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
            //Debug.Log("[ShipManifest]:  ShipManifestAddon.OnVesselLoaded");
            try
            {

            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  ShipManifestAddon.OnVesselLoaded.  " + ex.ToString(), "Error", true);
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

        //Stock Toolbar button handlers
        private void OnGUIAppLauncherReady()
        {
            Utilities.LogMessage("ShipManifestAddon.OnGUIAppLauncherReady active...", "Info", Settings.VerboseLogging);
            try
            {
                if (ApplicationLauncher.Ready && HighLogic.LoadedSceneIsFlight && ShipManifestButton_Stock == null)
                {
                    ShipManifestButton_Stock = ApplicationLauncher.Instance.AddModApplication(
                        OnAppLaunchToggleOn,
                        OnAppLaunchToggleOff,
                        DummyVoid,
                        DummyVoid,
                        DummyVoid,
                        DummyVoid,
                        ApplicationLauncher.AppScenes.FLIGHT,
                        (Texture)GameDatabase.Instance.GetTexture("ShipManifest/Plugins/IconOff_38", false));
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
                if (ShipManifestButton_Stock != null)
                {
                    ApplicationLauncher.Instance.RemoveModApplication(ShipManifestButton_Stock);
                    ShipManifestButton_Stock = null;
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  ShipManifestAddon.OnGUIAppLauncherDestroyed.  " + ex.ToString(), "Error", true);
            }
        }
        private void OnAppLaunchToggleOn()
        {
            //Debug.Log("[ShipManifest]:  ShipManifestAddon.OnAppLaunchToggleOn");
            try
            {
                if (shouldToggle())
                {
                    Settings.ShowShipManifest = true;
                    ShipManifestButton_Stock.SetTexture((Texture)GameDatabase.Instance.GetTexture(Settings.ShowShipManifest ? "ShipManifest/Plugins/IconOn_38" : "ShipManifest/Plugins/IconOff_38", false));
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  ShipManifestAddon.OnAppLaunchToggleOn.  " + ex.ToString(), "Error", true);
            }
        }
        private void OnAppLaunchToggleOff()
        {
            //Debug.Log("[ShipManifest]:  ShipManifestAddon.OnAppLaunchToggleOff");
            try
            {
                if (shouldToggle())
                {
                    Settings.ShowShipManifest = false;
                    ShipManifestButton_Stock.SetTexture((Texture)GameDatabase.Instance.GetTexture(Settings.ShowShipManifest ? "ShipManifest/Plugins/IconOn_38" : "ShipManifest/Plugins/IconOff_38", false));
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage("Error in:  ShipManifestAddon.OnAppLaunchToggleOff.  " + ex.ToString(), "Error", true);
            }
        }

        #endregion

        #region Logic Methods

        public static bool CanKerbalsBeXferred(Part SelectedPart)
        {
            bool results = false;
            try
            {
                if (SelectedPart == smController.SelectedPartSource)
                {
                    // Source to target
                    // Are the parts capable of holding kerbals and are there kerbals to move?
                    if ((smController.SelectedPartTarget != null && smController.SelectedPartSource != smController.SelectedPartTarget) && smController.SelectedPartSource.protoModuleCrew.Count > 0)
                    {
                        // now, are the parts connected to each other in the same living space?
                        results = IsCLSInSameSpace();
                    }
                }
                else  //SelectedPart must be SeletedPartTarget
                {
                    // Target to Source
                    if ((smController.SelectedPartSource != null && smController.SelectedPartSource != smController.SelectedPartTarget) && smController.SelectedPartTarget.protoModuleCrew.Count > 0)
                    {
                        // now, are the parts connected to each other in the same living space?
                        results = IsCLSInSameSpace();
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in CanBeXferred.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
            return results;
        }

        public static bool shouldToggle()
        {
            //Debug.Log("[ShipManifest]:  ShipManifestAddon.shouldToggle");
            return (HighLogic.LoadedScene == GameScenes.FLIGHT && !MapView.MapIsEnabled && !PauseMenu.isOpen && !FlightResultsDialog.isDisplaying &&
                            FlightGlobals.fetch != null && FlightGlobals.ActiveVessel != null &&
                            smController.CanDrawButton
                            );
        }

        private static bool IsCLSInSameSpace()
        {
            bool results = false;
            try
            {
                if (smController.SelectedPartSource == null || smController.SelectedPartTarget == null) 
                    return false;
                if (Settings.EnableCLS)
                {
                    if (clsVessel != null)
                    {
                        if (smController.clsSpaceSource != null && smController.clsSpaceTarget != null)
                        {
                            if (smController.clsSpaceSource == smController.clsSpaceTarget)
                                results = true;
                        }
                    }
                }
                else
                {
                    results = true;
                }
            }
            catch (Exception ex)
            {
                if (!frameErrTripped)
                {
                    Utilities.LogMessage(string.Format(" in IsInCLS.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                    frameErrTripped = true;
                }
            }
            //Utilities.LogMessage("IsInCLS() - results = " + results.ToString() , "info", true);
            return results;
        }

        public static bool CanShowShipManifest()
        {
            if (Settings.ShowShipManifest
                && HighLogic.LoadedScene == GameScenes.FLIGHT
                && !vessel.isEVA
                && CameraManager.Instance.currentCameraMode != CameraManager.CameraMode.IVA 
                )
                return true;
            else
            {
                return false;
           }
        }
        #endregion

        #region Action Methods

        public static bool GetCLSVessel()
        {
            try
            {
                Utilities.LogMessage("GetCLSVessel - Active.", "Info", Settings.VerboseLogging);

                if (clsVessel == null)
                    if (clsAddon == null)
                    {
                        clsAddon = CLSClient.GetCLS();
                        if (clsAddon != null)
                            clsVessel = clsAddon.Vessel;
                        else
                            Utilities.LogMessage("GetCLSVessel - clsAddon is null.", "Info", Settings.VerboseLogging);
                    }
                    else
                        clsVessel = clsAddon.Vessel;
                else
                    clsVessel = clsAddon.Vessel;

                if (clsVessel != null)
                {
                    Utilities.LogMessage("GetCLSVessel - Complete.", "Info", Settings.VerboseLogging);
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

        public static bool EnableBlizzyToolBar()
        {
            if (Settings.EnableBlizzyToolbar)
            {
                try
                {
                    if (ToolbarManager.ToolbarAvailable)
                    {
                        ShipManifestButton_Blizzy = ToolbarManager.Instance.add("ShipManifest", "ShipManifest");
                        ShipManifestButton_Blizzy.TexturePath = "ShipManifest/Plugins/IconOff_24";
                        ShipManifestButton_Blizzy.ToolTip = "Ship Manifest";
                        ShipManifestButton_Blizzy.Visibility = new GameScenesVisibility(GameScenes.FLIGHT);
                        ShipManifestButton_Blizzy.OnClick += (e) =>
                        {
                            if (shouldToggle())
                            {
                                ShipManifestButton_Blizzy.TexturePath = Settings.ShowShipManifest ? "ShipManifest/Plugins/IconOff_24" : "ShipManifest/Plugins/IconOn_24";
                                Settings.ShowShipManifest = !Settings.ShowShipManifest;
                            }
                        };
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

        public static void ToggleToolbar()
        {
            // turn off SM at toolbar
            if (Settings.EnableBlizzyToolbar)
                ToggleBlizzyToolBar();
            else
            {
                if (Settings.ShowShipManifest)
                    ShipManifestButton_Stock.toggleButton.SetTrue();
                else
                    ShipManifestButton_Stock.toggleButton.SetFalse();
            }
        }

        public static void ToggleBlizzyToolBar()
        {
            ShipManifestButton_Blizzy.TexturePath = Settings.ShowShipManifest ? "ShipManifest/Plugins/IconOn_24" : "ShipManifest/Plugins/IconOff_24";
        }

        public void DisplayWindows()
        {
            string step = "";
            try
            {
                step = "0 - Start";
                ManifestStyle.SetupGUI();

                if (Settings.ShowDebugger)
                    Settings.DebuggerPosition = GUILayout.Window(398648, Settings.DebuggerPosition, DebuggerWindow.Display, " Ship Manifest -  Debug Console - Ver. " + Settings.CurVersion, GUILayout.MinHeight(20));

                if (FlightGlobals.fetch == null || FlightGlobals.ActiveVessel != vessel)
                {
                    step = "0a - Vessel Change";
                    smController.SelectedPartSource = smController.SelectedPartTarget = null;
                    smController.SelectedResource = null;
                    return;
                }

                step = "1 - Show Interface(s)";
                // Is the scene one we want to be visible in?
                if (HighLogic.LoadedScene == GameScenes.FLIGHT && !MapView.MapIsEnabled && !PauseMenu.isOpen && !FlightResultsDialog.isDisplaying)
                {
                    // why is this here?
                    //UpdateHighlighting();
                    if (ShipManifestAddon.CanShowShipManifest())
                    {
                        step = "2 - Show Manifest";
                        Settings.ManifestPosition = GUILayout.Window(398544, Settings.ManifestPosition, ManifestWindow.Display, "Ship's Manifest - " + vessel.vesselName, GUILayout.MinHeight(20));
                    }

                    // What windows do we want to show?
                    if (ShipManifestAddon.CanShowShipManifest() && Settings.ShowTransferWindow && smController.SelectedResource != null)
                    {
                        step = "3 - Show Transfer";
                        Settings.TransferPosition = GUILayout.Window(398545, Settings.TransferPosition, TransferWindow.Display, "Transfer - " + vessel.vesselName + " - " + smController.SelectedResource, GUILayout.MinHeight(20));
                    }

                    if (ShipManifestAddon.CanShowShipManifest() && Settings.ShowSettings)
                    {
                        step = "4 - Show Settings";
                        Settings.SettingsPosition = GUILayout.Window(398546, Settings.SettingsPosition, SettingsWindow.Display, "Ship Manifest Settings", GUILayout.MinHeight(20));
                    }

                    if (RosterWindow.resetRosterSize)
                    {
                        step = "5 - Reset Roster Size";
                        Settings.RosterPosition.height = 100; //reset hight
                        Settings.RosterPosition.width = 400; //reset width
                        RosterWindow.resetRosterSize = false;
                    }

                    if (Settings.ShowShipManifest && Settings.ShowRoster)
                    {
                        step = "6 - Show Roster";
                        Settings.RosterPosition = GUILayout.Window(398547, Settings.RosterPosition, RosterWindow.Display, "Ship Manifest Roster", GUILayout.MinHeight(20));
                    }

                    if (Settings.ShowShipManifest && Settings.ShowHatchWindow)
                    {
                        step = "6 - Show Hatches";
                        Settings.HatchWindowPosition = GUILayout.Window(398548, Settings.HatchWindowPosition, HatchWindow.Display, "Ship Manifest Hatches", GUILayout.MinHeight(20));
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in drawGui at or near step:  " + step + ".  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
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
                                Utilities.LogMessage("Transfer State:  " + XferState.ToString() + "...", "Info", Settings.VerboseLogging);
                                elapsed = 0;
                                XferState = XFERState.Run;
                            }
                            break;

                        case XFERState.Run:

                            deltaT = Planetarium.GetUniversalTime() - timestamp;
                            double deltaAmt = deltaT * flow_rate;

                            // This adjusts the delta when we get to the end of the xfer.
                            float XferAmount = 0f;
                            // which way we going?
                            if (XferMode == XFERMode.TargetToSource)
                                XferAmount = smController.tXferAmount;
                            else
                                XferAmount = smController.sXferAmount;

                            if (smController.AmtXferred + (float)deltaAmt >= XferAmount)
                            {
                                deltaAmt = XferAmount - smController.AmtXferred;
                                XferState = XFERState.Stop;
                                Utilities.LogMessage("10. Adjusted DeltaAmt = " + deltaAmt.ToString(), "Info", Settings.VerboseLogging);
                            }
                            Utilities.LogMessage("11. DeltaAmt = " + deltaAmt.ToString(), "Info", Settings.VerboseLogging);

                            // Lets increment the AmtXferred....
                            smController.AmtXferred += (float)deltaAmt;
                            Utilities.LogMessage("11a. AmtXferred = " + smController.AmtXferred.ToString(), "Info", Settings.VerboseLogging);

                            // Drain source...
                            // and let's make sure we can move the amount requested or adjust it and stop the flow after the move.
                            if (XferMode == XFERMode.TargetToSource)
                            {
                                // Source is target on Interface...
                                // if the amount to move exceeds either the balance of the source or the capacity of the target, reduce it.
                                if (smController.SelectedPartTarget.Resources[smController.SelectedResource].amount - deltaAmt < 0)
                                {
                                    deltaAmt = smController.SelectedPartTarget.Resources[smController.SelectedResource].amount;
                                    XferState = XFERState.Stop;
                                }
                                else if (smController.SelectedPartSource.Resources[smController.SelectedResource].amount + deltaAmt > smController.SelectedPartSource.Resources[smController.SelectedResource].maxAmount)
                                {
                                    deltaAmt = smController.SelectedPartSource.Resources[smController.SelectedResource].maxAmount - smController.SelectedPartSource.Resources[smController.SelectedResource].amount;
                                    XferState = XFERState.Stop;
                                }

                                smController.SelectedPartTarget.Resources[smController.SelectedResource].amount -= deltaAmt;
                                if (smController.SelectedPartTarget.Resources[smController.SelectedResource].amount < 0)
                                    smController.SelectedPartTarget.Resources[smController.SelectedResource].amount = 0;
                            }
                            else
                            {
                                // Source is source on Interface...
                                if (smController.SelectedPartSource.Resources[smController.SelectedResource].amount - deltaAmt < 0)
                                {
                                    deltaAmt = smController.SelectedPartSource.Resources[smController.SelectedResource].amount;
                                    XferState = XFERState.Stop;
                                }
                                else if (smController.SelectedPartTarget.Resources[smController.SelectedResource].amount + deltaAmt > smController.SelectedPartTarget.Resources[smController.SelectedResource].maxAmount)
                                {
                                    deltaAmt = smController.SelectedPartTarget.Resources[smController.SelectedResource].maxAmount - smController.SelectedPartTarget.Resources[smController.SelectedResource].amount;
                                    XferState = XFERState.Stop;
                                }

                                smController.SelectedPartSource.Resources[smController.SelectedResource].amount -= deltaAmt;
                                if (smController.SelectedPartSource.Resources[smController.SelectedResource].amount < 0)
                                    smController.SelectedPartSource.Resources[smController.SelectedResource].amount = 0;
                            }


                            Utilities.LogMessage("12. Drain Source Part = " + deltaAmt.ToString(), "Info", Settings.VerboseLogging);

                            // Fill target
                            if (XferMode == XFERMode.TargetToSource)
                                smController.SelectedPartSource.Resources[smController.SelectedResource].amount += deltaAmt;
                            else
                                smController.SelectedPartTarget.Resources[smController.SelectedResource].amount += deltaAmt;

                            Utilities.LogMessage("13. Fill Target Part = " + deltaAmt.ToString(), "Info", Settings.VerboseLogging);

                            Utilities.LogMessage("Transfer State:  " + XferState.ToString() + "...", "Info", Settings.VerboseLogging);
                            break;

                        case XFERState.Stop:

                            // play pump shutdown.
                            source2.Stop();
                            source3.Play();
                            timestamp = elapsed = 0;
                            XferState = XFERState.Off;
                            smController.AmtXferred = 0f;
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
                    Utilities.LogMessage(string.Format(" in RealModePumpXfer.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
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
                    if (!Settings.RealismMode)
                    {
                        if (timestamp != 0)
                            elapsed += Planetarium.GetUniversalTime() - timestamp;

                        if (elapsed > 1)
                        {
                            // Fire Board event for Texture Replacer.
                            if (Settings.EnableTextureReplacer)
                                GameEvents.onCrewBoardVessel.Fire(smController.evaAction);

                            // Spawn crew in parts and in vessel.
                            if (smController.SelectedPartSource == null)
                                smController.SelectedPartSource = smController.SelectedPartTarget;
                            if (smController.SelectedPartTarget == null)
                                smController.SelectedPartTarget = smController.SelectedPartSource;

                            smController.SelectedPartSource.vessel.SpawnCrew();
                            smController.SelectedPartTarget.vessel.SpawnCrew();
                            smController.RespawnCrew();

                            FireEventTriggers();
                            elapsed = timestamp = 0;
                            crewXfer = false;
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

                                Utilities.LogMessage("RealModeCrewXfer:  Updating Portraits...", "info", Settings.VerboseLogging);

                                // Spawn crew in parts and in vessel.
                                if (smController.SelectedPartSource == null)
                                    smController.SelectedPartSource = smController.SelectedPartTarget;
                                if (smController.SelectedPartTarget == null)
                                    smController.SelectedPartTarget = smController.SelectedPartSource;

                                smController.SelectedPartSource.vessel.SpawnCrew();
                                smController.SelectedPartTarget.vessel.SpawnCrew();
                                smController.RespawnCrew();
                                // play crew sit.
                                source2.Stop();
                                source3.Play();
                                ShipManifestAddon.timestamp = elapsed = 0;
                                XferState = XFERState.Off;
                                crewXfer = false;
                                isSeat2Seat = false;

                                // Fire Board event for Texture Replacer.
                                if (Settings.EnableTextureReplacer)
                                    GameEvents.onCrewBoardVessel.Fire(smController.evaAction);

                                // Notify Mods requiring it to update (Texture Replacer Kerbal (IVA) textures, ConnectedLivingSpaces.
                                FireEventTriggers();
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
                    Utilities.LogMessage(string.Format(" in RealModeCrewXfer.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                    XferState = XFERState.Stop;
                    frameErrTripped = true;
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

        public void RunSave()
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
                if (HighLogic.LoadedScene == GameScenes.FLIGHT && FlightGlobals.fetch != null && FlightGlobals.ActiveVessel != null)
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

        public static void Savelog()
        {
            try
            {
                // time to create a file...
                string filename = "DebugLog_" + DateTime.Now.ToString().Replace(" ", "_").Replace("/", "").Replace(":", "") + ".txt";

                string path = Directory.GetCurrentDirectory() + @"\GameData\ShipManifest\";
                if (Settings.DebugLogPath.StartsWith(@"\"))
                    Settings.DebugLogPath = Settings.DebugLogPath.Substring(2, Settings.DebugLogPath.Length - 2);

                if (!Settings.DebugLogPath.EndsWith(@"\"))
                    Settings.DebugLogPath += @"\";

                filename = path + Settings.DebugLogPath + filename;
                Utilities.LogMessage("File Name = " + filename, "Info", true);

                try
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (string line in Utilities.Errors)
                    {
                        sb.AppendLine(line);
                    }

                    File.WriteAllText(filename, sb.ToString());

                    Utilities.LogMessage("File written", "Info", true);
                }
                catch (Exception ex)
                {
                    Utilities.LogMessage("Error Writing File:  " + ex.ToString(), "Info", true);
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in Savelog.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        public static void FireEventTriggers()
        {
            // Per suggestion by shaw (http://forum.kerbalspaceprogram.com/threads/62270-0-23-Ship-Manifest-%28Manage-Crew-Science-Resources%29-v0-23-3-1-5-26-Feb-14?p=1033866&viewfull=1#post1033866)
            // and instructions for using CLS API by codepoet.

            //GameEvents.onVesselWasModified.Fire(vessel);
            Utilities.LogMessage("FireEventTriggers:  Active.", "info", Settings.VerboseLogging);
            GameEvents.onVesselChange.Fire(vessel);
        }

        #endregion

        public enum XFERState
        {
            Off,
            Start,
            Run,
            Stop
        }

        public enum XFERMode
        {
            SourceToTarget,
            TargetToSource
        }

    }

    public class ShipManifestModule : PartModule
    {
        [KSPEvent(guiActive = true, guiName = "Destroy Part", active = true)]
        public void DestoryPart()
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