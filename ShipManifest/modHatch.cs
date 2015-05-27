using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using HighlightingSystem;
using ConnectedLivingSpace;

namespace ShipManifest
{
    internal class ModHatch
    {
        private PartModule _hatchModule;
        internal PartModule HatchModule
        {
            get { return _hatchModule; }
            set { _hatchModule = value; }
        }

        private ICLSPart _clsPart;
        internal ICLSPart CLSPart
        {
            get { return _clsPart; }
            set { _clsPart = value; }
        }

        internal bool HatchOpen
        {
            get { return iModule.HatchOpen; }
            set { iModule.HatchOpen = value; }
        }

        internal string HatchStatus
        {
            get { return iModule.HatchStatus; }
        }

        internal bool IsDocked
        {
            get { return iModule.IsDocked; }
        }

        internal string Title
        {
            get 
            {
                string title = "";
                try
                {
                    if (null != _clsPart.Part.parent)
                        title = _clsPart.Part.parent.partInfo.title;
                    else
                        title = _clsPart.Part.partInfo.title;
                }
                catch
                {
                    title = "Unknown";
                }
                return title; 
            }
        }

        private IModuleDockingHatch iModule
        {
            get { return (IModuleDockingHatch)this.HatchModule; }
        }

        internal ModHatch() { }
        internal ModHatch(PartModule pModule, ICLSPart iPart)
        {
            this.HatchModule = pModule;
            this.CLSPart = iPart;
        }

        internal void OpenHatch(bool fireEvent = false)
        {
            iModule.HatchEvents["CloseHatch"].active = true;
            iModule.HatchEvents["OpenHatch"].active = false;
            iModule.HatchOpen = true;
            if (fireEvent)
                SMAddon.FireEventTriggers();
        }
        internal void CloseHatch(bool fireEvent = false)
        {
            iModule.HatchEvents["CloseHatch"].active = false;
            iModule.HatchEvents["OpenHatch"].active = true;
            iModule.HatchOpen = false;
            if (fireEvent)
                SMAddon.FireEventTriggers();
        }

        internal void Highlight(Rect rect)
        {
            try
            {
                if (rect.Contains(Event.current.mousePosition))
                {
                    SMHighlighter.SetPartHighlight(_clsPart.Part, SMSettings.Colors[SMSettings.MouseOverColor]);
                    SMHighlighter.EdgeHighight(_clsPart.Part, true);
                }
                else
                {
                    if (_clsPart.Part.highlightColor == SMSettings.Colors[SMSettings.MouseOverColor])
                    {
                        SMHighlighter.EdgeHighight(_clsPart.Part, false);
                        if (SMSettings.EnableCLS && SMAddon.smController.SelectedResources.Contains("Crew") && WindowTransfer.ShowWindow)
                        {
                            if (CLSPart.Space != null)
                                CLSPart.Highlight(true, true);
                            else
                            {
                                SMHighlighter.ClearPartHighlight(_clsPart.Part);
                            }
                        }
                        else
                        {
                            SMHighlighter.ClearPartHighlight(_clsPart.Part);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (!SMAddon.frameErrTripped)
                {
                    Utilities.LogMessage(string.Format(" in Hatch.Highlight.  Error:  {0}", ex.ToString()), "Error", true);
                    SMAddon.frameErrTripped = true;
                }


            }
        }
    }

}
