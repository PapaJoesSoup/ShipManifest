using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using HighlightingSystem;

namespace ShipManifest
{
    class ModAntenna
    {
        private PartModule _xmitterModule;
        internal PartModule XmitterModule
        {
            get { return _xmitterModule; }
            set { _xmitterModule = value; }
        }

        private PartModule _animateModule;
        internal PartModule AnimateModule
        {
            get { return _animateModule; }
            set { _animateModule = value; }
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
                    title = _spart.partInfo.title + "\r\n on " + _spart.parent.partInfo.title;
                }
                catch
                {
                    title = _spart.partInfo.title;
                }
                return title;
            }
        }

        private ModuleAnimateGeneric iModule
        {
            get { return (ModuleAnimateGeneric)this.AnimateModule; }
        }

        internal ModAntenna() { }

        internal ModAntenna(PartModule xModule, PartModule pModule, Part iPart)
        {
            this.XmitterModule = xModule;
            this.AnimateModule = pModule;
            this.SPart = iPart;
        }

        internal void ExtendAntenna()
        {
            // RT support:
            if (isRTModule)
            {
                if (TabAntenna.RTInstalled)
                {
                    //if (RemoteTech.API.API.HasFlightComputer(SMAddon.vessel.id))
                    //{
                    //    ConfigNode configNode = new ConfigNode();
                    //    configNode.AddValue("GUIDString", SMAddon.vessel.id);
                    //    configNode.AddValue("Executor", "ShipManifest");
                    //    configNode.AddValue("ReflectionType", "ShipManifest");
                    //    RemoteTech.API.API.QueueCommandToFlightComputer(configNode);
                    //}
                    //else
                        XmitterModule.Events["EventOpen"].Invoke();
                }
                else
                    XmitterModule.Events["EventOpen"].Invoke();
            }
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

        internal void MouseOverHighlight(Rect rect)
        {
            string step = "begin";
            try
            {
                if (rect.Contains(Event.current.mousePosition))
                {
                    SMHighlighter.SetPartHighlight(_spart, SMSettings.Colors[SMSettings.MouseOverColor]);
                    SMHighlighter.EdgeHighight(_spart, true);
                }
                else
                {
                    SMHighlighter.ClearPartHighlight(_spart);
                    SMHighlighter.EdgeHighight(_spart, false);
                }
            }
            catch (Exception ex)
            {
                Utilities.LogMessage(string.Format(" in modAntenna.Highlight at step {0}.  Error:  {1}", step, ex.ToString()), "Error", true);
            }
        }
    }
}
