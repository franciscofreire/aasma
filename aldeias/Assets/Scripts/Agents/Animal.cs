using UnityEngine;

public class Animal : Agent {
    private bool isAlive;

	public Animal(Vector2 pos): base(pos) {
        this.isAlive = true;
        this.energy = 20;
    }

	public override Action doAction() {

        int index = worldInfo.rnd.Next(sensorData.Cells.Count);
        Vector2I target = sensorData.Cells[index];
        return new Walk(this, target);
	}

	public override void OnWorldTick () {
		//Vector2 sum = pos+Vector2.right;
		//pos = new Vector2(sum.x%worldInfo.xSize, sum.y);
        /*updateSensorData();
        Action a = doAction();
        a.apply();*/
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