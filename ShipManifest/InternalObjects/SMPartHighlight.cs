using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ShipManifest.InternalObjects
{
  internal static class SMPart
  {

    internal static bool IsSelected(Part part)
    {
      return SMAddon.SmVessel.SelectedResourcesParts.Contains(part);
    }

    internal static bool IsCrew(Part part)
    {
      return IsSelected(part) && SMAddon.SmVessel.SelectedResources.Contains("Crew");
    }

    internal static bool IsTarget(Part part)
    {
      return SMAddon.SmVessel.SelectedPartsTarget.Contains(part);
    }

    internal static bool IsClsTarget(Part part)
    {
      return SMAddon.SmVessel.ClsPartTarget == part;
    }

    internal static bool IsSource(Part part)
    {
      return SMAddon.SmVessel.SelectedPartsSource.Contains(part);
    }

    internal static Color GetHighlightColor(Part part, out string colorstring)
    {
      colorstring = "clear";
      if (IsSource(part)) colorstring = SMSettings.SourcePartColor;
      else if (IsTarget(part)) colorstring = IsCrew(part) 
          ? IsClsTarget(part) ? SMSettings.TargetPartCrewColor : SMSettings.TargetPartColor
          : SMSettings.TargetPartColor;
      else if (IsSelected(part)) colorstring = SMSettings.ResourcePartColor;
      return SMSettings.Colors[colorstring];
    }
  }
}
