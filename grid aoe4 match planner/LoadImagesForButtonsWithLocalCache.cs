using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace grid_aoe4_match_planner
{
    public partial class Form1 : Form
    {
        public readonly HttpClient _httpClient = new HttpClient();
        public string localCacheDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ImageCache");

        private readonly string githubApiUrl = "https://api.github.com/repos/aoe4world/data/commits"; // Base URL for GitHub API
        private readonly string folderPath = "images"; // The path of the folder you're tracking

        public PictureBox[] pictureBoxesBuildings;
        public PictureBox[] pictureBoxesUnits;
        public PictureBox[] pictureBoxesUpgrades;
        public PictureBox[] pictureBoxesTechnologies;

        public Color buildingColor = Color.FromArgb(255, 52, 84, 105);
        public Color unitColor = Color.FromArgb(255, 130, 79, 52);
        public Color upgradeColor = Color.FromArgb(255, 50, 128, 107);
        public bool areImagesLoaded = false;

        // Adjust the method to save images to folders based on type
        public async Task CheckAndDownloadImagesAsync()
        {
            string repoUrl = "https://api.github.com/repos/aoe4world/data";
            Debug.WriteLineIf(DebugConfig.CheckAndDownloadImagesAsync, "\n///////////// CheckAndDownloadImagesAsync() called /////////////");

            var types = new[] { "buildings", "units", "upgrades", "technologies" };
            foreach (var type in types)
            {
                Debug.WriteLineIf(DebugConfig.CheckAndDownloadImagesAsync, $"Checking for changes in {type} folder...");

                string folderPath = $"images/{type}";
                var remoteFiles = await GetGitHubDirectoryContentsAsync(repoUrl, folderPath);

                var localFileShas = LoadLocalFileShas(type);
                if (HasFolderChanged(remoteFiles, localFileShas))
                {
                    Debug.WriteLineIf(DebugConfig.CheckAndDownloadImagesAsync, $"{type} folder contents have changed. Downloading updated files...");
                    foreach (var remoteFile in remoteFiles)
                    {
                        if (!localFileShas.TryGetValue(remoteFile.FileName, out string localSha) || localSha != remoteFile.Sha)
                        {
                            await SaveImageToCacheAsync(remoteFile.DownloadUrl, remoteFile.FileName, type);
                            localFileShas[remoteFile.FileName] = remoteFile.Sha;
                        }
                    }
                    SaveLocalFileShas(localFileShas, type);
                }
            }
            Debug.WriteLineIf(DebugConfig.CheckAndDownloadImagesAsync, "Finished CheckAndDownloadImagesAsync.");
        }


        public async Task<List<FileInfo>> GetGitHubDirectoryContentsAsync(string repoUrl, string folderPath)
        {
            Debug.WriteLineIf(DebugConfig.GetGitHubDirectoryContentsAsync, "\n///////////// GetGitHubDirectoryContentsAsync() called /////////////");
            try
            {
                Debug.WriteLineIf(DebugConfig.GetGitHubDirectoryContentsAsync, $"Fetching directory contents from GitHub: RepoUrl={repoUrl}, FolderPath={folderPath}");

                // Correct the full API URL for accessing contents
                string apiUrl = $"{repoUrl}/contents/{folderPath}?ref=main"; // Ensure we specify the correct branch
                Debug.WriteLineIf(DebugConfig.GetGitHubDirectoryContentsAsync, $"Constructed API URL: {apiUrl}");

                // Set up the request with User-Agent (required by GitHub)
                var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
                request.Headers.Add("User-Agent", "YourAppName");
                request.Headers.Authorization = new AuthenticationHeaderValue("Token", "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");

                Debug.WriteLineIf(DebugConfig.GetGitHubDirectoryContentsAsync, "Sending request to GitHub API...");

                // Send the request
                var response = await _httpClient.SendAsync(request);
                Debug.WriteLineIf(DebugConfig.GetGitHubDirectoryContentsAsync, $"Received response: {response.StatusCode}");

                // Check if the request was successful
                response.EnsureSuccessStatusCode(); // This will throw if the status is not successful

                Debug.WriteLineIf(DebugConfig.GetGitHubDirectoryContentsAsync, "Response was successful.");

                // Parse the response JSON
                string jsonResponse = await response.Content.ReadAsStringAsync();
                Debug.WriteLineIf(DebugConfig.GetGitHubDirectoryContentsAsync, "Parsing JSON...");

                var json = JArray.Parse(jsonResponse);
                var fileInfoList = new List<FileInfo>();

                // Loop through the files
                foreach (var file in json)
                {
                    if (file["type"].ToString() == "file")
                    {
                        string fileName = file["name"].ToString();
                        string fileSha = file["sha"].ToString();
                        string downloadUrl = file["download_url"].ToString();

                        fileInfoList.Add(new FileInfo(fileName, fileSha, downloadUrl));
                    }
                }

                return fileInfoList;
            }
            catch (Exception ex)
            {
                Debug.WriteLineIf(DebugConfig.GetGitHubDirectoryContentsAsync, $"Error fetching directory contents: {ex.Message}");
                MessageBox.Show($"Error fetching directory contents: {ex.Message}");
                return new List<FileInfo>();
            }
        }


        public class FileInfo
        {
            public string FileName { get; set; }
            public string Sha { get; set; }
            public string DownloadUrl { get; set; }

            public FileInfo(string fileName, string sha, string downloadUrl)
            {
                FileName = fileName;
                Sha = sha;
                DownloadUrl = downloadUrl;
            }
        }

        public bool HasFolderChanged(List<FileInfo> remoteFiles, Dictionary<string, string> localFileShas)
        {
            foreach (var remoteFile in remoteFiles)
            {
                if (!localFileShas.TryGetValue(remoteFile.FileName, out string localSha) || localSha != remoteFile.Sha)
                {
                    // File has changed or doesn't exist locally
                    return true;
                }
            }
            return false;
        }

        public async Task SaveImageToCacheAsync(string downloadUrl, string fileName, string folderType)
        {
            try
            {
                byte[] imageBytes = await _httpClient.GetByteArrayAsync(downloadUrl);
                using (var ms = new MemoryStream(imageBytes))
                {
                    var image = Image.FromStream(ms);

                    string folderPath = Path.Combine(localCacheDir, folderType);
                    Directory.CreateDirectory(folderPath); // Ensure type-specific directory exists
                    string cachePath = Path.Combine(folderPath, fileName);
                    image.Save(cachePath, ImageFormat.Png);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving image to cache: {ex.Message}");
            }
        }

        public void SaveLocalFileShas(Dictionary<string, string> fileShas, string folderType)
        {
            string json = JsonConvert.SerializeObject(fileShas);
            File.WriteAllText(Path.Combine(localCacheDir, $"{folderType}_cacheFileShas.json"), json);
        }

        public Dictionary<string, string> LoadLocalFileShas(string folderType)
        {
            string shaFilePath = Path.Combine(localCacheDir, $"{folderType}_cacheFileShas.json");
            if (!File.Exists(shaFilePath))
            {
                return new Dictionary<string, string>();
            }

            string json = File.ReadAllText(shaFilePath);
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }


        // Ensure local cache directory exists
        public void EnsureLocalCacheDirExists()
        {
            if (!Directory.Exists(localCacheDir))
            {
                Directory.CreateDirectory(localCacheDir);
            }
        }

        // Method to check if a local cache file exists
        public bool IsImageCached(string fileName, string folderType)
        {
            return File.Exists(GetLocalFilePath(fileName, folderType));
        }

        public string GetLocalFilePath(string fileName, string folderType)
        {
            return Path.Combine(localCacheDir, folderType, fileName);
        }

        public Image LoadImageFromCache(string fileName, string folderType)
        {
            return Image.FromFile(GetLocalFilePath(fileName, folderType));
        }

        // Save image to local cache and update its timestamp
        public void SaveImageToCache(Image image, string fileName, string folderType)
        {
            string folderPath = Path.Combine(localCacheDir, folderType);
            Directory.CreateDirectory(folderPath);
            string localFilePath = Path.Combine(folderPath, fileName);
            image.Save(localFilePath);
        }


        public async Task<bool> IsFolderChangedOnGithubAsync(string endFolder)
        {
            Debug.WriteLineIf(DebugConfig.IsFolderChangedOnGithubAsync, "\n///////////// IsFolderChangedOnGithubAsync() called /////////////");
            try
            {
                Debug.WriteLineIf(DebugConfig.IsFolderChangedOnGithubAsync, $"Checking if folder '{endFolder}' has changed on GitHub...");

                // GitHub API URL to get the latest commits for the specific folder
                string apiUrl = $"https://api.github.com/repos/aoe4world/data/commits?path=images/{endFolder}&sha=main";
                Debug.WriteLineIf(DebugConfig.IsFolderChangedOnGithubAsync, $"API URL for checking commits: {apiUrl}");

                // Set up the request
                var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
                request.Headers.Add("User-Agent", "YourApp"); // GitHub API requires a user-agent header
                Debug.WriteLineIf(DebugConfig.IsFolderChangedOnGithubAsync, "Request set up with User-Agent header.");

                // Send request to GitHub API
                Debug.WriteLineIf(DebugConfig.IsFolderChangedOnGithubAsync, "Sending request to GitHub API...");
                var response = await _httpClient.SendAsync(request);
                Debug.WriteLineIf(DebugConfig.IsFolderChangedOnGithubAsync, $"Received response: {response.StatusCode}");

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();
                Debug.WriteLineIf(DebugConfig.IsFolderChangedOnGithubAsync, "Response was successful.");

                // Parse the JSON response to get the latest commit
                string jsonResponse = await response.Content.ReadAsStringAsync();
                Debug.WriteLineIf(DebugConfig.IsFolderChangedOnGithubAsync, "Response content received. Parsing JSON...");

                var commits = JArray.Parse(jsonResponse);
                Debug.WriteLineIf(DebugConfig.IsFolderChangedOnGithubAsync, $"Parsed JSON. Number of commits found: {commits.Count}");

                if (commits.Count > 0)
                {
                    // Get the latest commit SHA for the folder
                    string latestCommitSha = commits[0]["sha"]?.ToString();
                    Debug.WriteLineIf(DebugConfig.IsFolderChangedOnGithubAsync, $"Latest commit SHA: {latestCommitSha}");

                    // Compare this commit SHA with a locally stored one (in a file, database, etc.)
                    string cacheFilePath = Path.Combine(localCacheDir, $"{endFolder}_lastCommitSha.txt");
                    Debug.WriteLineIf(DebugConfig.IsFolderChangedOnGithubAsync, $"Checking cache for stored commit SHA at: {cacheFilePath}");

                    if (File.Exists(cacheFilePath))
                    {
                        string lastStoredCommitSha = await File.ReadAllTextAsync(cacheFilePath);
                        Debug.WriteLineIf(DebugConfig.IsFolderChangedOnGithubAsync, $"Last stored commit SHA: {lastStoredCommitSha}");

                        if (lastStoredCommitSha == latestCommitSha)
                        {
                            Debug.WriteLineIf(DebugConfig.IsFolderChangedOnGithubAsync, "No changes detected in the folder (SHA matches).");
                            return false; // No changes
                        }
                    }

                    // Save the latest commit SHA for future comparisons
                    Debug.WriteLineIf(DebugConfig.IsFolderChangedOnGithubAsync, "Saving latest commit SHA to cache.");
                    await File.WriteAllTextAsync(cacheFilePath, latestCommitSha);
                    return true; // Folder has changed
                }

                Debug.WriteLineIf(DebugConfig.IsFolderChangedOnGithubAsync, "No commits found for the folder.");
                return false; // No commits found
            }
            catch (Exception ex)
            {
                Debug.WriteLineIf(DebugConfig.IsFolderChangedOnGithubAsync, $"Error checking folder changes: {ex.Message}");
                Debug.WriteLineIf(DebugConfig.IsFolderChangedOnGithubAsync, $"Stack Trace: {ex.StackTrace}");
                MessageBox.Show($"Error checking folder changes: {ex.Message}");
                return true; // Assume changes to avoid skipping updates in case of an error
            }
        }


        // Method to return all full filenames in github directory
        public async Task<string[]> GetImageFilenamesFromGithubFolderAsync(string endFolder)
        {
            string folderPath = $"https://raw.githubusercontent.com/aoe4world/data/main/images/{endFolder}/"; // Path to the folder in the repository
            string contentsFolder = $"images/{endFolder}";

            GitHubImageFetcher fetcher = new GitHubImageFetcher(contentsFolder, _httpClient);
            List<string> imageNames = await fetcher.GetSortedFileNamesAsync(true, true);
            List<string> imageUrlNames = imageNames
                                            .Select(x => folderPath + x)
                                            .ToList();
            string[] imageUrls = imageUrlNames.ToArray();

            return imageUrls;
        }

        // Generic method to load images for different types (buildings, units, etc.)
        public async Task<PictureBox[]> LoadImagesAsync(string folderType, Color imageColor)
        {
            try
            {
                EnsureLocalCacheDirExists();

                bool isFolderChanged = await IsFolderChangedOnGithubAsync(folderType);
                if (isFolderChanged)
                {
                    await CheckAndDownloadImagesAsync();
                }

                string[] imageUrls = await GetImageFilenamesFromGithubFolderAsync(folderType);

                List<Task<PictureBox?>> downloadTasks = new List<Task<PictureBox?>>();

                foreach (var url in imageUrls)
                {
                    string fileName = Path.GetFileName(url);
                    if (IsImageCached(fileName, folderType))
                    {
                        Image cachedImage = LoadImageFromCache(fileName, folderType);
                        var newFilename = Path.GetFileNameWithoutExtension(fileName);
                        downloadTasks.Add(Task.FromResult(CreatePictureBox(cachedImage, newFilename, imageColor)));
                    }
                    else
                    {
                        downloadTasks.Add(DownloadImageFromGitHubAsync(url, imageColor, fileName, folderType));
                    }
                }

                var downloadedImages = await Task.WhenAll(downloadTasks);
                return downloadedImages.Where(pb => pb != null).ToArray();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading images: {ex.Message}");
                MessageBox.Show($"Error loading images: {ex.Message}");
                return new PictureBox[0];
            }
        }


        // Updated method to download the image and cache it locally
        public async Task<PictureBox?> DownloadImageFromGitHubAsync(string imageUrl, Color color, string fileName, string folderType)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(imageUrl);
                if (response.IsSuccessStatusCode)
                {
                    byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();
                    using (var ms = new MemoryStream(imageBytes))
                    {
                        var image = Image.FromStream(ms);
                        SaveImageToCache(image, fileName, folderType);
                        var newFilename = Path.GetFileNameWithoutExtension(fileName);
                        return CreatePictureBox(image, newFilename, color);
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error downloading image: {ex.Message}");
                MessageBox.Show($"Error downloading image: {ex.Message}");
                return null;
            }
        }


        // Helper method to create PictureBox
        public PictureBox CreatePictureBox(Image image, string name, Color color)
        {
            var pictureBox = new PictureBox {
                Image = image,
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = color,
                Size = new Size(48, 48),
                Name = name
            };

            pictureBox.MouseDown += palettePictureBox_MouseDown;
            pictureBox.MouseMove += palettePictureBox_MouseMove;
            pictureBox.MouseUp += palettePictureBox_MouseUp;
            pictureBox.MouseEnter += palettePictureBox_MouseEnter;
            pictureBox.MouseLeave += palettePictureBox_MouseLeave;
            pictureBox.Paint += palettePictureBox_Paint;

            return pictureBox;
        }
    }
}

