using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grid_aoe4_match_planner
{
    public static class DebugConfig
    {
        // Form1
        public static bool InitializeAsync { get; set; } = false;

        // Flag Events
        public static bool Flag_Click { get; set; } = false;

        // Custom tool tip form
        public static bool CountBuildingNames { get; set; } = false;
        public static bool SetToolTip2 { get; set; } = false;

        // Load images
        public static bool IsFolderChangedOnGithubAsync { get; set; } = false; 
        public static bool GetGitHubDirectoryContentsAsync { get; set; } = false;
        public static bool CheckAndDownloadImagesAsync { get; set; } = false;

        // Drawing Panel Events
        public static bool CheckLocationIsEmpty { get; set; } = false;
        public static bool PlaceBuildingInDrawingPanel { get; set; } = false;

        // Drawing Panel Picturebox Events
        public static bool drawingPanelPictureBox_MouseUp { get; set; } = false;
        public static bool drawingPanelPictureBox_Paint { get; set; } = false;

        // Palette Picturebox Events
        public static bool palettePictureBox_MouseEnter { get; set; } = false;

        // Placed Building Class
        public static bool UpdateVillagerAllocations { get; set; } = false;
        public static bool UpdateVillagerAllocationsFromGrid { get; set; } = false;
        public static bool GetVillagerAllocations { get; set; } = false;

        // Villager Allocation Panel
        public static bool Create { get; set; } = false;
        public static bool ShowPanel { get; set; } = false;

        // Unit Panel
        public static bool Populate { get; set; } = false;
        public static bool UnitUpgradePBox_Click { get; set; } = false;

    }
}
