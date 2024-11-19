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
        public List<CivAbilityData> CivAbilityDataList = new List<CivAbilityData>();

        public async Task LoadAbilityDataAsync()
        {
            // List of URLs to fetch JSON data from
            var urls = new List<string>
            {
                "https://raw.githubusercontent.com/aoe4world/data/main/abilities/abbasid.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/abilities/ayyubids.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/abilities/byzantines.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/abilities/chinese.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/abilities/delhi.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/abilities/english.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/abilities/french.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/abilities/hre.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/abilities/japanese.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/abilities/jeannedarc.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/abilities/malians.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/abilities/mongols.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/abilities/orderofthedragon.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/abilities/ottomans.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/abilities/rus.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/abilities/zhuxi.json"
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

            // Deserialize JSON files to CivAbilityData objects
            CivAbilityDataList = jsonFiles
                .Select(json => {
                    try
                    {
                        return JsonSerializer.Deserialize<CivAbilityData>(json);
                    }
                    catch (JsonException ex)
                    {
                        // Log or handle deserialization exception case
                        MessageBox.Show($"Error deserializing JSON data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null; // Return null to avoid incomplete list addition
                    }
                })
                .Where(civAbilityData => civAbilityData != null) // Filter out null values
                .ToList();
        }
    }
}

