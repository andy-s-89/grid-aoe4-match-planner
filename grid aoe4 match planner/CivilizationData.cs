using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grid_aoe4_match_planner
{
    public class CivilizationData
    {
        public string name { get; set; }
        public string description { get; set; }
        public string classes { get; set; }
        public List<OverviewItem> overview { get; set; }
        public Dictionary<string, object> techtree { get; set; }
    }

    public class OverviewItem
    {
        public string title { get; set; }
        public List<string> list { get; set; }
        public string description { get; set; }
    }

    public class TechtreeItem
    {
        public Dictionary<string, object> subItems { get; set; }
    }
}

