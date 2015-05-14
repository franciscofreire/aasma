using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// An Habitant is an Agent that starts as part of a Tribe.
//    He can Pickup resources as long as he is able to carry them. Otherwise he won't pick them up and they will fall.
//    He can then Drop some or all of it wherever he wants. If he doesn't have enough of the resource he won't drop anything.
//    He can pickup Food if he wants to.
//    He can also pickup Wood.
public class Habitant : Agent {
    public readonly HabitantReactive agentImplReactive;
    public readonly HabitantDeliberative agentImplDeliberative;

    public SensorData sensorData;
    //public SensorInfo Sensors;

	public Tribe tribe;

	public static readonly Weight MAXIMUM_CARRIED_WEIGHT = new Weight(200);
	public FoodQuantity carriedFood;
	public WoodQuantity carriedWood;

	public bool CarryingFood {
		get {
			return (carriedFood != FoodQuantity.Zero);
		}
	}
	public bool CarryingWood {
		get {
			return (carriedWood != WoodQuantity.Zero);
		}
	}
	public Weight CarriedWeight {
		get {
			return carriedFood.Weight + carriedWood.Weight;
		}
	}

	public float affinity; // 0: Complete Civil; 1: Complete Warrior
	public bool  isLeader;

    public int tombstoneTickCounter = 0;
    public static readonly int MAX_TOMBSTONE = 50;
    public bool OldTombstone {
        get {
            return tombstoneTickCounter > MAX_TOMBSTONE;
        }
    }

    public void UpdateTombstoneCounter() {
        if(!Alive) {
            tombstoneTickCounter++;
        }
    }

	public static readonly Energy INITIAL_ENERGY = new Energy(100);

	public Habitant(WorldInfo world, Vector2 pos, Tribe tribe, float affinity): base(world, pos, INITIAL_ENERGY) {
		this.tribe  = tribe;
		this.affinity = affinity;
		this.isLeader = false;
        this.tribe = tribe;
        this.agentImplReactive = new HabitantReactive(this);
        this.agentImplDeliberative = new HabitantDeliberative(this);

        worldInfo.AddHabitantDeletedListener(removeFromWorldInfo);
	}

    public override void updateSensors() {
        IList<Tree> _trees;
        IList<Tree> _stumps;
        IList<Habitant> _enemies;
        IList<Animal> _animals;
        IList<Animal> _food;
        IList<Vector2I> _far_away_cells;
        IList<Vector2I> _meeting_point_cells;
        IList<Vector2I> _unclaimed_cells;
        IList<KeyValuePair<Vector2I,Tribe>> _territories;

        sensorData.NearbyCells = worldInfo.nearbyCellsInfo(
                this, 
                out _far_away_cells, 
                out _trees,
                out _stumps,
                out _enemies,
                out _animals,
                out _food,
                out _meeting_point_cells,
                out _unclaimed_cells,
                out _territories
        );
        sensorData.Cells = worldInfo.nearbyFreeCells(sensorData.NearbyCells);
        sensorData.Trees = _trees;
        sensorData.Stumps = _stumps;
        sensorData.Enemies = _enemies;
        sensorData.Animals = _animals;
        sensorData.Food = _food;
        sensorData.FarAwayCells = _far_away_cells;
        sensorData.MeetingPointCells = _meeting_point_cells;
        sensorData.UnclaimedCells = _unclaimed_cells;
        sensorData.Territories = _territories;
        

        sensorData.FoodTribe = new FoodQuantity(tribe.FoodStock.Count);
        sensorData.TribeFlags = tribe.FlagMachine.RemainingFlags;
        sensorData.TribeCellCount = tribe.cell_count;
        
        WorldTileInfo.TribeTerritory wti = 
            worldInfo.worldTiles.WorldTileInfoAtCoord(CoordConvertions.AgentPosToTile(this.pos)).tribeTerritory;
        sensorData.AgentIsInsideTribe = wti.IsClaimed && 
            wti.Flag.HasValue && (wti.Flag.Value.Tribe.Equals(tribe));
        sensorData.AgentTribe = tribe;

        sensorData.FillAdjacentCells (new Vector2I (pos));
        
        Vector2 posInFront = pos + orientation.ToVector2();
        Vector2I tileCoordInFront = CoordConvertions.AgentPosToTile(posInFront);
        sensorData.FrontCell = worldInfo.isInsideWorld(tileCoordInFront)
            ? tileCoordInFront
                : new Vector2I(pos); // VERIFYME: Not sure about this...
        
        Vector2 posAtLeft = pos + orientation.LeftOrientation().ToVector2();
        Vector2I tileCoordAtLeft = CoordConvertions.AgentPosToTile(posAtLeft);
        sensorData.LeftCell = worldInfo.isInsideWorld(tileCoordInFront)
            ? tileCoordAtLeft
                : new Vector2I(pos);
        
        Vector2 posAtRight = pos + orientation.RightOrientation().ToVector2();
        Vector2I tileCoordAtRight = CoordConvertions.AgentPosToTile(posAtRight);
        sensorData.RightCell = worldInfo.isInsideWorld(tileCoordInFront)
            ? tileCoordAtRight
                : new Vector2I(pos);
        
    } 

    public override void doAction() {
        if (worldInfo.Reactive)
            agentImplReactive.doAction();
        else
            agentImplDeliberative.doAction();
    }

    public override void removeFromWorldInfo() {
        // Remove agent reference in tile
        worldInfo.worldTiles.WorldTileInfoAtCoord(
            CoordConvertions.AgentPosToTile(pos)).Agent = null;

        // Remove agent from tribe
        tribe.RemoveHabitant(this);
    }

	public WoodQuantity PickupWood(WoodQuantity wood) {
		if(CanCarryWeight(wood.Weight)) {
			carriedWood = carriedWood + wood;
			return WoodQuantity.Zero;
		} else {
			return wood;
		}
	}

	public WoodQuantity DropWood(WoodQuantity wood) {
        if(carriedWood >= wood) {
            worldInfo.NotifyHabitantDroppedResourceListeners(this);
			carriedWood = carriedWood - wood;
			return wood;
		} else {
			return WoodQuantity.Zero;
		}
	}

	public FoodQuantity PickupFood(FoodQuantity food) {
		if(CanCarryWeight(food.Weight)) {
			carriedFood = carriedFood + food;
			return FoodQuantity.Zero;
		} else {
			return food;
		}
	}

	public FoodQuantity DropFood(FoodQuantity food) {
        if(carriedFood >= food) {
            worldInfo.NotifyHabitantDroppedResourceListeners(this);
			carriedFood = carriedFood - food;
			return food;
		} else {
			return FoodQuantity.Zero;
		}
	}
    
    public override void AnnounceDeath() {
        worldInfo.NotifyHabitantDiedListeners(this);
    }
    
    public override void AnnounceDeletion() {
        worldInfo.NotifyHabitantDeletedListeners(this);
    }

	//***************
	//** DECISIONS **
	//***************

    public void logFrontCell() {
        Logger.Log("Agent & Front: " + pos + " ; (" +
                       sensorData.FrontCell.x + "," + sensorData.FrontCell.y + ")",
                   Logger.VERBOSITY.AGENTS);
    }

    public override void OnWorldTick () {
        base.OnWorldTick();
        UpdateTombstoneCounter();
	}

	//*************
	//** SENSORS **
	//*************

	public bool EnemyInAdjacentPos(out Vector2I target) {
        target = new Vector2I(-1,-1);
        Vector2I frontCell = sensorData.FrontCell;
        Vector2I rightCell = sensorData.RightCell;
        Vector2I leftCell = sensorData.LeftCell;
        foreach(Habitant enemy in sensorData.Enemies) {
            Vector2I enemyPos = CoordConvertions.AgentPosToTile(enemy.pos);
            if(enemyPos == frontCell || enemyPos == rightCell || enemyPos == leftCell) {
                target = enemyPos;
                return true;
            } 
        }
        return false;
	}

    public bool AnimalInAdjacentPos(out Vector2I target) {
        target = new Vector2I(-1,-1);
        Vector2I frontCell = sensorData.FrontCell;
        Vector2I rightCell = sensorData.RightCell;
        Vector2I leftCell = sensorData.LeftCell;
        foreach(Animal animal in sensorData.Animals) {
            Vector2I animalPos = CoordConvertions.AgentPosToTile(animal.pos);
            if(animalPos == frontCell || animalPos == rightCell || animalPos == leftCell) {
                target = animalPos;
                return true;
            } 
        }
        return false;
	}

    public bool UnclaimedTerritoryInAdjacentPos(out Vector2I target) {
        if(UnclaimedTerritoryInPos(sensorData.FrontCell)) {
            target = sensorData.FrontCell;
            return true;
        }
        if(UnclaimedTerritoryInPos(sensorData.LeftCell)) {
            target = sensorData.LeftCell;
            return true;
        }
        if(UnclaimedTerritoryInPos(sensorData.RightCell)) {
            target = sensorData.RightCell;
            return true;
        }
        target = new Vector2I(-1,-1);
        return false;
    }

    public bool EnemyTerritoryInAdjacentPos(out Vector2I target) {
        if(EnemyTerritoryInPos(sensorData.FrontCell)) {
            target = sensorData.FrontCell;
            return true;
        }
        if(EnemyTerritoryInPos(sensorData.LeftCell)) {
            target = sensorData.LeftCell;
            return true;
        }
        if(EnemyTerritoryInPos(sensorData.RightCell)) {
            target = sensorData.RightCell;
            return true;
        }
        target = new Vector2I(-1,-1);
        return false;
    }
    
    public bool EnemyTerritoryInPos(Vector2I pos) {
        return worldInfo.isInsideWorld(pos) // Valid cell
            && worldInfo.worldTiles.WorldTileInfoAtCoord(pos)
               .tribeTerritory.IsClaimed // Occupied cell
            && !worldInfo.worldTiles.WorldTileInfoAtCoord(pos)
               .tribeTerritory.Flag.Value.Tribe.id.Equals(tribe.id) // Cell has an enemy flag
            && tribe.FlagMachine.CanMakeFlag(); // At least one flag available in tribe
    }
 
    public bool UnclaimedTerritoryInPos(Vector2I pos) {
        return worldInfo.isInsideWorld(pos) // Valid cell
            && !worldInfo.worldTiles.WorldTileInfoAtCoord(pos)
               .tribeTerritory.IsClaimed // Unoccupied cell
            && tribe.FlagMachine.CanMakeFlag(); // At least one flag available in tribe
    }

    public bool UnclaimedTerritoryAtLeft() {
        return worldInfo.isInsideWorld(sensorData.LeftCell) // Valid cell
            && !worldInfo.worldTiles.WorldTileInfoAtCoord(sensorData.LeftCell)
                .tribeTerritory.IsClaimed // Unoccupied cell
                && tribe.FlagMachine.CanMakeFlag(); // At least one flag available in tribe
    }
    public bool UnclaimedTerritoryAtRight() {
        return worldInfo.isInsideWorld(sensorData.RightCell) // Valid cell
            && !worldInfo.worldTiles.WorldTileInfoAtCoord(sensorData.RightCell)
                .tribeTerritory.IsClaimed // Unoccupied cell
                && tribe.FlagMachine.CanMakeFlag(); // At least one flag available in tribe
    }

    public bool AnimalsInFrontPositions() {
        foreach(Animal a in sensorData.Animals) {
            Vector2I animalPos = CoordConvertions.AgentPosToTile(a.pos);
            foreach(Vector2I sensorPos in sensorData.FarAwayCells) {
                if(animalPos == sensorPos) {
                    return true;
                }
            }
        }
        return false;
    }

    public bool EnemiesInFrontPositions() {
        foreach(Habitant h in sensorData.Enemies) {
            Vector2I enemyPos = CoordConvertions.AgentPosToTile(h.pos);
            foreach(Vector2I sensorPos in sensorData.FarAwayCells) {
                if(enemyPos == sensorPos) {
                    return true;
                }
            }
        }      
        return false;
    }

    public bool TreesInFrontPositions() {
        foreach(Tree t in sensorData.Trees) {
            foreach(Vector2I sensorPos in sensorData.FarAwayCells) {
                if(t.Pos == sensorPos) {
                    return true;
                }
            }
        }
        return false;
    }

    public bool FoodInFrontPositions() {
        foreach(Animal a in sensorData.Food) {
            Vector2I animalPos = CoordConvertions.AgentPosToTile(a.pos);
            foreach(Vector2I sensorPos in sensorData.FarAwayCells) {
                if(animalPos == sensorPos) {
                    return true;
                }
            }
        }
        return false;
    }

    public bool IsInTribeTerritory() {
        Flag? flag = 
            worldInfo.worldTiles.WorldTileInfoAtCoord(
                CoordConvertions.AgentPosToTile(this.pos)).tribeTerritory.Flag;
        return flag.HasValue && flag.Value.Tribe.Equals(this.tribe);
    }

    public bool CarryingResources() {
        return CarryingFood || CarryingWood;
    }

    public bool CanCarryWeight(Weight w) {
        return CarriedWeight + w <= MAXIMUM_CARRIED_WEIGHT;
    }

	private static Energy CRITICAL_ENERGY_LEVEL = new Energy(50);
	public bool LowEnergy() {
		return this.energy <= CRITICAL_ENERGY_LEVEL;
	}

    public bool FoodInAdjacentPos(out Vector2I target) {
        target = new Vector2I(-1,-1);
        Vector2I frontCell = sensorData.FrontCell;
        Vector2I rightCell = sensorData.RightCell;
        Vector2I leftCell = sensorData.LeftCell;
        foreach(Animal animal in sensorData.Food) {
            Vector2I animalPos = CoordConvertions.AgentPosToTile(animal.pos);
            if(animalPos == frontCell || animalPos == rightCell || animalPos == leftCell) {
                target = animalPos;
                return true;
            } 
        }
        return false;
    }

    public bool MeetingPointInFront() {
		return tribe.meetingPoint.IsInMeetingPoint(sensorData.FrontCell);
	}

    public Vector2I closestCell(IEnumerable<Vector2I> cells) {
        Vector2I source = CoordConvertions.AgentPosToTile(pos);
        return cells
            .Aggregate((c1,c2)=>c2.DistanceTo(source)<c1.DistanceTo(source) ? c2 : c1);
    }
    
    public bool closeToTribe() {
        CellCoordsAround cca = new CellCoordsAround(CoordConvertions.AgentPosToTile(pos), worldInfo);
        IEnumerable<Vector2I> neighbors = cca.CoordsAtDistance(2);
        foreach (Vector2I candidate in neighbors) {
            Flag? flag = worldInfo.worldTiles.WorldTileInfoAtCoord(candidate).tribeTerritory.Flag;
            if (flag == null || !flag.Value.Tribe.id.Equals(this.tribe.id))
                return false;
        }
        return true;
    }

    public bool AliveTreeInFront() {
        WorldTileInfo t = worldInfo.worldTiles.WorldTileInfoAtCoord(sensorData.FrontCell);
        return t.HasTree && t.Tree.Alive;
    }
    
    public bool AliveTree(Vector2I cell) {
        WorldTileInfo t = worldInfo.worldTiles.WorldTileInfoAtCoord(cell);
        return t.HasTree && t.Tree.Alive;
    }
    
    public bool DeadTreeInFront() {
        WorldTileInfo t = worldInfo.worldTiles.WorldTileInfoAtCoord(sensorData.FrontCell);
        return t.HasTree && !t.Tree.Alive;
    }
    
    public bool DeadTree(Vector2I cell) {
        WorldTileInfo t = worldInfo.worldTiles.WorldTileInfoAtCoord(cell);
        return t.HasTree && !t.Tree.Alive;
    }

    public bool StumpWithWoodInFront() {
        WorldTileInfo t = worldInfo.worldTiles.WorldTileInfoAtCoord(sensorData.FrontCell);
        return t.HasTree && !t.Tree.Alive && t.Tree.HasWood;
    }
    
    public bool StumpWithWood(Vector2I cell) {
        WorldTileInfo t = worldInfo.worldTiles.WorldTileInfoAtCoord(cell);
        return t.HasTree && !t.Tree.Alive && t.Tree.HasWood;
    }
    
    public bool DepletedTree(Vector2I cell) {
        WorldTileInfo t = worldInfo.worldTiles.WorldTileInfoAtCoord(cell);
        return t.HasTree && !t.Tree.HasWood;
    }
}

public struct SensorData {

    //############################################################################

    public VisionInfo CurrentVisionInfo;
    public HabitantSelfInfo CurrentSelfInfo;
    public TribeStocksInfo CurrentTribeStocks;

    //############################################################################

    public struct VisionInfo {
        public List<Vector2I> VisibleCoords;
        public List<AnimalInfo> VisibleAnimals;
        public List<HabitantInfo> VisibleHabitants;
        public List<TreeInfo> VisibleTrees;
    }

    public struct AnimalInfo {
        public Vector2 Position;
        public bool IsAlive;
        public FoodQuantity Food;
        public Vector2I Coord {
            get {
                return CoordConvertions.AgentPosToTile(Position);
            }
        }
    }

    public struct HabitantInfo {
        public HabitantId Id;
        public Vector2 Position;
        public bool IsAlive;
        public Tribe Tribe;
        public Vector2I Coord {
            get {
                return CoordConvertions.AgentPosToTile(Position);
            }
        }
    }

    public struct TreeInfo {
        public Vector2I Coord;
        public bool IsAlive;
        public WoodQuantity Wood;
    }

    public struct HabitantSelfInfo {
        public Vector2 Position;
        public Orientation Orientation;
        public Energy Energy;
        public WoodQuantity CarriedWood;
        public FoodQuantity CarriedFood;
        public Vector2I Coord {
            get {
                return CoordConvertions.AgentPosToTile(Position);
            }
        }
    }

    public struct TribeStocksInfo {
        public WoodQuantity WoodStock;
        public FoodQuantity FoodStock;
    }

    //############################################################################

    public struct HabitantId {
        private Habitant habitant;
        public HabitantId(Habitant h) {
            habitant = h;
        }
    }

    //############################################################################

    private static List<Vector2I> VisibleCoordsForHabitant(Habitant h) {
        Vector2I center = CoordConvertions.AgentPosToTile(h.pos);
        Vector2I forward = new Vector2I(h.orientation.ToVector2() * 1.5f); /*Make sure it casts to unit Vector2I*/
        Vector2I right = new Vector2I(h.orientation.RightOrientation().ToVector2() * 1.5f); /*Same as previous line*/

        List<Vector2I> coordsList = new List<Vector2I>();
        coordsList.Add(center+right);
        coordsList.Add(center-right);

        for(int forwardDist = 1; forwardDist<=2; forwardDist+=1) {
            for(int rightDist = -1; rightDist<=1; rightDist+=1) {
                Vector2I centerDist =  forward.TimesScalar(forwardDist) + right.TimesScalar(rightDist);
                coordsList.Add(center+centerDist);
            }
        }

        return coordsList;
    }

    private static List<AnimalInfo> AnimalInfosFromWorldAtCoords(WorldInfo world, List<Vector2I> coords) {
        IEnumerable<Animal> animalsInCoords = world.AllAnimals.Where(a=>coords.Contains(CoordConvertions.AgentPosToTile(a.pos)));
        List<AnimalInfo> animalInfos = new List<AnimalInfo>();
        foreach(var animal in animalsInCoords) {
            animalInfos.Add(AnimalInfoFromAnimal(animal));
        }
        return animalInfos;
    }

    private static AnimalInfo AnimalInfoFromAnimal(Animal a) {
        AnimalInfo info = new AnimalInfo();
        info.Position = a.pos;
        info.IsAlive = a.Alive;
        info.Food = a.Food;
        return info;
    }

    private static List<HabitantInfo> HabitantInfosFromWorldAtCoordsExceptHabitant(WorldInfo world, List<Vector2I> coords, Habitant habitant) {
        IEnumerable<Habitant> habitantsInCoords = world.AllHabitants.Where(h=>coords.Contains(CoordConvertions.AgentPosToTile(h.pos)));
        IEnumerable<Habitant> habsExceptHabitant = habitantsInCoords.Where(h=>h!=habitant);
        List<HabitantInfo> habitantInfos = new List<HabitantInfo>();
        foreach(var h in habsExceptHabitant) {
            habitantInfos.Add(HabitantInfoFromHabitant(h));
        }
        return habitantInfos;
    }

    private static HabitantInfo HabitantInfoFromHabitant(Habitant h) {
        HabitantInfo info = new HabitantInfo();
        info.Id = new HabitantId(h);
        info.Position = h.pos;
        info.IsAlive = h.Alive;
        info.Tribe = h.tribe;
        return info;
    }

    private static List<TreeInfo> TreeInfosFromWorldAtCoords(WorldInfo world, List<Vector2I> coords) {
        IEnumerable<Vector2I> coordsWithTrees = coords.Where(c=>world.worldTiles.WorldTileInfoAtCoord(c).HasTree);
        IEnumerable<Tree> treesAtCoords = coordsWithTrees.Select(c=>world.worldTiles.WorldTileInfoAtCoord(c).Tree);
        List<TreeInfo> treeInfos = new List<TreeInfo>();
        foreach(var t in treesAtCoords) {
            treeInfos.Add (TreeInfoFromTree(t));
        }
        return treeInfos;
    }

    private static TreeInfo TreeInfoFromTree(Tree t) {
        TreeInfo info = new TreeInfo();
        info.Coord = t.Pos;
        info.IsAlive = t.Alive;
        info.Wood = t.Wood;
        return info;
    }

    private static VisionInfo VisionInfoForHabitant(Habitant h) {
        VisionInfo habVisionInfo = new VisionInfo();
        List<Vector2I> habitantVisibleCoords = VisibleCoordsForHabitant(h);
        habVisionInfo.VisibleCoords = habitantVisibleCoords;
        habVisionInfo.VisibleAnimals = AnimalInfosFromWorldAtCoords(h.worldInfo, habitantVisibleCoords);
        habVisionInfo.VisibleHabitants = HabitantInfosFromWorldAtCoordsExceptHabitant(h.worldInfo, habitantVisibleCoords, h);
        habVisionInfo.VisibleTrees = TreeInfosFromWorldAtCoords(h.worldInfo, habitantVisibleCoords);
        return habVisionInfo;
    }

    private static HabitantSelfInfo HabitantSelfInfoForHabitant(Habitant h) {
        HabitantSelfInfo info = new HabitantSelfInfo();
        info.Position = h.pos;
        info.Orientation = h.orientation;
        info.Energy = h.energy;
        info.CarriedFood = h.carriedFood;
        info.CarriedWood = h.carriedWood;
        return info;
    }

    private static TribeStocksInfo TribeStocksForHabitant(Habitant h) {
        TribeStocksInfo info = new TribeStocksInfo();
        info.FoodStock = h.tribe.FoodStock;
        info.WoodStock = h.tribe.WoodStock;
        return info;
    }

    //############################################################################

    public static SensorData CurrentSensorDataForHabitant(Habitant h) {
        SensorData sensorData = new SensorData();
        sensorData.CurrentVisionInfo = VisionInfoForHabitant(h);
        sensorData.CurrentSelfInfo = HabitantSelfInfoForHabitant(h);
        sensorData.CurrentTribeStocks = TribeStocksForHabitant(h);
        return sensorData;
    }

    //############################################################################


    private IList<Vector2I> _cells;
    private Vector2I _front_cell;
    private Vector2I _left_cell;
    private Vector2I _right_cell;
    private IList<Vector2I> _far_away_cells;
    private IList<Vector2I> _adjacent_cells;
    private IList<Vector2I> _meeting_point_cells;
    private IList<KeyValuePair<Vector2I,Tribe>> _territories; 
    private IList<Vector2I> _unclaimed_cells;
    private IList<Tree> _trees;
    private IList<Tree> _stumps;
    private IList<Habitant> _enemies;
    private IList<Animal> _animals;
    private IList<Animal> _food;
    private FoodQuantity _food_tribe;
    private int _tribe_cell_count;
    private int _tribe_flags;
    private bool _agent_is_inside_tribe;
    private Tribe _agent_tribe;
    
    public IList<Vector2I> NearbyCells { get; set; }
    
    public Tribe AgentTribe {
        get { return this._agent_tribe; }
        set { this._agent_tribe = value; }
    }
    
    public IList<Vector2I> Cells
    {
        get { return _cells; }
        set { _cells = value; }
    }
    public IList<Vector2I> MeetingPointCells
    {
        get { return _meeting_point_cells; }
        set { _meeting_point_cells = value; }
    }
    public IList<Vector2I> EnemyTribeCells
    {
        get { 
            IList<Vector2I> enemyTribeCells = new List<Vector2I>();
            foreach(var entry in Territories) {
                if(!entry.Value.Equals(AgentTribe)) {
                    enemyTribeCells.Add(entry.Key);
                }
            }
            return enemyTribeCells;
        }
    }
    public IList<Tree> Trees
    {
        get { return _trees; }
        set { _trees = value; }
    }
    public IList<Tree> Stumps
    {
        get { return _stumps; }
        set { _stumps = value; }
    }
    
    public IList<Habitant> Enemies
    {
        get { return _enemies; }
        set { _enemies = value; }
    }
    
    public IList<Animal> Animals
    {
        get { return _animals; }
        set { _animals = value; }
    }
    
    public IList<Animal> Food
    {
        get { return _food; }
        set { _food = value; }
    }
    
    public Vector2I FrontCell
    {
        get { return _front_cell; }
        set { _front_cell = value; }
    }
    
    public Vector2I LeftCell
    {
        get { return _left_cell; }
        set { _left_cell = value; }
    }
    
    public Vector2I RightCell
    {
        get { return _right_cell; }
        set { _right_cell = value; }
    }
    
    public IList<Vector2I> AdjacentCells {
        get { return _adjacent_cells; }
        set { _adjacent_cells = value; }
    }
    
    public IList<Vector2I> FarAwayCells {
        get { return _far_away_cells; }
        set { _far_away_cells = value; }
    }
    
    public IList<Vector2I> UnclaimedCells {
        get { return _unclaimed_cells; }
        set { _unclaimed_cells = value; }
    }
    
    public FoodQuantity FoodTribe {
        get { return _food_tribe; }
        set { _food_tribe = value; }
    }
    
    public int TribeFlags {
        get { return _tribe_flags; }
        set { _tribe_flags = value; }
    }
    
    public int TribeCellCount {
        get { return _tribe_cell_count; }
        set { _tribe_cell_count = value; }
    }
    
    public bool AgentIsInsideTribe {
        get { return _agent_is_inside_tribe; }
        set { _agent_is_inside_tribe = value; }
    }
    
    public IList<KeyValuePair<Vector2I,Tribe>> Territories {
        get { return _territories; }
        set { _territories = value; }
    }
    
    public void FillAdjacentCells (Vector2I agentPos) {
        if (Cells != null) {
            AdjacentCells = new List<Vector2I> ();
            foreach (Vector2I pos in Cells) {
                if (agentPos.isAdjacent (pos)) {
                    AdjacentCells.Add (pos);
                }
            }
        }
    }
}