using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// An Habitant is an Agent that starts as part of a Tribe.
//    He can Pickup resources as long as he is able to carry them. Otherwise he won't pick them up and they will fall.
//    He can then Drop some or all of it wherever he wants. If he doesn't have enough of the resource he won't drop anything.
//    He can pickup Food if he wants to.
//    He can also pickup Wood.
public class Habitant : Agent {
    private AgentImplementation agentImplReactive;
    private AgentImplementation agentImplDeliberative;

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
        agentImplReactive = new HabitantReactive(this);
        agentImplDeliberative = new HabitantDeliberative(this);

        worldInfo.AddHabitantDeletedListener(removeFromWorldInfo);
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
    //FIXME
	public override bool EnemyInFront () {
        return false;
    }

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
	public override bool LowEnergy() {
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
}