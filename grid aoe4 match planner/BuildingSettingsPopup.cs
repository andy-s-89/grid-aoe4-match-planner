using grid_aoe4_match_planner;
using OxyPlot.WindowsForms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace grid_aoe4_match_planner
{
    public class BuildingSettingsPopup : Panel
    {
        // Controls
        public DateTimePicker dtpTimePlaced, dtpTimeStarted, dtpTimeFinished;
        public TextBox txtActualBuildTime;
        public ToolTip toolTip;

        // References
        public Form1 ParentForm { get; }
        public PlacedBuilding PlacedBuilding { get; }

        // Flags to prevent recursive or conflicting updates
        public bool _isUpdating;
        public bool _isUpdatingOverride;
        public bool _isUpdatingTimeStarted;
        public bool _isUpdatingTimeFinished;
        public bool _isUpdatingActualBuildTime;
        public bool _isCreated = false;

        // Track old values
        public double _oldTimeStarted;

        // Constants
        private const string TimeFormat = "mm:ss";
        private const int DefaultWidth = 150;
        private const int DefaultHeight = 300;

        public BuildingSettingsPopup(Form1 form, PlacedBuilding building)
        {
            ParentForm = form;
            PlacedBuilding = building;

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            toolTip = new ToolTip();
            BackColor = Color.LightGray;
            BorderStyle = BorderStyle.FixedSingle;
            Width = DefaultWidth;
            Height = DefaultHeight;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Padding = new Padding(0);
            Margin = new Padding(0);

            InitializeDateTimePickers();
            InitializeBuildTimeTextBox();
            InitializeTableLayout();
        }

        private void InitializeDateTimePickers()
        {
            dtpTimePlaced = CreateDateTimePicker((int)PlacedBuilding.TimePlaced, isTimeStartedPicker: false, isTimeFinishedPicker: false);
            dtpTimeStarted = CreateDateTimePicker((int)(PlacedBuilding.TimeStarted ?? 0), isTimeStartedPicker: true, isTimeFinishedPicker: false);
            dtpTimeFinished = CreateDateTimePicker((int)(PlacedBuilding.TimeFinished ?? 0), isTimeStartedPicker: false, isTimeFinishedPicker: true);
        }

        private void InitializeBuildTimeTextBox()
        {
            txtActualBuildTime = new TextBox {
                Width = 50,
                Text = PlacedBuilding.ActualBuildTime.ToString()
            };

            txtActualBuildTime.TextChanged += (sender, e) =>
            {
                if (_isUpdatingActualBuildTime) return;

                if (int.TryParse(txtActualBuildTime.Text, out int result))
                {
                    PlacedBuilding.ActualBuildTime = result;
                    ParentForm.drawingPanel.Invalidate();
                }
            };
        }

        private void InitializeTableLayout()
        {
            var tableLayout = new TableLayoutPanel {
                ColumnCount = 1,
                Dock = DockStyle.Fill,
                AutoSize = true
            };

            tableLayout.Controls.Add(CreateLabeledControl("Placed:", dtpTimePlaced), 0, 0);
            tableLayout.Controls.Add(CreateLabeledControl("Started:", dtpTimeStarted), 0, 1);
            tableLayout.Controls.Add(CreateLabeledControl("Finished:", dtpTimeFinished), 0, 2);
            tableLayout.Controls.Add(CreateBuildTimePanel(), 0, 3);

            Controls.Add(tableLayout);
        }


        private FlowLayoutPanel CreateBuildTimePanel()
        {
            var buildTimePanel = new FlowLayoutPanel {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = false
            };

            var lblBuildTime = new Label {
                Text = "Build Time:",
                Width = 80,
                TextAlign = ContentAlignment.MiddleLeft
            };

            buildTimePanel.Controls.Add(lblBuildTime);
            buildTimePanel.Controls.Add(txtActualBuildTime);

            return buildTimePanel;
        }

        private DateTimePicker CreateDateTimePicker(int totalSeconds, bool isTimeStartedPicker, bool isTimeFinishedPicker)
        {
            var dtp = new DateTimePicker {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = TimeFormat,
                ShowUpDown = true,
                Value = DateTime.Today.AddSeconds(totalSeconds),
                Width = 100
            };

            if (isTimeStartedPicker)
            {
                _oldTimeStarted = totalSeconds;

                dtp.ValueChanged += (sender, e) =>
                {
                    if (_isUpdating || _isUpdatingTimeFinished) return;

                    _isUpdatingTimeStarted = true;
                    double newTimeStarted = (dtp.Value - DateTime.Today).TotalSeconds;
                    double changeInTimeStarted = newTimeStarted - _oldTimeStarted;

                    PlacedBuilding.TimeStarted = newTimeStarted;
                    if (_isCreated)
                        PlacedBuilding.UpdateVillagerAllocations(changeInTimeStarted);
                    ParentForm.drawingPanel.Invalidate();

                    _oldTimeStarted = newTimeStarted;
                    _isUpdatingTimeStarted = false;
                    _isCreated = true;
                };
            }
            else if (isTimeFinishedPicker)
            {
                dtp.ValueChanged += (sender, e) =>
                {
                    if (_isUpdating || _isUpdatingTimeStarted) return;

                    _isUpdatingTimeFinished = true;
                    PlacedBuilding.TimeFinished = (dtp.Value - DateTime.Today).TotalSeconds;
                    ParentForm.drawingPanel.Invalidate();
                    _isUpdatingTimeFinished = false;
                };
            }
            else
            {
                dtp.ValueChanged += (sender, e) =>
                {
                    PlacedBuilding.TimePlaced = (dtp.Value - DateTime.Today).TotalSeconds;
                    ParentForm.drawingPanel.Invalidate();
                };
            }

            return dtp;
        }

        private FlowLayoutPanel CreateLabeledControl(string labelText, Control control)
        {
            var panel = new FlowLayoutPanel {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true
            };

            var label = new Label {
                Text = labelText,
                Width = 80,
                TextAlign = ContentAlignment.MiddleLeft
            };

            panel.Controls.Add(label);
            panel.Controls.Add(control);

            return panel;
        }

        public void UpdateDateTimePickers()
        {
            _isUpdatingOverride = true;
            dtpTimePlaced.Value = DateTime.Today.AddSeconds((int)PlacedBuilding.TimePlaced);
            dtpTimeStarted.Value = DateTime.Today.AddSeconds((int)(PlacedBuilding.TimeStarted ?? 0));
            dtpTimeFinished.Value = DateTime.Today.AddSeconds((int)(PlacedBuilding.TimeFinished ?? 0));
            _isUpdatingActualBuildTime = true;
            txtActualBuildTime.Text = ((int)PlacedBuilding.ActualBuildTime).ToString();
            _isUpdatingActualBuildTime = false;
            _isUpdatingOverride = false;
        }
    }
}
