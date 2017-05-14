using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
      Title = string.Format("{0}:  {1}", SMUtils.Localize("#smloc_debug_000"), SMSettings.CurVersion);

      // set input locks when mouseover window...
      //_inputLocked = GuiUtils.PreventClickthrough(ShowWindow, Position, _inputLocked);

      // Reset Tooltip active flag...
      ToolTipActive = false;

      Rect rect = new Rect(Position.width - 20, 4, 16, 16);
      if (GUI.Button(rect, new GUIContent("", SMUtils.Localize("#smloc_window_tt_001")))) // "Close Window"
      {
        ShowWindow = false;
        SMSettings.MemStoreTempSettings();
        ToolTip = "";
      }
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      GUILayout.BeginVertical();
      SMUtils.DebugScrollPosition = GUILayout.BeginScrollView(SMUtils.DebugScrollPosition, SMStyle.ScrollStyle,
        GUILayout.Height(300), GUILayout.Width(500));
      GUILayout.BeginVertical();

      List<string>.Enumerator errors = SMUtils.LogItemList.GetEnumerator();
      while (errors.MoveNext())
      {
        if (errors.Current == null) continue;
        GUILayout.TextArea(errors.Current, GUILayout.Width(460));
        
      }
      errors.Dispose();

      GUILayout.EndVertical();
      GUILayout.EndScrollView();

      GUILayout.BeginHorizontal();
      if (GUILayout.Button(SMUtils.Localize("#smloc_debug_001"), GUILayout.Height(20))) //"Clear log"
      {
        SMUtils.LogItemList.Clear();
        SMUtils.LogItemList.Add("Info:  Log Cleared at " + DateTime.UtcNow.ToString(CultureInfo.InvariantCulture) + " UTC.");
      }
      if (GUILayout.Button(SMUtils.Localize("#smloc_debug_002"), GUILayout.Height(20))) // "Save Log"
      {
        // Create log file and save.
        Savelog();
      }
      if (GUILayout.Button(SMUtils.Localize("#smloc_debug_003"), GUILayout.Height(20))) // "Close"
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
        SMUtils.LogMessage("File Name = " + filename, SMUtils.LogType.Info, true);

        try
        {
          StringBuilder sb = new StringBuilder();
          List<string>.Enumerator lines = SMUtils.LogItemList.GetEnumerator();
          while (lines.MoveNext())
          {
            if (lines.Current == null) continue;
            sb.AppendLine(lines.Current);
          }
          lines.Dispose();

          File.WriteAllText(filename, sb.ToString());

          SMUtils.LogMessage("File written", SMUtils.LogType.Info, true);
        }
        catch (Exception ex)
        {
          SMUtils.LogMessage("Error Writing File:  " + ex, SMUtils.LogType.Error, true);
        }
      }
      catch (Exception ex)
      {
        SMUtils.LogMessage(string.Format(" in Savelog.  Error:  {0} \r\n\r\n{1}", ex.Message, ex.StackTrace), SMUtils.LogType.Error,
          true);
      }
    }
  }
}