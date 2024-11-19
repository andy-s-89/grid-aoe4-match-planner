using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grid_aoe4_match_planner
{
    public class TechnologyManager
    {
        public List<Technology> Technologies { get; private set; }

        public TechnologyManager()
        {
            Technologies = new List<Technology>();
        }

        // Add a Technology to the list
        public void AddTechnology(Technology technology)
        {
            Technologies.Add(technology);
        }

        // Get a list of all Technology instances
        public List<Technology> GetAllTechnologies()
        {
            return Technologies;
        }

        // Remove a specific Technology from the list
        public void RemoveTechnology(Technology technology)
        {
            Technologies.Remove(technology);
        }

        // Method to find a villager by their PictureBox
        public Technology GetTechnologyByPictureBox(PictureBox pictureBox)
        {
            return Technologies.FirstOrDefault(u => u.ItemPictureBox == pictureBox);
        }
    }
}