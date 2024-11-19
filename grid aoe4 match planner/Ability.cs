using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grid_aoe4_match_planner
{
    public class AbilityCost
    {
        public int food { get; set; }
        public int wood { get; set; }
        public int stone { get; set; }
        public int gold { get; set; }
        public int vizier { get; set; }
        public int oliveoil { get; set; }
        public int total { get; set; }
        public int popcap { get; set; }
    }

    public class AbilityEffect
    {
        public string property { get; set; }
        public AbilitySelect select { get; set; }
        public string effect { get; set; }

        // Change type to object to handle different types (string, int, etc.)
        public object value { get; set; }

        public string type { get; set; }
        public int? duration { get; set; } // Nullable because it's not always present

        // Helper property to get value as nullable int
        public int? ValueAsInt {
            get {
                if (value is int intValue)
                    return intValue;
                if (int.TryParse(value?.ToString(), out intValue))
                    return intValue;
                return null;
            }
        }
    }

    public class AbilitySelect
    {
        public List<List<string>> @class { get; set; }
        public List<string> id { get; set; }
    }

    public class Ability
    {
        public string id { get; set; }
        public string baseId { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public int pbgid { get; set; }
        public string attribName { get; set; }
        public int age { get; set; }
        public List<string> civs { get; set; }
        public string description { get; set; }
        public List<string> classes { get; set; }
        public List<string> displayClasses { get; set; }
        public AbilityCost costs { get; set; }
        public List<string> producedBy { get; set; }
        public string icon { get; set; }
        public string active { get; set; }
        public double auraRange { get; set; }
        public List<AbilityEffect>? effects { get; set; }
        public List<string> activatedOn { get; set; }

        // Change type to object to handle different types (string, int, etc.)
        public object cooldown { get; set; }

        // Helper property to get cooldown as nullable int
        public int? CooldownAsInt {
            get {
                if (cooldown is int intValue)
                    return intValue;
                if (int.TryParse(cooldown?.ToString(), out intValue))
                    return intValue;
                return null;
            }
        }
    }

    public class CivAbilityData
    {
        public List<Ability> data { get; set; }
    }
}


