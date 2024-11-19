using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using System.Collections;

namespace grid_aoe4_match_planner
{
    public class TownCenter : PlacedBuilding
    {
        public TownCenter(building buildingInfo, double timePlaced, int column) : base(buildingInfo, timePlaced, column)
        {
            
        }

        // List to store rally points, each with a time and resource type for villagers
        public List<(double Time, string Resource)> RallyPoints { get; set; } = new List<(double Time, string Resource)>();

        // Method to add a rally point
        public void AddRallyPoint(double time, string resource)
        {
            RallyPoints.Add((time, resource));
        }

        // Method to get the next rally point based on time
        public (double Time, string Resource)? GetNextRallyPoint(double currentTime)
        {
            return RallyPoints.FirstOrDefault(r => r.Time > currentTime);
        }

        // Additional methods for managing rally points could be added as needed
    }

}
