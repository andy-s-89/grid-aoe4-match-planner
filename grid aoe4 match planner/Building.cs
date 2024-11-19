using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;

namespace grid_aoe4_match_planner
{
    public class costs
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

    public class armor
    {
        public string type { get; set; }
        public int value { get; set; }
    }

    public class sight
    {
        public int line { get; set; }
        public int height { get; set; }
    }

    public class building
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
        public costs costs { get; set; }
        public List<string> producedBy { get; set; }
        public string icon { get; set; }
        public int hitpoints { get; set; }
        public List<object> weapons { get; set; }  // Adjust this type if you know what the actual type is
        public List<armor> armor { get; set; }
        public sight sight { get; set; }
        public List<object> influences { get; set; }  // Adjust this type if you know what the actual type is
    }

    public class CivBuildingData
    {
        public List<building> data { get; set; }
    }

}
