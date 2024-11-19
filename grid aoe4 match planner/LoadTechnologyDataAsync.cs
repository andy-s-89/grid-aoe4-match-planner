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
        public List<CivTechnologyData> CivTechnologyDataList = new List<CivTechnologyData>();

        public async Task LoadTechnologyDataAsync()
        {
            // List of URLs to fetch JSON data from
            var urls = new List<string>
            {
                "https://raw.githubusercontent.com/aoe4world/data/main/technologies/abbasid.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/technologies/ayyubids.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/technologies/byzantines.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/technologies/chinese.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/technologies/delhi.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/technologies/english.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/technologies/french.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/technologies/hre.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/technologies/japanese.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/technologies/jeannedarc.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/technologies/malians.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/technologies/mongols.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/technologies/orderofthedragon.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/technologies/ottomans.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/technologies/rus.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/technologies/zhuxi.json"
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

            // Deserialize JSON files to CivTechnologyData objects
            CivTechnologyDataList = jsonFiles
                .Select(json => {
                    try
                    {
                        return JsonSerializer.Deserialize<CivTechnologyData>(json);
                    }
                    catch (JsonException ex)
                    {
                        // Log or handle deserialization exception case
                        MessageBox.Show($"Error deserializing JSON data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null; // Return null to avoid incomplete list addition
                    }
                })
                .Where(civTechnologyData => civTechnologyData != null) // Filter out null values
                .ToList();
        }
    }
}

