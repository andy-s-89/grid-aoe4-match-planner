using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.WindowsForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace grid_aoe4_match_planner
{
    public class VillagerAllocationChart : Panel
    {
        private readonly PlacedBuilding building;
        private readonly Panel targetPanel;
        public TableLayoutPanel layout;
        public DataGridView villagerGrid;
        public PlotView plotView;

        public VillagerAllocationChart(PlacedBuilding building, Panel targetPanel)
        {
            this.building = building;
            this.targetPanel = targetPanel;
            this.layout = new TableLayoutPanel();
            this.villagerGrid = new DataGridView();
            this.plotView = new PlotView();
            this.Width = 400;
            this.Height = 300;
        }

        public void SetupChartAndVillagerGrid()
        {
            // Create a new layout to hold both the chart and the DataGridView
            layout = new TableLayoutPanel {
                ColumnCount = 2,
                RowCount = 1,
                Dock = DockStyle.Fill,
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70)); // 70% for the chart
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30)); // 30% for the grid

            // Create a new PlotView
            plotView = new PlotView {
                Dock = DockStyle.Fill
            };

            // Create a new PlotModel
            PlotModel plotModel = new PlotModel { Title = null }; // null title to save space

            // Create and add a LineSeries for Progress
            LineSeries progressSeries = new LineSeries {
                Title = "Progress",
                Color = OxyColors.Blue,
                MarkerType = MarkerType.None,
                StrokeThickness = 2,
                TrackerFormatString = "Time: {2:mm\\:ss}" + Environment.NewLine + "Progress: {4:0}%"
            };

            // Create and add a ScatterSeries for Villagers
            ScatterSeries villagerSeries = new ScatterSeries {
                Title = "Villagers",
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                MarkerFill = OxyColors.Red,
                TrackerFormatString = "Time: {2:mm\\:ss}" + Environment.NewLine + "Vills: {4:0}"
            };

            // Initially populate the chart
            PopulateChart(plotView, plotModel, progressSeries, villagerSeries);

            // Add Series to the PlotModel
            plotModel.Series.Add(progressSeries);
            plotModel.Series.Add(villagerSeries);

            // Set DateTimeAxis for X-axis with mm:ss format
            plotModel.Axes.Add(new DateTimeAxis {
                Position = AxisPosition.Bottom,
                Title = "",
                StringFormat = "mm:ss",
                Minimum = DateTimeAxis.ToDouble(DateTime.Today.AddSeconds(building.TimeStarted.Value)),
                Maximum = DateTimeAxis.ToDouble(DateTime.Today.AddSeconds(building.TimeFinished.Value))
            });

            // Set the linear axis for progress on the Y-axis
            plotModel.Axes.Add(new LinearAxis {
                Position = AxisPosition.Left,
                Title = "",
                Minimum = 0,
                Maximum = 100
            });

            plotView.Model = plotModel;

            // Add the PlotView (chart) to the first column of the layout
            layout.Controls.Add(plotView, 0, 0);

            // Create and set up the DataGridView for villager allocations
            villagerGrid = CreateVillagerGrid(progressSeries, villagerSeries, plotView, plotModel);

            // Add the DataGridView to the second column of the layout
            layout.Controls.Add(villagerGrid, 1, 0);

            // Add the layout to the target panel
            targetPanel.Controls.Clear();
            targetPanel.Controls.Add(layout);

            targetPanel.Refresh();
        }

        private DataGridView CreateVillagerGrid(LineSeries progressSeries, ScatterSeries villagerSeries, PlotView plotView, PlotModel plotModel)
        {
            villagerGrid = new DataGridView {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnCount = 2,
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true
            };

            villagerGrid.Columns[0].Name = "Time (mm:ss)";
            villagerGrid.Columns[1].Name = "Villagers";

            // Hide both column and row headers
            villagerGrid.ColumnHeadersVisible = true;
            villagerGrid.RowHeadersVisible = false;

            // Populate the DataGridView with current villager allocations
            foreach (var allocation in building.GetVillagerAllocations())
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(allocation.Key);
                villagerGrid.Rows.Add(timeSpan.ToString(@"mm\:ss"), allocation.Value);
            }

            // Add event for when the user changes or adds a villager allocation
            villagerGrid.CellValueChanged += (sender, e) =>
            {
                // Update villager allocations from the grid
                building.UpdateVillagerAllocationsFromGrid(villagerGrid);

                // Refresh the chart after updating villager allocations
                PopulateChart(plotView, plotModel, progressSeries, villagerSeries);
            };

            return villagerGrid;
        }

        private void PopulateChart(PlotView plotView, PlotModel plotModel, LineSeries progressSeries, ScatterSeries villagerSeries)
        {
            // Clear existing data
            progressSeries.Points.Clear();
            villagerSeries.Points.Clear();

            // Populate progressSeries with progressIntervals
            foreach (var interval in building.progressIntervals)
            {
                DateTime time = DateTime.Today.AddSeconds(interval.Time);
                double progress = interval.Progress;
                progressSeries.Points.Add(DateTimeAxis.CreateDataPoint(time, progress));
            }

            // Populate villagerSeries with villager allocations
            foreach (var allocation in building.GetVillagerAllocations())
            {
                DateTime time = DateTime.Today.AddSeconds(allocation.Key);
                int villagers = allocation.Value;
                villagerSeries.Points.Add(new ScatterPoint(DateTimeAxis.ToDouble(time), villagers));
            }

            // Update X-axis limits
            double minTime = building.TimeStarted ?? building.progressIntervals.First().Time;
            double maxTime = building.TimeFinished ?? building.progressIntervals.Last().Time;

            if (building.GetVillagerAllocations().Count > 0)
            {
                minTime = Math.Min(minTime, building.GetVillagerAllocations().Keys.Min());
                maxTime = Math.Max(maxTime, building.GetVillagerAllocations().Keys.Max());
            }

            DateTimeAxis xAxis = (DateTimeAxis)plotModel.Axes.FirstOrDefault(a => a is DateTimeAxis);
            if (xAxis != null)
            {
                xAxis.Minimum = DateTimeAxis.ToDouble(DateTime.Today.AddSeconds(minTime));
                xAxis.Maximum = DateTimeAxis.ToDouble(DateTime.Today.AddSeconds(maxTime));
            }
            
            // Redraw the chart
            plotView.InvalidatePlot(true);
        }
    }
}
