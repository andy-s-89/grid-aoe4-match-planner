using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace grid_aoe4_match_planner
{
    public class VillagerManager
    {
        public List<Villager> Villagers { get; private set; }

        private List<string> maleNames = new List<string>
        {
        "Adam", "Bob", "Charles", "David", "Edward", "Frank", "George", "Henry", "Ian", "Jack", "Kevin", "Liam",
        "Michael", "Nathan", "Oscar", "Peter", "Quincy", "Roger", "Samuel", "Thomas", "Ulysses", "Victor", "William",
        "Xander", "Yusuf", "Zach"
        };

        private List<string> femaleNames = new List<string>
        {
        "Alice", "Beatrice", "Clara", "Diana", "Eva", "Fiona", "Grace", "Helen", "Isla", "Julia", "Katherine", "Lily",
        "Mia", "Nina", "Olivia", "Paula", "Queenie", "Rose", "Sophia", "Tina", "Uma", "Violet", "Wendy", "Xena", "Yara",
        "Zoe"
        };

        public VillagerManager(int initialCount, double initialGatherRate, Form1 myForm)
        {
            Villagers = new List<Villager>();
            Random random = new Random(); // To randomly assign male or female

            for (int i = 0; i < initialCount; i++)
            {
                char startingLetter = (char)('A' + i); // Calculate the starting letter based on index
                bool isMale = random.Next(2) == 0; // Randomly decide if the villager is male or female

                string name = isMale ? GetNameByLetter(maleNames, startingLetter) : GetNameByLetter(femaleNames, startingLetter);


                // Assign a default state (e.g., Idle) for each villager initially
                Villager vill = new Villager(name, 0, i, VillagerState.Idle, ResourceType.Food, initialGatherRate);
                vill.QueuedTime = -myForm.gridTime;
                Villagers.Add(vill);
            }
        }

        public Villager CreateVillagerFromTC(PlacedBuilding TC, UnitData unitData)
        {
            Random random = new Random(); // To randomly assign male or female

            char startingLetter = (char)('A' + Villagers.Count); // Calculate the starting letter based on index
            bool isMale = random.Next(2) == 0; // Randomly decide if the villager is male or female

            string name = isMale ? GetNameByLetter(maleNames, startingLetter) : GetNameByLetter(femaleNames, startingLetter);

            int duration = unitData.costs.time;

            Villager vill = new Villager(name, (int)TC.TimeFinished + TC.GetTotalQueueTime() + duration, TC.Column, VillagerState.Idle, ResourceType.Food, 40);
            vill.Duration = duration;
            vill.Parent = TC;
            vill.QueuedTime = (int)TC.TimeFinished + TC.GetTotalQueueTime();
            Villagers.Add(vill);

            return vill;
        }


        // Helper method to get a name from the list starting with the specific letter
        private string GetNameByLetter(List<string> names, char startingLetter)
        {
            foreach (var name in names)
            {
                if (name.StartsWith(startingLetter.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return name;
                }
            }
            return "Unknown"; // Fallback in case no name is found
        }


        // Method to set a villager to idle
        public void SetVillagerIdle(int villagerIndex, int startTime, int durationInSeconds)
        {
            if (villagerIndex >= 0 && villagerIndex < Villagers.Count)
            {
                Villagers[villagerIndex].SetState(startTime, VillagerState.Idle, durationInSeconds);
            }
        }


        // Assign villager to a new task
        public void AssignGatherTask(int villagerIndex, int startTime, ResourceType newTask, double newGatherRate)
        {
            if (villagerIndex >= 0 && villagerIndex < Villagers.Count)
            {
                Villagers[villagerIndex].Task = newTask;
                Villagers[villagerIndex].GatherRate = newGatherRate;
                Villagers[villagerIndex].SetState(startTime, VillagerState.Gathering);
            }
        }

        // Assign villager to a new task
        public void AssignWalkingTask(int villagerIndex, int startTime, int durationInSeconds)
        {
            if (villagerIndex >= 0 && villagerIndex < Villagers.Count)
            {
                Villagers[villagerIndex].SetState(startTime, VillagerState.Traveling, durationInSeconds);
            }
        }

        // Calculate resources gathered per minute by all villagers
        public Resources GatherResourcesOverTime()
        {
            Resources gatheredResources = new Resources(0, 0, 0, 0, 0, 0);
            foreach (var villager in Villagers)
            {
                // Check if the villager is currently gathering resources
                if (villager.State == VillagerState.Gathering)
                {
                    switch (villager.Task)
                    {
                        case ResourceType.Food:
                            gatheredResources.Food += (int)villager.GatherRate;
                            break;
                        case ResourceType.Wood:
                            gatheredResources.Wood += (int)villager.GatherRate;
                            break;
                        case ResourceType.Gold:
                            gatheredResources.Gold += (int)villager.GatherRate;
                            break;
                        case ResourceType.Stone:
                            gatheredResources.Stone += (int)villager.GatherRate;
                            break;
                        case ResourceType.OliveOil:
                            gatheredResources.OliveOil += (int)villager.GatherRate;
                            break;
                    }
                }
            }
            return gatheredResources;
        }

        // Remove a specific Villager from the list
        public void RemoveUnit(Villager villager)
        {
            Villagers.Remove(villager);
        }

        // Method to find a villager by their PictureBox
        public Villager GetVillagerByPictureBox(PictureBox pictureBox)
        {
            return Villagers.FirstOrDefault(v => v.ItemPictureBox == pictureBox);
        }
    }
}