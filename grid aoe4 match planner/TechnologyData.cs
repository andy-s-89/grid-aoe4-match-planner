using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grid_aoe4_match_planner
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class TechnologyEffect
    {
        public string property { get; set; }
        public Selector select { get; set; }
        public string effect { get; set; }
        public double? value { get; set; }
        public string type { get; set; }
    }

    public class Selector
    {
        public List<List<string>> @class { get; set; }
        public List<string> id { get; set; }
    }

    public class TechnologyCost
    {
        public int food { get; set; }
        public int wood { get; set; }
        public int stone { get; set; }
        public int gold { get; set; }
        public int vizier { get; set; }
        public int oliveoil { get; set; }
        public int total { get; set; }
        public int popcap { get; set; }
        public int time { get; set; }
    }

    public class TechnologyData
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
        public TechnologyCost costs { get; set; }
        public List<string> producedBy { get; set; }
        public string icon { get; set; }
        public List<TechnologyEffect> effects { get; set; }
    }

    public class CivTechnologyData
    {
        public List<TechnologyData> data { get; set; }
    }
}
