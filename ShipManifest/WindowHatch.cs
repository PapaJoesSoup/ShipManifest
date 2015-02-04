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
        public static string ToolTip = "";
        public static bool ToolTipActive = false;

        private static List<Hatch> _hatches = new List<Hatch>();
        public static List<Hatch> Hatches
        {
            get
            {
                if (_hatches == null)
                    _hatches = new List<Hatch>();
                return _hatches;
            }
            set
            {
                _hatches.Clear();
                _hatches = value;
            }
        }

        private static void GetHatches()
        {
            _hatches.Clear();
            try
            {
                SMAddon.UpdateCLSSpaces();
                foreach (ICLSSpace iSpace in SMAddon.clsVessel.Spaces)
                {
                    foreach (ICLSPart iPart in iSpace.Parts)
                    {
                        foreach (PartModule pModule in iPart.Part.Modules)
                        {
                            if (pModule.moduleName == "ModuleDockingHatch")
                            {
                                Hatch pHatch = new Hatch();
                                pHatch.HatchModule = pModule;
                                pHatch.CLSPart = iPart;
                                _hatches.Add(pHatch);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format("Error in GetHatches().\r\nError:  {0}", ex.ToString()), "Error", true);
            }
        }

        private static Vector2 DisplayViewer = Vector2.zero;
        public static void Display(int windowId)
        {
            // Reset Tooltip active flag...
            ToolTipActive = false;

            Rect rect = new Rect(366, 4, 16, 16);
            if (GUI.Button(rect, new GUIContent("", "Close Window")))
            {
                Settings.ShowHatch = false;
                ToolTip = "";
            }
            if (Event.current.type == EventType.Repaint)
                ToolTip = Utilities.SetActiveTooltip(rect, Settings.HatchPosition, GUI.tooltip, ref ToolTipActive, 0, 0);

            GetHatches();

            // This is a scroll panel (we are using it to make button lists...)
            GUILayout.BeginVertical();
            DisplayViewer = GUILayout.BeginScrollView(DisplayViewer, GUILayout.Height(200), GUILayout.Width(370));
            GUILayout.BeginVertical();
            GUI.enabled = true;
            GUILayout.Label("--------------------------------------------------------------", GUILayout.Height(10));
            GUILayout.Label("Hatch Control Panel ", GUILayout.Height(10));
            GUILayout.Label("--------------------------------------------------------------", GUILayout.Height(16));
            string step = "start";
            try
            {
                // Display all hatches
                foreach (Hatch iHatch in Hatches)
                {
                    bool isEnabled = false;
                    bool open = false;

                    // get hatch state
                    if (iHatch.IsDocked)
                        isEnabled = true;
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
                        iHatch.Highlight();
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

        public static void OpenAllHatches()
        {
            // iterate thru the hatch parts and open hatches
            // TODO: for realism, add a delay and a closing/opening sound
            foreach (Hatch iHatch in Hatches)
            {
                IModuleDockingHatch iModule = (IModuleDockingHatch)iHatch.HatchModule;
                if (iModule.IsDocked)
                {
                    iModule.HatchEvents["CloseHatch"].active = true;
                    iModule.HatchEvents["OpenHatch"].active = false;
                    iModule.HatchOpen = true;
                }
            }
            SMAddon.FireEventTriggers();
        }

        public static void CloseAllHatches()
        {
            // iterate thru the hatch parts and open hatches
            // TODO: for realism, add a delay and a closing/opening sound
            foreach (Hatch iHatch in Hatches)
            {
                IModuleDockingHatch iModule = (IModuleDockingHatch)iHatch.HatchModule;
                if (iModule.IsDocked)
                {
                    iModule.HatchEvents["CloseHatch"].active = false;
                    iModule.HatchEvents["OpenHatch"].active = true;
                    iModule.HatchOpen = false;
                }
            }
            SMAddon.FireEventTriggers();
        }

    }
}
