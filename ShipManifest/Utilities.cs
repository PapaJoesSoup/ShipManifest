using KSP.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ShipManifest
{
    public static class Utilities
    {
        public static String AppPath = KSPUtil.ApplicationRootPath.Replace("\\", "/");
        public static String PlugInPath = AppPath + "GameData/ShipManifest/Plugins/PluginData/ShipManifest/";
        public static Vector2 DebugScrollPosition = Vector2.zero;

        private static List<string> _errors = new List<string>();
        public static List<string> Errors
        {
            get { return _errors; }
        }

        public static void LoadTexture(ref Texture2D tex, String FileName)
        {
            LogMessage(String.Format("Loading Texture - file://{0}{1}", PlugInPath, FileName), "Info", Settings.VerboseLogging);
            WWW img1 = new WWW(String.Format("file://{0}{1}", PlugInPath, FileName));
            img1.LoadImageIntoTexture(tex);
        }

        public static void LogMessage(string error, string type, bool verbose)
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

        public static void ShowToolTips()
        {
            if (ShipManifestAddon.toolTip != null && ShipManifestAddon.toolTip.Trim().Length > 0)
            {
                LogMessage(String.Format("ShowToolTips: \r\nToolTip: {0}\r\nToolTipPos:  {1}", ShipManifestAddon.toolTip, ShipManifestAddon.ToolTipPos.ToString()), "Info", true);
                ShowToolTip(ShipManifestAddon.ToolTipPos, ShipManifestAddon.toolTip);
            }

            // Obtain the new value from the last repaint.
            string ToolTip = "";
            if (TransferWindow.ToolTip != null && TransferWindow.ToolTip.Trim().Length > 0)
                ToolTip = TransferWindow.ToolTip;
            if (RosterWindow.ToolTip != null && RosterWindow.ToolTip.Trim().Length > 0)
                ToolTip = RosterWindow.ToolTip;
            if (DebuggerWindow.ToolTip != null && DebuggerWindow.ToolTip.Trim().Length > 0)
                ToolTip = DebuggerWindow.ToolTip;
            if (ManifestWindow.ToolTip != null && ManifestWindow.ToolTip.Trim().Length > 0)
                ToolTip = ManifestWindow.ToolTip;
            if (SettingsWindow.ToolTip != null && SettingsWindow.ToolTip.Trim().Length > 0)
                ToolTip = SettingsWindow.ToolTip;
            if (HatchWindow.ToolTip != null && HatchWindow.ToolTip.Trim().Length > 0)
                ToolTip = HatchWindow.ToolTip;

            // Update stored tooltip.  We do this here so change can be picked up after the current onGUI.  
            // Tooltip will not display if changes are made during the curreint OnGUI.  (Unity issue with callback)
            ShipManifestAddon.toolTip = ToolTip;
        }

        public static void ShowToolTip(Vector2 toolTipPos, string ToolTip)
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

        public static string SetUpToolTip(Rect rect, Rect WindowPosition, string tooltip, float xOffset = 0, float yOffset = 0)
        {
            // this method assumes that we are in a repaint event...
            string ToolTip = "";
            if (rect.Contains(Event.current.mousePosition))
            {
                ShipManifestAddon.ToolTipPos = Event.current.mousePosition;
                ShipManifestAddon.ToolTipPos.x = ShipManifestAddon.ToolTipPos.x + WindowPosition.x + xOffset;
                ShipManifestAddon.ToolTipPos.y = ShipManifestAddon.ToolTipPos.y + WindowPosition.y + yOffset;
                ToolTip = tooltip;
                //Utilities.LogMessage(string.Format("Setup Tooltip - Mouse inside Rect: \r\nRectangle data:  {0} \r\nWindowPosition:  {1}\r\nToolTip:  {2}\r\nToolTip Pos:  {3}", rect.ToString(), WindowPosition.ToString(), ToolTip, ShipManifestAddon.ToolTipPos.ToString()), "Info", true);
            }
            else
                ToolTip = "";

            return ToolTip;
        }
        private static void EmptyWindow(int windowId)
        { }
    }
}
