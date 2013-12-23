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

        private float interval = 30F;

        private IButton button;

        public void Awake()
        {
            DontDestroyOnLoad(this);
            ResourceSettings.Load();
            InvokeRepeating("RunSave", interval, interval);

            button = ToolbarManager.Instance.add("ResourceManifest", "ResourceManifest");
            button.TexturePath = "ShipManifest/Plugins/IconOff_24";
            button.ToolTip = "Resource Manifest Settings";
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
            Resources.SetupGUI();

            if (ResourceSettings.ShowDebugger)
                ResourceSettings.ResourceDebuggerPosition = GUILayout.Window(398646, ResourceSettings.ResourceDebuggerPosition, DrawDebugger, " Resource Manifest Debug Console", GUILayout.MinHeight(20));

        }
        
        public void Update()
        {
            if (FlightGlobals.fetch != null && FlightGlobals.ActiveVessel != null)
            {
                if (HighLogic.LoadedScene == GameScenes.FLIGHT)
                {
                    //Instantiate the controller for the active vessel.
                    ManifestController.GetInstance(FlightGlobals.ActiveVessel).CanDrawButton = true;
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
                ManifestUtilities.LogMessage("Saving Manifest Settings...", "Info");
                ResourceSettings.Save();
            }
        }

        private void DrawDebugger(int windowId)
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
    }
}
