using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// An Habitant is an Agent that starts as part of a Tribe.
//    He can Pickup resources as long as he is able to carry them. Otherwise he won't pick them up and they will fall.
//    He can then Drop some or all of it wherever he wants. If he doesn't have enough of the resource he won't drop anything.
//    He can pickup Food if he wants to.
//    He can also pickup Wood.
public class Habitant : Agent {

	public Tribe tribe;

	public static readonly Weight MaximumCarriedWeight = new Weight(200);
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

	public static readonly Energy INITIAL_ENERGY = new Energy(100);

	public Habitant(WorldInfo world, Vector2 pos, Tribe tribe, float affinity): base(world, pos, INITIAL_ENERGY) {
		this.tribe  = tribe;
		this.affinity = affinity;
		this.isLeader = false;
	}

	public WoodQuantity PickupWood(WoodQuantity wood) {
		if((CarriedWeight + wood.Weight) <= MaximumCarriedWeight) {
			carriedWood = carriedWood + wood;
			return WoodQuantity.Zero;
		} else {
			return wood;
		}
	}

	public WoodQuantity DropWood(WoodQuantity wood) {
		if(carriedWood >= wood) {
			carriedWood = carriedWood - wood;
			return wood;
		} else {
			return WoodQuantity.Zero;
		}
	}

	public FoodQuantity PickupFood(FoodQuantity food) {
		if((CarriedWeight + food.Weight) <= MaximumCarriedWeight) {
			carriedFood = carriedFood + food;
			return FoodQuantity.Zero;
		} else {
			return food;
		}
	}

	public FoodQuantity DropFood(FoodQuantity food) {
		if(carriedFood >= food) {
			carriedFood = carriedFood - food;
			return food;
		} else {
			return FoodQuantity.Zero;
		}
	}

	//***************
	//** DECISIONS **
	//***************

    public void logFrontCell() {
        Debug.Log("Agent & Front: " + pos + " ; (" +
                  sensorData.FrontCell.x + "," + sensorData.FrontCell.y + ")");
    }

	public override Action doAction() {
        if ((EnemyInFront() || AnimalInFront()) && LowEnergy()) {
            // Reactive agent: Flee randomly
            int fleeIndex = WorldRandom.Next(sensorData.Cells.Count);
            Vector2I fleeTarget = sensorData.Cells[fleeIndex];
            return new Walk(this, fleeTarget);
        }
        else if (EnemyInFront() || AnimalInFront()) {
            return new Attack(this, sensorData.FrontCell);
        }
        else if (FoodInFront() && !CarryingResources()) {
            return new PickupFood(this, sensorData.FrontCell);
        }
       // else if (CutDownTreeWithWoodInFront() && !CarryingResources()) {
       //     return new PickupTree(this, sensorData.FrontCell);
       // }
       // else if (AliveTreeInFront() && !CarryingResources()) {
       //     return new CutTree(this, sensorData.FrontCell);
       // }
        else if (MeetingPointInFront() && CarryingFood) {
            return new DropFood(this, sensorData.FrontCell);
        }
        else if (MeetingPointInFront() && CarryingWood) {
            return new DropTree(this, sensorData.FrontCell);
        }
        else if (UnclaimedTerritoryInFront()) {
            return new PlaceFlag(this, sensorData.FrontCell);
        }
        
        // Reactive agent: Walk randomly
        int index = WorldRandom.Next(sensorData.Cells.Count);
        Vector2I target = sensorData.Cells[index];
        return new Walk(this, target);
    }

	public override void OnWorldTick () {
		updateSensorData();

		Action a = doAction();
		a.apply();
	}

	//*************
	//** SENSORS **
	//*************
	
	public override bool EnemyInFront() {
		Habitant habInFront = worldInfo.habitantInTile(sensorData.FrontCell);
		return habInFront != null && habInFront.tribe != this.tribe;
	}

	private bool AnimalInFront() {
        foreach(WorldInfo.Habitat h in worldInfo.habitats) {
            foreach(Agent a in h.animals) {
                if (a.pos.Equals (sensorData.FrontCell.ToVector2()) && a.Alive) {
                    return true;
                }
            }
        }
        return false;
	}

	private bool UnclaimedTerritoryInFront() {
        return worldInfo.isInsideWorld(sensorData.FrontCell) 
			&& !worldInfo.worldTiles.WorldTileInfoAtCoord(sensorData.FrontCell).tribeTerritory.IsClaimed;
    }

    public bool CarryingResources() {
        return CarryingFood || CarryingWood;
    }

	private static Energy CRITICAL_ENERGY_LEVEL = new Energy(20);
	private bool LowEnergy() {
		return this.energy <= CRITICAL_ENERGY_LEVEL;
	}

    private bool FoodInFront() {
        foreach(WorldInfo.Habitat h in worldInfo.habitats) {
            foreach(Agent a in h.animals) {
                if (a.pos.Equals (sensorData.FrontCell.ToVector2()) && !a.Alive) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool MeetingPointInFront() {
		return tribe.meetingPoint.IsInMeetingPoint(sensorData.FrontCell);
	}
}