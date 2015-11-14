using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using HighlightingSystem;

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
        if (SMSettings.RealismMode && !this.Retractable && (this.PanelState == ModuleDeployableSolarPanel.panelStates.EXTENDED || this.PanelState == ModuleDeployableSolarPanel.panelStates.EXTENDING))
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
          title = _spart.partInfo.title + "\r\n on " + _spart.parent.partInfo.title;
        }
        catch
        {
          title = _spart.partInfo.title;
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

  }
}
