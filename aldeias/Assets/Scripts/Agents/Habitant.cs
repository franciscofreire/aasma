using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Habitant : Agent {
	public WorldInfo.Tribe tribe;
	public float  affinity; // 0: Complete Civil; 1: Complete Warrior
	public bool   isLeader;
    public bool carryingFood;
    public bool carryingWood;

	public Habitant(Vector2 pos, WorldInfo.Tribe tribe, float affinity): base(pos) {
		this.tribe  = tribe;
		this.affinity = affinity;
		this.isLeader = false;
	}
    public override bool IsAlive() {
        return true;
    }

	public override Action doAction() {
		/*	if (enemy-in-front? or animal-in-front?) and low-energy?
				move();
			else if	enemy-in-front? or animal-in-front?
				attack();
			else if tree-in-front?
				crop-tree();
            else if stump-in-front?
                collectWood();
            else if food-in-front?
                collectFood();
            else if meeting-point-in-front and carrying-resources?
                dropResources();
			else if unclaimed-territory-in-front?
				place-flag()
			else
				move();
		*/
        
        int index = worldInfo.rnd.Next(sensorData.Cells.Count);
        
        Vector2I target = sensorData.Cells[index];
        
        return new Walk(this, target);

	}

	public override void OnWorldTick () {
		sensorData.Cells = worldInfo.nearbyFreeCells(worldInfo.nearbyCells(this));

        Action a = doAction();
        a.apply();

	}


	//*************
	//** SENSORS **
	//*************
	
	public override bool EnemyInFront() {
		Vector2 posInFront = pos + orientation.ToVector2();
		Vector2I tileCoordInFront = worldInfo.AgentPosToWorldXZ(posInFront);
		Habitant habInFront = worldInfo.habitantInTile(tileCoordInFront);
		return habInFront != null && habInFront.tribe != this.tribe;
	}

	private bool AnimalInFront() {
		// FIXME : Use sensorData (and worldinfo hasAgent?)
        Vector2 posInFront = pos + orientation.ToVector2();
        Vector2I tileCoordInFront = worldInfo.AgentPosToWorldXZ(posInFront);

        foreach(WorldInfo.Habitat h in worldInfo.habitats) {
            foreach(Agent a in h.animals) {
                if (a.pos.Equals (tileCoordInFront)) {
                    return true;
                }
            }
        }
        return false;
	}

	private bool TreeInFront() {

        return false;
	}

    private bool StumpInFront() {
        // TODO
        return false;
    }

	private bool UnclaimedTerritoryInFront() {
        // FIXME : Use sensorData (and worldinfo hasAgent?)
        Vector2 posInFront = pos + orientation.ToVector2();
        Vector2I tileCoordInFront = worldInfo.AgentPosToWorldXZ(posInFront);

        return worldInfo.isUnclaimedTerritory(tileCoordInFront);
    }

    private bool carryingResources() {
        return carryingFood || carryingWood;
    }

	private bool LowEnergy() {
		return this.energy <= CRITICAL_ENERGY_LEVEL;
	}

    private bool FoodInFront() {
        // TODO
        return false;
    }
}