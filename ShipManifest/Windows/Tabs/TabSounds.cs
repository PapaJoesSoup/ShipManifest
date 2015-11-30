using UnityEngine;

namespace ShipManifest.Windows.Tabs
{
  internal static class TabSounds
  {
    internal static string StrFlowCost = "0";

    // GUI tooltip and label support
    private static string _toolTip = "";
    private static Rect _rect;
    private static string _label = "";
    private static GUIContent _guiLabel;

    internal static string ToolTip = "";
    internal static bool ToolTipActive;
    internal static bool ShowToolTips = true;
    private static bool _canShowToolTips = true;
    internal static Rect Position = WindowSettings.Position;

    internal static void Display(Vector2 displayViewerPosition)
    {
      // Reset Tooltip active flag...
      ToolTipActive = false;
      _canShowToolTips = WindowSettings.ShowToolTips && ShowToolTips;

      Position = WindowSettings.Position;
      var scrollX = 20;
      var scrollY = 50;

      GUILayout.Label("Sounds", SMStyle.LabelTabHeader);
      GUILayout.Label("____________________________________________________________________________________________", SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(350));

      GUILayout.Label("Transfer Pump:", GUILayout.Height(20));

      // Pump Start Sound
      GUILayout.BeginHorizontal();
      _label = "Pump Starting: ";
      _toolTip = "Folder location where Pump Starting sound is stored.";
      _toolTip += "\r\nChange to point to your own custom sounds if desired.";
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Label(_guiLabel, GUILayout.Width(100));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, Position, GUI.tooltip, ref ToolTipActive, scrollX, scrollY - displayViewerPosition.y);
      SMSettings.PumpSoundStart = GUILayout.TextField(SMSettings.PumpSoundStart, GUILayout.Width(220));
      GUILayout.EndHorizontal();

      // Pump Run Sound
      GUILayout.BeginHorizontal();
      _label = "Pump Running: ";
      _toolTip = "Folder location where Pump Running sound is stored.";
      _toolTip += "\r\nChange to point to your own custom sounds if desired.";
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Label(_guiLabel, GUILayout.Width(100));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, Position, GUI.tooltip, ref ToolTipActive, scrollX, scrollY - displayViewerPosition.y);
      SMSettings.PumpSoundRun = GUILayout.TextField(SMSettings.PumpSoundRun, GUILayout.Width(220));
      GUILayout.EndHorizontal();

      // Pump Stop Sound
      GUILayout.BeginHorizontal();
      _label = "Pump Stopping: ";
      _toolTip = "Folder location where Pump Stopping sound is stored.";
      _toolTip += "\r\nChange to point to your own custom sounds if desired.";
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Label(_guiLabel, GUILayout.Width(100));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, Position, GUI.tooltip, ref ToolTipActive, scrollX, scrollY - displayViewerPosition.y);
      SMSettings.PumpSoundStop = GUILayout.TextField(SMSettings.PumpSoundStop, GUILayout.Width(220));
      GUILayout.EndHorizontal();

      GUILayout.Label(" ", GUILayout.Height(10));
      GUILayout.Label("Crew:", GUILayout.Height(20));
      // Crew Start Sound
      GUILayout.BeginHorizontal();
      _label = "Crew Exiting: ";
      _toolTip = "Folder location where Crew Exiting their seat sound is stored.";
      _toolTip += "\r\nChange to point to your own custom sounds if desired.";
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Label(_guiLabel, GUILayout.Width(100));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, Position, GUI.tooltip, ref ToolTipActive, scrollX, scrollY - displayViewerPosition.y);
      SMSettings.CrewSoundStart = GUILayout.TextField(SMSettings.CrewSoundStart, GUILayout.Width(220));
      GUILayout.EndHorizontal();

      // Crew Run Sound
      GUILayout.BeginHorizontal();
      _label = "Crew Xfering: ";
      _toolTip = "Folder location where Crew transferring sound is stored.";
      _toolTip += "\r\nChange to point to your own custom sounds if desired.";
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Label(_guiLabel, GUILayout.Width(100));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, Position, GUI.tooltip, ref ToolTipActive, scrollX, scrollY - displayViewerPosition.y);
      SMSettings.CrewSoundRun = GUILayout.TextField(SMSettings.CrewSoundRun, GUILayout.Width(220));
      GUILayout.EndHorizontal();

      // Crew Stop Sound
      GUILayout.BeginHorizontal();
      _label = "Crew Entering: ";
      _toolTip = "Folder location where Crew Entering sound is stored.";
      _toolTip += "\r\nChange to point to your own custom sounds if desired.";
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Label(_guiLabel, GUILayout.Width(100));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, Position, GUI.tooltip, ref ToolTipActive, scrollX, scrollY - displayViewerPosition.y);
      SMSettings.CrewSoundStop = GUILayout.TextField(SMSettings.CrewSoundStop, GUILayout.Width(220));
      GUILayout.EndHorizontal();
    }
  }
}
