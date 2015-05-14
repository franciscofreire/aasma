using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

    public void Eat(FoodQuantity food) {
        energy.Add(EnergyFromFood(food));
    }

    public void ChangePosition(Vector2 newPosition) {
        worldInfo.worldTiles.WorldTileInfoAtCoord(CoordConvertions.AgentPosToTile(this.pos)).Agent = null;
        worldInfo.worldTiles.WorldTileInfoAtCoord(CoordConvertions.AgentPosToTile(newPosition)).Agent = this;
        this.pos = newPosition;
    }

    public abstract void updateSensors();

    public virtual void doAction() {
        agentImplementation.doAction();
    }
    
    public virtual void OnWorldTick() {
        if(Alive) {
            updateSensors();
            doAction();
        }
    }
    
    public abstract void AnnounceDeath();
    public abstract void AnnounceDeletion();

    public abstract void removeFromWorldInfo();

    private void Clamp() {
        this.pos = CoordConvertions.AdjustAgentPos(this.pos);
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



public struct Orientation {
    //The clockwise amplitude of the angle between this orientation and the up orientation.
    private Radians radiansToUp;
    
    public Radians ToRadiansToUp() {
        return radiansToUp;
    }
    
    public Vector2 ToVector2() {//Up=(0,1), Down=(0,-1), Left=(-1,0), Right=(1,0)
        return new Vector2(Mathf.Sin(radiansToUp), Mathf.Cos(radiansToUp));
    }

    public Vector2I ToVector2I() {
        var v2 = ToVector2();
        return new Vector2I(v2*1.5f);//Ensure cast returns unit vector for Up,Down,Left,Right
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
