using KSP.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ShipManifest
{
    internal static class Utilities
    {
        internal static String AppPath = KSPUtil.ApplicationRootPath.Replace("\\", "/");
        internal static String PlugInPath = AppPath + "GameData/ShipManifest/Plugins/PluginData/ShipManifest/";
        internal static Vector2 DebugScrollPosition = Vector2.zero;

        // decimal string handlers for tex box
        internal static bool strHasDecimal;
        internal static bool strHasZero;


        private static List<string> _errors = new List<string>();
        internal static List<string> Errors
        {
            get { return _errors; }
        }

        internal static void LoadTexture(ref Texture2D tex, String FileName)
        {
            LogMessage(String.Format("Loading Texture - file://{0}{1}", PlugInPath, FileName), "Info", Settings.VerboseLogging);
            WWW img1 = new WWW(String.Format("file://{0}{1}", PlugInPath, FileName));
            img1.LoadImageIntoTexture(tex);
        }

        internal static string DisplayVesselResourceTotals(string selectedResource)
        {
            string displayAmount = "";
            double currAmount = 0;
            double totAmount = 0;
            try
            {
                if (selectedResource != "Crew" && selectedResource != "Science")
                {
                    foreach (Part part in SMAddon.smController._partsByResource[selectedResource])
                    {
                        currAmount += part.Resources[selectedResource].amount;
                        totAmount += part.Resources[selectedResource].maxAmount;
                    }
                }
                else if (selectedResource == "Crew")
                {
                    currAmount = (double)SMAddon.smController.Vessel.GetCrewCount();
                    totAmount = (double)SMAddon.smController.Vessel.GetCrewCapacity();
                }
                else if (selectedResource == "Science")
                {
                    foreach (Part part in SMAddon.smController._partsByResource[selectedResource])
                    {
                        foreach (PartModule module in part.Modules)
                        {
                            if (module is IScienceDataContainer)
                            {
                                currAmount += (double)((IScienceDataContainer)module).GetScienceCount();
                            }
                        }
                    }
                }
                if (selectedResource != "Science")
                    displayAmount = string.Format(" - ({0}/{1})", currAmount.ToString("#######0"), totAmount.ToString("######0"));
                else
                    displayAmount = string.Format(" - ({0})", currAmount.ToString("#######0"));
            }
            catch (Exception ex)
            {
                LogMessage(String.Format(" in DisplayResourceTotals().  Error:  {0}", ex.ToString()), "Error", true);
            }

            return displayAmount;
        }

        internal static void LogMessage(string error, string type, bool verbose)
        {
            try
            {
                // Add rolling error list. This limits growth.  Configure with ErrorListLength
                if (_errors.Count() > int.Parse(Settings.ErrorLogLength) && int.Parse(Settings.ErrorLogLength) > 0)
                    _errors.RemoveRange(0, _errors.Count() - int.Parse(Settings.ErrorLogLength));
                if (verbose)
                    _errors.Add(type + ": " + error);
                if (type == "Error" && Settings.AutoDebug)
                    Settings.ShowDebugger = true;
            }
            catch (Exception ex)
            {
                _errors.Add("Error: " + ex.ToString());
                Settings.ShowDebugger = true;
            }
        }

        internal static void ShowToolTips()
        {
            if (SMAddon.toolTip != null && SMAddon.toolTip.Trim().Length > 0)
            {
                //LogMessage(String.Format("ShowToolTips: \r\nToolTip: {0}\r\nToolTipPos:  {1}", SMAddon.toolTip, SMAddon.ToolTipPos.ToString()), "Info", Settings.VerboseLogging);
                ShowToolTip(SMAddon.ToolTipPos, SMAddon.toolTip);
            }

            // Obtain the new value from the last repaint.
            string ToolTip = "";
            if (WindowTransfer.ToolTip != null && WindowTransfer.ToolTip.Trim().Length > 0)
                ToolTip = WindowTransfer.ToolTip;
            if (WindowRoster.ToolTip != null && WindowRoster.ToolTip.Trim().Length > 0)
                ToolTip = WindowRoster.ToolTip;
            if (WindowDebugger.ToolTip != null && WindowDebugger.ToolTip.Trim().Length > 0)
                ToolTip = WindowDebugger.ToolTip;
            if (WindowManifest.ToolTip != null && WindowManifest.ToolTip.Trim().Length > 0)
                ToolTip = WindowManifest.ToolTip;
            if (WindowSettings.ToolTip != null && WindowSettings.ToolTip.Trim().Length > 0)
                ToolTip = WindowSettings.ToolTip;
            if (TabHatch.ToolTip != null && TabHatch.ToolTip.Trim().Length > 0)
                ToolTip = TabHatch.ToolTip;
            if (TabSolarPanel.ToolTip != null && TabSolarPanel.ToolTip.Trim().Length > 0)
                ToolTip = TabSolarPanel.ToolTip;

            // Update stored tooltip.  We do this here so change can be picked up after the current onGUI.  
            // Tooltip will not display if changes are made during the curreint OnGUI.  (Unity issue with onGUI callback functions)
            SMAddon.toolTip = ToolTip;
        }

        internal static void ShowToolTip(Vector2 toolTipPos, string ToolTip)
        {
            if (Settings.ShowToolTips && (ToolTip != null) && (ToolTip.Trim().Length > 0))
            {
                Vector2 size = ManifestStyle.ToolTipStyle.CalcSize(new GUIContent(ToolTip));
                Rect rect = new Rect(toolTipPos.x + 20, toolTipPos.y - 4, size.x, size.y);
                GUI.Window(0, rect, EmptyWindow, ToolTip, ManifestStyle.ToolTipStyle);
                GUI.BringWindowToFront(0);
                //Utilities.LogMessage(string.Format("ShowToolTip: \r\nRectangle data:  {0} \r\nToolTip:  {1}\r\nToolTipPos:  {2}", rect.ToString(), ToolTip, toolTipPos.ToString()), "Info", true);
            }
        }

        internal static string SetActiveTooltip(Rect rect, Rect WindowPosition, string toolTip, ref bool toolTipActive, float xOffset, float yOffset)
        {

            if (!toolTipActive && rect.Contains(Event.current.mousePosition))
            {
                toolTipActive = true;
                // Since we are using GUILayout, the curent mouse position returns a position with reference to the source Details viewer. 
                // Add the height of GUI elements already drawn to y offset to get the correct screen position
                if (rect.Contains(Event.current.mousePosition))
                {
                    SMAddon.ToolTipPos = Event.current.mousePosition;
                    SMAddon.ToolTipPos.x = SMAddon.ToolTipPos.x + WindowPosition.x + xOffset;
                    SMAddon.ToolTipPos.y = SMAddon.ToolTipPos.y + WindowPosition.y + yOffset;
                    //Utilities.LogMessage(string.Format("Setup Tooltip - Mouse inside Rect: \r\nRectangle data:  {0} \r\nWindowPosition:  {1}\r\nToolTip:  {2}\r\nToolTip Pos:  {3}", rect.ToString(), WindowPosition.ToString(), ToolTip, ShipManifestAddon.ToolTipPos.ToString()), "Info", true);
                }
                else
                    toolTip = "";
            }
            // We are in a loop so we don't need the return value from SetUpToolTip.  We will assign it instead.
            if (!toolTipActive)
                toolTip = "";
            return toolTip;
        }

        private static void EmptyWindow(int windowId)
        { }

        internal static string GetStringDecimal(string strValue)
        {
            if (strHasDecimal)
                strValue += ".";
            return strValue;
        }

        internal static string GetStringZero(string strValue)
        {
            if (strHasZero)
                strValue += "0";
            return strValue;
        }

        internal static void SetStringZero(string strValue)
        {
            if (strValue.Contains(".") && strValue.EndsWith("0"))
                strHasZero = true;
            else
                strHasZero = false;
        }

        internal static void SetStringDecimal(string strValue)
        {
            if (strValue.EndsWith(".") || strValue.EndsWith(".0"))
                strHasDecimal = true;
            else
                strHasDecimal = false;
        }

    }
}
