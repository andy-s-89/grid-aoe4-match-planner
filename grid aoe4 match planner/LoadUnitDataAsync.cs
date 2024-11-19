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
        public List<CivUnitData> CivUnitDataList = new List<CivUnitData>();

        public async Task LoadUnitDataAsync()
        {
            // List of URLs to fetch JSON data from
            var urls = new List<string>
            {
                "https://raw.githubusercontent.com/aoe4world/data/main/units/abbasid.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/units/ayyubids.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/units/byzantines.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/units/chinese.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/units/delhi.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/units/english.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/units/french.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/units/hre.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/units/japanese.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/units/jeannedarc.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/units/malians.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/units/mongols.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/units/orderofthedragon.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/units/ottomans.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/units/rus.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/units/zhuxi.json"
            };

            var fetchTasks = urls.Select(url => FetchJsonFromGitHubAsync(url)).ToList();
            var jsonFiles = new List<string>();

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

            // Deserialize JSON files to CivUnitData objects
            CivUnitDataList = jsonFiles
                .Select(json => {
                    try
                    {
                        return JsonSerializer.Deserialize<CivUnitData>(json, jsonOptions);
                    }
                    catch (JsonException ex)
                    {
                        // Log or handle deserialization exception case
                        MessageBox.Show($"Error deserializing JSON data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null; // Return null to avoid incomplete list addition
                    }
                })
                .Where(civUnitData => civUnitData != null) // Filter out null values
                .ToList();
        }
    }
}

