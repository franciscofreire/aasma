using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Habitant : Agent {
	public WorldInfo.Tribe tribe;
	public float  affinity; // 0: Complete Civil; 1: Complete Warrior
	public bool   isLeader;

	public Habitant(Vector2 pos, WorldInfo.Tribe tribe, float affinity): base(pos) {
		this.tribe  = tribe;
		this.affinity = affinity;
		this.isLeader = false;
	}

	public override Action doAction() {
		//TODO
		return null;
	}

	public override void OnWorldTick () {
		sensorData.Cells = worldInfo.nearbyFreeCells(worldInfo.nearbyCells(this));

		int index = worldInfo.rnd.Next(sensorData.Cells.Count);
		
		Vector2I target = sensorData.Cells[index];

		Action a = new Walk(this, target);
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
}