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
      return IsSelected(part) && SMAddon.SmVessel.SelectedResources.Contains(SMConditions.ResourceType.Crew.ToString());
    }

    internal static bool IsSource(Part part)
    {
      return SMAddon.SmVessel.SelectedPartsSource.Contains(part);
    }

    internal static bool IsTarget(Part part)
    {
      return SMAddon.SmVessel.SelectedPartsTarget.Contains(part);
    }

    internal static bool IsClsSource(Part part)
    {
      return SMAddon.SmVessel.ClsPartSource.Part == part;
    }

    internal static bool IsClsTarget(Part part)
    {
      return SMAddon.SmVessel.ClsPartTarget.Part == part;
    }

  }
}
