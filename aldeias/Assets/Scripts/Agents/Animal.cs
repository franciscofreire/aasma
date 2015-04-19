using UnityEngine;

public class Animal : Agent {
    private bool isAlive;

	public Animal(Vector2 pos): base(pos) {
        this.isAlive = true;
    }

	public override Action doAction() {
		//TODO
		return null;
	}

	public override void OnWorldTick () {
		Vector2 sum = pos+Vector2.right;
		pos = new Vector2(sum.x%worldInfo.xSize, sum.y);
	}

    public override void Die() {
        this.isAlive = false;
        this.orientation = ORIENTATION.DOWN;
    }

    public override bool IsAlive() {
        return isAlive;
    } 
	//*************
	//** SENSORS **
	//*************

	public override bool EnemyInFront() {
		return false;
	}
}