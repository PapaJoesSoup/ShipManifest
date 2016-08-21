using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Remoting;
using System.Text;
using ShipManifest.InternalObjects;
using UnityEngine;

namespace ShipManifest.Windows
{
  internal class WindowDebugger
  {
    internal static string Title = string.Format(" Ship Manifest -  Debug Console - Ver. {0}", SMSettings.CurVersion);
    internal static Rect Position = new Rect(0, 0, 0, 0);
    internal static bool ShowWindow;
    internal static string ToolTip = "";
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;

    internal static void Display(int windowId)
    {
      // Reset Tooltip active flag...
      ToolTipActive = false;

      Rect rect = new Rect(Position.width - 20, 4, 16, 16);
      if (GUI.Button(rect, new GUIContent("", "Close Window")))
      {
        ShowWindow = false;
        SMSettings.MemStoreTempSettings();
        ToolTip = "";
      }
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      GUILayout.BeginVertical();
      Utilities.DebugScrollPosition = GUILayout.BeginScrollView(Utilities.DebugScrollPosition, SMStyle.ScrollStyle,
        GUILayout.Height(300), GUILayout.Width(500));
      GUILayout.BeginVertical();

      List<string>.Enumerator errors = Utilities.LogItemList.GetEnumerator();
      while (errors.MoveNext())
      {
        if (errors.Current == null) continue;
        GUILayout.TextArea(errors.Current, GUILayout.Width(460));
        
      }

      GUILayout.EndVertical();
      GUILayout.EndScrollView();

      GUILayout.BeginHorizontal();
      if (GUILayout.Button("Clear log", GUILayout.Height(20)))
      {
        Utilities.LogItemList.Clear();
        Utilities.LogItemList.Add("Info:  Log Cleared at " + DateTime.UtcNow.ToString(CultureInfo.InvariantCulture) + " UTC.");
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
        SMSettings.MemStoreTempSettings();
      }
      GUILayout.EndHorizontal();

      GUILayout.EndVertical();
      GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
      SMAddon.RepositionWindow(ref Position);
    }

    internal static void Savelog()
    {
      try
      {
        // time to create a file...
        string filename = "DebugLog_" +
                       DateTime.Now.ToString(CultureInfo.InvariantCulture)
                         .Replace(" ", "_")
                         .Replace("/", "")
                         .Replace(":", "") + ".txt";

        string path = Directory.GetCurrentDirectory() + @"\GameData\ShipManifest\";
        if (SMSettings.DebugLogPath.StartsWith(@"\\"))
          SMSettings.DebugLogPath = SMSettings.DebugLogPath.Substring(2, SMSettings.DebugLogPath.Length - 2);
        else if (SMSettings.DebugLogPath.StartsWith(@"\"))
          SMSettings.DebugLogPath = SMSettings.DebugLogPath.Substring(1, SMSettings.DebugLogPath.Length - 1);

        if (!SMSettings.DebugLogPath.EndsWith(@"\"))
          SMSettings.DebugLogPath += @"\";

        filename = path + SMSettings.DebugLogPath + filename;
        Utilities.LogMessage("File Name = " + filename, Utilities.LogType.Info, true);

        try
        {
          StringBuilder sb = new StringBuilder();
          List<string>.Enumerator lines = Utilities.LogItemList.GetEnumerator();
          while (lines.MoveNext())
          {
            if (lines.Current == null) continue;
            sb.AppendLine(lines.Current);
          }

          File.WriteAllText(filename, sb.ToString());

          Utilities.LogMessage("File written", Utilities.LogType.Info, true);
        }
        catch (Exception ex)
        {
          Utilities.LogMessage("Error Writing File:  " + ex, Utilities.LogType.Error, true);
        }
      }
      catch (Exception ex)
      {
        Utilities.LogMessage(string.Format(" in Savelog.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), Utilities.LogType.Error,
          true);
      }
    }
  }
}