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
            LogMessage(String.Format("Loading Texture - file://{0}{1}", PlugInPath, FileName), "Info");
            WWW img1 = new WWW(String.Format("file://{0}{1}", PlugInPath, FileName));
            img1.LoadImageIntoTexture(tex);
        }

        public static void LogMessage(string error, string type)
        {
            _errors.Add(type + ": " + error);
        }
    }

    public static class Resources
    {
        public static GUIStyle WindowStyle;
        public static GUIStyle IconStyle;
        public static GUIStyle ButtonToggledStyle;
        public static GUIStyle ButtonToggledRedStyle;
        public static GUIStyle ButtonStyle;
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
                SetStyles();
            }
        }

        public static void SetStyles()
        {
            WindowStyle = new GUIStyle(GUI.skin.window);
            IconStyle = new GUIStyle();

            ButtonToggledStyle = new GUIStyle(GUI.skin.button);
            ButtonToggledStyle.normal.textColor = Color.green;
            ButtonToggledStyle.normal.background = ButtonToggledStyle.onActive.background;
            ButtonToggledStyle.fontSize = 14;
            ButtonToggledStyle.fontStyle = FontStyle.Normal;

            ButtonToggledRedStyle = new GUIStyle(ButtonToggledStyle);
            ButtonToggledRedStyle.normal.textColor = Color.red;
            ButtonToggledRedStyle.fontSize = 14;
            ButtonToggledRedStyle.fontStyle = FontStyle.Normal;

            ButtonStyle = new GUIStyle(GUI.skin.button);
            ButtonStyle.normal.textColor = Color.white;
            ButtonStyle.fontSize = 14;
            ButtonStyle.fontStyle = FontStyle.Normal;

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

    public class SettingsManager
    {
        //public Rect ManifestPosition;
        //public Rect TransferPosition;
        //public Rect RosterPosition;
        
        public Rect ResourceManifestPosition;
        public Rect ResourceTransferPosition;

        public Rect SettingsPosition;

        //public Rect DebuggerPosition;
        public Rect ResourceDebuggerPosition;
        public bool ShowDebugger;

        public void Load()
        {
            ManifestUtilities.LogMessage("Settings load started...", "Info");

            try
            {
                KSP.IO.PluginConfiguration configfile = KSP.IO.PluginConfiguration.CreateForType<ResourceManifestModule>();
                configfile.load();

                ResourceManifestPosition = configfile.GetValue<Rect>("ResourceManifestPosition");
                ResourceTransferPosition = configfile.GetValue<Rect>("ResourceTransferPosition");
                ResourceDebuggerPosition = configfile.GetValue<Rect>("ResourceDebuggerPosition");
                SettingsPosition = configfile.GetValue<Rect>("SettingsPosition");
                ShowDebugger = configfile.GetValue<bool>("ShowDebugger");

                //DebuggerPosition = configfile.GetValue<Rect>("DebuggerPosition");                
                //ManifestPosition = configfile.GetValue<Rect>("ManifestPosition");
                //TransferPosition = configfile.GetValue<Rect>("TransferPosition");
                //RosterPosition = configfile.GetValue<Rect>("RosterPosition");

                ManifestUtilities.LogMessage(string.Format("ResourceManifestPosition Loaded: {0}, {1}, {2}, {3}", ResourceManifestPosition.xMin, ResourceManifestPosition.xMax, ResourceManifestPosition.yMin, ResourceManifestPosition.yMax), "Info");
                ManifestUtilities.LogMessage(string.Format("ResourceTransferPosition Loaded: {0}, {1}, {2}, {3}", ResourceTransferPosition.xMin, ResourceTransferPosition.xMax, ResourceTransferPosition.yMin, ResourceTransferPosition.yMax), "Info");
                ManifestUtilities.LogMessage(string.Format("ResourceDebuggerPosition Loaded: {0}, {1}, {2}, {3}", ResourceDebuggerPosition.xMin, ResourceDebuggerPosition.xMax, ResourceDebuggerPosition.yMin, ResourceDebuggerPosition.yMax), "Info");
                ManifestUtilities.LogMessage(string.Format("SettingsPosition Loaded: {0}, {1}, {2}, {3}", SettingsPosition.xMin, SettingsPosition.xMax, SettingsPosition.yMin, SettingsPosition.yMax), "Info");
                ManifestUtilities.LogMessage(string.Format("ShowDebugger Loaded: {0}", ShowDebugger.ToString()), "Info");

                //ManifestUtilities.LogMessage(string.Format("DebuggerPosition Loaded: {0}, {1}, {2}, {3}", DebuggerPosition.xMin, DebuggerPosition.xMax, DebuggerPosition.yMin, DebuggerPosition.yMax), "Info");
                //ManifestUtilities.LogMessage(string.Format("ManifestPosition Loaded: {0}, {1}, {2}, {3}", ManifestPosition.xMin, ManifestPosition.xMax, ManifestPosition.yMin, ManifestPosition.yMax), "Info");
                //ManifestUtilities.LogMessage(string.Format("TransferPosition Loaded: {0}, {1}, {2}, {3}", TransferPosition.xMin, TransferPosition.xMax, TransferPosition.yMin, TransferPosition.yMax), "Info");
                //ManifestUtilities.LogMessage(string.Format("RosterPosition Loaded: {0}, {1}, {2}, {3}", RosterPosition.xMin, RosterPosition.xMax, RosterPosition.yMin, RosterPosition.yMax), "Info");
            }
            catch(Exception e)
            {
                ManifestUtilities.LogMessage(string.Format("Failed to Load Settings: {0} \r\n\r\n{1}", e.Message, e.StackTrace), "Exception");
            }
        }

        public void Save()
        {
            try
            {
                KSP.IO.PluginConfiguration configfile = KSP.IO.PluginConfiguration.CreateForType<ResourceManifestModule>();

                configfile.SetValue("ResourceManifestPosition", ResourceManifestPosition);
                configfile.SetValue("ResourceTransferPosition", ResourceTransferPosition);
                configfile.SetValue("SettingsPosition", SettingsPosition);
                configfile.SetValue("ResourceDebuggerPosition", ResourceDebuggerPosition);
                configfile.SetValue("ShowDebugger", ShowDebugger);

                //configfile.SetValue("ManifestPosition", ManifestPosition);
                //configfile.SetValue("TransferPosition", TransferPosition);
                //configfile.SetValue("RosterPosition", RosterPosition);
                //configfile.SetValue("DebuggerPosition", DebuggerPosition);

                configfile.save();

                ManifestUtilities.LogMessage(string.Format("ResourceManifestPosition Saved: {0}, {1}, {2}, {3}", ResourceManifestPosition.xMin, ResourceManifestPosition.xMax, ResourceManifestPosition.yMin, ResourceManifestPosition.yMax), "Info");
                ManifestUtilities.LogMessage(string.Format("ResourceTransferPosition Saved: {0}, {1}, {2}, {3}", ResourceTransferPosition.xMin, ResourceTransferPosition.xMax, ResourceTransferPosition.yMin, ResourceTransferPosition.yMax), "Info");
                ManifestUtilities.LogMessage(string.Format("SettingsPosition Saved: {0}, {1}, {2}, {3}", SettingsPosition.xMin, SettingsPosition.xMax, SettingsPosition.yMin, SettingsPosition.yMax), "Info");
                ManifestUtilities.LogMessage(string.Format("ResourceDebuggerPosition Saved: {0}, {1}, {2}, {3}", ResourceDebuggerPosition.xMin, ResourceDebuggerPosition.xMax, ResourceDebuggerPosition.yMin, ResourceDebuggerPosition.yMax), "Info");
                ManifestUtilities.LogMessage(string.Format("ShowDebugger Saved: {0}", ShowDebugger.ToString()), "Info");

                //ManifestUtilities.LogMessage(string.Format("ManifestPosition Saved: {0}, {1}, {2}, {3}", ManifestPosition.xMin, ManifestPosition.xMax, ManifestPosition.yMin, ManifestPosition.yMax), "Info");
                //ManifestUtilities.LogMessage(string.Format("TransferPosition Saved: {0}, {1}, {2}, {3}", TransferPosition.xMin, TransferPosition.xMax, TransferPosition.yMin, TransferPosition.yMax), "Info");
                //ManifestUtilities.LogMessage(string.Format("RosterPosition Saved: {0}, {1}, {2}, {3}", RosterPosition.xMin, RosterPosition.xMax, RosterPosition.yMin, RosterPosition.yMax), "Info");
                //ManifestUtilities.LogMessage(string.Format("DebuggerPosition Saved: {0}, {1}, {2}, {3}", DebuggerPosition.xMin, DebuggerPosition.xMax, DebuggerPosition.yMin, DebuggerPosition.yMax), "Info");
                //ManifestUtilities.LogMessage(string.Format("AllowRespawn Saved: {0}", ShowDebugger.ToString()), "Info");
            }
            catch (Exception e)
            {
                ManifestUtilities.LogMessage(string.Format("Failed to Save Settings: {0} \r\n\r\n{1}", e.Message, e.StackTrace), "Exception");
            }
        }
    }
}
