using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grid_aoe4_match_planner
{
    public class UpgradeManager
    {
        public List<Upgrade> Upgrades { get; private set; }

        public UpgradeManager()
        {
            Upgrades = new List<Upgrade>();
        }

        // Add a Upgrade to the list
        public void AddUpgrade(Upgrade upgrade)
        {
            Upgrades.Add(upgrade);
        }

        // Get a list of all Upgrade instances
        public List<Upgrade> GetAllUpgrades()
        {
            return Upgrades;
        }

        // Remove a specific Upgrade from the list
        public void RemoveUpgrade(Upgrade upgrade)
        {
            Upgrades.Remove(upgrade);
        }

        // Method to find a villager by their PictureBox
        public Upgrade GetUpgradeByPictureBox(PictureBox pictureBox)
        {
            return Upgrades.FirstOrDefault(u => u.ItemPictureBox == pictureBox);
        }
    }
}