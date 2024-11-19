using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace grid_aoe4_match_planner
{
    public partial class Form1 : Form
    {
        public void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if the focused control is a text input field, such as NumericUpDown
            if (this.ActiveControl is TextBoxBase || this.ActiveControl is NumericUpDown)
            {
                return; // Skip key handling for text input fields
            }

            // Handle Escape key to deselect all PictureBoxes
            if (e.KeyCode == Keys.Escape)
            {
                HandleEscapeKey();
            }
            // Handle Delete or Backspace key to delete selected PictureBoxes
            else if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            {
                HandleDeleteOrBackspaceKey();
            }
        }


        public void HandleEscapeKey()
        {
            // Hide any relevant tooltips or panels
            if (customToolTip != null)
            {
                customToolTip.Hide();
            }

            foreach (var building in placedBuildingManager.GetAllPlacedBuildings())
            {
                building.UnitPanelManager.Dispose();
                building.QueuePanelManager.Dispose();
                building.SettingsPanelManager.Dispose();
                building.VillagerAllocationPanelManager.Dispose();
                building.PopupButtonManager.Hide();
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


        public void HandleDeleteOrBackspaceKey()
        {
            // Hide the custom tooltip
            if (customToolTip != null)
            {
                customToolTip.Hide();
            }

            foreach (var building in placedBuildingManager.GetAllPlacedBuildings())
            {
                building.UnitPanelManager.Dispose();
                building.QueuePanelManager.Dispose();
                building.SettingsPanelManager.Dispose();
                building.VillagerAllocationPanelManager.Dispose();
                building.PopupButtonManager.Hide();
            }

            // Switch focus to Form1
            this.Focus();

            // Delete selected PictureBoxes
            DeleteSelectedBuildings();

            selectedBuildingList.Clear();
        }


        public void DeselectAllPictureBoxes()
        {
            selectedBuildingList.Clear();
            // iterate through all placed buildings within the drawingPanel
            foreach (PlacedBuilding bldg in placedBuildingManager.GetAllPlacedBuildings())
            {
                bldg.IsSelected = false;
                PictureBox? pictureBox = bldg.PlacedPictureBox;
                if (pictureBox != null)
                    pictureBox.Invalidate(); // Redraw the PictureBox
            }
        }


        public void DeleteSelectedBuildings()
        {
            // Iterate over a copy of the list to avoid modifying the collection while iterating
            foreach (PlacedBuilding building in selectedBuildingList.ToList())
            {
                if (building.IsLocked == false)
                {
                    // Dispose of building resources
                    building.Dispose();

                    // Remove the building from the placedBuildingManager if it's managed there
                    placedBuildingManager.RemovePlacedBuilding(building);

                    // Remove the building from the selectedBuildingList
                    selectedBuildingList.Remove(building);

                }
            }
        }
    }
}
