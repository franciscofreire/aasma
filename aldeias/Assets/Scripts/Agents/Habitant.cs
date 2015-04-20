using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Habitant : Agent {
	public WorldInfo.Tribe tribe;
	public float affinity; // 0: Complete Civil; 1: Complete Warrior
	public bool  isLeader;
	public bool  isCarryingFood;
	public bool  isCarryingWood;

	public Habitant(Vector2 pos, WorldInfo.Tribe tribe, float affinity): base(pos) {
		this.tribe  = tribe;
		this.affinity = affinity;
		this.isLeader = false;
	}

    public void logFrontCell() {
        Debug.Log("Agent & Front: " + pos + " ; (" +
                  sensorData.FrontCell.x + "," + sensorData.FrontCell.y + ")");
    }

    public override bool IsAlive() {
        return true;
    }
    public override void Die() {
        //TODO
    }

	public override Action doAction() {
        if ((EnemyInFront() || AnimalInFront()) && LowEnergy()) {
            // Reactive agent: Flee randomly
            int fleeIndex = worldInfo.rnd.Next(sensorData.Cells.Count);
            Vector2I fleeTarget = sensorData.Cells[fleeIndex];
            return new Walk(this, fleeTarget);
        }
        else if (EnemyInFront() || AnimalInFront()) {
            return new Attack(this, sensorData.FrontCell);
        }
        else if (FoodInFront() && !CarryingResources()) {
            return new PickupFood(this, sensorData.FrontCell);
        }
        else if (CutDownTreeWithWoodInFront() && !CarryingResources()) {
            return new PickupTree(this, sensorData.FrontCell);
        }
        else if (AliveTreeInFront() && !CarryingResources()) {
            return new CutTree(this, sensorData.FrontCell);
        }
        else if (MeetingPointInFront() && isCarryingFood) {
            return new DropFood(this, sensorData.FrontCell);
        }
        else if (MeetingPointInFront() && isCarryingWood) {
            return new DropTree(this, sensorData.FrontCell);
        }
        else if (UnclaimedTerritoryInFront()) {
            return new PlaceFlag(this, sensorData.FrontCell);
        }
            
            // Reactive agent: Walk randomly
            int index = worldInfo.rnd.Next(sensorData.Cells.Count);
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
                if (a.pos.Equals (sensorData.FrontCell.ToVector2()) && a.IsAlive()) {
                    return true;
                }
            }
        }
        return false;
	}

	private bool UnclaimedTerritoryInFront() {
        return worldInfo.isUnclaimedTerritory(sensorData.FrontCell);
    }

    public bool CarryingResources() {
        return isCarryingFood || isCarryingWood;
    }

	private bool LowEnergy() {
		return this.energy <= CRITICAL_ENERGY_LEVEL;
	}

    private bool FoodInFront() {
        foreach(WorldInfo.Habitat h in worldInfo.habitats) {
            foreach(Agent a in h.animals) {
                if (a.pos.Equals (sensorData.FrontCell.ToVector2()) && !a.IsAlive()) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool MeetingPointInFront() {
        foreach(Vector2I cell in tribe.meetingPoint.meetingPointCells) {
            if (Vector2I.Equal(cell, sensorData.FrontCell)) {
            return true;
            }
        }
        return false;
    }
}