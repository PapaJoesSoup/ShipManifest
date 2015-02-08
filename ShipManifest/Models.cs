using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ShipManifest
{
    public class KerbalModel
    {
        public ProtoCrewMember Kerbal { get; set; }
        public bool IsNew { get; set; }
        public float Stupidity;
        public float Courage;
        public bool Badass;
        public string Name;
        public string Profession;
        public string Title;

        public KerbalModel(ProtoCrewMember kerbal, bool isNew)
        {
            this.Kerbal = kerbal;
            Name = kerbal.name;
            Stupidity = kerbal.stupidity;
            Courage = kerbal.courage;
            Badass = kerbal.isBadass;
            Profession = "";
            Title = kerbal.experienceTrait.Title;
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
                MethodInfo dynMethod = HighLogic.CurrentGame.CrewRoster.GetType().GetMethod("AddCrewMember", BindingFlags.NonPublic | BindingFlags.Instance);
                Kerbal.rosterStatus = ProtoCrewMember.RosterStatus.Available;
                //HighLogic.CurrentGame.CrewRoster.AddCrewMember(Kerbal);
                dynMethod.Invoke(HighLogic.CurrentGame.CrewRoster, new object[] { Kerbal });
            }

            return string.Empty;
        }

        public static KerbalModel CreateKerbal()
        {
            ProtoCrewMember kerbal = CrewGenerator.RandomCrewMemberPrototype();
            return new KerbalModel(kerbal, true);
        }

        public void SyncKerbal()
        {
            Kerbal.name = Name;
            Kerbal.stupidity = Stupidity;
            Kerbal.courage = Courage;
            Kerbal.isBadass = Badass;
            if (Title != Profession)
                Kerbal.experienceTrait = KerbalModel.GetTrait(Kerbal, Profession);
            Utilities.LogMessage(string.Format("SyncKerbal.  Trait should be:  " + Profession + ".  New Trait:  {0}", Kerbal.experienceTrait.Title), "info", true);
        }

        private bool NameExists()
        {
            if(IsNew || Kerbal.name != Name)
            {
                return HighLogic.CurrentGame.CrewRoster.Exists(Name);
            }

            return false;
        }

        public static Experience.ExperienceTrait GetTrait(ProtoCrewMember kerbal, string Profession)
        {
            Experience.ExperienceTrait ThisTrait = kerbal.experienceTrait;
            Utilities.LogMessage("GetTrait Current Profession is:  " + kerbal.experienceTrait.Title, "info", true);
            foreach (ProtoCrewMember thiskerbal in HighLogic.CurrentGame.CrewRoster.Crew)
            {
                if (thiskerbal.experienceTrait.Title == Profession)
                {
                    Utilities.LogMessage("GetTrait " + Profession + " found", "info", true);
                    ThisTrait = thiskerbal.experienceTrait;
                    break;
                }
            }
            Utilities.LogMessage(string.Format("GetTrait.  Trait should be:  " + Profession + ".  New Trait:  {0}", ThisTrait.Title), "info", true);
            return ThisTrait;
        }
    }
}
