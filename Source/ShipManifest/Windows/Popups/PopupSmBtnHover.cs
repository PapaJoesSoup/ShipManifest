using ShipManifest.InternalObjects.Settings;
using UnityEngine;

namespace ShipManifest.Windows.Popups
{
  internal static class PopupSmBtnHover
  {
    internal static float TextWidth = 400;
    internal static float WindowWidth = 420;
    internal static float WindowHeight = 200 * CurrSettings.CurrentUIScale;
    internal static Rect Position = new Rect(Screen.width - 462 * CurrSettings.CurrentUIScale, (Screen.height / 2) - 200 * CurrSettings.CurrentUIScale, WindowWidth, WindowHeight);

    internal static bool ShowToolTips = true;

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
    internal static string Title = SmUtils.SmTags["#smloc_manifest_001"];
    internal static string descContent = SmUtils.SmTags["#smloc_popup_smbtn_hover_001"];
    internal static string aboutContent = SmUtils.SmTags["#smloc_popup_smbtn_hover_002"];

    internal static void Display(int windowId)
    {
      // set input locks when mouseover window...
      _inputLocked = GuiUtils.PreventClickthrough(ShowWindow, Position, _inputLocked);

      GUILayout.BeginVertical();
      GUILayout.Label("", SMStyle.SMSkin.label, GUILayout.Height(5 * CurrSettings.CurrentUIScale));

      GUILayout.Label(descContent, SMStyle.LabelStyleCenter, GUILayout.Width(TextWidth * CurrSettings.CurrentUIScale));
      GUILayout.Label("", SMStyle.SMSkin.label, GUILayout.Height(5 * CurrSettings.CurrentUIScale));
      GUILayout.Label(aboutContent, SMStyle.LabelStyleCenter, GUILayout.Width(TextWidth * CurrSettings.CurrentUIScale));

      GUILayout.EndVertical();
      //ResetZoomKeys();
      Position.width = WindowWidth;
      Position.height = WindowHeight;
      GuiUtils.RepositionWindow(ref Position);
    }

    internal static void RefreshUIScale()
    {
      WindowHeight = 200 * CurrSettings.CurrentUIScale;
      Position = new Rect(Screen.width - 462 * CurrSettings.CurrentUIScale, (Screen.height / 2) - 200 * CurrSettings.CurrentUIScale, WindowWidth, WindowHeight);
    }
  }
}
