using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ConnectedLivingSpace;

namespace ShipManifest
{
    static class TabAntenna
    {
        internal static string ToolTip = "";
        internal static bool ToolTipActive = false;
        internal static bool ShowToolTips = Settings.PanelToolTips;

        private static Vector2 DisplayViewerPosition = Vector2.zero;
        internal static void Display()
        {
            // Reset Tooltip active flag...
            ToolTipActive = false;
            ShowToolTips = Settings.ShowToolTips;

            GUILayout.BeginVertical();
            GUI.enabled = true;
            GUILayout.Label("--------------------------------------------------------------", GUILayout.Height(10));
            GUILayout.Label("Antenna Control Center ", GUILayout.Height(10));
            GUILayout.Label("--------------------------------------------------------------", GUILayout.Height(16));
            string step = "start";
            try
            {
                // Display all hatches
                foreach (ModAntenna iAntenna in SMAddon.smController.Antennas)
                {
                    string label = iAntenna.AntennaStatus + " - " + iAntenna.Title;
                    bool open = iAntenna.Extended;
                    bool newOpen = GUILayout.Toggle(open, label, GUILayout.Width(330), GUILayout.Height(40));
                    step = "button toggle check";
                    if (!open && newOpen)
                        iAntenna.ExtendAntenna();
                    else if (open && !newOpen)
                        iAntenna.RetractAntenna();

                    if (Event.current.type == EventType.Repaint)
                        iAntenna.Highlight(GUILayoutUtility.GetLastRect());
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in Solar Panel Tab at step {0}.  Error:  {1} \r\n\r\n{2}", step, ex.Message, ex.StackTrace), "Error", true);
            }
            GUILayout.EndVertical();
        }

        internal static void ExtendAllAntennas()
        {
            // iterate thru the hatch parts and open hatches
            // TODO: for realism, add a delay and a closing/opening sound
            foreach (ModAntenna iAntenna in SMAddon.smController.Antennas)
            {
                iAntenna.ExtendAntenna();
            }
        }

        internal static void RetractAllAntennas()
        {
            // iterate thru the hatch parts and open hatches
            // TODO: for realism, add a delay and a closing/opening sound
            foreach (ModAntenna iAntenna in SMAddon.smController.Antennas)
            {
                iAntenna.RetractAntenna();
            }
        }

    }
}
