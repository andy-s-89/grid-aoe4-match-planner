using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using System.Collections;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.IO;
using System.Numerics;

namespace grid_aoe4_match_planner
{
    public class PlacedBuilding
    {
        public Form1 myForm = Application.OpenForms.OfType<Form1>().FirstOrDefault();
        // Properties to store relevant information
        public int Column { get; set; }
        public double TimePlaced { get; set; }  // Time placed in seconds
        public double? TimeStarted { get; set; }  // Time started in seconds
        public double? TimeFinished { get; set; }  // Time finished in seconds
        public double BaseBuildTime { get; set; }  // Base build time for this building
        public double ActualBuildTime { get; set; }  // Actual build time for this building
        public bool BuildTimeOverride { get; set; } = false;  // Has the ActualBuildTime been overriden
        public bool ShowDimLines { get; set; } = true; // Show dimension lines or not

        // Villagers allocated for construction over time
        public SortedList<double, int> villagerAllocations;

        // List to store progress intervals
        public List<(double Time, double Progress)> progressIntervals = new List<(double Time, double Progress)>();
        
        public Queue<QueueableItem> ProductionQueue { get; set; } // Queue for unit production

        public building BuildingInfo { get; set; } // Building information imported from json
        public PictureBox? PlacedPictureBox { get; set; } // PictureBox in drawingPanel associated with the building

        public bool IsSelected = false;
        public bool IsDragged = false;
        public bool IsLocked = false;

        
        public List<string>? listOfUnits; // To store the list of units or other items

        public PopupButtons PopupButtonManager { get; private set; }
        public UnitPanel UnitPanelManager { get; private set; }
        public QueuePanel QueuePanelManager { get; private set; }
        public SettingsPanel SettingsPanelManager { get; private set; }
        public VillagerAllocationPanel VillagerAllocationPanelManager { get; private set; }


        // Constructor to initialize the PlacedBuilding object
        public PlacedBuilding(building buildingInfo, double timePlaced, int column)
        {
            TimePlaced = timePlaced;
            Column = column;
            BuildingInfo = buildingInfo;
            BaseBuildTime = buildingInfo.costs.time;
            ActualBuildTime = BaseBuildTime;
            villagerAllocations = new SortedList<double, int>();
            ProductionQueue = new Queue<QueueableItem>();

            // Initialize PopupButtons and UnitPanel
            PopupButtonManager = new PopupButtons(this);
            UnitPanelManager = new UnitPanel(this, "");
            QueuePanelManager = new QueuePanel(this, "");
            SettingsPanelManager = new SettingsPanel(this, "");
            VillagerAllocationPanelManager = new VillagerAllocationPanel(this, "");
        }

        // Add associated picture box thats shown in the drawingPanel
        // and initialize and create popup buttons
        public void AddPictureBox(PictureBox placedPictureBox)
        {
            PlacedPictureBox = placedPictureBox;
            PopupButtonManager.AssignButtonLocations(3,2);
            PopupButtonManager.Create();
        }

        public int GetTotalQueueTime()
        {
            int totalDuration = 0;

            foreach (var qItem in ProductionQueue)
            {
                totalDuration += qItem.Duration;
            }

            return totalDuration;
        }

        // Method to start construction
        public void StartConstruction(double startTime, int initialVillagers)
        {
            TimeStarted = TimePlaced + startTime;
            AllocateVillagers(TimePlaced + startTime, initialVillagers);
        }

        // Method to finish construction
        public void FinishConstruction()
        {
            if (TimeStarted.HasValue)
            {
                // Calculate TimeFinished based on TimeStarted and the dynamic build time
                TimeFinished = TimeStarted.Value + CalculateDynamicBuildTime();

                // Set ActualBuildTime by subtracting TimeStarted from TimeFinished
                ActualBuildTime = TimeFinished.Value - TimeStarted.Value;
            }
            else
            {
                MessageBox.Show("Error: TimeStarted is not set.");
            }
        }

        // Method to allocate villagers to the construction site
        public void AllocateVillagers(double time, int numberOfVillagers)
        {
            villagerAllocations[time] = numberOfVillagers;
        }

        // Method to change times in vill allocs
        public void UpdateVillagerAllocations(double changeInTimeStarted)
        {
            Debug.WriteLineIf(DebugConfig.UpdateVillagerAllocations, $"\n////// UpdateVillagerAllocations called with changeInTimeStarted: {changeInTimeStarted} ///////");

            // Update the villager allocations in the SortedList
            SortedList<double, int> updatedVillagerAllocations = new SortedList<double, int>();

            Debug.WriteLineIf(DebugConfig.UpdateVillagerAllocations, "Updating villager allocations:");
            foreach (var allocation in villagerAllocations)
            {
                double updatedTime = allocation.Key + changeInTimeStarted;
                updatedVillagerAllocations[updatedTime] = allocation.Value;

                Debug.WriteLineIf(DebugConfig.UpdateVillagerAllocations, $"Old time: {allocation.Key}, New time: {updatedTime}, Villager count: {allocation.Value}");
            }
            villagerAllocations = updatedVillagerAllocations;

            // calculate new finished time and actual build time from new villager allocations
            Debug.WriteLineIf(DebugConfig.UpdateVillagerAllocations, "Calculating new finished time and actual build time after updating allocations.");
            FinishConstruction();
        }

        // Method called from selected GridDataView to update Villager Allocations
        public void UpdateVillagerAllocationsFromGrid(DataGridView villagerGrid)
        {
            Debug.WriteLineIf(DebugConfig.UpdateVillagerAllocationsFromGrid, "\n//////// Starting UpdateVillagerAllocationsFromGrid() /////////");

            // Clear the current villagerAllocations before updating
            villagerAllocations.Clear();
            Debug.WriteLineIf(DebugConfig.UpdateVillagerAllocationsFromGrid, "Cleared existing villagerAllocations.");

            // Iterate through each row in the DataGridView
            foreach (DataGridViewRow row in villagerGrid.Rows)
            {
                // Ensure the row is not empty
                if (row.Cells[0].Value != null && row.Cells[1].Value != null)
                {
                    // Parse the time and villager count from the DataGridView
                    string timeText = row.Cells[0].Value.ToString();
                    string villagerText = row.Cells[1].Value.ToString();
                    Debug.WriteLineIf(DebugConfig.UpdateVillagerAllocationsFromGrid, $"Processing row - Time: {timeText}, Villagers: {villagerText}");

                    if (TimeSpan.TryParseExact(timeText, new[] { @"mm\:ss", @"m\:ss" }, null, out TimeSpan timeSpan) &&
                        int.TryParse(villagerText, out int numberOfVillagers))
                    {
                        double timeInSeconds = timeSpan.TotalSeconds;
                        Debug.WriteLineIf(DebugConfig.UpdateVillagerAllocationsFromGrid, $"Parsed time: {timeInSeconds} seconds, Villagers: {numberOfVillagers}");

                        // Add or update the allocation using the existing AllocateVillagers method
                        AllocateVillagers(timeInSeconds, numberOfVillagers);
                        Debug.WriteLineIf(DebugConfig.UpdateVillagerAllocationsFromGrid, $"Allocated {numberOfVillagers} villagers at {timeInSeconds} seconds.");
                    }
                    else
                    {
                        Debug.WriteLineIf(DebugConfig.UpdateVillagerAllocationsFromGrid, $"Failed to parse row - Time: {timeText}, Villagers: {villagerText}");
                    }
                }
                else
                {
                    Debug.WriteLineIf(DebugConfig.UpdateVillagerAllocationsFromGrid, "Skipped an empty or invalid row.");
                }
            }

            if (villagerAllocations.Count > 0)
            {
                TimeStarted = villagerAllocations.First().Key;
                Debug.WriteLineIf(DebugConfig.UpdateVillagerAllocationsFromGrid, $"Set TimeStarted to {TimeStarted} seconds based on first villager allocation.");
            }
            else
            {
                Debug.WriteLineIf(DebugConfig.UpdateVillagerAllocationsFromGrid, "No villager allocations found, TimeStarted not updated.");
            }

            FinishConstruction();
            Debug.WriteLineIf(DebugConfig.UpdateVillagerAllocationsFromGrid, "Called FinishConstruction method.");

            myForm.drawingPanel.Invalidate();
            Debug.WriteLineIf(DebugConfig.UpdateVillagerAllocationsFromGrid, "Invalidated the drawing panel.");
            Debug.WriteLineIf(DebugConfig.UpdateVillagerAllocationsFromGrid, "Finished UpdateVillagerAllocationsFromGrid.");
        }


        // Method to get the number of villagers allocated at a specific time
        public int GetVillagersAtTime(double time)
        {
            int villagers = 0;
            foreach (var entry in villagerAllocations)
            {
                if (entry.Key <= time)
                {
                    villagers = entry.Value;
                }
            }
            return villagers;
        }

        // Method to add a unit to the production queue
        public void AddToProductionQueue(QueueableItem qItem)
        {
            ProductionQueue.Enqueue(qItem);
        }

        // Method to remove a unit from the production queue (after it's produced)
        public Queue<QueueableItem> RemoveFromProductionQueue(QueueableItem qItem)
        {
            // Calculate the duration of the item to be removed
            var duration = qItem.Duration;

            // Create a new queue excluding the removed item
            var newQueue = new Queue<QueueableItem>();
            bool itemFound = false;

            foreach (var item in ProductionQueue)
            {
                if (item == qItem)
                {
                    itemFound = true;
                    qItem.Dispose();
                    continue; // Skip the item being removed
                }

                if (itemFound)
                {
                    // Adjust times for each item after the removed item
                    item.QueuedTime -= duration;
                    if (item is Unit)
                    {
                        ((Unit)item).BirthTime -= duration;
                    }
                    else if (item is Villager)
                    {
                        ((Villager)item).BirthTime -= duration;
                    }
                    else if (item is Upgrade)
                    {
                        ((Upgrade)item).ResearchedTime -= duration;
                    }
                    else if (item is Technology)
                    {
                        ((Technology)item).ResearchedTime -= duration;
                    }
                }

                // Add the item to the new queue
                newQueue.Enqueue(item);
            }

            // Return the new queue without gaps in times
            return newQueue;
        }

        // Method to remove a unit from the production queue (after it's produced)
        public void ClearProductionQueue()
        {
            foreach (QueueableItem qItem in ProductionQueue)
            {
                if (qItem is Unit)
                    myForm.unitManager.RemoveUnit((Unit)qItem);
                if (qItem is Villager)
                    myForm.villagerManager.RemoveUnit((Villager)qItem);

                qItem.Dispose();
            }
            
            ProductionQueue.Clear();
        }

        // Method to show production queue for testing purposes // needs updating for Unit instead of string placeholder
        public void ShowUnitProductionQueue()
        {
            if (ProductionQueue.Count > 0)
            {
                // Convert the queue to a comma-separated string
                string queueContents = string.Join(", ", ProductionQueue);

                // Display the contents in a message box
                MessageBox.Show($"Unit Production Queue: {queueContents}", "Queue Contents");
            }
            else
            {
                MessageBox.Show("The Unit Production Queue is empty.", "Queue Contents");
            }
        }

        // Method to calculate the total construction time with varying villagers
        public double CalculateDynamicBuildTime()
        {
            if (TimeStarted == null)
            {
                throw new InvalidOperationException("Construction has not started yet.");
            }

            double totalProgress = 0.0;  // Progress is between 0.0 and 1.0
            double totalTime = 0.0;      // Total time taken for the building to complete
            double previousTime = 0.0;   // Start from time 0

            if (villagerAllocations.Count == 0)
            {
                throw new InvalidOperationException("No villager allocations available for calculation.");
            }

            // Clear all intervals
            progressIntervals.Clear();
            // Initialize progress intervals with the start time
            progressIntervals.Add((TimeStarted.Value, 0.0));

            // Iterate through each villager allocation to calculate progress
            for (int i = 0; i < villagerAllocations.Count - 1; i++)
            {
                // Get current allocation
                var currentAllocation = villagerAllocations.ElementAt(i);
                double currentTime = currentAllocation.Key;
                int currentVillagers = currentAllocation.Value;

                // Get next allocation to calculate time interval
                var nextAllocation = villagerAllocations.ElementAt(i + 1);
                double nextTime = nextAllocation.Key;

                // Calculate time interval for this allocation
                double deltaT = nextTime - currentTime;

                if (deltaT > 0)
                {
                    // Calculate the rate of build progress for this interval
                    double buildRate = (2 + currentVillagers) / (3 * BaseBuildTime);

                    // Calculate the progress made during this interval
                    double progress = buildRate * deltaT;
                    totalProgress += progress;
                    totalTime += deltaT;

                    // Add to progress intervals
                    progressIntervals.Add((nextTime, Math.Min(totalProgress * 100, 100.0))); // Ensure progress does not exceed 100%

                    // If progress reaches or exceeds 100%, building is complete
                    if (totalProgress >= 1.0)
                    {
                        double excessProgress = totalProgress - 1.0;
                        double excessTime = excessProgress / buildRate;
                        totalTime -= excessTime;

                        // Set TimeFinished based on the calculated time
                        TimeFinished = TimeStarted.Value + totalTime;

                        // Record final interval
                        progressIntervals.Add((TimeFinished.Value, 100.0));
                        return totalTime;
                    }
                }

                // Update the previous time to the next allocation time
                previousTime = nextTime;
            }

            // If the building isn't finished after processing all allocations, continue calculating to 100% progress
            if (totalProgress < 1.0)
            {
                var lastAllocation = villagerAllocations.Last();
                double lastTime = lastAllocation.Key;
                int lastVillagers = lastAllocation.Value;

                double buildRate = (2 + lastVillagers) / (3 * BaseBuildTime);
                double remainingProgress = 1.0 - totalProgress;
                double remainingTimeToFinish = remainingProgress / buildRate;

                totalTime += remainingTimeToFinish;
                TimeFinished = TimeStarted.Value + totalTime;

                // Add the final progress interval
                progressIntervals.Add((TimeFinished.Value, 100.0));
            }

            return totalTime;
        }

        public SortedList<double, int> GetVillagerAllocations()
        {
            Debug.WriteLineIf(DebugConfig.GetVillagerAllocations, "\n///////////// GetVillagerAllocations() called /////////////");

            if (villagerAllocations.Count == 0)
            {
                Debug.WriteLineIf(DebugConfig.GetVillagerAllocations, "No villager allocations found.");
            }
            else
            {
                Debug.WriteLineIf(DebugConfig.GetVillagerAllocations, $"Villager allocations count: {villagerAllocations.Count}");
                foreach (var allocation in villagerAllocations)
                {
                    Debug.WriteLineIf(DebugConfig.GetVillagerAllocations, $"Time: {allocation.Key}s, Villagers: {allocation.Value}");
                }
            }

            return villagerAllocations;
        }


        public void Dispose()
        {
            // Dispose of the PictureBox if it exists
            if (PlacedPictureBox != null)
            {
                myForm?.drawingPanel.Controls.Remove(PlacedPictureBox); // Remove from parent control
                PlacedPictureBox.Dispose();
                PlacedPictureBox = null; // Release the reference
            }

            // Dispose of the popup buttons and unit panel
            PopupButtonManager.Destroy();
            UnitPanelManager.Dispose();
            SettingsPanelManager.Dispose();
            VillagerAllocationPanelManager.Dispose();

            // Clear any other references or lists
            villagerAllocations?.Clear();
            progressIntervals?.Clear();
            listOfUnits?.Clear();

            ClearProductionQueue();

            // Set other fields to null if they hold references
            myForm = null;
        }

    }
}
