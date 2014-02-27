using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine;
using Toolbar;

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


    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class ShipManifestBehaviour : MonoBehaviour
    {
        //Game object that keeps us running
        public static GameObject GameObjectInstance;
        public static SettingsManager ShipManifestSettings = new SettingsManager();

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
        public static bool IsStarted = false;

        [KSPField(isPersistant = true)]
        public static double flow_rate = (double)ShipManifestSettings.FlowRate;

        // Resource xfer vars
        public static bool sXferOn = false;
        public static bool tXferOn = false;

        // crew xfer vars
        public static bool crewXfer = false;
        public static double crewXferDelaySec = SettingsManager.IVATimeDelaySec;
        public static bool isSeat2Seat = false;
        public static double Seat2SeatXferDelaySec = 2;

        private IButton button;

        public void Awake()
        {
            DontDestroyOnLoad(this);
            ShipManifestSettings.Load();
            if (ShipManifestSettings.AutoSave)
                InvokeRepeating("RunSave", ShipManifestSettings.SaveIntervalSec, ShipManifestSettings.SaveIntervalSec);

            button = ToolbarManager.Instance.add("ResourceManifest", "ResourceManifest");
            button.TexturePath = "ShipManifest/Plugins/IconOff_24";
            button.ToolTip = "Ship Manifest";
            button.Visibility = new GameScenesVisibility(GameScenes.FLIGHT);
            button.OnClick += (e) =>
            {
                if (!MapView.MapIsEnabled && !PauseMenu.isOpen && !FlightResultsDialog.isDisplaying &&
                    FlightGlobals.fetch != null && FlightGlobals.ActiveVessel != null &&
                    ManifestController.GetInstance(FlightGlobals.ActiveVessel).CanDrawButton
                    )
                {
                    button.TexturePath = ManifestController.GetInstance(FlightGlobals.ActiveVessel).ShowResourceManifest ? "ShipManifest/Plugins/IconOff_24" : "ShipManifest/Plugins/IconOn_24";
                    ManifestController.GetInstance(FlightGlobals.ActiveVessel).ShowResourceManifest = !ManifestController.GetInstance(FlightGlobals.ActiveVessel).ShowResourceManifest;
                }
            };
        }

        public void OnDestroy()
        {
            CancelInvoke("RunSave");
            button.Destroy();
        }

        public void OnGUI()
        {
            ManifestStyle.SetupGUI();

            if (ShipManifestSettings.ShowDebugger)
                ShipManifestSettings.DebuggerPosition = GUILayout.Window(398648, ShipManifestSettings.DebuggerPosition, DebuggerWindow, " Ship Manifest -  Debug Console - Ver. " + ShipManifestSettings.CurVersion, GUILayout.MinHeight(20));
        }

        public void Update()
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
                        RealModeXfer();
                    }

                    if (crewXfer)
                    {
                        RealModeCrewXfer();
                    }
                }
            }
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

        private void RealModeXfer()
        {
            // This method is being executed every frame (OnUpdate)
            // if we are just starting this handler load needed config.
            bool XferOn = true;
            if (ShipManifestBehaviour.timestamp == 0)
            {
                // Default sound license: CC-By-SA
                // http://www.freesound.org/people/vibe_crc/sounds/59328/
                string path1 = ShipManifestSettings.PumpSoundStart; // "ShipManifest/Sounds/59328-1";
                string path2 = ShipManifestSettings.PumpSoundRun;   // "ShipManifest/Sounds/59328-2";
                string path3 = ShipManifestSettings.PumpSoundStop;  // "ShipManifest/Sounds/59328-3";

                LoadSounds("Pump", path1, path2, path3);
            }

            ManifestUtilities.LogMessage("4. XferOn = " + XferOn.ToString() + "...", "Info", SettingsManager.VerboseLogging);

            flow_rate = ShipManifestSettings.FlowRate;

            ManifestUtilities.LogMessage("5. FlowRate = " + flow_rate.ToString(), "Info", SettingsManager.VerboseLogging);

            if (flow_rate == 0)
            {
                XferOn = false;
                ManifestUtilities.LogMessage("6. XferOn set to False because FlowRate = 0...", "Info", SettingsManager.VerboseLogging);

                // play pump shutdown.
                source1.Stop();
                //source3.Play();
                return;
            }
            double deltaT = 0;

            // Has timestamp been initiated?
            if (ShipManifestBehaviour.timestamp > 0)
            {
                deltaT = Planetarium.GetUniversalTime() - ShipManifestBehaviour.timestamp;
                ManifestUtilities.LogMessage("7. deltaT = " + deltaT.ToString() + " timestamp: " + ShipManifestBehaviour.timestamp.ToString(), "Info", SettingsManager.VerboseLogging);
            }

            if (deltaT > 0)
            {
                ShipManifestBehaviour.timestamp = Planetarium.GetUniversalTime();
                ManifestUtilities.LogMessage("8. New timestamp: " + ShipManifestBehaviour.timestamp.ToString(), "Info", SettingsManager.VerboseLogging);

                elapsed += deltaT;

                // Play run sound when start sound is nearly done. (repeats)
                if (elapsed >= source1.clip.length - 0.25 && !IsStarted)
                {
                    source2.Play();
                    IsStarted = true;
                    ManifestUtilities.LogMessage("8a. Play pump sound (run)...", "Info", SettingsManager.VerboseLogging);
                }

                double deltaAmt = deltaT * flow_rate;
                ManifestUtilities.LogMessage("9. DeltaAmt = " + deltaAmt.ToString(), "Info", SettingsManager.VerboseLogging);

                // This adjusts the delta when we get to the end of the xfer.
                // Also sets IsStarted = false;
                float XferAmount = -1f;
                if (tXferOn)
                    XferAmount = ManifestController.GetInstance(FlightGlobals.ActiveVessel).tXferAmount;
                else
                    XferAmount = ManifestController.GetInstance(FlightGlobals.ActiveVessel).sXferAmount;

                if (ManifestController.GetInstance(FlightGlobals.ActiveVessel).AmtXferred + (float)deltaAmt >= XferAmount)
                {
                    deltaAmt = XferAmount - ManifestController.GetInstance(FlightGlobals.ActiveVessel).AmtXferred;
                    XferOn = false;
                    ManifestUtilities.LogMessage("10. Adjusted DeltaAmt = " + deltaAmt.ToString(), "Info", SettingsManager.VerboseLogging);
                }
                ManifestUtilities.LogMessage("11. DeltaAmt = " + deltaAmt.ToString(), "Info", SettingsManager.VerboseLogging);


                string SelectedResource = ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedResource;

                double maxAmount = 0;
                if (tXferOn)
                    maxAmount = ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartSource.Resources[SelectedResource].maxAmount;
                else
                    maxAmount = ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartTarget.Resources[SelectedResource].maxAmount;

                if (IsStarted) // Pump Start complete.
                {
                    // Lets increment the AmtXferred....
                    ManifestController.GetInstance(FlightGlobals.ActiveVessel).AmtXferred += (float)deltaAmt;
                    ManifestUtilities.LogMessage("11a. AmtXferred = " + ManifestController.GetInstance(FlightGlobals.ActiveVessel).AmtXferred.ToString(), "Info", SettingsManager.VerboseLogging);

                    // Drain source...
                    if (tXferOn)
                        ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartTarget.Resources[SelectedResource].amount -= deltaAmt;
                    else
                        ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartSource.Resources[SelectedResource].amount -= deltaAmt;

                    ManifestUtilities.LogMessage("12. Drain Source Part = " + deltaAmt.ToString(), "Info", SettingsManager.VerboseLogging);

                    // Fill target
                    if (tXferOn)
                        ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartSource.Resources[SelectedResource].amount += deltaAmt;
                    else
                        ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartTarget.Resources[SelectedResource].amount += deltaAmt;

                    ManifestUtilities.LogMessage("13. Fill Target Part = " + deltaAmt.ToString(), "Info", SettingsManager.VerboseLogging);
                }
                if (!XferOn)
                {
                    // play pump shutdown.
                    source2.Stop();
                    source3.Play();
                    ShipManifestBehaviour.timestamp = elapsed = 0;
                    IsStarted = false;
                    ManifestController.GetInstance(FlightGlobals.ActiveVessel).AmtXferred = 0f;
                    ManifestUtilities.LogMessage("14. End Loop. XferOn = " + XferOn.ToString(), "Info", SettingsManager.VerboseLogging);
                    if (tXferOn)
                        tXferOn = false;
                    else
                        sXferOn = false;
                }
                else
                {
                    ShipManifestBehaviour.timestamp = Planetarium.GetUniversalTime();
                    ManifestUtilities.LogMessage("15. Continue loop. XferOn = " + XferOn.ToString(), "Info", SettingsManager.VerboseLogging);
                }
            }
            else
            {
                ShipManifestBehaviour.timestamp = Planetarium.GetUniversalTime();
                ManifestUtilities.LogMessage("16. Continue loop. XferOn = " + XferOn.ToString(), "Info", SettingsManager.VerboseLogging);
            }
        }

        private void RealModeCrewXfer()
        {
            if (ShipManifestBehaviour.timestamp == 0)
            {
                // Default sound license: CC-By-SA
                // http://www.freesound.org/people/adcbicycle/sounds/14214/
                string path1 = ShipManifestSettings.CrewSoundStart; // "ShipManifest/Sounds/14214-1";
                string path2 = ShipManifestSettings.CrewSoundRun;   // "ShipManifest/Sounds/14214-2";
                string path3 = ShipManifestSettings.CrewSoundStop;  // "ShipManifest/Sounds/14214-3";

                LoadSounds("Crew", path1, path2, path3);
            }

            // have we waited long enough?
            if (elapsed >= crewXferDelaySec || (isSeat2Seat && elapsed > Seat2SeatXferDelaySec))
            {
                ManifestUtilities.LogMessage("Update:  Updating Portraits...", "info", SettingsManager.VerboseLogging);

                // Spawn crew in parts and in vessel.
                ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartSource.vessel.SpawnCrew();
                ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartTarget.vessel.SpawnCrew();
                ManifestController.GetInstance(FlightGlobals.ActiveVessel).RespawnCrew();

                // Reset State vars
                crewXfer = false;
                isSeat2Seat = false;
            }
            else
            {
                double deltaT = 0;

                // Has timestamp been initiated?
                if (ShipManifestBehaviour.timestamp > 0)
                {
                    deltaT = Planetarium.GetUniversalTime() - ShipManifestBehaviour.timestamp;
                    ManifestUtilities.LogMessage("7. deltaT = " + deltaT.ToString() + " timestamp: " + ShipManifestBehaviour.timestamp.ToString(), "Info", SettingsManager.VerboseLogging);
                }
                ShipManifestBehaviour.timestamp = Planetarium.GetUniversalTime();
                if (deltaT > 0)
                {
                    elapsed += deltaT;

                    // Play run sound when start sound is nearly done. (repeats)
                    if (elapsed >= source1.clip.length - 0.25 && !IsStarted)
                    {
                        source2.Play();
                        IsStarted = true;
                        ManifestUtilities.LogMessage("8a. Play crew sound (run)...", "Info", SettingsManager.VerboseLogging);
                        ManifestUtilities.LogMessage("Update:  Crew Transfer in progress. crewXfer = " + crewXfer.ToString(), "Info", SettingsManager.VerboseLogging);
                    }
                }
             }
            if (crewXfer)
            {
                ShipManifestBehaviour.timestamp = Planetarium.GetUniversalTime();
            }
            else
            {
                // play crew sit.
                IsStarted = false;
                source2.Stop();
                source3.Play();
                ShipManifestBehaviour.timestamp = elapsed = 0;
                ManifestUtilities.LogMessage("14. End Loop. crewXfer = " + crewXfer.ToString(), "Info", SettingsManager.VerboseLogging);
                ManifestUtilities.LogMessage("Update:  Updating Portraits complete. crewXfer = " + crewXfer.ToString(), "Info", SettingsManager.VerboseLogging);
            }
        }

        private void LoadSounds(string SoundType, string path1, string path2, string path3)
        {
            elapsed = 0;
            ManifestUtilities.LogMessage("1. loading "+ SoundType + " sounds...", "Info", true);

            GameObject go = new GameObject("Audio");

            source1 = go.AddComponent<AudioSource>();
            source2 = go.AddComponent<AudioSource>();
            source3 = go.AddComponent<AudioSource>();

            if (GameDatabase.Instance.ExistsAudioClip(path1) && GameDatabase.Instance.ExistsAudioClip(path2) && GameDatabase.Instance.ExistsAudioClip(path3))
            {
                sound1 = GameDatabase.Instance.GetAudioClip(path1);
                sound2 = GameDatabase.Instance.GetAudioClip(path2);
                sound3 = GameDatabase.Instance.GetAudioClip(path3);
                ManifestUtilities.LogMessage("2. " + SoundType + " sounds loaded...", "Info", true);

                // configure sources
                source1.clip = sound1; // Start sound
                source1.volume = 1f;
                source1.pitch = 1f;

                source2.clip = sound2; // Run sound
                source2.loop = true;
                source2.volume = 1f;
                source2.pitch = 1f;

                source3.clip = sound3; // Stop Sound
                source3.volume = 1f;
                source3.pitch = 1f;

                // now let's play the Pump start sound.
                source1.Play();
                ManifestUtilities.LogMessage("2a. Play " + SoundType + " sound (start)...", "Info", true);
            }
            else
            {
                ManifestUtilities.LogMessage("3. " + SoundType + " sound failed to load...", "Info", true);
            }
        }

        public void RunSave()
        {
            ManifestUtilities.LogMessage("RunSave in progress...", "info", SettingsManager.VerboseLogging);
            Save();
            ManifestUtilities.LogMessage("RunSave complete.", "info", SettingsManager.VerboseLogging);
        }

        private void Save()
        {
            if (HighLogic.LoadedScene == GameScenes.FLIGHT && FlightGlobals.fetch != null && FlightGlobals.ActiveVessel != null)
            {
                ManifestUtilities.LogMessage("Save in progress...", "info", SettingsManager.VerboseLogging); 
                ShipManifestSettings.Save();
                ManifestUtilities.LogMessage("Save comlete.", "info", SettingsManager.VerboseLogging);
            }
        }

        private void Savelog()
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
    }
}
