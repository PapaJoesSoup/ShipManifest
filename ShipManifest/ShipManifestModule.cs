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
    public class ShipManifestAddon : MonoBehaviour
    {

        #region Properties

        //Game object that keeps us running
        public static GameObject GameObjectInstance;
        public static SettingsManager ShipManifestSettings = new SettingsManager();

        public static ManifestController smController;

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
        private static IButton ShipManifestButton_Blizzy;
        private static ApplicationLauncherButton ShipManifestButton_Stock;
        private static bool frameErrTripped = false;

        #region Event handlers

        // this is the delagate needed to support the part event handlers
        // extern is needed, as the addon is considered external to KSP, and is expected by the part delagate call.
        static extern public Part.OnActionDelegate OnMouseExit(Part part);

        // this is the method used with the delagate
        public static void MouseExit(Part part)
        {
            smController.OnMouseHighlights();
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
                // Let't try to use Blizzy's toolbar
                if (!EnableBlizzyToolBar())
                {
                    // We failed to activate the toolbar, so revert to stock
                    GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
                }
            }
            else 
            {
                // Use stock Toolbar
                Debug.Log("[ShipManifest]:  Stock Toolbar Selected.");
                GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
            }
        }

        public static bool EnableBlizzyToolBar()
        {
            if (ShipManifestSettings.EnableBlizzyToolbar)
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
                                ShipManifestButton_Blizzy.TexturePath = SettingsManager.ShowShipManifest ? "ShipManifest/Plugins/IconOff_24" : "ShipManifest/Plugins/IconOn_24";
                                SettingsManager.ShowShipManifest = !SettingsManager.ShowShipManifest;
                                if (!SettingsManager.ShowShipManifest)
                                    smController.ClearResourceHighlighting();
                                else
                                    smController.SetResourceHighlighting();
                            }
                        };
                        Debug.Log("[ShipManifest]: Blizzy Toolbar available!");
                        return true;
                    }
                    else
                    {
                        Debug.Log("[ShipManifest]: Blizzy Toolbar not available!");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    // Blizzy Toolbar instantiation error.
                    Debug.Log("[ShipManifest]: " + ex.ToString());
                    return false;
                }
            }
            else
            {
                // No Blizzy Toolbar
                Debug.Log("[ShipManifest]: Blizzy Toolbar not Enabled.");
                return false;
            }
        }

        public void ToggleBlizzyToolBar()
        {
            ShipManifestButton_Blizzy.TexturePath = SettingsManager.ShowShipManifest ? "ShipManifest/Plugins/IconOff_24" : "ShipManifest/Plugins/IconOn_24";
            SettingsManager.ShowShipManifest = !SettingsManager.ShowShipManifest;
            if (!SettingsManager.ShowShipManifest)
                smController.ClearResourceHighlighting();
            else
                smController.SetResourceHighlighting();
        }
        
        public void Start()
        {
            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                GameEvents.onVesselChange.Add(OnVesselChange);
                vessel = FlightGlobals.ActiveVessel;
                smController = ManifestController.GetInstance(vessel);
                clsAddon = CLSClient.GetCLS();
                if (clsAddon != null)
                {
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
            if (ApplicationLauncher.Ready && HighLogic.LoadedSceneIsFlight && ShipManifestButton_Stock == null)
            {
                ShipManifestButton_Stock = ApplicationLauncher.Instance.AddModApplication(
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
            if (ShipManifestButton_Stock != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(ShipManifestButton_Stock);
                GameEvents.onGameSceneLoadRequested.Remove(OnGameSceneLoadRequested);
                GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
            }
        }

        public static bool shouldToggle()
        {
            return (!MapView.MapIsEnabled && !PauseMenu.isOpen && !FlightResultsDialog.isDisplaying &&
                            FlightGlobals.fetch != null && FlightGlobals.ActiveVessel != null &&
                            smController.CanDrawButton
                            );
        }

        void onAppLaunchToggleOn()
        {
            if (!shouldToggle()) { ShipManifestButton_Stock.SetFalse(); return; }
            SettingsManager.ShowShipManifest = true;
            ShipManifestButton_Stock.SetTexture((Texture)GameDatabase.Instance.GetTexture("ShipManifest/Plugins/IconOn_24", false));
            smController.SetResourceHighlighting();
        }

        void onAppLaunchToggleOff()
        {
            if (!shouldToggle()) { ShipManifestButton_Stock.SetTrue(); return; }
            AppLaunchToggleOff();
        }

        void AppLaunchToggleOff()
        {
            SettingsManager.ShowTransferWindow = false;
            SettingsManager.ShowShipManifest = false;
            ShipManifestButton_Stock.SetTexture((Texture)GameDatabase.Instance.GetTexture("ShipManifest/Plugins/IconOff_24", false));
            smController.ClearResourceHighlighting();
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
                        smController.CanDrawButton = true;

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
                if (!frameErrTripped)
                {
                    ManifestUtilities.LogMessage(string.Format(" in Update.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                    frameErrTripped = true;
                }
            }
        }

        public void OnVesselChange(Vessel newVessel)
        {
            try
            {
                if (vessel != null && SettingsManager.ShowShipManifest)
                {
                    if (newVessel.isEVA && !vessel.isEVA)
                    {
                        // turn off SM from toolbar
                        if (ShipManifestSettings.EnableBlizzyToolbar)
                            ToggleBlizzyToolBar();
                        else
                            AppLaunchToggleOff();

                        // kill selected resource and its associated highlighting.
                        smController.SelectedResource = null;
                        smController.SelectedResourceParts = null;
                        ManifestUtilities.LogMessage("New Vessel is a Kerbal on EVA.  ", "Info", true);
                    }
                }
                vessel = newVessel;
                smController = ManifestController.GetInstance(vessel);
                if (SettingsManager.CLSInstalled && ShipManifestSettings.EnableCLS)
                    if (clsVessel != clsAddon.Vessel)
                    {
                        clsVessel = clsAddon.Vessel;
                        ManifestUtilities.LogMessage("CLS Vessel changed.", "Info", true);
                    }
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
            if (ShipManifestButton_Blizzy != null)
            {
                ShipManifestButton_Blizzy.Destroy();
            }
            if (ShipManifestButton_Stock != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(ShipManifestButton_Stock);
            }
        }

        #endregion

        #region Logic Methods

        public static bool CanBeXferred(Part SelectedPart)
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
                        results = IsCLS();
                    }
                }
                else  //SelectedPart must be SeletedPartTarget
                {
                    // Target to Source
                    if ((smController.SelectedPartSource != null && smController.SelectedPartSource != smController.SelectedPartTarget) && smController.SelectedPartTarget.protoModuleCrew.Count > 0)
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
                            if (myPart.Part == smController.SelectedPartSource)
                            {
                                smController.clsPartSource = myPart;
                                break;
                            }
                        }
                        foreach (ICLSPart myPart in mySpace.Parts)
                        {
                            if (myPart.Part == smController.SelectedPartTarget)
                            {
                                smController.clsPartTarget = myPart;
                                break;
                            }
                        }
                    }

                    if (smController.clsPartSource.Space != null && smController.clsPartTarget.Space != null && smController.clsPartSource.Space == smController.clsPartTarget.Space)
                    {
                        results = true;
                    }
                    else if (smController.clsPartSource.Space == null)
                    {
                        ManifestUtilities.LogMessage("Source part space is null.", "Error", true);
                    }
                    else if (smController.clsPartTarget.Space == null)
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
                                XferAmount = smController.tXferAmount;
                            else
                                XferAmount = smController.sXferAmount;

                            if (smController.AmtXferred + (float)deltaAmt >= XferAmount)
                            {
                                deltaAmt = XferAmount - smController.AmtXferred;
                                XferState = XFERState.Stop;
                                ManifestUtilities.LogMessage("10. Adjusted DeltaAmt = " + deltaAmt.ToString(), "Info", SettingsManager.VerboseLogging);
                            }
                            ManifestUtilities.LogMessage("11. DeltaAmt = " + deltaAmt.ToString(), "Info", SettingsManager.VerboseLogging);

                            // Lets increment the AmtXferred....
                            smController.AmtXferred += (float)deltaAmt;
                            ManifestUtilities.LogMessage("11a. AmtXferred = " + smController.AmtXferred.ToString(), "Info", SettingsManager.VerboseLogging);

                            // Drain source...
                            // and let's make sure we can move the amount requested or adjust it and stop the flow after the move.
                            if (tXferOn)
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
                            }


                            ManifestUtilities.LogMessage("12. Drain Source Part = " + deltaAmt.ToString(), "Info", SettingsManager.VerboseLogging);

                            // Fill target
                            if (tXferOn)
                                smController.SelectedPartSource.Resources[smController.SelectedResource].amount += deltaAmt;
                            else
                                smController.SelectedPartTarget.Resources[smController.SelectedResource].amount += deltaAmt;

                            ManifestUtilities.LogMessage("13. Fill Target Part = " + deltaAmt.ToString(), "Info", SettingsManager.VerboseLogging);

                            ManifestUtilities.LogMessage("Transfer State:  " + XferState.ToString() + "...", "Info", SettingsManager.VerboseLogging);
                            break;

                        case XFERState.Stop:

                            // play pump shutdown.
                            source2.Stop();
                            source3.Play();
                            timestamp = elapsed = 0;
                            XferState = XFERState.Off;
                            smController.AmtXferred = 0f;
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
                if (!frameErrTripped)
                {
                    ManifestUtilities.LogMessage(string.Format(" in RealModePumpXfer.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                    frameErrTripped = true;
                }
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
                                if (SettingsManager.EnableTextureReplacer)
                                    GameEvents.onCrewBoardVessel.Fire(smController.evaAction);

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
                if (!frameErrTripped)
                {
                    ManifestUtilities.LogMessage("Transfer State:  " + XferState.ToString() + "...", "Error", true);
                    ManifestUtilities.LogMessage(string.Format(" in RealModeCrewXfer.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                    frameErrTripped = true;
                }
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

            GameEvents.onVesselWasModified.Fire(vessel);
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
            if (GUILayout.Button("Close", GUILayout.Height(20)))
            {
                // Create log file and save.
                SettingsManager.ShowDebugger = false;
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
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
