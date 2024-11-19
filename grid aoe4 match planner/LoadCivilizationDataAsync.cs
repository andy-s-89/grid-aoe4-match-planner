using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace grid_aoe4_match_planner
{
    public partial class Form1 : Form
    {
        public List<CivilizationData> civilizationDataList;
        public bool hasDataLoaded = false;

        // Deserialize each JSON string into its corresponding object
        JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            Converters =
            {
                new IntOrDoubleConverter(),
                new DoubleOrIntConverter() // Add any other converters as needed}
            }
        };

        public async Task LoadDataAsync()
        {
            try
            {
                // Start all data loading tasks in parallel
                var loadCivilizationDataTask = LoadCivilizationDataAsync();
                var loadAbilityDataTask = LoadAbilityDataAsync();
                var loadBuildingDataTask = LoadBuildingDataAsync();
                var loadTechnologyDataTask = LoadTechnologyDataAsync();
                var loadUnitDataTask = LoadUnitDataAsync();
                var loadUpgradeDataTask = LoadUpgradeDataAsync();

                // Wait for all tasks to complete
                await Task.WhenAll(
                    loadCivilizationDataTask,
                    loadAbilityDataTask,
                    loadBuildingDataTask,
                    loadTechnologyDataTask,
                    loadUnitDataTask,
                    loadUpgradeDataTask
                );

                // Set hasDataLoaded to true if all tasks complete successfully
                hasDataLoaded = true;
            }
            catch (Exception ex)
            {
                // Log or handle any exceptions in the entire loading process
                Debug.WriteLine($"An error occurred while loading data: {ex.Message}");
                hasDataLoaded = false; // In case of an error, data is not considered loaded
            }

        }

        public async Task LoadCivilizationDataAsync()
        {
            var urls = new List<string>
            {
                "https://raw.githubusercontent.com/aoe4world/data/main/civilizations/abbasid.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/civilizations/ayyubids.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/civilizations/byzantines.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/civilizations/chinese.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/civilizations/delhi.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/civilizations/english.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/civilizations/french.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/civilizations/hre.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/civilizations/japanese.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/civilizations/jeannedarc.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/civilizations/malians.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/civilizations/mongols.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/civilizations/orderofthedragon.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/civilizations/ottomans.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/civilizations/rus.json",
                "https://raw.githubusercontent.com/aoe4world/data/main/civilizations/zhuxi.json"
            };

            var fetchTasks = urls.Select(url => FetchJsonFromGitHubAsync(url)).ToList();
            var jsonFiles = await Task.WhenAll(fetchTasks);

            civilizationDataList = jsonFiles
                .Where(json => json != null) // Ensure only non-null results are processed
                .Select(json => JsonSerializer.Deserialize<CivilizationData>(json, jsonOptions))
                .ToList();
        }

        public async Task<string> FetchJsonFromGitHubAsync(string url)
        {
            try
            {
                return await _httpClient.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to fetch data from {url}: {ex.Message}");
                return null;
            }
        }
    }
}

