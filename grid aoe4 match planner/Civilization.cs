using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grid_aoe4_match_planner
{
    // create a civ class with starting resources
    public class Civilization
    {
        public string Name { get; set; }
        public Resources StartingResources { get; set; }
        public int StartingVillagers { get; set; }

        public Civilization(string name, Resources startingResources, int startingVillagers)
        {
            Name = name;
            StartingResources = startingResources;
            StartingVillagers = startingVillagers;
        }
    }
}
