using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// An Agent starts on a position in a world with a given Energy with an orientation.
//    When he is attacked, a certain amount of his Energy is removed (RemoveEnergy).
//    He is Alive as long as his Energy is greater than zero.
//    At any given moment, his senses (SensorData) provide him a view of its WorldInfo.
//    When he is hungry, the Agent can Eat some food and will get some Energy from it.
//    He can ChangePosition to any position. When he does, he lets the WorldInfo know that he is no longer in the tile he was and that he is in a new tile.
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
    
    private AgentImplementation agentImplementation;
    protected AgentImplementation AgentImpl {
        get {
            return agentImplementation;
        }
        set {
            agentImplementation = value;
        }
    }

	public Agent (WorldInfo world, Vector2 pos, Energy e) {
		this.worldInfo = world;
		this.pos = pos;
		this.energy = e;
		this.orientation = Orientation.Up;
	}

	public bool Alive {
		get { return energy > Energy.Zero; }
	}

    public virtual void RemoveEnergy(Energy e) {
		energy.Subtract(e);
    }

	public void Eat(FoodQuantity food) {
		energy.Add(EnergyFromFood(food));
	}

	public void ChangePosition(Vector2 newPosition) {
		worldInfo.worldTiles.WorldTileInfoAtCoord(CoordConvertions.AgentPosToWorldXZ(this.pos)).Agent = null;
		worldInfo.worldTiles.WorldTileInfoAtCoord(CoordConvertions.AgentPosToWorldXZ(newPosition)).Agent = this;
		this.pos = newPosition;
	}

	public Action doAction() {
      return agentImplementation.doAction();
   }
	
	public abstract void OnWorldTick();

	public void updateSensorData() {
		sensorData.Cells = worldInfo.nearbyFreeCells(worldInfo.nearbyCells(this));
        sensorData.FillAdjacentCells (new Vector2I (pos));
		
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

	// FIXME: I don't know where to put this function as it is not part of the Agent.
    // Or is it? It can also belong to the WorldInfo.
	public static Energy EnergyFromFood(FoodQuantity food) {
		return new Energy(food.Count);
	}
}

// Energy represents the energy that an agent has.
//    It is a non-negative quantity.
// TODO: How to ensure that no Energy is wrongly kept? That is, adding the same Energy multiple times.
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
        get { return _cells; }
        set { _cells = value; }
	}
	
	public Vector2I FrontCell
	{
        get { return _front_cell; }
        set { _front_cell = value; }
	}
	
	public SensorData(IList<Vector2I> cells, Vector2I front_cell)
	{
		_cells = cells;
		_front_cell = front_cell;
        _adjacent_cells = null;
	}

    public void FillAdjacentCells (Vector2I agentPos) {
        if (Cells != null) {
            AdjacentCells = new List<Vector2I> ();
            foreach (Vector2I pos in Cells) {
                if (agentPos.isAdjacent (pos)) {
                    AdjacentCells.Add (pos);
                }
            }
        }
    }
}

public struct Orientation {
    //The clockwise amplitude of the angle between this orientation and the up orientation.
    private Radians radiansToUp;
    
    public Radians ToRadiansToUp() {
        return radiansToUp;
    }
    
    public Vector2 ToVector2() {//Up=(0,1), Down=(0,-1), Left=(-1,0), Right=(1,0)
        return new Vector2(Mathf.Sin(radiansToUp), Mathf.Cos(radiansToUp));
    }
    
    public Quaternion ToQuaternion() {
        return Quaternion.AngleAxis(radiansToUp*Mathf.Rad2Deg, Vector3.up);
    }
    
    public Quaternion ToQuaternionInX() {
        return Quaternion.AngleAxis(radiansToUp*Mathf.Rad2Deg, Vector3.right);
    }
    
    private Orientation(Radians radiansToUp) {
        this.radiansToUp = radiansToUp;
    }
    
    public static Orientation FromRadians(Radians rad) {
        return new Orientation(rad);
    }
    
    public static Orientation FromDegrees(Degrees deg) {
        return new Orientation(deg);
    }
    
    public static bool operator== (Orientation o1, Orientation o2) { 
        return o1.radiansToUp.value == o2.radiansToUp.value;
    }
    
    public static bool operator!= (Orientation o1, Orientation o2) { 
        return !(o1 == o2);
    }

	public static readonly Orientation Up    = new Orientation(new Degrees(0));
	public static readonly Orientation Down  = new Orientation(new Degrees(180));
	public static readonly Orientation Left  = new Orientation(new Degrees(270));
	public static readonly Orientation Right = new Orientation(new Degrees(90));
}
