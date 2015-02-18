using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ConnectedLivingSpace;

namespace ShipManifest
{
    static class WindowHatch
    {
        internal static string ToolTip = "";
        internal static bool ToolTipActive = false;
        internal static bool ShowToolTips = Settings.HatchToolTips;

        private static Vector2 DisplayViewerPosition = Vector2.zero;
        internal static void Display(int windowId)
        {
            // Reset Tooltip active flag...
            ToolTipActive = false;
            ShowToolTips = Settings.HatchToolTips;

            Rect rect = new Rect(366, 4, 16, 16);
            if (GUI.Button(rect, new GUIContent("", "Close Window")))
            {
                Settings.ShowHatch = false;
                ToolTip = "";
            }
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, Settings.HatchPosition, GUI.tooltip, ref ToolTipActive, 0, 0);

            // This is a scroll panel (we are using it to make button lists...)
            GUILayout.BeginVertical();
            DisplayViewerPosition = GUILayout.BeginScrollView(DisplayViewerPosition, GUILayout.Height(200), GUILayout.Width(370));
            GUILayout.BeginVertical();
            GUI.enabled = true;
            GUILayout.Label("--------------------------------------------------------------", GUILayout.Height(10));
            GUILayout.Label("Hatch Control Panel ", GUILayout.Height(10));
            GUILayout.Label("--------------------------------------------------------------", GUILayout.Height(16));
            string step = "start";
            try
            {
                // Display all hatches
                foreach (Hatch iHatch in SMAddon.Hatches)
                {
                    bool isEnabled = true;
                    bool open = false;

                    // get hatch state
                    if (!iHatch.IsDocked)
                        isEnabled = false;
                    if (iHatch.HatchOpen)
                        open = true;

                    step = "gui enable";
                    GUI.enabled = isEnabled;
                    bool newOpen = GUILayout.Toggle(open, iHatch.HatchStatus + " - " + iHatch.Title, GUILayout.Width(330));
                    step = "button toggle check";
                    if (!open && newOpen)
                    {
                        iHatch.OpenHatch();
                    }
                    else if (open && !newOpen)
                    {
                        iHatch.CloseHatch();
                    }
                    if (Event.current.type == EventType.Repaint)
                        iHatch.Highlight(GUILayoutUtility.GetLastRect());
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in Hatches Window at step {0}.  Error:  {1} \r\n\r\n{2}", step, ex.Message, ex.StackTrace), "Error", true);
            }
            GUI.enabled = true;
            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Close All Hatches", GUILayout.Height(20)))
                CloseAllHatches();

            if (GUILayout.Button("Open All Hatches", GUILayout.Height(20)))
                OpenAllHatches();

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
        }

        internal static void OpenAllHatches()
        {
            // iterate thru the hatch parts and open hatches
            // TODO: for realism, add a delay and a closing/opening sound
            foreach (Hatch iHatch in SMAddon.Hatches)
            {
                IModuleDockingHatch iModule = (IModuleDockingHatch)iHatch.HatchModule;
                if (iModule.IsDocked)
                {
                    iModule.HatchEvents["CloseHatch"].active = true;
                    iModule.HatchEvents["OpenHatch"].active = false;
                    iModule.HatchOpen = true;
                }
            }
            //SMAddon.FireEventTriggers();
        }

        internal static void CloseAllHatches()
        {
            // iterate thru the hatch parts and open hatches
            // TODO: for realism, add a delay and a closing/opening sound
            foreach (Hatch iHatch in SMAddon.Hatches)
            {
                IModuleDockingHatch iModule = (IModuleDockingHatch)iHatch.HatchModule;
                if (iModule.IsDocked)
                {
                    iModule.HatchEvents["CloseHatch"].active = false;
                    iModule.HatchEvents["OpenHatch"].active = true;
                    iModule.HatchOpen = false;
                }
            }
            //SMAddon.FireEventTriggers();
        }

        internal static void HighlightAllHatches(bool enable)
        {
            foreach (Hatch iHatch in SMAddon.Hatches)
            {
                IModuleDockingHatch iModule = (IModuleDockingHatch)iHatch.HatchModule;
                if (enable)
                {
                    if (iModule.HatchOpen)
                        iModule.ModDockNode.part.SetHighlightColor(Settings.Colors[Settings.HatchOpenColor]);
                    else
                        iModule.ModDockNode.part.SetHighlightColor(Settings.Colors[Settings.HatchCloseColor]);
                    iModule.ModDockNode.part.SetHighlight(true, false);
                }
                else
                {
                    if (Settings.EnableCLS && SMAddon.smController.SelectedResource == "Crew" && Settings.ShowTransferWindow)
                    {
                        iHatch.CLSPart.Highlight(true, true);
                    }
                    else
                    {
                        iModule.ModDockNode.part.SetHighlight(false, false);
                        iModule.ModDockNode.part.SetHighlightDefault();
                        iModule.ModDockNode.part.SetHighlightType(Part.HighlightType.OnMouseOver);
                    }
                }
            }
        }
    }
}
