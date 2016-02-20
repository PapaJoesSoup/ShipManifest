using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ShipManifest.APIClients
{
  internal class InstalledMods
  {
    // Properties
    private static readonly Assembly[] Assemblies = AppDomain.CurrentDomain.GetAssemblies();
    private static readonly bool? DfInstalled = null;

    internal static bool IsDfInstalled
    {
      get { return DfInstalled ?? DFWrapper.InitDFWrapper(); }
    }

    internal static bool IsRtInstalled
    {
      get { return IsModInstalled("RemoteTech"); }
    }

    internal static bool IsSmInstalled
    {
      get { return IsModInstalled("ShipManifest"); }
    }

    internal static bool IsKisInstalled
    {
      get { return IsModInstalled("KIS"); }
    }

    internal static bool IsClsInstalled
    {
      get { return IsModInstalled("ConnectedLivingSpace"); }
    }

    // Methods
    internal static void DisplayAssemblyList()
    {
      var sortedAssemblies = (from a in Assemblies select a).OrderBy(a => a.FullName).ToList();
      foreach (var assembly in sortedAssemblies)
      {
        var fullName = assembly.FullName.Split(',');
        GUILayout.BeginHorizontal();
        GUILayout.Label(fullName[0], GUILayout.Width(190));
        GUILayout.Label(fullName[1]);
        GUILayout.EndHorizontal();
      }
    }

    internal static void DisplayModList()
    {
      var sortedAssemblies = (from a in Assemblies select a).OrderBy(a => a.FullName).ToList();
      foreach (var assembly in sortedAssemblies)
      {
        if (!IsKspAssembly(assembly.FullName))
        {
          var fullName = assembly.FullName.Split(',');
          GUILayout.BeginHorizontal();
          GUILayout.Label(fullName[0], GUILayout.Width(190));
          GUILayout.Label(fullName[1]);
          GUILayout.EndHorizontal();
        }
      }
    }

    internal static bool IsModInstalled(string assemblyName)
    {
      try
      {
        var assembly = (from a in Assemblies
          where a.FullName.Split(',')[0] == assemblyName
          select a).First();
        return assembly != null;
      }
      catch
      {
        return false;
      }
    }

    private static bool IsKspAssembly(string assemblyName)
    {
      var kspAssemblies = new List<string>
      {
        "Assembly-UnityScript",
        "Assembly-UnityScript-firstpass",
        "Assembly-CSharp",
        "Assembly-CSharp-firstpass",
        "Boo.Lang",
        "MiscUtil",
        "Mono.Cecil",
        "Mono.Cecil.Mdb",
        "Mono.Cecil.Pdb",
        "Mono.Security",
        "mscorlib",
        "System.Xml",
        "System",
        "System.Core",
        "System.Xml.Linq",
        "TDx.TDxInput",
        "TrackIRUnity",
        "UnityEngine.UI",
        "UnityEngine",
        "XmlDiffPatch"
      };

      try
      {
        return kspAssemblies.Any(assemblyName.Contains);
      }
      catch
      {
        return false;
      }
    }
  }
}