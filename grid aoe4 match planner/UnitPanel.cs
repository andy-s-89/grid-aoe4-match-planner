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
    public class UnitPanel : BasePopupPanel
    {
        public FlowLayoutPanel unitPanelContent;
        public PlacedBuilding parent;

        public UnitPanel(PlacedBuilding parentBuilding, string titleText) : base(titleText)
        {
            parent = parentBuilding;
            Create();
        }

        public void Create()
        {
            unitPanelContent = new FlowLayoutPanel {
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
            ContentPanel.Controls.Add(unitPanelContent);
            UpdatePosition(parent.PlacedPictureBox.Location);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of resources specific to UnitPanel here
                if (unitPanelContent != null)
                {
                    parent.myForm.drawingPanel.Controls.Remove(unitPanelContent);
                    unitPanelContent.Dispose();
                    unitPanelContent = null;
                }
            }
            base.Dispose(disposing);

            MyForm.drawingPanel.Invalidate();
        }

        public void Populate()
        {
            Debug.WriteLineIf(DebugConfig.Populate, $"\n/////// Unit Panel Populate() called ///////");
            // Check if the unit pictures have loaded from GitHub
            Debug.WriteLineIf(DebugConfig.Populate, $"areUnitImagesLoaded: {parent.myForm.areUnitImagesLoaded}");
            if (parent.myForm.areUnitImagesLoaded)
            {
                if (parent.BuildingInfo != null)
                {
                    string id = parent.BuildingInfo.id;
                    Debug.WriteLineIf(DebugConfig.Populate, $"BuildingInfo id: {id}");

                    var pickedCiv = parent.myForm.civilizationDataList[parent.myForm.chosenCivRef];
                    Debug.WriteLineIf(DebugConfig.Populate, $"chosenCivRef: {parent.myForm.chosenCivRef}, pickedCiv: {pickedCiv?.ToString()}");

                    // Access building id keys dynamically
                    if (pickedCiv.techtree != null && pickedCiv.techtree.ContainsKey(id))
                    {
                        Debug.WriteLineIf(DebugConfig.Populate, $"Techtree contains key '{id}'");

                        var jsonUnits = pickedCiv.techtree[id] as JsonElement?;
                        Debug.WriteLineIf(DebugConfig.Populate, $"jsonUnits.HasValue: {jsonUnits.HasValue}, ValueKind: {jsonUnits?.ValueKind}");

                        if (jsonUnits.HasValue && jsonUnits.Value.ValueKind == JsonValueKind.Object)
                        {
                            var unitsDict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonUnits.Value.GetRawText());
                            parent.listOfUnits = unitsDict.Keys.ToList();
                            Debug.WriteLineIf(DebugConfig.Populate, "Units in listOfUnits:");
                            foreach (var unit in parent.listOfUnits)
                            {
                                Debug.WriteLineIf(DebugConfig.Populate, $" - {unit}");
                            }

                            List<PictureBox> orderedPictureBoxes = new List<PictureBox>();

                            // Iterate through each unit name in the list
                            foreach (var unitName in parent.listOfUnits)
                            {
                                Debug.WriteLineIf(DebugConfig.Populate, $"Searching for PictureBox with unitName: {unitName}");
                                var picBox = parent.myForm.pictureBoxesUnits.FirstOrDefault(pBox => pBox.Name.Equals(unitName, StringComparison.Ordinal))
                                    ?? parent.myForm.pictureBoxesUpgrades.FirstOrDefault(pBox => pBox.Name.Equals(unitName, StringComparison.Ordinal))
                                    ?? parent.myForm.pictureBoxesTechnologies.FirstOrDefault(pBox => pBox.Name.Equals(unitName, StringComparison.Ordinal));

                                if (picBox != null)
                                {
                                    Debug.WriteLineIf(DebugConfig.Populate, $"Found PictureBox: {picBox.Name}");

                                    // Clone PictureBox to avoid removing it from the original parent
                                    PictureBox clonedPicBox = new PictureBox {
                                        Name = picBox.Name,
                                        Image = picBox.Image,
                                        Size = picBox.Size,
                                        SizeMode = picBox.SizeMode,
                                        BackColor = picBox.BackColor,
                                        Margin = new Padding(0),
                                        Padding = new Padding(0)
                                    };
                                    clonedPicBox.Click += UnitUpgradePBox_Click;
                                    unitPanelContent.Controls.Add(clonedPicBox);
                                }
                                else
                                {
                                    Debug.WriteLineIf(DebugConfig.Populate, $"PictureBox not found for unitName: {unitName}");
                                }
                            }
                        }
                        else
                        {
                            Debug.WriteLineIf(DebugConfig.Populate, "jsonUnits is either null or not an object.");
                        }
                    }
                    else
                    {
                        Debug.WriteLineIf(DebugConfig.Populate, $"Techtree does not contain key '{id}' or pickedCiv.techtree is null.");
                    }
                }
                else
                {
                    Debug.WriteLineIf(DebugConfig.Populate, "BuildingInfo is null.");
                }
            }
            else
            {
                Debug.WriteLineIf(DebugConfig.Populate, "Unit images have not loaded from GitHub.");
            }
        }

        public void UnitUpgradePBox_Click(object? sender, EventArgs e)
        {
            // Retrieve the PictureBox instance that triggered the event and get its name
            PictureBox pictureBox = sender as PictureBox;
            if (pictureBox == null) return;

            string name = pictureBox.Name;
            int queueTime = (int)parent.TimeFinished + parent.GetTotalQueueTime();

            // Check if chosenCivRef is within bounds of both data lists
            if (parent.myForm.chosenCivRef < 0 || parent.myForm.chosenCivRef >= parent.myForm.CivUnitDataList.Count ||
                parent.myForm.chosenCivRef >= parent.myForm.CivUpgradeDataList.Count || parent.myForm.chosenCivRef >= parent.myForm.CivTechnologyDataList.Count)
            {
                Debug.WriteLineIf(DebugConfig.UnitUpgradePBox_Click, $"Error: chosenCivRef {parent.myForm.chosenCivRef} is out of bounds.");
                Debug.WriteLineIf(DebugConfig.UnitUpgradePBox_Click, $"myForm.CivUnitDataList.Count {parent.myForm.CivUnitDataList.Count} is out of bounds.");
                Debug.WriteLineIf(DebugConfig.UnitUpgradePBox_Click, $"myForm.CivUpgradeDataList.Count {parent.myForm.CivUpgradeDataList.Count} is out of bounds.");
                Debug.WriteLineIf(DebugConfig.UnitUpgradePBox_Click, $"myForm.CivTechnologyDataList.Count {parent.myForm.CivTechnologyDataList.Count} is out of bounds.");
                return;
            }

            // Try to find the name in the unit data list first
            var unitData = parent.myForm.CivUnitDataList[parent.myForm.chosenCivRef].data.FirstOrDefault(u => u.id == name);

            if (unitData != null) // If it's a unit
            {
                if (unitData.id == "villager-1") // Special case: Villager unit
                {
                    var picBox = parent.myForm.pictureBoxesUnits.FirstOrDefault(pBox => pBox.Name == "villager-1");
                    if (picBox != null)
                    {
                        Villager villager = parent.myForm.villagerManager.CreateVillagerFromTC(parent, unitData);

                        // Create and configure the cloned PictureBox for the villager
                        PictureBox clonedPicBox = new PictureBox {
                            Name = picBox.Name,
                            Image = picBox.Image,
                            Size = new Size(parent.myForm.globalGridSize, parent.myForm.globalGridSize),
                            SizeMode = picBox.SizeMode,
                            BackColor = picBox.BackColor,
                            Margin = new Padding(0),
                            Padding = new Padding(0)
                        };

                        clonedPicBox.Location = new Point(
                            parent.Column * parent.myForm.globalGridSize + parent.myForm.translationOffsetX,
                            parent.myForm.drawingPanel.Height - (int)(((double)villager.QueuedTime / parent.myForm.gridTime) * parent.myForm.globalGridSize)
                            - parent.myForm.translationOffsetY - parent.myForm.globalGridSize);

                        // Attach the event handler for removing from queue
                        villager.AttachMouseClickHandler(clonedPicBox);

                        // Add villager to the production queue and add the PictureBox to the panel
                        parent.AddToProductionQueue(villager);
                        parent.myForm.drawingPanel.Controls.Add(clonedPicBox);
                        parent.myForm.drawingPanel.Invalidate();
                        villager.AddPictureBox(clonedPicBox);
                    }
                    return;
                }
                else // Generic unit processing
                {
                    Unit myUnit = new Unit(parent, unitData, queueTime);
                    parent.AddToProductionQueue(myUnit);
                    myUnit.AddUnitToDrawingPanel();
                    parent.myForm.unitManager.AddUnit(myUnit);
                    Debug.WriteLineIf(DebugConfig.UnitUpgradePBox_Click, $"Unit: {myUnit.Name}, Queued Time: {myUnit.QueuedTime}");
                    return;
                }
            }

            // If unitData was null, attempt to get the upgrade data
            var upgradeData = parent.myForm.CivUpgradeDataList[parent.myForm.chosenCivRef].data.FirstOrDefault(u => u.id == name);
            if (upgradeData != null) // It's an upgrade
            {
                Upgrade myUpgrade = new Upgrade(parent, upgradeData, queueTime);
                parent.AddToProductionQueue(myUpgrade);
                myUpgrade.AddUpgradeToDrawingPanel();
                parent.myForm.upgradeManager.AddUpgrade(myUpgrade);
                Debug.WriteLineIf(DebugConfig.UnitUpgradePBox_Click, $"Upgrade: {myUpgrade.Name}, Queued Time: {myUpgrade.QueuedTime}");
                return;
            }

            // If unitData was null, attempt to get the upgrade data
            var technologyData = parent.myForm.CivTechnologyDataList[parent.myForm.chosenCivRef].data.FirstOrDefault(u => u.id == name);
            if (technologyData != null) // It's an upgrade
            {
                Technology myTechnology = new Technology(parent, technologyData, queueTime);
                parent.AddToProductionQueue(myTechnology);
                myTechnology.AddTechnologyToDrawingPanel();
                parent.myForm.technologyManager.AddTechnology(myTechnology);
                Debug.WriteLineIf(DebugConfig.UnitUpgradePBox_Click, $"Technology: {myTechnology.Name}, Queued Time: {myTechnology.QueuedTime}");
                return;
            }

            Debug.WriteLineIf(DebugConfig.UnitUpgradePBox_Click, "No matching unit, upgrade or technology data found for the clicked PictureBox.");
        }
    }

}
