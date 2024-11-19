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
        // Declare FlowLayoutPanel at the class level
        private FlowLayoutPanel resourcesPanel;

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

        // Method to initialize the resource panel
        private void InitializeResourcePanel()
        {
            // Ensure drawingPanel exists (it should be part of your form)
            if (drawingPanel == null)
            {
                throw new Exception("drawingPanel does not exist or has not been initialized.");
            }

            // Create and configure the resources panel
            resourcesPanel = new FlowLayoutPanel {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(5),
                BackColor = Color.White,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Height = 60
            };

            // Add the panel to the drawingPanel
            drawingPanel.Controls.Add(resourcesPanel);
            resourcesPanel.BringToFront(); // Ensure it appears on top

            // Initialize resources when selecting a civ (e.g., English civ)
            Resources startingResources = new Resources(200, 200, 100, 0, 0, 6); // English starting resources
            InitializeResourceDisplay(resourcesPanel, startingResources, resourceImages);
        }

        // Method to initialize resource display in the panel
        private void InitializeResourceDisplay(FlowLayoutPanel resourcesPanel, Resources resources, Dictionary<string, Image> resourceImages)
        {
            AddResourceDisplay("food", resources.Food, resourcesPanel, resourceImages["food"]);
            AddResourceDisplay("wood", resources.Wood, resourcesPanel, resourceImages["wood"]);
            AddResourceDisplay("gold", resources.Gold, resourcesPanel, resourceImages["gold"]);
            AddResourceDisplay("stone", resources.Stone, resourcesPanel, resourceImages["stone"]);
            AddResourceDisplay("oliveoil", resources.OliveOil, resourcesPanel, resourceImages["oliveoil"]);
            AddResourceDisplay("popcap", resources.PopCap, resourcesPanel, resourceImages["popcap"]);
        }

        // Method to add resource icon and value to the panel
        private void AddResourceDisplay(string resourceName, int resourceValue, FlowLayoutPanel panel, Image resourceIcon)
        {
            PictureBox resourceIconBox = new PictureBox {
                Image = resourceIcon,
                Size = new Size(32, 32), // Set the size of the icon
                SizeMode = PictureBoxSizeMode.StretchImage,
                Margin = new Padding(5)
            };

            Label resourceValueLabel = new Label {
                Text = resourceValue.ToString(),
                AutoSize = true,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Padding = new Padding(5),
                Margin = new Padding(5)
            };

            // Add icon and label to the panel
            panel.Controls.Add(resourceIconBox);
            panel.Controls.Add(resourceValueLabel);
        }

        private void UpdateResourceDisplay(FlowLayoutPanel resourcesPanel, Resources updatedResources)
        {
            foreach (Control control in resourcesPanel.Controls)
            {
                if (control is Label resourceLabel)
                {
                    // Update the label text based on its name or order in the panel
                    if (resourceLabel.Text.StartsWith("Food"))
                    {
                        resourceLabel.Text = updatedResources.Food.ToString();
                    }
                    else if (resourceLabel.Text.StartsWith("Wood"))
                    {
                        resourceLabel.Text = updatedResources.Wood.ToString();
                    }
                    else if (resourceLabel.Text.StartsWith("Gold"))
                    {
                        resourceLabel.Text = updatedResources.Gold.ToString();
                    }
                    else if (resourceLabel.Text.StartsWith("Stone"))
                    {
                        resourceLabel.Text = updatedResources.Stone.ToString();
                    }
                    else if (resourceLabel.Text.StartsWith("OliveOil"))
                    {
                        resourceLabel.Text = updatedResources.OliveOil.ToString();
                    }
                    else if (resourceLabel.Text.StartsWith("PopCap"))
                    {
                        resourceLabel.Text = updatedResources.PopCap.ToString();
                    }
                }
            }
        }
    }
}
