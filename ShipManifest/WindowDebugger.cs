using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine;
using ConnectedLivingSpace;

namespace ShipManifest
{
    internal class WindowDebugger
    {
        internal static Rect Position = new Rect(0, 0, 0, 0);
        internal static bool ShowWindow = false;
        internal static string ToolTip = "";
        internal static bool ToolTipActive = false;
        internal static bool ShowToolTips = true;

        internal static void Display(int windowId)
        {
            // Reset Tooltip active flag...
            ToolTipActive = false;

            Rect rect = new Rect(Position.width - 20, 4, 16, 16);
            if (GUI.Button(rect, new GUIContent("", "Close Window")))
            {
                ShowWindow = false;
                ToolTip = "";
            }
            if (Event.current.type == EventType.Repaint && ShowToolTips == true)
                ToolTip = Utilities.SetActiveTooltip(rect, WindowDebugger.Position, GUI.tooltip, ref ToolTipActive, 0, 0);

            GUILayout.BeginVertical();
            Utilities.DebugScrollPosition = GUILayout.BeginScrollView(Utilities.DebugScrollPosition, GUILayout.Height(300), GUILayout.Width(500));
            GUILayout.BeginVertical();

            foreach (string error in Utilities.Errors)
                GUILayout.TextArea(error, GUILayout.Width(460));

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear log", GUILayout.Height(20)))
            {
                Utilities.Errors.Clear();
                Utilities.Errors.Add("Info:  Log Cleared at " + DateTime.UtcNow.ToString() + " UTC.");
            }
            if (GUILayout.Button("Save Log", GUILayout.Height(20)))
            {
                // Create log file and save.
                Savelog();
            }
            if (GUILayout.Button("Close", GUILayout.Height(20)))
            {
                // Create log file and save.
                ShowWindow = false;
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
        }

        internal static void Savelog()
        {
            try
            {
                // time to create a file...
                string filename = "DebugLog_" + DateTime.Now.ToString().Replace(" ", "_").Replace("/", "").Replace(":", "") + ".txt";

                string path = Directory.GetCurrentDirectory() + @"\GameData\ShipManifest\";
                if (SMSettings.DebugLogPath.StartsWith(@"\\"))
                    SMSettings.DebugLogPath = SMSettings.DebugLogPath.Substring(2, SMSettings.DebugLogPath.Length - 2);
                else if (SMSettings.DebugLogPath.StartsWith(@"\"))
                    SMSettings.DebugLogPath = SMSettings.DebugLogPath.Substring(1, SMSettings.DebugLogPath.Length - 1);

                if (!SMSettings.DebugLogPath.EndsWith(@"\"))
                    SMSettings.DebugLogPath += @"\";

                filename = path + SMSettings.DebugLogPath + filename;
                Utilities.LogMessage("File Name = " + filename, "Info", true);

                try
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (string line in Utilities.Errors)
                    {
                        sb.AppendLine(line);
                    }

                    File.WriteAllText(filename, sb.ToString());

                    Utilities.LogMessage("File written", "Info", true);
                }
                catch (Exception ex)
                {
                    Utilities.LogMessage("Error Writing File:  " + ex.ToString(), "Error", true);
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in Savelog.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), "Error", true);
            }
        }

    }
}
