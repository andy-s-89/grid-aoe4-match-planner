using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grid_aoe4_match_planner
{
    public class PlacedBuildingManager
    {
        public List<PlacedBuilding> placedBuildings;

        public PlacedBuildingManager()
        {
            placedBuildings = new List<PlacedBuilding>();
        }

        // Manages creation of buildings
        public PlacedBuilding CreateBuilding(building buildingInfo, double timePlaced, int column)
        {
            bool isTownCentre = buildingInfo.baseId.Contains("town-center", StringComparison.OrdinalIgnoreCase);

            if (isTownCentre)
            {
                return new TownCenter(buildingInfo, timePlaced, column); // Create a TownCenter
            }
            else
            {
                return new PlacedBuilding(buildingInfo, timePlaced, column); // Create a regular PlacedBuilding
            }
        }

        // Add a PlacedBuilding to the list
        public void AddPlacedBuilding(PlacedBuilding building)
        {
            placedBuildings.Add(building);
        }

        // Get a list of all PlacedBuilding instances
        public List<PlacedBuilding> GetAllPlacedBuildings()
        {
            return placedBuildings;
        }

        // Remove a specific PlacedBuilding from the list
        public void RemovePlacedBuilding(PlacedBuilding building)
        {
            placedBuildings.Remove(building);
        }

        // Method to get the PlacedBuilding instance by its associated PictureBox
        public PlacedBuilding GetPlacedBuilding(PictureBox pictureBox)
        {
            // Search through the list to find the matching PlacedBuilding
            foreach (var building in placedBuildings)
            {
                if (building.PlacedPictureBox == pictureBox)
                {
                    return building;
                }
            }

            // Return null if no matching PlacedBuilding is found
            return null;
        }

        // Example of iterating over all buildings and printing their progress
        public void PrintAllBuildingsProgress()
        {
            foreach (var building in placedBuildings)
            {
                MessageBox.Show($"Building progress: {building.CalculateDynamicBuildTime()} seconds");
            }
        }
    }

}
