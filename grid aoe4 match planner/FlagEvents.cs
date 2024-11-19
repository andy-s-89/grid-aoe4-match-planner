using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace grid_aoe4_match_planner
{

    public partial class Form1
    {
        public string[] ListOfCivs = new string[] {"abbasid","ayyubids","byzantines","chinese","delhi",
                                        "english","french","hre","japanese","jeannedarc","malians",
                                        "mongols","orderofthedragon","ottomans","rus","zhuxi"};
        public List<string> listOfBuildings;
        public int chosenCivRef;
        public string chosenCivName = "";

        public void Flag_Click(object sender, EventArgs e)
        {
            Debug.WriteLineIf(DebugConfig.Flag_Click, "\n///////////// Flag_Click() called /////////////");
            // Check if images are loaded before proceeding
            if (areImagesLoaded)
            {
                Debug.WriteLineIf(DebugConfig.Flag_Click, "Images are loaded. Processing flag click event.");

                // Retrieve the PictureBox that triggered the event
                PictureBox pictureBox = sender as PictureBox;
                if (pictureBox == null)
                {
                    Debug.WriteLineIf(DebugConfig.Flag_Click, "Error: sender is not a PictureBox.");
                    return;
                }

                // Get civilization name from PictureBox name
                string civName = pictureBox.Name;
                Debug.WriteLineIf(DebugConfig.Flag_Click, $"Selected civilization: {civName}");

                // Find the index of the selected civilization in the ListOfCivs array
                chosenCivRef = Array.IndexOf(ListOfCivs, civName);
                if (chosenCivRef == -1)
                {
                    Debug.WriteLineIf(DebugConfig.Flag_Click, "Error: Civilization not found in ListOfCivs.");
                    return;
                }

                // Set the chosen civilization name and retrieve its data
                chosenCivName = ListOfCivs[chosenCivRef];
                Debug.WriteLineIf(DebugConfig.Flag_Click, $"Chosen civilization name: {chosenCivName}");

                var pickedCiv = civilizationDataList[chosenCivRef];
                listOfBuildings = pickedCiv.techtree.Keys.ToList();

                // Remove the specific building if it exists in the list
                listOfBuildings.Remove("capital-town-center-1");
                Debug.WriteLineIf(DebugConfig.Flag_Click, "List of buildings loaded, excluding 'capital-town-center-1'.");

                // Create a list to store ordered PictureBoxes for each building
                List<PictureBox> orderedPictureBoxes = new List<PictureBox>();

                // Iterate through each building name in listOfBuildings
                foreach (var buildingName in listOfBuildings)
                {
                    // Find PictureBox with the matching name in pictureBoxesBuildings
                    var picBox = pictureBoxesBuildings.FirstOrDefault(pBox => pBox.Name == buildingName);

                    if (picBox == null)
                    {
                        Debug.WriteLineIf(DebugConfig.Flag_Click, $"Warning: PictureBox for building '{buildingName}' not found.");
                    }
                    else
                    {
                        Debug.WriteLineIf(DebugConfig.Flag_Click, $"PictureBox for building '{buildingName}' found and added.");
                    }

                    // Add the found PictureBox (or null if not found) to the ordered list
                    orderedPictureBoxes.Add(picBox);
                }

                // Convert the ordered PictureBoxes to an array if needed for flow layout
                PictureBox[] orderedPictureBoxArray = orderedPictureBoxes.ToArray();
                Debug.WriteLineIf(DebugConfig.Flag_Click, "Ordered PictureBox array created.");

                // Suspend layout changes in flowLayoutPanel to improve performance during batch updates
                flowLayoutPanel.SuspendLayout();
                Debug.WriteLineIf(DebugConfig.Flag_Click, "flowLayoutPanel layout suspended.");

                // Clear existing controls in the flowLayoutPanel
                flowLayoutPanel.Controls.Clear();
                Debug.WriteLineIf(DebugConfig.Flag_Click, "flowLayoutPanel controls cleared.");

                // Add the ordered PictureBox array to the flowLayoutPanel
                flowLayoutPanel.Controls.AddRange(orderedPictureBoxArray);
                Debug.WriteLineIf(DebugConfig.Flag_Click, "Ordered PictureBoxes added to flowLayoutPanel.");

                // Resume layout changes in flowLayoutPanel
                flowLayoutPanel.ResumeLayout();
                Debug.WriteLineIf(DebugConfig.Flag_Click, "flowLayoutPanel layout resumed.");

                // Attempt to add initial villager images to the drawing panel
                try
                {
                    AddStartingVillsToDrawingPanel();
                    Debug.WriteLineIf(DebugConfig.Flag_Click, "Starting villagers added to drawing panel.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLineIf(DebugConfig.Flag_Click, $"Error adding starting villagers: {ex.Message}");
                }

                // Attempt to add the starting town center to the drawing panel
                try
                {
                    AddStartingTCToDrawingPanel();
                    Debug.WriteLineIf(DebugConfig.Flag_Click, "Starting town center added to drawing panel.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLineIf(DebugConfig.Flag_Click, $"Error adding starting town center: {ex.Message}");
                }

                // Trigger a redraw of the drawing panel
                drawingPanel.Invalidate();
                Debug.WriteLineIf(DebugConfig.Flag_Click, "drawingPanel invalidated for redraw.");
            }
            else
            {
                Debug.WriteLineIf(DebugConfig.Flag_Click, "Images not loaded. Flag click event ignored.");
            }
        }


        private void AddStartingTCToDrawingPanel()
        {
            var picBox = pictureBoxesBuildings.FirstOrDefault(pBox => pBox.Name == "capital-town-center");

            var buildingInfo = CivBuildingDataList[chosenCivRef].data.FirstOrDefault(b => b.baseId == picBox.Name);

            // Clone PictureBox to avoid removing it from the original parent
            if (picBox != null && buildingInfo != null)
            {

                PictureBox clonedPicBox = new PictureBox {
                    Name = picBox.Name,
                    Image = picBox.Image,
                    Size = new Size(globalGridSize, globalGridSize),
                    SizeMode = picBox.SizeMode,
                    BackColor = picBox.BackColor,
                    Margin = new Padding(0),
                    Padding = new Padding(0)
                };

                // Attach necessary events to cloned PictureBox
                clonedPicBox.MouseDown += drawingPanelPictureBox_MouseDown;
                clonedPicBox.MouseMove += drawingPanelPictureBox_MouseMove;
                clonedPicBox.MouseUp += drawingPanelPictureBox_MouseUp;
                clonedPicBox.MouseEnter += drawingPanelPictureBox_MouseEnter;
                clonedPicBox.MouseLeave += drawingPanelPictureBox_MouseLeave;
                clonedPicBox.Paint += drawingPanelPictureBox_Paint;
                clonedPicBox.Show();

                // Create and configure PlacedBuilding instance
                PlacedBuilding bldg = placedBuildingManager.CreateBuilding(buildingInfo, -gridTime, 6);
                bldg.AddPictureBox(clonedPicBox);
                clonedPicBox.Location = new Point(6 * globalGridSize + translationOffsetX, drawingPanel.Height - 2 * globalGridSize + translationOffsetY);

                // Add PictureBox to drawingPanel and configure PlacedBuilding
                drawingPanel.Controls.Add(clonedPicBox);
                placedBuildingManager.AddPlacedBuilding(bldg);
                bldg.IsLocked = true;
                bldg.TimeStarted = 0;
                bldg.TimeFinished = 0;
                bldg.ShowDimLines = false;
            }
            else
            {
                Debug.WriteLine("Error: Could not find the starting town center or its building information.");
            }
        }

        public void AddStartingVillsToDrawingPanel()
        {
            var picBox = pictureBoxesUnits.FirstOrDefault(pBox => pBox.Name == "villager-1");

            // Clone PictureBox to avoid removing it from the original parent
            if (picBox != null)
            {
                foreach (var villager in villagerManager.Villagers)
                {
                    PictureBox clonedPicBox = new PictureBox {
                        Name = picBox.Name,
                        Image = picBox.Image,
                        Size = new Size(globalGridSize, globalGridSize),
                        SizeMode = picBox.SizeMode,
                        BackColor = picBox.BackColor,
                        Margin = new Padding(0),
                        Padding = new Padding(0)
                    };


                    clonedPicBox.Location = new Point(villager.Column * globalGridSize + translationOffsetX, drawingPanel.Height - 2 * globalGridSize + translationOffsetY);
                    clonedPicBox.MouseHover += ItemPictureBox_MouseHover; // Assign the hover event
                    drawingPanel.Controls.Add(clonedPicBox);
                    villager.AddPictureBox(clonedPicBox);
                    villager.ShowDimLines = false;
                }
            }
        }
    }
}
