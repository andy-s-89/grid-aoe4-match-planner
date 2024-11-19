using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace grid_aoe4_match_planner
{
    public partial class Form1 : Form
    {
        public List<CivBuildingData> CivBuildingDataList = new List<CivBuildingData>();

        public async Task LoadBuildingDataAsync()
        {
            // List of URLs to fetch JSON data from
            var urls = new List<string>
            {
                "https://raw.githubusercontent.com/aoe4world/data/main/buildings/abbasid.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/buildings/ayyubids.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/buildings/byzantines.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/buildings/chinese.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/buildings/delhi.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/buildings/english.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/buildings/french.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/buildings/hre.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/buildings/japanese.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/buildings/jeannedarc.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/buildings/malians.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/buildings/mongols.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/buildings/orderofthedragon.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/buildings/ottomans.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/buildings/rus.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/buildings/zhuxi.json"
            };

            var fetchTasks = urls.Select(url => FetchJsonFromGitHubAsync(url)).ToList();
            List<string> jsonFiles = new List<string>();

            // Error handling for JSON fetch
            foreach (var fetchTask in fetchTasks)
            {
                try
                {
                    var json = await fetchTask.ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(json))
                    {
                        jsonFiles.Add(json);
                    }
                    else
                    {
                        // Log or handle empty response case
                        MessageBox.Show($"Failed to fetch data from URL: {urls[fetchTasks.IndexOf(fetchTask)]}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    // Log or handle fetch exception case
                    MessageBox.Show($"Error fetching data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            // Deserialize JSON files to CivBuildingData objects
            CivBuildingDataList = jsonFiles
                .Select(json => {
                    try
                    {
                        return JsonSerializer.Deserialize<CivBuildingData>(json);
                    }
                    catch (JsonException ex)
                    {
                        // Log or handle deserialization exception case
                        MessageBox.Show($"Error deserializing JSON data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null; // Return null to avoid incomplete list addition
                    }
                })
                .Where(civBuildingData => civBuildingData != null) // Filter out null values
                .ToList();
        }
    }
}

