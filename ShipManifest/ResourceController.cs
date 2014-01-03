using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ShipManifest
{
    public partial class ManifestController
    {
        // Resource Controller.  This module contains Resource Manifest specific code. 
        // I made id a partial class to allow file level segregation of resource and crew functionality
        // for improved readability and management by coders.

        // variables used for moving resources.  set to a negative to allow slider to function.
        public float XferAmount = -1f;

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
                    // Part has Resources, now where to put them...  (may be more than one place... :)
                    foreach (PartResource resource in part.Resources)
                    {
                        // Realism Mode.  we want to exclude solid fuels...
                        if (!ResourceManifestBehaviour.ResourceSettings.RealismMode || (ResourceManifestBehaviour.ResourceSettings.RealismMode && resource.info.name.ToLower() != "solidfuel"))
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
                SetPartHighlight(_selectedPartSource, Color.green);
                
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
                SetPartHighlight(_selectedPartTarget, Color.red);

                // reset transfer amount (for resource xfer slider control)
                XferAmount = -1f;
            }
        }

        #endregion

        #region GUI Stuff

        // Resource Manifest Window
        // This window displays a list of available resources and the parts containing each resource on the focused vessel.
        // It give you the ability to see the parts containing the selected Resource.
        private Vector2 ScrollViewerResourceManifest = Vector2.zero;
        private Vector2 ScrollViewerResourceManifest2 = Vector2.zero;
        private void ResourceManifestWindow(int windowId)
        {
            GUILayout.BeginVertical();

            ScrollViewerResourceManifest = GUILayout.BeginScrollView(ScrollViewerResourceManifest, GUILayout.Height(100), GUILayout.Width(300));
            GUILayout.BeginVertical();

            if (IsPreLaunch)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(string.Format("Fill Vessel"), GUILayout.Width(130), GUILayout.Height(20)))
                {
                    FillVesselResources();
                }
                if (GUILayout.Button(string.Format("Empty Vessel"), GUILayout.Width(130), GUILayout.Height(20)))
                {
                    EmptyVesselResources();
                }
                GUILayout.EndHorizontal();
            }

            foreach (string resourceName in PartsByResource.Keys)
            {
                var style = resourceName == SelectedResource ? ManifestStyle.ButtonToggledStyle : ManifestStyle.ButtonStyle;

                if (GUILayout.Button(string.Format("{0}", resourceName), style, GUILayout.Width(265), GUILayout.Height(20)))
                {
                    ClearHighlight(_selectedPartSource);
                    ClearHighlight(_selectedPartTarget);

                    // Let's clear all highlighting
                    if (SelectedResourceParts != null)
                    {
                        foreach (Part oldPart in SelectedResourceParts)
                        {
                             ClearHighlight(oldPart);
                        }
                    }

                    // Now let's update our lists...
                    _selectedPartSource = _selectedPartTarget = null;
                    SelectedResource = resourceName;
                    SelectedResourceParts = PartsByResource[SelectedResource];

                    // Finally, set highlights on parts with selected resource.
                    if (SelectedResourceParts != null)
                    {
                        foreach (Part newPart in SelectedResourceParts)
                        {
                            SetPartHighlight(newPart, Color.yellow);
                        }
                    }
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.Label(SelectedResource != null ? string.Format("{0}", SelectedResource) : "No Resource Selected", GUILayout.Width(300), GUILayout.Height(20));

            ScrollViewerResourceManifest2 = GUILayout.BeginScrollView(ScrollViewerResourceManifest2, GUILayout.Height(100), GUILayout.Width(300));
            GUILayout.BeginVertical();

            if (SelectedResource != null)
            {
                SelectedResourceParts = PartsByResource[SelectedResource];
                foreach (Part part in SelectedResourceParts)
                {
                    string resourcename = part.Resources[SelectedResource].info.name;
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(string.Format("{0}, ({1}/{2})", part.partInfo.title, part.Resources[SelectedResource].amount.ToString("######0.####"), part.Resources[SelectedResource].maxAmount.ToString("######0.####")), GUILayout.Width(265));
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();

            var transferStyle = ShowResourceTransferWindow ? ManifestStyle.ButtonToggledStyle : ManifestStyle.ButtonStyle;
            if (GUILayout.Button("Transfer Resource", transferStyle, GUILayout.Width(150), GUILayout.Height(20)))
            {
                if (SelectedResource != null)
                {
                    ShowResourceTransferWindow = !ShowResourceTransferWindow;
                }
            }

            var settingsStyle = ResourceManifestBehaviour.ResourceSettings.ShowSettings ? ManifestStyle.ButtonToggledStyle : ManifestStyle.ButtonStyle;
            if (GUILayout.Button("Settings", settingsStyle, GUILayout.Width(150), GUILayout.Height(20)))
            {
                ResourceManifestBehaviour.ResourceSettings.ShowSettings = !ResourceManifestBehaviour.ResourceSettings.ShowSettings;
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
        }

        // Resource Transfer Window
        // This window allows you some control over the selected resource on a selected source and target part
        private Vector2 SourceScrollViewerTransfer = Vector2.zero;
        private Vector2 SourceScrollViewerTransfer2 = Vector2.zero;
        private Vector2 TargetScrollViewerTransfer = Vector2.zero;
        private Vector2 TargetScrollViewerTransfer2 = Vector2.zero;
        private void ResourceTransferWindow(int windowId)
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
                var style = part == SelectedPartSource ? ManifestStyle.ButtonToggledStyle : ManifestStyle.ButtonStyle;

                // Draw the button and add action
                if (GUILayout.Button(string.Format("{0}", part.partInfo.title), style, GUILayout.Width(265), GUILayout.Height(20)))
                {
                    SelectedPartSource = part;
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.Label(SelectedPartSource != null ? string.Format("{0}", SelectedPartSource.partInfo.title) : "No Part Selected", GUILayout.Width(300), GUILayout.Height(20));

            // Source Part resource Details
            // this Scroll viewer is for the details of the part selected above.
            SourceScrollViewerTransfer2 = GUILayout.BeginScrollView(SourceScrollViewerTransfer2, GUILayout.Height(55), GUILayout.Width(300));
            GUILayout.BeginVertical();

            if (SelectedPartSource != null)
            {
                foreach (PartResource resource in SelectedPartSource.Resources)
                {
                    if (resource.info.name == SelectedResource)
                    {
                        // This routine assumes that a resource has been selected on the Resource manifest window.
                        string flowtext = "Off";
                        bool flowbool = SelectedPartSource.Resources[SelectedResource].flowState;
                        if (flowbool)
                        {
                            flowtext = "On";
                        }
                        else
                        {
                            flowtext = "Off";
                        }
                        PartResource.FlowMode flowmode = SelectedPartSource.Resources[SelectedResource].flowMode;

                        GUILayout.BeginHorizontal();
                        GUILayout.Label(string.Format("({0}/{1})", resource.amount.ToString("#######0.####"), resource.maxAmount.ToString("######0.####")), GUILayout.Width(175), GUILayout.Height(20));
                        GUILayout.Label(string.Format("{0}", flowtext), GUILayout.Width(30), GUILayout.Height(20));
                        //GUILayout.Label(string.Format("{0}", flowmode.ToString()), GUILayout.Width(30), GUILayout.Height(20));
                        if (GUILayout.Button("Flow", GUILayout.Width(50), GUILayout.Height(20)))
                        {
                            if (flowbool)
                            {
                                SelectedPartSource.Resources[SelectedResource].flowState = false;
                                flowtext = "Off";
                            }
                            else
                            {
                                SelectedPartSource.Resources[SelectedResource].flowState = true;
                                flowtext = "On";
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
                            XferAmount = GUILayout.HorizontalSlider(XferAmount, 0, (float) maxXferAmount, GUILayout.Width(80));

                            // set the conditions for a button style change.
                            var style = SelectedPartSource ? ManifestStyle.ButtonToggledStyle : ManifestStyle.ButtonStyle;
                            if (GUILayout.Button("Xfer", GUILayout.Width(50), GUILayout.Height(20)))
                            {
                                TransferResource(SelectedPartSource, SelectedPartTarget, (double) XferAmount);
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
                var style = part == SelectedPartTarget ? ManifestStyle.ButtonToggledRedStyle : ManifestStyle.ButtonStyle;

                if (GUILayout.Button(string.Format("{0}", part.partInfo.title), style, GUILayout.Width(265), GUILayout.Height(20)))
                {
                    SelectedPartTarget = part;
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.Label(SelectedPartTarget != null ? string.Format("{0}", SelectedPartTarget.partInfo.title) : "No Part Selected", GUILayout.Width(300), GUILayout.Height(20));

            // Target Part resource details
            TargetScrollViewerTransfer2 = GUILayout.BeginScrollView(TargetScrollViewerTransfer2, GUILayout.Height(55), GUILayout.Width(300));
            GUILayout.BeginVertical();

            // --------------------------------------------------------------------------
            if (SelectedPartTarget != null)
            {
                foreach (PartResource resource in SelectedPartTarget.Resources)
                {
                    if (resource.info.name == SelectedResource)
                    {
                        // This routine assumes that a resource has been selected on the Resource manifest window.
                        string flowtext = "Off";
                        bool flowbool = SelectedPartTarget.Resources[SelectedResource].flowState;
                        PartResource.FlowMode flowmode = SelectedPartTarget.Resources[SelectedResource].flowMode;

                        GUILayout.BeginHorizontal();
                        GUILayout.Label(string.Format("({0}/{1})", resource.amount.ToString("#######0.####"), resource.maxAmount.ToString("######0.####")), GUILayout.Width(175), GUILayout.Height(20));
                        GUILayout.Label(string.Format("{0}", flowtext), GUILayout.Width(30), GUILayout.Height(20));
                        //GUILayout.Label(string.Format("{0}", flowmode.ToString()), GUILayout.Width(30), GUILayout.Height(20));
                        if (GUILayout.Button("Flow", GUILayout.Width(50), GUILayout.Height(20)))
                        {
                            if (flowbool)
                            {
                                SelectedPartTarget.Resources[SelectedResource].flowState = false;
                                flowtext = "Off";
                            }
                            else
                            {
                                SelectedPartTarget.Resources[SelectedResource].flowState = true;
                                flowtext = "On";
                            }
                        }
                        GUILayout.EndHorizontal();
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
                    SetPartHighlight(SelectedPartSource, Color.green);
                    SetPartHighlight(SelectedPartTarget, Color.red);
                }
                else
                {
                    SetPartHighlight(SelectedPartSource, Color.yellow);
                    SetPartHighlight(SelectedPartTarget, Color.yellow);
                }
            }
        }

        // this is the delagate needed to support the part envent hadlers
        // extern is needed, as the addon is considered external to KSP, and is expected by the part delagate call.
        extern Part.OnActionDelegate OnMouseExit(Part part);

        // this is the method used with the delagate
        void MouseExit(Part part)
        {
            SetPartHighlights();
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
                if (ResourceManifestBehaviour.ResourceSettings.RealismMode)
                {
                    // now lets make some noise and slow the process down...
                    if (ResourceManifestBehaviour.ResourceSettings.VerboseLogging)
                        ManifestUtilities.LogMessage("Playing pump sound...", "Info");

                    // This flag enables the Update handler in ResourceManifestBehaviour
                    ResourceManifestBehaviour.XferOn = true;
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

        #endregion
    }
}
