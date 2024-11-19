using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace grid_aoe4_match_planner
{
    public static class BuildingUtilities
    {
        // Method to get non-zero costs using reflection
        public static Dictionary<string, int> GetNonZeroCosts(costs costs)
        {
            // Define the set of relevant resource keys
            var resourceKeys = new HashSet<string> { "food", "wood", "stone", "gold", "vizier", "oliveoil" };

            // Get non-zero costs from the 'costs' object
            var nonZeroCosts = typeof(costs)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance) // Only public instance properties
                .Where(prop => prop.PropertyType == typeof(int)) // Ensure the property is of type int
                .Where(prop => (int)prop.GetValue(costs) != 0) // Filter out properties with value 0
                .ToDictionary(prop => prop.Name, prop => (int)prop.GetValue(costs)); // Convert to Dictionary

            // Count how many of the resource keys are non-zero
            int nonZeroResourceCount = nonZeroCosts.Keys.Count(key => resourceKeys.Contains(key));

            // If there is more than one resource cost left, keep "total"; otherwise, remove it
            if (nonZeroResourceCount <= 1)
            {
                nonZeroCosts.Remove("total");
            }

            return nonZeroCosts;
        }

        // Method to get the summed non-zero costs for a list of buildings
        public static Dictionary<string, int> GetSummedNonZeroCostsForBuildings(List<building> buildings)
        {
            // Initialize a dictionary to hold the summed costs
            Dictionary<string, int> summedCosts = new Dictionary<string, int>
            {
                { "food", 0 },
                { "wood", 0 },
                { "stone", 0 },
                { "gold", 0 },
                { "vizier", 0 },
                { "oliveoil", 0 },
                { "total", 0 },
                { "popcap", 0 },
                { "time", 0 }
            };

            // Sum up costs for all buildings
            foreach (var building in buildings)
            {
                foreach (var prop in typeof(costs).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (prop.PropertyType == typeof(int))
                    {
                        string propName = prop.Name;
                        int propValue = (int)prop.GetValue(building.costs);

                        // Add the value to the corresponding summedCosts key
                        if (summedCosts.ContainsKey(propName))
                        {
                            summedCosts[propName] += propValue;
                        }
                    }
                }
            }

            // Filter out only non-zero costs
            var nonZeroCosts = summedCosts
                .Where(kvp => kvp.Value != 0)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            // Define the resource keys to check
            var resourceKeys = new HashSet<string> { "food", "wood", "stone", "gold", "vizier", "oliveoil" };

            // Count how many of the resource keys are left in the filtered non-zero costs
            int nonZeroResourceCount = nonZeroCosts.Keys.Count(key => resourceKeys.Contains(key));

            // If there is more than one resource cost (food, wood, stone, etc.) left, include "total"
            if (nonZeroResourceCount <= 1)
            {
                nonZeroCosts.Remove("total");
            }

            return nonZeroCosts;
        }

    }
}
