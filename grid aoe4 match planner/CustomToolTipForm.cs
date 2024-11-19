using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;

namespace grid_aoe4_match_planner
{
    public class CustomToolTipForm : Form
    {
        public PictureBox pictureBox;
        public Label label;
        public Form1 parentForm;

        public Dictionary<string, Image> resourceImages = new Dictionary<string, Image>
        {
            { "food", Properties.Resources.food },
            { "wood", Properties.Resources.wood },
            { "gold", Properties.Resources.gold },
            { "stone", Properties.Resources.stone },
            { "oliveoil", Properties.Resources.oliveoil },
            { "popcap", Properties.Resources.popcap },
            { "time", Properties.Resources.time },
            { "total", Properties.Resources.total }
        };


        public CustomToolTipForm(Form1 parent)
        {
            this.parentForm = parent; // Reference to the main form (Form1)

            // Set form properties
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.BackColor = Color.LightYellow;
            this.Padding = new Padding(2);  // Add padding for the border
        }


        // Method to set the tooltip for a villager
        public void SetToolTipForVillager(Villager villager)
        {
            // Clear any previous controls
            this.Controls.Clear();

            // Create a main FlowLayoutPanel to arrange items vertically
            FlowLayoutPanel mainPanel = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown, // Arrange vertically
                WrapContents = false,
                Padding = new Padding(10) // Some padding around the panel
            };

            // Create a label for the villager's name, set it to bold and underline
            Label villagerNameLabel = new Label {
                Text = villager.Name,
                Font = new Font(this.Font, FontStyle.Bold | FontStyle.Underline),
                AutoSize = true,
                Padding = new Padding(0, 0, 0, 10) // Padding below the name for spacing
            };

            // Create a RichTextBox to display the villager's task history
            RichTextBox taskHistoryBox = new RichTextBox {
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = this.BackColor,
                Width = 200, // Set a reasonable minimum width for readability
                Height = 100, // Set a reasonable minimum height for readability
                Font = new Font(this.Font, FontStyle.Regular),
                Margin = new Padding(0, 10, 0, 0) // Spacing between elements
            };

            // Generate the task history string
            string taskHistoryDetails = string.Join(Environment.NewLine, villager.TaskHistory.Select(task => task.ToString()));
            taskHistoryBox.Text = taskHistoryDetails;

            // Add controls to the flowlayoutpanel in the right order
            mainPanel.Controls.Add(villagerNameLabel);
            mainPanel.Controls.Add(taskHistoryBox);

            this.Controls.Add(mainPanel);

            // Adjust the form size based on the content
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            // Set a minimum size for the form to prevent it from being too small
            this.MinimumSize = new Size(220, 150);
        }

        public void SetToolTipForVillagerWithGrid(Villager villager)
        {
            // Clear previous controls
            this.Controls.Clear();

            // Create a label for the villager's name and set it to bold and underlined
            Label villagerNameLabel = new Label {
                Text = villager.Name,
                Font = new Font(this.Font, FontStyle.Bold | FontStyle.Underline),
                AutoSize = true,
                Padding = new Padding(0, 0, 0, 10) // Padding below the name
            };

            // Add the villager's name to the tooltip form
            this.Controls.Add(villagerNameLabel);

            // Create and configure the DataGridView for task history
            DataGridView taskHistoryGrid = new DataGridView {
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Height = 200, // Adjust height based on your needs
                Width = this.Width - 10,
                BackgroundColor = this.BackColor, // Match the form's background
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
            };

            // Define columns for the DataGridView
            taskHistoryGrid.Columns.Add("StartTime", "Start Time");
            taskHistoryGrid.Columns.Add("TaskState", "Task");
            taskHistoryGrid.Columns.Add("DurationInSeconds", "Duration (s)");
            taskHistoryGrid.Columns.Add("Resource", "Resource");
            taskHistoryGrid.Columns.Add("Building", "Building");

            // Populate the DataGridView with TaskRecords from the villager's TaskHistory
            foreach (var task in villager.TaskHistory)
            {
                taskHistoryGrid.Rows.Add(
                    task.StartTime,
                    task.TaskState.ToString(),
                    task.DurationInSeconds == -1 ? "Indefinite" : task.DurationInSeconds.ToString(),
                    task.Resource.ToString(),
                    task.Building != null ? task.Building.BuildingInfo.name : "N/A"
                );
            }

            // Add the DataGridView to the form
            this.Controls.Add(taskHistoryGrid);

            // Auto-size the form based on content
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        }


        // Method to set tooltip data based on a list of PlacedBuilding objects
        public void SetToolTip2(List<PlacedBuilding> buildingList)
        {
            Debug.WriteLineIf(DebugConfig.SetToolTip2, "\n///////////// SetToolTip2() called /////////////");
            // Check if the buildingList is not null to prevent null reference exceptions
            if (buildingList != null)
            {
                Debug.WriteLineIf(DebugConfig.SetToolTip2, "SetToolTip2 called with a building list of count: " + buildingList.Count);

                if (buildingList.Count == 1)
                {
                    // Single building selection, extract information to show detailed costs for this specific building
                    Debug.WriteLineIf(DebugConfig.SetToolTip2, "Single building selected. Preparing single-selection tooltip.");

                    // Initialize a list to store building information
                    List<building> buildingInfoList = new List<building>();

                    // Get the first building from the list and its associated cost data
                    PlacedBuilding selectedBuilding = buildingList.First();
                    building buildingInfo = selectedBuilding.BuildingInfo;
                    costs buildingCosts = buildingInfo.costs;

                    Debug.WriteLineIf(DebugConfig.SetToolTip2, "Building selected: " + buildingInfo.name + " with ID: " + buildingInfo.id);

                    // Add building info to the list
                    buildingInfoList.Add(buildingInfo);

                    // Filter out costs that are zero to only show applicable resources
                    Dictionary<string, int> nonZeroCosts = BuildingUtilities.GetNonZeroCosts(buildingCosts);
                    Debug.WriteLineIf(DebugConfig.SetToolTip2, "Filtered non-zero costs for single building tooltip: " + string.Join(", ", nonZeroCosts.Select(kv => kv.Key + "=" + kv.Value)));

                    // Get a list of PictureBox controls for the counted buildings
                    List<PictureBox> countedBuildingPictureBoxes = CountBuildingNames(buildingInfoList, parentForm.pictureBoxesBuildings.ToList());
                    Debug.WriteLineIf(DebugConfig.SetToolTip2, "Counted building picture boxes for tooltip display: " + countedBuildingPictureBoxes.Count);

                    // Set up the tooltip to display the image and data for this single building
                    SetToolTipWithImages(nonZeroCosts, countedBuildingPictureBoxes);
                }
                else
                {
                    // Multi-building selection, display a summary of total resource costs for all buildings
                    Debug.WriteLineIf(DebugConfig.SetToolTip2, "Multiple buildings selected. Preparing multi-selection tooltip.");

                    // Initialize a list to accumulate building information for all selected buildings
                    List<building> buildingInfoList = new List<building>();

                    // Iterate through each selected building and add its BuildingInfo to the list
                    foreach (PlacedBuilding placedBuilding in buildingList)
                    {
                        buildingInfoList.Add(placedBuilding.BuildingInfo);
                        Debug.WriteLineIf(DebugConfig.SetToolTip2, "Added building to multi-selection: " + placedBuilding.BuildingInfo.name);
                    }

                    // Calculate summed non-zero costs across all buildings in the selection
                    Dictionary<string, int> nonZeroCosts = BuildingUtilities.GetSummedNonZeroCostsForBuildings(buildingInfoList);
                    Debug.WriteLineIf(DebugConfig.SetToolTip2, "Filtered summed non-zero costs for multi-selection tooltip: " + string.Join(", ", nonZeroCosts.Select(kv => kv.Key + "=" + kv.Value)));

                    // Get a list of PictureBox controls for the counted buildings
                    List<PictureBox> countedBuildingPictureBoxes = CountBuildingNames(buildingInfoList, parentForm.pictureBoxesBuildings.ToList());
                    Debug.WriteLineIf(DebugConfig.SetToolTip2, "Counted building picture boxes for tooltip display: " + countedBuildingPictureBoxes.Count);

                    // Set up the tooltip to display the images and aggregated data for the selected buildings
                    SetToolTipWithImages(nonZeroCosts, countedBuildingPictureBoxes);
                }
            }
            else
            {
                Debug.WriteLineIf(DebugConfig.SetToolTip2, "SetToolTip2 called with a null building list, exiting method.");
            }
        }


        public void SetToolTipWithImages(Dictionary<string, int> costs, List<PictureBox> groupOfSelectedPictureBoxes)
        {
            // Clear previous controls
            this.Controls.Clear();

            // Create a main FlowLayoutPanel to arrange items vertically
            FlowLayoutPanel mainPanel = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown, // Arrange vertically
                WrapContents = false,
                Padding = new Padding(10) // Some padding around the panel
            };

            // Panel to hold selected building picture boxes and names
            FlowLayoutPanel selectedBuildingsPanel = new FlowLayoutPanel {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight, // Arrange horizontally
                WrapContents = true,
                Padding = new Padding(10), // Padding around the selected buildings panel
                Margin = new Padding(0, 0, 0, 10) // Margin to separate from the main panel
            };

            // Add PictureBoxes and their names to the selectedBuildingsPanel
            foreach (var pb in groupOfSelectedPictureBoxes)
            {
                // Create a horizontal panel to hold picture box and its name
                FlowLayoutPanel buildingItemPanel = new FlowLayoutPanel {
                    AutoSize = true,
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = false,
                    Margin = new Padding(0, 5, 0, 5), // Some space between items
                    Padding = new Padding(0)
                };

                // Add the PictureBox to the panel
                buildingItemPanel.Controls.Add(pb);

                // Create a Label for the PictureBox's name
                Label nameLabel = new Label {
                    Text = pb.Name, // Assuming the PictureBox's Name property is set to the building name
                    AutoSize = true,
                    Font = new Font("Segoe UI", 14, FontStyle.Regular),
                    Padding = new Padding(5, 5, 5, 5),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                // Add the Label to the panel
                buildingItemPanel.Controls.Add(nameLabel);

                // Add the building item panel to the selectedBuildingsPanel
                selectedBuildingsPanel.Controls.Add(buildingItemPanel);
            }

            // Add the selectedBuildingsPanel to the main panel
            mainPanel.Controls.Add(selectedBuildingsPanel);

            // Calculate the maximum width of all images after scaling
            int maxImageWidth = resourceImages.Values.Max(img => (int)(img.Width * 0.5));

            // Iterate over each non-zero cost
            foreach (var cost in costs)
            {
                if (cost.Value > 0)
                {
                    // Create a horizontal FlowLayoutPanel for each image-label pair
                    FlowLayoutPanel itemPanel = new FlowLayoutPanel {
                        AutoSize = true,
                        FlowDirection = FlowDirection.LeftToRight, // Arrange horizontally
                        WrapContents = false,
                        Margin = new Padding(0, 5, 0, 5), // Some space between items
                        Padding = new Padding(0) // Ensure no additional padding inside the panel
                    };

                    // Initialize variables for PictureBox and Label
                    PictureBox pictureBox = null;
                    Label valueLabel = null;

                    // If there is an icon in resources for the cost, add it
                    if (resourceImages.ContainsKey(cost.Key))
                    {
                        // Get the image and calculate the new size (50% scale)
                        var image = resourceImages[cost.Key];
                        int scaledWidth = (int)(image.Width * 0.5);
                        int scaledHeight = (int)(image.Height * 0.5);

                        // Calculate left and right padding to center-align the image in maxImageWidth
                        int sidePadding = (maxImageWidth - scaledWidth) / 2;

                        // Create PictureBox for the resource image with scaled size and calculated side padding
                        pictureBox = new PictureBox {
                            Image = image,
                            SizeMode = PictureBoxSizeMode.Zoom,
                            Size = new Size(scaledWidth, scaledHeight),
                            Margin = new Padding(sidePadding, 0, sidePadding, 0) // Add equal left and right padding
                        };

                        // Add PictureBox to the item panel
                        itemPanel.Controls.Add(pictureBox);
                    }

                    // Initialize label's text
                    string labelText = cost.Key == "time" ? FormatSeconds(cost.Value) : cost.Value.ToString();

                    // Create Label for the resource value
                    valueLabel = new Label {
                        Text = labelText,
                        AutoSize = true, // Allow the label to auto-size
                        TextAlign = ContentAlignment.MiddleLeft,
                        Anchor = AnchorStyles.Left | AnchorStyles.Top, // Left align and top anchor
                        Font = new Font("Segoe UI", 16, FontStyle.Regular), // Increased font size
                        Padding = new Padding(0, 0, 0, 0), // No additional padding inside the label
                        Margin = new Padding(10, 0, 10, 0) // Fixed left margin for alignment
                    };

                    // Add Label to the item panel
                    itemPanel.Controls.Add(valueLabel);

                    // Adjust the height of the itemPanel to ensure proper vertical alignment
                    int pictureBoxHeight = pictureBox?.Height ?? 0;
                    int valueLabelHeight = valueLabel?.Height ?? 0;
                    itemPanel.Height = Math.Max(pictureBoxHeight, valueLabelHeight);

                    // Add the item panel to the main panel
                    mainPanel.Controls.Add(itemPanel);
                }
            }

            // Add the main panel to the Form
            this.Controls.Add(mainPanel);

            // Adjust the size of the form based on the content
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.FormBorderStyle = FormBorderStyle.None; // Optional, to remove form borders
        }


        public List<PictureBox> CountBuildingNames(List<building> selectedBuildingList, List<PictureBox> pictureBoxesBuildings)
        {
            Debug.WriteLineIf(DebugConfig.CountBuildingNames, "\n///////////// CountBuildingNames() called /////////////");
            // Group buildings by name, count occurrences, and keep the first ID for each group
            var groupedBuildings = selectedBuildingList
                .GroupBy(b => b.name)
                .Select(group => new {
                    Name = group.Key,               // Building name
                    Count = group.Count(),          // Count of buildings with the same name
                    Id = group.First().baseId       // Take the first base ID from the group
                })
                .ToList();

            Debug.WriteLineIf(DebugConfig.CountBuildingNames, "Grouped buildings created, total unique groups: " + groupedBuildings.Count);

            // List to store modified PictureBox objects
            List<PictureBox> groupOfSelectedPictureBoxes = new List<PictureBox>();

            // Iterate through each group of buildings
            foreach (var group in groupedBuildings)
            {
                Debug.WriteLineIf(DebugConfig.CountBuildingNames, "Processing group: " + group.Name + " with count: " + group.Count);

                // Find the matching PictureBox in the provided list
                var matchingPictureBox = pictureBoxesBuildings.FirstOrDefault(pb => pb.Name == group.Id);
                if (matchingPictureBox == null)
                {
                    Debug.WriteLineIf(DebugConfig.CountBuildingNames, "No matching PictureBox found for building ID: " + group.Id);
                    continue;
                }

                // Clone the PictureBox for modifications
                var clonedPictureBox = ClonePictureBox(matchingPictureBox);
                if (clonedPictureBox == null || clonedPictureBox.Image == null)
                {
                    Debug.WriteLineIf(DebugConfig.CountBuildingNames, "Cloning failed or PictureBox image missing for building: " + group.Name);
                    continue;
                }

                Debug.WriteLineIf(DebugConfig.CountBuildingNames, "Cloned PictureBox for building: " + group.Name);

                // Create a copy of the image to add count text
                Bitmap imageWithCount = new Bitmap(clonedPictureBox.Image);

                using (Graphics g = Graphics.FromImage(imageWithCount))
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                    // Set up font for the count text
                    Font font = new Font("Arial", 20, FontStyle.Regular);
                    string text = group.Count.ToString();

                    // Calculate the text position in the bottom-right corner
                    SizeF textSize = g.MeasureString(text, font);
                    PointF textPosition = new PointF(imageWithCount.Width - textSize.Width - 5, imageWithCount.Height - textSize.Height - 5);

                    // Draw the count text with an outline and fill
                    using (GraphicsPath path = new GraphicsPath())
                    {
                        path.AddString(text, font.FontFamily, (int)FontStyle.Bold, g.DpiY * font.Size / 72, textPosition, StringFormat.GenericDefault);

                        // Draw outline of the text
                        using (Pen outlinePen = new Pen(Color.Black, 5)) // Outline thickness
                        {
                            g.DrawPath(outlinePen, path);
                            Debug.WriteLineIf(DebugConfig.CountBuildingNames, "Text outline drawn for building: " + group.Name);
                        }

                        // Fill the text with white color
                        using (Brush fillBrush = new SolidBrush(Color.White))
                        {
                            g.FillPath(fillBrush, path);
                            Debug.WriteLineIf(DebugConfig.CountBuildingNames, "Text filled for building: " + group.Name);
                        }
                    }
                }

                // Update the PictureBox with the modified image and assign the group name
                clonedPictureBox.Image = imageWithCount;
                clonedPictureBox.Name = group.Name;

                // Add to the list of PictureBoxes with the updated count image
                groupOfSelectedPictureBoxes.Add(clonedPictureBox);
                Debug.WriteLineIf(DebugConfig.CountBuildingNames, "Added PictureBox with count overlay to the final list for building: " + group.Name);
            }

            Debug.WriteLineIf(DebugConfig.CountBuildingNames, "Total PictureBoxes prepared with count overlays: " + groupOfSelectedPictureBoxes.Count);
            return groupOfSelectedPictureBoxes;
        }


        public PictureBox ClonePictureBox(PictureBox original)
        {
            PictureBox clone = new PictureBox {
                Image = original.Image,
                Size = original.Size,
                SizeMode = original.SizeMode,
                Margin = original.Margin,
                Padding = original.Padding,
                Anchor = original.Anchor,
                BackColor = original.BackColor,
                BorderStyle = original.BorderStyle,
                Name = original.Name
            };

            // Copy other relevant properties if needed
            return clone;
        }


        public string FormatSeconds(int totalSeconds)
        {
            // Calculate minutes and seconds
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            // Format minutes and seconds into "M:SS" format
            return $"{minutes}:{seconds:D2}";
        }


        // Override the OnPaint method to draw a custom border
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (Pen pen = new Pen(Color.Black, 2)) // Custom border color and width
            {
                e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
            }
        }


        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Intercept key presses
            if (keyData == Keys.Escape)
            {
                // Hide the custom tooltip
                this.Hide();

                // Switch focus back to Form1
                parentForm.Focus();

                // Call the Escape key handler in Form1
                parentForm.HandleEscapeKey();

                return true; // Indicate that the key press was handled
            }
            else if (keyData == Keys.Delete || keyData == Keys.Back)
            {
                // Hide the custom tooltip
                this.Hide();

                // Switch focus back to Form1
                parentForm.Focus();

                // Call the Delete/Backspace key handler in Form1
                parentForm.HandleDeleteOrBackspaceKey();

                return true; // Indicate that the key press was handled
            }

            // Call the base class's ProcessCmdKey to handle other keys normally
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }

}
