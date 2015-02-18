using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using ConnectedLivingSpace;

namespace ShipManifest
{
    internal static class WindowManifest
    {
        #region Manifest Window - Gui Layout Code

        internal static string ToolTip = "";
        internal static bool ToolTipActive = false;
        internal static bool ShowToolTips = Settings.ManifestToolTips;

        // Ship Manifest Window
        // This window displays options for managing crew, resources, and flight checklists for the focused vessel.
        private static Vector2 SMScrollViewerPosition = Vector2.zero;
        private static Vector2 ResourceScrollViewerPosition = Vector2.zero;
        internal static void Display(int windowId)
        {
            // Reset Tooltip active flag...
            ToolTipActive = false;
            ShowToolTips = Settings.ManifestToolTips;

            GUIContent label = new GUIContent("", "Close Window");
            if (SMAddon.crewXfer || SMAddon.XferOn)
            {
                label = new GUIContent("", "Action in progress.  Cannot close window");
                GUI.enabled = false;
            }
            Rect rect = new Rect(304, 4, 16, 16);
            if (GUI.Button(rect, label))
            {
                Settings.ShowShipManifest = false;
                SMAddon.smController.SelectedResource = null;
                SMAddon.smController.SelectedPartSource = SMAddon.smController.SelectedPartTarget = null;
                SMAddon.ToggleToolbar();
                ToolTip = "";
            }
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Settings.ManifestPosition, GUI.tooltip, ref ToolTipActive, 0, 0);
            GUI.enabled = true;
            try
            {
                GUILayout.BeginVertical();
                SMScrollViewerPosition = GUILayout.BeginScrollView(SMScrollViewerPosition, GUILayout.Height(100), GUILayout.Width(300));
                GUILayout.BeginVertical();

                if (SMAddon.smController.IsPreLaunch)
                {
                    PreLaunchGUI();
                }

                // Now the Resource Buttons
                ResourceButtonList();

                GUILayout.EndVertical();
                GUILayout.EndScrollView();

                GUILayout.Label(SMAddon.smController.SelectedResource != null ? string.Format("{0}", SMAddon.smController.SelectedResource) : "No Resource Selected", GUILayout.Width(300), GUILayout.Height(20));

                // Resource Details List Viewer
                ResourceDetailsViewer();

                GUILayout.BeginHorizontal();

                var settingsStyle = Settings.ShowSettings ? ManifestStyle.ButtonToggledStyle : ManifestStyle.ButtonStyle;
                if (GUILayout.Button("Settings", settingsStyle, GUILayout.Width(70), GUILayout.Height(20)))
                {
                    try
                    {
                        Settings.ShowSettings = !Settings.ShowSettings;
                    }
                    catch (Exception ex)
                    {
                        Utilities.LogMessage(string.Format(" opening Settings Window.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                    }
                }

                var rosterStyle = Settings.ShowRoster ? ManifestStyle.ButtonToggledStyle : ManifestStyle.ButtonStyle;
                if (GUILayout.Button("Roster", rosterStyle, GUILayout.Width(70), GUILayout.Height(20)))
                {
                    try
                    {
                        Settings.ShowRoster = !Settings.ShowRoster;
                        if (!Settings.ShowRoster)
                        {
                            WindowRoster.SelectedKerbal = null;
                            WindowRoster.ToolTip = "";
                        }
                    }
                    catch (Exception ex)
                    {
                        Utilities.LogMessage(string.Format(" opening Roster Window.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                    }
                }

                if (!Settings.EnableCLS)
                    GUI.enabled = false;
                var hatchesStyle = Settings.ShowHatch ? ManifestStyle.ButtonToggledStyle : ManifestStyle.ButtonStyle;
                if (GUILayout.Button("Hatches", hatchesStyle, GUILayout.Width(70), GUILayout.Height(20)))
                {
                    try
                    {
                        SMAddon.UpdateCLSSpaces();
                        SMAddon.GetHatches();
                        Settings.ShowHatch = !Settings.ShowHatch;
                    }
                    catch (Exception ex)
                    {
                        Utilities.LogMessage(string.Format(" opening Hatches Window.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                    }
                }
                GUI.enabled = true;
                var panelsStyle = Settings.ShowPanel ? ManifestStyle.ButtonToggledStyle : ManifestStyle.ButtonStyle;
                if (GUILayout.Button("Solar Panels", panelsStyle, GUILayout.Width(80), GUILayout.Height(20)))
                {
                    try
                    {
                        SMAddon.smController.GetSolarPanels();
                        Settings.ShowPanel = !Settings.ShowPanel;
                    }
                    catch (Exception ex)
                    {
                        Utilities.LogMessage(string.Format(" opening Solar Panels Window.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in Ship Manifest Window.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        #endregion

        #region Ship Manifest Window Gui components

        private static void PreLaunchGUI()
        {
            try
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(string.Format("Fill Crew"), ManifestStyle.ButtonStyle, GUILayout.Width(130), GUILayout.Height(20)))
                {
                    SMAddon.smController.FillVesselCrew();
                }
                if (GUILayout.Button(string.Format("Empty Crew"), ManifestStyle.ButtonStyle, GUILayout.Width(130), GUILayout.Height(20)))
                {
                    SMAddon.smController.EmptyVesselCrew();
                }
                GUILayout.EndHorizontal();

                if (Settings.EnablePFResources)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(string.Format("Fill Resources"), ManifestStyle.ButtonStyle, GUILayout.Width(130), GUILayout.Height(20)))
                    {
                        SMAddon.smController.FillVesselResources();
                    }
                    if (GUILayout.Button(string.Format("Empty Resources"), ManifestStyle.ButtonStyle, GUILayout.Width(130), GUILayout.Height(20)))
                    {
                        SMAddon.smController.EmptyVesselResources();
                    }
                    GUILayout.EndHorizontal();
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in PreLaunchGUI.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        private static void ResourceButtonList()
        {
            try
            {
                foreach (string resourceName in SMAddon.smController.PartsByResource.Keys)
                {
                    GUILayout.BeginHorizontal();
                    int width = 265;
                    if ((!Settings.RealismMode || SMAddon.smController.IsPreLaunch) && resourceName != "Crew" && resourceName != "Science")
                        width = 175;

                    string DisplayAmounts = Utilities.DisplayVesselResourceTotals(resourceName);
                    var style = SMAddon.smController.SelectedResource == resourceName ? ManifestStyle.ButtonToggledStyle : ManifestStyle.ButtonStyle;
                    if (GUILayout.Button(string.Format("{0}", resourceName + DisplayAmounts), style, GUILayout.Width(width), GUILayout.Height(20)))
                    {
                        try
                        {
                            if (!SMAddon.crewXfer && !SMAddon.XferOn)
                            {
                                // Now let's update our lists...
                                if (SMAddon.smController.SelectedResource != resourceName)
                                {
                                    SMAddon.smController.SelectedResource = resourceName;
                                    SMAddon.smController.SelectedPartSource = SMAddon.smController.SelectedPartTarget = null;
                                }
                                else if (SMAddon.smController.SelectedResource == resourceName)
                                {
                                    SMAddon.smController.SelectedResource = null;
                                    SMAddon.smController.SelectedPartSource = SMAddon.smController.SelectedPartTarget = null;
                                }
                                if (SMAddon.smController.SelectedResource != null)
                                {
                                    Settings.ShowTransferWindow = true;
                                }
                                else
                                {
                                    Settings.ShowTransferWindow = false;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Utilities.LogMessage(string.Format("Error selecting Resource.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                        }
                    }
                    if ((!Settings.RealismMode || SMAddon.smController.IsPreLaunch) && resourceName != "Crew" && resourceName != "Science")
                    {
                        if (GUILayout.Button(string.Format("{0}", "Dump"), ManifestStyle.ButtonStyle, GUILayout.Width(45), GUILayout.Height(20)))
                        {
                            SMAddon.smController.DumpResource(resourceName);
                        }
                        if (GUILayout.Button(string.Format("{0}", "Fill"), ManifestStyle.ButtonStyle, GUILayout.Width(35), GUILayout.Height(20)))
                        {
                            SMAddon.smController.FillResource(resourceName);
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in ResourceButtonList.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

        private static void ResourceDetailsViewer()
        {
            try
            {
                ResourceScrollViewerPosition = GUILayout.BeginScrollView(ResourceScrollViewerPosition, GUILayout.Height(100), GUILayout.Width(300));
                GUILayout.BeginVertical();

                if (SMAddon.smController.SelectedResource != null)
                {
                    foreach (Part part in SMAddon.smController.PartsByResource[SMAddon.smController.SelectedResource])
                    {
                        string resourcename = "";
                        if (SMAddon.smController.SelectedResource != "Crew" && SMAddon.smController.SelectedResource != "Science")
                        {
                            resourcename = part.Resources[SMAddon.smController.SelectedResource].info.name;
                            GUILayout.BeginHorizontal();
                            GUILayout.Label(string.Format("{0}, ({1}/{2})", part.partInfo.title, part.Resources[SMAddon.smController.SelectedResource].amount.ToString("######0.####"), part.Resources[SMAddon.smController.SelectedResource].maxAmount.ToString("######0.####")), GUILayout.Width(265));
                            GUILayout.EndHorizontal();
                        }
                        else if (SMAddon.smController.SelectedResource == "Crew")
                        {
                            resourcename = SMAddon.smController.SelectedResource;
                            GUILayout.BeginHorizontal();
                            GUILayout.Label(string.Format("{0}, ({1}/{2})", part.partInfo.title, part.protoModuleCrew.Count.ToString(), part.CrewCapacity.ToString()), GUILayout.Width(265));
                            GUILayout.EndHorizontal();
                        }
                        else if (SMAddon.smController.SelectedResource == "Science")
                        {
                            resourcename = SMAddon.smController.SelectedResource;
                            int ScienceCount = 0;
                            foreach (PartModule pm in part.Modules)
                            {
                                if (pm is ModuleScienceContainer)
                                    ScienceCount += ((ModuleScienceContainer)pm).GetScienceCount();
                                else if (pm is ModuleScienceExperiment)
                                    ScienceCount += ((ModuleScienceExperiment)pm).GetScienceCount();
                            }
                            GUILayout.BeginHorizontal();
                            GUILayout.Label(string.Format("{0}, ({1})", part.partInfo.title, ScienceCount.ToString()), GUILayout.Width(265));
                            GUILayout.EndHorizontal();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in ResourceDetailsViewer.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        #endregion
    }
}
