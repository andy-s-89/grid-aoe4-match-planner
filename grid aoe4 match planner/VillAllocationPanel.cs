using grid_aoe4_match_planner;
using OxyPlot.WindowsForms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace grid_aoe4_match_planner
{
    public class VillagerAllocationPanel : BasePopupPanel
    {
        public ToolTip toolTip;
        public PlacedBuilding parent;
        public PlotView plotView;
        public DataGridView villagerGrid;
        public VillagerAllocationChart chartPanelContent;
        public bool _isUpdatingGrid;

        public VillagerAllocationPanel(PlacedBuilding building, string titleText) : base(titleText)
        {
            this.parent = building;
            Create();
        }

        private void Create()
        {
            Debug.WriteLineIf(DebugConfig.Create, "\n/////// Villager Panel Create() invoked ////////."); // Log method entry

            toolTip = new ToolTip();

            this.BackColor = Color.LightGray;
            this.BorderStyle = BorderStyle.FixedSingle;
            this.Width = 150;
            this.Height = 200;
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            if (parent.villagerAllocations.Count <= 0)
            {
                Debug.WriteLineIf(DebugConfig.Create, "Villager allocations are empty. Exiting Create().");
                return;
            }
            Debug.WriteLineIf(DebugConfig.Create, $"Villager allocations count: {parent.villagerAllocations.Count}");

            chartPanelContent = new VillagerAllocationChart(parent, contentPanel);
            Debug.WriteLineIf(DebugConfig.Create, "VillagerAllocationChart created.");

            chartPanelContent.SetupChartAndVillagerGrid();
            Debug.WriteLineIf(DebugConfig.Create, "SetupChartAndVillagerGrid() called on chartPanelContent.");

            Debug.WriteLineIf(DebugConfig.Create, "Create() completed.");
        }


        public override void ShowPanel()
        {
            Debug.WriteLineIf(DebugConfig.ShowPanel, "\n////// Villager Panel ShowPanel() invoked. //////"); // Log the method entry
            base.ShowPanel(); // Show base components first
            Debug.WriteLineIf(DebugConfig.ShowPanel, "Base panel components shown.");

            Create();
            Debug.WriteLineIf(DebugConfig.ShowPanel, "Create() method executed.");

            parent.PopupButtonManager.Hide();
            Debug.WriteLineIf(DebugConfig.ShowPanel, "PopupButtonManager.Hide() called.");

            if (parent.villagerAllocations.Count <= 0)
            {
                Debug.WriteLineIf(DebugConfig.ShowPanel, "Villager allocations are empty. Exiting ShowPanel().");
                return;
            }

            Debug.WriteLineIf(DebugConfig.ShowPanel, $"Villager allocations count: {parent.villagerAllocations.Count}");

            UpdateVillagerGrid();
            Debug.WriteLineIf(DebugConfig.ShowPanel, "Villager grid updated.");

            chartPanelContent.SetupChartAndVillagerGrid();
            Debug.WriteLineIf(DebugConfig.ShowPanel, "Chart and villager grid set up in chartPanelContent.");

            plotView = chartPanelContent.plotView;
            villagerGrid = chartPanelContent.villagerGrid;
            Debug.WriteLineIf(DebugConfig.ShowPanel, $"PlotView assigned: {plotView != null}, VillagerGrid assigned: {villagerGrid != null}");

            ContentPanel.Controls.Add(chartPanelContent);

            var position = parent.PlacedPictureBox.Location;
            Debug.WriteLineIf(DebugConfig.ShowPanel, $"Updating panel position to: {position}");
            UpdatePosition(position);

            Debug.WriteLineIf(DebugConfig.ShowPanel, "ShowPanel() completed.");
        }


        // Method for updating the Villager Grid values
        public void UpdateVillagerGrid()
        {
            if (chartPanelContent == null) return;

            villagerGrid = chartPanelContent.villagerGrid;
            _isUpdatingGrid = true;
            villagerGrid.Rows.Clear();

            // Populate the DataGridView with updated villager allocations
            foreach (var allocation in parent.villagerAllocations)
            {
                // Convert the time back to minutes and seconds for display
                TimeSpan time = TimeSpan.FromSeconds(allocation.Key);
                string timeString = $"{time.Minutes:D2}:{time.Seconds:D2}";
                villagerGrid.Rows.Add(timeString, allocation.Value);
            }
            _isUpdatingGrid = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of resources specific to VillagerAllocationPanel here
                if (chartPanelContent != null)
                {
                    parent.myForm.drawingPanel.Controls.Remove(this);
                    ContentPanel.Dispose();
                    chartPanelContent = null;
                }
            }
            base.Dispose(disposing);

            MyForm.drawingPanel.Invalidate();
        }
    }
}
