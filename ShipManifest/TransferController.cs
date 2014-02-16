using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ShipManifest
{
    public partial class ManifestController
    {
        // Transfer Controller.  This module contains Resource Manifest specific code. 
        // I made it a partial class to allow file level segregation of resource and crew functionality
        // for improved readability and management by coders.

        // variables used for moving resources.  set to a negative to allow slider to function.
        public float sXferAmount = -1f;
        public float tXferAmount = -1f;
        public float AmtXferred = 0f;

        // Flags to show windows
        public bool ShowResourceTransferWindow { get; set; }
        public bool ShowResourceManifest { get; set; }


        #region Datasource properties

        // dataSource for Resource manifest and Resourcetransfer windows
        // Provides a list of resources and the parts that contain that resource.
        private Dictionary<string, List<Part>> _partsByResource;
        private Dictionary<string, List<Part>> PartsByResource
        {
            get
            {
                if (_partsByResource == null)
                    _partsByResource = new Dictionary<string, List<Part>>();
                else
                    _partsByResource.Clear();

                foreach (Part part in Vessel.Parts)
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

                    // Now, let's get Resources.
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
                return _partsByResource;
            }
        }

        // dataSource for Resource manifest and ResourceTransfer windows
        // Provides a list of parts for a given resource.
        private List<Part> _selectedResourceParts;
        public List<Part> SelectedResourceParts
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
                if (_selectedResourceParts != null)
                {
                    foreach (Part part in _selectedResourceParts)
                    {
                        Part.OnActionDelegate OnMouseExit = MouseExit;
                        part.RemoveOnMouseExit(OnMouseExit);
                    }
                }

                _selectedResourceParts = value;

                // This adds an event handler to each newly selected part,
                // to manage mouse exit events and preserve highlighting.
                if (_selectedResourceParts != null)
                {
                    foreach (Part part in _selectedResourceParts)
                    {
                         Part.OnActionDelegate OnMouseExit = MouseExit;
                        part.AddOnMouseExit(OnMouseExit);
                    }
                }
            }
        }

        // dataSource for Resource manifest and ResourceTransfer windows
        // Holds the Resource.info.name selected in the Resource Manifest Window.
        private string _selectedResource;
        public string SelectedResource
        {
            get
            {
                return _selectedResource;
            }
            set
            {
                _selectedResource = value;
                SelectedResourceParts = PartsByResource[_selectedResource];
            }
        }

        private Part _selectedPartSource;
        public Part SelectedPartSource
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

                SetPartHighlight(_selectedPartSource, Color.yellow);
                _selectedPartSource = value;
                SetPartHighlight(_selectedPartSource, SettingsManager.Colors[SettingsManager.SourcePartColor]);
                
                // reset transfer amount (for resource xfer slider control)
                sXferAmount = -1f;
                tXferAmount = -1f;
            }
        }

        private Part _selectedPartTarget;
        public Part SelectedPartTarget
        {
            get
            {
                if (_selectedPartTarget != null && !Vessel.Parts.Contains(_selectedPartTarget))
                    _selectedPartTarget = null;
                return _selectedPartTarget;
            }
            set
            {
                SetPartHighlight(_selectedPartTarget, Color.yellow);
                _selectedPartTarget = value;
                SetPartHighlight(_selectedPartTarget, SettingsManager.Colors[SettingsManager.TargetPartColor]);

                // reset transfer amount (for resource xfer slider control)
                sXferAmount = -1f;
                tXferAmount = -1f;
            }
        }

        public PartModule SelectedModuleSource;
        public PartModule SelectedModuleTarget;

        
        #endregion

        #region GUI Layout TransferWindow)

        // Resource Transfer Window
        // This window allows you some control over the selected resource on a selected source and target part
        private Vector2 SourceScrollViewerTransfer = Vector2.zero;
        private Vector2 SourceScrollViewerTransfer2 = Vector2.zero;
        private Vector2 TargetScrollViewerTransfer = Vector2.zero;
        private Vector2 TargetScrollViewerTransfer2 = Vector2.zero;
        private void TransferWindow(int windowId)
        {
            GUILayout.BeginHorizontal();
            //Left Column Begins
            GUILayout.BeginVertical();

            // This is a scroll panel (we are using it to make button lists...)
            SourceScrollViewerTransfer = GUILayout.BeginScrollView(SourceScrollViewerTransfer, GUILayout.Height(120), GUILayout.Width(300));
            GUILayout.BeginVertical();

            if (SelectedResource != "")
            {
                SelectedResourceParts = PartsByResource[SelectedResource];
            }

            foreach (Part part in SelectedResourceParts)
            {
                // set the conditions for a button style change.
                var style = part == SelectedPartSource ? ManifestStyle.ButtonToggledSourceStyle : ManifestStyle.ButtonStyle;

                // Draw the button and add action
                if (GUILayout.Button(string.Format("{0}", part.partInfo.title), style, GUILayout.Width(265), GUILayout.Height(20)))
                {
                    SelectedModuleSource = null;
                    SelectedPartSource = part;
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            // Text above Source Details.
            if (SelectedResource == "Crew")
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(SelectedPartSource != null ? string.Format("{0}", SelectedPartSource.partInfo.title) : "No Part Selected", GUILayout.Width(190), GUILayout.Height(20));
                if (GUILayout.Button("Update Portraits", ManifestStyle.ButtonStyle, GUILayout.Width(110), GUILayout.Height(20)))
                {
                    RespawnCrew();
                }
                GUILayout.EndHorizontal();
            }
            else
                GUILayout.Label(SelectedPartSource != null ? string.Format("{0}", SelectedPartSource.partInfo.title) : "No Part Selected", GUILayout.Width(300), GUILayout.Height(20));

            // Source Part resource Details
            // this Scroll viewer is for the details of the part selected above.
            SourceScrollViewerTransfer2 = GUILayout.BeginScrollView(SourceScrollViewerTransfer2, GUILayout.Height(80), GUILayout.Width(300));
            GUILayout.BeginVertical();

            if (SelectedPartSource != null)
            {
                if (SelectedResource == "Crew")
                {
                    foreach (ProtoCrewMember crewMember in SelectedPartSource.protoModuleCrew)
                    {
                        // This routine assumes that a resource has been selected on the Resource manifest window.
                        GUILayout.BeginHorizontal();
                        if (crewMember.seat != null)
                        {
                            if (GUILayout.Button(">>", ManifestStyle.ButtonStyle, GUILayout.Width(15), GUILayout.Height(20)))
                            {
                                MoveCrewMember(crewMember, SelectedPartSource);
                            }
                        }
                        GUILayout.Label(string.Format("  {0}", crewMember.name), GUILayout.Width(190), GUILayout.Height(20));                        
                        if ((SelectedPartTarget != null && SelectedPartSource != SelectedPartTarget) && (SelectedPartTarget.protoModuleCrew.Count < SelectedPartTarget.CrewCapacity && SelectedPartSource.protoModuleCrew.Count > 0))
                        {
                            if (GUILayout.Button("Xfer", ManifestStyle.ButtonStyle, GUILayout.Width(50), GUILayout.Height(20)))
                            {
                                TransferCrewMember(SelectedPartSource, SelectedPartTarget, crewMember);
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                else if (SelectedResource == "Science")
                {
                    foreach (PartModule pm in SelectedPartSource.Modules)
                    {
                        // Containers.
                        int scienceCount = 0;
                        int capacity = 0;
                        if (pm is ModuleScienceContainer)
                        {
                            scienceCount = ((ModuleScienceContainer)pm).GetScienceCount();
                            capacity = ((ModuleScienceContainer)pm).capacity;
                        }
                        else if (pm is ModuleScienceExperiment)
                        {
                            scienceCount = ((ModuleScienceExperiment)pm).GetScienceCount();
                            capacity = 1;
                        }

                        if (pm is ModuleScienceExperiment || pm is ModuleScienceContainer)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label(string.Format("{0} - ({1})", pm.moduleName, scienceCount.ToString()), GUILayout.Width(205), GUILayout.Height(20));

                            // If we have target selected, it is not the same as the source, and there is science to xfer.
                            if ((SelectedModuleTarget != null && pm != SelectedModuleTarget) && scienceCount > 0)
                            {
                                if (GUILayout.Button("Xfer", ManifestStyle.ButtonStyle, GUILayout.Width(50), GUILayout.Height(20)))
                                {
                                    SelectedModuleSource = pm;
                                    TransferScience(SelectedModuleSource, SelectedModuleTarget);
                                    SelectedModuleSource = null;
                                }
                            }
                            GUILayout.EndHorizontal();
                        }
                    }
                }
                else
                {
                    // resources are left....
                    foreach (PartResource resource in SelectedPartSource.Resources)
                    {
                        if (resource.info.name == SelectedResource)
                        {
                            // This routine assumes that a resource has been selected on the Resource manifest window.
                            string flowtextS = "Off";
                            bool flowboolS = SelectedPartSource.Resources[SelectedResource].flowState;
                            if (flowboolS)
                            {
                                flowtextS = "On";
                            }
                            else
                            {
                                flowtextS = "Off";
                            }
                            PartResource.FlowMode flowmodeS = SelectedPartSource.Resources[SelectedResource].flowMode;

                            GUILayout.BeginHorizontal();
                            GUILayout.Label(string.Format("({0}/{1})", resource.amount.ToString("#######0.####"), resource.maxAmount.ToString("######0.####")), GUILayout.Width(175), GUILayout.Height(20));
                            GUILayout.Label(string.Format("{0}", flowtextS), GUILayout.Width(30), GUILayout.Height(20));
                            if (GUILayout.Button("Flow", GUILayout.Width(50), GUILayout.Height(20)))
                            {
                                if (flowboolS)
                                {
                                    SelectedPartSource.Resources[SelectedResource].flowState = false;
                                    flowtextS = "Off";
                                }
                                else
                                {
                                    SelectedPartSource.Resources[SelectedResource].flowState = true;
                                    flowtextS = "On";
                                }
                            }
                            GUILayout.EndHorizontal();
                            if ((SelectedPartTarget != null && SelectedPartSource != SelectedPartTarget) && (SelectedPartSource.Resources[resource.info.name].amount > 0 && SelectedPartTarget.Resources[resource.info.name].amount < SelectedPartTarget.Resources[resource.info.name].maxAmount))
                            {
                                if (!ShipManifestBehaviour.tXferOn)
                                {
                                    // let's determine how much of a resource we can move to the target.
                                    double maxXferAmount = SelectedPartTarget.Resources[resource.info.name].maxAmount - SelectedPartTarget.Resources[resource.info.name].amount;
                                    if (maxXferAmount > SelectedPartSource.Resources[resource.info.name].amount)
                                    {
                                        maxXferAmount = SelectedPartSource.Resources[resource.info.name].amount;
                                    }

                                    // This is used to set the slider to the max amount by default.  
                                    // OnUpdate draws every frame, so we need a way to ignore this or the slider will stay at max
                                    // We set XferAmount to -1 when we set new source or target parts.
                                    if (sXferAmount == -1)
                                    {
                                        sXferAmount = (float)maxXferAmount;
                                    }

                                    // create xfer slider;
                                    GUILayout.BeginHorizontal();
                                    GUILayout.Label(string.Format("Xfer Amt:  {0}", sXferAmount.ToString("#######0.####")), GUILayout.Width(125), GUILayout.Height(20));
                                    sXferAmount = GUILayout.HorizontalSlider(sXferAmount, 0, (float)maxXferAmount, GUILayout.Width(80));

                                    if (GUILayout.Button("Xfer", ManifestStyle.ButtonStyle, GUILayout.Width(50), GUILayout.Height(20)))
                                    {
                                        TransferResource(SelectedPartSource, SelectedPartTarget, (double)sXferAmount);
                                    }
                                    GUILayout.EndHorizontal();
                                }
                            }
                        }
                    }
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            // Okay, we are done with the left column of the dialog...
            GUILayout.EndVertical();

            // Right Column Begins...
            GUILayout.BeginVertical();

            // target part list
            TargetScrollViewerTransfer = GUILayout.BeginScrollView(TargetScrollViewerTransfer, GUILayout.Height(120), GUILayout.Width(300));
            GUILayout.BeginVertical();

            foreach (Part part in SelectedResourceParts)
            {
                var style = part == SelectedPartTarget ? ManifestStyle.ButtonToggledTargetStyle : ManifestStyle.ButtonStyle;

                if (GUILayout.Button(string.Format("{0}", part.partInfo.title), style, GUILayout.Width(265), GUILayout.Height(20)))
                {
                    SelectedPartTarget = part;
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.Label(SelectedPartTarget != null ? string.Format("{0}", SelectedPartTarget.partInfo.title) : "No Part Selected", GUILayout.Width(300), GUILayout.Height(20));

            // Target Part resource details
            TargetScrollViewerTransfer2 = GUILayout.BeginScrollView(TargetScrollViewerTransfer2, GUILayout.Height(80), GUILayout.Width(300));
            GUILayout.BeginVertical();

            // --------------------------------------------------------------------------
            if (SelectedPartTarget != null)
            {
                if (SelectedResource == "Crew")
                {
                    foreach (ProtoCrewMember crewMember in SelectedPartTarget.protoModuleCrew)
                    {
                        // This routine assumes that a resource has been selected on the Resource manifest window.
                        GUILayout.BeginHorizontal();
                        if (crewMember.seat != null)
                        {
                            if (GUILayout.Button(">>", ManifestStyle.ButtonStyle, GUILayout.Width(15), GUILayout.Height(20)))
                            {
                                MoveCrewMember(crewMember, SelectedPartTarget);
                            }
                        }
                        GUILayout.Label(string.Format("  {0}", crewMember.name), GUILayout.Width(190), GUILayout.Height(20));
                        if ((SelectedPartSource != null && SelectedPartSource != SelectedPartTarget) && (SelectedPartSource.protoModuleCrew.Count < SelectedPartSource.CrewCapacity && SelectedPartTarget.protoModuleCrew.Count > 0))
                        {
                            // set the conditions for a button style change.
                            if (GUILayout.Button("Xfer", ManifestStyle.ButtonStyle, GUILayout.Width(50), GUILayout.Height(20)))
                            {
                                TransferCrewMember(SelectedPartTarget, SelectedPartSource, crewMember);
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                else if (SelectedResource == "Science")
                {
                    int count = 0;
                    foreach (PartModule tpm in SelectedPartTarget.Modules)
                    {
                        if (tpm is ModuleScienceContainer)
                            count += 1;
                    }

                    foreach (PartModule pm in SelectedPartTarget.Modules)
                    {
                        // Containers.
                        int scienceCount = 0;
                        if (pm is ModuleScienceContainer)
                        {
                            scienceCount = ((ModuleScienceContainer)pm).GetScienceCount();
                            GUILayout.BeginHorizontal();
                            GUILayout.Label(string.Format("{0} - ({1})", pm.moduleName, scienceCount.ToString()), GUILayout.Width(205), GUILayout.Height(20));
                            // set the conditions for a button style change.
                            bool ShowReceive = false;
                            if (pm == SelectedModuleTarget)
                                ShowReceive = true;
                            else if (count == 1)
                                ShowReceive = true;
                                SelectedModuleTarget = pm;
                            var style = ShowReceive ? ManifestStyle.ButtonToggledTargetStyle : ManifestStyle.ButtonStyle;
                            if (GUILayout.Button("Recv", style, GUILayout.Width(50), GUILayout.Height(20)))
                            {
                                SelectedModuleTarget = pm;
                            }
                            GUILayout.EndHorizontal();
                        }
                    }
                }                        
                else
                {
                    // Resources
                    foreach (PartResource resource in SelectedPartTarget.Resources)
                    {
                        if (resource.info.name == SelectedResource)
                        {
                            // This routine assumes that a resource has been selected on the Resource manifest window.
                            string flowtextT = "Off";
                            bool flowboolT = SelectedPartTarget.Resources[SelectedResource].flowState;
                            if (flowboolT)
                            {
                                flowtextT = "On";
                            }
                            else
                            {
                                flowtextT = "Off";
                            }
                            PartResource.FlowMode flowmodeT = resource.flowMode;

                            GUILayout.BeginHorizontal();
                            GUILayout.Label(string.Format("({0}/{1})", resource.amount.ToString("#######0.####"), resource.maxAmount.ToString("######0.####")), GUILayout.Width(175), GUILayout.Height(20));
                            GUILayout.Label(string.Format("{0}", flowtextT), GUILayout.Width(30), GUILayout.Height(20));
                            if (GUILayout.Button("Flow", ManifestStyle.ButtonStyle, GUILayout.Width(50), GUILayout.Height(20)))
                            {
                                if (flowboolT)
                                {
                                    SelectedPartTarget.Resources[SelectedResource].flowState = false;
                                    flowtextT = "Off";
                                }
                                else
                                {
                                    SelectedPartTarget.Resources[SelectedResource].flowState = true;
                                    flowtextT = "On";
                                }
                            }
                            GUILayout.EndHorizontal();
                            if ((SelectedPartSource != null && SelectedPartSource != SelectedPartTarget) && (SelectedPartTarget.Resources[resource.info.name].amount > 0 && SelectedPartSource.Resources[resource.info.name].amount < SelectedPartSource.Resources[resource.info.name].maxAmount))
                            {
                                // create xfer slider;
                                if (!ShipManifestBehaviour.sXferOn)
                                {
                                    // let's determine how much of a resource we can move to the Source.
                                    double maxXferAmount = SelectedPartSource.Resources[resource.info.name].maxAmount - SelectedPartSource.Resources[resource.info.name].amount;
                                    if (maxXferAmount > SelectedPartTarget.Resources[resource.info.name].amount)
                                    {
                                        maxXferAmount = SelectedPartTarget.Resources[resource.info.name].amount;
                                    }

                                    // This is used to set the slider to the max amount by default.  
                                    // OnUpdate draws every frame, so we need a way to ignore this or the slider will stay at max
                                    // We set XferAmount to -1 when we set new source or target parts.
                                    if (tXferAmount == -1)
                                    {
                                        tXferAmount = (float)maxXferAmount;
                                    }

                                    GUILayout.BeginHorizontal();
                                    GUILayout.Label(string.Format("Xfer Amt:  {0}", tXferAmount.ToString("#######0.####")), GUILayout.Width(125), GUILayout.Height(20));
                                    tXferAmount = GUILayout.HorizontalSlider(tXferAmount, 0, (float)maxXferAmount, GUILayout.Width(80));

                                    if (GUILayout.Button("Xfer", GUILayout.Width(50), GUILayout.Height(20)))
                                    {
                                        TransferResource(SelectedPartTarget, SelectedPartSource, (double)tXferAmount);
                                    }
                                    GUILayout.EndHorizontal();
                                }
                            }
                        }
                    }
                }
            }
            // --------------------------------------------------------------------------
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
        }

        #endregion

        #region Event handlers

        // this is the delagate needed to support the part event handlers
        // extern is needed, as the addon is considered external to KSP, and is expected by the part delagate call.
        extern Part.OnActionDelegate OnMouseExit(Part part);

        // this is the method used with the delagate
        void MouseExit(Part part)
        {
            SetPartHighlights();
        }

        #endregion

        #region Methods

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

        public void SetPartHighlights()
        {
            if (ShowResourceManifest && SelectedResourceParts != null)
            {
                foreach (Part thispart in SelectedResourceParts)
                {
                    if (thispart != SelectedPartSource && thispart != SelectedPartTarget)
                    {
                        SetPartHighlight(thispart, Color.yellow);
                    }
                }
                if (ShowResourceTransferWindow)
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

        private void MoveCrewMember(ProtoCrewMember crewMember, Part part)
        {
            // Build source and target seat indexes.
            int curIdx = crewMember.seatIdx;
            int newIdx = curIdx;
            InternalSeat sourceSeat = crewMember.seat;
            if (newIdx + 1 == part.CrewCapacity)
                newIdx = -1;

            // get target seat from part's inernal model
            InternalSeat targetSeat = part.internalModel.seats[newIdx + 1];

            // Do we need to swap places with another Kerbal?
            if (targetSeat.taken)
            {
                // get Kerbal to swap with through his seat...
                ProtoCrewMember targetMember = targetSeat.kerbalRef.protoCrewMember;

                // Swap places.

                // Remove the crew members from the part...
                RemoveCrew(crewMember, part);
                RemoveCrew(targetMember, part);

                // At this point, the kerbals are in the "ether".
                // this may be why there is an issue with refreshing the internal view.. 
                // It may allow (or expect) a board call from an (invisible) eva object.   
                // If I can manage to properly trigger that call... then all should properly refresh...
                // I'll look into that...

                // Add the crew members back into the part at their new seats.
                part.AddCrewmemberAt(crewMember, newIdx + 1);
                part.AddCrewmemberAt(targetMember, curIdx);

                // these lines actually spawn the kerbals in their views, but they do not seem refresh the portraits....  
                // maybe an eva object will do it.
                crewMember.seat.SpawnCrew();
                targetMember.seat.SpawnCrew();
                Vessel.SpawnCrew();
            }
            else
            {
                // Just move.
                RemoveCrew(crewMember, part);
                part.AddCrewmemberAt(crewMember, newIdx + 1);
                crewMember.seat.SpawnCrew();
                Vessel.SpawnCrew();
            }
        }

        private void TransferCrewMember(Part source, Part target, ProtoCrewMember crewMember)
        {
            RemoveCrew(crewMember, source);
            crewMember.rosterStatus = ProtoCrewMember.RosterStatus.AVAILABLE;

            target.AddCrewmember(crewMember);
            crewMember.rosterStatus = ProtoCrewMember.RosterStatus.ASSIGNED;
            
            if (crewMember.seat != null)
                crewMember.seat.SpawnCrew();
            else
                ManifestUtilities.LogMessage(string.Format("Crew member seat is null."), "Info");
        }

        private void TransferScience(PartModule source, PartModule target)
        {
            ScienceData[] moduleScience = null;
            int i;
            try
            {
                if (source is ModuleScienceContainer)
                {
                    if (((ModuleScienceContainer)source) != null)
                        moduleScience = ((ModuleScienceContainer)source).GetData();
                    else
                        moduleScience = null;
                    //ManifestUtilities.LogMessage("Science Data Count:  " + ((ModuleScienceContainer) source).GetStoredDataCount(), "Info");
                    //ManifestUtilities.LogMessage("Science Data s Collectable:  " + ((ModuleScienceContainer) source).dataIsCollectable, "Info");
                }
                else
                {
                    if (((ModuleScienceExperiment)source) != null)
                    {
                        moduleScience = ((ModuleScienceExperiment)source).GetData();
                        if (SettingsManager.VerboseLogging)
                            ManifestUtilities.LogMessage("moduleScience is collected:  ", "Info");
                    }
                    else
                    {
                        moduleScience = null;
                        if (SettingsManager.VerboseLogging)
                            ManifestUtilities.LogMessage("moduleScience is null:  ", "Info");
                    }
                }

                if (moduleScience != null && moduleScience.Length > 0)
                {
                    for (i = 0; i < moduleScience.Length; i++)
                    {
                        if (SettingsManager.VerboseLogging)
                            ManifestUtilities.LogMessage(string.Format("moduleScience has data..."), "Info");

                        if (((ModuleScienceContainer)target) != null)
                        {
                            if (((ModuleScienceContainer)target).AddData(moduleScience[i]))
                            {
                                if (source is ModuleScienceContainer)
                                {
                                    //((ModuleScienceContainer)source).
                                    if (SettingsManager.VerboseLogging)
                                        ManifestUtilities.LogMessage(string.Format("((ModuleScienceContainer)source) is not null"), "Info");

                                    ((ModuleScienceContainer)source).RemoveData(moduleScience[i]);
                                }
                                else
                                {
                                    if (ShipManifestBehaviour.ShipManifestSettings.RealismMode)
                                    {
                                        if (SettingsManager.VerboseLogging)
                                            ManifestUtilities.LogMessage(string.Format("((Module ScienceExperiment xferred.  Dump Source data"), "Info");
                                        ((ModuleScienceExperiment)source).DumpData(moduleScience[i]);
                                    }
                                    else
                                    {
                                        if (SettingsManager.VerboseLogging)
                                            ManifestUtilities.LogMessage(string.Format("((Module ScienceExperiment xferred. Reset Experiment"), "Info");
                                        ((ModuleScienceExperiment)source).ResetExperiment();
                                    }
                                }
                            }
                            else
                            {
                                ManifestUtilities.LogMessage(string.Format("Science Data transfer failed..."), "Info");
                            }
                        }
                        else
                        {
                            if (SettingsManager.VerboseLogging)
                                ManifestUtilities.LogMessage(string.Format("((ModuleScienceExperiment)target) is null"), "Info");
                        }
                    }
                    if (SettingsManager.VerboseLogging)
                        ManifestUtilities.LogMessage(string.Format("Transfer Complete."), "Info");
                }
                else if (moduleScience == null)
                {
                    if (SettingsManager.VerboseLogging)
                        ManifestUtilities.LogMessage(string.Format("moduleScience is null..."), "Info");
                }
                else  // must be length then...
                {
                    if (SettingsManager.VerboseLogging)
                        ManifestUtilities.LogMessage(string.Format("moduleScience empty (no data)..."), "Info");
                }
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage("Error in Collection:  Error:  " + ex.ToString(), "Info");
            }
        }
        
        private void TransferResource(Part source, Part target, double XferAmount)
        {
            if (source.Resources.Contains(SelectedResource) && target.Resources.Contains(SelectedResource))
            {
                double maxAmount = target.Resources[SelectedResource].maxAmount;
                double sourceAmount = source.Resources[SelectedResource].amount;
                double targetAmount = target.Resources[SelectedResource].amount;
                if (XferAmount == 0)
                {
                    XferAmount = maxAmount - targetAmount;
                }

                // make sure we have enough...
                if (XferAmount > sourceAmount)
                {
                    XferAmount = sourceAmount;
                }
                if (ShipManifestBehaviour.ShipManifestSettings.RealismMode)
                {
                    // now lets make some noise and slow the process down...
                    if (SettingsManager.VerboseLogging)
                        ManifestUtilities.LogMessage("Playing pump sound...", "Info");


                    // This flag enables the Update handler in ResourceManifestBehaviour
                    if (source == SelectedPartSource)
                        ShipManifestBehaviour.sXferOn = true;
                    else
                        ShipManifestBehaviour.tXferOn = true;
                }
                else
                {
                    // Fill target
                    target.Resources[SelectedResource].amount += XferAmount;

                    // Drain source...
                    source.Resources[SelectedResource].amount -= XferAmount;
                }
            }
        }

        private InternalSeat GetFreeSeat(List<InternalSeat> seats)
        {
            foreach (InternalSeat seat in seats)
            {
                if (!seat.taken)
                {
                    return seat;
                }
            }
            return null;
        }

        private void logCrewMember(ProtoCrewMember crewMember, Part target)
        {
            try
            {
                ManifestUtilities.LogMessage(string.Format("Crew member:"), "Info");
                ManifestUtilities.LogMessage(string.Format(" - Name:          " + crewMember.name), "Info");
                ManifestUtilities.LogMessage(string.Format(" - KerbalRef:     " + (crewMember.KerbalRef != null).ToString()), "Info");
                if (crewMember.KerbalRef != null)
                    ManifestUtilities.LogMessage(" - kerbalRef.Name:   " + crewMember.KerbalRef.name, "info");
                ManifestUtilities.LogMessage(string.Format(" - rosterStatus:  " + crewMember.rosterStatus.ToString()), "Info");
                ManifestUtilities.LogMessage(string.Format(" - seatIdx:       " + crewMember.seatIdx.ToString()), "Info");
                ManifestUtilities.LogMessage(string.Format(" - seat != null:  " + (crewMember.seat != null).ToString()), "Info");
                if (crewMember.seat != null)
                {
                    ManifestUtilities.LogMessage(string.Format(" - seat part name:     " + crewMember.seat.part.partInfo.name.ToString()), "Info");
                    //ManifestUtilities.LogMessage(string.Format(" - portraitCamera:     " + crewMember.seat.portraitCamera.enabled.ToString()), "Info");
                }
            }
            catch (Exception ex)
            {
                ManifestUtilities.LogMessage(string.Format("Error logging crewMember:  " + ex.ToString()), "Error");
            }
        }
        #endregion
    }
}
