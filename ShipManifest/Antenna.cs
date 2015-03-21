using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ShipManifest
{
    class Antenna
    {
        private PartModule _antennaModule;
        internal PartModule AntennaModule
        {
            get { return _antennaModule; }
            set { _antennaModule = value; }
        }

        private Part _spart;
        internal Part SPart
        {
            get { return _spart; }
            set { _spart = value; }
        }

        internal bool Extended
        {
            get
            {
                if (iModule.Events["Toggle"].guiName == "Retract")
                    return true;
                else
                    return false;
            }
        }

        internal string AntennaStatus
        {
            get
            {
                if (iModule.Events["Toggle"].guiName == "Extend")
                    return "Retracted";
                else
                    return "Extended";
            }
        }
        internal string Title
        {
            get
            {
                string title = "";
                try
                {
                    title = iModule.part.parent.partInfo.title;
                }
                catch
                {
                    title = "Unknown";
                }
                return title;
            }
        }

        private ModuleAnimateGeneric iModule
        {
            get { return (ModuleAnimateGeneric)this.AntennaModule; }
        }

        internal Antenna() { }
        internal Antenna(PartModule pModule, Part iPart)
        {
            this.AntennaModule = pModule;
            this.SPart = iPart;
        }

        internal void ExtendAntenna()
        {
            if (iModule.Events["Toggle"].guiName == "Extend")
                iModule.Toggle();
        }

        internal void RetractAntenna()
        {
            if (iModule.Events["Toggle"].guiName == "Retract")
                iModule.Toggle();
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
                Utilities.LogMessage(string.Format(" in SolarPanel.Highlight at step {0}.  Error:  {1}", step, ex.ToString()), "Error", true);
            }
        }
    }
}
