using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace grid_aoe4_match_planner
{
    public class UpgradeCost
    {
        public int food { get; set; }
        public int wood { get; set; }
        public int stone { get; set; }
        public int gold { get; set; }
        public int vizier { get; set; }
        public int oliveoil { get; set; }
        public int total { get; set; }
        public int popcap { get; set; }
        [JsonConverter(typeof(IntOrDoubleConverter))]
        public int time { get; set; }
    }

    public class UpgradeData
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
        public bool unique { get; set; }
        public UpgradeCost costs { get; set; }
        public List<string> producedBy { get; set; }
        public string icon { get; set; }
        public string unlocks { get; set; }
    }

    public class CivUpgradeData
    {
        public List<UpgradeData> data { get; set; }
    }

}
