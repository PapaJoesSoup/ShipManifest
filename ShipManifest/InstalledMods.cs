using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;

namespace ShipManifest
{
  class InstalledMods
  {
    private static Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

    internal static bool IsRTInstalled
    {
      get
      {
        return IsModInstalled("RemoteTech");
      }
    }
    internal static bool IsSMInstalled
    {
      get
      {
        return IsModInstalled("ShipManifest");
      }
    }
    internal static bool IsKISInstalled
    {
      get
      {
        return IsModInstalled("KIS");
      }
    }
    internal static bool IsCLSInstalled
    {
      get
      {
        return IsModInstalled("ConnectedLivingSpace");
      }
    }

    internal static void DisplayAssemblyList()
    {
      List<Assembly> SortedAssemblies = (from a in assemblies where a.FullName == a.FullName select a).OrderBy(a => a.FullName).ToList();
      foreach (Assembly assembly in SortedAssemblies)
      {
        string[] fullName = assembly.FullName.Split(',');
        GUILayout.BeginHorizontal();
        GUILayout.Label(fullName[0], GUILayout.Width(190));
        GUILayout.Label(fullName[1]);
        GUILayout.EndHorizontal();
      }
    }

    internal static void DisplayModList()
    {
      List<Assembly> SortedAssemblies = (from a in assemblies where a.FullName == a.FullName select a).OrderBy(a => a.FullName).ToList();
      foreach (Assembly assembly in SortedAssemblies)
      {
        if (!IsKSPAssembly(assembly.FullName))
        {
          string[] fullName = assembly.FullName.Split(',');
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
        Assembly assembly = (from a in assemblies
                             where a.FullName.Split(',')[0] == assemblyName
                             select a).First();
        return assembly != null;
      }
      catch
      {
        return false;
      }
    }

    private static bool IsKSPAssembly(string assemblyName)
    {
      List<string> KspAssemblies = new List<string>();
      KspAssemblies.Add("Assembly-UnityScript");
      KspAssemblies.Add("Assembly-UnityScript-firstpass");
      KspAssemblies.Add("Assembly-CSharp");
      KspAssemblies.Add("Assembly-CSharp-firstpass");
      KspAssemblies.Add("Boo.Lang");
      KspAssemblies.Add("MiscUtil");
      KspAssemblies.Add("Mono.Cecil");
      KspAssemblies.Add("Mono.Cecil.Mdb");
      KspAssemblies.Add("Mono.Cecil.Pdb");
      KspAssemblies.Add("Mono.Security");
      KspAssemblies.Add("mscorlib");
      KspAssemblies.Add("System.Xml");
      KspAssemblies.Add("System");
      KspAssemblies.Add("System.Core");
      KspAssemblies.Add("System.Xml.Linq");
      KspAssemblies.Add("TDx.TDxInput");
      KspAssemblies.Add("TrackIRUnity");
      KspAssemblies.Add("UnityEngine.UI");
      KspAssemblies.Add("UnityEngine");
      KspAssemblies.Add("XmlDiffPatch");

      try
      {
        foreach (string Name in KspAssemblies)
        {
          if (assemblyName.Contains(Name))
            return true;
        }
        return false;

      }
      catch
      {
        return false;
      }
    }
  }
}
