using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace grid_aoe4_match_planner
{
    public class QueuePanel : BasePopupPanel
    {
        public FlowLayoutPanel queuePanelContent;
        public PlacedBuilding parent;

        public QueuePanel(PlacedBuilding parentBuilding, string titleText) : base(titleText)
        {
            parent = parentBuilding;
            Create();
        }

        public void Create()
        {
            queuePanelContent = new FlowLayoutPanel {
                AutoSize = false,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                MinimumSize = new Size(192, 192),
                MaximumSize = new Size(192, 192),
                AutoScroll = true,
                Padding = new Padding(0),
                Margin = new Padding(0),
                BackColor = Color.White
            };
        }

        public override void ShowPanel()
        {
            base.ShowPanel(); // Show base components first
            Create();
            Populate();
            parent.PopupButtonManager.Hide();
            ContentPanel.Controls.Add(queuePanelContent);
            UpdatePosition(parent.PlacedPictureBox.Location);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of resources specific to QueuePanel here
                if (queuePanelContent != null)
                {
                    parent.myForm.drawingPanel.Controls.Remove(queuePanelContent);
                    queuePanelContent.Dispose();
                    queuePanelContent = null;
                }
            }
            base.Dispose(disposing);

            MyForm.drawingPanel.Invalidate();
        }

        public void Populate()
        {
            AddQueueItemsToPanel();
        }

        public void AddQueueItemsToPanel()
        {
            // Clear existing controls before adding new ones
            queuePanelContent.Controls.Clear();

            // Loop through each item in the production queue
            foreach (QueueableItem qItem in parent.ProductionQueue)
            {
                // Create a new PictureBox as a copy of the original
                PictureBox copiedPictureBox = new PictureBox {
                    Name = qItem.ItemPictureBox.Name,
                    Image = qItem.ItemPictureBox.Image,
                    Size = qItem.ItemPictureBox.Size,
                    SizeMode = qItem.ItemPictureBox.SizeMode,
                    BackColor = qItem.ItemPictureBox.BackColor,
                    Margin = qItem.ItemPictureBox.Margin,
                    Padding = qItem.ItemPictureBox.Padding
                };

                // Optionally, add event handlers for interactions with the copied PictureBox
                copiedPictureBox.Click += (sender, e) => {
                    // Define any click behavior for the copied PictureBox here, if needed
                };

                // Add the copied PictureBox to the FlowLayoutPanel
                queuePanelContent.Controls.Add(copiedPictureBox);
            }

            // Optional layout adjustments
            queuePanelContent.AutoScroll = true;
            queuePanelContent.Refresh();
        }


        public void UnitUpgradePBox_Click(object? sender, EventArgs e)
        {
        }
    }
}
