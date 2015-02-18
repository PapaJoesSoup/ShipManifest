using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ConnectedLivingSpace;

namespace ShipManifest
{
    static class WindowSolarPanel
    {
        internal static string ToolTip = "";
        internal static bool ToolTipActive = false;
        internal static bool ShowToolTips = Settings.PanelToolTips;

        private static Vector2 DisplayViewerPosition = Vector2.zero;
        internal static void Display(int windowId)
        {
            // Reset Tooltip active flag...
            ToolTipActive = false;
            ShowToolTips = Settings.ShowToolTips;

            Rect rect = new Rect(366, 4, 16, 16);
            if (GUI.Button(rect, new GUIContent("", "Close Window")))
            {
                Settings.ShowPanel = false;
                ToolTip = "";
            }
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Settings.PanelPosition, GUI.tooltip, ref ToolTipActive, 0, 0);

            // This is a scroll panel (we are using it to make button lists...)
            GUILayout.BeginVertical();
            DisplayViewerPosition = GUILayout.BeginScrollView(DisplayViewerPosition, GUILayout.Height(200), GUILayout.Width(370));
            GUILayout.BeginVertical();
            GUI.enabled = true;
            GUILayout.Label("--------------------------------------------------------------", GUILayout.Height(10));
            GUILayout.Label("Deployable Solar Panel Control Center ", GUILayout.Height(10));
            GUILayout.Label("--------------------------------------------------------------", GUILayout.Height(16));
            string step = "start";
            try
            {
                // Display all hatches
                foreach (SolarPanel iPanel in SMAddon.smController.SolarPanels)
                {
                    bool isEnabled = true;
                    string label = iPanel.PanelStatus + " - " + iPanel.Title;
                    if (iPanel.PanelState == ModuleDeployableSolarPanel.panelStates.BROKEN)
                    {
                        isEnabled = false;
                        label = iPanel.PanelStatus + " - (Broken) - " + iPanel.Title;
                    }
                    bool open = true;
                    if (iPanel.PanelState == ModuleDeployableSolarPanel.panelStates.RETRACTED || iPanel.PanelState == ModuleDeployableSolarPanel.panelStates.RETRACTING || iPanel.PanelState == ModuleDeployableSolarPanel.panelStates.BROKEN)
                        open = false;

                    step = "gui enable";
                    GUI.enabled = isEnabled;
                    if (!iPanel.CanBeRetracted)
                    {
                        label = iPanel.PanelStatus + " - (Locked) - " + iPanel.Title;;
                        isEnabled = false;
                    }
                    bool newOpen = GUILayout.Toggle(open, label, GUILayout.Width(330));
                    step = "button toggle check";
                    if (!open && newOpen)
                        iPanel.ExtendPanel();
                    else if (open && !newOpen)
                        iPanel.RetractPanel();

                    if (Event.current.type == EventType.Repaint)
                        iPanel.Highlight(GUILayoutUtility.GetLastRect());
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in Solar Panel Window at step {0}.  Error:  {1} \r\n\r\n{2}", step, ex.Message, ex.StackTrace), "Error", true);
            }
            GUI.enabled = true;
            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Retract All Solar Panels", GUILayout.Height(20)))
                RetractAllPanels();

            if (GUILayout.Button("Extend All Solar Panels", GUILayout.Height(20)))
                ExtendAllPanels();

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
        }

        internal static void ExtendAllPanels()
        {
            // iterate thru the hatch parts and open hatches
            // TODO: for realism, add a delay and a closing/opening sound
            foreach (SolarPanel iPanel in SMAddon.smController.SolarPanels)
            {
                ModuleDeployableSolarPanel iModule = (ModuleDeployableSolarPanel)iPanel.PanelModule;
                if (iModule.panelState == ModuleDeployableSolarPanel.panelStates.RETRACTED)
                {
                    iModule.Extend();
                }
            }
        }

        internal static void RetractAllPanels()
        {
            // iterate thru the hatch parts and open hatches
            // TODO: for realism, add a delay and a closing/opening sound
            foreach (SolarPanel iPanel in SMAddon.smController.SolarPanels)
            {
                ModuleDeployableSolarPanel iModule = (ModuleDeployableSolarPanel)iPanel.PanelModule;
                if (iModule.panelState == ModuleDeployableSolarPanel.panelStates.EXTENDED)
                {
                    iModule.Retract();
                }
            }
        }

    }
}
