﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ShipManifest
{
  internal static class GuiUtils
  {
    internal static string ColorSelector(out string ToolTip)
    {
      string thisColor = "";
      ToolTip = "";

      if (GUILayout.Button(new GUIContent("", "Blue"), SMStyle.ButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
        thisColor = "blue";
      if (Event.current.type == EventType.Repaint)
      {
        Rect rect = GUILayoutUtility.GetLastRect();
        ToolTip = GUI.tooltip;
      }
      if (GUILayout.Button(new GUIContent("", "Black"), SMStyle.ButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
        thisColor = "black";
      if (Event.current.type == EventType.Repaint)
      {
        Rect rect = GUILayoutUtility.GetLastRect();
        ToolTip = GUI.tooltip;
      }
      if (GUILayout.Button(new GUIContent("", "Clear"), SMStyle.ButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
        thisColor = "clear";
      if (Event.current.type == EventType.Repaint)
      {
        Rect rect = GUILayoutUtility.GetLastRect();
        ToolTip = GUI.tooltip;
      }
      if (GUILayout.Button(new GUIContent("", "Cyan"), SMStyle.ButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
        thisColor = "cyan";
      if (Event.current.type == EventType.Repaint)
      {
        Rect rect = GUILayoutUtility.GetLastRect();
        ToolTip = GUI.tooltip;
      }
      if (GUILayout.Button(new GUIContent("", "Gray"), SMStyle.ButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
        thisColor = "gray";
      if (Event.current.type == EventType.Repaint)
      {
        Rect rect = GUILayoutUtility.GetLastRect();
        ToolTip = GUI.tooltip;
      }
      if (GUILayout.Button(new GUIContent("", "Green"), SMStyle.ButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
        thisColor = "green";
      if (Event.current.type == EventType.Repaint)
      {
        Rect rect = GUILayoutUtility.GetLastRect();
        ToolTip = GUI.tooltip;
      }
      if (GUILayout.Button(new GUIContent("", "Magenta"), SMStyle.ButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
        thisColor = "magenta";
      if (Event.current.type == EventType.Repaint)
      {
        Rect rect = GUILayoutUtility.GetLastRect();
        ToolTip = GUI.tooltip;
      }
      if (GUILayout.Button(new GUIContent("", "Red"), SMStyle.ButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
        thisColor = "red";
      if (Event.current.type == EventType.Repaint)
      {
        Rect rect = GUILayoutUtility.GetLastRect();
        ToolTip = GUI.tooltip;
      }
      if (GUILayout.Button(new GUIContent("", "White"), SMStyle.ButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
        thisColor = "white";
      if (Event.current.type == EventType.Repaint)
      {
        Rect rect = GUILayoutUtility.GetLastRect();
        ToolTip = GUI.tooltip;
      }
      if (GUILayout.Button(new GUIContent("", "Yellow"), SMStyle.ButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
        thisColor = "yellow";
      if (Event.current.type == EventType.Repaint)
      {
        Rect rect = GUILayoutUtility.GetLastRect();
        ToolTip = GUI.tooltip;
      }
      return thisColor;
    }

  }
}
