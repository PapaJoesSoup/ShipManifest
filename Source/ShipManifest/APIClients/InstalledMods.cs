using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ShipManifest.APIClients
{
  internal class InstalledMods
  {
    private const float GuiWidth = 190;
    // Properties
    private static readonly Assembly[] Assemblies = AppDomain.CurrentDomain.GetAssemblies();

    internal static bool IsDfApiReady => DfWrapper.ApiReady;

    internal static bool IsDfInstalled => IsModInstalled("DeepFreeze");

    internal static bool IsRtInstalled => IsModInstalled("RemoteTech");

    internal static bool IsSmInstalled => IsModInstalled("ShipManifest");

    internal static bool IsKisInstalled => IsModInstalled("KIS");

    internal static bool IsClsInstalled => IsModInstalled("ConnectedLivingSpace");

    // Methods
    internal static void DisplayAssemblyList()
    {
      List<Assembly> sortedAssemblies = (from a in Assemblies select a).OrderBy(a => a.FullName).ToList();
      if (sortedAssemblies.Count == 0) return;
      List<Assembly>.Enumerator list = sortedAssemblies.GetEnumerator();
      while (list.MoveNext())
      {
        if (list.Current == null) continue;
        string[] fullName = list.Current.FullName.Split(',');
        GUILayout.BeginHorizontal();
        GUILayout.Label(fullName[0], GUILayout.Width(GuiWidth));
        GUILayout.Label(fullName[1]);
        GUILayout.EndHorizontal();
      }
      list.Dispose();
    }

    internal static void DisplayModList()
    {
      List<Assembly> sortedAssemblies = (from a in Assemblies select a).OrderBy(a => a.FullName).ToList();
      if (sortedAssemblies.Count == 0) return;
      List<Assembly>.Enumerator list = sortedAssemblies.GetEnumerator();
      while (list.MoveNext())
      {
        if (list.Current == null) continue;
        string[] fullName = list.Current.FullName.Split(',');
        GUILayout.BeginHorizontal();
        GUILayout.Label(fullName[0], GUILayout.Width(GuiWidth));
        GUILayout.Label(fullName[1]);
        GUILayout.EndHorizontal();
      }
      list.Dispose();
    }

    internal static bool IsModInstalled(string assemblyName)
    {
      try
      {
        Assembly assembly = (from a in Assemblies
          where a.FullName.Split(',')[0] == assemblyName
          select a).First();
        return assembly != null;
      }
      catch
      {
        return false;
      }
    }
  }
}
