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
        sensorData = SensorData.CurrentSensorDataForHabitant(this);
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

    public bool CarryingResources() {
        return CarryingFood || CarryingWood;
    }
    
    public bool CanCarryWeight(Weight w) {
        return CarriedWeight + w <= MAXIMUM_CARRIED_WEIGHT;
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

    public override void OnWorldTick () {
        base.OnWorldTick();
        UpdateTombstoneCounter();
	}

	//*************
	//** SENSORS **
	//*************

	



	

    public Vector2I closestCell(IEnumerable<Vector2I> cells) {
        Vector2I source = CoordConvertions.AgentPosToTile(pos);
        return cells
            .Aggregate((c1,c2)=>c2.DistanceTo(source)<c1.DistanceTo(source) ? c2 : c1);
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
        public List<TerritoryInfo> VisibleTerritories;
        public List<AnimalInfo> VisibleAnimals;
        public List<HabitantInfo> VisibleHabitants;
        public List<TreeInfo> VisibleTrees;
    }

    public struct TerritoryInfo {
        public Vector2I Coord;
        public Tribe Owner;
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
        public Tribe Tribe;
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

    //############################################################################

    private static List<Vector2I> VisibleCoordsForHabitant(Habitant h) {
        Vector2I center = CoordConvertions.AgentPosToTile(h.pos);
        Vector2I forward = h.orientation.ToVector2I(); /*Make sure it casts to unit Vector2I*/
        Vector2I right = h.orientation.RightOrientation().ToVector2I(); /*Same as previous line*/

        List<Vector2I> coordsList = new List<Vector2I>();
        coordsList.Add(center+right);
        coordsList.Add(center-right);

        for(int forwardDist = 1; forwardDist<=2; forwardDist+=1) {
            for(int rightDist = -1; rightDist<=1; rightDist+=1) {
                Vector2I centerDist =  forward.TimesScalar(forwardDist) + right.TimesScalar(rightDist);
                Vector2I coord = center+centerDist;
                if (h.worldInfo.isInsideWorld(coord))
                    coordsList.Add(coord);
            }
        }

        return coordsList;
    }

    private static List<TerritoryInfo> TerritoryInfosFromWorldAtCoords(WorldInfo world, List<Vector2I> coords) {
        List<TerritoryInfo> infos = new List<TerritoryInfo>();
        foreach(var c in coords) {
            var info = new TerritoryInfo();
            info.Coord = c;
            var worldTerritory = world.worldTiles.WorldTileInfoAtCoord(c).tribeTerritory;
            info.Owner = worldTerritory.Flag.HasValue ? worldTerritory.Flag.Value.Tribe : null;
            infos.Add(info);
        }
        return infos;
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
        habVisionInfo.VisibleTerritories = TerritoryInfosFromWorldAtCoords(h.worldInfo, habitantVisibleCoords);
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
        info.Tribe = h.tribe;
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
}