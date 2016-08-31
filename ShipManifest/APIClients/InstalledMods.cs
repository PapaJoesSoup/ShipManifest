﻿using System;
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

    internal static bool IsDfApiReady
    {
      get { return DFWrapper.APIReady; }
    }

    internal static bool IsDfInstalled
    {
      get { return IsModInstalled("DeepFreeze"); }
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
      List<Assembly> sortedAssemblies = (from a in Assemblies select a).OrderBy(a => a.FullName).ToList();
      if (sortedAssemblies.Count == 0) return;
      List<Assembly>.Enumerator list = sortedAssemblies.GetEnumerator();
      while (list.MoveNext())
      {
        if (list.Current != null)
        {
          string[] fullName = list.Current.FullName.Split(',');
          GUILayout.BeginHorizontal();
          GUILayout.Label(fullName[0], GUILayout.Width(190));
          GUILayout.Label(fullName[1]);
          GUILayout.EndHorizontal();
        }
      }
    }

    internal static void DisplayModList()
    {
      List<Assembly> sortedAssemblies = (from a in Assemblies select a).OrderBy(a => a.FullName).ToList();
      if (sortedAssemblies.Count == 0) return;
      List<Assembly>.Enumerator list = sortedAssemblies.GetEnumerator();
      while (list.MoveNext())
      {
        if (list.Current != null)
        {
          if (!IsKspAssembly(list.Current.FullName))
          {
            string[] fullName = list.Current.FullName.Split(',');
            GUILayout.BeginHorizontal();
            GUILayout.Label(fullName[0], GUILayout.Width(190));
            GUILayout.Label(fullName[1]);
            GUILayout.EndHorizontal();
          }
        }
      }
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

    private static bool IsKspAssembly(string assemblyName)
    {
      List<string> kspAssemblies = new List<string>
      {
        "Assembly-CSharp",
        "Assembly-CSharp-firstpass",
        "Assembly-UnityScript",
        "Assembly-UnityScript-firstpass",
        "Boo.Lang",
        "KSPAssets",
        "KSPCore",
        "KSPUtil",
        "MiscUtil",
        "Mono.Cecil",
        "Mono.Security",
        "mscorlib",
        "SaveUpgradePipeline.Core",
        "SaveUpgradePipeline.Scripts",
        "System",
        "System.Core",
        "System.Xml",
        "System.Xml.Linq",
        "TDx.TDxInput",
        "TrackIRUnity",
        "UnityEngine",
        "UnityEngine.Networking",
        "UnityEngine.UI",
        "Vectrosity",
        "XmlDiffPatch",
        "XmlDiffPatch.View"
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