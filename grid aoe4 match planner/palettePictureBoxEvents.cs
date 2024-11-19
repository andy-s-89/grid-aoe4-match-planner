using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grid_aoe4_match_planner
{
    public partial class Form1 : Form
    {
        public void palettePictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            buildingIsDragging = false;
            mouseDownLocation = e.Location;

            if (customToolTip != null)
            {
                customToolTip.Hide();
            }
        }

        public void palettePictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && !buildingIsDragging)
            {
                if (Math.Abs(e.X - mouseDownLocation.X) > 2 ||
                    Math.Abs(e.Y - mouseDownLocation.Y) > 2)
                {
                    buildingIsDragging = true;

                    coordsEnabled = false;
                    buildingIsDragging = false;
                    PictureBox pb = sender as PictureBox;

                    if (pb != null)
                    {
                        // Calculate the center of the PictureBox's image
                        Point centerPoint = new Point(globalGridSize / 2, globalGridSize / 2);
                        Bitmap imageBitmap = new Bitmap(pb.Image);

                        // Create a DataObject and add the PictureBox's image, name, and center point
                        DataObject data = new DataObject();
                        data.SetData("image", imageBitmap);
                        data.SetData("name", pb.Name);
                        data.SetData("centerPoint", centerPoint);

                        SetAllPictureBoxTagsToTransparent(flowLayoutPanel);
                        pasteMode = false;
                        // Start the drag-and-drop operation with the data
                        pb.DoDragDrop(data, DragDropEffects.Copy);

                    }
                }
            }
        }

        public void palettePictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (!buildingIsDragging)
            {
                PictureBox pb = sender as PictureBox;
                if (pb != null)
                {
                    // Check if the Tag is set to Color.White
                    if (pb.Tag is Color && (Color)pb.Tag == Color.White)
                    {
                        SetAllPictureBoxTagsToTransparent(flowLayoutPanel);
                        pasteMode = false;
                    }
                    else
                    {
                        SetAllPictureBoxTagsToTransparent(flowLayoutPanel);
                        pb.Tag = Color.White;
                        pasteMode = true;
                    }
                    pb.Invalidate();
                }
            }
        }


        public void palettePictureBox_MouseEnter(object sender, EventArgs e)
        {
            // Attempt to cast the sender to a PictureBox to make sure the event originated from a PictureBox
            PictureBox pictureBox = sender as PictureBox;

            // Check if the pictureBox is valid and if the building data list (CivBuildingDataList) is populated
            if (pictureBox != null && CivBuildingDataList != null)
            {
                Debug.WriteLineIf(DebugConfig.palettePictureBox_MouseEnter, "\nMouse entered PictureBox with name: " + pictureBox.Name);

                // Check if the custom tooltip form is not already created
                if (customToolTip == null)
                {
                    Debug.WriteLineIf(DebugConfig.palettePictureBox_MouseEnter, "Creating a new instance of CustomToolTipForm.");
                    customToolTip = new CustomToolTipForm(this);
                }

                // Retrieve the building information by matching the PictureBox's name with the building id
                var buildingInfo = CivBuildingDataList[chosenCivRef].data.FirstOrDefault(b => b.id == pictureBox.Name);
                Debug.WriteLineIf(DebugConfig.palettePictureBox_MouseEnter, buildingInfo != null
                    ? "Found building information for PictureBox: " + buildingInfo.id
                    : "No building information found for PictureBox.");

                // Check if valid building information was found
                if (buildingInfo != null)
                {
                    // Create a temporary building object for display in the tooltip
                    var tempbldg = placedBuildingManager.CreateBuilding(buildingInfo, 0, 0);
                    Debug.WriteLineIf(DebugConfig.palettePictureBox_MouseEnter, "Created a temporary PlacedBuilding instance for tooltip display.");

                    // Clear any previously selected buildings and add only this building to the list
                    selectedBuildingList.Clear();
                    selectedBuildingList.Add(tempbldg);
                    Debug.WriteLineIf(DebugConfig.palettePictureBox_MouseEnter, "Cleared selectedBuildingList and added the new building.");

                    // Set the tooltip's text and image based on the selected building information
                    customToolTip.SetToolTip2(selectedBuildingList);
                    Debug.WriteLineIf(DebugConfig.palettePictureBox_MouseEnter, "Set tooltip content using selectedBuildingList.");

                    // Position the tooltip above the PictureBox, horizontally centered
                    Point location = pictureBox.PointToScreen(new Point(pictureBox.Width / 2, -customToolTip.Height));
                    customToolTip.Location = location;
                    Debug.WriteLineIf(DebugConfig.palettePictureBox_MouseEnter, "Set tooltip position to: " + location);

                    // Bring the tooltip to the front and display it
                    customToolTip.BringToFront();
                    customToolTip.Show();
                    Debug.WriteLineIf(DebugConfig.palettePictureBox_MouseEnter, "Displayed tooltip for the building.");
                }
                else
                {
                    Debug.WriteLineIf(DebugConfig.palettePictureBox_MouseEnter, "Building info not found, tooltip will not be displayed.");
                }
            }
            else
            {
                Debug.WriteLineIf(DebugConfig.palettePictureBox_MouseEnter, "palettePictureBox_MouseEnter called, but PictureBox or CivBuildingDataList was null.");
            }
        }


        public void palettePictureBox_MouseLeave(object sender, EventArgs e)
        {
            selectedBuildingList.Clear();

            if (customToolTip != null)
            {
                customToolTip.Hide();
            }
        }


        public void palettePictureBox_Paint(object? sender, PaintEventArgs e)
        {
            // Draw border around PictureBox, color in Tag, trans or white.
            var pb = sender as PictureBox;
            if (pb.Tag != null)
                ControlPaint.DrawBorder(e.Graphics, pb.ClientRectangle, (Color)pb.Tag, ButtonBorderStyle.Solid);
        }



        // Function to deselect all building PictureBoxes in the palette
        public void SetAllPictureBoxTagsToTransparent(FlowLayoutPanel flowLayoutPanel)
        {
            foreach (Control control in flowLayoutPanel.Controls)
            {
                if (control is PictureBox pictureBox)
                {
                    pictureBox.Tag = Color.Transparent;
                    pictureBox.Invalidate();
                }
            }
        }

    }
}
