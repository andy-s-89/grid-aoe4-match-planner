using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace grid_aoe4_match_planner
{
    public abstract class QueueableItem
    {
        public string Name { get; set; }
        public PlacedBuilding? Parent { get; set; }
        public PictureBox? ItemPictureBox { get; set; }
        public int Duration { get; set; }
        public int QueuedTime { get; set; }
        // Any other common properties
        public bool ShowDimLines { get; set; } = true;
        public abstract void Dispose();
        // Method to attach MouseClick behavior for removing from queue
        public void AttachMouseClickHandler(PictureBox pictureBox)
        {
            pictureBox.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Right && Parent != null)
                {
                    Parent.ProductionQueue = Parent.RemoveFromProductionQueue(this);
                    Parent.myForm.drawingPanel.Invalidate();
                }
            };
        }
    }

    public class Unit : QueueableItem
    {
        public Form1 myForm = Application.OpenForms.OfType<Form1>().FirstOrDefault();
        public UnitData Data { get; set; }
        public int BirthTime { get; set; }
        public int Column { get; set; }

        public Unit(PlacedBuilding parent, UnitData data, int queuedTime)
        {
            Parent = parent;
            Data = data;
            QueuedTime = queuedTime;

            Duration = data.costs.time;
            BirthTime = QueuedTime + Duration;
            Name = data.name;
            Column = Parent.Column;
        }

        // Add associated picture box thats shown in the drawingPanel
        public void AddPictureBox(PictureBox unitPictureBox)
        {
            ItemPictureBox = unitPictureBox;
        }

        public void AddUnitToDrawingPanel()
        {
            var picBox = myForm.pictureBoxesUnits.FirstOrDefault(pBox => pBox.Name == Data.id);

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
            myForm.unitManager.RemoveUnit(this);

            if (ItemPictureBox != null)
            {
                myForm?.drawingPanel.Controls.Remove(ItemPictureBox); // Remove from parent control
                ItemPictureBox.Dispose();
                ItemPictureBox = null; // Release the reference
            }
        }
    }
}
