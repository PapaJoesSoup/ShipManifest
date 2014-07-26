using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine;
using ConnectedLivingSpace;

namespace ShipManifest
{
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

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class ShipManifestBehaviour : MonoBehaviour
    {

        #region Properties

        //Game object that keeps us running
        public static GameObject GameObjectInstance;
        public static SettingsManager ShipManifestSettings = new SettingsManager();

        // Vessel vars
        public static ICLSAddon clsAddon;
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
        public static double flow_rate = (double)ShipManifestSettings.FlowRate;

        // Resource xfer vars
        public static bool sXferOn = false;
        public static bool tXferOn = false;
        public static XFERState XferState = XFERState.Off;

        // crew xfer vars
        public static bool crewXfer = false;
        public static double crewXferDelaySec = SettingsManager.IVATimeDelaySec;
        public static bool isSeat2Seat = false;
        public static double Seat2SeatXferDelaySec = 2;

        #endregion

        // Toolbar Integration.
        private static IButton ShipManifestButtonBlizzy;
        private static ApplicationLauncherButton ShipManifestButtonStock;

        #region Event handlers

        // this is the delagate needed to support the part event handlers
        // extern is needed, as the addon is considered external to KSP, and is expected by the part delagate call.
        static extern public Part.OnActionDelegate OnMouseExit(Part part);

        // this is the method used with the delagate
        public static void MouseExit(Part part)
        {
            ManifestController.GetInstance(FlightGlobals.ActiveVessel).OnMouseHighlights();
        }

        public void Awake()
        {
            if (HighLogic.LoadedScene != GameScenes.FLIGHT) { return; } // don't do anything if we're not in a flight scene

            DontDestroyOnLoad(this);
            ShipManifestSettings.Load();
            if (ShipManifestSettings.AutoSave)
                InvokeRepeating("RunSave", ShipManifestSettings.SaveIntervalSec, ShipManifestSettings.SaveIntervalSec);

            if (ShipManifestSettings.EnableBlizzyToolbar)
            {
                ShipManifestButtonBlizzy = ToolbarManager.Instance.add("ResourceManifest", "ResourceManifest");
                ShipManifestButtonBlizzy.TexturePath = "ShipManifest/Plugins/IconOff_24";
                ShipManifestButtonBlizzy.ToolTip = "Ship Manifest";
                ShipManifestButtonBlizzy.Visibility = new GameScenesVisibility(GameScenes.FLIGHT);
                ShipManifestButtonBlizzy.OnClick += (e) =>
                {
                    if (shouldToggle())
                    {
                        ShipManifestButtonBlizzy.TexturePath = ManifestController.GetInstance(FlightGlobals.ActiveVessel).ShowShipManifest ? "ShipManifest/Plugins/IconOff_24" : "ShipManifest/Plugins/IconOn_24";
                        ManifestController.GetInstance(FlightGlobals.ActiveVessel).ShowShipManifest = !ManifestController.GetInstance(FlightGlobals.ActiveVessel).ShowShipManifest;
                        if (!ManifestController.GetInstance(FlightGlobals.ActiveVessel).ShowShipManifest)
                            ManifestController.GetInstance(FlightGlobals.ActiveVessel).ClearResourceHighlighting();
                        else
                            ManifestController.GetInstance(FlightGlobals.ActiveVessel).SetResourceHighlighting();
                    }
                };
            }
            else
            {
                GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
            }
        }

        public void Start()
        {
            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                GameEvents.onVesselChange.Add(OnVesselChange);

                clsAddon = CLSClient.GetCLS();
                if (clsAddon != null)
                {
                    ShipManifestSettings.EnableCLS = true;
                    SettingsManager.CLSInstalled = true;
                    RunSave();
                }
                else
                {
                    ShipManifestSettings.EnableCLS = false;
                    SettingsManager.CLSInstalled = false;
                    RunSave();
                }
                ManifestUtilities.LogMessage("CLS Installed?  " + SettingsManager.CLSInstalled.ToString(), "Info", true);
            }
         }

        public void OnGUI()
        {
            ManifestStyle.SetupGUI();

            if (SettingsManager.ShowDebugger)
                ShipManifestSettings.DebuggerPosition = GUILayout.Window(398648, ShipManifestSettings.DebuggerPosition, DebuggerWindow, " Ship Manifest -  Debug Console - Ver. " + ShipManifestSettings.CurVersion, GUILayout.MinHeight(20));
        }

        void OnGUIAppLauncherReady()
        {
            if (ApplicationLauncher.Ready && HighLogic.LoadedSceneIsFlight && ShipManifestButtonStock == null)
            {
                ShipManifestButtonStock = ApplicationLauncher.Instance.AddModApplication(
                    onAppLaunchToggleOn,
                    onAppLaunchToggleOff,
                    DummyVoid,
                    DummyVoid,
                    DummyVoid,
                    DummyVoid,
                    ApplicationLauncher.AppScenes.FLIGHT,
                    (Texture)GameDatabase.Instance.GetTexture("ShipManifest/Plugins/IconOff_24", false));
                GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
                GameEvents.onGameSceneLoadRequested.Add(OnGameSceneLoadRequested);
            }
        }

        void OnGameSceneLoadRequested(GameScenes scene)
        {
            if (ShipManifestButtonStock != null) {
                ApplicationLauncher.Instance.RemoveModApplication(ShipManifestButtonStock);
                GameEvents.onGameSceneLoadRequested.Remove(OnGameSceneLoadRequested);
                GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
            }
        }

        bool shouldToggle()
        {
            return (!MapView.MapIsEnabled && !PauseMenu.isOpen && !FlightResultsDialog.isDisplaying &&
                            FlightGlobals.fetch != null && FlightGlobals.ActiveVessel != null &&
                            ManifestController.GetInstance(FlightGlobals.ActiveVessel).CanDrawButton
                            );
        }

        void onAppLaunchToggleOn()
        {
            if (!shouldToggle()) { ShipManifestButtonStock.SetFalse(); return;  }
            ManifestController.GetInstance(FlightGlobals.ActiveVessel).ShowShipManifest = true;
            ShipManifestButtonStock.SetTexture((Texture)GameDatabase.Instance.GetTexture("ShipManifest/Plugins/IconOn_24", false));
            ManifestController.GetInstance(FlightGlobals.ActiveVessel).SetResourceHighlighting();
        }

        void onAppLaunchToggleOff()
        {
            if (!shouldToggle()) { ShipManifestButtonStock.SetTrue(); return; }
            ManifestController.GetInstance(FlightGlobals.ActiveVessel).ShowShipManifest = false;
            ShipManifestButtonStock.SetTexture((Texture)GameDatabase.Instance.GetTexture("ShipManifest/Plugins/IconOff_24", false));
            ManifestController.GetInstance(FlightGlobals.ActiveVessel).ClearResourceHighlighting();
        }

        void DummyVoid() { }

        public void Update()
        {
            try
            {
                if (FlightGlobals.fetch != null && FlightGlobals.ActiveVessel != null)
                {
                    if (HighLogic.LoadedScene == GameScenes.FLIGHT)
                    {
                        //Instantiate the controller for the active vessel.
                        ManifestController.GetInstance(FlightGlobals.ActiveVessel).CanDrawButton = true;

                        // Realism Mode Resource transfer operation (real time)
                        // XferOn is flagged in the Resource Controller
                        if (tXferOn || sXferOn)
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
                ManifestUtilities.LogMessage(string.Format(" in Update.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        public void OnVesselChange(Vessel newVessel)
        {
            try
            {
                if (newVessel.isEVA && !vessel.isEVA)
                {
                    // Let's clean up old clsVessel highlighing...
                    if (SettingsManager.CLSInstalled && ShipManifestSettings.EnableCLS && !vessel.isEVA && ManifestController.GetInstance(vessel).SelectedResource == "Crew")
                    {
                        // For some reason, clsVessel.Highlight(false) fails here.  no error, just does not turn off highlighting...
                        foreach (ICLSPart iPart in ShipManifestBehaviour.clsVessel.Parts)
                            iPart.Highlight(false);

                        // if we are EVA, then we don't need SM open.
                        if (clsVessel.Parts[0].Part.vessel.isEVA)
                            ManifestUtilities.LogMessage("clsVessel is EVA!! Why?.  ", "Info", SettingsManager.VerboseLogging);

                        ManifestUtilities.LogMessage("Setting Old Vessel Highlighting off.  ", "Info", SettingsManager.VerboseLogging);
                    }

                    // kill selected resource and its associated highlighting.
                    ManifestController.GetInstance(vessel).SelectedResource = null;
                    ManifestController.GetInstance(vessel).SelectedResourceParts = null;
                    ManifestController.GetInstance(vessel).ShowShipManifest = false;
                    ManifestController.GetInstance(vessel).ShowTransferWindow = false;
                    ManifestUtilities.LogMessage("New Vessel is a Kerbal on EVA.  ", "Info", true);
                }
                vessel = newVessel;
                if (SettingsManager.CLSInstalled && ShipManifestSettings.EnableCLS)
                    clsVessel = clsAddon.Vessel;
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format(" in OnVesselChange.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        public void OnDestroy()
        {
            GameEvents.onVesselChange.Remove(OnVesselChange);
            CancelInvoke("RunSave");
            GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
            if (ShipManifestButtonBlizzy != null) { 
                ShipManifestButtonBlizzy.Destroy(); 
            }
            if (ShipManifestButtonStock != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(ShipManifestButtonStock); 
            }
        }

        #endregion

        #region Logic Methods

        public static bool CanBeXferred(Part SelectedPart)
        {
            bool results = false;
            try
            {
                if (SelectedPart == ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartSource)
                {
                    // Source to target
                    // Are the parts capable of holding kerbals and are there kerbals to move?
                    if ((ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartTarget != null && ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartSource != ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartTarget) && ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartSource.protoModuleCrew.Count > 0)
                    {
                        // now, are the parts connected to each other in the same living space?
                        results = IsCLS();
                    }
                }
                else  //SelectedPart must be SeletedPartTarget
                {
                    // Target to Source
                    if ((ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartSource != null && ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartSource != ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartTarget) && ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartTarget.protoModuleCrew.Count > 0)
                    {
                        // now, are the parts connected to each other in the same living space?
                        results = IsCLS();
                    }
                }
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format(" in CanBeXferred.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }

            return results;
        }

        private static bool IsCLS()
        {
            bool results = false;
            try
            {
                if (ShipManifestSettings.EnableCLS)
                {
                    foreach (ICLSSpace mySpace in clsVessel.Spaces)
                    {
                        foreach (ICLSPart myPart in mySpace.Parts)
                        {
                            if (myPart.Part == ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartSource)
                            {
                                ManifestController.GetInstance(FlightGlobals.ActiveVessel).clsPartSource = myPart;
                                break;
                            }
                        }
                        foreach (ICLSPart myPart in mySpace.Parts)
                        {
                            if (myPart.Part == ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartTarget)
                            {
                                ManifestController.GetInstance(FlightGlobals.ActiveVessel).clsPartTarget = myPart;
                                break;
                            }
                        }
                    }

                    if (ManifestController.GetInstance(FlightGlobals.ActiveVessel).clsPartSource.Space != null && ManifestController.GetInstance(FlightGlobals.ActiveVessel).clsPartTarget.Space != null && ManifestController.GetInstance(FlightGlobals.ActiveVessel).clsPartSource.Space == ManifestController.GetInstance(FlightGlobals.ActiveVessel).clsPartTarget.Space)
                    {
                        results = true;
                    }
                    else if (ManifestController.GetInstance(FlightGlobals.ActiveVessel).clsPartSource.Space == null)
                    {
                        ManifestUtilities.LogMessage("Source part space is null.", "Error", true);
                    }
                    else if (ManifestController.GetInstance(FlightGlobals.ActiveVessel).clsPartTarget.Space == null)
                    {
                        ManifestUtilities.LogMessage("Target part space is null.", "Error", true);
                    }
                }
                else
                {
                    results = true;
                }
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format(" in IsCLS.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
            return results;
        }

        #endregion

        #region Action Methods

        private void RealModePumpXfer()
        {
            try
            {
                if (tXferOn || sXferOn)
                {
                    double deltaT = 0;
                    flow_rate = ShipManifestSettings.FlowRate;

                    switch (XferState)
                    {
                        case XFERState.Off:
                            // reset counters
                            timestamp = 0;

                            // Default sound license: CC-By-SA
                            // http://www.freesound.org/people/vibe_crc/sounds/59328/
                            string path1 = ShipManifestSettings.PumpSoundStart; // "ShipManifest/Sounds/59328-1";
                            string path2 = ShipManifestSettings.PumpSoundRun;   // "ShipManifest/Sounds/59328-2";
                            string path3 = ShipManifestSettings.PumpSoundStop;  // "ShipManifest/Sounds/59328-3";

                            // Load Sounds, and Play Sound 1
                            LoadSounds("Pump", path1, path2, path3, ShipManifestSettings.PumpSoundVol);
                            XferState = XFERState.Start;
                            break;

                        case XFERState.Start:

                            // calculate elapsed.
                            elapsed += Planetarium.GetUniversalTime();

                            // Play run sound when start sound is nearly done. (repeats)
                            if (elapsed >= source1.clip.length - 0.25)
                            {
                                source2.Play();
                                ManifestUtilities.LogMessage("Transfer State:  " + XferState.ToString() + "...", "Info", SettingsManager.VerboseLogging);
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
                            if (tXferOn)
                                XferAmount = ManifestController.GetInstance(FlightGlobals.ActiveVessel).tXferAmount;
                            else
                                XferAmount = ManifestController.GetInstance(FlightGlobals.ActiveVessel).sXferAmount;

                            if (ManifestController.GetInstance(FlightGlobals.ActiveVessel).AmtXferred + (float)deltaAmt >= XferAmount)
                            {
                                deltaAmt = XferAmount - ManifestController.GetInstance(FlightGlobals.ActiveVessel).AmtXferred;
                                XferState = XFERState.Stop;
                                ManifestUtilities.LogMessage("10. Adjusted DeltaAmt = " + deltaAmt.ToString(), "Info", SettingsManager.VerboseLogging);
                            }
                            ManifestUtilities.LogMessage("11. DeltaAmt = " + deltaAmt.ToString(), "Info", SettingsManager.VerboseLogging);

                            // Lets increment the AmtXferred....
                            ManifestController.GetInstance(FlightGlobals.ActiveVessel).AmtXferred += (float)deltaAmt;
                            ManifestUtilities.LogMessage("11a. AmtXferred = " + ManifestController.GetInstance(FlightGlobals.ActiveVessel).AmtXferred.ToString(), "Info", SettingsManager.VerboseLogging);

                            // Drain source...
                            // and let's make sure we can move the amount requested or adjust it and stop the flow after the move.
                            if (tXferOn)
                            {
                                // Source is target on Interface...
                                // if the amount to move exceeds either the balance of the source or the capacity of the target, reduce it.
                                if (ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartTarget.Resources[ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedResource].amount - deltaAmt < 0)
                                {
                                    deltaAmt = ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartTarget.Resources[ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedResource].amount;
                                    XferState = XFERState.Stop;
                                }
                                else if (ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartSource.Resources[ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedResource].amount + deltaAmt > ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartSource.Resources[ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedResource].maxAmount)
                                {
                                    deltaAmt = ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartSource.Resources[ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedResource].maxAmount - ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartSource.Resources[ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedResource].amount;
                                    XferState = XFERState.Stop;
                                }

                                ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartTarget.Resources[ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedResource].amount -= deltaAmt;
                            }
                            else
                            {
                                // Source is source on Interface...
                                if (ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartSource.Resources[ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedResource].amount - deltaAmt < 0)
                                {
                                    deltaAmt = ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartSource.Resources[ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedResource].amount;
                                    XferState = XFERState.Stop;
                                }
                                else if (ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartTarget.Resources[ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedResource].amount + deltaAmt > ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartTarget.Resources[ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedResource].maxAmount)
                                {
                                    deltaAmt = ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartTarget.Resources[ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedResource].maxAmount - ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartTarget.Resources[ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedResource].amount;
                                    XferState = XFERState.Stop;
                                }

                                ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartSource.Resources[ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedResource].amount -= deltaAmt;
                            }


                            ManifestUtilities.LogMessage("12. Drain Source Part = " + deltaAmt.ToString(), "Info", SettingsManager.VerboseLogging);

                            // Fill target
                            if (tXferOn)
                                ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartSource.Resources[ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedResource].amount += deltaAmt;
                            else
                                ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartTarget.Resources[ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedResource].amount += deltaAmt;

                            ManifestUtilities.LogMessage("13. Fill Target Part = " + deltaAmt.ToString(), "Info", SettingsManager.VerboseLogging);

                            ManifestUtilities.LogMessage("Transfer State:  " + XferState.ToString() + "...", "Info", SettingsManager.VerboseLogging);
                            break;

                        case XFERState.Stop:

                            // play pump shutdown.
                            source2.Stop();
                            source3.Play();
                            timestamp = elapsed = 0;
                            XferState = XFERState.Off;
                            ManifestController.GetInstance(FlightGlobals.ActiveVessel).AmtXferred = 0f;
                            tXferOn = sXferOn = false;
                            break;
                    }
                    ManifestUtilities.LogMessage("Transfer State:  " + XferState.ToString() + "...", "Info", SettingsManager.VerboseLogging);
                    if (XferState != XFERState.Off)
                        timestamp = Planetarium.GetUniversalTime();
                }
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format(" in RealModePumpXfer.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        private void RealModeCrewXfer()
        {
            try
            {
                if (crewXfer)
                {
                    if (!ShipManifestSettings.RealismMode)
                    {
                        if (timestamp != 0)
                            elapsed += Planetarium.GetUniversalTime() - timestamp;

                        if (elapsed > 1)
                        {
                            // Fire Board event for Texture Replacer.
                            if (SettingsManager.EnableTextureReplacer)
                                GameEvents.onCrewBoardVessel.Fire(ManifestController.GetInstance(FlightGlobals.ActiveVessel).evaAction);

                            // Spawn crew in parts and in vessel.
                            ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartSource.vessel.SpawnCrew();
                            ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartTarget.vessel.SpawnCrew();
                            ManifestController.GetInstance(FlightGlobals.ActiveVessel).RespawnCrew();

                            FireEventTriggers();
                            elapsed = timestamp = 0;
                            crewXfer = false;
                            ManifestUtilities.LogMessage("crewXfer State:  " + crewXfer.ToString() + "...", "Info", SettingsManager.VerboseLogging);
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
                                string path1 = ShipManifestSettings.CrewSoundStart; // "ShipManifest/Sounds/14214-1";
                                string path2 = ShipManifestSettings.CrewSoundRun;   // "ShipManifest/Sounds/14214-2";
                                string path3 = ShipManifestSettings.CrewSoundStop;  // "ShipManifest/Sounds/14214-3";

                                LoadSounds("Crew", path1, path2, path3, ShipManifestSettings.CrewSoundVol);
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

                                ManifestUtilities.LogMessage("Update:  Updating Portraits...", "info", SettingsManager.VerboseLogging);

                                // Spawn crew in parts and in vessel.
                                ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartSource.vessel.SpawnCrew();
                                ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartTarget.vessel.SpawnCrew();
                                ManifestController.GetInstance(FlightGlobals.ActiveVessel).RespawnCrew();

                                // play crew sit.
                                source2.Stop();
                                source3.Play();
                                ShipManifestBehaviour.timestamp = elapsed = 0;
                                XferState = XFERState.Off;
                                crewXfer = false;
                                isSeat2Seat = false;

                                // Fire Board event for Texture Replacer.
                                if (SettingsManager.EnableTextureReplacer)
                                    GameEvents.onCrewBoardVessel.Fire(ManifestController.GetInstance(FlightGlobals.ActiveVessel).evaAction);

                                // Notify Mods requiring it to update (Texture Replacer Kerbal (IVA) textures, ConnectedLivingSpaces.
                                FireEventTriggers();
                                break;
                        }
                        ManifestUtilities.LogMessage("Transfer State:  " + XferState.ToString() + "...", "Info", SettingsManager.VerboseLogging);
                        if (XferState != XFERState.Off)
                            timestamp = Planetarium.GetUniversalTime();
                    }
                }
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format(" in RealModeCrewXfer.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        private void LoadSounds(string SoundType, string path1, string path2, string path3, double dblVol)
        {
            try
            {
                elapsed = 0;
                ManifestUtilities.LogMessage("Loading " + SoundType + " sounds...", "Info", true);

                GameObject go = new GameObject("Audio");

                source1 = go.AddComponent<AudioSource>();
                source2 = go.AddComponent<AudioSource>();
                source3 = go.AddComponent<AudioSource>();

                if (GameDatabase.Instance.ExistsAudioClip(path1) && GameDatabase.Instance.ExistsAudioClip(path2) && GameDatabase.Instance.ExistsAudioClip(path3))
                {
                    sound1 = GameDatabase.Instance.GetAudioClip(path1);
                    sound2 = GameDatabase.Instance.GetAudioClip(path2);
                    sound3 = GameDatabase.Instance.GetAudioClip(path3);
                    ManifestUtilities.LogMessage(SoundType + " sounds loaded...", "Info", SettingsManager.VerboseLogging);

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
                    ManifestUtilities.LogMessage("Play " + SoundType + " sound (start)...", "Info", SettingsManager.VerboseLogging);
                }
                else
                {
                    ManifestUtilities.LogMessage(SoundType + " sound failed to load...", "Info", SettingsManager.VerboseLogging);
                }
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format(" in LoadSounds.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        public void RunSave()
        {
            try
            {
                ManifestUtilities.LogMessage("RunSave in progress...", "info", SettingsManager.VerboseLogging);
                Save();
                ManifestUtilities.LogMessage("RunSave complete.", "info", SettingsManager.VerboseLogging);
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format(" in RunSave.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        private void Save()
        {
            try
            {
                if (HighLogic.LoadedScene == GameScenes.FLIGHT && FlightGlobals.fetch != null && FlightGlobals.ActiveVessel != null)
                {
                    ManifestUtilities.LogMessage("Save in progress...", "info", SettingsManager.VerboseLogging);
                    ShipManifestSettings.Save();
                    ManifestUtilities.LogMessage("Save comlete.", "info", SettingsManager.VerboseLogging);
                }
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format(" in Save.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        private void Savelog()
        {
            try
            {
                // time to create a file...
                string filename = "DebugLog_" + DateTime.Now.ToString().Replace(" ", "_").Replace("/", "").Replace(":", "") + ".txt";

                string path = Directory.GetCurrentDirectory() + "\\GameData\\ShipManifest\\";
                if (ShipManifestSettings.DebugLogPath.StartsWith("\\"))
                    ShipManifestSettings.DebugLogPath = ShipManifestSettings.DebugLogPath.Substring(2, ShipManifestSettings.DebugLogPath.Length - 2);

                if (!ShipManifestSettings.DebugLogPath.EndsWith("\\"))
                    ShipManifestSettings.DebugLogPath += "\\";

                filename = path + ShipManifestSettings.DebugLogPath + filename;
                ManifestUtilities.LogMessage("File Name = " + filename, "Info", SettingsManager.VerboseLogging);

                try
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (string line in ManifestUtilities.Errors)
                    {
                        sb.AppendLine(line);
                    }

                    File.WriteAllText(filename, sb.ToString());

                    ManifestUtilities.LogMessage("File written", "Info", SettingsManager.VerboseLogging);
                }
                catch (Exception ex)
                {
                    ManifestUtilities.LogMessage("Error Writing File:  " + ex.ToString(), "Info", true);
                }
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format(" in Savelog.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        public static void FireEventTriggers()
        {
            // Per suggestion by shaw (http://forum.kerbalspaceprogram.com/threads/62270-0-23-Ship-Manifest-%28Manage-Crew-Science-Resources%29-v0-23-3-1-5-26-Feb-14?p=1033866&viewfull=1#post1033866)
            // and instructions for using CLS API by codepoet.

            GameEvents.onVesselWasModified.Fire(FlightGlobals.ActiveVessel);
            GameEvents.onVesselChange.Fire(FlightGlobals.ActiveVessel);
        }

        #endregion

        public enum XFERState
        {
            Off,
            Start,
            Run,
            Stop
        }

        private void DebuggerWindow(int windowId)
        {
            GUILayout.BeginVertical();
            ManifestUtilities.DebugScrollPosition = GUILayout.BeginScrollView(ManifestUtilities.DebugScrollPosition, GUILayout.Height(300), GUILayout.Width(500));
            GUILayout.BeginVertical();

            foreach (string error in ManifestUtilities.Errors)
                GUILayout.TextArea(error, GUILayout.Width(460));

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear log", GUILayout.Height(20)))
            {
                ManifestUtilities.Errors.Clear();
                ManifestUtilities.Errors.Add("Info:  Log Cleared at " + DateTime.UtcNow.ToString() + " UTC.");
            }
            if (GUILayout.Button("Save Log", GUILayout.Height(20)))
            {
                // Create log file and save.
                Savelog();
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
        }

    }
}
