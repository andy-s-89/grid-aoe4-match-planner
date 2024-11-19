using Newtonsoft.Json.Linq;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net.Mime;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace grid_aoe4_match_planner
{
    public partial class Form1 : Form
    {
        // variables for handling grid within drawingPanel
        public const int DefaultGridSize = 50;
        public int GridSizeChange = 5;
        public const int MinGridSize = 10;
        public int globalGridSize;
        public int gridTime = 10; // each grid square = 10 seconds
        public int translationOffsetX = 0;
        public int translationOffsetY = 0;
        public Point panStartPoint;

        // variables for handling selections and click actions within drawingPanel
        public bool coordsEnabled = false;
        public bool isPanning = false;
        public bool isSelecting = false;
        public bool isPasting = false;
        public bool pasteMode = false;
        public Point selectionStartPoint;
        public Rectangle selectionRectangle;

        // variables for palette picture box dragging events
        public Point mouseDownLocation;
        public bool buildingIsDragging = false;
        public Image dragImage;
        public Point dragImageLocation;

        // variables for the custom tool tip and calculating costs from selected buildings/picboxes
        public CustomToolTipForm customToolTip;
        public Dictionary<string, int> totalCosts;
        public List<PlacedBuilding> selectedBuildingList = new List<PlacedBuilding>();
        public PlacedBuildingManager placedBuildingManager = new PlacedBuildingManager();

        // create new civ class instance, vill manager, resources etc
        public Civilization myCiv;
        public VillagerManager villagerManager;
        public UnitManager unitManager;
        public UpgradeManager upgradeManager;
        public TechnologyManager technologyManager;

        public int currentTimeInSeconds = 0;
        private int mouseYPosition = -1;

        public bool areUnitImagesLoaded = false;


        // TO DO

        // make button1 go back to civ flag select screen

        // add moveable sticky note labels, add key function to make selection window permanent and total cost popup

        // look into lag when panning and messed up pictureboxes

        // add copy paste and move with keys functions

        // Add x number of different panels to right hand side of drawing panel for:
        // - Properties - slider for grid scale, show dimension lines, show time on axis,
        //   centre pictureboxes etc.
        // - Selection set - Options for selection set, buildings, units and/or upgrades.
        //   show info on whats selected, total resource cost, number of buildings and units etc
        // - Villager manager
        // - Build order print out


        /////// custom tooltip ///////////
        // format the information properly, reduce size of icons - maybe remove name at top
        // reduce blank space all around form and in between items
        // CHANGE TOOLTIP ON DRAWINGPANEL TO ONLY SHOW ON SELECTION SOMEHOW (REDESIGN)
        // TOO OBNOXIOUS AND GETS IN WAY


        /// <summary>
        /// Loading Unit and Upgrade images and producing Unit selection panel from the selected building
        /// </summary>
        /// 
        // create system to show only 1 of each unit according to upgrades
        // add newline division between upgrades and units
        // get units available according to age available

        // global age up times to enable units and upgrades

        // create status bar at bottom for loading times and error reporting
        // create build order list on right hand side panel
        // create command prompt / console at bottom
        // for user input/instruction/feedback/error messages

        // Alter previous task record from -1 to the correct duration,
        // this might require a start time and finish time for tasks
        // Add resources each second using timer, make sure resources
        // can accept doubles and then shows ints rounded down

        // Have Resource bar only show once civ is selected
        // Display Resources + Total gather rates + Add editable Current Time
        // and slider on y axis
        // Display Vills and Tasks
        // Destroy vill customtooltip form on exit just like building tooltip
        // Add villager pictureboxes with name and taskrecord shown on hover
        // Taskrecord to be editable when clicked, datagridview possibly
        // Have method for getting every villagers current task at a given time
        // Add gather rate and ResourceType to TaskRecords
        // Add gather symbols for each vill in the grid and dimension lines

        // when moving placedbuilding picturebox redraw dimension lines properly
        // make dimension lines red? when building selected make them dashed?
        // centre picturebox between placed and finished?
        // Add dimension lines when dragging and dropping buildings with
        // text denoting TimePlaced, TimeFinished etc

        // Add units to selection set box in drawingPanel
        // Add options for selection sets (buildings only, units only etc)

        // Colour first 4 columns red, green, gold, grey for food, wood, gold, stone
        // With selected vills right click on the column to go to and have list menu for resources
        //  i.e. sheep in food, straggler in wood, main gold vein in gold etc etc
        // Include for drop off distance for multiple drop offs straggler to tc for example
        // Have selected or hovered vills point to when/where they gather with arrow line
        // On arrow line have symbol of the food source with gather rate and walking distance?
        // When mouse hovers in the 4 resource columns highlight vills allocated to there,
        //  possibly incorporate right click to select vills and move them to another column
        //  this could be with sliders from 1 - number currently on sheep, farms, deer etc if applicable
        //  add a node point to where right click happened and then dragging the node would move the header
        //  of the newly created arrow and then showing another list menu popup with available resource to select
        // Have settings option for maps and available resources, be able to type them in e.g. 10 sheep, 7 starting deer,
        //  3 stragglers, 1 main treeline with 80 trees, starting gold/stone with default distances from TC

        // Have y axis timeline snap to nearby events like TimePlaced

        // Lock TimeFinished > TimeStarted >= TimePlaced
        // Options to show only buildings, hide queue etc

        // In drawingpanel events combine update unit/tech/upgrade locations with queueable item if possible?
        // And draw dimensions in drawingpanel

        ////// Currently doing this... /////
        //
        // In queue panel be able to reorder queue, remove, add, clear. show timespan
        //  of unit on hover in tooltip style notification e.g "spearman 2:25-2:40"
        //  Have some form of shading over the units to denote minutes passed, in a <| <| kind of style
        //  Add little number on queue panel for each unit to index them
        // Have properties in PlacedBuilding whether panels are applicable to that building
        //  for example remove construct, vill alloc and chart for TC
        //  and remove rally for buildings with no units
        // For building progress chart have y axis be HP of building but still show progress%
        // Make BuildTime decimal for accurate calculations?
        //
        // Add functions for created queued units, right click remove from queue,
        //  hover show info, drag to reorder?
        // TimePlaced started finished doesnt change for moved PlacedBuildings
        //  when settings dialogue box isnt open
        // Mali unique buildings dont show up on selection tooltip, the whole tooltip needs revamping anyway

        // Once queue popup is closed with x button, building stays selected but delete key doesnt delete it

        // Have a queue time text on the create units popup to show what time the queue is at
        // Combine the Chart and Vill Allocations into 1 panel

        // Check PlacedBuilding class UpdateVillagerAllocations methods now the popup has been changed
        // Make all members of SettingsPopup (datetimepickers etc) and VillAllocationPanel
        //  (chart elements and grid) accessible and not nullable
        // Make sure settings and vill alloc panels work as intended
        // In settings/times panel have lock option and lock all boxes/dtps

        // Changing build time text box doesnt change TimeFinished

        // Tweaking vill alloc popup location and size
        // Reposition Time Button to right hand side and create rally flag button on left side
        // Vill alloc when placedbuilding created, added _iscreated flag to offset the initial
        //  additional TimeStarted, but found new additions on the villagerallocgrid when
        //  moving placedbuilding effecting TimeStarted


        public Form1()
        {
            // load form controls
            InitializeComponent();
            // load eco and unit managers
            InitilizeManagers();
            // Call the method to initialize the resources panel
            InitializeResourcePanel();

            // Enable key event handling for the form
            this.KeyPreview = true;

            // Assign the KeyDown event handler
            this.KeyDown += Form1_KeyDown;

            this.BackColor = Color.FromArgb(22, 22, 25);
            drawingPanel.AllowDrop = true;
            globalGridSize = DefaultGridSize;

            // Call async initialization method
            InitializeAsync();


            foreach (var villager in villagerManager.Villagers)
            {
                villager.SetState(0, VillagerState.Idle, 2);
                villager.AssignGatherTask(2, ResourceType.Food, 40);
                //villager.PrintTaskHistory();
                //MessageBox.Show($"Villager Name: {villager.Name}, State: {villager.State}, Task: {villager.Task}, Gather Rate: {villager.GatherRate}");
            }
        }

        // Function to load data and images async at load
        public async void InitializeAsync()
        {
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                Debug.WriteLineIf(DebugConfig.InitializeAsync, "\n///////////// InitializeAsync started /////////////");
                // Only load images from GitHub if folders have changed, otherwise load from cache
                Debug.WriteLineIf(DebugConfig.InitializeAsync, "Starting to load images...");
                // Start loading images for each folder in parallel, but assign the result to pictureBoxesBuildings etc
                Task<PictureBox[]> loadBuildingImagesTask = LoadImagesAsync("buildings", buildingColor);
                Task<PictureBox[]> loadUnitImagesTask = LoadImagesAsync("units", unitColor);
                Task<PictureBox[]> loadUpgradeImagesTask = LoadImagesAsync("upgrades", upgradeColor);
                Task<PictureBox[]> loadTechnologyImagesTask = LoadImagesAsync("technologies", upgradeColor);

                // Start loading data and the necessary images in parallel
                Debug.WriteLineIf(DebugConfig.InitializeAsync, "Loading data...");
                var loadDataTask = LoadDataAsync();

                // Wait for all tasks to complete
                Debug.WriteLineIf(DebugConfig.InitializeAsync, "Waiting for all tasks to complete...");
                await Task.WhenAll(loadDataTask, loadBuildingImagesTask, loadUnitImagesTask,
                                    loadUpgradeImagesTask, loadTechnologyImagesTask);

                // Assign pictureBoxesBuildings once the task completes
                pictureBoxesBuildings = await loadBuildingImagesTask;
                pictureBoxesUnits = await loadUnitImagesTask;
                pictureBoxesUpgrades = await loadUpgradeImagesTask;
                pictureBoxesTechnologies = await loadTechnologyImagesTask;


                // Set flags to indicate loading completion
                Debug.WriteLineIf(DebugConfig.InitializeAsync, "Setting loading flags...");
                hasDataLoaded = true;
                areImagesLoaded = true;
                areUnitImagesLoaded = true;

                sw.Stop();
                Debug.WriteLineIf(DebugConfig.InitializeAsync, $"Initialization completed in {sw.ElapsedMilliseconds} ms");

            }
            catch (Exception ex)
            {
                Debug.WriteLineIf(DebugConfig.InitializeAsync, $"An error occurred: {ex.Message}");
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        // create new civ class instance, vill manager, resources etc
        public void InitilizeManagers()
        {
            myCiv = new Civilization("English", new Resources(200, 200, 100, 0, 0, 10), 6); // new civ english with 6 vills
            villagerManager = new VillagerManager(6, 40, this); // 6 villagers with a gather rate of 40 per minute
            unitManager = new UnitManager();
            upgradeManager = new UpgradeManager();
            technologyManager = new TechnologyManager();
        }


        public async void button1_Click(object sender, EventArgs e)
        {
            // make this return to civ select screen


        }


        // add timer event to add resources every second
        //private void Timer_Tick(object sender, EventArgs e)
        //{
        //    Resources gatheredResources = villagerManager.GatherResourcesOverTime();
        //    currentResources.Add(gatheredResources); /60 possibly? would need doubles then round in UI
        //    UpdateUI();  // Refresh the UI to show updated resources
        //}


    }
}
