using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ShipManifest
{
    public partial class ManifestController
    {
        // Resource Controller
        // This module contains Resource Manifest specific code.  I made it a partial class to allow file level segregation of functionality
        // for improved readability and management by coders.

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
                    // Part has Resources, now where to put it...  (may be more than one place... :)
                    foreach (PartResource resource in part.Resources)
                    {
                        bool vFound = false;
                        if (_partsByResource.Keys.Contains(resource.info.name))
                        {
                                vFound = true;
                                List<Part> eParts = _partsByResource[resource.info.name];
                                eParts.Add(part);
                        }
                        // if we iterate all the way thru _vesselResources we end up with vFound = false.
                        if (!vFound)
                        {
                            // found a new one.  lets add it to the list of resources.
                            List<Part> parts = new List<Part>();
                            parts.Add(part);
                            _partsByResource.Add(resource.info.name, parts);
                        }
                    }
                 }

                return _partsByResource;
            }
        }

        // DataSource for Resources by Part  Used in Transfer window.  
        // I may change this to the partlist above.

        private List<Part> _resourcesByPart;
        private List<Part> ResourcesByPart
        {
            get
            {
                if (_resourcesByPart == null)
                    _resourcesByPart = new List<Part>();
                else
                    _resourcesByPart.Clear();

                foreach (Part part in Vessel.Parts)
                {
                    if (part.Resources.Count > 0)
                        _resourcesByPart.Add(part);
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
                return _selectedResourceParts;
            }
            set
            {
                _selectedResourceParts = value;
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

        #endregion

        #region GUI Stuff

        private bool _showResourceTransferWindow { get; set; }
        public bool ShowResourceManifest { get; set; }


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
                var style = resourceName == SelectedResource ? Resources.ButtonToggledStyle : Resources.ButtonStyle;

                if (GUILayout.Button(string.Format("{0}", resourceName), style, GUILayout.Width(265), GUILayout.Height(20)))
                {
                    ClearHighlight(_selectedPartSource);
                    ClearHighlight(_selectedPartTarget);
                    _selectedPartSource = _selectedPartTarget = null;
                    SelectedResource = resourceName;
                    SelectedResourceParts = PartsByResource[SelectedResource];
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

            var transferStyle = _showResourceTransferWindow ? Resources.ButtonToggledStyle : Resources.ButtonStyle;

            if (GUILayout.Button("Transfer Resource", transferStyle, GUILayout.Width(150), GUILayout.Height(20)))
            {
                if (SelectedResource != null)
                {
                    _showResourceTransferWindow = !_showResourceTransferWindow;
                    if (!_showResourceTransferWindow)
                    {
                        ClearHighlight(_selectedPartSource);
                        ClearHighlight(_selectedPartTarget);
                        _selectedPartSource = _selectedPartTarget = null;
                    }
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
        }

        // Resource Transfer Window
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
            SourceScrollViewerTransfer = GUILayout.BeginScrollView(SourceScrollViewerTransfer, GUILayout.Height(100), GUILayout.Width(300));
            GUILayout.BeginVertical();

            if (SelectedResource != "")
            {
                SelectedResourceParts = PartsByResource[SelectedResource];
            }

            foreach (Part part in SelectedResourceParts)
            {
                // set the conditions for a button style change.
                var style = part == SelectedPartSource ? Resources.ButtonToggledStyle : Resources.ButtonStyle;

                // Draw the button and add action
                if (GUILayout.Button(string.Format("{0}", part.partInfo.title), style, GUILayout.Width(265), GUILayout.Height(20)))
                {
                    SelectedPartSource = part;
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.Label(SelectedPartSource != null ? string.Format("{0}", SelectedPartSource.partInfo.title) : "No Part Selected", GUILayout.Width(300), GUILayout.Height(20));

            // Source Part List
            // this Scroll viewer is for the details of the button selected above.
            SourceScrollViewerTransfer2 = GUILayout.BeginScrollView(SourceScrollViewerTransfer2, GUILayout.Height(50), GUILayout.Width(300));
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
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(string.Format("({0}/{1})", resource.amount.ToString("#######0.####"), resource.maxAmount.ToString("######0.####")), GUILayout.Width(140), GUILayout.Height(20));
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
                        GUILayout.Label(string.Format("{0}", flowtext), GUILayout.Width(30), GUILayout.Height(20));
                        if (SelectedPartTarget != null && (SelectedPartSource.Resources[resource.info.name].amount > 0 && SelectedPartTarget.Resources[resource.info.name].amount < SelectedPartTarget.Resources[resource.info.name].maxAmount))
                        {
                            // set the conditions for a button style change.
                            var style = SelectedPartSource ? Resources.ButtonToggledStyle : Resources.ButtonStyle;
                            if (GUILayout.Button("Xfer", GUILayout.Width(50), GUILayout.Height(20)))
                            {
                                TransferResource(SelectedPartSource, SelectedPartTarget, 0);
                            }
                        }
                        GUILayout.EndHorizontal();
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
            TargetScrollViewerTransfer = GUILayout.BeginScrollView(TargetScrollViewerTransfer, GUILayout.Height(100), GUILayout.Width(300));
            GUILayout.BeginVertical();

            foreach (Part part in SelectedResourceParts)
            {
                var style = part == SelectedPartTarget ? Resources.ButtonToggledRedStyle : Resources.ButtonStyle;

                if (GUILayout.Button(string.Format("{0}", part.partInfo.title), style, GUILayout.Width(265), GUILayout.Height(20)))
                {
                    SelectedPartTarget = part;
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.Label(SelectedPartTarget != null ? string.Format("{0}", SelectedPartTarget.partInfo.title) : "No Part Selected", GUILayout.Width(300), GUILayout.Height(20));

            // Target Selected Part details
            TargetScrollViewerTransfer2 = GUILayout.BeginScrollView(TargetScrollViewerTransfer2, GUILayout.Height(50), GUILayout.Width(300));
            GUILayout.BeginVertical();

            if (SelectedPartTarget != null)
            {
                foreach (PartResource resource in SelectedPartTarget.Resources)
                {
                    if (resource.info.name == SelectedResource)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(string.Format("({0}/{1})", resource.amount.ToString("######0.####"), resource.maxAmount.ToString("######0.####")), GUILayout.Width(265), GUILayout.Height(20));
                        GUILayout.EndHorizontal();
                    }
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
        }

        #endregion

        #region Methods

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

                // Fill target
                target.Resources[SelectedResource].amount += XferAmount;
 
                // Drain source...
                source.Resources[SelectedResource].amount -= XferAmount;
            }
        }

        private void FillVesselResources()
        {
            foreach (var part in ResourcesByPart)
            {
                foreach (PartResource resource in part.Resources)
                {
                    resource.amount = resource.maxAmount;
                }
             }
        }

        private void EmptyVesselResources()
        {
            foreach (var part in ResourcesByPart)
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
