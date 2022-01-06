using ShipManifest.InternalObjects.Settings;

namespace ShipManifest.Modules
{
  public class ModKerbal
  {
    public bool Badass;
    public bool Veteran;
    public float Courage;
    public ProtoCrewMember.Gender Gender;
    public ProtoCrewMember.KerbalSuit Suit;
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
      Veteran = kerbal.veteran;
      Trait = kerbal.trait;
      Gender = kerbal.gender;
      Suit = kerbal.suit;
      IsNew = isNew;
    }

    public ProtoCrewMember Kerbal { get; set; }
    public bool IsNew { get; set; }

    public string SubmitChanges()
    {
      if (NameExists())
      {
        return SmUtils.SmTags["#smloc_module_002"]; // "That name is in use!";
      }

      SyncKerbal();

      if (IsNew)
      {
        // Add to roster.
        Kerbal.rosterStatus = ProtoCrewMember.RosterStatus.Available;
        HighLogic.CurrentGame.CrewRoster.AddCrewMember(Kerbal);
      }
      return string.Empty;
    }

    public static ModKerbal CreateKerbal(ProtoCrewMember.KerbalType kerbalType)
    {
      ProtoCrewMember kerbal = CrewGenerator.RandomCrewMemberPrototype(kerbalType);
      return new ModKerbal(kerbal, true);
    }

    public void SyncKerbal()
    {
      if (CurrSettings.EnableKerbalRename)
      {
        Kerbal.ChangeName(Name);
        if (CurrSettings.EnableChangeProfession)
          KerbalRoster.SetExperienceTrait(Kerbal, Trait);
      }
      Kerbal.gender = Gender;
      Kerbal.suit = Suit;
      Kerbal.stupidity = Stupidity;
      Kerbal.courage = Courage;
      Kerbal.isBadass = Badass;
      Kerbal.veteran = Veteran;
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
