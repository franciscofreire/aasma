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
    
    public void RemoveEnergy(Energy e) {
        if (!Alive) { // Already dead: Do nothing
            return;
        }
        energy.Subtract(e);
        if (!Alive) { // First time he died: Notify listeners
            Logger.Log("[RIP] Agent @(" + pos.x + "," + pos.y + ")", Logger.VERBOSITY.AGENTS);
            Clamp();
            AnnounceDeath();
        }
    }
    public abstract void AnnounceDeath();
    public abstract void AnnounceDeletion();

    public abstract void removeFromWorldInfo();

	public void Eat(FoodQuantity food) {
		energy.Add(EnergyFromFood(food));
	}

    public void Clamp() {
        pos.x = ((int) pos.x) > pos.x
            ? (float) ((int) pos.x)
            : (float) ((int) pos.x) + 1;
        pos.y = ((int) pos.y) > pos.y
            ? (float) ((int) pos.y)
            : (float) ((int) pos.y) + 1;
    }

	public void ChangePosition(Vector2 newPosition) {
		worldInfo.worldTiles.WorldTileInfoAtCoord(CoordConvertions.AgentPosToTile(this.pos)).Agent = null;
		worldInfo.worldTiles.WorldTileInfoAtCoord(CoordConvertions.AgentPosToTile(newPosition)).Agent = this;
		this.pos = newPosition;
	}

	public Action doAction() {
      return agentImplementation.doAction();
    }
	
	public virtual void OnWorldTick() {
        if(Alive) {
            updateSensorData();
            Action a = doAction();
            a.apply();
        }
    }

	public void updateSensorData() {
        IList<Tree> _trees;
        IList<Tree> _stumps;
        IList<Habitant> _enemies;
        IList<Animal> _animals;
        IList<Animal> _food;

		sensorData.Cells = worldInfo.nearbyFreeCells(
            worldInfo.nearbyCells(this, out _trees,out _stumps,out _enemies,out _animals,out _food));

        sensorData._trees = _trees;
        sensorData._stumps = _stumps;
        sensorData._enemies = _enemies;
        sensorData._animals = _animals;
        sensorData._food = _food;

        sensorData.FillAdjacentCells (new Vector2I (pos));
		
		Vector2 posInFront = pos + orientation.ToVector2();
		Vector2I tileCoordInFront = CoordConvertions.AgentPosToTile(posInFront);
		sensorData.FrontCell = worldInfo.isInsideWorld(tileCoordInFront)
			? tileCoordInFront
			: new Vector2I(pos); // VERIFYME: Not sure about this...

        Vector2 posAtLeft = pos + orientation.LeftOrientation().ToVector2();
        Vector2I tileCoordAtLeft = CoordConvertions.AgentPosToTile(posAtLeft);
        sensorData._left_cell = worldInfo.isInsideWorld(tileCoordInFront)
            ? tileCoordAtLeft
            : new Vector2I(pos);

        Vector2 posAtRight = pos + orientation.RightOrientation().ToVector2();
        Vector2I tileCoordAtRight = CoordConvertions.AgentPosToTile(posAtRight);
        sensorData._right_cell = worldInfo.isInsideWorld(tileCoordInFront)
            ? tileCoordAtRight
            : new Vector2I(pos);

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
    public Vector2I _left_cell;
    public Vector2I _right_cell;
    public IList<Vector2I> _adjacent_cells;
    public IList<Tree> _trees;
    public IList<Tree> _stumps;
    public IList<Habitant> _enemies;
    public IList<Animal> _animals;
    public IList<Animal> _food;
	
	public IList<Vector2I> Cells
	{
        get { return _cells; }
        set { _cells = value; }
	}
    public IList<Tree> Tree
    {
        get { return _trees; }
        set { _trees = value; }
    }
    public IList<Tree> Stumps
    {
        get { return _stumps; }
        set { _stumps = value; }
    }

    public IList<Habitant> Enemies
    {
        get { return _enemies; }
        set { _enemies = value; }
    }

    public IList<Animal> Animals
    {
        get { return _animals; }
        set { _animals = value; }
    }

    public IList<Animal> Food
    {
        get { return _food; }
        set { _food = value; }
    }

	public Vector2I FrontCell
	{
        get { return _front_cell; }
        set { _front_cell = value; }
	}

    public Vector2I LeftCell
    {
        get { return _left_cell; }
        set { _left_cell = value; }
    }

    public Vector2I RightCell
    {
        get { return _right_cell; }
        set { _right_cell = value; }
    }

    public IList<Vector2I> AdjacentCells {
        get { return _adjacent_cells; }
        set { _adjacent_cells = value; }
    }
	
	public SensorData(IList<Vector2I> cells, Vector2I front_cell, 
                      Vector2I left_cell, Vector2I right_cell)
	{
		_cells = cells;
		_front_cell = front_cell;
        _left_cell = left_cell;
        _right_cell = right_cell;
        _adjacent_cells = null;
        _trees = null;
        _stumps = null;
        _enemies = null;
        _animals = null;
        _food = null;
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

    public Orientation LeftOrientation() {
        return new Orientation(this.radiansToUp + (new Degrees(-90)).Radians);
    }

    public Orientation RightOrientation() {
        return new Orientation(this.radiansToUp + (new Degrees(90)).Radians);
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
