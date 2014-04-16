using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine;
using Toolbar;
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

    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class ShipManifestBehaviour : MonoBehaviour
    {
        //Game object that keeps us running
        public static GameObject GameObjectInstance;
        public static SettingsManager ShipManifestSettings = new SettingsManager();

        public static CLSVessel clsVessel = CLSAddon.Instance.Vessel;

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
        public static Vessel vessel = null;

        private IButton button;

        #region Datasource properties

        // dataSource for Resource manifest and ResourceTransfer windows
        // Provides a list of resources and the parts that contain that resource.
        public static List<string> keyList = new List<string>();
        public static Dictionary<string, List<Part>> _partsByResource = null;
        public static Dictionary<string, List<Part>> PartsByResource
        {
            get
            {
                try
                {
                    if (_partsByResource == null || vessel == null)
                    {
                        _partsByResource = new Dictionary<string, List<Part>>();
                    }
                    if (FlightGlobals.ActiveVessel != vessel)
                    {
                        vessel = FlightGlobals.ActiveVessel;
                        clsVessel = CLSAddon.Instance.Vessel;
                        foreach (Part part in FlightGlobals.ActiveVessel.Parts)
                        {
                            // First let's Get any Crew...
                            if (part.CrewCapacity > 0 && ShipManifestBehaviour.ShipManifestSettings.EnableCrew)
                            {
                                bool vFound = false;
                                // is resource in the list yet?.
                                if (_partsByResource.Keys.Contains("Crew"))
                                {
                                    vFound = true;
                                    List<Part> eParts = _partsByResource["Crew"];
                                    eParts.Add(part);
                                }
                                if (!vFound)
                                {
                                    // found a new resource.  lets add it to the list of resources.
                                    List<Part> nParts = new List<Part>();
                                    nParts.Add(part);
                                    _partsByResource.Add("Crew", nParts);
                                }
                            }

                            // Let's Get any Science...
                            if (ShipManifestBehaviour.ShipManifestSettings.EnableScience)
                            {
                                bool mFound = false;
                                foreach (PartModule pm in part.Modules)
                                {
                                    // is resource in the list yet?.
                                    // 
                                    if (!mFound && (pm is ModuleScienceContainer || pm is ModuleScienceExperiment))
                                    {
                                        if (_partsByResource.Keys.Contains("Science"))
                                        {
                                            mFound = true;
                                            List<Part> eParts = _partsByResource["Science"];
                                            eParts.Add(part);
                                        }
                                        if (!mFound)
                                        {
                                            // found a new resource.  lets add it to the list of resources.
                                            List<Part> nParts = new List<Part>();
                                            nParts.Add(part);
                                            _partsByResource.Add("Science", nParts);
                                            mFound = true;
                                        }
                                    }
                                }
                            }

                            // Now, let's get flight Resources.
                            foreach (PartResource resource in part.Resources)
                            {
                                // Realism Mode.  we want to exclude Resources with TransferMode = NONE...
                                if (!ShipManifestBehaviour.ShipManifestSettings.RealismMode || (ShipManifestBehaviour.ShipManifestSettings.RealismMode && resource.info.resourceTransferMode != ResourceTransferMode.NONE))
                                {
                                    bool vFound = false;
                                    // is resource in the list yet?.
                                    if (_partsByResource.Keys.Contains(resource.info.name))
                                    {
                                        vFound = true;
                                        List<Part> eParts = _partsByResource[resource.info.name];
                                        eParts.Add(part);
                                    }
                                    if (!vFound)
                                    {
                                        // found a new resource.  lets add it to the list of resources.
                                        List<Part> nParts = new List<Part>();
                                        nParts.Add(part);
                                        _partsByResource.Add(resource.info.name, nParts);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ManifestUtilities.LogMessage("Error getting partsbyresource.  " + ex.ToString(), "Error", true);
                    _partsByResource = null;
                }

                if (_partsByResource != null)
                    keyList = new List<string>(_partsByResource.Keys);
                else
                    keyList = null;

                return _partsByResource;
            }
        }

        // dataSource for Resource manifest and ResourceTransfer windows
        // Holds the Resource.info.name selected in the Resource Manifest Window.
        private static string _prevSelectedResource;
        private static string _selectedResource;
        public static string SelectedResource
        {
            get
            {
                return _selectedResource;
            }
            set
            {
                try
                {
                    _prevSelectedResource = _selectedResource;
                    _selectedResource = value;

                    SelectedResourceParts = _partsByResource[_selectedResource];


                }
                catch (Exception ex)
                {
                    ManifestUtilities.LogMessage(" in Set SelectedResource.  Error:  " + ex.ToString(), "Error", true);
                }
            }
        }

        // Provides a list of parts for a given resource.
        private static List<Part> _selectedResourceParts;
        public static List<Part> SelectedResourceParts
        {
            get
            {
                if (_selectedResourceParts == null)
                    _selectedResourceParts = new List<Part>();
                return _selectedResourceParts;
            }
            set
            {
                // This removes the event handler from the currently selected parts, 
                // since we are going to be selecting different parts.
                // For Crew, we do something different so we skip.
                ChangeResourceHighlighting();

                _selectedResourceParts = value;

                SetResourceHighlighting();
                ManifestUtilities.LogMessage("Set SelectedResourceParts.", "Info", SettingsManager.VerboseLogging);
            }
        }

        private static Part _selectedPartSource;
        public static Part SelectedPartSource
        {
            get
            {
                try
                {
                    if (_selectedPartSource != null && !FlightGlobals.ActiveVessel.Parts.Contains(_selectedPartSource))
                        _selectedPartSource = null;

                    return _selectedPartSource;
                }
                catch (Exception ex)
                {
                    ManifestUtilities.LogMessage(" in Get SelectedPartSource.  Error:  " + ex.ToString(), "Error", true);
                    return null;
                }
            }
            set
            {
                try
                {

                    ClearSelectedHighlighting(false);

                    _selectedPartSource = value;

                    SetSelectedHighlighting(false);

                    // reset transfer amount (for resource xfer slider control)
                    ManifestController.GetInstance(FlightGlobals.ActiveVessel).sXferAmount = -1f;
                    ManifestController.GetInstance(FlightGlobals.ActiveVessel).tXferAmount = -1f;
                }
                catch (Exception ex)
                {
                    ManifestUtilities.LogMessage(" in Set SelectedPartSource.  Error:  " + ex.ToString(), "Error", true);
                }
                ManifestUtilities.LogMessage("Set SelectedPartSource.", "Info", SettingsManager.VerboseLogging);
            }
        }

        private static Part _selectedPartTarget;
        public static Part SelectedPartTarget
        {
            get
            {
                try
                {

                    if (_selectedPartTarget != null && !FlightGlobals.ActiveVessel.Parts.Contains(_selectedPartTarget))
                        _selectedPartTarget = null;
                    return _selectedPartTarget;
                }
                catch (Exception ex)
                {
                    ManifestUtilities.LogMessage(" in Get SelectedPartTarget.  Error:  " + ex.ToString(), "Error", true);
                    return null;
                }
            }
            set
            {
                try
                {
                    ClearSelectedHighlighting(true);

                    _selectedPartTarget = value;

                    SetSelectedHighlighting(true);

                    // reset transfer amount (for resource xfer slider control)
                    ManifestController.GetInstance(FlightGlobals.ActiveVessel).sXferAmount = -1f;
                    ManifestController.GetInstance(FlightGlobals.ActiveVessel).tXferAmount = -1f;
                }
                catch (Exception ex)
                {
                    ManifestUtilities.LogMessage(" in Set SelectedPartTarget.  Error:  " + ex.ToString(), "Error", true);
                }
                ManifestUtilities.LogMessage("Set SelectedPartTarget.", "Info", SettingsManager.VerboseLogging);
            }
        }

        public static PartModule SelectedModuleSource;
        public static PartModule SelectedModuleTarget;

        public static CLSPart CLSPartSource;
        public static CLSPart CLSPartTarget;

        public static GameEvents.FromToAction<Part, Part> evaAction;
        
        #endregion

        #region Event handlers

        // this is the delagate needed to support the part event handlers
        // extern is needed, as the addon is considered external to KSP, and is expected by the part delagate call.
        extern Part.OnActionDelegate OnMouseExit(Part part);

        // this is the method used with the delagate
        public static void MouseExit(Part part)
        {
            OnMouseHighlights();
        }

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
                    button.TexturePath = ManifestController.GetInstance(FlightGlobals.ActiveVessel).ShowShipManifest ? "ShipManifest/Plugins/IconOff_24" : "ShipManifest/Plugins/IconOn_24";
                    ManifestController.GetInstance(FlightGlobals.ActiveVessel).ShowShipManifest = !ManifestController.GetInstance(FlightGlobals.ActiveVessel).ShowShipManifest;
                    if (!ManifestController.GetInstance(FlightGlobals.ActiveVessel).ShowShipManifest)
                        RemoveResourceHighlighting();
                    else
                        SetResourceHighlighting();
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
                ManifestUtilities.LogMessage(" in Update.  Error:  " + ex.ToString(), "Error", true);
            }
        }
        
        #endregion

        #region Logic Methods

        public static bool CanBeXferred(Part SelectedPart)
        {
            bool results = false;
            try
            {
                if (SelectedPart == ShipManifestBehaviour.SelectedPartSource)
                {
                    // Source to target
                    // Are the parts capable of holding kerbals and are there kerbals to move?
                    if ((SelectedPartTarget != null && SelectedPartSource != SelectedPartTarget) &&
                                                (SelectedPartTarget.protoModuleCrew.Count < SelectedPartTarget.CrewCapacity &&
                                                SelectedPartSource.protoModuleCrew.Count > 0))
                    {
                        // now, are the parts connected to each other in the same living space?
                        results = IsCLS();
                    }
                }
                else  //SelectedPart must be SeletedPartTarget
                {
                    // Target to Source
                    if ((SelectedPartSource != null && SelectedPartSource != SelectedPartTarget) &&
                        (SelectedPartSource.protoModuleCrew.Count < SelectedPartSource.CrewCapacity &&
                        SelectedPartTarget.protoModuleCrew.Count > 0))
                    {
                        // now, are the parts connected to each other in the same living space?
                        if (IsCLS())
                        {
                            results = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(" in CanBeXferred.  Error:  " + ex.ToString(), "Error", true);
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
                    foreach (CLSSpace mySpace in clsVessel.Spaces)
                    {
                        foreach (CLSPart myPart in mySpace.Parts)
                        {
                            if ((Part)myPart == SelectedPartSource)
                            {
                                CLSPartSource = myPart;
                                break;
                            }
                        }
                        foreach (CLSPart myPart in mySpace.Parts)
                        {
                            if ((Part)myPart == SelectedPartTarget)
                            {
                                CLSPartTarget = myPart;
                                break;
                            }
                        }
                    }

                    if (CLSPartSource.Space != null && CLSPartTarget.Space != null && CLSPartSource.Space == CLSPartTarget.Space)
                    {
                        results = true;
                    }
                    else if (CLSPartSource.Space == null)
                    {
                        ManifestUtilities.LogMessage("Source part space is null.", "Error", true);
                    }
                    else if (CLSPartTarget.Space == null)
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
                ManifestUtilities.LogMessage(" in IsCLS.  Error:  " + ex.ToString(), "Error", true);
            }
            return results;
        }

        #endregion

        #region Action Methods

        public static void OnMouseHighlights()
        {
            try
            {
                if (ManifestController.GetInstance(FlightGlobals.ActiveVessel).ShowShipManifest && SelectedResourceParts != null)
                {
                    if (SelectedResource == "Crew" && ShipManifestSettings.EnableCLS)
                    {
                         if (ManifestController.GetInstance(FlightGlobals.ActiveVessel).ShowTransferWindow)
                        {
                            SetPartHighlight(SelectedPartSource, SettingsManager.Colors[SettingsManager.SourcePartColor]);
                            SetPartHighlight(SelectedPartTarget, SettingsManager.Colors[SettingsManager.TargetPartCrewColor]);
                        }
                    }
                    else
                    {
                        foreach (Part thispart in SelectedResourceParts)
                        {
                            if (thispart != SelectedPartSource && thispart != SelectedPartTarget)
                            {
                                SetPartHighlight(thispart, Color.yellow);
                            }
                        }
                        if (ManifestController.GetInstance(FlightGlobals.ActiveVessel).ShowTransferWindow)
                        {
                            SetPartHighlight(SelectedPartSource, SettingsManager.Colors[SettingsManager.SourcePartColor]);
                            SetPartHighlight(SelectedPartTarget, SettingsManager.Colors[SettingsManager.TargetPartColor]);
                        }
                        else
                        {
                            SetPartHighlight(SelectedPartSource, Color.yellow);
                            SetPartHighlight(SelectedPartTarget, Color.yellow);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(" in  OnMouseHighlights.  Error:  " + ex.ToString(), "Error", true);
            }
        }

        private void RealModePumpXfer()
        {
            try
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


                    double maxAmount = 0;
                    if (tXferOn)
                        maxAmount = SelectedPartSource.Resources[SelectedResource].maxAmount;
                    else
                        maxAmount = SelectedPartTarget.Resources[SelectedResource].maxAmount;

                    if (IsStarted) // Pump Start complete.
                    {
                        // Lets increment the AmtXferred....
                        ManifestController.GetInstance(FlightGlobals.ActiveVessel).AmtXferred += (float)deltaAmt;
                        ManifestUtilities.LogMessage("11a. AmtXferred = " + ManifestController.GetInstance(FlightGlobals.ActiveVessel).AmtXferred.ToString(), "Info", SettingsManager.VerboseLogging);

                        // Drain source...
                        if (tXferOn)
                            SelectedPartTarget.Resources[SelectedResource].amount -= deltaAmt;
                        else
                            SelectedPartSource.Resources[SelectedResource].amount -= deltaAmt;

                        ManifestUtilities.LogMessage("12. Drain Source Part = " + deltaAmt.ToString(), "Info", SettingsManager.VerboseLogging);

                        // Fill target
                        if (tXferOn)
                            SelectedPartSource.Resources[SelectedResource].amount += deltaAmt;
                        else
                            SelectedPartTarget.Resources[SelectedResource].amount += deltaAmt;

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
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(" in RealModeXfer.  Error:  " + ex.ToString(), "Error", true);
            }
        }

        private void RealModeCrewXfer()
        {
            try
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
                    SelectedPartSource.vessel.SpawnCrew();
                    SelectedPartTarget.vessel.SpawnCrew();
                    ManifestController.GetInstance(FlightGlobals.ActiveVessel).RespawnCrew();

                    // Notify Mods requiring it to update (Texture Replacer Kerbal (IVA) textures, ConnectedLivingSpaces.
                    // Per suggestion by shaw (http://forum.kerbalspaceprogram.com/threads/62270-0-23-Ship-Manifest-%28Manage-Crew-Science-Resources%29-v0-23-3-1-5-26-Feb-14?p=1033866&viewfull=1#post1033866)
                    // and instructions for using CLS API by codepoet.

                    // Add Extraplanetary LaunchPad support.   This is actually the event I was searching for back at the beginning.. yay!
                    GameEvents.onCrewBoardVessel.Fire(evaAction);
                    GameEvents.onVesselChange.Fire(FlightGlobals.ActiveVessel);

                    // Refresh after CLS refreshes...  (this maintains highlighing....)
                    clsVessel = CLSAddon.Instance.Vessel;
                    SelectedResourceParts = _partsByResource[_selectedResource];

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
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(" in RealModeCrewXfer.  Error:  " + ex.ToString(), "Error", true);
            }
        }

        private void LoadSounds(string SoundType, string path1, string path2, string path3)
        {
            try
            {
                elapsed = 0;
                ManifestUtilities.LogMessage("1. loading " + SoundType + " sounds...", "Info", true);

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
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(" in LoadSounds.  Error:  " + ex.ToString(), "Error", true);
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
                ManifestUtilities.LogMessage(" in RunSave.  Error:  " + ex.ToString(), "Error", true);
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
                ManifestUtilities.LogMessage(" in Save.  Error:  " + ex.ToString(), "Error", true);
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
                ManifestUtilities.LogMessage(" in Savelog.  Error:  " + ex.ToString(), "Error", true);
            }
        }

        public static void RemoveResourceHighlighting()
        {
            try
            {
                if (_selectedResourceParts != null && _selectedResource != "Crew")
                {
                    foreach (Part part in _selectedResourceParts)
                    {
                        ClearHighlight(part);
                        Part.OnActionDelegate OnMouseExit = MouseExit;
                        part.RemoveOnMouseExit(OnMouseExit);
                    }
                }
                if (clsVessel.Spaces != null && _selectedResource == "Crew")
                {
                    if (SelectedPartSource != null)
                    {
                        ClearHighlight(SelectedPartSource);
                        Part.OnActionDelegate OnMouseExit = MouseExit;
                        SelectedPartSource.RemoveOnMouseExit(OnMouseExit);
                    }
                    if (SelectedPartTarget != null)
                    {
                        ClearHighlight(SelectedPartTarget);
                        Part.OnActionDelegate OnMouseExit = MouseExit;
                        SelectedPartTarget.RemoveOnMouseExit(OnMouseExit);
                    }
                    foreach (CLSSpace space in clsVessel.Spaces)
                    {
                        space.Highlight(false);
                    }
                }
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(" in RemoveResourceHighlighting.  Error:  " + ex.ToString(), "Error", SettingsManager.VerboseLogging);
            }
        }

        public static void ChangeResourceHighlighting()
        {
            try
            {
                if (_selectedResourceParts != null && _prevSelectedResource != "Crew")
                {
                    foreach (Part part in _selectedResourceParts)
                    {
                        ClearHighlight(part);
                        Part.OnActionDelegate OnMouseExit = MouseExit;
                        part.RemoveOnMouseExit(OnMouseExit);
                    }
                }
                if (clsVessel.Spaces != null && _prevSelectedResource == "Crew")
                {
                    if (SelectedPartSource != null)
                    {
                        ClearHighlight(SelectedPartSource);
                        Part.OnActionDelegate OnMouseExit = MouseExit;
                        SelectedPartSource.RemoveOnMouseExit(OnMouseExit);
                    }
                    if (SelectedPartTarget != null)
                    {
                        ClearHighlight(SelectedPartTarget);
                        Part.OnActionDelegate OnMouseExit = MouseExit;
                        SelectedPartTarget.RemoveOnMouseExit(OnMouseExit);
                    }
                    foreach (CLSSpace space in clsVessel.Spaces)
                    {
                        space.Highlight(false);
                    }
                }
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(" in ChangeResourceHighlighting.  Error:  " + ex.ToString(), "Error", SettingsManager.VerboseLogging);
            }
        }

        public static void SetResourceHighlighting()
        {
            try
            {
                // This adds an event handler to each newly selected part,
                // to manage mouse exit events and preserve highlighting.
                if (clsVessel.Spaces != null && _selectedResource == "Crew")
                {
                    foreach (CLSSpace space in clsVessel.Spaces)
                    {
                        space.Highlight(true);
                    }
                    if (SelectedPartSource != null)
                    {
                        Part.OnActionDelegate OnMouseExit = MouseExit;
                        SelectedPartSource.AddOnMouseExit(OnMouseExit);
                    }
                    if (SelectedPartTarget != null)
                    {
                        Part.OnActionDelegate OnMouseExit = MouseExit;
                        SelectedPartTarget.AddOnMouseExit(OnMouseExit);
                    }
                }
                if (_selectedResourceParts != null && _selectedResource != "Crew")
                {
                    foreach (Part part in _selectedResourceParts)
                    {
                        Part.OnActionDelegate OnMouseExit = MouseExit;
                        part.AddOnMouseExit(OnMouseExit);
                    }
                }
                OnMouseHighlights();
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(" in SetResourceHighlighting.  Error:  " + ex.ToString(), "Error", SettingsManager.VerboseLogging);
            }
        }

        public static void ClearSelectedHighlighting(bool IsTargetPart)
        {
            try
            {
                Part selectedpart = null;
                if (IsTargetPart)
                {
                    selectedpart = _selectedPartTarget;
                }
                else
                    selectedpart = _selectedPartSource;

                if (ShipManifestBehaviour.SelectedResource == "Crew" && ShipManifestSettings.EnableCLS)
                {
                    if (selectedpart != null)
                    {
                        //turn off Selected part highlghting
                        Part.OnActionDelegate OnMouseExit = MouseExit;
                        selectedpart.RemoveOnMouseExit(OnMouseExit);

                        foreach (CLSPart part in clsVessel.Parts)
                        {
                            if ((Part)part == selectedpart)
                            {
                                // turn on default CLS highlighting
                                part.Highlight(true);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    // reset part to normal resource color
                    SetPartHighlight(selectedpart, Color.yellow);
                }
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(" in  ClearPartHighlighting(" + IsTargetPart.ToString() + ").  Error:  " + ex.ToString(), "Error", true);
            }
        }

        public static void SetSelectedHighlighting(bool IsTargetPart)
        {
            try
            {
                Part selectedpart = null;
                if (IsTargetPart)
                    selectedpart = _selectedPartTarget;
                else
                    selectedpart = _selectedPartSource;

                if (SelectedResource == "Crew" && ShipManifestSettings.EnableCLS)
                {
                    if (selectedpart != null)
                    {
                        foreach (CLSPart part in clsVessel.Parts)
                        {
                            if ((Part)part == selectedpart)
                            {
                                // let's turn off CLS highlighting and Turn on Target Part Highlighing.
                                if (IsTargetPart)
                                {
                                    CLSPartTarget = part;
                                    CLSPartTarget.Highlight(false);
                                }
                                else
                                {
                                    CLSPartSource = part;
                                    CLSPartSource.Highlight(false);
                                }

                                //turn on Selected part highlghting
                                if (IsTargetPart)
                                    SetPartHighlight(selectedpart, SettingsManager.Colors[SettingsManager.TargetPartCrewColor]);
                                else
                                    SetPartHighlight(selectedpart, SettingsManager.Colors[SettingsManager.SourcePartColor]);

                                Part.OnActionDelegate OnMouseExit = MouseExit;
                                selectedpart.AddOnMouseExit(OnMouseExit);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    // Set part highlighting on the target part...
                    SetPartHighlight(_selectedPartTarget, SettingsManager.Colors[SettingsManager.TargetPartColor]);
                }
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(" in  SetPartHighlighting(" + IsTargetPart.ToString() + ").  Error:  " + ex.ToString(), "Error", true);
            }
        }

        public static void ClearHighlight(Part part)
        {
            try
            {

                if (part != null)
                {
                    part.SetHighlightDefault();
                    part.SetHighlight(false);
                }
                if (SelectedPartSource != null && SelectedResource == "Crew" && ShipManifestSettings.EnableCLS)
                {
                    foreach (CLSPart thisPart in clsVessel.Parts)
                    {
                        if ((Part)thisPart == part)
                        {
                            CLSPartSource.Highlight(true);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(" in  ClearHighlight.  Error:  " + ex.ToString(), "Error", true);
            }
        }

        public static void SetPartHighlight(Part part, Color color)
        {
            try
            {

                if (part != null)
                {
                    part.SetHighlightColor(color);
                    part.SetHighlight(true);
                }
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(" in  SetPartHighlight.  Error:  " + ex.ToString(), "Error", true);
            }
        }

        #endregion

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
