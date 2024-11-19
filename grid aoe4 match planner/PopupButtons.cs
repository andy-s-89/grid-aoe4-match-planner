using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace grid_aoe4_match_planner
{
    public class PopupButtons
    {
        public List<PictureBox> popupButtons = new List<PictureBox>();
        public List<Point> buttonLocations = new List<Point>();
        public PlacedBuilding parent;

        public PopupButtons(PlacedBuilding parentBuilding)
        {
            this.parent = parentBuilding;
        }

        public void Create()
        {
            // Create buttons as needed and assign event handlers
            popupButtons.Clear();
            PictureBox AddUnitPB = CreatePopupButton(buttonLocations[0], Properties.Resources.AddUnitIcon);
            AddUnitPB.MouseClick += AddUnitPB_MouseClick;

            PictureBox QueuePB = CreatePopupButton(buttonLocations[1], Properties.Resources.QueueIcon);
            QueuePB.MouseClick += QueuePB_MouseClick;

            PictureBox StopwatchPB = CreatePopupButton(buttonLocations[2], Properties.Resources.StopwatchIcon);
            StopwatchPB.MouseClick += StopwatchPB_MouseClick;

            PictureBox BuildPB = CreatePopupButton(buttonLocations[3], Properties.Resources.BuildIcon);
            BuildPB.MouseClick += BuildPB_MouseClick;

            PictureBox ChartPB = CreatePopupButton(buttonLocations[4], Properties.Resources.ChartIcon);
            ChartPB.MouseClick += ChartPB_MouseClick;

            popupButtons.Add(AddUnitPB);
            popupButtons.Add(QueuePB);
            popupButtons.Add(StopwatchPB);
            popupButtons.Add(BuildPB);
            popupButtons.Add(ChartPB);
        }

        public void Show()
        {
            foreach (var button in popupButtons)
            {
                parent.myForm?.drawingPanel.Controls.Add(button);
                button.BringToFront();
            }
        }

        public void Hide()
        {
            foreach (var button in popupButtons)
            {
                parent.myForm?.drawingPanel.Controls.Remove(button);
            }
        }

        public void Destroy()
        {
            foreach (var button in popupButtons)
            {
                if (!button.IsDisposed)
                {
                    parent.myForm?.drawingPanel.Controls.Remove(button);
                    button.Dispose();
                }
            }
            popupButtons.Clear();
        }

        private PictureBox CreatePopupButton(Point pt, Image image)
        {
            return new PictureBox {
                Size = new Size((int)(parent.myForm.globalGridSize * 0.8), (int)(parent.myForm.globalGridSize * 0.8)),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Location = pt,
                Image = image
            };
        }

        // Calculate button locations once a picturebox has been added for the building
        public void AssignButtonLocations(int leftButtonCount, int rightButtonCount)
        {
            buttonLocations.Clear();

            if (parent.PlacedPictureBox == null) return;

            // Calculate the center point of the main PictureBox
            int mainBoxCenterX = parent.PlacedPictureBox.Left + parent.PlacedPictureBox.Width / 2;
            int mainBoxCenterY = parent.PlacedPictureBox.Top + parent.PlacedPictureBox.Height / 2;

            // Define the button size and distance from the PictureBox center
            int buttonSize = (int)(parent.myForm.globalGridSize * 0.8);
            int distance = parent.myForm.globalGridSize;

            // Calculate spacing between buttons on each side
            int verticalSpacing = buttonSize + (int)(0.2 * buttonSize);

            // Add left side buttons
            for (int i = 0; i < leftButtonCount; i++)
            {
                int offsetY = (int)(i - (leftButtonCount - 1) / 2.0f) * verticalSpacing;
                int buttonCenterX = mainBoxCenterX - distance; // Left side
                int buttonCenterY = mainBoxCenterY + offsetY;

                // Calculate top-left position of the button
                int buttonLeft = buttonCenterX - buttonSize / 2;
                int buttonTop = buttonCenterY - buttonSize / 2;

                buttonLocations.Add(new Point(buttonLeft, buttonTop));
            }

            // Add right side buttons
            for (int i = 0; i < rightButtonCount; i++)
            {
                int offsetY = (int)(i - (rightButtonCount - 1) / 2.0f) * verticalSpacing;
                int buttonCenterX = mainBoxCenterX + distance; // Right side
                int buttonCenterY = mainBoxCenterY + offsetY;

                // Calculate top-left position of the button
                int buttonLeft = buttonCenterX - buttonSize / 2;
                int buttonTop = buttonCenterY - buttonSize / 2;

                buttonLocations.Add(new Point(buttonLeft, buttonTop));
            }
        }


        // Method to show the buttons in the correct places relative to parent picturebox
        public void UpdatePositions()
        {
            // Check if PlacedPictureBox and popupButtons are not null
            if (parent.PlacedPictureBox == null || popupButtons == null) return;

            // Refresh button locations
            AssignButtonLocations(3,2);

            // Loop through buttons and set their locations
            for (int i = 0; i < popupButtons.Count(); i++)
            {
                // Calculate the X and Y coordinates to position buttons relative to picturebox
                popupButtons[i].Location = buttonLocations[i];
                // Resize buttons based on globalGridSize
                popupButtons[i].Size = new Size((int)(parent.myForm.globalGridSize * 0.8), (int)(parent.myForm.globalGridSize * 0.8));
            }
        }

        private void AddUnitPB_MouseClick(object? sender, MouseEventArgs e)
        {
            parent.UnitPanelManager.ShowPanel();
        }

        private void QueuePB_MouseClick(object? sender, MouseEventArgs e)
        {
            parent.QueuePanelManager.ShowPanel();
        }

        private void StopwatchPB_MouseClick(object? sender, MouseEventArgs e)
        {
            parent.SettingsPanelManager.ShowPanel();
        }

        private void ChartPB_MouseClick(object? sender, MouseEventArgs e)
        {
            parent.VillagerAllocationPanelManager.ShowPanel();
        }

        private void BuildPB_MouseClick(object? sender, MouseEventArgs e)
        {
            //throw new NotImplementedException();
        }


    }

}
