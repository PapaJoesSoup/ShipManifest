using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ShipManifest
{
    class Antenna
    {
        private PartModule _xmitterModule;
        internal PartModule XmitterModule
        {
            get { return _xmitterModule; }
            set { _xmitterModule = value; }
        }

        private PartModule _animaeModule;
        internal PartModule AnimateModule
        {
            get { return _animaeModule; }
            set { _animaeModule = value; }
        }

        private Part _spart;
        internal Part SPart
        {
            get { return _spart; }
            set { _spart = value; }
        }

        internal bool isRTModule
        {
            get
            {
                if (XmitterModule.moduleName == "ModuleRTAntenna")
                    return true;
                else
                    return false;
            }
        }

        internal bool Extended
        {
            get
            {
                if (isRTModule)
                    return XmitterModule.Events["EventClose"].active;
                else
                    return iModule.Events["Toggle"].guiName == "Retract";
            }
        }

        internal string AntennaStatus
        {
            get
            {
                // RT support:
                if (isRTModule)
                {
                    if (XmitterModule.Events["EventClose"].active)
                        return "Activated";
                    else
                        return "Deactivated";
                }
                else
                {
                    if (iModule.Events["Toggle"].guiName == "Retract")
                        return "Extended";
                    else
                        return "Retracted";
                }
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
            get { return (ModuleAnimateGeneric)this.AnimateModule; }
        }

        internal Antenna() { }

        internal Antenna(PartModule xModule, PartModule pModule, Part iPart)
        {
            this.XmitterModule = xModule;
            this.AnimateModule = pModule;
            this.SPart = iPart;
        }

        internal void ExtendAntenna()
        {
            // RT support:
            if (isRTModule)
                XmitterModule.Events["EventOpen"].Invoke();
            else if (iModule.Events["Toggle"].guiName == "Extend")
                iModule.Toggle();
        }

        internal void RetractAntenna()
        {
            // RT support:
            if (isRTModule)
                XmitterModule.Events["EventClose"].Invoke();
            else if (iModule.Events["Toggle"].guiName == "Retract")
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
