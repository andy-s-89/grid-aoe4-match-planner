using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace grid_aoe4_match_planner
{
    public partial class Form1 : Form
    {
        public class DoubleBufferedPanel : Panel
        {
            public bool IsSelecting { get; set; }
            public Rectangle SelectionRectangle { get; set; }

            public DoubleBufferedPanel()
            {
                this.DoubleBuffered = true;
                this.ResizeRedraw = true;

                // Set default values
                IsSelecting = false;
                SelectionRectangle = Rectangle.Empty;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                if (IsSelecting)
                {
                    using (Pen pen = new Pen(Color.White, 1))
                    {
                        e.Graphics.DrawRectangle(pen, SelectionRectangle);
                    }
                }
            }
        }

        public void drawingPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                // Start panning
                isPanning = true;
                panStartPoint = e.Location;
            }
            else if (e.Button == MouseButtons.Left)
            {
                if (pasteMode == true)
                {
                    // paste selected building picturebox into gridtile if empty
                    PastePlacedBuildingInDrawingPanel();
                    isPasting = true;
                }
                else
                {
                    // Start selection
                    isSelecting = true;
                    selectionStartPoint = e.Location;
                    drawingPanel.SelectionRectangle = new Rectangle();
                    drawingPanel.IsSelecting = true;
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                // Hide the custom tooltip
                if (customToolTip != null)
                {
                    customToolTip.Hide();
                }

                // Switch focus to Form1
                this.Focus();

                // Deselect all PictureBoxes
                DeselectAllPictureBoxes();

                // Deselect building selection picbox too
                SetAllPictureBoxTagsToTransparent(flowLayoutPanel);
                pasteMode = false;

                selectedBuildingList.Clear();
            }
        }


        public void drawingPanel_MouseMove(object sender, MouseEventArgs e)
        {
            coordsEnabled = true;
            // Update the mouse Y position
            mouseYPosition = e.Y;

            if (isPanning)
            {
                // Perform panning
                translationOffsetX += e.X - panStartPoint.X;
                translationOffsetY -= e.Y - panStartPoint.Y;
                panStartPoint = e.Location;
                UpdateBuildingPopups();
            }
            else if (isSelecting)
            {
                // Update selection rectangle
                drawingPanel.SelectionRectangle = GetSelectionRectangle(selectionStartPoint, e.Location);
            }
            else if (pasteMode && isPasting)
            {
                // paste selected building picturebox into gridtile if empty
                PastePlacedBuildingInDrawingPanel();
            }
            drawingPanel.Invalidate();
        }


        public void drawingPanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                // Stop panning
                isPanning = false;
                translationOffsetX += e.X - panStartPoint.X;
                translationOffsetY -= e.Y - panStartPoint.Y;
                drawingPanel.Invalidate();
            }
            else if (e.Button == MouseButtons.Left)
            {
                if (pasteMode && isPasting)
                    isPasting = false;
                else if (isSelecting)
                {
                    // Finish selection
                    isSelecting = false;
                    drawingPanel.IsSelecting = false;
                    SelectPictureBoxesInRectangle(drawingPanel.SelectionRectangle);
                    drawingPanel.Invalidate(); // Trigger repaint to remove selection rectangle

                    if (customToolTip == null)
                    {
                        customToolTip = new CustomToolTipForm(this);
                    }

                    // Set the tooltip text
                    customToolTip.SetToolTip2(selectedBuildingList);

                    // Get bounding rectangle of selected PictureBoxes
                    Rectangle rect = CalculateBoundingRectangleForSelectedPictureBoxes();

                    // Translate top right of bounding box to where the drawingPanel is on screen
                    Point location = drawingPanel.PointToScreen(new Point(rect.Right + 10, rect.Top));

                    customToolTip.Location = location;
                    customToolTip.BringToFront();
                    customToolTip.Show();
                }
            }
        }

        // handles dragging a building image over the drawing panel
        public void drawingPanel_DragOver(object sender, DragEventArgs e)
        {
            coordsEnabled = true;

            if (e.Data.GetDataPresent("image"))
            {
                Bitmap originalImage = (Bitmap)e.Data.GetData("image");
                Point centerPoint = (Point)e.Data.GetData("centerPoint");

                // Add background color before resizing
                Bitmap imageWithBackground = AddBackgroundColor(originalImage, Color.FromArgb(52, 84, 105));

                // Resize the image to match the globalGridSize
                Bitmap resizedImage = ResizeImage(imageWithBackground, globalGridSize, globalGridSize);

                // Calculate the position to draw the resized image centered at the mouse position
                Point mousePosition = drawingPanel.PointToClient(new Point(e.X, e.Y));
                dragImageLocation = new Point(mousePosition.X - centerPoint.X, mousePosition.Y - centerPoint.Y);

                // Store the resized image for drawing
                dragImage = resizedImage;

                drawingPanel.Invalidate(); // Trigger a redraw
                e.Effect = DragDropEffects.Copy;
            }
        }

        // handles what happens when building picturebox is dragged out of drawing panel
        public void drawingPanel_DragLeave(object sender, EventArgs e)
        {
            dragImage = null;
            drawingPanel.Invalidate();
        }

        // the drag drop event that is called when a picturebox is dragged and dropped on the drawing panel
        public void drawingPanel_DragDrop(object sender, DragEventArgs e)
        {
            PlaceBuildingInDrawingPanel(sender, e);

            // Clear the drag image and trigger a repaint
            dragImage = null;
            drawingPanel.Invalidate();
        }


        public void drawingPanel_MouseWheel(object? sender, MouseEventArgs e)
        {
            GridSizeChange = globalGridSize / 5;
            Point mousePosition = drawingPanel.PointToClient(Cursor.Position);
            int oldGlobalGridSize = globalGridSize;

            if (e.Delta > 0)
                globalGridSize += GridSizeChange;
            else if (e.Delta < 0 && globalGridSize > MinGridSize)
                globalGridSize -= GridSizeChange;

            AdjustTranslationOffset(mousePosition, oldGlobalGridSize);
            drawingPanel.Invalidate();
        }


        public void drawingPanel_MouseLeave(object sender, EventArgs e)
        {
            coordsEnabled = false;
            drawingPanel.Invalidate();
        }


        public void drawingPanel_Paint(object sender, PaintEventArgs e)
        {
            // Draw the dackground grid tiles
            DrawGrid(e.Graphics, globalGridSize);
            // Draw the grid coordinates of mouse on screen
            if (coordsEnabled)
                DrawGridLocation(e.Graphics);

            // Update the position of all pictureboxes placed in grid
            UpdateAllPictureBoxes();
            // Draw the dimension lines from timeplaced to timefinished if applicable
            DrawAllDimensionLines(sender, e);

            // draw the dragged and dropped image if applicable
            if (dragImage != null)
            {
                e.Graphics.DrawImage(dragImage, dragImageLocation);
            }

            // Draw horizontal white line at mouseY to denote time
            // Only draw if the mouse is inside the panel
            if (mouseYPosition >= 0)
            {
                // Get the drawing graphics
                Graphics g = e.Graphics;

                // Define the pen for the line (white with a width of 1 pixel)
                using (Pen whitePen = new Pen(Color.White, 1))
                {
                    // Draw a horizontal line across the full width of the drawing panel
                    g.DrawLine(whitePen, 0, mouseYPosition, drawingPanel.Width, mouseYPosition);
                }
            }
        }


        public void UpdateAllPictureBoxes()
        {
            UpdateBuildingPictureBoxes();
            UpdateUnitPictureBoxes();
            UpdateUpgradePictureBoxes();
            UpdateTechnologyPictureBoxes();
            UpdateVillagerPictureBoxes();
            UpdateBuildingPopups();
        }

        public void DrawAllDimensionLines(object sender, PaintEventArgs e)
        {
            DrawDimensionLinesBuildings(sender, e);
            DrawDimensionLinesUnits(sender, e);
            DrawDimensionLinesVillagers(sender, e);
            DrawDimensionLinesUpgrades(sender, e);
            DrawDimensionLinesTechnologies(sender, e);
        }

        // Function to update the location and size of all building PictureBoxes
        public void UpdateBuildingPictureBoxes()
        {
            // Step 1: Variable to track if any settings popup is visible during the update
            bool settingsPopupVisible = false;

            // Step 2: Loop through each placed building to update its PictureBox
            foreach (PlacedBuilding bldg in placedBuildingManager.placedBuildings)
            {
                // Step 3: Check if the building object is not null
                if (bldg != null)
                {
                    // Step 4: Get the PictureBox associated with this building
                    PictureBox? pictureBox = bldg.PlacedPictureBox;

                    // Step 5: Ensure the PictureBox exists and the building is not being dragged
                    if (pictureBox != null && !bldg.IsDragged)
                    {
                        // Step 6: Update the PictureBox size based on the global grid size
                        pictureBox.Size = new Size(globalGridSize, globalGridSize);

                        // Step 7: Calculate the new location for the PictureBox
                        // - X-coordinate: building's column number times the grid size, with translation applied
                        // - Y-coordinate: The building's time placed (in seconds) is divided by the gridTime 
                        //                 to scale it, then multiplied by the grid size. After that, 
                        //                 translation and globalGridSize adjustments are applied.
                        pictureBox.Location = new Point(
                            bldg.Column * globalGridSize + translationOffsetX,   // X position based on column and translation
                            drawingPanel.Height -                                // Inverted Y-axis for drawing panel
                            (int)((bldg.TimePlaced / gridTime) * globalGridSize) // Y position scaled by time placed
                            - translationOffsetY                                 // Apply vertical translation
                            - globalGridSize                                     // Adjust for the box size
                        );

                        // Step 8: Check if the building's settings popup is currently visible
                        // If visible, mark the popup as active
                        if (bldg.SettingsPanelManager.settingsPanelContent != null && bldg.SettingsPanelManager.settingsPanelContent.Visible)
                        {
                            settingsPopupVisible = true;
                        }
                    }
                }
            }

            // Step 9: If any settings popup was visible, loop again to update date-time pickers
            //         (only done after all PictureBoxes are updated)
            if (settingsPopupVisible)
            {
                foreach (PlacedBuilding bldg in placedBuildingManager.placedBuildings)
                {
                    // Step 10: Check if the building and its settings popup are visible, then update pickers
                    if (bldg?.SettingsPanelManager.settingsPanelContent?.Visible == true)
                    {
                        bldg.SettingsPanelManager.settingsPanelContent.UpdateDateTimePickers();
                    }
                }
            }
        }


        // function to update the location of all unit PictureBoxes
        public void UpdateUnitPictureBoxes()
        {
            foreach (Unit unit in unitManager.Units)
            {
                if (unit != null)
                {
                    PictureBox? pictureBox = unit.ItemPictureBox;

                    if (pictureBox != null)
                    {
                        // Capture relevant values for debugging
                        int unitColumn = unit.Column;
                        int unitQueuedTime = unit.QueuedTime;
                        int panelHeight = drawingPanel.Height;
                        int calculatedYPosition = panelHeight - (int)(((double)unitQueuedTime / gridTime) * globalGridSize)
                                                    - translationOffsetY - globalGridSize;
                        int calculatedXPosition = unitColumn * globalGridSize + translationOffsetX;

                        // Update the PictureBox location based on queue time and column
                        pictureBox.Size = new Size(globalGridSize, globalGridSize);
                        pictureBox.Location = new Point(calculatedXPosition, calculatedYPosition);

                    }
                    else
                    {
                        Debug.WriteLine("PictureBox is null for unit: " + unit.Name);
                    }
                }
                else
                {
                    Debug.WriteLine("Unit is null.");
                }
            }
        }

        // function to update the location of all unit PictureBoxes
        public void UpdateUpgradePictureBoxes()
        {
            foreach (Upgrade upgrade in upgradeManager.Upgrades)
            {
                if (upgrade != null)
                {
                    PictureBox? pictureBox = upgrade.ItemPictureBox;

                    if (pictureBox != null)
                    {
                        // Capture relevant values for debugging
                        int upgradeColumn = upgrade.Column;
                        int upgradeQueuedTime = upgrade.QueuedTime;
                        int panelHeight = drawingPanel.Height;
                        int calculatedYPosition = panelHeight - (int)(((double)upgradeQueuedTime / gridTime) * globalGridSize)
                                                    - translationOffsetY - globalGridSize;
                        int calculatedXPosition = upgradeColumn * globalGridSize + translationOffsetX;

                        // Update the PictureBox location based on queue time and column
                        pictureBox.Size = new Size(globalGridSize, globalGridSize);
                        pictureBox.Location = new Point(calculatedXPosition, calculatedYPosition);

                    }
                    else
                    {
                        Debug.WriteLine("PictureBox is null for upgrade: " + upgrade.Name);
                    }
                }
                else
                {
                    Debug.WriteLine("Upgrade is null.");
                }
            }
        }

        public void UpdateTechnologyPictureBoxes()
        {
            foreach (Technology technology in technologyManager.Technologies)
            {
                if (technology != null)
                {
                    PictureBox? pictureBox = technology.ItemPictureBox;

                    if (pictureBox != null)
                    {
                        // Capture relevant values for debugging
                        int technologyColumn = technology.Column;
                        int technologyQueuedTime = technology.QueuedTime;
                        int panelHeight = drawingPanel.Height;
                        int calculatedYPosition = panelHeight - (int)(((double)technologyQueuedTime / gridTime) * globalGridSize)
                                                    - translationOffsetY - globalGridSize;
                        int calculatedXPosition = technologyColumn * globalGridSize + translationOffsetX;

                        // Update the PictureBox location based on queue time and column
                        pictureBox.Size = new Size(globalGridSize, globalGridSize);
                        pictureBox.Location = new Point(calculatedXPosition, calculatedYPosition);

                    }
                    else
                    {
                        Debug.WriteLine("PictureBox is null for technology: " + technology.Name);
                    }
                }
                else
                {
                    Debug.WriteLine("Technology is null.");
                }
            }
        }


        public void UpdateBuildingPopups()
        {
            foreach (PlacedBuilding bldg in placedBuildingManager.placedBuildings)
            {
                if (bldg != null && bldg.PlacedPictureBox != null)
                {
                    Point anchorPoint = bldg.PlacedPictureBox.Location;

                    if (bldg.UnitPanelManager != null)
                        bldg.UnitPanelManager.UpdatePosition(anchorPoint);
                    if (bldg.QueuePanelManager != null)
                        bldg.QueuePanelManager.UpdatePosition(anchorPoint);
                    if (bldg.SettingsPanelManager != null)
                        bldg.SettingsPanelManager.UpdatePosition(anchorPoint);
                    if (bldg.VillagerAllocationPanelManager != null)
                        bldg.VillagerAllocationPanelManager.UpdatePosition(anchorPoint);
                    if (bldg.PopupButtonManager.popupButtons.Count() > 0)
                        bldg.PopupButtonManager.UpdatePositions();
                }
            }
        }

        // function to update the location of all villagers PictureBoxes
        public void UpdateVillagerPictureBoxes()
        {
            foreach (Villager vil in villagerManager.Villagers)
            {
                if (vil != null)
                {
                    PictureBox? pictureBox = vil.ItemPictureBox;
                    if (pictureBox != null)
                    {
                        pictureBox.Size = new Size(globalGridSize, globalGridSize);
                        pictureBox.Location = new Point(
                            vil.Column * globalGridSize + translationOffsetX,
                            drawingPanel.Height - (int)(((double)vil.QueuedTime / gridTime) * globalGridSize)
                            - translationOffsetY - globalGridSize);
                    }
                }
            }
        }

        public void DrawDimensionLinesBuildings(object sender, PaintEventArgs e)
        {
            // Get the graphics object
            Graphics g = e.Graphics;

            // Pen for drawing
            Pen redPen = new Pen(Color.Red, 1);

            foreach (PlacedBuilding bldg in placedBuildingManager.placedBuildings)
            {
                if (bldg != null && bldg.TimeFinished.HasValue && bldg.ShowDimLines == true) // Check if TrueFinishedTime has a value
                {
                    PictureBox pictureBox = bldg.PlacedPictureBox;

                    // Convert TimePlaced and TimeFinished to Y coordinates
                    int yTimePlaced = drawingPanel.Height - (int)((bldg.TimePlaced / gridTime) * globalGridSize) - translationOffsetY;
                    int yTimeFinished = drawingPanel.Height - (int)((bldg.TimeFinished.Value / gridTime) * globalGridSize) - translationOffsetY; // Access TrueFinishedTime.Value safely
                    int yTimeStarted = drawingPanel.Height - (int)((bldg.TimeStarted.Value / gridTime) * globalGridSize) - translationOffsetY;

                    // X coordinate based on the column of the picturebox
                    int xCoordinate = pictureBox.Location.X + (pictureBox.Width / 2);

                    // Draw the red dimension line
                    g.DrawLine(redPen, xCoordinate, yTimePlaced, xCoordinate, yTimeFinished);

                    // Draw ticks at TimePlaced, TimeStarted, and TrueFinishedTime
                    DrawDimensionTick(g, redPen, xCoordinate, yTimePlaced);  // Tick for TimePlaced
                    DrawDimensionTick(g, redPen, xCoordinate, yTimeStarted); // Tick for TimeStarted
                    DrawDimensionTick(g, redPen, xCoordinate, yTimeFinished); // Tick for TrueFinishedTime
                }
            }
        }

        public void DrawDimensionLinesUnits(object sender, PaintEventArgs e)
        {
            // Get the graphics object
            Graphics g = e.Graphics;

            // Pen for drawing
            Pen redPen = new Pen(Color.Red, 1);

            foreach (Unit unit in unitManager.Units)
            {
                if (unit != null && unit.ShowDimLines == true)
                {
                    PictureBox pictureBox = unit.ItemPictureBox;

                    // Convert TimePlaced and TimeFinished to Y coordinates
                    int yQueuedTime = drawingPanel.Height - (int)(((double)unit.QueuedTime / gridTime) * globalGridSize) - translationOffsetY;
                    int yBirthTime = drawingPanel.Height - (int)(((double)unit.BirthTime / gridTime) * globalGridSize) - translationOffsetY;

                    // X coordinate based on the column of the picturebox
                    int xCoordinate = pictureBox.Location.X + (pictureBox.Width / 2);

                    // Draw the red dimension line
                    g.DrawLine(redPen, xCoordinate, yQueuedTime, xCoordinate, yBirthTime);

                    // Draw ticks at TimePlaced, TimeStarted, and TrueFinishedTime
                    DrawDimensionTick(g, redPen, xCoordinate, yQueuedTime);  // Tick for QueuedTime
                    DrawDimensionTick(g, redPen, xCoordinate, yBirthTime); // Tick for BirthTime
                }
            }
        }

        public void DrawDimensionLinesVillagers(object sender, PaintEventArgs e)
        {
            // Get the graphics object
            Graphics g = e.Graphics;

            // Pen for drawing
            Pen redPen = new Pen(Color.Red, 1);

            foreach (Villager vil in villagerManager.Villagers)
            {
                PictureBox pictureBox = vil.ItemPictureBox;

                if (vil != null && pictureBox != null && vil.ShowDimLines == true)
                {
                    

                    // Convert TimePlaced and TimeFinished to Y coordinates
                    int yQueuedTime = drawingPanel.Height - (int)(((double)vil.QueuedTime / gridTime) * globalGridSize) - translationOffsetY;
                    int yBirthTime = drawingPanel.Height - (int)(((double)vil.BirthTime / gridTime) * globalGridSize) - translationOffsetY;

                    // X coordinate based on the column of the picturebox
                    int xCoordinate = pictureBox.Location.X + (pictureBox.Width / 2);

                    // Draw the red dimension line
                    g.DrawLine(redPen, xCoordinate, yQueuedTime, xCoordinate, yBirthTime);

                    // Draw ticks at TimePlaced, TimeStarted, and TrueFinishedTime
                    DrawDimensionTick(g, redPen, xCoordinate, yQueuedTime);  // Tick for QueuedTime
                    DrawDimensionTick(g, redPen, xCoordinate, yBirthTime); // Tick for BirthTime
                }
            }
        }

        public void DrawDimensionLinesUpgrades(object sender, PaintEventArgs e)
        {
            // Get the graphics object
            Graphics g = e.Graphics;

            // Pen for drawing
            Pen redPen = new Pen(Color.Red, 1);

            foreach (Upgrade upgrade in upgradeManager.Upgrades)
            {
                if (upgrade != null && upgrade.ShowDimLines == true)
                {
                    PictureBox pictureBox = upgrade.ItemPictureBox;

                    // Convert TimePlaced and TimeFinished to Y coordinates
                    int yQueuedTime = drawingPanel.Height - (int)(((double)upgrade.QueuedTime / gridTime) * globalGridSize) - translationOffsetY;
                    int yBirthTime = drawingPanel.Height - (int)(((double)upgrade.ResearchedTime / gridTime) * globalGridSize) - translationOffsetY;

                    // X coordinate based on the column of the picturebox
                    int xCoordinate = pictureBox.Location.X + (pictureBox.Width / 2);

                    // Draw the red dimension line
                    g.DrawLine(redPen, xCoordinate, yQueuedTime, xCoordinate, yBirthTime);

                    // Draw ticks at TimePlaced, TimeStarted, and TrueFinishedTime
                    DrawDimensionTick(g, redPen, xCoordinate, yQueuedTime);  // Tick for QueuedTime
                    DrawDimensionTick(g, redPen, xCoordinate, yBirthTime); // Tick for BirthTime
                }
            }
        }

        public void DrawDimensionLinesTechnologies(object sender, PaintEventArgs e)
        {
            // Get the graphics object
            Graphics g = e.Graphics;

            // Pen for drawing
            Pen redPen = new Pen(Color.Red, 1);

            foreach (Technology technology in technologyManager.Technologies)
            {
                if (technology != null && technology.ShowDimLines == true)
                {
                    PictureBox pictureBox = technology.ItemPictureBox;

                    // Convert TimePlaced and TimeFinished to Y coordinates
                    int yQueuedTime = drawingPanel.Height - (int)(((double)technology.QueuedTime / gridTime) * globalGridSize) - translationOffsetY;
                    int yBirthTime = drawingPanel.Height - (int)(((double)technology.ResearchedTime / gridTime) * globalGridSize) - translationOffsetY;

                    // X coordinate based on the column of the picturebox
                    int xCoordinate = pictureBox.Location.X + (pictureBox.Width / 2);

                    // Draw the red dimension line
                    g.DrawLine(redPen, xCoordinate, yQueuedTime, xCoordinate, yBirthTime);

                    // Draw ticks at TimePlaced, TimeStarted, and TrueFinishedTime
                    DrawDimensionTick(g, redPen, xCoordinate, yQueuedTime);  // Tick for QueuedTime
                    DrawDimensionTick(g, redPen, xCoordinate, yBirthTime); // Tick for BirthTime
                }
            }
        }


        public void DrawDimensionTick(Graphics g, Pen pen, int xCoordinate, int yCoordinate)
        {
            int halfTickLength = globalGridSize / 4;
            float scaledLength = halfTickLength * 0.6f; // Scale the 45-degree line by 0.6

            // Draw the perpendicular tick line (horizontal), centered at the time point
            g.DrawLine(pen, xCoordinate - halfTickLength, yCoordinate, xCoordinate + halfTickLength, yCoordinate);

            // Draw the smaller 45-degree line, centered on the tick
            g.DrawLine(pen, xCoordinate - (int)scaledLength, yCoordinate + (int)scaledLength,
                            xCoordinate + (int)scaledLength, yCoordinate - (int)scaledLength);
        }


        // Functions for handling and drawing grid, used in mousewheel event and paint
        public void AdjustTranslationOffset(Point mousePosition, int oldGridSize)
        {
            int panelHeight = drawingPanel.Height;
            int gridX = (mousePosition.X - translationOffsetX) / oldGridSize;
            int gridY = (panelHeight - mousePosition.Y - translationOffsetY) / oldGridSize;

            translationOffsetX = mousePosition.X - gridX * globalGridSize;
            translationOffsetY = panelHeight - mousePosition.Y - gridY * globalGridSize;
        }


        public void DrawGrid(Graphics g, int cellSize)
        {
            // Adjust the color intensity of the grid lines based on zoom level
            int alpha = globalGridSize; // Fading out the grid lines when zoomed out
            alpha = Math.Max(0, Math.Min(255, alpha)); // Clamping the value between 0 and 255
            Pen gridPen = new Pen(Color.FromArgb(alpha, Color.Gray), 1);
            Pen axisPen = new Pen(Color.White, 2);
            int panelHeight = drawingPanel.Height;
            int panelWidth = drawingPanel.Width;

            for (int x = translationOffsetX % cellSize; x < panelWidth; x += cellSize)
                g.DrawLine(x == translationOffsetX ? axisPen : gridPen, x, 0, x, panelHeight);
            for (int y = translationOffsetY % cellSize; y < panelHeight; y += cellSize)
                g.DrawLine(y == translationOffsetY ? axisPen : gridPen, 0, panelHeight - y, panelWidth, panelHeight - y);

            gridPen.Dispose();
            axisPen.Dispose();
        }

        public void DrawGridLocation(Graphics g)
        {
            Font font = new Font(this.Font, FontStyle.Regular);
            Brush brush = new SolidBrush(Color.White);
            Point mousePosition = drawingPanel.PointToClient(Cursor.Position);
            Point cellPoint = GetGridCellTime(mousePosition);

            mousePosition.Offset(0, 20);
            g.DrawString($"Column {cellPoint.X}, {FormatSeconds(cellPoint.Y)}", font, brush, mousePosition);

            font.Dispose();
            brush.Dispose();
        }

        public Point GetGridCell(Point mouseLocation)
        {
            int panelHeight = drawingPanel.Height;
            float transformedX = (mouseLocation.X - translationOffsetX) / (float)globalGridSize;
            float transformedY = (panelHeight - mouseLocation.Y - translationOffsetY) / (float)globalGridSize;

            int cellX = (int)Math.Floor(transformedX);
            int cellY = (int)Math.Floor(transformedY);

            return new Point(cellX, cellY);
        }


        public Point GetGridCellTime(Point mouseLocation)
        {
            int panelHeight = drawingPanel.Height;

            // Use double for higher precision
            double transformedX = (mouseLocation.X - translationOffsetX) / (double)globalGridSize;
            double transformedY = (panelHeight - mouseLocation.Y - translationOffsetY) / (double)globalGridSize;

            // Round to get more accurate grid coordinates
            int cellX = (int)Math.Floor(transformedX);

            // Convert the Y position to time in seconds
            int secondsY = (int)Math.Round(transformedY * gridTime);

            return new Point(cellX, secondsY);
        }

        // Format seconds into min:seconds
        public string FormatSeconds(int totalSeconds)
        {
            // Calculate minutes and seconds
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            // Format minutes and seconds into "M:SS" format
            return $"{minutes}:{seconds:D2}";
        }

        // image handling functions used in drag over drawing panel event
        public Bitmap ResizeImage(Bitmap image, int width, int height)
        {
            Bitmap resizedImage = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(resizedImage))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(image, 0, 0, width, height);
            }
            return resizedImage;
        }

        public Bitmap AddBackgroundColor(Bitmap image, Color backgroundColor)
        {
            Bitmap backgroundImage = new Bitmap(image.Width, image.Height);

            using (Graphics g = Graphics.FromImage(backgroundImage))
            {
                g.Clear(backgroundColor); // Fill the background with the specified color
                g.DrawImage(image, 0, 0); // Draw the original image on top of the background
            }

            return backgroundImage;
        }


        // Method to place a building's PictureBox in the drawing panel
        // Can handle Drag-and-Drop (DragEventArgs) or manual placement (PictureBox)
        public void PlaceBuildingInDrawingPanel(object sender = null, DragEventArgs e = null, PictureBox selectedPB = null)
        {
            // Step 1: Get the current mouse position in the drawingPanel
            Point mousePosition = drawingPanel.PointToClient(Cursor.Position);

            // Step 2: Convert the mouse position to the grid cell (column and time in seconds)
            Point gridCell = GetGridCellTime(mousePosition); // returns the column (X) and the center point in seconds (Y)
            Debug.WriteLineIf(DebugConfig.PlaceBuildingInDrawingPanel, $"gridCell = {gridCell}");

            // Step 3: Convert the grid cell (column and time) to an on-screen location in the drawing panel
            Point proposedInsertPoint = GridPointConvertion(gridCell);

            // Step 4: Check if the proposed grid cell is empty (no other building in the way)
            if (CheckLocationIsEmpty(gridCell))
            {
                Bitmap image = null;  // To store the building's image
                string name = null;   // To store the building's name
                Point centerPoint = Point.Empty;  // Optional, used in Drag-and-Drop

                // Step 5: Determine if the method is handling Drag-and-Drop or manual placement
                if (e != null && e.Data.GetDataPresent("image") && e.Data.GetDataPresent("name"))
                {
                    // Drag-and-Drop case: retrieve image, name, and center point from the DataObject
                    image = (Bitmap)e.Data.GetData("image");
                    name = (string)e.Data.GetData("name");
                    centerPoint = (Point)e.Data.GetData("centerPoint");
                }
                else if (selectedPB != null) // Manual placement case (Paste)
                {
                    // Paste case: retrieve the image and name from the selected PictureBox
                    image = (Bitmap)selectedPB.Image;
                    name = selectedPB.Name;
                }

                // Step 6: Only proceed if a valid image and name are provided
                if (image != null && !string.IsNullOrEmpty(name))
                {
                    // Step 7: Create a new PictureBox for the building
                    PictureBox pb = new PictureBox {
                        Image = image,
                        Name = name,
                        Size = new Size(globalGridSize, globalGridSize),  // Set the size according to grid size
                        SizeMode = PictureBoxSizeMode.StretchImage,       // Stretch the image to fit the PictureBox
                        BackColor = Color.FromArgb(255, 52, 84, 105),     // Background color for visibility
                        Location = proposedInsertPoint                    // Set the PictureBox location
                    };

                    // Step 8: Attach relevant event handlers to the PictureBox for interaction
                    pb.MouseDown += drawingPanelPictureBox_MouseDown;
                    pb.MouseMove += drawingPanelPictureBox_MouseMove;
                    pb.MouseUp += drawingPanelPictureBox_MouseUp;
                    pb.MouseEnter += drawingPanelPictureBox_MouseEnter;
                    pb.MouseLeave += drawingPanelPictureBox_MouseLeave;
                    pb.Paint += drawingPanelPictureBox_Paint;

                    // Step 9: Add the PictureBox to the drawing panel and make it visible
                    drawingPanel.Controls.Add(pb);
                    pb.Show();

                    // Step 10: Retrieve the building data from the CivBuildingDataList based on the building name
                    var buildingData = CivBuildingDataList[chosenCivRef].data.FirstOrDefault(b => b.id == name);

                    // Step 11: Create a new instance of PlacedBuilding
                    // gridCell.Y - (gridTime / 2) adjusts the time to center it in the grid
                    PlacedBuilding myBuilding = placedBuildingManager.CreateBuilding(buildingData, gridCell.Y - (gridTime / 2), gridCell.X);

                    // Step 12: Associate the newly created PictureBox with the building
                    myBuilding.AddPictureBox(pb);

                    // Step 13: Start and finish the building's construction (simulating instant build for now)
                    myBuilding.StartConstruction(1.0, 1);  // Starts build 1 second after placement, 1 villager (default values)
                    myBuilding.FinishConstruction();

                    // Step 14: Add the newly placed building to the placedBuildingManager for tracking
                    placedBuildingManager.AddPlacedBuilding(myBuilding);

                    // Step 15: Invalidate the drawing panel to force a redraw (reflect changes visually)
                    drawingPanel.Invalidate();
                }
            }
        }


        // Modify building placement logic to check if there are enough resources and subtract the costs.
        /*public void PlaceBuilding(Building building)
        {
            if (currentResources.HasEnoughResources(building.Costs))
            {
                currentResources.Subtract(building.Costs);
                // Place the building logic here
                // ...
            }
            else
            {
                MessageBox.Show("Not enough resources to place this building.");
            }
        }
        */



        // variation of the PlaceBuildingInDrawingPanel method,
        // for left click and hold spam when palette building is selected
        public void PastePlacedBuildingInDrawingPanel()
        {
            PictureBox? selectedPB = GetSelectedBuilding();
            if (selectedPB != null)
            {
                PlaceBuildingInDrawingPanel(selectedPB: selectedPB);
            }
        }


        // column,time conversion to on screen drawing panel location
        public Point GridPointConvertion2(Point gridPoint)
        {
            Point newPoint = new Point(gridPoint.X * globalGridSize + translationOffsetX,
                                            drawingPanel.Height - (int)((double)gridPoint.Y / gridTime) * globalGridSize
                                            + (globalGridSize / 2) + translationOffsetY + globalGridSize);

            return newPoint;
        }

        // This method takes a grid-based coordinate (X = column, Y = time in seconds)
        // and converts it to an on-screen position.
        public Point GridPointConvertion(Point gridPoint)
        {
            // Step 1: X-coordinate conversion (Column to on-screen X position)
            int onScreenX = gridPoint.X * globalGridSize + translationOffsetX;

            // Step 2: Y-coordinate conversion (Time to on-screen Y position)
            double timeInGridUnits = ((double)gridPoint.Y / gridTime);
            int onScreenY = drawingPanel.Height - (int)(timeInGridUnits * globalGridSize);
            onScreenY -= globalGridSize;  // TimePlaced value to top of picturebox
            onScreenY -= translationOffsetY;  // Account for vertical panning

            // Step 3: Return the new screen position as a Point
            return new Point(onScreenX, onScreenY);
        }


        // Method to check if a proposed grid cell location is empty of any placed buildings
        public bool CheckLocationIsEmpty(Point gridCell)
        {
            // Step 1: Calculate the time-placed check value by adjusting the Y-coordinate
            // The Y-coordinate of the gridcell represents the time placed. 
            // Subtract half of the gridTime (half height of picturebox).
            int checkTimePlaced = gridCell.Y - (gridTime / 2);

            // Step 2: Iterate through all placed buildings to check for potential overlap
            foreach (PlacedBuilding building in placedBuildingManager.GetAllPlacedBuildings())
            {
                // Step 3: Get the column of the current building (its X position in the grid)
                int column = building.Column;

                // Step 4: Get the time when the current building was placed (its Y position in the grid)
                double timePlaced = building.TimePlaced;

                // Step 5: Check if the columns match (if the building is in the same X position)
                // If the building's column is not the same as the proposed location's X, skip this building.
                if (column != gridCell.X)
                    continue;

                // Step 6: Check if the checkTimePlaced falls within the time range of the current building
                // We use gridTime to define the acceptable range, creating a time buffer around the placed time.
                // If checkTimePlaced is within the buffer of the current building's placed time, it's considered an intersection.
                if (checkTimePlaced > timePlaced - gridTime && checkTimePlaced < timePlaced + gridTime)
                {
                    // Step 7: Intersection found, log the details and return false (location is not empty)
                    Debug.WriteLineIf(DebugConfig.CheckLocationIsEmpty, $"Intersection found with building at column {column} and time placed {timePlaced}. Proposed time: {checkTimePlaced}");
                    return false;
                }
            }

            // Step 8: No intersections were found, log the details and return true (location is empty)
            Debug.WriteLineIf(DebugConfig.CheckLocationIsEmpty, $"No intersections found at column {gridCell.X} and proposed time {checkTimePlaced}");
            return true;
        }



        // Get the selected picturebox in the tool palette
        // called by PastePlacedBuildingInDrawingPanel()
        public PictureBox? GetSelectedBuilding()
        {
            foreach (Control control in flowLayoutPanel.Controls)
            {
                if (control is PictureBox pictureBox && pictureBox.Tag is Color tagColor && tagColor == Color.White)
                {
                    return pictureBox;
                }
            }

            // Return null if no PictureBox with Color.White Tag is found
            return null;
        }


        //
        // Functions for dealing with selected buildings and passing them to custom tool tip form
        //

        // Function to calculate the bounding rectangle of selected PictureBoxes
        public Rectangle CalculateBoundingRectangleForSelectedPictureBoxes()
        {
            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;

            // Create a list to hold selected PictureBoxes
            List<PictureBox> selectedPictureBoxes = GetSelectedPictureBoxes();

            foreach (var pictureBox in selectedPictureBoxes)
            {
                if (pictureBox.Left < minX)
                    minX = pictureBox.Left;
                if (pictureBox.Top < minY)
                    minY = pictureBox.Top;
                if (pictureBox.Right > maxX)
                    maxX = pictureBox.Right;
                if (pictureBox.Bottom > maxY)
                    maxY = pictureBox.Bottom;
            }

            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }

        // Function to get selected PictureBoxes for bounding box
        // used in CalculateBoundingRectangleForSelectedPictureBoxes()
        public List<PictureBox> GetSelectedPictureBoxes()
        {
            // Create a list to hold selected PictureBoxes
            List<PictureBox> selectedPictureBoxes = new List<PictureBox>();

            // iterate through all placed buildings within the drawingPanel
            foreach (PlacedBuilding bldg in placedBuildingManager.GetAllPlacedBuildings())
            {
                if (bldg.IsSelected)
                {
                    if (bldg.PlacedPictureBox != null)
                        selectedPictureBoxes.Add(bldg.PlacedPictureBox);
                }
            }

            return selectedPictureBoxes;
        }


        public Rectangle GetSelectionRectangle(Point start, Point end)
        {
            int x = Math.Min(start.X, end.X);
            int y = Math.Min(start.Y, end.Y);
            int width = Math.Abs(start.X - end.X);
            int height = Math.Abs(start.Y - end.Y);
            return new Rectangle(x, y, width, height);
        }


        // Iterate through all placed buildings in the drawingPanel and
        // if their picturebox intersects the selection box drawn by the user
        // then set PlacedBuildings IsSelected to true and add to selectedBuildingList.
        // If picture box is not intersecting selection set then set IsSelected to false.
        public void SelectPictureBoxesInRectangle(Rectangle rectangle)
        {
            SuspendLayout();

            // clear selection building list
            selectedBuildingList.Clear();

            // reset total costs within selection to 0
            ResetTotalCosts();

            // iterate through all placed buildings within the drawingPanel
            foreach (PlacedBuilding bldg in placedBuildingManager.GetAllPlacedBuildings())
            {
                // get associated picture box
                PictureBox? pictureBox = bldg.PlacedPictureBox;
                if (pictureBox != null)
                {
                    // if intersects then IsSelected = True
                    if (rectangle.IntersectsWith(pictureBox.Bounds))
                    {
                        bldg.IsSelected = true;
                        selectedBuildingList.Add(bldg);
                    }
                    else
                    {
                        bldg.IsSelected = false;
                    }

                    // update picturebox with selected boundary or not
                    pictureBox.Invalidate();
                }
            }
            ResumeLayout();
        }

        // get all selected PlacedBuildings in drawingPanel
        // and add to selectedBuildingList
        public void GetSelectedBuildingList()
        {
            // clear selection building list
            selectedBuildingList.Clear();

            // reset total costs within selection to 0
            ResetTotalCosts();

            // iterate through all placed buildings within the drawingPanel
            foreach (PlacedBuilding bldg in placedBuildingManager.GetAllPlacedBuildings())
            {
                if (bldg.IsSelected)
                    selectedBuildingList.Add(bldg);
            }
        }

        public void ResetTotalCosts()
        {
            // Initialize the total costs dictionary
            totalCosts = new Dictionary<string, int>
            {
                { "food", 0 },
                { "wood", 0 },
                { "gold", 0 },
                { "stone", 0 },
                { "total", 0 },
                { "time", 0 }
            };
        }

    }
}
