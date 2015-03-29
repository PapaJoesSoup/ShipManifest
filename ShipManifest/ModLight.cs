using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ShipManifest
{
    class ModLight
    {
        private PartModule _lightModule;
        internal PartModule LightModule
        {
            get { return _lightModule; }
            set { _lightModule = value; }
        }

        private Part _spart;
        internal Part SPart
        {
            get { return _spart; }
            set { _spart = value; }
        }

        internal string Title
        {
            get
            {
                string title = "";
                try
                {
                    title = iModule.part.partInfo.title + "\r\n on " + iModule.part.parent.partInfo.title;
                }
                catch
                {
                    title = iModule.part.partInfo.title;
                }
                return title;
            }
        }

        internal bool isOn
        {
            get
            {
                return iModule.isOn;
            }
        }

        internal string Status
        {
            get
            {
                if (iModule.isOn)
                    return "ON";
                else
                    return "OFF";
            }
        }

        private ModuleLight iModule
        {
            get { return (ModuleLight)this.LightModule; }
        }

        internal ModLight() { }
        internal ModLight(PartModule pModule, Part iPart)
        {
            this.LightModule = pModule;
            this.SPart = iPart;
        }

        internal void TurnOnLight()
        {
            iModule.LightsOn();
        }

        internal void TurnOffLight()
        {
            iModule.LightsOff();
        }

        internal void Highlight(Rect rect)
        {
            string step = "begin";
            try
            {
                if (rect.Contains(Event.current.mousePosition))
                {
                    step = "inside box - panel Extended/retracted?";
                    iModule.part.SetHighlightColor(Settings.Colors[Settings.PanelExtendedColor]);
                    step = "highlight on";
                    iModule.part.SetHighlight(true, false);
                }
                else
                {
                    step = "outside box - highlight off";
                    if (iModule.part.highlightColor == Settings.Colors[Settings.PanelExtendedColor] || iModule.part.highlightColor == Settings.Colors[Settings.PanelRetractedColor])
                    {
                        step = "highlight off - turning off highlighting";
                        iModule.part.SetHighlight(false, false);
                        iModule.part.SetHighlightDefault();
                        iModule.part.SetHighlightType(Part.HighlightType.OnMouseOver);
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in Light.Highlight at step {0}.  Error:  {1}", step, ex.ToString()), "Error", true);
            }
        }
    }
}
