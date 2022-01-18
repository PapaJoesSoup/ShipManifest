using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using ShipManifest.InternalObjects;
using ShipManifest.InternalObjects.Settings;
using ShipManifest.Process;
using UnityEngine;

namespace ShipManifest.Windows.Popups
{
  internal static class PopupCloseTransfer
  {
    internal static float WindowHeight = 185;
    internal static float WindowWidth = 420;
    internal static float TextWidth = 400;
    internal static Rect Position = CurrSettings.DefaultPosition;
    private static bool _inputLocked;
    private static bool _showWindow;
    internal static bool ShowWindow
    {
      get => _showWindow;
      set
      {
        if (!value)
        {
          InputLockManager.RemoveControlLock("SM_Window");
          _inputLocked = false;
        }
        _showWindow = value;
      }
    }

    // Content strings
    internal static string Title              = "Confirm Close Transfer Window";
    internal static GUIContent descContent    = new GUIContent(SmUtils.SmTags["#smloc_popup_transfer_001"]);
    internal static GUIContent closeContent   = new GUIContent(SmUtils.SmTags["#smloc_debug_003"]); // = Close
    internal static GUIContent cancelContent  = new GUIContent(SmUtils.SmTags["#smloc_settings_003"]); // = "Cancel"


    internal static void Display(int _windowId)
    {


      // set input locks when mouseover window...
      _inputLocked = GuiUtils.PreventClickthrough(ShowWindow, Position, _inputLocked);

      GUILayout.BeginVertical();
      GUILayout.Label("", GUILayout.Height(20));
      GUILayout.Label(descContent, SMStyle.LabelStyleCenter, GUILayout.Width(TextWidth));

      GUILayout.Label("", GUILayout.Height(30));
      GUILayout.BeginHorizontal();
      if (GUILayout.Button(closeContent, GUILayout.Height(20))) // "Close"
      {
        // Abort all Transfers and quit
        TransferPump.AbortAllPumpsInProcess(0);
        TransferPump.Paused = false;
        WindowTransfer.BtnCloseWindow();
        return;
      }
      if (GUILayout.Button(cancelContent, GUILayout.Height(20))) //"Cancel"
      {
        TransferPump.Paused = false;
        SMSound.SourcePumpRun.Play();
        ShowWindow = false;
      }
      GUILayout.EndHorizontal();
      GUILayout.EndVertical();
      //ResetZoomKeys();
      GUI.DragWindow(new Rect(0, 0, Screen.width, 30));
      Position.width = WindowWidth;
      Position.height = WindowHeight;
      GuiUtils.RepositionWindow(ref Position);
    }
  }
}
