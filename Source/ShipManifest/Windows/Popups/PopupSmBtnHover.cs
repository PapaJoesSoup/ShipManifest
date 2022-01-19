using ShipManifest.InternalObjects.Settings;
using UnityEngine;

namespace ShipManifest.Windows.Popups
{
  internal static class PopupSmBtnHover
  {
    internal static float WindowHeight = 200;
    internal static float WindowWidth = 420;
    internal static float TextWidth = 400;
    internal static Rect Position = new Rect(Screen.width - 460, (Screen.height / 2) - 200, WindowWidth, WindowHeight);

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
    internal static string Title = "Ship Manifest";
    internal static string descContent = "A tool to manage your ship's 'things'";
    internal static string aboutContent =
      "This mod provides many quality of life features for managing your vessels, crew and resources.\n\nLeft Click to toggle the Manifest window, where you can select what you wish to manage.\n\nRight Click to toggle the Transfer window for quick access to transferring resources and crew around your vessel(s)";

    internal static void Display(int _windowId)
    {


      // set input locks when mouseover window...
      _inputLocked = GuiUtils.PreventClickthrough(ShowWindow, Position, _inputLocked);

      GUILayout.BeginVertical();
      GUILayout.Label("", GUILayout.Height(5));

      GUILayout.Label(descContent, SMStyle.LabelStyleCenter, GUILayout.Width(TextWidth));
      GUILayout.Label("", GUILayout.Height(5));
      GUILayout.Label(aboutContent, SMStyle.LabelStyleCenter, GUILayout.Width(TextWidth));

      GUILayout.EndVertical();
      //ResetZoomKeys();
      Position.width = WindowWidth;
      Position.height = WindowHeight;
      GuiUtils.RepositionWindow(ref Position);
    }
  }
}
