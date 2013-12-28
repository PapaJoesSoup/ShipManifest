using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ShipManifest
{
    public partial class ManifestController
    {
        #region Singleton stuff

        private static Dictionary<WeakReference<Vessel>, ManifestController> controllers = new Dictionary<WeakReference<Vessel>, ManifestController>();

        public static ManifestController GetInstance(Vessel vessel)
        {
            foreach (var kvp in controllers.ToArray())
            {
                var wr = kvp.Key;
                var v = wr.Target;
                if (v == null)
                {
                    controllers.Remove(wr);
                    RenderingManager.RemoveFromPostDrawQueue(3, kvp.Value.drawGui);
                }
                else if (v == vessel)
                {
                    return controllers[wr];
                }
            }

            var commander = new ManifestController();
            controllers[new WeakReference<Vessel>(vessel)] = commander;
            return commander;
        }

        #endregion

        public ManifestController()
        {
            RenderingManager.AddToPostDrawQueue(3, drawGui);
        }

        public Vessel Vessel
        {
            get { return controllers.Single(p => p.Value == this).Key.Target; }
        }

        public bool IsPreLaunch
        {
            get
            {
                return Vessel.landedAt == "LaunchPad" || Vessel.landedAt == "Runway";
            }
        }

        public bool IsFlightScene
        {
            get { return HighLogic.LoadedScene == GameScenes.FLIGHT; }
        }

        public bool CanDrawButton = false;

        private string saveMessage = string.Empty;

        private void drawGui()
        {
            if (FlightGlobals.fetch == null)
            { return; }

            if (FlightGlobals.ActiveVessel != Vessel)
            { return; }

            Resources.SetupGUI();

            if (HighLogic.LoadedScene == GameScenes.FLIGHT && !MapView.MapIsEnabled && !PauseMenu.isOpen && !FlightResultsDialog.isDisplaying)
            {
                if (ShowResourceManifest)
                {
                    // Let's set all highlighting
                    SetPartHighlights();
                    ResourceManifestBehaviour.ResourceSettings.ResourceManifestPosition = GUILayout.Window(398544, ResourceManifestBehaviour.ResourceSettings.ResourceManifestPosition, ResourceManifestWindow, "Resource Manifest - " + Vessel.vesselName, GUILayout.MinHeight(20));
                }
                else
                {
                    // Let's clear all highlighting
                    if (SelectedResourceParts != null)
                    {
                        foreach (Part oldPart in SelectedResourceParts)
                        {
                            ClearHighlight(oldPart);
                        }
                        SelectedResourceParts = null;
                    }
                }

                if (ShowResourceManifest && ShowResourceTransferWindow)
                {
                    ResourceManifestBehaviour.ResourceSettings.ResourceTransferPosition = GUILayout.Window(398545, ResourceManifestBehaviour.ResourceSettings.ResourceTransferPosition, ResourceTransferWindow, "Resource Transfer - " + Vessel.vesselName + " - " + SelectedResource, GUILayout.MinHeight(20));
                }
            }
        }

        public void HideAllWindows()
        {
            ShowResourceTransferWindow = false;
            ClearHighlight(_selectedPartSource);
            ClearHighlight(_selectedPartTarget);
            _selectedPartSource = _selectedPartTarget = null; //clear selections
        }

    }
}
