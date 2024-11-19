using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grid_aoe4_match_planner
{
    public class Resources
    {
        public int Food { get; set; }
        public int Wood { get; set; }
        public int Gold { get; set; }
        public int Stone { get; set; }
        public int OliveOil { get;  set; }
        public int PopCap { get; set; }

        // Total counts all resources except for PopCap
        public int Total => Food + Wood + Gold + Stone + OliveOil;

        public Resources(int food, int wood, int gold, int stone, int oliveOil, int popCap)
        {
            Food = food;
            Wood = wood;
            Gold = gold;
            Stone = stone;
            OliveOil = oliveOil;
            PopCap = popCap;
        }

        // Check if there are enough resources to cover the costs
        public bool HasEnoughResources(Resources costs)
        {
            return Food >= costs.Food &&
                   Wood >= costs.Wood &&
                   Gold >= costs.Gold &&
                   Stone >= costs.Stone &&
                   OliveOil >= costs.OliveOil;
        }

        // Subtract resources based on costs
        public void Subtract(Resources costs)
        {
            if (HasEnoughResources(costs))
            {
                Food -= costs.Food;
                Wood -= costs.Wood;
                Gold -= costs.Gold;
                Stone -= costs.Stone;
                OliveOil -= costs.OliveOil;
            }
        }

        // Add resources over time from villagers or other sources
        public void Add(Resources additionalResources)
        {
            Food += additionalResources.Food;
            Wood += additionalResources.Wood;
            Gold += additionalResources.Gold;
            Stone += additionalResources.Stone;
            OliveOil += additionalResources.OliveOil;
        }
    }

}
