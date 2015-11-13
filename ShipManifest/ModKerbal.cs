using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ShipManifest
{
  public class ModKerbal
  {
    public ProtoCrewMember Kerbal { get; set; }
    public bool IsNew { get; set; }
    public float Stupidity;
    public float Courage;
    public bool Badass;
    public string Name;
    public string Trait;
    public ProtoCrewMember.Gender Gender;

    public ModKerbal(ProtoCrewMember kerbal, bool isNew)
    {
      this.Kerbal = kerbal;
      Name = kerbal.name;
      Stupidity = kerbal.stupidity;
      Courage = kerbal.courage;
      Badass = kerbal.isBadass;
      Trait = kerbal.trait;
      Gender = kerbal.gender;
      IsNew = isNew;
    }

    public string SubmitChanges()
    {
      if (NameExists())
      {
        return "That name is in use!";
      }

      SyncKerbal();

      if (IsNew)
      {
        // Add to roster.
        MethodInfo dynMethod = HighLogic.CurrentGame.CrewRoster.GetType().GetMethod("AddCrewMember", BindingFlags.NonPublic | BindingFlags.Instance);
        Kerbal.rosterStatus = ProtoCrewMember.RosterStatus.Available;
        dynMethod.Invoke(HighLogic.CurrentGame.CrewRoster, new object[] { Kerbal });
      }

      return string.Empty;
    }

    public static ModKerbal CreateKerbal()
    {
      ProtoCrewMember kerbal = CrewGenerator.RandomCrewMemberPrototype();
      return new ModKerbal(kerbal, true);
    }

    public void SyncKerbal()
    {
      if (SMSettings.EnableKerbalRename)
        Kerbal.name = Name;
      // remove old save game hack for backwards compatability...
      Kerbal.name = Kerbal.name.Replace(char.ConvertFromUtf32(1), "");

      // New trait management is easy!
      if (SMSettings.EnableKerbalRename && SMSettings.RenameWithProfession)
        KerbalRoster.SetExperienceTrait(Kerbal, Trait);
      Kerbal.gender = Gender;
      Kerbal.stupidity = Stupidity;
      Kerbal.courage = Courage;
      Kerbal.isBadass = Badass;
    }

    private bool NameExists()
    {
      if (IsNew || Kerbal.name != Name)
      {
        return HighLogic.CurrentGame.CrewRoster.Exists(Name);
      }

      return false;
    }

  }
}
