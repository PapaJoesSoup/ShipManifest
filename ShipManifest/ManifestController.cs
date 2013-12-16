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
        private string saveMessage = string.Empty;

        public bool CanDrawButton = false;

        private Part _selectedPart;
        public Part SelectedPart
        {
            get
            {
                if (_selectedPart != null && !Vessel.Parts.Contains(_selectedPart))
                    _selectedPart = null;
                return _selectedPart;
            }
            set
            {
                ClearHighlight(_selectedPart);
                _selectedPart = value;
                SetPartHighlight(value, Color.yellow);
            }
        }

        private Part _selectedPartSource;
        private Part SelectedPartSource
        {
            get
            {
                if (_selectedPartSource != null && !Vessel.Parts.Contains(_selectedPartSource))
                    _selectedPartSource = null;

                return _selectedPartSource;
            }
            set
            {
                if ((value != null && _selectedPartTarget != null) && value.uid == _selectedPartTarget.uid)
                    SelectedPartTarget = null;

                ClearHighlight(_selectedPartSource);
                _selectedPartSource = value;
                SetPartHighlight(_selectedPartSource, Color.green);
            }
        }

        private Part _selectedPartTarget;
        private Part SelectedPartTarget
        {
            get
            {
                if (_selectedPartTarget != null && !Vessel.Parts.Contains(_selectedPartTarget))
                    _selectedPartTarget = null;
                return _selectedPartTarget;
            }
            set
            {
                ClearHighlight(_selectedPartTarget);
                _selectedPartTarget = value;
                SetPartHighlight(_selectedPartTarget, Color.red);
            }
        }

        public static void ClearHighlight(Part part)
        {
            if (part != null)
            {
                part.SetHighlightDefault();
                part.SetHighlight(false);
            }
        }

        public static void SetPartHighlight(Part part, Color color)
        {
            if (part != null)
            {
                part.SetHighlightColor(color);
                part.SetHighlight(true);
            }
        }

        private void drawGui()
        {
            if (FlightGlobals.fetch == null)
            { return; }

            if (FlightGlobals.ActiveVessel != Vessel)
            { return; }

            Resources.SetupGUI();

            if (resetRosterSize)
            {
                CrewManifestBehaviour.CrewSettings.RosterPosition.height = 100; //reset hight
                resetRosterSize = false;
            }

            if (HighLogic.LoadedScene == GameScenes.FLIGHT && !MapView.MapIsEnabled && !PauseMenu.isOpen && !FlightResultsDialog.isDisplaying)
            {
                if (_showRosterWindow)
                {
                    CrewManifestBehaviour.CrewSettings.RosterPosition = GUILayout.Window(398543, CrewManifestBehaviour.CrewSettings.RosterPosition, RosterWindow, "Crew Roster", GUILayout.MinHeight(20));
                }

                if (ShowCrewManifest)
                {
                    CrewManifestBehaviour.CrewSettings.ManifestPosition = GUILayout.Window(398541, CrewManifestBehaviour.CrewSettings.ManifestPosition, CrewManifestWindow, "Crew Manifest - " + Vessel.vesselName, GUILayout.MinHeight(20));
                }

                if (_showTransferWindow)
                {
                    CrewManifestBehaviour.CrewSettings.TransferPosition = GUILayout.Window(398542, CrewManifestBehaviour.CrewSettings.TransferPosition, CrewTransferWindow, "Crew Transfer - " + Vessel.vesselName, GUILayout.MinHeight(20));
                }

                if (ShowResourceManifest)
                {
                    ResourceManifestBehaviour.ResourceSettings.ResourceManifestPosition = GUILayout.Window(398544, ResourceManifestBehaviour.ResourceSettings.ResourceManifestPosition, ResourceManifestWindow, "Resource Manifest - " + Vessel.vesselName, GUILayout.MinHeight(20));
                }

                if (_showResourceTransferWindow)
                {
                    ResourceManifestBehaviour.ResourceSettings.ResourceTransferPosition = GUILayout.Window(398545, ResourceManifestBehaviour.ResourceSettings.ResourceTransferPosition, ResourceTransferWindow, "Resource Transfer - " + Vessel.vesselName, GUILayout.MinHeight(20));
                }
            }
        }

        public void HideAllWindows()
        {
            _showRosterWindow = false;
            _showTransferWindow = false;
            _showResourceTransferWindow = false;
            ClearHighlight(_selectedPart);
            ClearHighlight(_selectedPartSource);
            ClearHighlight(_selectedPartTarget);

            _selectedPart = _selectedPartSource = _selectedPartTarget = null; //clear selections
            _selectedResource = "";
        }

    }
}
