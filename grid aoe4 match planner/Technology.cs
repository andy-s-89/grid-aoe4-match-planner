using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace grid_aoe4_match_planner
{
    public class Technology : QueueableItem
    {
        public Form1 myForm = Application.OpenForms.OfType<Form1>().FirstOrDefault();
        public TechnologyData Data { get; set; }
        public int ResearchedTime { get; set; }
        public int Column { get; set; }

        public Technology(PlacedBuilding parent, TechnologyData data, int queuedTime)
        {
            Parent = parent;
            Data = data;
            QueuedTime = queuedTime;

            Duration = data.costs.time;
            ResearchedTime = QueuedTime + Duration;
            Name = data.name;
            Column = Parent.Column;
        }

        // Add associated picture box thats shown in the drawingPanel
        public void AddPictureBox(PictureBox technologyPictureBox)
        {
            ItemPictureBox = technologyPictureBox;
        }

        public void AddTechnologyToDrawingPanel()
        {
            var picBox = myForm.pictureBoxesTechnologies.FirstOrDefault(pBox => pBox.Name == Data.id);

            // Clone PictureBox to avoid removing it from the original parent
            if (picBox != null)
            {
                PictureBox clonedPicBox = new PictureBox {
                    Name = picBox.Name,
                    Image = picBox.Image,
                    Size = new Size(myForm.globalGridSize, myForm.globalGridSize),
                    SizeMode = picBox.SizeMode,
                    BackColor = picBox.BackColor,
                    Margin = new Padding(0),
                    Padding = new Padding(0)
                };

                clonedPicBox.Location = new Point(
                            Column * myForm.globalGridSize + myForm.translationOffsetX,
                            myForm.drawingPanel.Height - (int)(((double)QueuedTime / myForm.gridTime) * myForm.globalGridSize)
                            - myForm.translationOffsetY - myForm.globalGridSize);

                // Attach the event handler for removing from queue
                AttachMouseClickHandler(clonedPicBox);

                myForm.drawingPanel.Controls.Add(clonedPicBox);
                myForm.drawingPanel.Invalidate();
                this.AddPictureBox(clonedPicBox);

            }
        }

        public override void Dispose()
        {
            myForm.technologyManager.RemoveTechnology(this);

            if (ItemPictureBox != null)
            {
                myForm?.drawingPanel.Controls.Remove(ItemPictureBox); // Remove from parent control
                ItemPictureBox.Dispose();
                ItemPictureBox = null; // Release the reference
            }
        }
    }
}
