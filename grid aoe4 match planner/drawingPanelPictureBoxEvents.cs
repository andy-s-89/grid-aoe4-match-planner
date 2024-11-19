using System;
using System.Diagnostics;
using System.Drawing;
using System.Text.Json;
using System.Windows.Forms;
using static System.Net.WebRequestMethods;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace grid_aoe4_match_planner
{
    public partial class Form1 : Form
    {
        public bool isDragging = false;
        public bool wasDragged = false; // Flag to check if the PictureBox was dragged
        public Point dragStartPoint;        
        public Point originalPictureBoxLocation;


        public void drawingPanelPictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;

            // Ensure sender is a PictureBox
            if (sender is not PictureBox pb)
            {
                Debug.WriteLine("Error: sender is not a PictureBox.");
                return;
            }

            if (customToolTip != null)
            {
                customToolTip.Hide();
            }

            PlacedBuilding bldg = placedBuildingManager.GetPlacedBuilding(pictureBox);

            if (bldg == null)
            {
                Debug.WriteLine("Error: can't find building that matches picturebox.");
                return;
            }

            if (bldg.IsLocked == false)
            {
                if (e.Button == MouseButtons.Left)
                {
                    isDragging = true;
                    wasDragged = false; // Reset the flag on mouse down

                    // Use absolute mouse position relative to drawing panel
                    dragStartPoint = drawingPanel.PointToClient(Cursor.Position);

                    // Store the original location of the PictureBox to calculate delta later
                    originalPictureBoxLocation = pictureBox.Location;
                }
                else if (e.Button == MouseButtons.Right)
                {
                    if (pasteMode)
                    {
                        SetAllPictureBoxTagsToTransparent(flowLayoutPanel);
                        pasteMode = false;
                    }
                    else
                    {
                        // Right click: Remove PictureBox and class instance from drawingPanel
                        placedBuildingManager.RemovePlacedBuilding(bldg);
                        bldg.Dispose();
                    }
                }
            }
        }

        public void drawingPanelPictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            // Ensure sender is a PictureBox
            if (sender is not PictureBox pb)
            {
                Debug.WriteLine("Error: sender is not a PictureBox.");
                return;
            }

            if (isDragging)
            {
                PictureBox pictureBox = sender as PictureBox;
                PlacedBuilding bldg = placedBuildingManager.GetPlacedBuilding(pictureBox);

                // Unlock picturebox location from being tied to TimePlaced during drag
                bldg.IsDragged = true;

                // Get current mouse position relative to drawing panel
                Point currentMousePosition = drawingPanel.PointToClient(Cursor.Position);

                // Calculate the new location based on the difference between start and current position
                Point newLocation = new Point(
                    originalPictureBoxLocation.X + (currentMousePosition.X - dragStartPoint.X),
                    originalPictureBoxLocation.Y + (currentMousePosition.Y - dragStartPoint.Y)
                );

                // Update the PictureBox location
                pictureBox.Location = newLocation;
                wasDragged = true;
            }

            drawingPanel.Invalidate();
        }


        public void drawingPanelPictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
            PictureBox pictureBox = sender as PictureBox;

            // Ensure sender is a PictureBox
            if (sender is not PictureBox pb)
            {
                Debug.WriteLineIf(DebugConfig.drawingPanelPictureBox_MouseUp, "Error: sender is not a PictureBox.");
                return;
            }

            // Check if PictureBox is valid
            Debug.WriteLineIf(DebugConfig.drawingPanelPictureBox_MouseUp, $"Mouse up on PictureBox: {pictureBox?.Name ?? "null"}");

            // Get the associated PlacedBuilding
            PlacedBuilding bldg = placedBuildingManager.GetPlacedBuilding(pictureBox);

            if (bldg != null)
            {
                Debug.WriteLineIf(DebugConfig.drawingPanelPictureBox_MouseUp, $"PlacedBuilding found: Column {bldg.Column}, TimePlaced {bldg.TimePlaced}, TimeStarted {bldg.TimeStarted}, TimeFinished {bldg.TimeFinished}");

                if (e.Button == MouseButtons.Left && !wasDragged)
                {
                    Debug.WriteLineIf(DebugConfig.drawingPanelPictureBox_MouseUp, "Left mouse button clicked without dragging.");
                    DeselectAllPictureBoxes();
                    Debug.WriteLineIf(DebugConfig.drawingPanelPictureBox_MouseUp, "Deselecting all PictureBoxes.");
                    bldg.IsSelected = true;
                    bldg.PopupButtonManager.Show();
                    Debug.WriteLineIf(DebugConfig.drawingPanelPictureBox_MouseUp, "PlacedBuilding IsSelected set to true. Showing menu buttons.");
                }

                if (e.Button == MouseButtons.Left && wasDragged)
                {
                    // Save old TimePlaced
                    double oldTimePlaced = bldg.TimePlaced;
                    Debug.WriteLineIf(DebugConfig.drawingPanelPictureBox_MouseUp, $"Old TimePlaced: {oldTimePlaced}");

                    // Snap to grid when drag is complete
                    Point mousePosition = drawingPanel.PointToClient(Cursor.Position);
                    Point timePlacedPoint = new Point(mousePosition.X, pictureBox.Location.Y + pictureBox.Height);
                    Point gridCell = GetGridCellTime(timePlacedPoint);

                    Debug.WriteLineIf(DebugConfig.drawingPanelPictureBox_MouseUp, $"Mouse Position: {mousePosition}, TimePlacedPoint: {timePlacedPoint}, GridCell: {gridCell}");

                    // Update Column and TimePlaced
                    bldg.Column = gridCell.X;
                    bldg.TimePlaced = gridCell.Y;

                    // Log the updated values
                    Debug.WriteLineIf(DebugConfig.drawingPanelPictureBox_MouseUp, $"New TimePlaced: {bldg.TimePlaced}, Column: {bldg.Column}");

                    // Calculate the time difference between old and new TimePlaced
                    double timeDiffInSeconds = bldg.TimePlaced - oldTimePlaced;
                    Debug.WriteLineIf(DebugConfig.drawingPanelPictureBox_MouseUp, $"Time difference in seconds: {timeDiffInSeconds}");

                    // Only adjust TimeStarted if it was not changed manually via DateTimePicker
                    if (bldg.SettingsPanelManager.settingsPanelContent != null && !bldg.SettingsPanelManager.settingsPanelContent._isUpdatingTimeStarted)
                    {
                        if (bldg.TimeStarted.HasValue)
                        {
                            bldg.TimeStarted += timeDiffInSeconds;
                            Debug.WriteLineIf(DebugConfig.drawingPanelPictureBox_MouseUp, $"Updated TimeStarted: {bldg.TimeStarted}");
                        }
                    }

                    // Adjust TimeFinished by the same difference, but ONLY if they have been set
                    if (bldg.TimeFinished.HasValue)
                    {
                        bldg.TimeFinished += timeDiffInSeconds;
                        Debug.WriteLineIf(DebugConfig.drawingPanelPictureBox_MouseUp, $"Updated TimeFinished: {bldg.TimeFinished}");
                    }

                    // Update villager allocations based on the time difference
                    bldg.UpdateVillagerAllocations(timeDiffInSeconds);

                    // Check and update the settings popup if it's open
                    if (bldg.SettingsPanelManager.settingsPanelContent != null)
                    {
                        Debug.WriteLineIf(DebugConfig.drawingPanelPictureBox_MouseUp, "Updating settings popup...");
                        bldg.SettingsPanelManager.settingsPanelContent._isUpdating = true; // Prevent ValueChanged event from firing
                        bldg.SettingsPanelManager.settingsPanelContent._oldTimeStarted = bldg.TimeStarted ?? 0; // Ensure oldTimeStarted is reset
                        bldg.SettingsPanelManager.settingsPanelContent.UpdateDateTimePickers();
                        bldg.SettingsPanelManager.settingsPanelContent._isUpdating = false;
                        Debug.WriteLineIf(DebugConfig.drawingPanelPictureBox_MouseUp, "Settings popup updated.");
                    }

                    // Update PictureBox to snap to grid
                    pictureBox.Location = new Point(
                        bldg.Column * globalGridSize + translationOffsetX,
                        drawingPanel.Height - (int)((bldg.TimePlaced / gridTime) * globalGridSize)
                        - translationOffsetY - globalGridSize
                    );

                    Debug.WriteLineIf(DebugConfig.drawingPanelPictureBox_MouseUp, $"PictureBox snapped to new location: {pictureBox.Location}");
                }

                // Reset dragging flags
                bldg.IsDragged = false;
            }
            else
            {
                Debug.WriteLineIf(DebugConfig.drawingPanelPictureBox_MouseUp, "PlacedBuilding not found for PictureBox.");
            }

            wasDragged = false; // Reset the drag flag
            Debug.WriteLineIf(DebugConfig.drawingPanelPictureBox_MouseUp, "Mouse up handling complete.\n");
        }




        public void drawingPanelPictureBox_MouseEnter(object sender, EventArgs e)
        {
            // Ensure sender is a PictureBox
            if (sender is not PictureBox pb)
            {
                Debug.WriteLine("Error: sender is not a PictureBox.");
                return;
            }

            PictureBox pictureBox = sender as PictureBox;
            PlacedBuilding bldg;
            System.Windows.Forms.ToolTip tp = new System.Windows.Forms.ToolTip();
            if (pictureBox != null)
            {
                bldg = placedBuildingManager.GetPlacedBuilding(pictureBox);
                if (bldg != null)
                {
                    string timeplaced = FormatSeconds((int)bldg.TimePlaced);
                    tp.SetToolTip(pictureBox, timeplaced);

                }
            }

            if (pictureBox != null && CivBuildingDataList != null)
            {
                if (customToolTip == null)
                {
                    customToolTip = new CustomToolTipForm(this);
                }

                bldg = placedBuildingManager.GetPlacedBuilding(pictureBox);
                if (bldg != null)
                {

                    building buildingInfo = bldg.BuildingInfo;

                    if (buildingInfo != null)
                    {
                        // is building selected
                        if (bldg.IsSelected)
                        {
                            // set selectedBuildingList to all selected buildings
                            GetSelectedBuildingList();
                        }
                        // if not selected then clear totals and add building to totals
                        else
                        {
                            selectedBuildingList.Clear();
                            selectedBuildingList.Add(bldg);
                        }

                        // Set tooltip text and image
                        customToolTip.SetToolTip2(selectedBuildingList);

                        // Position the tooltip near the PictureBox
                        Point location = pictureBox.PointToScreen(new Point(pictureBox.Width / 2, -customToolTip.Height));
                        customToolTip.Location = location;
                        customToolTip.BringToFront();

                        customToolTip.Show();
                    }
                }
            }
        }


        public void drawingPanelPictureBox_MouseLeave(object sender, EventArgs e)
        {
            selectedBuildingList.Clear();

            if (customToolTip != null)
            {
                customToolTip.Hide();
            }
        }


        public void drawingPanelPictureBox_Paint(object? sender, PaintEventArgs e)
        {
            // Ensure sender is a PictureBox
            if (sender is not PictureBox pb)
            {
                Debug.WriteLineIf(DebugConfig.drawingPanelPictureBox_Paint, "Error: sender is not a PictureBox.");
                return;
            }

            // Retrieve the associated PlacedBuilding object
            var bldg = placedBuildingManager.GetPlacedBuilding(pb);
            if (bldg == null)
            {
                Debug.WriteLineIf(DebugConfig.drawingPanelPictureBox_Paint, "Error: No PlacedBuilding found for the provided PictureBox.");
                return;
            }

            // Draw border around PictureBox if selected
            if (bldg.IsSelected)
            {
                ControlPaint.DrawBorder(e.Graphics, pb.ClientRectangle, Color.White, ButtonBorderStyle.Solid);
                Debug.WriteLineIf(DebugConfig.drawingPanelPictureBox_Paint, $"Drawing border for selected building: {bldg.BuildingInfo?.name}");
            }

            // Position unit panel if it exists
            if (bldg.UnitPanelManager != null)
            {
                bldg.UnitPanelManager.UpdatePosition(bldg.PlacedPictureBox.Location);
                Debug.WriteLineIf(DebugConfig.drawingPanelPictureBox_Paint, "Positioning Unit Panel for building.");
            }
            else
            {
                Debug.WriteLineIf(DebugConfig.drawingPanelPictureBox_Paint, "No Unit Panel found for building.");
            }

            // Position settings popup if it exists
            if (bldg.SettingsPanelManager.settingsPanelContent != null)
            {
                bldg.SettingsPanelManager.UpdatePosition(bldg.PlacedPictureBox.Location);
                Debug.WriteLineIf(DebugConfig.drawingPanelPictureBox_Paint, "Positioning Settings Popup for building.");
            }
            else
            {
                Debug.WriteLineIf(DebugConfig.drawingPanelPictureBox_Paint, "No Settings Popup found for building.");
            }

            // Position settings popup if it exists
            if (bldg.VillagerAllocationPanelManager.chartPanelContent != null)
            {
                bldg.VillagerAllocationPanelManager.UpdatePosition(bldg.PlacedPictureBox.Location);
                Debug.WriteLineIf(DebugConfig.drawingPanelPictureBox_Paint, "Positioning Villager Allocation Popup for building.");
            }
            else
            {
                Debug.WriteLineIf(DebugConfig.drawingPanelPictureBox_Paint, "No Villager Allocation Popup found for building.");
            }
        }


        // mouse hover event for villager
        public void ItemPictureBox_MouseHover(object sender, EventArgs e)
        {
            PictureBox villagerPictureBox = sender as PictureBox;

            Villager villager = villagerManager.GetVillagerByPictureBox(villagerPictureBox);

            if (villager != null)
            {
                // Use the villager object, for example, to show a tooltip
                CustomToolTipForm tooltip = new CustomToolTipForm(this);
                tooltip.SetToolTipForVillagerWithGrid(villager); // Set the tooltip for the specific villager

                // Position the tooltip near the PictureBox
                Point location = villagerPictureBox.PointToScreen(new Point(villagerPictureBox.Width / 2, -customToolTip.Height));
                tooltip.Location = location;
                tooltip.BringToFront();

                tooltip.Show();
            }

        }

    }
}
