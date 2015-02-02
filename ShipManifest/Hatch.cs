using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ConnectedLivingSpace;

namespace ShipManifest
{
    public class Hatch
    {
        private PartModule _hatchModule;
        public PartModule HatchModule
        {
            get
            {
                return _hatchModule;
            }
            set
            {
                _hatchModule = value;
            }
        }

        private ICLSPart _clsPart;
        public ICLSPart CLSPart
        {
            get
            {
                return _clsPart;
            }
            set
            {
                _clsPart = value;
            }
        }

        public bool HatchOpen
        {
            get
            {
                return iModule.HatchOpen;
            }
            set
            {
                iModule.HatchOpen = value;
            }
        }

        public string HatchStatus
        {
            get
            {
                return iModule.HatchStatus;
            }
        }

        public bool IsDocked
        {
            get
            {
                return iModule.IsDocked;
            }
        }

        public string Title
        {
            get
            {
                return iModule.ModDockNode.part.parent.partInfo.title;
            }
        }

        private IModuleDockingHatch iModule
        {
            get
            {
                return (IModuleDockingHatch)this.HatchModule;
            }
        }

        public Hatch() { }
        public Hatch(PartModule pModule, ICLSPart iPart)
        {
            this.HatchModule = pModule;
            this.CLSPart = iPart;
        }

        public void OpenHatch()
        {
            iModule.HatchEvents["CloseHatch"].active = true;
            iModule.HatchEvents["OpenHatch"].active = false;
            iModule.HatchOpen = true;
            ShipManifestAddon.FireEventTriggers();
        }
        public void CloseHatch()
        {
            iModule.HatchEvents["CloseHatch"].active = false;
            iModule.HatchEvents["OpenHatch"].active = true;
            iModule.HatchOpen = false;
            ShipManifestAddon.FireEventTriggers();
        }

        public void Highlight()
        {
            if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
            {
                if (iModule.HatchOpen)
                    iModule.ModDockNode.part.SetHighlightColor(Settings.Colors[Settings.HatchOpenColor]);
                else
                    iModule.ModDockNode.part.SetHighlightColor(Settings.Colors[Settings.HatchCloseColor]);
                iModule.ModDockNode.part.SetHighlight(true, false);
            }
            else
            {
                if (iModule.ModDockNode.part.highlightColor == Settings.Colors[Settings.HatchOpenColor] || iModule.ModDockNode.part.highlightColor == Settings.Colors[Settings.HatchCloseColor])
                {
                    if (Settings.EnableCLS && ShipManifestAddon.smController.SelectedResource == "Crew" && Settings.ShowTransferWindow)
                    {
                        CLSPart.Highlight(true, true);
                    }
                    else
                    {
                        iModule.ModDockNode.part.SetHighlight(false, false);
                        iModule.ModDockNode.part.SetHighlightDefault();
                        //Utilities.LogMessage("actual Default Color=" + iModule.ModDockNode.part.highlightColor.ToString(), "Info", true);
                        iModule.ModDockNode.part.SetHighlightType(Part.HighlightType.OnMouseOver);
                    }
                }
            }
        }
    }

}
