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
    internal static string Title = $" Ship Manifest -  Debug Console - Ver. {SMSettings.CurVersion}";
    internal static Rect Position = new Rect(0, 0, 0, 0);
    internal static bool ShowWindow;
    internal static string ToolTip = "";
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;

    internal static void Display(int windowId)
    {
      Title = $"{SmUtils.SmTags["#smloc_debug_000"]}:  {SMSettings.CurVersion}";

      // set input locks when mouseover window...
      //_inputLocked = GuiUtils.PreventClickthrough(ShowWindow, Position, _inputLocked);

      // Reset Tooltip active flag...
      ToolTipActive = false;

      Rect rect = new Rect(Position.width - 20, 4, 16, 16);
      if (GUI.Button(rect, new GUIContent("", SmUtils.SmTags["#smloc_window_tt_001"]))) // "Close Window"
      {
        ShowWindow = false;
        SMSettings.MemStoreTempSettings();
        ToolTip = "";
      }
      if (Event.current.type == EventType.Repaint && ShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(rect, GUI.tooltip, ref ToolTipActive, 10);

      GUILayout.BeginVertical();
      SmUtils.DebugScrollPosition = GUILayout.BeginScrollView(SmUtils.DebugScrollPosition, SMStyle.ScrollStyle,
        GUILayout.Height(300), GUILayout.Width(500));
      GUILayout.BeginVertical();

      List<string>.Enumerator errors = SmUtils.LogItemList.GetEnumerator();
      while (errors.MoveNext())
      {
        if (errors.Current == null) continue;
        GUILayout.TextArea(errors.Current, GUILayout.Width(460));
        
      }
      errors.Dispose();

      GUILayout.EndVertical();
      GUILayout.EndScrollView();

      GUILayout.BeginHorizontal();
      if (GUILayout.Button(SmUtils.SmTags["#smloc_debug_001"], GUILayout.Height(20))) //"Clear log"
      {
        SmUtils.LogItemList.Clear();
        SmUtils.LogItemList.Add($"Info:  Log Cleared at {DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)} UTC.");
      }
      if (GUILayout.Button(SmUtils.SmTags["#smloc_debug_002"], GUILayout.Height(20))) // "Save Log"
      {
        // Create log file and save.
        Savelog();
      }
      if (GUILayout.Button(SmUtils.SmTags["#smloc_debug_003"], GUILayout.Height(20))) // "Close"
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
        string filename = $"DebugLog_{DateTime.Now.ToString(CultureInfo.InvariantCulture).Replace(" ", "_").Replace("/", "").Replace(":", "")}.txt";

        string path = Directory.GetCurrentDirectory() + @"\GameData\ShipManifest\";
        if (SMSettings.DebugLogPath.StartsWith(@"\\"))
          SMSettings.DebugLogPath = SMSettings.DebugLogPath.Substring(2, SMSettings.DebugLogPath.Length - 2);
        else if (SMSettings.DebugLogPath.StartsWith(@"\"))
          SMSettings.DebugLogPath = SMSettings.DebugLogPath.Substring(1, SMSettings.DebugLogPath.Length - 1);

        if (!SMSettings.DebugLogPath.EndsWith(@"\"))
          SMSettings.DebugLogPath += @"\";

        filename = path + SMSettings.DebugLogPath + filename;
        SmUtils.LogMessage($"File Name = {filename}", SmUtils.LogType.Info, true);

        try
        {
          StringBuilder sb = new StringBuilder();
          List<string>.Enumerator lines = SmUtils.LogItemList.GetEnumerator();
          while (lines.MoveNext())
          {
            if (lines.Current == null) continue;
            sb.AppendLine(lines.Current);
          }
          lines.Dispose();

          File.WriteAllText(filename, sb.ToString());

          SmUtils.LogMessage("File written", SmUtils.LogType.Info, true);
        }
        catch (Exception ex)
        {
          SmUtils.LogMessage($"Error Writing File:  {ex}", SmUtils.LogType.Error, true);
        }
      }
      catch (Exception ex)
      {
        SmUtils.LogMessage($" in Savelog.  Error:  {ex.Message} \r\n\r\n{ex.StackTrace}", SmUtils.LogType.Error,
          true);
      }
    }
  }
}