using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// An Agent starts on a position in a world with a given Energy with an orientation.
//    When he is attacked, a certain amount of his Energy is removed (RemoveEnergy).
//    He is Alive as long as his Energy is greater than zero.
//    At any given moment, his senses (SensorData) provide him a view of its WorldInfo.
//    When he is hungry, the Agent can Eat some food and will get some Energy from it.
public abstract class Agent {

	//Every agent runs on it's own thread.
	//Every frame, each agent is signaled to decide what action it is going to perform.
	//   Agents that take too much time to decide will have to cope with their actions being performed late.
	//Every frame, the sensor values of each agent are computed and made available to their agents.
	//   They might be timestamped.
	//Every decision cycle, each agent copies it's sensor data to internal memory.
	//The state of every agent must be swapped on each new frame.

	public WorldInfo worldInfo;

	public Vector2 pos;
	public Orientation orientation;
	public Energy energy; // 0: No energy; 100: Full energy

	public SensorData sensorData;

	public Agent (WorldInfo world, Vector2 pos, Energy e) {
		this.worldInfo = world;
		this.pos = pos;
		this.energy = e;
		this.orientation = ORIENTATION.UP;
	}

	public bool Alive {
		get { return energy > Energy.Zero; }
	}

    public void RemoveEnergy(Energy e) {
		energy.Subtract(e);
    }

	public void Eat(FoodQuantity food) {
		energy.Add(EnergyFromFood(food));
	}

	public abstract Action doAction();
	
	public abstract void OnWorldTick();

	public void updateSensorData() {
		sensorData.Cells = worldInfo.nearbyFreeCells(worldInfo.nearbyCells(this));
		
		Vector2 posInFront = pos + orientation.ToVector2();
		Vector2I tileCoordInFront = CoordConvertions.AgentPosToWorldXZ(posInFront);
		sensorData.FrontCell = worldInfo.isInsideWorld(tileCoordInFront)
			? tileCoordInFront
				: new Vector2I(pos); // VERIFYME: Not sure about this...
	}
	//*************
	//** SENSORS **
	//*************

	public abstract bool EnemyInFront();

    public bool AliveTreeInFront() {
		WorldTileInfo t = worldInfo.worldTiles.WorldTileInfoAtCoord(sensorData.FrontCell);
		return t.HasTree && t.Tree.Alive;
    }
    
    public bool CutDownTreeWithWoodInFront() {
		WorldTileInfo t = worldInfo.worldTiles.WorldTileInfoAtCoord(sensorData.FrontCell);
		return t.HasTree && !t.Tree.Alive && t.Tree.HasWood;
    }

	//FIXME: I don't know where to put this function as it is not part of the Agent. Or is it? It can also belong to the WorldInfo.
	public static Energy EnergyFromFood(FoodQuantity food) {
		return new Energy(food.Count);
	}
}

// Energy represents the energy that an agent has.
//    It is a non-negative quantity.
//TODO: How to ensure that no Energy is wrongly kept? That is, adding the same Energy multiple times.
public struct Energy {
	public int Count;
	public Energy(int c) {
		Count = c;
	}
	public void Subtract(Energy e) {
		Count -= e.Count;
		if(Count < 0) {
			Count = 0;
		}
	}
	public void Add(Energy e) {
		Count += e.Count;
	}
	public static bool operator < (Energy e1, Energy e2) {
		return e1.Count < e2.Count;
	}
	public static bool operator > (Energy e1, Energy e2) {
		return e1.Count > e2.Count;
	}
	public static bool operator >= (Energy e1, Energy e2) {
		return e1.Count >= e2.Count;
	}
	public static bool operator <= (Energy e1, Energy e2) {
		return e1.Count <= e2.Count;
	}
	public static Energy Zero {
		get { return new Energy(0); }
	}
}


public struct SensorData {
	public IList<Vector2I> _cells;
	public Vector2I _front_cell;
	
	public IList<Vector2I> Cells
	{
        get;
        set;
	}
	
	public Vector2I FrontCell
	{
        get;
        set;
	}
	
	public SensorData(IList<Vector2I> cells, Vector2I front_cell)
	{
		_cells = cells;
		_front_cell = front_cell;
	}
}


public enum ORIENTATION {UP=0, DOWN=180, LEFT=270, RIGHT=90};

public struct Orientation {
	private ORIENTATION orientation;
	
	public static implicit operator Orientation(ORIENTATION orientation) {
		return new Orientation(orientation);
	}
	
	public static Orientation FromORIENTATION(ORIENTATION orientation) {
		return orientation;
	}
	
	public Vector2 ToVector2() {//Up=(0,1), Down=(0,-1), Left=(-1,0), Right=(1,0)
		switch (orientation) {
		case ORIENTATION.UP:
			return new Vector2(0f,1f);
		case ORIENTATION.DOWN:
			return new Vector2(0f,-1f);
		case ORIENTATION.LEFT:
			return new Vector2(-1f,0f);
		case ORIENTATION.RIGHT:
			return new Vector2(1f,0f);
		default:
			throw new System.Exception("Cannot convert "+orientation+" to UnityEngine.Vector2.");
		}
	}
	
	public Quaternion ToQuaternion() {
		return Quaternion.AngleAxis((float)orientation, Vector3.up);
	}
	
	public Quaternion ToQuaternionInX() {
		return Quaternion.AngleAxis((float)orientation, Vector3.right);
	}
	
	private Orientation(ORIENTATION orientation) {
		this.orientation = orientation;
	}
}