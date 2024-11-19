using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grid_aoe4_match_planner
{
    public class UnitManager
    {
        public List<Unit> Units { get; private set; }

        public UnitManager()
        {
            Units = new List<Unit>();
        }

        // Add a Unit to the list
        public void AddUnit(Unit unit)
        {
            Units.Add(unit);
        }

        // Get a list of all Unit instances
        public List<Unit> GetAllUnits()
        {
            return Units;
        }

        // Remove a specific Unit from the list
        public void RemoveUnit(Unit unit)
        {
            Units.Remove(unit);
        }

        // Method to find a villager by their PictureBox
        public Unit GetUnitByPictureBox(PictureBox pictureBox)
        {
            return Units.FirstOrDefault(u => u.ItemPictureBox == pictureBox);
        }
    }
}