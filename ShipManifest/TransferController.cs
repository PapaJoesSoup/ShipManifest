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
        public float XferAmount = -1f;
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
                    if (part.CrewCapacity > 0)
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

        // DataSource for Resources by Part  Used in Transfer window.  
        // I may change this to the partlist above.
        private List<Part> _resourcesByPart;
        public List<Part> ResourcesByPart
        {
            get
            {
                if (_resourcesByPart == null)
                    _resourcesByPart = new List<Part>();
                else
                {
                    foreach (Part part in _resourcesByPart)
                    {
                        _resourcesByPart.Add(part);
                    }
                    _resourcesByPart.Clear();
                }

                foreach (Part part in Vessel.Parts)
                {
                    if (part.Resources.Count > 0)
                    {
                        _resourcesByPart.Add(part);
                    }
                }

                return _resourcesByPart;
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
                XferAmount = -1f;
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
                XferAmount = -1f;
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
                if (GUILayout.Button("Update Portraits", GUILayout.Width(110), GUILayout.Height(20)))
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
                if (SelectedResource != "Crew" && SelectedResource != "Science")
                {
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
                                // let's determine how much of a resource we can move to the target.
                                double maxXferAmount = SelectedPartTarget.Resources[resource.info.name].maxAmount - SelectedPartTarget.Resources[resource.info.name].amount;
                                if (maxXferAmount > SelectedPartSource.Resources[resource.info.name].amount)
                                {
                                    maxXferAmount = SelectedPartSource.Resources[resource.info.name].amount;
                                }

                                // This is used to set the slider to the max amount by default.  
                                // OnUpdate draws every frame, so we need a way to ignore this or the slider will stay at max
                                // We set XferAmount to -1 when we set new source or target parts.
                                if (XferAmount == -1)
                                {
                                    XferAmount = (float)maxXferAmount;
                                }

                                // create xfer slider;
                                GUILayout.BeginHorizontal();
                                GUILayout.Label(string.Format("Xfer Amt:  {0}", XferAmount.ToString("#######0.####")), GUILayout.Width(125), GUILayout.Height(20));
                                XferAmount = GUILayout.HorizontalSlider(XferAmount, 0, (float)maxXferAmount, GUILayout.Width(80));

                                 if (GUILayout.Button("Xfer", GUILayout.Width(50), GUILayout.Height(20)))
                                {
                                    TransferResource(SelectedPartSource, SelectedPartTarget, (double)XferAmount);
                                }
                                GUILayout.EndHorizontal();
                            }
                        }
                    }
                }
                else if (SelectedResource == "Crew")
                {
                    foreach (ProtoCrewMember crewMember in SelectedPartSource.protoModuleCrew)
                    {
                        // This routine assumes that a resource has been selected on the Resource manifest window.
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(string.Format("{0}", crewMember.name), GUILayout.Width(205), GUILayout.Height(20));                        
                        if ((SelectedPartTarget != null && SelectedPartSource != SelectedPartTarget) && (SelectedPartTarget.protoModuleCrew.Count < SelectedPartTarget.CrewCapacity && SelectedPartSource.protoModuleCrew.Count > 0))
                        {
                            if (GUILayout.Button("Xfer", GUILayout.Width(50), GUILayout.Height(20)))
                            {
                                TransferCrew(SelectedPartSource, SelectedPartTarget, crewMember);
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
                            GUILayout.Label(string.Format("{0} - ({1}/{2})", pm.moduleName, scienceCount.ToString(), capacity), GUILayout.Width(205), GUILayout.Height(20));

                            // If we have target selected, it is not the same as the source, and there is science to xfer.
                            if ((SelectedModuleTarget != null && pm != SelectedModuleTarget) && scienceCount > 0)
                            {
                                if (GUILayout.Button("Xfer", GUILayout.Width(50), GUILayout.Height(20)))
                                {
                                    TransferScience(SelectedModuleSource, SelectedModuleTarget);
                                    SelectedModuleSource = SelectedModuleTarget = null;
                                }
                            }
                            GUILayout.EndHorizontal();
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
                    SelectedModuleTarget = null;
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
                if (SelectedResource != "Crew" && SelectedResource != "Science")
                {
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
                            if (GUILayout.Button("Flow", GUILayout.Width(50), GUILayout.Height(20)))
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
                        }
                    }
                }
                else if (SelectedResource == "Crew")
                {
                    foreach (ProtoCrewMember crewMember in SelectedPartTarget.protoModuleCrew)
                    {
                        // This routine assumes that a resource has been selected on the Resource manifest window.
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(string.Format("{0}", crewMember.name), GUILayout.Width(205), GUILayout.Height(20));
                        if ((SelectedPartSource != null && SelectedPartSource != SelectedPartTarget) && (SelectedPartSource.protoModuleCrew.Count < SelectedPartSource.CrewCapacity && SelectedPartTarget.protoModuleCrew.Count > 0))
                        {
                            // set the conditions for a button style change.
                            if (GUILayout.Button("Xfer", GUILayout.Width(50), GUILayout.Height(20)))
                            {
                                TransferCrew(SelectedPartTarget, SelectedPartSource, crewMember);
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                else if (SelectedResource == "Science")
                {
                    foreach (PartModule pm in SelectedPartTarget.Modules)
                    {
                        // Containers.
                        int scienceCount = 0;
                        if (pm is ModuleScienceContainer)
                        {
                            scienceCount = ((ModuleScienceContainer)pm).GetScienceCount();
                            GUILayout.BeginHorizontal();
                            GUILayout.Label(string.Format("{0} - ({1}/{2})", pm.moduleName, scienceCount.ToString(), ((ModuleScienceContainer)pm).capacity), GUILayout.Width(205), GUILayout.Height(20));
                            // set the conditions for a button style change.
                            bool ShowReceive = false;
                            if (pm == SelectedModuleTarget)
                                ShowReceive = true;
                            var style = ShowReceive ? ManifestStyle.ButtonToggledTargetStyle : ManifestStyle.ButtonStyle;
                            if (GUILayout.Button("Recv", style, GUILayout.Width(50), GUILayout.Height(20)))
                            {
                                SelectedModuleTarget = pm;
                            }
                            GUILayout.EndHorizontal();
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

        private void TransferCrew(Part source, Part target, ProtoCrewMember crewMember)
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
            if (source is ModuleScienceContainer)
                moduleScience = ((ModuleScienceContainer)source).GetData();
            else if (source is ModuleScienceExperiment)
                moduleScience = ((ModuleScienceExperiment)source).GetData();

            if (moduleScience != null && moduleScience.Length > 0)
            {
                for (i = 0; i < moduleScience.Length; i++)
                {
                    if (SettingsManager.VerboseLogging)
                        ManifestUtilities.LogMessage(string.Format("moduleScience has data..."), "Info");

                    if (((ModuleScienceContainer)target).AddData(moduleScience[i]))
                    {
                        if (source is ModuleScienceContainer)
                        {
                            if (((ModuleScienceContainer)source).IsRerunnable())
                            {
                                //((ModuleScienceContainer)source).
                                if (SettingsManager.VerboseLogging)
                                    ManifestUtilities.LogMessage(string.Format("((ModuleScienceContainer)source).IsRerunnable() = true..."), "Info");
                            }
                            else
                            {
                                //((ModuleScienceContainer)source).
                                if (SettingsManager.VerboseLogging)
                                    ManifestUtilities.LogMessage(string.Format("((ModuleScienceContainer)source).IsRerunnable() = false..."), "Info");
                            }
                            ((ModuleScienceContainer)source).RemoveData(moduleScience[i]);
                        }
                        else
                        {
                            if (((ModuleScienceExperiment)source).IsRerunnable())
                            {
                                if (SettingsManager.VerboseLogging)
                                    ManifestUtilities.LogMessage(string.Format("((ModuleScienceExperiment)source).IsRerunnable() = true..."), "Info");
                                ((ModuleScienceExperiment)source).ResetExperiment();
                            }
                            else
                            {
                                if (SettingsManager.VerboseLogging)
                                    ManifestUtilities.LogMessage(string.Format("((ModuleScienceExperiment)source).IsRerunnable() = false..."), "Info");
                                ((ModuleScienceExperiment)source).DumpData(moduleScience[i]);
                            }
                        }
                    }
                    else
                    {
                        ManifestUtilities.LogMessage(string.Format("Science Data transfer failed..."), "Info");
                    }
                }
            }
            else
            {
                if (SettingsManager.VerboseLogging)
                    ManifestUtilities.LogMessage(string.Format("moduleScience is null or empty..."), "Info");
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
                    ShipManifestBehaviour.XferOn = true;
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

        private void FillVesselResources()
        {
            foreach (Part part in ResourcesByPart)
            {
                foreach (PartResource resource in part.Resources)
                {
                    double fillAmount = resource.maxAmount;
                    resource.amount = 0;
                    resource.amount += fillAmount;
                }
             }
        }

        private void EmptyVesselResources()
        {
            foreach (Part part in ResourcesByPart)
            {
                foreach (PartResource resource in part.Resources)
                {
                    resource.amount = 0;
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
