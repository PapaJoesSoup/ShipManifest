using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ConnectedLivingSpace;

namespace ShipManifest
{
    static class TabSolarPanel
    {
        internal static string ToolTip = "";
        internal static bool ToolTipActive = false;
        internal static bool ShowToolTips = true;

        private static Vector2 DisplayViewerPosition = Vector2.zero;
        internal static void Display()
        {
            // Reset Tooltip active flag...
            ToolTipActive = false;

            GUILayout.BeginVertical();
            GUI.enabled = true;
            GUILayout.Label("--------------------------------------------------------------", GUILayout.Height(10));
            GUILayout.Label("Deployable Solar Panel Control Center ", GUILayout.Height(10));
            GUILayout.Label("--------------------------------------------------------------", GUILayout.Height(16));
            string step = "start";
            try
            {
                // Display all hatches
                foreach (ModSolarPanel iPanel in SMAddon.smController.SolarPanels)
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
                    bool newOpen = GUILayout.Toggle(open, label, GUILayout.Width(330), GUILayout.Height(40));
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
                Utilities.LogMessage(string.Format(" in Solar Panel Tab at step {0}.  Error:  {1} \r\n\r\n{2}", step, ex.Message, ex.StackTrace), "Error", true);
            }
            GUILayout.EndVertical();
        }

        internal static void ExtendAllPanels()
        {
            // iterate thru the hatch parts and open hatches
            // TODO: for realism, add a delay and a closing/opening sound
            foreach (ModSolarPanel iPanel in SMAddon.smController.SolarPanels)
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
            foreach (ModSolarPanel iPanel in SMAddon.smController.SolarPanels)
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
