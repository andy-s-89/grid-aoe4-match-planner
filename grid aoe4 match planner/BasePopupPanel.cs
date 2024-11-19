using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace grid_aoe4_match_planner
{
    public class BasePopupPanel : Panel, IDisposable
    {
        public Form1 MyForm { get; } = Application.OpenForms.OfType<Form1>().FirstOrDefault();

        protected FlowLayoutPanel basePanel;
        private Panel titleBar;
        private Label titleLabel;
        private Button closeButton;
        protected Panel contentPanel; // Panel for derived classes to add content

        private bool disposed = false;

        public BasePopupPanel(string titleText = "")
        {
            InitializeComponents(titleText);
        }

        private void InitializeComponents(string titleText)
        {
            basePanel = new FlowLayoutPanel {
                BackColor = Color.Gray,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            InitializeTitleBar(titleText);

            // Panel to hold content from derived classes
            contentPanel = new Panel {
                BackColor = Color.White,
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            // Add title bar and content area to the base panel
            basePanel.Controls.Add(titleBar);
            basePanel.Controls.Add(contentPanel);

            Controls.Add(basePanel);
        }

        private void InitializeTitleBar(string titleText)
        {
            titleBar = new Panel {
                Height = 30,
                Dock = DockStyle.Top,
                BackColor = Color.LightGray,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            titleLabel = new Label {
                Text = titleText,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.Black,
                Location = new Point(30, 5)
            };
            titleBar.Controls.Add(titleLabel);

            closeButton = new Button {
                Text = "×",
                Width = 25,
                Height = 25,
                Location = new Point(5, 2),
                FlatStyle = FlatStyle.Flat
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Click += (s, e) => Dispose();
            titleBar.Controls.Add(closeButton);
        }

        // Derived classes can add controls to `contentPanel` to appear below the title bar
        public Panel ContentPanel => contentPanel;

        public virtual void ShowPanel()
        {
            InitializeComponents("");
            MyForm?.drawingPanel.Controls.Add(basePanel);
            basePanel.BringToFront();
            basePanel.Show();
        }

        public void UpdatePosition(Point anchor, int offsetX = -10, int offsetY = 0)
        {
            if (basePanel == null) return;

            int newPanelX = anchor.X + offsetX - basePanel.Width;
            int newPanelY = anchor.Y + offsetY - basePanel.Height / 2;

            basePanel.Location = new Point(newPanelX, newPanelY);
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    closeButton.Click -= (s, e) => Dispose();

                    // Remove the panel from the parent container (e.g., drawingPanel) if present
                    if (basePanel?.Parent != null)
                    {
                        basePanel.Parent.Controls.Remove(basePanel);
                    }

                    // Dispose of all controls within basePanel
                    closeButton?.Dispose();
                    titleLabel?.Dispose();
                    titleBar?.Dispose();
                    contentPanel?.Dispose();
                    basePanel?.Dispose();
                }

                disposed = true;
                base.Dispose(disposing);

                // Optional: Refresh or invalidate the parent container to clear visual remnants
                MyForm?.drawingPanel?.Invalidate();
            }
        }
    }
}
