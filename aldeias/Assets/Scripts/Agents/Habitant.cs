using UnityEngine;

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
		//TODO
	}

	//************
	//***SENSORS**
	//************

	public bool EnemyInFront() {
		Vector2 posInFront = pos + orientation.ToVector2();
		int posInFront_x = (int)(posInFront.x + 0.5f);
		int posInFront_z = (int)(posInFront.y + 0.5f);
		Habitant habInFront = worldInfo.habitantInTile(posInFront_x, posInFront_z);
		return habInFront != null && habInFront.tribe != this.tribe;
	}
}