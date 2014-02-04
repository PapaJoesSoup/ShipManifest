using System;
using System.Collections.Generic;
using System.Linq;
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

            if(this.part != null && part.name == "resourceManifest")
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

        public static bool XferOn = false;

        private IButton button;

        public void Awake()
        {
            DontDestroyOnLoad(this);
            ShipManifestSettings.Load();
            if (ShipManifestSettings.AutoSave)
                InvokeRepeating("RunSave", ShipManifestSettings.SaveInterval, ShipManifestSettings.SaveInterval);

            button = ToolbarManager.Instance.add("ResourceManifest", "ResourceManifest");
            button.TexturePath = "ShipManifest/Plugins/IconOff_24";
            button.ToolTip = "Ship Manifest";
            button.Visibility = new GameScenesVisibility(GameScenes.FLIGHT);
            button.OnClick += (e) =>
            {
                if (!MapView.MapIsEnabled && !PauseMenu.isOpen && !FlightResultsDialog.isDisplaying  &&
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
                ShipManifestSettings.DebuggerPosition = GUILayout.Window(398648, ShipManifestSettings.DebuggerPosition, DebuggerWindow, " Ship Manifest Debug Console", GUILayout.MinHeight(20));
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
                    if (XferOn)
                        RealModeXfer();
                }
            }
        }

        public void RunSave()
        {
            Save();
        }

        private void Save()
        {
            if (HighLogic.LoadedScene == GameScenes.FLIGHT && FlightGlobals.fetch != null && FlightGlobals.ActiveVessel != null)
            {
                ShipManifestSettings.Save();
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
                //ManifestUtilities.Errors.Count;
                ManifestUtilities.Errors.Add("Info:  Log save not yet implemented.");
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
        }

        private void RealModeXfer()
        {
            // This method is being executed every frame (OnUpdate)
            // if we are just starting this handler load needed config.
            if (ShipManifestBehaviour.timestamp == 0)
            {
                elapsed = 0;
                ManifestUtilities.LogMessage("1. loading Pump sounds...", "Info");

                // Default sound license: CC-By-SA
                // http://www.freesound.org/people/vibe_crc/sounds/59328/
                string path1 = ShipManifestSettings.PumpSoundStart; // "ShipManifest/Sounds/59328-1";
                string path2 = ShipManifestSettings.PumpSoundRun;   // "ShipManifest/Sounds/59328-2";
                string path3 = ShipManifestSettings.PumpSoundStop;  // "ShipManifest/Sounds/59328-3";

                GameObject go = new GameObject("Audio");

                source1 = go.AddComponent<AudioSource>();
                source2 = go.AddComponent<AudioSource>();
                source3 = go.AddComponent<AudioSource>();

                if (GameDatabase.Instance.ExistsAudioClip(path1) && GameDatabase.Instance.ExistsAudioClip(path2) && GameDatabase.Instance.ExistsAudioClip(path3))
                {
                    sound1 = GameDatabase.Instance.GetAudioClip(path1);
                    sound2 = GameDatabase.Instance.GetAudioClip(path2);
                    sound3 = GameDatabase.Instance.GetAudioClip(path3);
                    ManifestUtilities.LogMessage("2. Pump sounds loaded...", "Info");

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
                    ManifestUtilities.LogMessage("2a. Play pump sound (start)...", "Info");
                }
                else
                {
                    ManifestUtilities.LogMessage("3. Pump sound failed to load...", "Info");
                }
            }

            if (SettingsManager.VerboseLogging)
                ManifestUtilities.LogMessage("4. XferOn = " + XferOn.ToString() + "...", "Info");

            flow_rate = ShipManifestSettings.FlowRate;

            if (SettingsManager.VerboseLogging)
                ManifestUtilities.LogMessage("5. FlowRate = " + flow_rate.ToString(), "Info");

            if (flow_rate == 0)
            {
                XferOn = false;
                if (SettingsManager.VerboseLogging)
                    ManifestUtilities.LogMessage("6. XferOn set to False because FlowRate = 0...", "Info");

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
                if (SettingsManager.VerboseLogging)
                    ManifestUtilities.LogMessage("7. deltaT = " + deltaT.ToString() + " timestamp: " + ShipManifestBehaviour.timestamp.ToString(), "Info");
            }

            if (deltaT > 0)
            {
                ShipManifestBehaviour.timestamp = Planetarium.GetUniversalTime();
                if (SettingsManager.VerboseLogging)
                    ManifestUtilities.LogMessage("8. New timestamp: " + ShipManifestBehaviour.timestamp.ToString(), "Info");
                
                elapsed += deltaT;

                // Play run sound when start sound is nearly done. (repeats)
                if (elapsed >= source1.clip.length - 0.25 && !IsStarted)
                {
                    source2.Play();
                    IsStarted = true;
                    if (SettingsManager.VerboseLogging)
                        ManifestUtilities.LogMessage("8a. Play pump sound (run)...", "Info");
                }

                double deltaAmt = deltaT * flow_rate;
                if (SettingsManager.VerboseLogging)
                    ManifestUtilities.LogMessage("9. DeltaAmt = " + deltaAmt.ToString(), "Info");

                // This adjusts the delta when we get to the end of the xfer.
                // Also sets IsStarted = false;
                if (ManifestController.GetInstance(FlightGlobals.ActiveVessel).AmtXferred + (float)deltaAmt >= ManifestController.GetInstance(FlightGlobals.ActiveVessel).XferAmount)
                {
                    deltaAmt = ManifestController.GetInstance(FlightGlobals.ActiveVessel).XferAmount - ManifestController.GetInstance(FlightGlobals.ActiveVessel).AmtXferred;
                    XferOn = false;
                    if (SettingsManager.VerboseLogging)
                        ManifestUtilities.LogMessage("10. DeltaAmt = " + deltaAmt.ToString(), "Info");
                }
                if (SettingsManager.VerboseLogging)
                    ManifestUtilities.LogMessage("11. Adjusted DeltaAmt = " + deltaAmt.ToString(), "Info");

 
                string SelectedResource = ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedResource;

                double maxAmount = ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartTarget.Resources[SelectedResource].maxAmount;

                if (IsStarted) // Pump Start complete.
                {
                    // Lets increment the AmtXferred....
                    ManifestController.GetInstance(FlightGlobals.ActiveVessel).AmtXferred += (float)deltaAmt;
                    if (SettingsManager.VerboseLogging)
                        ManifestUtilities.LogMessage("11a. AmtXferred = " + ManifestController.GetInstance(FlightGlobals.ActiveVessel).AmtXferred.ToString(), "Info");

                    // Drain source...
                    ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartSource.Resources[SelectedResource].amount -= deltaAmt;
                    if (SettingsManager.VerboseLogging)
                        ManifestUtilities.LogMessage("12. Drain Source Part = " + deltaAmt.ToString(), "Info");

                    // Fill target
                    ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartTarget.Resources[SelectedResource].amount += deltaAmt;
                    if (SettingsManager.VerboseLogging)
                        ManifestUtilities.LogMessage("13. Fill Target Part = " + deltaAmt.ToString(), "Info");
                }
                if (!XferOn)
                {
                    // play pump shutdown.
                    source2.Stop();
                    source3.Play();
                    ShipManifestBehaviour.timestamp = elapsed = 0;
                    IsStarted = false;
                    ManifestController.GetInstance(FlightGlobals.ActiveVessel).AmtXferred = 0f;
                    if (SettingsManager.VerboseLogging)
                        ManifestUtilities.LogMessage("14. End Loop. XferOn = " + XferOn.ToString(), "Info");
                }
                else
                {
                    ShipManifestBehaviour.timestamp = Planetarium.GetUniversalTime();
                    if (SettingsManager.VerboseLogging)
                        ManifestUtilities.LogMessage("15. Continue loop. XferOn = " + XferOn.ToString(), "Info");
                }
            }
            else
            {
                ShipManifestBehaviour.timestamp = Planetarium.GetUniversalTime();
                if (SettingsManager.VerboseLogging)
                    ManifestUtilities.LogMessage("16. Continue loop. XferOn = " + XferOn.ToString(), "Info");
            }
        }
    }
}
