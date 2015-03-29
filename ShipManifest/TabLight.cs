using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ConnectedLivingSpace;

namespace ShipManifest
{
    static class TabLight
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
            GUILayout.Label("External Light Control Center ", GUILayout.Height(10));
            GUILayout.Label("--------------------------------------------------------------", GUILayout.Height(16));
            string step = "start";
            try
            {
                // Display all Lights
                foreach (ModLight iLight in SMAddon.smController.Lights)
                {
                    string label = iLight.Status + " - " + iLight.Title;
                    bool OnState = iLight.isOn;
                    bool newOnState = GUILayout.Toggle(OnState, label, GUILayout.Width(330), GUILayout.Height(40));
                    step = "button toggle check";
                    if (!OnState && newOnState)
                        iLight.TurnOnLight();
                    else if (OnState && !newOnState)
                        iLight.TurnOffLight();

                    if (Event.current.type == EventType.Repaint)
                        iLight.Highlight(GUILayoutUtility.GetLastRect());
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in Light Tab at step {0}.  Error:  {1} \r\n\r\n{2}", step, ex.Message, ex.StackTrace), "Error", true);
            }
            GUILayout.EndVertical();
        }

        internal static void TurnOnAllLights()
        {
            // iterate thru the hatch parts and open hatches
            // TODO: for realism, add a delay and a closing/opening sound
            foreach (ModLight iLight in SMAddon.smController.Lights)
            {
                iLight.TurnOnLight();
            }
        }

        internal static void TurnOffAllLights()
        {
            // iterate thru the hatch parts and open hatches
            // TODO: for realism, add a delay and a closing/opening sound
            foreach (ModLight iLight in SMAddon.smController.Lights)
            {
                iLight.TurnOffLight();
            }
        }

    }
}
