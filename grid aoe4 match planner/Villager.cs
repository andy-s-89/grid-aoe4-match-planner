using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grid_aoe4_match_planner
{
    public class Villager : QueueableItem
    {
        public Form1 myForm = Application.OpenForms.OfType<Form1>().FirstOrDefault();
        public int BirthTime { get; set; }
        public int Column { get; set; }
        public VillagerState State { get; set; }
        public ResourceType Task { get; set; }
        public double GatherRate { get; set; }
        public List<TaskRecord> TaskHistory { get; set; } // List to keep track of task history

        public Villager(string name, int birthTime, int column, VillagerState initialState, ResourceType task, double gatherRate)
        {
            Name = name;
            BirthTime = birthTime;
            Column = column;
            State = initialState;
            Task = task;
            GatherRate = gatherRate;
            TaskHistory = new List<TaskRecord>(); // Initialize task history list

            // Log initial state with 0 seconds duration
            TaskHistory.Add(new TaskRecord(0, initialState));
        }

        // Add associated picture box thats shown in the drawingPanel
        public void AddPictureBox(PictureBox villagerPictureBox)
        {
            ItemPictureBox = villagerPictureBox;
        }

        // Assign villager to a new task
        public void AssignGatherTask(int startTime, ResourceType newTask, double newGatherRate)
        {
            Task = newTask;
            GatherRate = newGatherRate;
            SetState(startTime, VillagerState.Gathering);
        }

        public void SetState(int startTime, VillagerState newState, int durationInSeconds = -1, PlacedBuilding building = null)
        {
            // If the new state is indefinite, do not change the state unless a new command is given
            if (durationInSeconds == -1)
            {
                // Add the state with indefinite duration to the history
                TaskHistory.Add(new TaskRecord(startTime, newState, -1, Task, building));
            }
            else
            {
                // Add the state with the provided duration to the history
                TaskHistory.Add(new TaskRecord(startTime, newState, durationInSeconds, Task, building));
            }
        }

        public void PrintTaskHistory()
        {
            // Assuming 'TaskHistory' is a List<TaskRecord>
            string TaskHistoryDetails = string.Join(Environment.NewLine, TaskHistory.Select(task => task.ToString()));

            // Show the task history in a message box
            MessageBox.Show(TaskHistoryDetails, "Villager Task History");

        }

        public override void Dispose()
        {
            myForm.villagerManager.RemoveUnit(this);

            if (ItemPictureBox != null)
            {
                myForm?.drawingPanel.Controls.Remove(ItemPictureBox); // Remove from parent control
                ItemPictureBox.Dispose();
                ItemPictureBox = null; // Release the reference
            }
        }
    }

    // Enum to represent different villager states
    public enum VillagerState
    {
        Gathering,
        Idle,
        Building,
        Traveling
    }

    public enum ResourceType
    {
        Food,
        Wood,
        Gold,
        Stone,
        OliveOil,
        None
    }

    // Class to represent a task record
    public class TaskRecord
    {
        public int StartTime { get; set; }
        public VillagerState TaskState { get; set; }
        public int DurationInSeconds { get; set; } // -1 represents indefinite duration
        public ResourceType Resource { get; set; }        
        public PlacedBuilding Building { get; set; } // Optional: for building tasks

        public TaskRecord(int startTime, VillagerState taskState, int durationInSeconds = -1,
                            ResourceType resource = ResourceType.None, PlacedBuilding building = null)
        {
            StartTime = startTime;
            TaskState = taskState;
            Resource = resource;
            DurationInSeconds = durationInSeconds;
            Building = building;
        }

        public override string ToString()
        {
            string taskDetails;
            if (TaskState == VillagerState.Gathering && Resource != ResourceType.None)
            {
                if (DurationInSeconds == -1)
                {
                    taskDetails = $"{TaskState} {Resource}, Indefinitely";
                }
                else
                {
                    taskDetails = $"{TaskState} {Resource}, Started at: {StartTime} for {DurationInSeconds} seconds";
                }
            }
            else
            {
                if (DurationInSeconds == -1)
                {
                    taskDetails = $"{TaskState}, Indefinitely";
                }
                else
                {
                    taskDetails = $"{TaskState}, Started at: {StartTime} for {DurationInSeconds} seconds";
                }

                if (Building != null)
                {
                    taskDetails += $", Building: {Building.BuildingInfo.name}";
                }
            }
            return taskDetails;
        }

    }
}
