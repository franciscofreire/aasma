using UnityEngine;

public class Animal : Agent {
	public Animal(Vector2 pos): base(pos) {
	}

	public override Action doAction() {
		//TODO
		return null;
	}

	public override void OnWorldTick () {
		Vector2 sum = pos+Vector2.right;
		pos = new Vector2(sum.x%worldInfo.xSize, sum.y);
	}

	
	//*************
	//** SENSORS **
	//*************

	public override bool EnemyInFront() {
		return false;
	}
}