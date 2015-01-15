using KSP.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ShipManifest
{
    public static class ManifestUtilities
    {
        public static String AppPath = KSPUtil.ApplicationRootPath.Replace("\\", "/");
        public static String PlugInPath = AppPath + "GameData/ShipManifest/Plugins/PluginData/ShipManifest/";
        public static Vector2 DebugScrollPosition = Vector2.zero;

        private static List<string> _errors = new List<string>();
        public static List<string> Errors
        {
            get { return _errors; }
        }

        public static void LoadTexture(ref Texture2D tex, String FileName)
        {
            LogMessage(String.Format("Loading Texture - file://{0}{1}", PlugInPath, FileName), "Info", Settings.VerboseLogging);
            WWW img1 = new WWW(String.Format("file://{0}{1}", PlugInPath, FileName));
            img1.LoadImageIntoTexture(tex);
        }

        public static void LogMessage(string error, string type, bool verbose)
        {
            try
            {
                // Add rolling error list. This limits growth.  Configure with ErrorListLength
                if (_errors.Count() > int.Parse(Settings.ErrorLogLength) && int.Parse(Settings.ErrorLogLength) > 0)
                    _errors.RemoveRange(0, _errors.Count() - int.Parse(Settings.ErrorLogLength));
                if (verbose)
                    _errors.Add(type + ": " + error);
                if (type == "Error" && Settings.AutoDebug)
                    Settings.ShowDebugger = true;
            }
            catch (Exception ex)
            {
                _errors.Add("Error: " + ex.ToString());
                Settings.ShowDebugger = true;
            }
        }
    }

    public static class ManifestStyle
    {
        public static GUIStyle WindowStyle;
        public static GUIStyle IconStyle;
        public static GUIStyle ButtonSourceStyle;
        public static GUIStyle ButtonTargetStyle;
        public static GUIStyle ButtonToggledSourceStyle;
        public static GUIStyle ButtonToggledTargetStyle;
        public static GUIStyle ButtonStyle;
        public static GUIStyle ButtonToggledStyle;
        public static GUIStyle ErrorLabelRedStyle;
        public static GUIStyle LabelStyle;
        public static GUIStyle LabelStyleRed;
        public static GUIStyle LabelStyleYellow;
        public static GUIStyle LabelStyleGreen;

        public static void SetupGUI()
        {
            GUI.skin = HighLogic.Skin;
            if (WindowStyle == null)
            {
                Settings.LoadColors();
                SetStyles();
            }
        }

        public static void SetStyles()
        {
            WindowStyle = new GUIStyle(GUI.skin.window);
            IconStyle = new GUIStyle();

            ButtonStyle = new GUIStyle(GUI.skin.button);
            ButtonStyle.normal.textColor = Color.white;
            ButtonStyle.hover.textColor = Color.blue;
            ButtonStyle.fontSize = 14;
            ButtonStyle.fontStyle = FontStyle.Normal;

            ButtonToggledStyle = new GUIStyle(GUI.skin.button);
            ButtonToggledStyle.normal.textColor = Color.green;
            ButtonToggledStyle.fontSize = 14;
            ButtonToggledStyle.hover.textColor = Color.blue;
            ButtonToggledStyle.normal.background = ButtonToggledStyle.onActive.background;
            ButtonToggledStyle.fontStyle = FontStyle.Normal;

            ButtonSourceStyle = new GUIStyle(GUI.skin.button);
            ButtonSourceStyle.normal.textColor = Color.white;
            ButtonSourceStyle.fontSize = 14;
            ButtonSourceStyle.hover.textColor = Color.blue;
            ButtonSourceStyle.fontStyle = FontStyle.Normal;
            ButtonSourceStyle.alignment = TextAnchor.UpperLeft;

            ButtonToggledSourceStyle = new GUIStyle(GUI.skin.button);
            ButtonToggledSourceStyle.normal.textColor = Settings.Colors[Settings.SourcePartColor];
            ButtonToggledSourceStyle.fontSize = 14;
            ButtonToggledSourceStyle.hover.textColor = Color.blue;
            ButtonToggledSourceStyle.normal.background = ButtonToggledSourceStyle.onActive.background;
            ButtonToggledSourceStyle.fontStyle = FontStyle.Normal;
            ButtonToggledSourceStyle.alignment = TextAnchor.UpperLeft;

            ButtonTargetStyle = new GUIStyle(GUI.skin.button);
            ButtonTargetStyle.normal.textColor = Color.white;
            ButtonTargetStyle.fontSize = 14;
            ButtonTargetStyle.hover.textColor = Color.blue;
            ButtonTargetStyle.fontStyle = FontStyle.Normal;
            ButtonTargetStyle.alignment = TextAnchor.UpperLeft;

            ButtonToggledTargetStyle = new GUIStyle(GUI.skin.button);
            ButtonToggledTargetStyle.normal.textColor = Settings.Colors[Settings.TargetPartColor];
            ButtonToggledTargetStyle.fontSize = 14;
            ButtonToggledTargetStyle.hover.textColor = Color.blue;
            ButtonToggledTargetStyle.normal.background = ButtonToggledSourceStyle.onActive.background;
            ButtonToggledTargetStyle.fontStyle = FontStyle.Normal;
            ButtonToggledTargetStyle.alignment = TextAnchor.UpperLeft;

            ErrorLabelRedStyle = new GUIStyle(GUI.skin.label);
            ErrorLabelRedStyle.normal.textColor = Color.red;

            LabelStyle = new GUIStyle(GUI.skin.label);

            LabelStyleRed = new GUIStyle(LabelStyle);
            LabelStyleRed.normal.textColor = Color.red;

            LabelStyleYellow = new GUIStyle(LabelStyle);
            LabelStyleYellow.normal.textColor = Color.yellow;

            LabelStyleGreen = new GUIStyle(LabelStyle);
            LabelStyleGreen.normal.textColor = Color.green;
        }
    }
}
