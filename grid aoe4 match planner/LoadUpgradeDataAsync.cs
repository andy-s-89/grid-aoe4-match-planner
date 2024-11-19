using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace grid_aoe4_match_planner
{
    public partial class Form1 : Form
    {
        public List<CivUpgradeData> CivUpgradeDataList = new List<CivUpgradeData>();

        public async Task LoadUpgradeDataAsync()
        {
            // List of URLs to fetch JSON data from
            var urls = new List<string>
            {
                "https://raw.githubusercontent.com/aoe4world/data/main/upgrades/abbasid.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/upgrades/ayyubids.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/upgrades/byzantines.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/upgrades/chinese.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/upgrades/delhi.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/upgrades/english.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/upgrades/french.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/upgrades/hre.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/upgrades/japanese.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/upgrades/jeannedarc.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/upgrades/malians.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/upgrades/mongols.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/upgrades/orderofthedragon.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/upgrades/ottomans.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/upgrades/rus.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/upgrades/zhuxi.json"
            };

            // Fetch all JSON files concurrently
            var fetchTasks = urls.Select(url => FetchJsonFromGitHubAsync(url)).ToList();

            try
            {
                // Await all fetch tasks and gather results
                var jsonFiles = await Task.WhenAll(fetchTasks);

                // Log each result for debugging
                for (int i = 0; i < urls.Count; i++)
                {
                    if (jsonFiles[i] == null)
                    {
                        Debug.WriteLine($"Failed to fetch JSON from: {urls[i]}");
                    }
                    else
                    {
                        Debug.WriteLine($"Successfully fetched JSON from: {urls[i]}");
                    }
                }

                // Deserialize each JSON string into its corresponding object
                CivUpgradeDataList = jsonFiles
                    .Where(json => !string.IsNullOrEmpty(json)) // Skip null or empty JSON strings
                    .Select(json => {
                        try
                        {
                            return JsonSerializer.Deserialize<CivUpgradeData>(json);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Failed to deserialize JSON: {ex.Message}");
                            return null; // Skip failed deserializations
                        }
                    })
                    .Where(data => data != null) // Filter out any null deserialization results
                    .ToList();

                Debug.WriteLine($"Loaded {CivUpgradeDataList.Count} CivUpgradeData entries successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred while loading upgrade data: {ex.Message}");
            }

        }
    }
}
