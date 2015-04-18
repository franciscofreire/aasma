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
		IList<Vector2I> cells = worldInfo.nearbyFreeCells(worldInfo.nearbyCells(this));

		int index = worldInfo.rnd.Next(cells.Count);

		Vector2I cell = cells[index];
		move(this, worldInfo.WorldXZToAgentPos(cell));
	}

	//************
	//***SENSORS**
	//************

	public bool EnemyInFront() {
		Vector2 posInFront = pos + orientation.ToVector2();
		Vector2I tileCoordInFront = worldInfo.AgentPosToWorldXZ(posInFront);
		Habitant habInFront = worldInfo.habitantInTile(tileCoordInFront);
		return habInFront != null && habInFront.tribe != this.tribe;
	}
}