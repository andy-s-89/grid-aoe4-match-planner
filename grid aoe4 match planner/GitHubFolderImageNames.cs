using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace grid_aoe4_match_planner
{
    public class GitHubImageFetcher
    {
        public readonly string folderPath;
        public readonly HttpClient _client;

        public GitHubImageFetcher(string folderPath, HttpClient client)
        {
            this.folderPath = folderPath;
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<List<string>> GetFileNamesAsync(bool includeExtensions = true, bool imageOnly = true)
        {
            List<string> fileNames = new List<string>();

            string url = $"https://api.github.com/repos/aoe4world/data/contents/{folderPath}";
            _client.DefaultRequestHeaders.Add("User-Agent", "C# App");

            HttpResponseMessage response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            JArray contentArray = JArray.Parse(json);

            foreach (JObject item in contentArray)
            {
                string type = item["type"].ToString();
                if (type == "file")
                {
                    string name = item["name"].ToString();
                    if (imageOnly)
                    {
                        if (name.EndsWith(".png") || name.EndsWith(".jpg") || name.EndsWith(".jpeg") || name.EndsWith(".gif"))
                        {
                            fileNames.Add(includeExtensions ? name : RemoveExtension(name));
                        }
                    }
                    else
                    {
                        fileNames.Add(includeExtensions ? name : RemoveExtension(name));
                    }
                }
            }

            return fileNames;
        }

        public async Task<List<string>> GetSortedFileNamesAsync(bool includeExtensions = true, bool imageOnly = true)
        {
            List<string> fileNames = await GetFileNamesAsync(includeExtensions, imageOnly);
            return SortFileNames(fileNames);
        }

        public string RemoveExtension(string fileName)
        {
            int lastDotIndex = fileName.LastIndexOf('.');
            if (lastDotIndex > 0)
            {
                return fileName.Substring(0, lastDotIndex);
            }
            return fileName;
        }

        public List<string> SortFileNames(List<string> fileNames)
        {
            var sortedFileNames = fileNames.OrderBy(name => {
                // Assign a priority value to each filename based on the presence of "-1", "-2", "-3", "-4"
                if (name.Contains("-1")) return 0;
                if (name.Contains("-2")) return 1;
                if (name.Contains("-3")) return 2;
                if (name.Contains("-4")) return 3;
                return 4; // Other filenames come after the special ones
            }).ThenBy(name => name) // Then sort alphabetically
            .ToList();

            return sortedFileNames;
        }
    }
}