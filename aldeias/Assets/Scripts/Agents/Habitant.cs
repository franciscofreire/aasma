using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Habitant : Agent {
	public WorldInfo.Tribe tribe;
	public float affinity; // 0: Complete Civil; 1: Complete Warrior
	public bool  isLeader;
	public bool  carryingFood;
	public bool  carryingWood;

	public Habitant(Vector2 pos, WorldInfo.Tribe tribe, float affinity): base(pos) {
		this.tribe  = tribe;
		this.affinity = affinity;
		this.isLeader = false;
	}
    public override bool IsAlive() {
        return true;
    }
    public override void Die() {
        //TODO
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
		if (TreeInFront())
			return new CutTree(this, sensorData.FrontCell);

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
                if (a.pos.Equals (sensorData.FrontCell)) {
                    return true;
                }
            }
        }
        return false;
	}

    private bool StumpInFront() {
        // TODO
        return false;
    }

	private bool UnclaimedTerritoryInFront() {
        return worldInfo.isUnclaimedTerritory(sensorData.FrontCell);
    }

    private bool carryingResources() {
        return carryingFood || carryingWood;
    }

	private bool LowEnergy() {
		return this.energy <= CRITICAL_ENERGY_LEVEL;
	}

    private bool FoodInFront() {
        foreach(WorldInfo.Habitat h in worldInfo.habitats) {
            foreach(Agent a in h.animals) {
                if (a.pos.Equals (sensorData.FrontCell) && !a.IsAlive()) {
                    return true;
                }
            }
        }
        return false;
    }
}