using UnityEngine;

public class Habitant : Agent {
	public string tribeId;
	public float  affinity; // 0: Complete Civil; 1: Complete Warrior
	public bool   isLeader;

	public Habitant(Vector2 pos, string tribeId, float affinity): base(pos) {
		this.tribeId  = tribeId;
		this.affinity = affinity;
		this.isLeader = false;
	}

	public override void doAction() {
		//TODO
	}
}
