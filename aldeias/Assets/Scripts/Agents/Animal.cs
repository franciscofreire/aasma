using UnityEngine;

public class Animal : Agent {

	public static readonly Energy INITIAL_ENERGY = new Energy(20);

	public Animal(Vector2 pos): base(pos, INITIAL_ENERGY) { }

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

	//*************
	//** SENSORS **
	//*************

	public override bool EnemyInFront() {
		return false;
	}
}