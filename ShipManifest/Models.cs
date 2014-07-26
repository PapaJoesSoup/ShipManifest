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

        public KerbalModel(ProtoCrewMember kerbal, bool isNew)
        {
            this.Kerbal = kerbal;
            Name = kerbal.name;
            Stupidity = kerbal.stupidity;
            Courage = kerbal.courage;
            Badass = kerbal.isBadass;
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

        public void SyncKerbal()
        {
            Kerbal.name = Name;
            Kerbal.stupidity = Stupidity;
            Kerbal.courage = Courage;
            Kerbal.isBadass = Badass;
        }

        private bool NameExists()
        {
            if(IsNew || Kerbal.name != Name)
            {
                return HighLogic.CurrentGame.CrewRoster.Exists(Name);
            }

            return false;
        }
    }
}
