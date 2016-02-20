using ShipManifest.APIClients;
using UnityEngine;

namespace ShipManifest.Windows.Tabs
{
  internal static class TabInstalledMods
  {
    internal static bool ShowAllAssemblies;

    // GUI tooltip and label support
    //private static string _toolTip = "";
    //private static Rect _rect;
    //private static string _label = "";
    //private static GUIContent _guiLabel;

    internal static string ToolTip = "";
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;
    internal static Rect Position = WindowSettings.Position;

    internal static void Display(Vector2 displayViewerPosition)
    {
      // Reset Tooltip active flag...
      ToolTipActive = false;

      Position = WindowSettings.Position;
      //var scrollX = 20;
      //var scrollY = 50;

      GUILayout.BeginHorizontal();
      GUILayout.Label("Installed Mods  ", SMStyle.LabelTabHeader, GUILayout.Width(180));
      ShowAllAssemblies = GUILayout.Toggle(ShowAllAssemblies, "Show All Assemblies", SMStyle.ToggleStyleHeader);
      GUILayout.EndHorizontal();
      GUILayout.Label("____________________________________________________________________________________________",
        SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(350));
      if (ShowAllAssemblies)
        InstalledMods.DisplayAssemblyList();
      else
        InstalledMods.DisplayModList();
    }
  }
}