using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Toolbar;

namespace ShipManifest
{
    
    public class ResourceManifestModule : PartModule
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
    public class ResourceManifestBehaviour : MonoBehaviour
    {
        //Game object that keeps us running
        public static GameObject GameObjectInstance;
        public static SettingsManager ResourceSettings = new SettingsManager();
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
        public static double flow_rate = ResourceSettings.FlowRate;

        public static bool XferOn = false;

        private float interval = 30F;

        private IButton button;

        public void Awake()
        {
            DontDestroyOnLoad(this);
            ResourceSettings.Load();
            InvokeRepeating("RunSave", interval, interval);

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

            if (ResourceSettings.ShowDebugger)
                ResourceSettings.DebuggerPosition = GUILayout.Window(398647, ResourceSettings.DebuggerPosition, DebuggerWindow, " Ship Manifest Debug Console", GUILayout.MinHeight(20));
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
                ResourceSettings.Save();
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

            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
        }

        private void RealModeXfer()
        {
            // This method is being executed every frame (OnUpdate)
            // if we are just starting this handler load needed config.
            if (timestamp == 0)
            {
                elapsed = 0;
                ManifestUtilities.LogMessage("loading Pump sounds...", "Info");

                // Default sound license: CC-By-SA
                // http://www.freesound.org/people/vibe_crc/sounds/59328/
                string path1 = ResourceSettings.PumpSoundStart;  // "ShipManifest/Sounds/59328-1";
                string path2 = ResourceSettings.PumpSoundRun;  // "ShipManifest/Sounds/59328-2";
                string path3 = ResourceSettings.PumpSoundStop;  // "ShipManifest/Sounds/59328-3";

                GameObject go = new GameObject("Audio");

                source1 = go.AddComponent<AudioSource>();
                source2 = go.AddComponent<AudioSource>();
                source3 = go.AddComponent<AudioSource>();

                if (GameDatabase.Instance.ExistsAudioClip(path1) && GameDatabase.Instance.ExistsAudioClip(path2) && GameDatabase.Instance.ExistsAudioClip(path3))
                {
                    sound1 = GameDatabase.Instance.GetAudioClip(path1);
                    sound2 = GameDatabase.Instance.GetAudioClip(path2);
                    sound3 = GameDatabase.Instance.GetAudioClip(path3);
                    ManifestUtilities.LogMessage("Pump sounds loaded...", "Info");
                    
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
                }
                else
                {
                    ManifestUtilities.LogMessage("Pump sound failed to load...", "Info");
                }
            }

            if (ResourceSettings.VerboseLogging)
                ManifestUtilities.LogMessage("XferOn = true...", "Info");

            flow_rate = ResourceSettings.FlowRate;

            if (ResourceSettings.VerboseLogging)
                ManifestUtilities.LogMessage("FlowRate = " + flow_rate.ToString(), "Info");

            if (flow_rate == 0)
            {
                XferOn = false;
                if (ResourceSettings.VerboseLogging)
                    ManifestUtilities.LogMessage("XferOn set to False because FlowRate = 0...", "Info");

                // play pump shutdown.
                source1.Stop();
                //source3.Play();
                return;
            }
            double deltaT = 0;
            if (ResourceManifestBehaviour.timestamp > 0)
            {
                deltaT = Planetarium.GetUniversalTime() - ResourceManifestBehaviour.timestamp;
                if (ResourceSettings.VerboseLogging)
                    ManifestUtilities.LogMessage("deltaT = " + deltaT.ToString() + "  timestamp:  " + ResourceManifestBehaviour.timestamp.ToString(), "Info");
            }
            else
            {
                ResourceManifestBehaviour.timestamp = Planetarium.GetUniversalTime();
                if (ResourceSettings.VerboseLogging)
                    ManifestUtilities.LogMessage("timestamp:  " + ResourceManifestBehaviour.timestamp.ToString(), "Info");
            }

            if (deltaT > 0)
            {
                elapsed += deltaT;

                // Play run sound when start sound is nearly done. (repeats)
                if (elapsed >= source1.clip.length - 0.25 && !IsStarted)
                {
                    source2.Play();
                    IsStarted = true;
                }

                double deltaAmt = 0;
                deltaAmt = deltaT * flow_rate;

                string SelectedResource = ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedResource;

                double maxAmount = ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartTarget.Resources[SelectedResource].maxAmount;

                if (IsStarted) // Pump Start complete.
                {
                    // Drain source...
                    if (ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartSource.Resources[SelectedResource].amount - deltaAmt > 0)
                    {
                        ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartSource.Resources[SelectedResource].amount -= deltaAmt;
                        if (ResourceSettings.VerboseLogging)
                            ManifestUtilities.LogMessage("Drain - deltaAmt = " + deltaAmt.ToString(), "Info");
                    }
                    else
                    {
                        deltaAmt = ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartSource.Resources[SelectedResource].amount;
                        ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartSource.Resources[SelectedResource].amount = 0;
                        if (ResourceSettings.VerboseLogging)
                            ManifestUtilities.LogMessage("Drain - Resource = 0", "Info");
                        XferOn = false;
                    }

                    // Fill target
                    if (ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartTarget.Resources[SelectedResource].amount + deltaAmt < maxAmount)
                    {
                        ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartTarget.Resources[SelectedResource].amount += deltaAmt;
                        if (ResourceSettings.VerboseLogging)
                            ManifestUtilities.LogMessage("Fill - deltaAmt = " + deltaAmt.ToString(), "Info");
                    }
                    else
                    {
                        ManifestController.GetInstance(FlightGlobals.ActiveVessel).SelectedPartTarget.Resources[SelectedResource].amount = maxAmount;
                        if (ResourceSettings.VerboseLogging)
                            ManifestUtilities.LogMessage("Fill - MaxAmount Reached: " + maxAmount.ToString(), "Info");
                        XferOn = false;
                    }
                }
                if (!XferOn)
                {
                    // play pump shutdown.
                    source2.Stop();
                    source3.Play();
                    ResourceManifestBehaviour.timestamp = elapsed = 0;
                    IsStarted = false;
                }
                else
                {
                    ResourceManifestBehaviour.timestamp = Planetarium.GetUniversalTime();
                }
            }
            else
            {
                ResourceManifestBehaviour.timestamp = Planetarium.GetUniversalTime();
            }
        }
    }
}
