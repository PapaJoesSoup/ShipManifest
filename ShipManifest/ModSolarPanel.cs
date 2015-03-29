using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ShipManifest
{
    class ModSolarPanel
    {
        private PartModule _panelModule;
        internal PartModule PanelModule
        {
            get { return _panelModule; }
            set { _panelModule = value; }
        }

        private Part _spart;
        internal Part SPart
        {
            get { return _spart; }
            set { _spart = value; }
        }

        internal ModuleDeployableSolarPanel.panelStates PanelState
        {
            get { return iModule.panelState; }
        }

        internal string PanelStatus
        {
            get { return iModule.stateString; }
        }

        internal bool Retractable
        {
            get { return iModule.retractable; }
        }

        internal bool CanBeRetracted
        {
            get
            {
                if (Settings.RealismMode && !this.Retractable && (this.PanelState == ModuleDeployableSolarPanel.panelStates.EXTENDED || this.PanelState == ModuleDeployableSolarPanel.panelStates.EXTENDING))
                    return false;
                else
                    return true;
            }
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

        private ModuleDeployableSolarPanel iModule
        {
            get { return (ModuleDeployableSolarPanel)this.PanelModule; }
        }

        internal ModSolarPanel() { }
        internal ModSolarPanel(PartModule pModule, Part iPart)
        {
            this.PanelModule = pModule;
            this.SPart = iPart;
        }

        internal void ExtendPanel()
        {
            iModule.Extend();
        }

        internal void RetractPanel()
        {
            iModule.Retract();
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
