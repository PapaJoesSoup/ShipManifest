using System.Reflection;

namespace ShipManifest.Modules
{
  public class ModKerbal
  {
    public bool Badass;
    public float Courage;
    public ProtoCrewMember.Gender Gender;
    public string Name;
    public float Stupidity;
    public string Trait;

    public ModKerbal(ProtoCrewMember kerbal, bool isNew)
    {
      Kerbal = kerbal;
      Name = kerbal.name;
      Stupidity = kerbal.stupidity;
      Courage = kerbal.courage;
      Badass = kerbal.isBadass;
      Trait = kerbal.trait;
      Gender = kerbal.gender;
      IsNew = isNew;
    }

    public ProtoCrewMember Kerbal { get; set; }
    public bool IsNew { get; set; }

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
        MethodInfo dynMethod = HighLogic.CurrentGame.CrewRoster.GetType()
          .GetMethod("AddCrewMember", BindingFlags.NonPublic | BindingFlags.Instance);
        Kerbal.rosterStatus = ProtoCrewMember.RosterStatus.Available;
        dynMethod.Invoke(HighLogic.CurrentGame.CrewRoster, new object[] {Kerbal});
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
      if (SMSettings.EnableKerbalRename && SMSettings.EnableChangeProfession)
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