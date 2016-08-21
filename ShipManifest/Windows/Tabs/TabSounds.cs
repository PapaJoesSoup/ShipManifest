using ShipManifest.InternalObjects;
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
      int scrollX = 20;

      GUILayout.Label("Sounds", SMStyle.LabelTabHeader);
      GUILayout.Label("____________________________________________________________________________________________",
        SMStyle.LabelStyleHardRule, GUILayout.Height(10), GUILayout.Width(350));

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
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
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
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
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
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      SMSettings.PumpSoundStop = GUILayout.TextField(SMSettings.PumpSoundStop, GUILayout.Width(220));
      GUILayout.EndHorizontal();

      // Pump Sound Volume
      GUILayout.BeginHorizontal();
      _label = "Pump Volume: ";
      _toolTip = "How loud do you want it?";
      _toolTip += "\r\nMove slider to change volume.";
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Label(_guiLabel, GUILayout.Width(100));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // Volume Slider Control
      GUILayout.Label("Min", GUILayout.Width(40),GUILayout.Height(20));
      SMSettings.PumpSoundVol = GUILayout.HorizontalSlider((float)SMSettings.PumpSoundVol, 0f, 1f, GUILayout.Width(140), GUILayout.Height(20));
      _label = "Max";
      _toolTip = "Slide control to change the volume above.";
      GUILayout.Label(new GUIContent(_label, _toolTip), GUILayout.Width(40), GUILayout.Height(20));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
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
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
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
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
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
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      SMSettings.CrewSoundStop = GUILayout.TextField(SMSettings.CrewSoundStop, GUILayout.Width(220));
      GUILayout.EndHorizontal();

      // Crew Sound Volume
      GUILayout.BeginHorizontal();
      _label = "Crew Volume: ";
      _toolTip = "How loud do you want it?";
      _toolTip += "\r\nMove slider to change volume.";
      _guiLabel = new GUIContent(_label, _toolTip);
      GUILayout.Label(_guiLabel, GUILayout.Width(100));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);

      // Volume Slider Control
      GUILayout.Label("Min", GUILayout.Width(40), GUILayout.Height(20));
      SMSettings.CrewSoundVol = GUILayout.HorizontalSlider((float)SMSettings.CrewSoundVol, 0f, 1f, GUILayout.Width(140), GUILayout.Height(20));
      _label = "Max";
      _toolTip = "Slide control to change the volume above.";
      GUILayout.Label(new GUIContent(_label, _toolTip), GUILayout.Width(40), GUILayout.Height(20));
      _rect = GUILayoutUtility.GetLastRect();
      if (Event.current.type == EventType.Repaint && _canShowToolTips)
        ToolTip = SMToolTips.SetActiveToolTip(_rect, GUI.tooltip, ref ToolTipActive, scrollX);
      GUILayout.EndHorizontal();
    }
  }
}