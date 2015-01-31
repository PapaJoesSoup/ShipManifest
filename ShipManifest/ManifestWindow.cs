using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using ConnectedLivingSpace;

namespace ShipManifest
{
    public static class ManifestWindow
    {
        #region Manifest Window - Gui Layout Code

        public static string ToolTip = "";

        // Ship Manifest Window
        // This window displays options for managing crew, resources, and flight checklists for the focused vessel.
        private static Vector2 ScrollViewerShipManifest = Vector2.zero;
        private static Vector2 ScrollViewerResourceManifest2 = Vector2.zero;
        public static void Display(int windowId)
        {
            Rect rect = new Rect(304, 4, 16, 16);
            if (GUI.Button(rect, new GUIContent("", "Close Window")))
            {
                Settings.ShowShipManifest = false;
                ShipManifestAddon.ToggleToolbar();
                ToolTip = "";
            }
            if (Event.current.type == EventType.Repaint)
                ToolTip = Utilities.SetUpToolTip(rect, Settings.ManifestPosition, GUI.tooltip);
            try
            {
                GUILayout.BeginVertical();
                ScrollViewerShipManifest = GUILayout.BeginScrollView(ScrollViewerShipManifest, GUILayout.Height(100), GUILayout.Width(300));
                GUILayout.BeginVertical();

                if (ShipManifestAddon.smController.IsPreLaunch)
                {
                    PreLaunchGUI();
                }

                // Now the Resource Buttons
                ResourceButtonList();

                GUILayout.EndVertical();
                GUILayout.EndScrollView();

                GUILayout.Label(ShipManifestAddon.smController.SelectedResource != null ? string.Format("{0}", ShipManifestAddon.smController.SelectedResource) : "No Resource Selected", GUILayout.Width(300), GUILayout.Height(20));

                // Resource Details List Viewer
                ResourceDetailsViewer();

                GUILayout.BeginHorizontal();

                var settingsStyle = Settings.ShowSettings ? ManifestStyle.ButtonToggledStyle : ManifestStyle.ButtonStyle;
                if (GUILayout.Button("Settings...", settingsStyle, GUILayout.Width(100), GUILayout.Height(20)))
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
                if (GUILayout.Button("Roster...", rosterStyle, GUILayout.Width(100), GUILayout.Height(20)))
                {
                    try
                    {
                        Settings.ShowRoster = !Settings.ShowRoster;
                    }
                    catch (Exception ex)
                    {
                        Utilities.LogMessage(string.Format(" opening Roster Window.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                    }
                }

                if (!Settings.EnableCLS)
                    GUI.enabled = false;
                var hatchesStyle = Settings.ShowHatchWindow ? ManifestStyle.ButtonToggledStyle : ManifestStyle.ButtonStyle;
                if (GUILayout.Button("Hatches...", hatchesStyle, GUILayout.Width(100), GUILayout.Height(20)))
                {
                    try
                    {
                        Settings.ShowHatchWindow = !Settings.ShowHatchWindow;
                    }
                    catch (Exception ex)
                    {
                        Utilities.LogMessage(string.Format(" opening Hatches Window.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
                    }
                }
                GUI.enabled = true;
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
                    ShipManifestAddon.smController.FillVesselCrew();
                }
                if (GUILayout.Button(string.Format("Empty Crew"), ManifestStyle.ButtonStyle, GUILayout.Width(130), GUILayout.Height(20)))
                {
                    ShipManifestAddon.smController.EmptyVesselCrew();
                }
                GUILayout.EndHorizontal();

                if (Settings.EnablePFResources)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(string.Format("Fill Resources"), ManifestStyle.ButtonStyle, GUILayout.Width(130), GUILayout.Height(20)))
                    {
                        ShipManifestAddon.smController.FillVesselResources();
                    }
                    if (GUILayout.Button(string.Format("Empty Resources"), ManifestStyle.ButtonStyle, GUILayout.Width(130), GUILayout.Height(20)))
                    {
                        ShipManifestAddon.smController.EmptyVesselResources();
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
                foreach (string resourceName in ShipManifestAddon.smController.PartsByResource.Keys)
                {
                    GUILayout.BeginHorizontal();
                    int width = 265;
                    if ((!Settings.RealismMode || ShipManifestAddon.smController.IsPreLaunch) && resourceName != "Crew" && resourceName != "Science")
                        width = 175;

                    var style = ShipManifestAddon.smController.SelectedResource == resourceName ? ManifestStyle.ButtonToggledStyle : ManifestStyle.ButtonStyle;
                    if (GUILayout.Button(string.Format("{0}", resourceName), style, GUILayout.Width(width), GUILayout.Height(20)))
                    {
                        try
                        {
                            if (!ShipManifestAddon.crewXfer && !ShipManifestAddon.XferOn)
                            {
                                // Now let's update our lists...
                                if (ShipManifestAddon.smController.SelectedResource != resourceName)
                                {
                                    ShipManifestAddon.smController.SelectedResource = resourceName;
                                    ShipManifestAddon.smController.SelectedPartSource = ShipManifestAddon.smController.SelectedPartTarget = null;
                                }
                                else if (ShipManifestAddon.smController.SelectedResource == resourceName)
                                {
                                    ShipManifestAddon.smController.SelectedResource = null;
                                    ShipManifestAddon.smController.SelectedPartSource = ShipManifestAddon.smController.SelectedPartTarget = null;
                                }
                                if (ShipManifestAddon.smController.SelectedResource != null)
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
                    if ((!Settings.RealismMode || ShipManifestAddon.smController.IsPreLaunch) && resourceName != "Crew" && resourceName != "Science")
                    {
                        if (GUILayout.Button(string.Format("{0}", "Dump"), ManifestStyle.ButtonStyle, GUILayout.Width(45), GUILayout.Height(20)))
                        {
                            ShipManifestAddon.smController.DumpResource(resourceName);
                        }
                        if (GUILayout.Button(string.Format("{0}", "Fill"), ManifestStyle.ButtonStyle, GUILayout.Width(35), GUILayout.Height(20)))
                        {
                            ShipManifestAddon.smController.FillResource(resourceName);
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
                ScrollViewerResourceManifest2 = GUILayout.BeginScrollView(ScrollViewerResourceManifest2, GUILayout.Height(100), GUILayout.Width(300));
                GUILayout.BeginVertical();

                if (ShipManifestAddon.smController.SelectedResource != null)
                {
                    foreach (Part part in ShipManifestAddon.smController.PartsByResource[ShipManifestAddon.smController.SelectedResource])
                    {
                        string resourcename = "";
                        if (ShipManifestAddon.smController.SelectedResource != "Crew" && ShipManifestAddon.smController.SelectedResource != "Science")
                        {
                            resourcename = part.Resources[ShipManifestAddon.smController.SelectedResource].info.name;
                            GUILayout.BeginHorizontal();
                            GUILayout.Label(string.Format("{0}, ({1}/{2})", part.partInfo.title, part.Resources[ShipManifestAddon.smController.SelectedResource].amount.ToString("######0.####"), part.Resources[ShipManifestAddon.smController.SelectedResource].maxAmount.ToString("######0.####")), GUILayout.Width(265));
                            GUILayout.EndHorizontal();
                        }
                        else if (ShipManifestAddon.smController.SelectedResource == "Crew")
                        {
                            resourcename = ShipManifestAddon.smController.SelectedResource;
                            GUILayout.BeginHorizontal();
                            GUILayout.Label(string.Format("{0}, ({1}/{2})", part.partInfo.title, part.protoModuleCrew.Count.ToString(), part.CrewCapacity.ToString()), GUILayout.Width(265));
                            GUILayout.EndHorizontal();
                        }
                        else if (ShipManifestAddon.smController.SelectedResource == "Science")
                        {
                            resourcename = ShipManifestAddon.smController.SelectedResource;
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
